using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SubNetwork;


namespace AISDE
{
    public class Siec
    {

        protected int liczbaWezlow;
        protected int liczbaLaczy;
        protected int liczbaKabli;
        string algorytm;
        Wezel centrala;
        public List<Wezel> wezly;
        public List<Lacze> krawedzie;
        public List<Sciezka> sciezki;
        public List<int> tablicaMST;
        // tablica kierowania wezlami - zawiera referencje do Wezla, do ktorego nalezy sie udac w drodze z a do b
        Wezel[,] tablicaKierowaniaWezlami;
        // tablica kierowania krawedziami - zawiera indeks krawedzi ktora prowadzi z wzezla a do b, gdzie a i b sa sasiadami
        Lacze[,] tablicaKierowaniaLaczami;
        float[,] tablicaKosztow;
        int[,] suma;
        List<Lacze> usunieteKrawedzie;
        bool brakPowodzenia = false;

        public bool ustawPoczatkoweKoszty;

        public Siec()
        {
            wezly = new List<Wezel>();
            krawedzie = new List<Lacze>();
            sciezki = new List<Sciezka>();
            tablicaMST = new List<int>();
            tablicaKierowaniaLaczami = new Lacze[0, 0];
            tablicaKierowaniaWezlami = new Wezel[0, 0];
            tablicaKosztow = new float[0, 0];
            usunieteKrawedzie = new List<Lacze>();
            ustawPoczatkoweKoszty = true;
        }

        /// <summary>
        /// Konstruktor kopiujący
        /// </summary>
        /// <param name="siec"></param>
        public Siec(Siec siec)
        {
            this.liczbaWezlow = siec.liczbaWezlow;
            this.liczbaLaczy = siec.liczbaLaczy;
            this.liczbaKabli = siec.liczbaKabli;
            this.algorytm = siec.algorytm;
            this.centrala = siec.centrala;
            wezly = new List<Wezel>(siec.wezly);
            krawedzie = new List<Lacze>(siec.krawedzie);
            sciezki = new List<Sciezka>(siec.sciezki);
            tablicaMST = new List<int>(siec.tablicaMST);
            this.tablicaKierowaniaWezlami = siec.tablicaKierowaniaWezlami;
            this.tablicaKierowaniaLaczami = siec.tablicaKierowaniaLaczami;
            this.tablicaKosztow = siec.tablicaKosztow;
            this.suma = siec.suma;
            this.usunieteKrawedzie = new List<Lacze>(siec.usunieteKrawedzie);
            this.brakPowodzenia = siec.brakPowodzenia;
            ustawPoczatkoweKoszty = siec.ustawPoczatkoweKoszty;
        }

        public List<Lacze> usuniete
        {
            get { return usunieteKrawedzie; }
            set { this.usunieteKrawedzie = value; }
        }
        /* public int[,] tablicaKosztow
         {
             get { return suma; }
             set { suma = value; }
         }*/
        public float[,] Koszty
        {
            get { return tablicaKosztow; }
            set { this.tablicaKosztow = value; }
        }

        public Lacze[,] zwrocTabliceKierowaniaLaczami
        {
            get { return tablicaKierowaniaLaczami; }
            set { this.tablicaKierowaniaLaczami = value; }
        }

        public Wezel[,] zwrocTabliceKierowaniaWezlami
        {
            get { return tablicaKierowaniaWezlami; }
            set { this.tablicaKierowaniaWezlami = value; }
        }

        public List<Sciezka> zwroc_sciezki
        {
            get { return this.sciezki; }
            set { this.sciezki = value; }
        }

        public List<Lacze> zwroc_lacza
        {
            get { return krawedzie; }
            set { krawedzie = value; }
        }
        public List<int> zwroc_tablicaMST
        {
            get { return tablicaMST; }
        }

        public List<Wezel> zwroc_wezly
        {
            get { return wezly; }
        }

        public string Algorytm
        {
            get { return algorytm; }

        }

        public bool fail
        {
            get { return brakPowodzenia; }
        }

        public void ustawWagiLaczy(SubNetworkPointPool snpp)
        {
        }

        public void wylosujWagiLaczy()
        {
            Random rnd = new Random();
            foreach (Lacze krawedz in krawedzie)
            {
                krawedz.Waga = rnd.Next(101);
            }

        }


