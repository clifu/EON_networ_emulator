using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Klasa zawierająca tablicę komutacji pakietów w węzłach na brzegu sieci.
    /// </summary>
    public class BorderNodeCommutationTableRow
    {
        /// <summary>
        /// Jakim adresem IP przyszedł pakiet?
        /// </summary>
        public IPAddress IP_IN { get; set; }

        /// <summary>
        /// Jakim portem przyszedł pakiet?
        /// </summary>
        public short port_in { get; set; }

        /// <summary>
        /// Jakie pasmo przypisać temu łączu?
        /// </summary>
        public short band { get; set; }

        /// <summary>
        /// Jaką częstotliwość przypisać temu łączu? 
        /// </summary>
        public short frequency { get; set; }

        /// <summary>
        /// Jaką wydajnośc modulacji nadać łączu?
        /// </summary>
        public short modulationPerformance { get; set; }

        /// <summary>
        /// Jaką prędkość bitową nadać temu łączu? 
        /// </summary>
        public short bitRate { get; set; }

        /// <summary>
        /// Adres celu
        /// </summary>
        public IPAddress IP_Destination { get; set; }

        /// <summary>
        /// Na jaki port (routera) przeslać pakiet?
        /// </summary>
        public short Port { get; set; }

        /// <summary>
        /// Ile hopow pokona pakiet od brzegowego routera?
        /// </summary>
        public short hopsNumber { get; set; }

        public BorderNodeCommutationTableRow()
        {
            //empty
        }

        /// <summary>
        /// Konstruktor ze wszystkimi parametrami.
        /// </summary>
        /// <param name="IP_IN"></param>
        /// <param name="port_in"></param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <param name="modulationPerformance"></param>
        /// <param name="bitRate"></param>
        /// <param name="IP_Destination"></param>
        /// <param name="Port"></param>
        /// <param name="hopsNumber"></param>
        public BorderNodeCommutationTableRow(string IP_IN, short port_in, short band, short frequency,
            short modulationPerformance, short bitRate, string IP_Destination, short Port, short hopsNumber)
        {
            this.IP_IN = IPAddress.Parse(IP_IN);
            this.port_in = port_in;
            this.band = band;
            this.frequency = frequency;
            this.modulationPerformance = modulationPerformance;
            this.bitRate = bitRate;
            this.IP_Destination = IPAddress.Parse(IP_Destination);
            this.Port = Port;
            this.hopsNumber = hopsNumber;
        }
    }
}
