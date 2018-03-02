using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using NetworkingTools;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Text;
//using SharedMessageNames;

namespace NCC
{
    public class NCC
    {
        // obiekty słuzace wykorzystaniu niestatycznych metod tych klas

        private static Directory dr = new Directory();
        private static PolicyController pc = new PolicyController();
        private static Configuration conf = new Configuration();

        // lista z adresami IP i socketami interfejsów służących do słuchania
        public static Dictionary<string, string> InterfaceToListen = new Dictionary<string, string>();

        // lista z adresami IP interfejsów służących do wysyłania
        public static Dictionary<string, string> InterfaceToSend = new Dictionary<string, string>();

        //numer wpisywany przez uzytkownika okreslajacy numer domeny, wynosi 1 lub 2
        private static string number;

        public static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.WriteLine("Enter number of domain");
            number = Console.ReadLine();

            dr.ReadingFromDirectoryFile(ConfigurationManager.AppSettings["directory" + number + "_path"]);
            pc.ReadingFromPolicyFile(ConfigurationManager.AppSettings["policy" + number + "_path"]);
            conf.ReadingFromConfigFile(ConfigurationManager.AppSettings["config" + number + "_path"]);
            conf.ReadingIPFromConfigFile(ConfigurationManager.AppSettings["IP" + number + "_path"]);


            NCC new_NCC = new NCC();
            new_NCC.ListenForConnections();
            Console.ReadKey();
        }

        public NCC()
        {

        }

