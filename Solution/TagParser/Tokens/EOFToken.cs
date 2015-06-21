using System;

namespace TagParser.Tokens
{
    public class EOFToken : ParseToken
    {
        public new string ToString()
        {
            return "EOF";
        }

        public override string Render()
        {
            throw new Exception("Can't render EOF");
        }
    }
}
