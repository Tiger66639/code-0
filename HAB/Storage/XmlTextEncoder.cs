// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlTextEncoder.cs" company="">
//   
// </copyright>
// <summary>
//   Encodes data so that it can be safely embedded as text in XML documents.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Encodes data so that it can be safely embedded as text in XML documents.
    /// </summary>
    public class XmlTextEncoder : System.IO.TextReader
    {
        /// <summary>The entities.</summary>
        private static readonly System.Collections.Generic.Dictionary<char, string> Entities =
            new System.Collections.Generic.Dictionary<char, string>
                {
                    { '"', "&quot;" }, 
                    { '&', "&amp;" }, 
                    { '\'', "&apos;" }, 
                    { '<', "&lt;" }, 
                    { '>', "&gt;" }
                };

        /// <summary>The _buf.</summary>
        private readonly System.Collections.Generic.Queue<char> _buf = new System.Collections.Generic.Queue<char>();

        /// <summary>The _filter illegal chars.</summary>
        private readonly bool _filterIllegalChars;

        /// <summary>The _source.</summary>
        private readonly System.IO.TextReader _source;

        /// <summary>Initializes a new instance of the <see cref="XmlTextEncoder"/> class. The xml text encoder.</summary>
        /// <param name="source">The data to be encoded in UTF-16 format.</param>
        /// <param name="filterIllegalChars">It is illegal to encode certain characters in XML. If true, silently
        ///     omit these characters from the output; if false,<see langword="throw"/> an error when encountered.</param>
        public XmlTextEncoder(System.IO.TextReader source, bool filterIllegalChars = true)
        {
            _source = source;
            _filterIllegalChars = filterIllegalChars;
        }

        /// <summary>The encode.</summary>
        /// <param name="s">The s.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string Encode(string s)
        {
            using (var stream = new System.IO.StringReader(s)) using (var encoder = new XmlTextEncoder(stream)) return encoder.ReadToEnd();
        }

        /// <summary>The peek.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        public override int Peek()
        {
            PopulateBuffer();
            if (_buf.Count == 0)
            {
                return -1;
            }

            return _buf.Peek();
        }

        /// <summary>The read.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        public override int Read()
        {
            PopulateBuffer();
            if (_buf.Count == 0)
            {
                return -1;
            }

            return _buf.Dequeue();
        }

        /// <summary>The populate buffer.</summary>
        /// <exception cref="ArgumentException"></exception>
        private void PopulateBuffer()
        {
            const int endSentinel = -1;
            while (_buf.Count == 0 && _source.Peek() != endSentinel)
            {
                // Strings in .NET are assumed to be UTF-16 encoded [1].
                var c = (char)_source.Read();
                if (Entities.ContainsKey(c))
                {
                    // Encode all entities defined in the XML spec [2].
                    foreach (var i in Entities[c])
                    {
                        _buf.Enqueue(i);
                    }
                }
                else if (!(0x0 <= c && c <= 0x8) && !Enumerable.Contains(new[] { 0xB, 0xC }, c)
                         && !(0xE <= c && c <= 0x1F) && !(0x7F <= c && c <= 0x84) && !(0x86 <= c && c <= 0x9F)
                         && !(0xD800 <= c && c <= 0xDFFF) && !Enumerable.Contains(new[] { 0xFFFE, 0xFFFF }, c))
                {
                    // Allow if the Unicode codepoint is legal in XML [3].
                    _buf.Enqueue(c);
                }
                else if (char.IsHighSurrogate(c) && _source.Peek() != endSentinel
                         && char.IsLowSurrogate((char)_source.Peek()))
                {
                    // Allow well-formed surrogate pairs [1].
                    _buf.Enqueue(c);
                    _buf.Enqueue((char)_source.Read());
                }
                else if (!_filterIllegalChars)
                {
                    // Note that we cannot encode illegal characters as entity
                    // references due to the "Legal Character" constraint of
                    // XML [4]. Nor are they allowed in CDATA sections [5].
                    throw new System.ArgumentException(
                        string.Format("Illegal character: '{0:X}'", (int)c));
                }
            }
        }
    }
}