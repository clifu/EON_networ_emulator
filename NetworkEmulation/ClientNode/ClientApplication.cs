using NetworkingTools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ClientNode
{
    /// <summary>
    /// Główna klasa związana z obsługą aplikacji klienckiej. Odpalona zostaje po zakończeniu działania okienka klasy: <class>ClientNode.StartClientApplication.cs</class>.
    /// </summary>
    public partial class ClientApplication : Form
    {

        public static CPCC _CPCC;

        //Zmienna, która posłuży do nasłuchiwania nadchodzących wiadomości od strony chmury
        public static SocketListener sl = new SocketListener();

        public string adresdocelowy;

        //Zmienna, która posłuży do wysyłania wiadomości od użytkownika do chmury
        public static SocketSending sS = new SocketSending();

        //Token, który posłuży do zamknięcia sesji
        public static CancellationTokenSource _cts = new CancellationTokenSource();

        //Obiekt klasy ClientApplication, posłuży do odwołań do różnych pól klasy (głównie obsługa elementów związanych z WindowsForm)
        public ClientApplication _ClientApplication;

        //Gniazdo obsługujące wysyłanie wiadomosci przez połączenie TCP
        public Socket send;

        //Gniazdo obsługujące odbieranie wiadomości przez połączenie TCP
        public Socket socket;

        //Zmienna informująca o tym, czy przycisk odpowiedzialny za wysyłanie wiadomości został naciśnięty
        bool buttonSendClicked = false;

        //Zmienna informująca o tym, czy przycisk odpowiedzialny za wysyłanie wiadomości został naciśnięty
        bool buttonSendRandomClicked = false;

        //Zmienna informująca o tym, czy przycisk odpowiedzialny za zakończenie wysyłania wiadomości został naciśnięty
        bool buttonStopSendingClicked = false;

        //Zmienna informujaca o tym, czy przycisk odpowiedzialny za połączenie z chmurą został wciśnięty
        bool buttonConnectToCloudClicked = false;

        //Task, na nim uruchomione będzie wysyłanie wiadomości
        Task t;

        //IP aplikacji klienckiej dołączającej się do sieci
        string ClientIP;

        //Port apliakcji klienckiej dołączającej się do sieci
        string ClientPort;

        //Port na którym aplikacja kliencka będzie łączyła się z chmura
        string CloudPort;

        /// <summary>
        /// Lista zawierająca IP wszystkich aplikacji klieckich 
        /// <para>IP aplikacji przechowywane są w postaci stringów</para>
        /// </summary>
        List<string> clientsiplist;

        /// <summary>
        /// Lista zawierająca IP wszystkich połączeń z chmurą
        /// <para>IP chmury przechowywane są w postaci stringów</para>
        /// </summary>
        List<string> cloudsiplist;

        //Paczka która będzie wysyłana za pośrednictwem sieci
        Package EONpackage;

        //Nazwa użytkownika
        public static string ClientName;

        
        /// <summary>
        /// zmienne odczytywane z comboBoxe'a przy zestawianiu połączenia, 
        /// inicjuje je tu by do rozłączania połączenia nie używać danych odczytywanych z comboboxa
        /// tylko żeby brać te same wartości co zestawione połączenie
        /// </summary>
        //public static string destination, demandedCapacity;


        /// <summary>
        /// Konstruktor obiektu z klasy ClientApplication
        /// </summary>
        /// <param name="ClientIP">IP aplikacji klienckiej</param>
        /// <param name="ClientPort">Port aplikacji klienckiej</param>
        /// <param name="CloudPort">Port chmury</param>
        public ClientApplication(string ClientIP, string ClientPort, string CloudPort)
        {
            //Ustawienie CultureInfo na en-US spowoduje wyświetlanie się wyłapanych Exceptions w języku angielskim
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            //Zainicjowanie okna WindowsForm
            InitializeComponent();
            //Przypisanie referencji na dany obiekt
            _ClientApplication = this;
            //Przypisanie obiektowi IP aplikacji klienta, które zostało przekazane przez konstruktor z okna poziomu StartClientApplication
            this.ClientIP = ClientIP;
            //Przypisanie obiektowi Portu aplikacji klienta, które zostało przekazane przez konstruktor z okna poziomu StartClientApplication
            this.ClientPort = ClientPort;
            //Przypisanie obiektowi Portu chmury, które zostało przekazane przez konstruktor z okna poziomu StartClientApplication
            this.CloudPort = CloudPort;
            //Inicjalizacja listy zawierajacej ip klientów
            clientsiplist = new List<string>();
            //Inicjalizacja listy zawierajacej ip chmury, przez które będą łączyć się apliakcje klienckie
            cloudsiplist = new List<string>();
            //Liczba klientów - pobrana z pliku konfiguracyjnego
            int NbOfClients = Int32.Parse(ConfigurationManager.AppSettings[0]);
            //Odczytywanie z pliku konfiguracyjnego
            for (int i = 1; i <= NbOfClients; i++)
            {
                //String niezbędny do porównania IP, a później odpowiedniego nazwania aplikacji
                string temp = "ClientIP" + (i - 1);
                string temp2 = "ClientIP" + (i - 1) + (i - 1);

                //Dodawanie odczytanych IP aplikacjji klienckich do listy
                cloudsiplist.Add(ConfigurationManager.AppSettings[i + 2 * NbOfClients]);
                //Sprawdzenie czy wpisane IP w StartClientApplication jest takie samo IP w pliku konfiguracyjnym
                if (ConfigurationManager.AppSettings[i] == _ClientApplication.ClientIP)
                {
                    //Nadanie nazwy aplikacji - zgodnie z odczytanym IP z pliku konfiguracyjnego
                    string tempstring = ConfigurationManager.AppSettings[temp2];
                    _ClientApplication.Text = tempstring;
                    ClientName = tempstring;
                    //Dodanie IP klienckiej aplikacji do list
                    clientsiplist.Add(ConfigurationManager.AppSettings[i]);
                }
                else
                {
                    //Dodanie IP klienckiej aplikacji do listy
                    clientsiplist.Add(ConfigurationManager.AppSettings[i]);
                    //Dodanie IP aplikacji kliencjiej do listy
                    _ClientApplication.comboBoxClients.Items.Add(ConfigurationManager.AppSettings[temp2]);
                }
            }

            _CPCC = new CPCC(_ClientApplication.ClientIP, ClientName, ref _ClientApplication);
            _CPCC.runCPCC();
        }


        /// <summary>
        /// Funkcja uruchamiana w momencie naciśnięcia przycisku połączenie z chmura
        /// </summary>
        /// <param name="sender">Obiekt, który odpowiedzialny jest za wysłanie eventu</param>
        /// <param name="e">Otrzymany event po naciśnięciu przycisku</param>
        private void buttonConnectToCloud_Click(object sender, EventArgs e)
        {
            if (buttonConnectToCloudClicked == false)
            {
                buttonConnectToCloud.Enabled = false;
                buttonConnectToCloudClicked = true;
                string sourceIp;
                //Pobranie indeksu na którym w liście znajduje się IP naszej klienckiej aplikacji
                int cloudipindex = clientsiplist.IndexOf(_ClientApplication.ClientIP);
                //Pobranie IP chmury z listy
                string cloudIP = cloudsiplist[cloudipindex];
                try
                {
                    //Wiadomość która będzie otrzymywana
                    string message2 = null;
                    //Inicjalizacja ilości paczek
                    int numberOfPackages = 0;
                    //Inicjalizacja ID paczki
                    short frameID;
                    //Próba połączenia się z IP chmury, z którego bedziemy nasłuchiwali wiadomości
                    send = sS.ConnectToEndPoint(cloudIP);
                    if (send.Connected)
                    {
                        //Uruchomienie nasłuchiwania w aplikacji klienckiej 
                        socket = sl.ListenAsync(_ClientApplication.ClientIP);
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                //Odebrana wiadomość w postaci bytes
                                byte[] messagebytes;
                                //Zamienienie odebranej wiadomości na tablicę bajtów
                                messagebytes = sl.ProcessRecivedBytes(socket);
                                //Stworzenie timestampa
                                string timestamp2 = Timestamp.generateTimestamp();
                                //Zwiększenie liczby paczek - po odebraniu paczki
                                numberOfPackages++;
                                //Wydobycie ID paczki z otrzymanej wiadomości
                                frameID = Package.extractID(messagebytes);
                                //Odpakowanie adresy nadawcy z otrzymanej wiadomości
                                sourceIp = Package.exctractSourceIP(messagebytes).ToString();
                                //Stworzenie wiadomości, która zostanie wyświetlona na ekranie - odpakowanie treści wiadomości z paczki
                                string DestinationID = null;
                                int index = _CPCC.establishedConnections.FindIndex(x => x.Equals(sourceIp)) - 1;
                                DestinationID = _CPCC.establishedConnections[index];
                                message2 = DestinationID + ": " + Package.extractUsableMessage(messagebytes, Package.extractUsableInfoLength(messagebytes));
                                //Pojawienie się informacji o otrzymaniu wiadomości
                                _ClientApplication.updateLogTextBox("[" + timestamp2 + "] == RECEIVED MESSAGE number " + numberOfPackages +
                                                                    " == S_ClientID: " + DestinationID + " with FrameID=" + frameID);
                                //Zauktualizowanie wiadomości w polu ReceivedMessage
                                _ClientApplication.updateReceivedMessageTextBox(message2);
                                _ClientApplication.updateReceivedMessageTextBox("\r\n");
                                message2 = null;
                                messagebytes = null;
                                frameID = 0;

                            }
                        });
                    }
                    else

                    {
                        throw new NullReferenceException();

                    }

                }
                catch (Exception err)
                {
                    MessageBox.Show("Unable to connect to the Network Host!", "Attention!");
                    buttonConnectToCloud.Enabled = true;
                    buttonConnectToCloudClicked = false;
                }
            }
        }

        /// <summary>
        /// Funkcja uruchamiana w momencie naciśnięcia przycisku odpowiedzialnego za wysyłanie wiadomości o losowej długości
        /// </summary>
        /// <param name="sender">Obiekt, który odpowiedzialny jest za wysłanie eventu</param>
        /// <param name="e">Otrzymany event po naciśnięciu przycisku</param>
        private void buttonDifferentMessages_Click(object sender, EventArgs e)
        {
            if (comboBoxClients.SelectedItem != null)
            {
                //Pobranie celu do którego wysłana zostanie wiadomość
                //string destination = comboBoxClients.SelectedItem.ToString();

                string DestinationID = comboBoxClients.SelectedItem.ToString();


                //Zmiana ustawienia przycisku kończącego wysyłanie
                buttonStopSendingClicked = false;

                t = Task.Run(async () =>
                {
                    //Wygenerowanie losowego ID
                    short frameID = (short)(new Random()).Next(0, short.MaxValue);
                    string message = null;
                    //Liczba wysyłanych wiadomości
                    int nbOfMessages = Int32.Parse(textBoxHowManyMessages.Text);
                    //Wiadomość w postaci bytes
                    byte[] bytemessage;
                    //Pętla odpowiedzialna za wysłanie odpowiedniej ilości wiadomości.
                    for (int i = 0; i < nbOfMessages; i++)
                    {
                        //Generowanie losowej wiadomości o maksymalnej długości 37 bitów
                        message = RandomMessageGenerator.generateRandomMessage(37);
                        //Pobranie długości wygenerowanej wiadomości                     
                        short messageLength = (short)message.Length;
                        bytemessage = null;
                        //Stworzenie wysyłanej paczki
                        int portA = 0;
                        if(ClientIP=="127.0.0.2")
                        {
                            portA = 2;
                        }
                        else
                        {
                            portA = 1;
                        }
                        
                        int index  = _CPCC.establishedConnections.FindIndex(x => x.Equals(DestinationID)) + 1;

                        EONpackage = new Package(message,(short) portA,_CPCC.establishedConnections[index] , _ClientApplication.ClientIP, messageLength, Convert.ToInt16(1), (short)(-1),
                                        (short)(-1), (short)(-1), (short)(-1), frameID, 1);
                        bytemessage = EONpackage.toBytes();
                        //Stworzenie znacznika czasowego
                        string timestamp = Timestamp.generateTimestamp();
                        //Zamiana paczki na tablicę bajtów

                        if (send.Connected)
                        {
                            //Wysłanie wiadomości (tablicy bajtów) za pośrednictwem gniazda
                            sS.SendingPackageBytes(send, bytemessage);
                            //Zaktualizowanie LogEventTextBoxa
                            _ClientApplication.updateLogTextBox("[" + timestamp + "] == SENDING MESSAGE number " + (i + 1) + "==  D_ClientID: " + DestinationID + " with frameID: " + frameID);
                        }

                        var wait = await Task.Run(async () =>
                                {
                                    Stopwatch sw = Stopwatch.StartNew();
                                    await Task.Delay(10);
                                    sw.Stop();
                                    return sw.ElapsedMilliseconds;
                                });
                    }

                });
            }
            else
            {
                MessageBox.Show("You need to select a client!", "Important Message.",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

        }

        /// <summary>
        /// FUnkcja odpowiedzialna za podjecie próby ustanowienia połączenia - wysłanie wiadomości do NCC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void establishConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxClients.SelectedItem != null && textBoxDemandedCapacity.Text != "null")
                {
                    string destination = comboBoxClients.SelectedItem.ToString();
                    string demandedCapacity = textBoxDemandedCapacity.Text;
                    _CPCC.sendCallRequest(ClientName, destination, demandedCapacity);
                }
                else
                {
                    MessageBox.Show("You need to select a client and demanded capacity!", "Important Message.",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception err)
            {
                updateCPCCLog(err.ToString());
                updateCPCCLog("\r\n");
            }
        }

        /// <summary>
        /// Funkcja odpowiedzialna za aktualizowanie pola LogEvent - służy przesyłaniu wiadomości miedzy wątkami
        /// </summary>
        /// <param name="message">Wiadomość o którą zostanie zaktualizowane pole LogEvent</param>
        public void updateLogTextBox(string message)
        {
            _ClientApplication.textBoxLog.Invoke(new Action(delegate ()
            {
                _ClientApplication.textBoxLog.AppendText(message + "\r\n");
            }));
        }

        /// <summary>
        /// Funkcja odpowiedzialna za aktualizowanie pola LogEvent - służy przesyłaniu wiadomości miedzy wątkami
        /// </summary>
        /// <param name="message">Wiadomość o którą zostanie zaktualizowane pole LogEvent</param>
        public void updateCPCCLog(string message)
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }
            if (!textCPCClogs.IsHandleCreated)
            {
                textCPCClogs.CreateControl();
            }

            _ClientApplication.textCPCClogs.Invoke(new Action(delegate ()
            {
                _ClientApplication.textCPCClogs.AppendText(message);
            }));
        }


        /// <summary>
        /// Funkcja odpowiedzialna za aktualizowanie pola ReceivedMessage - służy przesyłaniu wiadomości miedzy wątkami
        /// </summary>
        /// <param name="message">Wiadomość o którą zostanie zaktualizowane pole ReceivedMessage</param>
        public void updateReceivedMessageTextBox(string message)
        {
            _ClientApplication.textBoxReceived.Invoke(new Action(delegate ()
            {
                _ClientApplication.textBoxReceived.AppendText(message);
            }));
        }

        private void BreakConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxClients.SelectedItem != null && textBoxDemandedCapacity.Text != "null")
                {
                    string destination = comboBoxClients.SelectedItem.ToString();
                    string demandedCapacity = textBoxDemandedCapacity.Text;
                    _CPCC.sendCallTearDown(ClientName, destination, demandedCapacity);
                }
                else
                {
                    MessageBox.Show("You need to select a client and demanded capacity!", "Important Message.",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception err)
            {
                updateCPCCLog(err.ToString());
                updateCPCCLog("\r\n");
            }
        }
        
    }
}
