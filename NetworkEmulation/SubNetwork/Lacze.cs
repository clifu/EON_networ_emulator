using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NetworkNode;
using SubNetwork;

namespace AISDE
{
    public class Lacze
    {
        protected int identyfikatorKrawedzi;
        protected int wezelpierwszy;
        protected int wezeldrugi;

        //~Piotrek 
        protected Wezel WezelPierwszy;
        protected Wezel WezelDrugi;
        //

        protected float waga;

        public Lacze(int _identyfikatorKrawedzi, int _wezelpierwszy, int _wezeldrugi)
        {
            this.identyfikatorKrawedzi = _identyfikatorKrawedzi;
            this.wezelpierwszy = _wezelpierwszy;
            this.wezeldrugi = _wezeldrugi;
            this.waga = 0;
        }

        public Lacze(int _identyfikatorKrawedzi, Wezel _WezelPierwszy, Wezel _WezelDrugi, int waga)
        {
            this.identyfikatorKrawedzi = _identyfikatorKrawedzi;
            this.WezelPierwszy = _WezelPierwszy;
            this.WezelDrugi = _WezelDrugi;
            this.wezelpierwszy = _WezelPierwszy.idWezla;
            this.wezeldrugi = _WezelDrugi.idWezla;

            short band = (short)(EONTable.capacity - Math.Sqrt(waga));

            int SNP1Index, SNP2Index;

            if (Wezel1.SNPP.snps[0].portOUT == -1)
            {
                SNP1Index = 0;
                Wezel1.SNPP.snps[0].portOUT = _identyfikatorKrawedzi;
            }

            else
                //Odnalezienie węzła z dołączonym interfejsem
                SNP1Index = this.Wezel1.SNPP.snps.FindIndex(x => x.portOUT == _identyfikatorKrawedzi);

            if (Wezel2.SNPP.snps[0].portIN == -1)
            {
                SNP2Index = 0;
                Wezel2.SNPP.snps[0].portIN = _identyfikatorKrawedzi;
            }
            else
                //Odnalezienie węzła z dołączonym interfejsem
                SNP2Index = this.Wezel2.SNPP.snps.FindIndex(x => x.portIN == _identyfikatorKrawedzi);

            //Jeżeli węzeł nie ma doprowadzonego takiego łącza
            if (SNP1Index == -1)
            {
                //Dodanie nowego interfejsu do wezla 1
                Wezel1.SNPP.snps.Add(new SubNetworkPoint(IPAddress.Parse(Wezel1.ip), -1, identyfikatorKrawedzi));
                SNP1Index = Wezel1.SNPP.snps.Count - 1;
            }

            //Jeżeli węzeł nie ma doprowadzonego takiego łącza
            if (SNP2Index == -1)
            {
                //Dodanie nowego interfejsu do wezla 2
                Wezel2.SNPP.snps.Add(new SubNetworkPoint(IPAddress.Parse(Wezel2.ip), identyfikatorKrawedzi, -1));
                SNP2Index = Wezel2.SNPP.snps.Count - 1;
            }

            if (EONTable.capacity >= Math.Sqrt(this.Waga))
            {
                //Na wyjsciu wezla 1   
                this.Wezel1.SNPP.snps[SNP1Index].eonTable
                    .addRow(new EONTableRowOut(0, (short)(EONTable.capacity - band)));
                //Na wejsciu wezla 2
                this.Wezel2.SNPP.snps[SNP2Index].eonTable
                    .addRow(new EONTableRowIN(0, (short)(EONTable.capacity - band)));

                this.waga = waga;
            }
        }

