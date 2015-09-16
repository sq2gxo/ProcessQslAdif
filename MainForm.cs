using ProcessQslAdif.adif;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessQslAdif
{
    public partial class MainForm : Form
    {
        Dictionary<DateTime, string> adiUser1 = new Dictionary<DateTime,string>();
        Dictionary<DateTime, string> adiUser2 = new Dictionary<DateTime,string>();

        string fileName = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load file with activity periods
       
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string cfglocation = System.IO.Path.Combine(dir, "dates.csv");

            System.IO.StreamReader file = new System.IO.StreamReader(cfglocation);
            string line;
            CultureInfo provider = CultureInfo.InvariantCulture;
            while ((line = file.ReadLine()) != null)
            {
                line.Trim();
                if (line.StartsWith("#")) {
                    continue;
                }
                var splitted = line.Split(';');
                var timestamp = DateTime.ParseExact(splitted[0] + splitted[1], "yyyyMMddHHmmss", provider);
                adiUser1.Add(timestamp, splitted[2].Trim());
                adiUser2.Add(timestamp, splitted[3].Trim());
            }
            file.Close();

            new QsoEntry().loadFromString("");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = @"ADIF log files (*.adi, *.adif)|*.adi;*.adif";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                fileName = openFileDialog1.FileName;
                var fnLen = fileName.Length;
                if (fnLen < 30)
                {
                    textBox1.Text = fileName;
                }
                else
                {
                    textBox1.Text = "..." +fileName.Substring(fnLen - 28, 28);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fileName == null)
            {
                MessageBox.Show("Select ADIF file");
                return;
            }
            var log = new AdifLog();
            log.loadFromFile(fileName);

            CultureInfo provider = CultureInfo.InvariantCulture;
            foreach (var q in log.qsos) {
                // get q datetime
                var timestamp = DateTime.ParseExact(q.fields["QSO_DATE"] + q.fields["TIME_ON"], "yyyyMMddHHmmss", provider);
                var tsKey = adiUser1.Keys.Where(n => n < timestamp).Max();
                q.fields["APP_LOGGER32_USER_1"] = adiUser1[tsKey];
                q.fields["APP_LOGGER32_USER_2"] = adiUser2[tsKey];
                // process freq
                var band = q.fields["BAND"];
                switch (band)
                {
                    case "160M":
                        q.fields["FREQ"] = "1.8";
                        break;
                    case "80M":
                        q.fields["FREQ"] = "3.5";
                        break;
                    case "40M":
                        q.fields["FREQ"] = "7.0";
                        break;
                    case "30M":
                        q.fields["FREQ"] = "10.1";
                        break;
                    case "20M":
                        q.fields["FREQ"] = "14.0";
                        break;
                    case "17M":
                        q.fields["FREQ"] = "18.1";
                        break;
                    case "15M":
                        q.fields["FREQ"] = "21.0";
                        break;
                    case "12M":
                        q.fields["FREQ"] = "24.9";
                        break;
                    case "10M":
                        q.fields["FREQ"] = "28.0";
                        break;
                    case "6M":
                        q.fields["FREQ"] = "50.0";
                        break;
                    case "2M":
                        q.fields["FREQ"] = "144";
                        break;
                    case "70CM":
                        q.fields["FREQ"] = "432";
                        break;
                }
            }
            var ext = Path.GetExtension(fileName);
            var pathWithoutExtension = fileName.Remove(fileName.Length - ext.Length);

            log.saveToFile(pathWithoutExtension + "-proc" + ext);

            MessageBox.Show("ADIF processing complete");
        }
    }
}
