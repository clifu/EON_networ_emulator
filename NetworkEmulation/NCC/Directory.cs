using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;


namespace NCC
{

    /// <summary>
    /// klasa odpowiadająca za translację adresów, jej logi wyświtlane będą kolorem białym na niebieskim tle
    /// </summary>
    public class Directory
    {
        //słownik służący do translacji adresów, pierwsza pozycja to ID czyli np. Franek, a druga pozycja to jego IP
        public Dictionary<string, string> AddressTranslationTable = new Dictionary<string, string>();

        // adresy IP których szukamy w słowniku w pierwszym NCC
        public string DestinationAdddress;

        public string OriginAddress;

        // adres IP szukany w funkcji SingleAddressTranslation
        public string address;

        //zmienna określajaca czy klienci są w tej samej domenie
        public bool OneDomain;

        /// <summary>
        /// niestatyczna metoda zmieniajaca ID na IP, jesli znajduje się ono w bazie, czyli pliku tekstowym
        /// <param name="OriginID"></param>
        /// <param name="DestinationID"></param>
        //// <returns></returns>
        public bool AddressTranslation(string OriginID, string DestinationID)
        {
            try
            {
                //zmienna pomocnicza pokazująca czy translacja adresów się udała
                bool DirectoryAccess = false;

               
                OneDomain = false;

                //ustawienie koloru czcionki w celu łatwiejszego rozróżnienia logów
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                
               

                // funkcja szukająca dla danego ID klienta odpowiadającego mu adresu IP w słowniku
                // jesli nie znajdzie takeigo ID to zwraca nulla
                AddressTranslationTable.TryGetValue(OriginID, out OriginAddress);
                AddressTranslationTable.TryGetValue(DestinationID, out DestinationAdddress);

                //jeśli obydwa adresy sa nullami nie znamy ani nadawcy ani odbiorcy, nie można ustawic połączenia
                if(OriginAddress==null && DestinationAdddress == null)
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory:There are no clients with ID's: {0}, {1} in that domain", OriginID, DestinationID);
                    DirectoryAccess = false;
                }
                //jeśli nadawca nie jest nullem to znaczy, że jest w anszej domenie a odbiorca nie
                else if(OriginAddress!=null && DestinationAdddress == null)
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory:There is client with ID: {0} {1}, but there are no client with ID: {2} in that domain", OriginID, OriginAddress, DestinationID);
                    DirectoryAccess = true;
                }
                //nadawca jest nullem, a odbiorca nie jest. Sytuacja w przypadku przejścia do NCC drugiej domeny, czego w tym projekcie nie rozpatrujemy
                else if(OriginAddress == null && DestinationAdddress != null)
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory:There is client with ID: {0}, {1}, but there are no client with ID: {2} in that domain", DestinationID,DestinationAdddress,OriginID);
                    DirectoryAccess = false;
                }
                //nadawca i odbiorca są w tej domenie, możemy zrobić tramslację ich adresów
                else
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory:Both clients witd ID's: {0}, {1} with IP's: {2}, {3} are in that domain", OriginID, DestinationID, OriginAddress, DestinationAdddress);
                    DirectoryAccess = true;
                    OneDomain = true;
                }
                Console.ForegroundColor = ConsoleColor.Black;
                return DirectoryAccess;
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                String tmp = e.ToString();
                Console.WriteLine("Cannot translate address");
                return false;
            }
        }


        public bool SingleAddressTranslation(string ID)
        {
            try
            {
                //zmienna pomocnicza pokazująca czy translacja adresów się udała
                bool DirectoryAccess = false;

                //ustawienie koloru czcionki w celu łatwiejszego rozróżnienia logów
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                // funkcja szukająca dla danego ID klienta odpowiadającego mu adresu IP w słowniku
                // jesli nie znajdzie takeigo ID to zwraca nulla
                AddressTranslationTable.TryGetValue(ID, out address);
             
                //jeśli obydwa adresy sa nullami nie znamy ani nadawcy ani odbiorcy, nie można ustawic połączenia
                if (address == null )
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory:There are no client with ID: {0} in that domain",ID);
                    DirectoryAccess = false;
                }
                else
                {
                    Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Directory: Client witd ID: {0} with IP: {1} is in that domain", ID, address);
                    DirectoryAccess = true;
                }
                Console.ForegroundColor = ConsoleColor.Black;
                return DirectoryAccess;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                String tmp = e.ToString();
                Console.WriteLine("Cannot translate address");
                return false;
            }
        }

    
        /// <summary>
        /// funkcja, która sprawdza czy obaj klienci sa w tej domenie potrzebna do określenia ilościm hopów
        /// </summary>
        /// <param name="OriginIP"></param>
        /// <param name="DestinationIP"></param>
        /// <returns></returns>
        public string ChoosingHopsNumber(string OriginIP, string DestinationIP)
        {
            try
            {
                string hopsnumber = null;

                bool address1 = AddressTranslationTable.ContainsValue(OriginIP);
                bool address2 = AddressTranslationTable.ContainsValue(DestinationIP);

                if(address1 == true && address2 == true)
                {
                    hopsnumber = "1";
                }
                else if(address1==true && address2 == false)
                {
                    hopsnumber = "2";
                }
                else if(address1==false && address2 == true)
                {
                    hopsnumber = "2";
                }
                else if(address1==false && address2 == false)
                {
                    //taka sytuacja nie może się wydarzyć w sumie
                    hopsnumber = "1";
                }
                return hopsnumber;
            }
            catch (Exception e)
            {
                return null;
            }
        }





        /// <summary>
        /// funkcją czytająca z pliku konfiguracyjenego wywoływana w Mainie, jako parametr przyjmuje nazwę pliku
        /// </summary>
        /// <param name="path"></param>
        public void ReadingFromDirectoryFile(string path)
        {
            string line;
            char[] delimiterChars = { '#' };
            string[] words;
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        words = line.Split(delimiterChars);
                        AddressTranslationTable.Add(words[0], words[1]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read from file");
            }
        }
    }
}
