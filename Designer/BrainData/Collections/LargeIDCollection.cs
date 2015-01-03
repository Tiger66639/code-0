// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LargeIDCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of neuron id's specifically suited for large sets. This class doesn't tax the event monitoring
//   system to much, by only registering 1 time, for the entire list instead of for each item seperatly (like the
//   <see cref="SmallIDCollection" /> does). Also provides streaming functionality.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection of neuron id's specifically suited for large sets. This class doesn't tax the event monitoring
    ///     system to much, by only registering 1 time, for the entire list instead of for each item seperatly (like the
    ///     <see cref="SmallIDCollection" /> does). Also provides streaming functionality.
    /// </summary>
    public class LargeIDCollection : System.Collections.Specialized.INotifyCollectionChanged, 
                                     System.Collections.Generic.ICollection<ulong>, 
                                     System.Collections.Generic.IEnumerable<ulong>, 
                                     System.Collections.IEnumerable, 
                                     System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f event monitor.</summary>
        private LargeIDCollectionEventMonitor fEventMonitor;

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.HashSet<ulong> fItems =
            new System.Collections.Generic.HashSet<ulong>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="LargeIDCollection" /> class.
        /// </summary>
        /// <remarks>
        ///     Need to make certain that we are notified when the network has changed, so that we can keep the list
        ///     updated.
        /// </remarks>
        public LargeIDCollection()
        {
            fEventMonitor = new LargeIDCollectionEventMonitor(this);
        }

        #region IEnumerable<ulong> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<ulong> GetEnumerator()
        {
            return fItems.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return fItems.GetEnumerator();
        }

        #endregion

        /// <summary>Saves the data as a binary stream to the specified path.</summary>
        /// <param name="path">The path.</param>
        public void SaveData(string path)
        {
            using (var iStr = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var iWriter = new System.IO.BinaryWriter(iStr);
                foreach (var u in fItems)
                {
                    iWriter.Write(u);
                }
            }
        }

        /// <summary>Reads the data as a binary stream from the specified path.</summary>
        /// <param name="path">The path.</param>
        internal void ReadData(string path)
        {
            if (System.IO.File.Exists(path))
            {
                using (var iStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var iReader = new System.IO.BinaryReader(iStream);
                    var iToRead = (iStream.Length - iStream.Position) / 8;

                        // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                    while (iToRead > 0)
                    {
                        Add(iReader.ReadUInt64());
                        iToRead--;
                    }
                }
            }
        }

        #region INotifyCollectionChanged Members

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Called when the collection has changed. Triggers the event.</summary>
        /// <param name="args">The args.</param>
        protected virtual void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }

        #endregion

        #region ICollection<ulong> Members

        /// <summary>Adds the specified item.</summary>
        /// <param name="item">The item.</param>
        public void Add(ulong item)
        {
            if (fItems.Contains(item) == false)
            {
                InternalInsert(item);
            }
        }

        /// <summary>adds the item</summary>
        /// <param name="item">The item.</param>
        protected virtual void InternalInsert(ulong item)
        {
            fItems.Add(item);
            OnCollectionChanged(
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Add, 
                    item));
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public virtual void Clear()
        {
            fItems.Clear();
            OnCollectionChanged(
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        /// <summary>Determines whether [contains] [the specified item].</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.</returns>
        public bool Contains(ulong item)
        {
            return fItems.Contains(item);
        }

        /// <summary>Copies to.</summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(ulong[] array, int arrayIndex)
        {
            fItems.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return fItems.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>Removes the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Remove(ulong item)
        {
            if (fItems.Contains(item))
            {
                return InternalRemove(item);
            }

            return false;
        }

        /// <summary>removes the item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected virtual bool InternalRemove(ulong item)
        {
            OnCollectionChanged(
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Remove, 
                    item));
            return fItems.Remove(item);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
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
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var i in fItems)
            {
                XmlStore.WriteElement(writer, "ID", i);
            }
        }

        #endregion
    }
}