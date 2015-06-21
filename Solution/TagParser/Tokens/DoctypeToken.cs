using System.Text;

namespace TagParser.Tokens
{
    public class DoctypeToken : ParseToken
    {
        private readonly string _name;
        private readonly string _data;

        public DoctypeToken(string name, string data)
        {
            _name = name;
            _data = data;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Data
        {
            get { return _data; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Doctype: ").Append(_name).Append(' ').Append(_data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<!").Append(_name).Append(' ').Append(_data).Append(">");
            return result.ToString();
        }
    }
}
