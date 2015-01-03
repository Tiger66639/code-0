// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlObjectStore.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for objects that need to be stored in xml with the exact
//   type specified.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for objects that need to be stored in xml with the exact
    ///     type specified.
    /// </summary>
    public class XmlObjectStore : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     The value.
        /// </summary>
        public object Data { get; set; }

        #region IXmlSerializable Members

        /// <summary>The get schema.</summary>
        /// <returns>The <see cref="XmlSchema"/>.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            reader.ReadStartElement("Type");
            var iFound = reader.ReadString();
            var iType = Brain.Current.GetNeuronType(iFound);
            reader.ReadEndElement();

            var valueSerializer = new System.Xml.Serialization.XmlSerializer(iType);
            Data = valueSerializer.Deserialize(reader);

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Type");
            writer.WriteString(Data.GetType().ToString());
            writer.WriteEndElement();

            var valueSerializer = new System.Xml.Serialization.XmlSerializer(Data.GetType());
            valueSerializer.Serialize(writer, Data);
        }

        #endregion
    }
}