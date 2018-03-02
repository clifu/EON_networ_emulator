using NetworkingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Collections.Specialized;
using System.IO;

namespace NewNMS
{
    public partial class Application : Form
    {
        //Zmienna, która posłuży do wysyłania wiadomości od użytkownika do chmury
        public static SocketSending sS = new SocketSending();

        // public Application _Application;

        public static CancellationTokenSource _cts = new CancellationTokenSource();

        public static Listening ls = new Listening();

        private OperationConfiguration oc = new OperationConfiguration();

        //odwoływanie się do funkcji czytających z pliku konfiguracyjnego
        public static Reading rd = new Reading();

        // socket służacy do dodawania go do listy socketów w celu dalszej komunikacji
        public Socket send;

        // socket służący do nasłuchiwania
        public Socket listener;

        //socket służący do przekazywania go do funkcji wysyłającej w zależności od wyboru do którego routera ma pójść wiadomość
        public Socket sending;

        public Application _Application;

        //lista socketów, które będą łączone w momencie przyjścia wiadomości Node is up
        public List<Socket> sends_socket;

        //lista socketów z zestawionym polaczeniem 
        public List<Socket> listening_socket = new List<Socket>();

        private object _syncRoot = new object();

        // lista zawierająca adresy IP network nodów, pochodzące z wiadomości keep alive
        public List<string> routers_IP;

        // tablica bajtów żądania przesyłanego od NMSa 
        public byte[] message_bytes, bytes_to_send;

        private static bool boolListening = true;

        //lista portów  do agentów 
        private List<string> interfaces;

        private List<string> paths;

        //wiersz tablicy komutacji do wysłania do agenta
        private List<string> CommutationTableRow;

        public List<string> list;

        private int constPort;

        private int numbersOfParameter;


        public Application()
        {
            //Ustawienie CultureInfo na en-US spowoduje wyświetlanie się wyłapanych Exceptions w języku angielskim
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            _Application = this;
            InitializeComponent();
            routers_IP = new List<string>();
            sends_socket = new List<Socket>();
            //dodawanie opcji do comboboxów
            ShowComboBox();
            HidetextBoxes();
            readConfiguration();
        }




        /// <summary>
        ///  metoda zwracająca socket z listy socketów w zalezności od wybrania adresu w comboBocRouters
        /// </summary>
        /// <returns> Socket</returns>
        private Socket returnSocket()
        {
            int i;
            try
            {
                //pętla po wszystkich wartościach w comboBoxRouters
                for (i = 0; i < comboBoxRouters.Items.Count; i++)
                {
                    if (comboBoxRouters.SelectedIndex == i)
                    {
                        sending = sends_socket[i];
                    }
                }
            }
            catch (Exception e)
            {
                listBoxReceived.Items.Add("Fail with access to the socket");
            }

            return sending;
        }

