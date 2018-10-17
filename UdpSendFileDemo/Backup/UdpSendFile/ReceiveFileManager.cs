using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CSharpWin
{
    /* 作者：Starts_2000
     * 日期：2009-07-25
     * 网站：http://www.csharpwin.com CS 程序员之窗。
     * 你可以免费使用或修改以下代码，但请保留版权信息。
     * 具体请查看 CS程序员之窗开源协议（http://www.csharpwin.com/csol.html）。
     */

    public class ReceiveFileManager : IDisposable
    {
        private string _path;
        private string _tempFileName;
        private string _fileName;
        private long _partCount;
        private int _partSize;
        private long _length;
        private FileStream _fileStream;
        private Stream _syncStream;

        private int _interval = 5000;
        private DateTime _lastReceiveTime;

        public ReceiveFileManager(
            string path,
            string fileName,
            long partCount, 
            int partSize,
            long length)
        {
            _path = path;
            _fileName = fileName;
            _partCount = partCount;
            _partSize = partSize;
            _length = length;
            Create();
        }

        public long PartCount
        {
            get { return _partCount; }
        }

        internal Stream SyncStream
        {
            get { return _syncStream; }
        }

        private void Create()
        {
            _tempFileName = string.Format("{0}\\_{1}", _path, _fileName);
            _fileStream = new FileStream(
                _tempFileName,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None,
                _partSize,
                true);
            _syncStream = Stream.Synchronized(_fileStream);
            _lastReceiveTime = DateTime.Now;
        }

        public void ReceiveBuffer(int index, byte[] buffer)
        {
            _fileStream.Position = index * _partSize;
            _fileStream.BeginWrite(
                buffer,
                0,
                buffer.Length,
                new AsyncCallback(EndWrite),
                index);
            _lastReceiveTime = DateTime.Now;
        }

        private void EndWrite(IAsyncResult result)
        {
            _fileStream.EndWrite(result);
            int index = (int)result.AsyncState;
            if (index == _partCount - 1)
            {
                Dispose();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            _fileStream.Flush();
            _fileStream.Close();
            _fileStream.Dispose();
            _fileStream = null;
        }

        #endregion
    }
}
