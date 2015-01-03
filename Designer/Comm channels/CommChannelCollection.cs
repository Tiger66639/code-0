// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommChannelCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of communication channels.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection of communication channels.
    /// </summary>
    /// <remarks>
    ///     Monitors the brain for changes in the neurons, so that objects may be
    ///     removed when they no longer are sins.
    /// </remarks>
    public class CommChannelCollection : Data.ObservedCollection<CommChannel>, System.Xml.Serialization.IXmlSerializable
    {
        #region fields

        /// <summary>The f event monitor.</summary>
        private readonly CommChannelCollectionEventMonitor fEventMonitor;

        #endregion

        #region EventMonitor

        /// <summary>The remove all.</summary>
        /// <param name="item">The item.</param>
        internal void RemoveAll(Neuron item)
        {
            var iFound = (from i in this where i.Item == item select i).ToList();

                // to delete, we need to convert to a new list
            foreach (var i in iFound)
            {
                RemoveItemDirect(IndexOf(i)); // can't raise events during callback.
            }
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CommChannelCollection"/> class. 
        ///     Initializes a new instance of the <see cref="CommChannelCollection"/>
        ///     class.</summary>
        public CommChannelCollection()
        {
            fEventMonitor = new CommChannelCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="CommChannelCollection"/> class. Initializes a new instance of the <see cref="CommChannelCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public CommChannelCollection(BrainData owner)
            : base(owner)
        {
            fEventMonitor = new CommChannelCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="CommChannelCollection"/> class. Initializes a new instance of the <see cref="CommChannelCollection"/>
        ///     class.</summary>
        /// <param name="monitorChanges">if set to <c>false</c> no changes are monitored, which is used for
        ///     exporting modules.</param>
        public CommChannelCollection(bool monitorChanges)
        {
            if (monitorChanges)
            {
                fEventMonitor = new CommChannelCollectionEventMonitor(this);
            }
        }

        #endregion

        #region Overrides

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            if (fEventMonitor != null)
            {
                fEventMonitor.Clear();
            }

            base.ClearItems();
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, CommChannel item)
        {
            if (fEventMonitor != null)
            {
                var iIndex = IndexOf(item);
                if (iIndex == -1)
                {
                    // we only monitor unique numbers, so check for this.
                    fEventMonitor.AddItem(item.NeuronID);
                }
            }

            base.InsertItem(index, item);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iToRemove = this[index];
            base.RemoveItem(index);
            if (fEventMonitor != null)
            {
                fEventMonitor.RemoveItem(iToRemove.NeuronID);
            }
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, CommChannel item)
        {
            if (fEventMonitor != null)
            {
                var iToRemove = this[index];
                fEventMonitor.RemoveItem(iToRemove.NeuronID);
                base.SetItem(index, item);
                fEventMonitor.AddItem(item.NeuronID);
            }
            else
            {
                base.SetItem(index, item);
            }
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

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var iName = reader.Name;
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != iName && reader.EOF == false)
            {
                if (ReadXmlContent(reader) == false)
                {
                    // if for some reason, we failed to read the item, log an error, and advance to the next item so that we don't get in a loop.
                    LogService.Log.LogError(
                        "CommChannelCollection.ReadXml", 
                        string.Format("Failed to read xml element {0} in stream.", reader.Name));
                    reader.Skip();
                }
            }

            if (reader.EOF == false)
            {
                reader.ReadEndElement();
            }
        }

        /// <summary>Reads the content of the XML.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ReadXmlContent(System.Xml.XmlReader reader)
        {
            var iName = reader.Name;
            try
            {
                var iNode = ReadNode(reader, iName);
                if (iNode != null)
                {
                    Add(iNode);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("CommChannelCollection.ReadXml", e.ToString());
                reader.MoveToContent(); // try to recover the file position?
                while (reader.Name != iName)
                {
                    reader.Skip();
                }

                reader.ReadEndElement();
            }

            return false;
        }

        /// <summary>The read node.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iName">The i name.</param>
        /// <returns>The <see cref="CommChannel"/>.</returns>
        private CommChannel ReadNode(System.Xml.XmlReader reader, string iName)
        {
            CommChannel iNode = null;
            if (iName == "TextChannel")
            {
                iNode = new TextChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "AudioChannel")
            {
                iNode = new AudioChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "ImageChannel")
            {
                iNode = new ImageChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "ReflectionChannel")
            {
                iNode = new ReflectionChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "WordNetChannel")
            {
                iNode = new WordNetChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "ChatBotChannel")
            {
                iNode = new ChatBotChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "GridChannel")
            {
                iNode = new GridChannel();
                iNode.ReadXml(reader);
            }
            else if (iName == "CommChannel")
            {
                reader.MoveToAttribute("xsi:type");
                reader.ReadAttributeValue(); // the xsi:type should specify the object type to read.
                iName = reader.Value;
                reader.MoveToElement();
                iNode = ReadNode(reader, iName);
            }

            return iNode;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var iTextSer = new System.Xml.Serialization.XmlSerializer(typeof(TextChannel));
            var iAudioSer = new System.Xml.Serialization.XmlSerializer(typeof(AudioChannel));
            var iImageSer = new System.Xml.Serialization.XmlSerializer(typeof(ImageChannel));
            var iReflectionSer = new System.Xml.Serialization.XmlSerializer(typeof(ReflectionChannel));
            var iWordNetSer = new System.Xml.Serialization.XmlSerializer(typeof(WordNetChannel));
            var iChatbotSer = new System.Xml.Serialization.XmlSerializer(typeof(ChatBotChannel));
            var iGridSer = new System.Xml.Serialization.XmlSerializer(typeof(GridChannel));
            foreach (var item in this)
            {
                if (item is TextChannel)
                {
                    iTextSer.Serialize(writer, item);
                }
                else if (item is AudioChannel)
                {
                    iAudioSer.Serialize(writer, item);
                }
                else if (item is ImageChannel)
                {
                    iImageSer.Serialize(writer, item);
                }
                else if (item is WordNetChannel)
                {
                    iWordNetSer.Serialize(writer, item);
                }
                else if (item is ReflectionChannel)
                {
                    iReflectionSer.Serialize(writer, item);
                }
                else if (item is ChatBotChannel)
                {
                    iChatbotSer.Serialize(writer, item);
                }
                else if (item is GridChannel)
                {
                    iGridSer.Serialize(writer, item);
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }
        }

        #endregion
    }
}