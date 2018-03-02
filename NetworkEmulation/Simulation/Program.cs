using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCalbleCloud;

namespace Simulation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Run(() =>
            {
               SubNetwork.Program.Main(new string[0]);
            });
            Task.Run(() =>
                       {
                           SubNetwork.Program.Main(new string[0]);
                       });
            Task.Run(() =>
                       {
                           SubNetwork.Program.Main(new string[0]);
                       });

            //Task.Run(() => { CableCloud.Main(new string[0]); });

            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NetworkNode.Program.Main(new string[0]);
            NCC.NCC.Main(new string[0]);
            NCC.NCC.Main(new string[0]);
            ClientNode.Program.Main();
            ClientNode.Program.Main();
            ClientNode.Program.Main();
        }
    }
}
