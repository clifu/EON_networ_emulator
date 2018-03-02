using System;
using System.Linq;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa odpowiedzialna za tworzenie losowej wiadomości, która będzie wysyłana do innego użytkownika
    /// </summary>
    public class RandomMessageGenerator
    {
        /// <summary>
        /// Funkcja służąca do stworzenia wiadomości, o losowej długości i zawartości
        /// </summary>
        /// <param name="maxLengthOfMessage">Maksymalna długość wysyłanej wiadomości</param>
        /// <returns>Losowa wiadomość</returns>
        public static string generateRandomMessage(int maxLengthOfMessage)
        {
            string message = null;
            //Lista znaków z których może zostać utworzona wiadomość
            string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
            //Zmienna potrzebna do losowania znaków zez zbioru
            int num;
            //Obiekt klasy Random, służy wyborowi długości wiadomości
            Random randomstrlength = new Random();
            //Obiekt klasy Random, służy wyborowi indeksu w celu pobrania odpowiedniego znaku
            Random rand = new Random();
            //Wybranie długości wiadomości

            /*
             *
              Random r = new Random();
               int rInt = r.Next(0, 100); //for ints
              int range = 100;
             */
            int strlength = randomstrlength.Next(1, 20);

            for (int i = 0; i < strlength; i++)
            {
                //Wylosowanie indeksu
                num = rand.Next(0, chars.Length - 1);
                //Tworzenei wiadomości
                message = message + chars[num];
            }

            return message;
        }
    }
}
