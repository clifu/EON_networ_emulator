using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

 

namespace NetworkingTools
{
    /// <summary>
    /// Zestaw klas z publicznie dostępnymi funkcjami
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Program P = new Program();
            P.runTests();
        }

        public void runTests()
        {
            NetworkingToolsTester NTT = new NetworkingToolsTester();
            NTT.testChangeMessageString();
            Console.ReadKey();
        }
    }
}
