using System.Text;

namespace TagParser.Tokens
{
    public class ProcessingInstructionToken : ParseToken
    {
        private readonly string _target;
        private readonly string _data;

        public ProcessingInstructionToken(string target, string data)
        {
            _target = target;
            _data = data;
        }

        public string Target
        {
            get { return _target; }
        }

        public string Data
        {
            get { return _data; }
        }

        public new string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("PI: ").Append(_target).Append(' ').Append(_data);
            return result.ToString();
        }

        public override string Render()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<?").Append(_target).Append(' ').Append(_data).Append("?>");
            return result.ToString();
        }
    }
}
