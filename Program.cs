using System;
using nanoFramework.Hardware.Esp32.Rmt.Helpers;

namespace ProntoToRmtItem
{
    class Program
    {
        static void Main(string[] args)
        {
            var testCode = "0000 0067 0000 0015 0060 0018 0018 0018 0030 " + 
            "0018 0030 0018 0030 0018 0018 0018 0030 0018 0018 0018 0018 " +
            "0018 0030 0018 0018 0018 0030 0018 0030 0018 0030 0018 0018 " +
            "0018 0018 0018 0030 0018 0018 0018 0018 0018 0030 0018 0018 03f6";
            
            var testItems = RmtHelpers.Pronto2RmtItems(testCode);
            
            Console.WriteLine("Hello World!");
        }
    }
}
