using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UDPTran;
using UDPTran.Save;

namespace UDPTran
{
    /// <summary>
    /// 基础沟通组件
    /// </summary>
    class Dispatcher
    {
        //ip地址
        private IPEndPoint hostIPEndPoint = null;

        private static object locker = new object();

        private static object lockerResend = new object();
        //通信socket
        private Socket socket;
        private Socket socketMsg;
        //private Socket socket1;
        //发送缓冲区
        private Dictionary<int, DataPool> sendOutPool;
        //接收缓冲区
        private Dictionary<int, DataPool> ReceivePool;
        //重发请求缓冲区
        private Dictionary<int, ResendPool> ResendBufferPool = new Dictionary<int, ResendPool>();

        //IP地址设定
        private IPEndPoint RemoteIPEndPoint;
        //private IPEndPoint RemotePoint;
        private Thread ServiceStart;

        //指令缓冲池
        private Dictionary<string, string> MsgBuffer = new Dictionary<string, string>();

        private DataPool dataPool;
        private ResendPool pool;

        private PacketUtil packetUtil = new PacketUtil();
        public Dispatcher(string IP, int port)
        {

            //初始化接受IP
            IPAddress iPAddress = IPAddress.Parse(IP);
            RemoteIPEndPoint = new IPEndPoint(iPAddress, port);

            IPAddress selfAddress = IPAddress.Parse("192.168.109.25");
            hostIPEndPoint = new IPEndPoint(selfAddress, 8090);

            //接收池与发送池初始化
            sendOutPool = new Dictionary<int, DataPool>();
            ReceivePool = new Dictionary<int, DataPool>();

            //服务启动
            ServiceStart = new Thread(Service);
            ServiceStart.Start();

            //丢包检查系统启动
            Thread checkThread = new Thread(CheckLostPack);
            checkThread.Start();

            //Thread thread1 = new Thread(MsgService);
            //thread1.Start();
        }

        public void setHostIPEndPoint(string IP, int port)
        {
            //自身IP初始化
            IPAddress selfAddress = IPAddress.Parse(IP);
            hostIPEndPoint = new IPEndPoint(selfAddress, 8090);
        }


        //服务开始
        private void Service(object objects)
        {
            //初始化socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(hostIPEndPoint);



            //设置监听的端口号
            IPEndPoint ReceiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint AbReceiveEndPoint = (EndPoint)ReceiveEndPoint;

            //存储一些关于数据包的信息
            byte[] TempInfo = new byte[1040];

            Thread PackProcess;
            int dataSize;
            object ReceiveTempData;


            while (true)
            {

                dataSize = socket.ReceiveFrom(TempInfo, ref AbReceiveEndPoint);
                byte[] infoByte = new byte[1040];
                TempInfo.CopyTo(infoByte, 0);
                if (dataSize == 1040)
                {
                    ReceiveTempData = new ReceiveData(infoByte, AbReceiveEndPoint);
                    PackProcess = new Thread(PacketProcess);
                    PackProcess.Start(ReceiveTempData);
                }
                else if (dataSize == 1008)
                {
                    ReceiveTempData = new ReceiveData(infoByte, AbReceiveEndPoint);
                    PackProcess = new Thread(ProcessArrayInfo);
                    PackProcess.Start(ReceiveTempData);
                }
                else
                {
                    ReceiveTempData = new ReceiveData(infoByte, AbReceiveEndPoint);
                    PackProcess = new Thread(ProcessRequest);
                    PackProcess.Start(ReceiveTempData);
                }

            }
        }


        /// <summary>
        /// 指令处理方式
        /// </summary>
        private void MsgService()
        {
            socketMsg = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socketMsg.Bind(new IPEndPoint(hostIPEndPoint.Address, 8060));

            int dataSize = -1;
            byte[] bufferArray = new byte[1024];

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint endPoint = (EndPoint)iPEndPoint;
            ReceiveData receiveData;

            while (true)
            {
                dataSize = socketMsg.ReceiveFrom(bufferArray, ref endPoint);
                if (dataSize > 0)
                {
                    byte[] temp = new byte[1024];
                    bufferArray.CopyTo(temp, 0);
                    receiveData = new ReceiveData(temp, endPoint);
                    Thread thread = new Thread(processMsg);
                    thread.Start(receiveData);
                }
            }
        }

