using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Identity4.Mongo.Core
{
    public class Autoincrement : IAutoincrement
    {
        private readonly IMongoCollection<MongoDocumentCounter> _counters;
        private readonly string _entity;

        public Autoincrement(IMongoCollection<MongoDocumentCounter> counters, string entity)
        {
            _counters = counters;
            _entity = entity;
        }

        public int GetNext()
        {
            return _counters.FindOneAndUpdate(Builders<MongoDocumentCounter>.Filter.Eq("_id", _entity),
                Builders<MongoDocumentCounter>.Update.Inc(field => field.Value, 1),
                new FindOneAndUpdateOptions<MongoDocumentCounter>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                }
            ).Value;
        }
    }
}
