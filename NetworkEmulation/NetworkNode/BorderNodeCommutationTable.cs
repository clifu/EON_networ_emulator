using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Klasa z tabelą komutacji węzłów brzegowych.
    /// TODO: Jak dodajesz do listy nowy wiersz, to musisz sprawdzic, czy juz nie istnieje podobny wpis (z takim 
    /// TODO: samym IP_IN i port_in)
    /// </summary>
    public class BorderNodeCommutationTable
    {
        /// <summary>
        /// Tabela komutacji węzłów brzegowych
        /// </summary>
        public List<BorderNodeCommutationTableRow> Table { get; set; }

        public BorderNodeCommutationTable()
        {
            Table = new List<BorderNodeCommutationTableRow>();
        }

        /// <summary>
        /// Funkcja obliczajaca efektywnosc modulacji na podstawie ilosci hopow do celu, jaka modulacje ma uzyc.
        /// </summary>
        /// <param name="hopsNumber"></param>
        public static short determineModulationPerformance(short hopsNumber)
        {
            try
            {
                if (hopsNumber <= 0)
                    throw new Exception("hopsNumber is " + hopsNumber);
                if (hopsNumber <= 8)
                    return (short)(9 - hopsNumber);
                else
                    return 1;
            }
            //Jak sie cos nie uda to bierzemy modulacje o najmniejszej wydajnosci (N)
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
                return 1;
            }
        }

        /// <summary>
        /// Funkcja obliczajaca potrzebne pasmo przy danej przeplynowsci bitowej i wydajnosci modulacji
        /// </summary>
        /// <param name="bitRate"></param>
        /// <param name="modulationPerformance"></param>
        /// <returns></returns>
        public static short determineBand(short bitRate, short modulationPerformance)
        {
            //Maksymalna wydajnosc widmowa modulacji to 8bit/s/Hz
            short maxLinkSpectrumPerformance = 8;

            short band;

            //Jak wartosciowosc modulacji nie przekracza maksymalnej wydajnosci widmowej lacza
            if (modulationPerformance < maxLinkSpectrumPerformance)
                band = (short)Math.Ceiling((double)bitRate / modulationPerformance);
            else
                //A jak przekracza, to dzielimy przez maksymalna wydajmosc widmowa lacza
                band = (short)Math.Ceiling((double)bitRate / maxLinkSpectrumPerformance);

            return band;
        }

        /// <summary>
        /// Funkcja odnajdujaca rzad, w ktorym jest okreslone IP i nr portu
        /// </summary>
        /// <param name="IP_IN"></param>
        /// <param name="port_in"></param>
        /// <returns></returns>
        public BorderNodeCommutationTableRow FindRow(string IP_IN, short port_in, string IP_Destination)
        {
            //Jeden rzad tablicy komutacji 
            BorderNodeCommutationTableRow borderRow = null;

            try
            {
                IPAddress IP = IPAddress.Parse(IP_IN);

                //Wyszukiwanie takiego rzedu, ktory ma podane IP_IN i port_in
                borderRow = this.Table.Find(row => (row.IP_IN.ToString() == IP_IN) && (row.port_in == port_in) && (row.IP_Destination.ToString() == IP_Destination));

                return borderRow;
            }
            catch (Exception E)
            {
                return borderRow;
            }

        }

        public BorderNodeCommutationTableRow FindRow(string IP_IN,  string IP_Destination, short frequency)
        {
            //Jeden rzad tablicy komutacji 
            BorderNodeCommutationTableRow borderRow = null;

            try
            {
                IPAddress IP = IPAddress.Parse(IP_IN);

                //Wyszukiwanie takiego rzedu, ktory ma podane IP_IN i czestotliwosc
                borderRow = this.Table.Find(row => (row.IP_IN.ToString() == IP_IN) && (row.frequency == frequency) && (row.IP_Destination.ToString() == IP_Destination));

                return borderRow;
            }
            catch (Exception E)
            {
                return borderRow;
            }

        }

        /// <summary>
        /// Funkcja służąca do zamiany nagłówków w wiadomościach zawartych w kolejne; Wiadomości pochodzą od klienta.
        /// </summary>
        /// <param name="q">Kolejka tablic bajtów</param>
        /// <returns></returns>
        public Queue<byte[]> changeHeaderForMessagesFromClient(Queue<byte[]> q)
        {
            Queue<byte[]> tempQueue = new Queue<byte[]>();
            var tmp = q.ToArray();
            int max = q.Count();
            for (int i = 0; i < max; i++)
            {
                tmp[i] = this.changePackageHeader(tmp[i]);
            }

            for (int i = 0; i < max; i++)
            {
                tempQueue.Enqueue(tmp[i]);
            }

            return tempQueue;
        }

        /// <summary>
        /// Funkcja słuząca do zamiany nagłówka 
        /// </summary>
        /// <param name="message">Wiadomość odebrana w postaci tablicy bajtów</param>
        /// <returns></returns>
        public byte[] changePackageHeader(byte[] message)
        {
            byte[] msg = new byte[64];
            msg = message;
            Package p = new Package(msg);

            BorderNodeCommutationTableRow row = new BorderNodeCommutationTableRow();
            
            row = this.FindRow(p.IP_Source.ToString(), p.portNumber, p.IP_Destination.ToString());

            p.changeBand(row.band);
            p.changeFrequency(row.frequency);
            p.changeModulationPerformance(row.modulationPerformance);
            p.changeBitRate(row.bitRate);
            p.changePort(row.Port);

            msg = p.toBytes();

            return msg;
        }
        
        /// <summary>
        /// Funkcja słuząca do zamiany nagłówka, uwzgledniajaca "-1" w czestotliwosci
        /// </summary>
        /// <param name="message">Wiadomość odebrana w postaci tablicy bajtów</param>
        /// <returns></returns>
        public byte[] changePackageHeader2(byte[] message, ref CommutationField commutationField)
        {
            byte[] msg = new byte[64];
            msg = message;
            Package p = new Package(msg);

            //Jak czestotliwosc jest rowna -1
            if(p.frequency == -1)
            {
                BorderNodeCommutationTableRow row = new BorderNodeCommutationTableRow();

                row = this.FindRow(p.IP_Source.ToString(), p.portNumber, p.IP_Destination.ToString());

                //Gdy udalo sie znalezc wpis w tabeli, podmien pola naglowka
                if (row != null)
                {
                    p.changeBand(row.band);
                    p.changeFrequency(row.frequency);
                    p.changeModulationPerformance(row.modulationPerformance);
                    p.changeBitRate(row.bitRate);
                   // p.changePort(row.Port);
                }
                //Jak sie nie udalo, to wpisz -2 do czestotliwosci
                else
                {
                    //-2 oznacza "upusc pakiet"
                    p.changeFrequency(-2);
                }

                msg = p.toBytes();
            }
            //Jak nie bylo -1, to odwolujemy sie do tabeli komutacji, ale nie brzegowej
            
                msg = commutationField.commutationTable.changePackageHeader(msg);
            


            return msg;
        }

        /// <summary>
        /// Funkcja odnajdujaca indeks rzedu o okreslony IP i porcie
        /// </summary>
        /// <param name="IP_IN"></param>
        /// <param name="port_in"></param>
        /// <returns></returns>
        public int FindIndex(string IP_IN, short port_in, string IP_Destination)
        {
            int index;
            try
            {
                index = this.Table.FindIndex(r => (r.IP_IN.ToString() == IP_IN) && (r.port_in == port_in) && (r.IP_Destination.ToString() == IP_Destination));
                return index;
            }
            catch (Exception E)
            {
                return -1;
            }
        }
    }


}
