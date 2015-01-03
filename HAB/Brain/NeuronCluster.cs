// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronCluster.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="Neuron" /> that represents a set of other neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see cref="Neuron" /> that represents a set of other neurons.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For instance, all the neurons that have the same link meaning in a
    ///         <see cref="Neuron.LinksTo" /> list, or a global neuron that has many
    ///         specific instances, like the neuroncluster representing the name 'Jan',
    ///         with specific children, me, and my current neighbour, Jan De Ceeser (or
    ///         something). A <see cref="NeuronCluster" /> can also represent a sequence
    ///         (of digits to form a number for instance) or a logical relation between
    ///         other neurons (jan and tom). A custer has a
    ///         <see cref="JaStDev.HAB.NeuronCluster.Meaning" /> property that defines
    ///         the meaning of the cluster. Another possible meaning can be 'function',
    ///         in which case it should contain <see cref="Expression" /> neurons.
    ///     </para>
    ///     <para>
    ///         All operations on the <see cref="JaStDev.HAB.NeuronCluster.Children" />
    ///         list should be locked.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.NeuronCluster, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Object, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Asset, typeof(NeuronCluster))]

    // this is a cluster so that we can also store the assets in there.
    [NeuronID((ulong)PredefinedNeurons.POSGroup, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ImportanceLevel, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Frame, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VisualFrame, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Frames, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ValueToInspect, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameSequences, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameSequence, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameSequenceItemValue, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameElementId, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameImportance, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Frame_Core, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Frame_peripheral, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Frame_extra_thematic, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameElementAllowMulti, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetRole, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetRestrictions, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetLogicValue, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetRestriction, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetRestrictionModifier, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.RestrictionModifierInclude, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.RestrictionModifierExclude, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.IsFrameEvoker, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FrameElementResultType, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.RestrictionDefinesFullContent, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.SubFrame, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.LemmaId, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Flow, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowItemConditionalPart, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowItemConditional, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowItemIsLoop, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowItemRequiresSelection, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowIsFloating, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowIsNonDestructiveFloating, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FloatingFlowKeepsData, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.RequiresFloatingSeparator, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FloatingFlowSplits, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FlowType, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TextPatternTopic, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.PatternRule, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TopicFilter, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TextPatternOutputs, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.InvalidResponsesForPattern, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.RequiresResponse, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ResponseForOutputs, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ParsedPatternStart, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ParsedPatternPart, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ParsedPatternOutput, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ResponsesForEmptyParse, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.UsedResponses, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ConversationStarts, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.ParsedVariable, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.ParsedThesVar, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.ParsedAssetVar, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.DoPatterns, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Operand, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ParsedDoPattern, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.AssignAdd, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.AssignAddNot, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.AssignRemove, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.OutputListTraversalMode, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Random, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Sequence, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.DoAfterStatement)]
    [NeuronID((ulong)PredefinedNeurons.RepeatOutputPatterns)]
    [NeuronID((ulong)PredefinedNeurons.DoOnStartup)]
    [NeuronID((ulong)PredefinedNeurons.InputPatternPartialMode, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.PartialInputPattern, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.PartialInputPatternFallback, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Calculate, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Evaluate, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Context, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.PatternAtStartOfInput, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.PatternAtEndOfInput, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Delay, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.CollectSpaces, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.SubTopics, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.SubRules, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ConversationLogHistory, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.OnStarted)]
    [NeuronID((ulong)PredefinedNeurons.OnShutDown)]
    [NeuronID((ulong)PredefinedNeurons.OnSinActivity)]
    [NeuronID((ulong)PredefinedNeurons.OnSinCreated)]
    [NeuronID((ulong)PredefinedNeurons.OnSinDestroyed)]
    public class NeuronCluster : Neuron
    {
        /// <summary>The f buffered children.</summary>
        private System.Collections.IList fBufferedChildren; // for fast access by the processors.

        /// <summary>The f buffer ref count.</summary>
        private int fBufferRefCount;

                    // keeps track of how many threads are curently using the bufferedChildren: this is so we know when we can recycle the list and when we still need to wait.

        /// <summary>The f meaning.</summary>
        private ulong fMeaning = EmptyId;

        /// <summary>Gets the f children.</summary>
        private ChildList FChildren
        {
            get
            {
                lock (this)
                {
                    // checking + creating needs to be atomic, otherwise duplicate lists.
                    if (ChildrenIdentifier == null)
                    {
                        ChildrenIdentifier = Factories.Default.ChildLists.GetList(this);

                            // this list should always be made: a cluster normally has children.
                    }
                }

                return ChildrenIdentifier;
            }
        }

        /// <summary>resturns the first child of the cluster.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron FindFirstChild()
        {
            ulong iRes = 0;
            if (ChildrenIdentifier != null)
            {
                var iChildren = Children;
                iChildren.Lock();
                try
                {
                    if (ChildrenIdentifier.Count > 0)
                    {
                        iRes = ChildrenIdentifier[0];
                    }
                }
                finally
                {
                    iChildren.Dispose();
                }
            }

            if (iRes != 0)
            {
                return Brain.Current[iRes];
            }

            return null;
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="NeuronCluster" /> class.
        /// </summary>
        /// <remarks>
        ///     This constructor is <see langword="public" /> for streaming, should be
        ///     fixed later on
        /// </remarks>
        internal NeuronCluster()
        {
        }

        /// <summary>Finalizes an instance of the <see cref="NeuronCluster"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="NeuronCluster"/> is reclaimed by garbage
        ///     collection.</summary>
        ~NeuronCluster()
        {
            fMeaning = 0;
        }

        #endregion

        #region prop

        #region Children

        /// <summary>Gets the children.</summary>
        public ChildrenAccessor Children
        {
            get
            {
                return Factories.Default.ChildrenAccFactory.Get(FChildren, this, false);
            }
        }

        /// <summary>
        ///     Gets the children accessor in a writable mode, which is faster for
        ///     editing.
        /// </summary>
        /// <value>
        ///     The children W.
        /// </value>
        public ChildrenAccessor ChildrenW
        {
            get
            {
                return Factories.Default.ChildrenAccFactory.Get(FChildren, this, true);
            }
        }

        /// <summary>
        ///     Gets an object that uniquely identifies the list that was changed.
        ///     This can be used in dicationaries for instance, like the designer's
        ///     eventManager.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public ChildList ChildrenIdentifier { get; private set; }

        /// <summary>
        ///     Provides direct access to the ChildList. Only use when inside a lock.
        /// </summary>
        /// <remarks>
        ///     This has to be <see langword="public" /> cause the
        ///     <see cref="EventManager" /> uses this prop to monitor changes.
        /// </remarks>
        /// <value>
        ///     The identifier.
        /// </value>
        public ChildList ChildrenDirect
        {
            get
            {
                return FChildren;
            }
        }

        /// <summary>The release buffered children.</summary>
        /// <param name="list">The list.</param>
        internal void ReleaseBufferedChildren(System.Collections.IList list)
        {
            if (list != null)
            {
                lock (this)
                {
                    fBufferRefCount--;
                    if (fBufferRefCount == 0 && fBufferedChildren != list)
                    {
                        if (list is System.Collections.Generic.List<Neuron>)
                        {
                            Factories.Default.NLists.Recycle((System.Collections.Generic.List<Neuron>)list, false);
                        }
                        else if (list is System.Collections.Generic.List<NeuronCluster>)
                        {
                            Factories.Default.CLists.Recycle(
                                (System.Collections.Generic.List<NeuronCluster>)list, 
                                false);
                        }
                    }
                }
            }
        }

        /// <summary>if there is a buffered list, return this.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="IList"/>.</returns>
        internal System.Collections.IList TryGetBufferedChildren()
        {
            lock (this)
            {
                if (fBufferedChildren != null)
                {
                    fBufferRefCount++;
                }

                return fBufferedChildren;
            }
        }

        /// <summary>Gets the children, when possible from a buffered list, otherwise, the
        ///     new list is buffered. This is a speed optimizer for clusters invlolved
        ///     with the processor: so that we don't always need to rebuild lists that
        ///     never change. Whenever the cluster is changed, the lsit is cleaned.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="IList"/>.</returns>
        internal System.Collections.Generic.IList<T> GetBufferedChildren<T>() where T : Neuron
        {
            lock (this)
            {
                // we need to make the bool check and return a single operation (still not enough though to make certain that the buffer doesn't get recycled during usage.
                if (fBufferedChildren is System.Collections.Generic.IList<T>)
                {
                    fBufferRefCount++;
                    return (System.Collections.Generic.IList<T>)fBufferedChildren;
                }
            }

            var iMemFac = Factories.Default;
            System.Collections.Generic.List<T> iResList;
            if (typeof(T) == typeof(NeuronCluster))
            {
                iResList = iMemFac.CLists.GetBuffer() as System.Collections.Generic.List<T>;
            }
            else if (typeof(T) == typeof(Neuron))
            {
                iResList = iMemFac.NLists.GetBuffer() as System.Collections.Generic.List<T>;
            }
            else
            {
                iResList = new System.Collections.Generic.List<T>();
            }

            if (ChildrenIdentifier != null)
            {
                // if there is no list of children, don't need to try and buffer anything.
                System.Collections.Generic.List<ulong> iIds;
                LockManager.Current.RequestLock(this, LockLevel.Children, false);
                try
                {
                    iIds = iMemFac.IDLists.GetBuffer(ChildrenIdentifier.Count);
                    iIds.AddRange(ChildrenIdentifier);
                }
                finally
                {
                    LockManager.Current.ReleaseLock(this, LockLevel.Children, false);
                }

                lock (this)
                    fBufferedChildren = null;

                        // make certain that we assign the value at the end of calculation. do inside a lock, to be save
                if (iResList.Capacity < iIds.Count)
                {
                    iResList.Capacity = iIds.Count;
                }

                foreach (var i in iIds)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        iResList.Add((T)iFound);
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }

            lock (this)
            {
                // make certain that only 1 thread can access this code at a time for this object. This is to make certain that we don't have mem leaks because multpile threads calcualted the same list and try to store it at the same time.
                fBufferRefCount++;
                if (fBufferedChildren == null)
                {
                    // don't need to assign it 2 times, first result is ok.
                    fBufferedChildren = iResList;
                }
                else if (iResList is System.Collections.Generic.List<Neuron>)
                {
                    iMemFac.NLists.Recycle(iResList as System.Collections.Generic.List<Neuron>);
                }
                else if (iResList is System.Collections.Generic.List<NeuronCluster>)
                {
                    iMemFac.CLists.Recycle(iResList as System.Collections.Generic.List<NeuronCluster>);
                }
            }

            return (System.Collections.Generic.IList<T>)fBufferedChildren;
        }

        #endregion

        #region Meaning

        /// <summary>
        ///     Gets/sets the meaning of the relationship between the different items
        ///     in this cluster.
        /// </summary>
        /// <remarks>
        ///     <para>Examples of meanings are: or, and, number, sequence.</para>
        ///     <para>
        ///         This also keeps track of the
        ///         <see cref="JaStDev.HAB.Neuron.MeaningUsageCount" /> whenever the
        ///         meaning of this cluster is changed. When set, and the cluster is stil
        ///         a temporaryr cluster, it is regiestered to the network.
        ///     </para>
        /// </remarks>
        public ulong Meaning
        {
            get
            {
                return fMeaning;
            }

            set
            {
                System.Diagnostics.Debug.Assert(value != TempId);
                if (fMeaning != value)
                {
                    if (ID == TempId)
                    {
                        Brain.Current.Add(this); // do before trying to create a lock.
                    }

                    Neuron iOld, iNew;
                    var iReq = BuildMeaningChangeLock(value, out iOld, out iNew);
                    LockManager.Current.RequestLocks(iReq);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        if (iOld != null)
                        {
                            iOld.DecMeaningUnsafe();
                        }

                        fMeaning = value;
                        if (iNew != null)
                        {
                            iNew.IncMeaningUnsafe();
                        }

                        if (Brain.Current.HasNeuronChangedEvents)
                        {
                            Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Meaning", this));
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iReq);
                    }

                    IsChanged = true;
                    if (iOld != null)
                    {
                        iOld.IsChanged = true;
                    }

                    if (iNew != null)
                    {
                        iNew.IsChanged = true;
                    }
                }
            }
        }

        /// <summary>Sets the meaning through a neuron and not an ID. This is a bit faster
        ///     for some <see langword="internal"/> functions.</summary>
        /// <remarks>A bit faster.</remarks>
        /// <param name="value">The value.</param>
        internal void SetMeaning(Neuron value)
        {
            System.Diagnostics.Debug.Assert(value.ID != EmptyId);
            if (value.ID == TempId)
            {
                Brain.Current.Add(value); // we need to make certain that the item is registered in the network.
            }

            if (ID == TempId)
            {
                Brain.Current.Add(this); // do before trying to create a lock.
            }

            if (fMeaning != value.ID)
            {
                Neuron iOld;
                var iReq = BuildMeaningChangeLock(value, out iOld);
                LockManager.Current.RequestLocks(iReq);

                    // we need to make the value-assign + IsChanged-assign a singleton operation, so that the Flusher (or anything else) can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                try
                {
                    if (fMeaning != EmptyId)
                    {
                        iReq[2].Neuron.DecMeaningUnsafe();
                    }

                    fMeaning = value.ID;
                    if (fMeaning != EmptyId)
                    {
                        value.IncMeaningUnsafe();
                    }

                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Meaning", this));
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iReq);
                }

                IsChanged = true;
                if (value != null)
                {
                    value.IsChanged = true;
                }

                if (iOld != null)
                {
                    iOld.IsChanged = true;
                }
            }
        }

        /// <summary>Builds a lock for a change in meaning.</summary>
        /// <param name="value">The value.</param>
        /// <param name="old">The old.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildMeaningChangeLock(Neuron value, out Neuron old)
        {
            var iRes = LockRequestList.Create();

            var iReq = LockRequestInfo.Create();
            iReq.Neuron = this;
            iReq.Level = LockLevel.Value;
            iReq.Writeable = true;
            iRes.Add(iReq);

            if (value != this)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = value;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                iRes.Add(iReq);
            }

            if (fMeaning != EmptyId && fMeaning != ID && fMeaning != value.ID)
            {
                // don't do duplicate locks.
                iReq = LockRequestInfo.Create();
                iReq.Neuron = Brain.Current[fMeaning];
                old = iReq.Neuron;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                iRes.Add(iReq);
            }
            else
            {
                old = null;
            }

            return iRes;
        }

        /// <summary>Builds a lock for a change in meaning.</summary>
        /// <param name="value">The value.</param>
        /// <param name="oldVal">The old Val.</param>
        /// <param name="newVal">The new Val.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildMeaningChangeLock(ulong value, out Neuron oldVal, out Neuron newVal)
        {
            var iRes = LockRequestList.Create();

            var iReq = LockRequestInfo.Create();
            iReq.Neuron = this;
            iReq.Level = LockLevel.Value;
            iReq.Writeable = true;
            iRes.Add(iReq);

            if (value != EmptyId && value != ID)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = Brain.Current[value];
                newVal = iReq.Neuron;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                iRes.Add(iReq);
            }
            else
            {
                newVal = null;
            }

            if (fMeaning != EmptyId && fMeaning != value && fMeaning != ID)
            {
                // don't lock the same list 2 times: otherwise problems.
                iReq = LockRequestInfo.Create();
                iReq.Neuron = Brain.Current[fMeaning];
                oldVal = iReq.Neuron;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                iRes.Add(iReq);
            }
            else
            {
                oldVal = null;
            }

            return iRes;
        }

        /// <summary>Builds a lock for the cluster and all it's children.</summary>
        /// <param name="writeable">The writeable.</param>
        /// <param name="children">The children.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildClusterLock(bool writeable, System.Collections.Generic.List<Neuron> children)
        {
            LockRequestInfo iReq;
            LockRequestInfo iFound;

            var iMemFac = Factories.Default;
            System.Collections.Generic.List<ulong> iIds;
            var iDict = iMemFac.LockRequestDicts.GetBuffer();
            iReq = LockRequestInfo.Create(this, LockLevel.Children, writeable);
            iDict.Add(ID, iReq);
            using (var iChildren = Children)
            {
                iIds = iMemFac.IDLists.GetBuffer(iChildren.CountUnsafe);
                iIds.AddRange(iChildren);
            }

            foreach (var i in iIds)
            {
                // can't get the neurons inside he accessor lock cause that can cause deadlocks.s
                var iN = Brain.Current[i];
                children.Add(iN);

                    // so that the caller also has access to the neurons themselves without having to do an extra cache search again.
                if (iDict.TryGetValue(i, out iFound) == false
                    || (iFound.Level != LockLevel.Parents && iFound.Level != LockLevel.All))
                {
                    // need to prevent duplicates, cause this can screw up the lock system.
                    iReq = LockRequestInfo.Create(iN, LockLevel.Parents, writeable);
                    iDict.Add(i, iReq);
                }
            }

            var iRes = LockRequestList.CreateBig();
            iRes.AddRange(iDict.Values);
            iMemFac.IDLists.Recycle(iIds);
            iMemFac.LockRequestDicts.Recycle(iDict);
            return iRes;
        }

        /// <summary>Sets the meaning without performing any locking (so unsafe) + without
        ///     changing the frozen state. This is used during a duplication process,
        ///     when everything is already locked.</summary>
        /// <remarks>Doesn't remove any previous meaning, since this is only called on the
        ///     duplicates, which are always new.</remarks>
        /// <param name="value">The value.</param>
        internal void SetMeaningUnsafe(Neuron value)
        {
            System.Diagnostics.Debug.Assert(value.ID != EmptyId);
            if (value.ID == TempId)
            {
                throw new System.NotSupportedException(
                    "can't create new objects while assigning data during a duplication stage.");
            }

            if (fMeaning != value.ID)
            {
                fMeaning = value.ID;
                if (fMeaning != EmptyId)
                {
                    value.IncMeaningUnsafe();
                }

                if (Brain.Current.HasNeuronChangedEvents)
                {
                    Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Meaning", this));
                }
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NeuronCluster" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.NeuronCluster;
            }
        }

        #endregion

        /// <summary>The recycle data.</summary>
        /// <param name="fromGC">The from gc.</param>
        protected override void RecycleData(bool fromGC)
        {
            base.RecycleData(fromGC);
            if (ChildrenIdentifier != null)
            {
                var iMemFac = Factories.Default;
                iMemFac.ChildLists.Recycle(ChildrenIdentifier, fromGC);

                    // no need to recycle the IDlist itself, this can be buffered, which saves a little time for reusing it.
                ChildrenIdentifier = null;
            }
        }

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            ClearBufferedChildren(fromGC);
        }

        /// <summary>so we can clear the list of children seperatly.</summary>
        /// <param name="fromGC"></param>
        internal void ClearBufferedChildren(bool fromGC = false)
        {
            lock (this)
            {
                // do a lock when we clear out the buffers: could be that 2 threads are trying to clear at the same time: double recycled list: oeps.
                if (fBufferedChildren != null)
                {
                    if (fBufferRefCount == 0)
                    {
                        // only recycle when no-one is using the list.
                        if (fBufferedChildren is System.Collections.Generic.List<Neuron>)
                        {
                            Factories.Default.NLists.Recycle(
                                (System.Collections.Generic.List<Neuron>)fBufferedChildren, 
                                fromGC);
                        }
                        else if (fBufferedChildren is System.Collections.Generic.List<NeuronCluster>)
                        {
                            Factories.Default.CLists.Recycle(
                                (System.Collections.Generic.List<NeuronCluster>)fBufferedChildren, 
                                fromGC);
                        }
                    }

                    fBufferedChildren = null;
                }
            }
        }

        #endregion

        #region Functions

        /// <summary>Sets the id without notifying of the change to the brain. This is used
        ///     by the brain itself when the neuron is added.</summary>
        /// <param name="p">The p.</param>
        protected internal override void SetId(ulong p)
        {
            base.SetId(p);
            if (IsEmpty(p) == false && fMeaning != EmptyId)
            {
                // when the meaning gets set when there is no id, we don't let the other object know, to keep it all ok when the object is never used (it would not decrement it's id).
                Brain.Current[fMeaning].MeaningUsageCount++;
            }
        }

        /// <summary>The change type to.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public override Neuron ChangeTypeTo(System.Type type)
        {
            ClearChildren();

                // before changing the type from cluster to something else, make certain that all the children have been unregistered, otherwise we get false references (the cluster is gone, but the child still has a ref).
            return base.ChangeTypeTo(type);
        }

        /// <summary>Compares this neuron with anohter neuron using the specified operator.</summary>
        /// <remarks><para>The <paramref name="right"/> part doesn't have to be solved in
        ///         advance. So it may be expressions which will be solved at the
        ///         appropriate time, when needed. This means that for some operators, the<paramref name="right"/> part doesn't have to be solved (logical
        ///         and/or).</para>
        /// <para>In this base implementation, the compare is performed on the id's,
        ///         descendents can change this if they want (to compare numbers or string
        ///         for instance).</para>
        /// </remarks>
        /// <param name="right">The neuron to compare it with.</param>
        /// <param name="op">The <see langword="operator"/> to use.</param>
        /// <returns>True if both id's are the same.</returns>
        protected internal override bool CompareWith(Neuron right, Neuron op)
        {
            if (op.ID == (ulong)PredefinedNeurons.And)
            {
                return base.CompareWith(right, op);
            }

            var iRight = right as NeuronCluster;
            if (iRight != null)
            {
                var iThisTime = Time.GetTime(this);
                if (iThisTime.HasValue)
                {
                    var iRightTime = Time.GetTime(iRight);
                    if (iRightTime.HasValue)
                    {
                        return CompareDateTime(iThisTime, iRightTime, op);
                    }
                }
                else
                {
                    var iThisTimeSpan = Time.GetTimeSpan(this);
                    if (iThisTimeSpan.HasValue)
                    {
                        var iRightTimeSpan = Time.GetTimeSpan(iRight);
                        if (iRightTimeSpan.HasValue)
                        {
                            return CompareTimeSpan(iThisTimeSpan, iRightTimeSpan, op);
                        }
                    }
                }
            }

            return base.CompareWith(right, op);
        }

        /// <summary>The compare time span.</summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="op">The op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CompareTimeSpan(System.TimeSpan? left, System.TimeSpan? right, Neuron op)
        {
            switch (op.ID)
            {
                case (ulong)PredefinedNeurons.Equal:
                    return left == right;
                case (ulong)PredefinedNeurons.Smaller:
                    return left < right;
                case (ulong)PredefinedNeurons.SmallerOrEqual:
                    return left <= right;
                case (ulong)PredefinedNeurons.Bigger:
                    return left > right;
                case (ulong)PredefinedNeurons.BiggerOrEqual:
                    return left >= right;
                case (ulong)PredefinedNeurons.Different:
                    return left != right;
                default:
                    LogService.Log.LogError(
                        "NeuronCluster.CompareWith", 
                        string.Format("Invalid operator found: {0}.", op));
                    return false;
            }
        }

        /// <summary>The compare date time.</summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="op">The op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CompareDateTime(System.DateTime? left, System.DateTime? right, Neuron op)
        {
            switch (op.ID)
            {
                case (ulong)PredefinedNeurons.Equal:
                    return left == right;
                case (ulong)PredefinedNeurons.Smaller:
                    return left < right;
                case (ulong)PredefinedNeurons.SmallerOrEqual:
                    return left <= right;
                case (ulong)PredefinedNeurons.Bigger:
                    return left > right;
                case (ulong)PredefinedNeurons.BiggerOrEqual:
                    return left >= right;
                case (ulong)PredefinedNeurons.Different:
                    return left != right;
                default:
                    LogService.Log.LogError(
                        "NeuronCluster.CompareWith", 
                        string.Format("Invalid operator found: {0}.", op));
                    return false;
            }
        }

        /// <summary>Removes the child in an unsafe manner (no locking), but registers the
        ///     change.</summary>
        /// <param name="toRemove">To remove.</param>
        internal void RemoveChildUnsafe(Neuron toRemove)
        {
            FChildren.RemoveDirect(toRemove);

                // we do a direct remove, don't need to remove the neuron from our list as well, only in 1 direction.
            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>removes all references to the pseicified item without updating the
        ///     list in the item and without locking anything.</summary>
        /// <param name="toRemove"></param>
        internal void RemoveAllChildrenUnsafe(Neuron toRemove)
        {
            while (FChildren.RemoveDirect(toRemove))
            {
                ; // we do a direct remove, don't need to remove the neuron from our list as well, only in 1 direction.
            }

            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>Adds the neurons as children without locking anything + doesnt' change
        ///     the frozen state. This is called durign the duplication process, when
        ///     everything is already locked.</summary>
        /// <param name="list">The list.</param>
        internal void AddChildrenUnsafe(System.Collections.Generic.List<Neuron> list)
        {
            FChildren.AddRange(list);
            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>adds the child without triggering an update in the child itself.</summary>
        /// <param name="neuron"></param>
        internal void AddChildDirect(Neuron neuron)
        {
            FChildren.InsertDirect(ChildrenIdentifier.Count, neuron);
            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>The add child unsafe.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void AddChildUnsafe(Neuron neuron)
        {
            FChildren.Add(neuron);
            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>The insert child unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <param name="neuron">The neuron.</param>
        internal void InsertChildUnsafe(int index, Neuron neuron)
        {
            if (index != -1)
            {
                // if there was no valid index nr, it is added to the back.
                FChildren.Insert(index, neuron);
            }
            else
            {
                FChildren.Add(neuron);
            }

            ClearBufferedChildren();

                // needs to be done now inside the list, this is the savest so that other threads get updated asap.
        }

        /// <summary>
        ///     Clears the list of children in a THREAD SAVE way. All the children are
        ///     also locked before the operation is performed.
        /// </summary>
        internal void ClearChildren()
        {
            if (ChildrenIdentifier != null)
            {
                // if there aren't any children, don't need to clear anything.
                var iMemFac = Factories.Default;
                var iChildren = iMemFac.NLists.GetBuffer();
                var iReq = BuildClusterLock(true, iChildren);
                LockManager.Current.RequestLocks(iReq);
                try
                {
                    if (ChildrenIdentifier.Count > 0 && ChildrenIdentifier.Count != iChildren.Count)
                    {
                        // if the content of the list was changed while getting the data for the lock and actually locking it, do a slower, but more secure remove.
                        for (var i = 0; i < iChildren.Count; i++)
                        {
                            // the last item in iReq is the lock on the cache (auto created by the LockManager), this doens't have a neuron, so skip this.
                            iChildren[i].RemoveClusterUnsafe(this);
                            if (ChildrenIdentifier[i] == iChildren[i].ID)
                            {
                                ChildrenIdentifier.RemoveAt(i);

                                    // this is a little faster, but not always correct (if there was an insert or something).
                            }
                            else
                            {
                                ChildrenIdentifier.Remove(iChildren[i]);
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < iChildren.Count; i++)
                        {
                            // the last item in iReq is the lock on the cache (auto created by the LockManager), this doens't have a neuron, so skip this.
                            iChildren[i].RemoveClusterUnsafe(this);
                        }

                        ChildrenIdentifier.ClearDirect();

                            // don't use 'clear', this will try to call 'REmoveCluster again, which is already done.
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iReq, true);
                    for (var i = 0; i < iChildren.Count; i++)
                    {
                        // the last item in iReq is the lock on the cache (auto created by the LockManager), this doens't have a neuron, so skip this.
                        iChildren[i].IsChanged = true;
                    }

                    IsChanged = true;
                    iMemFac.NLists.Recycle(iChildren);
                }
            }
        }

        /// <summary><para>Tries to add the specified child to the list. This is thread save.
        ///         When this cluster was deleted after the lock, the add isn't performed
        ///         and we don't wait for the delete to finish.</para>
        /// <para>Reads the class from xml file.</para>
        /// </summary>
        /// <param name="reader"></param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            ReadNormalXml(reader);
        }

        /// <summary>The read normal xml.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadNormalXml(System.Xml.XmlReader reader)
        {
            if (reader.LocalName == "Meaning")
            {
                // the very original version forgot to save the meaning value, so we need to check if the element exists.
                fMeaning = XmlStore.ReadElement<ulong>(reader, "Meaning");
            }

            var iIsEmpty = reader.IsEmptyElement;
            reader.ReadStartElement("Children");
            if (iIsEmpty == false)
            {
                ChildrenIdentifier = Factories.Default.ChildLists.GetList(this);
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    var iChild = XmlStore.ReadElement<ulong>(reader, "ID");
                    ChildrenIdentifier.AddFromLoad(iChild);
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Writes the class to xml files</summary>
        /// <param name="writer">The xml writer to use</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "Meaning", Meaning);
            XmlStore.WriteIDList(writer, "Children", FChildren);
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
            fMeaning = reader.ReadUInt64();
            var iNrChildren = reader.ReadInt32();
            if (iNrChildren > 0)
            {
                ChildrenIdentifier = Factories.Default.ChildLists.GetList(this, iNrChildren);
                while (iNrChildren > 0)
                {
                    iNrChildren--;
                    var iNew = reader.ReadUInt64();
                    ChildrenIdentifier.AddFromLoad(iNew);
                }
            }

            return iRes;
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            writer.Write(fMeaning);
            if (ChildrenIdentifier != null)
            {
                // if there is no list, nothing to write.
                WriteIDList(writer, ChildrenIdentifier.List);
            }
            else
            {
                writer.Write(0);
            }
        }

        #endregion
    }
}