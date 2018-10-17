using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpWin
{
    /* 作者：Starts_2000
     * 日期：2009-07-25
     * 网站：http://www.csharpwin.com CS 程序员之窗。
     * 你可以免费使用或修改以下代码，但请保留版权信息。
     * 具体请查看 CS程序员之窗开源协议（http://www.csharpwin.com/csol.html）。
     */

    public delegate void ReadFileBufferEventHandler(
        object sender,
        ReadFileBufferEventArgs e);

    public class ReadFileBufferEventArgs : EventArgs
    {
        private int _index;
        private byte[] _buffer;

        public ReadFileBufferEventArgs(int index, byte[] buffer)
            : base()
        {
            _index = index;
            _buffer = buffer;
        }

        public int Index
        {
            get { return _index; }
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }
    }
}
