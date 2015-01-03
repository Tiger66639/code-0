// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaursItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that contains <see cref="ThesaurusItem" />s and which monitors the network for changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection that contains <see cref="ThesaurusItem" />s and which monitors the network for changes.
    /// </summary>
    public class ThesaursItemCollection : Data.ObservedCollection<ThesaurusItem>, 
                                          Data.IOnCascadedChanged, 
                                          Data.INotifyCascadedPropertyChanged, 
                                          Data.ICascadedNotifyCollectionChanged
    {
        /// <summary>The remove from event.</summary>
        /// <param name="item">The item.</param>
        public void RemoveFromEvent(ThesaurusItem item)
        {
            var iIndex = IndexOf(item);
            if (iIndex >= 0)
            {
                RemoveItemDirect(iIndex);
            }
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThesaursItemCollection" /> class.
        /// </summary>
        public ThesaursItemCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaursItemCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public ThesaursItemCollection(object owner)
            : base(owner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaursItemCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toCopy">To copy.</param>
        public ThesaursItemCollection(object owner, System.Collections.Generic.List<ThesaurusItem> toCopy)
            : base(owner, toCopy)
        {
        }

        #endregion

        #region events

        /// <summary>
        ///     Occurs when [cascaded collection changed].
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        /// <summary>
        ///     Occurs when [cascaded property changed].
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        #endregion

        #region Overrides

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            ThesaurusItemCollectionEventMonitor.Default.Clear();
            base.ClearItems();

            // event raise needs to be done before the clear so that we can reach the list of items. 
            var iArgs =
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            var iCArgs = new Data.CascadedCollectionChangedEventArgs(this, iArgs);
            Data.EventEngine.OnCollectionChanged(this, iCArgs);

            base.ClearItems();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ThesaurusItem item)
        {
            base.InsertItem(index, item);
            ThesaurusItemCollectionEventMonitor.Default.AddItem(item);
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                var iNeuron = iItem.Item;
                if (iNeuron != null && iNeuron.ID != Neuron.EmptyId)
                {
                    // in case it got deleted.
                    if (Neuron.IsEmpty(iNeuron.ID) == false)
                    {
                        // only try to remove if it isn't already.
                        ThesaurusItemCollectionEventMonitor.Default.RemoveItem(iNeuron.ID);
                    }
                }
            }

            base.RemoveItem(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ThesaurusItem item)
        {
            var iOld = this[index];
            if (iOld != null)
            {
                if (Neuron.IsEmpty(iOld.ID) == false)
                {
                    // only try to remove if it isn't already.
                    ThesaurusItemCollectionEventMonitor.Default.RemoveItem(iOld.ID);
                }
            }

            base.SetItem(index, item);
            ThesaurusItemCollectionEventMonitor.Default.AddItem(item);
        }

        /// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the
        ///     provided arguments.</summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Data.EventEngine.OnCollectionChanged(this, new Data.CascadedCollectionChangedEventArgs(this, e));
            }
        }

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion
    }
}