using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Account : IAccount, IClient
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly IDictionary<string, IArtefact> _knownArtefacts = new Dictionary<string, IArtefact>();
        private readonly INetwork _network;
        private readonly ICryptography _cryptography;

        public Account(INetwork network, ICryptography cryptography, string name)
        {
            _network = network;
            _cryptography = cryptography;
            Name = name;
            Self = CreateSelf();
            SetRelationWeight(Name, Name, Weight.Max);
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
        public IEnumerable<IPeer> TrustedPeers => Peers.Where(p => p.Trust > 0);

        public void Endorce(string name)
        {
            ProducePeer(name).Endorce();
            OnEndorcedAgent(name);
        }

        public void EndorceArtefact(IArtefact artefact)
        {
            if (artefact.OwnerName is null)
                throw new ArgumentException("Tried to endorce artefact without owner");
            var owner = ProducePeer(artefact.OwnerName);
            if (KnowsArtefact(artefact.Name))
            {
                artefact = GetArtefact(artefact.Name);
                if (owner.Name != artefact.OwnerName)
                    throw new ArgumentException($"Tried to endorce artefact with invalid owner: {owner.Name}");
            }
            else
                _knownArtefacts.Add(artefact.Name, artefact);
            if (artefact.IsEndorcedBy(Name))
                return;
            IncreaseTrust(owner.Name, ArtefactEndorcementTrustFactor);
            OnEndorcedArtefact(artefact);
        }

        public bool EndorcesArtefact(string agentName, string artefactName)
            => KnowsArtefact(artefactName)
            && GetArtefact(artefactName).IsEndorcedBy(agentName);

        public bool KnowsArtefact(string name)
            => _knownArtefacts.ContainsKey(name);

        public IArtefact GetArtefact(string name)
            => _knownArtefacts[name];

        public void ForgetArtefact(string name)
        {
            _knownArtefacts.Remove(name);
        }

        public IPeer GetPeer(string name)
            => name == Name ? Self
            : _peers.TryGetValue(name, out var peer) ? peer
            : throw new NotFound<IPeer>(name);

        public SignedWeight GetTrust(string name) => GetPeer(name).Trust;
        public SignedWeight SetTrust(string name, SignedWeight trust) => GetPeer(name).Trust = trust;
        public SignedWeight IncreaseTrust(string name, Weight factor) => GetPeer(name).IncreaseTrust(factor);
        public SignedWeight DecreaseTrust(string name, Weight factor) => GetPeer(name).DecreaseTrust(factor);

        public Money GetMoney(string name) => GetPeer(name).Money;
        public void SetMoney(string name, Money money) => GetPeer(name).Money = money;

        public void SetRelationWeight(string subjectName, string objectName, Weight value)
        {
            var subject = GetPeer(subjectName);
            var relation = subject.GetRelation(objectName);
            relation.Strength = value;
        }

        public Weight GetRelationWeight(string subjectName, string objectName)
        {
            var subject = GetPeer(subjectName);
            var relation = subject.GetRelation(objectName);
            return relation.Strength;
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
            var peerAssessments = TrustedPeers
                .ToDictionary(peer => peer, peer => GetUpdatesFromPeer(peer, peersToUpdate))
                .SelectMany(x => x.Value.Select(y => (target: x.Key, subject: y.Key, money: y.Value)))
                .GroupBy(iu => iu.subject)
                .ToDictionary(
                g => GetPeer(g.Key),
                g => g.Select(x => (x.target, x.money)).ToArray());
            foreach (var kvp in peerAssessments)
                SyncPeer(totalTrust, kvp.Key, kvp.Value);
        }

        public IArtefact CreateArtefact(string name)
        {
            var artefact = new Artefact(name, Name);
            OnCreatedArtefact(artefact);
            return artefact;
        }

        public void DestroyArtefact(string name)
        {
            if (!KnowsArtefact(name) || GetArtefact(name).OwnerName != Name)
                throw new ArgumentException("Tried to destroy artefact not owned");
            OnDestroyArtefact(GetArtefact(name));
        }

        public bool Update(string subjectName, SignedAction signedAction)
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

        private IPeer CreateSelf()
        {
            var self = new Peer(Name, PublicKey, _peers.Values.Select(p => p.AsRelation()))
            {
                Trust = SignedWeight.Max,
                IsEndorced = true
            };
            self.AddRelation(self);
            return self;
        }

        private void SyncPeer(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            subject.Money = ComputeMoney(totalTrust, subject, assessments);
        }

        private Money ComputeMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var meanAssessment = ComputeMeanAssessment(assessments);
            var weightedMeanAssessment = (Money)new[] { (sumOfTrusts, meanAssessment), (totalTrust - sumOfTrusts, (float)subject.Money) }.WeightedAverage();
            return weightedMeanAssessment;
        }

        private Money ComputeMeanAssessment((IPeer target, Money money)[] assessments)
            => (Money)assessments.Select(a => ((float)a.target.Trust, (float)a.money)).WeightedMean();

        private IDictionary<string, Money> GetUpdatesFromPeer(IPeer peer, string[] peersToUpdate)
            => _network.RequestUpdate(
                peer.Name,
                peer.Relations.Select(r => r.Agent.Name).Intersect(peersToUpdate).ToArray());

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
                case CreateArtefactAction caa:
                    WhenCreateArtefact(peer, caa);
                    break;
                case DestroyArtefactAction daa:
                    WhenDestroyArtefact(peer, daa);
                    break;
                case EndorceArtefactAction daa:
                    WhenEndorceArtefact(peer, daa);
                    break;
                case NoAction _:
                    break;
                default: throw new NotImplementedException("Action not implemented: " + action);
            }
        }

        private void WhenRenewKey(IPeer peer, RenewKeyAction ra)
        {
            if (peer.PublicKey.SequenceEqual(ra.NewKey))
                return;
            if (!peer.PublicKey.SequenceEqual(ra.OldKey))
                throw new InvalidOperationException($"Invalid old key: {ra.OldKey}, expected: {peer.PublicKey}");
            peer.PublicKey = ra.NewKey;
        }

        private void WhenConnect(IPeer peer, ConnectAction action)
        {
            if (peer.IsConnectedTo(action.AgentName))
                return;
            peer.AddRelation(_network.FindAgent(action.AgentName));
        }

        private void WhenEndorce(IPeer peer, EndorceAction ea)
        {
            var relation = ProduceRelation(peer, ea.AgentName);
            if (relation.IsEndorced)
            {
                peer.DecreaseTrust(DoubleEndorceDistrustFactor);
                return;
            }
            AddMoneyFromEndorcement(peer, relation);
            relation.IsEndorced = true;
        }

        private void WhenEndorceArtefact(IPeer peer, ArtefactAction action)
        {
            var artefact = action.Artefact;
            if (KnowsArtefact(action.Artefact.Name))
            {
                artefact = GetArtefact(artefact.Name);
                if (artefact.OwnerName != action.Artefact.OwnerName)
                {
                    peer.DecreaseTrust(EndorceCounterfeitArtefactDistrustFactor);
                    return;
                }
                if (artefact.IsEndorcedBy(peer.Name))
                {
                    peer.DecreaseTrust(DoubleEndorceArtefactDistrustFactor);
                    return;
                }
            }
            else
                _knownArtefacts.Add(artefact.Name, artefact);

            var relation = ProduceRelation(peer, artefact.OwnerName);
            AddMoneyFromEndorcement(peer, relation, ArtefactMoneyFactor);
            relation.IncreaseStrength(ArtefactEndorcementTrustFactor);
            artefact.AddEndorcer(peer);
        }

        private Relation ProduceRelation(IPeer source, string targetName) 
            => source.GetRelation(targetName)
                ?? source.AddRelation(_network.FindAgent(targetName));

        private void WhenCreateArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Artefact.Name))
                return;
            if (OtherPeerHasArtefact(action.Artefact.Name))
                peer.DecreaseTrust(CounterfeitArtefactDistrustFactor);
            else
                AddArtefact(peer, action.Artefact);
        }

        private void WhenDestroyArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Artefact.Name))
                DestroyArtefact(peer, action.Artefact);
            else
                peer.DecreaseTrust(DestroyOthersArtefactDistrustFactor);
        }

        private bool OtherPeerHasArtefact(string name)
            => _knownArtefacts.ContainsKey(name);

        private void AddArtefact(IPeer peer, IArtefact artefact)
        {
            peer.AddArtefact(artefact);
            _knownArtefacts.Add(artefact.Name, artefact);
        }

        private void DestroyArtefact(IPeer peer, IArtefact artefact)
        {
            peer.RemoveArtefact(artefact);
            _knownArtefacts.Remove(artefact.Name);
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation, float factor = 1)
        {
            var addedMoney = factor * endorcer.Trust * (1 - (float)relation.Strength);
            if (addedMoney <= 0) return;
            var endorcedPeer = ProducePeer(relation.Agent.Name);
            endorcedPeer.Money += (Money)addedMoney;
        }

        private IPeer ProducePeer(string name)
            => IsConnectedTo(name) ? GetPeer(name) : Connect(name);

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

        private void OnCreatedArtefact(IArtefact artefact)
        {
            SendAction(new CreateArtefactAction(artefact));
        }

        private void OnDestroyArtefact(IArtefact artefact)
        {
            SendAction(new DestroyArtefactAction(artefact));
        }

        private void OnEndorcedArtefact(IArtefact artefact)
        {
            SendAction(new EndorceArtefactAction(artefact));
        }

        private void SendAction(IAction action)
        {
            var signedAction = _cryptography.Sign(action);
            foreach (var peer in Peers)
                _network.SendAction(peer.Name, Name, signedAction);
        }
    }
}