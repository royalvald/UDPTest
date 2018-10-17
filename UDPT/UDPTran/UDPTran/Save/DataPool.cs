using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTran
{
    class DataPool
    {
        //包ID
        private int iDNumber;
        //包数据
        public Dictionary<int, byte[]> dic;
        //目的地址
        public EndPoint endPoint;
        //当前数据包收到总数
        private int count = 0;
        //剩余时间
        public int leftTime;
       

        public DataPool(int ID,Dictionary<int,byte[]> dic,EndPoint endPoint)
        {
            this.iDNumber = ID;
            this.dic = dic;
            this.endPoint = endPoint;
        }

        /// <summary>
        /// 含有等待时间的构造方法
        /// </summary>
        /// <param name="ID">数据包ID</param>
        /// <param name="dic">数据包分片相关数据</param>
        /// <param name="endPoint">远程端口号</param>
        /// <param name="leftTime">等待时间</param>
        public DataPool(int ID, Dictionary<int, byte[]> dic, EndPoint endPoint,int leftTime)
        {
            this.iDNumber = ID;
            this.dic = dic;
            this.endPoint = endPoint;
            this.leftTime = leftTime;
        }

        //当前数据池理论总数据包
        private int totalCount = 0;
        //只允许读取当前计数总量不允许修改
        public int Count { get => count; }
        public int TotalCount { get => totalCount;  }
        public int IDNumber { get => iDNumber;  }




        //添加数据

        /// <summary>
        /// 添加单个数据
        /// </summary>
        /// <param name="bytes"></param>

        public void AddBytes(byte[] bytes)
        {
            int ID;
            int Index;
            PacketUtil packetUtil = new PacketUtil();
            ID = packetUtil.GetID(bytes);
            Index = packetUtil.GetIndex(bytes);
            if (ID == IDNumber)
            {
                if (!dic.ContainsKey(Index))
                {
                    dic.Add(Index, bytes);
                    //如果数据包数据中信息大于4则为传输数据包所以可以获取其总数据包个数，否则为请求数据包
                    if (bytes.Length > 4)
                        GetTotalCount(bytes);
                }
            }
        }

        /// <summary>
        /// 添加List信息，一般是发送缓冲池调用，而接受池一般是调用另外的的AddByte
        /// </summary>
        /// <param name="list"></param>
        public void AddList(List<byte[]> list)
        {
            //将数据中ID和Endpoint信息写入
            PacketUtil packetUtil = new PacketUtil();
           // iDNumber = packetUtil.GetID(list[0]);
            
            int index;

            //此方法是添加list，调用前需清空dictionary
            dic.Clear();

            foreach (var item in list)
            {
                //获取index,和数据一起存入dictionary
                index = packetUtil.GetIndex(item);
                dic.Add(index, item);
            }
        }
        /// <summary>
        /// 获取总数据包数目
        /// </summary>
        /// <param name="bytes"></param>
        private void GetTotalCount(byte[] bytes)
        {
            PacketUtil packetUtil = new PacketUtil();
            totalCount = packetUtil.GetCount(bytes);
        }
        /// <summary>
        /// 当前数据包个数增加一个
        /// </summary>
        public void CountPlus()
        {
            count++;
        }

        public void RefreshTime()
        {
            this.leftTime = 30000;
        }

        public byte[] ReturnDic(int key)
        {
            return dic[key];
        }

    }
}
