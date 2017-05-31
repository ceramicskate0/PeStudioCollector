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
        private List<IOC> Files = new List<IOC>();
        private List<string> ReadInFiles = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "PEstudio CSV output read in";
            fdlg.InitialDirectory = Directory.GetCurrentDirectory();
            fdlg.Filter = "All files (*.csv)|*.csv";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                ReadInFiles = fdlg.FileNames.ToList();
            }
        }

        private void ReadCSVfile(string FilePath)
        {

        }
    }
}
