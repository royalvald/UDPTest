using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace UDPTSecond
{
    class FileSend
    {
        /// <summary>
        /// 传入文件地址和通讯socket
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="socket">通讯socket</param>
        /// <param name="Length">分片长度（建议1040）</param>
        public FileSend(string FilePath, Socket socket, int Length, EndPoint endPoint)
        {
            SendFile(FilePath, socket, Length, endPoint);
        }

        public void SendFile(string FilePath, Socket socket, int Length, EndPoint endPoint)
        {
            if (File.Exists(FilePath))
            {
                if (socket != null)
                {
                    FileStream stream = null;
                    try
                    {
                        stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                    }
                    catch (Exception e)
                    {

                    }
                    int packID = 65532;
                    int position = 0;
                    int readSize = 0;
                    int index = 0;
                    byte[] bufferRead = new byte[1040];

                    while (position < stream.Length)
                    {
                        readSize = stream.Read(bufferRead, 16, 1024);
                        PackUtil.CreatHeader(packID, index, PackUtil.GetPiecesNum((int)stream.Length, 1024), readSize, bufferRead);
                        socket.SendTo(bufferRead, SocketFlags.None, endPoint);
                        position += readSize;
                        index++;
                    }
                }
            }
        }
    }
}
