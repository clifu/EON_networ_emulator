using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingTools
{
    public class MessageNames
    {
        //wszystko musi być public static string, żeby wszędzie zadziałało ;)

        //Wysyłane gdy chcemy zestawić połączenie
        public static string CALL_REQUEST = "CALLREQUEST";

        //Wysyłane do CPCC, czyli pojawia się to w nasłuchiwaniu
        public static string CALL_INDICATION = "CALLINDICATION";

        //Odsyłane do NCC po otrzymaniu CALL_INDICATION
        public static string CALL_CONFIRMED = "CALLCONFIRMED";

        //
        public static string CALL_COORDINATION = "CALLCOORDINATION";

        //
        public static string CONNECTION_REQUEST = "CONNECTIONREQUEST";

        //
        public static string CONNECTION_CONFIRMED = "CONNECTIONCONFIRMED";

        //
        public static string LINK_CONNECTION_DEALLOCATION = "LINKCONNECTIONDEALLOCATION";

        //
        public static string LINK_CONNECTION_REQUEST = "LINKCONNECTIONREQUEST";

        //
        public static string ROUTE_TABLE_QUERY = "ROUTETABLEQUERY";

        //
        public static string PEER_COORDINATION = "PEERCOORDINATION";

        public static string LOCAL_TOPOLOGY = "LOCALTOPOLOGY";

        //RC-RC
        public static string NETWORK_TOPOLOGY = "NETWORKTOPOLOGY";

        public static string NETWORK_TOPOLOGY_RESPONSE = "NETWORKTOPOLOGYRESPONSE";

        //RC-RC
        public static string ROUTE_PATH = "ROUTEPATH";

        //RC-RC w odpowiedzi na ROUTE_PATH
        public static string ROUTED_PATH = "ROUTEDPATH";

        public static string FAILED_ROUTE_TABLE_QUERY = "FAILEDROUTETABLEQUERY";

        public static string CALL_TEARDOWN = "CALLTEARDOWN";

        public static string CALL_TEARDOWN_CONFIRMATION = "CALLTEARDOWNCONFIRMED";

        public static string CONNECTION_TEARDOWN = "CONNECTIONTEARDOWN";

        public static string CONNECTION_TEARDOWN_CONFIRMED = "CONNECTIONTEARDOWNCONFIRMED";

        public static string GET_PATH = "GETPATH";

        public static string SNP_RELEASE = "SNPRELEASE";

        //Uszkodzone łącze
        public static string KILL_LINK = "KILL_LINK";
    }
}
