// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorManager.cs" company="">
//   
// </copyright>
// <summary>
//   This class stores references to all the running processors in order to provide data for them that can be
//   used in the designer. Also manages the creation of processors for the designer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     This class stores references to all the running processors in order to provide data for them that can be
    ///     used in the designer. Also manages the creation of processors for the designer.
    /// </summary>
    /// <remarks>
    ///     This is an entry point through the stati <see cref="ProcessorManager.Current" />.
    /// </remarks>
    public class ProcessorManager : Data.ObservableObject, IProcessorFactory, IProcessorsOwner
    {
        #region inner types

        /// <summary>
        ///     The mode in which data is displayed for the debugger. (so we don't calculate everything all the time.
        /// </summary>
        public enum DisplayMode
        {
            /// <summary>
            ///     Variable data is displayed
            /// </summary>
            Variables, 

            /// <summary>
            ///     Processor data is displayed
            /// </summary>
            Processors
        }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorManager" /> class.
        /// </summary>
        public ProcessorManager()
        {
            fProcessors = new Data.ObservedCollection<ProcManItem>(this);
            fProcessors.CollectionChanged += fProcessors_CollectionChanged;
            BrainData.Current.AfterLoad += Current_AfterLoad;
            ThreadManager.Default.SplitStarting += Current_SplitPerformed;
            ThreadManager.Default.ActivityStarted += Current_ActivityStarted;
            ThreadManager.Default.ActivityStopped += Current_ActivityStopped;
        }

        #endregion

        #region Fields

        /// <summary>The maxdelay.</summary>
        private const int MAXDELAY = 400;

        /// <summary>The f selected processor.</summary>
        private ProcItem fSelectedProcessor;

        /// <summary>The f current.</summary>
        private static readonly ProcessorManager fCurrent = new ProcessorManager();

        /// <summary>The f processors.</summary>
        private readonly Data.ObservedCollection<ProcManItem> fProcessors;

        /// <summary>The f selected watch.</summary>
        private int fSelectedWatch = -1;

        /// <summary>The f displaymode.</summary>
        private DisplayMode fDisplaymode = DisplayMode.Processors;

        /// <summary>The f processor count.</summary>
        private int fProcessorCount;

        /// <summary>The f active processor count.</summary>
        private int fActiveProcessorCount;

        /// <summary>The f total processor count.</summary>
        private int fTotalProcessorCount;

        /// <summary>The f selected path.</summary>
        private SplitPath fSelectedPath;

        /// <summary>The f atttached dict.</summary>
        private readonly AttachedNeuronsDict fAtttachedDict = new AttachedNeuronsDict();

        /// <summary>The f has processors.</summary>
        private bool fHasProcessors;

        // bool fDisplayVariables;
        /// <summary>The f wait for stop.</summary>
        private readonly System.Threading.ManualResetEvent fWaitForStop = new System.Threading.ManualResetEvent(false);

        #endregion

        #region Prop

        #region Current

        /// <summary>
        ///     Gets the current processor manager.
        /// </summary>
        public static ProcessorManager Current
        {
            get
            {
                return fCurrent;
            }
        }

        #endregion

        #region Processors

        /// <summary>
        ///     Gets the list of processors currently running.  These are 'root' processors, splits are stored in the proc items.
        /// </summary>
        public Data.ObservedCollection<ProcManItem> Processors
        {
            get
            {
                return fProcessors;
            }
        }

        #endregion

        #region UIProcessors

        /// <summary>
        ///     Gets/sets the list of processors currently running, as a non observable list.   This is used for the
        ///     ui, so we can modify the processors list from other threads.
        /// </summary>
        /// <remarks>
        ///     We allow for a setter, so that we can change the list from another thread, to indicate list change.
        /// </remarks>
        public System.Collections.Generic.List<ProcManItem> UIProcessors
        {
            get
            {
                if (Debugmode != DebugMode.Off)
                {
                    // when debugging off: don't show any ui processors.
                    lock (fProcessors) return Enumerable.ToList(fProcessors);
                }

                return null;
            }
        }

        #endregion

        #region ProcessorCount

        /// <summary>
        ///     Gets/sets the total number of processors currently created since the last idle time. This is used to build the
        ///     name of new proc items.
        /// </summary>
        public int ProcessorCount
        {
            get
            {
                return fProcessorCount;
            }

            private set
            {
                fProcessorCount = value;
                OnPropertyChanged("ProcessorCount");

                // App.Current.Dispatcher.BeginInvoke(new Action<string>(OnPropertyChanged), "ProcessorCount");    //we call async, to prevent deadlocks from Ui?
            }
        }

        /// <summary>
        ///     Increments the proc count with 1 in a thread save way.
        /// </summary>
        public void IncProcCount()
        {
            System.Threading.Interlocked.Increment(ref fProcessorCount);
            OnPropertyChanged("ProcessorCount");
        }

        #endregion

        #region TotalProcessorCount

        /// <summary>
        ///     Gets/sets the total nr of processors that have been created since the start of the application.
        /// </summary>
        public int TotalProcessorCount
        {
            get
            {
                return fTotalProcessorCount;
            }

            set
            {
                fTotalProcessorCount = value;
                OnPropertyChanged("TotalProcessorCount");
            }
        }

        /// <summary>
        ///     A thread save way to increment the TotalProcessorCount by 1.
        /// </summary>
        internal void IncTotalProcessorCount()
        {
            System.Threading.Interlocked.Increment(ref fTotalProcessorCount);
            OnPropertyChanged("TotalProcessorCount");
        }

        #endregion

        #region ActiveProcessorCount

        /// <summary>
        ///     Gets/sets the total nr of active (running) processors.
        /// </summary>
        public int ActiveProcessorCount
        {
            get
            {
                return fActiveProcessorCount;
            }

            set
            {
                fActiveProcessorCount = value;
                OnPropertyChanged("ActiveProcessorCount");
            }
        }

        /// <summary>
        ///     A thread save way to increment the ActiveProcessorCount by 1.
        /// </summary>
        internal void IncActiveProcessorCount()
        {
            System.Threading.Interlocked.Increment(ref fActiveProcessorCount);
            OnPropertyChanged("ActiveProcessorCount");
        }

        /// <summary>
        ///     A thread save way to decrement the ActiveProcessorCount by 1.
        /// </summary>
        internal void DecActiveProcessorCount()
        {
            System.Threading.Interlocked.Decrement(ref fActiveProcessorCount);
            OnPropertyChanged("ActiveProcessorCount");
        }

        #endregion

        #region SelectedProcessor

        /// <summary>
        ///     Gets the curently selected processor item.
        /// </summary>
        /// <remarks>
        ///     Is automaically set when the selection is changed on the <see cref="ProcessorsOverview" /> control or when the user
        ///     control
        ///     for a proc item is activated.
        /// </remarks>
        public ProcItem SelectedProcessor
        {
            get
            {
                return fSelectedProcessor;
            }

            internal set
            {
                if (fSelectedProcessor != null)
                {
                    var iProc = fSelectedProcessor.Processor;
                    if (iProc != null && iProc.NextStatement != null && BrainData.Current.NeuronInfo != null)
                    {
                        // we check if neuroninfo is assigned, in case we are loading a new project and are stopping the previous processors in the mean time.  This can give the situation where a processor is terminated when NeuronInfo is no longer valid.
                        BrainData.Current.NeuronInfo[iProc.NextStatement.ID].IsNextStatement = false;
                    }
                }

                fSelectedProcessor = value;
                if (fSelectedProcessor != null)
                {
                    var iProc = fSelectedProcessor.Processor;
                    if (iProc != null && iProc.NextStatement != null)
                    {
                        BrainData.Current.NeuronInfo[iProc.NextStatement.ID].IsNextStatement = true;
                    }
                }

                OnPropertyChanged("SelectedProcessor");
                UpdateProcessorStats();
            }
        }

        /// <summary>The update processor stats.</summary>
        internal void UpdateProcessorStats()
        {
            if (fSelectedProcessor != null)
            {
                var iProc = fSelectedProcessor.Processor;
                if (iProc != null)
                {
                    BrainData.Current.Debugmode = iProc.DebugMode;
                    if (Displaymode == DisplayMode.Processors)
                    {
                        BuildProcData();
                    }
                    else
                    {
                        BuildVarData();
                    }
                }
                else if (Displaymode == DisplayMode.Processors)
                {
                    ClearProcData();
                }
            }
            else if (Displaymode == DisplayMode.Processors)
            {
                ClearProcData();
            }
        }

        /// <summary>The build proc data.</summary>
        private void BuildProcData()
        {
            if (fSelectedProcessor != null)
            {
                foreach (var i in Watches)
                {
                    // make certain no watch is displaying any data, there is no selected proc.
                    i.LoadValuesFor(fSelectedProcessor.Processor);
                }
            }
        }

        /// <summary>The clear var data.</summary>
        private void ClearVarData()
        {
            foreach (var i in Processors)
            {
                i.ClearValues();
            }
        }

        /// <summary>The clear proc data.</summary>
        private void ClearProcData()
        {
            if (Watches != null)
            {
                // can be null if we reload a new project while there were still processors running.
                foreach (var i in Watches)
                {
                    // make certain no watch is displaying any data, there is no selected proc.
                    if (i.Values != null)
                    {
                        i.Values.Clear();
                    }
                }
            }
        }

        /// <summary>The build var data.</summary>
        private void BuildVarData()
        {
            ClearVarData();
            if (SelectedWatchIndex > -1)
            {
                // only build the var data if there is a selected watch.
                var iVar = Watches[SelectedWatchIndex].Item as Variable;
                if (iVar != null)
                {
                    DisplayValuesFor(iVar);
                }
            }
        }

        #endregion

        #region Watches

        /// <summary>
        ///     Gets the list of variables which should be displayed in a short list for selecting to monitor.
        /// </summary>
        public System.Collections.Generic.IList<Watch> Watches
        {
            get
            {
                return BrainData.Current.DesignerData != null ? BrainData.Current.DesignerData.Watches : null;
            }
        }

        #endregion

        #region SelectedWatchIndex

        /// <summary>
        ///     Gets/sets the index of the selected variable that needs to be monitored.
        /// </summary>
        public int SelectedWatchIndex
        {
            get
            {
                return fSelectedWatch;
            }

            set
            {
                if (value != fSelectedWatch)
                {
                    if (value >= -1 && value < Watches.Count)
                    {
                        fSelectedWatch = value;
                        OnPropertyChanged("SelectedWatchIndex");
                        if (value > -1)
                        {
                            DisplayValuesFor(Watches[value].NeuronInfo.Neuron as Variable);
                        }
                        else
                        {
                            DisplayValuesFor(null);
                        }
                    }
                    else
                    {
                        throw new System.IndexOutOfRangeException();
                    }
                }
            }
        }

        #endregion

        #region SplitPaths

        /// <summary>
        ///     Gets the list of paths that have been stored to find processors that follow the same path.
        /// </summary>
        public Data.ObservedCollection<SplitPath> SplitPaths
        {
            get
            {
                return BrainData.Current.DesignerData != null ? BrainData.Current.DesignerData.SplitPaths : null;
            }
        }

        #endregion

        #region SelectedPath

        /// <summary>
        ///     Gets or sets the split path that is selected to be followed. This property is set by the split paths themselves,
        ///     when selected/deselected.
        /// </summary>
        /// <value>The selected path.</value>
        public SplitPath SelectedPath
        {
            get
            {
                return fSelectedPath;
            }

            internal set
            {
                fSelectedPath = value;
                OnPropertyChanged("SelectedPath");
            }
        }

        #endregion

        #region Debugmode

        /// <summary>
        ///     Gets/sets the currently selected debug mode.  When a processor is selected, it is the debug mode assigned to
        ///     processor, otherwise it indicates the mode used to create new processors with.
        /// </summary>
        public DebugMode Debugmode
        {
            get
            {
                if (SelectedProcessor != null && SelectedProcessor.Processor != null)
                {
                    return SelectedProcessor.Processor.DebugMode;
                }

                if (BrainData.Current.DesignerData != null)
                {
                    return BrainData.Current.Debugmode;
                }

                return DebugMode.Off;
            }

            set
            {
                if (SelectedProcessor != null)
                {
                    SelectedProcessor.Processor.DebugMode = value;
                }
                else if (BrainData.Current.DesignerData != null)
                {
                    BrainData.Current.Debugmode = value;
                }
                else
                {
                    throw new System.InvalidOperationException("No data file loaded.");
                }
            }
        }

        /// <summary>
        ///     allows the ui to be updated when the debug mode was changed.
        /// </summary>
        internal void DebugModeChanged()
        {
            OnPropertyChanged("Debugmode");
            OnPropertyChanged("IsInDebugMode");
            ProcessorsChanged();
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance is in debug mode.
        ///     Can be used to bind to boolean values.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is in debug mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsInDebugMode
        {
            get
            {
                return Debugmode != DebugMode.Off;
            }
        }

        #region PlaySpeed

        /// <summary>
        ///     Gets/sets the speed at which processor steps are played during a slow motion debug mode.
        /// </summary>
        public System.TimeSpan PlaySpeed
        {
            get
            {
                if (BrainData.Current.DesignerData != null)
                {
                    return BrainData.Current.DesignerData.PlaySpeed;
                }

                return System.TimeSpan.MinValue;
            }

            set
            {
                if (BrainData.Current.DesignerData != null)
                {
                    BrainData.Current.DesignerData.PlaySpeed = value;
                    DebugProcessor.UpdateDebugTimer(MAXDELAY - value.Milliseconds); // we need to inverse the value.
                    OnPropertyChanged("PlaySpeed");
                    OnPropertyChanged("PlaySpeedMSec");
                }
                else
                {
                    throw new System.InvalidOperationException("No data file loaded.");
                }
            }
        }

        #endregion

        #region PlaySpeedMSec

        /// <summary>
        ///     Gets or sets the play speed in milli seconds.
        /// </summary>
        /// <value>The play speed M sec.</value>
        public int PlaySpeedMSec
        {
            get
            {
                return PlaySpeed.Milliseconds;
            }

            set
            {
                PlaySpeed = new System.TimeSpan(0, 0, 0, 0, value);
            }
        }

        #endregion

        #region HasProcessors

        /// <summary>
        ///     Gets if there are curently processors alive.
        /// </summary>
        public bool HasProcessors
        {
            get
            {
                return fHasProcessors;
            }

            private set
            {
                fHasProcessors = value;
                OnPropertyChanged("HasProcessors");
            }
        }

        #endregion

        #region Displaymode

        /// <summary>
        ///     Gets/sets the mode by which data is displayed.
        ///     This allows for some optimisations regarding the calculated data.
        /// </summary>
        public DisplayMode Displaymode
        {
            get
            {
                return fDisplaymode;
            }

            set
            {
                if (fDisplaymode != value)
                {
                    fDisplaymode = value;
                    switch (value)
                    {
                        case DisplayMode.Variables:
                            ClearProcData();
                            BuildVarData();
                            break;
                        case DisplayMode.Processors:
                            BuildProcData();
                            ClearVarData();
                            break;
                        default:
                            throw new System.InvalidOperationException();
                    }

                    OnPropertyChanged("Displaymode");
                }
            }
        }

        #endregion

        #region AtttachedDict

        /// <summary>
        ///     Gets the dictionary with all the neurons that have been attached to a processor.
        /// </summary>
        internal AttachedNeuronsDict AtttachedDict
        {
            get
            {
                return fAtttachedDict;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Stops al the processors that were still running making certain that any deadlocks are also solved and making
        ///     certain that any paused
        ///     processor is first woken up again. This can not be used to terminate only some processors, it should always kill
        ///     all since
        ///     cleaning the deadlocks requires lifting all the neuronlocks, also those of still running procs.
        /// </summary>
        public void StopAndUnDeadLock()
        {
            if (HasProcessors)
            {
                fWaitForStop.Reset();
                ThreadManager.Default.StopAllProcessors();

                    // first ask to stop the processors before we do the continue, this way, all processors that start up again, will stop asap.
                ContinueAllProcs();

                    // needs to be done before the end-deadlock, cause otherwise we get stuck since 1 processor is waiting for an signal from the debugger, which is not under control of the threadmanager.
                ThreadManager.Default.EndDeadLock();

                    // we don't just stop all the processors, we make certain that any deadlock is overhauled.
                System.Action<int> iNewCheck = CheckKillCount;
                iNewCheck.BeginInvoke(0, null, null);
                fWaitForStop.WaitOne(); // we wait untill the async function lets us know that the wait is done.
            }
        }

        /// <summary>The check kill count.</summary>
        /// <param name="nrTries">The nr tries.</param>
        private void CheckKillCount(int nrTries)
        {
            nrTries++;
            if (nrTries < 500)
            {
                ContinueAllProcs(); // maybe some processors are still in pause, so try to continue them as well.
                System.Threading.Thread.Sleep(40); // crued wait of this thread on all other threads to finish.

                if (ActiveProcessorCount - ThreadManager.Default.KillBlockCount > 0)
                {
                    System.Action<int> iNewCheck = CheckKillCount;
                    iNewCheck.BeginInvoke(nrTries, null, null);
                }
                else
                {
                    fWaitForStop.Set();
                }
            }
            else
            {
                fWaitForStop.Set();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(ShowKillError));
            }
        }

        /// <summary>The show kill error.</summary>
        private void ShowKillError()
        {
            System.Windows.MessageBox.Show(
                "The network was unable to stop all processors in an appropriate time, please restart the application!", 
                "Stop processors", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>
        ///     Stops al the processors that were still running making certain that any paused processor is first woken up again.
        ///     This doesn't unblock any deadlock. It should be used when trying to stop some but not all processors.
        /// </summary>
        public void StopProcessors()
        {
            if (HasProcessors)
            {
                var iKillCount = ThreadManager.Default.KillBlockCount;

                    // need to store a local copy of this value, cause it gets reset when all are done, if we don't keep a local copy, the calculation could be wrong at the end.
                ThreadManager.Default.StopAllProcessors();
                ContinueAllProcs();
                var iNrTries = 0;
                while (ActiveProcessorCount - iKillCount > 0)
                {
                    // crued wait of this thread on all other threads to finish.
                    iNrTries++;
                    if (iNrTries < 200)
                    {
                        System.Threading.Thread.Sleep(20);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "The network was unable to stop all processors in an appropriate time, please restart the application!", 
                            "Stop processors", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Warning);
                        break;
                    }
                }
            }
        }

        /// <summary>Creates a <see cref="Processor"/> that can be used in calls to <see cref="TextSin.Process"/> or other methods
        ///     that start a main input processing thread.</summary>
        /// <remarks>This method creates a <see cref="DebugProcessor"/> so that the user can follow the translation process.  It is
        ///     also added to the <see cref="BraindData.Processors"/> list so it can be depicted.
        /// <para>
        ///         Can be called from all kinds of threads, so all UI interaction must be done async.</para>
        /// </remarks>
        /// <returns>The <see cref="Processor"/>.</returns>
        public Processor CreateProcessor()
        {
            IncProcCount();
            return new DebugProcessor();
        }

        /// <summary>Called when a processor is about to be started. This is always called after a<see cref="IProcessorFactory.CreateProcessor"/>
        ///     was called, but can also be called at other times, when a processor gets reused.</summary>
        /// <param name="proc"></param>
        public void ActivateProc(Processor proc)
        {
            var iProc = (DebugProcessor)proc;
            iProc.DoBeforeRun();

                // this was originally done during construction, but it has to be done for the recycled items as well, as late as possible, so do it like this.
            iProc.DebugMode = BrainData.Current.Debugmode;
            if (iProc.DebugMode != DebugMode.Off)
            {
                // don't update the ui when debugging not activated.
                var iNew = new ProcItem(iProc);
                lock (fProcessors) // need to make this thread save.
                    Processors.Add(iNew);
                ProcessorsChanged();
            }
        }

        /// <summary>Asks each <see cref="ProcItem"/> in <see cref="ProcessorManager.Processors"/> to calculate and display the
        ///     current
        ///     value(s) for the specified variable.</summary>
        /// <param name="toDisplay">Variable to display values for.</param>
        public void DisplayValuesFor(Variable toDisplay)
        {
            if (Displaymode == DisplayMode.Variables)
            {
                foreach (var i in Processors)
                {
                    i.GetValuesFor(toDisplay);
                }
            }
        }

        /// <summary>
        ///     Asks every processor to continue execution (<see cref="DebuProcessor.IsPaused" /> is false) and makes certain
        ///     that we don't stop because of a breakOnExcecution by disabling this temporarely.
        /// </summary>
        internal void ContinueAllProcs()
        {
            var iBreak = BrainData.Current.BreakOnException;
            BrainData.Current.BreakOnException = false;
            try
            {
                ContinueProcs(Processors);
            }
            finally
            {
                BrainData.Current.BreakOnException = iBreak;
            }
        }

        /// <summary>Continues the procs recursivelly, but only those that weren't prevented from being killed. This allows
        ///     us to use the same function for 'kill all' and 'kill all but this'.</summary>
        /// <param name="list">The list.</param>
        private void ContinueProcs(System.Collections.Generic.IList<ProcManItem> list)
        {
            foreach (var i in Enumerable.ToArray(list))
            {
                // we make a local copy of the array, cause it might get modified from while in the loop.
                var iProc = i as ProcItem;
                if (iProc != null)
                {
                    if (iProc.Processor != null && iProc.Processor.PreventKill == false)
                    {
                        // could be that processor is already dead.
                        iProc.Processor.IsPaused = false;
                    }
                }
                else
                {
                    var iFolder = i as ProcManFolder;
                    if (iFolder != null)
                    {
                        ContinueProcs(iFolder.Processors);
                    }
                    else
                    {
                        throw new System.InvalidOperationException();
                    }
                }
            }
        }

        /// <summary>The process root.</summary>
        /// <param name="e">The e.</param>
        private void ProcessRoot(SplitEventArgs e)
        {
            if (Debugmode != DebugMode.Off)
            {
                var iRoot = (DebugProcessor)e.Processor;
                var iIndex = e.ToSplit.Count - 1;
                if (iIndex >= 0)
                {
                    // don't try to build any debug data if there are no split paths.
                    var iId = e.ToSplit[iIndex].ID;
                    iRoot.SplitPath.Add(iId);

                        // the thread manager always assigns the last neuron in the list to the processor that requested the split.
                    GetAllPathsForProc(iRoot);

                        // always get all the paths it belongs to, this is the most accurate, this way we can also split on data neurons, which can change from run to run, and still pick up again afterwarths.

                    // if (iRoot.SplitPath.Count == 1)
                    // GetAllPathsForProc(iRoot);
                    // else
                    // FilterPathsForProc(iRoot);
                }
            }
        }

        /// <summary>removes all the filter paths from a processor which it is no longer following.</summary>
        /// <param name="proc">The proc.</param>
        private void FilterPathsForProc(DebugProcessor proc)
        {
            var iPathIndex = 0;

                // it's faster to update the list of path we are already following instead of finding new ones.
            var iIndex = proc.SplitPath.Count - 1;
            if (iIndex >= 0)
            {
                while (iPathIndex < proc.Paths.Count)
                {
                    var iPath = proc.Paths[iPathIndex];
                    if (iPath.Items.Count <= iIndex || iPath.Items[iIndex].ItemID != proc.SplitPath[iIndex])
                    {
                        proc.Paths.RemoveAt(iPathIndex);
                        proc.IsInCurrentPath = false; // just in case it was in there.
                    }
                    else
                    {
                        iPathIndex++;
                        if (iPath == SelectedPath)
                        {
                            proc.IsInCurrentPath = true;
                            proc.IsPaused = true;

                                // whenever we find a processor that belongs to the selected path, we pause it, so the user can check it out.
                        }
                    }
                }

                if (SelectedPath != null && iIndex < SelectedPath.Items.Count && SelectedPath.Items[iIndex].IsBreakPoint)
                {
                    proc.IsPaused = true; // we pause all processors if the item was selected as full breakpoint.
                }
            }
        }

        /// <summary>The process subs.</summary>
        /// <param name="e">The e.</param>
        private void ProcessSubs(SplitEventArgs e)
        {
            if (Debugmode != DebugMode.Off)
            {
                var iRoot = (DebugProcessor)e.Processor;
                for (var i = 0; i < e.SubProcessors.Count; i++)
                {
                    var iSub = (DebugProcessor)e.SubProcessors[i];

                        // has to be debugProcessor cause we always create the root procs, which are debugProcessors, and all subs are of the same type as the root.
                    iSub.SplitPath.AddRange(iRoot.SplitPath);
                    iSub.SplitPath.Add(e.ToSplit[i].ID);
                    foreach (var iPath in iRoot.Paths)
                    {
                        iSub.Paths.Add(iPath);
                    }

                    GetAllPathsForProc(iSub);

                        // always get all the paths it belongs to, this is the most accurate, this way we can also split on data neurons, which can change from run to run, and still pick up again afterwarths.
                }
            }
        }

        /// <summary>Gets all the split-paths that a processor belongs to.</summary>
        /// <param name="proc">The i sub.</param>
        private void GetAllPathsForProc(DebugProcessor proc)
        {
            var iIndex = proc.SplitPath.Count - 1;
            var iId = proc.SplitPath[iIndex];

            proc.Paths.Clear();
            proc.IsInCurrentPath = false;
            foreach (var iPath in SplitPaths)
            {
                if (iPath.Items.Count > iIndex && iId == iPath.Items[iIndex].ItemID)
                {
                    proc.Paths.Add(iPath);
                    if (iPath == SelectedPath)
                    {
                        proc.IsInCurrentPath = true;
                    }
                }
            }

            if (SelectedPath != null && iIndex < SelectedPath.Items.Count && SelectedPath.Items[iIndex].IsBreakPoint)
            {
                proc.IsPaused = true; // we pause all processors if the item was selected as full breakpoint.
            }
        }

        /// <summary>Removes the specified proc item from processors tree, by recursively checking if the folder owns the specified
        ///     procitem.</summary>
        /// <param name="toRemove">To remove.</param>
        internal void RemoveFromTree(ProcItem toRemove)
        {
            InternalRemoveFromTree(toRemove, Processors);
        }

        /// <summary>The internal remove from tree.</summary>
        /// <param name="toRemove">The to remove.</param>
        /// <param name="list">The list.</param>
        private void InternalRemoveFromTree(ProcItem toRemove, System.Collections.Generic.IList<ProcManItem> list)
        {
            if (list.Remove(toRemove) == false)
            {
                foreach (var i in list)
                {
                    var iFolder = i as ProcManFolder;
                    if (iFolder != null)
                    {
                        InternalRemoveFromTree(toRemove, iFolder.Processors);
                    }
                }
            }
        }

        /// <summary>
        ///     Call this when you are done changing hte processors list, so that the ui can be updated.
        /// </summary>
        public void ProcessorsChanged()
        {
            OnPropertyChanged("UIProcessors");
        }

        #endregion

        #region Event handlers

        /// <summary>Handles the AfterLoad event of the Current BrainData.</summary>
        /// <remarks>When data is loaded, need to raise property changed so that everything gets updated.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Current_AfterLoad(object sender, System.EventArgs e)
        {
            OnPropertyChanged("Watches");
            OnPropertyChanged("SplitPaths");
            OnPropertyChanged("PlaySpeed");
            OnPropertyChanged("PlaySpeedMSec");
            OnPropertyChanged("Debugmode");
        }

        /// <summary>Handles the CollectionChanged event of the fProcessors control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing
        ///     the event data.</param>
        internal void fProcessors_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Action ResetSelectedProc = () =>
                {
                    // the following statements must be done from the main UI thread.
                    SelectedProcessor = null;
                };

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (fProcessors.Count - e.NewItems.Count == 0)
                    {
                        // first add
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<string>(OnPropertyChanged), 
                            "HasProcessors");

                            // needs to be called async cause this gets called from within a lock, which can block the UI
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Contains(SelectedProcessor))
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            ResetSelectedProc);
                    }

                    if (Processors.Count == 0)
                    {
                        // when all have stopped, reset the counter.
                        ProcessorCount = 0; // so we start fresh.
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<string>(OnPropertyChanged), 
                            "HasProcessors");

                            // needs to be called async cause this gets called from within a lock, which can block the UI
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    if (System.Windows.Application.Current != null)
                    {
                        // this can be null for a reset, when the app is abruptly closed.
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            ResetSelectedProc);
                        ProcessorCount = 0; // so we start fresh.
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<string>(OnPropertyChanged), 
                            "HasProcessors");

                            // needs to be called async cause this gets called from within a lock, which can block the UI
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>Handles the SplitPerformed event of the Brain.</summary>
        /// <remarks>stores the split path in the debug processors.  This can be done from here, doesn't need special thread sync
        ///     handling since
        ///     all the sub processors are new</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.SplitEventArgs"/> instance containing the event data.</param>
        private void Current_SplitPerformed(object sender, SplitEventArgs e)
        {
            ProcessSubs(e);
            ProcessRoot(e);
        }

        /// <summary>The current_ activity stopped.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Current_ActivityStopped(object sender, System.EventArgs e)
        {
            // Brain.Current.IsEditMode = true;                                     //allow for better re-use of deleted ids
            HasProcessors = false;
            Processors.Clear(); // this is to clear out any stuff that didn't get removed properly.
            ProcessorsChanged();
        }

        /// <summary>The current_ activity started.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Current_ActivityStarted(object sender, System.EventArgs e)
        {
            // Brain.Current.IsEditMode = false;                                 //while running, make certain that the edit mode is turned off, so that buffered variable values don't cause problems
            HasProcessors = true;
        }

        #endregion
    }
}