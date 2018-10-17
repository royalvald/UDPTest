using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace CSharpWin
{
    /* 作者：Starts_2000
     * 日期：2009-07-25
     * 网站：http://www.csharpwin.com CS 程序员之窗。
     * 你可以免费使用或修改以下代码，但请保留版权信息。
     * 具体请查看 CS程序员之窗开源协议（http://www.csharpwin.com/csol.html）。
     */

    public class UdpRecieveFile
    {
        private UdpClient _udpClient;
        private int _port = 8900;
        private bool _started;
        private string _path;
        private ReceiveFileManager _receiveFileManager;

        public UdpRecieveFile(string path, int port)
        {
            _path = path;
            _port = port;
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

        public void Start()
        {
            if (!_started)
            {
                UdpClient.BeginReceive(
                    new AsyncCallback(ReceiveCallback),
                    null);
                _started = true;
            }
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

        private void SendCallback(IAsyncResult result)
        {
            UdpClient.EndSend(result);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = UdpClient.EndReceive(result, ref remoteEP);
            UdpClient.BeginReceive(
                    new AsyncCallback(ReceiveCallback),
                    null);
            SendCell cell = new SendCell();
            cell.FromBuffer(buffer);
            switch (cell.MessageID)
            {
                case 0:
                    OnStartRecieve((TraFransfersFileStart)cell.Data, remoteEP);
                    break;
                case 1:
                    OnRecieveBuffer((TraFransfersFile)cell.Data, remoteEP);
                    break;
            }
        }

        private void OnRecieveBuffer(
            TraFransfersFile traFransfersFile,
            IPEndPoint remoteEP)
        {
            _receiveFileManager.ReceiveBuffer(
                traFransfersFile.Index,
                traFransfersFile.Buffer);
            if (_receiveFileManager.PartCount == traFransfersFile.Index + 1)
            {
                Send(2, "OK", remoteEP);
            }
            else
            {
                Send(1, "OK", remoteEP);
            }
        }

        private void OnStartRecieve(
            TraFransfersFileStart traFransfersFileStart,
            IPEndPoint remoteEP)
        {
            _receiveFileManager = new ReceiveFileManager(
                _path,
                traFransfersFileStart.FileName,
                traFransfersFileStart.PartCount,
                traFransfersFileStart.PartSize,
                traFransfersFileStart.Length);
            Send(0, "OK", remoteEP);
        }
    }
}
