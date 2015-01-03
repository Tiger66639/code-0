// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   The different types of debug mode available.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    #region enum DebugMode

    /// <summary>
    ///     The different types of debug mode available.
    /// </summary>
    public enum DebugMode
    {
        /// <summary>
        ///     No debuging is done, no events are send about the state of the processor.
        /// </summary>
        Off, 

        /// <summary>
        ///     All events are raised and the debugger stops execution at every step.
        /// </summary>
        Normal, 

        /// <summary>
        ///     All events are raised and the debugger pauses for a short while before executing the next step.
        /// </summary>
        SlowMotion
    }

    #endregion

    #region ProcsAdded event types

    /// <summary>
    ///     Event arguments for the ProcsAdded event.
    /// </summary>
    public class ProcsAddedEventArgs : System.EventArgs
    {
        /// <summary>Gets or sets the procs.</summary>
        public Processor[] Procs { get; set; }
    }

    /// <summary>
    ///     delegate used for events that contain a list of processors.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ProcsAddedHandler(object sender, ProcsAddedEventArgs e);

    #endregion

    #region InvalidNeuronChange event types

    /// <summary>
    ///     Event arguments for the InvalidNeuronChange event.
    /// </summary>
    public class InvalidNeuronChangeEventArgs : System.EventArgs
    {
        /// <summary>Gets or sets the debug data.</summary>
        public InvalidChangeDebugData DebugData { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the neuron that was changed, was attached to the
        /// </summary>
        /// <value><c>true</c> if [processor was owner]; otherwise, <c>false</c>.</value>
        public bool ProcessorWasOwner { get; set; }
    }

    /// <summary>
    ///     delegate used for events raised when a neuron was changed in another processor than to which it was attached.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void InvalidNeuronChangeHandler(object sender, InvalidNeuronChangeEventArgs e);

    #endregion

    /// <summary>
    ///     A <see cref="Processor" /> that provides debug information.
    /// </summary>
    /// <remarks>
    ///     Each processor provides its own blocking mechanism.  So debugging is always at the level of
    ///     1 processor, no jumps between threads.  This allows you to debug a single process step after
    ///     step much easier without jumping from one process to the other.
    /// </remarks>
    public class DebugProcessor : Processor, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>Updates the debug timer used for the slowmotion.</summary>
        /// <param name="time">The new time to use for slow motion.</param>
        internal static void UpdateDebugTimer(int time)
        {
            if (fTimer != null)
            {
                fTimer.Change(time, time);
            }
        }

        /// <summary>The handle break point.</summary>
        /// <param name="toProcess">The to process.</param>
        /// <param name="index">The index.</param>
        private void HandleBreakPoint(Expression toProcess, int index)
        {
            if (DebugMode != DebugMode.Off && fBreakSwitch != null && ThreadManager.Default.StopRequested == false)
            {
                // only try to do a breakpoint if there is no stop requested, cause this would cause the whole thing to hang: a breakpoint needs a user interaction, but the UI is blocked for as long as there are still processors runnin.
                if (SelectedFrame != null)
                {
                    // is null when a 'return' is done.
                    SelectedFrame.NextIndex = index;

                        // we must update this manually each time, otherwise we loose this record, cause the processor doesn't store this info all the time in it's callframes, so we can't regenerate it. SelectedFrame is changed in this thread, so it is ok.
                }

                // we don't need a lock round this, breakpoints has some internal thread syncing stuff.
                if (BrainData.Current.BreakPoints.Contains(toProcess))
                {
                    // if this is a breakpoint, we always need to pause processing.
                    BrainData.Current.BreakPoints.OnBreakPointReached(toProcess, this);
                    SignalBreakSwitch();
                }

                if (fUIUpdateRequested)
                {
                    UnlockCallFrames();

                        // In case we pause inside a LockExpression, we need to unfreeze all items, otherwise, the ui might freeze up.
                    SetPausedValue(true); // just before we pause, let the ui know.
                    if (IsUIActive)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Expression, Neuron, int>(UpdateUI), 
                            toProcess, 
                            NeuronToSolve, 
                            CurrentLink);
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Expression>(SetNextStatement), 
                            toProcess);
                    }

                    fBreakSwitch.WaitOne();
                    RelockCallFrames();

                        // when we start again, don't forget to lock the items again that we have unlocked for the UI.
                    if (IsUIActive)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Expression, Neuron, int>(UpdateUI), 
                            null, 
                            NeuronToSolve, 
                            CurrentLink);

                            // we reset again after the break, so that it doesn't store a possible bogus value
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Expression>(SetNextStatement), 
                            null); // we reset again after the break, so that it doesn't store a possible bogus value
                    }

                    SetPausedValue(false);
                    fUIUpdateRequested = false;
                }

                if (fBreakNextStep || DebugMode == DebugMode.SlowMotion)
                {
                    SignalBreakSwitch();

                        // don't reset BreakNextStep here, this is done after the code has been processed, so that the processor knows about the breakOnNextStep when the statement causes a split.
                }
            }
        }

        /// <summary>The signal break switch.</summary>
        private void SignalBreakSwitch()
        {
            fUIUpdateRequested = true; // so the UI can be updated.
            fBreakSwitch.Reset();
        }

        /// <summary>Sets the <see cref="DebugProcessor.NextStatement"/>, so it can be changed in the UI thread.</summary>
        /// <param name="next">The next.</param>
        private void SetNextStatement(Expression next)
        {
            if (IsRunning)
            {
                // it could be that this proc has been killed while this command was on the UI dispatcher's list.
                NextStatement = next;
            }
        }

        /// <summary>Updates all the UI dat objects to represent the internal state of the processor so it can be displayed.
        ///     This function should only be called from within the UI thread.</summary>
        /// <param name="next">The next.</param>
        /// <param name="toSolve"></param>
        /// <param name="curLink"></param>
        private void UpdateUI(Expression next, Neuron toSolve, int curLink)
        {
            if (next != null)
            {
                if (MeaningsToExec.Count > curLink && curLink > -1)
                {
                    // when a split is resolved, we don't have a link anymore.
                    SelectedMeaning = MeaningsToExec[curLink];
                }
                else
                {
                    SelectedMeaning = null;
                }

                OnPropertyChanged("NeuronToSolve");
                OnPropertyChanged("VariablesDisplay");
                OnPropertyChanged("GlobalsDisplay");
                OnPropertyChanged("ExecutionFrames");
                OnPropertyChanged("StackDisplay");
            }

            NextStatement = next;
        }

        /// <summary>Gets all variables that hold a reference to the specified neuron by the prcoessor.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<Variable> GetAllVarsFor(Neuron item)
        {
            System.Collections.Generic.IEnumerable<VarDict> iVars;
            if (fMemProfilerData != null && fMemProfilerData.VariablesStack != null)
            {
                iVars = fMemProfilerData.VariablesStack;
            }
            else
            {
                iVars = Mem.VariableValues;
            }

            foreach (var i in iVars)
            {
                foreach (var u in i)
                {
                    foreach (var n in u.Value.Data)
                    {
                        if (n == item)
                        {
                            yield return u.Key;
                        }
                    }
                }
            }

            foreach (var u in GlobalValues)
            {
                foreach (var n in u.Value)
                {
                    if (n == item)
                    {
                        yield return u.Key;
                    }
                }
            }

            if (item
                == (NeuronToSolve != null || fMemProfilerData == null ? NeuronToSolve : fMemProfilerData.CurrentFrom))
            {
                yield return Brain.Current[(ulong)PredefinedNeurons.CurrentFrom] as Variable;
            }

            if (item == (NeuronToSolve != null || fMemProfilerData == null ? CurrentTo : fMemProfilerData.CurrentTo))
            {
                yield return Brain.Current[(ulong)PredefinedNeurons.CurrentTo] as Variable;
            }

            if (item == (NeuronToSolve != null || fMemProfilerData == null ? CurrentSin : fMemProfilerData.CurrentSin))
            {
                yield return Brain.Current[(ulong)PredefinedNeurons.CurrentSin] as Variable;
            }

            if (item
                == (NeuronToSolve != null || fMemProfilerData == null ? CurrentMeaning : fMemProfilerData.CurrentMeaning))
            {
                yield return Brain.Current[(ulong)PredefinedNeurons.CurrentMeaning] as Variable;
            }

            System.Collections.Generic.List<Neuron> iCurrentInfo = null;
            if (fMemProfilerData != null && fMemProfilerData.CurrentInfo != null)
            {
                iCurrentInfo = fMemProfilerData.CurrentInfo;
            }
            else if (CurrentInfo != null)
            {
                iCurrentInfo = CurrentInfo.ToList();
            }

            if (iCurrentInfo != null && iCurrentInfo.Contains(item))
            {
                yield return Brain.Current[(ulong)PredefinedNeurons.CurrentInfo] as Variable;
            }
        }

        #region Inner types

        /// <summary>
        ///     Wraps a debugNeuron together with it's split value weight.
        /// </summary>
        public class SplitResultItem
        {
            /// <summary>
            ///     Gets or sets the weight.
            /// </summary>
            /// <value>The weight.</value>
            public double Weight { get; set; }

            /// <summary>
            ///     Gets or sets the item.
            /// </summary>
            /// <value>The item.</value>
            public DebugNeuron Item { get; set; }
        }

        #endregion

        #region Fields

        /// <summary>The f pause for exception.</summary>
        private bool fPauseForException;

                     // used to let a debugProcessor know that an exception was logged for which we need to break execution.

        /// <summary>The f paused.</summary>
        private bool fPaused;

                     // a manual switch which stores that the user pressed the pause button. This is used to temporarely block the timer routine that allows the slow motion video play.

        /// <summary>The f ui update requested.</summary>
        private bool fUIUpdateRequested;

                     // allows to let the processor run faster by only updating the screen when he is paused.

        /// <summary>The f break switch.</summary>
        private System.Threading.ManualResetEvent fBreakSwitch;

                                                  // used for step processing, so we can wait for the ui thread to signal a continue..

        /// <summary>The f break next step.</summary>
        private bool fBreakNextStep; // switch used for a step to next expression.

        /// <summary>The f timer.</summary>
        private static System.Threading.Timer fTimer; // used for auto play debug mode.

        /// <summary>The f executing neuron.</summary>
        private DebugNeuron fExecutingNeuron;

        /// <summary>The f execution frames stack.</summary>
        private readonly System.Collections.Generic.Stack<ExecutionFrame> fExecutionFramesStack =
            new System.Collections.Generic.Stack<ExecutionFrame>();

                                                                          // we use an extra stack together with the observableCollection so that we can always  update SelectedFrame from the proc's thread correctly, while providing UI through observable, which can be updated in UI thread, on demand, when needed.

        /// <summary>The f selected frame.</summary>
        private ExecutionFrame fSelectedFrame;

        /// <summary>The f is paused.</summary>
        private bool fIsPaused; // stores the actual value if the processor is paused or not

        /// <summary>The f is running.</summary>
        private bool fIsRunning = true;

        /// <summary>The f meanings to exec.</summary>
        private System.Collections.Generic.List<DebugNeuron> fMeaningsToExec =
            new System.Collections.Generic.List<DebugNeuron>();

        /// <summary>The f selected meaning.</summary>
        private DebugNeuron fSelectedMeaning;

        /// <summary>The f next statement.</summary>
        private Expression fNextStatement;

        /// <summary>The f stack size.</summary>
        private int fStackSize; // keeps track of the current nr of items on the stack, for UI display

        /// <summary>The f split path.</summary>
        private readonly System.Collections.Generic.List<ulong> fSplitPath =
            new System.Collections.Generic.List<ulong>();

        /// <summary>The f paths.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<SplitPath> fPaths =
            new System.Collections.ObjectModel.ObservableCollection<SplitPath>();

        /// <summary>The f is in current path.</summary>
        private bool fIsInCurrentPath;

        /// <summary>The f is ui active.</summary>
        private bool fIsUIActive; // true when the ui is displaying data for this proc, so we need to update the UI

        /// <summary>The f mem profiler data.</summary>
        internal volatile Profiler.MemProfilerData fMemProfilerData;

                                                   // in case we are profiling, we want to keep track of some data, processor local.
        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugProcessor" /> class.
        /// </summary>
        public DebugProcessor()
        {
            IsSplitting = false;
            DebugMode = BrainData.Current.Debugmode;
        }

        /// <summary>Initializes a new instance of the <see cref="DebugProcessor"/> class.</summary>
        /// <param name="mode">The initial debug mode to use.</param>
        public DebugProcessor(DebugMode mode)
        {
            IsSplitting = false;
            DebugMode = mode;
        }

        /// <summary>
        ///     Performs some initialization tasks that should be called just before a processor is about to run.
        /// </summary>
        internal void DoBeforeRun()
        {
            if (DebugMode != DebugMode.Off)
            {
                fBreakSwitch = new System.Threading.ManualResetEvent(true);
                if (DebugMode == DebugMode.SlowMotion)
                {
                    if (fTimer == null)
                    {
                        var iMSec = (int)ProcessorManager.Current.PlaySpeed.TotalMilliseconds;
                        fTimer = new System.Threading.Timer(OnTimer, null, iMSec, iMSec);
                    }
                }
            }

            ProcessorManager.Current.IncTotalProcessorCount();
            ProcessorManager.Current.IncActiveProcessorCount();
        }

        /// <summary>
        ///     Initializes the <see cref="DebugProcessor" /> class so that everything is set up for debugging.
        /// </summary>
        public static void Init()
        {
            WPFLog.WPFLog.Default.PreviewWriteToLog += Log_PreviewWriteToLog;

                // we use this event cause this is triggered in the same thread as where the log happened, which we need to check if there is a processor present in the thread.
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the processor is paused because of a breakpoint or new step in a slowmotion.
        /// </summary>
        public event System.EventHandler Paused;

        /// <summary>
        ///     Occurs when the processor continues after it was paused.
        /// </summary>
        public event System.EventHandler Continued;

        /// <summary>
        ///     Raised when the TextSin has got some text it wants to output.
        /// </summary>
        public event ProcsAddedHandler ProcsAdded;

        /// <summary>
        ///     Raised when a neuron was changed in another processor than to which it was attached.
        /// </summary>
        public event InvalidNeuronChangeHandler InvalidNeuronChange;

        #endregion

        #region Prop

        #region DebugMode

        /// <summary>
        ///     Gets the current debugging mode. This can be used to find out if this is a slowmotion timer.
        /// </summary>
        public DebugMode DebugMode { get; internal set; }

        #endregion

        #region SplitPath

        /// <summary>
        ///     Gets the list of neuron ids that define the path of this processor.
        ///     At a split, each processor gets a unique value assigned.  New processors inherit the path
        ///     of their root processor (the one that started the split). This info is usefull during debug
        ///     sessions, to find the processors that cause a problem.
        /// </summary>
        public System.Collections.Generic.List<ulong> SplitPath
        {
            get
            {
                return fSplitPath;
            }
        }

        #endregion

        #region SplitWeight

        /// <summary>
        ///     Gets the current split weight (for display purposes only).
        /// </summary>
        /// <value>The split weight.</value>
        public double SplitWeight
        {
            get
            {
                return SplitValues.CurrentWeight;
            }
        }

        #endregion

        #region SplitValuesForDebug

        #region SplitResults

        /// <summary>
        ///     Gets the list of curent split values, for displaying.
        /// </summary>
        public System.Collections.Generic.List<SplitResultItem> SplitResults
        {
            get
            {
                var iRes = new System.Collections.Generic.List<SplitResultItem>();
                foreach (var i in SplitValues.GetAllValues())
                {
                    var iNew = new SplitResultItem();
                    iNew.Item = new DebugNeuron(i.Key);
                    iNew.Weight = i.Value;
                    iRes.Add(iNew);
                }

                return iRes;
            }
        }

        #endregion

        #endregion

        /// <summary>
        ///     Gets the split path, represented as a string in reverse order. (the most important item is usually at the end.
        /// </summary>
        /// <value>The split path as string.</value>
        public string ReverseSplitPathAsString
        {
            get
            {
                var iBuilder = new System.Text.StringBuilder();
                if (SplitPath.Count > 0)
                {
                    iBuilder.Append(BrainData.Current.NeuronInfo[SplitPath[SplitPath.Count - 1]].DisplayTitle);
                    for (var i = SplitPath.Count - 2; i >= 0; i--)
                    {
                        iBuilder.Append("/" + BrainData.Current.NeuronInfo[SplitPath[i]].DisplayTitle);
                    }
                }
                else
                {
                    iBuilder.Append("root");
                }

                return iBuilder.ToString();
            }
        }

        #region ExecutionFrames

        /// <summary>
        ///     Gets the list of execution frames for this processor.
        /// </summary>
        /// <remarks>
        ///     An <see cref="ExecutionFrame" /> contains all the info of 1
        ///     function that is called.  Since a function can call another function, we must keep
        ///     a stack of all currently called functions, hence this stack.
        /// </remarks>
        public System.Collections.Generic.IList<ExecutionFrame> ExecutionFrames
        {
            get
            {
                var iRes = new System.Collections.Generic.List<ExecutionFrame>();
                foreach (var i in Enumerable.Reverse(ExecutionFramesStack))
                {
                    // we need to reverse the stack content cause otherwise we get the last frame as first in the list.
                    iRes.Add(i);
                }

                return iRes;
            }
        }

        #endregion

        #region Paths

        /// <summary>
        ///     Gets the list of <see cref="SplitPath" />s that this processor is following.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<SplitPath> Paths
        {
            get
            {
                return fPaths;
            }
        }

        #endregion

        #region SelectedFrame

        /// <summary>
        ///     Gets/sets the currently selected execution frame.
        /// </summary>
        public ExecutionFrame SelectedFrame
        {
            get
            {
                return fSelectedFrame;
            }

            internal set
            {
                if (value != fSelectedFrame)
                {
                    if (fSelectedFrame != null)
                    {
                        fSelectedFrame.IsSelected = false;
                    }

                    fSelectedFrame = value;
                    if (fSelectedFrame != null)
                    {
                        fSelectedFrame.IsSelected = true;
                    }

                    // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(OnPropertyChanged), "SelectedFrame");
                    OnPropertyChanged("SelectedFrame");

                        // this should be thread save cause property changed works accross threads.
                }
            }
        }

        #endregion

        #region StackDisplay

        /// <summary>
        ///     Gets the list of stack items currently used by the processor.
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> StackDisplay
        {
            get
            {
                var fStack = new System.Collections.Generic.List<DebugNeuron>();
                foreach (var i in NeuronStack)
                {
                    fStack.Add(new DebugNeuron(i, false));
                }

                return fStack;
            }
        }

        #endregion

        #region VariablesDisplay

        /// <summary>
        ///     Gets/sets the list of variables currently active for this processor.
        /// </summary>
        public System.Collections.Generic.IList<VariableValue> VariablesDisplay
        {
            get
            {
                var iRes = new System.Collections.Generic.List<VariableValue>();
                foreach (var i in Mem.VariableValues)
                {
                    foreach (var u in i)
                    {
                        iRes.Add(new VariableValue(u));
                    }
                }

                return iRes;
            }
        }

        #endregion

        #region GlobalsDisplay

        /// <summary>
        ///     Gets the list of globals currently active in the processor.
        /// </summary>
        public System.Collections.Generic.IList<VariableValue> GlobalsDisplay
        {
            get
            {
                var iRes = new System.Collections.Generic.List<VariableValue>();
                foreach (var u in GlobalValues)
                {
                    iRes.Add(new VariableValue(u));
                }

                return iRes;
            }
        }

        #endregion

        #region ArgumentsDisplay

        /// <summary>
        ///     Gets the list of globals currently active in the processor.
        /// </summary>
        public System.Collections.Generic.IList<FunctionValue> ArgumentsDisplay
        {
            get
            {
                var iRes = new System.Collections.Generic.List<FunctionValue>();
                var i = 0;
                foreach (var u in Mem.ParametersStack)
                {
                    iRes.Add(new FunctionValue(i++, u));
                }

                return iRes;
            }
        }

        #endregion

        #region ReturnValuesDisplay

        /// <summary>
        ///     Gets the list of globals currently active in the processor.
        /// </summary>
        public System.Collections.Generic.IList<FunctionValue> ReturnValuesDisplay
        {
            get
            {
                var iRes = new System.Collections.Generic.List<FunctionValue>();
                var i = 0;
                foreach (var u in Mem.LastReturnValues)
                {
                    iRes.Add(new FunctionValue(i++, u));
                }

                return iRes;
            }
        }

        #endregion

        #region ExecutingNeuron

        /// <summary>
        ///     Gets the neuron currently being executed.
        /// </summary>
        public DebugNeuron ExecutingNeuron
        {
            get
            {
                return fExecutingNeuron;
            }

            internal set
            {
                if (value != fExecutingNeuron)
                {
                    fExecutingNeuron = value;
                    if (value != null)
                    {
                        // Display CurrentFrom statically, as it was when it got compiled.
                        value.IsMonitorEnabled = false;
                    }

                    OnPropertyChanged("ExecutingNeuron");

                        // this should be thread save cause property changed works accross threads.
                }
            }
        }

        #endregion

        #region StackSize

        /// <summary>
        ///     Gets/sets the number of items currently on the stack.
        /// </summary>
        /// <remarks>
        ///     This is used to display in WPF.
        /// </remarks>
        public int StackSize
        {
            get
            {
                return fStackSize;
            }

            internal set
            {
                fStackSize = value;
                OnPropertyChanged("StackSize");
            }
        }

        #endregion

        #region IsPaused

        /// <summary>
        ///     Gets/sets if the debugger is curently waiting for the user to continue the execution.
        /// </summary>
        /// <remarks>
        ///     Use this prop to pause the processor.
        /// </remarks>
        public bool IsPaused
        {
            get
            {
                return fIsPaused;
            }

            set
            {
                if (value != fIsPaused)
                {
                    if (value)
                    {
                        DebugPause();
                    }
                    else
                    {
                        DebugContinue();
                    }
                }
            }
        }

        /// <summary>Sets the actual paused value to the field + raises events.  This function is called from seperate threads.
        ///     This is the actual setter so that we have a real view of the value, not what we wanted.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void SetPausedValue(bool value)
        {
            fIsPaused = value;
            if (value)
            {
                var iSelected = ProcessorManager.Current.SelectedProcessor;
                if (iSelected != null && iSelected.Processor == this)
                {
                    // if this is the currently selected processor, update the state of the visualisations for the selected proc (this will recalcluate the variable values displayed in the editor.
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(ProcessorManager.Current.UpdateProcessorStats));

                        // need async, this function gets called from different threads.
                }

                OnPaused();
            }
            else
            {
                OnContinue();
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "IsPaused");
        }

        #endregion

        #region IsInCurrentPath

        /// <summary>
        ///     Gets wether this debug processor is still following the currently selected path or not.
        ///     This is used to provide a visual queue.
        /// </summary>
        public bool IsInCurrentPath
        {
            get
            {
                return fIsInCurrentPath;
            }

            internal set
            {
                fIsInCurrentPath = value;
                OnPropertyChanged("IsInCurrentPath");
            }
        }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets/sets the the processor is running or not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return fIsRunning;
            }

            set
            {
                fIsRunning = value;
                OnPropertyChanged("IsRunning");
            }
        }

        #endregion

        #region MeaningsToExec

        /// <summary>
        ///     Gets the list of links that are being executed (the links of the currently executing neuron).
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> MeaningsToExec
        {
            get
            {
                return fMeaningsToExec;
            }

            internal set
            {
                if (value != fMeaningsToExec)
                {
                    fMeaningsToExec = value;
                    OnPropertyChanged("MeaningsToExec");

                        // this should be thread save cause property changed works accross threads.
                }
            }
        }

        #endregion

        #region SelectedMeaning

        /// <summary>
        ///     Gets/sets the neuron - meaning that is currently being executed.
        /// </summary>
        public DebugNeuron SelectedMeaning
        {
            get
            {
                return fSelectedMeaning;
            }

            set
            {
                if (value != fSelectedMeaning)
                {
                    if (fSelectedMeaning != null)
                    {
                        fSelectedMeaning.IsSelected = false;
                    }

                    fSelectedMeaning = value;
                    if (fSelectedMeaning != null)
                    {
                        fSelectedMeaning.IsSelected = true;
                    }

                    OnPropertyChanged("SelectedMeaning");
                }
            }
        }

        #endregion

        #region NextStatement

        /// <summary>
        ///     Gets/sets the statement that will be executed next.
        /// </summary>
        public Expression NextStatement
        {
            get
            {
                return fNextStatement;
            }

            private set
            {
                if (value != fNextStatement)
                {
                    var iSelectedProc = ProcessorManager.Current.SelectedProcessor;
                    if (iSelectedProc != null && iSelectedProc.Processor == this && fNextStatement != null
                        && BrainData.Current.NeuronInfo != null)
                    {
                        // neuronInfo can be null if this was called just in between the loading of a new project, (so there were stil processors running when load was requested).
                        BrainData.Current.NeuronInfo[fNextStatement.ID].IsNextStatement = false;
                    }

                    fNextStatement = value;
                    if (iSelectedProc != null && iSelectedProc.Processor == this && fNextStatement != null
                        && BrainData.Current.NeuronInfo != null)
                    {
                        // neuronInfo can be null if this was called just in between the loading of a new project, (so there were stil processors running when load was requested).
                        BrainData.Current.NeuronInfo[fNextStatement.ID].IsNextStatement = true;
                    }

                    OnPropertyChanged("NextStatement");
                }
            }
        }

        #endregion

        #region IsUIActive

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        internal bool IsUIActive
        {
            get
            {
                return fIsUIActive;
            }

            set
            {
                fIsUIActive = value;
                if (value)
                {
                    OnPropertyChanged("ExecutingNeuron");
                    OnPropertyChanged("MeaningsToExec");
                    UpdateUI(NextStatement, NeuronToSolve, CurrentLink);
                }
            }
        }

        /// <summary>
        ///     Gets the execution frames stack. We provide access to this stack so we can build a display path, based on
        ///     the current exeuction location, in case of an error that gets logged.
        /// </summary>
        /// <value>The execution frames stack.</value>
        internal System.Collections.Generic.Stack<ExecutionFrame> ExecutionFramesStack
        {
            get
            {
                return fExecutionFramesStack;
            }
        }

        #endregion

        #region IsSplitting

        /// <summary>
        ///     Gets the wether this processor is currently in the process of splitting.
        /// </summary>
        /// <remarks>
        ///     This is used by the MemProfiler to block the registration of new neurons (the duplicates),
        ///     so that they can be processed in group later on (for the correct procs).
        /// </remarks>
        public bool IsSplitting { get; internal set; }

        #endregion

        #endregion

        #region functions

        #region Event raisers

        /// <summary>Called when new processors were added to the list.</summary>
        /// <param name="toAdd">The proces that werer added.</param>
        protected void OnProcsAdded(Processor[] toAdd)
        {
            var iEvent = ProcsAdded;
            if (iEvent != null)
            {
                iEvent(this, new ProcsAddedEventArgs { Procs = toAdd });
            }
        }

        /// <summary>
        ///     Called when the processor is paused. Triggers the event.
        /// </summary>
        protected virtual void OnPaused()
        {
            if (Paused != null)
            {
                Paused(this, System.EventArgs.Empty);
            }
        }

        /// <summary>The on continue.</summary>
        protected virtual void OnContinue()
        {
            if (Continued != null)
            {
                Continued(this, System.EventArgs.Empty);
            }
        }

        /// <summary>The on attached neuron changed.</summary>
        /// <param name="data">The data.</param>
        internal void OnAttachedNeuronChanged(InvalidChangeDebugData data)
        {
            IsPaused = true; // stop the processor causing the problems
            var iEvent = InvalidNeuronChange;
            if (iEvent != null)
            {
                iEvent(this, new InvalidNeuronChangeEventArgs { DebugData = data, ProcessorWasOwner = true });
            }
        }

        /// <summary>The on invalid neuron change.</summary>
        /// <param name="data">The data.</param>
        internal void OnInvalidNeuronChange(InvalidChangeDebugData data)
        {
            IsPaused = true; // stop the processor causing the problems
            var iEvent = InvalidNeuronChange;
            if (iEvent != null)
            {
                iEvent(this, new InvalidNeuronChangeEventArgs { DebugData = data, ProcessorWasOwner = false });
            }
        }

        #endregion

        /// <summary>
        ///     signals the debug processor that the next execution step can be performed.
        /// </summary>
        /// <remarks>
        ///     This simply sets an auto event.
        /// </remarks>
        public void DebugContinue()
        {
            if (fBreakSwitch != null)
            {
                NextStatement = null;

                    // reset the next statement, cause we are doing a run, so this can be cleared again.
                fPaused = false;
                fBreakSwitch.Set();
            }
        }

        /// <summary>The debug pause.</summary>
        public void DebugPause()
        {
            if (fBreakSwitch != null)
            {
                fPaused = true;

                    // this lets the object know a manual pause was requested, so that the timer can skip auto continue.
                SignalBreakSwitch();
            }
        }

        /// <summary>The debug step next.</summary>
        public void DebugStepNext()
        {
            if (fBreakSwitch != null)
            {
                fPaused = false;
                fBreakNextStep = true;
                fBreakSwitch.Set();
            }
        }

        /// <summary>The on timer.</summary>
        /// <param name="timer">The timer.</param>
        private void OnTimer(object timer)
        {
            if (fPaused == false)
            {
                fBreakSwitch.Set();
            }
        }

        /// <summary>Handles the PreviewWriteToLog event of the Log control.</summary>
        /// <remarks>This checks if there is a processor defined in the thread that changes the list.  If so, the log item is comming
        ///     from the
        ///     execution engine, so attach a tag to it that allows us to find the neuron again that triggered the log item.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WPFLog.LogItemEventArgs"/> instance containing the event data.</param>
        /// <returns>True, indicating that the log should always happen.</returns>
        private static bool Log_PreviewWriteToLog(object sender, WPFLog.LogItemEventArgs e)
        {
#if !(BASIC || PRO) // basic and pro version don't allow jumping to code bits.
         if (CurrentProcessor != null)
         {
            DebugProcessor iCur = CurrentProcessor as DebugProcessor;
            DisplayPath iPath = DisplayPath.CreateFromProcessor(iCur);
            e.Item.Tag = iPath;
            if (iCur != null && BrainData.Current.BreakOnException == true && ThreadManager.Default.StopRequested == false)            {

// only stop if we are not trying to kill all the processors, cause this would hang the application.
               iCur.fPauseForException = true;
               iCur.DebugPause();
               iCur.SetPausedValue(true);
               App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<LogItem>(DisplayDebugData), e.Item);
            }
         }
