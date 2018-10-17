using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using CSharpWin;
using System.IO;
using System.Diagnostics;

namespace UdpSendFileDemo
{
    public partial class Form1 : Form
    {
        private UdpSendFile sendFile;

        public Form1()
        {
            InitializeComponent();
            linkLabel1.Click += delegate(object sender, EventArgs e)
            {
               Process.Start("www.csharpwin.com");
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sendFile.FileName = ofd.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sendFile = new UdpSendFile(
                tbRemoteIP.Text,
                int.Parse(tbRemotePort.Text),
                int.Parse(tbLocalPort.Text));
            sendFile.Start();
        }
    }
}