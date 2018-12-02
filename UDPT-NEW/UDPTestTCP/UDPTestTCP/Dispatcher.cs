using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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
        /// 远程端口
        /// </summary>
        private IPEndPoint remoteEndPoint;

        public enum Info { receive, finshed, send, retran }

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
        }

        /// <summary>
        /// Tcp控制文件传输
        /// </summary>
        private void TcpStartTran()
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
                                FileSend file = new FileSend(remoteEndPoint);
                                file.SendFile(@"H:\test.pdf");
                                stream.Write(InfoToBytes(Info.finshed), 0, 2);
                                Thread lostProcess = new Thread(ProcessLost);
                                lostProcess.Start(stream);
                            }
                            break;
                        case 2:
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
                case Info.finshed:
                    i = 2;
                    break;
                case Info.receive:
                    i = 1;
                    break;
                case Info.retran:
                    i = 4;
                    break;
                case Info.send:
                    i = 3;
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
                    if (readSize == 4)
                    {
                        int tag = BitConverter.ToInt32(infoBytes, 0);
                        switch (tag)
                        {
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3:
                                {
                                    FileStream fs = File.Create("./templost");
                                    while (true)
                                    {
                                        if ((readSize = stream.Read(dataBytes, 0, 1024)) > 2)
                                        {
                                            fs.Write(dataBytes, 0, readSize);
                                        }
                                        if((readSize = stream.Read(dataBytes, 0, 1024)) ==2)
                                        {
                                            tag = BitConverter.ToInt16(dataBytes, 0);
                                            if(tag==2)
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
                        }
                    }
                }
            }
        }

        private void SendPack(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;
            
            UDPRetran(remoteEndPoint, "./templost", "");
            stream.Write(InfoToBytes(Info.finshed), 0, 2);
        }

        //根据临时文件缺损信息发送重传请求
        private void UDPRetran(EndPoint endPoint,string tempFilePath,string sendFilePath)
        {
            if (File.Exists(tempFilePath))
            {
                if (File.Exists(sendFilePath))
                {
                    //临时文件
                    FileStream tempStream = File.Open(tempFilePath, FileMode.Open, FileAccess.Read);
                    //待发送文件
                    FileStream fileStream = File.Open(sendFilePath, FileMode.Open, FileAccess.Read);
                    int position = 4;
                    int index = 0;
                    byte[] indexBytes = new byte[4];
                    byte[] infoBytes;
                    tempStream.Position = position;
                    while (position<tempStream.Length)
                    {
                        tempStream.Read(indexBytes, 0, 4);
                        index = BitConverter.ToInt32(indexBytes, 0);
                        fileStream.Position = index * 1024;
                        infoBytes = new byte[1024];
                        fileStream.Read(infoBytes, 0, 1024);
                        SendRetranBytes(infoBytes);
                    }
                    
                    tempStream.Close();
                    fileStream.Close();
                }
            }
            
        }


        //将数据片包装后发送出去，该函数主要目的时给数据片添加头部信息并发送
        private void SendRetranBytes(object objects)
        {
            
        }
    }
}
