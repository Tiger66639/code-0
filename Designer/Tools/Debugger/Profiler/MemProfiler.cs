// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemProfiler.cs" company="">
//   
// </copyright>
// <summary>
//   Data that gets stored in a processor, so we can track stuff, per
//   processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Profiler
{
    using System.Linq;

    /// <summary>
    ///     Data that gets stored in a processor, so we can track stuff, per
    ///     processor.
    /// </summary>
    internal class MemProfilerData
    {
        /// <summary>
        ///     last CurrentFrom
        /// </summary>
        public Neuron CurrentFrom;

        /// <summary>
        ///     last Currentinfo.
        /// </summary>
        public System.Collections.Generic.List<Neuron> CurrentInfo;

        /// <summary>
        ///     last CurrentMeaning
        /// </summary>
        public Neuron CurrentMeaning;

        /// <summary>
        ///     last CurrentSin
        /// </summary>
        public Neuron CurrentSin;

        /// <summary>
        ///     last CurrentTo
        /// </summary>
        public Neuron CurrentTo;

        /// <summary>
        ///     Stores if we exited from a terminator instruction (like exit solve).
        ///     This allows us to detemine if we need to store a new splitpath or not.
        /// </summary>
        public bool ExitFromTerminator = false;

        /// <summary>
        ///     Stores the location that the processor exited (but not yet completed
        ///     stopped)
        /// </summary>
        public Search.DisplayPath ExitPoint;

        /// <summary>
        ///     The name that should be used for the processor. Filled in by
        ///     <see cref="ProcItem" /> when the processor is done.
        /// </summary>
        public string Name;

        /// <summary>
        ///     This var stores the stack of variables as it was when the processor
        ///     processed it's last statement. We need this, since there is a gap
        ///     between the last statement and the moment that the processor signals
        ///     it is done. In this gab, the stack gets destroyed. This can't be
        ///     changed, it's the penalty of profiling.
        /// </summary>
        public System.Collections.Generic.List<VarDict> VariablesStack;
    }

    /// <summary>
    ///     Provides memory (neuron creation/destruction) monitoring for the network
    ///     so that you can trace leaks: temporary neurons that get created but not
    ///     destroyed somehow.
    /// </summary>
    public class MemProfiler : Data.ObservableObject
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MemProfiler" /> class.
        /// </summary>
        public MemProfiler()
        {
            Brain.Current.Cleared += Network_Cleared;
            ThreadManager.Default.ActivityStopped += Default_ActivityStopped;
        }

        #region Fields

        /// <summary>The f current.</summary>
        private static readonly MemProfiler fCurrent = new MemProfiler();

        /// <summary>The f is active.</summary>
        private bool fIsActive;

        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<MemProfiledProcessor> fItems;

        /// <summary>The f temp data.</summary>
        private readonly System.Collections.Generic.Dictionary<DebugProcessor, MemProfiledProcessor> fTempData =
            new System.Collections.Generic.Dictionary<DebugProcessor, MemProfiledProcessor>();

                                                                                                     // this dict stores the data of all the processors that are done before the entire process is done (all procs), so they still need to be processed (some neurons might still get deleted because they are frozen.

        /// <summary>The f neurons.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, MemProfilerItem> fNeurons =
            new System.Collections.Generic.Dictionary<Neuron, MemProfilerItem>();

                                                                                        // all the neurons that are currently being monitored, because they were created during execution of a thread.
        #endregion

        #region Prop

        #region Current

        /// <summary>
        ///     Gets the current memory profiler.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public static MemProfiler Current
        {
            get
            {
                return fCurrent;
            }
        }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets the wether the profiler is running or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return fIsActive;
            }

            set
            {
                if (fIsActive != value)
                {
                    fIsActive = value;
                    if (value)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }

                    OnPropertyChanged("IsActive");
                }
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of processors that had memory leaks.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public System.Collections.Generic.List<MemProfiledProcessor> Items
        {
            get
            {
                return fItems;
            }

            set
            {
                fItems = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Stops monitoring changes to the network.
        /// </summary>
        private void Deactivate()
        {
            ThreadManager.Default.AllowRecycling = true;

                // was turned recycling off, otherwise the procs loose the refs to there data and we can't build the profiler results.
            Brain.Current.NeuronChanged -= Current_NeuronChanged;
            Brain.Current.NeuronUnfrozen -= Current_NeuronUnfrozen;
            Items = null;
        }

        /// <summary>
        ///     Starts monitoring changes to the network.
        /// </summary>
        private void Activate()
        {
            ThreadManager.Default.AllowRecycling = false;

                // need to turn recycling off, otherwise the procs loose the refs to there data and we can't build the profiler results.
            Brain.Current.NeuronChanged += Current_NeuronChanged;
            Brain.Current.NeuronUnfrozen += Current_NeuronUnfrozen;
            Items = null; // so the prop is updated in ui's.
            fItems = new System.Collections.Generic.List<MemProfiledProcessor>();

                // we need a list to put the results in.
        }

        /// <summary>Handles the Cleared event of the <see cref="Current"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Network_Cleared(object sender, System.EventArgs e)
        {
            Items = null;
            fNeurons.Clear();
            fTempData.Clear();
        }

        /// <summary>Handles the ActivityStopped event of the threadmanager.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Default_ActivityStopped(object sender, System.EventArgs e)
        {
            if (IsActive)
            {
                foreach (var i in fNeurons)
                {
                    var iData = i.Value.CreatedBy.fMemProfilerData;
                    MemProfiledProcessor iProc;
                    if (fTempData.TryGetValue(i.Value.CreatedBy, out iProc) && iData != null)
                    {
                        // iData can be null when we didi a stop.
                        if (i.Key.IsDeleted == false)
                        {
                            if (iProc == null)
                            {
                                iProc = new MemProfiledProcessor(iData.ExitPoint);
                                fTempData[i.Value.CreatedBy] = iProc;
                                iProc.Name = iData.Name;
                            }

                            iProc.Items.Add(i.Value);
                            iProc.SplitPath = i.Value.CreatedBy.SplitPath;
                            i.Value.Owner = iProc; // for display, so we have quick ref to the processor value.
                            i.Value.Variables.AddRange(
                                from u in i.Value.CreatedBy.GetAllVarsFor(i.Key) select new MemProfiledVar(u));
                        }
                    }
                }

                foreach (var i in fTempData)
                {
                    if (i.Value != null && i.Value.Items.Count > 0)
                    {
                        fItems.Add(i.Value);
                    }
                }

                lock (fTempData) fTempData.Clear();
                lock (fNeurons) fNeurons.Clear();
                RefreshItems();
            }
        }

        /// <summary>The current_ neuron unfrozen.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Current_NeuronUnfrozen(object sender, FreezeEventAgrs e)
        {
            lock (fNeurons)
            {
                MemProfilerItem iItem;
                if (fNeurons.TryGetValue(e.Neuron, out iItem)
                    && ((DebugProcessor)Processor.CurrentProcessor).ExecutionFramesStack.Count > 0)
                {
                    // if the processor no longer has any execution frames, it is terminating. In that case, An unfreeze is called during the deletion of the frozen items, we don't need to register this.
                    iItem.UnfrozenAt = Search.DisplayPath.CreateFromProcessor(
                        (DebugProcessor)Processor.CurrentProcessor);
                }
            }
        }

        /// <summary>Handles the NeuronChanged event of the Network.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Current_NeuronChanged(object sender, NeuronChangedEventArgs e)
        {
            switch (e.Action)
            {
                case BrainAction.Created:
                    RegisterCreation(e.OriginalSource);
                    break;
                case BrainAction.Removed:
                    RegisterDestruction(e.OriginalSource);
                    break;
                default:
                    break;
            }
        }

        /// <summary>Registers the destruction of a neuron, so that the profiler knows it
        ///     no longer is a leak candidate.</summary>
        /// <param name="item">The item.</param>
        private void RegisterDestruction(Neuron item)
        {
            lock (fNeurons) fNeurons.Remove(item);
        }

        /// <summary>The register creation.</summary>
        /// <param name="item">The item.</param>
        private void RegisterCreation(Neuron item)
        {
            var iProc = Processor.CurrentProcessor as DebugProcessor;

                // we always need to keep a ref to the original proc cause this has the only correct callstack, while duplicating a proc.
            if (iProc != null && iProc.IsSplitting == false)
            {
                // we only do mem profiling on a processor, not on the designer + if the processor is splitting itself, we handle the duplicated items in batch later on
                if (iProc.fMemProfilerData == null)
                {
                    iProc.fMemProfilerData = new MemProfilerData();
                }

                var iItem = new MemProfilerItem(item, Search.DisplayPath.CreateFromProcessor(iProc));
                lock (fNeurons) fNeurons.Add(item, iItem);
            }
        }

        /// <summary>Processes all the duplicates created during a split. This is done
        ///     seperatly cause</summary>
        /// <param name="data">The data.</param>
        internal void ProcessSplit(ProcessorSplitter data)
        {
            var iFound = false;
            if (IsActive)
            {
                if (data.TargetClonedGlobals != null)
                {
                    foreach (var i in data.TargetClonedGlobals)
                    {
                        for (var u = 0; u < i.Value.Count; u++)
                        {
                            for (var count = 0; count < i.Value[u].Count; count++)
                            {
                                var n = i.Value[u][count];
                                if (n.ID != Neuron.TempId)
                                {
                                    // we don't register temps. cause a temp might still die correctly. We only register when it actually gets used.
                                    lock (fNeurons)

                                        // could be that the neuron was duplicated for a global and re-used, in which case it is already in the dict.
                                        iFound = fNeurons.ContainsKey(n);
                                    if (iFound == false)
                                    {
                                        var iProc = (DebugProcessor)data.Processors[count];
                                        var iItem = new MemProfilerItem(
                                            n, 
                                            Search.DisplayPath.CreateFromProcessor(iProc));
                                        iItem.DuplicatedFor = new MemProfiledVar(i.Key);
                                        iItem.CreatedFor = iProc;
                                        lock (fNeurons) fNeurons.Add(n, iItem);
                                    }
                                }
                            }
                        }
                    }
                }

                if (data.TargetNeuronToSolve != null)
                {
                    for (var i = 0; i < data.TargetNeuronToSolve.Count; i++)
                    {
                        var n = data.TargetNeuronToSolve[i];
                        if (n != null)
                        {
                            // CurrentFrom might have been deleted already, in which case we have a null.
                            lock (fNeurons)

                                // could be that the neuron was duplicated for a global and re-used, in which case it is already in the dict.
                                iFound = fNeurons.ContainsKey(n);
                            if (iFound == false)
                            {
                                var iProc = (DebugProcessor)data.Processors[i];
                                var iItem = new MemProfilerItem(n, Search.DisplayPath.CreateFromProcessor(iProc));
                                iItem.CreatedFor = iProc;
                                lock (fNeurons) fNeurons.Add(n, iItem);
                            }
                        }
                    }
                }

                if (data.TargetStacks != null)
                {
                    foreach (System.Collections.Generic.IList<Neuron> iList in data.TargetStacks)
                    {
                        for (var i = 0; i < iList.Count; i++)
                        {
                            var n = iList[i];
                            lock (fNeurons)

                                // could be that the neuron was duplicated for a global and re-used, in which case it is already in the dict.
                                iFound = fNeurons.ContainsKey(n);
                            if (iFound == false)
                            {
                                var iProc = (DebugProcessor)data.Processors[i];
                                var iItem = new MemProfilerItem(n, Search.DisplayPath.CreateFromProcessor(iProc));
                                iItem.CreatedFor = iProc;
                                lock (fNeurons) fNeurons.Add(n, iItem);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Called when a processor is done.</summary>
        /// <param name="proc">The proc.</param>
        internal void ProcessorFinised(DebugProcessor proc)
        {
            lock (fTempData) fTempData.Add(proc, null);
        }

        /// <summary>
        ///     Refreshes the items-list. We do this for the UI, since we keep using
        ///     the same list object, the UI object wont refresh.
        /// </summary>
        private void RefreshItems()
        {
            System.Collections.Generic.List<MemProfiledProcessor> iItems;
            if (Items != null)
            {
                iItems = new System.Collections.Generic.List<MemProfiledProcessor>(Items);
            }
            else
            {
                iItems = new System.Collections.Generic.List<MemProfiledProcessor>();
            }

            Items = iItems;
        }

        #endregion
    }
}