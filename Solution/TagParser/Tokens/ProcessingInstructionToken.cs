using System.Text;

namespace TagParsing.Tokens
{
    public class ProcessingInstructionToken : ParseToken
    {
        private readonly string target;
        private readonly string data;

        public ProcessingInstructionToken(string target, string data)
        {
            this.target = target;
            this.data = data;
        }

        public string Target
        {
            get { return target; }
        }

        public string Data
        {
            get { return data; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("PI: ").Append(target).Append(' ').Append(data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<?").Append(target).Append(' ').Append(data).Append("?>");
            return result.ToString();
        }
    }
}
