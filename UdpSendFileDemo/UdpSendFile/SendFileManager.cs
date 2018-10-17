using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace CSharpWin
{
    /* 作者：Starts_2000
     * 日期：2009-07-25
     * 网站：http://www.csharpwin.com CS 程序员之窗。
     * 你可以免费使用或修改以下代码，但请保留版权信息。
     * 具体请查看 CS程序员之窗开源协议（http://www.csharpwin.com/csol.html）。
     */

    public class SendFileManager : IDisposable
    {
        private FileStream _fileStream;
        private long _partCount;
        private long _length;
        private int _partSize = 1024 * 5;
        private int _index = 0;

        public SendFileManager(string fileName)
        {
            Create(fileName);
        }

        public SendFileManager(string fileName, int partSize)
        {
            _partSize = partSize;
            Create(fileName);
        }

        public event ReadFileBufferEventHandler ReadFileBuffer;

        public long PartCount
        {
            get { return _partCount; }
        }

        public long Length
        {
            get { return _length; }
        }

        public int PartSize
        {
            get { return _partSize; }
        }

        internal FileStream FileStream
        {
            get { return _fileStream; }
        }

        private void Create(string fileName)
        {
            _fileStream = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _partSize,
                true);
            _length = _fileStream.Length;
            _partCount = _length / _partSize;
            if (_length % _partSize != 0)
            {
                _partCount++;
            }
        }

        public void Read(int index)
        {
            int size = _partSize;
            if (Length - _partSize * index < _partSize)
            {
                size = (int)(Length - _partSize * index);
            }
            byte[] buffer = new byte[size];
            ReadFileObject obj = new ReadFileObject(index, buffer);
            FileStream.Position = index * _partSize;
            FileStream.BeginRead(
                buffer,
                0,
                size,
                new AsyncCallback(EndRead),
                obj);
        }

        public void Read()
        {
            int index = Interlocked.Increment(ref _index);
            Read(index - 1);
        }

        private void EndRead(IAsyncResult result)
        {
            int length = FileStream.EndRead(result);
            ReadFileObject state = (ReadFileObject)result.AsyncState;
            int index = state.Index;
            byte[] buffer = state.Buffer;
            ReadFileBufferEventArgs e = null;
            if (length < _partSize)
            {
                byte[] realBuffer = new byte[length];
                Buffer.BlockCopy(buffer, 0, realBuffer, 0, length);
                e = new ReadFileBufferEventArgs(index, realBuffer);
            }
            else
            {
                e = new ReadFileBufferEventArgs(index, buffer);
            }
            OnReadFileBuffer(e);
        }

        protected void OnReadFileBuffer(ReadFileBufferEventArgs e)
        {
            if (ReadFileBuffer != null)
            {
                ReadFileBuffer(this, e);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        #endregion
    }
}
