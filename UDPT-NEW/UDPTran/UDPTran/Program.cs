﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace UDPTran
{
	class Program
	{
		static void Main(string[] args)
		{
            //Dispatcher dispatcher = new Dispatcher("192.168.109.75", 8090);

            //dispatcher.setHostIPEndPoint("192.168.109.22", 8090);
            //FileStream fs = new FileStream(@"H:\f1.pdf", FileMode.Open,FileAccess.Read);
            //byte[] bytes = new byte[fs.Length];
            //fs.Read(bytes, 0, (int)fs.Length);
            //fs.Close();
            FileStream stream = File.Open(@"E:\test.txt",FileMode.Open,FileAccess.Read);
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            Console.WriteLine(BitConverter.ToInt32(bytes,0));
            //dispatcher.InfoSend(bytes);
            //dispatcher.SendFile(fs);
            //bytes = null;
            //fs.Close();
            //Console.WriteLine("send completed");
            
		}
	}
}
