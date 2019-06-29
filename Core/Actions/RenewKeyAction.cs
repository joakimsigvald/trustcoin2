using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class RenewKeyAction : ActionBase
    {
        public RenewKeyAction(byte[] oldKey, byte[] newKey)
        {
            OldKey = oldKey;
            NewKey = newKey;
        }

        public byte[] OldKey { get; }
        public byte[] NewKey { get; }

        protected RenewKeyAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OldKey = (byte[])info.GetValue(nameof(OldKey), typeof(byte[]));
            NewKey = (byte[])info.GetValue(nameof(NewKey), typeof(byte[]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(OldKey), OldKey);
            info.AddValue(nameof(NewKey), NewKey);
        }
    }
}