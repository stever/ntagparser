using System.Text;

namespace TagParser.Tokens
{
    public class CommentToken : ParseToken
    {
        private readonly string _comment;

        public CommentToken(string comment)
        {
            _comment = comment;
        }

        public string Comment
        {
            get { return _comment; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Comment: ").Append(_comment);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<!--").Append(_comment).Append("-->");
            return result.ToString();
        }
    }
}
