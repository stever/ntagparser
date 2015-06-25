using System.Reflection;
using System.Text;
using log4net;

namespace TagParsing
{
    public class Attribute
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string name;
        private string value;

        public Attribute(string name)
        {
            this.name = name;
            value = null;
        }

        public Attribute(string name, string value)
        {
            this.name = name;
            if (value != null && !value.ToLower().Equals("true"))
                this.value = value;
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return value; }
            set
            {
                if (value == null) return;
                if (this.value != null)
                {
                    if (Log.IsWarnEnabled)
                    {
                        StringBuilder msg = new StringBuilder();
                        msg.Append("Overwriting previous attribute value. ");
                        msg.Append("Attribute name is \"").Append(name).Append("\". ");
                        msg.Append("Old value is \"").Append(this.value).Append("\". ");
                        msg.Append("New value is \"").Append(value).Append("\".");
                        Log.Warn(msg.ToString());
                    }
                }
                this.value = value;
            }
        }
    }
}
