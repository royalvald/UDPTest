using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Dispatcher d1 = new Dispatcher("192.168.109.35", 0);
            Socket s1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] bytes = Encoding.UTF8.GetBytes("hello world");
            byte[] bytes1 = Encoding.UTF8.GetBytes("welcome");

            IPAddress iPAddress = IPAddress.Parse("192.168.109.35");
            IPEndPoint iPEndPoint1 = new IPEndPoint(iPAddress, 8080);
            IPEndPoint iPEndPoint2 = new IPEndPoint(iPAddress, 8090);
            
            while(true)
            {
                s1.SendTo(bytes, (EndPoint)iPEndPoint1);
                s1.SendTo(bytes1, (EndPoint)iPEndPoint2);
            }
        }
    }
}
