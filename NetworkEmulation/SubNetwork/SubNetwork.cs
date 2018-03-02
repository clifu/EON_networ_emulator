using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubNetwork
{
    /// <summary>
    /// Klasa reprezentujaca podsiec
    /// </summary>
    public class SubNetwork
    {
        public List<SubNetworkPointPool> SNPPs;

        public SubNetwork()
        {
            this.SNPPs = new List<SubNetworkPointPool>();
        }
    }
}
