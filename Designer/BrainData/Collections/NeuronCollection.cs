// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A custom observable collection for neurons that provides streaming
//   capabilities by saving the id's of the neurons, not the actual object.
//   Also provides functionality to monitor changes in the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>A custom observable collection for neurons that provides streaming
    ///     capabilities by saving the id's of the neurons, not the actual object.
    ///     Also provides functionality to monitor changes in the brain.</summary>
    /// <typeparam name="T">The actual specific neuron type.</typeparam>
    public class NeuronCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, 
                                       System.Xml.Serialization.IXmlSerializable
        where T : Neuron
    {
        #region fields

        /// <summary>
        ///     We keep a <see langword="ref" /> to the event monitor, in this object,
        ///     to keep it alive, because if there is no child item, the event monitor
        ///     is no where referenced in the event manager.
        /// </summary>
        private readonly NeuronCollectionEventMonitor<T> fEventMonitor;

        #endregion

        #region ctor/dtor

        /// <summary>Initializes a new instance of the <see cref="NeuronCollection{T}"/> class. 
        ///     Initializes a new instance of the<see cref="JaStDev.HAB.Designer.NeuronCollection`1"/> class.</summary>
        public NeuronCollection()
        {
            fEventMonitor = new NeuronCollectionEventMonitor<T>(this);

                // don't need to add to the eventMananger, this is done when items are added to the list
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronCollection{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.Designer.NeuronCollection`1"/> class.</summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="collection"/> parameter cannot be null.</exception>
        public NeuronCollection(System.Collections.Generic.IEnumerable<T> collection)
            : base(collection)
        {
            fEventMonitor = new NeuronCollectionEventMonitor<T>(this);

                // don't need to add to the eventMananger, this is done when items are added to the list
        }

        #endregion

        #region overrides

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, T item)
        {
            var iIndex = IndexOf(item);
            if (iIndex == -1 && item != null)
            {
                // we only monitor unique numbers, so check for this. item can be nul when we are resetting the default neurons
                fEventMonitor.AddItem(item.ID);
            }

            base.InsertItem(index, item);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iToRemove = this[index];
            base.RemoveItem(index);
            var iIndex = IndexOf(iToRemove);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.RemoveItem(iToRemove.ID);
            }
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, T item)
        {
            var iToRemove = this[index];
            var iIndex = IndexOf(item);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.AddItem(item.ID);
            }

            base.SetItem(index, item);
            iIndex = IndexOf(iToRemove);
            if (iIndex == -1)
            {
                // we only monitor unique numbers, so check for this.
                fEventMonitor.RemoveItem(iToRemove.ID);
            }
        }

        #endregion

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

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("ID");
                var iId = ulong.Parse(reader.ReadString());
                var ifound = Brain.Current[iId] as T;
                if (ifound != null)
                {
                    Add(ifound);
                }
                else
                {
                    LogService.Log.LogError("NeuronCollection", string.Format("Failed to find item with id: {0}.", iId));
                }

                reader.ReadEndElement();

                // reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var i in this)
            {
                if (i != null)
                {
                    // i can be null when we are reloading the list of default neurons
                    writer.WriteStartElement("ID");
                    writer.WriteString(i.ID.ToString());
                    writer.WriteEndElement();
                }
            }
        }

        #endregion

        #region EventMonitor Members

        /// <summary>Replaces all references to the specified neuron with the new value.</summary>
        /// <param name="old">The neuron to replace.</param>
        /// <param name="value">The value.</param>
        public void Replace(T old, T value)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i] == old)
                {
                    this[i] = value;
                }
            }
        }

        /// <summary>The remove all.</summary>
        /// <param name="value">The value.</param>
        public void RemoveAll(T value)
        {
            while (Remove(value))
            {
                ;
            }
        }

        #endregion
    }
}