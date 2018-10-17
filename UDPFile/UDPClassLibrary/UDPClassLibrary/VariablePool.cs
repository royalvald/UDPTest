using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SocketByUDPClassLibrary
{
    public class VariablePool
    {
        private List<byte> m_listTransports;
        private int m_iPost;
        private static int PostInitValue = 2;

        /// <summary>
        　　        /// 返回变量池
        　　        /// </summary>
        　　        /// <returns></returns>
        public byte[] GetVariablePool()
        {
            SetCount();
            return this.m_listTransports.ToArray();
        }

        /// <summary>
        　　        /// Put时初始化
        　　        /// </summary>
        public void Init()
        {
            m_listTransports = new List<byte>();
            m_listTransports.AddRange(BitConverter.GetBytes(new ushort()));
        }

        /// <summary>
        　　        /// Get时初始化
        　　        /// </summary>
        　　        /// <param name="in_listData">接收到的List</param>
        public void Init(byte[] in_listData)
        {
            m_listTransports = new List<byte>();
            m_listTransports.AddRange(in_listData);
        }

        /// <summary>
        　　        /// 取得下一个值
        　　        /// </summary>
        　　        /// <param name="in_iPost"></param>
        　　        /// <returns></returns>
        public byte[] GetNextData()
        {
            byte bLength = m_listTransports[m_iPost];
            byte[] listResData = new byte[bLength];
            int iNextOne = 1;

            Array.Copy(m_listTransports.ToArray(), m_iPost + iNextOne, listResData, 0, bLength);
            m_iPost += bLength + iNextOne;

            return listResData;
        }

        /// <summary>
        　　        /// 获得最大重复值数量
        　　        /// </summary>
        　　        /// <returns></returns>
        public int GetCount()
        {
            byte[] bUshot = new byte[2];
            bUshot[0] = m_listTransports[0];
            bUshot[1] = m_listTransports[1];

            return BitConverter.ToInt16(bUshot, 0);
        }

        /// <summary>
        　　        /// 修改最大值
        　　        /// </summary>
        public void SetCount()
        {
            Dictionary<string, int> listCountMap = new Dictionary<string, int>();

            string strName;

            m_iPost = PostInitValue;
            do
            {
                strName = Encoding.Default.GetString(GetNextData());
                if (listCountMap.ContainsKey(strName))
                {
                    // 如果包含就增加1
                    listCountMap[strName]++;
                }
                else
                {
                    // 不包含就新键一条
                    listCountMap.Add(strName, 1);
                }

                // 取值过程,只需要下标变动,不需要接收返回值
                GetNextData();

                // 当下标越界时结束循环
            } while (m_iPost < m_listTransports.Count);

            // 找出最大值并修改
            ushort usMax = (ushort)listCountMap.Values.Max();
            byte[] bUshot = BitConverter.GetBytes(usMax);
            m_listTransports[0] = bUshot[0];
            m_listTransports[1] = bUshot[1];

        }

        /// <summary>
        　　        /// 从变量池取得对象
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">字段名</param>
        　　        /// <param name="in_iIndex">下标</param>
        　　        /// <returns></returns>
        public object Get(string in_strFieldName, int in_iIndex)
        {
            int iCount = -1;

            List<byte> listResData = new List<byte>();
            string strName;
            byte[] listValue;

            m_iPost = PostInitValue;
            do
            {
                strName = Encoding.Default.GetString(GetNextData());
                if (strName == in_strFieldName)
                {
                    // 匹配字段名
                    iCount++;
                }
                listValue = GetNextData();
                if (iCount == in_iIndex)
                {
                    // 匹配下标
                    return SetByteToValue(listValue);
                }
                // 当下标越界时结束循环
            } while (m_iPost < m_listTransports.Count);

            return null;
        }

        /// <summary>
        　　        /// 转换出字段名字节
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">字段名字</param>
        　　        /// <returns></returns>
        public byte[] SetNameToByte(string in_strFieldName)
        {
            return System.Text.Encoding.ASCII.GetBytes(in_strFieldName);
        }

        /// <summary>
        　　        /// 为变量池添加对象
        　　        /// </summary>
        　　        /// <param name="in_strFieldName"></param>
        　　        /// <param name="in_objValue"></param>
        public void Put(string in_strFieldName, Object in_objValue)
        {
            // 分别转换出字段长度,字段,值长度,值
            byte[] listFieldName = SetNameToByte(in_strFieldName);
            byte bFieldNameLength = (byte)listFieldName.Length;
            byte[] listValue = SetValueToByte(in_objValue);
            byte bValueLength = (byte)listValue.Length;



            m_listTransports.Add(bFieldNameLength);
            m_listTransports.AddRange(listFieldName);
            m_listTransports.Add(bValueLength);
            m_listTransports.AddRange(listValue);

        }

        /// <summary>
        　　        /// 转换出值字节(序列化)
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">值名字</param>
        　　        /// <returns></returns>
        public byte[] SetValueToByte(Object in_objValue)
        {
            var memoryStream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, in_objValue);
            byte[] buffer = memoryStream.ToArray();
            memoryStream.Close();
            return buffer;

        }

        /// <summary>
        　　        /// 反序列化
        　　        /// </summary>
        　　        /// <param name="in_strFieldName">值名字</param>
        　　        /// <returns></returns>
        public Object SetByteToValue(byte[] in_listValue)
        {
            var memoryStream = new MemoryStream(in_listValue);
            var formatter = new BinaryFormatter();
            Object obj = formatter.Deserialize(memoryStream);
            memoryStream.Close();
            return obj;

        }

    }
}
