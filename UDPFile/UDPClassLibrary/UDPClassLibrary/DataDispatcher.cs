using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using SocketByUDPClassLibrary;

namespace UDPClassLibrary
{
    // 定义一个接受到数据时响应的委托
    public delegate void OnDataArrivedDelegate(byte[] in_listVariablePool);

    public class DataDispatcher
    {
        // 定义完成事件
        public event OnDataArrivedDelegate OnDataArrived;

        // 接收池                                            
        private Dictionary<int, DataPools> m_listInDataPools;

        // 发送池                                            
        private Dictionary<int, DataPools> m_listOutDataPools;

        // 本机IP地址
        private IPEndPoint m_ipSelfIP;

        // 接收时的服务变量
        private Socket m_SocketService;

        private Thread m_threadStartService;

        // 线程锁
        private Mutex m_mutexPick;

        // 并发缓解
        private int m_iPush = 1;

        // 等待时间(毫秒)
        private int m_iWait = 500;

        /// <summary>
        　　        /// 构造方法
        　　        /// </summary>
        public DataDispatcher(string in_strIPInfo)
        {
            // 获得IP
            m_ipSelfIP = StrToEndPoint(in_strIPInfo);

            // 数据池初始化
            m_listInDataPools = new Dictionary<int, DataPools>();
            m_listOutDataPools = new Dictionary<int, DataPools>();

            // 启动服务
            m_threadStartService = new Thread(StartService);
            m_threadStartService.Start();

            m_mutexPick = new Mutex();

            Thread threadTimer = new Thread(MyTime);
            threadTimer.Start();

        }

        /// <summary>
        　　        /// 启动服务
        　　        /// </summary>
        public void StartService()
        {
            // 使用Socket服务
            m_SocketService = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // 绑定网络地址
            m_SocketService.Bind(m_ipSelfIP);

            // 客户机IP
            IPEndPoint senderPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderEndPoint = (EndPoint)(senderPoint);

            // 数据大小
            int iDataSize;

            // 字节数据
            byte[] bData = new byte[2048];
            List<byte[]> listIncomeDatas = new List<byte[]>();
            Thread threadAllotPack;

            object objCustomParm;
            // 循环监听是否取到包
            while (true)
            {
                bData = new byte[2048];

                //接收信息
                iDataSize = m_SocketService.ReceiveFrom(bData, ref senderEndPoint);

                // 如果是数据包 接收,如果是重发请求包 重发
                if (iDataSize == 2048)
                {
                    // 自定义参数
                    objCustomParm = new CustomParm(bData, senderEndPoint);

                    // 添加线程
                    threadAllotPack = new Thread(AllotPack);

                    // 分配收到的包
                    threadAllotPack.Start(objCustomParm);

                }
                else
                {
                    // 根据信息重发
                    SendByInfo((ResendInfo)SetByteToValue(bData));

                    Console.WriteLine("收到请求包");
                }

            }
        }

        /// <summary>
        　　        /// 发送数据
        　　        /// </summary>
        　　        /// <param name="in_listToSendData"></param>
        　　        /// <param name="in_strDesIP"></param>
        public void Send(byte[] in_listToSendData, string in_strDesIP)
        {

            List<byte> listByte = new List<byte>();
            listByte.AddRange(in_listToSendData);
            // 设置目标IP，设置TCP端口号
            IPEndPoint ipEndPoint = StrToEndPoint(in_strDesIP);

            // 调用包组件
            ByePacketUtil stPacketUtil = new ByePacketUtil();

            // 声明包组变量
            List<byte[]> listToSendDatas = stPacketUtil.SplitPacket(listByte);

            // 添加到发送池
            m_listOutDataPools.Add(stPacketUtil.GetJointHeadID(listToSendDatas[0]), new DataPools(0, 0, null, 30000, listToSendDatas));

            foreach (var blistPackage in listToSendDatas)
            {
                //循环发送每个包
                byte[] bSends = new byte[2048];
                bSends = blistPackage.ToArray();

                // 发送
                m_SocketService.SendTo(bSends, bSends.Length, SocketFlags.None, ipEndPoint);

                // 缓解并发压力
                Thread.Sleep(m_iPush);

            }



        }