        /// <summary>
        /// funkca zajmująca się rozróżnianiem otrzymywanych wiadomości
        /// </summary>
        /// <param name="data"></param>
        private void ReceivedMessage(byte[] data)
        {

            string message = Encoding.ASCII.GetString(data);
            char[] delimeter = { '#' };
            string[] words = message.Split(delimeter);

            //if rozróżniający wiadomość na podstawie pierwszego wyrazu, a w przypadku CALL CONFIRMED
            // również osttaniego, który pokazuje od kogo wiadomość idzie
            if (words[0] == MessageNames.CALL_REQUEST)
            {
                ProcessCallRequest(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_COORDINATION)
            {
                ProcessCallCoordination(words[1], words[2], words[3], words[4]);
            }
            else if (words[1] == MessageNames.CONNECTION_CONFIRMED)
            {
                ProcessConnectionConfirmedFromCC(words[2], words[3], words[0]);
            }
            else if (words[0] == MessageNames.CALL_CONFIRMED && words[5] == "CPCC")
            {
                ProcessCallConfirmedFromCPCC(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_CONFIRMED && words[5] == "NCC")
            {
                ProcessCallConfirmedFromNCC(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_TEARDOWN && words[5] == "CPCC" && words[6] == "REQUEST")
            {
                ProcessCallTeardownRequestFromCPCC(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_TEARDOWN && words[5] == "NCC" && words[6] == "REQUEST")
            {
                ProcessPeerCoordinationForTearDown(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_TEARDOWN_CONFIRMATION && words[5] == "CPCC" && words[6] == "OK")
            {
                ProcessCalltearDownConfirmationFromCPCC(words[1], words[2], words[3], words[4]);
            }
            else if (words[0] == MessageNames.CALL_TEARDOWN_CONFIRMATION && words[5] == "NCC" && words[6] == "OK")
            {
                ProcessCallTearDownConfirmationFromNCC(words[1], words[2], words[3], words[4]);
            }
            else if (words[1] == MessageNames.CONNECTION_TEARDOWN_CONFIRMED)
            {
                ProcessConnectionTearDownConfirmed(words[2], words[3], words[0]);
            }
            else if (words[1] == MessageNames.CONNECTION_CONFIRMED + "NOT")
            {
                ProcessConnectionConfirmedNOTFromCC(words[2], words[3], words[0]);
            }
        }

        /// <summary>
        /// funcjja wywolywana jesli NCC otrzymało wiadomść CALL REQUEST, procesująca tę wiadomość
        /// </summary>
        /// <param name="OriginID"></param>
        /// <param name="DestinationID"></param>
        /// <param name="demandedCapacity"></param>
        /// <param name="CPCC_IP"></param>
        public static void ProcessCallRequest(string OriginID, string DestinationID, string demandedCapacity, string Client_IP)
        {
            string ip_CPCC;
            conf.Client_with_CPCC.TryGetValue(Client_IP, out ip_CPCC);
            // powrót do defaultowego koloru czcionki na konsoli i wypisanie logów
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + " Received Call Request  " + "from Client's CPCC with address:" + ip_CPCC + " (ID,ID):" + OriginID + ", " + DestinationID + " with: " + demandedCapacity);

            //translacja adresów odbywająca się w klasie Directory i funkcji AddressTranslation
            bool directoryaccess = dr.AddressTranslation(OriginID, DestinationID);
            // sprawdzenie dostępu w klasie PolicyController i funkcji Policy
            bool policyaccess = pc.Policy(ConfigurationManager.AppSettings["policy" + number + "_path"], OriginID);

            //pobranie wartości adresu IP tego NCC w celu przesłania go w kolejnej wiadomości
            string NCC_address;
            InterfaceToListen.TryGetValue("interface_listen", out NCC_address);

            //jeśli oba warunki spełnione to wywołujemy funkcję SendCallCoordination
            // 
            if (policyaccess == true && directoryaccess == true && dr.OneDomain == true)
            {
                SendCallIndication(dr.OriginAddress, dr.DestinationAdddress, demandedCapacity, NCC_address);
            }
            else if (policyaccess == true && directoryaccess == true)
            {
                SendCallCoordination(dr.OriginAddress, DestinationID, demandedCapacity, NCC_address);
            }
        }
        /// <summary>
        /// Funkcja przesyłająca Call Coordination do kolejengo NCC
        /// </summary>
        /// <param name="OriginIP"></param>
        /// <param name="DestinationID"></param>
        /// <param name="demandedCapacity"></param>
        /// <param name="NCC_address"></param>
        public static void SendCallCoordination(string OriginIP, String DestinationID, string demandedCapacity, string NCC_address)
        {

            // wiadomość która będzie wysłana do kolejengo NCC
            string message = null;
            message = MessageNames.CALL_COORDINATION + "#" + OriginIP + "#" + DestinationID + "#" + demandedCapacity + "#" + NCC_address + "#";

            // pobranie adresu IP kolejnego NCC ze słownika adresów IP do wysyłania
            string ip;
            InterfaceToSend.TryGetValue("interface_to_NCC", out ip);


            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Sent Call Coordination to NCC with IP: " + ip + " (IP,ID):" + OriginIP + ", " + DestinationID + " with: " + demandedCapacity);

            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);

            // stworzenie klienta UDp i wysłanie wiadomości
            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));

        }
        /// <summary>
        /// funkcja wywoływana wtedy, gdy NCC otrzymało wiadomość CallCoordination
        /// </summary>
        /// <param name="OriginIP"></param>
        /// <param name="DestinationID"></param>
        /// <param name="demandedCapacity"></param>
        /// <param name="NCC_address"></param>
        public static void ProcessCallCoordination(string OriginIP, string DestinationID, string demandedCapacity, string NCC_address)
        {

            //sprawdzenie w Directory  i policy pojedynczego adresu docelowego 
            bool directoryaccess = dr.SingleAddressTranslation(DestinationID);
            bool policyaccess = pc.Policy(ConfigurationManager.AppSettings["policy" + number + "_path"], DestinationID);


            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Received Call Coordination from NCC with address: " + NCC_address + " (IP,ID): " + OriginIP + ", " + DestinationID + " with: " + demandedCapacity);

            string IP;
            InterfaceToListen.TryGetValue("interface_listen", out IP);

            if (policyaccess == true && directoryaccess == true)
            {
                SendCallIndication(OriginIP, dr.address, demandedCapacity, IP);
            }
        }

        public static void SendCallIndication(string OriginIP, string DestinationIP, string demandedCapacity, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;
            message = MessageNames.CALL_INDICATION + "#" + OriginIP + "#" + DestinationIP + "#" + demandedCapacity + "#" + NCC_IP + "#";

            string ip = null;
            conf.Client_with_CPCC.TryGetValue(DestinationIP, out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Indication to CPCC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP + " with: " + demandedCapacity);

            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);



            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }



