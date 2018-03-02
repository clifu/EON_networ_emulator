using NetworkingTools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewNMS
{
    public class Reading
    {/*
        private OperationConfiguration oc = new OperationConfiguration();

        public string DataInCommutationTableRow()
        {
            string combindedString;
            string connections = string.Empty;
            List<Data> data = new List<Data>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            List<string> conn = new List<string>();

            foreach (var key in data)
            {

                if (key.Keysettings.StartsWith("commtable"))
                {
                    conn.Add(((key.SettingsValue).ToString()));
                }

            }
            combindedString = string.Join("#", conn.ToArray());
            string info_to_agent = combindedString.ToString();

            return info_to_agent;
        }

        public string DataInEONTableRowIN()
        {
            string combindedString;
            string connections = string.Empty;
            List<Data> data = new List<Data>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            List<string> conn = new List<string>();

            foreach (var key in data)
            {

                if (key.Keysettings.StartsWith("EONTableIN"))
                {
                    conn.Add(((key.SettingsValue).ToString()));
                }

            }
            combindedString = string.Join("$", conn.ToArray());
            string info_to_agent = combindedString.ToString();

            return info_to_agent;
        }

        public string DataInEONTableRowOUT()
        {
            string combindedString;
            string connections = string.Empty;
            List<Data> data = new List<Data>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            List<string> conn = new List<string>();

            foreach (var key in data)
            {

                if (key.Keysettings.StartsWith("EONTableOUT"))
                {
                    conn.Add(((key.SettingsValue).ToString()));
                }

            }
            combindedString = string.Join("&", conn.ToArray());
            string info_to_agent = combindedString.ToString();

            return info_to_agent;
        }


        public string DataInBorderTable()
        {
            string combindedString;
            string connections = string.Empty;
            List<Data> data = new List<Data>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            List<string> conn = new List<string>();

            foreach (var key in data)
            {
                if (key.Keysettings.StartsWith("BorderTable"))
                {
                    conn.Add(((key.SettingsValue).ToString()));
                }
            }
            combindedString = string.Join("*", conn.ToArray());
            string info_to_agent = combindedString.ToString();

            return info_to_agent;
        }


   
*/
    }
}
