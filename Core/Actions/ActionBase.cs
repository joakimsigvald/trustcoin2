using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Trustcoin.Core.Actions
{
    public abstract class ActionBase : IAction
    {
        protected ActionBase()
        {
        }

        protected ActionBase(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
        }

        public byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using var ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }
    }
}
