﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;

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
            byte[] infoBytes = new byte[4];
            int readSize= 0;
            client.Connect(remoteEndPoint);
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(InfoToBytes(Info.send), 0, 4);
                while ((readSize= stream.Read(infoBytes, 0, 4)) == 0) ;
                if(readSize==4)
                {
                    switch (BitConverter.ToInt32(infoBytes, 0))
                    {
                        case 1:
                            {
                                FileSend file = new FileSend(remoteEndPoint);
                                file.SendFile(@"H:\test.pdf");
                                stream.Write(InfoToBytes(Info.finshed), 0, 4);
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
            int i = 0;
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
    }
}
