using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AISDE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkingTools;
using NetworkNode;
using SubNetwork;

namespace SubNetworka
{
    [TestClass]
    public class RCTester
    {
        [TestMethod]
        public void testSNPPsToString()
        {
            List<SubNetworkPointPool> snpps = new List<SubNetworkPointPool>();
            SubNetworkPoint snp1 = new SubNetworkPoint(IPAddress.Parse("127.0.0.1"), 1, 2);
            SubNetworkPoint snp2 = new SubNetworkPoint(IPAddress.Parse("127.0.0.2"), 3, 4);
            SubNetworkPoint snp3 = new SubNetworkPoint(IPAddress.Parse("127.0.0.3"), 5, 6);
            snpps.Add(new SubNetworkPointPool(snp1));
            snpps.Add(new SubNetworkPointPool(snp2));
            snpps.Add(new SubNetworkPointPool(snp3));

            RoutingController RC = new RoutingController();

            string message = RC.snppsToString(snpps);

            Assert.AreEqual("127.0.0.1#2#3#127.0.0.2#4#5#127.0.0.3", message);
        }

        [TestMethod]
        public void testReadGetPathFromRCResponse()
        {
            string stringPath = "127.0.0.1#1#127.0.0.2#2#127.0.0.3";
            RoutingController RC = new RoutingController();
            Topology topology = new Topology();
            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(1, w1, w2));
            network.krawedzie.Add(new Lacze(2, w2, w3));
            RC.GetTopology.network = new Siec(network);
            Sciezka path = new Sciezka(w1, w3);
            RC.GetTopology.network.algorytmFloyda();
            path.wyznaczSciezke(w1, w3, 2, 0, RC.GetTopology.network);
            string response = "127.0.0.41#" + MessageNames.ROUTED_PATH + "#" + stringPath;

            string generatedResponse = RC.generateGetPathFromRCResponse(path);

            Assert.AreEqual(generatedResponse, response);

        }

        [TestMethod]
        public void testGenerateGetPathResponse()
        {
            //string stringPath = "127.0.0.1#12#127.0.0.2#23#127.0.0.3";
            RoutingController RC = new RoutingController(1);
            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(12, w1, w2));
            network.krawedzie.Add(new Lacze(23, w2, w3));
            RC.GetTopology.network = network;
            RC.GetTopology.network.algorytmFloyda();
            short band = 5;
            short frequency = 6;

            Sciezka path = new Sciezka(w1, w3);
            path.wyznaczSciezke(w1, w2, band, frequency, network);
            string generatedStringPath = RoutingController.PathToString(path);

            //string getPathRequest = $"127.0.0.111#{MessageNames.GET_PATH}#{w1.ip}#{w3.ip}";

            RC.GetTopology.pathsCopy.Add(new NetworkPath(path));

            //string response = RC.generateGetPathResponse(RC.GetTopology.pathsCopy[0]);
            string righResponse = $"{RC.ip}#{MessageNames.GET_PATH}#{generatedStringPath}#{path.frequency}#{path.band}";


