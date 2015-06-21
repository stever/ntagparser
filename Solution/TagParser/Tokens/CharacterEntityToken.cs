using System;
using System.Text;

namespace TagParser.Tokens
{
    public class CharacterEntityToken : ParseToken
    {
        private readonly char _char;

        public CharacterEntityToken(string hex)
        {
            _char = (char)Convert.ToInt32(hex, 16);
        }

        public CharacterEntityToken(int value)
        {
            _char = (char)value;
        }

        public CharacterEntityToken(char c)
        {
            _char = c;
        }

        public char Character
        {
            get { return _char; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Char Entity: ").Append(Parser.ToNamestring(_char));
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("&#").Append((int)_char).Append(";");
            return result.ToString();
        }
    }
}