        // tworzenie wiadomości w zalezności od zaznaczonego typu wiadomości w comboBocActions
        /// <summary>
        /// tworzenie wiadomości w zalezności od zaznaczonego typu wiadomości w comboBocActions
        /// </summary>
        /// <returns> byte[] </returns>
        private byte[] returnBytes()
        {
            byte[] table_in_bytes = new byte[128];
            try
            {

                if (comboBoxTables.GetItemText(comboBoxTables.SelectedItem) == "Commutation Table")
                {

                    if (comboBoxActions.GetItemText(comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;;
                        string command = string.Empty;
                        string freq_in = string.Empty;
                        string port_in = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        freq_in = (textBoxFrequencyIN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        builder = command + "#" + "3" + "#" + freq_in + "#" + port_in;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();

                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string freq_in = string.Empty;
                        string port_in = string.Empty;
                        string freq_out = string.Empty;
                        string port_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        freq_in = (textBoxFrequencyIN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        freq_out = (textBoxFrequencyOUT.Text).ToString();
                        port_out = (textBoxPort_OUT.Text).ToString();
                        builder = command + "#" + "3" + freq_in + "#" + port_in + "#" + freq_out + "#" + port_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                     {
                        string command = string.Empty;
                        command = "Commutation Table";
                        short length = (Int16)command.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), command, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                }
                if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table")
                {

                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string port_in = string.Empty;
                        string port_out = string.Empty;
                        string Hops = string.Empty;
                        string command = string.Empty;
                        string band_out = string.Empty;
                        string Cloud_IP = string.Empty;
                        string Modulation = string.Empty;
                        string BitRate = string.Empty;
                        string IP_IN = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        IP_IN = (textBox_IP_IN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Modulation = (textBoxModulation.Text).ToString();
                        BitRate = (textBoxBitrate.Text).ToString();
                        Cloud_IP = (textBoxDestination_IP.Text).ToString();
                        port_out = (textBoxPort_OUT.Text).ToString();
                        Hops = (textBoxHops.Text).ToString();

                        builder = command + "#" + "1" + "#" + IP_IN + "#" + port_in + "#" + band_out + "#" + Frequency_out + "#" +
                            Modulation + "#" + BitRate + "#" + Cloud_IP + "#" + port_out + "#" + Hops;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();

                    }
                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string IP_IN = string.Empty;
                        string port_in = string.Empty;
                        string destination_IP = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        IP_IN = (textBox_IP_IN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        //destination_IP = (textBoxDestination_IP.Text).ToString();

                        builder = command + "#" + "1" + "#" + IP_IN + "#" + port_in + "#"; /* +destination_IP*/
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                      else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                      {
                        table_in_bytes = null;
                        string command = string.Empty;
                        command = "Border Node Commutation Table";
                        short length = (Int16)command.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), command, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                }
                if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table")
                {

                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string Band_out = string.Empty;
                        string Band_in = string.Empty;
                        string frequency_in = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        Band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Band_in = (textBoxBand_IN.Text).ToString();
                        frequency_in = (textBoxFrequencyIN.Text).ToString();

                        builder = command + "#" + "2" + "#" + frequency_in + "#" + Band_in + "#" + Frequency_out + "#" + Band_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();

                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string Band_out = string.Empty;
                        string Band_in = string.Empty;
                        string frequency_in = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        Band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Band_in = (textBoxBand_IN.Text).ToString();
                        frequency_in = (textBoxFrequencyIN.Text).ToString();

                        builder = command + "#" + "2" + "#" + frequency_in + "#" + Band_in + "#" + Frequency_out + "#" + Band_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                    {
                        table_in_bytes = null;
                        string command = string.Empty;
                        command = "EON Table";
                        short length = (Int16)command.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), command, length);
                        table_in_bytes = commutation_table.toBytes();
                    }
                }
            }
            catch (Exception e)
            {
                listBoxReceived.Items.Add("Error occured with getting data for the agent");
                table_in_bytes = null;
            }
            return table_in_bytes;
        }

        /// <summary>
        /// funkcja odpalana po naciśnięciu przycisku Run
        /// </summary>
        private void ListenForConnections()
        {
            Task.Run(() =>
            {
                // petla po wszystkich wartościach portów agentów appconfig
                foreach (var address in interfaces)
                {
                    Socket socketClient = null;
                    Socket listener = null;
                    try
                    {
                        byte[] bytes = new Byte[64];

                        IPAddress ipAddress = IPAddress.Parse(address);
                        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, constPort);
                        // Create a TCP/IP socket.  
                        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(localEndPoint);
                        listener.Listen(100);
                        try
                        {
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    socketClient = listener.Accept();
                                    listening_socket.Add(socketClient);
                                    ReceivedMessage(socketClient);
                                }
                            });
                        }
                        catch (OperationCanceledException)
                        {
                            listBoxReceived.Items.Add("Exception during listetning");
                        }
                    }
                    catch (SocketException se)
                    {
                        UpdateListBoxReceived("Exception during connecting with network node");
                    }
                }
            });
        }

        // funkcja dodajaca wiadomości do ListBoxa wraz z invokiem 
        public void UpdateListBoxReceived(string message)
        {
            _Application.listBoxReceived.Invoke(new Action(delegate ()
            {
                _Application.listBoxReceived.Items.Add(message);
                listBoxReceived.SelectedIndex = listBoxReceived.Items.Count - 1;
            }));
        }

        /// <summary>
        /// funkcja dodająca statyczne elementy do comboboxów, nie zmieniające się w czasie
        /// </summary>
        private void ShowComboBox()
        {
            // elementy comboboxe'a wybierające operacje do wykonania
            comboBoxActions.Items.Add("DELETE");
            comboBoxActions.Items.Add("ADD");
            comboBoxActions.Items.Add("TOPOLOGY");

            comboBoxTables.Items.Add("Commutation Table");
            comboBoxTables.Items.Add("EON Table");
            comboBoxTables.Items.Add("Border Node Commutation Table");

        }


