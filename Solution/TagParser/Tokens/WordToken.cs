using System.Text;

namespace TagParser.Tokens
{
    public class WordToken : ParseToken
    {
        private readonly string _word;

        public WordToken(string word)
        {
            _word = word;
        }

        public string Word
        {
            get { return _word; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Word: ");
            result.Append(WithQuotes(_word));
            return result.ToString();
        }

        private static string WithQuotes(string str)
        {
            StringBuilder result = new StringBuilder();
            result.Append(str.Length == 1 ? '\'' : '"');
            result.Append(str);
            result.Append(str.Length == 1 ? '\'' : '"');
            return result.ToString();
        }

        public override string Render()
        {
            return _word;
        }
    }
}
