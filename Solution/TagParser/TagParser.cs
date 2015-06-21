using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TagParser.Tokens;
using log4net;

namespace TagParser
{
    public class TagParser : Parser
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Parser state

        private enum State
        {
            Recover,            // Recovery after Tag Error
            Initial,
            Number,
            OpenTag,            // Start Tag
            TagName,            // Element Content
            EndTag1,            // End Tag
            EndTag2,
            SGML,
            DTD1,               // Document Type Declaration
            DTD2,
            DTD3,
            Comment1,           // Comment
            Comment2,
            Comment3,
            Comment4,
            PITarget,           // Processing Instruction - Target
            PIData,             // Processing Instruction - Data
            EndPI,
            Entity,             // Entity Reference
            Ref,
            Char,
            Hex,                // Character Reference (Hex)
            Decimal,            // Character Reference (DEC)
            Spaces,
            Tag,
            EmptyElement1,     // Empty Element
            EmptyElement2,
            Name1,              // Element Attribute
            Name2,
            Value1,             // Attribute Value
            Value2,
            Label,
            Quoted,
            Comma,
            CDATA1,             // CData Section
            CDATA2,
            CDATA3,
            CDATA4,
            CDATA5,
            Script1,            // New state for collecting content of the SCRIPT element.
            Script2,
            Script3,
            Script4,
            Script5,
            Script6,
            Script7,
            Script8,
            Script9,
        }

        private static string Description(State state)
        {
            switch (state)
            {
                case State.Recover:
                    return "error recovery";
                case State.Initial:
                    return "text content";
                case State.Number:
                    return "number text content";
                case State.OpenTag:
                    return "tag markup entry";
                case State.TagName:
                    return "start-tag name";
                case State.EndTag1:
                    return "end-tag begin";
                case State.EndTag2:
                    return "end-tag name";
                case State.SGML:
                    return "SGML tag begin";
                case State.DTD1:
                    return "DTD identifier";
                case State.DTD2:
                    return "DTD white-space";
                case State.DTD3:
                    return "DTD unparsed data";
                case State.Comment1:
                    return "comment entry-sequence";
                case State.Comment2:
                    return "comment content";
                case State.Comment3:
                    return "comment exit-sequence A";
                case State.Comment4:
                    return "comment exit-sequence B";
                case State.PITarget:
                    return "PI target identifier";
                case State.PIData:
                    return "PI data";
                case State.EndPI:
                    return "PI end";
                case State.Entity:
                    return "entity markup begin";
                case State.Ref:
                    return "entity reference";
                case State.Char:
                    return "character reference begin";
                case State.Hex:
                    return "hexadecimal character";
                case State.Decimal:
                    return "decimal character";
                case State.Spaces:
                    return "white-space content";
                case State.Tag:
                    return "tag attribute parse begin";
                case State.EmptyElement1:
                    return "empty-element exception 1";
                case State.EmptyElement2:
                    return "empty-element exception 2";
                case State.Name1:
                    return "attribute name A";
                case State.Name2:
                    return "attribute name B";
                case State.Value1:
                    return "attribute value begin";
                case State.Value2:
                    return "attribute value end";
                case State.Label:
                    return "literal value";
                case State.Quoted:
                    return "quoted value";
                case State.Comma:
                    return "inverted-comma delimited value";
                case State.CDATA1:
                    return "CData entry-sequence A";
                case State.CDATA2:
                    return "CData entry-sequence B";
                case State.CDATA3:
                    return "CData identifier";
                case State.CDATA4:
                    return "CData exit-sequence A";
                case State.CDATA5:
                    return "CData exit-sequence B";
                case State.Script1:
                    return "Script element content";
                case State.Script2:
                    return "Script escape step 1 ('<')";
                case State.Script3:
                    return "Script escape step 2 ('/')";
                case State.Script4:
                    return "Script escape step 3 ('s')";
                case State.Script5:
                    return "Script escape step 4 ('c')";
                case State.Script6:
                    return "Script escape step 5 ('r')";
                case State.Script7:
                    return "Script escape step 6 ('i')";
                case State.Script8:
                    return "Script escape step 7 ('p')";
                case State.Script9:
                    return "Script escape step 8 ('t')";
                default:
                    throw new Exception("Unexpected case");
            }
        }

