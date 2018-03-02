using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NetworkNode
{
    /// <summary>
    /// Tablica komutacji routera, także brzegowego.
    /// </summary>
    public class CommutationTable
    {
        public List<CommutationTableRow> Table { get; set; }

        public CommutationTable()
        {
            this.Table = new List<CommutationTableRow>();
        }

        /// <summary>
        /// Konstruktor z referencja na tablice komutacji routera
        /// </summary>
        /// <param name="row"></param>
        public CommutationTable(List<CommutationTableRow> table)
        {
            this.Table = table;
        }

        /// <summary>
        /// Funkcja służąca do zamiany nagłówków w wiadomościach zawartych w kolejce, gdy wiadomości pochodzą od klienta.
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
        /// Funkcja służąca do zamiany nagłówka 
        /// </summary>
        /// <param name="message">Wiadomość odebrana w postaci tablicy bajtów</param>
        /// <returns></returns>
        public byte[] changePackageHeader(byte[] message)
        {
            byte[] msg = new byte[64];
            msg = message;
            Package p = new Package(msg);

            CommutationTableRow row = new CommutationTableRow();
            row = this.FindRow(p.frequency, p.portNumber);

            p.changePort(row.port_out);
            if (row.frequency_out == -1)
            {
                p.changeFrequency(row.frequency_out);
                p.changeBand(-1);
                p.changeModulationPerformance(-1);
                p.changeBitRate(-1);
            }

            msg = p.toBytes();

            return msg;
        }

        /// <summary>
        /// Funkcja odpowiedzialna za znalezienie wiersza w tablicy komutacji
        /// </summary>
        /// <param name="frequency_in">Częstotliwość wejściowa</param>
        /// <param name="port_in">Port wejsciowy</param>
        /// <returns>Wiersz tablicy komutacji</returns>
        public CommutationTableRow FindRow(short frequency_in, short port_in)
        {
            //Jeden rzad tablicy komutacji 
            CommutationTableRow commutationRow = null;

            try
            {
                //Wyszukiwanie takiego rzedu, ktory ma podane IP_IN i port_in
                commutationRow = this.Table.Find(row => (row.frequency_in == frequency_in) && (row.port_in == port_in));

                return commutationRow;
            }
            catch (Exception E)
            {
                return commutationRow;
            }

        }

        /// <summary>
        /// Funkcja znajdująca odpowiedni indeks, na którym znajduje się dany wiersz tablicy komutacji
        /// </summary>
        /// <param name="frequency_in">Częstotliwosć wejsciowa</param>
        /// <param name="port_in">Port wejsciowy</param>
        /// <returns>Indeks wiersza</returns>
        public int FindIndex(short frequency_in, short port_in)
        {
            int index;
            try
            {
                index = this.Table.FindIndex(row => (row.frequency_in == frequency_in) && (row.port_in == port_in));
                return index;
            }
            catch (Exception E)
            {
                return -1;
            }
        }

    }
}
