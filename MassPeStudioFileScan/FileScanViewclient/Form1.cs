using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileScanViewclient
{
    public partial class Form1 : Form
    {
        public List<FileIOC> Files = new List<FileIOC>();
        public List<string> ReadInFiles = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        public void Form1_Load(object sender, EventArgs e)
        {

        }

        public void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Files.Clear();
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "PEstudio Scan Summary CSV output read in";
            fdlg.InitialDirectory = Directory.GetCurrentDirectory();
            fdlg.Filter = "All files (*_Scan_Summary.csv)|*_Scan_Summary.csv";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                ReadInFiles = fdlg.FileNames.ToList();
                foreach (string Filez in ReadInFiles)
                {
                    ReadSummaryCSVfile(Filez);
                }
            }
        }

        public void ReadSummaryCSVfile(string FilePath)
        {
            using (var fs = File.OpenRead(FilePath))
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    FileIOC Fileioc = new FileIOC();
                    var line = reader.ReadLine();
                    var values = line.Split(',').ToArray();
                    if (line.ElementAt(0) == ',' && line.ElementAt(1) == ',' && line.ElementAt(2) == ',')
                    {
                        Fileioc.AddIOC(Convert.ToInt32(line.ElementAt(line.Count() - 1)), line.ElementAt(line.Count()).ToString());
                    }
                    else
                    {
                        Fileioc.Filename = values[0];
                        Fileioc.Type = values[1];
                        Fileioc.VTresults = values[2];
                        Fileioc.TotalSeverity = Convert.ToInt32(values[3]);
                        Fileioc.MD5 = values[4];
                        Fileioc.SHA1 = values[5];
                        Fileioc.AddIOC(Convert.ToInt32(values[6]),values[7].ToString());
                    }
                    Files.Add(Fileioc);
                }
            }
        }
         
        public void ReadVTCSVfile(string FilePath)
        {
            using (var fs = File.OpenRead(FilePath))
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    FileIOC Fileioc = new FileIOC();
                    var line = reader.ReadLine();
                    var values = line.Split(',').ToArray();
                    Fileioc.Filename = values[0];
                    Fileioc.VTresults = values[1];
                    Files.Add(Fileioc);
                }
            }
        }

        private void openVTSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Files.Clear();
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "PEstudio Virus Total CSV output read in";
            fdlg.InitialDirectory = Directory.GetCurrentDirectory();
            fdlg.Filter = "All files (*_VirusTotal_Summary.csv)|*_VirusTotal_Summary.csv";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                ReadInFiles = fdlg.FileNames.ToList();
                foreach (string Filez in ReadInFiles)
                {
                ReadVTCSVfile(Filez);
                }
            }
        }
    }
}
