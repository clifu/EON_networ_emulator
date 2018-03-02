using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Configuration;
using NetworkingTools;
using System.Collections.Specialized;
using System.Net.NetworkInformation;

namespace NetworkCalbleCloud
{

    public class CableCloud
    {

        public delegate Socket SocketDelegate(Socket socket);

        private OperationConfiguration oc = new OperationConfiguration();

        public static SocketDelegate sd;


        private static NameValueCollection readSettings = ConfigurationManager.AppSettings;


        private static SocketListener sl = new SocketListener();


        private static SocketSending sS = new SocketSending();


        private static CancellationTokenSource _cts = new CancellationTokenSource();


        private static List<Socket> socketListenerList = new List<Socket>();


        private static List<Socket> socketSendingList = new List<Socket>();


        private static List<Data> tmp = new List<Data>();


        private static List<string> tableFrom = new List<string>();


        private static List<string> tableTo = new List<string>();

        // private static List<List<byte[]>> listOfList = new List<List<byte[]>>();


        public static byte[] msg;


        private static short frequency;

        private static bool Listening = true;

        private static bool Last = true;


        public static void Main(string[] args)
        {
            sd = new SocketDelegate(CallbackSocket);
            messageHandling();

            Console.WriteLine("Press Esc to stop the CableCloud");
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
            // closeAllSocket();

        }

        /// <summary>
        /// Funkcja zwracjaca adres IP na ktorym nasluchuje socket, w postaci stringa
        /// </summary>      
        /// <param name="socket"></param>
        public static string takingAddresListenerSocket(Socket socket)
        {
            IPEndPoint ippoint = socket.LocalEndPoint as IPEndPoint;
            return ippoint.Address.ToString();
        }

        /// <summary>
        /// Funkcja zwracjaca adres IP hosta, z ktorym laczy sie socket przekazywany w parametrze, w postaci stringa
        /// </summary>      
        /// <param name="socket"></param>
        public static string takingAddresSendingSocket(Socket socket)
        {
            IPEndPoint ippoint = socket.RemoteEndPoint as IPEndPoint;

            return ippoint.Address.ToString();
        }

        /// <summary>
        /// Funkcja do obslugi socketow i wiadomosci przychochacych oraz wychodzacych
        /// </summary> 
        /// <remarks> 
        /// W funkcji tej zczytujemy dane o laczach oraz kierowaniu z pliku konfiguracyjnego
        /// Uruchamiamy sluchacza na kazdym z adresow podanych w pliku cofiguracyjnym, a po nawiazaniu polaczenia przychodzacego odrazu probujemy
        /// nawiazac polaczenie wychodzace na sokecie wychodzacym
        /// Kazdy sluchac dziala na oddzielnym watku
        /// </remarks>
        private static void messageHandling()
        {
            //Zmienna do przechowywania klucza na adres wychodzacy powiazany z socketem sluchaczem
            string settingsString = "";

            //Zmienna do przechowywania numeru po ktorym identyfikujemy klucz na socket wychodzacy
            string str;

            //pobranie wlasnosci zapisanych w pliku konfiguracyjnym
            tmp = OperationConfiguration.ReadAllSettings(readSettings);

            //przeszukanie wszystkich kluczy w liscie 
            foreach (var key in tmp)
            {

                if (key.Keysettings.StartsWith("Listener"))
                {
                    //Uruchamiamy watek na kazdym z tworzonych sluchaczy
                    var task = Task.Run(() =>
                    {

                        ListenAsync(key.SettingsValue, key.Keysettings);

                    });



                }

                //jezeli klucz zaczyna sie od TableFrom to uzupelniamy liste
                else if (key.Keysettings.StartsWith("TableFrom"))
                    tableFrom.Add(key.SettingsValue);

                //jezeli klucz zaczyna sie od TableTo to uzupelniamy liste
                else if (key.Keysettings.StartsWith("TableTo"))
                    tableTo.Add(key.SettingsValue);
            }

        }

