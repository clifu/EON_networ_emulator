using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
//Project->add reference->assemblies->extensions->Mircosoft.VisualStudio.QualityTools.UnitTestFramework, version 10.1.0.0
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa do testowania NetworkingTools
    /// </summary>
    [TestClass]
    public class NetworkingToolsTester
    {

        /// <summary>
        /// Testuje konstruktor Package()
        /// </summary>
        [TestMethod]
        public void packageConstructorTest1()
        {
            Package P = new Package();

            //Czy adresy IP maja po 4 Bajty?
            Assert.AreEqual(P.IP_Destination.GetAddressBytes().Length, 4);
            Assert.AreEqual(P.IP_Source.GetAddressBytes().Length, 4);

            //Czy adresy IP sa dobrze wpisane?
            Assert.AreEqual(P.IP_Destination.ToString(), "127.0.0.1");
            Assert.AreEqual(P.IP_Source.ToString(), "127.0.0.1");
        }

        /// <summary>
        /// Testuje konstruktor Package(string usableMessage)
        /// </summary>
        [TestMethod]
        public void packageStringConstructorTest1()
        {
            //jakis napis
            string inscription = "Ala wcale nie ma zadnego kora!";

            //ten napis bedzie wiadomoscia uzytkowa pakietu
            Package P = new Package(inscription);

            //Czy te napisy sie zgadzaja?
            Assert.AreEqual(inscription, P.usableMessage);

            //Czy adresy IP maja po 4 Bajty?
            Assert.AreEqual(P.IP_Destination.GetAddressBytes().Length, 4);
            Assert.AreEqual(P.IP_Source.GetAddressBytes().Length, 4);

            //czy port jest dobrze wpisany?
            Assert.AreEqual(P.portNumber, 1);

            //Czy adresy IP sa dobrze wpisane?
            Assert.AreEqual(P.IP_Destination.ToString(), "127.0.0.1");
            Assert.AreEqual(P.IP_Source.ToString(), "127.0.0.1");

            IPAddress IP_Source = IPAddress.Parse("127.0.0.1");
            IPAddress IP_Destination = IPAddress.Parse("127.0.0.1");

            byte[] bytes = P.toBytes();

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractDestinationIP(bytes), IP_Destination);

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractSourceIP(bytes), IP_Source);

            //czy port wynikajacy z bajtow to domyslnie ustawiony port (1)? 
            Assert.AreEqual(Package.extractPortNumber(bytes), 1);

            //czy napis ma taka sama dlugosc, co dlugosc pola z info uzytkowym?
            Assert.AreEqual(inscription.Length, P.usableInfoLength);
        }

        /// <summary>
        /// Testuje konstruktor Package(string usableMessage, short port, string IP_Source, string IP_Destination)
        /// </summary>
        [TestMethod]
        public void package4ParamsConstructorTest()
        {
            //jakies wartosci moje
            string inscription = "Lubie placki";
            short portNumber = 3;
            IPAddress IP_Source = IPAddress.Parse("127.10.10.10");
            IPAddress IP_Destination = IPAddress.Parse("127.10.20.30");

            Package P = new Package(inscription, portNumber,
                IP_Destination.ToString(), IP_Source.ToString(), (short)inscription.Length);

            //Czy te napisy sie zgadzaja?
            Assert.AreEqual(inscription, P.usableMessage);

            //Czy adresy IP maja po 4 Bajty?
            Assert.AreEqual(P.IP_Destination.GetAddressBytes().Length, 4);
            Assert.AreEqual(P.IP_Source.GetAddressBytes().Length, 4);

            //czy port jest dobrze wpisany?
            Assert.AreEqual(P.portNumber, portNumber);

            //Czy adresy IP sa dobrze wpisane?
            Assert.AreEqual(P.IP_Destination, IP_Destination);
            Assert.AreEqual(P.IP_Source, IP_Source);

            byte[] bytes = P.toBytes();

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractDestinationIP(bytes), IP_Destination);

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractSourceIP(bytes), IP_Source);

            //czy port wynikajacy z bajtow to ustawiony w testach port?
            Assert.AreEqual(Package.extractPortNumber(bytes), portNumber);
        }

        [TestMethod]
        public void packageAllParamsConstructorTest()
        {
            string inscription = "Mam malo wody";
            short portNumber = 2;
            IPAddress IP_Source = IPAddress.Parse("192.168.0.1");
            IPAddress IP_Destination = IPAddress.Parse("192.168.0.10");
            short packageNumber = 5;
            short frequency = 2;
            short band = 4;
            short usableInfoLength = (short)inscription.Length;
            short modulationPerformance = 1;
            short bitRate = 7;
            short ID = 17;
            short howManyPackages = 1;

            Package P = new Package(inscription, portNumber, IP_Destination.ToString(), IP_Source.ToString(), usableInfoLength, packageNumber, frequency, band, modulationPerformance, bitRate, ID, howManyPackages);

            //Czy pola sie dobrze wpisaly?
            Assert.AreEqual(inscription, P.usableMessage);
            Assert.AreEqual(portNumber, P.portNumber);
            Assert.AreEqual(IP_Destination, P.IP_Destination);
            Assert.AreEqual(IP_Source, P.IP_Source);
            Assert.AreEqual(packageNumber, P.packageNumber);
            Assert.AreEqual(band, P.band);
            Assert.AreEqual(usableInfoLength, P.usableInfoLength);
            Assert.AreEqual(modulationPerformance, P.modulationPerformance);
            Assert.AreEqual(bitRate, P.bitRate);
            Assert.AreEqual(ID, P.ID);
            Assert.AreEqual(howManyPackages, P.howManyPackages);

            //zamiana na tablice bajtow pol
            byte[] inscriptionBytes = Encoding.ASCII.GetBytes(inscription);
            byte[] portNumberBytes = BitConverter.GetBytes(portNumber);
            byte[] IP_DestinationBytes = IP_Destination.GetAddressBytes();
            byte[] IP_SourceBytes = IP_Source.GetAddressBytes();
            byte[] packageNumberBytes = BitConverter.GetBytes(packageNumber);
            byte[] frequencyBytes = BitConverter.GetBytes(frequency);
            byte[] bandBytes = BitConverter.GetBytes(band);
            byte[] usableInfoLengthBytes = BitConverter.GetBytes(usableInfoLength);
            byte[] modulationPerformanceBytes = BitConverter.GetBytes(modulationPerformance);
            byte[] bitRateBytes = BitConverter.GetBytes(bitRate);
            byte[] IDBytes = BitConverter.GetBytes(ID);
            byte[] howManyPackagesBytes = BitConverter.GetBytes(howManyPackages);

            //stworzenie listy bajtow z naglowkiem, takiej jak w klasie package
            var headerBytes = new List<byte>();
            headerBytes.AddRange(portNumberBytes);
            headerBytes.AddRange(IP_DestinationBytes);
            headerBytes.AddRange(IP_SourceBytes);
            headerBytes.AddRange(packageNumberBytes);
            headerBytes.AddRange(frequencyBytes);
            headerBytes.AddRange(bandBytes);
            headerBytes.AddRange(usableInfoLengthBytes);
            headerBytes.AddRange(modulationPerformanceBytes);
            headerBytes.AddRange(bitRateBytes);
            headerBytes.AddRange(IDBytes);
            headerBytes.AddRange(howManyPackagesBytes);

            //wypelnienie jej zerami
            headerBytes = Package.fillBytesWIth0(Package.headerMaxLength, headerBytes);

            //stworzenie listy bajtow z wiadomoscia
            var usableInfoBytes = new List<byte>();
            usableInfoBytes.AddRange(inscriptionBytes);

            //wypelnienie jej zerami
            usableInfoBytes = Package.fillBytesWIth0(Package.usableInfoMaxLength, usableInfoBytes);

            //sprawdzenie, czy bajty sa takie same
            Assert.IsTrue(usableInfoBytes.SequenceEqual(P.usableInfoBytes));
            Assert.IsTrue(headerBytes.SequenceEqual(P.headerBytes));
        }

        /// <summary>
        /// Testuje zamienianie pakietu na tablice bajtow
        /// </summary>
        [TestMethod]
        public void packageToByteArrayTest()
        {
            Package P = new Package();

            //Czy pakiet ma dobra dlugosc?
            Assert.AreEqual(P.toBytes().Length, 64);

            //Czy naglowek ma dobra dlugosc?
            Assert.AreEqual(Package.headerMaxLength, P.headerBytes.Count);

            //Sprawdzenie dlugosci pola wiadomosci uzytkowej
            Assert.AreEqual(Package.usableInfoMaxLength, P.usableInfoBytes.Count);
        }

        /// <summary>
        /// Testuje funkcje extractPortNumber
        /// </summary>
        [TestMethod]
        public void extractPortNumberTest()
        {
            Package P = new Package();
            byte[] bytes = P.toBytes();

            //czy nr portu to domylnie ustawiony 1?
            Assert.AreEqual(Package.extractPortNumber(bytes), 1);
        }

        /// <summary>
        /// Testuje funkcje extractDestinationIP
        /// </summary>
        [TestMethod]
        public void extractDestinationIPTest()
        {
            Package P = new Package();
            byte[] bytes = P.toBytes();

            IPAddress IP = IPAddress.Parse("127.0.0.1");

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractDestinationIP(bytes), IP);
        }

        /// <summary>
        /// Testuje funkcje Package.extract()
        /// </summary>
        [TestMethod]
        public void extractSourceIPTest()
        {
            Package P = new Package();
            byte[] bytes = P.toBytes();

            IPAddress IP = IPAddress.Parse("127.0.0.1");

            //czy adres IP to domyslnie ustawiony 127.0.0.1?
            Assert.AreEqual(Package.exctractSourceIP(bytes), IP);
        }

        /// <summary>
        /// Testuje wycinanie wiadomosci z tablicy bajtow.
        /// </summary>
        [TestMethod]
        public void extractUsableMessageTest()
        {
            string inscription = "nowa, zmieniona wiadomosc";
            Package P = new Package();

            //zamien wiadomosc na tablice bajtow
            byte[] bytes = Encoding.ASCII.GetBytes(inscription);

            //zmienienie wiadomosci w postaci bajtow w pakiecie
            P.changeMessage(inscription);

            Assert.AreEqual(inscription, P.usableMessage);

            bytes = Package.fillBytesWIth0(Package.usableInfoMaxLength, new List<byte>(bytes)).ToArray();

            //czy tablica bajtow liczy tyle samo
            Assert.AreEqual(bytes.Length, P.usableInfoBytes.Count);

            //czy bajty sa takie same
            Assert.IsTrue(bytes.SequenceEqual(P.usableInfoBytes));

            //Czy napisy sa takie same
            Assert.AreEqual(inscription, Package.extractUsableMessage(P.toBytes(), (short)inscription.Length));
        }

        /// <summary>
        /// Testuje metodę Package.changeMessage(string).
        /// </summary>
        [TestMethod]
        public void testChangeMessageString()
        {
            Package P = new Package();

            string message = "Nowa wiadomosc";
            short messageLength = (short)message.Length;
            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            var messageBytesList = Package.fillBytesWIth0(Package.usableInfoMaxLength, messageBytes.ToList());

            //zmiana wiadomosci
            P.changeMessage(message);
            var packageBytes = P.toBytes();

            //Czy wiadomosci sa te same?
            Assert.AreEqual(message, P.usableMessage);

            //Czy dlugosc wiadomosci sie zmienila na nowa?
            Assert.AreEqual(messageLength, P.usableInfoLength);

            var usableInfoLengthBytes = Package.extract(16, 2, packageBytes);

            //Czy najty z dlugoscia wiadomosci sa zmienione na nowe?
            Assert.IsTrue(messageLengthBytes.SequenceEqual(usableInfoLengthBytes));

            //Czy bajty z wiadomoscia sa zmienione na nowe?
            Assert.IsTrue(messageBytesList.SequenceEqual(P.usableInfoBytes));
        }

        /// <summary>
        /// Testuje metodę Package.changeMessage(byte[]).
        /// </summary>
        [TestMethod]
        public void testChangeMessageBytes()
        {
            Package P = new Package();

            string message = "Nowa wiadomosc";
            short messageLength = (short)message.Length;
            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            var messageBytesList = Package.fillBytesWIth0(Package.usableInfoMaxLength, messageBytes.ToList());

            //zmiana tablicy bajtow z wiadomoscia i zerami 
            P.changeMessage(messageBytes);
            var packageBytes = P.toBytes();

            //Czy wiadomosci sa te same?
            Assert.AreEqual(message, P.usableMessage);

            //Czy dlugosc wiadomosci sie zmienila na nowa?
            Assert.AreEqual(messageLength, P.usableInfoLength);

            var usableInfoLengthBytes = Package.extract(16, 2, packageBytes);

            //Czy najty z dlugoscia wiadomosci sa zmienione na nowe?
            Assert.IsTrue(messageLengthBytes.SequenceEqual(usableInfoLengthBytes));

            //Czy bajty z wiadomoscia sa zmienione na nowe?
            Assert.IsTrue(messageBytesList.SequenceEqual(P.usableInfoBytes));
        }

        [TestMethod]
        public void testChangeParam()
        {
            string inscription = "Mam malo wody";
            short portNumber = 10;
            IPAddress IP_Source = IPAddress.Parse("127.0.0.1");
            IPAddress IP_Destination = IPAddress.Parse("127.0.0.2");
            short packageNumber = 5;
            short frequency = 2;
            short band = 4;
            short usableInfoLength = (short)inscription.Length;
            short modulationPerformance = 11;
            short bitRate = 7;
            short ID = 17;
            short howManyPackages = 1;

            //zamiana na tablice bajtow pol
            byte[] inscriptionBytes = Encoding.ASCII.GetBytes(inscription);
            byte[] portNumberBytes = BitConverter.GetBytes(portNumber);
            byte[] IP_DestinationBytes = IP_Destination.GetAddressBytes();
            byte[] IP_SourceBytes = IP_Source.GetAddressBytes();
            byte[] packageNumberBytes = BitConverter.GetBytes(packageNumber);
            byte[] frequencyBytes = BitConverter.GetBytes(frequency);
            byte[] bandBytes = BitConverter.GetBytes(band);
            byte[] usableInfoLengthBytes = BitConverter.GetBytes(usableInfoLength);
            byte[] modulationPerformanceBytes = BitConverter.GetBytes(modulationPerformance);
            byte[] bitRateBytes = BitConverter.GetBytes(bitRate);
            byte[] IDBytes = BitConverter.GetBytes(ID);
            byte[] howManyPackagesBytes = BitConverter.GetBytes(howManyPackages);

            Package P = new Package();

            //zmiana pola
            P.changeMessage(inscription);
            //czy sie dobrze wpisalo
            Assert.AreEqual(inscription, P.usableMessage);
            //czy bajty dobrze przypisalo
            Assert.IsTrue(inscriptionBytes.SequenceEqual(Package.extract(Package.headerMaxLength, (short)inscription.Length, P.toBytes())));

            P.changePort(portNumber);
            Assert.AreEqual(portNumber, P.portNumber);
            Assert.IsTrue(portNumberBytes.SequenceEqual(Package.extract(0, 2, P.toBytes())));

            P.changeDestinationIP(IP_Destination.ToString());
            Assert.AreEqual(IP_Destination, P.IP_Destination);
            Assert.IsTrue(IP_DestinationBytes.SequenceEqual(Package.extract(2, 4, P.toBytes())));

            P.changeSourceIP(IP_Source.ToString());
            Assert.AreEqual(IP_Source, P.IP_Source);
            Assert.IsTrue(IP_SourceBytes.SequenceEqual(Package.extract(6, 4, P.toBytes())));

            P.changePackageNumber(packageNumber);
            Assert.AreEqual(packageNumber, P.packageNumber);
            Assert.IsTrue(packageNumberBytes.SequenceEqual(Package.extract(10, 2, P.toBytes())));

            P.changeFrequency(frequency);
            Assert.AreEqual(frequency, P.frequency);
            Assert.IsTrue(frequencyBytes.SequenceEqual(Package.extract(12, 2, P.toBytes())));

            P.changeBand(band);
            Assert.AreEqual(band, P.band);
            Assert.IsTrue(bandBytes.SequenceEqual(Package.extract(14, 2, P.toBytes())));

            P.changeUsableInfoLength(usableInfoLength);
            Assert.AreEqual(usableInfoLength, P.usableInfoLength);
            Assert.IsTrue(usableInfoLengthBytes.SequenceEqual(Package.extract(16, 2, P.toBytes())));

            P.changeModulationPerformance(modulationPerformance);
            Assert.AreEqual(modulationPerformance, P.modulationPerformance);
            Assert.IsTrue(modulationPerformanceBytes.SequenceEqual(Package.extract(18, 2, P.toBytes())));

            P.changeBitRate(bitRate);
            Assert.AreEqual(bitRate, P.bitRate);
            Assert.IsTrue(bitRateBytes.SequenceEqual(Package.extract(20, 2, P.toBytes())));

            P.changeID(ID);
            Assert.AreEqual(ID, P.ID);
            Assert.IsTrue(IDBytes.SequenceEqual(Package.extract(22, 2, P.toBytes())));

            P.changeHowManyPackages(howManyPackages);
            Assert.AreEqual(P.howManyPackages, howManyPackages);
            Assert.IsTrue(howManyPackagesBytes.SequenceEqual(Package.extract(24, 2, P.toBytes())));
        }
    }
}