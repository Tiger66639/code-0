// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayText.cs" company="">
//   
// </copyright>
// <summary>
//   A class that stores information regarding an overlay that is displayed on
//   top of images in order to indicate the presence of a link on a neuron.
//   This is used to give a visual queue that a flow item has code attached or
//   also on explorer items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A class that stores information regarding an overlay that is displayed on
    ///     top of images in order to indicate the presence of a link on a neuron.
    ///     This is used to give a visual queue that a flow item has code attached or
    ///     also on explorer items.
    /// </summary>
    public class OverlayText : Data.ObservableObject, System.Xml.Serialization.IXmlSerializable
    {
        #region fields

        /// <summary>The f text.</summary>
        private string fText;

        /// <summary>The f tooltip.</summary>
        private string fTooltip;

        /// <summary>The f overlay color.</summary>
        private System.Windows.Media.Color fOverlayColor;

        /// <summary>The f item id.</summary>
        private ulong fItemID;

        #endregion

        #region prop

        #region Text

        /// <summary>
        ///     Gets/sets the short text to display on top of the image of neurons.
        /// </summary>
        public string Text
        {
            get
            {
                return fText;
            }

            set
            {
                fText = value;
                OnPropertyChanged("Text");
            }
        }

        #endregion

        #region Tooltip

        /// <summary>
        ///     Gets/sets the text that should be used to display in the tooltip,
        ///     which shows a bit more info about the overlay.
        /// </summary>
        public string Tooltip
        {
            get
            {
                return fTooltip;
            }

            set
            {
                fTooltip = value;
                OnPropertyChanged("Tooltip");
            }
        }

        #endregion

        #region Foreground

        /// <summary>
        ///     Gets/sets the the color to use for the text, as a brush. This is used
        ///     to bind against from xaml.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Windows.Media.Brush Foreground
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(OverlayColor);
            }
        }

        #endregion

        #region OverlayColor

        /// <summary>
        ///     Gets/sets the color to use for the text.
        /// </summary>
        public System.Windows.Media.Color OverlayColor
        {
            get
            {
                return fOverlayColor;
            }

            set
            {
                fOverlayColor = value;
                OnPropertyChanged("OverlayColor");
                OnPropertyChanged("Foreground");
            }
        }

        #endregion

        #region ItemID

        /// <summary>
        ///     Gets/sets the id of the neuron that should be used as the meaning of a
        ///     link in order to display the overlay.
        /// </summary>
        public ulong ItemID
        {
            get
            {
                return fItemID;
            }

            set
            {
                fItemID = value;
                OnPropertyChanged("ItemID");
            }
        }

        #endregion

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            Text = XmlStore.ReadElement<string>(reader, "Text");
            Tooltip = XmlStore.ReadElement<string>(reader, "Tooltip");

            reader.ReadStartElement("OverlayColor");
            if (reader.Name == "A")
            {
                // this is to capture an old format that wrote colors using A,R,G,B elements.
                OverlayColor = ReadColor(reader);
            }
            else
            {
                OverlayColor =
                    (System.Windows.Media.Color)
                    System.Windows.Media.ColorConverter.ConvertFromString(reader.ReadString());
            }

            reader.ReadEndElement();

            ItemID = XmlStore.ReadElement<ulong>(reader, "ItemID");

            reader.ReadEndElement();
        }

        /// <summary>The read color.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Color"/>.</returns>
        private System.Windows.Media.Color ReadColor(System.Xml.XmlReader reader)
        {
            var iCol = new System.Windows.Media.Color();

            iCol.A = XmlStore.ReadElement<byte>(reader, "A");
            iCol.R = XmlStore.ReadElement<byte>(reader, "R");
            iCol.G = XmlStore.ReadElement<byte>(reader, "G");
            iCol.B = XmlStore.ReadElement<byte>(reader, "B");
            iCol.ScA = XmlStore.ReadElement<float>(reader, "ScA");
            iCol.ScR = XmlStore.ReadElement<float>(reader, "ScR");
            iCol.ScG = XmlStore.ReadElement<float>(reader, "ScG");
            iCol.ScB = XmlStore.ReadElement<float>(reader, "ScB");
            return iCol;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Text", Text);
            XmlStore.WriteElement(writer, "Tooltip", Tooltip);

            XmlStore.WriteElement(writer, "OverlayColor", OverlayColor.ToString());

            XmlStore.WriteElement(writer, "ItemID", ItemID);
        }

        #endregion
    }
}