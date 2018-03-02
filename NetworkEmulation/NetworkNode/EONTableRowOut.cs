using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Klasa z rzedem tabeli zajetych pasm na wyjsciu routera.
    /// </summary>
    public class EONTableRowOut
    {
        /// <summary>
        /// Na jakiej częstotliwości zaczyna się pasmo?
        /// </summary>
        public short busyFrequency { get; set; }

        /// <summary>
        /// Zajęte pasmo na wyjściu węzła.
        /// </summary>
        public short busyBandOUT { get; set; }

        public EONTableRowOut(short busyFrequency, short busyBandOut)
        {
            this.busyBandOUT = busyBandOut;
            this.busyFrequency = busyFrequency;
        }
    }
}
