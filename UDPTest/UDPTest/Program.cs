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
        static TcpClient tcpClient = new TcpClient();
        static void Main(string[] args)
        {


            AsyncCallback callback = new AsyncCallback(BeginConnect);

            tcpClient.BeginConnect(@"192.168.109.75", 8090, callback, tcpClient);
            while (true) ;

        }

        static void BeginConnect(IAsyncResult ar)
        {
            FileStream fs = new FileStream(@"H:\f1.pdf", FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[1024];
            int readSize = 0;
            int fileLength = (int)fs.Length;
            int position = 0;
            TcpClient client = (TcpClient)ar.AsyncState;
            client.EndConnect(ar);
            if (tcpClient.Connected)
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
