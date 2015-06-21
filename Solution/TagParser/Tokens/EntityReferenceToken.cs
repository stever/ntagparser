using System.Text;

namespace TagParser.Tokens
{
    public class EntityReferenceToken : ParseToken
    {
        private readonly string _name;

        public EntityReferenceToken(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Entity Reference: ").Append(_name);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("&").Append(_name).Append(";");
            return result.ToString();
        }
    }
}
