using System.Text;

namespace TagParsing.Tokens
{
    public class ScriptToken : ParseToken
    {
        private readonly string script;

        public ScriptToken(string script)
        {
            this.script = script;
        }

        public string Script
        {
            get { return script; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Script: ").Append(script);
            return result.ToString();
        }

        public override string Render()
        {
            return script;
        }
    }
}
