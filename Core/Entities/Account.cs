using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Account : IAccount, IClient
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly INetwork _network;
        private readonly ICryptography _cryptography;

        public Account(INetwork network, ICryptography cryptography, string name)
        {
            _network = network;
            _cryptography = cryptography;
            Name = name;
            Self = CreateSelf();
        }

        private IPeer CreateSelf()
        {
            var self = new Peer(Name, PublicKey, _peers.Values.Select(p => p.AsRelation()))
            {
                Trust = Weight.Max,
                IsEndorced = true
            };
            self.AddRelation(self);
            return self;
        }

        public string Name { get; private set; }
        public byte[] PublicKey => _cryptography.PublicKey;

        public IPeer Self { get; }

        public IPeer Connect(string name)
        {
            if (IsConnectedTo(name))
                return GetPeer(name);
            var newPeer = _network.FindAgent(name).AsPeer();
            _peers[name] = newPeer;
            OnAddedConnection(name);
            return newPeer;
        }

        public bool IsConnectedTo(string name)
            => name == Name || _peers.ContainsKey(name);

        public IEnumerable<IPeer> Peers => _peers.Values.Append(Self);

        public void Endorce(string name)
        {
            ProducePeer(name).Endorce();
            OnEndorcedAgent(name);
        }

        public IPeer GetPeer(string name)
            => name == Name ? Self
            : _peers.TryGetValue(name, out var peer) ? peer
            : throw new NotFound<Peer>(name);

        public Weight GetTrust(string name) => GetPeer(name).Trust;
        public Weight SetTrust(string name, Weight trust) => GetPeer(name).Trust = trust;
        public Weight IncreaseTrust(string name, Weight factor) => GetPeer(name).IncreaseTrust(factor);
        public Weight ReduceTrust(string name, Weight factor) => GetPeer(name).ReduceTrust(factor);

        public Money GetMoney(string name) => GetPeer(name).Money;
        public void SetMoney(string name, Money money) => GetPeer(name).Money = money;

        public void SetRelationWeight(string subjectName, string objectName, Weight value)
        {
            var subject = GetPeer(subjectName);
            var relation = subject.GetRelation(objectName);
            relation.Weight = value;
        }

        public Weight GetRelationWeight(string subjectName, string objectName)
        {
            var subject = GetPeer(subjectName);
            var relation = subject.GetRelation(objectName);
            return relation.Weight;
        }

        public void RenewKeys()
        {
            var oldKey = PublicKey;
            _cryptography.RenewKeys();
            OnRenewedKey(oldKey, PublicKey);
        }

        public override string ToString() => Name;

        public void SyncAll()
        {
            var totalTrust = Peers.Sum(p => p.Trust);
            string[] peersToUpdate = Peers.Select(peer => peer.Name).ToArray();
            var peerAssessments = Peers
                .ToDictionary(peer => peer, peer => GetUpdatesFromPeer(peer, peersToUpdate))
                .SelectMany(x => x.Value.Select(y => (target: x.Key, subject: y.Key, money: y.Value)))
                .GroupBy(iu => iu.subject)
                .ToDictionary(
                g => GetPeer(g.Key),
                g => g.Select(x => (x.target, x.money)).ToArray());
            foreach (var kvp in peerAssessments)
                SyncPeer(totalTrust, kvp.Key, kvp.Value);
        }

        private void SyncPeer(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            subject.Money = ComputeMoney(totalTrust, subject, assessments);
        }

        private Money ComputeMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var meanAssessment = ComputeMeanAssessment(subject, assessments);
            var weightedMeanAssessment = (Money)new[] { (sumOfTrusts, meanAssessment), (totalTrust - sumOfTrusts, (float)subject.Money) }.WeightedAverage();
            return weightedMeanAssessment;
        }

        private Money ComputeMeanAssessment(IPeer subject, (IPeer target, Money money)[] assessments)
            => (Money)assessments.Select(a => ((float)a.target.Trust, (float)a.money)).WeightedMean();

        private IDictionary<string, Money> GetUpdatesFromPeer(IPeer peer, string[] peersToUpdate)
            => _network.RequestUpdate(
                peer.Name,
                peer.Relations.Select(r => r.Agent.Name).Intersect(peersToUpdate).ToArray());

        public bool Update(string subjectName, ISignedAction signedAction)
        {
            if (!IsConnectedTo(subjectName))
                return false;
            var peer = GetPeer(subjectName);
            _cryptography.VerifySignature(signedAction, peer);
            UpdatePeer(peer, signedAction.Action);
            return true;
        }

        public IDictionary<string, Money> RequestUpdate(string[] subjectNames)
        {
            var requestedPeers = Peers.Select(p => p.Name)
                .Intersect(subjectNames)
            .Select(GetPeer)
            .ToArray();
            return requestedPeers.ToDictionary(p => p.Name, p => p.Money);
        }

        private void UpdatePeer(IPeer peer, IAction action)
        {
            switch (action)
            {
                case ConnectAction ca:
                    WhenConnect(peer, ca);
                    break;
                case EndorceAction ea:
                    WhenEndorce(peer, ea);
                    break;
                case RenewKeyAction ra:
                    WhenRenewKey(peer, ra);
                    break;
                case NoAction _:
                    break;
                default: throw new NotImplementedException("Action not implemented: " + action);
            }
        }

        private void WhenRenewKey(IPeer peer, RenewKeyAction ra)
        {
            if (peer.PublicKey == ra.NewKey)
                return;
            if (peer.PublicKey != ra.OldKey)
                throw new InvalidOperationException($"Invalid old key: {ra.OldKey}, expected: {peer.PublicKey}");
            peer.PublicKey = ra.NewKey;
        }

        private void WhenEndorce(IPeer peer, EndorceAction ea)
        {
            if (peer.Endorces(ea.AgentName))
                return;
            var relation = peer.GetRelation(ea.AgentName);
            if (!relation.IsConnected)
                relation = peer.AddRelation(_network.FindAgent(ea.AgentName));
            AddMoneyFromEndorcement(peer, relation);
            relation.IsEndorced = true;
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation)
        {
            var addedMoney = (2 * endorcer.Trust - 1) * (1 - (float)relation.Weight);
            if (addedMoney <= 0) return;
            var endorcedPeer = ProducePeer(relation.Agent.Name);
            endorcedPeer.Money += (Money)addedMoney;
        }

        private IPeer ProducePeer(string name)
            => IsConnectedTo(name) ? GetPeer(name) : Connect(name);

        private void WhenConnect(IPeer peer, ConnectAction action)
        {
            if (peer.IsConnectedTo(action.AgentName))
                return;
            peer.AddRelation(_network.FindAgent(action.AgentName));
        }

        private void OnAddedConnection(string agentName)
        {
            SendAction(new ConnectAction(agentName));
        }

        private void OnRenewedKey(byte[] oldKey, byte[] newKey)
        {
            SendAction(new RenewKeyAction(oldKey, newKey));
        }

        private void OnEndorcedAgent(string agentName)
        {
            SendAction(new EndorceAction(agentName));
        }

        private void SendAction(IAction action)
        {
            var signedAction = _cryptography.Sign(action);
            foreach (var peer in Peers)
                _network.SendAction(peer.Name, Name, signedAction);
        }
    }
}