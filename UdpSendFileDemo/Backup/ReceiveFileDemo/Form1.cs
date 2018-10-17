using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CSharpWin;
using System.Diagnostics;

namespace ReceiveFileDemo
{
    public partial class Form1 : Form
    {
        private UdpRecieveFile recieveFile;

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
            recieveFile = new UdpRecieveFile(
                Application.StartupPath, 
                int.Parse(tbLocalPort.Text));
            recieveFile.Start();
        }
    }
}