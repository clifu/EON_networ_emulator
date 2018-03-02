using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Klasa reprezentujaca pole komutacyjne.
    /// </summary>
    public class CommutationField
    {
        //Maksymalny rozmiar bufora wejsciowego
        public short maxBuffInSize = 20;

        //Maksymalny rozmiar bufora wyjsciowego
        public short maxBuffOutSize = 20;

        //Bufor wejsciowy
        public volatile Buffer bufferIn;

        //Lista wyjsciowych buforow
        public volatile List<Buffer> BuffersOut;

        //Referencja na tablice komutacji brzegowego wezla sieciowego
        public volatile BorderNodeCommutationTable borderNodeCommutationTable;

        //Referencja na tablice komutacji wezla sieciowego
        public volatile CommutationTable commutationTable;

        //Referencja na tablice eonowa wezla sieciowego
        public volatile EONTable EonTable;


        // public NetworkNode _networkNode;

        /// <summary>
        /// Konstruktor 
        /// TODO: Kazdy router ma na razie 3 bufory wyjsciowe
        /// </summary>
        public CommutationField()
        {
            bufferIn = new Buffer();
            BuffersOut = new List<Buffer>();
            for (int i = 0; i < 3; i++)
            {
                BuffersOut.Add(new Buffer());
            }
            maxBuffInSize = (short)bufferIn.bufferSize;
            maxBuffOutSize = (short)bufferIn.bufferSize;
        }

        /// <summary>
        /// Konstruktor okreslajacy ilosc buforow wyjsciowych
        /// </summary>
        /// <param name="buffersOutCount"></param>
        public CommutationField(int buffersOutCount) : this()
        {
            bufferIn = new Buffer();
            BuffersOut = new List<Buffer>(buffersOutCount);
        }

        /// <summary>
        /// Konstruktor z referencjami na tablice i liczba wyjsciowych buforow
        /// </summary>
        /// <param name="borderNodeCommutationTable"></param>
        /// <param name="commutationTable"></param>
        /// <param name="EonTable"></param>
        public CommutationField(ref BorderNodeCommutationTable borderNodeCommutationTable,
            ref CommutationTable commutationTable, ref EONTable EonTable, int buffersOutCount) : this()
        {
            //bufferIn = new Buffer();
            // BuffersOut = new List<Buffer>(buffersOutCount);
            this.borderNodeCommutationTable = borderNodeCommutationTable;
            this.EonTable = EonTable;
            this.commutationTable = commutationTable;
        }

        /// <summary>
        /// Funkcja probujaca dolaczyc jeden pakiet do kolejki wyjsciowego bufora.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public async Task<Queue<byte[]>> processPackageBufferOut(byte[] packageBytes, Queue<byte[]> queue)
        {
            //Jezeli jest prawie pelen bufor...
            if (queue.Count == maxBuffOutSize - 1)
            {
                //Tworzenie kopii oryginalnej kolejki
                var returnQueue = new Queue<byte[]>(queue);

                //Dodanie pakietu na ostatnie miejsce w kolejce
                returnQueue.Enqueue(packageBytes);

                //Zwracamy kojelke
                return queue;
            }
            //Jak nie jest prawie pelen, to dodajemy po prostu pakiet do kolejki
            else
            {
                queue.Enqueue(packageBytes);
                return null;
            }
        }

        /// <summary>
        /// Funkcja przetwarzajaca pakiety. Nie dziala dobrze!
        /// </summary>
        /// <param name="packageBytes"></param
        public List<Queue<byte[]>> processPackage(byte[] packageBytes)
        {
            //jezeli bufor jest prawie pelny (brakuje mu jeden pakiet)
            if (bufferIn.queue.Count == maxBuffInSize - 1)
            {
                //dodanie do bufora pakietu
                bufferIn.queue.Enqueue(packageBytes);

                //Wyprozniamy bufor wejsciowy i zapisujemy jego stan do tego listOfQUeues
                var listOfQueues = bufferIn.emptyBufferIN();

                //do zwrotu wartosci lista o maksymalnej wielkosci rownej ilosci buforow wyjsciowych
                List<Queue<byte[]>> returnListOfQueues = new List<Queue<byte[]>>();

                for (int k = 0; k < BuffersOut.Count; k++)
                {
                    returnListOfQueues.Add(new Queue<byte[]>());
                }
                //ale listOfQueues powinno miec zawsze 3 kolejki na I etapie...
                for (int i = 0; i < listOfQueues.Count; i++)
                {
                    //Jak jeden pakiet ma ustawiona czestotliwosc na -1, to jest szansa, ze wszystkie maja i 
                    //Jezeli wszystkie pakiety posiadaja -1 w czestotliwosci
                    if (Package.extractFrequency(listOfQueues[i].Peek()) == -1 &&
                        Buffer.checkIfAllPackagesFrequency(listOfQueues[i], -1))
                    {
                        //Sprawdzanie w tablicach komutacji routera i wpisywanie do naglowkow odpowiednich wpisow
                        List<Queue<byte[]>> copyList = new List<Queue<byte[]>>();
                        copyList.Add(listOfQueues[i]);

                        listOfQueues[i] = new Queue<byte[]>(borderNodeCommutationTable.changeHeaderForMessagesFromClient(
                                    (copyList[i])));

                        //Nowa, zmieniona czestotliwosc. 
                        //TODO: Na razie jest jedna, a na drugi etap trzeba bedzie przygotowac sortowanie, dzielenie i 
                        //TODO: dodawanie do konkretnych kolejek

                        //Podpatrujemy na jakas czestotliwosc z pakietu
                        short freq = Package.extractFrequency(listOfQueues[i].Peek());

                        //sprawdzamy, czy wszystkie czestotliwosci sa takie same
                        if (Package.extractFrequency(listOfQueues[i].Peek()) == freq &&
                            Buffer.checkIfAllPackagesFrequency(listOfQueues[i], freq))
                        {
                            //szukanie takiej kolejki, ze wierzchni wpis ma interesujaca nas czestotliwosc
                            var queue = listOfQueues.Find(q => Package.extractFrequency(q.Peek()) == freq);

                            //TODO: Jak sprawdzic, na ktory bufor wyjsciowy skierowac te pakiety co wczesniej mialy -1?

                            // BuffersOut.Add(new Buffer(listOfQueues[i].Count));

                            //A gdy nie udalo sie znalezc, to dodajemy elementy kolejki z wczesniejszymi "-1" do kolejki z freq
                            if (queue == null)
                            {
                                int iterator = 0;
                                iterator = listOfQueues[i].Count();
                                while (iterator > 0)
                                {
                                    //TODO: asynchronicznie dodawaj po jednym pakiecie do bufora wyjsciowego 
                                    //Zdejmowanie z jednej kolejki i dopisywanie do drugiej. Nie trzeba znowu sortowac - pakiety z 
                                    //"-1" maja prawdopodobnie inne pochodzenie
                                    queue.Enqueue(listOfQueues[i].Dequeue());
                                    iterator--;
                                }
                            }
                        }
                        else
                        {
                            //Rzucamy wyjatek gdy czestotliwosci nie sa takie same
                            throw new Exception(
                                "CommutationField.processPackage(): Zamienione czestotliwosci z -1 na inna nie sa takie same!");
                        }
                    }
                    else
                    {
                        //Sprawdzanie w tablicach komutacji routera i wpisywanie do naglowkow odpowiednich wpisow
                        List<Queue<byte[]>> copyList = new List<Queue<byte[]>>();
                        copyList.Add(listOfQueues[i]);

                        listOfQueues[i] = new Queue<byte[]>(commutationTable.changeHeaderForMessagesFromClient(
                                    (copyList[i])));

                        //Rzad tablicy komutacji, ktory odpowiada czestotliwosci i numerowi portu 
                        //var row = commutationTable.FindRow(Package.extractFrequency(joinedQueue.Peek()),
                        // Package.extractPortNumber(joinedQueue.Peek()));

                        //short frequency_out = row.frequency_out;
                        //short port_out = row.port_out;
                    }
                    //jak nie ma -1 w czestoltiwosci, to wchodzimy jeszcze raz w petle
                    //      else
                    //      {
                    //Nowa, pusta kolejka
                    Queue<byte[]> tempBuffOutQueue;

                    //Czy juz bylo kopiowane do tempa?
                    bool hasBeenCopied = false;

                    //Dla kazdego pakietu w kolejce bedzie usuwana z listOfQueues[i] i dodawana do bufora.
                    while (listOfQueues[i].Count > 0)
                    {

                        //Jezeli bufor jest przepelniony, to skopiuje do tempa cala zawartosc bufora i bufor sie
                        //wyprozni. Jezeli nie jest przepelniony, to funkcja addPackage zwroci null i wtedy
                        //temp po prostu bedzie pusty.
                        tempBuffOutQueue =
                            BuffersOut[i].addPackage(listOfQueues[i].Dequeue());

                        if (tempBuffOutQueue == null)
                            continue;
                        else
                        {
                            //Jak jeszcze nie kopiowalismy z tempa do tablicy returnListOfQueues, to kopiujemy
                            if (!hasBeenCopied)
                            {
                                //Kopiowanie do listy wyjsciowej zawartosci tempa (przez kopie!)
                                returnListOfQueues[i] = new Queue<byte[]>(tempBuffOutQueue);

                                //Petla dziala dalej, a bufor wyjsciowy pola komutacyjnego sie zapelnia.

                                hasBeenCopied = true;
                            }
                            //A jak juz kopiowalismy, dodajemy znowu zawartosc tempa na koniec listy
                            else
                            {
                                returnListOfQueues.Add(new Queue<byte[]>(tempBuffOutQueue));

                                //W sumie niepotrzebne
                                hasBeenCopied = false;
                            }
                        }
                    }



                    //Kopiowanie z podkolejek do kolejnych buforow wyjsciowych
                    //BuffersOut[i].queue = new Queue<byte[]>(listOfQueues[i]);

                    var tmp = new Queue<byte[]>(returnListOfQueues[i]);

                    //sortowanie kolejek wedlug ID
                    returnListOfQueues[i] = Buffer.sortQueueByID(ref tmp);

                    tmp = new Queue<byte[]>(returnListOfQueues[i]);

                    //Lista podkolejek o pakietach o tym samym ID
                    var listOfSameIDQueues = Buffer.divideSortedQueueByID(ref tmp);

                    //Kazda z tych podkolejek sortuje wedlug numeru pakietu
                    for (int j = 0; j < listOfSameIDQueues.Count; j++)
                    {
                        var temp = listOfSameIDQueues[j];
                        //Sortowanie po numerze pakietu
                        listOfSameIDQueues[j] = new Queue<byte[]>(Buffer.sortQueueByPackageNumber(ref temp));
                    }

                    //Sklejanie kolejek z powrotem
                    var joinedQueue = new Queue<byte[]>(Buffer.joinSortedByPackageNumberQueues(listOfSameIDQueues));

                    //TODO: Zmien to. Trzeba zajrzec w tablice komutacji i skierowac na odpowiedni bufor.
                    //BuffersOut[i].queue = joinedQueue;

                    //Rzad tablicy komutacji, ktory odpowiada czestotliwosci i numerowi portu 
                    /* var row = commutationTable.FindRow(Package.extractFrequency(joinedQueue.Peek()),
                         Package.extractPortNumber(joinedQueue.Peek()));*/

                    //short frequency_out = row.frequency_out;
                    //short port_out = row.port_out;


                    //Jesli pakiety maja -1 w czestotliwosciach
                    // if (Package.extractFrequency(joinedQueue.Peek()) == 1)
                    //  {
                    //TODO: To jest wpisane z palca na razie, ale potem trzeba bedzie zrobic tak:
                    //joinedQueue = new Queue<byte[]>( borderNodeCommutationTable.changeHeaderForMessagesFromClient(joinedQueue));
                    //   Package P = new Package(joinedQueue.Dequeue());
                    //  P.changeFrequency(-1);
                    //  P.changePort(3);
                    // joinedQueue.Enqueue(P.toBytes());
                    //  }

                    //Wpisanie naglowkow do kolejki z pakietami czestotliwosci i portu wyjsciowego. 
                    //Chmurka juz bedzie wiedziala, jak pokierowac pakiety z tej kolejki.
                    //joinedQueue = commutationTable.changeHeaderForMessagesFromClient(joinedQueue);

                    //Kopiowanie kolejki do listy zwracanej.
                    returnListOfQueues[i] = new Queue<byte[]>(joinedQueue);
                    //  }
                }

                //TIMER
                /*
                //Task, który służy wprowadzeniu opóźnienia między kolejnymi wysłanymi pakietami
                var delay = Task.Run(async () =>
               {
                   Stopwatch sw = Stopwatch.StartNew();
                   await Task.Delay(300);
                   sw.Stop();
                   return sw.ElapsedMilliseconds;
               });
               */
                //Po odczekaniu tego czasu wyprozniamy bufory.
                for (int i = 0; i < BuffersOut.Count; i++)
                {
                    //Kopiowanie zawartosci bufora do zwracanej listy (tworzenie nowego elementu tej listy)
                    returnListOfQueues.Add(new Queue<byte[]>(BuffersOut[i].queue));

                    //czyszczenie bufora
                    BuffersOut[i].queue.Clear();
                }

                return returnListOfQueues;
            }
            //Jak bufor wejsciowy nie jest przepelniony
            else
            {
                byte[] msg = new byte[64];
                msg = packageBytes;
                Package p = new Package(msg);


                BorderNodeCommutationTableRow borderRow = null;
                borderRow = borderNodeCommutationTable.Table.Find(rowe => (rowe.IP_IN.ToString() == p.IP_Source.ToString() && (rowe.port_in == p.portNumber) && (rowe.IP_Destination.ToString() == p.IP_Destination.ToString())));
                //dodanie do bufora pakietu
                if (borderRow != null)
                {
                    bufferIn.queue.Enqueue(packageBytes);
                }
                return null;
            }
        }
    }
}