#endif
            return true;
        }

        /// <summary>Displays the debug data. We can't call WindowMain.Current from another thread, so therefor this helper function.</summary>
        /// <param name="data">The data.</param>
        private static void DisplayDebugData(LogService.LogItem data)
        {
            ((Search.DisplayPath)data.Tag).SelectPathResult();
            System.Windows.MessageBox.Show("Processing has been halted because of the following error: " + data.Text);
        }

        #endregion

        #region Overwrites 

        /// <summary>Tries to solve the specified neuron.</summary>
        /// <param name="toSolve"></param>
        /// <remarks><para>Solving is done by walking down every 'To' link in the neuron and pushing this to reference on the stack after
        ///         which it calls
        ///         the <see cref="Link.Meaning"/>'s <see cref="Neuron.Actions"/> list.</para>
        /// <para>this is an internal function, cause the output instruction needs to be able to let a neuron be solved
        ///         sequential (<see cref="Processor.Solve"/> is async and solves the intire stack.</para>
        /// </remarks>
        protected override void SolveNeuron(Neuron toSolve)
        {
            UpdateSolvableNeuron(toSolve);

                // request ui update at the time that the solve is requested to make certain we display the exact dat that is being solved.
            StackSize -= 1;
            base.SolveNeuron(toSolve);
        }

        /// <summary>Updates the UI part to display the currently solvable neuron and it's links.</summary>
        /// <param name="toSolve">To solve.</param>
        private void UpdateSolvableNeuron(Neuron toSolve)
        {
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                if (toSolve != null)
                {
                    fExecutingNeuron = new DebugNeuron(toSolve, false);
                    System.Collections.Generic.List<ulong> iMeanings;
                    using (var iLinks = toSolve.LinksOut)
                        iMeanings = (from i in iLinks select i.MeaningID).ToList();

                            // we extract the id in a temp list to make certain that we don't cause any dead locks: when retrieving the meaning, we lock the brain, if another thread tries to get a lock on this list, while it has the brain locked, we get a dead lock.  this needs to be avoided.
                    fMeaningsToExec = (from i in iMeanings select new DebugNeuron(Brain.Current[i], false)).ToList();
                }
                else
                {
                    fExecutingNeuron = null;
                    fMeaningsToExec = new System.Collections.Generic.List<DebugNeuron>();
                }

                if (IsUIActive)
                {
                    OnPropertyChanged("ExecutingNeuron");
                    OnPropertyChanged("MeaningsToExec");
                }
            }
        }

        /// <summary>Returns the evalulation of the condition.</summary>
        /// <param name="compareTo">The posssible list of neurons that must be found through the condition.</param>
        /// <param name="expression"></param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <remarks>If there is a <b>compareTo</b> value specified, a case statement is presumed, and all the
        ///     neurons in the compareTo must also be in Condition (after solving a possible expression) and no more.
        ///     If compareTo is null, a boolean expression is presumed and it's result is returned or an error logged
        ///     and false returned.
        ///     When the condition is empty, true is always presumed.</remarks>
        protected override bool EvaluateCondition(System.Collections.Generic.IEnumerable<Neuron> compareTo, 
            ConditionalExpression expression, 
            int index)
        {
            try
            {
                HandleBreakPoint(expression, index);
                return base.EvaluateCondition(compareTo, expression, index);
            }
            finally
            {
                fBreakNextStep = false;

                    // reset after item was executed, this way, a split can copy the fBreakNextStep to the subprocessors (otherwise it doesn't know about it cause it has been reset).
            }
        }

        /// <summary>Pushes the frame on the call-frame stack.</summary>
        /// <param name="frame">The frame.</param>
        public override void PushFrame(CallFrame frame)
        {
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                var iNew = new ExecutionFrame(frame);
                ExecutionFramesStack.Push(iNew);
                SelectedFrame = iNew; // needs to be set in this thread, to keep proper track of the value
            }

            base.PushFrame(frame);
        }

        /// <summary>Pops the last frame on the call-frame stack. Makes certain that the pop will also remove
        ///     any variables that were registered because of the frame.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame PopFrame()
        {
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                if (ExecutionFramesStack.Count > 0)
                {
                    ExecutionFramesStack.Pop();
                    SelectedFrame = ExecutionFramesStack.Count > 0 ? ExecutionFramesStack.Peek() : null;
                }
                else
                {
                    SelectedFrame = null;
                }
            }

            return base.PopFrame();
        }

        /// <summary>
        ///     Clears all the frames from the call-frame stack.
        /// </summary>
        public override void ClearFrames()
        {
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                ExecutionFramesStack.Clear();
                SelectedFrame = null;
            }

            base.ClearFrames();
        }

        /// <summary>
        ///     resets all the data in the processor so that it can be reused again.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            fPauseForException = false;
            IsSplitting = false;
            fPaused = false;
            fUIUpdateRequested = false;
            DebugMode = default(DebugMode);
            fBreakSwitch = null;
            fBreakNextStep = false;
            fExecutingNeuron = null;
            fExecutionFramesStack.Clear();
            fSelectedFrame = null;
            fIsPaused = default(bool);
            fIsRunning = true;
            fMeaningsToExec.Clear();
            fSelectedMeaning = null;
            fNextStatement = null;
            fStackSize = 0;
            fSplitPath.Clear();
            fPaths.Clear();
            fIsInCurrentPath = false;
            fIsUIActive = default(bool);
            fMemProfilerData = null;
        }

        /// <summary>If there is a wait event, it means we need to block execution untill we get a signal to continue.</summary>
        /// <param name="toProcess"></param>
        protected override void Process(Expression toProcess)
        {
            try
            {
                var iNext = 0; // calculating NextExp is used a lot, it's faster if we only do when required.
                Search.DisplayPath iPath = null;
                if (DebugMode != DebugMode.Off)
                {
                    // when debugging is turned off, don't create any debug-neurons.
                    iNext = NextExp;
                    SelectedFrame.CurrentIndex = iNext;
                    HandleBreakPoint(toProcess, iNext);
                    iPath = GetDisplayPathForProfiler(toProcess, iNext);
                }

                base.Process(toProcess);
                if (DebugMode != DebugMode.Off)
                {
                    // when debugging is turned off, don't create any debug-neurons.
                    if (fPauseForException)
                    {
                        fPauseForException = false;
                        HandleBreakPoint(toProcess, iNext);
                    }

                    HandleProfiler(toProcess, iNext, iPath);
                }
            }
            finally
            {
                fBreakNextStep = false;

                    // reset after item was executed, this way, a split can copy the fBreakNextStep to the subprocessors (otherwise it doesn't know about it cause it has been reset).
            }
        }

        /// <summary>The get display path for profiler.</summary>
        /// <param name="toProcess">The to process.</param>
        /// <param name="prevNext">The prev next.</param>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        private Search.DisplayPath GetDisplayPathForProfiler(Expression toProcess, int prevNext)
        {
            Search.DisplayPath iRes = null;
            if (fMemProfilerData != null)
            {
                if (toProcess.IsTerminator
                    || (SelectedFrame != null && SelectedFrame.Frame.Code != null
                        && SelectedFrame.Frame.Code.Count - 1 == prevNext && NeuronStack.Count == 0))
                {
                    if (fMemProfilerData.ExitFromTerminator == false)
                    {
                        // when exiting a stack, we get several calls, but we take from the deepest level, so make certain that we don't overwrite the display path with less important versions.
                        iRes = Search.DisplayPath.CreateFromProcessor(this);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Performs all operations for the profiler when a single instruction is executed.</summary>
        /// <param name="toProcess">To process.</param>
        /// <param name="prevNext">The prev Next.</param>
        /// <param name="path">The path.</param>
        private void HandleProfiler(Expression toProcess, int prevNext, Search.DisplayPath path)
        {
            if (fMemProfilerData != null)
            {
                if (toProcess.IsTerminator
                    || (SelectedFrame != null && SelectedFrame.Frame.Code != null
                        && SelectedFrame.Frame.Code.Count - 1 == prevNext && NeuronStack.Count == 0))
                {
                    if (fMemProfilerData.ExitFromTerminator == false)
                    {
                        fMemProfilerData.ExitFromTerminator = toProcess.IsTerminator;
                        fMemProfilerData.ExitPoint = path;
                        fMemProfilerData.VariablesStack =
                            new System.Collections.Generic.List<VarDict>(Mem.VariableValues);

                            // we make a copy of the last stack, so we can find vars that contained leaks.
                        fMemProfilerData.CurrentFrom = NeuronToSolve;
                        if (CurrentInfo != null)
                        {
                            fMemProfilerData.CurrentInfo = CurrentInfo.ToList();
                        }

                        fMemProfilerData.CurrentMeaning = CurrentMeaning;
                        fMemProfilerData.CurrentSin = CurrentSin;
                        fMemProfilerData.CurrentTo = CurrentTo;
                    }
                }
            }
        }

        /// <summary>Call this method if you want to remove something from the internal stack.
        ///     for internal use only.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        /// <remarks>This method only removes the item from the stack.  It is provided so that
        ///     inheriters can do something more during a stack change, like let observers know.</remarks>
        public override Neuron Pop()
        {
            StackSize = NeuronStack.Count - 1;
            return base.Pop();
        }

        /// <summary>Pushes the specified neuron on it's internal stack.</summary>
        /// <param name="toPush">The neuron to add.</param>
        public override void Push(Neuron toPush)
        {
            base.Push(toPush);
            StackSize = NeuronStack.Count;
        }

        /// <summary>Returns an array of the specified size, filled with processors.</summary>
        /// <param name="count">The nr of processors to create.</param>
        /// <returns>An array of newly created processors (not initialized).</returns>
        /// <remarks>Descendents should reimplement this so they can give another type (for debugging).</remarks>
        public override Processor[] CreateProcessors(int count)
        {
            var iRes = new Processor[count];
            while (count > 0)
            {
                var iSub = (DebugProcessor)ProcessorFactory.GetSubProcessor();
                iSub.DebugMode = DebugMode;
                iSub.DoBeforeRun();
                if (fBreakNextStep && iSub.fBreakSwitch != null)
                {
                    iSub.DebugPause();
                }

                iSub.StackSize = StackSize;
                iRes[count - 1] = iSub; // -1 cause the index is 1 less than the size.
                count--;
            }

            OnProcsAdded(iRes);
            return iRes;
        }

        /// <summary>
        ///     Called when the processor is finished with solving the stack. Raises the <see cref="Processor.Finished" /> event.
        /// </summary>
        protected override void OnFinished()
        {
            base.OnFinished();
            ProcessorManager.Current.DecActiveProcessorCount();
            IsRunning = false;
            SelectedMeaning = null;
            ProcessorManager.Current.AtttachedDict.ProcessorFinished(this);
            if (Profiler.MemProfiler.Current.IsActive)
            {
                Profiler.MemProfiler.Current.ProcessorFinised(this);
            }
        }

        /// <summary>The kill.</summary>
        public override void Kill()
        {
            var iBreak = BrainData.Current.BreakOnException;

                // need to make certain that we don't try to break while killing this one.
            BrainData.Current.BreakOnException = false;
            try
            {
                base.Kill();
                IsPaused = false; // we need to make certain that we are not paused so that the kill can take place.
            }
            finally
            {
                BrainData.Current.BreakOnException = iBreak;
            }
        }

        /// <summary>Clones this processor to the specified processors, thereby letting the subprocessors know they are clones.</summary>
        /// <param name="procs">The procs.</param>
        /// <param name="isAccum">if set to <c>true</c> [is accum].</param>
        /// <returns>The <see cref="ProcessorSplitter"/>.</returns>
        /// <remarks>We need to override this and set items here which we can't assign during the<see cref="DebugProcessor.CreateProcessors"/> overwrite</remarks>
        protected override ProcessorSplitter CloneProcessors(Processor[] procs, bool isAccum = false)
        {
            IsSplitting = true;
            try
            {
                var iRes = base.CloneProcessors(procs);
                Profiler.MemProfiler.Current.ProcessSplit(iRes);
                ProcessorManager.Current.AtttachedDict.ProcessSplit(iRes);
                var iExecutingNeuron = ExecutingNeuron;
                foreach (DebugProcessor i in procs)
                {
                    var iNeuronToSolve = i.NeuronToSolve;
                    if (iExecutingNeuron != null && iNeuronToSolve != null)
                    {
                        i.ExecutingNeuron = iExecutingNeuron.Duplicate(iNeuronToSolve);
                    }
                    else
                    {
                        i.ExecutingNeuron = null;
                    }

                    i.MeaningsToExec = MeaningsToExec;
                }

                return iRes;
            }
            finally
            {
                IsSplitting = false;
            }
        }

        #region Attached neurons

        /// <summary>Stores the value for the speicified global.</summary>
        /// <param name="global"></param>
        /// <param name="value"></param>
        protected override void StoreGlobalValue(Global global, System.Collections.Generic.List<Neuron> value)
        {
            base.StoreGlobalValue(global, value);
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                ProcessorManager.Current.AtttachedDict.ProcessVarForWatch(global, value, this);
            }
        }

        /// <summary>Stores the value for the speicified variable.</summary>
        /// <param name="var">The variable to store a new value for.</param>
        /// <param name="value">The value to assign to the var.</param>
        protected override void StoreVariableValue(Variable var, System.Collections.Generic.List<Neuron> value)
        {
            base.StoreVariableValue(var, value);
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                ProcessorManager.Current.AtttachedDict.ProcessVarForWatch(var, value, this);
            }
        }

        /// <summary>Stores the value for the speicified local. (this is a faster way for locals).
        ///     This is provided for debuggers so they can overwrite if needed.</summary>
        /// <param name="loc">The local</param>
        /// <param name="store">where to store the data.</param>
        /// <param name="value">The data to store.</param>
        protected override void StoreVariableValue(
            Local loc, 
            VarValuesList store, System.Collections.Generic.List<Neuron> value)
        {
            base.StoreVariableValue(loc, store, value);
            if (DebugMode != DebugMode.Off)
            {
                // when debugging is turned off, don't create any debug-neurons.
                ProcessorManager.Current.AtttachedDict.ProcessVarForWatch(loc, value, this);
            }
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>The property changed.</summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises the <see cref="DebugProcessor.PropertyChanged"/> event.</summary>
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name)
        {
            var iEvent = PropertyChanged; // this is done to make it thread save.
            if (iEvent != null)
            {
                iEvent(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}