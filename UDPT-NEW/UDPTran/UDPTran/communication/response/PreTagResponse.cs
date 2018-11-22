using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UDPTran
{
    
    [Serializable]
    class PreTagResponse:Response
    {
       private ResponseStatus responseStatus;
       private EndPoint RemotePoint;
        public PreTagResponse(EndPoint endPoint,ResponseStatus status)
        {
            this.RemotePoint = endPoint;
            this.responseStatus = status;
        }
    }
}
