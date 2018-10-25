using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace UDPTran
{
    class PacketUtil
    {
        /// <summary>
        /// 包单个最大长度
        /// </summary>
        private int packetLength = 1040;
        /// <summary>
        /// 头部信息长度大小
        /// </summary>
        private int headLength = 16;
        /// <summary>
        /// 数据段信息长度
        /// </summary>
        private static int maxContextLength = 1024;
        private static int headIDLength = 4;
        private static int headIndexLength = 4;
        private static int headPackCountLength = 4;
        private static int headContextLength = 4;

        public int PacketLength { get => packetLength; }
        public int HeadLength { get => headLength; }
        public int MaxContextLength { get => maxContextLength; }
        public int HeadIDLength { get => headIDLength; }
        public int HeadIndexLength { get => headIndexLength; }
        public int HeadPackCountLength { get => headPackCountLength; }
        public int HeadContextLength { get => headContextLength; }

        /// <summary>
        /// 信息分片
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public List<byte[]> InfoToPacket(byte[] FileInfo)
        {
            //文件长度
            int Length = FileInfo.Length;
            //分包后放入list
            List<byte[]> list = new List<byte[]>();
            //分包暂存

            //标志当前位置
            int position = 0;
            //当前文件生成的ID
            int ID = CreatID();
            //分包中指示当前的索引
            int Index;
            //指示当前分包的总数
            int Count = PackCount(FileInfo);
            //标志当前包中信息长度
            int ContextLength;
            //填充位置
            byte insert = (byte)1;
            Index = 0;//索引初始化
            while (position < Length)
            {
                byte[] bytes = new byte[packetLength];
                //先判断文件剩余长度
                if (Length - position > MaxContextLength)
                {
                    ContextLength = MaxContextLength;
                    Array.Copy(CreatHeader(ID, Index, Count, ContextLength), 0, bytes, 0, 16);
                    Array.Copy(FileInfo, position, bytes, HeadLength, ContextLength);
                }
                else
                {
                    ContextLength = Length - position;
                    Array.Copy(CreatHeader(ID, Index, Count, ContextLength), 0, bytes, 0, 16);
                    Array.Copy(FileInfo, position, bytes, HeadLength, ContextLength);
                    //剩余位置进行填充
                    for (int i = ContextLength + HeadLength; i < MaxContextLength; i++)
                    {
                        bytes[i] = insert;
                    }
                }
                //索引和数据指针位置更新
                Index++;
                position += ContextLength;

                list.Add(bytes);

            }
            return list;
        }


        public List<byte[]> InfoToPacket(byte[] FileInfo,int packID,int positions,int count)
        {
            //文件长度
            int Length = FileInfo.Length;
            //分包后放入list
            List<byte[]> list = new List<byte[]>();
            //分包暂存

            //标志当前位置
            int position = 0;
            //当前文件生成的ID
            int ID = packID;
            //分包中指示当前的索引
            int Index;
            //指示当前分包的总数
            int Count = count;
            //标志当前包中信息长度
            int ContextLength;
            //填充位置
            byte insert = (byte)1;
            Index = positions/1024;//索引初始化
            while (position < Length)
            {
                byte[] bytes = new byte[packetLength];
                //先判断文件剩余长度
                if (Length - position > MaxContextLength)
                {
                    ContextLength = MaxContextLength;
                    Array.Copy(CreatHeader(ID, Index, Count, ContextLength), 0, bytes, 0, 16);
                    Array.Copy(FileInfo, position, bytes, HeadLength, ContextLength);
                }
                else
                {
                    ContextLength = Length - position;
                    Array.Copy(CreatHeader(ID, Index, Count, ContextLength), 0, bytes, 0, 16);
                    Array.Copy(FileInfo, position, bytes, HeadLength, ContextLength);
                    //剩余位置进行填充
                    for (int i = ContextLength + HeadLength; i < MaxContextLength; i++)
                    {
                        bytes[i] = insert;
                    }
                }
                //索引和数据指针位置更新
                Index++;
                position += ContextLength;

                list.Add(bytes);

            }
            return list;
        }

        //检查数据包是否完整
        public bool TotalCheckBool(Dictionary<int, byte[]> dic)
        {
            bool IsCompleted = true;
            int TotalPack = 0;
            int PackID = 0;
            byte[] bytes;
            //为了防止出现丢包情况，所以将字典中所有数据进行遍历，如果找到其中一个不为空那么就读取其数据值，
            for (int i = 0; i < 100; i++)
            {
                if (dic.ContainsKey(i))
                {
                    bytes = dic[i];
                    //获取文件总包数
                    TotalPack = GetCount(bytes);
                    PackID = GetID(bytes);
                    break;
                }


            }
            //查询所有键值对的key，如果发现缺少则返回校验失败
            for (int i = 0; i < TotalPack; i++)
            {
                if (!dic.ContainsKey(i))
                {
                    IsCompleted = false;
                    break;
                }
            }
            return IsCompleted;
        }


        /// <summary>
        /// 此方法返回缺少的数据小包，格式为（index,packID）
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public Dictionary<int, int> TotalCheck(Dictionary<int, byte[]> dic)
        {

            Dictionary<int, int> LostPairs = new Dictionary<int, int>();
            int TotalPack = 0;
            int PackID = 0;
            byte[] bytes;
            //为了防止出现丢包情况，所以将字典中所有数据进行遍历，如果找到其中一个不为空那么就读取其数据值，
            for (int i = 0; i < 100; i++)
            {
                if (dic.ContainsKey(i))
                {
                    bytes = dic[i];
                    //获取文件总包数
                    TotalPack = GetCount(bytes);
                    PackID = GetID(bytes);
                    break;
                }


            }
            //查询所有键值对的key，如果发现缺少则添加进dictionary返回

            for (int i = 0; i < TotalPack; i++)
            {
                if (!dic.ContainsKey(i))
                {
                    LostPairs.Add(i, PackID);
                }
            }
            return LostPairs;
        }

        //运行此方法前要先调用检查方法，确定包的完整性
        public byte[] PackIntoFile(Dictionary<int, byte[]> dic)
        {
            List<byte> list = new List<byte>();
            byte[] TempFile = new byte[MaxContextLength];
            byte[] bytes = dic[0];
            int ContextLength;
            int TotalCount = GetCount(bytes);
            for (int i = 0; i < TotalCount - 1; i++)
            {
                Array.Copy(dic[i], 16, TempFile, 0, MaxContextLength );
                list.AddRange(TempFile);
            }
            //以上仅仅处理了总包数-1数量的数据包，剩下的一个数据包因为自身数据特殊(即数据包可能存在填充数据)，所以应该分开处理

            ContextLength = GetContexLength(dic[TotalCount - 1]);
            byte[] LastByte = new byte[ContextLength];
            Array.Copy(dic[TotalCount - 1], 16, LastByte, 0, ContextLength);
            list.AddRange(LastByte);

            return list.ToArray();
        }

        //制造头部信息
        private byte[] CreatHeader(int ID, int Index, int Count, int ContextLength)
        {
            byte[] bytes = new byte[16];
            //添加ID值
            Array.Copy(BitConverter.GetBytes(ID), 0, bytes, 0, 4);
            //添加包索引
            Array.Copy(BitConverter.GetBytes(Index), 0, bytes, 4, 4);
            //添加包总数量
            Array.Copy(BitConverter.GetBytes(Count), 0, bytes, 8, 4);
            //添加内容长度信息
            Array.Copy(BitConverter.GetBytes(ContextLength), 0, bytes, 12, 4);

            return bytes;
        }
        //得到待处理数据的总长度
        private int PackCount(byte[] bytes)
        {
            int length = bytes.Length;
            if (length % MaxContextLength == 0)
                return length / MaxContextLength;
            else
                return length / MaxContextLength + 1;
        }
        //生成包ID
        private int CreatID()
        {
            Random rd = new Random();
            return rd.Next(0, 32767);
        }

        //拼包时候获取数据中总包数，和上面的PackCount不一样，这个是从Dictionary中获取总包数
        public int GetCount(byte[] bytes)
        {
            byte[] SumByte = new byte[4];
            SumByte[0] = bytes[8];
            SumByte[1] = bytes[9];
            SumByte[2] = bytes[10];
            SumByte[3] = bytes[11];
            int TotalCount = BitConverter.ToInt32(SumByte, 0);

            return TotalCount;
        }

        //获取包ID
        public int GetID(byte[] bytes)
        {
            byte[] IDArray = new byte[4];
            IDArray[0] = bytes[0];
            IDArray[1] = bytes[1];
            IDArray[2] = bytes[2];
            IDArray[3] = bytes[3];
            int IDNumber = BitConverter.ToInt32(IDArray, 0);

            return IDNumber;
        }

        //获取包内容长度
        public int GetContexLength(byte[] bytes)
        {
            byte[] Length = new byte[4];
            Length[0] = bytes[12];
            Length[1] = bytes[13];
            Length[2] = bytes[14];
            Length[3] = bytes[15];
            int TextLength = BitConverter.ToInt32(Length, 0);

            return TextLength;
        }

        //获取包的内部索引
        public int GetIndex(byte[] bytes)
        {
            byte[] length = new byte[4];
            length[0] = bytes[4];
            length[1] = bytes[5];
            length[2] = bytes[6];
            length[3] = bytes[7];
            int Index = BitConverter.ToInt32(length, 0);

            return Index;
        }



        /// <summary>
        /// 字节数组转化为类
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public object BytesToObject(byte[] bytes)
        {
            var memoryStream = new MemoryStream(bytes);
            var formalTer = new BinaryFormatter();
            var objects = formalTer.Deserialize(memoryStream);
            memoryStream.Close();

            return objects;
        }

        /// <summary>
        /// 类转化为字节数组
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public byte[] ObjectToBytes(object objects)
        {
            var memoryStream = new MemoryStream();
            var formalTer = new BinaryFormatter();
            formalTer.Serialize(memoryStream, objects);
            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();

            return bytes;
        }

        public byte[] AddHead(byte[] bytes,int packID,int index,int count,int ContextLength)
        {
            byte[] tmpBytes = new byte[1040];
            Array.Copy(CreatHeader(packID, index, count, ContextLength), 0, tmpBytes, 0, 16);
            Array.Copy(bytes, 0, tmpBytes, 16, ContextLength);

            return tmpBytes;
        }
    }
}
