using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security;

namespace MassPeStudioFileScan
{
    class Program
    {
        public static List<string> Files = new List<string>();
        public static List<string> cmds = new List<string>();
        public static string configFile = "Config.conf";
        public static string PEstudioLoc = "";
        public static string FileCopyLoc = "";
        public static string outputLocation = "";//must be dir to put file to look at in
        private static string Pass;
        private static string Domain;
        private static string Username;
        public static string  CleanUpXMLs = "";
        public static string FileTypesToMove = "*.com *.vbs *.cs *.exe *.wav *.bin *.bny *.php *.ws *.wsf *.run *.rgs *.msi *.job *.hta *.jar *.wsc *.ps2 *.psc1 *.psc2 *.pdf *.inf *.dll *.psm *.cmd *.bat *.ps1 *.ps *.js *.jse *.vbe *.vb *.zip *.swf *.java *.cv1 *.doc *.docx *.dot *.docm *.xls *.xlsx *.xlt *.xla *.xll *xlsm *.ppt *.pptx *.tmp *.htm *.html *.xhtml *.msg *.dat *.sys";                 
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ReadConfig();
                DisplayAdminMenu();
                DisplayMainMenu();
            }
            else
           {
                ReadConfig();
                ParseExeCmdArgs(args);
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\n--ALL DONE SCANNING--");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void ParseExeCmdArgs(string[] argz)
        {
            try
            {
                for (int x = 0; x < argz.Length - 1; ++x)
                {
                    switch (argz[x].ToLower())
                    {
                        case "-t":
                            FileCopyLoc = argz[x + 1];
                            break;
                        case "-u":
                            Username = argz[x + 1];
                            break;
                       case "-h":
                            DisaplyHelp();
                            break;
                        case "-p":
                            Pass = argz[x + 1];
                            MainMenuOption2();//Scan Dir or File local
                            break;
                        case "-c":
                            CleanUpXMLs = "-c";
                            break;
                        case "-d":
                            Domain = argz[x + 1];
                            break;
                        case "-lt":
                            if (CMDerrorCheck(1))
                            {
                                MainMenuOption1(argz[x + 1]);//Scan Dir or File local
                            }
                            else
                            {
                                Error("-ERROR-  CMD Args suck and might be out of order error. Type -h for help.");
                            }
                            break;
                        case "-rt":
                            if (CMDerrorCheck(1) && CMDerrorCheck(2))
                            {
                                MainMenuOption2(argz[x + 1]);//scan remote
                            }
                            else
                            {
                                Error("-ERROR-  CMD Args suck and might be out of order error. Type -h for help.");
                            }
                            break;
                    }
                }
            }
            catch
            {
                DisaplyHelp();
                Environment.Exit(0);
            }
        }

        static bool CMDerrorCheck(int Check)
        {//true is pass / false is fail
            switch (Check)
            {
                case 1://check config values
                    {
                        if (string.IsNullOrEmpty(FileCopyLoc) && string.IsNullOrEmpty(PEstudioLoc) && string.IsNullOrEmpty(PEstudioLoc))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case 2://check creds
                    {
                        if (string.IsNullOrEmpty(Pass) && string.IsNullOrEmpty(Domain) && string.IsNullOrEmpty(Username))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
            }
            return false;//none of the checks worked?!?
        }

        static void DisplayMainMenu()
        {
            Console.WriteLine("\nPlease select the number from menu below to decide what to do:");
            Console.WriteLine("\n-1) Read Config file again:");
            Console.WriteLine("\n0) Display Admin info:");
            Console.WriteLine("\n1) Input file path for single file to scan or Dir path to scan (Recusive all sub Dirs)(Local Machine):");
            Console.WriteLine("\n2) Input file path for single file to scan or Dir path to scan (Recusive all sub Dirs)(Remote Machine):");
            Console.WriteLine("\n99) EXIT App");

            int opt = 0;
            try
            {
                opt = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Error("ERROR - Your inputs sucks error. Hit any key to exit or to try again.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            switch (opt)
            {
                case -1:
                    DisplayAdminMenu();
                    break;
                case 0:
                    DisplayAdminMenu();
                    break;
                case 1:
                    MainMenuOption1();//Scan Dir or File local
                    break;
                case 2:
                    MainMenuOption2();//Scan Dir or File remote
                    break;
                case 99:
                    Environment.Exit(0);
                    break;
            }
            DisplayMainMenu();
        }

        static void DisplayAdminMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nWelcome to the Mass Scanning PeStudio File Scanner App\n");
            Console.WriteLine("-----------------Admin Notes--------------------------");
            Console.WriteLine("  Expected Config File Loc: " +Directory.GetCurrentDirectory().ToString()+"\\"+ configFile + "\n");
            Console.WriteLine("  Output Loc: "+ outputLocation+"\n");
            Console.WriteLine("  PESTUDIO Loc Loc: "+ PEstudioLoc+"\n");
            Console.WriteLine("  Copy Files here Loc: "+ FileCopyLoc+"\n");
            Console.WriteLine("-------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void DisaplyHelp()
        {
            Console.WriteLine(@"
            Mass Pestudio File Scan Help Menu:
           
            Commands:            

            Scripted Commands:
            -t CMD line for the Location to copy a file from.
                -u Username (withour Domain)
                -p Password for account
                -d Domain Name (or use '.' for local account on remote machine)

            Interactive Calls for Commands:
            -lt Interactivley copy and scan file from Local Target
            -rt Interactivley copy and scan file from Remote Target
            
            Usage:
            MassPeStudioFileScan.exe -t C:\{File Path you have access to} (or) \\{filepath you have access to} -u {Username} -p {Password} -d {Domain Name}
            MassPeStudioFileScan.exe -lt C:\{File Path}
            MassPeStudioFileScan.exe -rt \\{File Path you have access to}
            MassPeStudioFileScan.exe
            ");
        }

        static void GetCreds()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" NOTE: Its recommend this account be Admin on the remote machine or domain\n");
            Console.ForegroundColor=ConsoleColor.White;
            Console.WriteLine("Please Enter Your Account Name (No Domain):");
            Username = Console.ReadLine();
            Console.WriteLine("\nPlease Enter above accounts Domain (if local type '.'):");
            Domain = Console.ReadLine();
            Console.WriteLine("\nPlease Enter above accounts Password:");
            Pass = Console.ReadLine();
            Console.Clear();
        }

        static void ReadConfig()
        {
                    string line;
                    if (!File.Exists(configFile))
                    {
                        File.Create(configFile).Close();
                        File.WriteAllText(configFile, "outputxmldir=\nfiletypestomove=\npestudioloc=\nfilecopyloc=\n");
                    }
                    using (var reader = new StreamReader(configFile, Encoding.UTF8))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            line.TrimStart();
                            line.TrimEnd();
                            line.ToLower();
                            try
                            {
                                cmds = line.ToLower().Split('=').ToList();
                                switch (cmds.ElementAt(0))
                                {
                                    case ("outputxmldir"):
                                        {
                                            outputLocation = cmds.ElementAt(1);
                                            break;
                                        }
                                    case ("filetypestomove"):
                                        {
                                            FileTypesToMove = cmds.ElementAt(1);
                                            break;
                                        }
                                    case ("pestudioloc"):
                                        {
                                            PEstudioLoc = cmds.ElementAt(1);
                                            break;
                                        }
                                    case ("filecopyloc"):
                                        {
                                            FileCopyLoc = cmds.ElementAt(1);
                                            break;
                                        }
                                }
                            }
                            catch
                            {
                                Error("ERROR - Your config Sucks. You output location for the pestudio files is bad.'var=value' format plz");
                            }
                        }
                        reader.Close();
                    }
                        
        }

        static void MainMenuOption1(string FromPath="")
        {
            string Frompath;
            Console.WriteLine("Please input file path (Local machine only supported):");
            if (FromPath == "")
            {
                Frompath = Console.ReadLine();
            }
            else
            {
                Frompath = FromPath;
            }
            if (String.IsNullOrEmpty(FileTypesToMove))
            {
                Console.WriteLine("I noticed you have no specfic file types to move. Do you want to use default(y/n)?");
                string ans=Console.ReadLine().ToLower();
                if (ans.ToLower() == "n")
                {
                    Console.WriteLine("Input Your file Types (ie *.exe *.dll .{space}.{space}.):");
                    FileTypesToMove=Console.ReadLine();
                }
            }
            Console.Clear();
            RunCMD("robocopy " + Frompath + " "+ FileCopyLoc + " " + FileTypesToMove + " /XF pagefile.sys hiberfil.sys /XJ /max:26214400 /R:0 /xjd /s");
            ProcessDirectory(FileCopyLoc, Frompath);
        }

        static void MainMenuOption2(string FromPath = "")
        {
            string Frompath;
            Console.WriteLine("Please input file path (UNC Only):");
            if (FromPath == "")
            {
                Frompath = Console.ReadLine();
                GetCreds();
            }
            else
            {
                Frompath = FromPath;
            }
            if (String.IsNullOrEmpty(FileTypesToMove))
            {
                Console.WriteLine("I noticed you have no specfic file types to move. Do you want to use default(y/n)?");
                string ans = Console.ReadLine().ToLower();
                if (ans.ToLower() == "n")
                {
                    Console.WriteLine("Input Your file Types (ie *.exe *.dll .{space}.{space}.):");
                    FileTypesToMove = Console.ReadLine();
                }
            }
            Console.Clear();
            try
            {
                if (File.Exists(FromPath) || Directory.Exists(FromPath))
                {
                    RunCMDasDiffrentUser("robocopy " + Frompath + " " + FileCopyLoc + " " + FileTypesToMove + " /XF pagefile.sys hiberfil.sys /XJ /max:26214400 /R:0 /xjd /s");
                    ProcessDirectory(FileCopyLoc, Frompath);
                }
                else
                {
                    Error("-ERROR-  CMD Args suck and cant find what u wanted error. Type -h for help.");
                }
            }
            catch
            {
                Error("-ERROR-  CMD Args suck and cant find what u wanted error. Type -h for help.");
            }
        }

        static void ProcessDirectory(string targetDirectory, string OrginalPath)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                List<string> temp = fileName.Split('\\').ToList();
                try
                {
                ProcessFile(temp.ElementAt(temp.Count - 1), OrginalPath, targetDirectory);
                }
                catch (Exception e)
                {
                    Error(" -Error- Failed to process " + fileName +"    \nReason: "+ e.Message.ToString());
                }
                File.Delete(fileName);
            }
            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                try
                {
                    ProcessDirectory(subdirectory, OrginalPath);
                }
                catch (Exception e)
                {
                    Error(" -Error- Failed to process " + subdirectory + "    \nReason: " + e.Message.ToString());
                }
            }
        }

