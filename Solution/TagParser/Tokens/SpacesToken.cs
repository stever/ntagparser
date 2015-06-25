using System.Text;

namespace TagParsing.Tokens
{
    public class SpacesToken : ParseToken
    {
        private readonly string spaces;

        public SpacesToken(string spaces)
        {
            this.spaces = spaces;
        }

        public string Spaces
        {
            get { return spaces; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Spaces: ");
            result.Append(spaces.Length == 1 ? '\'' : '"');
            foreach (char c in spaces)
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
            result.Append(spaces.Length == 1 ? '\'' : '"');
            return result.ToString();
        }

        public override string Render()
        {
            return spaces;
        }
    }
}
