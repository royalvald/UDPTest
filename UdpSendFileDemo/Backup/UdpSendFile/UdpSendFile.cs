using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace CSharpWin
{
    /* 作者：Starts_2000
     * 日期：2009-07-25
     * 网站：http://www.csharpwin.com CS 程序员之窗。
     * 你可以免费使用或修改以下代码，但请保留版权信息。
     * 具体请查看 CS程序员之窗开源协议（http://www.csharpwin.com/csol.html）。
     */

    public class UdpSendFile
    {
        private UdpClient _udpClient;
        private string _remoteIP = "127.0.0.1";
        private int _remotePort = 8900;
        private int _port = 8899;
        private bool _started;
        private string _fileName;
        SendFileManager _sendFileManage;

        public UdpSendFile(
            string remoteIP,
            int remotePort,
            int port) 
        {
            _remoteIP = remoteIP;
            _remotePort = remotePort;
            _port = port;
        }

        public string FileName
        {
            get { return _fileName; }
            set 
            {
                _fileName = value;
                if (_sendFileManage != null)
                {
                    _sendFileManage.Dispose();
                }
                _sendFileManage = new SendFileManager(_fileName);
                _sendFileManage.ReadFileBuffer += new ReadFileBufferEventHandler(
                    SendFileManageReadFileBuffer);
                TraFransfersFileStart ts = new TraFransfersFileStart(
                    new FileInfo(_fileName).Name,
                    _sendFileManage.Length,
                    _sendFileManage.PartCount,
                    _sendFileManage.PartSize);
                Send(0, ts);
            }
        }

        public UdpClient UdpClient
        {
            get
            {
                if (_udpClient == null)
                {
                    _udpClient = new UdpClient(_port);

                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    UdpClient.Client.IOControl(
                        (int)SIO_UDP_CONNRESET, 
                        new byte[] { Convert.ToByte(false) }, 
                        null);
                }
                return _udpClient;
            }
        }

        public IPEndPoint RemoteEP
        {
            get { return new IPEndPoint(IPAddress.Parse(_remoteIP), _remotePort); }
        }

        public void Start()
        {
            if (!_started)
            {
                _started = true;
                UdpClient.BeginReceive(
                    new AsyncCallback(ReceiveCallback),
                    null);
            }
        }

        private void SendFileManageReadFileBuffer(
            object sender, ReadFileBufferEventArgs e)
        {
            TraFransfersFile ts = new TraFransfersFile(
                e.Index, e.Buffer);
            Send(1, ts);
        }

        public void Send(int messageID, object data, IPEndPoint remoteIP)
        {
            SendCell cell = new SendCell(messageID, data);
            byte[] buffer = cell.ToBuffer();
            UdpClient.BeginSend(
                buffer,
                buffer.Length,
                remoteIP,
                new AsyncCallback(SendCallback),
                null);
        }

        public void Send(int messageID, object data)
        {
            Send(messageID, data, RemoteEP);
        }

        private void SendCallback(IAsyncResult result)
        {
            UdpClient.EndSend(result);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any,0);
            byte[] buffer = UdpClient.EndReceive(result, ref remoteEP);
            UdpClient.BeginReceive(
                    new AsyncCallback(ReceiveCallback),
                    null);
            SendCell cell = new SendCell();
            cell.FromBuffer(buffer);
            switch (cell.MessageID)
            {
                case 0:
                case 1:
                    _sendFileManage.Read();
                    break;
                case 2:
                    _sendFileManage.Dispose();
                    break;
            }
        }
    }
}
