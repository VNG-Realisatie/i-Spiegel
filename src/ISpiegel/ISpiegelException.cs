using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel
{
    class ISpiegelException : Exception
    {
        public ISpiegelException(string message) : base(message)
        {         
        }
    }
}