        public int algorytmPrima()
        {
            //Bedziemy szukac najtanszych krawedzi, ktore spelniaja zalozenia algorytmu Prima, czyli: jeden wierzcholek krawedzi nalezy do drzewa, a drugi nie.
            //
            //Najtansza krawedź jest pierwsza bo Lista będzie posortowana 
            // try
            // {



            //Dwa konce lacza o najtanszej wadze oznaczamy jako odwiedzone
            // krawedzie[0].Wezel1.Odwiedzony = true;
            // krawedzie[0].Wezel2.Odwiedzony = true;
            // tablicaMST.Add(krawedzie[0].idKrawedzi);
            for (int i = 0; i < wezly.Count; i++)
            {
                wezly[i].Odwiedzony = false;
            }

            wezly[0].Odwiedzony = true;
            //Sa to dwa konce najtanszej krawedzi
            int liczbaOdwiedzonychWezlow = 1;
            int najlepszeLacze = 0;
            int wykorzystaneKrawedzie = 0;
            int koniec = 0;

            Lacze pomocnicze = new Lacze(0, 0, 0);

            do
            {
                //Petla do-While nie bedzie sprawdzala wykorzystanych juz krawedzi.
                int k = 0;
                koniec = 0;
                //za kazdym razem szukam najlepszego wierzcholka ktory pasuje do warunkow algorytmu 
                do
                {
                    //Jeden z wierzcholkow musi byc nalezec do drzewa drugi-nie. Sprawdzam czy ten warunek zachodzi. 
                    //Odwoluje sie do wlasnosci klasy Wezel "Odwiedzony". Numer indeksu pomniejszony o 1 gdyz ID zaczyna sie od 1 a indeksowanie od 0
                    //Pierwsze poloczenie ktore spelnia warunki algorytmu zapisuje jako najlepsze po przez zapisanie indeksu do tego poloczenia.
                    if (krawedzie[k].Wezel1.Odwiedzony == true ^ krawedzie[k].Wezel2.Odwiedzony == true)
                    {
                        najlepszeLacze = k;
                        koniec = 1;
                    }
                    k++;

                } while (koniec == 0);
                //Petla nie sprawdza juz wykorzystanych Krawedzi
                for (int i = wykorzystaneKrawedzie; i < zwroc_lacza.Count; i++)
                {

                    //jeden z wierzcholkow musi byc nalezec do drzewa drugi-nie
                    //Przeszukuje liste krawedzi w poszukiwaniu takich, ktore spelniaja warunki i maja mniejsza wage niz dotychczas najlepsze lacze
                    if (krawedzie[i].Wezel1.Odwiedzony == true ^ krawedzie[i].Wezel2.Odwiedzony == true)
                    {
                        if (krawedzie[i].Waga < krawedzie[najlepszeLacze].Waga)
                        {
                            najlepszeLacze = i;
                        }
                    }

                }
                //Wezly nalezace do krawedzi oznaczam jako odwiedzone
                krawedzie[najlepszeLacze].Wezel1.Odwiedzony = true;
                krawedzie[najlepszeLacze].Wezel2.Odwiedzony = true;

                tablicaMST.Add(krawedzie[najlepszeLacze].idKrawedzi);

                //Tu jest to usprawnienie
                //Po sprawdzeniu krawedzi przesuwam ja na miejsce zalezace od ilosci sprawdzonych krawedzi
                //Gdy kolejny raz wchodzi do petli for to nie sprawdza juz tej krawedzi, mniej razy chodzi petla.   
                if (najlepszeLacze != wykorzystaneKrawedzie)
                {
                    pomocnicze = krawedzie[najlepszeLacze];
                    krawedzie.RemoveAt(najlepszeLacze);
                    krawedzie.Insert(wykorzystaneKrawedzie, pomocnicze);

                }
                wykorzystaneKrawedzie++;
                liczbaOdwiedzonychWezlow++;

                //no wiadomo krazy do czasu az wszystkie wierzcholki beda nalezaly do drzewa 
            }
            while (liczbaOdwiedzonychWezlow != wezly.Count);

            //  }
            //  catch (Exception)
            //  {
            //      brakPowodzenia = true;
            //  }

            return 0;
        }

