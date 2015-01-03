// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronExplorer.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data of a network explorer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Contains all the data of a network explorer.
    /// </summary>
    public class NeuronExplorer : Data.ObservableObject, System.Windows.IWeakEventListener
    {
        /// <summary>The update selection range.</summary>
        internal void UpdateSelectionRange()
        {
            var i = 0;
            while (i < Items.Count)
            {
                // we get strange 'collectionModified exceptions sometimes, so put it in a classic loop, it's not that many anyway.                                                       
                var iItem = Items[i] as NeuronExplorerItem;
                i++;
                if (iItem != null)
                {
                    if (Selection.Contains(iItem.ID))
                    {
                        iItem.IsSelected = true;
                    }
                    else
                    {
                        iItem.IsSelected = false;
                    }
                }
            }

            var iSelected = Selection.SelectedItem as NeuronExplorerItem;
            if (iSelected != null && iSelected.Item is NeuronCluster)
            {
                if (IsChildrenLoaded)
                {
                    Children = new ExplorerChildrenCollection(iSelected, (NeuronCluster)iSelected.Item);
                }
            }
            else
            {
                Children = null; // there is no selection, so reset the children.
            }
        }

        /// <summary>
        ///     Tries to load the child explorer items for this explorer item (if we wrap a neuronCluster).
        /// </summary>
        public void TryLoadChildren()
        {
            try
            {
                var iItem = Selection.SelectedItem as NeuronExplorerItem;
                if (iItem != null)
                {
                    var iCluster = iItem.Item as NeuronCluster;
                    if (iCluster != null)
                    {
                        Children = new ExplorerChildrenCollection(iItem, iCluster);

                            // the collection needs an INeuronWrapper as owner. Since the owner isn't really used in this case, we use the cluster as it's own owner.
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ExplorerItem.TryLoadChildren", e.ToString());
            }
        }

        /// <summary>
        ///     Deletes all the selected items, and generating undo data for all. Note: at the time
        ///     of implementation, the UndoSTore didn't yet support multi threaded access, so only
        ///     call this from the main thread, otherwise use <see cref="NeuronExplorer.DeleteSelectedItemsNoUndo" />
        /// </summary>
        public void DeleteSelectedItems()
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var toDeleteLater = new System.Collections.Generic.List<Neuron>();

                    // when an item can't be deleted the first try, because it is used as a meaning, we try later on, when the other neurons have been deleted, maybe it now no longer is used as a meaning.  We try this for as long as we can delete neurons.
                foreach (var i in Selection.SelectedIds)
                {
                    Neuron iNeuron;
                    if (Brain.Current.TryFindNeuron(i, out iNeuron))
                    {
                        if (iNeuron.CanBeDeleted)
                        {
                            WindowMain.DeleteItemFromBrain(iNeuron);
                        }
                        else
                        {
                            toDeleteLater.Add(iNeuron);
                        }
                    }
                }

                var iFound = true;
                while (toDeleteLater.Count > 0 && iFound)
                {
                    iFound = false;
                    var i = 0;
                    while (i < toDeleteLater.Count)
                    {
                        if (toDeleteLater[i].CanBeDeleted)
                        {
                            iFound = true;
                            WindowMain.DeleteItemFromBrain(toDeleteLater[i]);
                            toDeleteLater.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>
        ///     Deletes the selected items without generating undo data. This can be
        ///     used from another thread (than the ui).
        /// </summary>
        public void DeleteSelectedItemsNoUndo()
        {
            var toDeleteLater = new System.Collections.Generic.List<Neuron>();

                // when an item can't be deleted the first try, because it is used as a meaning, we try later on, when the other neurons have been deleted, maybe it now no longer is used as a meaning.  We try this for as long as we can delete neurons.
            foreach (var i in Selection.SelectedIds)
            {
                Neuron iNeuron;
                if (Brain.Current.TryFindNeuron(i, out iNeuron))
                {
                    if (iNeuron.CanBeDeleted)
                    {
                        Brain.Current.Delete(iNeuron);
                    }
                    else
                    {
                        toDeleteLater.Add(iNeuron);
                    }
                }
            }

            var iFound = true;
            while (toDeleteLater.Count > 0 && iFound)
            {
                iFound = false;
                var i = 0;
                while (i < toDeleteLater.Count)
                {
                    if (toDeleteLater[i].CanBeDeleted)
                    {
                        iFound = true;
                        Brain.Current.Delete(toDeleteLater[i]);
                        toDeleteLater.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        #region Fields

        /// <summary>The f items.</summary>
        private ExplorerItemCollection fItems = new ExplorerItemCollection();

        /// <summary>The f max visible.</summary>
        private int fMaxVisible;

        // private bool fSearchStart;
        /// <summary>The f event monitor.</summary>
        private NeuronExplorerEventMonitor fEventMonitor;

        /// <summary>The f item count.</summary>
        private ulong fItemCount;

        /// <summary>The f max scroll value.</summary>
        private ulong fMaxScrollValue;

        /// <summary>The f current scroll pos.</summary>
        private ulong fCurrentScrollPos = Neuron.StartId;

                      // we start from StartId, so that we don't display 0 and 1, which are invalid values.

        // private ulong fPrevSearched;
        /// <summary>The f item height.</summary>
        private double fItemHeight = 20.0;

                       // we provide a default height, which will be used by all items. This allows us to calculate the number of visible items.

        /// <summary>The f actual height.</summary>
        private double fActualHeight;

        /// <summary>The f children.</summary>
        private ExplorerChildrenCollection fChildren;

                                           // possible children of the neuronCluster, only created when loaded.

        /// <summary>The f is children loaded.</summary>
        private bool fIsChildrenLoaded;

        #endregion

        #region ctor~

        /// <summary>Initializes a new instance of the <see cref="NeuronExplorer"/> class.</summary>
        public NeuronExplorer()
        {
            Selection = new ExplorerSelection(this);
            fEventMonitor = new NeuronExplorerEventMonitor(this);

                // needs to be done before initialize, otherwise OnMaxVisibleChanged will fail
            NetworkLoaded();

                // when we first start up, we make certain that the init values are always set.  We don't get this event when the application starts without a default project (so a new empty project is used).
            ClearedEventManager.AddListener(Brain.Current, this);

            // CommandBindings.AddRange(App.Current.MainWindow.CommandBindings);                                  //we do this so we can access the command bindings, declared on the main window, even when the form is floating.
            BrainData.Current.AfterLoad += Current_AfterLoad;
            BrainData.Current.AfterSave += Current_AfterSave;
        }

        /// <summary>after saving, the deleted items at the end of the list are truncated, so updated the listCount.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_AfterSave(object sender, System.EventArgs e)
        {
            ItemCount = Brain.Current.NextID;
        }

        /// <summary>Finalizes an instance of the <see cref="NeuronExplorer"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations before the<see cref="NeuronExplorer"/> is reclaimed by garbage collection.</summary>
        ~NeuronExplorer()
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                // only try to unregister listeners when the app is not shutting down, otherwise it wont work anymore.
                LoadedEventManager.RemoveListener(Brain.Current, this);
                ClearedEventManager.RemoveListener(Brain.Current, this);
                System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(
                    BrainData.Current.DefaultMeaningIds, 
                    this);
            }
        }

        #endregion

        #region prop

        #region Items

        /// <summary>
        ///     Gets the list of items that are currently loaded.
        /// </summary>
        public ExplorerItemCollection Items
        {
            get
            {
                return fItems;
            }

            internal set
            {
                fItems = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        #region Selection

        /// <summary>
        ///     stores selection data, when null, there is no selection.
        /// </summary>
        public ExplorerSelection Selection { get; private set; }

        #endregion

        #region ItemCount

        /// <summary>
        ///     Gets/sets the the total number of neurons in the list.
        /// </summary>
        public ulong ItemCount
        {
            get
            {
                return fItemCount;
            }

            set
            {
                if (value != fItemCount)
                {
                    fItemCount = value;
                    OnPropertyChanged("ItemCount");
                    MaxScrollValue = value - (ulong)MaxVisible;
                }
            }
        }

        #endregion

        #region SearchData

        /// <summary>
        ///     Gets/sets the searchdata currently active.
        /// </summary>
        internal ExplorerSearcher SearchData { get; set; }

        #endregion

        #region MaxVisible

        /// <summary>
        ///     Gets/sets the how many items can be visible i
        /// </summary>
        public int MaxVisible
        {
            get
            {
                return fMaxVisible;
            }

            set
            {
                if (value != fMaxVisible)
                {
                    fMaxVisible = value;
                    MaxScrollValue = ItemCount - (ulong)value;
                    fEventMonitor.MaxVisible = (ulong)value;
                    RenderListFrom(CurrentScrollPos);
                    OnPropertyChanged("MaxVisible");
                }
            }
        }

        #endregion

        #region ItemHeight

        /// <summary>
        ///     Gets/sets the the height used by each <see cref="ExplorerItem" /> in the list.
        /// </summary>
        public double ItemHeight
        {
            get
            {
                return fItemHeight;
            }

            set
            {
                if (value != fItemHeight)
                {
                    fItemHeight = value;
                    MaxVisible = (int)(ActualHeight / value);
                    OnPropertyChanged("ItemHeight");
                }
            }
        }

        #endregion

        #region ActualHeight

        /// <summary>
        ///     Gets/sets the total height available for all the visible items.
        /// </summary>
        public double ActualHeight
        {
            get
            {
                return fActualHeight;
            }

            set
            {
                if (value != fItemHeight)
                {
                    fActualHeight = value;
                    MaxVisible = (int)(value / ItemHeight);
                    OnPropertyChanged("ActualHeight");
                }
            }
        }

        #endregion

        #region MaxScrollValue

        /// <summary>
        ///     Gets/sets the the maximimum scroll value.  This is ItemCount - MaxVisible.
        /// </summary>
        public ulong MaxScrollValue
        {
            get
            {
                return fMaxScrollValue;
            }

            internal set
            {
                fMaxScrollValue = value;
                OnPropertyChanged("MaxScrollValue");
            }
        }

        #endregion

        #region CurrentScrollPos

        /// <summary>
        ///     Gets/sets the the id of the topmost currently visible neuron.
        /// </summary>
        public ulong CurrentScrollPos
        {
            get
            {
                return fCurrentScrollPos;
            }

            set
            {
                if (value != fCurrentScrollPos)
                {
                    if (value < 2)
                    {
                        // check the bounderies.
                        value = 2;
                    }
                    else if (value > MaxScrollValue)
                    {
                        value = MaxScrollValue;
                    }

                    ulong iDif;
                    if (value > fCurrentScrollPos)
                    {
                        iDif = value - fCurrentScrollPos;
                    }
                    else
                    {
                        iDif = fCurrentScrollPos - value;
                    }

                    if (iDif >= (ulong)MaxVisible)
                    {
                        // shortcut: if we scroll more than is visible, we simply jump to the correct part in the list.
                        RenderListFrom(value);
                    }
                    else
                    {
                        if (value > fCurrentScrollPos)
                        {
                            // if newvalue bigger than old -> we scroll down.
                            for (ulong i = 0;
                                 i < iDif && i + fCurrentScrollPos + (ulong)MaxVisible < Brain.Current.NextID - 1;
                                 i++)
                            {
                                // add to back.
                                CreateExplorerItem(fCurrentScrollPos + (ulong)fItems.Count, fItems.Count);

                                    // use fItems.Count to make certain we always use the correct value.
                            }

                            for (ulong i = 0; i < iDif; i++)
                            {
                                // remove from front.
                                fItems.RemoveAt(0);
                            }
                        }
                        else
                        {
                            for (ulong i = 1; i <= iDif; i++)
                            {
                                // add to front
                                CreateExplorerItem(fCurrentScrollPos - i, 0);

                                    // i is negative so we go to the front of the list.
                            }

                            while (fItems.Count > MaxVisible + 1)
                            {
                                fItems.RemoveAt(fItems.Count - 1); // remove from end for as long as needed.
                            }
                        }
                    }

                    fEventMonitor.CurrentScrollPos = value;

                        // dp props can only be read from ui thread, which is a problem for the eventmonitor, so give it a copy.
                    fCurrentScrollPos = value;
                    OnPropertyChanged("CurrentScrollPos");
                }
            }
        }

        #endregion

        #region IsChildrenLoaded

        /// <summary>
        ///     Gets/sets the ether we should try to load the Children of the currently selected item or not.
        /// </summary>
        /// <remarks>
        ///     this always needs a backing prop, cause there might not always be an item selected/or selected that
        ///     has children, but the next one might.
        /// </remarks>
        public bool IsChildrenLoaded
        {
            get
            {
                return fIsChildrenLoaded;
            }

            set
            {
                if (value != fIsChildrenLoaded)
                {
                    fIsChildrenLoaded = value;
                    if (value)
                    {
                        TryLoadChildren();
                    }
                    else
                    {
                        Children = null;
                    }

                    OnPropertyChanged("IsChildrenLoaded");
                }
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children assigned to this  <see cref="NeuronCluster" /> (if we are a wrapper for one and the
        ///     explorer indicates they should be loaded).
        /// </summary>
        /// <remarks>
        ///     This list is not automatically loaded, this is done by the <see cref="NeuronExplorer" /> when this item is selected
        ///     through <see cref="ExplorerItem.TryLoadChildren" />.
        /// </remarks>
        public ExplorerChildrenCollection Children
        {
            get
            {
                return fChildren;
            }

            internal set
            {
                if (value != fChildren)
                {
                    fChildren = value;
                    OnPropertyChanged("Children");
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Called when the network is laoded, inits everything.
        /// </summary>
        private void NetworkLoaded()
        {
            System.Collections.Specialized.CollectionChangedEventManager.AddListener(
                BrainData.Current.DefaultMeaningIds, 
                this);
            ItemCount = Brain.Current.NextID;
            CurrentScrollPos = 0; // we also clear this value cause we go back to the beginning state.
            RenderListFrom(Neuron.StartId); // fill with all the values.
            SearchData = null; // always need to start a new search when the network got reloaded.
        }

        /// <summary>Handles the AfterLoad event of the Current control.</summary>
        /// <remarks>each time that the defaultMeaningIDs list is changed, we need to re attach the listener cause load creates a new
        ///     list.
        ///     We also initiate the explorer view whenever a new project has been loaded.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Current_AfterLoad(object sender, System.EventArgs e)
        {
            if (BrainData.Current.DefaultMeaningIds != null)
            {
                // could be that something went wrong during hte load, in which case the event still gets called, but some things are invalid.
                NetworkLoaded();
            }
        }

        /// <summary>Clears the list of items and fills it again starting from the specified id value.  Only
        ///     fills for MAXVISIBLE nr of values.  Could be improved by calculating the nr of items actually visible.</summary>
        /// <param name="start">Id value from where to start filling with neurons.</param>
        private void RenderListFrom(ulong start)
        {
            fItems.Clear();
            if (BrainData.Current.NeuronInfo != null)
            {
                // when loading, neuronInfo is null.
                for (var i = start; i < Brain.Current.NextID && i <= start + (ulong)MaxVisible; i++)
                {
                    CreateExplorerItem(i, fItems.Count);
                }
            }
        }

        /// <summary>The create explorer item.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="index">The index.</param>
        private void CreateExplorerItem(ulong neuron, int index)
        {
            try
            {
                var iNew = CreateExplorerItem(neuron);
                fItems.Insert(index, iNew);
            }
            catch (System.Exception e)
            {
                var iEr = e.ToString();
                LogService.Log.LogError("NeuronExplorer.RenderListFrom", iEr);
                var iEmpty = new InvalidExplorerItem(neuron, iEr);
                fItems.Insert(index, iEmpty);
            }
        }

        /// <summary>Creates a new explorer item for the speceified id.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="ExplorerItem"/>.</returns>
        internal ExplorerItem CreateExplorerItem(ulong neuron)
        {
            ExplorerItem iNew;
            if (neuron >= (ulong)PredefinedNeurons.EndOfStatic && neuron < (ulong)PredefinedNeurons.Dynamic)
            {
                iNew = new ReservedExplorerItem(neuron);
            }
            else
            {
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(neuron, out iFound))
                {
                    iNew = new NeuronExplorerItem(iFound);
                    ((NeuronExplorerItem)iNew).IsSelected = Selection.Contains(neuron);

                        // we need to depict if it was selected or not.
                }
                else if (Brain.Current.IsDeletedID(neuron))
                {
                    iNew = new FreeExplorerItem(neuron);
                }
                else
                {
                    iNew = new InvalidExplorerItem(neuron, "Neuron not found in brain!");
                }
            }

            return iNew;
        }

        /// <summary>Inserts the neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void InsertNeuron(Neuron neuron)
        {
            var iScrollPos = CurrentScrollPos;
            if (neuron.ID >= iScrollPos && neuron.ID < (iScrollPos + (ulong)MaxVisible))
            {
                ExplorerItem iNew = new NeuronExplorerItem(neuron);
                if (neuron.ID == (iScrollPos + (ulong)MaxVisible))
                {
                    fItems.Add(iNew);
                }
                else
                {
                    fItems[(int)(neuron.ID - iScrollPos)] = iNew;
                }
            }
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>true if the listener handled the event. It is considered an error by the<see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the
        ///     listener does not handle. Regardless, the method should return false if it receives an event that it does not
        ///     recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.Collections.Specialized.CollectionChangedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, System.Collections.Specialized.NotifyCollectionChangedEventArgs>(
                        DefaultMeaningIds_CollectionChanged), 
                    sender, 
                    (System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                return true;
            }

            if (managerType == typeof(ClearedEventManager))
            {
                Brain_Cleared(sender, e);

                    // somehow got back called from UI thread, so do a regular call, otherwise we get an empty Items list since the async call gets executed after the load.

                // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<object, EventArgs>(Brain_Cleared), sender, (EventArgs)e); //important to call async otherwise, the ItemCount and Scrollbar values aren't updated correclty (wrong thread).
                return true;
            }

            return false;
        }

        /// <summary>Need to check if there are any items currently displayed that are involved in the change, if so, update.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultMeaningIds_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var iPossibleItems = from i in Items
                                 where i is NeuronExplorerItem && ((NeuronExplorerItem)i).Item != null
                                 select (NeuronExplorerItem)i;

                // some items, like error items have no item, in those cases, the join crashes because of illegal null reference if we don't filter them out first.
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var iRes = from ulong iId in e.NewItems
                               join NeuronExplorerItem iItem in iPossibleItems on iId equals iItem.Item.ID
                               select iItem;
                    foreach (var i in iRes)
                    {
                        i.SetDefaultMeaning(true);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    iRes = from ulong iId in e.OldItems
                           join NeuronExplorerItem iItem in iPossibleItems on iId equals iItem.Item.ID
                           select iItem;
                    foreach (var i in iRes)
                    {
                        i.SetDefaultMeaning(false);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    iRes = from ulong iId in e.OldItems
                           join NeuronExplorerItem iItem in iPossibleItems on iId equals iItem.Item.ID
                           select iItem;
                    foreach (var i in iRes)
                    {
                        i.SetDefaultMeaning(false);
                    }

                    iRes = from ulong iId in e.NewItems
                           join NeuronExplorerItem iItem in iPossibleItems on iId equals iItem.Item.ID
                           select iItem;
                    foreach (var i in iRes)
                    {
                        i.SetDefaultMeaning(true);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (NeuronExplorerItem i in Items)
                    {
                        i.SetDefaultMeaning(false);
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>Handles the Cleared event of the Brain.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Brain_Cleared(object sender, System.EventArgs e)
        {
            Items = new ExplorerItemCollection(); // we recreate this list so that it's event monitor is reregistered. 
            fEventMonitor = new NeuronExplorerEventMonitor(this);

                // also needs to be reloaded, so that we reregister to monitor for changes.
            ItemCount = Brain.Current.NextID;

                // when the network gets cleared, we also need to reset the nr of items in the explorerer.
        }

        #endregion
    }
}