        /// <summary>
        /// funkcja pobierająca adres IP z socketa 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static string getIPAddressRemote(Socket socket)
        {
            //string address;
            IPEndPoint ipend = socket.RemoteEndPoint as IPEndPoint;
            return ipend.Address.ToString();
        }

        /// <summary>
        /// funkcja obsługująca przychodzącą wiadomość
        /// </summary>
        /// <param name="listener"></param>
        private void ReceivedMessage(Socket listener)
        {
            string usableMessage = "";

            while (true)
            {

                // wyczyszczenie paczki przed jej wysłaniem oraz przed ponownym wysyłaniem 
                usableMessage = string.Empty;

                //przypisywanie do paczki wiadomości przesłanej przez klienta, w tym przypadku przez agenta
                byte[] nmspackage = new byte[64];

                // tylko jeden wątek moze wykonywac ten kod w danym czasie
                //  lock (_syncRoot)
                //   {
                nmspackage = ls.ProcessRecivedByteMessage(listener);
                //   }
                // wykonuje się tylko wtedy jeśli agent nie  jest podłączony
                if (!listener.Connected)
                {
                    // pobranie indeksu socketu z listy soecketów
                    int index = listening_socket.IndexOf(listener);
                    UpdateListBoxReceived("Network Node" + getIPAddressRemote(sends_socket.ElementAt(index)) + " is disconnected");
                    // rozłączanie obu socketów
                    listener.Disconnect(true);
                    //send.Disconnect(true);

                    //usuwanie socketów, adresów z list by przy ponownym połączeniu dodawać je na ponownie
                    listening_socket.RemoveAt(index);
                    sends_socket.RemoveAt(index);
                    routers_IP.RemoveAt(index);

                    // wyświetlanie pobranego adresu IP z list podłączonych agentów
                    _Application.comboBoxRouters.Invoke(new Action(delegate ()
                    {

                        comboBoxRouters.Items.RemoveAt(index);

                    }));
                    break;
                }
                // wykonuje się tylko wtedy jeśli agent jest podłączony
                else
                {
                    string sourceip;
                    //tylko jesli paczka nie jest nullem
                    if (nmspackage != null)
                    {
                        usableMessage = NMSPackage.extractUsableMessage(nmspackage, NMSPackage.extractUsableInfoLength(nmspackage));
                        //tylko w przypadku pierwszej wiadomości od danego agenta
                        if (usableMessage == "Network node is up")
                        {
                            sourceip = NMSPackage.exctractSourceIP(nmspackage).ToString();
                            //jesli lista z adresami IP routerów nie zawiera danego IP to je dodaje a następnie wyśwuietlam komunikat 
                            if (!routers_IP.Contains(sourceip))
                            {
                                routers_IP.Add(sourceip);
                                UpdateListBoxReceived("Network Node: " + sourceip + " is up");
                                UpdateListBoxReceived(listening_socket.Count.ToString());

                            }
                            //tworze połączenie z socketem routera, który wysłał do mnie wiadomość
                            send = sS.ConnectToEndPoint(NMSPackage.exctractSourceIP(nmspackage).ToString());
                            // a następnie dodaje ten socket do listy socketów, by potem móc z nich korzystać
                            sends_socket.Add(send);

                            short numberOfRouter = NMSPackage.extractNumberOfRouterNumber(nmspackage);

                            List<string> configurationRouter = ReadingFromFile(paths.ElementAt(numberOfRouter - 1));
                            foreach (var line in configurationRouter)
                            {
                                short length = (Int16)line.Length;
                                NMSPackage tablePackage = new NMSPackage(interfaces.ElementAt(numberOfRouter - 1), line, length);
                                byte[] tablePackageInBytes = tablePackage.toBytes();
                                send.Send(tablePackageInBytes);
                                Task.Delay(10);
                            }



                            /* z kazdą wiadomością "Network node is up" dodaje IP routera do checkboca w celu mozliwości wybrania
                             docelwoego punktu komunikacji */
                            _Application.comboBoxRouters.Invoke(new Action(delegate ()
                            {

                                _Application.comboBoxRouters.Items.Add(sourceip);

                            }));

                        }
                        //jesli wiadmośc keep alive
                        else if (usableMessage == "Keep Alive")
                        {

                            UpdateListBoxReceived(usableMessage);

                        }
                    }
                    //jesli paczka jest nullem
                    else
                    {
                        int index = listening_socket.IndexOf(listener);
                        // stwierdzam, że agent nie odpowiada, a potem go rozłączam
                        UpdateListBoxReceived("Network Node" + getIPAddressRemote(sends_socket.ElementAt(index)) + " is not responding");
                        UpdateListBoxReceived("Network Node" + getIPAddressRemote(sends_socket.ElementAt(index)) + " is disconnected");

                        listening_socket.RemoveAt(index);
                        sends_socket.RemoveAt(index);
                        routers_IP.RemoveAt(index);
                        _Application.comboBoxRouters.Invoke(new Action(delegate ()
                        {

                            comboBoxRouters.Items.RemoveAt(index);

                        }));
                        //odłączanie
                        listener.Disconnect(true);
                        //send.Disconnect(true);
                        //usuwanie socketów, adresów z list by przy ponownym połączeniu dodawać je na ponownie

                        break;

                    }

                }

            }

        }

