using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPTest
{
    class Dispatcher
    {
        private IPEndPoint iPEndPoint1;
        private IPEndPoint iPEndPoint2;

        public Dispatcher(string s1, int port)
        {
            IPAddress iPAddress = IPAddress.Parse(s1);
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);

            //双端口监听
            //1号端口
            iPAddress = IPAddress.Parse("192.168.109.35");
            iPEndPoint = new IPEndPoint(iPAddress, 8080);
            Thread t1 = new Thread(Service);
            t1.Start(iPEndPoint);
            //2号端口
            iPAddress = IPAddress.Parse("192.168.109.35");
            iPEndPoint = new IPEndPoint(iPAddress, 8090);
            Thread t2 = new Thread(Service);
            t2.Start(iPEndPoint);
        }

        private void Service(object objects)
        {
            IPEndPoint iPEndPoint = (IPEndPoint)objects;
            Socket s1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s1.Bind(iPEndPoint);

            byte[] bytes = new byte[2048];
            int receiveSize;
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            EndPoint endPoint = (EndPoint)ipEndPoint;
            while(true)
            {
                receiveSize = s1.ReceiveFrom(bytes, ref endPoint);

                Console.WriteLine(Encoding.UTF8.GetString(bytes));
            }
        }
    }
}
