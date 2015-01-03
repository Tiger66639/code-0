// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for
//   <see cref="JaStDev.HAB.Designer.ProcItem.Processor" /> objects, updated
//   for WPF displaying.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A wrapper class for
    ///     <see cref="JaStDev.HAB.Designer.ProcItem.Processor" /> objects, updated
    ///     for WPF displaying.
    /// </summary>
    public class ProcItem : ProcManItem
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ProcItem"/> class.</summary>
        /// <param name="toWrap">The processor to wrap.</param>
        public ProcItem(DebugProcessor toWrap)
        {
            Processor = toWrap;
            if (toWrap != null)
            {
                toWrap.Finished += Processor_Finished;
                toWrap.Paused += ProcItem_Paused;
                toWrap.Continued += ProcItem_Continued;
                toWrap.ProcsAdded += Proc_ProcsAdded;
                toWrap.InvalidNeuronChange += toWrap_InvalidNeuronChange;
            }

            fValues.CollectionChanged += fValues_CollectionChanged;
            fName = ProcessorManager.Current.ProcessorCount.ToString();
        }

        #endregion

        #region Fields

        /// <summary>The f values.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<DebugNeuron> fValues =
            new System.Collections.ObjectModel.ObservableCollection<DebugNeuron>();

        /// <summary>The f is view open.</summary>
        private bool fIsViewOpen;

        /// <summary>The f stack width.</summary>
        private System.Windows.GridLength fStackWidth;

        /// <summary>The f links to solve width.</summary>
        private System.Windows.GridLength fLinksToSolveWidth;

        /// <summary>The f variables width.</summary>
        private System.Windows.GridLength fVariablesWidth;

        /// <summary>The f globals width.</summary>
        private System.Windows.GridLength fGlobalsWidth;

        /// <summary>The f arguments width.</summary>
        private System.Windows.GridLength fArgumentsWidth;

        /// <summary>The f name.</summary>
        private string fName;

        /// <summary>The f invalid change data.</summary>
        private InvalidChangeDebugData fInvalidChangeData;

        #endregion

        #region Prop

        #region HasValues

        /// <summary>
        ///     Gets wether there are currently values available for display.
        /// </summary>
        public bool HasValues
        {
            get
            {
                return fValues.Count > 0;
            }
        }

        #endregion

        #region Processor

        /// <summary>
        ///     Gets the processor that is wrapped by this item.
        /// </summary>
        public DebugProcessor Processor { get; private set; }

        #endregion

        #region Values

        /// <summary>
        ///     Gets the list of values that should be displayed for this node.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<DebugNeuron> Values
        {
            get
            {
                return fValues;
            }
        }

        #endregion

        #region IsSplit

        /// <summary>
        ///     Gets a value indicating whether this instance is split for several
        ///     other processor items.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is split; otherwise, <c>false</c> .
        /// </value>
        public virtual bool IsSplit
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IsViewOpen

        /// <summary>
        ///     Gets/sets the wether the detailed view for this processor is currently
        ///     opened or not.
        /// </summary>
        public bool IsViewOpen
        {
            get
            {
                return fIsViewOpen;
            }

            set
            {
                if (fIsViewOpen != value)
                {
                    fIsViewOpen = value;
                    OnPropertyChanged("IsViewOpen");
                    Processor.IsUIActive = value;

                        // also let the processor know so he doesn't do to many UI stuff when not requered.
                    if (value)
                    {
                        var iMain = (WindowMain)System.Windows.Application.Current.MainWindow;
                        iMain.AddItemToOpenDocuments(this);
                    }
                    else
                    {
                        BrainData.Current.OpenDocuments.Remove(this);
                    }
                }
            }
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the processor. This is filled in by default as a
        ///     number, but can be changed to track processors more easely.
        /// </summary>
        public string Name
        {
            get
            {
                return fName;
            }

            set
            {
                fName = value;
                OnPropertyChanged("Name");
            }
        }

        #endregion

        #region StackWidth

        /// <summary>
        ///     Gets/sets the width of the stack data view.
        /// </summary>
        public System.Windows.GridLength StackWidth
        {
            get
            {
                return fStackWidth;
            }

            set
            {
                fStackWidth = value;
                OnPropertyChanged("StackWidth");
            }
        }

        #endregion

        #region LinksToSolveWidth

        /// <summary>
        ///     Gets/sets the width of the column that displays the links that will be
        ///     solved by the processor.
        /// </summary>
        public System.Windows.GridLength LinksToSolveWidth
        {
            get
            {
                return fLinksToSolveWidth;
            }

            set
            {
                fLinksToSolveWidth = value;
                OnPropertyChanged("LinksToSolve");
            }
        }

        #endregion

        #region VariablesWidth

        /// <summary>
        ///     Gets/sets the width of the variables column.
        /// </summary>
        public System.Windows.GridLength VariablesWidth
        {
            get
            {
                return fVariablesWidth;
            }

            set
            {
                fVariablesWidth = value;
                OnPropertyChanged("VariablesWidth");
            }
        }

        #endregion

        #region GlobalsWidth

        /// <summary>
        ///     Gets/sets the width of the globals column.
        /// </summary>
        public System.Windows.GridLength GlobalsWidth
        {
            get
            {
                return fGlobalsWidth;
            }

            set
            {
                fGlobalsWidth = value;
                OnPropertyChanged("GlobalsWidth");
            }
        }

        #endregion

        #region ArgumentsWidth

        /// <summary>
        ///     Gets/sets the width of the column that contains the function arguments
        ///     and return values.
        /// </summary>
        public System.Windows.GridLength ArgumentsWidth
        {
            get
            {
                return fArgumentsWidth;
            }

            set
            {
                fArgumentsWidth = value;
                OnPropertyChanged("ArgumentsWidth");
            }
        }

        #endregion

        #region InvalidChangeData

        /// <summary>
        ///     Gets/sets the data generated by a neuronchange that was not in the
        ///     attached processor.
        /// </summary>
        public InvalidChangeDebugData InvalidChangeData
        {
            get
            {
                return fInvalidChangeData;
            }

            private set
            {
                fInvalidChangeData = value;
                OnPropertyChanged("InvalidChangeData");
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Called when a split <paramref name="path"/> was unselected. Allows us
        ///     to update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public override void OnSplitPathUnSelected(SplitPath path)
        {
            Processor.Paths.Remove(path);
        }

        /// <summary>Called when a split <paramref name="path"/> was selected. Allows us to
        ///     update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public override void OnSplitPathSelected(SplitPath path)
        {
            var iIndex = Processor.SplitPath.Count - 1;
            var iId = Processor.SplitPath[iIndex];

            if (path.Items.Count > iIndex && iId == path.Items[iIndex].ItemID)
            {
                Processor.Paths.Add(path);
                Processor.IsInCurrentPath = true;
            }
        }

        /// <summary>Assigns the value(s) stored in the specified variable (for this
        ///     processor) to <see cref="JaStDev.HAB.Designer.ProcItem.Values"/></summary>
        /// <remarks>this is thread safe.</remarks>
        /// <param name="toDisplay">To display.</param>
        public override void GetValuesFor(Variable toDisplay)
        {
            var iList = Enumerable.ToList(toDisplay.GetValueWithoutInit(Processor));
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<System.Collections.Generic.List<Neuron>>(DisplayValues), 
                iList);
        }

        /// <summary>Displays the values.</summary>
        /// <param name="list">The list.</param>
        private void DisplayValues(System.Collections.Generic.List<Neuron> list)
        {
            Values.Clear();
            foreach (var i in list)
            {
                Values.Add(new DebugNeuron(i));
            }
        }

        /// <summary>
        ///     Checks if there is an <see cref="InvalidChangeData" /> record, and if
        ///     so removes it correctly. Called when this processor has finished or
        ///     continues.
        /// </summary>
        private void ClearInvalidChangeData()
        {
            if (InvalidChangeData != null)
            {
                var iClearWatches = false;
                if (InvalidChangeData.Originator == this)
                {
                    InvalidChangeData.Originator = null;
                    if (InvalidChangeData.Owner == null)
                    {
                        iClearWatches = true;
                    }
                }
                else
                {
                    InvalidChangeData.Owner = null;
                    if (InvalidChangeData.Originator == null)
                    {
                        iClearWatches = true;
                    }
                }

                if (iClearWatches)
                {
                    foreach (var i in InvalidChangeData.Watches)
                    {
                        i.InvalidChangeData = null;
                    }
                }

                InvalidChangeData = null;
            }
        }

        /// <summary>The remove from owner.</summary>
        private void RemoveFromOwner()
        {
            if (Owner != null)
            {
                // if this is null, we have a problem, somewhere, the UI got seriously behind, so ask the main Processormanager to remove it at the end of the ride.
                var iFolder = Owner as ProcManFolder;
                if (iFolder != null)
                {
                    lock (iFolder.Processors) iFolder.Processors.Remove(this);
                    var iFolderOwner = (IProcessorsOwner)iFolder.Owner;
                    while (iFolderOwner != null && (iFolder.Processors.Count == 0 || iFolder.Processors.Count == 1))
                    {
                        lock (iFolder.Processors)
                        {
                            if (iFolder.Processors.Count == 1)
                            {
                                if (System.Threading.Monitor.TryEnter(iFolderOwner.Processors))
                                {
                                    try
                                    {
                                        var iIndex = iFolderOwner.Processors.IndexOf(iFolder);
                                        if (System.Threading.Monitor.TryEnter(iFolder.Processors[0]))
                                        {
                                            // we are moving an item that is running in another thread, so we need to make certain it can not add processors while moving it up the tree. Offcourse, if we use a normal lock, we can get deadlocks because of the order (we first lock the list, than the child, while the child first locks itself and than the list). To solve this, we give precedence to the child. If we can't lock now, we do another pass in the loop and try again later on.
                                            var iToMove = iFolder.Processors[0];

                                                // need to remove before we can add to other list, becaus of owner.
                                            try
                                            {
                                                iFolder.Processors.RemoveAt(0); // this doesn't trigger a complete clear.
                                                if (iIndex > -1)
                                                {
                                                    iFolderOwner.Processors[iIndex] = iToMove;
                                                }
                                                else
                                                {
                                                    iFolderOwner.Processors.Add(iToMove);
                                                }
                                            }
                                            finally
                                            {
                                                System.Threading.Monitor.Exit(iToMove);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        System.Threading.Monitor.Exit(iFolderOwner.Processors);
                                    }
                                }
                            }
                            else if (iFolder.Processors.Count == 0)
                            {
                                lock (iFolderOwner.Processors) iFolderOwner.Processors.Remove(iFolder);
                            }
                            else
                            {
                                iFolder.ProcessorsChanged();
                            }

                            iFolderOwner.ProcessorsChanged();

                            iFolder = iFolderOwner as ProcManFolder;
                            if (iFolder != null)
                            {
                                iFolderOwner = (IProcessorsOwner)iFolder.Owner;
                            }
                            else
                            {
                                iFolderOwner = null;
                            }
                        }
                    }
                }
                else
                {
                    var iOwner = (IProcessorsOwner)Owner;
                    if (iOwner != null)
                    {
                        // this function also gets called when the processor is finishted, to remove the original processor.
                        lock (iOwner.Processors) iOwner.Processors.Remove(this);
                        iOwner.ProcessorsChanged();
                    }
                }
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<ProcItem>(ProcessorManager.Current.RemoveFromTree), 
                    this); // final resort, at end of processing, let the processormanager remove it if possible.
            }
        }

        /// <summary>The clear values.</summary>
        public override void ClearValues()
        {
            Values.Clear();
        }

        #endregion

        #region Event handlers

        /// <summary>Handles the CollectionChanged event of the <see cref="fValues"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void fValues_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasValues");
        }

        /// <summary>The proc_ procs added.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Proc_ProcsAdded(object sender, ProcsAddedEventArgs e)
        {
            IProcessorsOwner iOwner;
            lock (this)
            {
                // we put a lock on the entire object while adding procs. this will prevent another thread to move this object up in the tree while we are creating a new folder. If we don't do this, we might get a null object as owner for a short while, causing a crach.
                var iFolder = new ProcManFolder(this);

                    // important: we need to let the object know it is the root of a split, at this time, not async, otherwise the remove can fail.
                lock (iFolder.Processors)
                {
                    foreach (DebugProcessor i in e.Procs)
                    {
                        // can only add debug processors this way, need to do it at this time, because they otherwise might not be removed from the list if they are done faster than the async function is called.
                        if (i.IsRunning)
                        {
                            iFolder.Processors.Add(new ProcItem(i));
                        }
                    }

                    iOwner = (IProcessorsOwner)Owner;

                        // we get, just on time, to try and make certain we always have an owner.
                    lock (iOwner.Processors)
                    {
                        // need to make certain that the index can't change for as long as we haven't removed this item from the owner.
                        var iIndex = iOwner.Processors.IndexOf(this);
                        if (iIndex > -1)
                        {
                            // when this processor is still in the list of the owner (so still running), replace the item with the folder item.
                            System.Diagnostics.Debug.Assert(iFolder.Owner == null);
                            iOwner.Processors[iIndex] = iFolder;

                                // this will remove 'this' from the processors list, which resets 'this.owner'.
                        }
                        else
                        {
                            iOwner.Processors.Add(iFolder);
                        }
                    }

                    iFolder.Processors.Add(this);
                }
            }

            iOwner.ProcessorsChanged();
        }

        /// <summary>Handles the Finished event of all the Processors created by the
        ///     processor manager.</summary>
        /// <remarks>Makes certain that processor objects are removed from the list.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Processor_Finished(object sender, System.EventArgs e)
        {
            // this function will be called from all kinds of threads, so make certain that we udpate the UI from the UI thread.
            RemoveFromOwner();
            Processor.Finished -= Processor_Finished;
            Processor.Paused -= ProcItem_Paused;
            Processor.Continued -= ProcItem_Continued;
            Processor.ProcsAdded -= Proc_ProcsAdded;
            Processor.InvalidNeuronChange -= toWrap_InvalidNeuronChange;
            ClearInvalidChangeData();
            if (Processor.fMemProfilerData != null)
            {
                // this way, any memory profiler processor shows the same name as in the processoroverview.
                Processor.fMemProfilerData.Name = Name;
            }

            Processor = null;
        }

        /// <summary>Handles the Paused event of the ProcItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ProcItem_Paused(object sender, System.EventArgs e)
        {
            if (ProcessorManager.Current.SelectedWatchIndex > -1
                && ProcessorManager.Current.SelectedWatchIndex < ProcessorManager.Current.Watches.Count)
            {
                var iVar =
                    ProcessorManager.Current.Watches[ProcessorManager.Current.SelectedWatchIndex].NeuronInfo.Neuron as
                    Variable;
                GetValuesFor(iVar);
            }
        }

        /// <summary>Handles the Continued event of the ProcItem control.</summary>
        /// <remarks>need to make certain that any records from a previous data change that
        ///     was invalid (due to an attached neuron), are removed.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ProcItem_Continued(object sender, System.EventArgs e)
        {
            ClearInvalidChangeData();
        }

        /// <summary>Handles the InvalidNeuronChange event of the processor.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InvalidNeuronChangeEventArgs"/> instance containing
        ///     the event data.</param>
        private void toWrap_InvalidNeuronChange(object sender, InvalidNeuronChangeEventArgs e)
        {
            if (e.ProcessorWasOwner)
            {
                e.DebugData.Owner = this;
            }
            else
            {
                e.DebugData.Originator = this;
            }

            InvalidChangeData = e.DebugData;
        }

        #endregion
    }
}