using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkNode;
using SubNetwork;


namespace AISDE
{
    public class Sciezka
    {
        /// <summary>
        /// Pasmo zajete na sciezce
        /// </summary>
        public short band;

        //Czestotliwosc sciezki
        public short frequency;

        private List<Lacze> ListaKrawedziSciezki = new List<Lacze>();
        private List<Wezel> ListaWezlowSciezki = new List<Wezel>();
        //krawedzieSciezki[0] to krawedz wychodzaca z wezlySciezki[0] i dochodzaca do wezlySciezki[1]

        //~Piotrek 
        protected Wezel WezelPierwszy; //Skad idziemy
        protected Wezel WezelDrugi; //Dokad idziemy
        //

        private static int wywolaniaWyznaczSciezke;

        protected int wezelpierwszy;
        protected int wezeldrugi;

        public Sciezka()
        {
            this.ListaWezlowSciezki = new List<Wezel>();
            this.ListaKrawedziSciezki = new List<Lacze>();
            this.Wezel1 = null;
            this.Wezel2 = null;
            this.wezel1 = 0;
            this.wezel2 = 0;
            wywolaniaWyznaczSciezke = 0;
            band = 0;
            frequency = -1;
        }

        public Sciezka(Wezel Pierwszy, Wezel Ostatni):this()
        {
            this.Wezel1 = Pierwszy;
            this.Wezel2 = Ostatni;
            this.wezel1 = Pierwszy.idWezla;
            this.wezel2 = Ostatni.idWezla;
        }

        public List<Lacze> zwroc_ListaKrawedziSciezki
        {
            get { return ListaKrawedziSciezki; }
            set { this.ListaKrawedziSciezki = value; }
        }

        public List<Lacze> KrawedzieSciezki
        {
            get { return this.ListaKrawedziSciezki; }
            set { this.ListaKrawedziSciezki = value; }
        }

        public List<Wezel> WezlySciezki
        {
            get { return ListaWezlowSciezki; }
            set { ListaWezlowSciezki = value; }
        }

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

        /// <summary>
        /// Wyznacza sciezke z Skad do Dokad o pasmie band i czestotliwosci frequency. Wpisuje band i frequency!
        /// </summary>
        /// <param name="Skad"></param>
        /// <param name="Dokad"></param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <param name="network">Sieć, zawierająca tablice kierowania węzłami, łączami, kosztów.</param>
        /// <returns></returns>
        public List<Lacze> wyznaczSciezke(Wezel Skad, Wezel Dokad, short band, short frequency, Siec network)
        {
            var path =  wyznaczSciezke(Skad, Dokad, network.zwrocTabliceKierowaniaLaczami,
                network.zwrocTabliceKierowaniaWezlami,
                ref network.wezly, band, network.Koszty, frequency);

            //Wpisanie wartosci czetotliwosci i pasma do sciezki
            if (path != null && path.Count != 0)
            {
                this.band = band;
                this.frequency = frequency;
            }

            return path;
        }

