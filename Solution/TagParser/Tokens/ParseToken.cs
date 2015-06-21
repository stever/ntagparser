using System.Text;

namespace TagParser.Tokens
{
    public abstract class ParseToken
    {
        /// <summary>
        /// This method returns a descriptive string for the token.
        /// </summary>
        /// <returns>Descriptive string used for debug logging.</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("Type=\"").Append(GetType().Name).Append('"');
            return str.ToString();
        }

        /// <summary>
        /// This method should render to a string equivalent to a parsed source.
        /// </summary>
        /// <returns>String representation of the token value.</returns>
        public abstract string Render();
    }
}