        public int algorytmDijkstry()
        {
            int liczbaOdwiedzonychWezlow = 0;
            int INF = int.MaxValue;
            int najtanszyWezel = 0;
            int koniec;
            int k = 0;
            int nieDolaczoneWezly = 0;
            Wezel pomocniczy = new Wezel();
            List<Wezel> wezlyDijktry = new List<Wezel>(wezly);

            for (int i = 0; i < wezly.Count; i++)
            {
                wezly[i].Odwiedzony = false;
            }

            for (int i = 0; i < wezly.Count; i++)
            {
                if (wezly[i].listaKrawedzi.Count == 0)
                {
                    nieDolaczoneWezly++;
                }
            }

            /*
            Dla potrzeb algorytmu dodalem w kasie Wezel trzy 2 nowe zmienne i liste przechowujaca indeksy krawedzi, ktore sa doprowadzone do konkretnego Wezla.
            Dwie zmienne to : etykieta, czyli najtanszy koszt dotarcia do wierzcholka oraz zmienna dzieki ktorej wiem przez ktory wierzcholek nalezy do tego punktu isc.
            Pierwszy wierzcholek sciezki ma etykiete 0 a pozostale na nieskonczonosc, ktora zdefiniowalem jako 1000.
            */
            for (int i = 0; i < wezly.Count; i++)
            {
                wezly[i].Etykieta = INF;
            }
            sciezki[0].Wezel1.Etykieta = 0;

            do
            {
                koniec = 0;
                //wybieram pierwszy dowolny wezel, ktory posluzy mi jako odnosnik do wyszukiwania najkorzystniejszego wezla.
                do
                {
                    if (wezly[k].Odwiedzony == false)
                    {
                        najtanszyWezel = k;
                        koniec = 1;
                    }
                    k++;

                } while (koniec == 0);

                //Poszukiwanie najtanszego
                for (int i = liczbaOdwiedzonychWezlow; i < wezly.Count; i++)
                {
                    if (wezly[i].Odwiedzony == false)
                    {
                        if (wezly[i].Etykieta < wezly[najtanszyWezel].Etykieta)
                        {
                            najtanszyWezel = i;
                        }
                    }
                }
                //Tu wykorzystuje liste ktora stworzylem w klasie wezel po to, aby nie szukac wsrod wszystkich krawedzi. Wiem z gory ktore krawedzie musze przegladnac.
                //Wiem jakich mam sasiadow
                foreach (Lacze krawedz in wezly[najtanszyWezel].listaKrawedzi)
                {
                    //Jezeli wybrany jest najtanszy wezel to sprawdzam, ktory jest to wezel w krawedzi. WezelPierwszy, czy WezelDrugi
                    if (wezly[najtanszyWezel].idWezla == krawedz.Wezel1.idWezla)
                    {
                        //Jezeli ten sasiad byl juz przegladany to nie bedzie dla niego lepszego polonczenia, jest "skreslony"
                        if (krawedz.Wezel2.Odwiedzony == false)
                        {
                            if (krawedz.Wezel2.Etykieta > (krawedz.Waga + krawedz.Wezel1.Etykieta))
                            {
                                krawedz.Wezel2.Etykieta = krawedz.Waga + krawedz.Wezel1.Etykieta;
                                //Jezeli to polonczenie okazuje sie byc najkorzystniejsze to zmieniam Wezel przez ktory droga jest najtansza
                                krawedz.Wezel2.NajlepiejPrzez = wezly[najtanszyWezel];
                                // temp = krawedz.idKrawedzi;
                            }
                        }
                    }
                    else
                    {
                        if (krawedz.Wezel1.Odwiedzony == false)
                        {
                            if (krawedz.Wezel1.Etykieta > (krawedz.Waga + krawedz.Wezel2.Etykieta))
                            {
                                krawedz.Wezel1.Etykieta = krawedz.Waga + krawedz.Wezel2.Etykieta;
                                krawedz.Wezel1.NajlepiejPrzez = wezly[najtanszyWezel];
                            }
                        }
                    }
                }

                wezly[najtanszyWezel].Odwiedzony = true;
                //tablicaMST.Add(temp);

                // Przejmuje referencje do obiektu, usuwam najkorzystniejszy wezel z jego miejsca w liscie i umieszczam na miejscach, gdzie nie bede juz sprawdzal
                //Pętle for beze zaczynal dla i= liczbieOdwiedzonychWezlow czyli petla ich już nie obejmuje, czyli petla krazy mniejszą ilosc razy.
                if (najtanszyWezel != liczbaOdwiedzonychWezlow)
                {
                    pomocniczy = wezly[najtanszyWezel];
                    //usuwanie z listy
                    wezly.RemoveAt(najtanszyWezel);
                    //dodawanie do listy na miejscu wskazanym 
                    wezly.Insert(liczbaOdwiedzonychWezlow, pomocniczy);
                }

                liczbaOdwiedzonychWezlow++;

            } while ((liczbaOdwiedzonychWezlow + nieDolaczoneWezly) != (wezly.Count - 1));

            //Tutaj sprawdzalem czy wypisuje jak trzeba sciezke

            Wezel zmienna1 = sciezki[0].Wezel2;
            Wezel zmienna = sciezki[0].Wezel2.NajlepiejPrzez;

            try
            {
                do
                {
                    for (int i = 0; i < liczbaLaczy; i++)
                    {
                        if ((krawedzie[i].Wezel1.idWezla == zmienna1.idWezla && krawedzie[i].Wezel2.idWezla == zmienna.idWezla) || (krawedzie[i].Wezel2.idWezla == zmienna1.idWezla && krawedzie[i].Wezel1.idWezla == zmienna.idWezla))
                        {
                            tablicaMST.Add(krawedzie[i].idKrawedzi);
                        }
                    }
                    zmienna1 = zmienna;
                    zmienna = zmienna.NajlepiejPrzez;

                } while (zmienna1 != sciezki[0].Wezel1);
            }
            catch
            {

            }
            //Lacze result = krawedzie.Find(x => x.Wezel1.idWezla == sciezki[0].Wezel2.idWezla , x => x.Wezel2.idWezla == zmienna.idWezla);
            /* while (zmienna.NajlepiejPrzez != sciezki[0].Wezel1) 
             {
                 zmienna = zmienna.NajlepiejPrzez;
                     Console.WriteLine(zmienna.idWezla);             
             }
             Console.WriteLine(sciezki[0].Wezel1.idWezla);*/


            return 0;
        }

