using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace NetworkingTools
{ 
    /// <summary>
    /// Klasa reprezentująca pakiet NMS
    /// </summary>

    public class NMSPackage
    {
        //Dlugosc pola informacji uzytkowej [B] 40
        public short usableInfoMaxLength { get; }

        //Dlugosc naglowka, [B]. 24
        public short headerMaxLength { get; }

        //numer portu, 2B
        public short numberOfRouter;

        //numer portu, 2B
        public short destinationport;

        //adres IPv4 celu, 4B
        public IPAddress IP_Destination;

        //adres IPv4 źródła, 4B
        public IPAddress IP_Source;

        //Wlasciwa dlugosc informacji uzytkowej, bez wypelnienia zerowymi bajtami
        public short usableInfoLength { get; set; }

        //40-bajtowe pole z wiadomością użytkową
        public List<byte> usableInfoBytes { get; set; }

        //pole z wiadomością Ala ma kota
        public string usableMessage { get; set; }

        //pole z częstotliwością
        public int freq_in;

        public List<byte> headerBytes { get; set; }

        /// <summary>
        /// Ustawia wielkosc pol pakietu, nr portu
        /// </summary>
        public NMSPackage()
        {
            usableInfoMaxLength = 104;
            headerMaxLength = 24;

            //domyslny numer portu źródłowego
            numberOfRouter = 1;

            //domyslny numer portu docelowego
            destinationport = 11000;

            //domyslny adres celu
            IP_Destination = IPAddress.Parse("127.0.0.1");

            //domyslny adres zrodla2
            IP_Source = IPAddress.Parse("127.0.0.1");

            //wiadomosc keep alive
            usableMessage = string.Empty;

            //freq_in = 500;

            //zapisz pola do list z bajtami
            this.toBytes();
        }

        /// <summary>
        /// Konstruktor z wiadomoscia uzytkowa.
        /// </summary>
        public NMSPackage(string usableMessage) : this()
        {
            this.usableMessage = usableMessage;
            this.usableInfoLength = (short)usableMessage.Length;

            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }


        /// <summary>
        /// Wiadomość Node is up, która przekazuje IP i port rotera do komunikacji z NMSem
        /// </summary>
        public NMSPackage(string IP_Source, short numberOfRouter, string usableMessage, short usableInfoLength) : this()
        {
            this.IP_Source = IPAddress.Parse(IP_Source);
            this.numberOfRouter = numberOfRouter;
            this.usableMessage = usableMessage;
            this.usableInfoLength = usableInfoLength;
            
            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }


        /// <summary>
        /// Konstruktor przede wszystkim do wiadomości Keep Alive
        /// </summary>
        public NMSPackage(string IP_Source, string usableMessage, short usableInfoLength) : this()
        {
            this.IP_Source = IPAddress.Parse(IP_Source);
            this.usableMessage = usableMessage;
            this.usableInfoLength = usableInfoLength;


            //Aktualizacja wartosci tablic z bajtami
            actualizeBytes();
        }
        /// <summary>
        /// wiadomość wysyłana przez NMSa do agentów
        /// </summary>
        public NMSPackage( int freq_in,string IP_Node2, string usableMessage, short usableInfoLength) : this()
        {

            this.freq_in = freq_in;
            this.IP_Destination = IPAddress.Parse(IP_Node2);
            this.usableInfoLength = usableInfoLength;
            this.usableMessage = usableMessage;

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
            //zapisanie częstotliwosci 
            byte[] freq_in_bytes = BitConverter.GetBytes(freq_in);

            //zapisanie nru portu w postaci tablicy bajtow (2B)
            byte[] numberOfRouter_bytes = BitConverter.GetBytes(numberOfRouter);

            //zapisanie nru portu w postaci tablicy bajtow (2B)
            byte[] destinationport_bytes = BitConverter.GetBytes(destinationport);

            //zmiana ip celu na bajty (4B)
            byte[] IP_Destination_bytes = IP_Destination.GetAddressBytes();

            //zmiana ip zrodla na bajty (4B)
            byte[] IP_Source_bytes = IP_Source.GetAddressBytes();

            //zamiana dlugosci informacji uzytkowej na bajty
            byte[] usableInfoLengthBytes = BitConverter.GetBytes(usableInfoLength);

            //jak headerBytes jest juz jakis niezerowy
            if (this.headerBytes != null)
                //to trzeba go wyzerowac
                this.headerBytes = null;

            this.headerBytes = new List<byte>();

            //Dodanie wszystkich pol w kolejnosci, w postaci bajtow, do listy bajtow
            headerBytes.AddRange(numberOfRouter_bytes); //2 
            headerBytes.AddRange(destinationport_bytes); //2 
            headerBytes.AddRange(IP_Source_bytes);//4
            headerBytes.AddRange(IP_Destination_bytes);//4
            headerBytes.AddRange(usableInfoLengthBytes);//2


            //wypelnienie jej zerami
            headerBytes = fillBytesWIth0(24, headerBytes);

            //jak nie jest nullem, to trzeba to wyzerowac
            if (usableInfoBytes != null)
                usableInfoBytes = null;

            this.usableInfoBytes = new List<byte>();

            //dodanie wiadomosci do listy
            usableInfoBytes.AddRange(Encoding.ASCII.GetBytes(usableMessage));

            //uzupełnienie zerami
            usableInfoBytes = fillBytesWIth0(104, usableInfoBytes);
        }


        /// <summary>
        /// Wypelnia liste bajtow zerami do okreslonego miejsca
        /// </summary>

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
        /// Wycina z tablicy bajtow te od zrodlowego IP i zamienia je na IPAddress.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static IPAddress exctractSourceIP(byte[] packageBytes)
        {
            //Source IP sie zaczyna na 4. pozycji i ma dlugosc 4B
            byte[] bytes = NMSPackage.extract(4, 4, packageBytes);

            //Konwersja na adres IP
            return new IPAddress(bytes);
        }

        /// <summary>
        /// Wycina z tablicy bajtow i konwertuje na shorta dlugosc pola z informacja uzytkowa.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractUsableInfoLength(byte[] packageBytes)
        {
            
            byte[] bytes = NMSPackage.extract(12, 2, packageBytes);

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
            //Wiadomosc uzytkowa jest od 24. indeksu i ma dlugosc 40
            byte[] bytes = NMSPackage.extract(24, usableInfoLength, packageBytes);

            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Wycina z tablicy bajtow te od zrodlowego IP i zamienia je na IPAddress.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static IPAddress exctractDestinationIP(byte[] packageBytes)
        {
            //nr pakietu jest od 10 do 11 w tablicy
            byte[] bytes = NMSPackage.extract(8, 4, packageBytes);

            //Konwersja na adres IP
            return new IPAddress(bytes);
        }

        /// <summary>
        /// Odnajduje i konwertuje bajty z pakietu na liczbe typu short.
        /// </summary>
        /// <param name="packageBytes"> Pakiet w postaci bajtów.</param>
        /// <returns>Nr portu z pakietu.</returns>
        public static short extractNumberOfRouterNumber(byte[] packageBytes)
        {
            //Nr portu sie zaczyna od indeksu 0 i ma dlugosc 2
            byte[] bytes = NMSPackage.extract(0, 2, packageBytes);

            //konwertuje bajty na shorta, 0 to indeks mowiacy, skad zaczac w tablicy
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Odnajduje i konwertuje bajty z pakietu na liczbe typu short.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public static short extractDestinationPortNumber(byte[] packageBytes)
        {
            //Destination port, zaczyna sie ną drugiej pozycji i ma długośc 2B
            byte[] bytes = NMSPackage.extract(2, 2, packageBytes);

            //Konwersja na adres IP
            return BitConverter.ToInt16(bytes, 0);
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

    }
}
