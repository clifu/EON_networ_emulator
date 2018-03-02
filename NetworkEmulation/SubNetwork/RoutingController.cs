using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AISDE;
using NetworkingTools;
using NetworkNode;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

namespace SubNetwork
{
    /// <summary>
    /// Klasaz funkcjonalnoscia RC.
    /// </summary>
    public class RoutingController
    {
        /// <summary>
        /// Numer RC z całej topologii
        /// </summary>
        public int numberRC;

        //Topologia sieci
        private Topology topology;

        /// <summary>
        /// Lista par: SNPP z najwyższym adresem IP podsieci, IP Routing Controllera z tej podsieci
        /// </summary>
        public List<SNandRC> SN_RCs;

        /// <summary>
        /// Do wysylania wiadomosci przez TCP pomiędzy komponentami g8080
        /// </summary>
        //public Socket sender;

        /// <summary>
        /// Do odbierania wiadomosci przez TCP pomiędzy komponentami g8080
        /// </summary>
       // public Socket receiver;

        /// <summary>
        /// Numer portu na kotrym wysyla i nasluchuje
        /// </summary>
        public int portNumber;

        public volatile IPEndPoint ipep;

        public volatile UdpClient newsock;

        //słownik przechowujący id i ip węzła w celu odnalezienia konkretnego węzła w liscie wezłów
        Dictionary<string, int> nodes_names = new Dictionary<string, int>();

        //Lista tablic zajetosci EONowej. Jeden wpis do listy to jeden interfejs EONowy danego routera.
        //public List<EONTable> EonTables;

        public RoutingController()
        {
            this.topology = new Topology();
            this.mySubNetwork = new SubNetwork();
            OtherSubNetworks = new List<SubNetwork>();
            SN_RCs = new List<SNandRC>();
            this.numberRC = 1;
            //sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //Odczytanie z managera konfiguracji adresu IP RC
            this.ip = IPAddress.Parse(ConfigurationManager.AppSettings["RC" + numberRC]);
            this.portNumber = 11000;
            ReadingFromNodesFile("RC" + numberRC + "ip.txt");
            ReadingFromPathFile("RC" + numberRC + "link.txt");
            ReadingFromSnAndRCsFile("RC" + numberRC + "SnAndRCs.txt");

            ipep = new IPEndPoint(this.ip, this.portNumber);
            newsock = new UdpClient(ipep);
            //TODO: Wyjatek ze nie mozna wykorzystac dwa razy tego samego socketa
            //this.EonTables = new List<EONTable>();
        }

        /// <summary>
        /// Konstruktor, określający numer RC z ogólnej topologii
        /// </summary>
        /// <param name="numberRc"></param>
        public RoutingController(int numberRc)
        {

            this.topology = new Topology();
            this.mySubNetwork = new SubNetwork();
            OtherSubNetworks = new List<SubNetwork>();
            SN_RCs = new List<SNandRC>();
            this.numberRC = numberRc;
            //sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //Odczytanie z managera konfiguracji adresu IP RC
            this.ip = IPAddress.Parse(ConfigurationManager.AppSettings["RC" + numberRC]);
            this.portNumber = 11000;
            ReadingFromNodesFile("RC" + numberRC + "ip.txt");
            ReadingFromPathFile("RC" + numberRC + "link.txt");
            ReadingFromSnAndRCsFile("RC" + numberRC + "SnAndRCs.txt");

            ipep = new IPEndPoint(this.ip, this.portNumber);
            newsock = new UdpClient(ipep);
        }

        public Topology GetTopology
        {
            get { return this.topology; }
            set { this.topology = value; }
        }

        /// <summary>
        /// Podsieć, w której znajduje się RC
        /// </summary>
        public SubNetwork mySubNetwork;

        /// <summary>
        /// Lista innych podsieci z topologii
        /// </summary>
        public List<SubNetwork> OtherSubNetworks;

        /// <summary>
        /// Adres IP tego RC
        /// </summary>
        public IPAddress ip;

        /// <summary>
        /// Wysyłanie wiadomości do 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="message"></param>
        private void SendMessage(string ipaddress, string message)
        {
            byte[] data = new byte[64];

            UdpClient newsock = new UdpClient();

            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ipaddress), this.portNumber);

