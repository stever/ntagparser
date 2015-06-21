using System.Text;

namespace TagParser.Tokens
{
    public class CDataToken : ParseToken
    {
        private readonly string _data;

        public CDataToken(string data)
        {
            _data = data;
        }

        public string Data
        {
            get { return _data; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("CData: ").Append(_data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<![CData[").Append(_data).Append("]]>");
            return result.ToString();
        }
    }
}
