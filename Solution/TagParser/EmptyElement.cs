using System;

namespace TagParser
{
    public class EmptyElement : Tag
    {
        // Example: <BR/>

        public EmptyElement(Tag tag)
            : base(tag.Name, tag.IsCaseSensitive)
        {
            // Ensure provided tag is not an end-tag.
            if (IsEndTag)
                throw new ArgumentException("End-tag cannot be provided to EmptyElement class constructor!");

            // Copy attributes.
            foreach (var attrib in tag.Attributes)
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
            return ToString(true, null);
        }
    }
}