        static void ProcessFile(string file, string OrginalPath, string targetDirectory)
        {
            Console.WriteLine(" -Processing "+OrginalPath+"\\"+ file);
            RunPeStudio(file, OrginalPath, targetDirectory);
        }

        static void RunPeStudio(string File, string Path, string targetDirectory)
        {
            bool started = false;
            var process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + PEstudioLoc + "\\pestudiox.exe -file:" + targetDirectory + "\\" + File + " -xml:" +  outputLocation+ "\\" + File + ".xml";
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = PEstudioLoc;
            process.StartInfo = startInfo;
            started = process.Start();
            var procId = process.Id;
            Console.WriteLine("\nRunning PESTUDIO PID: " + procId + "\n   cmd.exe - ARGS: " + startInfo.Arguments + " && cmd /c ReadPeStudioXML.exe " +""+ CleanUpXMLs);
            DateTime time = DateTime.Now;
            while (Process.GetProcesses().Any(x => x.Id == Convert.ToInt32(procId)) != false)
            {
                if (time.Minute - DateTime.Now.Minute > 3)
                {
                    Error("\nWARNING - So the process called to do pestudiox.exe is slow please kill it if this is an issue\n");
                    break;
                }
            }
        }
        
        static void RunCMDasDiffrentUser(string Cmd)
        {
            bool started = false;
            var process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Cmd;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = FileCopyLoc;
            var securePass = new SecureString();
            foreach (char c in Pass)
            {
                securePass.AppendChar(c);
            }
            startInfo.Password=securePass;
            startInfo.UserName=Username;
            startInfo.Domain = Domain;
            Pass = "";
            GC.Collect();
            GC.WaitForPendingFinalizers();
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            started = process.Start();
            var procId = process.Id;
            Console.WriteLine("Running CMD PID: " + procId + "\n   cmd.exe - ARGS: " + startInfo.Arguments);
            DateTime time = DateTime.Now;
            while (Process.GetProcesses().Any(x => x.Id == Convert.ToInt32(procId)) != false)
            {
                if (time.Minute - DateTime.Now.Minute > 3)
                {
                    Error("\nWARNING - So the process called to do " + Cmd + " is slow please kill it if this is an issue\n");
                    break;
                }
            }
        }

        static void RunCMD(string Cmd)
        {
            bool started = false;
            var process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C "+Cmd;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = FileCopyLoc;
            process.StartInfo = startInfo;
            started = process.Start();
            var procId = process.Id;
            Console.WriteLine("Running CMD PID: " + procId + "\n   cmd.exe - ARGS: " + startInfo.Arguments);
            DateTime time = DateTime.Now;
            while (Process.GetProcesses().Any(x => x.Id == Convert.ToInt32(procId)) != false)
            {
                if (time.Minute - DateTime.Now.Minute > 3)
                {
                    Error("\nWARNING - So the process called to do " + Cmd + " is slow please kill it if this is an issue\n");
                    break;
                }
            }
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        //TODO
        /*public static void RunPowerShell(string Cmd)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Powershell.exe";
            startInfo.Arguments = "" + Cmd;
            process.StartInfo = startInfo;
            process.Start();
        }*/
    }
}
