using System.Text;

namespace TagParsing.Tokens
{
    public class CommentToken : ParseToken
    {
        private readonly string comment;

        public CommentToken(string comment)
        {
            this.comment = comment;
        }

        public string Comment
        {
            get { return comment; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Comment: ").Append(comment);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<!--").Append(comment).Append("-->");
            return result.ToString();
        }
    }
}
