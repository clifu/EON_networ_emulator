using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientNode;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ClientApp klient1 = new ClientApp();
            //  ClientApp klient2 = new ClientApp();
            // ClientNode.Form1 form1 = new Form1();
            //  ClientNode.Form1 form2 = new Form1();
            /* Application.EnableVisualStyles();
             Application.SetCompatibleTextRenderingDefault(false);
             Application.Run(new MultiFormContext(new ClientApplication("127.0.0.1","11000","11000"), new ClientApplication("127.0.0.1", "11000", "11000")));
             */
            // Class1 clas=new Class1();
            //clas.SendingMessage();

            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\NetworkCableCloud\\bin\\Debug\\NetworkCableCloud.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\SubNetwork\\bin\\Debug\\SubNetwork.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\SubNetwork\\bin\\Debug\\SubNetwork.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\ClientNode\\bin\\Debug\\ClientNode.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\ClientNode\\bin\\Debug\\ClientNode.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\NetworkNode\\bin\\Debug\\NetworkNode.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\NetworkNode\\bin\\Debug\\NetworkNode.exe");
            Process.Start("C:\\Users\\Paweł\\Dropbox\\tsst-project\\DANIEL\\Sklejany\\NetworkEmulation\\NetworkNode\\bin\\Debug\\NetworkNode.exe");
        }

        public class MultiFormContext : ApplicationContext
        {
            private int openForms;
            public MultiFormContext(params Form[] forms)
            {
                openForms = forms.Length;

                foreach (var form in forms)
                {
                    form.FormClosed += (s, args) =>
                    {
                        //When we have closed the last of the "starting" forms, 
                        //end the program.
                        if (Interlocked.Decrement(ref openForms) == 0)
                            ExitThread();
                    };

                    form.Show();
                }
            }
        }
    }
}