        public static void ProcessCallConfirmedFromCPCC(string OriginIP, string DestinationIP, string demandedCapacity, string Client_IP)
        {
            string ip;
            conf.Client_with_CPCC.TryGetValue(Client_IP, out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Received Call Confirmed from CPCC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP + " with: " + demandedCapacity);

            string IP;
            InterfaceToListen.TryGetValue("interface_listen", out IP);


            if (OriginIP != "127.0.0.6")
            {
                if (number == 2.ToString())
                {
                    SendCallConfirmedToNCC(OriginIP, DestinationIP, demandedCapacity, IP);
                }
                else
                {
                    SendConnectionRequest(OriginIP, DestinationIP, demandedCapacity, IP);
                }
            }
            if (OriginIP == "127.0.0.6")
            {
                if (number == 1.ToString())
                {
                    SendCallConfirmedToNCC(OriginIP, DestinationIP, demandedCapacity, IP);
                }
                else
                {
                    SendConnectionRequest(OriginIP, DestinationIP, demandedCapacity, IP);
                }
            }

        }

        public static void SendCallConfirmedToNCC(string OriginIP, string DestinationIP, string demandedCapacity, string NCC2_IP)
        {

            string message = null;
            message = MessageNames.CALL_CONFIRMED + "#" + OriginIP + "#" + DestinationIP + "#" + demandedCapacity + "#" + NCC2_IP + "#" + "NCC" + "#";

            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);

