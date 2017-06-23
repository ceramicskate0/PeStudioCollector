using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.IO;

namespace OutputAnalyzer
{
    class Program
    {

        public static string OutputFilePath = "";
        public static string InputDirPath = "";
        public static string EmailServer = "";
        public static string Emailbody = "";
        public static string Emailto = "";
        public static string Emailfrom = "";
        public static string Emailsubj = "";
        public static string OutputFile = "_Scan_Summary.csv";
        public static string VTOutputFile = "_VirusTotal_Summary.csv";
        public static bool Email = false;
        public static bool GetVTOnly = false;
        public static string ConfigFile = "Config.conf";
        public static string SearchTermsFile = "SearchTerms.conf";
        public static int VTthreshold = 0;
        public static List<string> FilesInInputDirPath = new List<string>();
        public static List<FileIOC> MachineFileList = new List<FileIOC>();
        public static List<string> SearchTerms= new List<string> ();

        static void Main(string[] args)
        {
            //get initial inst
            GetAndParseArgs(args);
            
            //Get list of all the CSV files
            if (GetVTOnly)
            {
                FindAllCSVFiles(InputDirPath, VTOutputFile);
                StartToAnalyzeFiles();
            }
            else
            {
                FindAllCSVFiles(InputDirPath, OutputFile);
                StartToAnalyzeFiles();
            }
        }

        private static void StartToAnalyzeFiles()
        {
           //Analyze all the files
            for (int x=0;x<FilesInInputDirPath.Count;++x)
            {
                ReadOutputFile(FilesInInputDirPath.ElementAt(x));
                if (MachineFileList.Count>10000)
                {
                    ANALYSIS(MachineFileList);
                    MachineFileList.Clear();
                }
                else
                {
                    //TODO:keep storing in memory
                }
            }
            ANALYSIS(MachineFileList);
        }

        private static void ANALYSIS(List<FileIOC> IOC)
        {
            try
            {
            for (int x=0;x<IOC.Count;++x)
            {
              if (VTthreshold>=Convert.ToInt32(IOC.ElementAt(x).VTresults))
              {

              }
            }
            }
            catch
            {
                Error("--ERROR-- ANALYSIS Method Crit Error Failed.");
                Environment.Exit(1);
            }
        }

        private static void GetAndParseArgs(string[] argz)
        {
            if (argz.Length > 0)
            {
                try
                {
                    for (int x = 0; x < argz.Length; ++x)
                    {
                        switch (argz[x].ToLower())
                        {
                            case "-o":
                                OutputFilePath = argz[x + 1];
                                break;
                            case "-d":
                                InputDirPath = argz[x + 1];
                                break;
                            case "-e":
                                Email = true;
                                break;
                            case "-vt":
                                GetVTOnly = true;
                                break;
                            case "-st":
                                SearchTermsFile = argz[x + 1];
                                break;
                        }
                    }
                }
                catch
                {
                    Error("--ERROR-- Parse Args Failed.");
                    Environment.Exit(1);
                }
            }
            else
            {
                ReadConfig(ConfigFile);
                ReadSearchTermFile(SearchTermsFile);
            }
        }

        private static void ReadSearchTermFile(string file)
        {
             string line;
            if (!File.Exists(file))
            {
                Error("--Error-- File not found!!!");
                Environment.Exit(1);
            }
            else
            {
                using (var reader = new StreamReader(file, Encoding.UTF8))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        SearchTerms.Add(line);
                    }
                    reader.Close();
                }
            }
        }

        private static void ReadConfig(string file)
        {
            string line;
            if (!File.Exists(file))
            {
                Error("--Error-- File not found!!!");
                Environment.Exit(1);
            }
            else
            {
                using (var reader = new StreamReader(file, Encoding.UTF8))
                {
                    List<string> arg = new List<string>();
                    while ((line = reader.ReadLine()) != null)
                    {
                        arg = line.Split('=').ToList();
                        string cmd = arg.ElementAt(0);
                        string val = arg.ElementAt(1);
                        switch (cmd.ToLower())
                        {
                            case "mailserver":
                                EmailServer=val;
                                break;
                            case "mailto":
                                Emailto=val;
                                break;
                            case "mailsubject":
                                Emailsubj=val;
                                break;
                            case "mailfrom":
                                Emailfrom=val;
                                break;
                            case "outputdir":
                                OutputFilePath=val;
                                break;
                            case "inputdir":
                                InputDirPath = val;
                                break;
                            case "vt":
                                VTthreshold = Convert.ToInt32(val);
                                break;
                        }
                    }
                    reader.Close();
                }
            }
        }

        private static void ReadOutputFile(string file)
        {
            string line;
            if (!File.Exists(file))
            {
                Error("--Error-- File not found!!!");
            }
            else
            {
                using (var reader = new StreamReader(file, Encoding.UTF8))
                {
                    List<string> CSVline = new List<string>();
                    FileIOC Fileioc = new FileIOC();

                    line = reader.ReadLine();
                    CSVline = line.Split(',').ToList();
                    //TODO:its a new file analysis and new Fileioc
                    while ((line = reader.ReadLine()) != null)
                    {                     
                        //TODO:does it have only 1 arg after # of commas and add IOC to current File IOC
                        //TODO:else its a new file analysis and new Fileioc
                        CSVline = line.Split(',').ToList();
                        MachineFileList.Add(Fileioc);
                    }
                    reader.Close();
                }
            }
        }

        private static void SendEmail(string to, string from, string subject, string emailServer)
        {

            try
            {
                SmtpClient client = new SmtpClient(emailServer);
                MailAddress fromAddr = new MailAddress(from);
                MailMessage message = new MailMessage();
                message.To.Add(to);
                message.From = fromAddr;
                message.Body += Emailbody;
                message.Body += Environment.NewLine;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject += subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                try
                {
                    client.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Email SendEmail() Error: " + e.Message.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Email SendEmail() Error: " + e.Message.ToString());
            }
        }

        private static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void FindAllCSVFiles(string DirPath, string FindStr)
        {
            FilesInInputDirPath = Directory.GetFiles(InputDirPath, FindStr).ToList();
        }

        private static void HelpMenu()
        {
            Console.WriteLine(@"
Help menu for Pestudio XML OutPutAnalyzer:
            This app will read in a file containing 1 search string per line to find in the output files from the MassPeStudioFileScan App

            Args:
            -o Output File name or Path
            -d Dir to be parsed for Pestudio csv output files output only properly named csv files will be scanned in this dir.    
            -e Send email when done (Email options in config)
            -st File path to search terms to find in output files
            -vt Search Virus Total output files

            Usage:
            CMD line usage: OutputAnalyzer -d {dir path}\*_Scan_Summary.csv or {dir path}\*_VirusTotal_Summary.csv
            Note:
            If no Args i will try and read from CWD a file called Config.conf for my instructions
            ");
        }
    }
}
