// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Processor.cs" company="">
//   
// </copyright>
// <summary>
//   Used to let inheriters know which list is being executed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    #region Enums

    /// <summary>
    ///     Used to let inheriters know which list is being executed.
    /// </summary>
    /// <remarks>
    ///     Since we always know at compile time which list we are trying to call,
    ///     we can easely pass this info along without to much overhead to the
    ///     designer for instance.
    /// </remarks>
    public enum ExecListType
    {
        /// <summary>The rules.</summary>
        Rules, 

        /// <summary>The actions.</summary>
        Actions, 

        /// <summary>The children.</summary>
        Children, 

        /// <summary>
        ///     This type is only used by execution frames (visual aids used by the designer) at the moment
        /// </summary>
        Conditional, 

        /// <summary>
        ///     No code list defined.
        /// </summary>
        None
    }

    /// <summary>
    ///     Determins the processing direction that the processor is currently handling.
    /// </summary>
    /// <remarks>
    ///     The process direction always starts with input. This indicates that input data is being processed.
    ///     At a certain point, the brain can execute an <see cref="OutputIntruction" />.  This changes the
    ///     direction of the process to generate output data.
    /// </remarks>
    public enum ProcessDirection
    {
        /// <summary>The input.</summary>
        Input, 

        /// <summary>The output.</summary>
        Output
    }

    #endregion

    #region Exceptions

    /// <summary>
    ///     Used to allow a processor to terminate processing quickly from anywhere in the code: simply raise the exception.
    /// </summary>
    /// <remarks>
    ///     Using an exception to let all the processors stop is ok since this is a situation that shouldn't happen to often
    ///     and
    ///     is only time critical to set up, not to call.  By using the exception system for this, we can also handle this
    ///     gracefully
    ///     with respect to frozen neurons and others.
    /// </remarks>
    [System.Serializable]
    public class ProcessorStopException : System.Exception
    {
        // For guidelines regarding the creation of new exception types, see
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

        /// <summary>Initializes a new instance of the <see cref="ProcessorStopException"/> class.</summary>
        public ProcessorStopException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProcessorStopException"/> class.</summary>
        /// <param name="message">The message.</param>
        public ProcessorStopException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProcessorStopException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ProcessorStopException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProcessorStopException"/> class.</summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ProcessorStopException(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion

    /// <summary>
    ///     Used by the <see cref="Brain" /> object to perform translations from 1 type of <see cref="Neuron" /> to another
    ///     using
    ///     <see cref="Neuron.Rules" /> and <see cref="Neuron.Actions" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A processor tries to solve all the neurons on it's stack for as long as there remains a neuron on it.  If a
    ///         solve run is complete
    ///         for a specific neuron, and there is something still on the stack, it continues to solve that neuron, and so on.
    ///     </para>
    ///     <para>
    ///         The property <see cref="Processor.SplitData" /> determins if it is running as the child of another processor
    ///         due to a <see cref="Processor.Split" />
    ///         operation. Sub processors are used to solve conflicts between multiple possibilities by trying out different
    ///         scenarios at the same
    ///         time.
    ///     </para>
    ///     <para>
    ///         After the split has been resolved, the last processor continues processing.
    ///     </para>
    ///     <para>
    ///         Descendents should reimplement CreateProcessors.
    ///     </para>
    ///     <para>
    ///         When a processor is finished, the <see cref="Processor.Finished" /> event is raised.
    ///     </para>
    /// </remarks>
    public class Processor
    {
        /// <summary>
        ///     resets all the data in the processor so that it can be reused again.
        /// </summary>
        public virtual void Reset()
        {
            fNeuronStack.Clear();
            fState = State.Normal;
            fCallFrameStack.Clear();
            CurrentLink = 0;
            CurrentLinks = null;
            CurrentMeaning = null;
            CodeCluster = null;
            CurrentTo = null;
            CurrentInfo = null;
            fNeuronToSolve = null;
            Thread = null;

            fNeedsBlockHandleRelease = true;
            fSplitData = null;
            fSplitValues.Clear();
            CurrentSin = null;
            ThreadNeuron = null;
            fThreadBlocker = null;
            fFrozenNeurons.Clear();
            fPreventKill = default(bool);
            BlockedProcessor = null;
            Mem.Clear(this);
            TreeFinished = null;

                // we always reset this callback when done, to make certain that there are no leaks. when clear is called, the callback should already have been called.
        }

        /// <summary>The check stop requested.</summary>
        /// <exception cref="ProcessorStopException"></exception>
        internal void CheckStopRequested()
        {
            if ((ThreadManager.Default.StopRequested && PreventKill == false) || KillRequested)
            {
                // this will allow us to check every frame if a stop was requested, this should be sufficiently grained.
                KillRequested = false;
                throw new ProcessorStopException();
            }
        }

        #region internal types

        /// <summary>
        ///     Defines the different states that a processor can have. This is primarely used for split, continue, break and exit
        ///     requests.
        /// </summary>
        public enum State
        {
            /// <summary>The normal.</summary>
            Normal, 

            /// <summary>The break.</summary>
            Break, 

            /// <summary>The exit.</summary>
            Exit, 

            /// <summary>The exit neuron.</summary>
            ExitNeuron, 

            /// <summary>
            ///     When  the procesor is no longer running.
            /// </summary>
            Terminated, 

            /// <summary>
            ///     when it was not possible to convert the neuron to someting else.
            /// </summary>
            NotUnderstood
        }

        /// <summary>
        ///     Used by the processor to store the contents of all the outgoing links on the currently executing processor.
        ///     This is used to build a buffer, so the processor can remember all the links while processing them (this way,
        ///     they can be changed, while preserving the execution order of the current run.
        /// </summary>
        internal class LinkContent
        {
            /// <summary>Gets or sets the info.</summary>
            public System.Collections.Generic.List<Neuron> Info { get; set; }

            /// <summary>Gets or sets the to.</summary>
            public Neuron To { get; set; }

            /// <summary>Gets or sets the meaning.</summary>
            public Neuron Meaning { get; set; }
        }

        /// <summary>
        ///     This class is used as a temp store for all the arguments that are required when this processor is cloned to other
        ///     processors.
        /// </summary>
        public class ProcessorCloneData
        {
            /// <summary>The frames.</summary>
            public System.Collections.Generic.List<CallFrame> Frames;

            /// <summary>The stack.</summary>
            public System.Collections.Generic.List<Neuron> Stack;
        }

        /// <summary>
        ///     a delegate type for calling a function when an entire tree of processors is done. This is a delegate and not an
        ///     event so we can
        ///     pass the function ref along to other processors (and reset it again).
        /// </summary>
        public delegate void TreeFinishedCallback();

        #endregion

        #region fields

        /// <summary>
        ///     Stores all the neurons that need to be executed.
        /// </summary>
        private readonly System.Collections.Generic.Stack<Neuron> fNeuronStack =
            new System.Collections.Generic.Stack<Neuron>(Settings.InitProcessorStackSize);

        /// <summary>The f call frame stack.</summary>
        private readonly System.Collections.Generic.Stack<CallFrame> fCallFrameStack =
            new System.Collections.Generic.Stack<CallFrame>();

        /// <summary>The f neuron to solve.</summary>
        private Neuron fNeuronToSolve;

                       // stores a ref to the neuron we are solving, important for passing along to sub processors.

        /// <summary>The f split data.</summary>
        private volatile SplitData fSplitData;

                                   // contains data for sub procesors, also indicates that this is a sub processor.

        /// <summary>The f split values.</summary>
        private volatile SplitResultsDict fSplitValues = new SplitResultsDict();

                                          // contains all the split result values currently assigned to this processor.

        /// <summary>The f thread blocker.</summary>
        private System.Threading.AutoResetEvent fThreadBlocker;

        /// <summary>The f frozen neurons.</summary>
        private readonly System.Collections.Generic.HashSet<Neuron> fFrozenNeurons =
            new System.Collections.Generic.HashSet<Neuron>();

                                                                    // contains all the neurons that have been frozen within the context of this processor.

        /// <summary>The f state.</summary>
        private State fState = State.Normal;

        /// <summary>The f prevent kill.</summary>
        private volatile bool fPreventKill;

        /// <summary>The f needs block handle release.</summary>
        private bool fNeedsBlockHandleRelease = true;

        /// <summary>The f split allowed.</summary>
        private bool fSplitAllowed = true;

        /// <summary>
        ///     keeps track if this proc can be recycled when it's done or not: when part of a split, the
        ///     entire split needs to be done before it can be recycled.
        /// </summary>
        /// <summary>
        ///     this stack is a temporary store to pass over parameter values to the inside of a function.
        /// </summary>
        public MemoryFactory Mem;

        /// <summary>The f current processor.</summary>
        [System.ThreadStatic]
        public static Processor fCurrentProcessor; // must be a field, can't use ThreadStatic otherwise.

        #endregion

        #region Events

        /// <summary>
        ///     Raised when the processor is finished processing.
        /// </summary>
        public event System.EventHandler Finished;

        /// <summary>
        ///     this delegate is called when this processor is finished processing and, if the proc was part of a split, no more
        ///     other processors of the split
        ///     are running anymore.
        ///     This is required so that the <see cref="Sin.CallSinDestroyEvent" /> can be blocked until the processing is done.
        ///     It is not an event but a delegate (function pointer), so that we can easily pass along this value from the root
        ///     processor to all the subs + reset
        ///     the reference if the processor is done but not the last one (save resources).
        /// </summary>
        public TreeFinishedCallback TreeFinished;

        #endregion

        #region prop

        #region CurrentProcessor

        /// <summary>
        ///     This static has a unique value for each thread.  It contains which <see cref="Processor" /> is currently
        ///     running in the current thread.  This is used by the <see cref="Brain.Add" /> to check for valid
        ///     operations.
        /// </summary>
        public static Processor CurrentProcessor
        {
            get
            {
                return fCurrentProcessor;
            }

            internal set
            {
                if (fCurrentProcessor != null)
                {
                    fCurrentProcessor.Thread = null;
                }

                fCurrentProcessor = value;
                if (value != null)
                {
                    value.Thread = System.Threading.Thread.CurrentThread;
                }
            }
        }

        #endregion

        #region Thread

        /// <summary>
        ///     Gets the thread that this processor is currently running in (primarely used so we can do a dump).
        /// </summary>
        public System.Threading.Thread Thread { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether a kill was requested (but the processor is still running).
        /// </summary>
        /// <value><c>true</c> if [kill requested]; otherwise, <c>false</c>.</value>
        public bool KillRequested { get; internal set; }

        #region SplitAllowed

        /// <summary>
        ///     Gets/sets the wether this processor allows a split instruction or not. This is used so that some callbacks can't
        ///     perform a split.
        ///     A split is allowed by default in normal processors.
        /// </summary>
        public bool SplitAllowed
        {
            get
            {
                return fSplitAllowed;
            }

            set
            {
                fSplitAllowed = value;
            }
        }

        #endregion

        #region PreventKill

        /// <summary>
        ///     Gets/sets the wether this processor should not be killed, during a global kill process. This allows
        ///     for 'kill all but this' functionility. All Processors that had this property set to true, wont be stopped.
        /// </summary>
        /// <remarks>
        ///     Warning: when a processor is prevented from a kill through this property, you need to manually reset it after
        ///     the kill action was performed, also when the proc died.
        /// </remarks>
        public bool PreventKill
        {
            get
            {
                return fPreventKill;
            }

            set
            {
                if (value != fPreventKill)
                {
                    fPreventKill = value;
                    if (value)
                    {
                        ThreadManager.Default.KillBlockCountInc();

                            // thread save way to increment the nr of threads that are blocked from stopping.
                    }
                    else
                    {
                        ThreadManager.Default.KillBlockCountDec();
                    }
                }
            }
        }

        /// <summary>
        ///     Resets the prevent kill, without triggering an update in the treadmanager (from which this is called
        ///     once the kill-all is done).
        /// </summary>
        internal void ResetPreventKill()
        {
            fPreventKill = false;
        }

        #endregion

        #region NeuronToSolve

        /// <summary>
        ///     Gets the Neuron currently being solved.
        /// </summary>
        /// <remarks>
        ///     This can be usefull to do queries from.  It is considered to be an always valid variable (
        ///     or better a constant) from within <see cref="Instruction" />s and search criteria, so from
        ///     within neural programs.
        /// </remarks>
        public virtual Neuron NeuronToSolve
        {
            get
            {
                return fNeuronToSolve;
            }

            internal set
            {
                fNeuronToSolve = value;
            }
        }

        #endregion

        #region GlobalValues

        /// <summary>
        ///     Gets the dictionary containing all the global values valid for this processor.
        /// </summary>
        public System.Collections.Generic.Dictionary<Global, System.Collections.Generic.List<Neuron>> GlobalValues
        {
            get
            {
                return Mem.GlobalValues;
            }
        }

        #endregion

        #region CurrentState

        /// <summary>
        ///     Gets the current state of the brain.
        /// </summary>
        /// <value>The state of the current.</value>
        internal State CurrentState
        {
            get
            {
                return fState;
            }

            set
            {
                fState = value;
            }
        }

        #endregion

        #region NeedsBlockHandleRelease

        /// <summary>
        ///     Gets the value that indicates if this processor still has a waitHandle for a 'blockedSolve' which needs to be set.
        ///     It can't
        ///     do this itself since setting the handle can only be done after the thread has been released.
        ///     By default, this is true, only when this processor was part of a split and is not the last one in the split, should
        ///     the handle
        ///     not be set.
        /// </summary>
        internal bool NeedsBlockHandleRelease
        {
            get
            {
                return fNeedsBlockHandleRelease;
            }

            set
            {
                fNeedsBlockHandleRelease = value;
            }
        }

        #endregion

        #region SplitData

        /// <summary>
        ///     Gets or sets the split data.
        /// </summary>
        /// <remarks>
        ///     contains data for sub procesors, also indicates that this is a sub processor.
        ///     Must be internal so that it can't be seen by reflector.
        /// </remarks>
        /// <value>The split data.</value>
        internal SplitData SplitData
        {
            get
            {
                return fSplitData;
            }

            set
            {
                fSplitData = value;
            }
        }

        #endregion

        #region SplitValues

        /// <summary>
        ///     Gets the dictionary containing all the split result values for this processor. This is a thread safe accessible
        ///     dictionary.
        /// </summary>
        /// <remarks>
        ///     Because this is a dictionary, it is not possible to store the same neuron 2 times.  This guarantees that the list
        ///     always contains unique items.
        /// </remarks>
        /// <value>The split values.</value>
        protected internal SplitResultsDict SplitValues
        {
            get
            {
                return fSplitValues;
            }
        }

        #endregion

        #region NextExp

        /// <summary>
        ///     Gets/sets the index of the <see cref="Expression" /> that will be executed next.
        /// </summary>
        /// <remarks>
        ///     This points to the next expression for the split.
        ///     Changing this value will cause a jump in the execution position.
        /// </remarks>
        public int NextExp
        {
            get
            {
                if (CallFrameStack.Count > 0)
                {
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null)
                    {
                        return iFrame.NextExp;
                    }
                }

                return -1;
            }

            set
            {
                if (CallFrameStack.Count > 0)
                {
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null)
                    {
                        iFrame.NextExp = value;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("Can't change execution positions: no frame present.");
                }
            }
        }

        #endregion

        #region CurrentExpression

        /// <summary>
        ///     Gets the current expression that is being (or just was) executed.
        /// </summary>
        /// <value>The current expression.</value>
        public Expression CurrentExpression
        {
            get
            {
                var iId = NextExp;

                    // we always point to the next step already when a step is executing (this is done for the split).
                if (CurrentCode != null && iId >= 0 && iId < CurrentCode.Count)
                {
                    return CurrentCode[iId];
                }

                return null;
            }
        }

        #endregion

        #region CurrentLink

        /// <summary>
        ///     Gets/sets the index of the link that is currently being processed in a <see cref="Processor.Solve" />  operation.
        /// </summary>
        /// <remarks>
        ///     Changing this value will cause a jump in the execution position.
        /// </remarks>
        public int CurrentLink { get; set; }

        #endregion

        #region CurrentLinks

        /// <summary>
        ///     Gets the list of all the links that are currently being executed.
        /// </summary>
        /// <remarks>
        ///     These are stored at the class level so we can pass them along during a split.  If we don't do this, a sub processor
        ///     could get screwed up if the currently executing neuron gets changed when he is picking up after the split.
        /// </remarks>
        internal System.Collections.Generic.List<LinkContent> CurrentLinks { get; set; }

        #endregion

        #region CurrentSin

        /// <summary>
        ///     Gets/sets the Sin that triggered this processor / for which it is generating data.
        /// </summary>
        /// <remarks>
        ///     This is stored in the processor cause a processor can only have 1 current sin and is
        ///     local for the process.
        ///     It is provided so that the <see cref="CurrentSin" /> system variable can easely find this
        ///     value.
        ///     <para>
        ///         This var is automatically filled in once a processor starts.  It can be changed by the
        ///         <see cref="OutputInstruction" /> when it turns the processor into an output generator.
        ///     </para>
        /// </remarks>
        public Sin CurrentSin { get; set; }

        #endregion

        #region CurrentMeaning

        /// <summary>
        ///     Gets/sets the meaning neuron that is currently being executed in order to solve the
        ///     <see cref="Processor.NeuronToSolve" />.
        /// </summary>
        public Neuron CurrentMeaning { get; internal set; }

        #endregion

        #region CodeCluster

        /// <summary>
        ///     Gets the cluster that actually contained to code that was initially called for the current link.
        /// </summary>
        public NeuronCluster CodeCluster { get; internal set; }

        #endregion

        #region CurrentTo

        /// <summary>
        ///     Gets the the neuron pointed too by the currently executing link.
        /// </summary>
        public Neuron CurrentTo { get; internal set; }

        #endregion

        #region BlockedWaitHandle

        /// <summary>
        ///     Gets the wait handle that should be waited on when this processor is started from a blockedSolve instruction, to
        ///     find
        ///     out when the processor and all of it's subs have finished. So this only gets signalled when all processors, created
        ///     by a split
        ///     have also finished.
        /// </summary>
        /// <summary>
        ///     Gets or sets the processor that is blocked and waiting for this thread to finish. This is together with the
        ///     BlockWaithandle.
        ///     We store the other processor here, so we can change the Threadmanager's lists in 1 lock at the end of this thread
        ///     instead of 2 locks.
        /// </summary>
        /// <value>
        ///     The blocked processor.
        /// </value>
        internal Processor BlockedProcessor { get; set; }

        #endregion

        #region CurrentInfo

        /// <summary>
        ///     Gets the list with the currently valid info neurons or null if there is none.
        /// </summary>
        public System.Collections.Generic.List<Neuron> CurrentInfo { get; internal set; }

        #endregion

        #region CurrentCode

        /// <summary>
        ///     Gets the list of code that is currently being executed.
        /// </summary>
        /// <remarks>
        ///     This is stored for splits.  This way, code before the split argument can change the currently executing code
        ///     while still keeping the original code for the remainder of the function as would be the case when there was
        ///     no split.  If we don't store this, a split would result in the recalculation of the current code, which might
        ///     differ from the original processor's code.
        /// </remarks>
        public System.Collections.Generic.IList<Expression> CurrentCode
        {
            get
            {
                if (CallFrameStack.Count > 0)
                {
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null)
                    {
                        return iFrame.Code;
                    }
                }

                return null;
            }
        }

        #endregion

        #region CurrentExpressionSource

        /// <summary>
        ///     Gets the Neuron that contains the code currently being executed.  Which type of code that is being executed (the
        ///     list), can
        ///     be found throught <see cref="Processor.CurrentExecListType" />
        /// </summary>
        /// <remarks>
        ///     This is primarely provided for debuggers and designers.
        /// </remarks>
        public Neuron CurrentExecSource
        {
            get
            {
                if (CallFrameStack.Count > 0)
                {
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null)
                    {
                        return iFrame.ExecSource;
                    }
                }

                return null;
            }
        }

        #endregion

        #region CurrentExecListType

        /// <summary>
        ///     Gets the type of list that is currently being executed, which is attached to
        ///     <see cref="Processor.CurrentExpressionSource" />.
        /// </summary>
        /// <remarks>
        ///     This is primarely provided for debuggers and designers.
        /// </remarks>
        public ExecListType CurrentExecListType
        {
            get
            {
                if (CallFrameStack.Count > 0)
                {
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null)
                    {
                        return iFrame.CodeListType;
                    }
                }

                return ExecListType.None;
            }
        }

        #endregion

        #region Neuronstack

        /// <summary>
        ///     the data stack.
        /// </summary>
        protected internal System.Collections.Generic.Stack<Neuron> NeuronStack
        {
            get
            {
                return fNeuronStack;
            }
        }

        #endregion

        #region CallStack

        /// <summary>
        ///     Gets the stack containing the callframe data (for conditionals and sub routines). (dont use directly)
        /// </summary>
        /// <remarks>
        ///     don't use this stack directly, use <see cref="Processor.PushFrame" /> and <see cref="Processor.PopFrame" />
        ///     instead.
        /// </remarks>
        internal System.Collections.Generic.Stack<CallFrame> CallFrameStack
        {
            get
            {
                return fCallFrameStack;
            }
        }

        #endregion

        /// <summary>
        ///     For a BlockedCall instruction, we need to be able to pass along which cluster needs to be called by the new
        ///     processor.
        ///     This prop passes the value along.
        /// </summary>
        internal NeuronCluster ToCall { get; set; }

        #region Count

        /// <summary>
        ///     Gets the number of neurons currently on the stack.
        /// </summary>
        /// <value>The nr of items on the stack.</value>
        public int Count
        {
            get
            {
                return fNeuronStack.Count;
            }
        }

        #endregion

        #region ThreadNeuron

        /// <summary>
        ///     Gets the Neuron that represents the thread in which this processor is running.
        /// </summary>
        public Neuron ThreadNeuron { get; set; }

        #endregion

        #region ThreadBlocker

        /// <summary>
        ///     Gets the object to block, restart the processor using the <see cref="AwakeInstruction" /> and
        ///     <see cref="SuspendInstruction" /> .
        /// </summary>
        public System.Threading.AutoResetEvent ThreadBlocker
        {
            get
            {
                if (fThreadBlocker == null)
                {
                    fThreadBlocker = new System.Threading.AutoResetEvent(false);
                }

                return fThreadBlocker;
            }
        }

        #endregion

        #endregion

        #region Functions

        #region Solve/call/branch

        /// <summary>
        ///     Tries to solve all the <see cref="Neuron" />s currently on the stack untill the stack is empty, untill there is
        ///     only 1 neuron
        ///     left that can't be solved anymore or untill <see cref="Process.Exit" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Inheriters should reinplement <see cref="Processor.InternalSolve" /> since this is called by all solving
        ///         routines (this and
        ///         <see cref="Processor.SolveBlocked" />).
        ///     </para>
        ///     this method simply starts an internal thread.
        /// </remarks>
        public virtual void Solve()
        {
            ThreadManager.Default.ReserveThread(this, ThreadRequestType.Normal);
        }

        /// <summary>
        ///     Solves the content of the processor and waits untill it is completly solved.
        /// </summary>
        public virtual void SolveBlocked()
        {
            ThreadManager.Default.ReserveThread(this, ThreadRequestType.Blocked);

                // this will start a thread and let this one wait. We need to start a new thread cause a processor sets some thread global variables.  This is just safer to let it run in a different thread.
        }

        /// <summary>starts the specified code cluster in a new processor (not necesarely a new thread) and waits untill the call is
        ///     done.</summary>
        /// <param name="toCall"></param>
        public virtual void CallBlocked(NeuronCluster toCall)
        {
            ToCall = toCall;
            ThreadManager.Default.ReserveThread(this, ThreadRequestType.Blocked);

                // this will start a thread and let this one wait. We need to start a new thread cause a processor sets some thread global variables.  This is just safer to let it run in a different thread.
        }

        /// <summary>
        ///     pops last item from stack to solve, solves it and continues untill there is nothing more on the stack.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Actual solving is done by <see cref="Processor.SolveStack" />.  This one mostly takes care of the current
        ///         processor.
        ///     </para>
        /// </remarks>
        internal void InternalSolve()
        {
            try
            {
                CurrentProcessor = this; // we need to indicate that this thread is run for this processor
                try
                {
#if DEBUG && !ANDROID // android doesn't like giving names to the treads.
                    if (string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
                    {
                        System.Threading.Thread.CurrentThread.Name = "Processor";

                            // this is for easy debugging, so we recognise the thread in VS.
                    }

#endif
                    SolveStack();
                }
                catch (ProcessorStopException)
                {
                    FinishSplitOnException();
                }
                catch (System.Exception e)
                {
                    // we need a catch all handler since this is the entry point of the thread.
                    LogService.Log.LogError("Processor.InternalSolve", string.Format("Internal error: {0}.", e));
                    FinishSplitOnException();
                }

                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    OnFinished();
                }
            }
            finally
            {
                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    CurrentProcessor = null;
                }
            }
        }

        /// <summary>Starts the processor for a timer. This will call the cluster, after which a normal stack operation
        ///     is started, if there are still items left.</summary>
        /// <param name="cluster">The cluster.</param>
        internal void StartForTimer(NeuronCluster cluster)
        {
            try
            {
                CurrentProcessor = this; // we need to indicate that this thread is run for this processor
                try
                {
#if DEBUG && !ANDROID // android doesn't like giving names to the treads.
                    if (string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
                    {
                        System.Threading.Thread.CurrentThread.Name = "Processor";

                            // this is for easy debugging, so we recognise the thread in VS.
                    }

#endif
                    Call(cluster);
                    if (CurrentProcessor == this)
                    {
                        // only continue if no blocked-solve was called, otherewise we need to get out fo this function withtout advancing the exec position.
                        SolveStack();
                    }
                }
                catch (ProcessorStopException)
                {
                    FinishSplitOnException();
                }
                catch (System.Exception e)
                {
                    // we need a catch all handler since this is the entry point of the thread.
                    LogService.Log.LogError("Processor.InternalSolve", string.Format("Internal error: {0}.", e));
                    FinishSplitOnException();
                }

                if (CurrentProcessor == this)
                {
                    // only continue if no blocked-solve was called, otherewise we need to get out fo this function withtout advancing the exec position.
                    OnFinished();
                }
            }
            finally
            {
                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    CurrentProcessor = null;
                }
            }
        }

        /// <summary>
        ///     Solves the stack from the specified neuron. Used for starting a subprocessor.
        /// </summary>
        /// <param name="from">From.</param>
        internal void SolveStackAfterSplit()
        {
#if DEBUG && !ANDROID // android doesn't like giving names to the treads.
            if (string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
            {
                System.Threading.Thread.CurrentThread.Name = "SubProc for: "
                                                             + (fNeuronToSolve != null
                                                                    ? fNeuronToSolve.ID.ToString()
                                                                    : "split in End-of-split-callback");

                    // this is for easy debugging, so we recognise the thread in VS.
            }

#endif
            var iHasNeuronToSolve = false;
            System.Diagnostics.Debug.Assert(CurrentProcessor == null);
            CurrentProcessor = this; // we need to indicate that this thread is run for this processor
            try
            {
                try
                {
                    RelockCallFrames();
                    iHasNeuronToSolve = NeuronToSolve != null && CurrentMeaning != null;

                        // curentMeaning must also be assigned, if it isn't,  we are handling an 'actions' list and we should do a normal stack solve.
                    if (iHasNeuronToSolve)
                    {
                        SolveNeuronAfterSplitOrBlockedSolve();
                    }
                    else
                    {
                        var iFrame = CallFrameStack.Peek();

                            // it could be that the split was started from the 'FinishSplit' callback. In that case, there is no neuron, but something on the stack to process, so check this first.
                        if (iFrame != null)
                        {
                            iFrame.NextExp++;

                                // this is not done when a split is performed because it is done after the process statement.
                            ProcessFrames();

                                // this will continue to process where the parent left off cause the split copied over the current statement position
                            if (NeuronStack.Count != 0 && fState != State.Exit)
                            {
                                // originally, there were no neurons on the stack after the split, so the split handler code added neurons, so make certain that the state is reset + global values are cleared if needed.
                                fState = State.Normal; // needs to be reset, just in case.
                            }

                            if (CurrentProcessor == this && Mem.VariableValues.Count > 0)
                            {
                                // we got here, but there is no more neuron to process, so remove any var stacks. can be 0, when the first frame on the stack owned the first variable list. (this is the case if there happened a split inside of a callback of another split). + if we did a blocked-solve, simply fall through, cause we aren't done yet.
                                PopVariableValues();
                            }
                        }
                        else
                        {
                            LogService.Log.LogWarning("Processor.SolveStackAfterSplit", "Nothing to execute.");
                        }
                    }

                    if (CurrentProcessor == this)
                    {
                        // only continue if no blocked-solve was called, otherewise we need to get out fo this function withtout advancing the exec position.
                        SolveStack();
                    }
                }
                catch (ProcessorStopException)
                {
                    FinishSplitOnException();
                }
                catch (System.Exception e)
                {
                    // if there is an exception, we still need to check and update the end of the split.
                    LogService.Log.LogError("Processor.SolveStackAfterSplit", string.Format("Internal error: {0}.", e));
                    FinishSplitOnException();
                }

                if (CurrentProcessor == this)
                {
                    // only continue if no blocked-solve was called, otherewise we need to get out fo this function withtout advancing the exec position.
                    OnFinished();
                }
            }
            finally
            {
                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    CurrentProcessor = null;
                }
            }
        }

        /// <summary>
        ///     Checks all the callframes to see if any has a lock, if so, this is requested again so that it is valid when freed.
        ///     This keeps all things deadlock save: sub processors, created inside a lockexpression need to wait until the root
        ///     processor is out of the lockExpression.
        /// </summary>
        protected void RelockCallFrames()
        {
            foreach (var i in Enumerable.Reverse(CallFrameStack))
            {
                // we need to reverse the order of hte stack: oldest locks need to be requested first.
                if (i.Locks != null)
                {
                    LockManager.Current.RequestLocks(i.Locks);
                }
            }
        }

        /// <summary>
        ///     Releases all the locks that are currently held on the stack (from LockExpresions). This
        ///     can be used by descendents to unlock these value temporarely, while the processor is paused
        ///     for instance, so that no ui would freeze. When this function is called, don't forget to lock
        ///     them again with <see cref="Processor.RelockCallFrames" />
        /// </summary>
        protected void UnlockCallFrames()
        {
            foreach (var i in Enumerable.Reverse(CallFrameStack))
            {
                // we need to reverse the order of hte stack: oldest locks need to be requested first.
                if (i.Locks != null)
                {
                    LockManager.Current.ReleaseLocks(i.Locks);
                }
            }
        }

        /// <summary>
        ///     Finishes the split when an exception occured.
        /// </summary>
        private void FinishSplitOnException()
        {
            try
            {
                var iLastOfSplit = true;
                try
                {
                    System.Diagnostics.Debug.WriteLine("FinishSplitOnException");
                    ClearFrames(); // can't forget to clear the frames.
                    if (SplitData != null)
                    {
                        // when we get here, let it know we are ready.
                        iLastOfSplit = ThreadManager.Default.FinishSplit(SplitData);
                    }
                }
                finally
                {
                    NeedsBlockHandleRelease = iLastOfSplit;
                    if (iLastOfSplit)
                    {
                        DeleteFrozenItems();

                            // any neurons that were set to be deleted, still need removing, to keep the system going as good as possible, even after an exception in one of the processors.
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Processor.FinishSplitOnException", string.Format("Internal error: {0}.", e));
            }
        }

        /// <summary>
        ///     Called when the processor is finished with solving the stack. Raises the <see cref="Processor.Finished" /> event.
        /// </summary>
        protected virtual void OnFinished()
        {
            System.Diagnostics.Debug.Assert(CallFrameStack.Count == 0);
            CurrentState = State.Terminated;
            if (Finished != null)
            {
                Finished(this, System.EventArgs.Empty);
            }

            if (TreeFinished != null && NeedsBlockHandleRelease)
            {
                // if NeedsBlockHandleRelease == true -> this is the last processor of a split, or there was no split (so, it's also the last one). When the processor is finished and there are no other processor in the split anymore, the entire tree has been processed, so we can call the 'TreeFinished' event.
                TreeFinished();
            }

            TreeFinished = null;

                // we always reset this callback when done, to make certain that there are no leaks. when clear is called, the callback should already have been called.
        }

        /// <summary>
        ///     Solves (or tries to solve) all the neurons on the stack.
        /// </summary>
        /// <remarks>
        ///     Actual solving is done by another method.  This one mostly handles state changes in the procesor like for exit
        ///     requests.
        ///     The state of the processor is checked before any code is called.
        /// </remarks>
        private void SolveStack()
        {
            Neuron iToSolve;
            var iLoop = true;
            while (iLoop)
            {
                if (NeuronStack.Count > 0 && fState != State.Exit)
                {
                    // we check the exit state before we call any code, cause this function can be called from 'SolveStackFrom', in which case we must be prepared for an exit state.
                    iToSolve = NeuronStack.Pop();
                    CurrentLink = 0; // solveNeuron doesn't do this cause it allows arbitrary start pos.
                    SolveNeuron(iToSolve);
                    if (fState == State.ExitNeuron)
                    {
                        // need to reset the state if an exit neuron was requested.
                        fState = State.Normal;
                    }

                    if (CurrentProcessor != this)
                    {
                        // if we started a blocked-solve, need to get out of this function untill back to root so we can start the new processor in the same thread as the caller.
                        iLoop = false;
                    }
                }
                else
                {
                    if (fState == State.Exit)
                    {
                        // if we requested an exit, but there are still items on the stack, we need to remove them, otherwise they could get processed again, when this is the last proc in a split.
                        NeuronStack.Clear();
                    }

                    var iLastOfSplits = false;

                        // when true, this thread was the last to resolve in a split set.  This is used to determin if we need to continue processing or not.
                    var iSplitData = SplitData;
                    if (iSplitData != null)
                    {
                        // when we get here, let it know the split is ready, so we can check if we need to continue after resolving it.
                        iLastOfSplits = ThreadManager.Default.FinishSplit(iSplitData);
                    }

                    if (CurrentProcessor == this)
                    {
                        // we could have requested a new blocked solve.
                        if (NeuronStack.Count != 0 && iLastOfSplits && fState != State.Exit)
                        {
                            // we we requested an exit, we always need to quit.
                            fState = State.Normal;

                                // always reset the state, cause the loop properly exited through the Exit statement.  After the split has been resolved, the stack is empty again, so if there is something on it after resolving the split, the callback put it on there, and we're back going again.
                        }
                        else
                        {
                            NeedsBlockHandleRelease = iLastOfSplits || iSplitData == null;
                            if (NeedsBlockHandleRelease)
                            {
                                // we only delete frozen items if we are the last proc, otherwise we store them so they can be copied over later on.  This allows us to use the auto update mechanisme where items are removed from the lists in the processors when they are no longer frozen.
                                DeleteFrozenItems();
                            }

                            iLoop = false; // we really want to quit.
                        }
                    }
                    else
                    {
                        iLoop = false;
                    }
                }
            }
        }

        /// <summary>Tries to solve the specified neuron.</summary>
        /// <remarks><para>Solving is done by walking down every 'To' link in the neuron and pushing this to reference on the stack after
        ///         which it calls
        ///         the <see cref="Link.Meaning"/>'s <see cref="Neuron.Actions"/> list.</para>
        /// <para>this is an internal function, cause the output instruction needs to be able to let a neuron be solved
        ///         sequential (<see cref="Processor.Solve"/> is async and solves the intire stack.</para>
        /// </remarks>
        /// <param name="toSolve"></param>
        protected internal virtual void SolveNeuron(Neuron toSolve)
        {
            Mem.VariableValues.Push();

                // a new function needs a new dictionary for the variables, so that variable values are local to the function.
            try
            {
                SolveNeuronWithoutVarChange(toSolve);
            }
            finally
            {
                if (CurrentProcessor == this)
                {
                    // could be taht a blocked-solve was started, in which case, we don't want to pop the variables just yet.
                    PopVariableValues(); // remove the dictionary cause the function is ready.
                }
            }
        }

        /// <summary>Solves the neuron without changing the variable dictionary stack and without initializing to the first
        ///     link so that execution can be picked up from the current position.</summary>
        /// <remarks>When solving, we need to check if the item needs to be sent to the output sin.  This is the
        ///     case if we are in output mode + the current neuron to solve has not outgoing links that it can solve itself
        /// <para>
        ///         also sets <see cref="Processor.NeuronToSolve"/></para>
        /// </remarks>
        /// <param name="toSolve">To solve.</param>
        private void SolveNeuronWithoutVarChange(Neuron toSolve)
        {
            NeuronToSolve = toSolve;

            // don't init current link cause this way we can start from an arbitrary position like with SolveNeuronFrom
            // also: can't put a 'using' for the accessor to the links round the whole loop, cause then we can't change the currenly changing neuron,
            // wich should be possible.
            CurrentLinks = BuildExecList(toSolve);
            SolveLinks();
            if (CurrentProcessor == this)
            {
                // only reset if there was no blocked solve.
                NeuronToSolve = null;

                    // reset as soon as possible so that the split callback doesn't get bothered by this value.
            }
        }

        /// <summary>
        ///     Solves all the links, stored in <see cref="Processor.CurrentLinks" />.
        ///     this is static so that we can swithc processor in the middle of a run (required to get a blockedSolve done on the
        ///     same thread).
        /// </summary>
        private static void SolveLinks()
        {
            var iActionsLink = -1;

                // keeps track+stores the expressions of the 'Actions' link for the neuron to solve.  This is handled seperatly.                                               
            var iPr = CurrentProcessor;
            while (iPr.CurrentLinkIsValid())
            {
                // using this type of loop allows the processor to easely perform code jumps.
                iPr.SetCurrentPos(iPr.CurrentLinks[iPr.CurrentLink]);
                if (iPr.CurrentMeaning != null && iPr.CurrentMeaning.ID != (ulong)PredefinedNeurons.Actions)
                {
                    iPr.ExecuteMeaning();
                    if (iPr != CurrentProcessor || iPr.CurrentState == State.Exit
                        || iPr.CurrentState == State.ExitNeuron || iPr.NeuronToSolve.ID == Neuron.EmptyId)
                    {
                        // if CurrentFrom got deleted, we can't process any further and need to stop.  This is a safety precaution.  Also, if we switched processor, we did a blockedSolve and so we need to get back to the root
                        break;
                    }
                }
                else if (iPr.CurrentMeaning == null)
                {
                    LogService.Log.LogError(
                        "Processor.SolveNeuron", 
                        "Internal error: link has an invalid meaning cause it can't be found in the brain.");
                }
                else if (iActionsLink == -1)
                {
                    // this is simply a quicker way to find the 'action's ref.  We have to walk through all the links anyway, so we will pass the 'actions' link
                    iActionsLink = iPr.CurrentLink;
                }
                else
                {
                    LogService.Log.LogWarning(
                        "Processor.SolveNeuronWithoutVarChange", 
                        "Found more than 1 link declared as the 'actions' cluster!");

                        // it's a double 'actions' list, which should not be declared, not illegal, but lets get the user informed.
                }

                iPr.CurrentLink++; // if we don't do this, we never get out of the loop.
            }

            if (iPr == CurrentProcessor)
            {
                // when starting a blocked-solve, don't do the end of links yet.
                iPr.CurrentMeaning = null;
                iPr.CurrentLink = iActionsLink;
                if (iActionsLink > -1 && iPr.CurrentState < State.Exit)
                {
                    // we don't want call the 'Action's list when an exit or an exitNeuron was requested.
                    iPr.OnNeuronProcessed(iPr.NeuronToSolve, iPr.CurrentLinks[iActionsLink].To as NeuronCluster);
                }
                else
                {
                    iPr.OnNeuronProcessed(iPr.NeuronToSolve, null);
                }
            }
        }

        /// <summary>Checks if the current link is valid and tries to solve it if it isn't.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CurrentLinkIsValid()
        {
            LinkContent iContent;
            if (CurrentLink < CurrentLinks.Count)
            {
                iContent = CurrentLinks[CurrentLink];
            }
            else
            {
                return false;
            }

            while (iContent.To.ID == Neuron.EmptyId || iContent.Meaning.ID == Neuron.EmptyId)
            {
                // if, between creation of the link-content list and execution at this stage, the meaning or to neuron have been deleted, don't execute the link, but go to the next one.
                CurrentLink++;
                if (CurrentLink < CurrentLinks.Count)
                {
                    iContent = CurrentLinks[CurrentLink];
                }
                else
                {
                    break;
                }
            }

            return CurrentLink < CurrentLinks.Count;
        }

        /// <summary>The build exec list.</summary>
        /// <param name="toSolve">The to solve.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<LinkContent> BuildExecList(Neuron toSolve)
        {
            var iRes = new System.Collections.Generic.List<LinkContent>();
            var iToProcess = Factories.Default.LinkLists.GetBuffer();
            try
            {
                if (toSolve.LinksOutIdentifier != null)
                {
                    using (var iList = toSolve.LinksOut)
                        iToProcess.AddRange(iList);

                            // make a local copy so we don't get a possible deadlock situation while trying to retrieve the meaning.
                }

                var iMemFac = Factories.Default;
                foreach (var i in iToProcess)
                {
                    var iNew = new LinkContent();
                    iNew.Meaning = i.Meaning;
                    iNew.To = i.To;
                    if (i.InfoIdentifier != null)
                    {
                        var iTemp = i.Info.ConvertTo<Neuron>();
                        iNew.Info = iTemp;
                    }

                    iRes.Add(iNew);
                }

                return iRes;
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iToProcess);
            }
        }

        /// <summary>Sets the current pos (meaning, info, to</summary>
        /// <param name="link">The link.</param>
        private void SetCurrentPos(LinkContent link)
        {
            CurrentMeaning = link.Meaning;

                // Need to store the meaning and To seperatly, in case the link gets deleted, need the link for the Info
            CurrentInfo = link.Info;
            CurrentTo = link.To;
        }

        /// <summary>
        ///     Executes the rules of the neuron on the meaning of the current link.
        /// </summary>
        protected virtual void ExecuteMeaning()
        {
            CodeCluster = CurrentMeaning.RulesCluster;
            if (CodeCluster != null)
            {
                var iItems = CodeCluster.GetBufferedChildren<Expression>();
                try
                {
                    Process(CurrentMeaning, iItems, ExecListType.Rules);

                        // this will continue to process where the parent left off cause the split copied over the current statement position
                }
                finally
                {
                    CodeCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }
        }

        /// <summary>Called when all normall links have been processed except the actions (if any), which are executed here.</summary>
        /// <remarks>This can be used to remove the neuron from the stack, but also to indicate that the 'actions' link is being
        ///     executed.</remarks>
        /// <param name="toSolve">neuron that was solved.</param>
        /// <param name="cluster">The cluster taht contains the code.</param>
        protected virtual void OnNeuronProcessed(Neuron toSolve, NeuronCluster cluster)
        {
            if (cluster != null)
            {
                var iActions = cluster.GetBufferedChildren<Expression>();
                try
                {
                    if (iActions != null)
                    {
                        // quick check, if there are no items at this point, we don't try to execute.  This is not thread save, can't block cause ConvertTo does the actual blocking.
                        CodeCluster = cluster;
                        Process(toSolve, iActions, ExecListType.Actions);
                    }
                }
                finally
                {
                    cluster.ReleaseBufferedChildren((System.Collections.IList)iActions);
                }
            }
        }

        /// <summary>Tries to solve the specified neuron starting from the specified link and statement.</summary>
        /// <remarks>Note: <see cref="Processor.NeuronToSolve"/> must be assigned.</remarks>
        /// <param name="fromBlocked">The from Blocked.</param>
        protected void SolveNeuronAfterSplitOrBlockedSolve(bool fromBlocked = false)
        {
            try
            {
                if (CurrentMeaning != null)
                {
                    // Currentmeaning is set during the
                    var iFrame = CallFrameStack.Peek();
                    if (iFrame != null && iFrame.Code != null)
                    {
                        if (fromBlocked == false)
                        {
                            // when we are restarting a previously blocked processor, it already it at the correct location.
                            iFrame.NextExp++;

                                // this is not done when a split is performed because it is done after the process statement.
                        }

                        ProcessFrames();

                            // this will continue to process where the parent left off cause the split copied over the current statement position
                    }

                    if (CurrentState == State.Exit)
                    {
                        return;
                    }

                    if (CurrentState == State.ExitNeuron)
                    {
                        // with an exit neuron, still need to execute the actions that were attached to it.
                        OnNeuronProcessed(NeuronToSolve, null);

                            // when we ask for an exitNeuron, we can't call the action code anymore.
                        return;
                    }

                    if (CurrentProcessor == this)
                    {
                        // if we did a blocked-solve, simply fall through, cause we aren't done yet.
                        CurrentLink++; // we need to advance caus current link has been processed, need to do next.
                        SolveLinks();

                            // we hook back into the regular procedure, don't need var dict handling, this is done here cause a split makes a duplicate of the original's dict.
                        NeuronToSolve = null;

                            // reset as soon as possible so that the split callback doesn't get bothered by this value.
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "Processor.SolveNeuronFrom", 
                        "Internal error: Can't find currently executing meaning.");
                }

                // if (NeuronToSolve.ID != Neuron.EmptyId)                                                         //we have already checked if NeuronToSolve is set, in the caller.
                // {

                // }
                // else
                // Log.LogWarning("Processor.SolveNeuronFrom", "Neuron to solve has been deleted, moving on to next neuron.");
            }
            finally
            {
                if (CurrentProcessor == this && Mem.VariableValues.Count > 0)
                {
                    // can be 0, when the first frame on the stack owned the first variable list. (this is the case if there happened a split inside of a callback of another split). + if we did a blocked-solve, simply fall through, cause we aren't done yet.
                    PopVariableValues();

                        // remove the dictionary cause the function is ready and it was put on there due to the split.
                }
            }
        }

        /// <summary>Tries to process the list of neurons.</summary>
        /// <remarks>Don't init the pos, this way it can be used to start from an arbitrary pos like with SolveNeuronFrom.</remarks>
        /// <param name="toExec">The neuron who's code needs executing.  This is not used in this class but is provided
        ///     for inheritters, so they can do something with it.</param>
        /// <param name="toProcess">The to Process.</param>
        /// <param name="listType">The list Type.</param>
        protected void Process(
            Neuron toExec, System.Collections.Generic.IList<Expression> toProcess, 
            ExecListType listType)
        {
            if (toExec != null)
            {
                // && toExec.ID != Neuron.EmptyId
                if (toProcess != null)
                {
                    try
                    {
                        var iFrame = Factories.Default.CallFrames.GetCallFrame();
                        iFrame.ExecSource = toExec;
                        iFrame.CodeListType = listType;
                        iFrame.Code = toProcess;
                        PushFrame(iFrame);
                        ProcessFrames();
                    }
                    catch (ProcessorStopException)
                    {
                        // when a stop is requested, don't log it, let it dripple up, so the proceessor actualy stops completely.
                        throw;
                    }
                    catch (System.Exception e)
                    {
                        // very important to have a catch here, otherwise we can't log the error correctly (don't know anymore who raised the error).
                        LogService.Log.LogError("Processor.Process", e.ToString());
                    }
                }
                else
                {
                    LogService.Log.LogWarning("Processor.Process", "Nothing to process.");
                }
            }
            else
            {
                LogService.Log.LogWarning(
                    "Processor.Process", 
                    "Neuron to process has been deleted, moving on to next neuron.");
            }
        }

        /// <summary>
        ///     processes all the frames for the current
        /// </summary>
        private static void ProcessFrames()
        {
            if (CurrentProcessor.CallFrameStack.Count > 0)
            {
                var iFrame = CurrentProcessor.CallFrameStack.Peek();
                while (iFrame != null)
                {
                    // using this type of loop allows the processor to easely perform code jumps.
                    while (iFrame.NextExp >= 0 && iFrame.Code != null && CurrentProcessor.NextExp < iFrame.Code.Count)
                    {
                        var iExp = iFrame.Code[iFrame.NextExp];
                        CurrentProcessor.Process(iExp);

                            // we don't call iExp.Execute directly, this way, a debugger or something else can do something just before and/or after a statement is performed.
                        iFrame.NextExp++;

                            // important to do this after process, otherwise the debugger gets screwed.  We can do this, even tough 'process' can change the stack because of the local ref. it does give a problem for the split though, which needs to increment it's last value
                        CallFrame iNew;
                        if (CurrentProcessor.CallFrameStack.Count > 0)
                        {
                            iNew = CurrentProcessor.CallFrameStack.Peek();

                                // we always retrieve the frame after an expression, this way, we can move around in the call frame stack.
                        }
                        else
                        {
                            iNew = null;
                        }

                        if (iNew != iFrame)
                        {
                            // we only try to update the CallFrameStack, if there is a new frame or we are at the end of the current one.
                            iFrame = iNew;
                            break;
                        }
                    }

                    if (iFrame != null)
                    {
                        iFrame.UpdateCallFrameStack(CurrentProcessor);
                        if (CurrentProcessor.CallFrameStack.Count > 0)
                        {
                            // after
                            iFrame = CurrentProcessor.CallFrameStack.Peek();
                        }
                        else
                        {
                            iFrame = null;
                        }
                    }
                }
            }
        }

        /// <summary>Tries to process a single expression.</summary>
        /// <param name="toProcess"></param>
        protected internal virtual void Process(Expression toProcess)
        {
            try
            {
                toProcess.Execute(this);
            }
            catch (System.Exception e)
            {
                // very important to have a catch here, otherwise we can't log the error correctly (don't know anymore who raised the error).
                LogService.Log.LogError("Processor.Process", e.ToString());
            }
        }

        /// <summary>Converts the neuron cluster's children to expressions and tries to execute them.
        ///     Note: when not called within the context of an already running processor, use<see cref="Processor.CallSingle"/> instead, which provides proper processor support.</summary>
        /// <param name="cluster">The cluster.</param>
        public virtual void Call(NeuronCluster cluster)
        {
            if (cluster != null)
            {
                Mem.VariableValues.Push();

                    // a new function needs a new dictionary for the variables, so that variable values are local to the function.
                var iFrame = CallFrame.Create(cluster);
                iFrame.CausedNewVarDict = true;
                PushFrame(iFrame);
                ProcessFrames();
            }
            else
            {
                LogService.Log.LogError("Processor.Call", "Failed to perform call: NeuronCluster ref is null.");
            }
        }

        /// <summary>Executes the code that is found in the cluster. This is done async</summary>
        /// <param name="cluster"></param>
        public void CallSingle(NeuronCluster cluster)
        {
            ThreadManager.Default.ReserveThread(this, ThreadRequestType.SingleCall, cluster);
        }

        /// <summary>Executes the code that is found in the cluster. This is done in the same thread
        ///     as the calling thread. During the call, the current thread is converted into a new
        ///     processor, so don't call from within the context of a processor, use <see cref="Processor.Call"/> instead
        ///     in those situations. This function provides proper processor cleanup.</summary>
        /// <param name="cluster">The cluster.</param>
        internal void CallSingleSync(NeuronCluster cluster)
        {
            try
            {
                CurrentProcessor = this; // we need to indicate that this thread is run for this processor
                try
                {
#if DEBUG && !ANDROID // android doesn't like giving names to the treads.
                    if (string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
                    {
                        System.Threading.Thread.CurrentThread.Name = "Processor";

                            // this is for easy debugging, so we recognise the thread in VS.
                    }

#endif
                    Call(cluster);
                    if (CurrentProcessor == this)
                    {
                        // only continue if no blocked-solve was called, otherewise we need to get out fo this function withtout advancing the exec position.
                        if (NeuronStack.Count > 0 && fState != State.Exit)
                        {
                            SolveStack();
                        }
                        else
                        {
                            var iLastOfSplits = false;

                                // when true, this thread was the last to resolve in a split set.  This is used to determin if we need to continue processing or not. True by default, in case there was no split
                            var iSplitData = SplitData;
                            if (iSplitData != null)
                            {
                                // when we get here, let it know the split is ready, so we can check if we need to continue after resolving it.
                                iLastOfSplits = ThreadManager.Default.FinishSplit(iSplitData);
                            }

                            if (CurrentProcessor == this)
                            {
                                // we could have requested a new blocked solve.
                                if (NeuronStack.Count != 0 && iLastOfSplits && fState != State.Exit)
                                {
                                    // we we requested an exit, we always need to quit.
                                    fState = State.Normal;

                                        // always reset the state, cause the loop properly exited through the Exit statement.  After the split has been resolved, the stack is empty again, so if there is something on it after resolving the split, the callback put it on there, and we're back going again.
                                    SolveStack();
                                }
                                else
                                {
                                    NeedsBlockHandleRelease = iLastOfSplits || iSplitData == null;
                                    if (NeedsBlockHandleRelease)
                                    {
                                        // we only delete frozen items if we are the last proc, otherwise we store them so they can be copied over later on.  This allows us to use the auto update mechanisme where items are removed from the lists in the processors when they are no longer frozen.
                                        DeleteFrozenItems();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ProcessorStopException)
                {
                    FinishSplitOnException();
                }
                catch (System.Exception e)
                {
                    // we need a catch all handler since this is the entry point of the thread.
                    LogService.Log.LogError("Processor.InternalSolve", string.Format("Internal error: {0}.", e));
                    FinishSplitOnException();
                }

                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    OnFinished();
                }
            }
            finally
            {
                if (CurrentProcessor == this)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    CurrentProcessor = null;
                }
            }
        }

        /// <summary>resets the blocked processor and continues a single frame.</summary>
        /// <param name="proc">The proc.</param>
        internal static void ContinueBlocedProcessor(Processor proc)
        {
            CurrentProcessor = proc;
#if DEBUG && !ANDROID // android doesn't like giving names to the treads.
            if (string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
            {
                System.Threading.Thread.CurrentThread.Name = "BlockedSolve return";
            }

#endif
            var iHasNeuronToSolve = false;
            try
            {
                try
                {
                    iHasNeuronToSolve = proc.NeuronToSolve != null && proc.CurrentMeaning != null;

                        // curentMeaning must also be assigned, if it isn't,  we are handling an 'actions' list and we should do a normal stack solve.
                    if (iHasNeuronToSolve)
                    {
                        proc.SolveNeuronAfterSplitOrBlockedSolve(true);
                    }
                    else
                    {
                        var iFrame = proc.CallFrameStack.Peek();
                        if (iFrame != null)
                        {
                            // iFrame.NextExp++;                                                                      //don't think this is required, at the start of the blocked-solve, we already advanced?
                            ProcessFrames();

                                // this will continue to process where the parent left off cause the split copied over the current statement position
                            if (proc.NeuronStack.Count != 0 && proc.fState != State.Exit)
                            {
                                // originally, there were no neurons on the stack after the split, so the split handler code added neurons, so make certain that the state is reset + global values are cleared if needed.
                                proc.fState = State.Normal; // needs to be reset, just in case.
                            }
                        }
                        else
                        {
                            LogService.Log.LogWarning("Processor.SolveStackAfterSplit", "Nothing to execute.");
                        }
                    }

                    if (CurrentProcessor == proc)
                    {
                        // if we requested a new blocked-solve, don't call finish just yet cause it's not done yet.
                        proc.SolveStack();
                    }
                }
                catch (ProcessorStopException)
                {
                    proc.FinishSplitOnException();
                }
                catch (System.Exception e)
                {
                    // if there is an exception, we still need to check and update the end of the split.
                    LogService.Log.LogError("Processor.SolveStackAfterSplit", string.Format("Internal error: {0}.", e));
                    proc.FinishSplitOnException();
                }

                if (CurrentProcessor == proc)
                {
                    // if we requested a new blocked-solve, don't call finish just yet cause it's not done yet.
                    proc.OnFinished();
                }
            }
            finally
            {
                if (CurrentProcessor == proc)
                {
                    // need to check if there was a blockedSolve, in which case we need to continue. 
                    CurrentProcessor = null;
                }
            }
        }

        #endregion

        #region Stack Operations

        /// <summary>Pushes the frame on the call-frame stack.</summary>
        /// <param name="frame">The frame.</param>
        public virtual void PushFrame(CallFrame frame)
        {
            CallFrameStack.Push(frame);
        }

        /// <summary>Pops the last frame on the call-frame stack. Makes certain that the pop will also remove
        ///     any variables that were registered because of the frame.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public virtual CallFrame PopFrame()
        {
            if (CallFrameStack.Count > 0)
            {
                var iRes = CallFrameStack.Pop();
                CleanFrame(iRes);
                return iRes;
            }

            return null;
        }

        /// <summary>
        ///     exits all the frames until the first callframe is found. The
        /// </summary>
        /// <param name="args"></param>
        internal void ExitFramesUntillReturn()
        {
            var i = PopFrame();
            while (i != null)
            {
                if (i is CallInstCallFrame || i is ExpressionBlockFrame)
                {
                    break;
                }

                i = PopFrame();
            }

            if (CallFrameStack.Count > 0)
            {
                i = CallFrameStack.Peek();
                if (i is ForEachCallFrame)
                {
                    // the ForEach calframe behaves a bit differently. When we do an break, the new frame on the top gets to update itself, the foreach does this by going to the next item in the list. We don't want this, we want to go to the next code item, so we let the calframe know it needs to do something differently.
                    ((ForEachCallFrame)i).DidBreak = true;
                }
            }
        }

        /// <summary>The clean frame.</summary>
        /// <param name="frame">The frame.</param>
        private void CleanFrame(CallFrame frame)
        {
            if (frame.HasLocals)
            {
                if (Mem.VariableValues.Count > 0)
                {
                    // can be empty whe we did a stop-processor.
                    var iDict = Mem.VariableValues.Peek();
                    foreach (var i in frame.LocalsBuffer)
                    {
                        if (i.Value != null)
                        {
                            StoreVariableValue(i.Key, i.Value);
                        }
                        else
                        {
                            iDict.Remove(i.Key);

                                // if the list was null, there was no previous value stored in the dict, so remove the key.
                        }
                    }
                }

                frame.LocalsBuffer.Clear();
            }

            if (frame.CausedNewVarDict)
            {
                PopVariableValues();
            }

            if (frame.Locks != null)
            {
                LockManager.Current.ReleaseLocks(frame.Locks);
            }

            frame.Release();
        }

        /// <summary>
        ///     Clears all the frames from the call-frame stack.
        /// </summary>
        public virtual void ClearFrames()
        {
            foreach (var i in CallFrameStack)
            {
                CleanFrame(i);
            }

            CallFrameStack.Clear();
        }

        /// <summary>
        ///     Pops the last dict of variable values from the stack.
        /// </summary>
        public virtual void PopVariableValues()
        {
            Mem.VariableValues.Pop();
        }

        /// <summary>Pushes the specified neuron on it's internal stack.</summary>
        /// <param name="toPush">The neuron to add.</param>
        public virtual void Push(Neuron toPush)
        {
            fNeuronStack.Push(toPush);
        }

        /// <summary>Removes the last node from the stack.</summary>
        /// <remarks>You can't do this directly on a stack list cause other types of processors might want
        ///     to do some extra stuf, like a warn a debugger.</remarks>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public virtual Neuron Pop()
        {
            if (fNeuronStack.Count > 0)
            {
                return fNeuronStack.Pop();
            }

            return null;
        }

        /// <summary>
        ///     Returns the neuron currently at the top of the Neuron execution stack (neurons that still need to be solved).
        /// </summary>
        /// <returns>The current top of the stack or <see cref="PredefinedNeurons.Empty" /> if there is nothing on the stack.</returns>
        public virtual Neuron Peek()
        {
            if (fNeuronStack.Count > 0)
            {
                return fNeuronStack.Peek();
            }

            return Brain.Current[(ulong)PredefinedNeurons.Empty];
        }

        #endregion

        #region Conditional flow

        /// <summary>Returns the evalulation of the condition.</summary>
        /// <param name="compareTo">The posssible list of neurons that must be found through the condition.</param>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="index">The index of the expression in the condition that is being evaluated.  This is provided for the
        ///     debugger.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <remarks>If there is a <b>compareTo</b> value specified, a case statement is presumed, and all the
        ///     neurons in the compareTo must also be in Condition (after solving a possible expression) and no more.
        ///     If compareTo is null, a boolean expression is presumed and it's result is returned or an error logged
        ///     and false returned.
        ///     When the condition is empty, true is always presumed.</remarks>
        protected internal virtual bool EvaluateCondition(System.Collections.Generic.IEnumerable<Neuron> compareTo, 
            ConditionalExpression expression, 
            int index)
        {
            var iCondNeuron = expression.ExpData.Condition;
            if (compareTo == null)
            {
                IGetBool iGetBool = iCondNeuron;
                if (iGetBool != null && iGetBool.CanGetBool())
                {
                    return iGetBool.GetBool(this);
                }

                var iCond = iCondNeuron as ResultExpression;
                if (iCond != null)
                {
                    var iRes = Mem.ArgumentStack.Push();
                    try
                    {
                        iCond.GetValue(this);
                        foreach (var i in iRes)
                        {
                            if (i != null && i.ID == (ulong)PredefinedNeurons.False)
                            {
                                return false;
                            }
                        }
                    }
                    finally
                    {
                        Mem.ArgumentStack.Pop();
                    }
                }
                else if (iCondNeuron != null)
                {
                    LogService.Log.LogWarning(
                        "Condional part.Evaluate", 
                        "Only cased condionals allow for regular neurons, otherwise, a result expression (like a bool expression) is expected");
                    return false;
                }

                return true;
            }

            System.Collections.Generic.IEnumerable<Neuron> iCond = Neuron.SolveResultExpNoStackChange(iCondNeuron, this);
            if (iCond != null)
            {
                var iRes = Enumerable.SequenceEqual(compareTo, iCond);
                Mem.ArgumentStack.Pop();
                return iRes;
            }

            return true;

                // it's the empty value, no condition to compare to (which is the last one), and indicates the 'else', so always evaluate to true.
        }

        #endregion

        #region Control flow

        /// <summary>
        ///     Kills the processor if it is running.
        /// </summary>
        public virtual void Kill()
        {
            KillRequested = true;
        }

        /// <summary>
        ///     Stops the entire <see cref="Processor.Solve" /> process so that a result node can be returned.
        /// </summary>
        public virtual void Exit()
        {
            ClearFrames();
            CurrentState = State.Exit;
        }

        /// <summary>
        ///     Exits the current call frame (the cotinue statement).
        /// </summary>
        /// <remarks>
        ///     Is done by removing all the frames untill we find the first conditonal frame.
        /// </remarks>
        internal void ContinueConditional()
        {
            if (CallFrameStack.Count > 1)
            {
                PopFrame();
                var iFrame = CallFrameStack.Peek();
                while (
                    !(iFrame is ForEachCallFrame || iFrame is UntilCallFrame || iFrame is LoopedCallFrame
                      || iFrame is ForQueryCallFrame))
                {
                    if (CallFrameStack.Count > 1)
                    {
                        // we must still be able to do a peek: the conditional must remain on the stack.
                        PopFrame();
                        iFrame = CallFrameStack.Peek();
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Break statement used outside of a conditional.");
                    }
                }
            }
            else
            {
                throw new System.InvalidOperationException("No frames on call stack.");
            }
        }

        /// <summary>
        ///     Exits the current conditional (the break statement).
        /// </summary>
        /// <remarks>
        ///     This is done by
        /// </remarks>
        internal void ExitConditional()
        {
            if (CallFrameStack.Count > 0)
            {
                var iLast = PopFrame(); // a break statement should be used in one of the conditional parts.
                while (
                    !(iLast is ForEachCallFrame || iLast is UntilCallFrame || iLast is LoopedCallFrame
                      || iLast is ForQueryCallFrame))
                {
                    if (CallFrameStack.Count > 0)
                    {
                        iLast = PopFrame();
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Break statement used outside of a conditional.");
                    }
                }

                if (iLast is ForQueryCallFrame)
                {
                    // the last loop was a query, so make certain it gets closed.
                    ((ForQueryCallFrame)iLast).GotoEnd();
                }

                iLast = CallFrameStack.Peek();
                if (iLast is ForEachCallFrame)
                {
                    // the ForEach calframe behaves a bit differently. When we do an break, the new frame on the top gets to update itself, the foreach does this by going to the next item in the list. We don't want this, we want to go to the next code item, so we let the calframe know it needs to do something differently.
                    ((ForEachCallFrame)iLast).DidBreak = true;
                }
            }
            else
            {
                throw new System.InvalidOperationException("Break statement used outside of a conditional.");
            }
        }

        /// <summary>
        ///     Stops solving the current neuron.
        /// </summary>
        internal void ExitNeuron()
        {
            ClearFrames(); // clearing the entire stack will stop executing the link.
            CurrentState = State.ExitNeuron; // this stops the entire neuron.
        }

        #endregion

        #region Sub processor

        /// <summary>Returns an array of the specified size, filled with processors.</summary>
        /// <param name="count">The nr of processors to create.</param>
        /// <returns>An array of newly created processors (not initialized).</returns>
        /// <remarks>Descendents should reimplement this so they can give another type (for debugging).</remarks>
        public virtual Processor[] CreateProcessors(int count)
        {
            var iRes = new Processor[count];
            for (var i = 0; i < count; i++)
            {
                iRes[i] = ProcessorFactory.GetSubProcessor();
            }

            return iRes;
        }

        /// <summary>Clones this processor to the specified processors, thereby letting the subprocessors know they are clones.</summary>
        /// <param name="toClone">The processors that need to become a clone of this.</param>
        /// <param name="isAccum">if set to <c>true</c> the split is an accumulated split (so the weights fo the splitresults will
        ///     be added, sub processors dont' inherit result of root).</param>
        /// <returns>An object that did the cloning, which should be recycled by the caller</returns>
        protected internal virtual ProcessorSplitter CloneProcessors(Processor[] toClone, bool isAccum = false)
        {
            var iSplitter = new ProcessorSplitter();
            iSplitter.SourceVariableValues = new System.Collections.Generic.List<VarDict>(Mem.VariableValues);

                // need to make a copy of the stack so we preserve the order of the stack (all 'copy' functions of the stack change the order).
            iSplitter.Split(this, toClone);
            for (var i = 0; i < toClone.Length; i++)
            {
                var iProc = toClone[i];
                iProc.SplitData = new SplitData();

                    // we need to create the split data in order to indicate it is a clone.
                iProc.SplitData.Processor = iProc;
                iProc.CloneCurrentPos(i, iSplitter);
                iProc.CloneCallStack(i, iSplitter);
                iProc.CloneNeuronStack(i, iSplitter.TargetStacks);
                iProc.CloneGlobals(i, iSplitter);
                iProc.UpdateVariablesAfterSplit(i, iSplitter);

                    // must be done after all clones have been made, otherwise, variable contents can't be updated correctly.
                if (isAccum == false)
                {
                    // accummulated split doesn't copy the results of the root proc.
                    iProc.SplitValues.InitFrom(SplitValues);
                }
                else
                {
                    iProc.SplitData.IsAccum = true;
                }

                iProc.BlockedProcessor = BlockedProcessor;
                iProc.TreeFinished = TreeFinished;

                    // if the root proc had a callback, make certain we pass it along, so that the eventual end result can raise the event.
                iProc.CloneReturnVaules(Mem.LastReturnValues);
            }

            return iSplitter;
        }

        /// <summary>copies the lists of return values from the specified source to us.</summary>
        /// <param name="source"></param>
        private void CloneReturnVaules(System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>> source)
        {
            var iMemFac = Factories.Default;
            foreach (var i in Enumerable.Reverse(source))
            {
                // add in reverse order so that the return statements are in the same order as the original, cause stack returns in reverse order normally
                var iNew = iMemFac.NLists.GetBuffer();

                    // make a copy of the list.  use the factory of the currently active proc, not the one we are copying to, cause the first can have empty lists, the second doesn't
                iNew.AddRange(i);
                Mem.LastReturnValues.Push(iNew);
            }
        }

        /// <summary>The clone globals.</summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        private void CloneGlobals(int index, ProcessorSplitter data)
        {
            var iMemFac = Factories.Default;
            if (data.TargetCopiedGlobals != null)
            {
                foreach (var i in data.TargetCopiedGlobals)
                {
                    var iNew = iMemFac.NLists.GetBuffer();

                        // each proc needs a unique list, otherwise we can't recycle. use the factory of the currently active proc, not the one we are copying to, cause the first can have empty lists, the second doesn't
                    iNew.AddRange(i.Value);
                    StoreGlobalValue(i.Key, iNew);
                }
            }

            if (data.TargetClonedGlobals != null)
            {
                foreach (var i in data.TargetClonedGlobals)
                {
                    var iVal = iMemFac.NLists.GetBuffer();

                        // use the factory of the currently active proc, not the one we are copying to, cause the first can have empty lists, the second doesn't
                    foreach (var u in i.Value)
                    {
                        iVal.Add(u[index]);
                        if (u[index].ID != Neuron.TempId)
                        {
                            u[index].SetIsFrozen(true, this);

                                // all cloned vars need to be frozen except when they are still temps: in this case they already are frozen.
                        }
                    }

                    StoreGlobalValue(i.Key, iVal);
                }
            }

            if (data.TargetSharedGlobals != null)
            {
                foreach (var i in data.TargetSharedGlobals)
                {
                    StoreGlobalValue(i.Key, i.Value);
                }
            }
        }

        /// <summary>for cloning multiple neurons at the same time.</summary>
        /// <param name="i">The i.</param>
        /// <param name="data">The data.</param>
        private void CloneCurrentPos(int i, ProcessorSplitter data)
        {
            NeuronToSolve = data.TargetNeuronToSolve[i];
            if (NeuronToSolve != null)
            {
                NeuronToSolve.SetIsFrozen(true, this);
            }

            CurrentTo = data.Source.CurrentTo;
            if (data.Source.CurrentInfo != null)
            {
                CurrentInfo = Enumerable.ToList(data.Source.CurrentInfo);

                    // the info is also the same, but we must copy the list, otherwise we might loose the contents.
            }

            CurrentLink = data.Source.CurrentLink;
            CurrentSin = data.Source.CurrentSin;
            CurrentMeaning = data.Source.CurrentMeaning;
            CurrentLinks = data.Source.CurrentLinks;
        }

        /// <summary>Updates the contents of all the variables after a split so that they point to the cloned
        ///     versions.</summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        private void UpdateVariablesAfterSplit(int index, ProcessorSplitter data)
        {
            var iMemFac = Factories.Default;
            for (var counter = 0; counter < data.SourceVariableValues.Count; counter++)
            {
                var iNew = Mem.VariableValues.Push();

                System.Collections.Generic.List<Neuron> iVal = null;
                foreach (var i in data.SourceVariableValues[counter])
                {
                    // we need to make certain that the content of the variables is updated so that they contain the clones of the stack content.
                    switch (i.Key.SplitReactionID)
                    {
                        case 0:

                            // no split reaction defined, so make certain that the var contains a processor local value.
                            iVal = iMemFac.NLists.GetBuffer();
                            if (iVal.Capacity < i.Value.Data.Count)
                            {
                                iVal.Capacity = i.Value.Data.Count;
                            }

                            foreach (var iNeuron in i.Value.Data)
                            {
                                if (iNeuron.IsDeleted == false)
                                {
                                    System.Collections.Generic.List<Neuron> iFound;
                                    if (data.Clones.TryGetValue(iNeuron, out iFound))
                                    {
                                        iVal.Add(iFound[index]);
                                    }
                                    else
                                    {
                                        iVal.Add(iNeuron);
                                    }
                                }
                            }

                            break;
                        case (ulong)PredefinedNeurons.shared: // simply copy the values over.
                            iVal = i.Value.Data;
                            break;
                        case (ulong)PredefinedNeurons.Copy: // simply copy the values over.
                            iVal = iMemFac.NLists.GetBuffer();
                            foreach (var iNeuron in i.Value.Data)
                            {
                                if (iNeuron.IsDeleted == false)
                                {
                                    iVal.Add(iNeuron);
                                }
                            }

                            break;
                        case (ulong)PredefinedNeurons.Duplicate:
                            iVal = iMemFac.NLists.GetBuffer();
                            foreach (var iNeuron in i.Value.Data)
                            {
                                if (iNeuron.IsDeleted == false)
                                {
                                    System.Collections.Generic.List<Neuron> iFound;
                                    if (data.Clones.TryGetValue(iNeuron, out iFound))
                                    {
                                        iVal.Add(iFound[index]);
                                    }
                                    else
                                    {
                                        LogService.Log.LogError(
                                            "processor", 
                                            "internal error: can't found split value for variable.");
                                    }
                                }
                            }

                            break;
                    }

                    // possible speed optimisation: make a difference between debug-run and normal run?
                    StoreVariableValue(i.Key, iVal);

                        // we store the value this way, so that any descendent gets warned of the add.
                }
            }
        }

        /// <summary>The clone neuron stack.</summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        private void CloneNeuronStack(
            int index, System.Collections.Generic.List<System.Collections.Generic.List<Neuron>> data)
        {
            foreach (System.Collections.Generic.IList<Neuron> iList in data)
            {
                var iClone = iList[index];
                iClone.SetIsFrozen(true, this);
                Push(iClone);
            }
        }

        /// <summary>Clones the call stack.</summary>
        /// <param name="index">The index.</param>
        /// <param name="splitter">The splitter, provides extra data for the split process</param>
        private void CloneCallStack(int index, ProcessorSplitter splitter)
        {
            var iPrev = CurrentProcessor;
            CurrentProcessor = this;

                // during the duplication, the callframes try to use the proc's buffers to generate new objects. But the new processor (this one), isn't active yet, so if we don't temporarily switch this, the original proc's buffers will get exhausted because of all the splits it performs. But if we temporarily switch this, every processor provides for it's own callframes.
            try
            {
                foreach (var i in splitter.Frames)
                {
                    var iNew = i.Duplicate(index, splitter);
                    PushFrame(iNew);
                }
            }
            finally
            {
                CurrentProcessor = iPrev;
            }
        }

        /// <summary>Copies the content of the SplitValues to the specified list. Warning: list should already be locked for
        ///     writing, this function doesn't do this.</summary>
        /// <remarks>Called when a subprocessor is ready and it's result must be copied over.</remarks>
        /// <param name="list">The neuron list.</param>
        internal void CopyResultsTo(IDListAccessor list)
        {
            var iRes = SplitValues.GetNeurons(); // get a local copy, this is thread safe.
            list.AddRange(iRes);
        }

        /// <summary>Copies the content of this processor to the specified processor. This is thread safe.</summary>
        /// <param name="to">Processor to copy these split values to.</param>
        internal void CopyResultsTo(SplitData to)
        {
            var iProc = to.Processor;
            SplitValues.CopyTo(iProc.SplitValues, to.IsAccum);
            var iPrev = CurrentProcessor;
            CurrentProcessor = null;

                // we temporarely set the proc to null so that the lock-factory gets disabled. We do this cause there can be lots of frozen items, which generates a lot of lock items, which are only recycled for 1 processor, so we get an inbalance in the memory buffers, which only causes the mem usage to grow, so don't use it for this section.
            var iReq = BuildFrozenLock(true);
            LockManager.Current.RequestLocks(iReq);

                // important: we need to locke the list of processors on all the frozen neurons before locking any frozenLocks of the processors, cause otherwise we can get deadlocks with the 'IsChanged' which first locks the neuron and then the processor, so we must do the same.
            try
            {
                lock (fFrozenNeurons)
                {
                    // needs to be frozen in case one of the neurons gets deleted in another thread which causes it to be removed from this proc, which would cause problems if we don't protect the access from multi threads
                    lock (iProc.fFrozenNeurons)
                    {
                        foreach (var i in fFrozenNeurons)
                        {
                            iProc.fFrozenNeurons.Add(i);
                            i.ChangeFreezeUnsafe(this, iProc);
                        }

                        fFrozenNeurons.Clear();
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
                CurrentProcessor = iPrev;
            }
        }

        /// <summary>Builds the  lock for all the neurons that are currently frozen.</summary>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildFrozenLock(bool writeable)
        {
            var iRes = LockRequestList.Create();
            LockRequestInfo iReq;

            foreach (var i in fFrozenNeurons)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = i;
                iReq.Level = LockLevel.Processors;
                iReq.Writeable = true;
                iRes.Add(iReq);
            }

            return iRes;
        }

        #endregion

        #region Frozen neurons

        /// <summary>copies all the items frozen for this processor to another processor so
        ///     that all items are frozen for both.</summary>
        /// <param name="dest"></param>
        internal void CopyFrozenTo(Processor dest)
        {
            var iReq = BuildFrozenLock(true);
            LockManager.Current.RequestLocks(iReq);

                // important: we need to locke the list of processors on all the frozen neurons before locking any frozenLocks of the processors, cause otherwise we can get deadlocks with the 'IsChanged' which first locks the neuron and then the processor, so we must do the same.
            try
            {
                lock (fFrozenNeurons)
                {
                    // needs to be frozen in case one of the neurons gets deleted in another thread which causes it to be removed from this proc, which would cause problems if we don't protect the access from multi threads
                    lock (dest.fFrozenNeurons)
                    {
                        foreach (var i in fFrozenNeurons)
                        {
                            dest.fFrozenNeurons.Add(i);
                            i.AddFreezeUnsafe(dest);
                        }
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }
        }

        /// <summary>
        ///     Deletes the frozen items.
        /// </summary>
        /// <remarks>
        ///     This is usually empty because it is already copied over by the ThreadManager to the head data, or deleted.
        /// </remarks>
        internal void DeleteFrozenItems()
        {
            Neuron[] iToRemove;
            lock (fFrozenNeurons)
            {
                iToRemove = Enumerable.ToArray(fFrozenNeurons);
                fFrozenNeurons.Clear();
            }

            System.Collections.Generic.List<Neuron> iNextToRemove = null;
            while (iToRemove != null)
            {
                // could be that we have a list of a,b,c where a is the meaning for b, so a can't be deleted until b is deleted. To check for this, use a loop that runs for as long as we can delete something.
                foreach (var i in iToRemove)
                {
                    // we do this outside of the loop, so a delete doesn't cause an error cause a neuron wants to unfreeze (we might be deleting a parent for instance, that has children who are also frozen).
                    if (i.UnFreezeFor(this))
                    {
                        if (Brain.Current.Delete(i, false) == false)
                        {
                            // we are cleaning all frozen items, don't need to unfreeze the item again when deleting, causes a deadlock.
                            if (iNextToRemove == null)
                            {
                                iNextToRemove = new System.Collections.Generic.List<Neuron>();
                            }

                            iNextToRemove.Add(i);
                        }
                    }
                }

                if (iNextToRemove != null && iNextToRemove.Count > 0 && iToRemove.Length != iNextToRemove.Count)
                {
                    // if there are new failed items and we managed to delete some others, do another attempt. Don't try again if all of the items failed deletion: nothing can be done anymore.
                    iToRemove = iNextToRemove.ToArray();
                    iNextToRemove.Clear();
                }
                else
                {
                    iToRemove = null;
                }
            }
        }

        /// <summary>Freezes the specified neuron to the processor so that, when the processor has ended (and the
        ///     entire split tree to which the processor belonged, has also ended, the neuron will be deleted.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void AddFrozenNeuron(Neuron neuron)
        {
            lock (fFrozenNeurons) fFrozenNeurons.Add(neuron);
        }

        /// <summary>Removes the specified frozen neuron from this processor so that it no longer gets deleted
        ///     when all the processing is done.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void RemoveFrozenNeuron(Neuron neuron)
        {
            lock (fFrozenNeurons) fFrozenNeurons.Remove(neuron);
        }

        #endregion

        #region Variables

        /// <summary>Stores the value for the speicified variable.</summary>
        /// <remarks>This is provided, so that inheriters can perform extra tasks when a variable is assigned a new value.</remarks>
        /// <param name="var">The variable to store a new value for.</param>
        /// <param name="value">The value to assign to the var.</param>
        protected internal virtual void StoreVariableValue(Variable var, System.Collections.Generic.List<Neuron> value)
        {
            // this function can be optimized: some use temp lists to pass the data to here, instead fill into the dict's result directly.
            if (Mem.VariableValues.Count > 0)
            {
                var iDict = Mem.VariableValues.Peek();
                System.Diagnostics.Debug.Assert(iDict != null);
                if (value != null)
                {
                    // it could be null, some functions return a null value. If this is the case, we store an empty list.
                    iDict.Set(var, value); // this will consume the value.
                }
                else
                {
                    iDict.Add(var); // if the var is already in the dict, it will be reset.
                }
            }
            else if (ThreadManager.Default.StopRequested == false)
            {
                // if the stop is requested, don't log the error, it is normal
                LogService.Log.LogError("Processor.StoreVariableValue", "Variables dictionary is empty.");
            }
        }

        /// <summary>Stores the value for the speicified global.</summary>
        /// <param name="global"></param>
        /// <param name="value"></param>
        protected internal virtual void StoreGlobalValue(Global global, System.Collections.Generic.List<Neuron> value)
        {
            var iDict = GlobalValues;
            if (global.SplitReactionID != (ulong)PredefinedNeurons.shared)
            {
                // recycle the data when possible.
                System.Collections.Generic.List<Neuron> iOld;
                if (iDict.TryGetValue(global, out iOld))
                {
                    Factories.Default.NLists.Recycle(iOld);
                }
            }

            iDict[global] = value;
        }

        /// <summary>Stores the value for the speicified local. (this is a faster way for locals).
        ///     This is provided for debuggers so they can overwrite if needed.</summary>
        /// <param name="loc">The local</param>
        /// <param name="store">where to store the data.</param>
        /// <param name="value">The data to store.</param>
        protected internal virtual void StoreVariableValue(
            Local loc, 
            VarValuesList store, System.Collections.Generic.List<Neuron> value)
        {
            store.Data = value;
        }

        #endregion

        #endregion
    }
}