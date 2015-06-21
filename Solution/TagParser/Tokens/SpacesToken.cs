using System.Text;

namespace TagParser.Tokens
{
    public class SpacesToken : ParseToken
    {
        private readonly string _spaces;

        public SpacesToken(string spaces)
        {
            _spaces = spaces;
        }

        public string Spaces
        {
            get { return _spaces; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Spaces: ");
            result.Append(_spaces.Length == 1 ? '\'' : '"');
            foreach (char c in _spaces)
            {
                switch (c)
                {
                    case ' ':
                        result.Append(' ');
                        break;
                    case '\t':
                        result.Append("\\t");
                        break;
                    case '\n':
                        result.Append("\\n");
                        break;
                    case '\r':
                        result.Append("\\r");
                        break;
                    default:
                        result.Append('?');
                        break;
                }
            }
            result.Append(_spaces.Length == 1 ? '\'' : '"');
            return result.ToString();
        }

        public override string Render()
        {
            return _spaces;
        }
    }
}