            string ip;
            InterfaceToSend.TryGetValue("interface_to_NCC", out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Confirmed to NCC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP + " with: " + demandedCapacity);

            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }



        public static void ProcessCallConfirmedFromNCC(string OriginIP, string DestinationIP, string demandedCapacity, string NCC2_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Received Call Confirmed from NCC with address: " + NCC2_IP + " (IP,IP): " + OriginIP + ", " + DestinationIP + " with: " + demandedCapacity);

            string IP;
            InterfaceToListen.TryGetValue("interface_listen", out IP);

            SendConnectionRequest(OriginIP, DestinationIP, demandedCapacity, IP);

        }

        private static void SendConnectionRequest(string OriginIP, string DestinationIP, string demandedCapacity, string IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;
            string hops = dr.ChoosingHopsNumber(OriginIP, DestinationIP);
            message = IP + "#" + MessageNames.CONNECTION_REQUEST + "#" + "PUT" + "#" + "NCC" + "#" + OriginIP + "#" + DestinationIP + "#" + demandedCapacity + "#" + hops + "#";

            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);

            string ip;
            InterfaceToSend.TryGetValue("interface_to_CC", out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Connection Request to CC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP + " with capacity: " + demandedCapacity);


            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }

        private static void ProcessConnectionConfirmedFromCC(string OriginIP, string DestinationIP, string CC_IP)
        {


            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Received Connection Confirmed from CC address: " + CC_IP + " (IP,IP): " + OriginIP + ", " + DestinationIP);

            string NCC_IP;
            InterfaceToListen.TryGetValue("interface_listen", out NCC_IP);

            SendCallConfirmedToCPCC(OriginIP, DestinationIP, NCC_IP);

        }

        private static void SendCallConfirmedToCPCC(string OriginIP, string DestiantionIP, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;

            message = MessageNames.CALL_CONFIRMED + "#" + OriginIP + "#" + DestiantionIP + "#" + NCC_IP + "#";

            byte[] messagebyte = new byte[message.Length];

            messagebyte = Encoding.ASCII.GetBytes(message);

            string IP;
            conf.Client_with_CPCC.TryGetValue(OriginIP, out IP);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Confirmed to CPCC with address: " + IP + " (IP,IP): " + OriginIP + ", " + DestiantionIP);

            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, IP, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }


        ///   //// //// //// / // / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / / /

        // POD TYM będzie CALL TEARDOWN



        private void ProcessCallTeardownRequestFromCPCC(string OriginID, string DestinationID, string Capacity, string Client_IP)
        {
            string ip_CPCC;
            conf.Client_with_CPCC.TryGetValue(Client_IP, out ip_CPCC);
            // powrót do defaultowego koloru czcionki na konsoli i wypisanie logów
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Process Call Teardown from CPCC with address:" + ip_CPCC + " (ID,ID):" + OriginID + ", " + DestinationID);

            //translacja adresów odbywająca się w klasie Directory i funkcji AddressTranslation
            bool directoryaccess = dr.AddressTranslation(OriginID, DestinationID);

            //pobranie wartości adresu IP tego NCC w celu przesłania go w kolejnej wiadomości
            string NCC_address;
            InterfaceToListen.TryGetValue("interface_listen", out NCC_address);

            //jeśli oba warunki spełnione to wywołujemy funkcję SendCallCoordination
            // 
            if (directoryaccess == true && dr.OneDomain == true)
            {
                // SendConnectionTearDown(dr.OriginAddress, dr.DestinationAdddress, Capacity, NCC_address);
                SendCallTearDownTOCPCC(dr.OriginAddress, dr.DestinationAdddress, Capacity, NCC_address);
            }
            else if (directoryaccess == true && dr.OneDomain == false)
            {
                SendPeerCoordinationForTearDown(dr.OriginAddress, DestinationID, Capacity, NCC_address);
            }
        }

        private void SendPeerCoordinationForTearDown(string OriginIP, string DestinationID, string Capacity, string NCC_address)
        {

            string message = null;

            message = MessageNames.CALL_TEARDOWN + "#" + OriginIP + "#" + DestinationID + "#" + Capacity + "#" + NCC_address + "#" + "NCC" + "#" + "REQUEST" + "#";

            byte[] messagebyte = new byte[message.Length];

            messagebyte = Encoding.ASCII.GetBytes(message);

            // pobranie adresu IP kolejnego NCC ze słownika adresów IP do wysyłania
            string ip;
            InterfaceToSend.TryGetValue("interface_to_NCC", out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Teardown to NCC with address: " + ip + " (IP,ID): " + OriginIP + ", " + DestinationID);

            // stworzenie klienta UDp i wysłanie wiadomości
            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));

        }

        private void ProcessPeerCoordinationForTearDown(string OriginIP, string DestinationID, string Capacity, string NCC_address)
        {

            //sprawdzenie w Directory  i policy pojedynczego adresu docelowego 
            bool directoryaccess = dr.SingleAddressTranslation(DestinationID);


            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Received Call Teardown from NCC with address: " + NCC_address + " (IP,ID): " + OriginIP + ", " + DestinationID);

            //pobranie ze słownika adresu IP do danego CPCC( sytuacja gdy tylko z pierwszej doemny wysyłamy do drugiej)

            string IP;
            InterfaceToListen.TryGetValue("interface_to_listen", out IP);

            if (directoryaccess == true)
            {
                SendCallTearDownTOCPCC(OriginIP, dr.address, Capacity, IP);
            }
        }

        private void SendCallTearDownTOCPCC(string OriginIP, string DestinationIP, string Capacity, string NCC_address)
        {
            string message = null;
            Console.BackgroundColor = ConsoleColor.White;
            message = MessageNames.CALL_TEARDOWN + "#" + OriginIP + "#" + DestinationIP + "#" + Capacity + "#" + NCC_address + "#" + "REQUEST" + "#";

            byte[] messagebyte = new byte[message.Length];

            messagebyte = Encoding.ASCII.GetBytes(message);


            string ip = null;
            conf.Client_with_CPCC.TryGetValue(DestinationIP, out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Teardown to CPCC with address: " + ip + "(IP,IP): " + OriginIP + ", " + DestinationIP);

            // stworzenie klienta UDp i wysłanie wiadomości
            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }

        private void ProcessCalltearDownConfirmationFromCPCC(string OriginIP, string DestinationIP, string demandedCapacity, string Client_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + MessageNames.CALL_TEARDOWN_CONFIRMATION + " from Client's CPCC: " + Client_IP + "(IP,IP): " + OriginIP + ", " + DestinationIP);

            string IP;
            InterfaceToListen.TryGetValue("interface_listen", out IP);

            if (OriginIP != "127.0.0.6")
            {
                if (number == 2.ToString())
                {
                    SendCallTearDownConfirmationToNCC(OriginIP, DestinationIP, demandedCapacity, IP);
                }
                else
                {
                    SendConnectionTearDown(OriginIP, DestinationIP, demandedCapacity, IP);
                }
            }
            if (OriginIP == "127.0.0.6")
            {
                if (number == 1.ToString())
                {
                    SendCallTearDownConfirmationToNCC(OriginIP, DestinationIP, demandedCapacity, IP);
                }
                else
                {
                    SendConnectionTearDown(OriginIP, DestinationIP, demandedCapacity, IP);
                }
            }

        }

        private void SendCallTearDownConfirmationToNCC(string OriginIP, string DestinationIP, string Capacity, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;
            message = MessageNames.CALL_TEARDOWN_CONFIRMATION + "#" + OriginIP + "#" + DestinationIP + "#" + Capacity + "#" + NCC_IP + "#" + "NCC" + "#" + "OK" + "#";
            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);

            // pobranie adresu IP kolejnego NCC ze słownika adresów IP do wysyłania
            string ip;
            InterfaceToSend.TryGetValue("interface_to_NCC", out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Teardown Confirmation to NCC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP);


            // stworzenie klienta UDp i wysłanie wiadomości
            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }

        private void ProcessCallTearDownConfirmationFromNCC(string OriginIP, string DestinationIP, string Capacity, string NCC2_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Received Call Teardown Confirmation from NCC with address: " + NCC2_IP + " (IP,IP): " + OriginIP + ", " + DestinationIP);

            string IP;
            InterfaceToListen.TryGetValue("interface_listen", out IP);

            SendConnectionTearDown(OriginIP, DestinationIP, Capacity, IP);
        }

        private void SendConnectionTearDown(string OriginIP, string DestinationIP, string Capacity, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;
            string hops = dr.ChoosingHopsNumber(OriginIP, DestinationIP);
            message = NCC_IP + "#" + MessageNames.CONNECTION_TEARDOWN + "#" + "PUT" + "#" + "NCC" + "#" + OriginIP + "#" + DestinationIP + "#" + hops + "#"; ;

            byte[] messagebyte = new byte[message.Length];
            messagebyte = Encoding.ASCII.GetBytes(message);

            string ip;
            InterfaceToSend.TryGetValue("interface_to_CC", out ip);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Connection TearDown to CC with address: " + ip + " (IP,IP): " + OriginIP + ", " + DestinationIP);


            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, ip, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }



        private static void ProcessConnectionTearDownConfirmed(string OriginIP, string DestinationIP, string CC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + " Received Connection Teardown Confirmed from CC with address : " + CC_IP + " between clients with IP's: " + OriginIP + ", " + DestinationIP);

            string NCC_IP;
            InterfaceToListen.TryGetValue("interface_listen", out NCC_IP);

            SendConnectionTearDownConfirmedToCPCC(OriginIP, DestinationIP, NCC_IP);

        }

        private static void SendConnectionTearDownConfirmedToCPCC(string OriginIP, string DestiantionIP, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;

            message = MessageNames.CALL_TEARDOWN_CONFIRMATION + "#" + OriginIP + "#" + DestiantionIP + "#" + NCC_IP + "#";

            byte[] messagebyte = new byte[message.Length];

            messagebyte = Encoding.ASCII.GetBytes(message);

            string IP;
            conf.Client_with_CPCC.TryGetValue(OriginIP, out IP);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call TearDown Confirmed to CPCC with address: " + IP + " (IP,IP): " + OriginIP + ", " + DestiantionIP);


            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, IP, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }

        private void ProcessConnectionConfirmedNOTFromCC(string OriginIP, string DestinationIP, string CC_address)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "Received Connection Coonfirmed with status FAILURE from CC with address : " + CC_address + " between clients with IP's: " + OriginIP + ", " + DestinationIP);

            string NCC_IP;
            InterfaceToListen.TryGetValue("interface_listen", out NCC_IP);

            SendCallConfirmedNOTTOCPCC(OriginIP, DestinationIP, NCC_IP);

        }

        private void SendCallConfirmedNOTTOCPCC(string OriginIP, string DestinationIP, string NCC_IP)
        {
            Console.BackgroundColor = ConsoleColor.White;
            string message = null;

            message = MessageNames.CALL_CONFIRMED + "NOT" + "#" + OriginIP + "#" + DestinationIP + "#" + NCC_IP + "#";

            byte[] messagebyte = new byte[message.Length];

            messagebyte = Encoding.ASCII.GetBytes(message);

            string IP;
            conf.Client_with_CPCC.TryGetValue(OriginIP, out IP);

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "] " + "Sent Call Confirmed with status Failure to CPCC with address: " + IP + " (IP,IP): " + OriginIP + ", " + DestinationIP);

            UdpClient client = new UdpClient();
            client.Send(messagebyte, messagebyte.Length, IP, Int32.Parse(ConfigurationManager.AppSettings["port"]));
        }



        private void ListenForConnections()
        {
            Console.BackgroundColor = ConsoleColor.White;
            byte[] data = new byte[64];
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string iptolisten;
            InterfaceToListen.TryGetValue("interface_listen", out iptolisten);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(iptolisten), Int32.Parse(ConfigurationManager.AppSettings["port"]));
            UdpClient newsock = new UdpClient(ipep);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    data = newsock.Receive(ref sender);
                    if (data.Length > 0)
                    {
                        ReceivedMessage(data);
                    }
                    data = null;
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}

