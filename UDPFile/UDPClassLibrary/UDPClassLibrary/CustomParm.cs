using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPClassLibrary
{
    class CustomParm
    {
        public byte[] ParmBytes { get; set; }
        public EndPoint ParmEndPoint { get; set; }

        public CustomParm(byte[] ParmBytes, EndPoint ParmEndPoint)
        {
            this.ParmBytes = ParmBytes;
            this.ParmEndPoint = ParmEndPoint;
        }

    }

}
