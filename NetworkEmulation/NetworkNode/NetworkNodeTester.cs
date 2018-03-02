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
    [TestClass]
    public class NetworkNodeTester
    {
        [TestMethod]
        public void testFindCloudSocketAndPort1()
        {
            //Jakies dane do pakietu
            string inscription = "Mam malo wody";
            short portNumber = 2;
            IPAddress IP_Source = IPAddress.Parse("192.168.0.1");
            IPAddress IP_Destination = IPAddress.Parse("192.168.0.10");
            short packageNumber = 5;
            short frequency = 2;
            short band = 4;
            short usableInfoLength = (short)inscription.Length;

            //adres socketa z chmury
            short cloudSocketPort = 1;
            IPAddress cloudSocketIPAddress = IPAddress.Parse("127.0.10.12");

            //stworzenie nowego pakietu
            Package P = new Package(inscription, portNumber, IP_Destination.ToString(), IP_Source.ToString(),
                usableInfoLength, packageNumber, frequency, band);

            //Nowy wpis do tablicy komutacji pakietow, zawierajacy czestotliwosc z powyzszego pakietu
            CommutationTableRow row = new CommutationTableRow(frequency, cloudSocketPort, frequency, portNumber);

            Assert.IsNotNull(row);

            NetworkNode node = new NetworkNode();

            Assert.IsNotNull(node.borderNodeCommutationTable);
            Assert.IsNotNull(node.commutationTable);
            Assert.IsNotNull(node.eonTable);

            //dodanie do tablicy komutacji pakietow wezla sieciowego nowego rzedu
            node.commutationTable.Table.Add(row);

            //wyodrebnienie adresu IP socketa chmury i portu tego socketa
            var tuple = node.determineFrequencyAndPort(P.toBytes());
           
            //sprawdzenie, czy sie zgadzaja
            Assert.AreEqual(tuple.Item1, frequency);
            Assert.AreEqual(tuple.Item2, portNumber);
        }

        [TestMethod]
        public void testCheckAvailability()
        {
            //Jakies dane do pakietu
            string inscription = "Mam malo wody";
            short portNumber = 2;
            IPAddress IP_Source = IPAddress.Parse("192.168.0.1");
            IPAddress IP_Destination = IPAddress.Parse("192.168.0.10");
            short packageNumber = 5;
            short frequency = 2;
            short band = 4;
            short usableInfoLength = (short)inscription.Length;

            //stworzenie nowego pakietu
            Package P = new Package(inscription, portNumber, IP_Destination.ToString(), IP_Source.ToString(),
                usableInfoLength, packageNumber, frequency, band);

            //nowy rzad wejsciowy tablicy 
            EONTableRowIN rowIn = new EONTableRowIN(frequency, band);

            NetworkNode node = new NetworkNode();

            //Wpisanie w tablice EONową węzła 
            node.eonTable.addRow(rowIn);

            //Wartosci powinny byc te same
            Assert.AreEqual(node.eonTable.TableIN[0].busyFrequency, frequency);
            Assert.AreEqual(node.eonTable.TableIN[0].busyBandIN, band);

            //Powinno byc zajete
            Assert.IsFalse(node.eonTable.CheckAvailability(frequency, band, "in"));

            //Powinno byc zajete
            Assert.IsFalse(node.eonTable.CheckAvailability(rowIn));

            //Powinno byc wolne
            Assert.IsTrue(node.eonTable.CheckAvailability(frequency, band, "out"));

            //Powinno sie zazebic troche i zgrzytnac
            Assert.IsFalse(node.eonTable.CheckAvailability(1, 3, "in"));

            //Nie powinny sie pokrywac
            Assert.IsTrue(node.eonTable.CheckAvailability(0, 2, "in"));

            //====================================================================================
            //nowy rzad wyjsciowy tablicy 
            EONTableRowOut rowOut = new EONTableRowOut(frequency, band);

            //dodanie rzedu do tablicy
            node.eonTable.addRow(rowOut);

            //Wartosci powinny byc te same
            Assert.AreEqual(node.eonTable.TableOut[0].busyFrequency, rowOut.busyFrequency);
            Assert.AreEqual(node.eonTable.TableOut[0].busyBandOUT, rowOut.busyBandOUT);

            //powinno byc zajete
            Assert.IsFalse(node.eonTable.CheckAvailability(rowOut));

            //Powinno byc zajete
            Assert.IsFalse(node.eonTable.CheckAvailability(frequency, band, "out"));

            //Powinno sie zazebic troche i zgrzytnac
            Assert.IsFalse(node.eonTable.CheckAvailability(1, 3, "out"));

            //Nie powinny sie pokrywac
            Assert.IsTrue(node.eonTable.CheckAvailability(0, 2, "out"));
        }

        /// <summary>
        /// Testuje dodawanie rzedow do tablicy EONa
        /// </summary>
        [TestMethod]
        public void testAddEONRow()
        {
            short frequency = 2;
            short band = 3;

            //stworzenie nowej tabeli
            EONTable table = new EONTable();

            //stworzenie nowych rzedow
            EONTableRowIN rowIn = new EONTableRowIN(frequency, band);
            EONTableRowOut rowOut = new EONTableRowOut(frequency, band);

            //Dodanie rzedow
            table.addRow(rowIn);
            table.addRow(rowOut);

            //Sprawdzenie, czy wybrane czestotliwosci sa zajete
            for (int i = frequency; i < frequency+band; i++)
            {
                Assert.IsTrue(table.InFrequencies[i] == frequency);
                Assert.IsTrue(table.OutFrequencies[i] == frequency);
            }

            //Sprawdzenie wolnych czestotliwosci
            for (int i = 0; i < frequency; i++)
            {
                Assert.IsTrue(table.InFrequencies[i] == -1);
                Assert.IsTrue(table.OutFrequencies[i] == -1);
            }

            for (int i = frequency+band; i < EONTable.capacity ; i++)
            {
                Assert.IsTrue(table.InFrequencies[i] == -1);
                Assert.IsTrue(table.OutFrequencies[i] == -1);
            }
        }

        /// <summary>
        /// Sprawdza usuwanie wiersza z tablicy EONowej
        /// </summary>
        [TestMethod]
        public void testDeleteEONRow()
        {
            short frequency = 2;
            short band = 3;

            //stworzenie nowej tabeli
            EONTable table = new EONTable();

            //stworzenie nowych rzedow
            EONTableRowIN rowIn = new EONTableRowIN(frequency, band);
            EONTableRowOut rowOut = new EONTableRowOut(frequency, band);

            //Dodanie rzedow
            table.addRow(rowIn);
            table.addRow(rowOut);

            //usuniecie rzedow
            table.deleteRow(rowIn);
            table.deleteRow(rowOut);

            Assert.IsTrue(table.CheckAvailability(rowIn));
            Assert.IsTrue(table.CheckAvailability(rowOut));

            //Kazdy wpis w tabeli po wyczyszczeniu powinien byc rowny -1
            foreach (short f in table.InFrequencies)
            {
                Assert.AreEqual(-1, f);
            }

            //Nie powinno byc rzedu w tablicy
            Assert.IsFalse(table.TableIN.Contains(rowIn));

            //Kazdy wpis w tabeli po wyczyszczeniu powinien byc rowny -1
            foreach (short f in table.OutFrequencies)
            {
                Assert.AreEqual(-1, f);
            }

            //Nie powinno byc rzedu w tablicy
            Assert.IsFalse(table.TableOut.Contains(rowOut));
        }

        [TestMethod]
        public void testAddCommutationTableRow()
        {
            short frequency_in = 2;
            short port_in = 3;
            short frequency_out = 4;
            short port_out = 5;

            //stworzenie nowego rzedu
            CommutationTableRow row = new CommutationTableRow(frequency_in, port_in, frequency_out, port_out);

            //stworzenie nowej tablicy komutacji
            CommutationTable table = new CommutationTable();

            //dodanie nowegor rzedu do tablicy komutacji
            table.Table.Add(row);
            
            //Powinny byc takie same
            Assert.AreEqual(table.Table[0].frequency_out, frequency_out);
            Assert.AreEqual(table.Table[0].port_out, port_out);
            Assert.AreEqual(table.Table[0].frequency_in, frequency_in);
            Assert.AreEqual(table.Table[0].port_in, port_in);
        }

        [TestMethod]
        public void testAddBorderNodeCommutationTableRow()
        {
            IPAddress IP_IN = IPAddress.Parse("123.123.123.123");
            short port_in = 1;
            short band = 2;
            short frequency = 3;
            short modulationPerformance = 4;
            short bitRate = 5;
            IPAddress IPSocketOut = IPAddress.Parse("234.234.234.234");
            short socketPort = 6;
            short hopsNumber = 7;

            //stworzenie nowego rzedu tablicy komutacji routera brzegowego
            BorderNodeCommutationTableRow row = new BorderNodeCommutationTableRow(IP_IN.ToString(), port_in, band, frequency, 
                modulationPerformance, bitRate, IPSocketOut.ToString(), socketPort, hopsNumber);

            //nowa tabela komutacji wezla brzegowego
            BorderNodeCommutationTable table = new BorderNodeCommutationTable();

            //Dodanie wiersza do tabeli komutacji wezla brzegowego
            table.Table.Add(row);

            //Powinny byc takie same
            Assert.AreEqual(row, table.Table[0]);
        }

        [TestMethod]
        public void testDetermineModulationPerformance()
        {
            for (int i = 1; i < 9; i++)
            {
                //i - ilosc hopow
                //9-i - wartosc wydajnosci modulacji
                Assert.AreEqual(9-i, BorderNodeCommutationTable.determineModulationPerformance((short)i));
            }

            //W przeciwnych przypadkach powinno zwrocic 1
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(9));
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(19));
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(119));
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(0));
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(-1));
            Assert.AreEqual(1, BorderNodeCommutationTable.determineModulationPerformance(-100));
        }

        [TestMethod]
        public void testFindRow1()
        {
            string IP = "127.0.0.1";
            short port = 2;

            //Nowa tabela
            BorderNodeCommutationTable table = new BorderNodeCommutationTable();

            //Nowy rząd
            BorderNodeCommutationTableRow row = new BorderNodeCommutationTableRow(IP, port, 0, 0 , 0 , 0 , "0.0.0.0", 0, 0);
            
            //Dodanie rzedu do tabeli
            table.Table.Add(row);

        //    Assert.AreEqual(row, table.FindRow(IP, port));

           // Assert.AreEqual(0, table.FindIndex(IP, port));
        }

        [TestMethod]
        public void testGenerateCharacteristicInfo()
        {
            IPAddress IP_IN = IPAddress.Parse("123.123.123.123");
            short port_in = 1;
            short band = 2;
            short frequency = 3;
            short modulationPerformance = 4;
            short bitRate = 5;
            IPAddress IPSocketOut = IPAddress.Parse("234.234.234.234");
            short socketPort = 6;
            short hopsNumber = 7;

            //stworzenie nowego rzedu tablicy komutacji routera brzegowego
            BorderNodeCommutationTableRow row = new BorderNodeCommutationTableRow(IP_IN.ToString(), port_in, band, frequency,
                modulationPerformance, bitRate, IPSocketOut.ToString(), socketPort, hopsNumber);

            //pozostale dane do pakietu, korzysta tez z danych od pakietu
            string inscription = "Mam malo wody";
            short portNumber = port_in;
            IPAddress IP_Source = IP_IN;
            IPAddress IP_Destination = IPSocketOut;
            short packageNumber = 5;
            short usableInfoLength = (short)inscription.Length;
            short ID = 17;
            short howManyPackages = 1;

            //Stworzenie pakietu
            Package P = new Package(inscription, portNumber, IP_Destination.ToString(), IP_Source.ToString(), usableInfoLength, packageNumber, 0, 0, 0, 0, ID, howManyPackages);

            BorderNodeCommutationTable table = new BorderNodeCommutationTable();

            table.Table.Add(row);

            Package P2 = new Package(RouterClientPortIn.GenerateCharacteristicInfo(P.toBytes(), ref table));

            Assert.AreEqual(row.frequency, P2.frequency);
            Assert.AreEqual(row.band, P2.band);
            Assert.AreEqual(row.bitRate, P2.bitRate);
            Assert.AreEqual(row.modulationPerformance, P2.modulationPerformance);
        }
    }
}
