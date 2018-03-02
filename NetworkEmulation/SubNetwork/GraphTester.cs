using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AISDE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkNode;

namespace SubNetwork
{
    [TestClass]
    public class GraphTester
    {
        public Siec network;
        public List<Wezel> nodes;
        public List<IPAddress> ips;
        public List<Lacze> links;
        public Topology topology;

        //ustawia wartosci poczatkowe do testow
        public void setUpValues1()
        {
            network = new Siec();

            nodes = new List<Wezel>();
            ips = new List<IPAddress>();
            links = new List<Lacze>();
            topology = new Topology();

            ips.Add(IPAddress.Parse("127.0.0.2"));
            ips.Add(IPAddress.Parse("127.0.0.31"));
            ips.Add(IPAddress.Parse("127.0.0.17"));
            ips.Add(IPAddress.Parse("127.0.0.4"));
            ips.Add(IPAddress.Parse("127.0.0.33"));
            ips.Add(IPAddress.Parse("127.0.0.15"));
            ips.Add(IPAddress.Parse("127.0.0.6"));

            nodes.Add(new Wezel(2, "127.0.0.2"));
            nodes.Add(new Wezel(31, "127.0.0.31"));
            nodes.Add(new Wezel(17, "127.0.0.17"));
            nodes.Add(new Wezel(4, "127.0.0.4"));
            nodes.Add(new Wezel(33, "127.0.0.33"));
            nodes.Add(new Wezel(15, "127.0.0.15"));
            nodes.Add(new Wezel(6, "127.0.0.6"));

            //Dodawanie krawedzi
            links.Add(new Lacze(0, nodes[0], nodes[1], 0)); //2-31
            links.Add(new Lacze(1, nodes[1], nodes[2], 0)); //31-17
            links.Add(new Lacze(2, nodes[2], nodes[3], 0)); //17-4 
            links.Add(new Lacze(3, nodes[2], nodes[4], 0)); //17-33
            links.Add(new Lacze(4, nodes[2], nodes[5], 0)); //17-15
            links.Add(new Lacze(5, nodes[4], nodes[5], 0)); //33-15 
            links.Add(new Lacze(6, nodes[5], nodes[6], 0)); //15-6

            links.Add(new Lacze(7, nodes[1], nodes[0], 0)); //31-2
            links.Add(new Lacze(8, nodes[2], nodes[1], 0)); //17-31
            links.Add(new Lacze(9, nodes[3], nodes[2], 0)); //4-17
            links.Add(new Lacze(10, nodes[4], nodes[2], 0)); //33-17
            links.Add(new Lacze(11, nodes[5], nodes[2], 0)); //15-17
            links.Add(new Lacze(12, nodes[5], nodes[4], 0)); //15-33 
            links.Add(new Lacze(13, nodes[6], nodes[5], 0)); //6-15
        }