        /// <summary>
        /// Funkcja zwracjaca adres IP hosta na ktory nalezy kierowac otrzymana wiadomosc
        /// </summary>      
        /// <param name="fromAndLambda">Parametr okresla socket na ktorym odebrana została wiadomosc oraz częstotliwosc w formacie "adres f" </param>
        public static string pushMessageTable(string fromAndLambda)
        {
            //W dwoch listach zawieraja sie dane o tym jakie wezly lacza sie ze soba
            //Na tych samych poziomach znajduja sie odpowiadajace sobie informacje
            //Na tej podstawie okreslamy, ktorym socketem wysylac
            return tableTo.ElementAt(tableFrom.IndexOf(fromAndLambda));
        }

        /// <summary>
        /// Funkcja zwracjaca socket na ktory nalezy kierowac otrzymana wiadomosc
        /// ///Wartosc jest liczona na podstawie list z tabelami kierowania
        /// </summary>      
        /// <param name="fromAndLambda">Parametr okresla socket na ktorym odebrana została wiadomosc oraz częstotliwosc w formacie "adres f" </param>
        public static Socket sendingThroughSocket(string addressAndLambda)
        {
            //Przeszukujemy liste w poszukiwaniu socketu spelniajacegowymagania
            foreach (var socket in socketSendingList)
            {
                //zwracamy socket jeśli host z ktorym sie laczy spelnia warunek zgodnosci adresow z wynikiem kierowania lacza
                if (takingAddresSendingSocket(socket) == pushMessageTable(addressAndLambda))
                {
                    return socket;
                }

            }

            return null;
        }

        /// <summary>
        /// Funkcja wykorzystywana w delegacie, dodaje kolejnego sluchacza do listy po nawiazaniu polaczenia
        /// Zwraca socket
        /// </summary>      
        /// <param name="socket">Parametrem jest socket  </param>
        public static Socket CallbackSocket(Socket socket)
        {
            //Dodajemy socket do listy sluchaczy
            socketListenerList.Add(socket);
            return socket;
        }

        /// <summary>
        /// Funkcja sluzaca zamykaniu wszystkich socketow, zakonczeniu polaczen i usunieciu ich z pamieci
        /// </summary>     
        private static void closeAllSocket()
        {
            //wyciagam wszystkie sockety z listy sluchaczy i kolejno zamykam je i usuwam z listy
            foreach (var socket in socketListenerList)
            {
                socket.Close();
                socketListenerList.Remove(socket);
            }

            //wyciagam wszystkie sockety z listy socketow do wysylania wiadomosci0 i kolejno zamykam je i usuwam z listy
            foreach (var socket in socketSendingList)
            {
                socket.Close();
                socketSendingList.Remove(socket);
            }

        }

        /// <summary>
        /// Funkcja sluży do 
        /// Zwraca socket
        /// </summary>      
        /// <param name="adresIPListener">Parametrem jest adres IP na ktorym nasluchujemy  </param>
        ///  /// <param name="key">Parametrem jest warotsc klucza wlasnosci z pliku config  </param>
        private static Socket ListenAsync(string adresIPListener, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket socketClient = null;
            Socket listener = null;
            IPAddress ipAddress =
                         ipAddress = IPAddress.Parse(adresIPListener);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);


            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                byte[] bytes = new Byte[1024];

                // Create a TCP/IP socket.  
                listener = new Socket(ipAddress.AddressFamily,
                   SocketType.Stream, ProtocolType.Tcp);

                if (!listener.IsBound)
                {
                    //zabindowanie na sokecie punktu koncowego
                    listener.Bind(localEndPoint);
                    listener.Listen(100);
                }

                cancellationToken.ThrowIfCancellationRequested();

