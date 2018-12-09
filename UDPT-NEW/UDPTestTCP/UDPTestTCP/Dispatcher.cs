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

namespace UDPTestTCP
{
    class Dispatcher
    {

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
        public enum Info { receive, receiveEnd, send, sendEnd, retran, retranEnd, complete, refuse, OK }

        public static bool ReceiveContinue = true;

        //工具类使用
        private PacketUtil packetUtil = new PacketUtil();
        /// <summary>
        /// 传入需要连接的IP地址和端口号，以及设定本机通讯的IP地址和端口号
        /// </summary>
        /// <param name="hostIPAddress">本机地址</param>
        /// <param name="hostPort">本机端口</param>
        /// <param name="remoteIPAddress">远程地址</param>
        /// <param name="remotePort">远程端口</param>
        public Dispatcher(string hostIPAddress, string remoteIPAddress, int hostPort = 8090, int remotePort = 8090)
        {
            //初始化配置
            Initialization(hostIPAddress, remoteIPAddress, hostPort, remotePort);


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
            IPAddress address = IPAddress.Parse(hostIPAddress);
            this.hostEndPoint = new IPEndPoint(address, hostPort);
            IPAddress addressRemote = IPAddress.Parse(remoteIPAddress);
            this.remoteEndPoint = new IPEndPoint(addressRemote, remotePort);
            this.remoteAddress = addressRemote;
            this.remoteDataEndPoint = new IPEndPoint(addressRemote, 9000);
            //初始化UDPClient(默认9000端口)
            sendClient = new UdpClient(9000);
        }

        /// <summary>
        /// 主程序运行
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

        public void TCPService(object objects)
        {
            TcpClient tempClient = (TcpClient)objects;
            var stream = tempClient.GetStream();
            byte[] bytes = new byte[10];
            int readSize = 0;
            while (true)
            {
                readSize = stream.Read(bytes, 0, 2);
                if (readSize == 2)
                {
                    int tag = BitConverter.ToInt16(bytes, 0);
                    switch (tag)
                    {
                        case 1:
                            break;
                        case 4:
                            break;
                        case 5:
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Tcp控制文件传输
        /// </summary>
        private void TcpStartTran(object objects)
        {
            client = new TcpClient(new IPEndPoint(hostEndPoint.Address, 8060));
            byte[] infoBytes = new byte[2];
            int readSize = 0;
            client.Connect(remoteEndPoint);
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(InfoToBytes(Info.send), 0, 2);
                while ((readSize = stream.Read(infoBytes, 0, 2)) == 0) ;
                if (readSize == 2)
                {
                    switch (BitConverter.ToInt16(infoBytes, 0))
                    {
                        case 1:
                            {
                                //传入的是远程数据的接收节点
                                FileSend file = new FileSend(remoteDataEndPoint);
                                file.SendFile(@"H:\test.pdf");
                                stream.Write(InfoToBytes(Info.sendEnd), 0, 2);
                                Thread lostProcess = new Thread(ProcessLost);
                                lostProcess.Start(stream);
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
                case Info.receive:
                    i = 1;
                    break;
                case Info.receiveEnd:
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
        /// 子线程处理重传请求
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
                            case 9:
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
                                            if (tag == 2)
                                                fs.Close();
                                            Thread lostRetran = new Thread(SendPack);
                                            lostRetran.Start(stream);
                                            return;
                                        }
                                    }
                                }
                            //eak;
                            case 4:
                                break;
                            case 5:
                                return;
                        }
                    }
                }
            }
        }

        private void SendPack(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;
            byte[] commandBytes = new byte[2];
            int readSize = 0;
            stream.Write(InfoToBytes(Info.retran), 0, 2);

            //等待回传确认指令
            while (true)
            {
                if ((readSize = stream.Read(commandBytes, 0, 2)) == 2)
                {
                    int tag = BitConverter.ToInt16(commandBytes, 0);
                    if (tag == 2)
                        break;
                }

            }
            //基于重传文件信息进行文件重传
            UDPRetran(remoteEndPoint, "./templost", "");
            stream.Write(InfoToBytes(Info.retranEnd), 0, 2);
        }

        //根据临时文件缺损信息发送重传请求
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

                    fileStream.Position = 4;
                    fileStream.Read(indexBytes, 0, 4);
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


        //将数据片包装后发送出去，该函数主要目的时给数据片添加头部信息并发送
        private void SendRetranBytes(byte[] bytes, int ID, int index, int count, int contextLength)
        {
            byte[] headerBytes = packetUtil.CreatHeader(ID, index, count, contextLength);
            byte[] infoBytes = new byte[1040];
            Array.Copy(headerBytes, 0, infoBytes, 0, 16);
            Array.Copy(bytes, 0, infoBytes, 16, 1024);
            sendClient.Send(infoBytes, 1040, remoteDataEndPoint);
        }


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
                infoList.AddRange(infoBytes);
                if (infoList.Count > 5 * 1024 * 1024)
                {
                    Thread thread = new Thread(ByteToFile);
                    thread.Start(infoList.ToArray());
                    infoList = new List<byte>();
                }
            }
        }


        private void ByteToFile(object objects)
        {

        }

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
            }
        }

    }
}
