using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPClassLibrary
{
    [Serializable]// 注意这里的可序列化标志
    class ResendInfo
    {
        public int Key { get; set; }
        public EndPoint EndPoint { get; set; }
        public int Index { get; set; }
        public EndPoint MyEndPoint { get; set; }

        public ResendInfo(int Key, EndPoint EndPoint, int Index, EndPoint MyEndPoint)
        {
            this.Key = Key;
            this.EndPoint = EndPoint;
            this.Index = Index;
            this.MyEndPoint = MyEndPoint;
        }
    }

}