            try
            {
                data = Encoding.ASCII.GetBytes(message);
                newsock.Send(data, data.Length, sender);

              //  writeLog("sending message: " + message + " to " + ipaddress);

                newsock.Close();
            }
            catch (Exception)
            {
                newsock.Close();
            }
        }

        /// <summary>
        /// Wysyla wiadomośc na konkretny port.
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="message"></param>
        /// <param name="portNumber"></param>
        private void SendMessage(string ipaddress, string message, int portNumber)
        {
            byte[] data = new byte[64];

            //IPEndPoint ip = new IPEndPoint(this.ip, 11001);
            UdpClient newsock = new UdpClient();

            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ipaddress), portNumber);

            try
            {
                data = Encoding.ASCII.GetBytes(message);
                newsock.Send(data, data.Length, sender);

               // writeLog("sending message: " + message + " to " + ipaddress);

                newsock.Close();
            }
            catch (Exception)
            {
                newsock.Close();
            }
        }

        /// <summary>
        /// Odbieranie wiadomości
        /// </summary>
        public void Run()
        {
            Task.Run(() =>
            {
                byte[] data = new byte[64];

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                //IPEndPoint sender = new IPEndPoint(IPAddress.Parse("xxx.xxx.xxx.xxx", 0);

                try
                {
                    while (true)
                    {
                        /*if (!receiver.IsBound)
                            receiver.Bind(sender);*/

                        //receiver.Receive(data, SocketFlags.None);
                        data = newsock.Receive(ref sender);

                        string receivedMesage = Encoding.ASCII.GetString(data);

                        char separator = '#';
                        string[] words = receivedMesage.Split(separator);

                      //  writeLog("received: " + receivedMesage + " from " + words[0]);

                        Task.Run(() => chooseAction(words));
                    }
                }
                catch (Exception E)
                {
                    //Console.WriteLine("RoutingController.Run(): " + E.Message);
                }
            });
        }

        /// <summary>
        /// Funkcja uruchamiajaca odpowiednie funkcje na podsawie otrzymanej wiadomości
        /// 
        /// </summary>
        /// <param name="words"></param>
        private void chooseAction(string[] words)
        {
            string message = words[1];

            if (message == MessageNames.LOCAL_TOPOLOGY)
            {
                //TODO: Co mowi LRM do RC
                handleLocalTopology(words);
            }
            else if (message == MessageNames.NETWORK_TOPOLOGY)
            {
                handleNetworkTopology(words);
            }
            else if (message == MessageNames.ROUTE_TABLE_QUERY)
            {
                handleRouteTableQuery(words);
            }
            else if (message == MessageNames.ROUTE_PATH)
            {
                handleRoutePath(words);
            }
            else if (message == MessageNames.GET_PATH)
            {
                handleGetPath(words);
            }
            else if (message == MessageNames.KILL_LINK)
            {
                handleKillLink(words);
            }
            else
            {

            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="words"></param>
        public void handleLocalTopology(string[] words)
        {
            try
            {
                string lrmIP = words[0];
                //ALLOCATION albo DEALLOCATION
                string action = words[2];
                string snppIP = words[3];
                int port = Int32.Parse(words[4]);
                string in_or_out = words[5];
                short band = Int16.Parse(words[6]);
                short frequency = Int16.Parse(words[7]);
                if (this.numberRC==5 )
                {
                    if (port!=7351 && port!=33351 && port!=3761)
                    {
                        writeLog(" received LocalTopology from LRM with IP: " + lrmIP +
                                 " with action: " + action + " ip = " + snppIP + "  port =" + port + ", "
                                 + in_or_out + " band=" + band + " frequency=" + frequency);
                    }
                }
                else
                {
                    writeLog(" received LocalTopology from LRM with IP: " + lrmIP +
                             " with action: " + action + " ip = " + snppIP + "  port =" + port + ", "
                             + in_or_out + " band=" + band + " frequency=" + frequency);
                }
                        
                if (action == "ALLOCATION")
                {
                    //Jezeli sie nie powiodło, to wypisujemy na ekran
                    if (!allocateLink(snppIP, port, in_or_out, band, frequency))
                    {
                        throw new Exception("Failed to allocate link: ip=" + snppIP + " port=" + port + ", " +
                            in_or_out + " band=" + band + " frequency=" + frequency);
                    }
                    if (this.numberRC == 5)
                    {
                        if (port != 7351 && port != 33351 && port != 3761)
                        {
                            writeLog(" allocated link: ip=" + snppIP + " port=" + port + ", " + in_or_out + " band=" + band + " frequency=" + frequency);
                        }
                    }
                    else
                    {
                        writeLog(" allocated link: ip=" + snppIP + " port=" + port + ", " + in_or_out + " band=" + band + " frequency=" + frequency);

                    }
                }
                else if (action == "DEALLOCATION")
                {
                    if (!deallocateLink(snppIP, port, in_or_out, band, frequency))
                    {
                        throw new Exception("Failed to deallocate link: ip=" + snppIP + " port=" + port + ", " + in_or_out + " band=" + band + " frequency=" + frequency);
                    }
                    if (this.numberRC == 5)
                    {
                        if (port != 7351 && port != 33351 && port != 3761)
                        {
                            writeLog("deallocated link: ip=" + snppIP + " port=" + port + ", " + in_or_out + " band=" + band + " frequency=" + frequency);
                        }
                    }
                    else
                    {
                        writeLog("deallocated link: ip=" + snppIP + " port=" + port + ", " + in_or_out + " band=" + band + " frequency=" + frequency);

                    }
                }
                else
                {
                    throw new Exception("Wrong action: " + action + "Failed to handle LocalTopology.");
                }
            }
            catch (Exception E)
            {
                if (E.Message.Contains("Failed to"))
                {
                    writeLog(E.Message);
                }
            }
            //TODO:
            /*

            Zatem LRM mówi MOJEIP#LOCAL_TOPOLOGY#KTÓRY_ROUTER#KTÓRY_PORT#IN_or_OUT#BAND#

             string localTopologyMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                           MessageNames.LOCAL_TOPOLOGY + "#" + "ALLOCATION" + "#" + routerAddress + "#" + routerPort + "#" + in_Or_Out + "#" + band + "#" + frequency;

             Zatem LRM mówi MOJEIP#LOCAL_TOPOLOGY#ALLOCATION#KTÓRY_ROUTER#KTÓRY_PORT#IN_or_OUT#BAND#

            
             string localTopologyMessage = ConfigurationManager.AppSettings["LRM" + numberLRM] + "#" +
                       MessageNames.LOCAL_TOPOLOGY + "#"+"DEALLOCATION"+"#" + routerAddress + "#" + routerPort + "#" + in_Or_Out + "#" + correctanceOfOperation + "#" + frequency;
             
             */
        }



        /// <summary>
        /// Obsługuje zapytanie NetworkTopology
        /// </summary>
        /// <param name="words"></param>
        public void handleNetworkTopology(string[] words)
        {
            //Czytanie wiadomosci
            string rcIP = words[0];
            string ipSource = words[2];
            string ipDestination = words[3];
            short band = Int16.Parse(words[4]);
            short frequency = Int16.Parse(words[5]);

            Wezel w1 = this.topology.network.wezly.Find(x => x.ip == ipSource);
            Wezel w2 = this.topology.network.wezly.Find(x => x.ip == ipDestination);

            writeLog(" received NetworkTopology from RC with IP: " + rcIP + " from source : " + ipSource + " to destiantion: "
                     + ipDestination + " with band: " + band + " and frequency: " +frequency);

            //uruchomienie algorytmu najkrotszej sciezki
            this.topology.runShortestPathAlgorithm();

            //wyznaczenie sciezki
            NetworkPath networkPath = this.topology.getNetworkPath(w1.SNPP, w2.SNPP, band, this.topology.network, frequency);

            //Dodanie do listy potencjalnych sciezek, o ile nie jest pusta
            if (networkPath.snpps.Count != 0)
            {
                this.topology.pathsCopy.Add(networkPath);
            }

            //wygenerowanie wiadomosci
            string message = this.generateNetworkTopologyRCesponse(networkPath.path);

            //wysylanie wiadomosci na porcie 11001
            SendMessage(rcIP, message, 11002);

            writeLog(" sending NetworkTopologyResponse to RC: " + rcIP + " with content: " + message);

        }

        /// <summary>
        /// Obsługuje zapytanie typu RouteTableQuery
        /// </summary>
        /// <param name="words"></param>
        public void handleRouteTableQuery(string[] words)
        {
            writeLog(" received " + MessageNames.ROUTE_TABLE_QUERY + " from CC with IP " + words[0]);
            string ccIP = words[0];
            string pathFrom = words[2];
            string pathTo = words[3];
            short bitRate = Int16.Parse(words[4]);
            short hopsNumber = Int16.Parse(words[5]);

            //Na koncu tego zapytania jest pusty string
            if (words.Length == 8)
            {
                short frequency = Int16.Parse(words[6]);
                determineAndSendPath(ccIP, pathFrom, pathTo, bitRate, hopsNumber, frequency, (short)(frequency + 1));
            }
            else
            {
                determineAndSendPath(ccIP, pathFrom, pathTo, bitRate, hopsNumber, 0, (short)EONTable.capacity);
            }
        }

        /// <summary>
        /// Próbuje wyznaczyć ścieżkę od częstotliwości początkowej do końcowej.
        /// </summary>
        /// <param name="pathFrom"></param>
        /// <param name="pathTo"></param>
        /// <param name="bitRate"></param>
        /// <param name="hopsNumber"></param>
        /// <param name="startFrequency"></param>
        /// <param name="endFrequency"></param>
        public void determineAndSendPath(string ccIP, string pathFrom, string pathTo, short bitRate, short hopsNumber, short startFrequency,
            short endFrequency)
        {
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(hopsNumber);
            short band = BorderNodeCommutationTable.determineBand(bitRate, modulationPerformance);

            //Odnalezienie wezlow poczatkowych i koncowych. Indeksy tez sie przydadza
            int wFromIndex = this.topology.network.wezly.FindIndex(x => x.ip.ToString() == pathFrom);
            int wToIndex = this.topology.network.wezly.FindIndex(x => x.ip.ToString() == pathTo);
            Wezel wFrom = this.topology.network.wezly[wFromIndex];
            Wezel wTo = this.topology.network.wezly[wToIndex];

            //Lista strukturek reprezentujących podsieci, do których trzeba się zgłosić
            List<SNandRC> sNandRcs = new List<SNandRC>();

            //Czy lista jest kompletna?
            bool sNandRcsComplete = false;

            NetworkPath networkPath = new NetworkPath();

            //Zapuszczenie algorytmu najkrotszej sciezki
            this.topology.runShortestPathAlgorithm();

            if (wFrom != null && wTo != null)
            {
                for (short i = startFrequency; i < endFrequency; i++)
                {
                    //Wyznaczenie nadścieżki
                    networkPath = topology.getNetworkPath(wFrom.SNPP,
                        wTo.SNPP, band, topology.network, i);

                    if (networkPath == null || networkPath.snpps.Count == 0)

                    {
                        continue;
                    }

                    //Uzupełnienie listy z wyszukanymi węzłami
                    if (!sNandRcsComplete)
                    {
                        foreach (var snpp in networkPath.snpps)
                        {
                            //Wyszukiwanie węzłów, które reprezentują podsieci i do których RC trzeba się zwrócić.
                            SNandRC sNandRc = SN_RCs.Find(x => x.snpp.snps[0].ipaddress.ToString() == snpp.snps[0].ipaddress.ToString());
                            if (sNandRc != null)
                                sNandRcs.Add(sNandRc);
                        }
                        sNandRcsComplete = true;
                    }

                    if (resolveSNPPS(ref networkPath, band, i))
                    {

                        writeLog(" sending RouteTableQueryResponse to CC with IP: " +ccIP);
                        SendMessage(ccIP, generateRouteTableQueryResponse(networkPath));
                        break;
                    }
                }

                //Dodanie do listy potencjalnych sciezek, o ile nie jest pusta
                if (networkPath != null)
                {
                    if (networkPath.snpps.Count != 0)
                        this.topology.pathsCopy.Add(networkPath);
                }
                else
                {
                    sendRouteTableQueryFailureMessage(ccIP, pathFrom, pathTo, band);
                }
            }
        }

        /// <summary>
        /// Funkcja obslugujaca zgloszenie ROUTE_PATH od jednego RC do drugiego RC
        /// </summary>
        /// <param name="words"></param>
        public void handleRoutePath(string[] words)
        {
            //Czytanie wiadomosci
            string rcIP = words[0];
            string ipSource = words[2];
            string ipDestination = words[3];
            short band = Int16.Parse(words[4]);
            short frequency = Int16.Parse(words[5]);

            writeLog(" received RoutePath from RC with IP: " + rcIP + " from source : " + ipSource + " to destiantion: "
                     + ipDestination + " with band: " + band + " and frequency: " + frequency);


            Wezel w1 = this.topology.network.wezly.Find(x => x.ip == ipSource);
            Wezel w2 = this.topology.network.wezly.Find(x => x.ip == ipDestination);

            /*
            //stworzenie snpp poczatkowego i koncowego
            SubNetworkPointPool snpp1 = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(ipSource)));
            SubNetworkPointPool snpp2 = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(ipDestination))); */

            //uruchomienie algorytmu najkrotszej sciezki
            this.topology.runShortestPathAlgorithm();

            //wyznaczenie sciezki
            NetworkPath networkPath = this.topology.getNetworkPath(w1.SNPP, w2.SNPP, band, this.topology.network, frequency);

            //Dodanie do listy potencjalnych sciezek, o ile nie jest pusta
            if (networkPath.snpps.Count != 0)
            {
                this.topology.pathsCopy.Add(networkPath);
                writeLog("Generated path from " + networkPath.path.WezlySciezki[0].ip + " to " + networkPath.path.WezlySciezki[networkPath.path.WezlySciezki.Count - 1].ip);

            }

            //wygenerowanie wiadomosci
            string message = this.generateGetPathFromRCResponse(networkPath.path);

            //zamiana na bajty
            //byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            //wysylanie wiadomosci na porcie 11001

            writeLog(" sending generated path in subnetwork to RC with IP: " + rcIP);
            SendMessage(rcIP, message, 11001);

            //wyslanie wiadomosci
            //this.sender.SendTo(messageBytes, new IPEndPoint(IPAddress.Parse(rcIP), this.portNumber));
        }

        /// <summary>
        /// Obsługuje zapytanie GET PATH wysyłane przez CC do RC. Zwraca ścieżkęz zapisanych ścieżek
        /// </summary>
        /// <param name="words"></param>
        public void handleGetPath(string[] words)
        {
            writeLog(" received " + MessageNames.GET_PATH + " from CC with IP: " + words[0]);
            string ccIP = words[0];
            string pathFrom = words[2];
            string pathTo = words[3];

            //Odnalezienie ścieżki
            NetworkPath networkPath = FindPath(pathFrom, pathTo);

            writeLog(" sending " + MessageNames.GET_PATH + "RESPONSE to CC " + ccIP);
            //Wysłanie wiadomości
            SendMessage(ccIP, generateGetPathResponse(networkPath));
        }

        /// <summary>
        /// Wpisuje nieskonczony koszt do krawedzi o konkretnym ID
        /// </summary>
        /// <param name="words"></param>
        public void handleKillLink(string[] words)
        {
            int linkID = Int32.Parse(words[2]);
            int linkIndex = this.topology.network.krawedzie.FindIndex(x => x.idKrawedzi == linkID);

            if (linkIndex != -1)
            {
                this.topology.network.krawedzie.RemoveAt(linkIndex);
                this.topology.network.ustawPoczatkoweKoszty = true;
                this.topology.pathsCopy.RemoveAll(x =>
                    x.path.KrawedzieSciezki.FindIndex(y => y.idKrawedzi == linkID) != -1);
                /*
                int snp1Index = topology.network.krawedzie[linkIndex].Wezel1.SNPP.snps
                    .FindIndex(x => x.portOUT == linkID);
                int snp2Index = topology.network.krawedzie[linkIndex].Wezel2.SNPP.snps
                    .FindIndex(x => x.portIN == linkID);

                if (snp1Index != -1 && snp2Index != -1)
                {
                    //Całkowite zapchanie portów łącza
                    for (short i = 0; i < EONTable.capacity; i++)
                    {
                        //Dodanie wpisu do tabeli eonowej na wyjściu węzła 1
                        topology.network.krawedzie[linkIndex].Wezel1.SNPP.snps[snp1Index].eonTable
                            .addRow(new EONTableRowOut(i, 1));
                        //Dodanie wpisu do tabeli eonowej na wejściu węzła 2
                        topology.network.krawedzie[linkIndex].Wezel2.SNPP.snps[snp2Index].eonTable
                            .addRow(new EONTableRowIN(i, 1));
                    }

                    //Zmiana kosztów łączy
                    topology.network.krawedzie[linkIndex].changeCost();
                    */
                writeLog(" killed a link no. " + words[2]);
            }
            else
            {
                writeLog(" failed to kill a link no. " + words[23]);
            }
        }



        /// <summary>
        /// Dopisuje na początek wiadomości timestamp i nr RC, zmienia kolory na konsoli.
        /// </summary>
        /// <param name="message">Wiadomość do wypisania</param>
        public void writeLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[" + Timestamp.generateTimestampRC() + "]" + " RC" + this.numberRC + " " + message);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void writeLogWithPath(string message, string stringPath)
        {
            var words = stringPath.Split('#');
            List<string> links = new List<string>();
            List<string> nodes = new List<string>();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(Timestamp.generateTimestampRC() + " RC" + this.numberRC + " " + message);
            for (int j = 0; j < words.Length; j++)
            {
                //Nieparzyste
                if (((double)j / 2) != 0)
                {
                    nodes.Add(words[j]);
                }
                else
                {
                    links.Add(words[j]);
                }
            }

            int i = 0;
            Wezel temp = new Wezel();
            if (words.Length == 0 && words[0] == "")
            {
                Console.WriteLine("Empty path");
            }
            else
            {
                while (i < words.Length - 1)
                {
                    Console.WriteLine($"from {words[i]}");
                    Console.WriteLine($" link {words[i + 1]}");
                    i = i + 2;
                }
                Console.Write($"end node {words[i]}");
                //TODO
            }

            Console.Write("");
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void writeLogWithPathRequest()
        {

        }

        /// <summary>
        /// Odnajduje ścieżkę z listy zapisanych ścieżek. Zwraca nulla gdy nie uda się znaleźć
        /// </summary>
        /// <param name="pathFrom"></param>
        /// <param name="pathTo"></param>
        public NetworkPath FindPath(string pathFrom, string pathTo)
        {
            //Wyszukiwanie ścieżki z listy kopii
            return this.topology.pathsCopy.Find(x =>
                x.snpps[0].snps[0].ipaddress.ToString() == pathFrom && x.snpps[x.snpps.Count - 1].snps[0].ipaddress.ToString() == pathTo);
        }


        /// <summary>
        /// Generuje odpowiedź na zapytanie GET PATH od CC
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string generateGetPathResponse(NetworkPath networkPath)
        {
            if (networkPath != null)
                return $"{this.ip}#{MessageNames.GET_PATH}#{networkPath.path.frequency}#{networkPath.path.band}#{PathToString(networkPath.path)}#";
            else
            {
                return $"{this.ip}#{MessageNames.GET_PATH}#";
            }
        }

        /// <summary>
        /// Usuwa konkretny wpis z tablicy eonowej.
        /// </summary>
        /// <param name="ip">IP węzła, którego mamy usunąć wpis.</param>
        /// <param name="linkID">Numer portu z SubNetworkPoint.</param>
        /// <param name="in_or_out">"in" albo "out"</param>
        /// <param name="band"></param>
        /// <returns></returns>
        public bool deallocateLink(string ip, int linkID, string in_or_out, short band, short frequency)
        {
            //Wyszukiwanie indeksu węzła, którego snpp jest takie własnie
            int nodeIndex = this.topology.network.wezly.FindIndex(x => x.ip == ip);

            int linkIndex = this.topology.network.krawedzie.FindIndex(y => y.idKrawedzi == linkID);

            int snpIndex;

            if (in_or_out == "in" || in_or_out == "IN")
            {
                snpIndex = this.topology.network.wezly[nodeIndex].SNPP.snps.FindIndex(x => x.portIN == linkID);

                //Usuniecie wpisu z tablicy eonowej
                bool returnValue = topology.network.wezly[nodeIndex].SNPP.snps[snpIndex].eonTable
                    .deleteRow(new EONTableRowIN(frequency, band));

                this.topology.network.krawedzie[linkIndex].updateCost(frequency, band);

                return returnValue;
            }
            else if (in_or_out == "out" || in_or_out == "OUT")
            {
                snpIndex = this.topology.network.wezly[nodeIndex].SNPP.snps.FindIndex(x => x.portOUT == linkID);

                //Usuniecie wpisu z tablicy eonowej
                bool returnValue = topology.network.wezly[nodeIndex].SNPP.snps[snpIndex].eonTable
                    .deleteRow(new EONTableRowOut(frequency, band));

                this.topology.network.krawedzie[linkIndex].updateCost(frequency, band);

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        public bool allocateLink(string ip, int linkID, string in_or_out, short band, short frequency)
        {
            //Wyszukiwanie indeksu węzła, którego snpp jest takie własnie
            int nodeIndex = this.topology.network.wezly.FindIndex(x => x.ip == ip);

            int linkIndex = this.topology.network.krawedzie.FindIndex(y => y.idKrawedzi == linkID);

            int snpIndex;

            if (in_or_out == "in" || in_or_out == "IN")
            {
                snpIndex = this.topology.network.wezly[nodeIndex].SNPP.snps.FindIndex(x => x.portIN == linkID);

                //Dodanie wpisu do tablicy eonowej
                bool returnValue = topology.network.wezly[nodeIndex].SNPP.snps[snpIndex].eonTable
                    .addRow(new EONTableRowIN(frequency, band));

                this.topology.network.krawedzie[linkIndex].updateCost(frequency, band);

                return returnValue;
            }
            else if (in_or_out == "out" || in_or_out == "OUT")
            {
                snpIndex = this.topology.network.wezly[nodeIndex].SNPP.snps.FindIndex(x => x.portOUT == linkID);

                //Dodanie wpisu do tablicy eonowej
                bool returnValue = topology.network.wezly[nodeIndex].SNPP.snps[snpIndex].eonTable
                    .addRow(new EONTableRowOut(frequency, band));

                this.topology.network.krawedzie[linkIndex].updateCost(frequency, band);

                return returnValue;
            }
            else
            {
                return false;
            }
        }


        public bool resolveSNPPS(ref NetworkPath networkPath, short band, short frequency)
        {
            //topology.getNetworkPath();
            foreach (var snpp in networkPath.snpps)
            {
                //if (canIGoOnAtThisFrequency)
                //{
                SNandRC sNandRc = SN_RCs.Find(x => x.snpp.snps[0].ipaddress.ToString() == snpp.snps[0].ipaddress.ToString());

                if (sNandRc != null)
                {
                    //IP węzła modelującego podsieć
                    string oldNodeIP = sNandRc.snpp.snps[0].ipaddress.ToString();

                    //Łącze dochodzące do węzła modelującego podsieć
                    int previousLink = networkPath.path.KrawedzieSciezki.FindIndex(x =>
                        x.Wezel2.ip == oldNodeIP);

                    //Łącze wychodzące z węzła modelującego podsieć
                    int nextLink = networkPath.path.KrawedzieSciezki.FindIndex(x =>
                        x.Wezel1.ip == oldNodeIP);

                    Wezel w1 = networkPath.path.zwroc_ListaKrawedziSciezki[previousLink].Wezel1;
                    Wezel w2 = networkPath.path.zwroc_ListaKrawedziSciezki[nextLink].Wezel2;

                    SubNetworkPointPool SNPPFrom = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w1.ip)));
                    SubNetworkPointPool SNPPTo = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w2.ip)));

                    // writeLog("Resolving path from " + SNPPFrom.snps[0].ipaddress + " to " + SNPPTo.snps[0].ipaddress);

                    //Czesciowa sciezka, którą trzeba dodać do nadścieżki
                    var partOfThePath = this.getPathFromRC(sNandRc.RC_IP.ToString(), SNPPFrom, SNPPTo, band,
                        frequency);

                    //Sciezka jest pusta
                    if (partOfThePath == null || partOfThePath.snpps.Count == 0)
                    {
                        //canIGoOnAtThisFrequency = false;
                        return false;
                    }
                    //Ścieżka jest niepusta
                    else
                    {
                        //insertSubPath(ref networkPath, partOfThePath, previousLink, nextLink);
                        return true;
                    }
                }
            }
            //}

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public string generateRouteTableQueryResponse(NetworkPath networkPath)
        {
            return this.ip + "#" + MessageNames.ROUTE_TABLE_QUERY + "#"
                 + networkPath.path.frequency + "#" + networkPath.path.band + "#" + PathToString(networkPath.path) + "#";
        }

        /// <summary>
        /// Wstawia podsciezke do glownej sciezki
        /// </summary>
        /// <param name="networkPath"></param>
        /// <param name="partialNetworkPath"></param>
        /// <param name="previousLink"></param>
        /// <param name="nextLink"></param>
        public void insertSubPath(ref NetworkPath networkPath, NetworkPath partialNetworkPath, int previousLink, int nextLink)
        {
            List<Lacze> links1 = new List<Lacze>();

            //Kopia elementów od 0 do previousLink-tego elementu
            links1.AddRange(networkPath.path.KrawedzieSciezki.GetRange(0, previousLink));

            List<Lacze> links2 = new List<Lacze>();

            //Kopia elementów od nextLink + 1 aż do końca
            links2.AddRange(networkPath.path.KrawedzieSciezki.GetRange(nextLink + 1,
                networkPath.path.KrawedzieSciezki.Count - nextLink - 1));

            networkPath.path.KrawedzieSciezki = new List<Lacze>();
            networkPath.path.KrawedzieSciezki.AddRange(links1);
            networkPath.path.KrawedzieSciezki.AddRange(partialNetworkPath.path.KrawedzieSciezki);
            networkPath.path.KrawedzieSciezki.AddRange(links2);

            //Aktualizacja wezlow
            networkPath.path.aktualizujWezly();

            //Aktualizacja SNPPow
            networkPath.actualizeSNPPs();
        }

        public void sendRouteTableQueryFailureMessage(string requestorIP, string pathFrom, string pathTo, short band)
        {
            //Wysylamy pusta sciezke
            string message = this.ip + "#" + MessageNames.ROUTE_TABLE_QUERY + "#" + pathFrom + "#" +
                             pathTo + "#" + band; //TODO zmien 'band' na przeplywnosc czy coś takiego 

            //Wyslanie wiaomości do elementu sterującego, który prosił o wyznaczenie ścieżki, że sie jej nie da wyznaczyć.
            SendMessage(requestorIP, message);
        }

        /// <summary>
        /// Generuje sciezke od jednego RC do drugiego RC
        /// </summary>
        /// <param name="rc_ip"></param>
        /// <param name="Source"></param>
        /// <param name="Destnation"></param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public NetworkPath NetworkTopologyRequest(string rc_ip, SubNetworkPointPool Source,
            SubNetworkPointPool Destnation, short band, short frequency)
        {
            try
            {
                //Generowanie wiadomosci
                string message = generateNetworkTopologyMessage(Source, Destnation, band, frequency);

                //Zamiana jej na tablice bajtow
                byte[] data = Encoding.ASCII.GetBytes(message);

                //Tworzenie IPEndPointa z rc, do którego wysyłamy prośbę o wyznaczenie ścieżki. Nasluchiwanie na porcie 11001
                IPEndPoint rcEndPoint = new IPEndPoint(this.ip, 11002);
                IPEndPoint myEndpoint = new IPEndPoint(IPAddress.Any, 0);

                UdpClient client = new UdpClient(rcEndPoint);
                //Polaczenie z endpointem
                //if (!this.sender.IsBound)
                //this.sender.Bind(rcEndPoint);

                //Wyslanie zapytania do RC o IP okreslonym przez rc_ip
                SendMessage(rc_ip, message);

                //alokacja bajtow do odbierania
                byte[] receivedBytes = new byte[0];

                receivedBytes = client.Receive(ref myEndpoint);

                client.Close();

                //Zmiana jej na stringa
                string receivedMesage = Encoding.ASCII.GetString(receivedBytes);

                //Tworzenie obiektu klasy NetworkPath ze sciezka wpisana 
                NetworkPath networkPath = readNetworkTopologyResponse(receivedMesage, rc_ip);

                //Przypisanie wartosci pasma i czestotliwosci do sciezki
                networkPath.path.band = band;
                networkPath.path.frequency = frequency;

                return networkPath;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
                return null;
            }

            return null;
        }

        /// <summary>
        /// Wyznacza ścieżkę i wolną częstotliwość w ścieżce. W razie niepowodzenia zwraca pustą ścieżkę.
        /// </summary>
        /// <param name="rc_ip">IP RC, do którego RC zwraca się o wyznaczenie ścieżki.</param>
        /// <param name="Source">Węzeł przed węzłem modelującym podsieć.</param>
        /// <param name="Destnation">Węzeł po węźle modelującym podsieć.</param>
        /// <param name="band">Ilość szczelin.</param>
        /// <returns></returns>
        /*[Obsolete]
        public NetworkPath TryGetPathFromSNMPAtFrequencies(string rc_ip, Wezel nodeFrom,
            Wezel nodeTo, short band, ref NetworkPath upperNetworkPath)
        {
            SubNetworkPointPool SNPPFrom = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(nodeFrom.ip)));
            SubNetworkPointPool SNPPTo = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(nodeTo.ip)));

            //RC po kolei próbuje otrzymać jakaś ścieżkę 
            for (short i = 0; i < EONTable.capacity; i++)
            {
                if (nodeFrom.eonTable.OutFrequencies[i] == -1 && nodeTo.eonTable.InFrequencies[i] == -1)
                {
                    upperNetworkPath = this.topology.getNetworkPath(SNPPFrom, SNPPTo, band, this.topology.network, i);

                    if (upperNetworkPath != null || upperNetworkPath.snpps.Count != 0)
                    {
                        //Próba wyznaczenia i-tej ścieżki 
                        var path = this.getPathFromRC(rc_ip, SNPPFrom, SNPPTo, band, i);

                        if (path != null || path.snpps.Count != 0)
                            return path;
                    }
                }
                else
                {
                    continue;
                }
            }
            //Zwracanie pustej sciezki
            return new NetworkPath();

        }*/

        /// <summary>
        /// Generuje sciezke od jednego RC do drugiego RC
        /// </summary>
        /// <param name="rc_ip"></param>
        /// <param name="Source"></param>
        /// <param name="Destnation"></param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public NetworkPath getPathFromRC(string rc_ip, SubNetworkPointPool Source,
            SubNetworkPointPool Destnation, short band, short frequency)
        {
            try
            {
                //Generowanie wiadomosci
                string message = generateGetPathFromRCMessage(Source, Destnation, band, frequency);

                //Zamiana jej na tablice bajtow
                byte[] data = Encoding.ASCII.GetBytes(message);

                //Tworzenie IPEndPointa z rc, do którego wysyłamy prośbę o wyznaczenie ścieżki. Nasluchiwanie na porcie 11001
                IPEndPoint rcEndPoint = new IPEndPoint(this.ip, 11001);
                IPEndPoint myEndpoint = new IPEndPoint(IPAddress.Any, 0);

                UdpClient client = new UdpClient(rcEndPoint);
                //Polaczenie z endpointem
                //if (!this.sender.IsBound)
                //this.sender.Bind(rcEndPoint);

                //TODO: Wyskakuje blad, ze tylko jedno uzycie gniazda sieciowego jest dozwolone

                //Wyslanie zapytania do RC o IP okreslonym przez rc_ip
                SendMessage(rc_ip, message);
                //this.sender.SendTo(data, rcEndPoint);

                //alokacja bajtow do odbierania
                byte[] receivedBytes = new byte[0];

                //if (!this.receiver.IsBound)
                //this.receiver.Bind(rcEndPoint);


                receivedBytes = client.Receive(ref myEndpoint);

                client.Close();


                //Odebranie wiadomości od tamtego RC
                // UdpClient udpClient = new UdpClient(rcEndPoint);
                //receivedBytes = udpClient.Receive(ref rcEndPoint);
                //this.receiver.ReceiveFrom(receivedBytes, SocketFlags.None, ref rcendPoint);

                //this.receiver.ReceiveFrom(receivedBytes, ref rcEndPoint);

                //Zmiana jej na stringa
                string receivedMesage = Encoding.ASCII.GetString(receivedBytes);

                //Tworzenie obiektu klasy NetworkPath ze sciezka wpisana 
                NetworkPath networkPath = readGetPathFromRCResponse(receivedMesage, rc_ip);

                //Przypisanie wartosci pasma i czestotliwosci do sciezki
                networkPath.path.band = band;
                networkPath.path.frequency = frequency;

                return networkPath;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
                return null;
            }

            return null;
        }

        /// <summary>
        /// Generuje zapytanie do innego RC o wyznaczenie sciezki od Source do Destination przy dostepnym pasmie band
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Destination"></param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public string generateGetPathFromRCMessage(SubNetworkPointPool Source, SubNetworkPointPool Destination, short band, short frequency)
        {
            return this.ip + "#" + MessageNames.ROUTE_PATH + "#" + Source.snps[0].ipaddress + "#" +
                   Destination.snps[0].ipaddress + "#" + band + "#" + frequency;
        }

        /// <summary>
        /// Generuje odpowiedź na żądanie wyznaczenia ścieżki
        /// </summary>
        /// <param name="pathString"></param>
        /// <returns></returns>
        public string generateGetPathFromRCResponse(Sciezka path)
        {
            //Parsowanie sciezki do stringa
            string pathString = PathToString(path);

            //Stworzenie wiadomosci
            return this.ip + "#" + MessageNames.ROUTED_PATH + "#" + pathString;
            //TODO przetestuj
        }

        /// <summary>
        /// Generuje sciezke NetworkPath na podstawie otrzymanej wiadomosci
        /// </summary>
        /// <param name="message"></param>
        /// <param name="rcIP"></param>
        /// <returns></returns>
        public NetworkPath readGetPathFromRCResponse(string message, string rcIP)
        {
            var words = message.Split('#');

            //Jezeli otrzymalismy wiadomosc od dobrego RC, jezeli jest to odpowiedz na ROUTE_PATH i jezeli sciezka jest niepusta
            if (words[1] == MessageNames.ROUTED_PATH && words[0] == rcIP && words[2] != "")
            {
                StringBuilder SB = new StringBuilder();

                //TODO przetestuj
                //Wyciaganie stringa ze sciezka z wiadomosci
                string pathString = message.Substring(words[0].Length + 1 + words[1].Length + 1);

                //Nowa sciezka
                NetworkPath networkPath = new NetworkPath(stringToPath(pathString));

                return networkPath;
            }
            else
            {
                //zwroc pusta sciezke
                return new NetworkPath();
            }
        }

        /// <summary>
        /// Tworzy zapytanie Network Topology. 
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Destination">IP klienta końcowego.</param>
        /// <param name="band"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public string generateNetworkTopologyMessage(SubNetworkPointPool Source, SubNetworkPointPool Destination, short band, short frequency)
        {
            return this.ip + "#" + MessageNames.NETWORK_TOPOLOGY + "#" + Source.snps[0].ipaddress + "#" +
                   Destination.snps[0].ipaddress + "#" + band + "#" + frequency;
        }

        /// <summary>
        /// Generuje odpowiedź na żądanie wyznaczenia ścieżki przez RC z innej domeny.
        /// </summary>
        /// <param name="pathString"></param>
        /// <returns></returns>
        public string generateNetworkTopologyRCesponse(Sciezka path)
        {
            //Parsowanie sciezki do stringa
            string pathString = PathToString(path);

            //Stworzenie wiadomosci
            return this.ip + "#" + MessageNames.NETWORK_TOPOLOGY_RESPONSE + "#" + pathString;
            //TODO przetestuj
        }

        /// <summary>
        /// Generuje sciezke NetworkPath na podstawie otrzymanej wiadomosci
        /// </summary>
        /// <param name="message"></param>
        /// <param name="rcIP"></param>
        /// <returns></returns>
        public NetworkPath readNetworkTopologyResponse(string message, string rcIP)
        {
            var words = message.Split('#');

            //Jezeli otrzymalismy wiadomosc od dobrego RC, jezeli jest to odpowiedz na ROUTE_PATH i jezeli sciezka jest niepusta
            if (words[1] == MessageNames.NETWORK_TOPOLOGY_RESPONSE && words[0] == rcIP && words[2] != "")
            {
                StringBuilder SB = new StringBuilder();

                //TODO przetestuj
                //Wyciaganie stringa ze sciezka z wiadomosci
                string pathString = message.Substring(words[0].Length + 1 + words[1].Length + 1);

                //Nowa sciezka
                NetworkPath networkPath = new NetworkPath(stringToPath(pathString));

                return networkPath;
            }
            else
            {
                //zwroc pusta sciezke
                return new NetworkPath();
            }
        }

        /// <summary>
        /// Zamienia sekwencję SNPPów na wiadomośc do wysłania 
        /// </summary>
        /// <param name="snpps"></param>
        /// <returns></returns>
        [Obsolete]
        public string snppsToString(List<SubNetworkPointPool> snpps)
        {
            //Wyjsciowy string
            StringBuilder SB = new StringBuilder();
            SB.Append(snpps[0].snps[0].ipaddress.ToString());
            SB.Append("#");
            SB.Append(snpps[0].snps[0].portOUT.ToString());
            SB.Append("#");

            for (int i = 1; i < snpps.Count - 1; i++)
            {
                SB.Append(snpps[i].snps[0].portIN.ToString());
                SB.Append("#");
                SB.Append(snpps[i].snps[0].ipaddress.ToString());
                SB.Append("#");
                SB.Append(snpps[i].snps[0].portOUT.ToString());
                SB.Append("#");
            }

            SB.Append(snpps[snpps.Count - 1].snps[0].portIN.ToString());
            SB.Append("#");
            SB.Append(snpps[snpps.Count - 1].snps[0].ipaddress.ToString());

            return SB.ToString();
        }

        /// <summary>
        /// Zamienia stringa ze sciezka na obiekt klasy Sciezka.
        /// </summary>
        /// <param name="stringPath"></param>
        /// <returns></returns>
        public static Sciezka stringToPath(string stringPath)
        {
            if (stringPath == "")
            {
                return new Sciezka();
            }

            var words = stringPath.Split('#');

            Wezel w1 = new Wezel(-1, words[0]);
            Sciezka sc = new Sciezka();

            //Dodanie pierwszego wezla
            sc.Wezel1 = w1;
            sc.WezlySciezki.Add(w1);

            for (int i = 1; i < words.Length; i = i + 2)
            {
                //Wezel drugi z lacza
                Wezel W2 = new Wezel(-1, words[i + 1]);
                sc.WezlySciezki.Add(W2);

                //Dodanie nowej krawedzi 
                sc.KrawedzieSciezki.Add(new Lacze(Int32.Parse(words[i]), sc.WezlySciezki[sc.WezlySciezki.Count - 2], W2,
                    1));
            }

            //Ostatni wezel
            sc.Wezel2 = sc.WezlySciezki[sc.WezlySciezki.Count - 1];

            return sc;
        }


        /// <summary>
        /// Funkcja, generujaca stringa w postaci IPseparatorIPseparatorIP ze sciezki
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PathToString(Sciezka path)
        {
            StringBuilder SB = new StringBuilder();
            string separator = "#";
            if (path.KrawedzieSciezki.Count <= 0)
            {
                return "";
            }
            else
            {
                SB.Append(path.KrawedzieSciezki[0].Wezel1.ip);

                foreach (Lacze link in path.KrawedzieSciezki)
                {
                    SB.Append(separator);
                    SB.Append(link.idKrawedzi);
                    SB.Append(separator);
                    SB.Append(link.Wezel2.ip);
                }

                return SB.ToString();
            }
        }


        public void ReadingFromPathFile(string path)
        {
            string line;
            char[] delimiterChars = { '#' };
            string[] words;
            int id1, id2, pathID, index1, index2;

            try
            {
                using (StreamReader file = new StreamReader(path))
                {

                    while ((line = file.ReadLine()) != null)
                    {
                        words = line.Split(delimiterChars);
                        id1 = Int32.Parse(words[1]);
                        id2 = Int32.Parse(words[2]);
                        pathID = Int32.Parse(words[0]);
                        index1 = nodes_names.Values.ToList().IndexOf(id1);
                        index2 = nodes_names.Values.ToList().IndexOf(id2);

                        Wezel w1 = this.topology.network.wezly.Find(x => x.idWezla == id1);
                        Wezel w2 = this.topology.network.wezly.Find(x => x.idWezla == id2);

                        Lacze lacze = new Lacze(pathID, w1, w2);
                        this.topology.network.krawedzie.Add(lacze);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read from file");
            }
        }

        public void ReadingFromNodesFile(string path)
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
                        int id = Int32.Parse(words[1]);
                        Wezel w = new Wezel(id, words[0]);
                        this.topology.network.wezly.Add(w);
                        nodes_names.Add(words[0], id);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read from file");
            }
        }

        public void ReadingFromSnAndRCsFile(string path)
        {
            string line;
            char[] delimiterChars = { '#' };
            string[] words;
            SubNetworkPointPool snpp;
            SNandRC sNandRc;
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        words = line.Split(delimiterChars);
                        sNandRc = new SNandRC(words[1], words[0]);
                        this.SN_RCs.Add(sNandRc);
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
