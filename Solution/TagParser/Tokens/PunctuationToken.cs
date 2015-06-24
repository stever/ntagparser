using System.Globalization;
using System.Text;

namespace TagFormattedDocumentParser.Tokens
{
    public class PunctuationToken : ParseToken
    {
        private readonly char character;

        public PunctuationToken(char c)
        {
            character = c;
        }

        public char Character
        {
            get { return character; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Punctuation: ").Append(character);
            return result.ToString();
        }

        public override string Render()
        {
            return character.ToString(CultureInfo.InvariantCulture);
        }
    }
}
