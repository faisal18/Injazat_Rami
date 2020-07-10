using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Injazat_Rami
{
    class MemberAPI
    {
        public static bool API(string filename)
        {
            bool result_b = false;
            string result = string.Empty;

            string MRSuccessPath = System.Configuration.ConfigurationManager.AppSettings.Get("MRSuccessPath");
            string MRFailPath = System.Configuration.ConfigurationManager.AppSettings.Get("MRFailPath");


            Custom.MRInfo("Uploading Member Register file " + Path.GetFileNameWithoutExtension(filename));

            try
            {

                //filename = @"C:\Important Files\Injazat\Members\Main Files\good_Request_2.json";
                result_b = POST_FILE_Call(filename, true, MRFailPath, MRSuccessPath);

                //string data = File.ReadAllText(@"C:\Important Files\Injazat\Members\Main Files\good_Request_2.json");
                //data = "[\r\n    {\r\n        \"divisionNumber\": \"GRP001\",\r\n        \"dob\": \"1503187200000\",\r\n        \"effectiveDate\": 1582803285000,\r\n        \"email\": \"\",\r\n        \"fileName\": \"Test_x\",\r\n        \"firstName\": \"Faisal\",\r\n        \"gender\": \"M\",\r\n        \"groupNumber\": \"GRP001\",\r\n        \"inceptionDate\": 1582803285000,\r\n        \"insuredId\": \"1234567_X\",\r\n        \"lastName\": \"SAB\",\r\n        \"lineNo\": 1,\r\n        \"memberNo\": \"Member_Y\",\r\n        \"nationalId\": \"1234567_Y\",\r\n        \"payerId\": 1,\r\n        \"phoneNo\": \"05011784\",\r\n        \"relationship\": \"I\",\r\n        \"terminationDate\": 1605008085000\r\n    }\r\n]";


                //using (WebClient wc = new WebClient())
                //{
                //    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                //    wc.Headers.Add("User-Agent: Other");
                //    wc.Headers.Add("Content-Type:multipart/form-data");
                //    wc.Headers.Set("user", username);
                //    wc.Headers.Set("key", key);

                //    string data = File.ReadAllText(@"C:\Important Files\Injazat\Members\Main Files\good_Request_2.json");
                //    data = "[\r\n    {\r\n        \"divisionNumber\": \"GRP001\",\r\n        \"dob\": \"1503187200000\",\r\n        \"effectiveDate\": 1582803285000,\r\n        \"email\": \"\",\r\n        \"fileName\": \"Test_x\",\r\n        \"firstName\": \"Faisal\",\r\n        \"gender\": \"M\",\r\n        \"groupNumber\": \"GRP001\",\r\n        \"inceptionDate\": 1582803285000,\r\n        \"insuredId\": \"1234567_X\",\r\n        \"lastName\": \"SAB\",\r\n        \"lineNo\": 1,\r\n        \"memberNo\": \"Member_Y\",\r\n        \"nationalId\": \"1234567_Y\",\r\n        \"payerId\": 1,\r\n        \"phoneNo\": \"05011784\",\r\n        \"relationship\": \"I\",\r\n        \"terminationDate\": 1605008085000\r\n    }\r\n]";
                //    //data = "[{\"divisionNumber\": \"GRP001\",\"dob\": \"1503187200000\",\"effectiveDate\": 1582803285000,\"email\": \"\",\"fileName\": \"Test_x\",\"firstName\": \"Faisal\",\"gender\": \"M\",\"groupNumber\": \"GRP001\",\"inceptionDate\": 1582803285000,\"insuredId\": \"1234567_X\",\"lastName\": \"SAB\",\"lineNo\": 1,\"memberNo\": \"Member_Y\",\"nationalId\": \"1234567_Y\",\"payerId\": 1,\"phoneNo\": \"05011784\",\"relationship\": \"I\",\"terminationDate\": 1605008085000}]";
                //    //byte[] B_result = wc.UploadFile(URL, data);

                //    //byte[] B_result = wc.UploadFile(URL, data);
                //    //result = Encoding.UTF8.GetString(B_result);

                //    //Custom.MRInfo("Upload Success for file " + filename);
                //    //result_b = true;

                //    //CreateOutput(Path.GetFileName(filename), File.ReadAllText(filename), result);
                //    //SaveFile(File.ReadAllText(filename), Path.GetFileName(filename), MRSuccessPath);
                //}
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    result = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                else
                    result = ex.Message + ex.InnerException + ex.Source;

                WriteResponse(ex, filename, result, MRFailPath);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                WriteResponse(ex, filename, result, MRFailPath);
            }
            return result_b;
        }


        private static bool POST_FILE_Call(string file, bool fetch_file, string MRFailPath, string MRSuccessPath)
        {
            string result = string.Empty;
            string json = string.Empty;
            string result2 = string.Empty;
            bool result_boolean = false;

            try
            {

                string URL = System.Configuration.ConfigurationManager.AppSettings.Get("MemberAPI");
                string user = System.Configuration.ConfigurationManager.AppSettings.Get("MemberAPI_user");
                string key = System.Configuration.ConfigurationManager.AppSettings.Get("MemberAPI_key");


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                request.Headers.Add("user", user);
                request.Headers.Add("key", key);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {

                    //JObject yo = JObject.Parse(file);
                    JArray arr = JArray.Parse(File.ReadAllText(file));
                    json = arr.ToString(Newtonsoft.Json.Formatting.None);


                    //json = File.ReadAllText(file);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ApplicationException("Error Occured");
                    }

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (StreamReader reader = new StreamReader(responseStream))
                            {
                                CreateOutput(Path.GetFileName(file), File.ReadAllText(file), result);
                                SaveFile(File.ReadAllText(file), Path.GetFileName(file), MRSuccessPath);
                                result_boolean = true;
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(data);
                        if (fetch_file)
                        {
                            result2 = "Error Response: " + ex.Message + "\nError Message:" + sr.ReadToEnd() + "\nConverted Json:\n" + json + "\n--------------------------------------------------------------------------------------------------------------------------------------\n";
                        }
                        else if (!fetch_file)
                        {
                            result2 = "Error Response: " + ex.Message + "\nError Message:" + sr.ReadToEnd();
                        }

                        WriteResponse(ex, file, result2, MRFailPath);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteResponse(ex, file, result2, MRFailPath);
            }
            return result_boolean;
        }

        private static void SaveFile(string data, string filename, string path)
        {
            Console.WriteLine("Saving File");
            try
            {
                using (StreamWriter writer = File.CreateText(path + filename))
                {
                    writer.Write(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Custom.MRInfo("Error saving file " + filename + " to path " + path + " reason \n" + ex.Message + "\n" + ex.InnerException);
            }
        }
        private static void CreateOutput(string filename, string request, string response)
        {
            try
            {
                string delimeter = "|";
                string outputfilepath = System.Configuration.ConfigurationManager.AppSettings.Get("MRResultPath") + "Output.csv";

                if (!File.Exists(outputfilepath))
                {
                    using (StreamWriter writer = File.AppendText(outputfilepath))
                    {
                        writer.Write("DateTime" + delimeter + "Filename" + delimeter + "Request" + delimeter + "Response\n");
                    }
                }
                using (StreamWriter writer = File.AppendText(outputfilepath))
                {
                    writer.Write(DateTime.Now.ToString("yyyyMMddHHmmss") + delimeter + filename + (request.Replace('\n', ' ')) + delimeter + (response.Replace('\n', ' ')) + "\n");
                }
            }
            catch (Exception ex)
            {
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
            }
        }

        private static void WriteResponse(Exception ex, string filename, string result, string MRFailPath)
        {
            try
            {
                Custom.MRInfo("Upload fail for file " + filename);
                Custom.MRInfo(ex.Message + ex.InnerException + ex.Source);
                Custom.MRInfo(result);
                CreateOutput(Path.GetFileName(filename), File.ReadAllText(filename), result);
                SaveFile(File.ReadAllText(filename), Path.GetFileName(filename), MRFailPath);
            }
            catch (Exception ex1)
            {
                Custom.MRInfo(ex1.Message);
            }
        }
    }
}
