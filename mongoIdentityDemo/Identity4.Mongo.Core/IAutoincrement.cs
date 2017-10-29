using System;
using System.Collections.Generic;
using System.Text;

namespace Identity4.Mongo.Core
{
    public interface IAutoincrement
    {
        int GetNext();
    }
}
