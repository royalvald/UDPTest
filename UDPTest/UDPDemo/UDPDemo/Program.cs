using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UDPDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("192.168.109.25");
            IPEndPoint endPoint = new IPEndPoint(address, 8090);
            UdpClient client = new UdpClient(endPoint);
            byte[] bytes = Encoding.UTF8.GetBytes("hello world");

            IPAddress address1 = IPAddress.Parse("192.168.109.75");
            IPEndPoint endPoint1 = new IPEndPoint(address1, 8090);

            client.Send(bytes, bytes.Length, endPoint1);
        }
    }
}
