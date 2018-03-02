using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SubNetwork
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();
                string s = Console.ReadLine();
                ControlConnection cc = new ControlConnection(s);
                Task.Run(() => { cc.ReceivedMessage(); });

                LinkResourceManager lrm = new LinkResourceManager(s);
                Task.Run(() =>
                {
                    lrm.ReceivedMessage();
                });
                RoutingController rc = new RoutingController(Int32.Parse(s));
                Task.Run(() =>
                {
                    rc.Run();
                });

                
                //Petla wykonuje sie poki nie nacisniemy klawisza "esc"
                while (true)
                {
                    string tmp = Console.ReadLine().ToString();
                    if(tmp=="exit")
                    {
                        break;                    
                    }
                    else if(tmp.StartsWith("kill"))
                        {
                        lrm.killLink(tmp);
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }



        }
    }
}
