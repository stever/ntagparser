using System.Text;
using TagParser.Tokens;

namespace TagParser
{
    public abstract class Parser
    {
        protected ParseReader Stream; // Character input stream.
        protected int MaxErrors;
        protected int NumErrors;
        protected int NumWarnings;
        protected int NumRecoveries;

        /// <summary>
        /// This is the constructor for the abstract parser class.
        /// </summary>
        /// <param name="stream">Character input stream.</param>
        protected Parser(ParseReader stream)
        {
            Init(stream);
        }

        /// <summary>
        /// This method (re)initialises the parser.
        /// </summary>
        /// <param name="stream">Character input stream.</param>
        private void Init(ParseReader stream)
        {
            Stream = stream;
            MaxErrors = 1000;
            NumErrors = 0;
            NumWarnings = 0;
            NumRecoveries = 0;
        }

        /// <summary>
        /// Abstract method to provide the next token parsed from the input stream.
        /// </summary>
        /// <returns>Next token from the input stream.</returns>
        public abstract ParseToken GetNextToken();

        /// <summary>
        /// The count of characters read on the stream.
        /// </summary>
        public int CharCount
        {
            get { return Stream.CharCount; }
        }

        /// <summary>
        /// The current stream checksum.
        /// </summary>
        /// <returns></returns>
        public char Checksum
        {
            get { return Stream.Checksum; }
        }

        /// <summary>
        /// Produces the line number and column number to be added to error and warning messages.
        /// </summary>
        /// <returns>Character position prefix.</returns>
        public string GetCharacterPosition()
        {
            return (new StringBuilder())
                .Append(Stream.LineNumber).Append(':').Append(Stream.ColumnNumber)
                .ToString();
        }

        /// <summary>
        /// This method is used to report the final status of the parser.
        /// </summary>
        /// <returns>The completion report as a string.</returns>
        public string GetCompletionReport()
        {
            StringBuilder report = new StringBuilder();
            report.Append("Parsed ");
            report.Append(Stream.LineNumber);
            report.Append(" line");
            if (Stream.LineNumber > 1) report.Append('s');
            report.Append(" containing ");
            report.Append(Stream.CharCount);
            report.Append(" characters.");
            if ((NumErrors + NumRecoveries) > 0 || NumWarnings > 0)
            {
                report.Append(" Reported ");
                if ((NumErrors + NumRecoveries) > 0)
                {
                    report.Append(NumErrors + NumRecoveries);
                    report.Append(" error");
                    if (NumErrors + NumRecoveries != 1)
                    {
                        report.Append('s');
                    }
                    if (NumRecoveries > 0)
                    {
                        report.Append(" (recovered ");
                        report.Append(NumRecoveries).Append(')');
                    }
                    if (NumWarnings == 0)
                    {
                        report.Append('.');
                    }
                    else
                    {
                        report.Append(" and ");
                    }
                }
                if (NumWarnings > 0)
                {
                    report.Append(NumWarnings);
                    report.Append(" warnings.");
                }
            }
            return report.ToString();
        }

        /// <summary>
        /// This method returns the string representation of a character.
        /// </summary>
        /// <param name="c">Character value to represent.</param>
        /// <returns>Label used to represent all characters in text logging.</returns>
        public static string ToNamestring(int c)
        {
            switch (c)
            {
                case -1: return "EOF";
                case 0x00: return "0x" + c.ToString("X").ToUpper() + " <NUL>";
                case 0x01: return "0x" + c.ToString("X").ToUpper() + " <SOH>";
                case 0x02: return "0x" + c.ToString("X").ToUpper() + " <STX>";
                case 0x03: return "0x" + c.ToString("X").ToUpper() + " <ETX>";
                case 0x04: return "0x" + c.ToString("X").ToUpper() + " <EOT>";
                case 0x05: return "0x" + c.ToString("X").ToUpper() + " <ENQ>";
                case 0x06: return "0x" + c.ToString("X").ToUpper() + " <ACK>";
                case 0x07: return "0x" + c.ToString("X").ToUpper() + " <BEL>";
                case 0x08: return "0x" + c.ToString("X").ToUpper() + " <BS>";
                case 0x09: return "0x" + c.ToString("X").ToUpper() + " <HT>";
                case 0x0A: return "0x" + c.ToString("X").ToUpper() + " <LF>";
                case 0x0B: return "0x" + c.ToString("X").ToUpper() + " <VT>";
                case 0x0C: return "0x" + c.ToString("X").ToUpper() + " <FF>";
                case 0x0D: return "0x" + c.ToString("X").ToUpper() + " <CR>";
                case 0x0E: return "0x" + c.ToString("X").ToUpper() + " <SO>";
                case 0x0F: return "0x" + c.ToString("X").ToUpper() + " <SI>";
                case 0x10: return "0x" + c.ToString("X").ToUpper() + " <DLE>";
                case 0x11: return "0x" + c.ToString("X").ToUpper() + " <DC1>";
                case 0x12: return "0x" + c.ToString("X").ToUpper() + " <DC2>";
                case 0x13: return "0x" + c.ToString("X").ToUpper() + " <DC3>";
                case 0x14: return "0x" + c.ToString("X").ToUpper() + " <DC4>";
                case 0x15: return "0x" + c.ToString("X").ToUpper() + " <NAK>";
                case 0x16: return "0x" + c.ToString("X").ToUpper() + " <SYN>";
                case 0x17: return "0x" + c.ToString("X").ToUpper() + " <ETB>";
                case 0x18: return "0x" + c.ToString("X").ToUpper() + " <CAN>";
                case 0x19: return "0x" + c.ToString("X").ToUpper() + " <EM>";
                case 0x1A: return "0x" + c.ToString("X").ToUpper() + " <SUB>";
                case 0x1B: return "0x" + c.ToString("X").ToUpper() + " <ESC>";
                case 0x1C: return "0x" + c.ToString("X").ToUpper() + " <FS>";
                case 0x1D: return "0x" + c.ToString("X").ToUpper() + " <GS>";
                case 0x1E: return "0x" + c.ToString("X").ToUpper() + " <RS>";
                case 0x1F: return "0x" + c.ToString("X").ToUpper() + " <US>";
                case 0x20: return "0x" + c.ToString("X").ToUpper() + " <SP>";
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2E:
                case 0x2F:
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x38:
                case 0x39:
                case 0x3A:
                case 0x3B:
                case 0x3C:
                case 0x3D:
                case 0x3E:
                case 0x3F:
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x46:
                case 0x47:
                case 0x48:
                case 0x49:
                case 0x4A:
                case 0x4B:
                case 0x4C:
                case 0x4D:
                case 0x4E:
                case 0x4F:
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x56:
                case 0x57:
                case 0x58:
                case 0x59:
                case 0x5A:
                case 0x5B:
                case 0x5C:
                case 0x5D:
                case 0x5E:
                case 0x5F:
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6A:
                case 0x6B:
                case 0x6C:
                case 0x6D:
                case 0x6E:
                case 0x6F:
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                case 0x78:
                case 0x79:
                case 0x7A:
                case 0x7B:
                case 0x7C:
                case 0x7D:
                case 0x7E:
                    {
                        StringBuilder result = new StringBuilder();
                        result.Append((char)c);
                        return result.ToString();
                    }
                case 0x7F:
                    return "0x" + c.ToString("X").ToUpper() + " <DEL>";
                default:
                    {
                        StringBuilder result = new StringBuilder();
                        result.Append("0x");
                        result.Append(c.ToString("X").ToUpper());
                        return result.ToString();
                    }
            }
        }
    }
}
