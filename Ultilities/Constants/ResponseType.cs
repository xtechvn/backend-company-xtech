using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultilities.Constants
{
    
    public enum ResponseType
    {
        SUCCESS = 0,
        FAILED = 1,
        ERROR = 2,
        EXISTS = 3,
        PROCESSING = 4,
        EMPTY = 5,
        CONFIRM = 6,
        NOT_EXISTS = 7
    }
}
