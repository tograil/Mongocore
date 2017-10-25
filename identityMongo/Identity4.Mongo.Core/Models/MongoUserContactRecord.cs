using System;

namespace Identity4.Mongo.Core.Models
{
    public abstract class MongoUserContactRecord : IEquatable<MongoUserEmail>
    {
        protected MongoUserContactRecord(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; private set; }
        public ConfirmationOccurrence ConfirmationRecord { get; private set; }

        public bool IsConfirmed()
        {
            return ConfirmationRecord != null;
        }

        public void SetConfirmed()
        {
            SetConfirmed(new ConfirmationOccurrence());
        }

        public void SetConfirmed(ConfirmationOccurrence confirmationRecord)
        {
            if (ConfirmationRecord == null)
            {
                ConfirmationRecord = confirmationRecord;
            }
        }

        public void SetUnconfirmed()
        {
            ConfirmationRecord = null;
        }

        public bool Equals(MongoUserEmail other)
        {
            return other.Value.Equals(Value);
        }
    }
}
