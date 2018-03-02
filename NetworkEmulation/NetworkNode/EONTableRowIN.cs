using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Klasa z rzedem tabeli zajetych pasm na wejsciu routera.
    /// </summary>
    public class EONTableRowIN
    {
        /// <summary>
        /// Na jakiej częstotliwości zaczyna się pasmo?
        /// </summary>
        public short busyFrequency { get; set; }

        /// <summary>
        /// Zajęte pasmo na wejściu węzła.
        /// </summary>
        public short busyBandIN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="busyFrequency"></param>
        /// <param name="busyBandIn"></param>
        public EONTableRowIN(short busyFrequency, short busyBandIn )
        {
            this.busyBandIN = busyBandIn;
            this.busyFrequency = busyFrequency;
        }
    }
}
