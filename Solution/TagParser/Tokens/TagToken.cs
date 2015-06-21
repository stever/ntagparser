using System.Text;

namespace TagParser.Tokens
{
    public class TagToken : ParseToken
    {
        private Tag _tag;

        public TagToken(Tag tag)
        {
            _tag = tag;
        }

        public Tag Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Tag: ").Append(_tag);
            return result.ToString();
        }

        public override string Render()
        {
            return _tag.ToString();
        }
    }
}