        /// <summary>
        /// Nie wpisuje wartosci czestotliwosci i pasma do sciezki! 
        /// </summary>
        /// <param name="Skad"></param>
        /// <param name="Dokad"></param>
        /// <param name="tablicaKierowaniaLaczami"></param>
        /// <param name="tablicaKierowaniaWezlami"></param>
        /// <param name="listaWezlow"></param>
        /// <param name="band"></param>
        /// <param name="tablicaKosztow"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public List<Lacze> wyznaczSciezke(Wezel Skad, Wezel Dokad, Lacze[,] tablicaKierowaniaLaczami,
            Wezel[,] tablicaKierowaniaWezlami, ref List<Wezel> listaWezlow, short band, float[,] tablicaKosztow, short frequency)
        {
            try
            {
                //Jedno wywolanie wiecej
                wywolaniaWyznaczSciezke += 1;

                //zwracana lista
                List<Lacze> tempList = new List<Lacze>();

                //indeksy wezlow
                int w1, w2;

                //Odnalezienie indeksow
                w1 = listaWezlow.FindIndex(x => x.ip == Skad.ip);
                w2 = listaWezlow.FindIndex(x => x.ip == Dokad.ip);

                //Zabezpieczenie przed nieskonczonym wywolywaniem sie tej samej funkcji. Po 200 wywolaniach tej funkcji wychodzimy. 
                if (wywolaniaWyznaczSciezke < 200)
                {
                    if (tablicaKierowaniaWezlami.Length >0 && tablicaKierowaniaWezlami[w1, w2] != null)
                    {
                        if (tablicaKierowaniaWezlami[w1, w2] == Dokad)
                        {
                            var cost = tablicaKosztow[w1, w2];

                            //Odnalezienie, jakim łączem idzie ścieżka
                            var link = tablicaKierowaniaLaczami[w1, w2];

                            //Odnalezienie SNP wejściowego łącza
                            SubNetworkPoint snpSkad = Skad.SNPP.snps.Find(x => x.portOUT == link.idKrawedzi);

                            //Odnalezienie SNP wyjsciowego łącza
                            SubNetworkPoint snpDokad = Dokad.SNPP.snps.Find(x => x.portIN == link.idKrawedzi);

                            //Gdy pasmo jest wolne na danej czestotliwosci
                            if (cost <= (EONTable.capacity - band) * (EONTable.capacity - band) &&
                                snpSkad.eonTable.CheckAvailability(frequency, band, "out") &&
                                snpDokad.eonTable.CheckAvailability(frequency, band, "in"))
                            {
                                tempList.Add(tablicaKierowaniaLaczami[w1, w2]);
                            }
                            else
                            {
                                return null;
                            }
                            //TODO 
                        }
                        else
                        {
                            //Dodajemy sciezke z Skad do punktu posredniego, wskazanego przez tablice kierowania Wezlami
                            var path = wyznaczSciezke(Skad, tablicaKierowaniaWezlami[w1, w2],
                                tablicaKierowaniaLaczami, tablicaKierowaniaWezlami, ref listaWezlow, band,
                                tablicaKosztow, frequency);
                            if (path != null)
                            {
                                tempList.AddRange(path);
                            }
                            else
                            {
                                return null;
                            }
                            //TODO:Tutaj wyskakuje nullpointerexception, ze nie mozna dodac AddRangem nulla. 
                            //TODO:A funkcja zwraca null, gdy probuje wyznaczyc sciezke od wezla 6 do 6 zdaje sie.
                            //Potem doklejamy drugi koniec listy, liste krawedzi z posredniego do koncowego
                            path = wyznaczSciezke(tablicaKierowaniaWezlami[w1, w2],
                                Dokad, tablicaKierowaniaLaczami, tablicaKierowaniaWezlami, ref listaWezlow, band,
                                tablicaKosztow, frequency);
                            if (path != null)
                            {
                                tempList.AddRange(path);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        //Kopiowanie listy krawedzi sciezki
                        ListaKrawedziSciezki = new List<Lacze>(tempList);

                        if (ListaKrawedziSciezki.Count != 0)
                        {
                            //wyzerowanie wezlow sciezki
                            WezlySciezki = new List<Wezel>();

                            //Wyznaczenie wezlow sciezki
                            wyznaczWezly(ListaKrawedziSciezki[0].Wezel1);
                        }

                        wywolaniaWyznaczSciezke--;
                        return tempList;
                    }
                }
                else
                    wywolaniaWyznaczSciezke--;
                return new List<Lacze>();
            }
            catch (Exception E)
            {
                wywolaniaWyznaczSciezke--;
                return new List<Lacze>();
            }
        }

        public List<Lacze> wyznaczSciezke(Wezel Skad, Wezel Dokad, Lacze[,] tablicaKierowaniaLaczami, 
            Wezel[,] tablicaKierowaniaWezlami, ref List<Wezel> listaWezlow, short band, float[,] tablicaKosztow)
        {
            return this.wyznaczSciezke(Skad, Dokad, tablicaKierowaniaLaczami, tablicaKierowaniaWezlami, ref listaWezlow,
                band, tablicaKosztow, 0);
        }

        /// <summary>
        /// Aktualizuje listę węzłów po zmianach w krawędziach ścieżki.
        /// </summary>
        public void aktualizujWezly()
        {
            this.WezlySciezki = new List<Wezel>();
            wyznaczWezly(this.KrawedzieSciezki[0].Wezel1);
        }

        public void wyznaczWezly(Wezel Skad)
        {
            //znalezienie lacza wychodzacego od wezla
            Lacze lacze = KrawedzieSciezki.Find(x => x.Wezel1 == Skad);
            if (lacze != null)
            {
                if (!ListaWezlowSciezki.Contains(lacze.Wezel1))
                    ListaWezlowSciezki.Add(lacze.Wezel1);
                if (!ListaWezlowSciezki.Contains(lacze.Wezel2))
                    ListaWezlowSciezki.Add(lacze.Wezel2);

                //Wyznaczamy wezly dalej, zaczynajac od drugiego wezla
                wyznaczWezly(lacze.Wezel2);
            }

            /*
            foreach (Lacze lacze in ListaKrawedziSciezki)
            {
                if (!ListaWezlowSciezki.Contains(lacze.Wezel1))
                    ListaWezlowSciezki.Add(lacze.Wezel1);
                if (!ListaWezlowSciezki.Contains(lacze.Wezel2))
                    ListaWezlowSciezki.Add(lacze.Wezel2);
                else
                    continue;
            } */
        }

        public void pokazSciezke()
        {
            int i = 0;
            Wezel temp = new Wezel();
            if (ListaKrawedziSciezki.Count == 0)
            {
                Console.WriteLine("Sciezka jest pusta.");
            }
            else
            {
                temp = ListaWezlowSciezki[ListaWezlowSciezki.Count - 1];
                Console.WriteLine($"Sciezka z wezla nr {ListaWezlowSciezki[0].idWezla} do wezla nr {temp.idWezla}");
                while (ListaKrawedziSciezki.ElementAtOrDefault(i) != null)
                {
                    Console.WriteLine($"Od wezla : {ListaWezlowSciezki[i].idWezla}");
                    Console.WriteLine($"przejdz krawedzia: {ListaKrawedziSciezki[i].idKrawedzi}");
                    i++;
                }
                Console.WriteLine("Jestes na miejscu!");
            }

        }
    }
}
