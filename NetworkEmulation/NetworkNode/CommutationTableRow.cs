using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Rząd tabeli kierowania pakietów dla węzłów NIEbrzegowych. 
    /// </summary>
    public class CommutationTableRow
    {
        /// <summary>
        /// Tunelem o jakiej częstotliwości przyszedł pakiet?
        /// Liczby naturalne od 0 do 63.
        /// </summary>
        public short frequency_in { get; set; }

        /// <summary>
        /// Port (numer łącza) z którego przychodzi pakiet
        /// </summary>
        public short port_in { get; set; }

        /// <summary>
        /// Częstotliwość, z którą wychodzi pakiet
        /// </summary>
        public short frequency_out { get; set; }

        /// <summary>
        /// Na jaki port skierować pakiet?
        /// </summary>
        public short port_out { get; set; }



        public CommutationTableRow()
        {
            //empty
        }

        /// <summary>
        /// Konstruktor wiersza tablicy komutacji
        /// </summary>
        /// <param name="frequency_in">Częstotliwość z którą przychodzi pakiet</param>
        /// <param name="port_in">Port (numer łącza) z którego przychodzi pakiet</param>
        /// <param name="frequency_out">Częstotliwość, z którą wychodzi pakiet</param>
        /// <param name="port_out">Port (numer łacza), którym wychodzi pakiet</param>
        public CommutationTableRow(short frequency_in, short port_in, short frequency_out, short port_out)
        {
            this.frequency_in = frequency_in;
            this.port_in = port_in;
            this.frequency_out = frequency_out;
            this.port_out = port_out;
        }
    }
}
