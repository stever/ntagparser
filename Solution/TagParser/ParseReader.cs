using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace TagParser
{
    /// <summary>
    /// This class provides the character input stream to the Parser class.
    /// It supports a pushback queue to assist the Parser class deal with unexpected input.
    /// </summary>
    public class ParseReader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Optional filename or URL to identify the stream.
        /// </summary>
        private readonly string _filename;

        /// <summary>
        /// Input stream reader
        /// </summary>
        private readonly TextReader _stream;

        /// <summary>
        /// Pushback queue is used to push characters back onto input stream to be re-parsed.
        /// </summary>
        private readonly Stack<int> _pushbackQueue;

        /// <summary>
        /// Checksum on the raw data from the input stream.
        /// </summary>
        private char _checksum = (char)0;

        /// <summary>
        /// The character count is simply used for keeping a note of the size of the document.
        /// </summary>
        private int _charCount;

        /// <summary>
        /// The column number is used in reporting errors.
        /// </summary>
        private int _columnNumber;

        /// <summary>
        /// The line number is used in reporting errors.
        /// </summary>
        private int _lineNumber = 1;

        /// <summary>
        /// Constructor using a content string.
        /// </summary>
        /// <param name="text">Content string.</param>
        public ParseReader(string text)
        {
            _stream = new StringReader(text);
            _pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Constructor for the ParseReader class.
        /// </summary>
        /// <param name="reader">The character input stream.</param>
        public ParseReader(TextReader reader)
        {
            _stream = reader;
            _pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Constructor for the ParseReader class.
        /// </summary>
        /// <param name="reader">The character input stream.</param>
        /// <param name="filename">Optional filename or URL to identify the stream.</param>
        public ParseReader(TextReader reader, string filename)
        {
            _stream = reader;
            _filename = filename;
            _pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Filename or URL string to identify the string.
        /// </summary>
        public string Filename
        {
            get { return _filename; }
        }

        /// <summary>
        /// Push character back into the stream.
        /// </summary>
        /// <param name="c">Character to push back into the stream.</param>
        public void Pushback(char c)
        {
            Log.InfoFormat("Pushback Char: '{0}'", c);
            _pushbackQueue.Push(c);
        }

        /// <summary>
        /// Push whole string back into the stream.
        /// </summary>
        /// <param name="str">String to push back into the stream.</param>
        public void Pushback(string str)
        {
            Log.InfoFormat("Pushback string: \"{0}\"", str);
            for (int i = str.Length - 1; i > -1; i--)
                _pushbackQueue.Push(str[i]);
        }

        /// <summary>
        /// The current line number.
        /// </summary>
        public int LineNumber
        {
            get { return _lineNumber; }
        }

        /// <summary>
        /// The current column position.
        /// </summary>
        public int ColumnNumber
        {
            get { return _columnNumber; }
        }

        /// <summary>
        /// The character read count.
        /// </summary>
        public int CharCount
        {
            get { return _charCount; }
        }

        /// <summary>
        /// The current stream checksum.
        /// </summary>
        public char Checksum
        {
            get { return _checksum; }
        }

        /// <summary>
        /// This method reads a single character from the raw input stream.
        /// </summary>
        /// <returns>Next character from input.</returns>
        private int RawRead()
        {
            int c = _stream.Read();
            if (c != -1)
            {
                // Count new lines and track column position.
                if (c == '\n')
                {
                    _lineNumber += 1;
                    _columnNumber = 0;
                }
                else
                {
                    _columnNumber++;
                }

                // Count characters read.
                _charCount++;

                // Update the checkum.
                UpdateChecksum((char)c);
            }
            return c;
        }

        /// <summary>
        /// This method reads a single character from the input buffer.
        /// </summary>
        /// <returns>Next character from input.</returns>
        public int Read()
        {
            int nextChar;
            do
            {
                // Pop last character on the queue if there are items pushed-back.
                nextChar = _pushbackQueue.Count > 0 ? _pushbackQueue.Pop() : RawRead();

            } while (nextChar == '\r'); // Ignore linefeed.

            Log.InfoFormat("Char: {0}", Parser.ToNamestring(nextChar));
            return nextChar;
        }

        /// <summary>
        /// This method is used to update the checksum for the stream.
        /// </summary>
        /// <param name="nextChar">The next character on the input stream.</param>
        private void UpdateChecksum(char nextChar)
        {
            // http://atlas.csd.net/~cgadd/knowbase/CRC0013.HTM
            // xor char with the checksum
            if (nextChar != -1)
            {
                _checksum ^= nextChar;
            }
        }
    }
}
