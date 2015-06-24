using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace TagFormattedDocumentParser
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
        private readonly string filename;

        /// <summary>
        /// Input stream reader
        /// </summary>
        private readonly TextReader stream;

        /// <summary>
        /// Pushback queue is used to push characters back onto input stream to be re-parsed.
        /// </summary>
        private readonly Stack<int> pushbackQueue;

        /// <summary>
        /// The character count is simply used for keeping a note of the size of the document.
        /// </summary>
        private int charCount;

        /// <summary>
        /// The column number is used in reporting errors.
        /// </summary>
        private int columnNumber;

        /// <summary>
        /// The line number is used in reporting errors.
        /// </summary>
        private int lineNumber = 1;

        /// <summary>
        /// Constructor using a content string.
        /// </summary>
        /// <param name="text">Content string.</param>
        public ParseReader(string text)
        {
            stream = new StringReader(text);
            pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Constructor for the ParseReader class.
        /// </summary>
        /// <param name="reader">The character input stream.</param>
        public ParseReader(TextReader reader)
        {
            stream = reader;
            pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Constructor for the ParseReader class.
        /// </summary>
        /// <param name="reader">The character input stream.</param>
        /// <param name="filename">Optional filename or URL to identify the stream.</param>
        public ParseReader(TextReader reader, string filename)
        {
            stream = reader;
            this.filename = filename;
            pushbackQueue = new Stack<int>();
        }

        /// <summary>
        /// Filename or URL string to identify the string.
        /// </summary>
        public string Filename
        {
            get { return filename; }
        }

        /// <summary>
        /// Push character back into the stream.
        /// </summary>
        /// <param name="c">Character to push back into the stream.</param>
        public void Pushback(char c)
        {
            Log.InfoFormat("Pushback Char: '{0}'", c);
            pushbackQueue.Push(c);
        }

        /// <summary>
        /// Push whole string back into the stream.
        /// </summary>
        /// <param name="str">String to push back into the stream.</param>
        public void Pushback(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            Log.InfoFormat("Pushback string: \"{0}\"", str);
            for (int i = str.Length - 1; i >= 0; i--)
                pushbackQueue.Push(str[i]);
        }

        /// <summary>
        /// The current line number.
        /// </summary>
        public int LineNumber
        {
            get { return lineNumber; }
        }

        /// <summary>
        /// The current column position.
        /// </summary>
        public int ColumnNumber
        {
            get { return columnNumber; }
        }

        /// <summary>
        /// The character read count.
        /// </summary>
        public int CharCount
        {
            get { return charCount; }
        }

        /// <summary>
        /// This method reads a single character from the raw input stream.
        /// </summary>
        /// <returns>Next character from input.</returns>
        private int RawRead()
        {
            int c = stream.Read();
            if (c != -1)
            {
                // Count new lines and track column position.
                if (c == '\n')
                {
                    lineNumber += 1;
                    columnNumber = 0;
                }
                else
                {
                    columnNumber++;
                }

                // Count characters read.
                charCount++;
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
                nextChar = pushbackQueue.Count > 0 ? pushbackQueue.Pop() : RawRead();

            } while (nextChar == '\r'); // Ignore linefeed.

            Log.InfoFormat("Char: {0}", Parser.ToNamestring(nextChar));
            return nextChar;
        }
    }
}
