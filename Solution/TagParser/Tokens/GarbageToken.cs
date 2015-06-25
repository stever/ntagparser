using System.Text;

namespace TagParsing.Tokens
{
    public class GarbageToken : ParseToken
    {
        private readonly string data;

        public GarbageToken(string data)
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
            result.Append("Garbage: ").Append(data);
            return result.ToString();
        }

        public override string Render()
        {
            return data;
        }
    }
}
