using System.Globalization;
using System.Text;

namespace TagParser.Tokens
{
    public class PunctuationToken : ParseToken
    {
        private readonly char _character;

        public PunctuationToken(char c)
        {
            _character = c;
        }

        public char Character
        {
            get { return _character; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Punctuation: ").Append(_character);
            return result.ToString();
        }

        public override string Render()
        {
            return _character.ToString(CultureInfo.InvariantCulture);
        }
    }
}