                //Nasluchujemy bez przerwy
                while (Last)
                {

                    if (Listening)
                    {
                        //oczekiwanie na polaczenie
                        socketClient = listener.Accept();
                        //dodanie do listy sluchaczy po przez delegata
                        sd(socketClient);
                        Socket send = null;

                        //Znajac dlugosc slowa "Listener" pobieram z calej nazwy klucza tylko index, ktory wykorzystam aby dopasowac do socketu OUT
                        string str = key.Substring(8, key.Length - 8);

                        //Sklejenie czesci wspolnej klucza dla socketu OUT oraz indeksu 
                        string settingsString = "Sending" + str;

                        //Dodanie socketu do listy socketow OUT
                        socketSendingList.Add(sS.ConnectToEndPoint(OperationConfiguration.getSetting(settingsString, readSettings)));
                        Listening = false;
                        LingerOption myOpts = new LingerOption(true, 1);
                        socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, myOpts);
                        socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                        socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                        Console.WriteLine("Polaczenie na  " + takingAddresListenerSocket(socketClient));


                        string fromAndFrequency;


                        //Oczekiwanie w petli na przyjscie danych
                        while (true)
                        {

                            //Odebranie tablicy bajtow na obslugiwanym w watku sluchaczu

                            msg = sl.ProcessRecivedBytes(socketClient);

                            // Package.extractHowManyPackages(msg);
                            // listByte.Add(msg);

                            //Wykonuje jezeli nadal zestawione jest polaczenie
                            if (socketClient.Connected)
                            {
                                stateReceivedMessage(msg, socketClient);
                                //Uzyskanie czestotliwosci zawartej w naglowku- potrzebna do okreslenia ktorym laczem idzie wiadomosc
                                frequency = Package.extractPortNumber(msg);
                                fromAndFrequency = takingAddresListenerSocket(socketClient) + " " + frequency;

                                //wyznaczenie socketu przez ktory wyslana zostanie wiadomosc
                                send = sendingThroughSocket(fromAndFrequency);
                                stateSendingMessage(msg, send,fromAndFrequency);
                                //wyslanei tablicy bajtow
                                sS.SendingPackageBytes(send, msg);


                            }
                            else
                            {
                                //Jezeli host zerwie polaczneie to usuwamy go z listy przetrzymywanych socketow, aby rozpoczac proces nowego polaczenia
                                int numberRemove = socketListenerList.IndexOf(socketClient);
                                socketListenerList.RemoveAt(numberRemove);
                                socketSendingList.RemoveAt(numberRemove);
                                break;


                            }
                        }
                        Listening = true;

                    }
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Socket Exception: {se}");
            }
            finally
            {
                // StopListening();
            }
            if (socketClient == null)
            {
                return new Socket(ipAddress.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);
            }

            return socketClient;
        }

        public static void stateReceivedMessage(byte[] bytes, Socket socket)
        {
            int length = Package.extractUsableMessage(bytes, Package.extractUsableInfoLength(bytes)).Length;
            short numberOfLink = Package.extractPortNumber(bytes);
            short ID = Package.extractID(bytes);
            short messageNumber = Package.extractPackageNumber(bytes);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Node on interface " + takingAddresListenerSocket(socket) + " from link number: " + numberOfLink + 
                " -  {0,2} bytes recieved. Number ID: {1,5}, number of package: " + messageNumber + "/" + Package.extractHowManyPackages(bytes), length, ID);
            Console.ResetColor();
        }

        public static void stateSendingMessage(byte[] bytes, Socket socket,string toTable)
        {
            if (socket != null)
            {
                int length = Package.extractUsableMessage(bytes, Package.extractUsableInfoLength(bytes)).Length;
                short numberOfLink = Package.extractPortNumber(bytes);
                short ID = Package.extractID(bytes);
                short messageNumber = Package.extractPackageNumber(bytes);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Node on interface " + takingAddresSendingSocket(socket) + " on link number: " + numberOfLink +
                    " -  {0,2} bytes sent. Number ID: {1,5}, number of package: " + messageNumber+"/"+Package.extractHowManyPackages(bytes), length, ID);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Node {0} which is next hop is not responding", tableTo.ElementAt(tableFrom.IndexOf(toTable)));
                Console.ResetColor();
            }

        }



    }
}

