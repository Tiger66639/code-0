// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueBotPropDecl.cs" company="">
//   
// </copyright>
// <summary>
//   Manages a bot's property that has a double, <see langword="int" /> or text
//   value. UI should bind to 'NeuronInfo.DisplayTitle'.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages a bot's property that has a double, <see langword="int" /> or text
    ///     value. UI should bind to 'NeuronInfo.DisplayTitle'.
    /// </summary>
    public class ValueBotPropDecl : BaseBotPropDecl
    {
        /// <summary>Writes the property's value to an XML stream.</summary>
        /// <param name="writer">The writer.</param>
        public override void WriteXmlValue(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, XmlElementName, NeuronInfo.DisplayTitle);
        }

        /// <summary>Reads the property's value from an XML stream.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXmlValue(System.Xml.XmlReader reader)
        {
            NeuronInfo.DisplayTitle = XmlStore.ReadElement<string>(reader, XmlElementName);
        }
    }
}