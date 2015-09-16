using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessQslAdif.adif
{
    public class AdifLog
    {
        public List<QsoEntry> qsos = new List<QsoEntry>();

        public void loadFromFile(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string line;
            CultureInfo provider = CultureInfo.InvariantCulture;
            string fileContent = "";
            while ((line = file.ReadLine()) != null)
            {
                fileContent += line.Trim();
            }
            file.Close();

            // strip adif header
            var idx = 0;
            fileContent = fileContent.ToUpper();
            var headerEndIdx = fileContent.IndexOf("<EOH>");
            if (headerEndIdx >= 0)
            {
                idx = headerEndIdx + 5;
            }
            while (true)
            {
                var eor = fileContent.IndexOf("<EOR>", idx);
                if (eor < 0)
                {
                    break;
                }
                var qsoLine = fileContent.Substring(idx, eor - idx);
                var qso = new QsoEntry();
                qso.loadFromString(qsoLine);
                qsos.Add(qso);
                idx = eor + 5;
            }
        }

        public void saveToFile(string path)
        {
            using (StreamWriter outfile = new StreamWriter(path))
            {
                outfile.WriteLine("Processed by ProcessQslAdif by SQ2GXO");
                outfile.WriteLine("<EOH>");
                foreach (var q in qsos)
                {
                    outfile.WriteLine(q.ToString());
                    outfile.WriteLine("");
                }
            }
        }
    }
}
