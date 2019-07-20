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
        private byte _createdAccountCount = 0;
        public uint _resilientArtefactCount = 0;
        public uint _transientArtefactCount = 0;
        private readonly IDictionary<AgentId, IPeer> _peers = new Dictionary<AgentId, IPeer>();
        private readonly IDictionary<string, Transaction> _pendingTransactions = new Dictionary<string, Transaction>();
        private readonly IDictionary<ArtefactId, Artefact> _knownArtefacts = new Dictionary<ArtefactId, Artefact>();
        private readonly ICryptography _cryptography;
        private readonly LimitedQueue<string> _receivedTransactions = new LimitedQueue<string>(100);

        public Account(ICryptography cryptography, string name, byte number)
            : this(cryptography, name, new AgentId(number))
        {
        }

        private Account(ICryptography cryptography, string name, AgentId id)
        {
            _cryptography = cryptography;
            Name = name;
            Id = id;
            Self = CreateSelf();
            SetRelationWeight(Id, Id, Weight.Max);
        }

        public string Name { get; }
        public AgentId Id { get; }
        public ICollection<Artefact> Artefacts => _knownArtefacts.Values;
        public byte[] PublicKey => _cryptography.PublicKey;

        public IPeer Self { get; }

        public bool IsConnectedTo(AgentId id)
            => id == Id || _peers.ContainsKey(id);

        public IEnumerable<IPeer> OtherPeers => _peers.Values;
        public IEnumerable<IPeer> Peers => OtherPeers.Append(Self);

        public IAccount CreateChild(string name)
        {
            var child = new Account(_cryptography, name, Id + ++_createdAccountCount);
            return child;
        }

        public bool KnowsArtefact(ArtefactId id)
            => _knownArtefacts.ContainsKey(id);

        public Artefact ProduceArtefact(Artefact artefact)
            => _knownArtefacts.TryGetValue(artefact.Id, out var knownArtefact) 
            ? knownArtefact
            : new Artefact(artefact, default);

        public Artefact CreateArtefact(string name, bool isResilient)
            => new Artefact(Id / (isResilient ? NextResilientNumber: NextTransientNumber), name, Id);

        private uint NextTransientNumber
            => _transientArtefactCount = (_transientArtefactCount + 1) % int.MaxValue;

        private uint NextResilientNumber
            => _resilientArtefactCount < int.MaxValue 
            ? int.MaxValue + ++_resilientArtefactCount 
            : throw new OutOfBounds<uint>(uint.MaxValue);

        public Artefact GetArtefact(ArtefactId id)
            => _knownArtefacts[id];

        public void ForgetArtefact(ArtefactId id)
        {
            _knownArtefacts.Remove(id);
        }

        public IPeer GetPeer(AgentId id)
            => id == Id ? Self
            : _peers.TryGetValue(id, out var peer) ? peer
            : throw new NotFound<IPeer>("id", $"{id}");

        public SignedWeight GetTrust(AgentId id) => GetPeer(id).Trust;
        public SignedWeight SetTrust(AgentId id, SignedWeight trust) 
            => GetPeer(id).Trust = id == Id ? SignedWeight.Max : trust;
        public SignedWeight IncreaseTrust(AgentId id, Weight factor) 
            => id == Id ? SignedWeight.Max : GetPeer(id).IncreaseTrust(factor);
        public SignedWeight DecreaseTrust(AgentId id, Weight factor) 
            => id == Id ? SignedWeight.Max : GetPeer(id).DecreaseTrust(factor);

        public Money GetMoney(AgentId id) => GetPeer(id).Money;
        public void SetMoney(AgentId id, Money money) => GetPeer(id).Money = money;
        public void IncreaseMoney(AgentId id, Money money) 
            => GetPeer(id).IncreaseMoney(money);
        public void DecreaseMoney(AgentId id, Money money) 
            => GetPeer(id).DecreaseMoney(money);

        public void SetRelationWeight(AgentId subjectId, AgentId objectId, Weight value)
        {
            if (subjectId == objectId)
                value = Weight.Max;
            var subject = GetPeer(subjectId);
            var relation = subject.GetRelation(objectId);
            relation.Strength = value;
        }

        public Weight GetRelationWeight(AgentId subjectId, AgentId objectId)
        {
            var subject = GetPeer(subjectId);
            var relation = subject.GetRelation(objectId);
            return relation.Strength;
        }

        public void RenewKeys()
        {
            _cryptography.RenewKeys();
        }

        public override string ToString() => Name;

        public void RememberArtefact(Artefact artefact)
        {
            _knownArtefacts.Add(artefact.Id, artefact);
        }

        public void VerifySignature(SignedAction signedAction, IPeer peer)
        {
            _cryptography.VerifySignature(signedAction, peer);
        }

        private IPeer CreateSelf()
        {
            var self = new Peer(Name, Id, PublicKey, _peers.Values.Select(p => p.AsRelation()))
            {
                Trust = SignedWeight.Max,
            };
            self.AddRelation(self);
            return self;
        }

        public void AddPeer(IPeer peer)
        {
            _peers.Add(peer.Id, peer);
        }

        public SignedAction Sign(IAction action)
            => _cryptography.Sign(action);

        public IClient GetClient(INetwork network, ITransactionFactory transactionFactory)
            => new Client(network, GetActor(network, transactionFactory));

        public IActor GetActor(INetwork network, ITransactionFactory transactionFactory)
            => new Actor(network, this, transactionFactory);

        public void MoveArtefact(Artefact artefact, AgentId ownerId)
        {
            RemoveArtefact(artefact);
            AddArtefact(artefact, ownerId);
        }

        public void RemoveArtefact(Artefact artefact)
        {
            ForgetArtefact(artefact.Id);
            if (IsConnectedTo(artefact.OwnerId))
                GetPeer(artefact.OwnerId).RemoveArtefact(artefact);
        }

        public void AddArtefact(Artefact artefact, AgentId ownerId)
        {
            if (!IsConnectedTo(ownerId))
                return;
            var newArtefact = new Artefact(artefact, ownerId);
            GetPeer(ownerId).AddArtefact(newArtefact);
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