// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BreakPointCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The break point event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{

    #region events

    /// <summary>The break point event args.</summary>
    public class BreakPointEventArgs : System.EventArgs
    {
        /// <summary>
        ///     The breakpoint for which this event was raised.
        /// </summary>
        public Expression BreakPoint { get; set; }

        /// <summary>
        ///     The proccessor that triggered the event. (this is usually a debug processor.
        /// </summary>
        public Processor Processor { get; set; }
    }

    /// <summary>
    ///     A delegate for events raised by the <see cref="BreakPointCollection" /> containing the expression for which the
    ///     event
    ///     was raised.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void BreakPointEventHandler(object sender, BreakPointEventArgs e);

    #endregion

    /// <summary>
    ///     A collection used to store breakpoints.
    /// </summary>
    public class BreakPointCollection : System.Collections.Generic.ICollection<Expression>, 
                                        System.Collections.Specialized.INotifyCollectionChanged, 
                                        System.Xml.Serialization.IXmlSerializable
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BreakPointCollection"/> class.</summary>
        public BreakPointCollection()
        {
            fEventMonitor = new BreakPointCollectionEventMonitor(this);
        }

        #endregion

        #region IEnumerable<Expression> Members

        /// <summary>Gets an enumerator for the breakpoints.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<Expression> GetEnumerator()
        {
            return fItems.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>The get enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return fItemsList.GetEnumerator();
        }

        #endregion

        #region EventMonitor Members

        /// <summary>Replaces the specified id with a new object (if no object, a remove will be done).</summary>
        /// <param name="id">The id.</param>
        /// <param name="item">The item.</param>
        internal void Replace(ulong id, Expression item)
        {
            var iPrev = fItems[id];
            if (item != null)
            {
                fItemsLock.EnterWriteLock();
                try
                {
                    fItems[id] = item;
                }
                finally
                {
                    fItemsLock.ExitWriteLock();
                }

                OnCollectionChanged(
                    new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                        System.Collections.Specialized.NotifyCollectionChangedAction.Replace, 
                        item, 
                        iPrev));
            }
            else
            {
                Remove(iPrev);
            }
        }

        #endregion

        #region Fields

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, Expression> fItems =
            new System.Collections.Generic.Dictionary<ulong, Expression>(); // we use a dict for fast searching.

        /// <summary>The f items list.</summary>
        private readonly System.Collections.Generic.List<Expression> fItemsList =
            new System.Collections.Generic.List<Expression>();

                                                                     // we need this to have a correct index position for the CollectionChanged event. otherwise we can't use ui binding. In expressions, takes up less, they are in mem anyway.

        /// <summary>The f items lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fItemsLock =
            new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

                                                               // this is to make it a bit thread safe internally.

        /// <summary>
        ///     We keep a ref to the event monitor, in this object, to keep it alive, because if there is no breakpoint, the
        ///     event monitor is no where referenced in the event manager.
        /// </summary>
        private BreakPointCollectionEventMonitor fEventMonitor;

        #endregion

        #region Events

        /// <summary>
        ///     Raised when a breakpoint has been reached and a debugger is waiting for an action.
        /// </summary>
        public event BreakPointEventHandler BreakPointReached;

        /// <summary>The on break point reached.</summary>
        /// <param name="toProcess">The to process.</param>
        /// <param name="processor">The processor.</param>
        internal void OnBreakPointReached(Expression toProcess, Processor processor)
        {
            if (BreakPointReached != null)
            {
                BreakPointReached(this, new BreakPointEventArgs { BreakPoint = toProcess, Processor = processor });
            }
        }

        #endregion

        #region ICollection<Expression> Members

        /// <summary>Add the item as a breakpoint.</summary>
        /// <param name="item"></param>
        public void Add(Expression item)
        {
            fItemsLock.EnterUpgradeableReadLock();
            try
            {
                if (fItems.ContainsKey(item.ID) == false)
                {
                    fItemsLock.EnterWriteLock();
                    try
                    {
                        fItems.Add(item.ID, item);
                        fItemsList.Add(item);
                    }
                    finally
                    {
                        fItemsLock.ExitWriteLock();
                    }

                    OnCollectionChanged(
                        new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                            System.Collections.Specialized.NotifyCollectionChangedAction.Add, 
                            item));
                }
            }
            finally
            {
                fItemsLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        ///     Remove all the breakpoints.
        /// </summary>
        public void Clear()
        {
            fItemsLock.EnterWriteLock();
            try
            {
                fItems.Clear();
                fItemsList.Clear();
            }
            finally
            {
                fItemsLock.ExitWriteLock();
            }

            OnCollectionChanged(
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            foreach (var iObj in BrainData.Current.OpenDocuments)
            {
                var iEditor = iObj as CodeEditor;
                if (iEditor != null)
                {
                    iEditor.ResetAllBreakPoints();
                }
            }
        }

        /// <summary>Check if the specified item has a breakpoint declard.</summary>
        /// <param name="item"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Contains(Expression item)
        {
            fItemsLock.EnterReadLock();
            try
            {
                return fItems.ContainsKey(item.ID);
            }
            finally
            {
                fItemsLock.ExitReadLock();
            }
        }

        /// <summary>Not implemented</summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The array Index.</param>
        public void CopyTo(Expression[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Returns the number of breakpoints.
        /// </summary>
        public int Count
        {
            get
            {
                fItemsLock.EnterReadLock();
                try
                {
                    return fItems.Count;
                }
                finally
                {
                    fItemsLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        ///     this list is not read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return fItemsLock.IsWriteLockHeld;
            }
        }

        /// <summary>Removes the specified breakpoint.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if it succeeed, otherwise false.</returns>
        public bool Remove(Expression item)
        {
            fItemsLock.EnterWriteLock();
            try
            {
                if (fItems.ContainsKey(item.ID))
                {
                    OnCollectionChanged(
                        new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                            System.Collections.Specialized.NotifyCollectionChangedAction.Remove, 
                            item, 
                            fItemsList.IndexOf(item)));
                    fItemsList.Remove(item);
                    return fItems.Remove(item.ID);
                }
            }
            finally
            {
                fItemsLock.ExitWriteLock();
            }

            return false;
        }

        /// <summary>Removes the specified breakpoint through it's id. This is used to remove a breakpoint when the neuron is already
        ///     deleted (callback
        ///     for the event system).</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if it succeeed, otherwise false.</returns>
        public bool Remove(ulong item)
        {
            fItemsLock.EnterWriteLock();
            try
            {
                Expression iFound;
                if (fItems.TryGetValue(item, out iFound))
                {
                    OnCollectionChanged(
                        new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                            System.Collections.Specialized.NotifyCollectionChangedAction.Remove, 
                            iFound, 
                            fItemsList.IndexOf(iFound)));
                    fItemsList.Remove(iFound);
                    return fItems.Remove(item);
                }
            }
            finally
            {
                fItemsLock.ExitWriteLock();
            }

            return false;
        }

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        ///     Raised when a new breakpoint is added or removed.
        /// </summary>
        /// <remarks>
        ///     Note: The event for the reset is called before the actual cleaning of the list, this allows you to access the data
        ///     while
        ///     checking the values.
        /// </remarks>
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>The on collection changed.</summary>
        /// <param name="args">The args.</param>
        protected virtual void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
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
                var iVal = reader.ReadString();
                var iConverted = ulong.Parse(iVal);
                var iFound = Brain.Current[iConverted] as Expression;
                if (iFound != null)
                {
                    Add(iFound); // we use the regular add, so that the event monitoring system gets warned when loaded.
                }
                else
                {
                    LogService.Log.LogError(
                        "BreakPoints.Read", 
                        string.Format("Expression with ID {0} not found in brain.", iConverted));
                }

                reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var i in fItems.Keys)
            {
                writer.WriteStartElement("ID");
                writer.WriteString(i.ToString());
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}