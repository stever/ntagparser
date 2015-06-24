using System;
using System.Text;

namespace TagFormattedDocumentParser.Tokens
{
    public class CharacterEntityToken : ParseToken
    {
        private readonly char character;

        public CharacterEntityToken(string hex)
        {
            character = (char)Convert.ToInt32(hex, 16);
        }

        public CharacterEntityToken(int value)
        {
            character = (char)value;
        }

        public CharacterEntityToken(char c)
        {
            character = c;
        }

        public char Character
        {
            get { return character; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Char Entity: ").Append(Parser.ToNamestring(character));
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("&#").Append((int)character).Append(";");
            return result.ToString();
        }
    }
}
