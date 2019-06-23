using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Business
{

    public class Peer : IPeer
    {
        public const float BaseTrust = 0.5f;

        public bool IsEndorced { get; private set; }
        private readonly IDictionary<string, IAgent> _relations = new Dictionary<string, IAgent>();
        private IAccount _subject;
        private float _trust;

        private Peer(IEnumerable<IAgent> relations) {
            _relations = relations.ToDictionary(r => r.Name);
        }

        public static IPeer GetSelf(IAccount target)
            => new Peer(target.Peers.Select(p => (IAgent)new Agent(p)))
            {
                Name = target.Name,
                Trust = 1
            };

        public static IPeer GetPeer(IAccount subject, IAgent target)
            => new Peer(target.Relations)
            {
                Name = target.Name,
                Subject = subject,
                Trust = BaseTrust
            };

        public string Name { get; private set; }

        private IAccount Subject
        {
            set
            {
                if (_subject != null)
                    throw new InvalidOperationException("Cannot connect subject to peer twice");
                _subject = value;
            }
        }

        public ICollection<IAgent> Relations => _relations.Values;

        public float Trust
        {
            get => _trust;
            set
            {
                if (value < 0 || value > 1)
                    throw new OutOfBounds();
                _trust = value;
            }
        }

        public void Endorce(IAccount account)
        {
            if (account != _subject)
                throw new ArgumentException("Only subject can endorce peer");
            if (IsEndorced) return;
            IsEndorced = true;
            Trust = (1 + Trust) * 0.5f;
        }

        public bool Endorces(string name) => GetRelation(name)?.IsEndorced ?? false;

        private IAgent GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : null;

        public float IncreaseTrust(float factor)
        {
            if (factor < 0 || factor > 1)
                throw new OutOfBounds();
            return Trust += (1 - Trust) * factor; ;
        }

        public float ReduceTrust(float factor)
        {
            if (factor < 0 || factor > 1)
                throw new OutOfBounds();
            return Trust *= 1 - factor;
        }
    }
}