        public Lacze(int _identyfikatorKrawedzi, Wezel _WezelPierwszy, Wezel _WezelDrugi, short band, short frequency)
        {
            this.identyfikatorKrawedzi = _identyfikatorKrawedzi;
            this.WezelPierwszy = _WezelPierwszy;
            this.WezelDrugi = _WezelDrugi;
            this.wezelpierwszy = _WezelPierwszy.idWezla;
            this.wezeldrugi = _WezelDrugi.idWezla;

            int SNP1Index, SNP2Index;

            if (Wezel1.SNPP.snps[0].portOUT == -1)
            {
                SNP1Index = 0;
                Wezel1.SNPP.snps[0].portOUT = _identyfikatorKrawedzi;
            }

            else
                //Odnalezienie węzła z dołączonym interfejsem
                SNP1Index = this.Wezel1.SNPP.snps.FindIndex(x => x.portOUT == _identyfikatorKrawedzi);

            if (Wezel2.SNPP.snps[0].portIN == -1)
            {
                SNP2Index = 0;
                Wezel2.SNPP.snps[0].portIN = _identyfikatorKrawedzi;
            }

            else
                //Odnalezienie węzła z dołączonym interfejsem
                SNP2Index = this.Wezel2.SNPP.snps.FindIndex(x => x.portIN == _identyfikatorKrawedzi);

            //Jeżeli węzeł nie ma doprowadzonego takiego łącza
            if (SNP1Index == -1)
            {
                //TODO: Zmien konstruktor SubNetworkPointa!
                //Dodanie nowego interfejsu do wezla 1
                Wezel1.SNPP.snps.Add(new SubNetworkPoint(IPAddress.Parse(Wezel1.ip), -1, _identyfikatorKrawedzi));
                SNP1Index = Wezel1.SNPP.snps.Count - 1;
            }

            //Jeżeli węzeł nie ma doprowadzonego takiego łącza
            if (SNP2Index == -1)
            {
                //Dodanie nowego interfejsu do wezla 2
                Wezel2.SNPP.snps.Add(new SubNetworkPoint(IPAddress.Parse(Wezel2.ip), _identyfikatorKrawedzi, -1));
                SNP2Index = Wezel2.SNPP.snps.Count - 1;
            }

            if (band <= EONTable.capacity)
            {
                //Na wyjsciu wezla 1   
                this.Wezel1.SNPP.snps[SNP1Index].eonTable.addRow(new EONTableRowOut(frequency, band));
                //Na wejsciu wezla 2
                this.Wezel2.SNPP.snps[SNP2Index].eonTable.addRow(new EONTableRowIN(frequency, band));

                this.waga = (float)Math.Pow(band, 2);
            }
        }

        /// <summary>
        /// Konstruktor Lacza bez zajtego pasma
        /// </summary>
        /// <param name="_identyfikatorKrawedzi"></param>
        /// <param name="_WezelPierwszy"></param>
        /// <param name="_WezelDrugi"></param>
        public Lacze(int _identyfikatorKrawedzi, Wezel _WezelPierwszy, Wezel _WezelDrugi)
            : this(_identyfikatorKrawedzi, _WezelPierwszy, _WezelDrugi, 0, 0)
        {
        }

        /// <summary>
        /// O ile w obu końcach łącza jest dany wpis tablocy Eonowej, zmienia koszt łącza.
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="band"></param>
        public void updateCost(short frequency, short band)
        {
            //Odnalezienie indeksu SNP, który jest przypisan do tego łącza
            int snpIndex2 = this.Wezel2.SNPP.snps.FindIndex(x => x.portIN == this.idKrawedzi);
            int snpIndex1 = this.Wezel1.SNPP.snps.FindIndex(x => x.portOUT == this.idKrawedzi);

            //Jezeli sie udalo znalezc indeksy
            if (snpIndex1 != -1 && snpIndex2 != -1)
            {
                //Jeżeli odnaleziony SNP w węźle posiada wpis do tablicy eonowej o takim samym paśmie i częstotliwośći, co w rowOut
                if (this.Wezel2.SNPP.snps[snpIndex2].eonTable.TableIN.FindIndex(y => y.busyBandIN == band && y.busyFrequency == frequency) != -1
                    && this.Wezel1.SNPP.snps[snpIndex1].eonTable.TableOut.FindIndex(y =>
                        y.busyBandOUT == band && y.busyFrequency == frequency) != -1)
                {
                    changeCost();
                }
            }
        }

        /// <summary>
        /// Podlicza całe pasmo zajęte na węźle pierwszym, na danym interfejsie i wylicza z niego koszt.
        /// </summary>
        public void changeCost()
        {
            int sum = 0;
            var SNP = Wezel1.SNPP.snps.Find(x => x.portOUT == this.idKrawedzi);

            foreach (short value in SNP.eonTable.OutFrequencies)
            {
                if (value != -1)
                    sum++;
            }

            //Całe zajęte pasmo do kwadratu to waga
            this.Waga = (float)(Math.Pow(sum, 2));
        }

        public int idKrawedzi
        {
            get { return identyfikatorKrawedzi; }
            set { identyfikatorKrawedzi = value; }
        }

        //~Piotrek
        public Wezel Wezel1
        {
            get { return WezelPierwszy; }
            set { WezelPierwszy = value; }
        }

        public Wezel Wezel2
        {
            get { return WezelDrugi; }
            set { WezelDrugi = value; }
        }
        //

        public int wezel1
        {
            get { return wezelpierwszy; }
            set { wezelpierwszy = value; }
        }

        public int wezel2
        {
            get { return wezeldrugi; }
            set { wezeldrugi = value; }
        }
        public float Waga
        {
            get { return waga; }
            set { waga = value; }
        }
    }
}