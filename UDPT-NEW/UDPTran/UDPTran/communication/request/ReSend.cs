using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTran
{
    [Serializable]
    class ReSend:Request
    {
        //重发数据包应该包括以下信息，重发的目的地以及大包的ID和大包内的数据小包的Index
        private EndPoint endPoint;
        private byte[] Info;
        private int packID;
        private int index;

        //重发请求包构造方法
        public ReSend(EndPoint endPoint,int PackID,int Index,RequestType requestType)
        {
            this.endPoint = endPoint;
            this.packID = PackID;
            this.index = Index;
            this.requestType = requestType;
        }

        public ReSend(EndPoint endPoint,byte[] bytes,RequestType requestType)
        {
            this.endPoint = endPoint;
            this.Info = bytes;
            this.requestType = requestType;
        }

        //设置属性的GET方法
        public EndPoint EndPoint { get => endPoint;  }
        public int PackID { get => packID;  }
        public int Index { get => index;  }
    }
}
