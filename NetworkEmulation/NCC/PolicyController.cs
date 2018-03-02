using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingTools;

namespace NCC
{
    
    /// <summary>
    /// klasa która odpowiadaza autentykację, uwierzytelnienie i sprawdzenie czy dany klient jest uprawniony do uzywania tej usługi
    /// logi Policy będą wypisywane na żółto z czarnym tłem
    /// </summary>
    public class PolicyController
    {
        // lista zawierająca uwierzytelnionych klientów
        private List<string> ListofClients = new List<string>();

        public bool Policy(string path, string OriginID)
        {
            //zmienna uzależniająca dostęp użytkownika do usługi
            bool access = false;

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkCyan;


            if (ListofClients.Contains(OriginID))
            {
                Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Policy: Client with ID: {0} is registered and can use services", OriginID);
                access = true;
            }
            else
            {
                Console.WriteLine("[" + Timestamp.generateTimestamp()  + "]" + "Policy: Client with ID: {0} is unregistered and cannot use services", OriginID);
                access = false;
            }
            Console.ForegroundColor = ConsoleColor.Black;
            return access;
        }

        public void ReadingFromPolicyFile(string path)
        {
            string line;
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        ListofClients.Add(line);
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
