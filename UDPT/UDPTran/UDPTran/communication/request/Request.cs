using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTran
{
    public enum RequestType { PreTag, ReSend };
    class Request : TranTag
    {
        public RequestType requestType;
    }
}
