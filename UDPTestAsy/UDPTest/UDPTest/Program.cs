using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace UDPTest
{
	class Program
	{
		static void Main(string[] args)
		{
			IPAddress iPAddress = IPAddress.Parse("192.168.109.75");
			IPEndPoint endPoint = new IPEndPoint(iPAddress, 8090);
			TcpListener listener = new TcpListener(endPoint);
			NetworkStream stream;
			FileStream fs = new FileStream("/home/charmer/test1.pdf", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			byte[] buffer = new byte[1024];
			listener.Start(10);
			int size;
			//TcpClient client = listener.AcceptTcpClient();

			AsyncCallback callback = new AsyncCallback(Accept);
			while (true)
			{
				listener.BeginAcceptTcpClient(callback, listener);
			}
			void Accept(IAsyncResult ar)
			{
				TcpListener listener1 = (TcpListener)ar.AsyncState;
				TcpClient client = listener1.EndAcceptTcpClient(ar);
				if (client.Connected)
                {
                    Console.WriteLine("已经连接");
                    stream = client.GetStream();

                    while ((size = stream.Read(buffer, 0, 1024)) > 0)
                        fs.Write(buffer, 0, size);
                    fs.Close();
                    Console.WriteLine("finshed");
                }
				else
                {
                    fs.Close();
                    Console.WriteLine("finshed");
                }

			}

				
				


		}
	}
}
