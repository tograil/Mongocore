using System;
using System.Collections.Generic;
using System.Text;

namespace Identity4.Mongo.Core
{
    public class MongoUserToken
    {
        public string LoginProvider { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
