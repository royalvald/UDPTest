using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTran.Save
{
    class Remove
    {
        private int packID;

        private int index;

        public int PackID { get => packID; set => packID = value; }
        public int Index { get => index; set => index = value; }

        public Remove(int ID,int index)
        {
            this.packID = ID;
            this.index = index;
        }
    }
}
