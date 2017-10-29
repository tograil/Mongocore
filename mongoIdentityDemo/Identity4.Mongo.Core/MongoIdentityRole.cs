using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Identity4.Mongo.Core
{
    public class MongoIdentityRole
    {
        public MongoIdentityRole()
        {
            //Id = new MongoDocumentCounter<MongoIdentityRole>().Next().Value;
        }

        public MongoIdentityRole(string name) : this()
        {
            Name = name;
        }

        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public override string ToString() => Name;
    }
}
