using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axtension;

namespace BOSSI
{
    class Program
    {
        static void ifdie(bool test, string msg)
        {
            if (test)
            {
                Console.WriteLine(msg);
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            string sEXESpec = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string sEXEName = Path.GetFileName(sEXESpec); 
            string sCFGSpec = Path.ChangeExtension(sEXEName, "cfg");

            Axtension.Config cfg = new Config(sCFGSpec);

            string awaiting = cfg.Retrieve("awaiting", string.Empty);
            ifdie(string.Empty == awaiting, "awaiting not defined in " + sCFGSpec);

            string processed = cfg.Retrieve("processed", string.Empty);
            ifdie(string.Empty == processed, "processed not defined in " + sCFGSpec);

            string connection = cfg.Retrieve("connection", string.Empty);
            ifdie(string.Empty == connection, "connection not defined in " + sCFGSpec);

            string account = cfg.Retrieve("account", string.Empty);
            ifdie(account == string.Empty, "account not defined in " + sCFGSpec);

            string password = cfg.Retrieve("password", string.Empty);
            ifdie(password == string.Empty, "password not defined in " + sCFGSpec);

            string postoffice = cfg.Retrieve("postoffice", string.Empty);
            ifdie(postoffice == string.Empty, "postoffice not defined in " + sCFGSpec);

            string port = cfg.Retrieve("port");
            ifdie(port == null, "port not defined in " + sCFGSpec);

            string ssl = cfg.Retrieve("ssl");
            ifdie(ssl == null, "ssl not defined in " + sCFGSpec);

            string recipient = cfg.Retrieve("recipient", "");
            ifdie(string.Empty == recipient, "recipient not defined in " + sCFGSpec);

            Axtension.Mail oMail = new Axtension.Mail();
            oMail.Account = account;
            oMail.Password = password;
            oMail.Postoffice = postoffice;
            oMail.Port = int.Parse(port);
            oMail.SSL = ssl.ToUpper() == "Y";

            List<string> lLog = new List<string>();
            Axtension.SQL sql = new Axtension.SQL();
            sql.Connect(connection);
            try
            {
                var sqlFiles = Directory.EnumerateFiles(awaiting, "*.sql", SearchOption.TopDirectoryOnly);

                foreach (string sqlFile in sqlFiles)
                {
                    string sqlCode = File.ReadAllText(sqlFile);
                    sql.Exec(sqlCode);
                    File.Move(sqlFile, Path.Combine(processed, Path.GetFileName(sqlFile)));
                    lLog.Add(sqlCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            oMail.Shriek(account, recipient, "[BOSSI] " + DateTime.Now.ToString(), string.Join("\n", lLog.ToArray()));
        }
    }
}

