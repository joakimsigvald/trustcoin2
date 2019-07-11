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
    public class Account : IAccount
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly IDictionary<string, Transaction> _pendingTransactions = new Dictionary<string, Transaction>();
        private readonly IDictionary<string, IArtefact> _knownArtefacts = new Dictionary<string, IArtefact>();
        private readonly ICryptography _cryptography;
        private readonly LimitedQueue<string> _receivedTransactions = new LimitedQueue<string>(100);

        public Account(ICryptography cryptography, string name)
        {
            _cryptography = cryptography;
            Name = name;
            Self = CreateSelf();
            SetRelationWeight(Name, Name, Weight.Max);
        }

        public string Name { get; private set; }
        public ICollection<IArtefact> Artefacts => _knownArtefacts.Values;
        public byte[] PublicKey => _cryptography.PublicKey;

        public IPeer Self { get; }

        public bool IsConnectedTo(string name)
            => name == Name || (name != null && _peers.ContainsKey(name));

        public IEnumerable<IPeer> Peers => _peers.Values.Append(Self);

        public bool KnowsArtefact(string name)
            => _knownArtefacts.ContainsKey(name);

        public IArtefact ProduceArtefact(string name)
            => _knownArtefacts.TryGetValue(name, out var artefact) 
            ? artefact 
            : new Artefact(name, null);

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
            _cryptography.RenewKeys();
        }

        public override string ToString() => Name;

        public void RememberArtefact(IArtefact artefact)
        {
            _knownArtefacts.Add(artefact.Name, artefact);
        }

        public void VerifySignature(SignedAction signedAction, IPeer peer)
        {
            _cryptography.VerifySignature(signedAction, peer);
        }

        private IPeer CreateSelf()
        {
            var self = new Peer(Name, PublicKey, _peers.Values.Select(p => p.AsRelation()))
            {
                Trust = SignedWeight.Max,
            };
            self.AddRelation(self);
            return self;
        }

        public void AddPeer(IPeer peer)
        {
            _peers.Add(peer.Name, peer);
        }

        public SignedAction Sign(IAction action)
            => _cryptography.Sign(action);

        public IClient GetClient(INetwork network)
            => new Client(network, GetActor(network));

        public IActor GetActor(INetwork network)
            => new Actor(network, this);

        public void MoveArtefact(IArtefact artefact, string ownerName)
        {
            RemoveArtefact(artefact);
            AddArtefact(artefact.Name, ownerName);
        }

        public void RemoveArtefact(IArtefact artefact)
        {
            ForgetArtefact(artefact.Name);
            if (IsConnectedTo(artefact.OwnerName))
                GetPeer(artefact.OwnerName).RemoveArtefact(artefact);
        }

        public void AddArtefact(string artefactName, string ownerName)
        {
            if (!IsConnectedTo(ownerName))
                return;
            var newArtefact = new Artefact(artefactName, ownerName);
            GetPeer(ownerName).AddArtefact(newArtefact);
            RememberArtefact(newArtefact);
        }

        public void AddTransaction(Transaction transaction)
            => _pendingTransactions.Add(transaction.Key, transaction);

        public Transaction GetPendingTransaction(string key)
            => _pendingTransactions[key];

        public bool HasPendingTransaction(string key)
            => _pendingTransactions.ContainsKey(key);

        public void ClosePendingTransaction(string key)
        {
            _pendingTransactions.Remove(key);
        }

        public void AddPendingTransaction(Transaction transaction)
        {
            _pendingTransactions.Add(transaction.Key, transaction);
        }

        public void AddReceivedTransaction(string key)
        {
            _receivedTransactions.Enqueue(key);
        }

        public bool HasReceivedTransaction(string key)
            => _receivedTransactions.Contains(key);
    }
}