        private void buttonListen_Click_1(object sender, EventArgs e)
        {
            _Application.listBoxReceived.Items.Add("Running");
            ListenForConnections();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (comboBoxRouters.SelectedItem != null)
            {
                Socket socket = null;
                byte[] bytes = null;
                socket = returnSocket();
                bytes = returnBytes();
                socket.Send(bytes);
                HidetextBoxes();
            }
        }

        private void readConfiguration()
        {
            List<Data> data = new List<Data>();
            interfaces = new List<string>();
            paths = new List<string>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            foreach (var key in data)
            {
                if (key.Keysettings.StartsWith("interface"))
                {
                    interfaces.Add((key.SettingsValue));
                }
                else if (key.Keysettings.StartsWith("port"))
                {
                    constPort = Int32.Parse(key.SettingsValue);
                }
                else if (key.Keysettings.StartsWith("number"))
                {
                    numbersOfParameter = Int32.Parse(key.SettingsValue);
                }
                else if (key.Keysettings.StartsWith("path"))
                {
                    paths.Add((key.SettingsValue));
                }
            }

        }

        public List<string> ReadingFromFile(string path)
        {
            List<string> listLine = new List<string>();
            string line;
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        //takeParameterFromFile(line);
                        listLine.Add(line);
                        // MessageBox.Show(line);
                    }
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return listLine;

        }

        public void takeParameterFromFile(string line)
        {
            string a, b, c, d;

            Char delimiter = '#';
            String[] parameters = line.Split(delimiter);
            if (parameters.Length == numbersOfParameter)
                foreach (var substring in parameters)
                {
                    Console.WriteLine(substring);
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EnableTextBoxes();
        }

        private void EnableTextBoxes()
        {
            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Commutation Table" && comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {
                textBoxFrequencyIN.Visible = true;
                textBox_Port_IN.Visible = true;
                textBoxPort_OUT.Visible = true;
                textBoxFrequencyOUT.Visible = true;
            }
            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Commutation Table" && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {
           
                textBoxFrequencyIN.Visible = true;
                textBox_Port_IN.Visible = true;

            }
   

            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table" && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {

                textBoxBand_OUT.Visible = true;
                textBoxFrequencyOUT.Visible = true;
                textBoxBand_IN.Visible = true;
                textBoxFrequencyIN.Visible = true;
            }

            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table" && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {
                textBoxBand_OUT.Visible = true;
                textBoxFrequencyOUT.Visible = true;
                textBoxBand_IN.Visible = true;
                textBoxFrequencyIN.Visible = true;

            }
        

            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table" && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {
                textBox_IP_IN.Visible = true;
                textBox_Port_IN.Visible = true;
                textBoxBand_OUT.Visible = true;
                textBoxFrequencyOUT.Visible = true;
                textBoxModulation.Visible = true;
                textBoxBitrate.Visible = true;
                textBoxDestination_IP.Visible = true;
                textBoxPort_OUT.Visible = true;
                textBoxHops.Visible = true;
            }
            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table" && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {
                textBox_IP_IN.Visible = true;
                //textBoxDestination_IP.Visible = true;
                textBox_Port_IN.Visible = true;

            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HidetextBoxes();
        }

        private void HidetextBoxes()
        {
            textBox_IP_IN.Visible = false;

            textBoxBand_IN.Visible = false;

            textBoxModulation.Visible = false;

            textBoxBitrate.Visible = false;

            textBoxDestination_IP.Visible = false;

            textBoxHops.Visible = false;

            textBoxBand_OUT.Visible = false;

            textBoxFrequencyOUT.Visible = false;

            textBox_Port_IN.Visible = false;

            textBoxFrequencyIN.Visible = false;

            textBoxPort_OUT.Visible = false;
        }
    }
}







