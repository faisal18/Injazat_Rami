using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Injazat_Rami
{
    class Program
    {
        static void Main(string[] args)
        {

            Claims CS = new Claims();
            MemberRegister MR = new MemberRegister();
            try
            {
                string to_Start = System.Configuration.ConfigurationManager.AppSettings.Get("to_start");

                if (to_Start.ToLower() == "claim")
                {
                    CS.Execute();
                }
                else if (to_Start.ToLower() == "member")
                {
                    MR.Execute();
                }
                else if(to_Start.ToLower() == "both")
                {
                    Thread Claim = new Thread(() => { CS.Execute(); });
                    Claim.Start();

                    Thread Member = new Thread(() => { MR.Execute(); });
                    Member.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.Read();
            }
            
        }
    }
}
