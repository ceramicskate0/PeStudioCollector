using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace ReadPeStudioXML
{
    class Program
    {
        public static string InputFile = "";
        public static XmlTextReader reader = new XmlTextReader(InputFile);
        public static List<FileIOC> MachineFileList = new List<FileIOC>();
        public static int RecordCounter = 0;
        public static int ModVari = 10000;
        public static int FileCount=1;
        public static bool FirstRun = true;
        public static bool CleanUpXMLs = false;
        public static string MachineName = "";
        public static string OutputFile = MachineName + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_Scan_Summary.csv";

        static void Main(string[] args)
        {
            ParseArgs(args);
            CountLineInFile(OutputFile);
            ReadandParseXML();
            WriteCSV();
        }

        static int CountLineInFile(string outfile)
        {
            if (File.Exists(outfile))
        {
            RecordCounter = File.ReadLines(outfile).Count();
            return RecordCounter = File.ReadLines(outfile).Count();
        }
        else
        {
            File.Create(outfile).Close();
            RecordCounter = File.ReadLines(outfile).Count();
            return RecordCounter = File.ReadLines(outfile).Count();
        }
        }

        static void ParseArgs(string[] argz)
        {
            try
            {
                if (argz.Length > 0)
                {
                    for (int x = 0; x < argz.Length - 1; ++x)
                    {
                        switch (argz[x].ToLower())
                        {
                            case "-c":
                                CleanUpXMLs = true;
                                break;
                            case "-m":
                                MachineName = argz[x + 1] + "_";
                                break;
                            case "-f":
                                if (File.Exists(argz[x + 1]) && argz[x + 1].ElementAt(argz[x + 1].Length).ToString().ToLower() == "l" && argz[x + 1].ElementAt(argz[x + 1].Length - 1).ToString().ToLower() == "m" && argz[x + 1].ElementAt(argz[x + 1].Length - 2).ToString().ToLower() == "x")
                                {
                                    InputFile = argz[x + 1];
                                }
                                else
                                {
                                    Error("ERROR invalid file found");
                                    Environment.Exit(1);
                                }
                                break;
                            case "-i":
                                Console.WriteLine("Enter PeStudio XML file to read:");
                                InputFile = Console.ReadLine();
                                if (File.Exists(InputFile) && InputFile.ElementAt(InputFile.Length).ToString().ToLower() == "l" && InputFile.ElementAt(InputFile.Length - 1).ToString().ToLower() == "m" && InputFile.ElementAt(InputFile.Length - 2).ToString().ToLower() == "x")
                                {

                                }
                                else
                                {
                                    Error("ERROR invalid file found. Enter to exit.");
                                    Console.ReadKey();
                                    Environment.Exit(1);
                                }
                                break;
                            case "-h":
                                DisplayHelp();
                                break;
                            case "-o":
                                OutputFile = argz[x + 1];
                                break;
                        }
                    }
                }
                else
                {
                    DisplayHelp();
                    Environment.Exit(0);
                }
            }
            catch
            {
                DisplayHelp();
                Environment.Exit(0);
            }
        }

        static void ReadandParseXML()
        {
            FileIOC Fileioc = new FileIOC();
            string CurrentElement="";
            int sev = 0;
            string ioc="";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        CurrentElement = reader.Name;
                        if (string.IsNullOrEmpty(Fileioc.Filename))
                            Fileioc.Filename = reader.GetAttribute("name");
                        if (string.IsNullOrEmpty(Fileioc.Count))
                        Fileioc.Count = reader.GetAttribute("count");
                        sev = Convert.ToInt32(reader.GetAttribute("severity"));
                        break;
                    case XmlNodeType.Text: //Display the text in each element
                        if (CurrentElement == "indicator")
                        {
                            ioc=reader.Value;
                        }
                        if (string.IsNullOrEmpty(Fileioc.SHA1) && CurrentElement.ToLower()=="sha1")
                            Fileioc.SHA1 = reader.Value;;
                        if (string.IsNullOrEmpty(Fileioc.MD5) && CurrentElement.ToLower() == "md5")
                            Fileioc.MD5 = reader.Value;;
                        if (string.IsNullOrEmpty(Fileioc.Type) && CurrentElement.ToLower() == "type")
                            Fileioc.Type = reader.Value;;
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element
                        if (reader.Name == "indicators")
                        {
                            Console.Write(reader.Name);
                        }
                        if (sev > 0 && string.IsNullOrEmpty(ioc)==false)
                        {
                        Fileioc.AddIOC(sev, ioc);
                        sev = 0;
                        ioc = "";
                        }
                        break;
                }
            }
            MachineFileList.Add(Fileioc);
            if (CleanUpXMLs)
            {
                File.Delete(InputFile);
            }
        }

        static void WriteCSV()
        {
            if (RecordCounter % ModVari >= 0)
            {
                Error("\nWARNING Your file ("+OutputFile+")may exceed Excels max limit for performance. Ill break it up for you to be sure you dont crash Excel.");
                WriteFile(0, OutputFile);
            }
            else
            {
                WriteFile(0, OutputFile);
            }
        }

        static void WriteFile(int CurrentCount,string outputFile)
        {
            string FileN = "";
            string PathN="";
            double t = RecordCounter % ModVari;
            if ((File.Exists(outputFile) && CountLineInFile(outputFile) > ModVari && RecordCounter != 0))//make new file this one is full
            {
                FirstRun = false;
                ++FileCount;
                FileN = Path.GetFileNameWithoutExtension(outputFile);
                PathN = Path.GetFullPath(outputFile).Replace("\\" + FileN + ".csv", "");
                OutputFile = PathN + "\\" + FileN + FileCount.ToString() + ".csv";
                while (RecordCounter>ModVari)
                {
                    if (File.Exists(OutputFile))
                    { 
                    FileN = Path.GetFileNameWithoutExtension(outputFile);
                    PathN = Path.GetFullPath(outputFile).Replace("\\" + FileN + ".csv", "");
                    OutputFile = PathN + "\\" + FileN + FileCount.ToString() + ".csv";
                    }
                    CountLineInFile(OutputFile);
                    ++FileCount;
                }
                WriteFile(RecordCounter = CountLineInFile(OutputFile), OutputFile);
            }
            else
            {
                using (StreamWriter Writer = File.AppendText(outputFile))
                {
                    if ( FirstRun ==true)
                    {
                        RecordCounter = 0;
                    }
                    for (int x = 0; x < MachineFileList.Count; ++x)
                    {
                        if (RecordCounter % ModVari != 0 || RecordCounter == 0)
                        {
                            Writer.WriteLine(MachineFileList.ElementAt(x).Filename + "," + MachineFileList.ElementAt(x).Type + "," + MachineFileList.ElementAt(x).VTresults + "," + MachineFileList.ElementAt(x).Count + "," + MachineFileList.ElementAt(x).MD5 + "," + MachineFileList.ElementAt(x).SHA1 + "," + MachineFileList.ElementAt(x).TotalSeverity.ToString() + "," + MachineFileList.ElementAt(x).IOC.ElementAt(0).text + "," + MachineFileList.ElementAt(x).IOC.ElementAt(0).num.ToString());
                            for (int y = 1; y < MachineFileList.ElementAt(x).IOC.Count; ++y)
                            {
                                Writer.WriteLine("," + "," + "," + "," + "," + "," + "," + MachineFileList.ElementAt(x).IOC.ElementAt(y).text + "," + MachineFileList.ElementAt(x).IOC.ElementAt(y).num.ToString());
                            }
                            FirstRun = false;
                        }
                        else
                        {
                            FirstRun = false;
                            FileN = Path.GetFileNameWithoutExtension(outputFile);
                            PathN = Path.GetFullPath(outputFile).Replace("\\" + FileN + ".csv", "");
                            File.Create(FileN + FileCount.ToString() + ".csv");
                            OutputFile = PathN + "\\" + FileN + FileCount.ToString() + ".csv";
                            ++FileCount;
                            WriteFile(RecordCounter = CountLineInFile(outputFile), OutputFile);
                            x = MachineFileList.Count + 1;
                        }
                        ++RecordCounter;
                    }
                }
            }
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void DisplayHelp()
        {
            Console.WriteLine(@"
            Help menu for Pestudio XML parser:
            This app currently only looks at the top part of the xml for indicatgors. To change this you will need to add them to the pestudio configs.

            Args:
            -f File to be parsed for csv output
            -i Go interactive
            -h Show Help Menu
            -o Tell app to use non default output file and path other than PWD
            -m Machine Name for use in file output
            -c Clean up XML files once parsed

            Notes:
            Only Supports csv output at this time.
            Output file drops in current working dir for app.
            
            Usage:
            CMD line usage: ReadPeStudioXML -f {File path}.xml   
            Interactive Usage: ReadPeStudioXML -i 
            App generates 1 output file per scan and will overwrite.
            ");
        }
    }
}