        private void SendByInfo(ResendInfo in_resendInfo)
        {
            int iKey = in_resendInfo.Key;
            int iIndex = in_resendInfo.Index;

            // 调用包组件
            ByePacketUtil packetUtil = new ByePacketUtil();

            if (m_listOutDataPools.ContainsKey(iKey))
            {
                foreach (var bItem in m_listOutDataPools[iKey].PacketParking)
                {
                    // 得到包的Index
                    int iPacketIndex = packetUtil.GetJointHeadIndex(bItem);
                    if (iPacketIndex == iIndex)
                    {
                        // 发送
                        m_SocketService.SendTo(bItem, bItem.Length, SocketFlags.None, in_resendInfo.MyEndPoint);
                    }
                }
            }

        }



        /// <summary>
        　　        /// 分配收到的包
        　　        /// </summary>
        　　        /// <param name="in_iData"></param>
        private void AllotPack(object in_objCustomParm)
        {

            byte[] listDataIn = ((CustomParm)in_objCustomParm).ParmBytes;
            EndPoint endPointIn = ((CustomParm)in_objCustomParm).ParmEndPoint;

            // 默认等待时间(毫秒)
            int iLeftDef = 30000;

            // 调用包组件
            ByePacketUtil packetUtil = new ByePacketUtil();

            // 得到包的ID
            int iPacketID = packetUtil.GetJointHeadID(listDataIn);

            // 得到包的Index
            int iPacketIndex = packetUtil.GetJointHeadIndex(listDataIn);

            // 得到包的份数
            int iPacketCount = packetUtil.GetJointHeadCount(listDataIn);

            m_mutexPick.WaitOne();
            Console.WriteLine("收到" + iPacketIndex + "/" + iPacketCount);
            // 操作接收池 
            if (m_listInDataPools.ContainsKey(iPacketID))
            {
                // 存在就存一条

                m_listInDataPools[iPacketID].PacketParking.Add(listDataIn);
                m_listInDataPools[iPacketID].ExistPack++;
                m_listInDataPools[iPacketID].LeftTime = iLeftDef;

            }
            else
            {
                // 不存在就新建一条
                List<byte[]> listTempDatas = new List<byte[]>();
                listTempDatas.Add(listDataIn);
                m_listInDataPools.Add(iPacketID, new DataPools(1, iPacketCount, endPointIn, iLeftDef, listTempDatas));
            }

            // 检查接收此包后是否有完整大包
            if (m_listInDataPools[iPacketID].PacketParking.Count() == iPacketCount)
            {
                // 临时变量池
                List<byte> listTempPool;

                // 若大包完整执行封包
                listTempPool = packetUtil.JointPacket(m_listInDataPools[iPacketID].PacketParking);

                if (OnDataArrived != null)
                {
                    // 事件抛出数据
                    OnDataArrived(listTempPool.ToArray());
                }


                Console.WriteLine("开始组包");

                // 删除相关数据
                RemoveByKey(iPacketID);
            }

            m_mutexPick.ReleaseMutex();


        }

        /// <summary>
        　　        /// 每1秒执行一次丢包检查
        　　        /// </summary>
        private void MyTime()
        {
            do
            {
                Thread.Sleep(m_iWait);
                CheckTimesIn();

            } while (true);
        }

        /// <summary>
        　　        ///  检查接收池包否完整,若超时则释放资源
        　　        /// </summary>
        private void CheckTimesIn()
        {
            foreach (int iItemKey in m_listInDataPools.Keys)
            {
                DataPools stValue = m_listInDataPools[iItemKey];
                stValue.LeftTime -= m_iWait;

                if (stValue.LeftTime <= 0)
                {
                    RemoveByKey(iItemKey);
                }
                else if (stValue.LeftTime <= 27000)
                {
                    if (stValue.ExistPack < stValue.TotalPack)
                    {
                        List<ResendInfo> listResendInfos = FindLostPack(iItemKey, m_listInDataPools[iItemKey].EndPoint);

                        // 请求重发
                        Resend(listResendInfos);
                    }
                }
            }
        }

