using System.Runtime.Serialization;
using Trustcoin.Core.Actions;

namespace Trustcoin.Core.Entities
{
    public abstract class TransationAction : ActionBase
    {
        protected TransationAction(Transaction transaction)
        {
            Transaction = transaction;
        }

        protected TransationAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var transactionKey = (string)info.GetValue(nameof(Transaction.Key), typeof(string));
            var name = (string)info.GetValue(nameof(Artefact.Name), typeof(string));
            var ownerName = (string)info.GetValue(nameof(Artefact.OwnerName), typeof(string));
            var receiverName = (string)info.GetValue(nameof(Transaction.ReceiverName), typeof(string));
            Transaction = new Transaction
            {
                Key = transactionKey,
                Artefact = new Artefact(name, ownerName),
                ReceiverName = receiverName
            };
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Transaction.Key), Transaction.Key);
            info.AddValue(nameof(Artefact.Name), Transaction.Artefact.Name);
            info.AddValue(nameof(Artefact.OwnerName), Transaction.Artefact.OwnerName);
            info.AddValue(nameof(Transaction.ReceiverName), Transaction.ReceiverName);
        }

        public Transaction Transaction { get; }
    }
}