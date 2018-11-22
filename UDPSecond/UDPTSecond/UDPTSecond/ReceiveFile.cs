using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPTSecond
{
    class ReceiveFile
    {
        private EndPoint receiveEndPoint;

        public ReceiveFile(string IP,int port)
        {
            IPAddress address = IPAddress.Parse(IP);
            this.receiveEndPoint = new IPEndPoint(address, port);

        }

        public void ReceiveFile()
    }
}
