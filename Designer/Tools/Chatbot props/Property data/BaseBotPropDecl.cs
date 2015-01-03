// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseBotPropDecl.cs" company="">
//   
// </copyright>
// <summary>
//   the base class for objects that declare a bot property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     the base class for objects that declare a bot property.
    /// </summary>
    public abstract class BaseBotPropDecl : Data.ObservableObject, 
                                            INeuronWrapper, 
                                            INeuronInfo, 
                                            System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f title.</summary>
        private string fTitle;

        /// <summary>The f tool tip.</summary>
        private string fToolTip;

        /// <summary>The f xml element name.</summary>
        private string fXmlElementName;

        #region ToolTip

        /// <summary>
        ///     Gets/sets the tooltip that should be used for this property.
        /// </summary>
        public string ToolTip
        {
            get
            {
                return fToolTip;
            }

            set
            {
                fToolTip = value;
                OnPropertyChanged("ToolTip");
            }
        }

        #endregion

        #region Title

        /// <summary>
        ///     Gets/sets the display title to use for this property.
        /// </summary>
        public string Title
        {
            get
            {
                return fTitle;
            }

            set
            {
                fTitle = value;
                OnPropertyChanged("Title");
            }
        }

        #endregion

        #region XmlElementName

        /// <summary>
        ///     Gets/sets the name of the element that should be used for
        ///     reading/writing the value(s) to/from an xml file
        /// </summary>
        public string XmlElementName
        {
            get
            {
                return fXmlElementName;
            }

            set
            {
                fXmlElementName = value;
                OnPropertyChanged("XmlElementName");
            }
        }

        #endregion

        /// <summary>Gets the category.</summary>
        public string Category
        {
            get
            {
                return NeuronInfo.Category;
            }
        }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo { get; private set; }

        #endregion

        /// <summary>Creates a list of botprop decls from the appropriate type (as defined
        ///     in the extra info of the neuronData).</summary>
        /// <param name="items">The items to convert into botpropDecl objects.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<BaseBotPropDecl> CreateFor(System.Collections.Generic.IEnumerable<ulong> items)
        {
            var iRes = new System.Collections.Generic.List<BaseBotPropDecl>();
            foreach (var i in items)
            {
                try
                {
                    var iItem = Brain.Current[i];
                    var iInfo = BrainData.Current.NeuronInfo[iItem];
                    if (iInfo.CustomData != null)
                    {
                        iInfo.CustomData.Position = 0;
                        var iBinReader = new System.IO.BinaryReader(iInfo.CustomData);
                        var iCustomData = iBinReader.ReadString(); // get the xml data out of the custom data field.
                        if (string.IsNullOrEmpty(iCustomData) == false)
                        {
                            BaseBotPropDecl iData;

                            var iSettings = new System.Xml.XmlReaderSettings();
                            iSettings.IgnoreComments = true;
                            iSettings.IgnoreProcessingInstructions = true;
                            iSettings.IgnoreWhitespace = false;
                            using (
                                System.IO.TextReader iInput =
                                    new System.IO.StringReader("<?xml version='1.0' encoding='utf-8'?>" + iCustomData))
                            {
                                // there is no xml header, just an element.
                                using (var iReader = System.Xml.XmlReader.Create(iInput, iSettings))
                                {
                                    if (iReader.IsStartElement())
                                    {
                                        iData = GetObject(iReader.Name.ToLower());
                                        iReader.ReadStartElement();
                                        XmlStore.SkipSpaces(iReader);
                                        if (iData != null)
                                        {
                                            iData.SetItemInfo(iItem, iInfo);
                                            iData.ReadXml(iReader);
                                            iRes.Add(iData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("project properties", e.ToString());
                }
            }

            return iRes;
        }

        /// <summary>checks the <paramref name="name"/> and creates the appropriate object.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="BaseBotPropDecl"/>.</returns>
        private static BaseBotPropDecl GetObject(string name)
        {
            switch (name)
            {
                case "boolprop":
                    return new BoolBotPropDecl();
                case "listprop":
                    return new ListBotPropDecl();
                case "valueprop":
                    return new ValueBotPropDecl();
            }

            LogService.Log.LogError("Properties", "Invalid Module property type detected: " + name);
            return null;
        }

        /// <summary>Writes the property's value to an XML stream.</summary>
        /// <param name="writer">The writer.</param>
        public abstract void WriteXmlValue(System.Xml.XmlWriter writer);

        /// <summary>Reads the property's value from an XML stream.</summary>
        /// <param name="reader">The reader.</param>
        public abstract void ReadXmlValue(System.Xml.XmlReader reader);

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item { get; private set; }

        /// <summary>Sets the <paramref name="item"/> and <paramref name="info"/></summary>
        /// <param name="item">The item.</param>
        /// <param name="info">The info.</param>
        private void SetItemInfo(Neuron item, NeuronData info)
        {
            Item = item;
            NeuronInfo = info;
        }

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
            Title = XmlStore.ReadElNoCase<string>(reader, "title");
            XmlStore.SkipSpaces(reader);
            ToolTip = XmlStore.ReadElNoCase<string>(reader, "tooltip");
            XmlStore.SkipSpaces(reader);
            XmlElementName = XmlStore.ReadElNoCase<string>(reader, "name");
            XmlStore.SkipSpaces(reader);
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Title", fTitle);
            XmlStore.WriteElement(writer, "Tooltip", fToolTip);
            XmlStore.WriteElement(writer, "Name", fXmlElementName);
        }

        #endregion
    }
}