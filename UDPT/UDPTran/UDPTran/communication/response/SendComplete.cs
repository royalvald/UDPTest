using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTran.communication.response
{
    //传输完成后的Tag
    [Serializable]
    class SendComplete :Response
    {
        public ResponseStatus responseStatus;
        public int PackID;

        public SendComplete(int ID,ResponseStatus responseStatus)
        {
            this.PackID = ID;
            this.responseStatus = responseStatus;
        }
    }
}