        [TestMethod]
        public void setUpValues2(short band)
        {
            network = new Siec();

            nodes = new List<Wezel>();
            ips = new List<IPAddress>();
            links = new List<Lacze>();
            topology = new Topology();

            ips.Add(IPAddress.Parse("127.0.0.2"));
            ips.Add(IPAddress.Parse("127.0.0.31"));
            ips.Add(IPAddress.Parse("127.0.0.17"));
            ips.Add(IPAddress.Parse("127.0.0.4"));
            ips.Add(IPAddress.Parse("127.0.0.33"));
            ips.Add(IPAddress.Parse("127.0.0.15"));
            ips.Add(IPAddress.Parse("127.0.0.6"));

            nodes.Add(new Wezel(2, "127.0.0.2"));
            nodes.Add(new Wezel(31, "127.0.0.31"));
            nodes.Add(new Wezel(17, "127.0.0.17"));
            nodes.Add(new Wezel(4, "127.0.0.4"));
            nodes.Add(new Wezel(33, "127.0.0.33"));
            nodes.Add(new Wezel(15, "127.0.0.15"));
            nodes.Add(new Wezel(6, "127.0.0.6"));

            //Dodawanie krawedzi
            links.Add(new Lacze(0, nodes[0], nodes[1], band)); //2-31
            links.Add(new Lacze(1, nodes[1], nodes[2], band)); //31-17
            links.Add(new Lacze(2, nodes[2], nodes[3], band)); //17-4 
            links.Add(new Lacze(3, nodes[2], nodes[4], band)); //17-33
            links.Add(new Lacze(4, nodes[2], nodes[5], band)); //17-15
            links.Add(new Lacze(5, nodes[4], nodes[5], band)); //33-15 
            links.Add(new Lacze(6, nodes[5], nodes[6], band)); //15-6

            links.Add(new Lacze(7, nodes[1], nodes[0], band)); //31-2
            links.Add(new Lacze(8, nodes[2], nodes[1], band)); //17-31
            links.Add(new Lacze(9, nodes[3], nodes[2], band)); //4-17
            links.Add(new Lacze(10, nodes[4], nodes[2], band)); //33-17
            links.Add(new Lacze(11, nodes[5], nodes[2], band)); //15-17
            links.Add(new Lacze(12, nodes[5], nodes[4], band)); //15-33 
            links.Add(new Lacze(13, nodes[6], nodes[5], band)); //6-15

            network.krawedzie = new List<Lacze>(links);
            network.wezly = new List<Wezel>(nodes);
            topology.network = new Siec(network);
        }

        [TestMethod]
        public void testNodeConstructor()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");

            Assert.AreEqual("127.0.0.1", w1.SNPP.snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.2", w2.SNPP.snps[0].ipaddress.ToString());
        }

        [TestMethod]
        public void testLinkConstructor1()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");

            Lacze l = new Lacze(12, w1, w2);

            SubNetworkPoint SNP1 = l.Wezel1.SNPP.snps.Find(x => x.ipaddress.ToString() == "127.0.0.1" && x.portOUT == 12);
            SubNetworkPoint SNP2 = l.Wezel2.SNPP.snps.Find(x => x.ipaddress.ToString() == "127.0.0.2" && x.portIN == 12);

            Assert.IsNotNull(SNP1);
            Assert.IsNotNull(SNP2);

