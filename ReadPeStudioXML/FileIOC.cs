using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadPeStudioXML
{
    class FileIOC
    {        
        public List<IOC> IOC = new List<IOC>();

        public string Filename { get; set; }
        public string SHA1 { get; set; }
        public string MD5 { get; set; }
        public string Type { get; set; }
        public string VTresults { get; set; }
        private int totalSeverity;

        public int TotalSeverity
        {
            set
            {
                for (int x = 0; x < IOC.Count; ++x)
                {
                    totalSeverity =+ IOC.ElementAt(x).num;
                }
            }
            get
            {
                return totalSeverity;
            }
        }

        public string Count { get; set; }

        public void AddIOC(int num, string text)
        {
            IOC value = new IOC();
            value.num = num;
            value.text = text;
            IOC.Add(value);
            if (text.ToLower().Contains("virustotal"))
            {
                VTresults = text;
            }
        }
    }
}
