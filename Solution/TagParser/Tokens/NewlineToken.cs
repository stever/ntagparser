namespace TagParser.Tokens
{
    public class NewlineToken : ParseToken
    {
        public new string ToString()
        {
            return "Newline";
        }

        public override string Render()
        {
            return "\n";
        }
    }
}
