// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapNote.cs" company="">
//   
// </copyright>
// <summary>
//   An item that can be displayed on a <see cref="MindMap" /> and that
//   represents a small textual note.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An item that can be displayed on a <see cref="MindMap" /> and that
    ///     represents a small textual note.
    /// </summary>
    /// <remarks>
    ///     The contents of the note is declared with the
    ///     <see cref="JaStDev.HAB.Designer.MindMapItem.Description" /> property.
    /// </remarks>
    public class MindMapNote : PositionedMindMapItem
    {
        #region fields

        /// <summary>The f title.</summary>
        private string fTitle;

        #endregion

        /// <summary>Gets the description title.</summary>
        public override string DescriptionTitle
        {
            get
            {
                return "Note: " + Title;
            }
        }

        #region Title

        /// <summary>
        ///     Gets/sets the title of this note
        /// </summary>
        public string Title
        {
            get
            {
                return fTitle;
            }

            set
            {
                OnPropertyChanging("Title", fTitle, value);
                fTitle = value;
                OnPropertyChanged("Title");
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>Reads the content of the XML.</summary>
        /// <param name="reader">The reader.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            Height = XmlStore.ReadElement<double>(reader, "Height");
            Width = XmlStore.ReadElement<double>(reader, "Width");
            X = XmlStore.ReadElement<double>(reader, "X");
            Y = XmlStore.ReadElement<double>(reader, "Y");
            Title = XmlStore.ReadElement<string>(reader, "Title");

            if (reader.IsEmptyElement == false)
            {
                DescriptionText = reader.ReadOuterXml();
            }
            else
            {
                reader.ReadStartElement("FlowDocument");
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Height", Height);
            XmlStore.WriteElement(writer, "Width", Width);
            XmlStore.WriteElement(writer, "X", X);
            XmlStore.WriteElement(writer, "Y", Y);
            XmlStore.WriteElement(writer, "Title", Title);
            if (DescriptionText != null)
            {
                writer.WriteRaw(DescriptionText);
            }
            else
            {
                writer.WriteStartElement("FlowDocument");
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}