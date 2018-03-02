using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Klasa reprezentująca bufor
    /// </summary>
    public class Buffer
    {
        //Kolejka z pakietami
        public Queue<byte[]> queue;

        //Ilosc pakietow w buforze
        public int bufferSize;

        //po jakim czasie oproznic bufor
        public double timeToEmpty = 500; //[ms]

        public Buffer()
        {
            bufferSize = 20;

            //Wielkosc pakietu
            int packageSize = Package.headerMaxLength + Package.usableInfoMaxLength;

            //Tworzenie instancji nowej kolejki
            queue = new Queue<byte[]>(bufferSize * packageSize);
        }

        /// <summary>
        /// Konstruktor ze specyfikacja wielkości bufora.
        /// </summary>
        /// <param name="bufferSize"></param>
        public Buffer(int bufferSize) : this()
        {
            this.bufferSize = bufferSize;
        }

        /// <summary>
        /// Dodaje do bufora nowy pakiet i wyproznia go w zaleznosci od tego, czy byl to ostatni pakiet w buforze, czy 
        /// nie
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public Queue<byte[]> addPackage(byte[] packageBytes)
        {
            //Jezeli jest prawie pelen bufor...
            if (queue.Count == bufferSize - 1)
            {
                //Tworzenie kopii oryginalnej kolejki
                var returnQueue = new Queue<byte[]>(queue);

                //Dodanie pakietu na ostatnie miejsce w kolejce
                returnQueue.Enqueue(packageBytes);

                //Czyscimy kojelke
                queue.Clear();

                //Zwracamy kopie tamtej kolejki
                return returnQueue;
            }
            //Jak nie jest prawie pelen, to dodajemy po prostu pakiet do kolejki
            else
            {
                queue.Enqueue(packageBytes);
                return null;
            }
        }

        /// <summary>
        /// Funkcja, ktora sortuje dane w kolejce, posortowane czesci dodaje do 
        /// osobnych kolejek i wrzuca te kolejki do listy kolejek, odpowiadajacych konkretnemu socketowi.
        /// </summary>
        /// <returns>
        /// Liste kolejek pakietow
        /// TODO: A co jak podczas sortowania dojdzie nam nowy pakiet do bufora?
        /// </returns>
        public List<Queue<byte[]>> emptyBufferIN()
        {
            //przypisanie do tymczasowej zmiennej posortowanego 
            Queue<byte[]> tempQueue = new Queue<byte[]>(sortQueueByFrequency(ref this.queue));

            //Czyszczenie bufora
            this.queue.Clear();

            //Uruchamia dzielenie kolejki wedlug czestotliwosci na liste podkolejek
            return divideSortedQueueByFrequency(ref tempQueue);
        }

        /// <summary>
        /// Dzieli kolejke na liste kolejek wedlug czestotliwosci w celu przekierowania na konkretne bufory wyjsciowe.
        /// Kolejka musi byc posortowana!
        /// </summary>
        /// <param name="_queue"></param>
        /// <returns></returns>
        public static List<Queue<byte[]>> divideSortedQueueByFrequency(ref Queue<byte[]> _queue)
        {
            try
            {
                //Jak kolejka jest nullem albo jest pusta
                if (_queue == null | _queue.Count == 0)
                    return null;

                //Na listach wygodniej
                List<byte[]> list = new List<byte[]>();

                //Dodanie do listy calej kolejki. 
                list.AddRange(_queue);

                //Lista pogrupowanych kolejek
                List<Queue<byte[]>> listOfQueues = new List<Queue<byte[]>>();

                byte[] tmp = list[0];
                //Dodanie pierwszej kolejki do listy kolejek
                listOfQueues.Add(new Queue<byte[]>());

                //Indeks kolejki w liscie kolejek
                int index = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    //Jezeli kolejny element ma takia sama czestotliwosc, jak poprzedni
                    if (Package.extractFrequency(list[i]) == Package.extractFrequency(tmp))
                    {
                        //to dodajemy go do kolejki
                        listOfQueues[index].Enqueue(list[i]);

                        //I ustawiamy tmp na obecny element
                        tmp = list[i];
                    }
                    //Jezeli kojelny element nie ma takiej samej czestotliwosci, jak poprzedni
                    else
                    {
                        //To tworzymy nowa kolejke
                        listOfQueues.Add(new Queue<byte[]>());

                        //zwiekszamy aktualny indeks o 1
                        index++;

                        //i dodajemy nowy element do nowej kolejki
                        listOfQueues[index].Enqueue(list[i]);

                        //ustawiamy tmp na obecny element
                        tmp = list[i];
                    }
                }

                return listOfQueues;
            }
            catch (Exception E)
            {
                Console.WriteLine("Buffer.divideSortedQueueByIP: " + E.Message);
                return null;
            }
        }

        /// <summary>
        /// Dzieli kolejke na liste kolejek wedlug czestotliwosci w celu przekierowania na konkretne bufory wyjsciowe.
        /// Kolejka musi byc posortowana!
        /// </summary>
        /// <param name="_queue"></param>
        /// <returns></returns>
        public static List<Queue<byte[]>> divideSortedQueueByID(ref Queue<byte[]> _queue)
        {
            try
            {
                //Jak kolejka jest nullem albo jest pusta
                if (_queue == null | _queue.Count == 0)
                    return null;

                //Na listach wygodniej
                List<byte[]> list = new List<byte[]>();

                //Dodanie do listy calej kolejki. 
                list.AddRange(_queue);

                //Lista pogrupowanych kolejek
                List<Queue<byte[]>> listOfQueues = new List<Queue<byte[]>>();

                byte[] tmp = list[0];
                //Dodanie pierwszej kolejki do listy kolejek
                listOfQueues.Add(new Queue<byte[]>());

                //Indeks kolejki w liscie kolejek
                int index = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    //Jezeli kolejny element ma takia sama czestotliwosc, jak poprzedni
                    if (Package.extractID(list[i]) == Package.extractID(tmp))
                    {
                        //to dodajemy go do kolejki
                        listOfQueues[index].Enqueue(list[i]);

                        //I ustawiamy tmp na obecny element
                        tmp = list[i];
                    }
                    //Jezeli kojelny element nie ma takiej samej czestotliwosci, jak poprzedni
                    else
                    {
                        //To tworzymy nowa kolejke
                        listOfQueues.Add(new Queue<byte[]>());

                        //zwiekszamy aktualny indeks o 1
                        index++;

                        //i dodajemy nowy element do nowej kolejki
                        listOfQueues[index].Enqueue(list[i]);

                        //ustawiamy tmp na obecny element
                        tmp = list[i];
                    }
                }

                return listOfQueues;
            }
            catch (Exception E)
            {
                Console.WriteLine("Buffer.divideSortedQueueByIP: " + E.Message);
                return null;
            }
        }

        /// <summary>
        /// Sortuje kolejke wedlug portu.
        /// </summary>
        /// <param name="_queue"></param>
        /// <returns></returns>
        public static Queue<byte[]> sortQueueByPackageNumber(ref Queue<byte[]> _queue)
        {
            //Tworzenie listy
            List<byte[]> list = new List<byte[]>();

            //Dodanie calej kolejki do listy
            list.AddRange(_queue);

            //Sortowanie Listy wedlug numeru portu, ale malejaco
            list.Sort((x, y) => Package.extractPackageNumber(x).CompareTo(Package.extractPackageNumber(y)));

            //nowa kolejka
            var sortedQueue = new Queue<byte[]>();

            //Dodawanie kolejnych elementow listy do kolejki
            for (int i = 0; i < list.Count; i++)
            {
                sortedQueue.Enqueue(list[i]);
            }

            //Czyszczenie oryginalnej kolejki
            _queue = sortedQueue;

            //Zwraca posortowana kolejke
            return sortedQueue;
        }

        /// <summary>
        /// Sortuje kolejke wedlug portu.
        /// </summary>
        /// <param name="_queue"></param>
        /// <returns></returns>
        public static Queue<byte[]> sortQueueByID(ref Queue<byte[]> _queue)
        {
            //Tworzenie listy
            List<byte[]> list = new List<byte[]>();

            //Dodanie calej kolejki do listy
            list.AddRange(_queue);

            //Sortowanie Listy wedlug ID pakietu
            list.Sort((x, y) => Package.extractID(x).CompareTo(Package.extractID(y)));

            //nowa kolejka
            var sortedQueue = new Queue<byte[]>();

            //Dodawanie kolejnych elementow listy do kolejki
            for (int i = 0; i < list.Count; i++)
            {
                sortedQueue.Enqueue(list[i]);
            }

            //Czyszczenie oryginalnej kolejki
            //_queue = sortedQueue;

            //Zwraca posortowana kolejke
            return sortedQueue;
        }

        /// <summary>
        /// Sortuje kolejke wedlug czestotliwosci.
        /// </summary>
        /// <param name="_queue"></param>
        /// <returns></returns>
        public static Queue<byte[]> sortQueueByFrequency(ref Queue<byte[]> _queue)
        {
            //Tworzenie listy
            List<byte[]> list = new List<byte[]>();

            //Dodanie calej kolejki do listy
            list.AddRange(_queue);

            //Sortowanie Listy wedlug czestotliwosci
            list.Sort((x, y) => Package.extractFrequency(x).CompareTo(Package.extractFrequency(y)));

            //Sortowanie listy wedlug adresow IP celu
            /*list.Sort((x, y) =>
                BitConverter.ToInt32(Package.exctractDestinationIP(x).GetAddressBytes(), 0)
                    .CompareTo(BitConverter.ToInt32(Package.exctractDestinationIP(y).GetAddressBytes(), 0))); */

            //nowa kolejka
            var sortedQueue = new Queue<byte[]>();

            //Dodawanie kolejnych elementow listy do kolejki
            for (int i = 0; i < list.Count; i++)
            {
                sortedQueue.Enqueue(list[i]);
            }

            //Czyszczenie oryginalnej kolejki
            _queue = sortedQueue;

            //Zwraca posortowana kolejke
            return sortedQueue;
        }

        /// <summary>
        /// Sprawdza, czy czestotliwosc wszystkich pakietow w kolejce jest jednakowa
        /// </summary>
        /// <returns></returns>
        public static bool checkIfAllPackagesFrequency(Queue<byte[]> _queue, short frequency)
        {
            //Konwertowanie kolejki do tablicy
            var table = _queue.ToArray();

            foreach (byte[] bytes in table)
            {
                //Jak tylko znajdzie jeden niepasujacy pakiet to zwraca falsz
                if (Package.extractFrequency(bytes) != frequency)
                    return false;
            }
            //Jak sie nie udalo nic ustawic na falsz no to trudno, zwroc prawde...
            return true;
        }

        /// <summary>
        /// Funkcja sklejajaca posortowane (malejaco) wg nr pakietu pakiety
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Queue<byte[]> joinSortedByPackageNumberQueues(List<Queue<byte[]>> list)
        {
            Queue<byte[]> queue = new Queue<byte[]>();

            for (int i = 0; i < list.Count; i++)
            {
                while (list[i].Count > 0)
                {
                    //Wstawienie konca posortowanej kolejki do kolejki (ale tam jest malejaca kolejnosc)
                    queue.Enqueue(list[i].Dequeue());
                }
            }

            return queue;
        }
    }
}
