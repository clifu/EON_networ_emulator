using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubNetwork
{
    /// <summary>
    /// Klasa przechowująca adres IP Routing Controllera 
    /// </summary>
    public class SNandRC
    {
        public IPAddress RC_IP;
        public SubNetworkPointPool snpp;

        public SNandRC()
        {
            snpp = new SubNetworkPointPool();
        }

        public SNandRC(string rc_ip, string snpp_ip)
        {
            try
            {
                RC_IP = IPAddress.Parse(rc_ip);
                snpp = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(snpp_ip)));
            }
            catch (Exception E)
            {
                Console.WriteLine("SNandRC(): " + E.Message);
            }

        }
    }
}
