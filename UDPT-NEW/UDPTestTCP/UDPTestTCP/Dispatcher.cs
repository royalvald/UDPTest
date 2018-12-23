using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UDPTran;
using UDPTestTCP.Folder;

namespace UDPTestTCP
{
    class Dispatcher
    {
        #region 初始化组件设置

        //通讯用tcp(默认listener用主机端口，而client用主机地址+8060端口)
        private TcpListener listener;
        private TcpClient client;
        //主机端口和远程端口
        /// <summary>
        /// 主机端口
        /// </summary>
        private IPEndPoint hostEndPoint;
        /// <summary>
        /// 远程端口指令端口
        /// </summary>
        private IPEndPoint remoteEndPoint;

        private IPEndPoint remoteDataEndPoint;
        private IPAddress remoteAddress;

        private UdpClient sendClient;

        /// <summary>
        /// 指令控制所用的几个指令信息
        /// </summary>
        private enum Info { reSend, reSendEnd, send, sendEnd, retran, retranEnd, complete, refuse, OK }


        /// <summary>
        /// 标记主程序以什么方式运行，是发送方还是接收方
        /// </summary>
        public enum Pattern { receive, send }
        //判断UDP文件传输的标记
        private static bool ReceiveContinue = true;

        private Dictionary<int, List<byte>> bufferInfo = new Dictionary<int, List<byte>>();

        //工具类使用
        private PacketUtil packetUtil = new PacketUtil();

        #endregion

        #region 路径设置
        //临时接收文件存储位置
        private string ReceiveSavePath { set; get; }= "F:/test.pdf";
        //临时重传信息文件存放位置
        private string tempLostInfoPath { set; get; } = "F:/test.info";
        //发送文件路径
        private string SendFilePath { set; get; } = "H:/test.pdf";
        #endregion

        /// <summary>
        /// 传入需要连接的IP地址和端口号，以及设定本机通讯的IP地址和端口号
        /// </summary>
        /// <param name="hostIPAddress">本机地址</param>
        /// <param name="hostPort">本机端口</param>
        /// <param name="remoteIPAddress">远程地址</param>
        /// <param name="remotePort">远程端口</param>
        public Dispatcher(Pattern pattern, string hostIPAddress, string remoteIPAddress, int hostPort = 8090, int remotePort = 8090)
        {
            //初始化配置
            Initialization(hostIPAddress, remoteIPAddress, hostPort, remotePort);

            //对于启动的模式进行判断，分别对应不同启动程序
            if (pattern == Pattern.receive)
                Service();//接收端程序
            else if (pattern == Pattern.send)
                TcpStartTran();//发送端程序
        }

        /// <summary>
        /// 初始化端口配置信息
        /// </summary>
        /// <param name="hostIPAddress"></param>
        /// <param name="remoteIPAddress"></param>
        /// <param name="hostPort"></param>
        /// <param name="remotePort"></param>
        private void Initialization(string hostIPAddress, string remoteIPAddress, int hostPort, int remotePort)
        {
            //本机TCP终端节点赋值
            IPAddress address = IPAddress.Parse(hostIPAddress);
            this.hostEndPoint = new IPEndPoint(address, hostPort);
            //远程TCP终端节点赋值
            IPAddress addressRemote = IPAddress.Parse(remoteIPAddress);
            this.remoteEndPoint = new IPEndPoint(addressRemote, remotePort);
            this.remoteAddress = addressRemote;
            //远程UDP终端节点赋值
            this.remoteDataEndPoint = new IPEndPoint(addressRemote, 9000);
            //初始化UDPClient(默认9000端口)
            sendClient = new UdpClient(9000);

            //初始化buffer配置
            bufferInfo.Add(0, new List<byte>());
        }

        /// <summary>
        /// 接收端主程序运行
        /// </summary>
        private void Service()
        {
            listener = new TcpListener(hostEndPoint);
            listener.Start(10);
            while (true)
            {
                TcpClient tempClient = listener.AcceptTcpClient();
                Thread thread = new Thread(TCPService);
                thread.Start(tempClient);
            }
        }

