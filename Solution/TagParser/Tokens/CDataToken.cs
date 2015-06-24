using System.Text;

namespace TagFormattedDocumentParser.Tokens
{
    public class CDataToken : ParseToken
    {
        private readonly string data;

        public CDataToken(string data)
        {
            this.data = data;
        }

        public string Data
        {
            get { return data; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("CDATA: ").Append(data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<![CDATA[").Append(data).Append("]]>");
            return result.ToString();
        }
    }
}
