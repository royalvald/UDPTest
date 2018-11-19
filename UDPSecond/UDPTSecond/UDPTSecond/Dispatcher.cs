using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UDPTSecond.requestAndResponse;
using System.Threading;

namespace UDPTSecond
{
    class Dispatcher
    {


        //本机传输数据socket
        private Socket hostSocket;
        //本机指令通信
        private TcpListener listener;
        private TcpClient client;
        //本机IP
        private IPEndPoint hostIPEndPoint;
        //远程IP
        private IPEndPoint remoteEndPoint;

        //分片信息长度
        private static int pieceLength = 2040;

        public Dispatcher(string IP, int port)
        {
            //远程地址配置
            IPAddress iPAddress = IPAddress.Parse(IP);
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
            this.remoteEndPoint = iPEndPoint;

            //本机地址配置
            int hostPort=0;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(ipAddress, hostPort);
            this.hostIPEndPoint = endPoint;
            hostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            hostSocket.Bind(hostIPEndPoint);

            //服务启动
            Thread thread = new Thread(Service);
            thread.Start();
        }

        //服务程序
        private void Service()
        {

        }

        public void SendFile(string filePath, EndPoint endPoint)
        {
            int FileLength, offset = 0, count;

            //单片文件长度
            int PackLength;

            //暂存数组以及加密数组
            byte[] bytes = new byte[2048];
            byte[] SecBytes;

            //取hash用
            MD5 mD5 = MD5.Create();


            if (File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                if (!(fs.Length > (long)int.MaxValue))
                    count = (int)fs.Length / pieceLength + 1;
                FileLength = (int)fs.Length;
                SecBytes = mD5.ComputeHash(fs);

                //此处应发送指令（关于传输内容，以及传输开始）
                TranStart tranStart = new TranStart(SecBytes, FileLength);
                SendPacket(PackUtil.ObjectToBytes(tranStart), endPoint);

                //接受指令

                //循环发送
                while (offset < FileLength)
                {
                    PackLength = GetSlice(fs, 0, bytes);
                    SendPacket(bytes, endPoint);
                    offset += PackLength;
                }

                fs.Close();

                //此处发送指令（文件传输结束提示进行拼包）
                TranEnd tranEnd = new TranEnd(SecBytes);
                SendPacket(PackUtil.ObjectToBytes(tranEnd), endPoint);

                //接受指令


                hostSocket.Close();
            }
        }


        /// <summary>
        /// 发送信息原子操作
        /// </summary>
        /// <param name="bytes">发送数据</param>
        /// <param name="endPoint">发送终端</param>
        public void SendPacket(byte[] bytes, EndPoint endPoint)
        {
            hostSocket.SendTo(bytes, bytes.Length, SocketFlags.None, endPoint);
        }

        /// <summary>
        /// 文件信息切片操作
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int GetSlice(FileStream fs, int start, byte[] bytes)
        {

            return fs.Read(bytes, start, 2040);


        }

        /// <summary>
        /// 发送指令信息
        /// </summary>
        /// <param name="tagRequest"></param>
        /// <param name="endPoint"></param>
        public void SendMSG(TagRequest tagRequest, EndPoint endPoint)
        {
            byte[] bytes = PackUtil.ObjectToBytes(tagRequest);
            hostSocket.SendTo(bytes, bytes.Length, SocketFlags.None, endPoint);
        }


       
    }
}
