using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace NetworkingTools
{
    //struktura pozwalajaca na przechowywanie dwóch wartosci typu string
    public struct Data
    {
        public Data(string keysettings, string strValue)
        {
            Keysettings = keysettings;
            SettingsValue = strValue;
        }
        //klucz wartosci z pliku konfiguracyjnego
        public string Keysettings { get; private set; }
        //wartość z pliku konfiguracyjnego
        public string SettingsValue { get; private set; }
    }


    public class OperationConfiguration
    {



        /*Funkcja zwraca liste struktur Data 
        *Odczytuje wszystkie ustawienia z pliku konfiguracyjnego i dodaje je do listy
        */
        public static List<Data> ReadAllSettings(NameValueCollection appSettings)
        {
            List<Data> settings = new List<Data>();

            try
            {

                //jeżeli nie ma ustawień to funkcja zwraca null
                if (appSettings.Count == 0)
                {
                    return null;
                }
                else
                {
                    //czyta wszystkie ustawienia według tablicy kluczy
                    foreach (var key in appSettings.AllKeys)
                    {
                        //dla każdego klucza dodaję ustawienia dla tego klucza
                        settings.Add(new Data(key, appSettings[key]));
                    }
                    return settings;
                }
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }

        public static List<string> ReadAllKeys(NameValueCollection appSettings)
        {
            List<string> list = new List<string>();
            try
            {
                //wczytanie wszystkich własności z App.config

                //jeżeli nie ma ustawień to funkcja zwraca null
                if (appSettings.Count == 0)
                {
                    return null;
                }
                else
                {
                    //czyta wszystkie ustawienia według tablicy kluczy
                    foreach (var key in appSettings.AllKeys)
                    {
                        //dodaje klucze do listy
                        list.Add(key);
                    }
                    return list;
                }
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }
        //Funkcja pozwala na pobranie wybranej własności, ustawien z App.config
        public static string getSetting(string key, NameValueCollection appSettings)
        {
            try
            {

                //zwraca wartość własności dla określonego klucza
                //Gdy nie ma takiego klucza to zwracamy null
                string result = appSettings[key] ?? null;
                //zwracamy znaleziona wlasnosc
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }
        public static NameValueCollection readSettings()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            return appSettings;
        }

    }
}