        //收到的网络数据包分为两种，一种是包含文件信息数据，另外一种就是包含请求的，所以应该分开处理
        /// <summary>
        /// 受到数据包处理方式
        /// </summary>
        /// <param name="TempData"></param>
        private void PacketProcess(object TempData)
        {


            int PackID;
            Dictionary<int, int> PackCheck;
            DataPool dataPool = null;


            //获取包装类中的数据
            byte[] bytes = ((ReceiveData)TempData).bytes;
            EndPoint endPoint = ((ReceiveData)TempData).endPoint;


            //调用工具类处理数据包数据
            // PacketUtil packetUtil = new PacketUtil();
            //获取数据包的ID,用作分类,获取数据池
            PackID = packetUtil.GetID(bytes);
            int index = packetUtil.GetIndex(bytes);




            //先判断总数据池中有没有相关ID的数据池
            lock (locker)
            {

                if (ResendBufferPool.ContainsKey(PackID))
                {
                    RemoveResendPool(PackID, index);
                }
                if (ReceivePool.ContainsKey(PackID))
                {
                    dataPool = ReceivePool[PackID];

                    dataPool.AddBytes(bytes);
                    dataPool.CountPlus();
                    dataPool.RefreshTime();
                }
                else
                {
                    ReceivePool.Add(PackID, new DataPool(PackID, new Dictionary<int, byte[]>(), endPoint, 30000));
                    dataPool = ReceivePool[PackID];
                    dataPool.AddBytes(bytes);
                    dataPool.CountPlus();
                    dataPool.RefreshTime();
                }
            }


            //Console.WriteLine(dataPool.Count);

            //检测是否可以进行拼包操作

            if (dataPool.Count == dataPool.TotalCount)
            {
                if (packetUtil.TotalCheckBool(dataPool.dic))
                {


                    FileStream f1 = File.Create(@"H:\test.pdf");


                    int count = packetUtil.GetCount(dataPool.dic[0]);
                    int contextLength = packetUtil.GetContexLength(dataPool.dic[count - 1]);
                    int i = 0;
                    while (i < count - 1)
                    {
                        f1.Write(dataPool.dic[i], packetUtil.HeadLength, packetUtil.MaxContextLength);
                        i++;
                        if (i % 100 == 0)
                        {
                            f1.Flush();
                        }
                    }
                    f1.Write(dataPool.dic[count - 1], packetUtil.HeadLength, contextLength);

                    f1.Close();
                    Console.WriteLine("finshed");


                    //完成后删除相关接收缓冲池
                    ReceivePool.Remove(PackID);
                }
                else
                {
                    Console.WriteLine("数据发现冗余");
                    PackCheck = packetUtil.TotalCheck(dataPool.dic);
                    ProcessLostPacket(PackCheck, dataPool.endPoint);
                }

            }

        }



        /// <summary>
        /// 收到请求包的处理方式
        /// </summary>
        /// <param name="objects"></param>
        private void ProcessRequest(object objects)
        {
            //Console.WriteLine("pack lost processing");
            //获取数据
            byte[] bytes = ((ReceiveData)objects).bytes;
            EndPoint tempEndPoint = ((ReceiveData)objects).endPoint;
            IPEndPoint tempIPEndPoint = (IPEndPoint)tempEndPoint;
            IPEndPoint tran = new IPEndPoint(tempIPEndPoint.Address, 8090);

            processReByte(bytes, tran);
        }


        private void ProcessArrayInfo(object objects)
        {
            byte[] bytes = ((ReceiveData)objects).bytes;
            EndPoint tempEndPoint = ((ReceiveData)objects).endPoint;
            IPEndPoint tempIPEndPoint = (IPEndPoint)tempEndPoint;
            IPEndPoint tran = new IPEndPoint(tempIPEndPoint.Address, 8090);
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            FileStream fs = new FileStream(@"H:\f1.pdf", FileMode.Open, FileAccess.Read);

            //count 为请求的数量
            int[] Info = new int[252];
            int temp = 0;
            int packID, index;
            int position = 0;
            int count = BitConverter.ToInt32(bytes, 4);

            int fileCount;
            if (fs.Length % 1024 == 0)
                fileCount = (int)fs.Length / 1024;
            else fileCount = (int)fs.Length / 1024 + 1;

            while (position < 8 + count * 4 - 1)
            {
                Info[temp] = BitConverter.ToInt32(bytes, position);
                temp++;
                position += 4;
            }
            packID = Info[0];

            temp = 2;
            int readSize;
            byte[] fileByte = new byte[1024];
            while (temp < count + 2)
            {

                fs.Position = Info[temp] * 1024;
                readSize = fs.Read(fileByte, 0, 1024);
                byte[] processBytes = packetUtil.AddHead(fileByte, packID, Info[temp], fileCount, readSize);
                socket1.SendTo(processBytes, processBytes.Length, SocketFlags.None, tran);
                //socket1.Dispose();
                temp++;
                if (temp % 7 == 0)
                    Thread.Sleep(1);
            }

            fs.Close();
        }

