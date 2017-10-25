using System;

namespace Identity4.Mongo.Core.Models
{
    public class FutureOccurrence : Occurrence
    {
        public FutureOccurrence()
        {
        }

        public FutureOccurrence(DateTime willOccurOn) : base(willOccurOn)
        {
        }
    }
}
