
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace UDPTran
{
    [Serializable]
    class PreTag:Request
    {
        //发送数据的头信息，主要包括目的IP，自身IP，数据包ID
        private EndPoint myEndPoint;
        private EndPoint remoteEndPoint;
        private int iD;

        
        public PreTag(EndPoint MyEndPoint,EndPoint RemoteEndPoint,int ID,RequestType requestType)
        {
            this.myEndPoint = MyEndPoint;
            this.remoteEndPoint = RemoteEndPoint;
            this.iD = ID;
            this.requestType = requestType;
        }

        public EndPoint MyEndPoint { get => myEndPoint; }
        public EndPoint RemoteEndPoint { get => remoteEndPoint;  }
        public int ID { get => iD; }
    }
}