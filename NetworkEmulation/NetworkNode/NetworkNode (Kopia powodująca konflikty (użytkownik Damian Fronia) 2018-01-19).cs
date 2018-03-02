using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;
using NetworkCalbleCloud;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace NetworkNode
{
    /// <summary>
    /// Klasa reprezentująca węzeł sieciowy
    /// </summary>
    public class NetworkNode : CableCloud
    {
        public delegate Socket SocketDelegate(Socket socket);


        public static SocketDelegate sd;


        private NameValueCollection mySettings = System.Configuration.ConfigurationManager.AppSettings;


        private SocketListener sl = new SocketListener();


        private SocketSending sS = new SocketSending();


        private static CancellationTokenSource _cts = new CancellationTokenSource();


        private static List<Socket> socketListenerList = new List<Socket>();


        private static List<Socket> socketSendingList = new List<Socket>();


        private static List<Data> tmp = new List<Data>();


        private readonly object _syncRoot = new object();


        public static byte[] msg;


        private static short portNumber;

        private static bool Listening = true;

        private static bool Last = true;

        private string numberOfRouter;

        public string NumberOfRouter { get { return numberOfRouter; } }

        /// <summary>
        /// Tablica z zajetymi pasmami
        /// </summary>
        public EONTable eonTable;

        /// <summary>
        /// Tablica komutacji - dla wszystkich rodzajow wezlow sieciowych
        /// </summary>
        public CommutationTable commutationTable;

        /// <summary>
        /// Tablica komutacji dla wezlow sieciowych na brzegu sieci. Najpierw router zaglada w pakiet
        /// i sprawdza, czy jest tam jakas czestotliwosc tunelu. Jak nie ma, to wlasnie w tej tablicy
        /// bedzie napisane, co dalej robic. Jak jest - to zwykla commutationTable.
        /// </summary>
        public BorderNodeCommutationTable borderNodeCommutationTable;

        /// <summary>
        /// Pole komutacyjne wraz z buforami wejsciowym i wyjsciowymi.
        /// </summary>
        public volatile CommutationField commutationField;

        private OperationConfiguration oc = new OperationConfiguration();

        public static object ConfigurationManager { get; private set; }

        /// <summary>
        /// Czy mozna wyczyscic bufory?
        /// </summary>
        public volatile bool canIClearMyBuffers = false;

        /// <summary>
        /// Czy mozna zerowac licznik?
        /// </summary>
        public volatile bool zeroTimer = false;

        /// <summary>
        /// Czy mozna komutowac pakiety?
        /// </summary>
        public volatile bool canICommutePackets;

        public bool CanICommutePackets
        {
            get { return canICommutePackets; }
            set
            {
                canICommutePackets = value;
                if (canICommutePackets == true)
                {
                    Task.Run(async () => await commutePackets());
                }
            }
        }

        public bool CanIClearMyBuffers
        {
            get { return canIClearMyBuffers; }
            set
            {
                canIClearMyBuffers = value;
                if (canIClearMyBuffers == true)
                {
                    Task.Run(async () => await sendPackage(socketSendingList.ElementAt(0)));
                }
            }
        }


        public NetworkNode()
        {
            this.commutationTable = new CommutationTable();
            this.borderNodeCommutationTable = new BorderNodeCommutationTable();
            this.eonTable = new EONTable();
            this.commutationField = new CommutationField(ref borderNodeCommutationTable, ref commutationTable, ref eonTable, 1);

        }

        /// <summary>
        /// Wydostaje z pakietu, na jaki adres IP i nr portu w chmurze przeslac pakiet
        /// </summary>
        /// <param name="packageBytes"></param>
        /// <returns></returns>
        public Tuple<short, short> determineFrequencyAndPort(byte[] packageBytes)
        {
            try
            {
                short frequency = Package.extractFrequency(packageBytes);

                //Znajdz taki rzad, dla ktorego wartosc czestotliwosci jest rowna czestotliwosci wejsciowej.
                var row = commutationTable.Table.Find(r => r.frequency_in == frequency);

                return new Tuple<short, short>(row.frequency_out, row.port_out);
            }
            catch (Exception E)
            {
                Console.WriteLine("NetworkNode.determineCloudSocketIPAndPort(): failed to get " +
                                  "Cloud's socket IP and port. " + E.Message);
                return null;
            }
        }

        public void Run()
        {

            // sd = new SocketDelegate(CallbackSocket);

            Console.WriteLine("ID router:");
            numberOfRouter = Console.ReadLine().ToString();


            //Uruchomienie agenta
            ManagmentAgent agent = new ManagmentAgent();
            if (numberOfRouter == "2")
            {
                   Task.Run(() => agent.Run(numberOfRouter, ref commutationTable, ref borderNodeCommutationTable, ref eonTable));
            }

            Task.Run(() => CC());

            //pobranie wlasnosci zapisanych w pliku konfiguracyjnym
            tmp = OperationConfiguration.ReadAllSettings(mySettings);


            //przeszukanie wszystkich kluczy w liscie 
            foreach (var key in tmp)
            {

                if (key.Keysettings.StartsWith(numberOfRouter + "Sending"))
                {
                    //Uruchamiamy watek na kazdym z tworzonych sluchaczy
                    //Task.Run(()=>
                    CreateConnect2(key.SettingsValue, key.Keysettings);//);
                }
            }
            /*  //jezeli klucz zaczyna sie od TableFrom to uzupelniamy liste
              else if (key.Keysettings.StartsWith("TableFrom"))
                  tableFrom.Add(key.SettingsValue);

              //jezeli klucz zaczyna sie od TableTo to uzupelniamy liste
              else if (key.Keysettings.StartsWith("TableTo"))
                  tableTo.Add(key.SettingsValue);*/
            ConsoleKeyInfo cki;

            //Petla wykonuje sie poki nie nacisniemy klawisza "esc"
            while (true)
            {
                cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Escape)
                {
                    _cts.Cancel();
                    break;
                }
            }


        }


        public void CC()
        {
            byte[] data = new byte[64];
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string ipaddress;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(System.Configuration.ConfigurationManager.AppSettings["UDP" + numberOfRouter]), 11000);
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
                   Task.Run(()=> setRouting(words));
                }


            }
            catch (Exception )
            {

            }

        }

        private void SendingMessageCC(string ipaddress, string message)
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

        }

        private void setRouting(string [] message)
        {
            

            string address = message[0];
            if (message[1] == MessageNames.CONNECTION_TEARDOWN) 
            {
                if (message[3] == "CC")
                {
                    short frequency = Int16.Parse(message[10]);
                    short port_In = Int16.Parse(message[5]);
                    short port_Out = Int16.Parse(message[7]);



                    CommutationTableRow rowToDelete = new CommutationTableRow();
                    rowToDelete = commutationTable.FindRow(Convert.ToInt16(message[7]), Convert.ToInt16(message[5]));
                    commutationTable.Table.Remove(rowToDelete);

                    string responseMessage = System.Configuration.ConfigurationManager.AppSettings["UDP" + numberOfRouter] + "#" + MessageNames.CONNECTION_TEARDOWN + "#OK" + "#" + "CC#";
                    SendingMessageCC(address, responseMessage);
                    Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Message  delete from table  {0} ", "COMUTATION");
                }
                else if (message[3] == "BORDERTABLE")
                {
                    string responseMessage = System.Configuration.ConfigurationManager.AppSettings["UDP" + numberOfRouter] + "#" + MessageNames.CONNECTION_TEARDOWN + "#OK" + "#" + "CC#";
                    

                    BorderNodeCommutationTableRow newRow = new BorderNodeCommutationTableRow();
                    //IP source--IP destination--czestotliwosc
                    newRow = borderNodeCommutationTable.FindRow(message[4], message[6], Int16.Parse(message[7]));
                    borderNodeCommutationTable.Table.Remove(newRow);
                    SendingMessageCC(address, responseMessage);

                    Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Message  delete from table  {0} ", "BORDER_NODE_COMUTATION");


                }
            }else if(message[1]==MessageNames.CONNECTION_REQUEST)
            {
                              
                    if (message[3] == "CC")
                    {
                        Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Received message filling Commutation Table  from CC " + message[0]);

                    short frequency = Int16.Parse(message[10]);
                        short port_In = Int16.Parse(message[5]);
                        short port_Out = Int16.Parse(message[7]);



                        CommutationTableRow commuteRow = new CommutationTableRow(frequency, port_In, frequency, port_Out);
                        commutationTable.Table.Add(commuteRow);
                   
                    string responseMessage = System.Configuration.ConfigurationManager.AppSettings["UDP" + numberOfRouter] + "#" +MessageNames.CONNECTION_REQUEST + "#OK" + "#" + "CC#";
                        SendingMessageCC(address, responseMessage);
                        Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Message  new registry to table  {0} ", "COMUTATION_TABLE"+
                         " Frequency: " + message[10] + " port_in: " + message[5] + " port_out: " + message[7]);
                    }
                    else if (message[3] == "BORDERTABLE")
                    {
                        Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Received message filling BorderNodeCommutationTable  from CC " + message[0]);
                    string responseMessage = System.Configuration.ConfigurationManager.AppSettings["UDP" + numberOfRouter] + "#" + MessageNames.CONNECTION_REQUEST + "#OK" + "#" + "CC#";
                    

                    BorderNodeCommutationTableRow newRow = new BorderNodeCommutationTableRow(
                                          message[4], Convert.ToInt16(message[5]), Convert.ToInt16(1), Convert.ToInt16(message[7]), Convert.ToInt16(7),
                                          Convert.ToInt16(1), message[6], Convert.ToInt16(2), Convert.ToInt16(1));
                        borderNodeCommutationTable.Table.Add(newRow);
                        SendingMessageCC(address, responseMessage);
                    Console.WriteLine("[" + Timestamp.generateTimestamp() + "] Message  new registry to table  {0} ", "BORDER_NODE_COMUTATION "+
                                                                                                                      "Frequency: " + message[7] + " port_in " + message[5]);           

                    }
                }

        }

        /// <summary>
        /// Zeby zuzycie procesora nie bylo 95% jak uruchamiam wezly sieciowe, 
        /// to niech kazdy task w petli while czeka czas podany na argumencie.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task<bool> waitABit(int time)
        {
            //Czekamy iles tam czasu
            await Task.Delay(time);
            //zwracamy prawde - mozesz kontynuowac tasku
            return true;
        }

        /// <summary>
        /// Odbieranie wiadomosci
        /// </summary>
        /// <param name="socketClient"></param>
        public async Task receiveMessage(Socket socketClient)
        {
            bool canIContinue = true;
            byte[] msg;
            while (Listening && canIContinue)
            {

                //czekamy iles milisekund
                canIContinue = await waitABit(100);

                //Console.WriteLine("receiveMessage()");

                if (canIContinue)
                {
                    //Odebranie tablicy bajtow na obslugiwanym w watku sluchaczu
                    msg = sl.ProcessRecivedBytes(socketClient);

                    if (msg != null)
                    {
                        //Gdy przyszla jakas wiadomosc, mozna zaczac komutowac pakiety


                        //wyswietlenie informacji na konsoli
                        //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                        //                " (receiveMessage)canICommutePackets= " + canICommutePackets);

                        stateReceivedMessageFromCableCloud(msg, socketClient);

                        if (commutationField.bufferIn.queue.Count >= commutationField.maxBuffInSize)
                        {
                            //canICommutePackets = true;
                            //Stary kolor konsoli
                            var color = Console.ForegroundColor;

                            //Ustawienie nowego koloru konsoli
                            Console.ForegroundColor = ConsoleColor.Cyan;

                            Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                                              "(receiveMessage) Dropped package " +
                                              Package.extractID(msg) +
                                              " number " + Package.extractPackageNumber(msg) + " of " +
                                              Package.extractHowManyPackages(msg));

                            //Przywrocenie starego koloru konsoli
                            Console.ForegroundColor = color;
                        }
                        else
                        {
                            //Dodanie do bufora wejsciowego wiadomosci, ktora przyszla
                            commutationField.bufferIn.queue.Enqueue(msg);

                            CanICommutePackets = true;

                            Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                                              " Package enqueued to the IN buffer (buffer size = " +
                                              commutationField.bufferIn.queue.Count + ") " + Package.extractID(msg) +
                                              " number " + Package.extractPackageNumber(msg) + " of " +
                                              Package.extractHowManyPackages(msg));

                        }
                    }
                    else
                    {

                        break;
                    }

                }

            }
        }

        /// <summary>
        /// Zdejmowanie pakietu z bufora wejsciowego, zmienianie jego pól, wpisywanie do bufora wyjsciowego
        /// </summary>
        public async Task commutePackets()
        {
            bool canIContinue = true;
            lock (_syncRoot)
            {
                while (commutationField.bufferIn.queue.Count > 0)
                {
                    if (commutationField.bufferIn.queue.Count > 0)
                    {
                        //czekamy iles milisekund
                        //canIContinue =  waitABit(100);

                        //wyswietlenie informacji na konsoli
                        //Console.WriteLine("commutePackets()");

                        //Jak jest niepusty bufor wejsciowy
                        if (commutationField.bufferIn.queue.Count != 0)
                        {
                            //Zdjecie pakietu z bufora wejsciowego
                            var temp = commutationField.bufferIn.queue.Dequeue();

                            //wyswietlenie informacji na konsoli
                            Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Package dequeued from the IN buffer (buffer size = " +
                                              commutationField.bufferIn.queue.Count + ") " + Package.extractID(temp) +
                                              " number " + Package.extractPackageNumber(temp) + " of " + Package.extractHowManyPackages(temp));

                            short ID = Package.extractID(temp);
                            short packageNumber = Package.extractPackageNumber(temp);
                            short howManyPackages = Package.extractHowManyPackages(temp);
                            //Podmiana naglowkow

                            temp = borderNodeCommutationTable.changePackageHeader2(temp, ref commutationField);


                            if (temp == null)
                            {

                                //Stary kolor konsoli
                                var color = Console.ForegroundColor;

                                //Ustawienie nowego koloru konsoli
                                Console.ForegroundColor = ConsoleColor.Cyan;

                                //Wyswietlenie wiadomosci o upuszczeniu pakietu
                                Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Dropped package " +
                                                 ID + " number " + packageNumber + " of " +
                                                  howManyPackages);
                                //Przywrocenie starego koloru konsoli
                                Console.ForegroundColor = color;

                                //Po prostu nie dodajemy pakietu do bufora

                            }
                            else
                            {

                                //Wywalamy pakiet bo nie wiadomo dokad ma isc.
                                if (Package.extractFrequency(temp) == -2)
                                {
                                    //Stary kolor konsoli
                                    var color = Console.ForegroundColor;

                                    //Ustawienie nowego koloru konsoli
                                    Console.ForegroundColor = ConsoleColor.Cyan;

                                    //Wyswietlenie wiadomosci o upuszczeniu pakietu
                                    Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Dropped package " +
                                                      Package.extractID(temp) +
                                                      " number " + Package.extractPackageNumber(temp) + " of " +
                                                      Package.extractHowManyPackages(temp));
                                    //Przywrocenie starego koloru konsoli
                                    Console.ForegroundColor = color;

                                    //Po prostu nie dodajemy pakietu do bufora
                                }
                                else
                                {
                                    if (commutationField.BuffersOut[0].queue.Count >= commutationField.maxBuffOutSize)
                                    {
                                        //Stary kolor konsoli
                                        var color = Console.ForegroundColor;

                                        //Ustawienie nowego koloru konsoli
                                        Console.ForegroundColor = ConsoleColor.Cyan;

                                        Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                                                          "(commutePackets) . Buffer OUT is full! Dropped package " +
                                                          Package.extractID(msg) +
                                                          " number " + Package.extractPackageNumber(msg) + " of " +
                                                          Package.extractHowManyPackages(msg));

                                        //Przywrocenie starego koloru konsoli
                                        Console.ForegroundColor = color;
                                    }
                                    else
                                    {
                                        //Dodanie podmienionego naglowka do kolejki wyjsciowej (od [0] bo to na razie lista)
                                        commutationField.BuffersOut[0].queue.Enqueue(temp);

                                        if (commutationField.BuffersOut[0].queue.Count >= commutationField.maxBuffOutSize)
                                        {
                                            CanIClearMyBuffers = true;
                                        }

                                        //Wyswietlenie informaji na ekranie
                                        Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Package added to the OUT buffer (buffer size = " +
                                                          commutationField.BuffersOut[0].queue.Count + ") " + Package.extractID(temp) +
                                                          " number " + Package.extractPackageNumber(temp) + " of " + Package.extractHowManyPackages(temp));

                                    }

                                }
                            }
                        }

                        //Gdy bufor wejsciowy jest pusty, to nie mozesz dalej komutowac
                        if (commutationField.bufferIn.queue.Count == 0)
                            CanICommutePackets = false;
                        //W przeciwnym razie komutuj dalej!
                        /* else
                             CanICommutePackets = true;*/

                        //wyswietlenie informacji na konsoli
                        //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                        //                " (commutePackets)canICommutePackets = " + canICommutePackets);
                    }
                }
            }
        }

        /// <summary>
        /// Zdejmowanie pakietu z bufora wyjsciowego i wysylanie go (jezeli uplynie timeout lub bufor jest pelny)
        /// </summary>
        /// <param name="send"></param>
        public async Task sendPackage(Socket send)
        {

            //Gdy bufory sa puste, to nie kontynuujemy
            bool canIContinue = true;

            while (commutationField.BuffersOut[0].queue.Count != 0)
            {
                //czekamy iles milisekund
                //canIContinue = await waitABit(100);

                //Console.WriteLine("sendPackage()");

                if (commutationField.BuffersOut[0].queue.Count != 0)
                {
                    //Jezeli rozmiar bufora osiagnal maksimum lub timer pozwolil na oproznienie buforow
                    if (commutationField.BuffersOut[0].queue.Count == commutationField.maxBuffOutSize
                        || CanIClearMyBuffers)
                    {
                        //kolejka jest pusta
                        if (commutationField.BuffersOut[0].queue.Count == 0)
                        {
                            //wyswietlenie informacji na konsoli
                            Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Empty OUT buffer!");
                            break;
                        }


                        //Dopoki kolejka nie jest pusta
                        while (commutationField.BuffersOut[0].queue.Count > 0)
                        {
                            //zdjecie pakietu z kolejki wyjsciowej
                            var tmp = commutationField.BuffersOut[0].queue.Dequeue();

                            //wyswietlenie informacji na konsoli
                            Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" + " Package dequeued from OUT buffer (buffer size = " +
                                              commutationField.BuffersOut[0].queue.Count + ") " + Package.extractID(tmp) +
                                              " number " + Package.extractPackageNumber(tmp) + " of " + Package.extractHowManyPackages(tmp));

                            //Wypisanie informacji na ekran o wyslaniu pakietu
                            stateSendingMessageToCableCloud(tmp, send);

                            //Zdjecie z kolejki pakietu i wyslanie go
                            sS.SendingPackageBytes(send, tmp);
                        }

                        //mozna wyzerowac licznik
                        zeroTimer = true;

                        //wyswietlenie informacji na konsoli
                        //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                        //                " (sendPackage)zeroTimer = " + zeroTimer);

                        //nie mozna czyscic buforow wyjsciowych
                        //  CanIClearMyBuffers = false;

                        //wyswietlenie informacji na konsoli
                        //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                        //                " (sendPackage)canIClearMyBuffers = " + canIClearMyBuffers);
                    }
                }

            }
            CanIClearMyBuffers = false;
        }

        /// <summary>
        /// Timer, ustalajacy pole canIClearBuffers dla wysylania pakietow
        /// </summary>
        public async Task timer()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                //stary kolor
                var color = Console.ForegroundColor;

                if (zeroTimer == true)
                {
                    //zmiana koloru konsoli
                    Console.ForegroundColor = ConsoleColor.Green;

                    sw = Stopwatch.StartNew();
                    CanIClearMyBuffers = false;

                    //wyswietlenie informacji na konsoli
                    //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                    //                " (timer)canIClearMyBuffers = " + canIClearMyBuffers);

                    zeroTimer = false;

                    //wyswietlenie informacji na konsoli
                    //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                    //                " (timer)zeroTimer = " + zeroTimer);
                }

                var wait = await Task.Run(async () =>
                {
                    int miliseconds = 500;
                    //Czekaj iles milisekund
                    await Task.Delay(miliseconds);

                    //zmiana koloru konsoli
                    Console.ForegroundColor = ConsoleColor.Green;

                    // Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ] Timer waits " + miliseconds + "ms...");

                    sw.Stop();
                    return sw.ElapsedMilliseconds;
                });

                //zmiana koloru konsoli
                Console.ForegroundColor = ConsoleColor.Green;

                //Gdy bufor wyjsciowy ma w sobie pakiety
                if (commutationField.BuffersOut[0].queue.Count > 0)
                    CanIClearMyBuffers = true;
                else
                    CanIClearMyBuffers = false;

                //wyswietlenie informacji na konsoli
                //Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ]" +
                //          " (timer)canIClearMyBuffers = " + canIClearMyBuffers);

                //Przywrocenie starego koloru konsoli
                Console.ForegroundColor = color;
            }
        }

        public async void CreateConnect2(string addressConnectIP, string key,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket socketClient = null;
            Socket listener = null;
            Socket socketSender = null;

            //Znajac dlugosc slowa "Sending" pobieram z calej nazwy klucza tylko index, ktory wykorzystam aby dopasowac do socketu IN
            ///1-Router
            ///2-Client
            ///3-NMS
            string typeOfSocket = key.Substring(8, key.Length - 8);
            string numberOfRouter = key.Substring(0, 1);

            //Sklejenie czesci wspolnej klucza dla socketu OUT oraz indeksu 
            string settingsString = numberOfRouter + "Listener" + typeOfSocket;

            IPAddress ipAddress =
                ipAddress = IPAddress.Parse(OperationConfiguration.getSetting(settingsString, mySettings));
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            if (!listener.IsBound)
            {
                //zabindowanie na sokecie punktu koncowego
                listener.Bind(localEndPoint);
                listener.Listen(100);
            }

            //Dodanie socketu do listy socketow OUT
            socketSendingList.Add(sS.ConnectToEndPoint(addressConnectIP));
            //oczekiwanie na polaczenie
            socketClient = listener.Accept();
            //dodanie do listy sluchaczy po przez delegata
            socketListenerList.Add(socketClient);

            Listening = true;

            string tmp = string.Empty;
            //wyznaczenie socketu przez ktory wyslana zostanie wiadomosc
            if (numberOfRouter == "1")
            {
                tmp = "127.0.0.12";

            }
            else if (numberOfRouter == "2")
            {
                tmp = "127.0.0.10";

            }
            else if (numberOfRouter == "3")
            {
                tmp = "127.0.0.8";

            }

            //Ustalenie socketa wysylajacego
            foreach (var socket in socketSendingList)
            {
                //zwracamy socket jeśli host z ktorym sie laczy spelnia warunek zgodnosci adresow z wynikiem kierowania lacza
                if (takingAddresSendingSocket(socket) == tmp)
                {
                    //Socket wysylajacy
                    socketSender = socket;
                }
            }

            //Uruchomienie timera
            Task.Run(async () => await timer());

            //Uruchomienie sluchania i wypelniania bufora
            Task.Run(async () => await receiveMessage(socketClient)).Wait();

            //Uruchomione zdejmowanie z bufora wejsciowego, podmiana naglowkow, wrzucenie do bufora wyjsciowego
            //  Task.Run(async () =>await commutePackets());

            //Uruchomione oproznianie bufora wyjsciowego (po timeoucie lub wypelnieniu bufora) i wysylanie pakietow
            //  Task.Run(async () =>await sendPackage(socketSender)).Wait();

        }


        public NameValueCollection getAppSetting { get { return mySettings; } }


        public static void stateReceivedMessageFromCableCloud(byte[] bytes, Socket socket)
        {
            if (bytes != null)
            {
                int length = Package.extractUsableMessage(bytes, Package.extractUsableInfoLength(bytes)).Length;
                short numberOfLink = Package.extractPortNumber(bytes);
                short ID = Package.extractID(bytes);
                short messageNumber = Package.extractPackageNumber(bytes);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ] Message about ID: {0,5} and number of package {1,2} / " + Package.extractHowManyPackages(bytes) + " received on port: " + numberOfLink, ID, messageNumber);
                Console.ResetColor();
            }
        }


        public static void stateSendingMessageToCableCloud(byte[] bytes, Socket socket)
        {
            if (socket != null)
            {
                int length = Package.extractUsableMessage(bytes, Package.extractUsableInfoLength(bytes)).Length;
                short numberOfLink = Package.extractPortNumber(bytes);
                short ID = Package.extractID(bytes);
                short messageNumber = Package.extractPackageNumber(bytes);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(" [ " + Timestamp.generateTimestamp() + " ] Message with ID: {0,5} and number of package {1,2} / " + Package.extractHowManyPackages(bytes) + " sent on port: " + numberOfLink, ID, messageNumber);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("CableCloud is not responding - link is not available");
                Console.ResetColor();
            }

        }
    }
}
