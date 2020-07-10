using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Injazat_Rami
{
    class MemberRegister
    {
        public void Execute()
        {
            try
            {
                string MRPath = System.Configuration.ConfigurationManager.AppSettings.Get("MemberRegisterInput").ToString();
                string MRCollection = System.Configuration.ConfigurationManager.AppSettings.Get("MemberRegisterCollected").ToString();

                List<string> Files = FetchFiles(MRPath);
                foreach (string file in Files)
                {
                    Console.WriteLine("Working on file " + file);
                    Custom.MRInfo("Working on file " + file);
                    string filename = Path.GetFileName(file);

                    List<MemberXML> obj_List = ExtractXMLData(file, Path.GetFileNameWithoutExtension(filename));
                    string result = GenerateJSON(obj_List);
                    if (UploadTransaction(result))
                    {
                        Console.WriteLine("Upload Success for file " + Path.GetFileNameWithoutExtension(result));
                        MoveFile(file, MRCollection + filename);
                    }
                    else
                    {
                        Console.WriteLine("Upload fail for file " + Path.GetFileNameWithoutExtension(result));
                    }
                }

                Console.WriteLine("Member Regiser Process Complete");
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }
        }

        private List<MemberXML> ExtractXMLData(string path,string filename)
        {
            List<MemberXML> List_Obj_xml = new List<MemberXML>();
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(path);
                XmlNodeList Members = xdoc.SelectNodes("Members//Member");
                foreach (XmlNode Member in Members)
                {

                    MemberXML Obj_xml = new MemberXML();

                    Obj_xml.payerId = Custom.TranformPayer(Custom.CheckifNodeExist(Member.SelectSingleNode("payerId")));
                    Obj_xml.insuredId = Custom.CheckifNodeExist(Member.SelectSingleNode("insuredId"));
                    //Obj_xml.divisionNumber = Custom.CheckifNodeExist(Member.SelectSingleNode("divisionNumber"));
                    //Obj_xml.groupNumber = Obj_xml.divisionNumber;

                    Obj_xml.memberNo = Custom.CheckifNodeExist(Member.SelectSingleNode("memberNo"));
                    Obj_xml.relationship = Custom.TransformRelationShip(Custom.CheckifNodeExist(Member.SelectSingleNode("relationship")));

                    Obj_xml.firstName = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("firstName")));
                    Obj_xml.gender = Custom.TransformGender(Custom.CheckifNodeExist(Member.SelectSingleNode("gender")));
                    Obj_xml.dob = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("dob")));
                    Obj_xml.terminationDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("terminationDate")));
                    //Obj_xml.effectiveDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("effectiveDate")));
                    Obj_xml.inceptionDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("inceptionDate")));

                    Obj_xml.fileName = filename;
                    Obj_xml.lastName = "LN";
                    Obj_xml.lineNo = "1";
                    Obj_xml.nationalId = "999-9999-9999999-9";
                    Obj_xml.phoneNo = "97120000000";
                    Obj_xml.email = "test@abc.com";
                    Obj_xml.divisionNumber = "GRP001";
                    Obj_xml.groupNumber = "GRP001";

                    Obj_xml.terminationDate = "1605045600000";
                    Obj_xml.effectiveDate = "1582754400000";

                    List_Obj_xml.Add(Obj_xml);


                    //Obj_xml.PayerID = Custom.CheckifNodeExist(Member.SelectSingleNode("PayerID"));
                    //Obj_xml.InsuredID = Custom.CheckifNodeExist(Member.SelectSingleNode("InsuredID"));
                    //Obj_xml.Division = Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//Division"));
                    //Obj_xml.Policy = Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//Policy"));
                    //Obj_xml.MemberNo = Custom.CheckifNodeExist(Member.SelectSingleNode("MemberNo"));
                    //Obj_xml.Relationship = Custom.TransformRelationShip(Custom.CheckifNodeExist(Member.SelectSingleNode("Relationship")));
                    //Obj_xml.LastName = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("LastName")));
                    //Obj_xml.FirstName = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("FirstName")));
                    //Obj_xml.PhoneNO = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("PhoneNO")));
                    //Obj_xml.Gender = Custom.TransformGender(Custom.CheckifNodeExist(Member.SelectSingleNode("Gender")));
                    //Obj_xml.BirthDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("BirthDate")));
                    //Obj_xml.TerminationDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//TerminationDate")));
                    //Obj_xml.EffectiveDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//EffectiveDate")));
                    //Obj_xml.InceptionDate = Custom.TranformDatetoMilliseconds(Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//InceptionDate")));

                    //Obj_xml.Policy = Custom.CheckifNodeExist(Member.SelectSingleNode("Contract//Policy"));
                    //Obj_xml.LastName = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("LastName")));
                    //Obj_xml.PhoneNO = Custom.TransformNullValues(Custom.CheckifNodeExist(Member.SelectSingleNode("PhoneNO")));


                    //groupNumber


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }

            return List_Obj_xml;
        }
        private string GenerateJSON(List<MemberXML> listXML)
        {
            string result = string.Empty;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");

                foreach (MemberXML member in listXML)
                {
                    sb.Append(TransformXML(member) + ",");
                    if (listXML.Count > 49)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        result = WriterJson(sb.Append("]").ToString(), member.insuredId);
                        sb = null;

                        if(sb == null)
                        {
                            Console.Write("ficka");
                        }
                    }
                }

                if (listXML.Count < 50)
                {
                    sb.Remove(sb.Length - 1, 1);
                    result = WriterJson(sb.Append("]").ToString(), listXML[0].insuredId.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);

            }

            return result;
        }
        private static bool UploadTransaction(string result)
        {
            bool resultB = false;
            try
            {
                resultB =  MemberAPI.API(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }

            return resultB;
        }

        private static string TransformXML(MemberXML XML)
        {
            string result = string.Empty;
            try
            {
                result = JsonConvert.SerializeObject(XML);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);

            }
            return result;
        }
        private static string WriterJson(string data, string InsuredID)
        {
            string result = string.Empty;
            string path = System.Configuration.ConfigurationManager.AppSettings.Get("MemberRegisterOutput").ToString();
            result = path + "\\" + InsuredID + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
            try
            {
                using (StreamWriter writer = File.CreateText(result))
                {
                    writer.Write(data);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);

            }
            return result;
        }

        private void MoveFile(string source, string destination)
        {
            try
            {
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                File.Move(source, destination);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }
        }
        private List<string> FetchFiles(string path)
        {
            List<string> List_Files = new List<string>();
            Console.WriteLine("Fetching Files");
            try
            {
                int takecount = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("MRTakeCount"));
                var dir = new DirectoryInfo(path);
                List_Files = dir.EnumerateFiles("*.xml", SearchOption.AllDirectories).Take(takecount).ToList().Select(l => l.FullName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }
            return List_Files;
        }

    }
}
