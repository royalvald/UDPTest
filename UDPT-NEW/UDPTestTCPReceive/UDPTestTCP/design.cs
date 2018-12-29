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

        /*指令的设计应该有独立性，不可能在程序执行的过程中加入指令，因为程序执行过程中如果需要加入指令控制在不阻塞情况下就必须进行
                    添加异步程序，而异步处理程序在实现时候并不是能够按照顺序进行执行和控制，所以应该让各个指令分开控制。
                    因此采用设计如下 将各个指令分为如下 传输开始 传输结束 重传开始 重传结束 
                     */
        private byte[] bytes = new byte[2];


        //监听模块应该只负责监听指令通道不涉及文件传输（但可以传输文件缺少的信息）
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
                                    //
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
