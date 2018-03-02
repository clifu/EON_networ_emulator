using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubNetwork
{
    /// <summary>
    /// SNPP
    /// </summary>
    public class SubNetworkPointPool
    {
        public List<SubNetworkPoint> snps;

        public SubNetworkPointPool()
        {
            snps = new List<SubNetworkPoint>();
        }

        /// <summary>
        /// Tworzy liste SNP i dodaje do niej pojedynczy SNP
        /// </summary>
        /// <param name="point"></param>
        public SubNetworkPointPool(SubNetworkPoint point):this()
        {
            snps.Add(point);
        }

        /// <summary>
        /// Dodaje SubNetworkPoint do listy
        /// </summary>
        /// <param name="SNP"></param>
        public void Add(SubNetworkPoint SNP)
        {
            snps.Add(SNP);
        }
    }
}
