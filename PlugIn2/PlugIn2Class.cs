using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using IPlugIn_Test;

namespace PlugIn2
{
    public class PlugIn2Class : IPlugIn
    {
        public void Execute()
        {
            Console.WriteLine(this.GetType().Name + " Inside Execute(): " + DateTime.Now.ToString());
        }
    }

    public class Qoo
    {
        public static int Main()
        {
            Console.WriteLine("Qoo Test");
            
            return 99;
        }
    }
}