        private State _state = State.Initial;

        /// <summary>
        /// Parser machine state.
        /// </summary>
        private State ParseState
        {
            get { return _state; }
            set
            {
                if (_state != value) Log.InfoFormat("Changing state {0} to {1} state.", _state, value);
                else Log.WarnFormat("Changing to same state! ({0})", Description(_state));
                _state = value;
            }
        }

        /*
        /// <summary>
        /// This produces a warning message when a character is accepted by default.
        /// </summary>
        /// <param name="c">The character accepted by default.</param>
        /// <param name="state">The parser state.</param>
        /// <returns>The warning message.</returns>
        private string GetCharDefaultAcceptWarningMessage(char c, State state)
        {
            return (new StringBuilder())
                    .Append("Character ").Append(ToNamestring(c)).Append(' ')
                    .Append("defaulted from the ").Append(state).Append(" state ")
                    .Append('(').Append(GetCharacterPosition()).Append(')')
                    .ToString();
        }
        */

        /// <summary>
        /// Produces the error message for invalid name character.
        /// </summary>
        /// <param name="c">The invalid character.</param>
        /// <param name="state">The parser state.</param>
        /// <returns>The error message.</returns>
        private string GetInvalidCharErrorMessage(char c, State state)
        {
            return (new StringBuilder())
                    .Append("Character ").Append(ToNamestring(c)).Append(' ')
                    .Append("cannot be accepted from the ").Append(state).Append(" state ")
                    .Append('(').Append(GetCharacterPosition()).Append(')')
                    .ToString();
        }

        /// <summary>
        /// This method reports that there does not exist an edge from the current
        /// state which matches the next character from input.
        /// </summary>
        /// <param name="edge">Next input character.</param>
        /// <param name="state">Current parse state.</param>
        /// <returns>Error message for when invalid character found.</returns>
        private string GetEdgeUnknownErrorMessage(char edge, State state)
        {
            return (new StringBuilder())
                    .Append("No edge labelled ").Append(ToNamestring(edge)).Append(' ')
                    .Append("from the ").Append(state).Append(" state ")
                    .Append('(').Append(GetCharacterPosition()).Append(')')
                    .ToString();
        }

        #endregion

        /// <summary>
        /// Table of known entities and their text equivalents.
        /// </summary>
        private readonly Dictionary<string, string> _entities;

        /// <summary>
        /// Flag for tag and attribute name case-sensitivity.
        /// </summary>
        private bool _caseSensitive;

        /// <summary>
        /// Constructor for TagParser.
        /// </summary>
        /// <param name="stream">Character stream reader.</param>
        public TagParser(ParseReader stream)
            : base(stream)
        {
            _entities = new Dictionary<string, string> { { "amp", "&" }, { "nbsp", " " }, { "quot", "\"" } };
            Log.Debug("Constructed TagParser");
        }

        /// <summary>
        /// Getter for case-sensitivity option property.
        /// </summary>
        /// <returns>True if case-sensitive option on.</returns>
        public bool IsCaseSensitive()
        {
            return _caseSensitive;
        }

        /// <summary>
        /// Setter for case-sensitivity option property.
        /// </summary>
        /// <param name="caseSensitive">Option to regulate case sensitivity.</param>
        public void IsCaseSensitive(bool caseSensitive)
        {
            _caseSensitive = caseSensitive;
        }

