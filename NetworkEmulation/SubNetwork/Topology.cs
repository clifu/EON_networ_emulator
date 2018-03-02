using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using AISDE;
using NetworkNode;

namespace SubNetwork
{
    /// <summary>
    /// Klasa reprezentująca topologię SubNetworka
    /// </summary>
    public class Topology
    {
        /// <summary>
        /// network z AISDE
        /// </summary>
        public Siec network;

        /// <summary>
        /// Lista potencjalnych linkow
        /// </summary>
        public List<Lacze> potentialLinks;

        public List<NetworkPath> pathsCopy;

        /// <summary>
        /// TablicaEONOwa - dla kazdego wezla jeden obiekt EONTable
        /// </summary>
        public List<EONTable> eonTable;

        public Topology()
        {
            network = new Siec();
            potentialLinks = new List<Lacze>();
            eonTable = new List<EONTable>();
            pathsCopy = new List<NetworkPath>();
        }

        public void addLink(Lacze l)
        {
            network.krawedzie.Add(l);
            network.aktualizujLiczniki();
            EONTable e = new EONTable();
        }

        /// <summary>
        /// Dodaje link do listy potencjalnych linkow w topologii
        /// </summary>
        /// <param name="l"></param>
        public void addPotentialLink(Lacze l)
        {
            try
            {
                if (this.network.wezly.Contains(l.Wezel1) && this.network.wezly.Contains(l.Wezel2))
                {
                    //Index nastepnego lacza w networki
                    int linkId = this.network.krawedzie.Count - 1 + this.potentialLinks.Count - 1 + 1;

                    //zmiana id krawedzi
                    l.idKrawedzi = linkId;

                    //dodanie krawedzi do listy potencjalnych krawedzi
                    this.potentialLinks.Add(l);
                }
                else
                {
                    throw new Exception("The network hasn't got one or both of the nodes with ids: " + l.Wezel1.idWezla +
                                        " and " + l.Wezel2.idWezla + "!");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        /// <summary>
        /// Dodaje link do listy potencjalnych linkow w topologii, zaczynajacy sie od wezla w1 i konczacy na w2.
        /// </summary>
        /// <param name="l"></param>
        public void addPotentialLink(Wezel w1, Wezel w2)
        {
            try
            {
                if (this.network.wezly.Contains(w1) && this.network.wezly.Contains(w2))
                {
                    //Index nastepnego lacza w networki
                    int linkId = this.network.krawedzie.Count - 1 + this.potentialLinks.Count - 1 + 1;
                    this.potentialLinks.Add(new Lacze(linkId, w1.idWezla, w2.idWezla));
                }
                else
                {
                    throw new Exception("The network hasn't got one or both of the nodes with ids: " + w1.idWezla +
                        " and " + w2.idWezla + "!");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        /// <summary>
        /// Dodaje SNP (wezel) do topologii.
        /// </summary>
        /// <param name="snp"></param>
        public void addNode(SubNetworkPoint snp)
        {
            //Konwersja IP na inta (jako ID wezla)
            int id = BitConverter.ToInt32(snp.ipaddress.GetAddressBytes(), 0);

            //Wspolrzedna X to id i Y to tez id, unikniemy niejednoznacznosci.
            this.network.zwroc_wezly.Add(new Wezel(id, id, id));
        }

        /// <summary>
        /// Uruchamia algorytm najkrotszej sciezki.
        /// </summary>
        public void runShortestPathAlgorithm()
        {
            this.network.algorytmFloyda();
        }

        /// <summary>
        /// Wyznacza ścieżkę pomiędzy jednym SNPP a drugim SNPP, uwzględniając łącza potencjalne. 
        /// </summary>
        /// <param name="beginningNetworkPointPool"></param>
        /// <param name="endNetworkPointPool"></param>
        /// <param name="band"></param>
        /// <returns></returns>
        [Obsolete]
        public List<SubNetworkPointPool> routePath(SubNetworkPointPool beginningNetworkPointPool,
            SubNetworkPointPool endNetworkPointPool, short band)
        {
            var path = getPathOfSNPPs(beginningNetworkPointPool, endNetworkPointPool, band);

            //0 w ścieżce oznacza, że nie da się zestawić połączenia
            if (path.Count == 0)
            {
                //Skopiowanie sieci
                var newNetwork = new Siec(network);

                newNetwork.krawedzie = newNetwork.krawedzie.OrderByDescending(o => o.Waga).ToList();

                for (int i = 0; i < newNetwork.krawedzie.Count; i++)
                {
                    Wezel W1 = new Wezel(newNetwork.krawedzie[i].Wezel1.idWezla, newNetwork.krawedzie[i].Wezel1.idWezla,
                        newNetwork.krawedzie[i].Wezel1.idWezla, newNetwork.krawedzie[i].Wezel1.ip);
                    Wezel W2 = new Wezel(newNetwork.krawedzie[i].Wezel2.idWezla, newNetwork.krawedzie[i].Wezel2.idWezla,
                        newNetwork.krawedzie[i].Wezel2.idWezla, newNetwork.krawedzie[i].Wezel2.ip);

                    Lacze potentialLink = potentialLinks.Find(x => x.Wezel1.idWezla == W1.idWezla && x.Wezel2.idWezla == W2.idWezla
                    && x.Wezel1.ip == W1.ip && x.Wezel2.ip == W2.ip);

                    //udalo sie znalezc lacze
                    if (potentialLink != null)
                    {
                        //Wstawiamy nowe łącze na koniec listy
                        potentialLink.idKrawedzi = newNetwork.krawedzie.Count;
                        newNetwork.krawedzie.Add(potentialLink);
                        newNetwork.aktualizujLiczniki();

                        //Po wstawieniu nowego łącza do eksperymentalnej sieci, zapuszaczamy algorytm Floyda
                        newNetwork.algorytmFloyda();

                        path = getPathOfSNPPs(beginningNetworkPointPool, endNetworkPointPool, band);

                        //nie udało się 
                        if (path.Count == 0)
                        {
                            continue;
                        }
                        //udało się, dzięki wstawieniu krawędzi z potencjalnej udało się stworzyć ścieżkę
                        else
                        {
                            //Pierwszy wezel sciezki
                            Wezel beginNode = newNetwork.wezly.Find(x => x.ip == path[0].snps[0].ipaddress.ToString());
                            //Ostatni wezel sciezki
                            Wezel endNode = newNetwork.wezly.Find(x => x.ip == path[path.Count - 1].snps[0].ipaddress.ToString());

                            //Wyznaczamy sciezke jeszcze raz, by uzyskac liste laczy
                            var links = new Sciezka().wyznaczSciezke(beginNode, endNode, newNetwork.zwrocTabliceKierowaniaLaczami,
                                newNetwork.zwrocTabliceKierowaniaWezlami, ref newNetwork.wezly, band, newNetwork.Koszty);

                            foreach (Lacze link in links)
                            {
                                //Jeżeli danej krawędzi z wyznaczonej ścieżki nie ma w wyjściowej topologii, trzeba ją dodać do tej topologii
                                if (!network.zwroc_lacza.Contains(link))
                                {
                                    network.krawedzie.Add(link);
                                    //jeszcze zmiana id krawedzi na jej indeks
                                    link.idKrawedzi = network.krawedzie.Count - 1;
                                    //Lacze juz nie jest potencjalne, lecz rzeczywiste
                                    this.potentialLinks.Remove(link);
                                }
                            }

                            return path;
                        }
                    }
                }

                //Zwrocenie pustej listy, gdy nic sie nie udalo zrobic
                return new List<SubNetworkPointPool>();
            }
            //A jak Count != 0, to mozna zwrocic sceizke
            else
            {
                return path;
            }
            return null;
        }

        public List<SubNetworkPointPool> getPathOfSNPPs(SubNetworkPointPool beginningNetworkPointPool,
            SubNetworkPointPool endNetworkPointPool, short band, Siec network)
        {
            //Zamiana adresow IP na inty
            int beginID = ipToInt(beginningNetworkPointPool.snps[0].ipaddress);
            int endID = ipToInt(endNetworkPointPool.snps[0].ipaddress);

            //Tworzenie wezlow
            Wezel beginNode = new Wezel(beginID, beginID, beginID, beginningNetworkPointPool.snps[0].ipaddress.ToString());
            Wezel endNode = new Wezel(endID, endID, endID, endNetworkPointPool.snps[0].ipaddress.ToString());

            //Wyszukiwanie sciezki
            Sciezka path = new Sciezka();

            path.wyznaczSciezke(beginNode, endNode, network.zwrocTabliceKierowaniaLaczami,
                network.zwrocTabliceKierowaniaWezlami, ref network.wezly, band, network.Koszty);

            if (path.KrawedzieSciezki.Count != 0)
            {
                network.sciezki.Add(path);
            }

            //network.zwroc_sciezki.Find((x) => x.Wezel1 == beginNode && x.Wezel2 == endNode));network.zwroc_sciezki.Find((x) => x.Wezel1 == beginNode && x.Wezel2 == endNode);

            //Gdy sciezki w networki nie ma, to nie da sie ustanowic polaczenia
            if (path == null)
                return null;
            else
            {
                List<SubNetworkPointPool> SNPPs = new List<SubNetworkPointPool>();
                foreach (Wezel w in path.WezlySciezki)
                {
                    SubNetworkPoint snp = new SubNetworkPoint(IPAddress.Parse(w.ip));

                    SubNetworkPointPool snpp = new SubNetworkPointPool();

                    snpp.Add(snp);

                    SNPPs.Add(snpp);
                }

                //Zwracanie listy SNPPów, gdzie kazdy z nich zawiera po jednym SNP.
                return SNPPs;
            }
        }

        /// <summary>
        /// Wyznacza ścieżkę między dwoma SNPPami, korzystając z istniejących łączy.
        /// </summary>
        /// <param name="beginningNetworkPoint"></param>
        /// <param name="endNetworkPoint"></param>
        /// <returns></returns>
        public List<SubNetworkPointPool> getPathOfSNPPs(SubNetworkPointPool beginningNetworkPointPool,
            SubNetworkPointPool endNetworkPointPool, short band)
        {
            return getPathOfSNPPs(beginningNetworkPointPool, endNetworkPointPool, band, this.network);
        }

        public NetworkPath getNetworkPath(SubNetworkPointPool beginningNetworkPointPool,
            SubNetworkPointPool endNetworkPointPool, short band, Siec network, short frequency)
        {
            //Zamiana adresow IP na inty
            int beginID = ipToInt(beginningNetworkPointPool.snps[0].ipaddress);
            int endID = ipToInt(endNetworkPointPool.snps[0].ipaddress);

            //Tworzenie wezlow
            Wezel beginNode = this.network.wezly.Find(x => x.SNPP == beginningNetworkPointPool);// new Wezel(beginID, beginID, beginID, beginningNetworkPointPool.snps[0].ipaddress.ToString());
            Wezel endNode = this.network.wezly.Find(x => x.SNPP == endNetworkPointPool); //new Wezel(endID, endID, endID, endNetworkPointPool.snps[0].ipaddress.ToString());

            //Wyszukiwanie sciezki
            Sciezka path = new Sciezka();

            //path.wyznaczSciezke(beginNode, endNode, network.zwrocTabliceKierowaniaLaczami,
                //network.zwrocTabliceKierowaniaWezlami, ref network.wezly, band, network.Koszty, frequency);

            path.wyznaczSciezke(beginNode, endNode, band, frequency, network);

            //Gdy sciezki w networki nie ma, to nie da sie ustanowic polaczenia
            if (path.KrawedzieSciezki.Count != 0)
            {
                //Nowy obiekt klasy NetworkPath ze ścieżką "path"
                NetworkPath networkPath = new NetworkPath(path);

                //Dodawanie do dodanych sciezek
                this.pathsCopy.Add(networkPath);

                network.sciezki.Add(path);

                return networkPath;
            }
            //Sciezka jest pusta
            else
            {
                return null;
            }
            //network.zwroc_sciezki.Find((x) => x.Wezel1 == beginNode && x.Wezel2 == endNode));network.zwroc_sciezki.Find((x) => x.Wezel1 == beginNode && x.Wezel2 == endNode);
        }

        /// <summary>
        /// Zamienia adres IP na int32
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static int ipToInt(IPAddress IP)
        {
            return BitConverter.ToInt32(IP.GetAddressBytes(), 0);
        }

        /// <summary>
        /// Zamienia int32 na IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static IPAddress intToIP(int ip)
        {
            IPAddress IP = new IPAddress(BitConverter.GetBytes(ip));
            return IP;
        }

        /// <summary>
        /// Funkcja, zmieniajaca koszt lacza na podstawie nowego pasma zuzywanego przez nowy zasob.
        /// </summary>
        /// <param name="band"></param>
        public bool changeCost(short band, int idLacza)
        {
            try
            {
                Lacze link = this.network.krawedzie[idLacza];

                SubNetworkPoint SNP1 = link.Wezel1.SNPP.snps.Find(x => x.portOUT == link.idKrawedzi);
                SubNetworkPoint SNP2 = link.Wezel2.SNPP.snps.Find(x => x.portIN == link.idKrawedzi);

                if (SNP1 == null || SNP2 == null)
                {
                    return false;
                }

                //Jezeli jest jeszcze miejsce
                if (EONTable.capacity - Math.Sqrt(link.Waga) >= band && SNP1.eonTable.FindFreeFrequency(band, "out") != -1
                    && SNP2.eonTable.FindFreeFrequency(band, "in") != -1)
                {
                    //Przypisanie nowego kosztu
                    link.Waga = (float)Math.Pow((Math.Sqrt(link.Waga) + band), 2);

                    //sukces
                    return true;
                }
                //Nie ma juz miejsca na laczu na takie pasmo (na wszelki wypadek)
                else
                {
                    //porazka
                    return false;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Topology.changeCost(): " + E.Message);
                return false;
            }
        }

        /// <summary>
        /// Funkcja, zmieniajaca koszt lacza na podstawie nowego pasma zuzywanego przez nowy zasob.
        /// </summary>
        /// <param name="band"></param>
        /// <param name="lacze"></param>
        /// <returns></returns>
        public bool changeCost(short band, Lacze lacze)
        {
            //Znalezienie indeksu
            try
            {
                int idLacza = this.network.krawedzie.FindIndex(x => x == lacze);
                return changeCost(band, idLacza);
            }
            catch (Exception E)
            {
                Console.WriteLine("Topology.changeCost(): " + E.Message);
                return false;
            }
        }
    }
}
