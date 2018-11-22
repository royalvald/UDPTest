using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTran.Save
{
    class ResendPool
    {
        private EndPoint myEndPoint;
        private EndPoint remotePoint;
        public Dictionary<int, int> dic;
        private int iD;

        public EndPoint MyEndPoint { get => myEndPoint; set => myEndPoint = value; }
        public EndPoint RemotePoint { get => remotePoint; set => remotePoint = value; }

        public ResendPool(EndPoint myEndPoint,EndPoint remoteEndPoint,Dictionary<int,int> dic,int id)
        {
            this.MyEndPoint = myEndPoint;
            this.RemotePoint = remoteEndPoint;
            this.dic = dic;
            this.iD = id;
        }

       
      
    }
}
