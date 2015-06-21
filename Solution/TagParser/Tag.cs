using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace TagParser
{
    /// <summary>
    /// This class is used to store instances of tags found within a parsed document.
    /// </summary>
    public class Tag
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _name;
        private readonly bool _caseSensitive;

        public readonly Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();

        /// <summary>
        /// This is a constructor for the class: Tag
        /// </summary>
        /// <param name="name">Name of this tag. This is an end-tag if the name begins '/'.</param>
        /// <param name="caseSensitive">True if tag and attribute names are case-sensitive.</param>
        public Tag(string name, bool caseSensitive)
        {
            if (!caseSensitive) name = name.ToLower();
            _caseSensitive = caseSensitive;
            _name = name;
        }

        /// <summary>
        /// Validate the attribute name.
        /// </summary>
        /// <param name="name">Attribute or tag name to validate.</param>
        /// <returns>True if the name is valid.</returns>
        private static bool IsValidName(string name)
        {
            /*
            Valid tag and attribute names.
            http://www.w3.org/TR/REC-xml/#NT-NameStartChar

            NameStartChar  ::= ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
            NameChar       ::= NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
            */

            // First char can be a letter or '_' or ':'.
            // Following chars can include numbers, '-', '.' and some others that might be unicode.
            // See: http://www.w3.org/TR/2000/REC-xml-20001006#NT-Name

            // Check the first character here.
            if (!TagParser.IsNameFirstChar(name[0]))
            {
                Log.ErrorFormat("Attribute ignored due to invalid name char: {0}", name[0]);
                return false;
            }

            // Check the other characters in the name.
            for (int i = 1; i < name.Length; i++)
            {
                if (!TagParser.IsNameFirstChar(name[i]))
                {
                    Log.ErrorFormat("Attribute ignored due to invalid name char: {0}", name[i]);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Name of the tag.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// True if case-sensitive option on.
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return _caseSensitive; }
        }

        /// <summary>
        /// True if this is an end tag.
        /// </summary>
        public bool IsEndTag
        {
            get { return _name[0] == '/'; }
        }

        /// <summary>
        /// This method adds an attribute to this tag.
        /// If the attribute already exists then set this value for the attribute.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Optional value associated the attribute.</param>
        public void AddAttribute(string name, string value)
        {
            if (!IsValidName(name)) return;
            if (!_caseSensitive) name = name.ToLower();

            // Make sure the attribute value doesn't contain invalid characters.
            if (value != null)
            {
                value = value.Replace("&amp;", "&");
                value = value.Replace("&quot;", "\"");
                value = value.Replace("&lt;", "<");
                value = value.Replace("&gt;", ">");
                value = value.Replace("&", "&amp;");
                value = value.Replace("\"", "&quot;");
                value = value.Replace("<", "&lt;");
                value = value.Replace(">", "&gt;");
            }

            Attribute attrib;
            if (!Attributes.ContainsKey(name))
            {
                attrib = new Attribute(name, value);
                Attributes.Add(name, attrib);
            }
            else
            {
                attrib = Attributes[name];
                attrib.Value = value;
            }
        }

        /// <summary>
        /// This method adds a attribute to this tag.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        public void AddAttribute(string name)
        {
            AddAttribute(name, null);
        }

        /// <summary>
        /// This method returns the value of a named attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute class instance.</returns>
        public Attribute GetAttribute(string name)
        {
            if (!_caseSensitive) name = name.ToLower();
            return Attributes[name];
        }

        /// <summary>
        /// This method returns the value of a named attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>String value of the attribute.</returns>
        public string GetAttributeValue(string name)
        {
            if (!_caseSensitive) name = name.ToLower();
            Attribute attrib = Attributes[name];
            return attrib == null ? null : attrib.Value;
        }

        /// <summary>
        /// This method returns a list of the attributes.
        /// </summary>
        /// <returns>List of attributes.</returns>
        public List<Attribute> GetAttributes()
        {
            return Attributes.Values.ToList();
        }

        /// <summary>
        /// This method writes the tag to a string, also supporting the ToString()
        /// methods of the extending classes EmptyElement and DummyElement.
        /// </summary>
        /// <param name="isEmptyElement">True if this start-tag is to be closed.</param>
        /// <param name="comment">An optional comment to be enclosed in the element.</param>
        /// <returns>String representation.</returns>
        protected string ToString(bool isEmptyElement, string comment)
        {
            // Check arguments.
            if (!isEmptyElement && comment != null)
                throw new ArgumentException("Non-element can't have comment.");

            // Result string builder.
            StringBuilder result = new StringBuilder();

            // Open start-tag.
            result.Append('<');

            // Tag or element name.
            result.Append(_name);

            // Attributes
            if (Attributes != null)
            {
                foreach (string attributeName in Attributes.Keys)
                {
                    Attribute attrib = GetAttribute(attributeName);
                    if (attrib == null) continue;

                    // Attribute name.
                    result.Append(' ');
                    result.Append(attributeName);

                    // Attribute value.
                    if (attrib.Value == null)
                    {
                        result.Append("=\"\"");
                    }
                    else
                    {
                        result.Append('=');
                        result.Append('"');
                        result.Append(escapeAttribute(attrib.Value));
                        result.Append('"');
                    }
                }
            }

            // Close start-tag or element.
            if (!isEmptyElement)
            {
                result.Append('>');
            }
            else
            {
                if (comment == null)
                {
                    //TODO: Check if the empty element syntax is allowed.
                    result.Append("/>");
                }
                else
                {
                    result.Append("><!--").Append(comment).Append("-->");
                    result.Append("</").Append(_name).Append('>');
                }
            }

            // Return result as string.
            return result.ToString();
        }

        /// <summary>
        /// This method is used to ensure that '&' are not misused in attribute values.
        /// </summary>
        /// <param name="value">Attribute value to process.</param>
        /// <returns>Attribute value with escaped '&' as may be required.</returns>
        private string escapeAttribute(string value)
        {
            if (value != null)
            {
                value = value.Replace("&", "&amp;");
                value = value.Replace("&amp;amp;", "&amp;");
            }
            else
            {
                value = "";
            }
            return value;
        }

        /// <summary>
        /// This method returns this tag as a string.
        /// </summary>
        /// <returns>Tag string.</returns>
        public new string ToString()
        {
            return ToString(false, null);
        }
    }
}