            Assert.AreEqual(12, SNP1.portOUT);
            Assert.AreEqual(12, SNP2.portIN);

            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), SNP1.ipaddress);
            Assert.AreEqual(IPAddress.Parse("127.0.0.2"), SNP2.ipaddress);
        }

        [TestMethod]
        public void testTables1()
        {
            Siec network = new Siec();

            List<Wezel> nodes = new List<Wezel>();
            List<IPAddress> ips = new List<IPAddress>();
            List<Lacze> links = new List<Lacze>();

            ips.Add(IPAddress.Parse("127.0.0.1"));
            ips.Add(IPAddress.Parse("127.0.0.2"));
            ips.Add(IPAddress.Parse("127.0.0.3"));
            ips.Add(IPAddress.Parse("127.0.0.4"));
            ips.Add(IPAddress.Parse("127.0.0.5"));
            ips.Add(IPAddress.Parse("127.0.0.6"));
            ips.Add(IPAddress.Parse("127.0.0.7"));

            //Tworzenie wezlow o id odpowiadajacych adresom IP z listy
            for (int i = 0; i < ips.Count; i++)
            {
                nodes.Add(new Wezel(i + 1, ips[i].ToString()));
            }

            //Dodawanie krawedzi
            links.Add(new Lacze(0, nodes[0], nodes[3], 1));
            links.Add(new Lacze(1, nodes[3], nodes[6], 1));
            links.Add(new Lacze(2, nodes[6], nodes[2], 1));
            links.Add(new Lacze(3, nodes[2], nodes[1], 1));
            links.Add(new Lacze(4, nodes[3], nodes[4], 1));
            links.Add(new Lacze(5, nodes[4], nodes[5], 1));
            links.Add(new Lacze(6, nodes[5], nodes[3], 1));
            links.Add(new Lacze(7, nodes[3], nodes[0], 1));
            links.Add(new Lacze(8, nodes[1], nodes[3], 1));

            //DOdanie wszystkich krawedzi do sieci
            network.krawedzie.AddRange(links);

            //Dodanie wszystkich wezlow do sieci
            network.wezly.AddRange(nodes);

            //zapuszczenie algorytmu floyda
            network.algorytmFloyda();

            //elementy sieci nie powinny byc zerowe
            Assert.IsFalse(network.Koszty.Length == 0);
            Assert.IsFalse(network.wezly.Count == 0);
            Assert.AreEqual(network.wezly.Count * network.wezly.Count, network.Koszty.Length);
            Assert.IsFalse(network.krawedzie.Count == 0);
            Assert.IsTrue(network.sprawdzSpojnosc());

            /*
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 3].idWezla);
            Assert.AreEqual(7, network.zwrocTabliceKierowaniaWezlami[3, 2].idWezla);
            Assert.AreEqual(7, network.zwrocTabliceKierowaniaWezlami[3, 1].idWezla);
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 6].idWezla);
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 2].idWezla);
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 1].idWezla);
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 4].idWezla);
            Assert.AreEqual(4, network.zwrocTabliceKierowaniaWezlami[0, 5].idWezla);
            Assert.AreEqual(5, network.zwrocTabliceKierowaniaWezlami[3, 5].idWezla);
            Assert.AreEqual(6, network.zwrocTabliceKierowaniaWezlami[4, 5].idWezla); */

            Sciezka path1_2 = new Sciezka(nodes[0], nodes[1]);
            path1_2.KrawedzieSciezki = path1_2.wyznaczSciezke(path1_2.Wezel1, path1_2.Wezel2, network.zwrocTabliceKierowaniaLaczami,
                network.zwrocTabliceKierowaniaWezlami, ref network.wezly, 1, network.Koszty, 10);

            path1_2.wyznaczWezly(path1_2.Wezel1);

            path1_2.pokazSciezke();

            Assert.IsTrue(path1_2.WezlySciezki.Contains(nodes[0]));
            Assert.IsTrue(path1_2.WezlySciezki.Contains(nodes[3]));
            Assert.IsTrue(path1_2.WezlySciezki.Contains(nodes[6]));
            Assert.IsTrue(path1_2.WezlySciezki.Contains(nodes[2]));
            Assert.IsTrue(path1_2.WezlySciezki.Contains(nodes[1]));

            //Wyswietlenie tablic Floydowych
            //network.tabliceFloyd(network.zwrocTabliceKierowaniaLaczami, network.zwrocTabliceKierowaniaWezlami, network.Koszty);
        }

        [TestMethod]
        public void testRoutePathsInNetwork()
        {
            Siec network = new Siec();

            List<Wezel> nodes = new List<Wezel>();
            List<IPAddress> ips = new List<IPAddress>();
            List<Lacze> links = new List<Lacze>();

            ips.Add(IPAddress.Parse("127.0.0.2"));
            ips.Add(IPAddress.Parse("127.0.0.31"));
            ips.Add(IPAddress.Parse("127.0.0.17"));
            ips.Add(IPAddress.Parse("127.0.0.4"));
            ips.Add(IPAddress.Parse("127.0.0.33"));
            ips.Add(IPAddress.Parse("127.0.0.15"));
            ips.Add(IPAddress.Parse("127.0.0.6"));

            nodes.Add(new Wezel(2, "127.0.0.2"));
            nodes.Add(new Wezel(31, "127.0.0.31"));
            nodes.Add(new Wezel(17, "127.0.0.17"));
            nodes.Add(new Wezel(4, "127.0.0.4"));
            nodes.Add(new Wezel(33, "127.0.0.33"));
            nodes.Add(new Wezel(15, "127.0.0.15"));
            nodes.Add(new Wezel(6, "127.0.0.6"));

            //Dodawanie krawedzi
            links.Add(new Lacze(0, nodes[0], nodes[1], 1, 0)); //2-31
            links.Add(new Lacze(1, nodes[1], nodes[2], 1, 0)); //31-17
            links.Add(new Lacze(2, nodes[2], nodes[3], 1, 0)); //17-4 
            links.Add(new Lacze(3, nodes[2], nodes[4], 1, 0)); //17-33
            links.Add(new Lacze(4, nodes[2], nodes[5], 1, 0)); //17-15
            links.Add(new Lacze(5, nodes[4], nodes[5], 1, 0)); //33-15 
            links.Add(new Lacze(6, nodes[5], nodes[6], 1, 0)); //15-6

            links.Add(new Lacze(7, nodes[1], nodes[0], 1, 0)); //31-2
            links.Add(new Lacze(8, nodes[2], nodes[1], 1, 0)); //17-31
            links.Add(new Lacze(9, nodes[3], nodes[2], 1, 0)); //4-17
            links.Add(new Lacze(10, nodes[4], nodes[2], 1, 0)); //33-17
            links.Add(new Lacze(11, nodes[5], nodes[2], 1, 0)); //15-17
            links.Add(new Lacze(12, nodes[5], nodes[4], 1, 0)); //15-33 
            links.Add(new Lacze(13, nodes[6], nodes[5], 1, 0)); //6-15

            //DOdanie wszystkich krawedzi do sieci
            network.krawedzie.AddRange(links);

            //Dodanie wszystkich wezlow do sieci
            network.wezly.AddRange(nodes);

            //zapuszczenie algorytmu floyda
            network.algorytmFloyda();

            //elementy sieci nie powinny byc zerowe
            Assert.IsFalse(network.Koszty.Length == 0);
            Assert.IsFalse(network.wezly.Count == 0);
            Assert.AreEqual(network.wezly.Count * network.wezly.Count, network.Koszty.Length);
            Assert.IsFalse(network.krawedzie.Count == 0);
            Assert.IsTrue(network.sprawdzSpojnosc());

            Sciezka path2_6 = new Sciezka(nodes[0], nodes[6]);
            path2_6.wyznaczSciezke(path2_6.Wezel1, path2_6.Wezel2, network.zwrocTabliceKierowaniaLaczami,
                network.zwrocTabliceKierowaniaWezlami, ref network.wezly, 1, network.Koszty, 1);

            path2_6.wyznaczWezly(path2_6.Wezel1);

            path2_6.pokazSciezke();

            Assert.IsTrue(path2_6.WezlySciezki.Contains(nodes[0]));
            Assert.IsTrue(path2_6.WezlySciezki.Contains(nodes[1]));
            Assert.IsTrue(path2_6.WezlySciezki.Contains(nodes[2]));
            Assert.IsTrue(path2_6.WezlySciezki.Contains(nodes[5]));
            Assert.IsTrue(path2_6.WezlySciezki.Contains(nodes[6]));

            //Wyswietlenie tablic Floydowych
            //network.tabliceFloyd(network.zwrocTabliceKierowaniaLaczami, network.zwrocTabliceKierowaniaWezlami, network.Koszty);
        }

        /*
        [TestMethod]
        public void testRoutePathInTopology()
        {
            setUpValues1();

            //DOdanie wszystkich krawedzi do sieci
            network.krawedzie.AddRange(links);

            //Dodanie wszystkich wezlow do sieci
            network.wezly.AddRange(nodes);

            //Stworzona siec jest siecia w topologii
            topology.network = network;

            //Jakies koszty laczy
            topology.changeCost(5, 0);
            topology.changeCost(6, 1);
            topology.changeCost(2, 3);
            topology.changeCost(2, 5);
            topology.changeCost(5, 4);
            topology.changeCost(10, 6);

            //pozostala na razie ma miec koszt najwiekszy mozliwy
            for (int i = 7; i <= 13; i++)
            {
                topology.changeCost(64, i);
            }

            topology.changeCost(64, 2);

            //zapuszczenie algorytmu floyda
            topology.network.algorytmFloyda();

            //elementy sieci nie powinny byc zerowe
            Assert.IsFalse(topology.network.Koszty.Length == 0);
            Assert.IsFalse(topology.network.wezly.Count == 0);
            Assert.AreEqual(topology.network.wezly.Count * topology.network.wezly.Count, topology.network.Koszty.Length);
            Assert.IsFalse(topology.network.krawedzie.Count == 0);
            Assert.IsTrue(topology.network.sprawdzSpojnosc());

            Sciezka path2_6 = new Sciezka(nodes[0], nodes[6]);

            //Sciezka niemozliwa do zrealizowania (pasmo 64, a najbardziej zajety element to ma pasmo juz zajete 10)
            path2_6.wyznaczSciezke(path2_6.Wezel1, path2_6.Wezel2, topology.network.zwrocTabliceKierowaniaLaczami,
                topology.network.zwrocTabliceKierowaniaWezlami, ref topology.network.wezly, 64, topology.network.Koszty);

            Assert.AreEqual(0, path2_6.KrawedzieSciezki.Count);

            //Ta juz mozliwa
            path2_6.wyznaczSciezke(path2_6.Wezel1, path2_6.Wezel2, topology.network.zwrocTabliceKierowaniaLaczami,
                topology.network.zwrocTabliceKierowaniaWezlami, ref topology.network.wezly, 54, topology.network.Koszty);

            path2_6.wyznaczWezly(path2_6.Wezel1);

            path2_6.pokazSciezke();

            Assert.AreEqual(nodes[0], path2_6.WezlySciezki[0]);
            Assert.AreEqual(nodes[1], path2_6.WezlySciezki[1]);
            Assert.AreEqual(nodes[2], path2_6.WezlySciezki[2]);
            Assert.AreEqual(nodes[4], path2_6.WezlySciezki[3]);
            Assert.AreEqual(nodes[5], path2_6.WezlySciezki[4]);
            Assert.AreEqual(nodes[6], path2_6.WezlySciezki[5]);

            //Stara sciezka
            Sciezka temp = path2_6;

            //Sciezka niemozliwa do zrealizowania (pasmo 60, a najbardziej zajety element to ma pasmo juz zajete 10)
            path2_6.wyznaczSciezke(path2_6.Wezel1, path2_6.Wezel2, topology.network.zwrocTabliceKierowaniaLaczami,
                topology.network.zwrocTabliceKierowaniaWezlami, ref topology.network.wezly, 60, topology.network.Koszty);

            //Sciezka sie nie powinna zmienic
            Assert.AreEqual(temp, path2_6);

            SubNetworkPoint SNP1 = new SubNetworkPoint(IPAddress.Parse(nodes[0].ip));
            SubNetworkPointPool SNPP1 = new SubNetworkPointPool(SNP1);
            SubNetworkPoint SNP2 = new SubNetworkPoint(IPAddress.Parse(nodes[6].ip));
            SubNetworkPointPool SNPP2 = new SubNetworkPointPool(SNP2);

            var snpps = topology.getPathOfSNPPs(SNPP1, SNPP2, 5);

            //Kazdy SNPP powinien miec po jednym snp
            foreach (var snpp in snpps)
            {
                Assert.AreEqual(1, snpp.snps.Count);
            }

            Assert.AreEqual("127.0.0.2", snpps[0].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.31", snpps[1].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.17", snpps[2].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.33", snpps[3].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.15", snpps[4].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.6", snpps[5].snps[0].ipaddress.ToString());

            var networkPath = topology.getNetworkPath(SNPP1, SNPP2, 5, topology.network, 5);

            Assert.AreEqual("127.0.0.2", networkPath.snpps[0].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.31", networkPath.snpps[1].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.17", networkPath.snpps[2].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.33", networkPath.snpps[3].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.15", networkPath.snpps[4].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.6", networkPath.snpps[5].snps[0].ipaddress.ToString());

            //Wyswietlenie tablic Floydowych
            //topology.network.tabliceFloyd(topology.network.zwrocTabliceKierowaniaLaczami, topology.network.zwrocTabliceKierowaniaWezlami, topology.network.Koszty);
        } */

        /*[TestMethod] [Obsolete]
        public void testUsePotentialLink()
        {
            setUpValues2(1);

            //zapchanie linka o id 1
            topology.changeCost(63, 1);

            Assert.AreEqual(Math.Pow(64, 2), topology.network.krawedzie[1].Waga);

            SubNetworkPoint SNP1 = new SubNetworkPoint(IPAddress.Parse(nodes[0].ip));
            SubNetworkPointPool SNPP1 = new SubNetworkPointPool(SNP1);
            SubNetworkPoint SNP2 = new SubNetworkPoint(IPAddress.Parse(nodes[6].ip));
            SubNetworkPointPool SNPP2 = new SubNetworkPointPool(SNP2);

            var path = topology.getPathOfSNPPs(SNPP1, SNPP2, 1);

            //Teraz nie powinno znalezc sciezki
            Assert.AreEqual(0, path.Count);

            path = topology.routePath(SNPP1, SNPP2, 1);

            //Teraz tez nie powinno znalezc sciezki
            Assert.AreEqual(0, path.Count);

            //Wezly linka o id
            Wezel w1 = topology.network.krawedzie[1].Wezel1;
            Wezel w2 = topology.network.krawedzie[1].Wezel2;

            //Stworzenie lacza o id 0 z waga 0
            Lacze link = new Lacze(0, w1, w2, 0);

            //Dodanie linka do listy potencjalnych laczy
            topology.potentialLinks.Add(link);

            path = topology.routePath(SNPP1, SNPP2, 1);

            //Teraz powinno znalezc sciezke
            Assert.AreNotEqual(0, path.Count);

            //Sprawdzenie kolejnych wezlow
            Assert.AreEqual("127.0.0.2", path[0].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.31", path[1].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.17", path[2].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.33", path[3].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.15", path[4].snps[0].ipaddress.ToString());
            Assert.AreEqual("127.0.0.6", path[5].snps[0].ipaddress.ToString());
        } */

        [TestMethod]
        public void testSNPPsConstructors()
        {
            setUpValues1();

            SubNetworkPoint SNP1 = new SubNetworkPoint(IPAddress.Parse(nodes[0].ip));
            SubNetworkPointPool SNPP1 = new SubNetworkPointPool(SNP1);
            SubNetworkPoint SNP2 = new SubNetworkPoint(IPAddress.Parse(nodes[6].ip));
            SubNetworkPointPool SNPP2 = new SubNetworkPointPool(SNP2);

            Assert.AreNotEqual("", SNP1.ipaddress);
            Assert.AreNotEqual("", SNP2.ipaddress);

            Assert.AreEqual(SNP1, SNPP1.snps[0]);
            Assert.AreEqual(SNP2, SNPP2.snps[0]);
        }

        [TestMethod]
        public void testKonstruktoraSciezki()
        {
            Sciezka path = new Sciezka(new Wezel(), new Wezel());

            Assert.IsNotNull(path.KrawedzieSciezki);
            Assert.IsNotNull(path.WezlySciezki);
            Assert.IsNotNull(path.Wezel2);
            Assert.IsNotNull(path.Wezel1);
        }

        [TestMethod]
        public void testChangeCost()
        {
            setUpValues1();

            //DOdanie wszystkich krawedzi do sieci
            network.krawedzie.AddRange(links);

            //Dodanie wszystkich wezlow do sieci
            network.wezly.AddRange(nodes);

            //Stworzona siec jest siecia w topologii
            topology.network = network;

            //Jakies koszty laczy
            topology.changeCost(5, 0);
            topology.changeCost(6, 1);
            topology.changeCost(2, 3);
            topology.changeCost(2, 5);
            topology.changeCost(5, 4);
            topology.changeCost(10, 6);

            //pozostala na razie ma miec koszt najwiekszy mozliwy
            for (int i = 7; i <= 13; i++)
            {
                topology.changeCost(64, i);
                Assert.AreEqual(Math.Pow(64, 2), topology.network.krawedzie[i].Waga);
            }

            topology.changeCost(64, 2);
            Assert.AreEqual(Math.Pow(64, 2), topology.network.krawedzie[2].Waga);

            Assert.AreEqual(Math.Pow(5, 2), topology.network.krawedzie[0].Waga);
            Assert.AreEqual(Math.Pow(6, 2), topology.network.krawedzie[1].Waga);
            Assert.AreEqual(Math.Pow(2, 2), topology.network.krawedzie[3].Waga);
            Assert.AreEqual(Math.Pow(2, 2), topology.network.krawedzie[5].Waga);
            Assert.AreEqual(Math.Pow(5, 2), topology.network.krawedzie[4].Waga);
            Assert.AreEqual(Math.Pow(10, 2), topology.network.krawedzie[6].Waga);

            topology.changeCost(5, 0);

            Assert.AreEqual(100, topology.network.krawedzie[0].Waga);

            topology.changeCost(64, 1);

            Assert.AreEqual(36, topology.network.krawedzie[1].Waga);
        }

        [TestMethod]
        public void testWyznaczWezly()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");

            Lacze l12 = new Lacze(12, w1, w2, 1);
            Lacze l23 = new Lacze(23, w2, w3, 1);

            Sciezka s = new Sciezka();

            s.KrawedzieSciezki.Add(l12);
            s.KrawedzieSciezki.Add(l23);

            s.wyznaczWezly(w1);

            Assert.AreEqual(w1, s.WezlySciezki[0]);
            Assert.AreEqual(w2, s.WezlySciezki[1]);
            Assert.AreEqual(w3, s.WezlySciezki[2]);
        }

        [TestMethod]
        public void testWyznaczSciezke()
        {

            Siec network = new Siec();
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");
            Wezel w3 = new Wezel(3, "127.0.0.3");
            network.wezly.Add(w1);
            network.wezly.Add(w2);
            network.wezly.Add(w3);
            network.krawedzie.Add(new Lacze(12, w1, w2));
            network.krawedzie.Add(new Lacze(23, w2, w3));
            short band = 5;
            short frequency = 6;
            Sciezka path = new Sciezka(w1, w3);
            network.algorytmFloyda();
            path.wyznaczSciezke(w1, w2, band, frequency, network);

            Assert.AreNotEqual(0, path.WezlySciezki.Count);
            Assert.AreNotEqual(0, path.KrawedzieSciezki.Count);
            Assert.AreEqual(w1.ip, path.WezlySciezki[0].ip);
            Assert.AreEqual(w2.ip, path.WezlySciezki[1].ip);
            Assert.AreEqual(w3.ip, path.WezlySciezki[2].ip);
        }

        [TestMethod]
        public void testUpdateCost()
        {
            Wezel w1 = new Wezel(1, "127.0.0.1");
            Wezel w2 = new Wezel(2, "127.0.0.2");

            Lacze link = new Lacze(12, w1, w2);

            Assert.AreEqual(0, link.Waga);

            //Dodanie wpisow do tablic eonowych wezla 1 na wyjsciu (f = 0, B = 17)
            link.Wezel1.SNPP.snps[0].eonTable.addRow(new EONTableRowOut(0, 17));
            //I na wejsciu wezla 2
            link.Wezel2.SNPP.snps[0].eonTable.addRow(new EONTableRowIN(0, 17));

            //Aktualizacja wagi krawędzi
            link.updateCost(0, 17);

            Assert.AreEqual(17*17, link.Waga);

            //f = 19, B = 5, ale tylko na jednym węźle
            link.Wezel1.SNPP.snps[0].eonTable.addRow(new EONTableRowOut(19, 5));

            link.updateCost(19, 5);

            Assert.AreEqual(17*17, link.Waga);

            link.Wezel2.SNPP.snps[0].eonTable.addRow(new EONTableRowIN(19, 5));

            link.updateCost(19, 5);

            Assert.AreEqual(22*22, link.Waga);
        }
    }
}
