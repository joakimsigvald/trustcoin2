using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core
{
    public class Peer : Agent, IPeer
    {
        public const float BaseTrust = 0.5f;

        private IAccount _subject;
        private float _trust;

        private Peer(string name, string publicKey, IEnumerable<IAgent> relations)
            : base(name, publicKey, relations)
        {
        }

        public static IPeer GetSelf(IAccount target)
            => new Peer(target.Name, target.PublicKey, target.Peers.Select(p =>  p.Clone()))
            {
                Trust = 1
            };

        public static IPeer GetPeer(IAccount subject, IAgent target)
            => new Peer(target.Name, target.PublicKey, target.Relations)
            {
                Subject = subject,
                Trust = BaseTrust
            };

        private IAccount Subject
        {
            set
            {
                if (_subject != null)
                    throw new InvalidOperationException("Cannot connect subject to peer twice");
                _subject = value;
            }
        }

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

        public float IncreaseTrust(float factor)
            => IsWeight(factor) 
            ? Trust += (1 - Trust) * factor 
            : throw new OutOfBounds();

        public float ReduceTrust(float factor)
            => IsWeight(factor) 
            ? Trust *= 1 - factor 
            : throw new OutOfBounds();

        private bool IsWeight(float value) => value >= 0 && value <= 1;
    }
}