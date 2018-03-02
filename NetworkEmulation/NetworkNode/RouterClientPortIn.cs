using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Klasa z funkcjonalnoscia portu wejsciowego od klienta
    /// </summary>
    public class RouterClientPortIn
    {
        public int port;

        /// <summary>
        /// Funkcja wpisujaca do pakietu w postaci tablicy bajtow.
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <param name="borderNodeCommutationTable"></param>
        /// <returns></returns>
        public static byte[] GenerateCharacteristicInfo(byte[] packageBytes, ref BorderNodeCommutationTable table)
        {
            try
            {
                //Wyjecie IP zrodla z pakietu
                string IP_in = Package.exctractSourceIP(packageBytes).ToString();

                //Numer portu, ktorym przyszedl pakiet od klienta
                short port_in = Package.extractPortNumber(packageBytes);

                //IP Destination, do ktorego klienta idzie
                string IPDestionation = Package.exctractDestinationIP(packageBytes).ToString() ;
                //Odnajduje okreslony rzad
                var row = table.FindRow(IP_in, port_in,IPDestionation);

                //Odtworzenie obiektu klasy pakiet 
                Package P = new Package(packageBytes);

                //Zmiana parametrow 
                P.changeBand(row.band);
                P.changeBitRate(row.bitRate);
                //TODO: Etap2 - niech samemu router ustala czestotliwosc na podstawie EONowej tablicy
                P.changeFrequency(row.frequency);
                P.changeModulationPerformance(row.modulationPerformance);

                //Zmiana numeru portu na socketowy
                P.changePort(row.Port);

                return P.toBytes();
            }
            catch (Exception E)
            {
                Console.WriteLine("RouterClientPortIn.GenerateUsableInfo: " + E.Message);
                return packageBytes;
            }
        }
    }
}
