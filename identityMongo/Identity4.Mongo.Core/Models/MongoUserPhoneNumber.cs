namespace Identity4.Mongo.Core.Models
{
    public class MongoUserPhoneNumber : MongoUserContactRecord
    {
        public MongoUserPhoneNumber(string phoneNumber) : base(phoneNumber)
        {
        }
    }
}
