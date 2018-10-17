using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTSecond.requestAndResponse
{
    class TranStart:TagRequest
    {
        //存储字节信息和文件长度信息
        private byte[] requestBytes;
        private int fileLength;

        public TranStart(byte[] bytes,int fileLength)
        {
            this.RequestBytes = bytes;
            this.FileLength = fileLength;
            this.RequestType = RequestType.TranStart;
        }

        public byte[] RequestBytes { get => requestBytes; set => requestBytes = value; }
        public int FileLength { get => fileLength; set => fileLength = value; }
    }
}