        public void algorytmFloyda()
        {
            aktualizujLiczniki();

            // ustawWagiLaczy();

            float INFINITY = 64*64; //Nieskonoczonosc

            //Ustawiamy początkowe wartości
            if (ustawPoczatkoweKoszty)
            {
                //tablica kosztow - zawiera koszt z wezla o identyfikatorze nr a do wezla nr b
                //Koszt dostania się z a do b to tablicaKosztow[a,b] przechowuje koszt dostania się z węzła a do węzła b
                tablicaKosztow = new float[liczbaWezlow, liczbaWezlow];

                // tablica kierowania wezlami - zawiera referencje do Wezla, do ktorego nalezy sie udac w drodze z a do b
                tablicaKierowaniaWezlami = new Wezel[liczbaWezlow, liczbaWezlow];

                // tablica kierowania krawedziami - zawiera indeks krawedzi ktora prowadzi z wzezla a do b, gdzie a i b sa sasiadami
                tablicaKierowaniaLaczami = new Lacze[liczbaWezlow, liczbaWezlow];

                ustawPoczatkoweKoszty = false;
            }

            //indeksy wezlow w tabeli
            int w1, w2;

            foreach (Lacze lacze in krawedzie)
            {
                w1 = wezly.FindIndex(x => x.idWezla == lacze.wezel1);
                w2 = wezly.FindIndex(x => x.idWezla == lacze.wezel2);
                if (w1 >= 0 && w2 >= 0)
                {
                    if (tablicaKierowaniaLaczami[w1, w2] == null) //Jezeli jeszcze nic nie bylo tutaj, to przypisujemy wartosci
                    {
                        tablicaKierowaniaLaczami[w1, w2] = lacze; //przypisanie wartosci do tablicy. Odejmuje 1, bo na wejsciu numeracja wezlow zaczyna sie od 1
                                                                  //tablicaKierowaniaLaczami[w2, w1] = lacze; //Z b do a tez idziemy przez dane lacze

                        tablicaKierowaniaWezlami[w1, w2] = lacze.Wezel2; //Idac z wezla1 do wezla2 dostaniemy sie do wezla2
                        //tablicaKierowaniaWezlami[w2, w1] = lacze.Wezel1; //I vice versa
                    }
                    else
                    {
                        Lacze temp = tablicaKierowaniaLaczami[w1, w2];
                        if (temp.Waga > lacze.Waga) //Jesli dotychczasowa waga byla wieksza, podmieniamy. Jesli nie - nic nie robimy.
                        {
                            tablicaKierowaniaLaczami[w1, w2] = lacze; //przypisanie wartosci do tablicy. Odejmuje 1, bo na wejsciu numeracja wezlow zaczyna sie od 1
                                                                      //tablicaKierowaniaLaczami[w2, w1] = lacze; //Z b do a tez idziemy przez dane lacze

                            tablicaKierowaniaWezlami[w1, w2] = lacze.Wezel2; //Idac z wezla1 do wezla2 dostaniemy sie do wezla2
                            //tablicaKierowaniaWezlami[w2, w1] = lacze.Wezel1; //I vice versa
                        }
                    }
                }
            }

/*
            foreach (Lacze lacze in krawedzie)
            {
                w1 = wezly.FindIndex(x => x.idWezla == lacze.wezel1);
                w2 = wezly.FindIndex(x => x.idWezla == lacze.wezel2);
                if (tablicaKierowaniaWezlami[w1, w2] == null)
                {
                    tablicaKierowaniaWezlami[w1, w2] = lacze.Wezel2; //Idac z wezla1 do wezla2 dostaniemy sie do wezla2
                    //tablicaKierowaniaWezlami[w2, w1] = lacze.Wezel1; //I vice versa
                }
                else
                {
                    Lacze temp = tablicaKierowaniaLaczami[w1, w2];
                    if (temp.Waga > lacze.Waga) //Jesli dotychczasowa waga byla wieksza, podmieniamy. Jesli nie - nic nie robimy.
                    {
                        tablicaKierowaniaWezlami[w1, w2] = lacze.Wezel2; //Idac z wezla1 do wezla2 dostaniemy sie do wezla2
                        //tablicaKierowaniaWezlami[w2, w1] = lacze.Wezel1; //I vice versa
                    }
                }
            } */
            //Potrzebujemy niby polowy z tego zakresu, ale nie wiem czy nie bedzie wprowadzonych lacz skierowanych. W tym przypadku trzeba by znowu wykorzystac 
            // [liczbaWezlow, liczbaWezlow]
            // tablicaKierowaniaWezlami[a, b] zwroci Wezel, przez ktory mamy przejsc idac z a do b
            // tablicakierowaniaLaczami[a, b] zwroci Lacze, ktore laczy bezposrednio a i b
            // jesli zawartosc komorki [a,b] to null, to znaczy, ze nie ma bezposredniego lacza z a do b
            // jesli zawartosc tablicaKierowaniaWelzami[a, b] = b, to istnieje bezposrednie polaczenie miedzy nimi
            // W przypadku bezposredniego polaczenia miedzy wezlami,tablica kierowania laczami [a, b] bedzie zawierala krawedz
            // o najmniejszym koszcie

            /*
            Console.WriteLine("Tablica poczatkowych kosztow:");
            for (int i = 0; i < liczbaWezlow; i++) //Wyswietlanie tablic: kosztow i kierowania wezlami
            {
                Console.WriteLine();
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    //Odejmowanie floatow
                    if (Math.Abs(tablicaKosztow[i, j] - INFINITY) < 1)
                        Console.Write("inf\t\t");
                    else
                        Console.Write($"{tablicaKosztow[i, j]}\t\t");
                }
            } */

            for (int i = 0; i < liczbaWezlow; i++)
            for (int j = 0; j < liczbaWezlow; j++)
            {
                if (i == j)
                    tablicaKosztow[i, j] = 0; //Koszt dojscia do samego siebie to 0
                else if (tablicaKierowaniaLaczami[i, j] != null) //Czyli istnieje pojedyncze lacze pomiedzy wezlem i a wezlem j
                    tablicaKosztow[i, j] = tablicaKierowaniaLaczami[i, j].Waga;
                else
                    tablicaKosztow[i, j] = INFINITY;
            }

            for (int k = 0; k < liczbaWezlow; k++) //Tak jak na slajdzie wykladowym
                for (int i = 0; i < liczbaWezlow; i++)
                    for (int j = 0; j < liczbaWezlow; j++)
                        if (tablicaKosztow[i, j] > tablicaKosztow[i, k] + tablicaKosztow[k, j])
                        {
                            tablicaKosztow[i, j] = tablicaKosztow[i, k] + tablicaKosztow[k, j];
                            //tablicaKosztow[j, i] = tablicaKosztow[i, k] + tablicaKosztow[k, j];
                            /*
                                                        Wezel w = znajdzNajblizszegoSasiadaProwadzocegoDo(wezly[i], wezly[j]);

                                                        if (w != null)
                                                        {
                                                            tablicaKierowaniaWezlami[i, j] = w; //k-ty Wezel lub blizszy
                                                            tablicaKierowaniaWezlami[j, i] = w;
                                                        }
                                                        */
                            tablicaKierowaniaWezlami[i, j] = wezly[k];
                        }

            for (int i = 0; i < liczbaWezlow; i++)
            {
                tablicaKierowaniaWezlami[i, i] = wezly[i];
            }

            /*for (int k = 0; k < liczbaWezlow; k++) //Tak jak na slajdzie wykladowym
                for (int i = 0; i < liczbaWezlow; i++)
                    for (int j = 0; j < liczbaWezlow; j++)
                        if (tablicaKosztow[i, j] > tablicaKosztow[i, k] + tablicaKosztow[k, j])
                        {
                            tablicaKierowaniaWezlami[i, j] = wezly[k];
                            //tablicaKierowaniaWezlami[j, i] = wezly[k];
                        }
                        */

            //tabliceFloyd(zwrocTabliceKierowaniaLaczami, tablicaKierowaniaWezlami, tablicaKosztow);
            /*foreach (Sciezka sciezka in sciezki)
            {
                sciezka.KrawedzieSciezki = sciezka.wyznaczSciezke(sciezka.Wezel1, sciezka.Wezel2, tablicaKierowaniaLaczami, 
                    tablicaKierowaniaWezlami, ref wezly, 1, Koszty);
                sciezka.wyznaczWezly(sciezka.Wezel1);
                //sciezka.pokazSciezke();
                //Console.WriteLine();
            } */
            //testFloyd(ref tablicaKierowaniaLaczami, ref tablicaKierowaniaWezlami);
        }