        /// <summary>
        /// 文件接收端的主程序
        /// </summary>
        /// <param name="objects"></param>
        public void TCPService(object objects)
        {
            TcpClient tempClient = (TcpClient)objects;
            var stream = tempClient.GetStream();
            byte[] bytes = new byte[10];
            int readSize = 0;

            //用线程去调用方便子程序结束以及资源回收
            Thread[] threads = new Thread[3];
            threads[1] = new Thread(ReceiveService);
            threads[2] = new Thread(SendRetranInfo);

            while (true)
            {
                readSize = stream.Read(bytes, 0, 2);
                if (readSize == 2)
                {
                    int tag = BitConverter.ToInt16(bytes, 0);

                    switch (tag)
                    {
                        case 3:
                            if(!(threads[1].ThreadState==ThreadState.Running))
                            threads[1].Start(stream);
                            break;
                        case 4:
                            ReceiveContinue = false;
                            CheckReceive(ReceiveSavePath);
                            ReceiveContinue = true;
                            threads[2].Start(stream);
                            //SendRetranInfo(stream, "0", "");
                            break;
                        case 5:
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// 发送端Tcp控制文件传输
        /// </summary>
        private void TcpStartTran()
        {
            //初始化连接
            client = new TcpClient(new IPEndPoint(hostEndPoint.Address, 8060));
            byte[] infoBytes = new byte[2];
            int readSize = 0;
            client.Connect(remoteEndPoint);
            if (client.Connected)
            {
                //开始指令传输
                NetworkStream stream = client.GetStream();
                stream.Write(InfoToBytes(Info.send), 0, 2);
                while ((readSize = stream.Read(infoBytes, 0, 2)) == 0) ;
                if (readSize == 2)
                {
                    switch (BitConverter.ToInt16(infoBytes, 0))
                    {
                        case 9:
                            {
                                //传入的是远程数据的接收节点
                                FileSend file = new FileSend(remoteDataEndPoint);
                                file.SendFile(@"H:\test.pdf");                             
                                Thread lostProcess = new Thread(ProcessLost);
                                lostProcess.Start(stream);
                                stream.Write(InfoToBytes(Info.sendEnd), 0, 2);
                            }
                            break;
                        case 8:
                            break;
                        case 3:
                            break;
                        case 4:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 将指令转化为字节信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private byte[] InfoToBytes(Info info)
        {
            short i = 0;
            switch (info)
            {
                case Info.reSend:
                    i = 1;
                    break;
                case Info.reSendEnd:
                    i = 2;
                    break;
                case Info.send:
                    i = 3;
                    break;
                case Info.sendEnd:
                    i = 4;
                    break;
                case Info.complete:
                    i = 5;
                    break;
                case Info.retran:
                    i = 6;
                    break;
                case Info.retranEnd:
                    i = 7;
                    break;
                case Info.refuse:
                    i = 8;
                    break;
                case Info.OK:
                    i = 9;
                    break;
            }
            return BitConverter.GetBytes(i);
        }


        /// <summary>
        /// 发送端子线程处理重传请求
        /// </summary>
        /// <param name="objects"></param>
        private void ProcessLost(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;
            int readSize = 0;
            byte[] infoBytes = new byte[4];
            byte[] dataBytes = new byte[1040];
            while (true)
            {
                if (stream.CanRead)
                {
                    readSize = stream.Read(infoBytes, 0, 2);
                    if (readSize == 2)
                    {
                        int tag = BitConverter.ToInt16(infoBytes, 0);
                        switch (tag)
                        {
                            case 1:
                                break;
                            case 2:
                                break;
                            case 6:
                                {
                                    FileStream fs = File.Create("./templost");
                                    while (true)
                                    {
                                        readSize = stream.Read(dataBytes, 0, 1024);
                                        if (readSize > 2)
                                        {
                                            fs.Write(dataBytes, 0, readSize);
                                        }
                                        else if (readSize == 2)
                                        {
                                            tag = BitConverter.ToInt16(dataBytes, 0);
                                            if (tag == 7)
                                                fs.Close();
                                            Thread lostRetran = new Thread(SendPack);
                                            lostRetran.Start(stream);
                                            break;
                                        }
                                    }
                                }
                                break;
                            case 4:
                                break;
                            case 5:
                                return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送端基于重传文件信息进行文件重传
        /// </summary>
        /// <param name="objects"></param>
        private void SendPack(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;
            byte[] commandBytes = new byte[2];
            int readSize = 0;
            stream.Write(InfoToBytes(Info.reSend), 0, 2);

            //等待回传确认指令
            while (true)
            {
                if ((readSize = stream.Read(commandBytes, 0, 2)) == 2)
                {
                    int tag = BitConverter.ToInt16(commandBytes, 0);
                    if (tag == 9)
                        break;
                }

            }
            //基于重传文件信息进行文件重传
            UDPRetran(remoteEndPoint, "./templost", SendFilePath);
            stream.Write(InfoToBytes(Info.reSendEnd), 0, 2);
        }

        //发送端根据临时文件缺损信息发送重传文件片段
        private void UDPRetran(EndPoint endPoint, string tempFilePath, string sendFilePath)
        {
            if (File.Exists(tempFilePath))
            {
                if (File.Exists(sendFilePath))
                {
                    //打开临时文件（发送文件缺少信息）
                    FileStream tempStream = File.Open(tempFilePath, FileMode.Open, FileAccess.Read);
                    //待发送文件
                    FileStream fileStream = File.Open(sendFilePath, FileMode.Open, FileAccess.Read);
                    int position = 8;
                    int index = 0;
                    int readSize = 0;
                    byte[] indexBytes = new byte[4];

                    tempStream.Position = 4;
                    tempStream.Read(indexBytes, 0, 4);
                    int packCount = BitConverter.ToInt32(indexBytes, 0);


                    byte[] infoBytes;
                    infoBytes = new byte[1024];
                    tempStream.Position = position;
                    while (position < tempStream.Length)
                    {
                        tempStream.Read(indexBytes, 0, 4);
                        index = BitConverter.ToInt32(indexBytes, 0);
                        fileStream.Position = index * 1024;
                        // infoBytes = new byte[1024];
                        readSize = fileStream.Read(infoBytes, 0, 1024);
                        SendRetranBytes(infoBytes, 65536, index, packCount, readSize);
                    }

                    tempStream.Close();
                    fileStream.Close();
                }
            }

        }


        //发送端将数据片包装后发送出去，该函数主要目的时给数据片添加头部信息并发送
        private void SendRetranBytes(byte[] bytes, int ID, int index, int count, int contextLength)
        {
            byte[] headerBytes = packetUtil.CreatHeader(ID, index, count, contextLength);
            byte[] infoBytes = new byte[1040];
            Array.Copy(headerBytes, 0, infoBytes, 0, 16);
            Array.Copy(bytes, 0, infoBytes, 16, 1024);
            sendClient.Send(infoBytes, 1040, remoteDataEndPoint);
        }

        /// <summary>
        /// 接收端UDP数据包接收
        /// </summary>
        /// <param name="objects"></param>
        private void ReceiveService(object objects)
        {
            var stream = (NetworkStream)objects;

            stream.Write(InfoToBytes(Info.OK), 0, 2);

            byte[] infoBytes = null;

            List<byte> infoList = new List<byte>();

            //接收UDP数据报
            while (true)
            {
                infoBytes = sendClient.Receive(ref remoteDataEndPoint);
                TempProcessInfo(infoBytes);
            }
        }

        /// <summary>
        /// 接收端处理刚刚从UDP端口获得的数据
        /// </summary>
        /// <param name="bytes"></param>
        private void TempProcessInfo(byte[] bytes)
        {
            List<byte> infoList = bufferInfo[0];
            //此处可能会出现list添加的元素全都和最后一个添加的数组元素相同，
            //最保险的方法是应该重新分配数组的内存并拷贝原始数据
            infoList.AddRange(bytes);
            if (infoList.Count > 5 * 1024 * 1024)
            {
                Thread thread = new Thread(ByteToFile);
                thread.Start(infoList.ToArray());
                bufferInfo[0] = new List<byte>();
            }
        }

        private void ByteToFile(object objects)
        {
            byte[] bytes = (byte[])objects;
            ByteToFile(bytes, ReceiveSavePath);
        }

        /// <summary>
        /// 接收端数据写入缓存
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="filePath"></param>
        private void ByteToFile(byte[] bytes, string filePath)
        {
            if (File.Exists(filePath))
            {
                FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
            else
            {
                FileStream fs = File.Create(filePath);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }

        //接收端检测文件接收是否结束，结束后立刻关闭标记，然后将剩余数据写入磁盘
        private void CheckReceive(string savePath)
        {
            // while (true)
            {
                if (ReceiveContinue == false)
                {
                    ByteToFile(bufferInfo[0].ToArray(), savePath);
                    bufferInfo[0] = null;
                }
            }
        }


        #region 重传信息发送操作
        /// <summary>
        /// 接收端根据文件缺失信息发送重传请求子程序(第一次传输完进行的检查)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="info"></param>
        private void SendRetranInfo(NetworkStream stream, FileCheckInfo info, string tempInfoSavepath)
        {

            List<byte> sendList = new List<byte>();
            List<int> lackPieces = info.lackPieces;


            sendList.AddRange(BitConverter.GetBytes(info.PackId));
            sendList.AddRange(BitConverter.GetBytes(info.Count));
            foreach (var item in lackPieces)
            {
                sendList.AddRange(BitConverter.GetBytes(item));
            }

            int position = 0;
            byte[] infoBytes = sendList.ToArray();

            //创建临时信息文件（包含文件缺失片段信息）
            FileStream fs = File.Create(tempInfoSavepath);
            fs.Write(infoBytes, 0, infoBytes.Length);
            fs.Close();

            while (position < infoBytes.Length)
            {
                if (position + 1024 < infoBytes.Length)
                {
                    stream.Write(infoBytes, position, 1024);
                    position += 1024;
                }
                else
                {
                    stream.Write(infoBytes, position, infoBytes.Length - position);
                    position = infoBytes.Length;
                }
            }

            stream.Write(InfoToBytes(Info.retranEnd), 0, 2);
        }

        /// <summary>
        /// 接收端根据文件缺失信息发送重传请求主程序(第一次传输完进行的检查)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="tempLostInfoSavepath"></param>
        private void SendRetranInfo(NetworkStream stream, string tempFilePath, string tempLostInfoSavepath)
        {
            FileCheckInfo checkInfo = packetUtil.FileCheck(tempFilePath);
            if (checkInfo.lackPieces.Count == 0)
                stream.Write(InfoToBytes(Info.complete), 0, 2);
            else
                SendRetranInfo(stream, checkInfo, tempLostInfoSavepath);
        }

        private void SendRetranInfo(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;
            string tempFilePath = ReceiveSavePath;
            string tempLostInfoSavepath = tempLostInfoPath;
            SendRetranInfo(stream, tempFilePath, tempLostInfoSavepath);
        }

        #endregion


        private void UpdateLostInfo(byte[] bytes,string lostInfoSavePath)
        {
            if(File.Exists(lostInfoSavePath))
            {

            }
        }
    }
}
