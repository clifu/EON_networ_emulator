using System;
using System.Globalization;
using System.Linq;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa odpowiedzialna za stworzenie znacznika czasowego, używanego przy wyświetlaniu logów
    /// </summary>
    public class Timestamp
    {
        private static object obj = new object();
        private static object obj2 = new object();
        private static object obj3 = new object();
        private static object obj4 = new object();
        private static object obj5 = new object();
        private static object obj6 = new object();


        /// <summary>
        /// Funkcja służąca do stworzenia znacznika czasowego w momencie wywołania
        /// </summary>
        /// <returns>Zwraca wartość znacznika czasowego</returns>
        public static string generateTimestamp()
        {
            lock (obj)
            {
                string time = null;
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return time;
            }
        }

        public static string generateTimestampCC()
        {
            lock (obj2)
            {
                string time = null;
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return time;
            }
        }

        public static string generateTimestampRC()
        {
            lock (obj3)
            {
                string time = null;
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return time;
            }
        }

        public static string generateTimestampLRM()
        {
            lock (obj4)
            {
                string time = null;
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return time;
            }
        }

        public static string generateTimestampRCC()
        {
            lock (obj5)
            {
                string time = null;
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return time;
            }
        }
    }
}
