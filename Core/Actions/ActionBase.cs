using Newtonsoft.Json;
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

        public IAction Clone()
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, this);
            ms.Seek(0, 0);
            return (IAction)formatter.Deserialize(ms);
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

    public abstract class ActionBase<TModel, TInterface> : ActionBase
        where TModel : TInterface
    {
        public ActionBase(TInterface model)
        {
            Model = model;
        }

        protected ActionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var json = info.GetString(nameof(TModel));
            Model = JsonConvert.DeserializeObject<TModel>(json);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(TModel), JsonConvert.SerializeObject(Model));
        }

        public TInterface Model { get; }
    }
}