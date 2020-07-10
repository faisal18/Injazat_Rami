using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Injazat_Rami
{
    class Claims
    {
        public void Execute()
        {
            string inputpath = string.Empty;
            string collectionpath = string.Empty;
            try
            {
                inputpath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimInput");
                collectionpath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimCollected");
                List<string> List = FetchXML(inputpath);
                if (List.Count > 0)
                {
                    for (int i = 0; i < List.Count; i++)
                    {
                        try
                        {
                            Console.WriteLine("Working on file " + List[i].ToString());
                            XmlDocument xdoc = new XmlDocument();

                            xdoc = TransformXML(List[i], true);
                            //Save Mainfile
                            SaveXML(xdoc, Path.GetFileNameWithoutExtension(List[i]) + "_MAIN");

                            //If to send divided
                            List<string> List_xml = DivideAndConquery(xdoc.OuterXml);
                            for (int j = 0; j < List_xml.Count; j++)
                            {
                                string filename = Path.GetFileNameWithoutExtension(List[i]) + "_Part_" + j;

                                SaveXML(List_xml[j], filename);
                                SendXml(List_xml[j], Path.GetFileNameWithoutExtension(filename));
                            }

                            //If not to divide
                            //SaveXML(xdoc, Path.GetFileNameWithoutExtension(List[i]));
                            //if (SendXml(xdoc.OuterXml.ToString(), Path.GetFileNameWithoutExtension(List[i])))
                            //{
                            //    MoveFile(List[i], collectionpath + Path.GetFileName(List[i]));
                            //}

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Custom.Info("Error in loop for filename " + Path.GetFileNameWithoutExtension(List[i]));
                            Custom.Info(ex.Message + ex.InnerException + ex.Source);
                        }
                    }
                }
                //SendXml();
                Console.WriteLine("Program complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.Read();
        }

        #region Controls
        private List<string> DivideAndConquery(string xml)
        {
            List<string> list_xmls = new List<string>();
            try
            {
                var xDoc = XDocument.Parse(xml); // loading source xml
                var xmls = xDoc.Root.Elements().ToArray(); // split into elements
                string Head = "<ICMRequest>";
                string bottom = "</ICMRequest>";
                string tempo = string.Empty;
                string creds = string.Empty;

                creds = xmls[0].ToString() + xmls[1].ToString();
                Head = Head + creds;

                for (int i = 2; i < xmls.Length; i++)
                {
                    string result = Head + xmls[i].ToString() + bottom;
                    list_xmls.Add(result);
                }
            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
            return list_xmls;
        }
        private void SaveXML(string xdoc, string filename)
        {
            string pathsaveXML = string.Empty;
            Console.WriteLine("Saving File");
            try
            {
                pathsaveXML = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimTransform");
                filename = pathsaveXML + "\\" + filename + ".xml";
                File.AppendAllText(filename, xdoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void SaveXML(XmlDocument xdoc, string filename)
        {
            string pathsaveXML = string.Empty;
            Console.WriteLine("Saving File");
            try
            {
                pathsaveXML = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimTransform");
                filename = pathsaveXML + "\\" + filename + ".xml";
                xdoc.Save(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private bool SendXml(string context, string filename_withoutextentions)
        {
            bool result = false;
            try
            {
                //string XMLPath = @"C:\tmp\Injazat\Claims\Output\20190930110533.xml";
                //string failpath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimUploadFail");

                Console.WriteLine("Uploading file: " + filename_withoutextentions);
                if (ICMRequest.Execute(context, filename_withoutextentions))
                {
                    Console.WriteLine("file uploaded successfully");
                    result = true;
                }
                else
                {
                    Console.WriteLine("Error Uploading File " + filename_withoutextentions);
                    Custom.Info("Error Uploading File " + filename_withoutextentions);
                    //SaveXML(context, filename_withoutextentions + ".xml", failpath);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
            return result;
        } 
        #endregion


        #region HelperFunctions


        private void MoveFile(string source,string destination)
        {
            try
            {
                if(File.Exists(destination))
                {
                    File.Delete(destination);
                }
                File.Move(source, destination);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
        }
        private void SendXml()
        {
            try
            {
                //string XMLPath = @"C:\tmp\Injazat\Claims\Output\20190930110533.xml";
                string outputpath = System.Configuration.ConfigurationManager.AppSettings.Get("ClaimTransform");
                string[] files = Directory.GetFiles(outputpath, "*.*");
                foreach (string file in files)
                {
                    string context = File.ReadAllText(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    Console.WriteLine("Uploading file: " + file);
                    if (ICMRequest.Execute(context, filename))
                    {
                        Console.WriteLine("file uploaded successfully");
                    }
                    else
                    {
                        Console.WriteLine("Error Uploading File " + file);
                        Custom.Info("Error Uploading File " + file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private List<string> FetchXML(string path)
        {
            List<string> List_Files = new List<string>();
            Console.WriteLine("Fetching Files");
            try
            {
                int takecount = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("TakeCount"));
                var dir = new DirectoryInfo(path);
                List_Files = dir.EnumerateFiles("*.xml", SearchOption.AllDirectories).Take(takecount).ToList().Select(l => l.FullName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.Info(ex.Message + ex.InnerException + ex.Source);
            }
            return List_Files;
        }
        private XmlDocument TransformXML(string filepath, bool isSaop)
        {
            XmlDocument xdoc = new XmlDocument();
            Console.WriteLine("Transforming File");
            try
            {
                xdoc.Load(filepath);
                xdoc.SelectSingleNode("//Username").InnerText = "syncUser";
                xdoc.SelectSingleNode("//Key").InnerText = "8nFL2Ba4-1A34-4a4a-b14b-7fB12eAF3f32";

                XmlNodeList Submissions = xdoc.SelectNodes("//Submission");

                foreach (XmlNode Submission in Submissions)
                {

                    //TransactionType should be 'Claim'
                    Submission.SelectSingleNode("Header/TransactionType").InnerText = "Claim";

                    //Add TransactionID tag
                    XmlNode TransactionID = Submission.SelectSingleNode("Transaction/ID");
                    //if (!Custom.CheckifNodeExistB(TransactionID))
                    //{
                    //    XmlNode Transaction = Submission.SelectSingleNode("Transaction");
                    //    Transaction.PrependChild(getTransactionID(xdoc));
                    //}
                    //Obsolete

                    //Add SubmissionDestination tag
                    //if (!Custom.CheckifNodeExistB(Submission.SelectSingleNode("SubmissionDestination")))
                    //{
                    //    XmlNode Header = Submission.SelectSingleNode("Header");
                    //    Header.AppendChild(getSubmissionDestination(xdoc));
                    //}
                    //Obsolete

                    XmlNodeList Encounters = Submission.SelectNodes("Transaction/Encounter");
                    //if (Encounters.Count < 1) 
                    //{
                    //    XmlNode Transaction = Submission.SelectSingleNode("Transaction");
                    //    string ProviderID = Transaction.SelectSingleNode("ProviderID").InnerText;
                    //    Transaction.AppendChild(getEncounter(xdoc, ProviderID));
                    //    Encounters = Submission.SelectNodes("Transaction/Encounter");
                    //}
                    //Obsolete

                    foreach (XmlNode Encounter in Encounters)
                    {
                        //Transform Encounter Date
                        if (Custom.CheckifNodeExistB(Encounter.SelectSingleNode("Start")))
                        {
                            Encounter.SelectSingleNode("Start").InnerText = Custom.TransformTDate(Encounter.SelectSingleNode("Start").InnerText);
                        }
                        if (Custom.CheckifNodeExistB(Encounter.SelectSingleNode("End")))
                        {
                            Encounter.SelectSingleNode("End").InnerText = Custom.TransformTDate(Encounter.SelectSingleNode("End").InnerText);
                        }
                    }


                    //Related to Activity only
                    XmlNodeList Activities = Submission.SelectNodes("Transaction//Activity");
                    foreach (XmlNode Acitiviy in Activities)
                    {
                        //Transform Activity Date
                        if (Custom.CheckifNodeExistB(Acitiviy.SelectSingleNode("Start")))
                        {
                            Acitiviy.SelectSingleNode("Start").InnerText = Custom.TransformTDate(Acitiviy.SelectSingleNode("Start").InnerText);
                        }
                        if (Custom.CheckifNodeExistB(Acitiviy.SelectSingleNode("End")))
                        {
                            Acitiviy.SelectSingleNode("End").InnerText = Custom.TransformTDate(Acitiviy.SelectSingleNode("End").InnerText);
                        }

                        // ActivityID should be Truncated to 30 CHARs
                        XmlNode ActivityId = Acitiviy.SelectSingleNode("ID");
                        ActivityId.InnerText = Custom.TruncateAcitivtiyID(ActivityId.InnerText, TransactionID.InnerText);

                        // Ordering Clinician value should be replicated in the Clinician field if the clinician is not available
                        if (Custom.CheckifNodeExistB(Acitiviy.SelectSingleNode("Clinician")))
                        {
                            Acitiviy.SelectSingleNode("Clinician").InnerText = Acitiviy.SelectSingleNode("OrderingClinician").InnerText;
                        }

                        //Acitiviy.AppendChild(getCodeTerm(xdoc));
                        //Code Term values (Currently sent as activity Type) should be replaced with EE Code Term values
                        Acitiviy.SelectSingleNode("CodeTerm").InnerText = Custom.MapCodeTerm(Acitiviy.SelectSingleNode("CodeTerm").InnerText);

                        //Add Financial Override
                        //Acitiviy.AppendChild(getXMlElem(xdoc));
                        //The above has now been obseleted now it has been moved under diagnosis with other tags
                    }

                    //Related to Diagnosis
                    //remove the diagnosi with Reasonforvisit
                    XmlNodeList Diagnosises = Submission.SelectNodes("Transaction//Diagnosis");
                    foreach (XmlNode diagnois in Diagnosises)
                    {
                        if (Custom.CheckifNodeExistB(diagnois.SelectSingleNode("Type")))
                        {
                            if(diagnois.SelectSingleNode("Type").InnerText == "ReasonForVisit")
                            {
                                diagnois.ParentNode.RemoveChild(diagnois);
                            }
                        }
                    }

                    //TO be commented as by RAMI on 17 Feb 2020
                    //XmlNodeList Diagnosises = Submission.SelectNodes("Transaction/Diagnosis");
                    //foreach (XmlNode Diagnosis in Diagnosises)
                    //{
                    //    //This is to optional for now, once Rami will confirm this then we will enable it
                    //    Diagnosis.AppendChild(getScrubbingHint(xdoc));
                    //}

                }
                Console.WriteLine("Process complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return xdoc;
        }
        private XmlDocument AppendSubmissionDestination(string filepath)
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.Load(filepath);

                XmlNodeList Submissions = xdoc.SelectNodes("ICMRequest//Submission");
                foreach (XmlNode Submission in Submissions)
                {
                    XmlNode Header = Submission.SelectSingleNode("Header");
                    Header.AppendChild(getSubmissionDestination(xdoc));
                }
            }
            catch (Exception ex)
            {
                Custom.Info(ex.Message);
            }
            return xdoc;
        }
        private XmlElement getScrubbingHint(XmlDocument SourceNode)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;

                XmlElement type = xdoc.CreateElement("ScrubbingHint");

                XmlElement Code = xdoc.CreateElement("Code");
                Code.InnerText = "FINANCIALS_OVERRIDE";

                XmlElement Value = xdoc.CreateElement("Value");
                Value.InnerText = "ALL";

                type.AppendChild(Code);
                type.AppendChild(Value);

                yo = type;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;
        }

        private XmlElement getXMlElem(XmlDocument SourceNode)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;

                XmlElement type = xdoc.CreateElement("type");
                type.InnerText = "FINANCIAL";
                XmlElement overrideDetail = xdoc.CreateElement("overrideDetail");
                XmlElement coverageDetail = xdoc.CreateElement("coverageDetail");
                XmlElement extraInfo = xdoc.CreateElement("extraInfo");
                XmlElement referenceId = xdoc.CreateElement("referenceId");
                referenceId.InnerText = "1752130";

                overrideDetail.AppendChild(type);
                coverageDetail.AppendChild(overrideDetail);
                extraInfo.AppendChild(coverageDetail);
                extraInfo.AppendChild(referenceId);
                yo = extraInfo;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;
        }
        private XmlElement getCodeTerm(XmlDocument SourceNode)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;
                XmlElement type = xdoc.CreateElement("CodeTerm");
                yo = type;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;

        }
        private XmlElement getSubmissionDestination(XmlDocument SourceNode)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;
                XmlElement type = xdoc.CreateElement("SubmissionDestination");
                type.InnerText = "2";
                yo = type;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;

        }
        private XmlElement getTransactionID(XmlDocument SourceNode)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;
                XmlElement type = xdoc.CreateElement("ID");
                type.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss");
                yo = type;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;

        }
        private XmlElement getEncounter(XmlDocument SourceNode,string ProviderID)
        {
            XmlElement yo = null;
            try
            {

                XmlDocument xdoc = SourceNode;
                XmlElement Encounter = xdoc.CreateElement("Encounter");
                yo = Encounter;

                XmlElement FacilityID = xdoc.CreateElement("FacilityID");
                FacilityID.InnerText = ProviderID;

                XmlElement Type = xdoc.CreateElement("Type");
                Type.InnerText = "1";

                XmlElement Start = xdoc.CreateElement("Start");
                Start.InnerText = "2017-01-01T00:00:00";

                XmlElement End = xdoc.CreateElement("End");
                End.InnerText = "2017-01-01T00:00:00";

                XmlElement StartType = xdoc.CreateElement("StartType");
                StartType.InnerText = "1";

                XmlElement EndType = xdoc.CreateElement("EndType");
                EndType.InnerText = "1";

                Encounter.AppendChild(FacilityID);
                Encounter.AppendChild(Type);
                Encounter.AppendChild(Start);
                Encounter.AppendChild(End);
                Encounter.AppendChild(StartType);
                Encounter.AppendChild(EndType);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return yo;

        }
        #endregion

    }
}
