using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkNode;
using SubNetwork;

namespace AISDE
{
    public class Wezel
    {
        protected int identyfikatorWezla;
        protected int wspolrzednaX;
        protected int wspolrzednaY;
        protected int liczbaKlientow;
        protected Wezel doMniePrzez;
        protected float etykieta;
        protected bool odwiedzony;
        protected List<Lacze> doprowadzoneKrawedzie = new List<Lacze>();
        public string ip = String.Empty;
        //public EONTable eonTable;
        public SubNetworkPointPool SNPP;

        public Wezel()
        {
            identyfikatorWezla = 0;
            wspolrzednaX = 0;
            wspolrzednaY = 0;
            doMniePrzez = null;
            etykieta = 0;
            odwiedzony = false;
            ip = "";
            doprowadzoneKrawedzie = new List<Lacze>();
            SNPP = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Any));
            //eonTable = new EONTable();
        }

        public Wezel(int identyfikatorWezla, int wspolrzednaX, int wspolrzednaY, int liczbaKlientow) : this()
        {
            this.identyfikatorWezla = identyfikatorWezla;
            this.wspolrzednaX = wspolrzednaX;
            this.wspolrzednaY = wspolrzednaY;
            this.liczbaKlientow = liczbaKlientow;
        }

        public Wezel(int identyfikatorWezla, int wspolrzednaX, int wspolrzednaY) : this()
        {
            this.identyfikatorWezla = identyfikatorWezla;
            this.wspolrzednaX = wspolrzednaX;
            this.wspolrzednaY = wspolrzednaY;
        }

        public Wezel(int identyfikatorWezla, int wspolrzednaX, int wspolrzednaY, string ip) : this()
        {
            this.identyfikatorWezla = identyfikatorWezla;
            this.wspolrzednaX = wspolrzednaX;
            this.wspolrzednaY = wspolrzednaY;
            this.ip = ip;
        }

        public Wezel(int id, string ip) : this()
        {
            this.identyfikatorWezla = id;
            this.ip = ip;
            this.SNPP = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(ip)));
        }

        public Wezel(int id, string ip, EONTable table) : this()
        {
            this.identyfikatorWezla = id;
            this.ip = ip;
            this.SNPP = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(this.ip)));
            this.SNPP.snps[0].eonTable = table;
        }

        public int LKlientow
        {
            get { return liczbaKlientow; }
            set { this.liczbaKlientow = value; }

        }

        public int idWezla
        {
            get { return identyfikatorWezla; }
            set { identyfikatorWezla = value; }
        }

        public int wspX
        {
            get { return wspolrzednaX; }
            set { wspolrzednaX = value; }
        }

        public int wspY
        {
            get { return wspolrzednaY; }
            set { wspolrzednaY = value; }
        }

        public bool Odwiedzony
        {
            get { return odwiedzony; }
            set { odwiedzony = value; }
        }

        public Wezel NajlepiejPrzez
        {
            get { return doMniePrzez; }
            set { doMniePrzez = value; }
        }

        public float Etykieta
        {
            get { return etykieta; }
            set { etykieta = value; }
        }

        public void wprowadzenieIndeksowKrawedzi(Lacze ktore)
        {
            doprowadzoneKrawedzie.Add(ktore);
        }
        public List<Lacze> listaKrawedzi
        {
            get { return doprowadzoneKrawedzie; }
        }

        /// <summary>
        /// Wyszukuje pierwszej wolnej szczeliny.
        /// </summary>
        /// <param name="band">Zajmowane pasmo</param>
        /// <param name="inOrOut">"in" albo "out"</param>
        /// <param name="linkID">ID łącza, które wchodzi lub wychodzi z węzła.</param>
        /// <returns>Wolna częstotliwość. Gdy się nie uda nic zrobić, </returns>
        public short FindFirstFreeFrequencyOut(short band, string inOrOut, int linkID)
        {
            SubNetworkPoint SNP = null;

            if (inOrOut == "in")
            {
                SNP = this.SNPP.snps.Find(x => x.portIN == linkID);

            }
            else if (inOrOut == "out")
            {
                SNP = this.SNPP.snps.Find(x => x.portIN == linkID);
            }

            if (SNP == null)
                return -1;

            for (short i = 0; i < EONTable.capacity; i++)
            {
                if (SNP.eonTable.CheckAvailability(i, band, inOrOut))
                {
                    return i;
                }
            }

            return -1;
        }

    }
}
