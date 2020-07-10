using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Injazat_Rami
{
    public static class Custom
    {
        public static void Info(string data)
        {
            string base_dire = System.Configuration.ConfigurationManager.AppSettings.Get("LogPath");
            using (StreamWriter writer = File.AppendText(base_dire))
            {
                writer.Write(DateTime.Now.ToString() + " : " + data.ToString() + "\n");
            }
        }
        public static void ActivityMapping(string transactionID,string OriginalActivityID,string TruncatedActivityID)
        {
            string base_dire = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimActivityPath");
            using (StreamWriter writer = File.AppendText(base_dire)) 
            {
                writer.Write(transactionID + "," + OriginalActivityID + "," + TruncatedActivityID + "\n");
            }
        }
        public static void MRInfo(string data)
        {
            string base_dire = System.Configuration.ConfigurationManager.AppSettings.Get("MRLogPath");
            using (StreamWriter writer = File.AppendText(base_dire))
            {
                writer.Write(DateTime.Now.ToString() + " : " + data.ToString() + "\n");
            }
        }
        public static string TranformDatetoMilliseconds(string data)
        {
            string result = string.Empty;
            DateTime d1 = new DateTime();
            try
            {
                if (DateTime.TryParse(data, out d1))
                {
                    result = d1.ToUniversalTime().
                        Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).
                        TotalMilliseconds.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }
        public static string TransformDate(string data)
        {

            try
            {
                string[] DateSplitter = data.Split('/');
                string[] TimeSplitter = DateSplitter[2].Split(':');
                string[] yearSplitter = TimeSplitter[0].Split(' ');

                int Year = int.Parse(yearSplitter[0]);
                int Month = int.Parse(DateSplitter[1]);
                int Day = int.Parse(DateSplitter[0]);
                int Hour = int.Parse(yearSplitter[1]);
                int Minute = int.Parse(TimeSplitter[1]);
                int Second = 00;


                DateTime newdatetime = new DateTime(Year, Month, Day, Hour, Minute, Second);
                data = newdatetime.ToString("yyyy-MM-dd HH:mm:ss");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return data;
        }
        public static string TransformGender(string data)
        {
            string result = string.Empty;
            try
            {
                switch (data)
                {
                    case "1":
                        result = "M";
                        break;
                    case "0":
                        result = "F";
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.Read();
            }
            return result;
        }
        public static string TranformPayer(string data)
        {
            string result = string.Empty;
            try
            {
                switch (data)
                {
                    case "D001":
                        result = "2";
                        break;
                    case "E001":
                        result = "3";
                        break;
                    case "A001":
                        result = "4";
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.Read();
            }
            return result;
        }
        public static string TransformRelationShip(string data)
        {
            string result = string.Empty;
            try
            {
                switch (data)
                {
                    case "Principal":
                        result = "I";
                        break;
                    case "Spouse":
                        result = "S";
                        break;
                    case "Child":
                        result = "D";
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }
        public static string TransformNullValues(string data)
        {
            string result = string.Empty;
            try
            {
                if (data.Length > 0)
                {
                    result = data;
                }
                else
                {
                    result = "Null";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }
        public static string CheckifNodeExist(XmlNode node)
        {
            string result = string.Empty;
            try
            {
                if (node != null)
                {
                    result = node.InnerText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }

        public static bool CheckifNodeExistB(XmlNode node)
        {
            bool result = false;
            try
            {
                if (node != null)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;
        }
        public static string TransformTDate(string data)
        {
            string result = string.Empty;
            DateTimeOffset d_temp = new DateTimeOffset();
            try
            {
                //DateTimeOffset d =  DateTimeOffset.Parse(data);
                if (DateTimeOffset.TryParse(data, out d_temp))
                {

                    DateTime d1 = d_temp.DateTime;
                    result = d1.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    result = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static string TrimData(string data)
        {
            string result = string.Empty;
            try
            {
                data = data.Replace("\r", "");
                result = data.Replace("\n", "");


            }
            catch (Exception ex)
            {
                Info(ex.Message);
            }
            return result;
        }

        public static bool Checkifwithbody(string filecontent)
        {
            bool result = false;
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filecontent);
                XmlNode node = xdoc.FirstChild;
                if (node.LocalName == "Envelope")
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message);
            }
            return result;
        }
        public static bool CheckifwithbodyXML(string filecontent)
        {
            bool result = false;
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(filecontent);
                XmlNode node = xdoc.FirstChild;
                if (node.LocalName == "Envelope")
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message);
            }
            return result;
        }

        public static string MapCodeTerm(string data)
        {
            string result = data;
            try
            {
                switch (data)
                {
                    case "3":
                        result = "CPT_04_2012";
                        break;
                    case "6":
                        result = "CDT";
                        break;
                    case "4":
                        result = "HCPCS";
                        break;
                    case "8":
                        result = "DSL";
                        break;
                    case "5":
                        result = "HAAD";
                        break;
                    case "9":
                        result = "DRG";
                        break;


                }

            }
            catch (Exception ex)
            {
                Info(ex.Message);
            }

            return result;
        }

        public static string TruncateAcitivtiyID(string data,string transactionID)
        {
            string result = string.Empty;


            try
            {
                if(data.Length>30)
                {
                    result = data.Substring(0, 29);
                    ActivityMapping(transactionID, data, result);
                }
                else
                {
                    result = data;
                }

                
            }
            catch (Exception ex)
            {
                Info(ex.Message);
            }
            return result;
        }

    }
}
