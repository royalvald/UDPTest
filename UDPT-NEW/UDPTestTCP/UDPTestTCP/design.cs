using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace UDPTestTCP
{
    class design
    {

        private byte[] bytes = new byte[2];

        private void Listen(Socket socket)
        {
            NetworkStream stream = new NetworkStream(socket);
            int readSize = 0;
            
            while(true)
            {
                Thread[] threads = new Thread[5];
                readSize= stream.Read(bytes, 0, 2);
                if(readSize>0)
                {
                    int tag = BitConverter.ToInt32(bytes, 0);
                    switch(tag)
                    {
                        case 1:
                            {
                                if (threads[1] == null)
                                {
                                    threads[1] = new Thread(Send);
                                    threads[1].Start();
                                }
                                else
                                {
                                    threads[1].Abort();
                                    threads[1] = null;
                                }
                            }
                            break;
                        case 2:
                            break;
                    }
                }
            }
        }

        private void Send()
        {
            
        }

        private void TcpSend(object objects)
        {
            NetworkStream stream = (NetworkStream)objects;

        }
    }
}
