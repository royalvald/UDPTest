using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UDPTran;
using System.Threading;

namespace UDPTestTCP
{
    class FileSend
    {
        //工具类和远程端口初始化
        PacketUtil packetUtil = new PacketUtil();
        private IPEndPoint remoteIPEndPoint;

        /// <summary>
        /// 传入远程端口
        /// </summary>
        /// <param name="endPoint"></param>
        public FileSend(IPEndPoint endPoint)
        {
            this.remoteIPEndPoint = endPoint;
        }

        public FileSend(string ip,int port=8010)
        {
            IPAddress address = IPAddress.Parse(ip);
            this.remoteIPEndPoint = new IPEndPoint(address, port);
        }


        public void SendFile(string filePath)
        {
            FileStream s = null;
            if (File.Exists(filePath))
                s = File.Open(filePath, FileMode.Open, FileAccess.Read);

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
                SendPacket(bytes, readSize, remoteIPEndPoint, ID, position, count);
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

        private void SendList(List<byte[]> list, EndPoint endPoint)
        {
            IPEndPoint RemoteIPEndPoint = (IPEndPoint)endPoint;
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket1.Bind(new IPEndPoint(hostIPEndPoint.Address, 8080));
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
    }
}
