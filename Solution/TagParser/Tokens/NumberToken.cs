using System.Globalization;
using System.Text;

namespace TagParsing.Tokens
{
    public class NumberToken : ParseToken
    {
        private readonly long number;

        public NumberToken(long number)
        {
            this.number = number;
        }

        public long Number
        {
            get { return number; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Number: ").Append(number);
            return result.ToString();
        }

        public override string Render()
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