        /// <summary>
        　　        ///  检查发送池包是否超时,若超时则释放资源
        　　        /// </summary>
        private void CheckTimesOut()
        {
            foreach (int iItemKey in m_listOutDataPools.Keys)
            {
                DataPools stValue = m_listOutDataPools[iItemKey];
                stValue.LeftTime -= m_iWait;

                if (stValue.LeftTime <= 0)
                {
                    m_listOutDataPools.Remove(iItemKey);
                }

            }
        }

        private void Resend(List<ResendInfo> in_listResendInfos)
        {
            foreach (var stResendInfo in in_listResendInfos)
            {
                // 缓解并发压力
                Thread.Sleep(m_iPush);

                byte[] bSends = SetValueToByte(stResendInfo);

                // 发送
                m_SocketService.SendTo(bSends, bSends.Length, SocketFlags.None, stResendInfo.EndPoint);

                Console.WriteLine("重发:" + stResendInfo.Index + "---" + stResendInfo.Key);
            }
        }

        /// <summary>
        　　        /// 转换出值字节(序列化)
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">值名字</param>
        　　        /// <returns></returns>
        public byte[] SetValueToByte(Object in_objValue)
        {
            var memoryStream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, in_objValue);
            byte[] buffer = memoryStream.ToArray();
            memoryStream.Close();
            return buffer;

        }

        /// <summary>
        　　        /// 反序列化
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">值名字</param>
        　　        /// <returns></returns>
        public Object SetByteToValue(byte[] in_listValue)
        {
            var memoryStream = new MemoryStream(in_listValue);
            var formatter = new BinaryFormatter();
            Object obj = formatter.Deserialize(memoryStream);
            memoryStream.Close();
            return obj;

        }

        /// <summary>
        　　        ///  生成重发请求列表
        　　        /// </summary>
        　　        /// <param name="in_iPackKey"></param>
        　　        /// <returns></returns>
        private List<ResendInfo> FindLostPack(int in_iPackKey, EndPoint in_EndPoint)
        {
            // 返回变量
            List<ResendInfo> listRes = new List<ResendInfo>();

            // 得到总包数
            int iCount = ((DataPools)m_listInDataPools[in_iPackKey]).TotalPack;

            // 总包数布尔值
            bool[] listAllTable = new bool[iCount];
            List<byte[]> listPools = m_listInDataPools[in_iPackKey].PacketParking;

            // 调用包组件
            ByePacketUtil packetUtil = new ByePacketUtil();
            foreach (byte[] listItem in listPools)
            {
                int i = packetUtil.GetJointHeadIndex(listItem);
                listAllTable[i] = true;
            }


            for (int i = 0; i < iCount; i++)
            {
                if (listAllTable[i] == false)
                {
                    listRes.Add(new ResendInfo(in_iPackKey, in_EndPoint, i, m_ipSelfIP));
                }
            }

            return listRes;
        }

        /// <summary>
        　　        /// 依据ID删除检查池的条目
        　　        /// </summary>
        　　        /// <param name="in_itemKey"></param>
        private void RemoveByKey(int in_itemKey)
        {
            m_listInDataPools.Remove(in_itemKey);
        }

        /// <summary>
        　　        /// 字符串转ip+端口
        　　        /// </summary>
        　　        /// <returns></returns>
        private IPEndPoint StrToEndPoint(string in_strIPandPort)
        {
            string strIP = in_strIPandPort.Split(':')[0];
            string strPort = in_strIPandPort.Split(':')[1];
            IPAddress stIP = IPAddress.Parse(strIP);
            IPEndPoint resEndPoint = new IPEndPoint(stIP, int.Parse(strPort));

            return resEndPoint;

        }
    }
}
