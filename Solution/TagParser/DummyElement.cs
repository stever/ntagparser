using System;

namespace TagParser
{
    public class DummyElement : Tag
    {
        // Example: <BR><!-- inserted missing start-tag --></BR>

        public string Comment { get; set; }

        /// <summary>
        /// Constructor for the DummyElement class.
        /// </summary>
        /// <param name="endTag">This must be an end-tag (tag name beginning '/').</param>
        public DummyElement(Tag endTag)
            : base(endTag.Name.Substring(1), endTag.IsCaseSensitive)
        {
            // Ensure provided tag is an end-tag.        
            if (!endTag.IsEndTag)
            {
                throw new ArgumentException("End-tag must be provided to DummyElement class constructor!");
            }

            // Copy attributes.
            foreach (var attrib in endTag.Attributes)
            {
                Attributes.Add(attrib.Key, attrib.Value);
            }
        }

        /// <summary>
        /// This method returns this dummy element as a string.
        /// </summary>
        /// <returns>Tag string.</returns>
        public new string ToString()
        {
            return ToString(true, Comment);
        }
    }
}
