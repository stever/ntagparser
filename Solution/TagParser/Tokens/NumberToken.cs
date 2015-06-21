using System.Globalization;
using System.Text;

namespace TagParser.Tokens
{
    public class NumberToken : ParseToken
    {
        private readonly long _number;

        public NumberToken(long number)
        {
            _number = number;
        }

        public long Number
        {
            get { return _number; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Number: ").Append(_number);
            return result.ToString();
        }

        public override string Render()
        {
            return _number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
