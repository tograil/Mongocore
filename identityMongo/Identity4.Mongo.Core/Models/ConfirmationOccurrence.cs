using System;

namespace Identity4.Mongo.Core.Models
{
    public class ConfirmationOccurrence : Occurrence
    {
        public ConfirmationOccurrence()
        {
        }

        public ConfirmationOccurrence(DateTime confirmedOn) : base(confirmedOn)
        {
        }
    }
}
