using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Test
{
    class Class1
    {
       public void SendingMessage()
        {
            byte[] data = new byte[64];

            UdpClient newsock = new UdpClient();

            IPEndPoint sender = new IPEndPoint(IPAddress.Parse("127.0.0.42"), 11000);

            try
            {
                while (true)
                {
                    string message = Console.ReadLine().ToString();
                    data = Encoding.ASCII.GetBytes(message);
                    newsock.Send(data, data.Length, sender);
                }

            }
            catch (Exception ex)
            {


            }

        }
    }
}
