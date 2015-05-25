using System;
//
using IPlugIn_Test;

namespace PlugIn1
{
    public class PlugIn1Class : IPlugIn
    {
        public void Execute()
        {
            Console.WriteLine(this.GetType().Name + "Inside Execute(): " + DateTime.Now.ToString());
        }
    }
}
