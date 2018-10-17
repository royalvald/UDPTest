using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTSecond.requestAndResponse
{
    class TranEnd : TagRequest
    {
        private byte[] requestBytes;

        public TranEnd(byte[] bytes)
        {
            this.requestBytes = bytes;
            this.RequestType = RequestType.TranEnd;
        }
    }
}