        private void processReByte(byte[] bytes, EndPoint endPoint)
        {
            PacketUtil packetUtil = new PacketUtil();
            int id = packetUtil.GetID(bytes);
            int index = packetUtil.GetIndex(bytes);
            byte[] infoBytes = sendOutPool[id].dic[index];

            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket1.Bind(new IPEndPoint(hostIPEndPoint.Address, 8080));
            socket1.SendTo(infoBytes, infoBytes.Length, SocketFlags.None, endPoint);
            //socket1.Dispose();
            //Thread.Sleep(1);


            /*Console.WriteLine("已发送请求");
            Console.WriteLine(id);
            Console.WriteLine(index);*/
        }







        //发送二进制数据
        public void InfoSend(byte[] Info)
        {
            //首先调用工具类对数据进行分片,然后接受返回的list
            PacketUtil packetUtil = new PacketUtil();
            List<byte[]> InfoList = packetUtil.InfoToPacket(Info);

            //先发送头部信息
            //SendTag(InfoList[0], RemoteIPEndPoint);
            //获取大包ID放入发送缓冲池
            int ID = packetUtil.GetID(InfoList[0]);

            DataPool dataPool = new DataPool(ID, new Dictionary<int, byte[]>(), (EndPoint)RemoteIPEndPoint);

            dataPool.AddList(InfoList);
            sendOutPool.Add(ID, dataPool);
            Console.WriteLine(dataPool.dic.Count);
            Socket temp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] tempBytes = Encoding.UTF8.GetBytes("I will send file");

            temp.SendTo(tempBytes, tempBytes.Length, SocketFlags.None, new IPEndPoint(RemoteIPEndPoint.Address, 8060));

            bool check = true;
            /*while(check)
            {
                if (MsgBuffer.ContainsKey(RemoteIPEndPoint.Address.ToString()))
                {
                    if (MsgBuffer[RemoteIPEndPoint.Address.ToString()].IndexOf("roger") >= 0)
                    {
                        check = false;
                        MsgBuffer.Remove(RemoteIPEndPoint.Address.ToString());
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine("response received, starting transmission");
            */
            //发送数据
            Send(dataPool, (EndPoint)RemoteIPEndPoint);
        }

        public void SendFile(FileStream s)
        {
            int position = 0;
            byte[] bytes = new byte[4096000];
            int length = 0;
            int readSize;
            length = (int)s.Length;
            int ID = 6552;
            int count;
            if (length % 1024 == 0)
                count = length / 1024;
            else count = length / 1024 + 1;


            while (position < s.Length)
            {
                readSize = s.Read(bytes, 0, bytes.Length);
                SendPacket(bytes, readSize, RemoteIPEndPoint, ID, position, count);
                position += readSize;
            }

            s.Close();
        }

        private void SendPacket(byte[] bytes, int size, EndPoint endPoint, int ID, int position, int count)
        {
            byte[] tmpArray = new byte[size];
            Array.Copy(bytes, 0, tmpArray, 0, size);
            List<byte[]> list = packetUtil.InfoToPacket(tmpArray, ID, position, count);
            SendList(list, endPoint);
        }

