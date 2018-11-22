
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace UDPTran
{
    [Serializable]
    class ReceiveData
    {
        public byte[] bytes;
        public EndPoint endPoint;

        public ReceiveData(byte[] bytes,EndPoint endPoint)
        {
            this.bytes = bytes;
            this.endPoint = endPoint;
        }
    }
}