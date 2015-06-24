using System.Text;

namespace TagParser.Tokens
{
    public class EntityReferenceToken : ParseToken
    {
        private readonly string name;

        public EntityReferenceToken(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Entity Reference: ").Append(name);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("&").Append(name).Append(";");
            return result.ToString();
        }
    }
}