        private void processMsg(object objects)
        {
            byte[] bytes = ((ReceiveData)objects).bytes;
            IPEndPoint endPoint = (IPEndPoint)(((ReceiveData)objects).endPoint);
            if (!MsgBuffer.ContainsKey(endPoint.Address.ToString()))
            {
                MsgBuffer.Add(endPoint.Address.ToString(), Encoding.UTF8.GetString(bytes));
                string s1 = Encoding.UTF8.GetString(bytes);
                Console.WriteLine(s1);
                Console.WriteLine("1111111111111111");
                /*数据处理*/

                Socket socketTemp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] infoBytes = Encoding.UTF8.GetBytes("roger");
                socketTemp.SendTo(infoBytes, infoBytes.Length, SocketFlags.None, new IPEndPoint(endPoint.Address, 8060));
                socketTemp.Dispose();
            }
            else
            {
                Socket socketTemp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] infoBytes = Encoding.UTF8.GetBytes("roger");
                socketTemp.SendTo(infoBytes, infoBytes.Length, SocketFlags.None, new IPEndPoint(endPoint.Address, 8060));
                socketTemp.Dispose();
            }
        }

        private bool checkMsg(byte[] bytes)
        {
            string s1 = Encoding.UTF8.GetString(bytes);
            if (s1.IndexOf("roger") >= 0)
                return true;
            else return false;
        }

        /// <summary>
        /// 填入相关信息即可放入到重发缓冲池
        /// </summary>
        /// <param name="packID"></param>
        /// <param name="index"></param>
        /// <param name="endPoint"></param>
        private void ResendProcess(int packID, int index, EndPoint endPoint, Socket socket1)

        {
            //重发请求只需要包含大包的ID和大包内的小包的索引
            byte[] InfoBytes = new byte[8];
            byte[] bytes;

            //对写入的ID和Index进行特殊处理
            int PID = packID;


            //写入ID
            bytes = BitConverter.GetBytes(PID);
            Array.Copy(bytes, 0, InfoBytes, 0, 4);

            //写入Index
            bytes = BitConverter.GetBytes(index);
            Array.Copy(bytes, 0, InfoBytes, 4, 4);


            IPEndPoint iPEndPoint = (IPEndPoint)endPoint;
            EndPoint tran = (EndPoint)(new IPEndPoint(iPEndPoint.Address, 8090));
            socket1.SendTo(InfoBytes, tran);
        }

        private void ResendArrayInfo(byte[] bytes, EndPoint endPoint, Socket socket1)
        {
            IPEndPoint iPEndPoint = (IPEndPoint)endPoint;
            EndPoint tran = (EndPoint)(new IPEndPoint(iPEndPoint.Address, 8090));
            socket1.SendTo(bytes, bytes.Length, SocketFlags.None, tran);
        }

        //发送单个信息的方法
        private void Send(DataPool dataPool, EndPoint endPoint)
        {
            IPEndPoint RemoteIPEndPoint = (IPEndPoint)endPoint;
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.Bind(new IPEndPoint(hostIPEndPoint.Address, 8080));
            int i = 0;
            foreach (var item in dataPool.dic)
            {
                socket1.SendTo(item.Value, item.Value.Length, SocketFlags.None, endPoint);
                //Thread.Sleep(1);
                if (i % 8 == 0)
                    Thread.Sleep(1);
                i++;
            }
            socket1.Dispose();
        }

        private void Send(byte[] bytes, EndPoint endPoint)
        {
            IPEndPoint RemoteIPEndPoint = (IPEndPoint)endPoint;
            socket.SendTo(bytes, bytes.Length, SocketFlags.None, endPoint);
        }

        private void SendList(List<byte[]> list, EndPoint endPoint)
        {
            IPEndPoint RemoteIPEndPoint = (IPEndPoint)endPoint;
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.Bind(new IPEndPoint(hostIPEndPoint.Address, 8080));
            int i = 0;
            foreach (var item in list)
            {
                socket1.SendTo(item, endPoint);
                i++;
                if (i % 8 == 0)
                    Thread.Sleep(1);
            }
            socket1.Dispose();
        }

        /// <summary>
        /// 发送整个缓冲池的数据
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="endPoint"></param>
        private void SendInfo(Dictionary<int, DataPool> dic)
        {
            foreach (var item in dic)
            {
                foreach (var Info in item.Value.dic)
                {
                    socket.SendTo(Info.Value, Info.Value.Length, SocketFlags.None, item.Value.endPoint);
                    //Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// 检查单个数据大包是否完整
        /// </summary>
        /// <param name="dataPool"></param>
        /// <returns></returns>
        public bool CheckPackComplete(DataPool dataPool)
        {
            //设置标记
            bool IsCompleted;
            if (dataPool.Count == dataPool.TotalCount)
                return IsCompleted = true;
            else return IsCompleted = false;
        }


        /// <summary>
        /// 拼包时候检测缺少的数据包进行重传处理
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="endPoint"></param>
        public void ProcessLostPacket(Dictionary<int, int> dic, EndPoint endPoint)
        {
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket1.Bind(new IPEndPoint(hostIPEndPoint.Address, 8080));
            int i = 0, packID = 0;
            bool isNull = true;
            byte[] resend;
            int tempCount = 4;
            List<int> list = new List<int>();
            foreach (var item in dic.Keys.ToList())
            {
                if (isNull)
                {
                    packID = dic[item];
                    isNull = false;
                }

                if (dic.ContainsKey(item))
                {
                    //ResendProcess(dic[item], item, endPoint, socket1);
                    //if (i % 5 == 0)
                    //    Thread.Sleep(1);
                    //i++;
                    //Thread.Sleep(1);

                    list.Add(item);
                    if (list.Count == 250)
                    {
                        tempCount = 8;
                        resend = new byte[1008];
                        foreach (var items in list)
                        {
                            Array.Copy(BitConverter.GetBytes(items), 0, resend, tempCount, 4);
                            tempCount += 4;
                        }
                        Array.Copy(BitConverter.GetBytes(dic[item]), 0, resend, 0, 4);
                        Array.Copy(BitConverter.GetBytes(list.Count), 0, resend, 4, 4);
                        ResendArrayInfo(resend, endPoint, socket1);
                        list.Clear();
                    }
                }
            }

            /* tempCount = 4;
             resend = new byte[1004];
             foreach (var items in list)
             {
                 if (isNull)
                 {
                     packID = dic[items];
                     isNull = false;
                 }
                 Array.Copy(BitConverter.GetBytes(items), 0, resend, tempCount, 4);
                 tempCount += 4;
             }
             Array.Copy(BitConverter.GetBytes(packID), 0, resend, 0, 4);
             ResendArrayInfo(resend, endPoint, socket1);*/

            tempCount = 8;
            resend = new byte[1008];
            foreach (var items in list)
            {
                Array.Copy(BitConverter.GetBytes(items), 0, resend, tempCount, 4);
                tempCount += 4;
                if (isNull)
                {
                    packID = dic[items];
                    isNull = false;
                }
            }
            Array.Copy(BitConverter.GetBytes(packID), 0, resend, 0, 4);
            Array.Copy(BitConverter.GetBytes(list.Count), 0, resend, 4, 4);
            ResendArrayInfo(resend, endPoint, socket1);
            list.Clear();

            socket1.Dispose();
        }


        /// <summary>
        /// 丢包检查
        /// </summary>
        public void CheckLostPack()
        {
            while (true)
            {
                //一秒一次查询过程
                Thread.Sleep(1000);
                CheckProcess();
            }
        }

        public void CheckProcess()
        {
            PacketUtil packetUtil = new PacketUtil();
            foreach (var item in ReceivePool)
            {
                if (item.Value.leftTime < 0)
                {
                    ReceivePool.Remove(item.Key);
                }

                if (ResendBufferPool.ContainsKey(item.Key))
                    ResendProcess(item.Value);

                if (item.Value.leftTime < 27000 && !ResendBufferPool.ContainsKey(item.Key))
                {

                    ResendProcess(item.Value);
                    Console.WriteLine("lost processing");
                    /*foreach (var items in packetUtil.TotalCheck(item.Value.dic))
                    {
                        Console.WriteLine(items.Value);
                        Console.WriteLine(items.Key);
                    }*/
                }



                item.Value.leftTime -= 1000;
            }

            //发送整个重发缓冲池的数据
            //SendInfo(ResendPool);
        }



        private void ResendProcess(DataPool dataPool)
        {
            int id = dataPool.IDNumber;
            ResendPool pool;
            if (ResendBufferPool.ContainsKey(id))
            {
                pool = ResendBufferPool[id];
                ProcessLostPacket(pool.dic, pool.RemotePoint);
            }
            else
            {
                pool = new ResendPool(this.hostIPEndPoint, dataPool.endPoint, packetUtil.TotalCheck(dataPool.dic), dataPool.IDNumber);
                ResendBufferPool.Add(dataPool.IDNumber, pool);
                ProcessLostPacket(pool.dic, pool.RemotePoint);
            }
        }


        private void RemoveResendPool(int packID, int index)
        {

            ResendBufferPool[packID].dic.Remove(index);

            if (ResendBufferPool[packID].dic.Count == 0)
                ResendBufferPool.Remove(packID);
        }
    }
}

