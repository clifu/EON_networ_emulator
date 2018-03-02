using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AISDE;

namespace SubNetwork
{
    public class NetworkPath
    {
        public Sciezka path;

        public List<SubNetworkPointPool> snpps;

        public NetworkPath()
        {
            this.path = new Sciezka();
            this.snpps = new List<SubNetworkPointPool>();
        }

        public NetworkPath(Sciezka path) : this()
        {
            this.path = path;

            //dla pewnosci wyznaczamy wezly w przekazanej sciezke zeby lista wezlow nie byla pusta
            path.wyznaczWezly(path.Wezel1);

            actualizeSNPPs();

        }

        /// <summary>
        /// Aktualizuje wartości SNPPów po zmianie w krawędziach ścieżki.
        /// </summary>
        public void actualizeSNPPs()
        {
            snpps = new List<SubNetworkPointPool>();
            foreach (Wezel w in path.WezlySciezki)
            {
                SubNetworkPoint snp = new SubNetworkPoint(IPAddress.Parse(w.ip));

                SubNetworkPointPool snpp = new SubNetworkPointPool();

                snpp.Add(snp);

                snpps.Add(snpp);
            }
        }
    }
}
