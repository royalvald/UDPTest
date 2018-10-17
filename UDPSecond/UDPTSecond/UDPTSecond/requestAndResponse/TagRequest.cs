using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTSecond
{
    enum RequestType { TranStart,TranEnd};
    class TagRequest
    {
        //请求标记
        private RequestType requestType;

        internal RequestType RequestType { get => requestType; set => requestType = value; }
    }
}