        /// <summary>
        /// Returns the next token from the input stream.
        /// </summary>
        /// <returns>Next token from the input stream.</returns>
        public override ParseToken GetNextToken()
        {
            Log.Debug("getNextToken()");

            // Buffer containing text in current context.
            StringBuilder buffer = new StringBuilder();
            string name = null;

            // Read nextToken character from the input stream.
            int c;
            while ((c = Stream.Read()) != -1)
            {
                Log.DebugFormat("c = {0}", c);

                char nextChar = (char)c;
                switch (ParseState)
                {
                    case State.Recover:
                        {
                            if (nextChar == '>')
                            {
                                ParseState = State.Initial;
                            }
                            break;
                        }
                    case State.Initial:
                        {
                            switch (nextChar)
                            {
                                case '<':
                                    {
                                        ParseState = State.OpenTag;
                                        if (buffer.Length == 0) break; // No token yet.
                                        return new WordToken(buffer.ToString());
                                    }

                                case '&':
                                    {
                                        ParseState = State.Entity;
                                        if (buffer.Length == 0) break; // No token yet.
                                        return new WordToken(buffer.ToString());
                                    }

                                case ' ':
                                case '\t':
                                case '\r':
                                    {
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Spaces;
                                        if (buffer.Length == 0) break; // No token yet.
                                        return new WordToken(buffer.ToString());
                                    }

                                case '\n':
                                    {
                                        if (buffer.Length == 0)
                                            return new NewlineToken();

                                        // Push newline back and return new token.
                                        Stream.Pushback(nextChar);
                                        return new WordToken(buffer.ToString());
                                    }

                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    {
                                        // Pushback number character, return text in buffer.
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Number;
                                        if (buffer.Length == 0) break;
                                        return new WordToken(buffer.ToString());
                                    }

                                case '\'':
                                case '`':
                                case '!':
                                case '"':
                                case '^':
                                case '*':
                                case '(':
                                case ')':
                                case '-':
                                case '_':
                                case '+':
                                case '=':
                                case '|':
                                case '[':
                                case ']':
                                case '{':
                                case '}':
                                case ':':
                                case ';':
                                case '@':
                                case '~':
                                case '#':
                                case ',':
                                case '.':
                                case '?':
                                case '/':
                                case '\\':
                                    {
                                        // If there's any characters in the buffer, pushback
                                        // the punctuation character and return text buffer.
                                        if (buffer.Length > 0)
                                        {
                                            Stream.Pushback(nextChar);
                                            return new WordToken(buffer.ToString());
                                        }
                                        return new PunctuationToken(nextChar);
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Number:
                        {
                            switch (nextChar)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }

                                default:
                                    {
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Initial;
                                        ParseToken token;
                                        try
                                        {
                                            token = new NumberToken(Convert.ToInt64(buffer.ToString()));
                                        }
                                        catch (FormatException)
                                        {
                                            token = new WordToken(buffer.ToString());
                                        }
                                        return token;
                                    }
                            }
                            break;
                        }

                    case State.Spaces:
                        {
                            switch (nextChar)
                            {
                                case ' ':
                                case '\t':
                                case '\r':
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }

                                default:
                                    {
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Initial;
                                        return new SpacesToken(buffer.ToString());
                                    }
                            }
                            break;
                        }

                    case State.OpenTag:
                        {
                            switch (nextChar)
                            {
                                case '!':
                                    {
                                        ParseState = State.SGML;
                                        break;
                                    }

                                case '?':
                                    {
                                        ParseState = State.PITarget;
                                        break;
                                    }

                                case '/':
                                    {
                                        buffer.Append(nextChar);
                                        ParseState = State.EndTag1;
                                        break;
                                    }

                                default:
                                    {
                                        if (IsNameFirstChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                            ParseState = State.TagName;
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.TagName:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        string tagname = buffer.ToString();
                                        Tag tag = new Tag(tagname, _caseSensitive);
                                        ParseState = tagname.ToLower().Equals("script") ? State.Script1 : State.Initial;
                                        return new TagToken(tag);
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        string tagname = buffer.ToString();
                                        Tag tag = GetTag(tagname);
                                        ParseState = tagname.ToLower().Equals("script") ? State.Script1 : State.Initial;
                                        return new TagToken(tag);
                                    }

                                case '/':
                                    {
                                        ParseState = State.EmptyElement1;
                                        break;
                                    }

                                default:
                                    {
                                        if (IsNameChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.EmptyElement1:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        string tagname = buffer.ToString();
                                        Tag tag = new Tag(tagname, _caseSensitive);
                                        ParseState = tagname.ToLower().Equals("script") ? State.Script1 : State.Initial;
                                        return new TagToken(tag);
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                default:
                                    {
                                        // Report unknown transition path as an error.
                                        Log.Error(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        ParseState = State.Recover;
                                        if (++NumErrors >= MaxErrors)
                                        {
                                            throw new MaxErrorsException();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Entity:
                        {
                            switch (nextChar)
                            {
                                case '#':
                                    {
                                        ParseState = State.Char;
                                        break;
                                    }

                                default:
                                    {
                                        if (IsNameFirstChar(nextChar))
                                        {
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Ref;
                                        }
                                        else
                                        {

                                            // Common error to have '&' in hand-written HTML not "&nbsp;" as it should be!
                                            // In this case a warning will be logged and the HTML will be corrected.
                                            Log.Debug(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Log.WarnFormat("Recovery assumed that '&' is not intended as an entity reference ({0})", GetCharacterPosition());
                                            if (Log.IsDebugEnabled)
                                            {
                                                Log.DebugFormat("nextChar='{0}'", nextChar);
                                                if (Stream.Filename != null)
                                                {
                                                    Log.DebugFormat("filename=\"{0}\"", Stream.Filename);
                                                }
                                            }
                                            Stream.Pushback(nextChar);
                                            Stream.Pushback("&amp;");
                                            NumRecoveries++;
                                            ParseState = State.Initial;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Ref:
                        {
                            switch (nextChar)
                            {
                                case ';':
                                    {
                                        ParseState = State.Initial;
                                        return new EntityReferenceToken(buffer.ToString());
                                    }

                                default:
                                    {

                                        // Normally only accept valid name characters.
                                        if (IsNameChar(nextChar))
                                        {
                                            buffer.Append(nextChar);

                                        }
                                        else
                                        {

                                            // A common error in hand-written HTML is to omit ';' at end of an entity reference.
                                            // HTML correction depends on whether or not the entity is recognised.
                                            // In any case the error will be handled and a warning message will be logged.
                                            Log.Debug(GetInvalidCharErrorMessage(nextChar, ParseState));

                                            // Check known entities to decide if the ';' was omitted
                                            string str = buffer.ToString();
                                            if (IsKnownEntity(str))
                                            {

                                                // This is a known entity, so assume that the ';' is missing.
                                                // Pushback last character and resume parsing from initial state.
                                                Log.WarnFormat("Recovery assumed that ; should have ended this entity reference ({0})", GetCharacterPosition());
                                                Stream.Pushback(nextChar);
                                                NumRecoveries++;
                                                ParseState = State.Initial;
                                                return new EntityReferenceToken(str);
                                            }

                                            // This is not a known entity, so we don't assume that the ';' is missing.
                                            // Push the whole string from buffer back onto stream and resume from initial state.
                                            Log.WarnFormat("Recovery assumed that text was not intended as an entity reference ({0})", GetCharacterPosition());
                                            buffer.Append(nextChar);
                                            Stream.Pushback(buffer.ToString());
                                            buffer = new StringBuilder();
                                            NumRecoveries++;
                                            ParseState = State.Initial;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Char:
                        {
                            switch (nextChar)
                            {
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                case '0':
                                    {
                                        buffer.Append(nextChar);
                                        ParseState = State.Decimal;
                                        break;
                                    }

                                case 'X':
                                case 'x':
                                    {
                                        ParseState = State.Hex;
                                        break;
                                    }

                                default:
                                    {
                                        // Unknown transition path from this state
                                        Log.Error(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        ParseState = State.Recover;
                                        if (++NumErrors >= MaxErrors)
                                        {
                                            throw new MaxErrorsException();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Hex:
                        {
                            switch (nextChar)
                            {
                                case ';':
                                    {
                                        ParseState = State.Initial;
                                        return new CharacterEntityToken(buffer.ToString());
                                    }

                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                case '0':
                                case 'a':
                                case 'b':
                                case 'c':
                                case 'd':
                                case 'e':
                                case 'f':
                                case 'A':
                                case 'B':
                                case 'C':
                                case 'D':
                                case 'E':
                                case 'F':
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }

                                default:
                                    {

                                        // A common error in hand-written HTML is to omit ';' at end of an entity reference.
                                        // Pushback last character and resume parsing from initial state.
                                        Log.Debug(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        string str = buffer.ToString();
                                        Log.WarnFormat("Recovery assumed that ; should have ended this character entity: {0} ({1})", str, GetCharacterPosition());
                                        Stream.Pushback(nextChar);
                                        NumRecoveries++;
                                        ParseState = State.Initial;
                                        return new CharacterEntityToken(str);
                                    }
                            }
                            break;
                        }

                    case State.Decimal:
                        {
                            switch (nextChar)
                            {
                                case ';':
                                    {
                                        ParseState = State.Initial;
                                        return new CharacterEntityToken(Convert.ToInt32(buffer.ToString()));
                                    }

                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                case '0':
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }

                                default:
                                    {

                                        // A common error in hand-written HTML is to omit ';' at end of an entity reference.
                                        // Pushback last character and resume parsing from initial state.
                                        Log.Debug(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        Log.WarnFormat("Recovery assumed that ; should have ended this character entity ({0})", GetCharacterPosition());
                                        Stream.Pushback(nextChar);
                                        NumRecoveries++;
                                        ParseState = State.Initial;
                                        return new CharacterEntityToken(Convert.ToInt32(buffer.ToString()));
                                    }
                            }
                            break;
                        }

                    case State.SGML:
                        {
                            switch (nextChar)
                            {
                                case '-':
                                    {
                                        ParseState = State.Comment1;
                                        break;
                                    }

                                case '[':
                                    {
                                        ParseState = State.CDATA1;
                                        break;
                                    }

                                default:
                                    {
                                        if (IsNameFirstChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                            ParseState = State.DTD1;
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        break;

                    case State.Comment1:
                        {
                            switch (nextChar)
                            {
                                case '-':
                                    {
                                        ParseState = State.Comment2;
                                        break;
                                    }

                                default:
                                    {
                                        // Unknown transition path from this state
                                        Log.Error(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        ParseState = State.Recover;
                                        if (++NumErrors >= MaxErrors)
                                        {
                                            throw new MaxErrorsException();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Comment2:
                        {
                            switch (nextChar)
                            {
                                case '-':
                                    {
                                        buffer.Append(nextChar);
                                        ParseState = State.Comment3;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Comment3:
                        {
                            buffer.Append(nextChar);
                            switch (nextChar)
                            {
                                case '-':
                                    {
                                        ParseState = State.Comment4;
                                        break;
                                    }

                                default:
                                    {
                                        ParseState = State.Comment2;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Comment4:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        ParseState = State.Initial;
                                        string data = buffer.ToString().Substring(0, buffer.Length - 2);
                                        return new CommentToken(data);
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        ParseState = State.Comment2;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.EndTag1:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        string tagname = buffer.ToString();
                                        Tag tag = new Tag(tagname, _caseSensitive);
                                        ParseState = tagname.ToLower().Equals("script") ? State.Script1 : State.Initial;
                                        return new TagToken(tag);
                                    }

                                default:
                                    {
                                        if (IsNameChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                            ParseState = State.EndTag2;
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.EndTag2:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        ParseState = State.Initial;
                                        Tag tag = new Tag(buffer.ToString(), _caseSensitive);
                                        return new TagToken(tag);
                                    }

                                default:
                                    {
                                        if (IsNameChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.CDATA1:
                        {
                            if (IsNameChar(nextChar))
                            {
                                buffer.Append(nextChar);
                                ParseState = State.CDATA2;
                            }
                            else
                            {
                                Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                Stream.Pushback(nextChar);
                                ParseState = State.Recover;
                                if (++NumErrors >= MaxErrors)
                                {
                                    throw new MaxErrorsException();
                                }
                            }
                            break;
                        }

                    case State.CDATA2:
                        {
                            switch (nextChar)
                            {
                                case '[':
                                    {
                                        if (buffer.ToString().ToUpper().Equals("CDATA"))
                                        {
                                            buffer = new StringBuilder();
                                            ParseState = State.CDATA3;
                                        }
                                        else
                                        {
                                            Log.Error("CData declaration expected");
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        if (IsNameChar(nextChar))
                                        {
                                            buffer.Append(nextChar);
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.CDATA3:
                        {
                            switch (nextChar)
                            {
                                case ']':
                                    {
                                        ParseState = State.CDATA4;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.CDATA4:
                        {
                            switch (nextChar)
                            {
                                case ']':
                                    {
                                        ParseState = State.CDATA5;
                                        break;
                                    }

                                default:
                                    {
                                        ParseState = State.CDATA3;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.CDATA5:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        ParseState = State.Initial;
                                        return new CDataToken(buffer.ToString());
                                    }

                                default:
                                    {
                                        ParseState = State.CDATA3;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.DTD1:
                        {
                            switch (nextChar)
                            {
                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {

                                        // Checking that the DTD name string is recognised.
                                        name = buffer.ToString();
                                        buffer = new StringBuilder();
                                        if ((name.ToUpper().Equals("DOCTYPE")) ||
                                            (name.ToUpper().Equals("ELEMENT")) ||
                                            (name.ToUpper().Equals("ATTLIST")) ||
                                            (name.ToUpper().Equals("ENTITY")) ||
                                            (name.ToUpper().Equals("NOTATION")))
                                        {

                                            ParseState = State.DTD2;

                                        }
                                        else
                                        {
                                            Log.ErrorFormat("Unrecognised DTD part \"{0}\"", name);
                                            Stream.Pushback(buffer.ToString());
                                            buffer = new StringBuilder();
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        if (!IsNameChar(nextChar))
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(buffer.ToString());
                                            buffer = new StringBuilder();
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.DTD2:
                        {
                            switch (nextChar)
                            {
                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        ParseState = State.DTD3;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.DTD3:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        ParseState = State.Initial;
                                        return new DoctypeToken(name, buffer.ToString());
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.PITarget:
                        {
                            switch (nextChar)
                            {
                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        name = buffer.ToString();
                                        buffer = new StringBuilder();
                                        ParseState = State.PIData;
                                    }
                                    break;

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.PIData:
                        {
                            switch (nextChar)
                            {
                                case '?':
                                    {
                                        ParseState = State.EndPI;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.EndPI:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        ParseState = State.Initial;
                                        return new ProcessingInstructionToken(name, buffer.ToString());
                                    }

                                default:
                                    {
                                        // Unknown transition path from this state.
                                        Log.Error(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Recover;
                                        if (++NumErrors >= MaxErrors)
                                            throw new MaxErrorsException();
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script1:
                        {
                            Log.DebugFormat("nextChar = {0}", nextChar);
                            switch (nextChar)
                            {
                                case '<':
                                    {
                                        ParseState = State.Script2;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script2:
                        {
                            switch (nextChar)
                            {
                                case '/':
                                    {
                                        ParseState = State.Script3;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append('<');
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script3:
                        {
                            switch (nextChar)
                            {
                                case 's':
                                    {
                                        ParseState = State.Script4;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script4:
                        {
                            switch (nextChar)
                            {
                                case 'c':
                                    {
                                        ParseState = State.Script5;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</s");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script5:
                        {
                            switch (nextChar)
                            {
                                case 'r':
                                    {
                                        ParseState = State.Script6;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</sc");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script6:
                        {
                            switch (nextChar)
                            {
                                case 'i':
                                    {
                                        ParseState = State.Script7;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</scr");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script7:
                        {
                            switch (nextChar)
                            {
                                case 'p':
                                    {
                                        ParseState = State.Script8;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</scri");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script8:
                        {
                            switch (nextChar)
                            {
                                case 't':
                                    {
                                        ParseState = State.Script9;
                                        break;
                                    }

                                default:
                                    {
                                        buffer.Append("</scrip");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Script9:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        Stream.Pushback("</script>");
                                        ParseState = State.Initial;
                                        return new ScriptToken(buffer.ToString());
                                    }

                                default:
                                    {
                                        buffer.Append("</script");
                                        buffer.Append(nextChar);
                                        ParseState = State.Script1;
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }

            // Warning if unprocessed content in buffer, or unexpected end-of-file.
            string leftovers = buffer.ToString();
            switch (ParseState)
            {
                case State.Initial:
                    {
                        if (leftovers.Length == 0) break;
                        return new WordToken(leftovers);
                    }

                case State.Number:
                    {
                        if (leftovers.Length == 0) break;
                        return new NumberToken(Convert.ToInt64(leftovers));
                    }

                case State.Spaces:
                    {
                        if (leftovers.Length == 0) break;
                        return new SpacesToken(leftovers);
                    }

                default:
                    {
                        NumWarnings++;
                        Log.Warn("Unexpected EOF");
                        break;
                    }
            }

            // EOF
            return new EOFToken();
        }

        /// <summary>
        /// This method returns a complete Tag instance.
        /// 
        /// Parsing continues from the point that this method was called until the
        /// end of the tag is found, or on error.
        /// </summary>
        /// <param name="name">Name of the tag instance.</param>
        /// <returns>Tag instance.</returns>
        protected Tag GetTag(string name)
        {
            Log.Debug("Entering getTag()");

            Tag tag = new Tag(name, _caseSensitive);
            StringBuilder attribute = new StringBuilder();
            StringBuilder value = new StringBuilder();

            ParseState = State.Tag;

            int c;
            while ((c = Stream.Read()) > 0)
            {
                char nextChar = (char)c;

                /*
                if (getState() == State.Recover) {
                    log.info("getTag() method returning due to error recovery mode");
                    return null;
                }
                */

                switch (ParseState)
                {
                    case State.Recover:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    return tag;
                            }
                            break;
                        }
                    case State.Tag:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    return tag;

                                case '/':
                                    {
                                        ParseState = State.EmptyElement2;
                                        break;
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                default:
                                    {
                                        attribute.Append(nextChar);
                                        if (IsNameChar(nextChar))
                                        {
                                            ParseState = State.Name1;
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(attribute.ToString());
                                            attribute = new StringBuilder();
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.EmptyElement2:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        return new EmptyElement(tag);
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                default:
                                    {
                                        // Unknown transition path from this state
                                        Log.Error(GetEdgeUnknownErrorMessage(nextChar, ParseState));
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Recover;
                                        if (++NumErrors >= MaxErrors)
                                        {
                                            throw new MaxErrorsException();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Name1:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        tag.AddAttribute(attribute.ToString());
                                        return tag;
                                    }

                                case '/':
                                    {
                                        ParseState = State.EmptyElement2;
                                        break;
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        ParseState = State.Name2;
                                        break;
                                    }

                                case '=':
                                    {
                                        ParseState = State.Value1;
                                        break;
                                    }

                                default:
                                    {
                                        attribute.Append(nextChar);
                                        if (!IsNameChar(nextChar))
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(attribute.ToString());
                                            attribute = new StringBuilder();
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Name2:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        tag.AddAttribute(attribute.ToString());
                                        return tag;
                                    }

                                case '/':
                                    {
                                        tag.AddAttribute(attribute.ToString());
                                        ParseState = State.EmptyElement2;
                                        break;
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                case '=':
                                    {
                                        ParseState = State.Value1;
                                        break;
                                    }

                                default:
                                    {
                                        tag.AddAttribute(attribute.ToString());
                                        if (IsNameChar(nextChar))
                                        {
                                            attribute = new StringBuilder();
                                            attribute.Append(nextChar); // New attribute.
                                            ParseState = State.Name1;

                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(nextChar);
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Value1:
                        {
                            switch (nextChar)
                            {
                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        // Ignore
                                        break;
                                    }

                                case '"':
                                    {
                                        ParseState = State.Quoted;
                                        break;
                                    }

                                case '\'':
                                    {
                                        ParseState = State.Comma;
                                        break;
                                    }

                                default:
                                    {
                                        value.Append(nextChar);
                                        if (IsNameChar(nextChar))
                                        {
                                            ParseState = State.Label;
                                        }
                                        else
                                        {
                                            Log.Error(GetInvalidCharErrorMessage(nextChar, ParseState));
                                            Stream.Pushback(value.ToString());
                                            value = new StringBuilder();
                                            ParseState = State.Recover;
                                            if (++NumErrors >= MaxErrors)
                                            {
                                                throw new MaxErrorsException();
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Value2:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        return tag;
                                    }

                                case '/':
                                    {
                                        ParseState = State.EmptyElement2;
                                        break;
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        ParseState = State.Tag;
                                        break;
                                    }

                                default:
                                    {
                                        Stream.Pushback(nextChar);
                                        ParseState = State.Tag;
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Label:
                        {
                            switch (nextChar)
                            {
                                case '>':
                                    {
                                        tag.AddAttribute(attribute.ToString(), value.ToString());
                                        return tag;
                                    }

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    {
                                        tag.AddAttribute(attribute.ToString(), value.ToString());
                                        attribute = new StringBuilder();
                                        value = new StringBuilder();
                                        ParseState = State.Tag;
                                        break;
                                    }

                                default:
                                    {
                                        value.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Quoted:
                        {
                            switch (nextChar)
                            {
                                case '"':
                                    {
                                        tag.AddAttribute(attribute.ToString(), value.ToString());
                                        attribute = new StringBuilder();
                                        value = new StringBuilder();
                                        ParseState = State.Value2;
                                        break;
                                    }

                                default:
                                    {
                                        value.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Comma:
                        {
                            switch (nextChar)
                            {
                                case '\'':
                                    {
                                        tag.AddAttribute(attribute.ToString(), value.ToString());
                                        attribute = new StringBuilder();
                                        value = new StringBuilder();
                                        ParseState = State.Value2;
                                        break;
                                    }

                                default:
                                    {
                                        value.Append(nextChar);
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }

            NumWarnings++;
            Log.Warn("Unexpected EOF");
            return null;
        }

        /// <summary>
        /// Decodes known entity.
        /// </summary>
        /// <param name="entity">Entity name.</param>
        /// <returns>Decoded entity, or null if entity unknown.</returns>
        private string DecodeEntity(string entity)
        {
            return _entities.ContainsKey(entity) ? _entities[entity] : null;
        }

        /// <summary>
        /// This checks if a the entity name is recognised.
        /// </summary>
        /// <param name="entity">Name of entity.</param>
        /// <returns>True if the entity name is recognised.</returns>
        private bool IsKnownEntity(string entity)
        {
            return DecodeEntity(entity) != null;
        }

        /// <summary>
        /// Character allowed as part of attribute or element name.
        /// </summary>
        /// <param name="c">Character to test.</param>
        /// <returns>True if the character is allowed for names.</returns>
        internal static bool IsNameFirstChar(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_' ||
                    c == ':';
        }

        /// <summary>
        /// Character allowed as part of attribute or element name.
        /// </summary>
        /// <param name="c">Character to test.</param>
        /// <returns>True if the character is allowed for names.</returns>
        internal static bool IsNameChar(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c >= '0' && c <= '9') ||
                    c == '-' ||
                    c == '_' ||
                    c == '.' ||
                    c == ':';
        }
    }
}
