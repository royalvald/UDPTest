using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPClassLibrary
{
    class Program
    {
        public static DataDispatcher dd;
        static void Main(string[] args)
        {
            dd = new DataDispatcher("192.168.167.105:8003");// 这里填写自己的ip
            dd.OnDataArrived += dd_OnDataArrived;
        }

        static void dd_OnDataArrived(byte[] in_listVariablePool)
        {
            Console.WriteLine("收到数据!");
            VariablePool vp = new VariablePool();
            vp.Init(in_listVariablePool);
            string ip = (string)vp.Get("IP", 0);// 获取发送方的IP
            // 以上是接受数据
            // 下方是发送数据
            vp.Init();
            vp.Put("IP", ip);
            vp.Put("string", DateTime.Now.ToString());
            dd.Send(vp.GetVariablePool(), ip);
        }
    }

}