            //Assert.AreEqual(righResponse, response);
        }

        [TestMethod]
        public void testGenerateGetPathFromRCResponse()
        {
            string stringPath = "127.0.0.1#1#127.0.0.2#2#127.0.0.3";
            RoutingController RC = new RoutingController();
            Topology topology = new Topology();
            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(1, w1, w2));
            network.krawedzie.Add(new Lacze(2, w2, w3));
            RC.GetTopology.network = new Siec(network);
            Sciezka path = new Sciezka(w1, w3);
            RC.GetTopology.network.algorytmFloyda();
            path.wyznaczSciezke(w1, w3, 2, 0, RC.GetTopology.network);
            string response = RC.generateGetPathFromRCResponse(path);

            Assert.AreEqual(RC.ip + "#" + MessageNames.ROUTED_PATH + "#" + stringPath, response);

        }

        [TestMethod]
        public void testGenerateNetworkTopologyMessage()
        {
            short band = 5;
            short frequency = 1;
            SubNetworkPointPool snppFrom = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse("127.0.0.1")));
            SubNetworkPointPool snppTo = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse("127.0.0.2")));

            RoutingController RC1 = new RoutingController(1);

            string message = RC1.generateNetworkTopologyMessage(snppFrom, snppTo, band, frequency);

            Assert.AreEqual(ConfigurationManager.AppSettings["RC" + RC1.numberRC] + "#" + MessageNames.NETWORK_TOPOLOGY
                            + "#127.0.0.1#127.0.0.2#5#1", message);
        }

        [TestMethod]
        public void testHandleGetPath()
        {
            string stringPath = "127.0.0.1#1#127.0.0.2#2#127.0.0.3";
            RoutingController RC = new RoutingController();
            Topology topology = new Topology();
            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(1, w1, w2));
            network.krawedzie.Add(new Lacze(2, w2, w3));
            RC.GetTopology.network = new Siec(network);
            Sciezka path = new Sciezka(w1, w3);
            RC.GetTopology.network.algorytmFloyda();
            short band = 5;
            short frequency = 6;
            path.wyznaczSciezke(w1, w3, band, frequency, RC.GetTopology.network);
            RC.GetTopology.pathsCopy.Add(new NetworkPath(path));
            RC.Run();
            //IP RC1
            IPEndPoint rcEndPoint = new IPEndPoint(RC.ip, RC.portNumber);

            //IP clienta
            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            string request = $"{IendPoint.Address}#{MessageNames.GET_PATH}#127.0.0.1#127.0.0.3";

            //Wysłanie wiadomości do RC
            var requestBytes = Encoding.ASCII.GetBytes(request);
            client.Send(requestBytes, requestBytes.Length, rcEndPoint);

            //Odebranie odpowiedzi
            string response = Encoding.ASCII.GetString(client.Receive(ref rcEndPoint));

            Assert.AreEqual(RC.ip + "#" + MessageNames.GET_PATH + "#" + stringPath + $"#{frequency}#{band}", response);
        }

        [TestMethod]
        public void testHandleNetworkTopology()
        {
            RoutingController RC1 = new RoutingController(1);
            RoutingController RC2 = new RoutingController(2);

            Topology topology1 = new Topology();
            Topology topology2 = new Topology();

            Siec network1 = new Siec();
            Siec network2 = new Siec();

            //Podsiec 1
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w4 = new Wezel(4, "127.0.0.4");
            Wezel w23 = new Wezel(23, "127.0.0.23");

            //Podsiec 2
            Wezel w1_p2 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            Wezel w4_p2 = new Wezel(4, "127.0.0.4");

            //Topologia ogólna:
            //1-(2-3)-4, w nawiasach podsiec 2
            //RC1 widzi to tak: 1-23-4
            //RC2 widzi to tak: 1-2-3-4

            //Podsiec 1
            Lacze l123 = new Lacze(123, w1, w23);
            Lacze l234 = new Lacze(234, w23, w4);

            //Podsiec 2
            Lacze l12 = new Lacze(12, w1_p2, w2);
            Lacze l23 = new Lacze(23, w2, w3);
            Lacze l34 = new Lacze(34, w3, w4_p2);

            network1.krawedzie.AddRange(new List<Lacze>() { l123, l234 });
            network1.wezly.AddRange(new List<Wezel>() { w1, w23, w4 });
            topology1.network = network1;
            RC1.GetTopology = topology1;

            network2.krawedzie.AddRange(new List<Lacze>() { l12, l23, l34 });
            network2.wezly.AddRange(new List<Wezel>() { w1_p2, w2, w3, w4_p2 });
            topology2.network = network2;
            RC2.GetTopology = topology2;

            RC2.Run();
            //RC1.Run();

            //Generowanie sciezki
            //TODO 
            var routedPath = RC1.NetworkTopologyRequest(RC2.ip.ToString(), new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w1.ip))),
                new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w4.ip))), 5, 0);

            Assert.AreEqual(w1.ip, routedPath.path.WezlySciezki[0].ip);
            Assert.AreEqual(w2.ip, routedPath.path.WezlySciezki[1].ip);
            Assert.AreEqual(w3.ip, routedPath.path.WezlySciezki[2].ip);
            Assert.AreEqual(w4.ip, routedPath.path.WezlySciezki[3].ip);
        }

        [TestMethod]
        public void testHandleNetworkTopologyResponse()
        {
            string stringPath = "127.0.0.1#1#127.0.0.2#2#127.0.0.3";
            RoutingController RC = new RoutingController();
            Topology topology = new Topology();
            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(1, w1, w2));
            network.krawedzie.Add(new Lacze(2, w2, w3));
            RC.GetTopology.network = new Siec(network);
            Sciezka path = new Sciezka(w1, w3);
            RC.GetTopology.network.algorytmFloyda();
            path.wyznaczSciezke(w1, w3, 2, 0, RC.GetTopology.network);
            string response = "127.0.0.41#" + MessageNames.NETWORK_TOPOLOGY + "#" + stringPath;

            string generatedResponse = RC.generateNetworkTopologyRCesponse(path);

            Assert.AreEqual(generatedResponse, response);
        }

        [TestMethod]
        public void testHandleLocalTopology()
        {
            RoutingController RC1 = new RoutingController(1);

            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");

            Lacze l12 = new Lacze(12, w1, w2);

            Siec network = new Siec();
            network.krawedzie.Add(l12);
            network.wezly.AddRange(new List<Wezel>() { w1, w2 });

            RC1.GetTopology.network = network;

            RC1.Run();


            //IP RC1
            IPEndPoint rcEndPoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            //IP clienta
            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            //============================================ALLOCATION===============================================
            //============================================OUT=======================================================

            string localTopologyMessage = "127.0.0.111" + "#" + MessageNames.LOCAL_TOPOLOGY + "#" + "ALLOCATION" + "#" +
                w1.ip + "#" + w1.SNPP.snps[0].portOUT + "#" + "out" + "#" + 5 + "#" + 0;

            byte[] mesasgeBytes = Encoding.ASCII.GetBytes(localTopologyMessage);

            client.Send(mesasgeBytes, mesasgeBytes.Length, rcEndPoint);

            //var freeOutFrequencies = new List<short>(EONTable.capacity);

            short frequency = 0;
            short band = 5;

            //Czekanie
            for (int i = 0; i < 1000000000; i++) ;


            //Alokacja na zerowej częstotliwości pasma równego 5
            for (short i = frequency; i < band; i++)
            {
                //freeOutFrequencies.Add(frequency);
                Assert.AreEqual(frequency, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.OutFrequencies[i]);
            }

            //A reszta jest wolna
            for (short i = band; i < EONTable.capacity; i++)
            {
                //freeOutFrequencies.Add(-1);
                Assert.AreEqual(-1, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.OutFrequencies[i]);
            }

            EONTableRowOut rowOut = new EONTableRowOut(frequency, band);

            Assert.AreEqual(rowOut.busyBandOUT, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.TableOut[0].busyBandOUT);
            Assert.AreEqual(rowOut.busyFrequency, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.TableOut[0].busyFrequency);

            //==============================IN======================================

            string localTopologyMessage2 = "127.0.0.111" + "#" + MessageNames.LOCAL_TOPOLOGY + "#" + "ALLOCATION" + "#" +
                                          w2.ip + "#" + w2.SNPP.snps[0].portIN + "#" + "in" + "#" + 5 + "#" + 0;

            byte[] mesasgeBytes2 = Encoding.ASCII.GetBytes(localTopologyMessage2);

            client.Send(mesasgeBytes2, mesasgeBytes2.Length, rcEndPoint);

            //var freeInFrequencies = new List<short>(64);

            //Czekanie
            for (int i = 0; i < 1000000000; i++) ;

            //Alokacja na zerowej częstotliwości pasma równego 5
            for (short i = frequency; i < band; i++)
            {
                //freeInFrequencies.Add(frequency);
                Assert.AreEqual(frequency, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.InFrequencies[i]);
            }

            //A reszta jest wolna
            for (short i = band; i < EONTable.capacity; i++)
            {
                //freeInFrequencies.Add(-1);
                Assert.AreEqual(-1, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.InFrequencies[i]);
            }

            EONTableRowIN rowIn = new EONTableRowIN(frequency, band);

            Assert.AreEqual(rowIn.busyBandIN, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.TableIN[0].busyBandIN);
            Assert.AreEqual(rowIn.busyFrequency, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.TableIN[0].busyFrequency);

            //====================================DEALLOCATION=========================================================================
            //====================================OUT==================================================================================

            //Wyslanie polecenia dealokacji do RC
            string localTopologyMessage3 = "127.0.0.111" + "#" + MessageNames.LOCAL_TOPOLOGY + "#" + "DEALLOCATION" + "#" +
                                          w1.ip + "#" + w1.SNPP.snps[0].portOUT + "#" + "out" + "#" + 5 + "#" + 0;

            byte[] mesasgeBytes3 = Encoding.ASCII.GetBytes(localTopologyMessage3);

            client.Send(mesasgeBytes3, mesasgeBytes3.Length, rcEndPoint);

            //Czekanie
            for (int i = 0; i < 1000000000; i++) ;

            //Zasoby powinny byc zdealokowane
            for (int i = 0; i < EONTable.capacity; i++)
            {
                Assert.AreEqual(-1, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.OutFrequencies[i]);
            }

            Assert.AreEqual(0, RC1.GetTopology.network.wezly[0].SNPP.snps[0].eonTable.TableOut.Count);

            //==============================IN======================================

            string localTopologyMessage4 = "127.0.0.111" + "#" + MessageNames.LOCAL_TOPOLOGY + "#" + "ALLOCATION" + "#" +
                                           w2.ip + "#" + w2.SNPP.snps[1].portIN + "#" + "in" + "#" + 5 + "#" + 0;

            byte[] mesasgeBytes4 = Encoding.ASCII.GetBytes(localTopologyMessage4);

            client.Send(mesasgeBytes4, mesasgeBytes4.Length, rcEndPoint);

            //Czekanie
            for (int i = 0; i < 1000000000; i++) ;

            //Zasoby powinny byc zdealokowane
            for (int i = 0; i < EONTable.capacity; i++)
            {
                Assert.AreEqual(-1, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.InFrequencies[i]);
            }

            Assert.AreEqual(0, RC1.GetTopology.network.wezly[1].SNPP.snps[0].eonTable.TableIN.Count);
        }

        [TestMethod]
        public void testReadSnAndRCsFromFile()
        {
            RoutingController RC1 = new RoutingController(1);

            var SN_RCs = new List<SNandRC>();

            SN_RCs.Add(new SNandRC("127.0.0.61", "127.0.0.35"));
            SN_RCs.Add(new SNandRC("127.0.0.44", "127.0.0.7"));

            Assert.AreEqual("127.0.0.61", SN_RCs[0].RC_IP.ToString());
            Assert.AreEqual("127.0.0.44", SN_RCs[1].RC_IP.ToString());

            Assert.AreEqual("127.0.0.35", SN_RCs[0].snpp.snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.7", SN_RCs[1].snpp.snps[0].ipaddress.ToString());
        }

        [TestMethod]
        public void testPath2StringAndStringToPath()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Lacze link12 = new Lacze(1, w1, w2, 1);

            Siec network = new Siec();

            network.krawedzie.Add(link12);
            network.wezly.Add(w1);
            network.wezly.Add(w2);

            Sciezka path = new Sciezka(w1, w2);

            path.KrawedzieSciezki.Add(link12);
            path.WezlySciezki.AddRange(new List<Wezel>() { w1, w2 });

            network.algorytmFloyda();

            network.sciezki.Add(path);

            path.wyznaczSciezke(w1, w2, network.zwrocTabliceKierowaniaLaczami, network.zwrocTabliceKierowaniaWezlami,
                ref network.wezly, 1, network.Koszty, 5);

            string stringPath = RoutingController.PathToString(path);

            Assert.AreEqual("127.0.0.1#1#127.0.0.2", stringPath);

            Sciezka derivativePath = RoutingController.stringToPath(stringPath);

            Assert.AreEqual(path.Wezel1.ip, derivativePath.Wezel1.ip);
            Assert.AreEqual(path.Wezel2.ip, derivativePath.Wezel2.ip);
            Assert.AreEqual(path.KrawedzieSciezki[0].idKrawedzi, derivativePath.KrawedzieSciezki[0].idKrawedzi);
            Assert.AreEqual(path.KrawedzieSciezki[0].Wezel1.ip, derivativePath.KrawedzieSciezki[0].Wezel1.ip);
            Assert.AreEqual(path.KrawedzieSciezki[0].Wezel2.ip, derivativePath.KrawedzieSciezki[0].Wezel2.ip);
        }

        [TestMethod]
        public void testReadFromConfig()
        {
            //Tworzenie Routing Controllerów o id 1, 2 i 3
            RoutingController RC1 = new RoutingController(1);
            RoutingController RC2 = new RoutingController(2);
            RoutingController RC3 = new RoutingController(3);

            //WĘZŁY RC1
            Assert.AreEqual("127.0.0.2", RC1.GetTopology.network.wezly[0].ip);
            Assert.AreEqual(2, RC1.GetTopology.network.wezly[0].idWezla);
            Assert.AreEqual("127.0.0.31", RC1.GetTopology.network.wezly[1].ip);
            Assert.AreEqual(31, RC1.GetTopology.network.wezly[1].idWezla);
            Assert.AreEqual("127.0.0.7", RC1.GetTopology.network.wezly[2].ip);
            Assert.AreEqual(357, RC1.GetTopology.network.wezly[2].idWezla);
            Assert.AreEqual("127.0.0.4", RC1.GetTopology.network.wezly[3].ip);
            Assert.AreEqual(4, RC1.GetTopology.network.wezly[3].idWezla);
            Assert.AreEqual("127.0.0.33", RC1.GetTopology.network.wezly[4].ip);
            Assert.AreEqual(33, RC1.GetTopology.network.wezly[4].idWezla);
            Assert.AreEqual("127.0.0.35", RC1.GetTopology.network.wezly[5].ip);
            Assert.AreEqual(35, RC1.GetTopology.network.wezly[5].idWezla);
            Assert.AreEqual("127.0.0.6", RC1.GetTopology.network.wezly[6].ip);
            Assert.AreEqual(6, RC1.GetTopology.network.wezly[6].idWezla);

            //WĘZŁY RC2
            Assert.AreEqual("127.0.0.2", RC2.GetTopology.network.wezly[0].ip);
            Assert.AreEqual(2, RC2.GetTopology.network.wezly[0].idWezla);
            Assert.AreEqual("127.0.0.31", RC2.GetTopology.network.wezly[1].ip);
            Assert.AreEqual(31, RC2.GetTopology.network.wezly[1].idWezla);
            Assert.AreEqual("127.0.0.3", RC2.GetTopology.network.wezly[2].ip);
            Assert.AreEqual(3, RC2.GetTopology.network.wezly[2].idWezla);
            Assert.AreEqual("127.0.0.5", RC2.GetTopology.network.wezly[3].ip);
            Assert.AreEqual(5, RC2.GetTopology.network.wezly[3].idWezla);
            Assert.AreEqual("127.0.0.7", RC2.GetTopology.network.wezly[4].ip);
            Assert.AreEqual(7, RC2.GetTopology.network.wezly[4].idWezla);
            Assert.AreEqual("127.0.0.35", RC2.GetTopology.network.wezly[5].ip);
            Assert.AreEqual(35, RC2.GetTopology.network.wezly[5].idWezla);
            Assert.AreEqual("127.0.0.4", RC2.GetTopology.network.wezly[6].ip);
            Assert.AreEqual(4, RC2.GetTopology.network.wezly[6].idWezla);
            Assert.AreEqual("127.0.0.33", RC2.GetTopology.network.wezly[7].ip);
            Assert.AreEqual(33, RC2.GetTopology.network.wezly[7].idWezla);
            Assert.AreEqual("127.0.0.6", RC2.GetTopology.network.wezly[8].ip);
            Assert.AreEqual(6, RC2.GetTopology.network.wezly[8].idWezla);

            //WĘZŁY RC3
            Assert.AreEqual("127.0.0.2", RC3.GetTopology.network.wezly[0].ip);
            Assert.AreEqual(2, RC3.GetTopology.network.wezly[0].idWezla);
            Assert.AreEqual("127.0.0.4", RC3.GetTopology.network.wezly[1].ip);
            Assert.AreEqual(4, RC3.GetTopology.network.wezly[1].idWezla);
            Assert.AreEqual("127.0.0.7", RC3.GetTopology.network.wezly[2].ip);
            Assert.AreEqual(7, RC3.GetTopology.network.wezly[2].idWezla);
            Assert.AreEqual("127.0.0.33", RC3.GetTopology.network.wezly[3].ip);
            Assert.AreEqual(33, RC3.GetTopology.network.wezly[3].idWezla);
            Assert.AreEqual("127.0.0.35", RC3.GetTopology.network.wezly[4].ip);
            Assert.AreEqual(35, RC3.GetTopology.network.wezly[4].idWezla);
            Assert.AreEqual("127.0.0.37", RC3.GetTopology.network.wezly[5].ip);
            Assert.AreEqual(37, RC3.GetTopology.network.wezly[5].idWezla);
            Assert.AreEqual("127.0.0.6", RC3.GetTopology.network.wezly[6].ip);
            Assert.AreEqual(6, RC3.GetTopology.network.wezly[6].idWezla);


            //ŁĄCZA RC1
            Lacze l2311 = new Lacze(2311, RC1.GetTopology.network.wezly[0], RC1.GetTopology.network.wezly[1]);
            Assert.AreEqual(l2311.Wezel1.ip, RC1.GetTopology.network.krawedzie[0].Wezel1.ip);
            Assert.AreEqual(l2311.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[0].Wezel1.idWezla);
            Assert.AreEqual(l2311.Wezel2.ip, RC1.GetTopology.network.krawedzie[0].Wezel2.ip);
            Assert.AreEqual(l2311.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[0].Wezel2.idWezla);

            Lacze l2312 = new Lacze(2312, RC1.GetTopology.network.wezly[1], RC1.GetTopology.network.wezly[0]);
            Assert.AreEqual(l2312.Wezel1.ip, RC1.GetTopology.network.krawedzie[1].Wezel1.ip);
            Assert.AreEqual(l2312.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[1].Wezel1.idWezla);
            Assert.AreEqual(l2312.Wezel2.ip, RC1.GetTopology.network.krawedzie[1].Wezel2.ip);
            Assert.AreEqual(l2312.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[1].Wezel2.idWezla);

            Lacze l3171 = new Lacze(3171, RC1.GetTopology.network.wezly[1], RC1.GetTopology.network.wezly[2]);
            Assert.AreEqual(l3171.Wezel1.ip, RC1.GetTopology.network.krawedzie[2].Wezel1.ip);
            Assert.AreEqual(l3171.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[2].Wezel1.idWezla);
            Assert.AreEqual(l3171.Wezel2.ip, RC1.GetTopology.network.krawedzie[2].Wezel2.ip);
            Assert.AreEqual(l3171.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[2].Wezel2.idWezla);

            Lacze l3172 = new Lacze(3172, RC1.GetTopology.network.wezly[2], RC1.GetTopology.network.wezly[1]);
            Assert.AreEqual(l3172.Wezel1.ip, RC1.GetTopology.network.krawedzie[3].Wezel1.ip);
            Assert.AreEqual(l3172.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[3].Wezel1.idWezla);
            Assert.AreEqual(l3172.Wezel2.ip, RC1.GetTopology.network.krawedzie[3].Wezel2.ip);
            Assert.AreEqual(l3172.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[3].Wezel2.idWezla);

            Lacze l471 = new Lacze(471, RC1.GetTopology.network.wezly[2], RC1.GetTopology.network.wezly[3]);
            Assert.AreEqual(l471.Wezel1.ip, RC1.GetTopology.network.krawedzie[4].Wezel1.ip);
            Assert.AreEqual(l471.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[4].Wezel1.idWezla);
            Assert.AreEqual(l471.Wezel2.ip, RC1.GetTopology.network.krawedzie[4].Wezel2.ip);
            Assert.AreEqual(l471.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[4].Wezel2.idWezla);

            Lacze l472 = new Lacze(472, RC1.GetTopology.network.wezly[3], RC1.GetTopology.network.wezly[2]);
            Assert.AreEqual(l472.Wezel1.ip, RC1.GetTopology.network.krawedzie[5].Wezel1.ip);
            Assert.AreEqual(l472.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[5].Wezel1.idWezla);
            Assert.AreEqual(l472.Wezel2.ip, RC1.GetTopology.network.krawedzie[5].Wezel2.ip);
            Assert.AreEqual(l472.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[5].Wezel2.idWezla);

            Lacze l3571 = new Lacze(3571, RC1.GetTopology.network.wezly[2], RC1.GetTopology.network.wezly[5]);
            Assert.AreEqual(l3571.Wezel1.ip, RC1.GetTopology.network.krawedzie[6].Wezel1.ip);
            Assert.AreEqual(l3571.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[6].Wezel1.idWezla);
            Assert.AreEqual(l3571.Wezel2.ip, RC1.GetTopology.network.krawedzie[6].Wezel2.ip);
            Assert.AreEqual(l3571.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[6].Wezel2.idWezla);

            Lacze l3572 = new Lacze(3572, RC1.GetTopology.network.wezly[5], RC1.GetTopology.network.wezly[2]);
            Assert.AreEqual(l3572.Wezel1.ip, RC1.GetTopology.network.krawedzie[7].Wezel1.ip);
            Assert.AreEqual(l3572.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[7].Wezel1.idWezla);
            Assert.AreEqual(l3572.Wezel2.ip, RC1.GetTopology.network.krawedzie[7].Wezel2.ip);
            Assert.AreEqual(l3572.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[7].Wezel2.idWezla);

            Lacze l33351 = new Lacze(33351, RC1.GetTopology.network.wezly[4], RC1.GetTopology.network.wezly[5]);
            Assert.AreEqual(l33351.Wezel1.ip, RC1.GetTopology.network.krawedzie[8].Wezel1.ip);
            Assert.AreEqual(l33351.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[8].Wezel1.idWezla);
            Assert.AreEqual(l33351.Wezel2.ip, RC1.GetTopology.network.krawedzie[8].Wezel2.ip);
            Assert.AreEqual(l33351.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[8].Wezel2.idWezla);

            Lacze l33352 = new Lacze(33352, RC1.GetTopology.network.wezly[5], RC1.GetTopology.network.wezly[4]);
            Assert.AreEqual(l33352.Wezel1.ip, RC1.GetTopology.network.krawedzie[9].Wezel1.ip);
            Assert.AreEqual(l33352.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[9].Wezel1.idWezla);
            Assert.AreEqual(l33352.Wezel2.ip, RC1.GetTopology.network.krawedzie[9].Wezel2.ip);
            Assert.AreEqual(l33352.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[9].Wezel2.idWezla);

            Lacze l3561 = new Lacze(3561, RC1.GetTopology.network.wezly[5], RC1.GetTopology.network.wezly[6]);
            Assert.AreEqual(l3561.Wezel1.ip, RC1.GetTopology.network.krawedzie[10].Wezel1.ip);
            Assert.AreEqual(l3561.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[10].Wezel1.idWezla);
            Assert.AreEqual(l3561.Wezel2.ip, RC1.GetTopology.network.krawedzie[10].Wezel2.ip);
            Assert.AreEqual(l3561.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[10].Wezel2.idWezla);

            Lacze l3562 = new Lacze(3562, RC1.GetTopology.network.wezly[6], RC1.GetTopology.network.wezly[5]);
            Assert.AreEqual(l3562.Wezel1.ip, RC1.GetTopology.network.krawedzie[11].Wezel1.ip);
            Assert.AreEqual(l3562.Wezel1.idWezla, RC1.GetTopology.network.krawedzie[11].Wezel1.idWezla);
            Assert.AreEqual(l3562.Wezel2.ip, RC1.GetTopology.network.krawedzie[11].Wezel2.ip);
            Assert.AreEqual(l3562.Wezel2.idWezla, RC1.GetTopology.network.krawedzie[11].Wezel2.idWezla);

            //ŁĄCZA RC2
            Lacze l2311r2 = new Lacze(2311, RC2.GetTopology.network.wezly[0], RC2.GetTopology.network.wezly[1]);
            Assert.AreEqual(l2311r2.Wezel1.ip, RC2.GetTopology.network.krawedzie[0].Wezel1.ip);
            Assert.AreEqual(l2311r2.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[0].Wezel1.idWezla);
            Assert.AreEqual(l2311r2.Wezel2.ip, RC2.GetTopology.network.krawedzie[0].Wezel2.ip);
            Assert.AreEqual(l2311r2.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[0].Wezel2.idWezla);

            Lacze l2312r2 = new Lacze(2312, RC2.GetTopology.network.wezly[1], RC2.GetTopology.network.wezly[0]);
            Assert.AreEqual(l2312r2.Wezel1.ip, RC2.GetTopology.network.krawedzie[1].Wezel1.ip);
            Assert.AreEqual(l2312r2.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[1].Wezel1.idWezla);
            Assert.AreEqual(l2312r2.Wezel2.ip, RC2.GetTopology.network.krawedzie[1].Wezel2.ip);
            Assert.AreEqual(l2312r2.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[1].Wezel2.idWezla);

            Lacze l3131 = new Lacze(3131, RC2.GetTopology.network.wezly[1], RC2.GetTopology.network.wezly[2]);
            Assert.AreEqual(l3131.Wezel1.ip, RC2.GetTopology.network.krawedzie[2].Wezel1.ip);
            Assert.AreEqual(l3131.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[2].Wezel1.idWezla);
            Assert.AreEqual(l3131.Wezel2.ip, RC2.GetTopology.network.krawedzie[2].Wezel2.ip);
            Assert.AreEqual(l3131.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[2].Wezel2.idWezla);

            Lacze l3132 = new Lacze(3132, RC2.GetTopology.network.wezly[2], RC2.GetTopology.network.wezly[1]);
            Assert.AreEqual(l3132.Wezel1.ip, RC2.GetTopology.network.krawedzie[3].Wezel1.ip);
            Assert.AreEqual(l3132.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[3].Wezel1.idWezla);
            Assert.AreEqual(l3132.Wezel2.ip, RC2.GetTopology.network.krawedzie[3].Wezel2.ip);
            Assert.AreEqual(l3132.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[3].Wezel2.idWezla);

            Lacze l351 = new Lacze(351, RC2.GetTopology.network.wezly[3], RC2.GetTopology.network.wezly[2]);
            Assert.AreEqual(l351.Wezel1.ip, RC2.GetTopology.network.krawedzie[4].Wezel1.ip);
            Assert.AreEqual(l351.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[4].Wezel1.idWezla);
            Assert.AreEqual(l351.Wezel2.ip, RC2.GetTopology.network.krawedzie[4].Wezel2.ip);
            Assert.AreEqual(l351.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[4].Wezel2.idWezla);

            Lacze l352 = new Lacze(352, RC2.GetTopology.network.wezly[2], RC2.GetTopology.network.wezly[3]);
            Assert.AreEqual(l352.Wezel1.ip, RC2.GetTopology.network.krawedzie[5].Wezel1.ip);
            Assert.AreEqual(l352.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[5].Wezel1.idWezla);
            Assert.AreEqual(l352.Wezel2.ip, RC2.GetTopology.network.krawedzie[5].Wezel2.ip);
            Assert.AreEqual(l352.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[5].Wezel2.idWezla);

            Lacze l371 = new Lacze(371, RC2.GetTopology.network.wezly[4], RC2.GetTopology.network.wezly[2]);
            Assert.AreEqual(l371.Wezel1.ip, RC2.GetTopology.network.krawedzie[6].Wezel1.ip);
            Assert.AreEqual(l371.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[6].Wezel1.idWezla);
            Assert.AreEqual(l371.Wezel2.ip, RC2.GetTopology.network.krawedzie[6].Wezel2.ip);
            Assert.AreEqual(l371.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[6].Wezel2.idWezla);

            Lacze l372 = new Lacze(372, RC2.GetTopology.network.wezly[2], RC2.GetTopology.network.wezly[4]);
            Assert.AreEqual(l372.Wezel1.ip, RC2.GetTopology.network.krawedzie[7].Wezel1.ip);
            Assert.AreEqual(l372.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[7].Wezel1.idWezla);
            Assert.AreEqual(l372.Wezel2.ip, RC2.GetTopology.network.krawedzie[7].Wezel2.ip);
            Assert.AreEqual(l372.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[7].Wezel2.idWezla);

            Lacze l571 = new Lacze(571, RC2.GetTopology.network.wezly[3], RC2.GetTopology.network.wezly[4]);
            Assert.AreEqual(l571.Wezel1.ip, RC2.GetTopology.network.krawedzie[8].Wezel1.ip);
            Assert.AreEqual(l571.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[8].Wezel1.idWezla);
            Assert.AreEqual(l571.Wezel2.ip, RC2.GetTopology.network.krawedzie[8].Wezel2.ip);
            Assert.AreEqual(l571.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[8].Wezel2.idWezla);

            Lacze l572 = new Lacze(572, RC2.GetTopology.network.wezly[4], RC2.GetTopology.network.wezly[3]);
            Assert.AreEqual(l572.Wezel1.ip, RC2.GetTopology.network.krawedzie[9].Wezel1.ip);
            Assert.AreEqual(l572.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[9].Wezel1.idWezla);
            Assert.AreEqual(l572.Wezel2.ip, RC2.GetTopology.network.krawedzie[9].Wezel2.ip);
            Assert.AreEqual(l572.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[9].Wezel2.idWezla);

            Lacze l451 = new Lacze(451, RC2.GetTopology.network.wezly[6], RC2.GetTopology.network.wezly[3]);
            Assert.AreEqual(l451.Wezel1.ip, RC2.GetTopology.network.krawedzie[10].Wezel1.ip);
            Assert.AreEqual(l451.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[10].Wezel1.idWezla);
            Assert.AreEqual(l451.Wezel2.ip, RC2.GetTopology.network.krawedzie[10].Wezel2.ip);
            Assert.AreEqual(l451.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[10].Wezel2.idWezla);

            Lacze l452 = new Lacze(452, RC2.GetTopology.network.wezly[3], RC2.GetTopology.network.wezly[6]);
            Assert.AreEqual(l452.Wezel1.ip, RC2.GetTopology.network.krawedzie[11].Wezel1.ip);
            Assert.AreEqual(l452.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[11].Wezel1.idWezla);
            Assert.AreEqual(l452.Wezel2.ip, RC2.GetTopology.network.krawedzie[11].Wezel2.ip);
            Assert.AreEqual(l452.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[11].Wezel2.idWezla);

            Lacze l5331 = new Lacze(5331, RC2.GetTopology.network.wezly[7], RC2.GetTopology.network.wezly[3]);
            Assert.AreEqual(l5331.Wezel1.ip, RC2.GetTopology.network.krawedzie[12].Wezel1.ip);
            Assert.AreEqual(l5331.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[12].Wezel1.idWezla);
            Assert.AreEqual(l5331.Wezel2.ip, RC2.GetTopology.network.krawedzie[12].Wezel2.ip);
            Assert.AreEqual(l5331.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[12].Wezel2.idWezla);

            Lacze l5332 = new Lacze(5332, RC2.GetTopology.network.wezly[3], RC2.GetTopology.network.wezly[7]);
            Assert.AreEqual(l5332.Wezel1.ip, RC2.GetTopology.network.krawedzie[13].Wezel1.ip);
            Assert.AreEqual(l5332.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[13].Wezel1.idWezla);
            Assert.AreEqual(l5332.Wezel2.ip, RC2.GetTopology.network.krawedzie[13].Wezel2.ip);
            Assert.AreEqual(l5332.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[13].Wezel2.idWezla);

            Lacze l3361 = new Lacze(3361, RC2.GetTopology.network.wezly[7], RC2.GetTopology.network.wezly[8]);
            Assert.AreEqual(l3361.Wezel1.ip, RC2.GetTopology.network.krawedzie[14].Wezel1.ip);
            Assert.AreEqual(l3361.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[14].Wezel1.idWezla);
            Assert.AreEqual(l3361.Wezel2.ip, RC2.GetTopology.network.krawedzie[14].Wezel2.ip);
            Assert.AreEqual(l3361.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[14].Wezel2.idWezla);

            Lacze l3362 = new Lacze(3362, RC2.GetTopology.network.wezly[8], RC2.GetTopology.network.wezly[7]);
            Assert.AreEqual(l3362.Wezel1.ip, RC2.GetTopology.network.krawedzie[15].Wezel1.ip);
            Assert.AreEqual(l3362.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[15].Wezel1.idWezla);
            Assert.AreEqual(l3362.Wezel2.ip, RC2.GetTopology.network.krawedzie[15].Wezel2.ip);
            Assert.AreEqual(l3362.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[15].Wezel2.idWezla);

            Lacze l7351 = new Lacze(7351, RC2.GetTopology.network.wezly[4], RC2.GetTopology.network.wezly[5]);
            Assert.AreEqual(l7351.Wezel1.ip, RC2.GetTopology.network.krawedzie[16].Wezel1.ip);
            Assert.AreEqual(l7351.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[16].Wezel1.idWezla);
            Assert.AreEqual(l7351.Wezel2.ip, RC2.GetTopology.network.krawedzie[16].Wezel2.ip);
            Assert.AreEqual(l7351.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[16].Wezel2.idWezla);

            Lacze l7352 = new Lacze(7352, RC2.GetTopology.network.wezly[5], RC2.GetTopology.network.wezly[4]);
            Assert.AreEqual(l7352.Wezel1.ip, RC2.GetTopology.network.krawedzie[17].Wezel1.ip);
            Assert.AreEqual(l7352.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[17].Wezel1.idWezla);
            Assert.AreEqual(l7352.Wezel2.ip, RC2.GetTopology.network.krawedzie[17].Wezel2.ip);
            Assert.AreEqual(l7352.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[17].Wezel2.idWezla);

            Lacze l3561r2 = new Lacze(3561, RC2.GetTopology.network.wezly[8], RC2.GetTopology.network.wezly[5]);
            Assert.AreEqual(l3561r2.Wezel1.ip, RC2.GetTopology.network.krawedzie[18].Wezel1.ip);
            Assert.AreEqual(l3561r2.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[18].Wezel1.idWezla);
            Assert.AreEqual(l3561r2.Wezel2.ip, RC2.GetTopology.network.krawedzie[18].Wezel2.ip);
            Assert.AreEqual(l3561r2.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[18].Wezel2.idWezla);

            Lacze l3652r2 = new Lacze(3561, RC2.GetTopology.network.wezly[5], RC2.GetTopology.network.wezly[8]);
            Assert.AreEqual(l3652r2.Wezel1.ip, RC2.GetTopology.network.krawedzie[19].Wezel1.ip);
            Assert.AreEqual(l3652r2.Wezel1.idWezla, RC2.GetTopology.network.krawedzie[19].Wezel1.idWezla);
            Assert.AreEqual(l3652r2.Wezel2.ip, RC2.GetTopology.network.krawedzie[19].Wezel2.ip);
            Assert.AreEqual(l3652r2.Wezel2.idWezla, RC2.GetTopology.network.krawedzie[19].Wezel2.idWezla);

            //ŁĄCZA RC3
            Lacze l741 = new Lacze(741, RC3.GetTopology.network.wezly[1], RC3.GetTopology.network.wezly[2]);
            Assert.AreEqual(l741.Wezel1.ip, RC3.GetTopology.network.krawedzie[0].Wezel1.ip);
            Assert.AreEqual(l741.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[0].Wezel1.idWezla);
            Assert.AreEqual(l741.Wezel2.ip, RC3.GetTopology.network.krawedzie[0].Wezel2.ip);
            Assert.AreEqual(l741.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[0].Wezel2.idWezla);

            Lacze l742 = new Lacze(742, RC3.GetTopology.network.wezly[2], RC3.GetTopology.network.wezly[1]);
            Assert.AreEqual(l742.Wezel1.ip, RC3.GetTopology.network.krawedzie[1].Wezel1.ip);
            Assert.AreEqual(l742.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[1].Wezel1.idWezla);
            Assert.AreEqual(l742.Wezel2.ip, RC3.GetTopology.network.krawedzie[1].Wezel2.ip);
            Assert.AreEqual(l742.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[1].Wezel2.idWezla);

            Lacze l271 = new Lacze(271, RC3.GetTopology.network.wezly[0], RC3.GetTopology.network.wezly[2]);
            Assert.AreEqual(l271.Wezel1.ip, RC3.GetTopology.network.krawedzie[2].Wezel1.ip);
            Assert.AreEqual(l271.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[2].Wezel1.idWezla);
            Assert.AreEqual(l271.Wezel2.ip, RC3.GetTopology.network.krawedzie[2].Wezel2.ip);
            Assert.AreEqual(l271.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[2].Wezel2.idWezla);

            Lacze l272 = new Lacze(272, RC3.GetTopology.network.wezly[2], RC3.GetTopology.network.wezly[0]);
            Assert.AreEqual(l272.Wezel1.ip, RC3.GetTopology.network.krawedzie[3].Wezel1.ip);
            Assert.AreEqual(l272.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[3].Wezel1.idWezla);
            Assert.AreEqual(l272.Wezel2.ip, RC3.GetTopology.network.krawedzie[3].Wezel2.ip);
            Assert.AreEqual(l272.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[3].Wezel2.idWezla);

            Lacze l4331 = new Lacze(4331, RC3.GetTopology.network.wezly[3], RC3.GetTopology.network.wezly[1]);
            Assert.AreEqual(l4331.Wezel1.ip, RC3.GetTopology.network.krawedzie[4].Wezel1.ip);
            Assert.AreEqual(l4331.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[4].Wezel1.idWezla);
            Assert.AreEqual(l4331.Wezel2.ip, RC3.GetTopology.network.krawedzie[4].Wezel2.ip);
            Assert.AreEqual(l4331.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[4].Wezel2.idWezla);

            Lacze l4332 = new Lacze(4332, RC3.GetTopology.network.wezly[1], RC3.GetTopology.network.wezly[3]);
            Assert.AreEqual(l4332.Wezel1.ip, RC3.GetTopology.network.krawedzie[5].Wezel1.ip);
            Assert.AreEqual(l4332.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[5].Wezel1.idWezla);
            Assert.AreEqual(l4332.Wezel2.ip, RC3.GetTopology.network.krawedzie[5].Wezel2.ip);
            Assert.AreEqual(l4332.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[5].Wezel2.idWezla);

            Lacze l2331 = new Lacze(2331, RC3.GetTopology.network.wezly[3], RC3.GetTopology.network.wezly[0]);
            Assert.AreEqual(l2331.Wezel1.ip, RC3.GetTopology.network.krawedzie[6].Wezel1.ip);
            Assert.AreEqual(l2331.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[6].Wezel1.idWezla);
            Assert.AreEqual(l2331.Wezel2.ip, RC3.GetTopology.network.krawedzie[6].Wezel2.ip);
            Assert.AreEqual(l2331.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[6].Wezel2.idWezla);

            Lacze l2332 = new Lacze(2332, RC3.GetTopology.network.wezly[0], RC3.GetTopology.network.wezly[3]);
            Assert.AreEqual(l2332.Wezel1.ip, RC3.GetTopology.network.krawedzie[7].Wezel1.ip);
            Assert.AreEqual(l2332.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[7].Wezel1.idWezla);
            Assert.AreEqual(l2332.Wezel2.ip, RC3.GetTopology.network.krawedzie[7].Wezel2.ip);
            Assert.AreEqual(l2332.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[7].Wezel2.idWezla);

            Lacze l35331 = new Lacze(35331, RC3.GetTopology.network.wezly[3], RC3.GetTopology.network.wezly[4]);
            Assert.AreEqual(l35331.Wezel1.ip, RC3.GetTopology.network.krawedzie[8].Wezel1.ip);
            Assert.AreEqual(l35331.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[8].Wezel1.idWezla);
            Assert.AreEqual(l35331.Wezel2.ip, RC3.GetTopology.network.krawedzie[8].Wezel2.ip);
            Assert.AreEqual(l35331.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[8].Wezel2.idWezla);

            Lacze l35332 = new Lacze(35332, RC3.GetTopology.network.wezly[4], RC3.GetTopology.network.wezly[3]);
            Assert.AreEqual(l35332.Wezel1.ip, RC3.GetTopology.network.krawedzie[9].Wezel1.ip);
            Assert.AreEqual(l35332.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[9].Wezel1.idWezla);
            Assert.AreEqual(l35332.Wezel2.ip, RC3.GetTopology.network.krawedzie[9].Wezel2.ip);
            Assert.AreEqual(l35332.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[9].Wezel2.idWezla);

            Lacze l7351r3 = new Lacze(7351, RC3.GetTopology.network.wezly[2], RC3.GetTopology.network.wezly[4]);
            Assert.AreEqual(l7351r3.Wezel1.ip, RC3.GetTopology.network.krawedzie[10].Wezel1.ip);
            Assert.AreEqual(l7351r3.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[10].Wezel1.idWezla);
            Assert.AreEqual(l7351r3.Wezel2.ip, RC3.GetTopology.network.krawedzie[10].Wezel2.ip);
            Assert.AreEqual(l7351r3.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[10].Wezel2.idWezla);

            Lacze l7352r3 = new Lacze(7352, RC3.GetTopology.network.wezly[4], RC3.GetTopology.network.wezly[2]);
            Assert.AreEqual(l7352r3.Wezel1.ip, RC3.GetTopology.network.krawedzie[11].Wezel1.ip);
            Assert.AreEqual(l7352r3.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[11].Wezel1.idWezla);
            Assert.AreEqual(l7352r3.Wezel2.ip, RC3.GetTopology.network.krawedzie[11].Wezel2.ip);
            Assert.AreEqual(l7352r3.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[11].Wezel2.idWezla);

            Lacze l35371 = new Lacze(35371, RC3.GetTopology.network.wezly[4], RC3.GetTopology.network.wezly[5]);
            Assert.AreEqual(l35371.Wezel1.ip, RC3.GetTopology.network.krawedzie[12].Wezel1.ip);
            Assert.AreEqual(l35371.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[12].Wezel1.idWezla);
            Assert.AreEqual(l35371.Wezel2.ip, RC3.GetTopology.network.krawedzie[12].Wezel2.ip);
            Assert.AreEqual(l35371.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[12].Wezel2.idWezla);

            Lacze l35372 = new Lacze(35371, RC3.GetTopology.network.wezly[5], RC3.GetTopology.network.wezly[4]);
            Assert.AreEqual(l35372.Wezel1.ip, RC3.GetTopology.network.krawedzie[13].Wezel1.ip);
            Assert.AreEqual(l35372.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[13].Wezel1.idWezla);
            Assert.AreEqual(l35372.Wezel2.ip, RC3.GetTopology.network.krawedzie[13].Wezel2.ip);
            Assert.AreEqual(l35372.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[13].Wezel2.idWezla);

            Lacze l3761 = new Lacze(3761, RC3.GetTopology.network.wezly[5], RC3.GetTopology.network.wezly[6]);
            Assert.AreEqual(l3761.Wezel1.ip, RC3.GetTopology.network.krawedzie[14].Wezel1.ip);
            Assert.AreEqual(l3761.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[14].Wezel1.idWezla);
            Assert.AreEqual(l3761.Wezel2.ip, RC3.GetTopology.network.krawedzie[14].Wezel2.ip);
            Assert.AreEqual(l3761.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[14].Wezel2.idWezla);

            Lacze l3762 = new Lacze(3672, RC3.GetTopology.network.wezly[6], RC3.GetTopology.network.wezly[5]);
            Assert.AreEqual(l3762.Wezel1.ip, RC3.GetTopology.network.krawedzie[15].Wezel1.ip);
            Assert.AreEqual(l3762.Wezel1.idWezla, RC3.GetTopology.network.krawedzie[15].Wezel1.idWezla);
            Assert.AreEqual(l3762.Wezel2.ip, RC3.GetTopology.network.krawedzie[15].Wezel2.ip);
            Assert.AreEqual(l3762.Wezel2.idWezla, RC3.GetTopology.network.krawedzie[15].Wezel2.idWezla);

        }

        [TestMethod]
        public void testRouteTableQuery()
        {
            RoutingController RC1 = new RoutingController(1);
            IPEndPoint RC1Endpoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            Siec network = new Siec();
            Wezel W1 = new Wezel(1, "127.0.0.1");
            Wezel W2 = new Wezel(2, "127.0.0.2");
            Wezel W3 = new Wezel(3, "127.0.0.3");

            network.krawedzie.Add(new Lacze(12, W1, W2));
            network.krawedzie.Add(new Lacze(23, W2, W3));
            network.wezly.AddRange(new List<Wezel>() { W1, W2, W3 });

            RC1.GetTopology.network = network;

            RC1.Run();

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //BitRate = 5, 4 hops
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.3#5#4";

            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            client.Send(messageBytes, messageBytes.Length, RC1Endpoint);

            byte[] receivedBytes = new byte[100];

            receivedBytes = client.Receive(ref RC1Endpoint);

            string response = Encoding.ASCII.GetString(receivedBytes);

            client.Close();

            Assert.AreEqual(RC1.ip + "#" + MessageNames.ROUTE_TABLE_QUERY +
                "#0#" + band + "#127.0.0.1#12#127.0.0.2#23#127.0.0.3#", response);


        }

        [TestMethod]
        public void testRouteTableQueryWithFrequency()
        {
            RoutingController RC1 = new RoutingController(1);
            IPEndPoint RC1Endpoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            Siec network = new Siec();
            Wezel W1 = new Wezel(1, "127.0.0.1");
            Wezel W2 = new Wezel(2, "127.0.0.2");
            Wezel W3 = new Wezel(3, "127.0.0.3");

            network.krawedzie.Add(new Lacze(12, W1, W2));
            network.krawedzie.Add(new Lacze(23, W2, W3));
            network.wezly.AddRange(new List<Wezel>() { W1, W2, W3 });

            RC1.GetTopology.network = network;

            RC1.Run();

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //Na czestotliwosci 17
            short frequency = 17;
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.3#5#4#" + frequency + "#";

            var messageBytes = Encoding.ASCII.GetBytes(message);

            client.Send(messageBytes, messageBytes.Length, RC1Endpoint);

            var receivedBytes = client.Receive(ref RC1Endpoint);

            var response = Encoding.ASCII.GetString(receivedBytes);

            Assert.AreEqual(RC1.ip + "#" + MessageNames.ROUTE_TABLE_QUERY +
                            "#" + frequency + "#" + band + "#127.0.0.1#12#127.0.0.2#23#127.0.0.3#", response);

            client.Close();
        }

        [TestMethod]
        public void test_HandleRouteTableQuery_LocalTopology()
        {
            Wezel[] wezly = new Wezel[6];
            for (int i = 0; i < 6; i++)
                wezly[i] = new Wezel(i, "127.0.0." + i);

            List<Lacze> lacza = new List<Lacze>();

            //{1}-12-{2}-23-{34}
            //{2}-25-{5}-53-{3}

            RoutingController RC = new RoutingController(1);

            RC.GetTopology.network = new Siec();

            RC.GetTopology.network.wezly = new List<Wezel>();
            RC.GetTopology.network.wezly.AddRange(wezly);


            RC.GetTopology.network.krawedzie.Add(new Lacze(12, wezly[1], wezly[2], 1, 63));
            RC.GetTopology.network.krawedzie.Add(new Lacze(23, wezly[2], wezly[3], 1, 63));
            RC.GetTopology.network.krawedzie.Add(new Lacze(34, wezly[3], wezly[4], 1, 63));
            RC.GetTopology.network.krawedzie.Add(new Lacze(25, wezly[2], wezly[5], 1, 63));
            RC.GetTopology.network.krawedzie.Add(new Lacze(53, wezly[5], wezly[3], 1, 63));

            RC.Run();

            IPEndPoint RC1Endpoint = new IPEndPoint(RC.ip, RC.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //BitRate = 5, 4 hops

            //Wiadomość od CC
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.4#5#4";

            byte[] mesasgeBytes = Encoding.ASCII.GetBytes(message);

            client.Send(mesasgeBytes, mesasgeBytes.Length, RC1Endpoint);

            byte[] receivedBytes = new byte[100];

            receivedBytes = client.Receive(ref RC1Endpoint);

            string response = Encoding.ASCII.GetString(receivedBytes);

            Assert.AreEqual($"{RC.ip}#{MessageNames.ROUTE_TABLE_QUERY}#0#{band}" +
                            $"#127.0.0.1#12#127.0.0.2#23#127.0.0.3#34#127.0.0.4#", response);

            //Wiadomośc od LRM - alokacja portu 12 OUT w węźle 127.0.0.1
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.1#12#out#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            //Wiadomośc od LRM - alokacja portu 12 IN w węźle 127.0.0.2
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.2#12#in#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            //Wiadomośc od LRM - alokacja portu 23 OUT w węźle 127.0.0.2
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.2#23#out#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            Assert.AreEqual(0, RC.GetTopology.network.wezly[2].SNPP.snps[0].eonTable.OutFrequencies[0]);
            //krawędź 23 to krawęźdź [1]
            Assert.AreNotEqual(4, RC.GetTopology.network.krawedzie[1].Waga);

            //Wiadomośc od LRM - alokacja portu 23 IN w węźle 127.0.0.3
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.3#23#in#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            Assert.AreEqual(0, RC.GetTopology.network.wezly[2].SNPP.snps[0].eonTable.OutFrequencies[0]);
            //krawędź 23 to krawęźdź [1]
            Assert.AreEqual(4, RC.GetTopology.network.krawedzie[1].Waga);

            //Wiadomośc od LRM - alokacja portu 34 OUT w węźle 127.0.0.3
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.3#34#out#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            //Wiadomośc od LRM - alokacja portu 34 OUT w węźle 127.0.0.4
            message = $"127.0.0.111#{MessageNames.LOCAL_TOPOLOGY}#ALLOCATION#127.0.0.4#34#in#{band}#0";

            RC.handleLocalTopology(message.Split('#'));

            //Wiadomość od CC
            message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.4#5#4";

            //mesasgeBytes = Encoding.ASCII.GetBytes(message);

            //client.Send(mesasgeBytes, mesasgeBytes.Length, RC1Endpoint);

            RC.handleRouteTableQuery(message.Split('#'));

            receivedBytes = client.Receive(ref RC1Endpoint);

            response = Encoding.ASCII.GetString(receivedBytes);

            //Na czestotliwosci 1 powinien sie zrobic jeszcze jeden tunel
            Assert.AreEqual($"{RC.ip}#{MessageNames.ROUTE_TABLE_QUERY}#1#{band}" +
                            $"#127.0.0.1#12#127.0.0.2#25#127.0.0.5#53#127.0.0.3#34#127.0.0.4#", response);

            message = $"127.0.0.111#{MessageNames.KILL_LINK}#25";

            RC.handleKillLink(message.Split('#'));

            //[3] to łącze 25
            //Assert.AreEqual(64*64, RC.GetTopology.network.krawedzie[3].Waga);
            Assert.AreEqual(-1, RC.GetTopology.network.krawedzie.FindIndex(x => x.idKrawedzi == 25));

            message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.4#5#4";

            RC.handleRouteTableQuery(message.Split('#'));

            receivedBytes = client.Receive(ref RC1Endpoint);

            response = Encoding.ASCII.GetString(receivedBytes);

            Assert.AreEqual($"{RC.ip}#{MessageNames.ROUTE_TABLE_QUERY}#1#{band}" +
                            $"#127.0.0.1#12#127.0.0.2#23#127.0.0.3#34#127.0.0.4#", response);
        }

        [TestMethod]
        public void testRouteTableQueryWith2RC1()
        {
            RoutingController RC1 = new RoutingController(1);
            IPEndPoint RC1Endpoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            Siec network = new Siec();
            //Wezel W0 = new Wezel(0, "127.0.0.0");
            Wezel W1 = new Wezel(1, "127.0.0.1");
            Wezel W23 = new Wezel(23, "127.0.0.23");
            Wezel W4 = new Wezel(4, "127.0.0.4");
            //Wezel W5 = new Wezel(5, "127.0.0.5");

            //network.krawedzie.Add(new Lacze(101, W0, W1));
            network.krawedzie.Add(new Lacze(123, W1, W23));
            network.krawedzie.Add(new Lacze(234, W23, W4));
            //network.krawedzie.Add(new Lacze(45, W4, W5));
            network.wezly.AddRange(new List<Wezel>() { W1, W23, W4 });

            RC1.GetTopology.network = network;

            Siec network2 = new Siec();

            Wezel W2 = new Wezel(2, "127.0.0.2");
            Wezel W3 = new Wezel(3, "127.0.0.3");

            network2.wezly.AddRange(new List<Wezel>() { W1, W2, W3, W4 });
            network2.krawedzie.Add(new Lacze(123, W1, W2));
            network2.krawedzie.Add(new Lacze(23, W2, W3));
            network2.krawedzie.Add(new Lacze(234, W3, W4));

            RoutingController RC2 = new RoutingController(2);
            RC2.GetTopology.network = network2;

            SNandRC sNandRc = new SNandRC(RC2.ip.ToString(), "127.0.0.23");

            RC1.SN_RCs.Add(sNandRc);

            RC1.Run();
            RC2.Run();

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //BitRate = 5, 4 hops
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.1#127.0.0.4#5#4";

            byte[] mesasgeBytes = Encoding.ASCII.GetBytes(message);

            client.Send(mesasgeBytes, mesasgeBytes.Length, RC1Endpoint);

            byte[] receivedBytes = new byte[100];

            receivedBytes = client.Receive(ref RC1Endpoint);

            string response = Encoding.ASCII.GetString(receivedBytes);

            client.Close();

            Assert.AreEqual(RC1.ip + "#" + MessageNames.ROUTE_TABLE_QUERY +
                            "#127.0.0.1#123#127.0.0.2#23#127.0.0.3#234#127.0.0.4#0#" + band, response);

        }

        [TestMethod]
        public void testRouteTableQueryWith2RC()
        {
            RoutingController RC1 = new RoutingController(1);
            IPEndPoint RC1Endpoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            Siec network = new Siec();
            Wezel W0 = new Wezel(0, "127.0.0.0");
            Wezel W1 = new Wezel(1, "127.0.0.1");
            Wezel W23 = new Wezel(23, "127.0.0.23");
            Wezel W4 = new Wezel(4, "127.0.0.4");
            Wezel W5 = new Wezel(5, "127.0.0.5");

            network.krawedzie.Add(new Lacze(101, W0, W1));
            network.krawedzie.Add(new Lacze(123, W1, W23));
            network.krawedzie.Add(new Lacze(234, W23, W4));
            network.krawedzie.Add(new Lacze(45, W4, W5));
            network.wezly.AddRange(new List<Wezel>() { W0, W1, W23, W4, W5 });

            RC1.GetTopology.network = network;

            Siec network2 = new Siec();

            Wezel W2 = new Wezel(2, "127.0.0.2");
            Wezel W3 = new Wezel(3, "127.0.0.3");

            network2.wezly.AddRange(new List<Wezel>() { W1, W2, W3, W4 });
            network2.krawedzie.Add(new Lacze(123, W1, W2));
            network2.krawedzie.Add(new Lacze(23, W2, W3));
            network2.krawedzie.Add(new Lacze(234, W3, W4));

            RoutingController RC2 = new RoutingController(2);
            RC2.GetTopology.network = network2;

            SNandRC sNandRc = new SNandRC(RC2.ip.ToString(), "127.0.0.23");

            RC1.SN_RCs.Add(sNandRc);

            RC1.Run();
            RC2.Run();

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //BitRate = 5, 4 hops
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.0#127.0.0.5#5#4";

            byte[] mesasgeBytes = Encoding.ASCII.GetBytes(message);

            client.Send(mesasgeBytes, mesasgeBytes.Length, RC1Endpoint);

            byte[] receivedBytes = new byte[100];

            receivedBytes = client.Receive(ref RC1Endpoint);

            string response = Encoding.ASCII.GetString(receivedBytes);

            client.Close();

            Assert.AreEqual(RC1.ip + "#" + MessageNames.ROUTE_TABLE_QUERY +
                            "#127.0.0.0#101#127.0.0.1#123#127.0.0.2#23#127.0.0.3#234#127.0.0.4#45#127.0.0.5#0#" + band, response);

        }

        [TestMethod]
        public void testRouteTableQueryWith2RCWith3Nodes()
        {
            RoutingController RC1 = new RoutingController(1);
            IPEndPoint RC1Endpoint = new IPEndPoint(RC1.ip, RC1.portNumber);

            IPEndPoint IendPoint = new IPEndPoint(IPAddress.Parse("127.0.0.111"), 11000);
            UdpClient client = new UdpClient(IendPoint);

            Siec network = new Siec();
            Wezel W0 = new Wezel(0, "127.0.0.0");
            Wezel W1 = new Wezel(1, "127.0.0.1");
            Wezel W23 = new Wezel(23, "127.0.0.23");
            Wezel W4 = new Wezel(4, "127.0.0.4");
            Wezel W5 = new Wezel(5, "127.0.0.5");

            network.krawedzie.Add(new Lacze(101, W0, W1));
            network.krawedzie.Add(new Lacze(123, W1, W23));
            network.krawedzie.Add(new Lacze(234, W23, W4));
            network.krawedzie.Add(new Lacze(45, W4, W5));
            network.wezly.AddRange(new List<Wezel>() { W0, W1, W23, W4, W5 });

            RC1.GetTopology.network = network;

            Siec network2 = new Siec();

            Wezel W2 = new Wezel(2, "127.0.0.2");
            Wezel W3 = new Wezel(3, "127.0.0.3");
            Wezel W9 = new Wezel(9, "127.0.0.9");

            network2.wezly.AddRange(new List<Wezel>() { W1, W2, W3, W4, W9 });

            network2.krawedzie.Add(new Lacze(123, W1, W2));
            network2.krawedzie.Add(new Lacze(23, W2, W3));
            network2.krawedzie.Add(new Lacze(29, W2, W9));
            network2.krawedzie.Add(new Lacze(93, W9, W3));
            network2.krawedzie.Add(new Lacze(234, W3, W4));

            RoutingController RC2 = new RoutingController(2);
            RC2.GetTopology.network = network2;

            SNandRC sNandRc = new SNandRC(RC2.ip.ToString(), "127.0.0.23");

            RC1.SN_RCs.Add(sNandRc);

            RC1.Run();
            RC2.Run();

            //4 hopy
            short modulationPerformance = BorderNodeCommutationTable.determineModulationPerformance(4);

            //Predkosc 5
            short band = BorderNodeCommutationTable.determineBand(5, modulationPerformance);

            //BitRate = 5, 4 hops
            string message = "127.0.0.111#" + MessageNames.ROUTE_TABLE_QUERY + "#127.0.0.0#127.0.0.5#5#4";

            byte[] mesasgeBytes = Encoding.ASCII.GetBytes(message);

            client.Send(mesasgeBytes, mesasgeBytes.Length, RC1Endpoint);

            byte[] receivedBytes = new byte[100];

            receivedBytes = client.Receive(ref RC1Endpoint);

            string response = Encoding.ASCII.GetString(receivedBytes);

            client.Close();

            Assert.AreEqual(RC1.ip + "#" + MessageNames.ROUTE_TABLE_QUERY +
                            "#127.0.0.0#101#127.0.0.1#123#127.0.0.2#23#127.0.0.3#234#127.0.0.4#45#127.0.0.5#0#" + band, response);

        }

        [TestMethod]
        public void testHandleRoutePath()
        {
            RoutingController RC1 = new RoutingController(1);
            RoutingController RC2 = new RoutingController(2);

            Topology topology1 = new Topology();
            Topology topology2 = new Topology();

            Siec network1 = new Siec();
            Siec network2 = new Siec();

            //Podsiec 1
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w4 = new Wezel(4, "127.0.0.4");
            Wezel w23 = new Wezel(23, "127.0.0.23");

            //Podsiec 2
            Wezel w1_p2 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            Wezel w4_p2 = new Wezel(4, "127.0.0.4");

            //Topologia ogólna:
            //1-(2-3)-4, w nawiasach podsiec 2
            //RC1 widzi to tak: 1-23-4
            //RC2 widzi to tak: 1-2-3-4

            //Podsiec 1
            Lacze l123 = new Lacze(123, w1, w23);
            Lacze l234 = new Lacze(234, w23, w4);

            //Podsiec 2
            Lacze l12 = new Lacze(12, w1_p2, w2);
            Lacze l23 = new Lacze(23, w2, w3);
            Lacze l34 = new Lacze(34, w3, w4_p2);

            network1.krawedzie.AddRange(new List<Lacze>() { l123, l234 });
            network1.wezly.AddRange(new List<Wezel>() { w1, w23, w4 });
            topology1.network = network1;
            RC1.GetTopology = topology1;

            network2.krawedzie.AddRange(new List<Lacze>() { l12, l23, l34 });
            network2.wezly.AddRange(new List<Wezel>() { w1_p2, w2, w3, w4_p2 });
            topology2.network = network2;
            RC2.GetTopology = topology2;

            RC2.Run();
            //RC1.Run();

            //Generowanie sciezki
            var routedPath = RC1.getPathFromRC(RC2.ip.ToString(), new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w1.ip))),
                new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w4.ip))), 5, 0);

            Assert.AreEqual(w1.ip, routedPath.path.WezlySciezki[0].ip);
            Assert.AreEqual(w2.ip, routedPath.path.WezlySciezki[1].ip);
            Assert.AreEqual(w3.ip, routedPath.path.WezlySciezki[2].ip);
            Assert.AreEqual(w4.ip, routedPath.path.WezlySciezki[3].ip);

            //Assert.AreEqual(w1.idWezla, routedPath.path.WezlySciezki[0].idWezla);
            //Assert.AreEqual(w2.idWezla, routedPath.path.WezlySciezki[1].idWezla);
            //Assert.AreEqual(w3.idWezla, routedPath.path.WezlySciezki[2].idWezla);
            //Assert.AreEqual(w4.idWezla, routedPath.path.WezlySciezki[3].idWezla);
        }

        [TestMethod]
        public void testGenerateGetPathFromRCMessage()
        {
            RoutingController RC = new RoutingController();
            SubNetworkPointPool Source = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse("127.0.0.1")));
            SubNetworkPointPool Destination = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse("127.0.0.9")));

            string message = RC.generateGetPathFromRCMessage(Source, Destination, 8, 5);

            Assert.AreEqual(ConfigurationManager.AppSettings["RC" + RC.numberRC] + "#" + MessageNames.ROUTE_PATH
                + "#127.0.0.1#127.0.0.9#8#5", message);
        }

        [TestMethod]
        public void testNetworkPathConstructor()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            Lacze l12 = new Lacze(1, w1, w2, 1);
            Lacze l23 = new Lacze(2, w2, w3, 1);

            Siec network = new Siec();

            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(l12);
            network.krawedzie.Add(l23);

            network.algorytmFloyda();

            Sciezka path = new Sciezka(w1, w3);

            network.sciezki.Add(path);

            path.wyznaczSciezke(w1, w3, network.zwrocTabliceKierowaniaLaczami, network.zwrocTabliceKierowaniaWezlami,
                ref network.wezly, 1, network.Koszty, 5);

            NetworkPath networkPath = new NetworkPath(path);

            Assert.AreEqual(l12, path.zwroc_ListaKrawedziSciezki[0]);
            Assert.AreEqual(l23, path.zwroc_ListaKrawedziSciezki[1]);

            Assert.AreEqual(w1, path.WezlySciezki[0]);
            Assert.AreEqual(w2, path.WezlySciezki[1]);
            Assert.AreEqual(w3, path.WezlySciezki[2]);

            SubNetworkPointPool snpp1 = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w1.ip)));
            SubNetworkPointPool snpp2 = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w2.ip)));
            SubNetworkPointPool snpp3 = new SubNetworkPointPool(new SubNetworkPoint(IPAddress.Parse(w3.ip)));

            Assert.AreEqual(snpp1.snps[0].ipaddress, networkPath.snpps[0].snps[0].ipaddress);
            Assert.AreEqual(snpp2.snps[0].ipaddress, networkPath.snpps[1].snps[0].ipaddress);
            Assert.AreEqual(snpp3.snps[0].ipaddress, networkPath.snpps[2].snps[0].ipaddress);

            Assert.AreEqual(l12, networkPath.path.KrawedzieSciezki[0]);
            Assert.AreEqual(l23, networkPath.path.KrawedzieSciezki[1]);
        }
    }
}
