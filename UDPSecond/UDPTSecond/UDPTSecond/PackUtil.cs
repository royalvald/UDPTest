using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UDPTSecond
{
    class PackUtil
    {
        //序列化操作

        /// <summary>
        /// 字节数组装化为类
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static object BytesToObject(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            var formal = new BinaryFormatter();
            object objects = formal.Deserialize(ms);
            ms.Close();

            return objects;
        }

        /// <summary>
        /// 类装化为字节数组
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static byte[] ObjectToBytes(object objects)
        {
            MemoryStream ms = new MemoryStream();
            var formal = new BinaryFormatter();
            formal.Serialize(ms, objects);
            byte[] bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }

        public static void CreatHeader(int packID,int index,int Count,int ContextLength,byte[] buffer)
        {
            //头部信息添加
            byte[] header = new byte[16];
            Array.Copy(BitConverter.GetBytes(packID), 0, header, 0, 4);
            Array.Copy(BitConverter.GetBytes(index), 0, header, 4, 4);
            Array.Copy(BitConverter.GetBytes(Count), 0, header, 8, 4);
            Array.Copy(BitConverter.GetBytes(ContextLength), 0, header, 12, 4);
            //添加到原数组(原有数组就是从16字节开始填充数据）
            Array.Copy(header, 0, buffer, 0, 16);
        }

        /// <summary>
        /// 获取文件分片片数
        /// </summary>
        /// <param name="fileLength"></param>
        /// <param name="pieceLength"></param>
        /// <returns></returns>
        public static int GetPiecesNum(int fileLength,int pieceLength)
        {
            if (fileLength % pieceLength == 0)
                return fileLength / pieceLength;
            else
                return fileLength / pieceLength + 1;
        }
    }
}
