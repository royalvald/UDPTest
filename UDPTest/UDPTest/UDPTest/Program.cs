using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace UDPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse("192.168.109.25");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8090);
            socket.Bind(iPEndPoint);
            socket.Listen(10);
            while (true)
            {
                Socket clientSocket = socket.Accept();
                Console.WriteLine("发送完毕");
                byte[] send = Encoding.UTF8.GetBytes("hello?");
                clientSocket.Send(send, SocketFlags.None);

                Console.WriteLine("发送完毕");
            }
        }
    }
}
