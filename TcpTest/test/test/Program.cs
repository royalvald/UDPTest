using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse("192.168.109.75");
            IPAddress iPAddress1 = IPAddress.Parse("192.168.109.25");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8090);
            IPEndPoint iPEndPoint1 = new IPEndPoint(iPAddress1, 8090);
            byte[] bytes = new byte[1024];
            socket.Bind(iPEndPoint1);
            socket.Connect(iPEndPoint);
            
            while(true)
            {
                socket.Receive(bytes);
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
            }
        }
    }
}
