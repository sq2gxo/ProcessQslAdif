using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessQslAdif.adif
{
    public class QsoEntry
    {
        public Dictionary<string, string> fields = new Dictionary<string, string>();

        public void loadFromString(string qsoLine)
        {
            Match match = Regex.Match(qsoLine, @"<([A-Z0-9_]+):(\d+):?[A-Z]?>([^<]+)");
            while(match.Success)
            {
                var fieldName = match.Groups[1].Value;
                var fieldLen = int.Parse(match.Groups[2].Value);
                var fieldVal = match.Groups[3].Value.Substring(0, fieldLen);
                fields.Add(fieldName, fieldVal);

                match = match.NextMatch();
            }
        }

        public string ToString()
        {
            string retVal = "";

            foreach (KeyValuePair<string, string> entry in fields)
            {
                var dataType = "";
                if (entry.Key == "QSO_DATE")
                {
                    dataType = ":D";
                }
                retVal += "<" + entry.Key + ":" + entry.Value.Length + dataType + ">" + entry.Value + " ";
            }
            retVal += "<EOR>";
            
            return retVal;
        }
    }
}