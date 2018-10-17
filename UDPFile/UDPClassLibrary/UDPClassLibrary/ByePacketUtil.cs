using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketByUDPClassLibrary
{
    public class ByePacketUtil
    {
        private int PacketMaxSize = 2048;
        private int UserMaxSize = 2040;
        private int HeadSize = 8;
        private int HeadIndexSize = 2;
        private int HeadCountSize = 2;
        private int HeadLengthSize = 2;
        private int HeadIDSize = 2;

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="in_listPools"></param>
        /// <returns></returns>
        public List<byte[]> SplitPacket(List<byte> in_listPools)
        {
            List<byte[]> listResBytes = new List<byte[]>();

            // 得到总长度
            int iPoolLenght = in_listPools.Count();
            Int16 HeadIndex = -1;
            Int16 HeadCount = GetCount(iPoolLenght);
            Int16 HeadLength = 0;
            Int16 HeadID = CreateID();

            //剩下的长度
            int iLeftLenght = in_listPools.Count();
            bool bIsOver = false;
            do
            {
                // 序列自增
                HeadIndex++;

                // 判断剩下的长度是否还要再分一个包
                if (iLeftLenght > UserMaxSize)
                {
                    // 多个包就是HeadLength=UserMaxSize=2042
                    HeadLength = (Int16)(UserMaxSize);
                }
                else
                {
                    // 否则HeadLength等于剩下的大小
                    HeadLength = (Int16)iLeftLenght;

                    // 标记循环结束
                    bIsOver = true;
                }

                // 建立一个包
                List<byte> listResData = new List<byte>();

                // 添加包头
                listResData.AddRange(CreateHead(HeadIndex, HeadCount, HeadLength, HeadID));

                byte[] listPatyData = new byte[HeadLength];
                // 依照HeadLength长度剪切in_listPools
                Array.Copy(in_listPools.ToArray(), iPoolLenght - iLeftLenght, listPatyData, 0, (long)HeadLength);

                //添加剩下内容
                listResData.AddRange(listPatyData);
                while (listResData.Count < 2048)
                {
                    listResData.AddRange(new byte[2048 - listResData.Count]);
                }

                listResBytes.Add(listResData.ToArray());
                // 剩下的长度减掉单前包的长度
                iLeftLenght -= HeadLength;
            } while (!bIsOver);

            return listResBytes;
        }





        /// <summary>
        /// 封包
        /// </summary>
        /// <param name="in_listBytes"></param>
        /// <returns></returns>
        public List<byte> JointPacket(List<byte[]> in_listBytes)
        {
            List<byte> listResDatas = new List<byte>();
            Dictionary<short, List<byte>> listPacketMap = new Dictionary<short, List<byte>>();

            // 
            short iCount = GetJointHeadCount(in_listBytes[0]);
            foreach (byte[] listData in in_listBytes)
            {
                // 得到索引
                short iHeadIndex = GetJointHeadIndex(listData);

                // 得到长度
                short iHeadLength = GetJointHeadLength(listData);


                byte[] listToAdd = new byte[iHeadLength];
                Array.Copy(listData, HeadSize, listToAdd, 0, iHeadLength);

                // 添加一段到字典
                List<byte> listSignal = new List<byte>();
                listSignal.AddRange(listToAdd);
                listPacketMap.Add(iHeadIndex, listSignal);
            }

            // 循环按照顺序添加到列表
            for (short i = 0; i < iCount; i++)
            {
                listResDatas.AddRange(listPacketMap[i].ToArray());
            }
            return listResDatas;


        }


        /// <summary>
        /// 封包时得到包序号
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        public short GetJointHeadIndex(byte[] listData)
        {
            byte[] listIndex = new byte[HeadIndexSize];
            Array.Copy(listData, 0, listIndex, 0, HeadIndexSize);
            return BitConverter.ToInt16(listIndex, 0);
        }

        /// <summary>
        /// 封包时得到总包数
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        public short GetJointHeadCount(byte[] listData)
        {
            byte[] listCount = new byte[HeadCountSize];
            Array.Copy(listData, HeadIndexSize, listCount, 0, HeadCountSize);
            return BitConverter.ToInt16(listCount, 0);
        }

        /// <summary>
        /// 封包时得到内容长度
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        private short GetJointHeadLength(byte[] listData)
        {
            byte[] listLength = new byte[HeadLengthSize];
            Array.Copy(listData, HeadIndexSize + HeadCountSize, listLength, 0, HeadLengthSize);
            return BitConverter.ToInt16(listLength, 0);
        }

        /// <summary>
        /// 封包时得到包ID
        /// </summary>
        /// <param name="listData"></param>
        /// <returns></returns>
        public short GetJointHeadID(byte[] listData)
        {
            byte[] listID = new byte[HeadIndexSize];
            Array.Copy(listData, HeadIndexSize + HeadCountSize + HeadLengthSize, listID, 0, HeadIndexSize);
            return BitConverter.ToInt16(listID, 0);
        }



        /// <summary>
        /// 创建一个包头
        /// </summary>
        /// <param name="in_iHeadIndex">序号</param>
        /// <param name="in_iHeadCount">总数</param>
        /// <param name="in_iHeadLength">内容长度</param>
        /// <returns></returns>
        public byte[] CreateHead(Int16 in_iHeadIndex, Int16 in_iHeadCount, Int16 in_iHeadLength, Int16 in_iHeadID)
        {
            List<byte> bHead = new List<byte>();
            byte[] bHeadIndex = new byte[HeadIndexSize];
            byte[] bHeadCount = new byte[HeadCountSize];
            byte[] bHeadLength = new byte[HeadLengthSize];
            byte[] bHeadID = new byte[HeadIDSize];
            bHeadIndex = BitConverter.GetBytes(in_iHeadIndex);
            bHeadCount = BitConverter.GetBytes(in_iHeadCount);
            bHeadLength = BitConverter.GetBytes(in_iHeadLength);
            bHeadID = BitConverter.GetBytes(in_iHeadID);
            bHead.AddRange(bHeadIndex);
            bHead.AddRange(bHeadCount);
            bHead.AddRange(bHeadLength);
            bHead.AddRange(bHeadID);
            return bHead.ToArray();
        }

        /// <summary>
        /// 拆包时统计包的数量
        /// </summary>
        /// <param name="iPoolLenght"></param>
        /// <returns></returns>
        private short GetCount(int iPoolLenght)
        {
            return (short)(iPoolLenght / (PacketMaxSize - HeadSize) + 1);
        }

        /// <summary>
        /// 生成一个2字节的ID
        /// </summary>
        /// <returns></returns>
        private short CreateID()
        {
            return (short)(new Random().Next(65535));
        }
    }
}
