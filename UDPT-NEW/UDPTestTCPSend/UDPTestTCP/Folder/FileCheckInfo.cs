using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTestTCP.Folder
{
    class FileCheckInfo
    {
        //这个类用于存储文件检查时候返回缺少的文件信息
        public int PackId { set; get; }
        public int Count { set; get; }
        public List<int> lackPieces { set; get; }

        public FileCheckInfo()
        {

        }
    }
}
