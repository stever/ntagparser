using System.Text;

namespace TagFormattedDocumentParser.Tokens
{
    public class DoctypeToken : ParseToken
    {
        private readonly string name;
        private readonly string data;

        public DoctypeToken(string name, string data)
        {
            this.name = name;
            this.data = data;
        }

        public string Name
        {
            get { return name; }
        }

        public string Data
        {
            get { return data; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Doctype: ").Append(name).Append(' ').Append(data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<!").Append(name).Append(' ').Append(data).Append(">");
            return result.ToString();
        }
    }
}
