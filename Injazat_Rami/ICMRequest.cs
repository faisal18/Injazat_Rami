using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Injazat_Rami
{
    class ICMRequest
    {
        public static bool Execute(string XML,string filename)
        {
            bool result = false;
            string failpath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimUploadFail");
            string SoapEnvelope = string.Empty;
            try
            {
                HttpWebRequest request = CreateWebRequest();
                XmlDocument soapEnvelopeXml = new XmlDocument();
                if (Custom.CheckifwithbodyXML(XML))
                {
                    soapEnvelopeXml.LoadXml(XML);
                }
                else
                {
                    soapEnvelopeXml.LoadXml(AppendEnvelope(AddNamespace(XML)));
                }

                SoapEnvelope = soapEnvelopeXml.OuterXml;

                string proxy = System.Configuration.ConfigurationManager.AppSettings.Get("Proxy");
                request.Proxy = new WebProxy("http://10.198.7.51:8181", true);

                using (Stream stream = request.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        string soapResult = rd.ReadToEnd();
                        result = true;
                        Console.WriteLine("Writing request and response for file: " + filename);
                        soapResult = RemapActivityID(soapResult);
                        CreateOutput(filename, soapEnvelopeXml.OuterXml, soapResult);
                    }
                }
            }

           
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
                CreateOutputFail(filename, SoapEnvelope, ex.Message + ex.InnerException + ex.Source);
                SaveXML(SoapEnvelope, filename, failpath);
            }
            return result;
        }
        private static void CreateOutput(string filename, string request, string response)
        {
            try
            {
                string delimeter = "|";
                string outputfilepath = System.Configuration.ConfigurationManager.AppSettings.Get("ResultPath") + "Output.csv";
                string Eachoutputfilepath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimUploadSuccess") + "\\" + filename;

                if (!File.Exists(outputfilepath))
                {
                    using (StreamWriter writer = File.AppendText(outputfilepath))
                    {
                        writer.Write("DateTime" + delimeter +"Filename"+delimeter+ "Request" + delimeter + "Response\n");
                    }
                }
                using (StreamWriter writer = File.AppendText(outputfilepath))
                {
                    writer.Write(DateTime.Now.ToString("yyyyMMddHHmmss") + delimeter + filename + delimeter + (request.Replace('\n', ' ')) + delimeter + (response.Replace('\n', ' ')) + "\n");
                }

                string nowdatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string ResponsePath = Eachoutputfilepath + "_response_" + nowdatetime + ".xml";
                string RequestPath = Eachoutputfilepath + "_request_" + nowdatetime + ".xml";
                using (StreamWriter writer = File.CreateText(ResponsePath))
                {
                    writer.Write(response);
                }
                using (StreamWriter writer = File.CreateText(RequestPath))
                {
                    writer.Write(request);
                }
            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message);
            }
        }
        private static void CreateOutputFail(string filename, string request, string response)
        {
            try
            {
                string delimeter = "|";
                string outputfilepath = System.Configuration.ConfigurationManager.AppSettings.Get("ResultPath") + "Output.csv";

                if (!File.Exists(outputfilepath))
                {
                    using (StreamWriter writer = File.AppendText(outputfilepath))
                    {
                        writer.Write("DateTime" + delimeter + "Filename" + delimeter + "Request" + delimeter + "Response\n");
                    }
                }
                using (StreamWriter writer = File.AppendText(outputfilepath))
                {
                    writer.Write(DateTime.Now.ToString("yyyyMMddHHmmss") + delimeter + filename + delimeter + (request.Replace('\n', ' ')) + delimeter + (response.Replace('\n', ' ')) + "\n");
                }

            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message);
            }
        }
        private static void  SaveXML(string data, string filename, string path)
        {
            string pathsaveXML = string.Empty;
            Console.WriteLine("Saving File");
            try
            {
                using (StreamWriter writer = File.CreateText(path + filename+".xml"))
                {
                    writer.Write(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Custom.Info("Error saving file " + filename + " to path " + path + " reason \n" + ex.Message + "\n" + ex.InnerException);
            }
        }


        private static string RemapActivityID(string response)
        {
            string result = string.Empty;
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(response);


                XmlNodeList activites = xdoc.SelectNodes("//SubmissionResponse//Transaction//Activity");
                foreach(XmlNode activity in activites)
                {
                    string tofind = activity.SelectSingleNode("ID").InnerText;

                    activity.SelectSingleNode("ID").InnerText = FetchActivityID(tofind);
                }

                result = xdoc.OuterXml;

            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
            return result;
        }
        private static string FetchActivityID(string truncatedActivityID)
        {
            string result = truncatedActivityID;
            string base_dire = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimActivityPath");
            try
            {
                string[] lines =  File.ReadAllLines(base_dire);
                foreach(string row in lines)
                {
                    if (row.Split(',')[2] == truncatedActivityID)
                    {
                        result = row.Split(',')[1];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
            return result;
        }

        private static HttpWebRequest CreateWebRequest()
        {
            string ICMURL = System.Configuration.ConfigurationManager.AppSettings.Get("ICMUrl");
            HttpWebRequest webRequest = null;

            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create(ICMURL);
                webRequest.Headers.Add(@"SOAP:Action");
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return webRequest;
        }
        private static string AddNamespace(string XML)
        {
            string result = string.Empty;
            try
            {

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(XML);

                XmlElement temproot = xdoc.CreateElement("ws", "ICMRequest", "http://ws.icm.dhc.com/");
                temproot.InnerXml = xdoc.DocumentElement.InnerXml;
                result = temproot.OuterXml;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }
        private static string AppendEnvelope(string data)
        {
            string head= @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" ><soapenv:Header/><soapenv:Body>";
            string end = @"</soapenv:Body></soapenv:Envelope>";
            return head + data + end;
        }


    }

}
