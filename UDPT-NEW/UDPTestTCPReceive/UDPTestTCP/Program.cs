using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPTestTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            Dispatcher dispatcher = new Dispatcher(Dispatcher.Pattern.receive, "192.168.109.58", "192.168.113.51");
        }
    }
}
