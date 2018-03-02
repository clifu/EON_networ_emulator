using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Klasa testujaca funkcjonalnosc z bufora
    /// </summary>
    [TestClass]
    public class BufferTester
    {
        /// <summary>
        /// Testeuje sortowanie kolejki.
        /// </summary>
        [TestMethod]
        public void testSortQueue()
        {
            Buffer b = new Buffer();
            //generacja losowej kolejki
            b.queue = generatePackageQueueFrequencies();

            //sortowanie po czestotliwosci
            b.queue = Buffer.sortQueueByFrequency(ref b.queue);

            //lista z bajtami
            List<byte[]> packageBytes = new List<byte[]>();
            packageBytes.AddRange(b.queue);

            //Adresy IP powinny byc dobrze powpisywane
            Assert.AreEqual(1, Package.extractFrequency(packageBytes[0]));
            Assert.AreEqual(1, Package.extractFrequency(packageBytes[1]));
            Assert.AreEqual(2, Package.extractFrequency(packageBytes[2]));
            Assert.AreEqual(2, Package.extractFrequency(packageBytes[3]));
            Assert.AreEqual(3, Package.extractFrequency(packageBytes[4]));
            Assert.AreEqual(3, Package.extractFrequency(packageBytes[5]));
            Assert.AreEqual(3, Package.extractFrequency(packageBytes[6]));
        }

        /// <summary>
        /// Generuje kolejke z pakiecikami
        /// </summary>
        public Queue<byte[]> generatePackageQueue()
        {
            //A taki sobie napis
            string inscription = "17 kotow ma ala";

            //serie pakietow
            List<List<Package>> Packages = new List<List<Package>>();
            for (int i = 0; i < 3; i++)
            {
                Packages.Add(new List<Package>());
            }

            //Ile bedzie wiadomosci w danej serii
            short[] howManyPackages = {3, 2, 3};

            //ID serii pakietow
            short[] IDs = {171, 172, 173};

            //IP serii pakietow
            string[] IPs = {"127.0.17.1", "127.0.17.2", "127.0.17.3"};

            Package P;

            //Dla kazdej serii pakietow
            for (int j = 0; j < howManyPackages.Length; j++)
            {
                P = new Package(inscription, 0, IPs[j], "0.0.0.0", (short) inscription.Length);
                //Dla kazdego pakietu z serii
                for (int i = 0; i < howManyPackages[j]; i++)
                {
                    P.changeID(IDs[j]);
                    P.changePackageNumber((short) i);
                    Packages[j].Add(P);
                }
            }

            //Kolejeczka z pakietami
            Queue<byte[]> queue = new Queue<byte[]>();

            //Dodanie nie w kolejnosci pakietow do kolejeczki
            queue.Enqueue(Packages[2][2].toBytes());
            queue.Enqueue(Packages[1][0].toBytes());
            queue.Enqueue(Packages[0][1].toBytes());
            queue.Enqueue(Packages[1][1].toBytes());
            queue.Enqueue(Packages[0][2].toBytes());
            queue.Enqueue(Packages[2][0].toBytes());
            queue.Enqueue(Packages[2][1].toBytes());

            return queue;
        }

        /// <summary>
        /// Generuje kolejke z pakiecikami
        /// </summary>
        public Queue<byte[]> generatePackageQueueFrequencies()
        {
            //A taki sobie napis
            string inscription = "17 kotow ma ala";

            //serie pakietow
            List<List<Package>> Packages = new List<List<Package>>();
            for (int i = 0; i < 3; i++)
            {
                Packages.Add(new List<Package>());
            }

            //Ile bedzie wiadomosci w danej serii
            short[] howManyPackages = {3, 2, 3};

            //ID serii pakietow
            short[] IDs = {171, 172, 173};

            //czestotliwosci serii pakietow
            short[] Frequencies = {1, 2, 3};

            Package P;

            //Dla kazdej serii pakietow
            for (int j = 0; j < howManyPackages.Length; j++)
            {
                P = new Package(inscription, 0, "127.17.17.17", "0.0.0.0", (short) inscription.Length, 0,
                    Frequencies[j], 1);

                //Dla kazdego pakietu z serii
                for (int i = 0; i < howManyPackages[j]; i++)
                {
                    P.changeID(IDs[j]);
                    P.changePackageNumber((short) i);
                    Packages[j].Add(P);
                }
            }

            //Kolejeczka z pakietami
            Queue<byte[]> queue = new Queue<byte[]>();

            //Dodanie nie w kolejnosci pakietow do kolejeczki
            queue.Enqueue(Packages[2][2].toBytes());
            queue.Enqueue(Packages[1][0].toBytes());
            queue.Enqueue(Packages[0][1].toBytes());
            queue.Enqueue(Packages[1][1].toBytes());
            queue.Enqueue(Packages[0][2].toBytes());
            queue.Enqueue(Packages[2][0].toBytes());
            queue.Enqueue(Packages[2][1].toBytes());

            return queue;
        }

        /// <summary>
        /// Generuje kolejke z pakiecikami
        /// </summary>
        public Queue<byte[]> generatePackageQueuePackageNumbers()
        {
            //A taki sobie napis
            string inscription = "17 kotow ma ala";

            //serie pakietow
            List<List<Package>> Packages = new List<List<Package>>();
            for (int i = 0; i < 3; i++)
            {
                Packages.Add(new List<Package>());
            }

            //Ile bedzie wiadomosci w danej serii
            short[] howManyPackages = { 3, 2, 3 };

            //ID serii pakietow
            short[] IDs = { 171, 172, 173 };

            //czestotliwosci serii pakietow
            short[] Frequencies = { 1, 2, 3 };

            Package P;

            //Dla kazdej serii pakietow
            for (int j = 0; j < howManyPackages.Length; j++)
            {
                P = new Package(inscription, 0, "127.17.17.17", "0.0.0.0", (short)inscription.Length, 0,
                    Frequencies[j], 1);

                //Dla kazdego pakietu z serii
                for (int i = 0; i < howManyPackages[j]; i++)
                {
                    P.changeID(IDs[j]);
                    P.changePackageNumber((short)i);
                    Packages[j].Add(P);
                }
            }

            //Kolejeczka z pakietami
            Queue<byte[]> queue = new Queue<byte[]>();

            //Dodanie nie w kolejnosci pakietow do kolejeczki
            queue.Enqueue(Packages[2][2].toBytes());
            queue.Enqueue(Packages[1][0].toBytes());
            queue.Enqueue(Packages[0][1].toBytes());
            queue.Enqueue(Packages[1][1].toBytes());
            queue.Enqueue(Packages[0][2].toBytes());
            queue.Enqueue(Packages[2][0].toBytes());
            queue.Enqueue(Packages[2][1].toBytes());

            return queue;
        }

        [TestMethod]
        public void TestDivideQueueByFrequency()
        {
            //stworzenie bufora
            Buffer b = new Buffer();

            //i jego kolejki
            b.queue = generatePackageQueueFrequencies();

            //sortowanie po adresach IP
            b.queue = Buffer.sortQueueByFrequency(ref b.queue);

            //Podzial na grupy z konkretnymi adresami IP
            List<Queue<byte[]>> listOfQueues = Buffer.divideSortedQueueByFrequency(ref b.queue);

            //Wyciagniecie kolejek w postaci tablic bo wygodniej 
            var array0 = listOfQueues[0].ToArray();
            var array1 = listOfQueues[1].ToArray();
            var array2 = listOfQueues[2].ToArray();

            //Sprawdzenie, czy czestotliwosci sie dobrze przydzielily
            Assert.AreEqual(1, Package.extractFrequency(array0[0]));
            Assert.AreEqual(1, Package.extractFrequency(array0[1]));
            Assert.AreEqual(2, Package.extractFrequency(array1[0]));
            Assert.AreEqual(2, Package.extractFrequency(array1[1]));
            Assert.AreEqual(3, Package.extractFrequency(array2[0]));
            Assert.AreEqual(3, Package.extractFrequency(array2[1]));
            Assert.AreEqual(3, Package.extractFrequency(array2[2]));
        }

        /// <summary>
        /// Testuje oproznianie bufora
        /// </summary>
        [TestMethod]
        public void testEmptyBuffer()
        {
            //Bufor o rozmiarze 7 pakietow
            Buffer b = new Buffer(7);

            //Jego kolejeczka (tez ma 7 pakietow)
            b.queue = generatePackageQueueFrequencies();

            //Oproznienie bufora i skopiowanie jego wartosci do listOfQueues
            var listOfQueues = b.emptyBufferIN();

            //Kolejka powinna byc pusta
            Assert.AreEqual(0, b.queue.Count);

            //ale nie powinna byc nullem
            Assert.IsNotNull(b.queue);

            //Wyciagniecie kolejek w postaci tablic bo wygodniej 
            var array0 = listOfQueues[0].ToArray();
            var array1 = listOfQueues[1].ToArray();
            var array2 = listOfQueues[2].ToArray();

            //Sprawdzenie, czy czestotliwosci sie dobrze przydzielily
            Assert.AreEqual(1, Package.extractFrequency(array0[0]));
            Assert.AreEqual(1, Package.extractFrequency(array0[1]));
            Assert.AreEqual(2, Package.extractFrequency(array1[0]));
            Assert.AreEqual(2, Package.extractFrequency(array1[1]));
            Assert.AreEqual(3, Package.extractFrequency(array2[0]));
            Assert.AreEqual(3, Package.extractFrequency(array2[1]));
            Assert.AreEqual(3, Package.extractFrequency(array2[2]));
        }

        /// <summary>
        /// Testuje sortowanie po numerze pakietu, malejaco
        /// </summary>
        [TestMethod]
        public void testSortRecedingByPackageNumber()
        {
            var queue = generatePackageQueue();

            var array = queue.ToArray();

            //czyszczenie kolejki
            queue.Clear();

            Package P;

            for (int i = 0; i < array.Length; i++)
            {
                P = new Package(array[i]);

                //wpisanie innego numerku pakietu
                P.changePackageNumber((short)i);

                //Dodanie do kolejki pakietu z numerkiem
                queue.Enqueue(P.toBytes());
            }

            //lista kolejek skladajaca sie z dwoch tych samych kolejek
            List<Queue<byte[]>> listOfQueues = new List<Queue<byte[]>>() {new Queue<byte[]>(queue), new Queue<byte[]>(queue) };

            var temp = new Queue<byte[]>(Buffer.joinSortedByPackageNumberQueues(listOfQueues));

            Assert.AreEqual(2*7, temp.Count);

            var tempTable = temp.ToArray();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Assert.AreEqual(j, Package.extractPackageNumber(tempTable[j]));
                }
                
                Assert.AreEqual(0, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(1, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(2, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(3, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(4, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(5, Package.extractPackageNumber(temp.Dequeue()));
                Assert.AreEqual(6, Package.extractPackageNumber(temp.Dequeue()));
            }
        }

        /// <summary>
        /// Testuje funkcje sortujace.
        /// </summary>
        [TestMethod]
        public void testSort()
        {
            var array = generatePackageQueue().ToArray();

            Queue<byte[]> queue = new Queue<byte[]>();

            Package P;

            //tabela wartosci pola w naglowku, nieposortowane
            short[] shorts = {5, 3, 6, 1, 2, 4, 0};

            //Tworzenie nieposortowanej tablicy(wedlug nru pakietu)
            for (int i = 0; i < shorts.Length; i++)
            {
                P = new Package(array[i]);
                P.changePackageNumber(shorts[i]);
                array[i] = P.toBytes();
                queue.Enqueue(array[i]);
            }

            //sortowanie
            queue = Buffer.sortQueueByPackageNumber(ref queue);

            //sprawdzanie, czy sie posortowaly
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(i, Package.extractPackageNumber(queue.Dequeue()));
            }

            //ID
            //Tworzenie nieposortowanej tablicy(wedlug ID)
            for (int i = 0; i < shorts.Length; i++)
            {
                P = new Package(array[i]);
                P.changeID(shorts[i]);
                array[i] = P.toBytes();
                queue.Enqueue(array[i]);
            }

            //sortowanie po ID
            queue = Buffer.sortQueueByID(ref queue);

            //srawdzanie, czy sie posortowaly
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(i, Package.extractPackageNumber(queue.Dequeue()));
            }

            //Czestotliwosc
            //Tworzenie nieposortowanej tablicy(wedlug czestotliwosci)
            for (int i = 0; i < shorts.Length; i++)
            {
                P = new Package(array[i]);
                P.changeFrequency(shorts[i]);
                array[i] = P.toBytes();
                queue.Enqueue(array[i]);
            }

            //sortowanie po czestotliwosci
            queue = Buffer.sortQueueByFrequency(ref queue);

            //srawdzanie, czy sie posortowaly
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(i, Package.extractPackageNumber(queue.Dequeue()));
            }

        }
    }
}
