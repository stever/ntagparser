using System.Text;

namespace TagParser.Tokens
{
    public class ScriptToken : ParseToken
    {
        private readonly string _script;

        public ScriptToken(string script)
        {
            _script = script;
        }

        public string Script
        {
            get { return _script; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Script: ").Append(_script);
            return result.ToString();
        }

        public override string Render()
        {
            return _script;
        }
    }
}
