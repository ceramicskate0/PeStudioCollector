﻿using System;
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
        public static string InputDir = "";
        public static List<FileIOC> MachineFileList = new List<FileIOC>();
        public static int RecordCounter = 0;
        public static int ModVari = 10000;
        public static int FileCount=1;
        public static bool FirstRun = true;
        public static bool CleanUpXMLs = true;
        public static bool JustPrintVTResults = false;
        public static string MachineName = "";
        public static string OutputFile = MachineName + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_Scan_Summary.csv";
        public static string VTOutputFile = MachineName + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_VirusTotal_Summary.csv";
        public static int MachineFileListCount = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Pestudio XML reading App");
            ParseArgs(args);
            try
            {
                if (Directory.Exists(InputDir))
                {
                    string[] fileEntries = Directory.GetFiles(InputDir).Distinct().ToArray();
                    foreach (string fileName in fileEntries.Distinct())
                    {
                        InputFile = fileName;
                        if (File.Exists(fileName) && Path.GetExtension(fileName) == ".xml")
                        {
                        ReadandParseXML();
                        if (JustPrintVTResults==false)
                        {
                            WriteOutputToCSV();
                        }
                        }
                    }
                }
                else if (File.Exists(InputFile) && Path.GetExtension(InputFile) == ".xml")
                {
                    ReadandParseXML();
                    if (JustPrintVTResults == false)
                    {
                        WriteOutputToCSV();
                    }
                }
                else
                {
                    Error(" -Error- Your input sucks error. File Does not exist or is not xml.\n"+InputFile);
                }
                WriteVirusTotalFile();
            }
            catch (Exception e)
            {
                Error(" -Error- Main error. "+e.Message.ToString());
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
                                CleanUpXMLs = false;
                                break;
                            case "-d":
                                InputDir = argz[x + 1];
                                break;
                            case "-m":
                                MachineName = argz[x + 1] + "_";
                                break;
                            case "-f":
                                if (File.Exists(argz[x + 1]) && Path.GetExtension(InputFile) == ".xml")
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
                                Console.Write(">");
                                InputFile = Console.ReadLine();
                                if (File.Exists(InputFile) && Path.GetExtension(InputFile) == ".xml")
                                {
                                    InputFile = argz[x + 1];
                                }
                                else
                                {
                                    Error("ERROR invalid file found. Enter to exit.");
                                    Console.ReadKey();
                                    Environment.Exit(1);
                                }
                                break;
                            case "-vt":
                                JustPrintVTResults = true;
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
            catch (Exception e)
            {
                Error("ERROR " + e.Message.ToString());
                DisplayHelp();
                Environment.Exit(0);
            }
        }

        static int CountLineInFile(string outfile)
        {
            if (File.Exists(outfile))
        {
            return RecordCounter = File.ReadLines(outfile).Count();
        }
        else
        {
            File.Create(outfile).Close();
            return RecordCounter = File.ReadLines(outfile).Count();
        }
        }

        static void ReadandParseXML()
        {
            XmlTextReader reader = new XmlTextReader(InputFile);
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
                            Fileioc.SHA1 = reader.Value;
                        if (string.IsNullOrEmpty(Fileioc.MD5) && CurrentElement.ToLower() == "md5")
                            Fileioc.MD5 = reader.Value;
                        if (string.IsNullOrEmpty(Fileioc.Type) && CurrentElement.ToLower() == "type")
                            Fileioc.Type = reader.Value;
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element
                        if (sev > 0 && string.IsNullOrEmpty(ioc)==false)
                        {
                        Fileioc.AddIOC(sev, ioc);
                        sev = 0;
                        ioc = "";
                        }
                        break;
                }
            }
            reader.Close();
            MachineFileList.Add(Fileioc);
            if (CleanUpXMLs)
            {
                File.Delete(InputFile);
            }
            Console.WriteLine("\nFile: " + Fileioc.Filename + "\n   VT: " + Fileioc.VTresults + "\n   Sev: " + sev);
        }

        static void WriteOutputToCSV()
        {
            if (File.Exists(OutputFile) == false)
            {
                File.Create(OutputFile).Close();
            }

            if (CountLineInFile(OutputFile) > ModVari)
            {
                Error("\n--WARNING-- Your file (" + OutputFile + ")may exceed Excels max limit for performance. Ill break it up for you to be sure we dont crash Excel.");
                string FileN = "";
                string PathN = "";
                if ((CountLineInFile(OutputFile) > ModVari && RecordCounter != 0))//make new file this one is full
                {
                    ++FileCount;
                    FileN = Path.GetFileNameWithoutExtension(OutputFile);
                    PathN = Path.GetFullPath(OutputFile).Replace("\\" + FileN + ".csv", "");
                    OutputFile = PathN + "\\" + FileN + FileCount.ToString() + ".csv";
                    while (RecordCounter > ModVari)
                    {
                        if (File.Exists(OutputFile))
                        {
                            FileN = Path.GetFileNameWithoutExtension(OutputFile);
                            PathN = Path.GetFullPath(OutputFile).Replace("\\" + FileN + ".csv", "");
                            OutputFile = PathN + "\\" + FileN + FileCount.ToString() + ".csv";
                        }
                        CountLineInFile(OutputFile);
                        ++FileCount;
                    }
                }
            }
           WriteFile();
        }

        static void WriteFile()
        {
            if (FirstRun == true)
            {
                RecordCounter = 0;
            }
            for (int x = MachineFileListCount; x < MachineFileList.Count; ++x)
            {
                File.AppendAllText(OutputFile, MachineFileList.ElementAt(x).Filename + "," + MachineFileList.ElementAt(x).Type + "," + MachineFileList.ElementAt(x).VTresults + "," + MachineFileList.ElementAt(x).Count + "," + MachineFileList.ElementAt(x).MD5 + "," + MachineFileList.ElementAt(x).SHA1 + "," + MachineFileList.ElementAt(x).TotalSeverity.ToString() + ",\"" + MachineFileList.ElementAt(x).IOC.ElementAt(0).text + "\"," + MachineFileList.ElementAt(x).IOC.ElementAt(0).num.ToString() + "\n");
                for (int y = 1; y < MachineFileList.ElementAt(x).IOC.Count; ++y)
                {
                    File.AppendAllText(OutputFile, "," + "," + "," + "," + "," + "," + ",\"" + MachineFileList.ElementAt(x).IOC.ElementAt(y).text + "\"," + MachineFileList.ElementAt(x).IOC.ElementAt(y).num.ToString() + "\n");
                }
                ++RecordCounter;
            }
            ++MachineFileListCount;
            FirstRun = false;
        }
                        
        static void WriteVirusTotalFile()
        {
            try
            {
                if (File.Exists(VTOutputFile) == false)
                {
                        for (int x = 0; x < MachineFileList.Count; ++x)
                        {
                            File.AppendAllText(VTOutputFile, MachineFileList.ElementAt(x).Filename + "," + MachineFileList.ElementAt(x).VTresults + "\n");
                        }
                }
                else
                {
                    File.Delete(VTOutputFile);
                        for (int x = 0; x < MachineFileList.Count; ++x)
                        {
                            File.AppendAllText(VTOutputFile, MachineFileList.ElementAt(x).Filename + "," + MachineFileList.ElementAt(x).VTresults + "\n");
                        }
                }
            }
            catch (Exception e)
            {
                Error(" -ERROR- Writing to VT summary file. " + e.Message.ToString());
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
            -d Dir to be parsed for csv output (only xml files will be parsed)
            -i Run interactive
            -h Show Help Menu
            -o Tell app to use non default output file and path other than PWD
            -m Machine Name for use in file output
            -c Clean up XML files once parsed
            -vt output just VirusTotal results for all files scanned

            Notes:
            Only Supports csv output at this time.
            Output file drops in current working dir for app.
            
            Usage:
            CMD line usage: ReadPeStudioXML -D {File path}.xml   
            Interactive Usage: ReadPeStudioXML -i 
            App generates 1 output file per scan and will overwrite.
            ");
        }
    }
}
