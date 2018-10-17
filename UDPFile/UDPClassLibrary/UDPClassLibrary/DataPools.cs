using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPClassLibrary
{
    class DataPools
    {
        public int ExistPack { get; set; }
        public int TotalPack { get; set; }
        public EndPoint EndPoint { get; set; }
        public int LeftTime { get; set; }
        public List<byte[]> PacketParking { get; set; }


        public DataPools(int ExistPack, int TotalPack, EndPoint EndPoint, int LeftTime, List<byte[]> PacketParking)
        {
            this.ExistPack = ExistPack;
            this.TotalPack = TotalPack;
            this.EndPoint = EndPoint;
            this.LeftTime = LeftTime;
            this.PacketParking = PacketParking;
        }
    }

}
