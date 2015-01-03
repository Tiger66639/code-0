// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItemDragData.cs" company="">
//   
// </copyright>
// <summary>
//   contains all the data required to create a thesaurus item from a wordnet
//   item in a drag operation. This class allows for easy streaming to and
//   from xml during the drag operation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     contains all the data required to create a thesaurus item from a wordnet
    ///     item in a drag operation. This class allows for easy streaming to and
    ///     from xml during the drag operation.
    /// </summary>
    public class WordNetItemDragData
    {
        /// <summary>Initializes a new instance of the <see cref="WordNetItemDragData"/> class. 
        ///     default constructor for xml streaming.</summary>
        public WordNetItemDragData()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WordNetItemDragData"/> class. Initializes a new instance of the <see cref="WordNetItemDragData"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        public WordNetItemDragData(WordNetItem item)
        {
            Description = item.Description;
            POS = item.POS;
            Synonyms = item.Synonyms;
        }

        /// <summary>Gets or sets the pos.</summary>
        public string POS { get; set; }

        /// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the synonyms.</summary>
        public System.Collections.Generic.List<string> Synonyms { get; set; }

        /// <summary>Gets the content of this object as an xml formmatted string.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetXml()
        {
            using (var iStr = new System.IO.MemoryStream())
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(WordNetItemDragData));
                using (System.IO.TextWriter iWriter = new System.IO.StreamWriter(iStr))
                {
                    iSer.Serialize(iWriter, this);
                    iStr.Position = 0;

                        // make certain we are back at the start for reading out the string and returning the result
                    var reader = new System.IO.StreamReader(iStr);
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>converts the input string back to a <paramref name="data"/> object.</summary>
        /// <param name="data"></param>
        /// <returns>The <see cref="WordNetItemDragData"/>.</returns>
        internal static WordNetItemDragData ParseXml(string data)
        {
            using (var iReader = new System.IO.StringReader(data))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(WordNetItemDragData));
                return (WordNetItemDragData)iSer.Deserialize(iReader);
            }
        }
    }
}