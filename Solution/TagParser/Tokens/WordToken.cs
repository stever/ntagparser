using System.Text;

namespace TagFormattedDocumentParser.Tokens
{
    public class WordToken : ParseToken
    {
        private readonly string word;

        public WordToken(string word)
        {
            this.word = word;
        }

        public string Word
        {
            get { return word; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Word: ");
            result.Append(WithQuotes(word));
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
            return word;
        }
    }
}
