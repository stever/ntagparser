using System.Reflection;
using System.Text;
using log4net;

namespace TagParser
{
    public class Attribute
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _name;
        private string _value;

        public Attribute(string name)
        {
            _name = name;
            _value = null;
        }

        public Attribute(string name, string value)
        {
            _name = name;
            if (value != null && !value.ToLower().Equals("true"))
                _value = value;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == null) return;
                if (_value != null)
                {
                    if (Log.IsWarnEnabled)
                    {
                        StringBuilder msg = new StringBuilder();
                        msg.Append("Overwriting previous attribute value. ");
                        msg.Append("Attribute name is \"").Append(_name).Append("\". ");
                        msg.Append("Old value is \"").Append(_value).Append("\". ");
                        msg.Append("New value is \"").Append(value).Append("\".");
                        Log.Warn(msg.ToString());
                    }
                }
                _value = value;
            }
        }
    }
}
