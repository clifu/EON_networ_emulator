using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa reprezentująca pakiet
    /// </summary>
    /// 
    /// <remarks>
    /// W sumie 64B
    /// 24B na naglowek
    /// 40B na informacje uzyteczna
    /// 
    /// Naglowek:
    /// port - 2B. Od 0 do 1 Jest zrodlowy, gdy slemy do chmury, a docelowy, gdy pakiet leci od chmury do routera
    /// IP celu - 4B. Od 2 do 5
    /// IP zrodla - 4B Od 6 do 9
    /// Nr pakietu  - 2B od 10 do 11
    /// Nr czestotliwosci - 2B od 12 do 13, short. nr = x => czestotliwosc = 193 000 GHz + x  * 12.5 GHz
    /// Pasmo - 2B od 14 do 15, short. Pasmo = 5 => pasmo = 12.5*5 [GHz]
    /// Dlugosc informacji uzytkowej - 2B od 16 do 17, short
    /// Wydajnosc modulacji, N. Ile bitow koduje jeden symbol - 2B od 18 do 19, short
    /// Z jaka predkowscia bitowa leci ten pakiet - 2B od 20 do 21, short
    /// Identyfikator pakietu, 2B od 22 do 23, short
    /// Ilosc pakietow w grupie, 2B od 24 do 25, short
    /// 
    /// Informacja uzytkowa (domyslnie):
    /// Tekst + 
    /// timestamp. 
    /// 
    /// Wiadomość od i do NMSa
    /// 
    /// </remarks>

    public class Package
    {
        //Dlugosc pola informacji uzytkowej [B] 38
        public static short usableInfoMaxLength = 38;

        //Dlugosc naglowka, [B]. 26
        public static short headerMaxLength = 26;

        //numer portu, 2B
        public short portNumber;

        //adres IPv4 celu, 4B
        public IPAddress IP_Destination;

        //adres IPv4 źródła, 4B
        public IPAddress IP_Source;

        //nr pakietu. Zaczyna sie od 1
        public short packageNumber { get; set; }

        //czestotliwosciowy odpowiednik lambdy
        public short frequency { get; set; }

        //Pasmo zajmowane przez kanal do transmisji pakietu
        public short band { get; set; }

        //Wlasciwa dlugosc informacji uzytkowej, bez wypelnienia zerowymi bajtami
        public short usableInfoLength { get; set; }

        //Wydajnosc modulacji, N. Ile bitow koduje jeden symbol
        public short modulationPerformance { get; set; }

        //Z jaka predkowscia bitowa leci ten pakiet. Rzeczywista predkosc = bitRate*12.5Gb/s
        public short bitRate { get; set; }

        //Identyfikator pakietu
        public short ID { get; set; }

        //ile pakietow jest w ogole?
        public short howManyPackages { get; set; }

        //40-bajtowe pole z wiadomością użytkową
        public List<byte> usableInfoBytes { get; set; }

        //pole z wiadomością Ala ma kota
        public string usableMessage { get; set; }

        public List<byte> headerBytes { get; set; }


        /// <summary>
        /// Ustawia wielkosc pol pakietu, nr portu
        /// </summary>
        public Package()
        {
            //domyslny numer portu
            portNumber = 1;

            //domyslny adres celu
            IP_Destination = IPAddress.Parse("127.0.0.1");

            //domyslny adres zrodla
            IP_Source = IPAddress.Parse("127.0.0.1");

            //pierwszy pakiet
            packageNumber = 1;

            //0 + 193 000 GHz...
            frequency = 0;

            //12.5GHz
            band = 1;

            //Jeden symbol koduje 1 bit
            modulationPerformance = 1;

            //predkosc bitowa, z jaka leci pakiet n*12.5Gb/s
            bitRate = 1;

            //losowanie ID
            ID = (short)(new Random()).Next(0, short.MaxValue);

            //Ile pakietow w grupie
            howManyPackages = 1;

            //Ala ma kota  + czas danego dnia w milisekundach
            usableMessage =
                "Ala ma kora, a kor nie ma lai" + DateTime.Now.Date.TimeOfDay.TotalMilliseconds.ToString();

            //Dlugosc tego stringa
            usableInfoLength = (short)usableMessage.Length;

            //zapisz pola do list z bajtamia
            this.toBytes();
        }

        /// <summary>
        /// Konstruktor odtwarzajacy Pakiet z tablicy bajtow
        /// </summary>
        /// <param name="packageBytes"></param>
        public Package(byte[] packageBytes)
        {
            //Wyekstraktowanie wartosci i przypisanie ich do konkretnych pol
            this.portNumber = Package.extractPortNumber(packageBytes);
            this.IP_Destination = Package.exctractDestinationIP(packageBytes);
            this.IP_Source = Package.exctractSourceIP(packageBytes);
            this.packageNumber = Package.extractPackageNumber(packageBytes);
            this.frequency = Package.extractFrequency(packageBytes);
            this.band = Package.extractBand(packageBytes);
            this.usableInfoLength = Package.extractUsableInfoLength(packageBytes);
            this.modulationPerformance = Package.extractModulationPerformance(packageBytes);
            this.bitRate = Package.extractBitRate(packageBytes);
            this.ID = Package.extractID(packageBytes);
            this.howManyPackages = Package.extractHowManyPackages(packageBytes);
            this.usableMessage = Package.extractUsableMessage(packageBytes, this.usableInfoLength);

            this.headerBytes = new List<byte>();
            this.usableInfoBytes = new List<byte>();

            //Aktualizacja list z bajtami
            for (int i = 0; i < headerMaxLength; i++)
            {
                headerBytes.Add(packageBytes[i]);
            }

            for (int i = headerMaxLength; i < headerMaxLength + usableInfoMaxLength; i++)
            {
                usableInfoBytes.Add(packageBytes[i]);
            }
        }

        /// <summary>
        /// Konstruktor z wiadomoscia uzytkowa.
        /// </summary>
        /// <param name="usableMessage"></param>
        public Package(string usableMessage) : this()
        {
            this.usableMessage = usableMessage;
            this.usableInfoLength = (short)usableMessage.Length;

            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }

        /// <summary>
        /// Konstruktor z ustawianiem wszystkich naglowkow.
        /// </summary>
        /// <param name="usableMessage"></param>
        /// <param name="port"></param>
        /// <param name="IP_Source"></param>
        /// <param name="IP_Destination"></param>
        public Package(string usableMessage, short port, string IP_Destination, string IP_Source, short usableInfoLength) : this()
        {
            this.usableMessage = usableMessage;
            this.portNumber = port;
            this.IP_Source = IPAddress.Parse(IP_Source);
            this.IP_Destination = IPAddress.Parse(IP_Destination);
            this.usableInfoLength = usableInfoLength;

            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }

        /// <summary>
        /// Konstruktor ze wszystkimi parametrami z naglowka i wiadomoscia.
        /// </summary>
        /// <param name="usableMessage"></param>
        /// <param name="port"></param>
        /// <param name="IP_Source"></param>
        /// <param name="IP_Destination"></param>
        /// <param name="packageNumber"></param>
        /// <param name="frequency"></param>
        /// <param name="band"></param>
        /// <param name="usableInfoLength"></param>
        public Package(string usableMessage, short port, string IP_Destination, string IP_Source, short usableInfoLength, short packageNumber,
            short frequency, short band) : this()
        {
            this.usableMessage = usableMessage;
            this.portNumber = port;
            this.IP_Source = IPAddress.Parse(IP_Source);
            this.IP_Destination = IPAddress.Parse(IP_Destination);
            this.packageNumber = packageNumber;
            this.frequency = frequency;
            this.band = band;
            this.usableInfoLength = usableInfoLength;

            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }

        public Package(string usableMessage, short port, string IP_Destination, string IP_Source, short usableInfoLength, short packageNumber,
            short frequency, short band, short modulationPerformance, short bitRate, short ID, short howManyPackages) : this()
        {
            this.usableMessage = usableMessage;
            this.portNumber = port;
            this.IP_Source = IPAddress.Parse(IP_Source);
            this.IP_Destination = IPAddress.Parse(IP_Destination);
            this.packageNumber = packageNumber;
            this.frequency = frequency;
            this.band = band;
            this.usableInfoLength = usableInfoLength;
            this.modulationPerformance = modulationPerformance;
            this.bitRate = bitRate;
            this.ID = ID;
            this.howManyPackages = howManyPackages;

            //aktualizacja tablic z bajtami
            actualizeBytes();
        }

        /// <summary>
        /// Zapisuje pakiet w postaci tablicy bajtow.
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] toBytes()
        {
            //zaktualizuj zawartosc list z bajtami
            actualizeBytes();

            var bytesList = new List<byte>(headerBytes);
            bytesList.AddRange(usableInfoBytes);

            return bytesList.ToArray();
        }

        /// <summary>
        /// Aktualizuje listy bajtow po zmianie jakiegos pola w obiekcie klasy Package.
        /// </summary>
        public void actualizeBytes()
        {
            //zapisanie nru portu w postaci tablicy bajtow
            byte[] portNumber_bytes = BitConverter.GetBytes(portNumber);

            //zmiana ip celu na bajty (4B)
            byte[] IP_Destination_bytes = IP_Destination.GetAddressBytes();

            //zmiana ip zrodla na bajty (4B)
            byte[] IP_Source_bytes = IP_Source.GetAddressBytes();

            //zmiana nru pakietu na bajty
            byte[] packageNumberBytes = BitConverter.GetBytes(packageNumber);

            //zmiana nru czestotliwosci na bajty
            byte[] frequencyBytes = BitConverter.GetBytes(frequency);

            //zmiana pasma na bajty
            byte[] bandBytes = BitConverter.GetBytes(band);

            //zamiana dlugosci informacji uzytkowej na bajty
            byte[] usableInfoLengthBytes = BitConverter.GetBytes(usableInfoLength);

            //zmiana wydajnosci modulacji na bajty
            byte[] modulationPerformanceBytes = BitConverter.GetBytes(modulationPerformance);

            //zmiana predkosci bitowej na bajty
            byte[] bitRateBytes = BitConverter.GetBytes(bitRate);

            //zmiana ID na bajty
            byte[] IDBytes = BitConverter.GetBytes(ID);

            //zmiana liczby pakietow w serii na bajty
            byte[] howManyPackagesBytes = BitConverter.GetBytes(howManyPackages);

            //jak headerBytes jest juz jakis niezerowy
            if (this.headerBytes != null)
                //to trzeba go wyzerowac
                this.headerBytes = null;

            this.headerBytes = new List<byte>();

            //Dodanie wszystkich pol w kolejnosci, w postaci bajtow, do listy bajtow
            headerBytes.AddRange(portNumber_bytes);
            headerBytes.AddRange(IP_Destination_bytes);
            headerBytes.AddRange(IP_Source_bytes);
            headerBytes.AddRange(packageNumberBytes);
            headerBytes.AddRange(frequencyBytes);
            headerBytes.AddRange(bandBytes);
            headerBytes.AddRange(usableInfoLengthBytes);
            headerBytes.AddRange(modulationPerformanceBytes);
            headerBytes.AddRange(bitRateBytes);
            headerBytes.AddRange(IDBytes);
            headerBytes.AddRange(howManyPackagesBytes);

            //wypelnienie jej zerami
            headerBytes = fillBytesWIth0(headerMaxLength, headerBytes);

            //jak nie jest nullem, to trzeba to wyzerowac
            if (usableInfoBytes != null)
                usableInfoBytes = null;

            this.usableInfoBytes = new List<byte>();

            //dodanie wiadomosci do listy
            usableInfoBytes.AddRange(Encoding.ASCII.GetBytes(usableMessage));

            //uzupełnienie zerami
            usableInfoBytes = fillBytesWIth0(usableInfoMaxLength, usableInfoBytes);
        }

        /// <summary>
        /// Wypelnia liste bajtow zerami do okreslonego miejsca
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static List<byte> fillBytesWIth0(int maxLength, List<byte> bytes)
        {
            if (bytes.Count > maxLength)
            {
                throw new Exception("Bytes table is longer than destination size. Cannot shorten the list!");
                //zwroce bytes, bo jak przyrownamy do outputu funkcji to bytes sie nie zmieni
                return bytes;
            }
            //Nie ma czego wypelniac zerami
            else if (bytes.Count == maxLength)
                return bytes;

            for (int i = bytes.Count; i < maxLength; i++)
            {
                //dodaję 0000 0000, aż rozmiar listy byteów urośnie do maksymalnej dlugosci pola
                bytes.Add(0x00);
            }

            return bytes;
        }

        /// <summary>
        /// Odnajduje i konwertuje bajty z pakietu na liczbe typu short.
        /// </summary>
        /// <param name="packageBytes"> Pakiet w postaci bajtów.</param>
        /// <returns>Nr portu z pakietu.</returns>
        public static short extractPortNumber(byte[] packageBytes)
        {
            //Nr portu sie zaczyna od indeksu 0 i ma dlugosc 2
            byte[] bytes = Package.extract(0, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow te od docelowego IP i zamienia je na IPAddress.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static IPAddress exctractDestinationIP(byte[] packageBytes)
        {
            //Destination IP sie zaczyna na 2. pozycji i ma dlugosc 4B
            byte[] bytes = Package.extract(2, 4, packageBytes);

            //Konwersja na adres IP
            return new IPAddress(bytes);
        }

        /// <summary>
        /// Wycina kawałek z tablicy bajtow o okreslonej dlugosci w okreslonym miejscu.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static byte[] extract(short startIndex, short length, byte[] packageBytes)
        {
            byte[] bytes = new byte[length];

            int k = 0;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                bytes[k] = packageBytes[i];
                k++;
            }

            return bytes;
        }

        /// <summary>
        /// Wycina z tablicy bajtow te od zrodlowego IP i zamienia je na IPAddress.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static IPAddress exctractSourceIP(byte[] packageBytes)
        {
            //Source IP sie zaczyna na 6. pozycji i ma dlugosc 4B
            byte[] bytes = Package.extract(6, 4, packageBytes);

            //Konwersja na adres IP
            return new IPAddress(bytes);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta nr pakietu.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractPackageNumber(byte[] packageBytes)
        {
            //nr pakietu jest od 10 do 11 w tablicy
            byte[] bytes = Package.extract(10, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta nr czestotliwosci.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractFrequency(byte[] packageBytes)
        {
            //czestotliwosc jest od 12 do 13 w tablicy
            byte[] bytes = Package.extract(12, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta pasmo.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractBand(byte[] packageBytes)
        {
            //pasmo jest od 14 do 15 w tablicy
            byte[] bytes = Package.extract(14, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta dlugosc pola z informacja uzytkowa.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractUsableInfoLength(byte[] packageBytes)
        {
            //dlugosc informacji uzytkowej jest od 16 do 17 w tablicy
            byte[] bytes = Package.extract(16, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta wydajnosc modulacji.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractModulationPerformance(byte[] packageBytes)
        {
            //wydajnosc modulacji jest od 18 do 19 w tablicy
            byte[] bytes = Package.extract(18, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta predkosc bitowa z jaka leci pakiet.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractBitRate(byte[] packageBytes)
        {
            //wydajnosc modulacji jest od 20 do 21 w tablicy
            byte[] bytes = Package.extract(20, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta ID pakietu.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractID(byte[] packageBytes)
        {
            //wydajnosc modulacji jest od 22 do 23 w tablicy
            byte[] bytes = Package.extract(22, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Wycina z tablicy bajtow fragment zawierajacy wiadomosc uzytkowa i zamienia ja na stringa
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <param name="usableInfoLength></param>
        /// <returns></returns>
        public static string extractUsableMessage(byte[] packageBytes, short usableInfoLength)
        {
            //Wiadomosc uzytkowa jest od 26. indeksu i ma dlugosc 38
            byte[] bytes = Package.extract(Package.headerMaxLength, usableInfoLength, packageBytes);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta całkowitą ilość paczek.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractHowManyPackages(byte[] packageBytes)
        {
            //ilość paczek znajduje się od 24 do 25
            byte[] bytes = Package.extract(24, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Zmienia wartosc wiadomosci pakietu i podmienia ja w tablicy bajtow. UWAGA: Uzupelnia ja zerami!
        /// Nalezy podac tablice bajtow BEZ zer na koncu!
        /// </summary>
        /// <param name="usableMessageBytes"></param>
        public void changeMessage(byte[] usableMessageBytes)
        {
            //Jak tablica bajtow jest za duza, to rzuc wyjatek
            if (usableMessageBytes.Length > usableInfoMaxLength)
                throw new Exception("Package.changeMessage(byte[]): Dlugosc podanej tablicy bajtow jest wieksza, niz " + usableInfoMaxLength + " bajtow!");
            //wpisanie pola z wiadomoscia
            this.usableMessage = Encoding.ASCII.GetString(usableMessageBytes);
            //przypisanie dlugosci wiadomosci
            this.usableInfoLength = (short)this.usableMessage.Length;
            //aktualizacja z wpisanych pol
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia wartosc wiadomosci uzytkowej i odpowiednie bajty
        /// </summary>
        /// <param name="message"></param>
        public void changeMessage(string message)
        {
            //Jak tablica bajtow jest za duza, to rzuc wyjatek
            if (message.Length > usableInfoMaxLength)
                throw new Exception("Package.changeMessage(string): Dlugosc podanego stringa bajtow jest wieksza, niz " + usableInfoMaxLength + " bajtow!");
            //przypisanie wiadomosci
            this.usableMessage = message;
            //przypisanie dlugosci wiadomosci
            this.usableInfoLength = (short)message.Length;
            //aktualizacja z gotowych wpisanych pol
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola port.
        /// </summary>
        /// <param name="port"></param>
        public void changePort(short port)
        {
            this.portNumber = port;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola IP_Source.
        /// </summary>
        /// <param name="IP"></param>
        public void changeSourceIP(string IP)
        {
            this.IP_Source = IPAddress.Parse(IP);
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola IP_Destination.
        /// </summary>
        /// <param name="IP"></param>
        public void changeDestinationIP(string IP)
        {
            this.IP_Destination = IPAddress.Parse(IP);
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola packageNumber.
        /// </summary>
        /// <param name="number"></param>
        public void changePackageNumber(short number)
        {
            this.packageNumber = number;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola frequency.
        /// </summary>
        /// <param name="frequency"></param>
        public void changeFrequency(short frequency)
        {
            this.frequency = frequency;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola band.
        /// </summary>
        /// <param name="band"></param>
        public void changeBand(short band)
        {
            this.band = band;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola usableInfoLength.
        /// </summary>
        /// <param name="length"></param>
        public void changeUsableInfoLength(short length)
        {
            this.usableInfoLength = length;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola modulationPerformance.
        /// </summary>
        /// <param name="performance"></param>
        public void changeModulationPerformance(short performance)
        {
            this.modulationPerformance = performance;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola bitRate.
        /// </summary>
        /// <param name="bitRate"></param>
        public void changeBitRate(short bitRate)
        {
            this.bitRate = bitRate;
            this.actualizeBytes();
        }
        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola ID.
        /// </summary>
        /// <param name="ID"></param>
        public void changeID(short ID)
        {
            this.ID = ID;
            this.actualizeBytes();
        }

        /// <summary>
        /// Zmienia i aktualizuje w tablicy bajtow wartosc pola howManyPackages.
        /// </summary>
        /// <param name="howManyPackages"></param>
        public void changeHowManyPackages(short howManyPackages)
        {
            this.howManyPackages = howManyPackages;
            this.actualizeBytes();
        }

        /// <summary>
        /// Zamienia obiekt klasy Package na tablice bajtow
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] PacketToByte(Package packet)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, packet);
            return ms.ToArray();
        }

        /// <summary>
        /// Zamienia tablice bajtow na obiekt klasy Package
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Package ByteToPacket(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            object o = (object)bf.Deserialize(ms);
            return (Package)o;
        }
    }
}
