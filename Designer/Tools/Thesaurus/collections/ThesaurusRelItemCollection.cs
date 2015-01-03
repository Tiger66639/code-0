// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusRelItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Contains neuron wrappers that represent thesaurus relationships.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains neuron wrappers that represent thesaurus relationships.
    /// </summary>
    /// <remarks>
    ///     provides also support for a 'null' element, which is a thesaurusRelItem
    ///     that doesn't wrap a neuron.
    /// </remarks>
    public class ThesaurusRelItemCollection : System.Collections.ObjectModel.ObservableCollection<ThesaurusRelItem>, 
                                              System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f event monitor.</summary>
        private readonly ThesaurusRelItemCollectionEventMonitor fEventMonitor;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ThesaurusRelItemCollection"/> class. 
        ///     Initializes a new instance of the<see cref="ThesaursItemCollection"/> class.</summary>
        public ThesaurusRelItemCollection()
        {
            fEventMonitor = new ThesaurusRelItemCollectionEventMonitor(this);
        }

        #endregion

        /// <summary>Removes all.</summary>
        /// <param name="item">The item.</param>
        public void RemoveAll(Neuron item)
        {
            var i = 0;
            while (i < Count)
            {
                if (this[i].Item == item)
                {
                    RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>Replaces the specified neuron with the new one in every<see cref="ThesaurusRelItem"/> of the collection.</summary>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        public void Replace(Neuron old, Neuron value)
        {
            foreach (var i in this)
            {
                if (i.Item == old)
                {
                    i.Item = value;
                }
            }
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>Inserts an <paramref name="item"/> into the collection at the
        ///     specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, ThesaurusRelItem item)
        {
            base.InsertItem(index, item);
            if (item.Item != null)
            {
                fEventMonitor.AddItem(item.Item.ID);
            }
        }

        /// <summary>Removes the item at the specified <paramref name="index"/> of the
        ///     collection.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index].Item;
            if (iItem != null && Neuron.IsEmpty(iItem.ID) == false)
            {
                // only try to remove if it isn't already.
                fEventMonitor.RemoveItem(this[index].Item.ID);
            }

            base.RemoveItem(index);
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, ThesaurusRelItem item)
        {
            var iItem = this[index].Item;
            if (iItem != null && Neuron.IsEmpty(iItem.ID) == false)
            {
                // only try to remove if it isn't already.
                fEventMonitor.RemoveItem(this[index].Item.ID);
            }

            base.SetItem(index, item);
            if (item.Item != null)
            {
                fEventMonitor.AddItem(item.Item.ID);
            }
        }

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

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("ID");
                ulong iID;
                var iVal = reader.ReadString();
                if (iVal != "null")
                {
                    iID = ulong.Parse(iVal);
                }
                else
                {
                    iID = Neuron.EmptyId;
                }

                if (iID != Neuron.EmptyId)
                {
                    var iFound = Brain.Current[iID];
                    if (iFound != null)
                    {
                        Add(new ThesaurusRelItem(iFound));

                            // we use the regular add, so that the event monitoring system gets warned when loaded.
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ThesaurusRelItemCollection.ReadXml", 
                            string.Format("Neuron with ID {0} not found in brain.", iID));
                    }
                }
                else
                {
                    Add(new ThesaurusRelItem(null));
                }

                reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var i in this)
            {
                writer.WriteStartElement("ID");
                if (i.Item != null)
                {
                    writer.WriteString(i.Item.ID.ToString());
                }
                else
                {
                    writer.WriteString("null");
                }

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}