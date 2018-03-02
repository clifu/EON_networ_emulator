using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using NetworkingTools;


namespace SubNetwork
{
    public struct StoreLambda
    {
        public short frequency
        { get; set; }
        public string token { get; set; }
    }

    public struct Directory
    {
        public string adres { get; set; }
        public string link { get; set; }
    }

    class LinkResourceManager
    {
        public NetworkNode.EONTable eonTable;

        public string numberLRM;

        List<SubNetworkPoint> snp;
        List<StoreLambda> listOfFrequency;
        string addressResponse;
        string tokenResponse;

        public LinkResourceManager(string numberLRM)
        {
            this.numberLRM = numberLRM;
            eonTable = new NetworkNode.EONTable();
            snp = new List<SubNetworkPoint>();
            listOfFrequency = new List<StoreLambda>();
            readingSNP();
        }

        private void SendingMessage(string ipaddress, string message)
        {
            byte[] data = new byte[64];

            UdpClient newsock = new UdpClient();

            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ipaddress), 11000);

            try
            {
                data = Encoding.ASCII.GetBytes(message);
                newsock.Send(data, data.Length, sender);

            }
            catch (Exception ex)
            {


            }

            newsock.Close();

        }

        /// <summary>
        /// Odbieranie wiadomości
        /// </summary>
        public void ReceivedMessage()
        {

            byte[] data = new byte[64];
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string ipaddress;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["LRM" + numberLRM]), 11000);
            UdpClient newsock = new UdpClient(ipep);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            try
            {

                while (true)
                {
                    data = newsock.Receive(ref sender);

                    string receivedMesage = Encoding.ASCII.GetString(data);

                    char separator = '#';
                    string[] words = receivedMesage.Split(separator);
                    Task.Run(() => chooiceAction(words));
                }

            }
            catch (Exception)
            {

            }


        }
        /// <summary>
        /// Wykonujemy operacje w zależności od żądania przychodzącego od CC
        /// </summary>
        /// <param name="message"></param>
        private void chooiceAction(string[] message)
        {
            string action = message[1];
            string address = message[0];
            //string token = message[2];

            if (action == MessageNames.LINK_CONNECTION_REQUEST)
            {

                allocationHandling(message, MessageNames.LINK_CONNECTION_REQUEST);
            }
            else if (action == MessageNames.LINK_CONNECTION_DEALLOCATION)
            {
                dealocationHandling(message, MessageNames.LINK_CONNECTION_DEALLOCATION);
            }
            else if (action == MessageNames.SNP_RELEASE + "DEALLOCATION")
            {
                dealocationHandling(message, MessageNames.SNP_RELEASE + "DEALLOCATIONRESPONSE");
            }
            else if (action == MessageNames.SNP_RELEASE)
            {
                allocationHandling(message, MessageNames.SNP_RELEASE + "RESPONSE");

            }
            else if (action == MessageNames.SNP_RELEASE + "RESPONSE")
            {
                string code = message[3];
                string token = message[2];
                if (code == "OK")
                {
                    //generateLogReceived("LRM", MessageNames.SNP_RELEASE + "RESPONSE OK",message[0]);
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           MessageNames.LINK_CONNECTION_REQUEST + "#" + token + "#" + "OK" + "#";
                    generateLogSend("CC", MessageNames.LINK_CONNECTION_REQUEST, addressResponse);
                    SendingMessage(addressResponse, responseMessage);

                }
                else if (code == "BAD")
                {
                    //  generateLogReceived("LRM", MessageNames.SNP_RELEASE + "RESPONSE BAD", message[0]);
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           MessageNames.LINK_CONNECTION_REQUEST + "#" + token + "#" + "BAD" + "#";
                    generateLogSend("CC", MessageNames.LINK_CONNECTION_REQUEST, addressResponse);
                    SendingMessage(addressResponse, responseMessage);

                }
            }
            else if (action == MessageNames.SNP_RELEASE + "DEALLOCATIONRESPONSE")
            {
                string code = message[3];
                string token = message[2];
                if (code == "OK")
                {
                    generateLogReceived("LRM", MessageNames.LINK_CONNECTION_DEALLOCATION + " OK", message[0]);
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           MessageNames.LINK_CONNECTION_DEALLOCATION + "#" + token + "#" + "OK" + "#";
                    generateLogSend("CC", MessageNames.LINK_CONNECTION_DEALLOCATION, addressResponse);
                    SendingMessage(addressResponse, responseMessage);

                }
                else if (code == "BAD")
                {
                    generateLogReceived("LRM", MessageNames.LINK_CONNECTION_DEALLOCATION + " BAD", message[0]);
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           MessageNames.LINK_CONNECTION_DEALLOCATION + "#" + token + "#" + "BAD" + "#";
                    generateLogSend("CC", MessageNames.LINK_CONNECTION_DEALLOCATION, addressResponse);
                    SendingMessage(addressResponse, responseMessage);

                }
            }
        }


        public void killLink(string text)
        {
            int numberOfLink;
            string[] words = text.Split(' ');
            try
            {
                numberOfLink = Int32.Parse(words[1]);

                string messageKillLink = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" + MessageNames.KILL_LINK + "#" + numberOfLink + "#";
                generateLogKillLink("RC", MessageNames.KILL_LINK, ConfigurationManager.AppSettings["RC" + numberLRM], numberOfLink);
                SendingMessage(ConfigurationManager.AppSettings["RC" + numberLRM], messageKillLink);

                generateLogKillLink("CC", MessageNames.KILL_LINK, ConfigurationManager.AppSettings["CC" + numberLRM], numberOfLink);
                SendingMessage(ConfigurationManager.AppSettings["CC" + numberLRM], messageKillLink);

            }
            catch (Exception)
            {
                Console.WriteLine("Bad data format");

            }
        }

        /// <summary>
        /// Zczytywanie adresow SNPow z pliku konfiguracyjnego
        /// </summary>
        public void readingSNP()
        {
            int numberOfSNP = Int32.Parse(ConfigurationManager.AppSettings[numberLRM + "SNPCount"]);
            for (int i = 0; i < numberOfSNP; i++)
            {
                string[] words = ConfigurationManager.AppSettings[numberLRM + "SNP" + (i + 1)].Split('#');
                snp.Add(new SubNetworkPoint(IPAddress.Parse(words[0]), Int32.Parse(words[1]), Int32.Parse(words[2])));
            }
        }

        /// <summary>
        /// Dodajemy częstotliwość
        /// </summary>
        /// <param name="frequencyTmp"></param>
        /// <param name="band"></param>
        /// <param name="index"></param>
        /// <param name="in_Or_Out"></param>
        /// <returns></returns>
        private bool addFrequency(string frequencyTmp, short band, int index, string in_Or_Out)
        {

            if (in_Or_Out == "IN")
                return snp[index].eonTable.addRow(new NetworkNode.EONTableRowIN(Int16.Parse(frequencyTmp), band));
            else
                return snp[index].eonTable.addRow(new NetworkNode.EONTableRowOut(Int16.Parse(frequencyTmp), band));

        }
        /// <summary>
        /// Funkcja sprawdza dostępność częstotliwości w tabeli EON
        /// </summary>
        /// <param name="band">Wymagana liczba szczelin na zestawienie połączenia</param>
        /// <returns>zwraca index częstotliwości na której możemy zestawić połącznie, przy braku takiej możliwości -1</returns>
        public short checkAvaliability(short band)
        {
            bool flag = true;
            for (short i = 0; i < eonTable.InFrequencies.Count; i++)
            {
                for (int j = i; j < i + band; j++)
                {
                    if (eonTable.InFrequencies[j] != (-1))
                    {
                        flag = false;
                    }
                }
                if (flag == true)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void dealocationHandling(string[] message, string function)
        {
            generateLogReceived("CC", function, message[0], message[8]);
            string address = message[0];
            short band = Int16.Parse(message[2]);
            string token = message[3];
            string hops = message[4];
            string in_Or_Out = message[5];
            string routerAddress = message[6];
            string routerPort = message[7];
            int index;
            string frequencyTmp = message[8];
            //pobieramy index SNPa, który ma parametry zgodne z obecnie analizowanym
            //Ze wzglęgu na różne numery łaczy w zależności od tego czy jest to wejście czy wyjście musmy rozróżnić to za pomoca if
            if (in_Or_Out == "IN")
            {
                index = snp.FindIndex(x => (x.ipaddress.ToString() == routerAddress) && (x.portIN == Int32.Parse(routerPort)));
            }
            else
            {
                index = snp.FindIndex(x => (x.ipaddress.ToString() == routerAddress) && (x.portOUT == Int32.Parse(routerPort)));
            }

            //Probujemy zarezerwowac czestotliwosc w danym routerze
            //Jest to jednoczesnie liczba szczelin zwolnionych
            if (index != (-1))
            {
                short correctanceOfOperation = snp[index].eonTable.deleteRowWithFrequency(Int16.Parse(frequencyTmp), in_Or_Out);
                //Jezeli rezerwacja przebiegla pomyslnie to dajemy znac RC i odpowiadamy potwierdzeniem do CC
                if (correctanceOfOperation != (-1))
                {

                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           function + "#" + token + "#" + "OK" + "#";
                    if (!function.StartsWith(MessageNames.SNP_RELEASE))
                    {
                        generateLogSend("CC", function, address);
                    }
                    SendingMessage(address, responseMessage);

                    string localTopologyMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                              MessageNames.LOCAL_TOPOLOGY + "#" + "DEALLOCATION" + "#" + translateLink(routerAddress, routerPort, in_Or_Out).adres + "#" + translateLink(routerAddress, routerPort, in_Or_Out).link + "#" + in_Or_Out + "#" + band + "#" + frequencyTmp + "#";
                    if (!function.StartsWith(MessageNames.SNP_RELEASE))
                    {
                        generateLogSend("RC", MessageNames.LOCAL_TOPOLOGY, ConfigurationManager.AppSettings["RC" + numberLRM]);
                    }
                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberLRM], localTopologyMessage);

                }
                else//Jeżeli nie powiodło się to odsyłamy do CC wiadomosc o błędzie
                {
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           function + "#" + token + "#" + "BAD" + "#";
                    generateLogSend("CC", function, address);
                    SendingMessage(address, responseMessage);

                }
            }
            else
            {
                addressResponse = address;
                tokenResponse = token;
                string messageToLRMIN = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                    MessageNames.SNP_RELEASE + "DEALLOCATION" + "#" + band + "#" + token + "#" + hops +
                                     "#" + in_Or_Out + "#" + routerAddress + "#" + routerPort + "#" + frequencyTmp + "#";
                //generateLogSend("LRM", MessageNames.SNP_RELEASE,
                // ConfigurationManager.AppSettings["LRMDomain" + numberLRM]);
                SendingMessage(ConfigurationManager.AppSettings["LRMDomain" + numberLRM], messageToLRMIN);



            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="function"></param>
        private void allocationHandling(string[] message, string function)
        {
            if(!function.StartsWith(MessageNames.SNP_RELEASE))
            generateLogReceived("CC", function, message[0], message[8]);
            string address = message[0];
            short band = Int16.Parse(message[2]);
            string token = message[3];
            string hops = message[4];
            string in_Or_Out = message[5];
            string routerAddress = message[6];
            string routerPort = message[7];
            int index;
            string frequencyTmp = message[8];
            //pobieramy index SNPa, który ma parametry zgodne z obecnie analizowanym
            //Ze wzglęgu na różne numery łaczy w zależności od tego czy jest to wejście czy wyjście musmy rozróżnić to za pomoca if
            if (in_Or_Out == "IN")
            {
                index = snp.FindIndex(x => (x.ipaddress.ToString() == message[6]) && (x.portIN == Int32.Parse(message[7])));
            }
            else
            {
                index = snp.FindIndex(x => (x.ipaddress.ToString() == message[6]) && (x.portOUT == Int32.Parse(message[7])));
            }


            int objectTmp = listOfFrequency.FindIndex(x => x.token == token);

            if (index != (-1))
            {
                //Probujemy zarezerwowac czestotliwosc w danym routerze 
                bool correctanceOfOperation = addFrequency(frequencyTmp, band, index, in_Or_Out);
                //Jezeli rezerwacja przebiegla pomyslnie to dajemy znac RC i odpowiadamy potwierdzeniem do CC
                if (correctanceOfOperation)
                {
                    if (!function.StartsWith(MessageNames.SNP_RELEASE))
                    {
                        Console.WriteLine("[" + Timestamp.generateTimestampLRM() + "] LRM " +
                                          translateLink(routerAddress, routerPort, in_Or_Out).link + " allocated band=" + band + " and frequency=" + frequencyTmp);
                    }
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           function + "#" + token + "#" + "OK" + "#";
                    if (!function.StartsWith(MessageNames.SNP_RELEASE))
                    {
                        generateLogSend("CC", function, address);
                    }
                    SendingMessage(address, responseMessage);

                    string localTopologyMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                              MessageNames.LOCAL_TOPOLOGY + "#" + "ALLOCATION" + "#" + translateLink(routerAddress, routerPort, in_Or_Out).adres + "#" + translateLink(routerAddress, routerPort, in_Or_Out).link + "#" + in_Or_Out + "#" + band + "#" + frequencyTmp + "#";
                    if (!function.StartsWith(MessageNames.SNP_RELEASE))
                    {
                        generateLogSend("RC", MessageNames.LOCAL_TOPOLOGY, ConfigurationManager.AppSettings["RC" + numberLRM]);
                    }
                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberLRM], localTopologyMessage);

                }
                else//Jeżeli nie powiodło się to odsyłamy do CC wiadomosc o błędzie
                {
                    string responseMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                           function + "#" + token + "#" + "BAD" + "#";
                    generateLogSend("CC", function, address);
                    SendingMessage(address, responseMessage);

                }
            }
            else
            {
                addressResponse = address;
                tokenResponse = token;
                string messageToLRMIN = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                                    MessageNames.SNP_RELEASE + "#" + band + "#" + token + "#" + hops +
                                     "#" + in_Or_Out + "#" + routerAddress + "#" + routerPort + "#" + frequencyTmp + "#";
                // generateLogSend("LRM", MessageNames.SNP_RELEASE,
                // ConfigurationManager.AppSettings["LRMDomain" + numberLRM]);
                SendingMessage(ConfigurationManager.AppSettings["LRMDomain" + numberLRM], messageToLRMIN);



            }
        }

        public Directory translateLink(string address, string port, string in_or_out)
        {
            if (address == "127.0.0.31" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.31";
                dr.link = "3172";
                return dr;
            }
            else if (address == "127.0.0.31" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.31";
                dr.link = "3171";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "3171";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "472";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "471";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "3172";
                return dr;
            }
            else if (address == "127.0.0.31" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.31";
                dr.link = "2312";
                return dr;
            }
            else if (address == "127.0.0.31" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.31";
                dr.link = "2311";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "3571";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "3572";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "4" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "7332";
                return dr;

            }
            else if (address == "127.0.0.5" && port == "4" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "7331";
                return dr;

            }
            else if (address == "127.0.0.33" && port == "4" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.33";
                dr.link = "7332";
                return dr;
            }
            else if (address == "127.0.0.33" && port == "4" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.33";
                dr.link = "7331";
                return dr;
            }
            else if (address == "127.0.0.33" && port == "3" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.33";
                dr.link = "33352";
                return dr;
            }
            else if (address == "127.0.0.33" && port == "3" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.33";
                dr.link = "33351";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.3";
                dr.link = "352";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "3" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.3";
                dr.link = "351";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.5";
                dr.link = "352";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "3" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.5";
                dr.link = "351";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "3" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.5";
                dr.link = "572";
                return dr;
            }
            else if (address == "127.0.0.5" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.5";
                dr.link = "571";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "571";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "3" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "572";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "371";
                return dr;
            }
            else if (address == "127.0.0.7" && port == "3" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.7";
                dr.link = "372";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "3" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.3";
                dr.link = "372";
                return dr;
            }
            else if (address == "127.0.0.3" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.3";
                dr.link = "371";
                return dr;
            }
            else if (address == "127.0.0.35" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.35";
                dr.link = "7351";
                return dr;
            }
            else if (address == "127.0.0.35" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.35";
                dr.link = "7352";
                return dr;
            }
            else if (address == "127.0.0.35" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.35";
                dr.link = "35372";
                return dr;
            }
            else if (address == "127.0.0.35" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.35";
                dr.link = "35371";
                return dr;
            }

            else if (address == "127.0.0.37" && port == "2" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.37";
                dr.link = "35371";
                return dr;
            }
            else if (address == "127.0.0.37" && port == "2" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.37";
                dr.link = "35372";
                return dr;
            }
            else if (address == "127.0.0.37" && port == "1" && in_or_out == "IN")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.37";
                dr.link = "3762";
                return dr;
            }
            else if (address == "127.0.0.37" && port == "1" && in_or_out == "OUT")
            {
                Directory dr = new Directory();
                dr.adres = "127.0.0.37";
                dr.link = "3761";
                return dr;
            }

            return new Directory();

        }
        public static void generateLogReceived(string nameRemoteObject, string function, string address, string frequency)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[" + Timestamp.generateTimestampLRM() + "]" + " LRM received message " + function + " from " +
                              nameRemoteObject + " with IP " + address + " Connection on frequency: " + frequency);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static void generateLogReceived(string nameRemoteObject, string function, string address)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[" + Timestamp.generateTimestampLRM() + "]" + " LRM received message " + function + " from " +
                              nameRemoteObject + " with IP " + address);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static void generateLogSend(string nameRemoteObject, string function, string address)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[" + Timestamp.generateTimestampLRM() + "]" + " LRM send message " + function + " to " +
                              nameRemoteObject + " IP:" + address);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void generateLogKillLink(string nameRemoteObject, string function, string address, int link)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[" + Timestamp.generateTimestampLRM() + "]" + " LRM send message " + function + " to " +
                              nameRemoteObject + " IP:" + address + " link: " + link);
            Console.ForegroundColor = ConsoleColor.Black;
        }


    }
}
