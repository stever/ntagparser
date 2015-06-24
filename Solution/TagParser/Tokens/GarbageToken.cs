using System.Text;

namespace TagParser.Tokens
{
    public class GarbageToken : ParseToken
    {
        private readonly string _data;

        public GarbageToken(string data)
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
            result.Append("Garbage: ").Append(_data);
            return result.ToString();
        }

        public override string Render()
        {
            return _data;
        }
    }
}
