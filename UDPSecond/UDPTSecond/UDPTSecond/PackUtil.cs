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
    }
}