        /// <summary>
        /// Znajduje najblizsy wezel prowadzacy do danego wezla
        /// </summary>
        /// <param name="skad"></param>
        /// <param name="dokad"></param>
        /// <returns></returns>
        public Wezel znajdzNajblizszegoSasiadaProwadzocegoDo(Wezel skad, Wezel dokad)
        {
            try
            {
                if (tablicaKierowaniaWezlami[skad.idWezla - 1, dokad.idWezla - 1] == dokad)
                    return dokad;
                else if (skad != null)
                {
                    return znajdzNajblizszegoSasiadaProwadzocegoDo(skad,
                        tablicaKierowaniaWezlami[skad.idWezla - 1, dokad.idWezla - 1]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception E)
            {
                return null;
            }

        }

        /// <summary>
        /// Znajduje indeks najbliższego węzła, prowadzącego do danego węzła
        /// </summary>
        /// <param name="indexSkad"></param>
        /// <param name="indexDokad"></param>
        /// <returns></returns>
        public int znajdzNajblizszegoSasiadaProwadzocegoDo(int indexSkad, int indexDokad)
        {
            if (tablicaKierowaniaWezlami[indexSkad, indexDokad] == wezly[indexDokad])
                return indexDokad;
            else
                return znajdzNajblizszegoSasiadaProwadzocegoDo(indexSkad,
                    tablicaKierowaniaWezlami[indexSkad, indexDokad].idWezla - 1);
        }

        /// <summary>
        /// Czy ścieżka z w1 do w2 prowadzi bezpośrednio do w2, czy są tam jeszcze jakieś węzły?
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public bool saBliskimiSasiadami(Wezel w1, Wezel w2)
        {
            if (tablicaKierowaniaWezlami[w1.idWezla - 1, w2.idWezla - 1] == w2)
                return true;
            else
                return false;
        }

        public void tabliceFloyd(Lacze[,] tablicaKierowaniaLaczami, Wezel[,] tablicaKierowaniaWezlami, float[,] tablicaKosztow)
        {
            Console.WriteLine("\n\nTablica Kierowania Laczami:");
            for (int i = 0; i < liczbaWezlow; i++)
            {
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    if (tablicaKierowaniaLaczami[i, j] == null)
                        Console.Write($"[{i},{j}]: null\t");
                    else
                        Console.Write($"[{i},{j}]: {tablicaKierowaniaLaczami[i, j].idKrawedzi}\t");
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("\n\nTablica najmniejszych kosztow:");
            for (int i = 0; i < liczbaWezlow; i++) //Wyswietlanie tablic: kosztow i kierowania wezlami
            {
                Console.WriteLine();
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    //Odejmowanie floatow
                    if (Math.Abs(tablicaKosztow[i, j] - float.MaxValue) < 1)
                        Console.Write("inf\t\t");
                    else
                        Console.Write($"{tablicaKosztow[i, j]}\t\t");
                }
            }

            Console.WriteLine("\n\nTablica kierowania wezlami: ");
            for (int i = 0; i < liczbaWezlow; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    if (tablicaKierowaniaWezlami[i, j] == null)
                        Console.Write("null\t");
                    else if (tablicaKierowaniaWezlami[i, j].idWezla == j + 1)
                        Console.Write($"SA ({tablicaKierowaniaWezlami[i, j].idWezla})\t");
                    else
                        Console.Write($"{tablicaKierowaniaWezlami[i, j].idWezla}\t");
                }
            }
        }

        public void testFloyd(ref Lacze[,] tablicaKierowaniaLaczami, ref Wezel[,] tablicaKierowaniaWezlami)
        {
            Console.WriteLine();
            Console.WriteLine();
            Sciezka S1 = new Sciezka();
            Console.WriteLine($"Podaj nr wierzcholka(od 1 do {wezly.Count}), z ktorego wyruszamy");
            int nr1 = 0, nr2 = 0;
            try
            {
                nr1 = Int32.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine($"Podaj liczby od 1 do  {wezly.Count} ");
            }

            Console.WriteLine($"Podaj nr wierzcholka (od 1 do  {wezly.Count}), do ktorego chcesz sie udac:");

            try
            {
                nr2 = Int32.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine($"Podaj liczby od 1 do  {wezly.Count} ");
            }

            S1.Wezel1 = wezly[nr1 - 1]; //Ustawiamy poczatek i koniec Sciezki
            S1.Wezel2 = wezly[nr2 - 1];

            S1.KrawedzieSciezki = S1.wyznaczSciezke(wezly[nr1 - 1], wezly[nr2 - 1], tablicaKierowaniaLaczami,
                tablicaKierowaniaWezlami, ref wezly, 1, Koszty);
            S1.wyznaczWezly(wezly[nr1 - 1]);
            S1.pokazSciezke();
        }


        //Odczytuje plik z miejsca wskazanego przez uzytkownika
        public void wczytaj_dane(string plik)
        {
            List<string> dane_z_pliku = new List<string>();

            foreach (var linia in File.ReadAllLines(plik))
            {
                if (linia.StartsWith("#"))
                    continue; //Pomijamy linie zaczynające się od "#"
                else
                    dane_z_pliku.Add(linia);
            }
            //~Piotrek          Konwersja do postaci tablicowej
            string[] dane = dane_z_pliku.ToArray();

            //Zapisuje do tablicy caly plik.
            //Jeden wiersz to jedna komorka tablicy.

            //To co sie tu dzieje pozwala na podzial i zapis fragmentow tekstu ktory jest oddzielony spacja
            string[] liczbyDane = dane[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //wedlug wzorcowego pliku w pierwszej lini trzecim elementem bedzie liczba wezlow, ktore nalezy zapisac.
            liczbaWezlow = Int32.Parse(liczbyDane[2]);

            for (int i = 0; i < liczbaWezlow; i++)
            {

                try
                {
                    //Ponownie uzywam funkcji do wyluskania danych bedacych w jednym wierszu
                    //Do zmiennej "liczbyDane" zapisuje id wezla oraz jego wspolrzedne, ktore sa oddzielone spacja
                    liczbyDane = dane[i + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //Tworze kolejne obiekty klasy Wezel i za pomoca wlasnosci zapisuje dane z pliku.
                    wezly.Add(new Wezel() { idWezla = Int32.Parse(liczbyDane[0]), wspX = Int32.Parse(liczbyDane[1]), wspY = Int32.Parse(liczbyDane[2]), LKlientow = Int32.Parse(liczbyDane[3]) });
                }
                catch (Exception)
                {

                }
            }

            liczbyDane = dane[liczbaWezlow + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            liczbaLaczy = Int32.Parse(liczbyDane[2]);

            for (int j = 0; j < liczbaLaczy; j++)
            {

                try
                {
                    //Analogicznie jak wezly jednak do indekcowania uzywam wiadomosci o liczbie wersow ktore byly wyzej, aby zapisywac kolejne dane

                    liczbyDane = dane[(liczbaWezlow + 2 + j)].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    krawedzie.Add(new Lacze(Int32.Parse(liczbyDane[0]),
                        wezly[Int32.Parse(liczbyDane[1]) - 1], wezly[Int32.Parse(liczbyDane[2]) - 1], Int32.Parse(liczbyDane[3])));
                    //Podaje dla obiektow klasy wezel nr ID krawedzi, ktore zostaly do niego doprowadzone
                    wezly[Int32.Parse(liczbyDane[1]) - 1].wprowadzenieIndeksowKrawedzi(krawedzie[Int32.Parse(liczbyDane[0]) - 1]);
                    wezly[Int32.Parse(liczbyDane[2]) - 1].wprowadzenieIndeksowKrawedzi(krawedzie[Int32.Parse(liczbyDane[0]) - 1]);
                }
                catch (Exception)
                {

                }
            }

            //Kolejny dziwny indeks w nastepnej lini, ale takze wynika z ilosci danych wczesniej zapisanych

            liczbyDane = dane[liczbaWezlow + liczbaLaczy + 2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            liczbaKabli = Int32.Parse(liczbyDane[2]);

            for (int j = 0; j < liczbaKabli; j++)
            {

                try
                {
                    //Analogicznie jak wezly jednak do indekcowania uzywam wiadomosci o liczbie wersow ktore byly wyzej, aby zapisywac kolejne dane

                    liczbyDane = dane[(liczbaWezlow + liczbaLaczy + 3 + j)].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //kable.Add(new Kabel(Int32.Parse(liczbyDane[0]),
                    //Int32.Parse(liczbyDane[1]), Int32.Parse(liczbyDane[2])));
                }
                catch (Exception)
                {

                }
            }

            //    wylosujWagiLaczy();
            krawedzie.Sort((x, y) => x.Waga.CompareTo(y.Waga));
            //Informacja o tym jaki algorytm nalezy uruchomic.
            /*        if (algorytm == "MST")
                    {
                        if (sprawdzSpojnosc())
                        {
                         //   algorytmPrima();
                        }


                        //To co jest dalej nie ma glebszego sensu bo jeszcze nie mamy kolejnych algorytmow 
                    }
                    else if (algorytm == "SCIEZKA")
                    {
                        liczbyDane = dane[(dane.Length - 1)].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        sciezki.Add(new Sciezka() { Wezel1 = wezly[Int32.Parse(liczbyDane[0]) - 1], Wezel2 = wezly[Int32.Parse(liczbyDane[1]) - 1] });


                    }
                    else
                    {
                        int obecneMiejsce = liczbaWezlow + liczbaLaczy + 3;
                        for (; obecneMiejsce < dane.Length; obecneMiejsce++)
                        {
                            liczbyDane = dane[(obecneMiejsce)].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            sciezki.Add(new Sciezka() { Wezel1 = wezly[Int32.Parse(liczbyDane[0]) - 1], Wezel2 = wezly[Int32.Parse(liczbyDane[1]) - 1] });
                        }
                        algorytmFloyda();
                    }*/

        }

        public bool sprawdzSpojnosc()
        {
            algorytmFloyda();
            for (int i = 0; i < liczbaWezlow; i++)
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    if (i != j)
                    {
                        if (tablicaKierowaniaWezlami[i, j] == null && j != i)
                            return false;
                        else
                            continue;
                    }
                    else
                        continue;


                }
            return true;

        }

        public void kosztCalejSciezki()
        {
            algorytmFloyda();
            suma = new int[liczbaWezlow, liczbaWezlow];
            Wezel Pierwszy, Ostatni;
            Sciezka S1;
            int pomocnicza;

            for (int i = 0; i < liczbaWezlow; i++)
            {
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    if (i != j)
                    {
                        pomocnicza = 0;
                        Pierwszy = wezly[i];
                        Ostatni = wezly[j];
                        S1 = new Sciezka(Pierwszy, Ostatni);
                        S1.zwroc_ListaKrawedziSciezki = S1.wyznaczSciezke(Pierwszy, Ostatni, tablicaKierowaniaLaczami,
                            tablicaKierowaniaWezlami, ref wezly, 1, Koszty);
                        S1.wyznaczWezly(Pierwszy);
                        foreach (Lacze krawedz in S1.zwroc_ListaKrawedziSciezki)
                        {
                            pomocnicza = pomocnicza + (int)krawedz.Waga;
                        }
                        suma[i, j] = pomocnicza;

                    }
                    else
                    {
                        suma[i, j] = 0;
                    }
                }
            }


        }

        public List<Lacze> MaxSciezka()
        {
            algorytmFloyda();
            // kosztCalejSciezki();
            float sumaMax = 0;
            int poczatek = 0, koniec = 0;
            for (int i = 0; i < liczbaWezlow; i++)
            {
                for (int j = i + 1; j < liczbaWezlow; j++)
                {
                    try
                    {
                        if (tablicaKosztow[i, j] > sumaMax)
                        {
                            sumaMax = tablicaKosztow[i, j];
                            poczatek = i;
                            koniec = j;
                        }
                    }
                    catch
                    {

                    }

                }
            }
            Wezel Pierwszy, Ostatni;
            Sciezka S1;


            Pierwszy = wezly[poczatek];
            Ostatni = wezly[koniec];
            S1 = new Sciezka(Pierwszy, Ostatni);
            S1.zwroc_ListaKrawedziSciezki = S1.wyznaczSciezke(Pierwszy, Ostatni, tablicaKierowaniaLaczami,
                tablicaKierowaniaWezlami, ref wezly, 1, Koszty);
            return S1.zwroc_ListaKrawedziSciezki;
        }

        public void dodawanieWierzcholkow(int ilosc)
        {
            wezly.Clear();
            Random rand = new Random();
            int wspX, wspY, klienci;
            for (int i = 0; i < ilosc; i++)
            {
                int idWezla = i + 1;

                wspX = rand.Next(81);
                wspY = rand.Next(81);
                klienci = rand.Next(10);

                wezly.Add(new Wezel(idWezla, wspX, wspY, klienci));

            }
            liczbaWezlow = wezly.Count;

            for (int i = 0; i < sciezki.Count; i++)
            {
                for (int j = 0; j < liczbaWezlow; j++)
                {
                    if (sciezki[i].Wezel1.idWezla == wezly[j].idWezla)
                    {
                        sciezki[i].Wezel1 = wezly[j];
                    }
                    if (sciezki[i].Wezel2.idWezla == wezly[j].idWezla)
                    {
                        sciezki[i].Wezel2 = wezly[j];
                    }
                }

            }
        }

        public void losoweKrawedzie(int ilosc)
        {
            krawedzie.Clear();
            Random rand = new Random();

            int ax, by = 0, wx, koniec = 0;

            for (int i = 0; i < ilosc; i++)
            {
                koniec = 0;
                ax = rand.Next(liczbaWezlow);
                wx = rand.Next(20);
                while (koniec == 0)
                {
                    by = rand.Next(liczbaWezlow);
                    if (by != ax)
                    {
                        koniec = 1;
                    }
                }
                krawedzie.Add(new Lacze(i + 1, wezly[ax], wezly[by], wx));
                wezly[ax].listaKrawedzi.Add(krawedzie[i]);
                wezly[by].listaKrawedzi.Add(krawedzie[i]);
            }
            liczbaLaczy = krawedzie.Count;
            wylosujWagiLaczy();
            krawedzie.Sort((x, y) => x.Waga.CompareTo(y.Waga));
            algorytmFloyda();

        }

        public void wagiOdleglosc()
        {
            foreach (Lacze krawedz in krawedzie)
            {
                krawedz.Waga = (float)Math.Sqrt(Math.Pow(krawedz.Wezel1.wspX - krawedz.Wezel2.wspX, 2) + Math.Pow(krawedz.Wezel1.wspY - krawedz.Wezel2.wspY, 2));
            }
        }

        public void krawedzieProjekt()
        {
            int idKraw = 1;
            int wx = 1;

            krawedzie.Clear();
            for (int i = 0; i < liczbaWezlow; i++)
            {
                for (int j = i + 1; j < liczbaWezlow; j++)
                {
                    krawedzie.Add(new Lacze(idKraw, wezly[i], wezly[j], wx)); //Dodajemy nowa krawedz
                    krawedzie[idKraw - 1].Waga = (float)Math.Sqrt(Math.Pow(krawedzie[idKraw - 1].Wezel1.wspX - krawedzie[idKraw - 1].Wezel2.wspX, 2) +
                        Math.Pow(krawedzie[idKraw - 1].Wezel1.wspY - krawedzie[idKraw - 1].Wezel2.wspY, 2)); //Ustawiamy koszt
                                                                                                             //nieposortowane.Add(new Lacze(idKraw, wezly[i], wezly[j])); //Dodajemy nowa krawedz
                                                                                                             // nieposortowane[idKraw - 1].Waga = (float)Math.Sqrt(Math.Pow(krawedzie[idKraw - 1].Wezel1.wspX - krawedzie[idKraw - 1].Wezel2.wspX, 2) +
                                                                                                             // Math.Pow(krawedzie[idKraw - 1].Wezel1.wspY - krawedzie[idKraw - 1].Wezel2.wspY, 2)); //Ustawiamy koszt
                    idKraw++;
                }
            }
            // krawedzie.Sort((x, y) => x.Waga.CompareTo(y.Waga));


        }

        public bool poprawnoscSciezkiDijkstry()
        {
            algorytmFloyda();

            int i = sciezki[0].Wezel1.idWezla;
            int j = sciezki[0].Wezel2.idWezla;
            if (i != j)
            {
                if (tablicaKierowaniaWezlami[i - 1, j - 1] == null && (j != i))
                    return false;
                else
                    return true;

            }
            else

                return true;

        }

        public void dodajWezel(Wezel w)
        {
            this.wezly.Add(w);
            this.liczbaWezlow++;
        }

        public void usunWezel(Wezel w)
        {
            try
            {
                this.wezly.Remove(w);
                this.liczbaWezlow--;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to delete the node.");
            }
        }

        /// <summary>
        /// Funkcja aktualizuje wartosci zmiennych takich, jak 
        /// </summary>
        public void aktualizujLiczniki()
        {
            this.liczbaWezlow = wezly.Count;
            this.liczbaLaczy = krawedzie.Count;
        }
    }

}