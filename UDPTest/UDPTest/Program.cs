using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;

namespace UDPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs = new FileStream(@"H:\f1.pdf", FileMode.Open, FileAccess.Read);
            TcpClient tcpClient = new TcpClient("192.168.109.75", 8090);
            byte[] bytes = new byte[1024];
            int readSize = 0;
            int fileLength = (int)fs.Length;
            int position=0;
            if(tcpClient.Connected)
            {
                NetworkStream stream = tcpClient.GetStream();
                while (position < fileLength)
                {
                    readSize = fs.Read(bytes, 0, 1024);
                    stream.Write(bytes, 0, readSize);
                    position += readSize;
                }

                stream.Close();
                fs.Close();
                tcpClient.Close();
                Console.WriteLine("finshed");
            }
        }
    }
}
