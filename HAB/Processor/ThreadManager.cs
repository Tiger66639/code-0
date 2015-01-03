// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadManager.cs" company="">
//   
// </copyright>
// <summary>
//   Determins the different ways that a processor can be started.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Determins the different ways that a processor can be started.
    /// </summary>
    internal enum ThreadRequestType
    {
        /// <summary>
        ///     a Neuron needs to be solved
        /// </summary>
        Normal, 

        /// <summary>
        ///     start after a split
        /// </summary>
        Split, 

        /// <summary>
        ///     the processor needs to be awaken after it was put to sleep
        /// </summary>
        Awake, 

        /// <summary>
        ///     The processor needs to be started for a timer.
        /// </summary>
        Timer, 

        /// <summary>
        ///     a single cluster list needs to be executed.
        /// </summary>
        SingleCall, 

        /// <summary>The blocked.</summary>
        Blocked
    }

    /// <summary>The thread request.</summary>
    internal class ThreadRequest
    {
        /// <summary>
        ///     Gets or sets the requesting processor for the thread.
        /// </summary>
        /// <value>The requestor.</value>
        public Processor Requestor { get; set; }

        /// <summary>
        ///     Gets or sets which function should be used to start the thread for the processor.
        /// </summary>
        /// <value>The start type.</value>
        public ThreadRequestType StartType { get; set; }

        /// <summary>
        ///     Gets or sets the neuroncluster that should be used as a callback, for timers.
        /// </summary>
        /// <value>The callback.</value>
        public NeuronCluster Callback { get; set; }
    }

    /// <summary>
    ///     This class is responsible for managing <see cref="Processor" />s that have been split into many different sub
    ///     processors.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To let the sub processors run in parallel and let them come back to 1 point when they are done, we use the
    ///         technique
    ///         of chaining 2 function calls after each other.  So every sub processor gets a callback ref to the ThreadManager
    ///         (<see cref="ThreadManager.SubProcessorFinished" />) that is called when the sub processor is done.  This checks
    ///         if all processors
    ///         are done and only if that condition is satisfied, allows the last processor to continue.
    ///     </para>
    ///     <para>
    ///         This class also provides thread management.  Only a fixed number of threads are allowed to run at the same time
    ///         as not to stress the
    ///         host app and OS to much. A processor will only be started when there is a thread available. Threads become
    ///         available as a previous
    ///         processor is done, or is blocked through the <see cref="BlockedSolveInstruction" />,
    ///         <see cref="SuspendInstruction" />/
    ///         <see cref="AwakeInstruction" />. We also allow to start a new thread when another one gest blocked, so as to
    ///         avoid the situation where
    ///         all allowed running processors are suspended and need to be awoken from another processor that can't be started
    ///         because  there is
    ///         no available slot.
    ///     </para>
    /// </remarks>
    public class ThreadManager
    {
        #region Inner types

        #region SplitArgs

        /// <summary>
        ///     Arguments for the <see cref="ThreadManager.Split" />
        /// </summary>
        internal class SplitArgs
        {
            /// <summary>Initializes a new instance of the <see cref="SplitArgs"/> class.</summary>
            /// <param name="handler">The handler.</param>
            public SplitArgs(Processor handler)
            {
                ToSplit = Factories.Default.NLists.GetBuffer();
                IsAccum = false;
                ThreadCount = 0;
            }

            /// <summary>
            ///     Gets or sets the list of neurons on which we need to split.
            /// </summary>
            /// <remarks>
            ///     This should also include the value that should be used for the current processor.
            /// </remarks>
            public System.Collections.Generic.List<Neuron> ToSplit { get; private set; }

            /// <summary>
            ///     Gets or sets the variable that will store the split result in each processor.
            /// </summary>
            /// <value>The variable.</value>
            public Variable Variable { get; set; }

            /// <summary>
            ///     Gets or sets the neuroncluster with the callback code that needs to be called when the split is done.
            /// </summary>
            /// <value>The callback.</value>
            public NeuronCluster Callback { get; set; }

            /// <summary>
            ///     Gets or sets the processor for which a split needs to be performed.
            /// </summary>
            /// <value>The processor.</value>
            public Processor Processor { get; set; }

            /// <summary>
            ///     Gets or sets the value that needs to be added, after multiplication by it's index, to each processor.
            /// </summary>
            /// <value>The weight.</value>
            public int Weight { get; set; }

            /// <summary>
            ///     when true, the weight of the results will be added instead of taking the max. Also, when it is an accumulated
            ///     split, the
            ///     sub processors don't inherited the split results of the root processor.
            ///     Default is false.
            /// </summary>
            public bool IsAccum { get; set; }

            /// <summary>
            ///     gets/sets the nr of threads to create
            ///     when 0, each item in the list gets its own thread.
            /// </summary>
            public int ThreadCount { get; set; }

            /// <summary>
            ///     returns the nr of threads that the system creates for this split. This is either 'ThreadCount', or when this is 0,
            ///     the nr of items in Tosplit.
            /// </summary>
            public int ActualNrThreads
            {
                get
                {
                    if (ThreadCount == 0)
                    {
                        if (ToSplit != null)
                        {
                            return ToSplit.Count;
                        }

                        return 0;
                    }

                    return ThreadCount;
                }
            }

            /// <summary>
            ///     releases the list back to the processor.
            /// </summary>
            internal void Recycle()
            {
                Factories.Default.NLists.Recycle(ToSplit);
                ToSplit = null;
            }

            /// <summary>The store var value.</summary>
            /// <param name="i">The i.</param>
            /// <param name="processor">The processor.</param>
            internal void StoreVarValue(int i, Processor processor)
            {
                if (ThreadCount == 0)
                {
                    // if there's an equal amount of processors as items to split, get the single item that needs to be stored.
                    Variable.StoreValue(ToSplit[i], processor);
                }
                else
                {
                    var iValues = Factories.Default.NLists.GetBuffer();
                    int iNrItems, iGroupSize;
                    iGroupSize = ToSplit.Count / ActualNrThreads;
                    if (i < ActualNrThreads - 1)
                    {
                        // if we are not assigning to the last processor, we can do a '/'  otherwise we need to make certain that we take the 'remainder'.
                        iNrItems = iGroupSize;
                    }
                    else
                    {
                        iNrItems = ToSplit.Count - (iGroupSize * i);
                    }

                    iValues.AddRange(ToSplit.GetRange(i * iGroupSize, iNrItems));
                    Variable.StoreValue(iValues, processor);
                }
            }
        }

        #endregion

        #endregion

        #region Fields

        /// <summary>The f default.</summary>
        private static ThreadManager fDefault;

        /// <summary>
        ///     stores all the processors that have requested a thread. Don't use directly, instead, use the
        ///     <see cref="ThreadManager.TryFreeThread" />
        ///     and <see cref="ThreadManager.ReserveThread" />.
        /// </summary>
        private readonly System.Collections.Generic.Queue<ThreadRequest> fRequestedThreads =
            new System.Collections.Generic.Queue<ThreadRequest>();

        /// <summary>
        ///     stores all the processors that are currently running in a thread. Don't use directly, instead, use the
        ///     <see cref="ThreadManager.TryFreeThread" />
        ///     and <see cref="ThreadManager.ReserveThread" />.
        /// </summary>
        private readonly System.Collections.Generic.HashSet<Processor> fRunningThreads =
            new System.Collections.Generic.HashSet<Processor>();

        /// <summary>The f blocked threads.</summary>
        private readonly System.Collections.Generic.HashSet<Processor> fBlockedThreads =
            new System.Collections.Generic.HashSet<Processor>();

                                                                       // keeps trac of the tracks that are currenlty blocked, but which should be considered as 'running' cause they are no longer queued. 

        /// <summary>The f kill block count.</summary>
        private volatile int fKillBlockCount;

        /// <summary>The f stop requested.</summary>
        private volatile bool fStopRequested;

        /// <summary>The f has running processors.</summary>
        private volatile bool fHasRunningProcessors;

        #endregion

        #region Events

        /// <summary>
        ///     Raised when a processor is being split into multiple other processors. The sub processors have
        ///     not yet been started when this event is raised, but they have been created and partially initialized.
        ///     The root processor has not yet been prepared completely.
        /// </summary>
        public event SplitHandler SplitStarting;

        /// <summary>
        ///     Occurs when one or more processors have been started. This event indicates the start of network activity.
        /// </summary>
        public event System.EventHandler ActivityStarted;

        /// <summary>
        ///     Occurs when one all processors have stopped. This event indicates the end of network activity.
        /// </summary>
        public event System.EventHandler ActivityStopped;

        #endregion

        #region Prop

        #region Default

        /// <summary>
        ///     Gets the default splitter for processors.
        /// </summary>
        /// <value>The default.</value>
        public static ThreadManager Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new ThreadManager();
                }

                return fDefault;
            }
        }

        #endregion

        #region StopRequested

        /// <summary>
        ///     Gets wether a stop of all the processors has been requested and not yet fully completed.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool StopRequested
        {
            get
            {
                return fStopRequested;
            }

            internal set
            {
                fStopRequested = value;
            }
        }

        #endregion

        #region HasRunningProcessors

        /// <summary>
        ///     Gets the wether the network still has any running processors left or not.
        /// </summary>
        /// <remarks>
        ///     Suspended processors are not considered to be running.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool HasRunningProcessors
        {
            get
            {
                return fHasRunningProcessors;
            }

            internal set
            {
                if (value != fHasRunningProcessors)
                {
                    fHasRunningProcessors = value;
                    if (value)
                    {
                        OnActivityStarted();
                    }
                    else
                    {
                        OnActivityStopped();
                    }
                }
            }
        }

        #endregion

        #region KillBlockCount

        /// <summary>
        ///     Gets the nr of processors that were set to prevent a 'kill' instruction. (to allow for kill all but this
        ///     functionality).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int KillBlockCount
        {
            get
            {
                return fKillBlockCount;
            }

            private set
            {
                fKillBlockCount = value;
            }
        }

        /// <summary>
        ///     Thread save way to incremtn the KillBlockCount.
        /// </summary>
        internal void KillBlockCountInc()
        {
            lock (fRunningThreads)
            {
                // we use a lock on fRunningthreads, since this is used as lock, when reading the value, to see if all processors have stopped.
                KillBlockCount++;
            }
        }

        /// <summary>
        ///     Thread save way to decrement the KillBlockCount.
        /// </summary>
        internal void KillBlockCountDec()
        {
            lock (fRunningThreads)
            {
                // we use a lock on fRunningthreads, since this is used as lock, when reading the value, to see if all processors have stopped.
                KillBlockCount--;
            }
        }

        #endregion

        /// <summary>The f allow recycling.</summary>
        private bool fAllowRecycling = true;

        #region AllowRecycling

        /// <summary>
        ///     Gets/sets the wether recycling of the processor's data is allowed or not. This is true by default, but can
        ///     cause problems for some debuggers, so it can be disabled for this reason. Watch out, turning this property off
        ///     will cause the system to use lot's more mem.
        /// </summary>
        public bool AllowRecycling
        {
            get
            {
                return fAllowRecycling;
            }

            set
            {
                fAllowRecycling = value;
            }
        }

        #endregion

        #endregion

        #region Functions

        #region Split

        /// <summary>
        ///     Stops all processors. It may take a little time before all processors have actually been stopped.
        /// </summary>
        public void StopAllProcessors()
        {
            Default.StopRequested = true;
        }

        /// <summary>
        ///     Ends a dead lock in the network. This is the most brutal way that a set of processors can be stopped.
        /// </summary>
        public void EndDeadLock()
        {
            Default.StopRequested = true;
            WaitHandleManager.Current.ReleaseAllHandles();

            // Processor[] iToRemove;
            lock (fRunningThreads)

                // blocked processors no longer keep a thread blocked, so nothing special needs to be done.
                fBlockedThreads.Clear();

            // while (iToRemove.Length > 0)                                                   
            // {
            // try
            // {
            // foreach (Processor i in iToRemove)
            // {
            // lock (fRunningThreads)                                         //while setting the waitHandle, make certain it is blocked, don't do the whole loop in the block, so that the other threads can also do some cleanup (they need the lock).
            // {
            // if (i != null && i.BlockedWaitHandle != null)
            // i.BlockedWaitHandle.Set();
            // WaitHandleManager.Current.ReleaseAllHandles();
            // }
            // }
            // lock (fRunningThreads)
            // iToRemove = fBlockedThreads.ToArray();
            // }
            // catch
            // {
            // //discard any errors, simply want to unlock the sytem.
            // }
            // }
            LockManager.Current.ReleaseAllLocks(); // need to make certain that nothing remains active over here.
        }

        /// <summary>Splits a processor into many different processors of the same kind who have the same stack content
        ///     as the original one (cloned). Each sub processor is started.</summary>
        /// <remarks><para>This technique is used when there are many different possible results valid while solving a<see cref="Neuron"/>
        ///         and different paths need to be walked untill only 1 valid remains (or not).</para>
        /// <para>The processor's stack gets cloned to each sub processor. Cloned Neurons are deep copies of an original.  That
        ///         is, they have a
        ///         different ID but the same list contents (links, children, values). the  neurons to which those lists point are
        ///         not cloned.
        ///         Cloning is important, cause during a split you often want to check different possiblities for the same values
        ///         (for instance,
        ///         did the word 'cup' mean cup of tea or world cup). This is done by constructing a KnoledgeNeuron (where, what,
        ///         when,...) which
        ///         should already be on the stack, so this gets duplicated.</para>
        /// <para>A processor can be split multiple times. For as long as the processor or one of it's sub processors is split
        ///         with the same
        ///         callback function and result list, the initial split is extended so it will also contain the newly created sub
        ///         processors.
        ///         If a new callback is created, the joins are stacked, that is, the last split must be completely solved untill
        ///         the split in front
        ///         of it can be solved.</para>
        /// </remarks>
        /// <exception cref="BrainException">When the processor is already the head of a split.</exception>
        /// <param name="args">The args.</param>
        internal void Split(SplitArgs args)
        {
            Processor[] iProcs = null;
            if (args.ActualNrThreads > 1)
            {
                // we need the procs for raising the event.
                iProcs = CreateProcessors(args);
            }

            OnSplit(args, iProcs);

                // we raise the event before we start the threads, so that a host can do stuff with the subs before they are actually started.
            if (args.ActualNrThreads > 1)
            {
                var iSubsForRequestor = new System.Collections.Generic.List<SplitData>(iProcs.Length);

                    // we must create a duplicate of the list for the Requestor's headData. That's because this list can be modified by other threads when they perform a split.
                for (var i = 0; i < iProcs.Length; i++)
                {
                    iSubsForRequestor.Add(iProcs[i].SplitData);
                }

                var iHead = PrepareRequestor(args, iSubsForRequestor);

                    // we need to have the head before preparing all the other processors.
                for (var i = 0; i < iProcs.Length; i++)
                {
                    var iSub = iProcs[i].SplitData;
                    iProcs[i].SplitValues.CurrentWeight += args.Weight * i;
                    iSub.Head = iHead;
                    args.StoreVarValue(i, iProcs[i]);
                    ReserveThread(iProcs[i], ThreadRequestType.Split);
                }

                args.Processor.SplitValues.CurrentWeight += args.Weight * iProcs.Length;
            }
            else
            {
                PrepareRequestor(args, new System.Collections.Generic.List<SplitData>());

                    // if there is only 1 data item in the split, we still need to let the processor know that a split was requested, so that the end code gets called correctly.                                            
            }

            args.StoreVarValue(args.ActualNrThreads - 1, args.Processor);

                // the last value to split on gets assigned to the calling processor.
            args.Processor.SplitData.NrSplitsAfterLastOfSplit++;

                // so that the LastOfSplit knows that a new split was performed.
            args.Recycle();
        }

        /// <summary>Finishes the split for the specified split data.  If this is the last, the callback
        ///     is called.</summary>
        /// <param name="args">The args.</param>
        /// <returns>True if the callback was called, otherwise false.</returns>
        internal bool FinishSplit(SplitData args)
        {
            var iRes = InternalFinishSplit(args);
            if (iRes)
            {
                // if this was the last in the split,we don't need to keep the lock on the head for finishing of the split.  This should make it a bit faster.
                iRes = HandleLastInSplit(args);
            }

            return iRes;
        }

        /// <summary>The internal finish split.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool InternalFinishSplit(SplitData args)
        {
            var iHead = args.Head;
            lock (iHead)
            {
                // we need a lock cause it can be modified from multiple threads (substraced and added when a new split is done)
                if (iHead.StillActive > 1)
                {
                    // we are done.
                    iHead.StillActive--;
                    args.Processor.SplitData = null;

                        // we reset the split data so that the caller knows this split is done (could be that the finisshplit causes a new split, which has to be finished off as well, which needs to be passed along to the caller.
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        // private void ProcessSubProc(Processor proc, Processor copyTo)
        // {
        // proc.CopyResultsTo(copyTo);                                             //no need to apply weight first, is done automatically during the copy
        // //proc.IsRecyclable = true;
        // if (AllowRecycling == true)
        // ProcessorFactory.Recycle(proc);                                                 //the split is done, the processors that aren't the last in the split are ready to be recycled, they have been dead for some time and all their data is now consumed.
        // }

        /// <summary>Performs the final tasks on a split when all the processors have finishted.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool HandleLastInSplit(SplitData args)
        {
            var iRes = true;
            var iDolastInSplit = true;
            while (iDolastInSplit)
            {
                iDolastInSplit = false;
                var iHead = args.Head;
                PrepareResults(args);
                var iFinishPrev = CleanSplitData(args);
                if (fStopRequested == false)
                {
                    args.Processor.CurrentState = Processor.State.Normal;

                        // in case that the processor did an exit, we need to reset it, cause the call might re-ignite the processor and get it running again.
                    args.Processor.CurrentMeaning = null;

                        // we reset the meaning to null primarely so that split paths (for debugs0 know that we are in a call .
                    args.Processor.CodeCluster = iHead.Callback;

                        // so the system knows which root is currently executing (mostly for building split paths -> debugging).
                    Processor.CurrentProcessor = args.Processor;

                        // need to reset the currentprocessor, this isn't done in the 'call'
                    args.Processor.Call(iHead.Callback);

                        // and ask to solve the results. We do a call (so with new vars on the processor) cause the 'join' routine should not have values from the previous processing.
                }

                if ((args.Processor.SplitData != null && args.Processor.Count == 0
                     && args.Processor.SplitData.NrSplitsAfterLastOfSplit > 0)

                    // if the call caused another split, which ended for this processor without new stuff on the stack, we need to call an extra finish, for that new split.
                    || (iHead.Previous != null && fStopRequested))
                {
                    // if we are asking for a stop, and there is still a previous split, also immidiatly to a finishSplit.
                    args = args.Processor.SplitData; // new split, so new splitData
                    iDolastInSplit = InternalFinishSplit(args);
                    iRes = iDolastInSplit;

                        // when there was a new split, but the new finishSplit doesn't promote this proc again as last, then the overal result is taht the current proc is not the last proc of the split 
                }
                else if (iFinishPrev)
                {
                    iDolastInSplit = InternalFinishSplit(args);
                    iRes = iDolastInSplit;
                }
            }

            return iRes;
        }

        /// <summary>cleans the split data + checks if there is a previous split that still needs to be handled.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CleanSplitData(SplitData args)
        {
            var iHead = args.Head;
            if (iHead.Previous == null)
            {
                // we must indicate that the split is over when there is no previous head. This must be done before we call the next code, so that it can initiate a new split correctly.
                args.Processor.SplitData = null;

                    // when the split has been handled, reset the splitdata so that we can properly continue processing if the split callback put any new neurons on the stack.
                return false;
            }

            args.Head = iHead.Previous;
            lock (args.Head)
            {
                var iIndex = args.Head.SubProcessors.IndexOf(iHead.Requestor);

                    // when this processor is the last processor of a sub split, we need to update the splitData-reference in the prev header: the splitData for the requestor that initiated the last split might not be the same (another processor had finished last), so we need to update this to the new processor that contains the result.
                if (iIndex >= 0)
                {
                    args.Head.SubProcessors[iIndex] = args;
                }
                else if (args.Head.Requestor == iHead.Requestor)
                {
                    args.Head.Requestor = args;

                        // if the previous requestor is the same as the current requestor, make certain that the new latest proc takes over from the previous requestor (which has died out)
                }
            }

            args.Processor.SplitData.NrSplitsAfterLastOfSplit = 0;

                // reset this value, so we can see if there were any new splits done during the callback.
            return true;
        }

        /// <summary>The prepare results.</summary>
        /// <param name="args">The args.</param>
        private void PrepareResults(SplitData args)
        {
            var iHead = args.Head;
            if (fStopRequested == false)
            {
                // when a stop is requested, we arent' interested in the resulst, we simply need to stop as fast as possible.
                args.Processor.SplitValues.ApplyWeight();

                    // before we copy over any results from other processors, we need to make certain that the weight of this processor is applied to it's results (as will be done for the results of the other procs when we copy over the data).
                if (iHead.Requestor != args)
                {
                    iHead.Requestor.Processor.CopyResultsTo(args);

                        // ProcessSubProc(iHead.Requestor.Processor, args.Processor);     
                }

                foreach (var i in iHead.SubProcessors)
                {
                    // get all the results from each processor, store in the last proc.
                    if (i != args)
                    {
                        i.Processor.CopyResultsTo(args); // ProcessSubProc(i.Processor, args.Processor);
                    }
                }
            }
        }

        /// <summary>Creates or searches for the correct head data that can be used given the specified arguments
        ///     and assigns it to the requesting processor. Also makes certain that the requesting processor has
        ///     split data.</summary>
        /// <remarks>An already existing headData item can be reused if we are trying to split an already split processor
        ///     which uses the same callback and result list neurons (because we can use the same callback and list).</remarks>
        /// <param name="args">The arguments to check while searching/creating the headData.</param>
        /// <param name="subs">The sub processors that should be assigned to the head.</param>
        /// <returns>A new head data object containing all the data required managing the split, or an already existing
        ///     item which can be reused and to which the sub processors have been added.</returns>
        private HeadData PrepareRequestor(SplitArgs args, System.Collections.Generic.List<SplitData> subs)
        {
            var iPrevSplit = args.Processor.SplitData;
            if (iPrevSplit != null &&

                // the calling proc is already part of a split, so we need to check if we can reuse the head or if we need to create a new one.
                iPrevSplit.Head.Callback == args.Callback)
            {
                if (args.IsAccum)
                {
                    // don't change back, only assign when accum has been requested 1 time.
                    iPrevSplit.IsAccum = true; // no need for lock, when assigned 1 time, always the same value.
                }

                lock (iPrevSplit.Head)
                {
                    iPrevSplit.Head.StillActive += subs.Count;

                        // we can't add 1 more here, as we do for the init setup.  The current thread is already included in the count.
                    iPrevSplit.Head.SubProcessors.AddRange(subs);
                    return iPrevSplit.Head;
                }
            }

            var iHeadData = new HeadData();
            iHeadData.StillActive = subs.Count + 1;

                // this value is assigned to the subdata of the head processor (this one).  + 1 cause we also need to take this processor into account.
            iHeadData.SubProcessors = subs; // this indicates that this processor becomes a head.
            iHeadData.Callback = args.Callback;
            if (iPrevSplit != null)
            {
                lock (iPrevSplit.Head) iHeadData.Previous = iPrevSplit.Head;
                if (Settings.LogSplitToOtherCallBack)
                {
                    LogService.Log.LogWarning(
                        "Split", 
                        string.Format(
                            "Performing a split with a callback ({0}) that differs from the previous callback ({1})", 
                            iHeadData.Callback.ID, 
                            iPrevSplit.Head.Callback.ID));
                }
            }
            else
            {
                args.Processor.SplitData = new SplitData();
                args.Processor.SplitData.Processor = args.Processor;
            }

            iHeadData.Requestor = args.Processor.SplitData;
            if (args.IsAccum)
            {
                // don't change back, only assign when accum has been requested 1 time.
                iHeadData.Requestor.IsAccum = true; // no need for lock, when assigned 1 time, always the same value.
            }

            args.Processor.SplitData.Head = iHeadData;
            return iHeadData;
        }

        /// <summary>Creates sub processors and provides all the data for them so that they are a duplicate of this one.</summary>
        /// <param name="args">Determins how many processors, from where to copy,...</param>
        /// <returns>The processors that were created wrapped into SplitData.</returns>
        private Processor[] CreateProcessors(SplitArgs args)
        {
            var iCount = args.ActualNrThreads - 1; // important: -1 cause we already have the proc of the caller.
            var iProc = args.Processor;

            var iSubs = iProc.CreateProcessors(iCount);
            var iSplitter = iProc.CloneProcessors(iSubs);
            iSplitter.Recycle(); // after the split is done, make certain we re-use as much mem as possible.
            return iSubs;
        }

        #endregion

        #region Buffering

        /// <summary>The release and get next.</summary>
        /// <param name="finished">The finished.</param>
        /// <returns>The <see cref="Processor"/>.</returns>
        internal Processor ReleaseAndGetNext(Processor finished)
        {
            var iRes = TryFreeThread(finished);
            if (AllowRecycling)
            {
                ProcessorFactory.Recycle(finished); // when processing is done, we can be reused.
            }

            return iRes;
        }

        /// <summary>The try free thread.</summary>
        /// <param name="finished">The finished.</param>
        /// <returns>The <see cref="Processor"/>.</returns>
        internal Processor TryFreeThread(Processor finished)
        {
            bool iIsDone;
            Processor iRes = null;
            lock (fRunningThreads)
            {
                fRunningThreads.Remove(finished);
                iIsDone = fRequestedThreads.Count + fRunningThreads.Count + fBlockedThreads.Count - KillBlockCount == 0;

                    // needs to be calculated before we restart any other thread.
                if (finished.NeedsBlockHandleRelease && finished.BlockedProcessor != null)
                {
                    // needs to be done inside the lock, for the TryStartNewThread, cause it modifies the lists.
                    iRes = finished.BlockedProcessor;
                    fBlockedThreads.Remove(iRes);
                    if (iRes.CurrentState != Processor.State.Terminated)
                    {
                        fRunningThreads.Add(iRes);

                            // only add as a running proc when it wasn't finished, otherwise we lock the engine 
                        if (finished.Mem.LastReturnValues.Count > 0)
                        {
                            foreach (var i in Enumerable.Reverse(finished.Mem.LastReturnValues))
                            {
                                iRes.Mem.LastReturnValues.Push(i); // copy over all the return values when done.
                            }

                            finished.Mem.LastReturnValues.Clear();
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Free thread", 
                            "Trying to start a blocked processor that's already terminated. This can be caused by 2 consecutive splits with other callbacks");
                        iRes = null;
                        TryStartNewThread(); // only start a new thread if this is not done yet.
                    }
                }
                else
                {
                    TryStartNewThread(); // only start a new thread if this is not done yet.
                }
            }

            if (iIsDone)
            {
                // if no more waiting threads, and the only ones that are still running were the blocked ones, reset the stop.
                StopRequested = false;
                foreach (var i in fRunningThreads)
                {
                    // when the stop has finished, we need to reset all the processors that were blocked from stopping, otherwise we can't stop them anymore.
                    i.ResetPreventKill();
                }

                foreach (var i in fRequestedThreads)
                {
                    i.Requestor.ResetPreventKill();
                }

                if (fRunningThreads.Count == 0 && fBlockedThreads.Count == 0)
                {
                    // if all are done, Let the brain know that there are no longer any threads running.
                    HasRunningProcessors = false;
                }

                KillBlockCount = 0;

                    // we make certain this is reset, so we don't get into accidents with this value on next runs.
            }

            return iRes;
        }

        /// <summary>Reserves the thread for a processor so that it will be started as soon as a new thread becomes available.
        ///     This may be immediatly.</summary>
        /// <remarks>All requests are queued, except <see cref="ThreadRequestType.Blocked"/> ones, which are handled immediatly.
        ///     This is because the requesting processor needs to wait untill the new one is free again, so it's thread has
        ///     become available, which we directly use for the new request. We do this to prevent a<see cref="BlockedSolveInstruction"/>
        ///     from taking to long. In other words, this is a way to take a shortcut in the queue.</remarks>
        /// <param name="toStart">To start.</param>
        /// <param name="startAs">Determins which function should be used to start the thread.</param>
        /// <param name="toCall">The to Call.</param>
        internal void ReserveThread(Processor toStart, ThreadRequestType startAs, NeuronCluster toCall = null)
        {
            if (Settings.MaxConcurrentProcessors == 0)
            {
                throw new System.InvalidOperationException(
                    "MaxConcurrentProcessors can not be 0, because than there are no processors allowed.");
            }

            lock (fRunningThreads)
            {
                HasRunningProcessors = true;
                if (startAs != ThreadRequestType.Blocked)
                {
                    var iNew = new ThreadRequest { Requestor = toStart, StartType = startAs, Callback = toCall };
                    fRequestedThreads.Enqueue(iNew);
                    TryStartNewThread();
                }
                else
                {
                    fRunningThreads.Remove(Processor.CurrentProcessor);
                    fBlockedThreads.Add(Processor.CurrentProcessor);
                    fRunningThreads.Add(toStart);
                    toStart.BlockedProcessor = Processor.CurrentProcessor;
                    Processor.CurrentProcessor = toStart; // let the caller know we switch processor.
                }
            }
        }

        /// <summary>Reserves the thread for a processor so that it will be started as soon as a new thread becomes available.
        ///     This may be immediatly.</summary>
        /// <remarks>All requests are queued, except <see cref="ThreadRequestType.Blocked"/> ones, which are handled immediatly.
        ///     This is because the requesting processor needs to wait untill the new one is free again, so it's thread has
        ///     become available, which we directly use for the new request. We do this to prevent a<see cref="BlockedSolveInstruction"/>
        ///     from taking to long. In other words, this is a way to take a shortcut in the queue.</remarks>
        /// <param name="toStart">To start.</param>
        /// <param name="callback">The callback.</param>
        internal void ReserveTimer(Processor toStart, NeuronCluster callback)
        {
            if (Settings.MaxConcurrentProcessors == 0)
            {
                throw new System.InvalidOperationException(
                    "MaxConcurrentProcessors can not be 0, because than there are no processors allowed.");
            }

            lock (fRunningThreads)
            {
                HasRunningProcessors = true;
                var iNew = new ThreadRequest
                               {
                                   Requestor = toStart, 
                                   StartType = ThreadRequestType.Timer, 
                                   Callback = callback
                               };
                fRequestedThreads.Enqueue(iNew);
                TryStartNewThread();
            }
        }

        /// <summary>
        ///     This is an internal function that tries the start new thread for a processor that is queued.
        ///     This should only be called from within a thread safe block, caus this function isn't.
        /// </summary>
        private void TryStartNewThread()
        {
            if (fRunningThreads.Count < Settings.MaxConcurrentProcessors && fRequestedThreads.Count > 0)
            {
                var iReq = fRequestedThreads.Dequeue();
                switch (iReq.StartType)
                {
                    case ThreadRequestType.Normal:
                        fRunningThreads.Add(iReq.Requestor);

                            // needs to be done before activating the thread, otherwise the callback might be finished before the processor is added as running.
                        System.Threading.ThreadPool.QueueUserWorkItem(RunInternalSolve, iReq.Requestor);

                            // we use the callback, so we can remove the proc from the RunningThreads list when it is done. There are other situations when it needs to be removed, but it always recovers from that, so it also needs to be removed when it's done  It can also happen when a tread is blocked.
                        break;
                    case ThreadRequestType.Split:
                        fRunningThreads.Add(iReq.Requestor);

                            // needs to be done before activating the thread, otherwise the callback might be finished before the processor is added as running.
                        System.Threading.ThreadPool.QueueUserWorkItem(RunSplit, iReq.Requestor);
                        break;
                    case ThreadRequestType.Awake:
                        fRunningThreads.Add(iReq.Requestor);

                            // needs to be done before activating the thread, otherwise the callback might be finished before the processor is added as running.
                        iReq.Requestor.ThreadBlocker.Set();
                        break;
                    case ThreadRequestType.Timer:
                        fRunningThreads.Add(iReq.Requestor);

                            // needs to be done before activating the thread, otherwise the callback might be finished before the processor is added as running.
                        System.Threading.ThreadPool.QueueUserWorkItem(RunTimer, iReq);
                        break;
                    case ThreadRequestType.SingleCall:
                        fRunningThreads.Add(iReq.Requestor);

                            // needs to be done before activating the thread, otherwise the callback might be finished before the processor is added as running.
                        System.Threading.ThreadPool.QueueUserWorkItem(RunSingleCall, iReq);
                        break;
                    case ThreadRequestType.Blocked:

                        // don't do anything.  this is handled by the ReserveThread call
                        break;
                    default:
                        throw new System.InvalidOperationException("Unknown start type.");
                }
            }
        }

        /// <summary>performs a single call</summary>
        /// <param name="toStart"></param>
        private void RunSingleCall(object toStart)
        {
            var iReq = (ThreadRequest)toStart;

            var iProc = iReq.Requestor;
            var iContinueBlockedSolve = false;
            while (iProc != null)
            {
                try
                {
                    if (iContinueBlockedSolve)
                    {
                        iContinueBlockedSolve = false;
                        Processor.ContinueBlocedProcessor(iProc);
                    }
                    else if (Processor.CurrentProcessor == null)
                    {
                        iProc.CallSingleSync(iReq.Callback);
                    }
                    else if (iProc.ToCall != null)
                    {
                        // we did a blocked call.
                        var iToCall = iProc.ToCall;
                        iProc.ToCall = null; // make certain that this values gets reset.
                        iProc.CallSingleSync(iToCall);
                    }
                    else
                    {
                        iProc.InternalSolve(); // it's a blocked solve.
                    }

                    if (Processor.CurrentProcessor != null)
                    {
                        // do this before the assign, otherwise the TryFreeThread no longer has an argument.
                        iProc = Processor.CurrentProcessor;

                            // could be that a blocked-solve was requested, in which case we need to continue processing.
                        continue;
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("TreadManager.RunSingleCall", e.ToString());
                }

                iProc = ReleaseAndGetNext(iProc);

                    // TryFreeThread(iProc);                                                     //we need to let the split manager know that this thread is finished. Could be taht we need to start another proc that was blocked.
                if (iProc != null)
                {
                    iContinueBlockedSolve = true;
                }
            }
        }

        /// <summary>starts a new timer object.</summary>
        /// <param name="toStart"></param>
        private void RunTimer(object toStart)
        {
            var iReq = (ThreadRequest)toStart;

            var iProc = iReq.Requestor;
            var iContinueBlockedSolve = false;
            while (iProc != null)
            {
                try
                {
                    if (iContinueBlockedSolve)
                    {
                        iContinueBlockedSolve = false;
                        Processor.ContinueBlocedProcessor(iProc);
                    }
                    else if (Processor.CurrentProcessor == null)
                    {
                        iProc.StartForTimer(iReq.Callback);
                    }
                    else if (iProc.ToCall != null)
                    {
                        // we did a blocked call.
                        var iToCall = iProc.ToCall;
                        iProc.ToCall = null; // make certain that this values gets reset.
                        iProc.CallSingleSync(iToCall);
                    }
                    else
                    {
                        iProc.InternalSolve(); // it's a blocked solve.
                    }

                    if (Processor.CurrentProcessor != null)
                    {
                        // do this before the assign, otherwise the TryFreeThread no longer has an argument.
                        iProc = Processor.CurrentProcessor;

                            // could be that a blocked-solve was requested, in which case we need to continue processing.
                        continue;
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("TreadManager.RunTimer", e.ToString());
                }

                iProc = ReleaseAndGetNext(iProc);

                    // TryFreeThread(iProc);                                                   //we need to let the split manager know that this thread is finished. Could be taht we need to start another proc that was blocked.
                if (iProc != null)
                {
                    iContinueBlockedSolve = true;
                }
            }
        }

        /// <summary>Runs the split instruction (for delegates).</summary>
        /// <param name="toStart">To start.</param>
        private void RunSplit(object toStart)
        {
            var iProc = (Processor)toStart;
            var iContinueBlockedSolve = false;
            while (iProc != null && (iProc.KillRequested == false || iProc.PreventKill))
            {
                try
                {
                    if (iContinueBlockedSolve)
                    {
                        iContinueBlockedSolve = false;
                        Processor.ContinueBlocedProcessor(iProc);
                    }
                    else if (Processor.CurrentProcessor == null)
                    {
                        iProc.SolveStackAfterSplit();
                    }
                    else if (iProc.ToCall != null)
                    {
                        // we did a blocked call.
                        var iToCall = iProc.ToCall;
                        iProc.ToCall = null; // make certain that this values gets reset.
                        iProc.CallSingleSync(iToCall);
                    }
                    else
                    {
                        iProc.InternalSolve();

                            // it's a blocked solve.                                                                                     //continue a new stack calc on the a new proc (called by blocked solve).
                    }

                    if (Processor.CurrentProcessor != null
                        && Processor.CurrentProcessor.CurrentState != Processor.State.Terminated)
                    {
                        // do this before the assign, otherwise the TryFreeThread no longer has an argument.
                        iProc = Processor.CurrentProcessor;

                            // could be that a blocked-solve was requested, in which case we need to continue processing.
                        continue;
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("TreadManager.RunSplit", e.ToString());
                }

                iProc = ReleaseAndGetNext(iProc);

                    // TryFreeThread(iProc);                                                   //we need to let the split manager know that this thread is finished. Could be taht we need to start another proc that was blocked.
                if (iProc != null)
                {
                    iContinueBlockedSolve = true;
                }
            }
        }

        /// <summary>starts a normal operation</summary>
        /// <param name="toStart">To start.</param>
        private void RunInternalSolve(object toStart)
        {
            var iProc = (Processor)toStart;
            var iContinueBlockedSolve = false;
            while (iProc != null)
            {
                try
                {
                    if (iContinueBlockedSolve)
                    {
                        iContinueBlockedSolve = false;
                        Processor.ContinueBlocedProcessor(iProc);
                    }
                    else if (iProc.ToCall != null)
                    {
                        // we did a blocked call.
                        var iToCall = iProc.ToCall;
                        iProc.ToCall = null; // make certain that this values gets reset.
                        iProc.CallSingleSync(iToCall);
                    }
                    else
                    {
                        iProc.InternalSolve(); // it's a blocked solve.
                    }

                    if (Processor.CurrentProcessor != null)
                    {
                        // do this before the assign, otherwise the TryFreeThread no longer has an argument.
                        iProc = Processor.CurrentProcessor;

                            // could be that a blocked-solve was requested, in which case we need to continue processing.
                        continue;
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("TreadManager.RunInternalSolve", e.ToString());
                }

                iProc = ReleaseAndGetNext(iProc);

                    // TryFreeThread(iProc);                                                   //we need to let the split manager know that this thread is finished.Could be taht we need to start another proc that was blocked.
                if (iProc != null)
                {
                    iContinueBlockedSolve = true;
                }
            }
        }

        #endregion

        #region Event callers

        /// <summary>
        ///     Called when processor activity has started.
        /// </summary>
        protected void OnActivityStarted()
        {
            if (ActivityStarted != null)
            {
                ActivityStarted(this, System.EventArgs.Empty);
            }
        }

        /// <summary>The on activity stopped.</summary>
        protected void OnActivityStopped()
        {
            if (ActivityStopped != null)
            {
                ActivityStopped(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Called when the <see cref="ThreadManager"/> performed a new split.</summary>
        /// <param name="args">The args.</param>
        /// <param name="subProcs">The sub procs.</param>
        internal void OnSplit(SplitArgs args, Processor[] subProcs)
        {
            if (SplitStarting != null)
            {
                var iArgs = new SplitEventArgs
                                {
                                    Callback = args.Callback, 
                                    Processor = args.Processor, 
                                    Variable = args.Variable
                                };

                // this is the public version of the split arguments data.
                iArgs.ToSplit.AddRange(args.ToSplit);
                if (subProcs != null)
                {
                    iArgs.SubProcessors = new System.Collections.Generic.List<Processor>(subProcs);
                }
                else
                {
                    iArgs.SubProcessors = new System.Collections.Generic.List<Processor>();
                }

                SplitStarting(this, iArgs);
            }
        }

        #endregion

        // public string DumpStacks()
        // {
        // StringBuilder iRes = new StringBuilder();
        // foreach (Processor i in fRunningThreads)
        // DumpStack(i, iRes);
        // foreach (Processor i in fBlockedThreads)
        // DumpStack(i, iRes);
        // return iRes.ToString();
        // }

        // private void DumpStack(Processor proc, StringBuilder res)
        // {
        // if ((proc.Thread.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != System.Threading.ThreadState.WaitSleepJoin)
        // {
        // StackTrace iTrace = new StackTrace(proc.Thread, true);
        // res.AppendLine(iTrace.ToString());
        // }
        // }
        #endregion
    }
}