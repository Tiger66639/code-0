// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmallIDCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Provides xml streaming for an ObservedCollection(ulong) collection +
//   tracks changes in the network. Without it,
//   <see cref="JaStDev.HAB.Designer.BrainData.DefaultMeaningIds" /> doesn't
//   load properly. Is used all over the place, where a set of neurons needs
//   to be remembered, but updating is required.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides xml streaming for an ObservedCollection(ulong) collection +
    ///     tracks changes in the network. Without it,
    ///     <see cref="JaStDev.HAB.Designer.BrainData.DefaultMeaningIds" /> doesn't
    ///     load properly. Is used all over the place, where a set of neurons needs
    ///     to be remembered, but updating is required.
    /// </summary>
    public class SmallIDCollection : Data.ObservedCollection<ulong>, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     We keep a <see langword="ref" /> to the event monitor, in this object,
        ///     to keep it alive, because if there is no child item, the event monitor
        ///     is no where referenced in the event manager.
        /// </summary>
        private readonly SmallIDCollectionEventMonitor fEventMonitor;

        #region EventMonitor Members

        /// <summary>Removes all references to the specified id.</summary>
        /// <param name="id">The id.</param>
        internal void RemoveAll(ulong id)
        {
            var iIndex = IndexOf(id);
            while (iIndex > -1)
            {
                RemoveItemDirect(iIndex);
                iIndex = IndexOf(id);
            }
        }

        #endregion

        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="SmallIDCollection"/> class. Initializes a new instance of the <see cref="NeuronIDCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public SmallIDCollection(object owner)
            : base(owner)
        {
            fEventMonitor = new SmallIDCollectionEventMonitor(this);

                // don't need to add to the eventMananger, this is done when items are added to the list
        }

        /// <summary>Initializes a new instance of the <see cref="SmallIDCollection"/> class. Initializes a new instance of the <see cref="NeuronIDCollection"/>
        ///     class.</summary>
        /// <param name="items">The list to copy the initial items from.</param>
        public SmallIDCollection(System.Collections.Generic.List<ulong> items)
            : base(null, items)
        {
            fEventMonitor = new SmallIDCollectionEventMonitor(this);

                // don't need to add to the eventMananger, this is done when items are added to the list
        }

        /// <summary>Initializes a new instance of the <see cref="SmallIDCollection"/> class. 
        ///     Initializes a new instance of the <see cref="NeuronIDCollection"/>
        ///     class.</summary>
        public SmallIDCollection()
        {
            fEventMonitor = new SmallIDCollectionEventMonitor(this);
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ulong item)
        {
            var iIndex = IndexOf(item);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.AddItem(item);
            }

            base.InsertItem(index, item);
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iToRemove = this[index];
            base.RemoveItem(index);
            var iIndex = IndexOf(iToRemove);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.RemoveItem(iToRemove);
            }
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ulong item)
        {
            var iToRemove = this[index];
            var iIndex = IndexOf(item);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.AddItem(item);
            }

            base.SetItem(index, item);
            iIndex = IndexOf(iToRemove);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.RemoveItem(iToRemove);
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
                ulong iId;
                iId = XmlStore.ReadElement<ulong>(reader, "ID");
                Add(iId);
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var i in this)
            {
                XmlStore.WriteElement(writer, "ID", i);
            }
        }

        #endregion
    }
}