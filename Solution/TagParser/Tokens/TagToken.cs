using System.Text;

namespace TagParsing.Tokens
{
    public class TagToken : ParseToken
    {
        private Tag tag;

        public TagToken(Tag tag)
        {
            this.tag = tag;
        }

        public Tag Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Tag: ").Append(tag);
            return result.ToString();
        }

        public override string Render()
        {
            return tag.ToString();
        }
    }
}
