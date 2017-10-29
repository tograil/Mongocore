using System.Security.Claims;

namespace Identity4.Mongo.Core
{
    public class MongoUserClaims
    {
        public MongoUserClaims()
        {
            
        }

        public MongoUserClaims(Claim claim)
        {
            Type = claim.Type;
            Value = claim.Value;
        }

        public string Type { get; set; }

        public string Value { get; set; }

        public Claim ToSecurityClaim()
        {
            return new Claim(Type, Value);
        }
    }
}
