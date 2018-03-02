using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;
using System.Threading;
using System.Net.Sockets;

namespace ClientNode
{


    public class ClientApp
    {
        public static SocketListener sl = new SocketListener();
        public static SocketSending sS = new SocketSending();
        public static CancellationTokenSource _cts = new CancellationTokenSource();
        public ClientApplication _Form1;

        public ClientApp()
        {

        }

    }
}
