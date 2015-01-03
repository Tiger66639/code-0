// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Neuron.cs" company="">
//   
// </copyright>
// <summary>
//   stores the list of links that need to be resolved and optionally an index into the list.
//   The index is created when tere are more links to resolve then neurons in the cache. In that case, it's faster to
//   reverse
//   the search and loop through cache while searching the list of links.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     stores the list of links that need to be resolved and optionally an index into the list.
    ///     The index is created when tere are more links to resolve then neurons in the cache. In that case, it's faster to
    ///     reverse
    ///     the search and loop through cache while searching the list of links.
    /// </summary>
    public class LinkResolverDataList
    {
        /// <summary>The index.</summary>
        public System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, int>> Index;

        /// <summary>The items.</summary>
        public System.Collections.Generic.List<Link> Items = Factories.Default.LinkLists.GetBuffer();
    }

    /// <summary>
    ///     temporarely stores the link data so we can resolve them in bulk when the whole neuron is loaded and there are no
    ///     more
    ///     locks that can cause deadlocks.
    /// </summary>
    public class LinkResolverData
    {
        // public Neuron Value;
        /// <summary>The links in.</summary>
        public LinkResolverDataList LinksIn = new LinkResolverDataList();

        /// <summary>The links out.</summary>
        public LinkResolverDataList LinksOut = new LinkResolverDataList();

        /// <summary>
        ///     the neuron that still needs solving.
        /// </summary>
        public Neuron ToResolve;

        /// <summary>The recycle.</summary>
        internal void Recycle()
        {
            var iMemFac = Factories.Default;
            if (LinksIn != null && LinksIn.Index == null)
            {
                // don't recycle the data if there was an index, cause in that case, the Items list was assigned to the neuron itself (no list copy).
                iMemFac.LinkLists.Recycle(LinksIn.Items);
            }

            if (LinksOut != null && LinksOut.Index == null)
            {
                // don't recycle the data if there was an index, cause in that case, the Items list was assigned to the neuron itself (no list copy).
                iMemFac.LinkLists.Recycle(LinksOut.Items);
            }
        }
    }

    /// <summary>
    ///     Base class for all objects that can be stored in a <see cref="Brain" />.
    /// </summary>
    /// <remarks>
    ///     It provides a list of all the neurons that this object links to + a list of all
    ///     the objects that link to this one.  Both lists are non mutiple.  To add
    ///     links, use the <see cref="Link" /> object and set it's <see cref="Link.To" /> and <see cref="Link.From" />
    ///     properties, or call the <see cref="Link.Destroy" /> function to remove the link
    ///     <para>
    ///         Descendents should re implement <see cref="Neuron.InternalDuplicateValues" />
    ///     </para>
    /// </remarks>

    #region Default neurons
    [NeuronID((ulong)PredefinedNeurons.Actions)]
    [NeuronID((ulong)PredefinedNeurons.Arguments)]
    [NeuronID((ulong)PredefinedNeurons.ArgumentsList)]
    [NeuronID((ulong)PredefinedNeurons.Case)]
    [NeuronID((ulong)PredefinedNeurons.Children)]
    [NeuronID((ulong)PredefinedNeurons.CaseLooped)]
    [NeuronID((ulong)PredefinedNeurons.Code)]
    [NeuronID((ulong)PredefinedNeurons.Condition)]
    [NeuronID((ulong)PredefinedNeurons.ForEach)]
    [NeuronID((ulong)PredefinedNeurons.QueryLoop)]
    [NeuronID((ulong)PredefinedNeurons.QueryLoopChildren)]
    [NeuronID((ulong)PredefinedNeurons.QueryLoopClusters)]
    [NeuronID((ulong)PredefinedNeurons.QueryLoopIn)]
    [NeuronID((ulong)PredefinedNeurons.QueryLoopOut)]
    [NeuronID((ulong)PredefinedNeurons.In)]
    [NeuronID((ulong)PredefinedNeurons.InfoToSearchFor)]
    [NeuronID((ulong)PredefinedNeurons.Instruction)]
    [NeuronID((ulong)PredefinedNeurons.LeftPart)]
    [NeuronID((ulong)PredefinedNeurons.ListToSearch)]
    [NeuronID((ulong)PredefinedNeurons.Looped)]
    [NeuronID((ulong)PredefinedNeurons.LoopItem)]
    [NeuronID((ulong)PredefinedNeurons.CaseItem)]
    [NeuronID((ulong)PredefinedNeurons.LoopStyle)]
    [NeuronID((ulong)PredefinedNeurons.Normal)]
    [NeuronID((ulong)PredefinedNeurons.Operator)]
    [NeuronID((ulong)PredefinedNeurons.Out)]
    [NeuronID((ulong)PredefinedNeurons.RightPart)]
    [NeuronID((ulong)PredefinedNeurons.Rules)]
    [NeuronID((ulong)PredefinedNeurons.SearchFor)]
    [NeuronID((ulong)PredefinedNeurons.Statements)]
    [NeuronID((ulong)PredefinedNeurons.ToSearch)]
    [NeuronID((ulong)PredefinedNeurons.Until)]
    [NeuronID((ulong)PredefinedNeurons.Height)]
    [NeuronID((ulong)PredefinedNeurons.Width)]
    [NeuronID((ulong)PredefinedNeurons.Empty)]
    [NeuronID((ulong)PredefinedNeurons.Neuron)]
    [NeuronID((ulong)PredefinedNeurons.Attribute)]
    [NeuronID((ulong)PredefinedNeurons.Amount)]
    [NeuronID((ulong)PredefinedNeurons.Index)]
    [NeuronID((ulong)PredefinedNeurons.LinkOut)]
    [NeuronID((ulong)PredefinedNeurons.List)]

    #endregion

    public class Neuron : System.Xml.Serialization.IXmlSerializable, 
                          Storage.NDB.INDBStreamable, 
                          IGetBool, 
                          IGetInt, 
                          IGetDouble, 
                          System.IComparable<Neuron>
    {
        /// <summary>Initializes a new instance of the <see cref="Neuron"/> class.</summary>
        internal Neuron()
        {
        }

        #region Rules

        /// <summary>
        ///     Gets the list of instructions that determin if this neuron is a valid value for the
        ///     current state of the <see cref="Brain" /> to be used by the <see cref="Processor" />.
        /// </summary>
        /// <remarks>
        ///     When the processor needs to get a neuron object for a translation result, it uses this
        ///     list of instructions to determin if the neuron is valid.
        /// </remarks>
        public System.Collections.ObjectModel.ReadOnlyCollection<Expression> Rules
        {
            get
            {
                var iCluster = RulesCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Expression> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Expression>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(iExps);
                    }

                    LogService.Log.LogError(
                        "Neuron.Rules", 
                        string.Format("Failed to convert rules list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region RulesCluster

        /// <summary>
        ///     Gets/sets the cluster containing all the expressions used as rules.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The NeuronCluster is stored in the <see cref="Neuron.LinksOut" /> list as a link with the
        ///         meaning 'Rules'. If there is no relationship, with the meaning, null is returned.s
        ///     </para>
        ///     <para>
        ///         see <see cref="Neuron.Rules" /> for more info.
        ///     </para>
        /// </remarks>
        public NeuronCluster RulesCluster
        {
            get
            {
                if (fRulesList == null)
                {
                    fRulesList = (NeuronCluster)FindFirstOut((ulong)PredefinedNeurons.Rules);
                }

                return fRulesList;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Rules, value);
            }
        }

        #endregion

        #region Actions

        /// <summary>
        ///     Gets the list of instructions that are executed after the <see cref="Processor" /> has
        ///     solved this neuron.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This can be used to perform actions at the end of handling every link. For instance, each link
        ///         can try to extract argument values while the actions list
        ///     </para>
        /// </remarks>
        public System.Collections.ObjectModel.ReadOnlyCollection<Expression> Actions
        {
            get
            {
                var iCluster = ActionsCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Expression> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Expression>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(iExps);
                    }

                    LogService.Log.LogError(
                        "Neuron.Actions", 
                        string.Format("Failed to convert actions list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region ActionsCluster

        /// <summary>
        ///     Gets/sets the cluster containing all the expressions used as actions.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The NeuronCluster is stored in the <see cref="Neuron.LinksOut" /> list as a link with the
        ///         meaning 'Actions'. If there is no relationship, with the meaning, null is returned.
        ///     </para>
        ///     <para>
        ///         see <see cref="Neuron.Actions" /> for more info.
        ///     </para>
        /// </remarks>
        public NeuronCluster ActionsCluster
        {
            get
            {
                return (NeuronCluster)FindFirstOut((ulong)PredefinedNeurons.Actions);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Actions, value);
            }
        }

        #endregion

        #region ModuleIndex

        /// <summary>
        ///     Gets the number of modules that reference this object. When greater then 0, it is referenced
        ///     and can not be deleted from the network.
        /// </summary>
        public int ModuleRefCount
        {
            get
            {
                return fModuleRefCount;
            }

            set
            {
                fModuleRefCount = value;
                SetIsChangedNoClearBuffers(true);

                    // we need to make certain that this data is saved. Don't need to clear any buffers, cause those aren't changed.
            }
        }

        #endregion

        #region AccessCounter

        /// <summary>
        ///     Gets how many times that this Neuron has been read by the <see cref="Brain" /> from file or internal storage.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         this is used to optimize the load of the brain so it can unload items correctly
        ///     </para>
        ///     <para>
        ///         This value is persisted, so it is accumulative.  Probably should best split this up eventually in a session
        ///         counter and accumulative one so optimizations for both can be made.
        ///     </para>
        ///     <para>
        ///         Can be turned on/off through <see cref="Settins.TrackNeuronAccess" />
        ///     </para>
        /// </remarks>
        public uint AccessCounter
        {
            get
            {
                return fAccessCounter;
            }

            internal set
            {
                if (Settings.TrackNeuronAccess && fAccessCounter != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fAccessCounter = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    IsChanged = true;
                }
            }
        }

        #endregion

        #region ClusteredByW

        /// <summary>
        ///     Gets the thread manager that provides access to the list of NeuronCluster id's that this Neuron is a child of.
        /// </summary>
        public ClustersAccessor ClusteredByW
        {
            get
            {
                return Factories.Default.ClustersAccFactory.Get(FClusterdBy, this, true);
            }
        }

        #endregion

        #region ClusteredByDirect

        /// <summary>
        ///     Provides direct access to the <see cref="ClusterList" />.
        /// </summary>
        /// <value>The identifier.</value>
        public ClusterList ClusteredByDirect
        {
            get
            {
                return FClusterdBy;
            }
        }

        #endregion

        #region CanBeDeleted

        /// <summary>
        ///     Gets a value indicating whether this instance can be deleted.
        /// </summary>
        /// <remarks>
        ///     An item can't be deleted when:
        ///     - it is used as the value for 1 or more <see cref="Link.Meaning" /> props,
        ///     - it is used as the value for 1 or more <see cref="NeuronCluster.Meaning" /> props,
        ///     - it is used as a value in one or more <see cref="Link.Info" /> lists or,
        ///     - it is a statically created item (id less than <see cref="PredefinedNeurons.Dynamic" />
        /// </remarks>
        /// <value>
        ///     <c>true</c> if this instance can be deleted; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeDeleted
        {
            get
            {
                return MeaningUsageCount == 0 && InfoUsageCount == 0 && ID >= (ulong)PredefinedNeurons.Dynamic;
            }
        }

        #endregion

        #region IComparable<Neuron> Members

        /// <summary>Compares this neuron with the other neuron.
        ///     By default, this first converts the neuron to a string and compares the 2 string values.</summary>
        /// <param name="other">The other.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public virtual int CompareTo(Neuron other)
        {
            return ToString().CompareTo(other.ToString());
        }

        #endregion

        /// <summary>Finalizes an instance of the <see cref="Neuron"/> class. </summary>
        ~Neuron()
        {
            if (System.Environment.HasShutdownStarted == false && typeof(Sin).IsInstanceOfType(this) == false)
            {
                // there are normally not many sin records. don't recycle them. When shutting down, simply let the neuron die, no need to buffer again.
                fID = 0;
                fInfoUsageCount = 0;
                fMeaningUsageCount = 0;
                fModuleRefCount = 0;
                fIsChanged = false;
                fIsChanged = false;
                fIsFrozen = false;
                RecycleData(true);
                ClearBuffers(true);
                if (NeuronFactory.Recycle(this))
                {
                    // make the neuron available again for reuse.
                    System.GC.ReRegisterForFinalize(this); // revive the object so the mem remains available.
                }
            }
        }

        /// <summary>Checks if the link exists between the args. Done from here, to get to the raw lists.
        ///     This is done in a thread unsafe way, only call this when the items have already been locked.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool LinkExistsUnsafe(Link.CreateArgs args)
        {
            if (args.To.LinksInIdentifier != null && args.From.LinksOutIdentifier != null)
            {
                if (args.From.FLinksOut.Count < args.To.FLinksIn.Count)
                {
                    foreach (var i in args.From.FLinksOut)
                    {
                        if (i.ToID == args.To.ID && i.MeaningID == args.Meaning.ID)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    foreach (var i in args.To.FLinksIn)
                    {
                        if (i.FromID == args.From.ID && i.MeaningID == args.Meaning.ID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>checks if the link exists, in an unsafe way (nothing is locked).</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool LinkExistsUnsafe(Neuron from, Neuron to, ulong meaningID)
        {
            if (to.LinksInIdentifier != null && from.LinksOutIdentifier != null)
            {
                if (from.fSortedLinksOut != null)
                {
                    System.Collections.Generic.Dictionary<ulong, Link> iFound;
                    if (@from.fSortedLinksOut.TryGetValue(meaningID, out iFound))
                    {
                        return iFound.ContainsKey(to.ID);
                    }
                }
                else if (to.fSortedLinksIn != null)
                {
                    System.Collections.Generic.Dictionary<ulong, Link> iFound;
                    if (to.fSortedLinksIn.TryGetValue(meaningID, out iFound))
                    {
                        return iFound.ContainsKey(from.ID);
                    }
                }
                else if (from.FLinksOut.Count < to.FLinksIn.Count)
                {
                    foreach (var i in from.FLinksOut)
                    {
                        if (i.ToID == to.ID && i.MeaningID == meaningID)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    foreach (var i in to.FLinksIn)
                    {
                        if (i.FromID == from.ID && i.MeaningID == meaningID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>Gets the common parents of the arguments in an unsafe.</summary>
        /// <param name="list">The list.</param>
        /// <param name="result">The result.</param>
        internal static void GetCommonParentUnsafe(System.Collections.Generic.IList<Neuron> list, System.Collections.Generic.List<Neuron> result)
        {
            var iStart = GetParentsSearchStart(list); // speedup
            if (iStart != null && iStart.fClusterdBy != null)
            {
                // if it is not clustered at all,  there can't be any results.
                var iPosResults = Factories.Default.IDLists.GetBuffer(iStart.fClusterdBy.Count);
                try
                {
                    iPosResults.AddRange(iStart.fClusterdBy);

                        // make a copy cause we are going to change the contents of the array.
                    System.Collections.Generic.HashSet<ulong> iSearchList;
                    foreach (var u in list)
                    {
                        if (u != iStart)
                        {
                            if (u.FClusterdBy.Count > 10)
                            {
                                // if fClusteredBy is big, we create a temp hasShet to speed things up
                                iSearchList = u.GetBufferedClusteredByHashUnsave();
                                try
                                {
                                    for (var i = 0; i < iPosResults.Count; i++)
                                    {
                                        if (iPosResults[i] > EmptyId && iSearchList.Contains(iPosResults[i]) == false)
                                        {
                                            iPosResults[i] = EmptyId;
                                        }
                                    }
                                }
                                finally
                                {
                                    u.ReleaseClusteredByHashUnsave(iSearchList);
                                }
                            }
                            else
                            {
                                for (var i = 0; i < iPosResults.Count; i++)
                                {
                                    if (iPosResults[i] > EmptyId && u.FClusterdBy.Contains(iPosResults[i]) == false)
                                    {
                                        iPosResults[i] = EmptyId;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var i in iPosResults)
                    {
                        if (i != EmptyId)
                        {
                            result.Add(Brain.Current[i]);
                        }
                    }
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iPosResults);
                }
            }
        }

        /// <summary>Gets the common parents of the arguments in an unsafe way. All values are returned as a a list of ulongs.</summary>
        /// <param name="list">The list.</param>
        /// <returns>A list of items ulong values which represents the list of comon parents. Values can be 0, so check for this.</returns>
        internal static System.Collections.Generic.List<ulong> GetCommonParentsUnsafeUlong(System.Collections.Generic.IList<Neuron> list)
        {
            System.Collections.Generic.List<ulong> iPosResults = null;
            var iStart = GetParentsSearchStart(list);
            if (iStart != null && iStart.fClusterdBy != null)
            {
                iPosResults = Factories.Default.IDLists.GetBuffer(iStart.fClusterdBy.Count);
                iPosResults.AddRange(iStart.fClusterdBy);

                    // make a copy cause we are going to change the contents of the array.
                System.Collections.Generic.HashSet<ulong> iSearchList;
                foreach (var u in list)
                {
                    if (u != iStart)
                    {
                        if (u.fClusterdBy.Count > 10)
                        {
                            // if fClusteredBy is big, we create a temp hasShet to speed things up. Can always be certain that clusteredBy is set, cause if it isn't, it was the smallest list and we woulnd't have gotten ths far.
                            iSearchList = u.GetBufferedClusteredByHashUnsave();
                            try
                            {
                                for (var i = 0; i < iPosResults.Count; i++)
                                {
                                    if (iPosResults[i] > EmptyId && iSearchList.Contains(iPosResults[i]) == false)
                                    {
                                        iPosResults[i] = EmptyId;
                                    }
                                }
                            }
                            finally
                            {
                                u.ReleaseClusteredByHashUnsave(iSearchList);
                            }
                        }
                        else
                        {
                            for (var i = 0; i < iPosResults.Count; i++)
                            {
                                if (iPosResults[i] > EmptyId && u.FClusterdBy.Contains(iPosResults[i]) == false)
                                {
                                    iPosResults[i] = EmptyId;
                                }
                            }
                        }
                    }
                }
            }

            return iPosResults;
        }

        /// <summary>Gets the neuron with the smallest parents list, to start the search at.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron GetParentsSearchStart(System.Collections.Generic.IList<Neuron> list)
        {
            var iCount = int.MaxValue;
            Neuron iRes = null;
            foreach (var i in list)
            {
                if (i.fClusterdBy != null && i.fClusterdBy.Count < iCount)
                {
                    iCount = i.fClusterdBy.Count;
                    iRes = i;
                }
                else if (i.fClusterdBy == null || i.fClusterdBy.Count == 0)
                {
                    // if 1 has no parents -> no possible results.
                    return null;
                }
            }

            return iRes;
        }

        /// <summary>returns all the neurons to which the parameters point to with a link.</summary>
        /// <param name="list"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected static System.Collections.Generic.List<ulong> GetCommonOutUnsafeUlong(System.Collections.Generic.IList<Neuron> list)
        {
            System.Collections.Generic.List<ulong> iResults = null;
            var iStart = GetLinksOutSearchStart(list);
            if (iStart != null && iStart.LinksOutIdentifier != null)
            {
                iResults = Factories.Default.IDLists.GetBuffer(iStart.LinksOutIdentifier.Count);
                iResults.AddRange(from i in iStart.LinksOutIdentifier select i.ToID);

                    // make a copy cause we are going to change the contents of the array.
                var iSearchList = new System.Collections.Generic.HashSet<ulong>();
                foreach (var u in list)
                {
                    if (u != iStart)
                    {
                        iSearchList.Clear();
                        foreach (var i in u.LinksInIdentifier)
                        {
                            iSearchList.Add(i.ToID);
                        }

                        for (var i = 0; i < iResults.Count; i++)
                        {
                            if (iResults[i] > EmptyId && iSearchList.Contains(iResults[i]) == false)
                            {
                                iResults[i] = EmptyId;
                            }
                        }
                    }
                }
            }

            return iResults;
        }

        /// <summary>returns all the neurons to which the parameters point to with a link.</summary>
        /// <param name="list"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected static System.Collections.Generic.List<ulong> GetCommonInUnsafeUlong(System.Collections.Generic.IList<Neuron> list)
        {
            System.Collections.Generic.List<ulong> iResults = null;
            var iStart = GetLinksInSearchStart(list);
            if (iStart != null)
            {
                iResults = Factories.Default.IDLists.GetBuffer(iStart.LinksInIdentifier.Count);
                iResults.AddRange(from i in iStart.LinksInIdentifier select i.FromID);

                    // make a copy cause we are going to change the contents of the array.
                var iSearchList = new System.Collections.Generic.HashSet<ulong>();
                foreach (var u in list)
                {
                    if (u != iStart)
                    {
                        iSearchList.Clear();
                        foreach (var i in u.LinksInIdentifier)
                        {
                            iSearchList.Add(i.FromID);
                        }

                        for (var i = 0; i < iResults.Count; i++)
                        {
                            if (iResults[i] > EmptyId && iSearchList.Contains(iResults[i]) == false)
                            {
                                iResults[i] = EmptyId;
                            }
                        }
                    }
                }
            }

            return iResults;
        }

        /// <summary>Gets the neuron with the smallest parents list, to start the search at.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron GetLinksOutSearchStart(System.Collections.Generic.IList<Neuron> list)
        {
            var iCount = int.MaxValue;
            Neuron iRes = null;
            foreach (var i in list)
            {
                if (i.LinksOutIdentifier != null && i.LinksOutIdentifier.Count < iCount)
                {
                    iCount = i.LinksOutIdentifier.Count;
                    iRes = i;
                }
                else if (i.LinksOutIdentifier == null || i.LinksOutIdentifier.Count == 0)
                {
                    // if 1 has no parents -> no possible results.
                    return null;
                }
            }

            return iRes;
        }

        /// <summary>Gets the neuron with the smallest parents list, to start the search at.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron GetLinksInSearchStart(System.Collections.Generic.IList<Neuron> list)
        {
            var iCount = int.MaxValue;
            Neuron iRes = null;
            foreach (var i in list)
            {
                if (i.LinksInIdentifier != null && i.LinksInIdentifier.Count < iCount)
                {
                    iCount = i.LinksInIdentifier.Count;
                    iRes = i;
                }
                else if (i.LinksInIdentifier == null || i.LinksInIdentifier.Count == 0)
                {
                    // if 1 has no parents -> no possible results.
                    return null;
                }
            }

            return iRes;
        }

        #region Internal types

        /// <summary>
        ///     a helper class used during the loading of the links, to find other neurons that are being loaded
        ///     and have not yet been registered (bur have already created a link).
        /// </summary>
        /// <remarks>
        ///     This is an internal type so that it can easely access the internal fields of the neuron.
        /// </remarks>
        internal class LinkResolver
        {
            /// <summary>The f default.</summary>
            private static readonly LinkResolver fDefault = new LinkResolver();

            /// <summary>
            ///     the set of neurons, currently being loaded.
            /// </summary>
            /// <remarks>
            ///     this is a dict that maps from ulong to neurons because the links-to-resolve only know the ids while
            ///     the neurons aren't registered yet, so they can't be requested from the brain.
            /// </remarks>
            private readonly System.Collections.Generic.Dictionary<ulong, LinkResolverData> fLoading =
                new System.Collections.Generic.Dictionary<ulong, LinkResolverData>();

            /// <summary>The f resolver lock.</summary>
            private readonly System.Threading.ReaderWriterLockSlim fResolverLock =
                new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

                                                                   // so we can have multiple reads, which should be faster. Also makes certain that we can't remove from 'loading' list before an entire track of the cache is processed.
            #region Default

            /// <summary>
            ///     Gets the entry point for this class.
            /// </summary>
            public static LinkResolver Default
            {
                get
                {
                    return fDefault;
                }
            }

            #endregion

            /// <summary>Adds the specified neuron to the list of items being loaded.</summary>
            /// <remarks>this makes adding a link to a neuron a thread safe process when the neuron is dangling (doesn't have
            ///     another neuron on the other side, or if it is to a neuron that is also being loaded.</remarks>
            /// <param name="iRes">The i Res.</param>
            /// <param name="item">The item.</param>
            internal void Add(LinkResolverData iRes, Neuron item)
            {
                fResolverLock.EnterWriteLock();
                try
                {
                    fLoading[item.ID] = iRes;
                }
                finally
                {
                    fResolverLock.ExitWriteLock();
                }
            }

            /// <summary>Removes the specified neuron from the temporary 'loading' list. Usually called when the neuron has been regisered
            ///     in the
            ///     cache.</summary>
            /// <param name="toRemove">To remove.</param>
            public void Remove(Neuron toRemove)
            {
                fResolverLock.EnterWriteLock();
                try
                {
                    fLoading.Remove(toRemove.ID);
                }
                finally
                {
                    fResolverLock.ExitWriteLock();
                }
            }

            /// <summary>Builds the link using the specified values, checking if there is no duplicate already existing.</summary>
            /// <remarks>Called by a neuron that is being loaded, when it is loading it's links. We check if there
            ///     is already a link created (but it's creator is not yet in the cache). If this is the case,
            ///     we add this link to the requestor (we do this so as to make it thread safe for the link resolve
            ///     process). If there is none found, we add the provided link to the requestor.</remarks>
            /// <param name="toResolve">The to Resolve.</param>
            /// <returns>The <see cref="Link"/>.</returns>
            internal Link BuildLinkOut(Link toResolve)
            {
                Neuron iFound;
                LinkResolverData iData;
                if (toResolve.ToID == toResolve.FromID)
                {
                    // self references have already been handled, so always return the link itself.
                    return toResolve;
                }

                if (TryFindInLoading(toResolve.ToID, out iData))
                {
                    // when building outgoing links, also check if the other side is also in the process of being loaded. If so, make certain that we get the link from the other side.
                    foreach (var i in iData.LinksIn.Items)
                    {
                        if (i.FromID == toResolve.FromID && i.MeaningID == toResolve.MeaningID)
                        {
                            return i;
                        }
                    }
                }
                else if (Brain.Current.TryFindInCash(toResolve.ToID, out iFound) && iFound.LinksInIdentifier != null)
                {
                    LockManager.Current.RequestLock(iFound, LockLevel.LinksIn, false);
                    try
                    {
                        if (iFound.fSortedLinksIn == null)
                        {
                            foreach (var i in iFound.LinksInIdentifier)
                            {
                                if (i.FromID == toResolve.FromID && i.MeaningID == toResolve.MeaningID)
                                {
                                    return i;
                                }
                            }
                        }
                        else
                        {
                            System.Collections.Generic.Dictionary<ulong, Link> iSubList;
                            if (iFound.fSortedLinksIn.TryGetValue(toResolve.MeaningID, out iSubList))
                            {
                                Link iRes;
                                if (iSubList.TryGetValue(toResolve.FromID, out iRes))
                                {
                                    return iRes;
                                }
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(iFound, LockLevel.LinksIn, false);
                    }
                }

                return toResolve;
            }

            /// <summary>The try find in loading.</summary>
            /// <param name="id">The id.</param>
            /// <param name="res">The res.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            private bool TryFindInLoading(ulong id, out LinkResolverData res)
            {
                fResolverLock.EnterReadLock();
                try
                {
                    return fLoading.TryGetValue(id, out res);
                }
                finally
                {
                    fResolverLock.ExitReadLock();
                }
            }

            /// <summary>Builds the link using the specified values, checking if there is no duplicate already existing.</summary>
            /// <remarks>When there is a duplicate link, this is removed, we log a warning and indicate that the neuron is
            ///     changed.  This way, we will automatically fix the neuron.</remarks>
            /// <param name="toResolve">The to Resolve.</param>
            /// <param name="resolveFor">The resolve For.</param>
            /// <returns>The <see cref="Link"/>.</returns>
            internal Link BuildLinkIn(Link toResolve, Neuron resolveFor)
            {
                Neuron iFound;

                // while loading incomming links, don't try to check the objects that are in the process of being loaded, if this is the case, the other side has already used our link, so don't do it again, otherwise we get an invalid state again..
                if (toResolve.FromID == toResolve.ToID)
                {
                    // self refs need to be taken into account: they have already been resolved during the read process, so can handle them really fast: simply return the link.
                    return toResolve;
                }

                if (Brain.Current.TryFindInCash(toResolve.FromID, out iFound))
                {
                    LockManager.Current.RequestLock(iFound, LockLevel.LinksOut, false);
                    try
                    {
                        if (iFound.fSortedLinksOut == null)
                        {
                            foreach (var i in iFound.LinksOutIdentifier)
                            {
                                if (i.ToID == toResolve.ToID && i.MeaningID == toResolve.MeaningID)
                                {
                                    return i;
                                }
                            }
                        }
                        else
                        {
                            System.Collections.Generic.Dictionary<ulong, Link> iSubList;
                            if (iFound.fSortedLinksOut.TryGetValue(toResolve.MeaningID, out iSubList))
                            {
                                Link iRes;
                                if (iSubList.TryGetValue(toResolve.ToID, out iRes))
                                {
                                    return iRes;
                                }
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(iFound, LockLevel.LinksOut, false);
                    }
                }

                return toResolve;
            }

            /// <summary>
            ///     For mass data loading, it can be more beneficial to clear the buffer after all have been loaded.
            /// </summary>
            internal void Clear()
            {
                fResolverLock.EnterWriteLock();
                try
                {
                    fLoading.Clear();
                }
                finally
                {
                    fResolverLock.ExitWriteLock();
                }
            }

            /// <summary>resolves the links of the specified neuron.</summary>
            /// <param name="toAdd"></param>
            /// <param name="cache">The cache.</param>
            internal void Resolve(LinkResolverData toAdd, MultiTrackCache cache)
            {
                if (toAdd.LinksOut.Items.Count > 0)
                {
                    if (toAdd.LinksOut.Index != null)
                    {
                        ResolveLinksOutInverse(toAdd, cache);
                    }
                    else
                    {
                        ResolveLinksOut(toAdd);
                    }
                }

                if (toAdd.LinksIn.Items.Count > 0)
                {
                    if (toAdd.LinksIn.Index != null)
                    {
                        ResolveLinksInInverse(toAdd, cache);
                    }
                    else
                    {
                        ResolveLinksIn(toAdd);
                    }
                }
            }

            /// <summary>does a revers link resolve: walks through all the items int eh cache instead of all the links to resolve.
            ///     This is faster if there are less items int he cache then there are links to resolve.</summary>
            /// <param name="toAdd"></param>
            /// <param name="cache"></param>
            private void ResolveLinksInInverse(LinkResolverData toAdd, MultiTrackCache cache)
            {
                var iData = toAdd.ToResolve;
                var iTrackData = Factories.Default.NLists.GetBuffer();
                try
                {
                    for (var i = 0; i < Settings.NrCacheTracks; i++)
                    {
                        // walk through each track in the cache.
                        iTrackData.Clear();
                        LockManager.Current.RequestCacheLock((ulong)i, true);
                        try
                        {
                            cache.GetTrackData(i, iTrackData);
                        }
                        finally
                        {
                            LockManager.Current.ReleaseCacheLock((ulong)i, true);

                                // we close before processing the list, so as not to create any deadlocks due to 2 different locks inside of each other.
                        }

                        foreach (var iCacheItem in iTrackData)
                        {
                            // walk through each neuron in the track.
                            System.Collections.Generic.Dictionary<ulong, int> iSubIndex;
                            if (toAdd.LinksIn.Index.TryGetValue(iCacheItem.ID, out iSubIndex))
                            {
                                ReplaceLinkIn(toAdd, iCacheItem, iSubIndex);
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iTrackData);
                }

                iData.LinksInIdentifier = toAdd.LinksIn.Items;
            }

            /// <summary>looks up the link(s) defined in the subIndex in the resolved item and uses these links to replace the ones of the
            ///     newly read neuron (which we are resolving)</summary>
            /// <param name="toAdd">To add.</param>
            /// <param name="resolved">The resolved.</param>
            /// <param name="subIndex">Index of the sub.</param>
            private void ReplaceLinkIn(
                LinkResolverData toAdd, 
                Neuron resolved, System.Collections.Generic.Dictionary<ulong, int> subIndex)
            {
                LockManager.Current.RequestLock(resolved, LockLevel.LinksOut, false);

                    // make certain that the other end of the link  can't change the link
                try
                {
                    System.Diagnostics.Debug.Assert(resolved.LinksOutIdentifier != null);

                        // if fLinksOut is null, the db is broken, cause the item-to-resolve is pointing to 'resolved'
                    if (resolved.fSortedLinksOut == null || subIndex.Count > resolved.LinksOutIdentifier.Count)
                    {
                        // fSortedLinksOut can profide a speed increase if subIndex is less then the total nr of links -> less items to check.
                        foreach (var iLink in resolved.LinksOutIdentifier)
                        {
                            int iIndexPos;
                            if (iLink.ToID == toAdd.ToResolve.ID && subIndex.TryGetValue(iLink.MeaningID, out iIndexPos))
                            {
                                toAdd.LinksIn.Items[iIndexPos] = iLink;
                                if (toAdd.ToResolve.fSortedLinksIn != null)
                                {
                                    // if the sortedlinks list was also created, update the reference to the correct link.
                                    toAdd.ToResolve.fSortedLinksIn[iLink.MeaningID][iLink.ToID] = iLink;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var iSubItem in subIndex)
                        {
                            var iItem = resolved.fSortedLinksOut[iSubItem.Key][toAdd.ToResolve.ID];
                            toAdd.LinksIn.Items[iSubItem.Value] = iItem;
                            if (toAdd.ToResolve.fSortedLinksIn != null)
                            {
                                // if the sortedlinks list was also created, update the reference to the correct link.
                                toAdd.ToResolve.fSortedLinksIn[iItem.MeaningID][iItem.ToID] = iItem;
                            }
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(resolved, LockLevel.LinksOut, false);
                }
            }

            /// <summary>The replace link out.</summary>
            /// <param name="toAdd">The to add.</param>
            /// <param name="resolved">The resolved.</param>
            /// <param name="subIndex">The sub index.</param>
            private void ReplaceLinkOut(
                LinkResolverData toAdd, 
                Neuron resolved, System.Collections.Generic.Dictionary<ulong, int> subIndex)
            {
                LockManager.Current.RequestLock(resolved, LockLevel.LinksIn, false);

                    // make certain that the other end of the link  can't change the link
                try
                {
                    System.Diagnostics.Debug.Assert(resolved.LinksInIdentifier != null);

                        // if fLinksOut is null, the db is broken, cause the item-to-resolve is pointing to 'resolved'
                    if (resolved.fSortedLinksIn == null || subIndex.Count > resolved.LinksInIdentifier.Count)
                    {
                        // fSortedLinksOut can profide a speed increase if subIndex is less then the total nr of links -> less items to check.
                        foreach (var iLink in resolved.LinksInIdentifier)
                        {
                            int iIndexPos;
                            if (iLink.FromID == toAdd.ToResolve.ID
                                && subIndex.TryGetValue(iLink.MeaningID, out iIndexPos))
                            {
                                toAdd.LinksOut.Items[iIndexPos] = iLink;
                                if (toAdd.ToResolve.fSortedLinksOut != null)
                                {
                                    // if the sortedlinks list was also created, update the reference to the correct link.
                                    toAdd.ToResolve.fSortedLinksOut[iLink.MeaningID][iLink.FromID] = iLink;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var iSubItem in subIndex)
                        {
                            var iItem = resolved.fSortedLinksIn[iSubItem.Key][toAdd.ToResolve.ID];
                            toAdd.LinksOut.Items[iSubItem.Value] = iItem;
                            if (toAdd.ToResolve.fSortedLinksOut != null)
                            {
                                // if the sortedlinks list was also created, update the reference to the correct link.
                                toAdd.ToResolve.fSortedLinksOut[iItem.MeaningID][iItem.FromID] = iItem;
                            }
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(resolved, LockLevel.LinksIn, false);
                }
            }

            /// <summary>The replace link out.</summary>
            /// <param name="toAdd">The to add.</param>
            /// <param name="resolved">The resolved.</param>
            /// <param name="subIndex">The sub index.</param>
            private void ReplaceLinkOut(
                LinkResolverData toAdd, 
                LinkResolverData resolved, System.Collections.Generic.Dictionary<ulong, int> subIndex)
            {
                if (resolved.LinksIn.Index == null)
                {
                    foreach (var iLink in resolved.LinksIn.Items)
                    {
                        int iIndexPos;
                        if (iLink.FromID == toAdd.ToResolve.ID && subIndex.TryGetValue(iLink.MeaningID, out iIndexPos))
                        {
                            toAdd.LinksOut.Items[iIndexPos] = iLink;
                            if (toAdd.ToResolve.fSortedLinksOut != null)
                            {
                                // if the sortedlinks list was also created, update the reference to the correct link.
                                toAdd.ToResolve.fSortedLinksOut[iLink.MeaningID][iLink.FromID] = iLink;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    System.Collections.Generic.Dictionary<ulong, int> iResolvedSub;
                    if (resolved.LinksIn.Index.TryGetValue(toAdd.ToResolve.ID, out iResolvedSub))
                    {
                        foreach (var iSubItem in subIndex)
                        {
                            var iItem = resolved.LinksIn.Items[iResolvedSub[iSubItem.Key]];
                            toAdd.LinksOut.Items[iSubItem.Value] = iItem;
                            if (toAdd.ToResolve.fSortedLinksOut != null)
                            {
                                // if the sortedlinks list was also created, update the reference to the correct link.
                                toAdd.ToResolve.fSortedLinksOut[iItem.MeaningID][iItem.FromID] = iItem;
                            }
                        }
                    }
                }
            }

            /// <summary>The resolve links in.</summary>
            /// <param name="toAdd">The to add.</param>
            private void ResolveLinksIn(LinkResolverData toAdd)
            {
                var iData = toAdd.ToResolve;
                iData.FLinksIn.Capacity = toAdd.LinksIn.Items.Count;
                if (toAdd.LinksIn.Items.Count >= Settings.MinNrOfLinksForIndex)
                {
                    // if there are enough links, build an index.
                    iData.fSortedLinksIn =
                        new System.Collections.Generic.Dictionary
                            <ulong, System.Collections.Generic.Dictionary<ulong, Link>>();
                }

                foreach (var i in toAdd.LinksIn.Items)
                {
                    var iToAdd = BuildLinkIn(i, iData);
                    iData.FLinksIn.Add(iToAdd);
                    if (iData.fSortedLinksIn != null)
                    {
                        // a regular resolve still needs to build the sorted links index, when doing an inverse resolve, this is not required..
                        AddToIndex(iData.fSortedLinksIn, iToAdd, iToAdd.FromID);
                    }
                }
            }

            /// <summary>The resolve links out.</summary>
            /// <param name="toAdd">The to add.</param>
            private void ResolveLinksOut(LinkResolverData toAdd)
            {
                var iData = toAdd.ToResolve;
                iData.FLinksOut.Capacity = toAdd.LinksOut.Items.Count;

                    // this needs to be done outside of the lock, otherwise we can get deadlocks (lock inside a lock)
                if (toAdd.LinksOut.Items.Count >= Settings.MinNrOfLinksForIndex)
                {
                    // if there are enough links, build an index.
                    iData.fSortedLinksOut =
                        new System.Collections.Generic.Dictionary
                            <ulong, System.Collections.Generic.Dictionary<ulong, Link>>();
                }

                foreach (var i in toAdd.LinksOut.Items)
                {
                    var iToAdd = BuildLinkOut(i);
                    iData.FLinksOut.Add(iToAdd);
                    if (iData.fSortedLinksOut != null)
                    {
                        AddToIndex(iData.fSortedLinksOut, iToAdd, iToAdd.ToID);
                    }
                }
            }

            /// <summary>The resolve links out inverse.</summary>
            /// <param name="toAdd">The to add.</param>
            /// <param name="cache">The cache.</param>
            private void ResolveLinksOutInverse(LinkResolverData toAdd, MultiTrackCache cache)
            {
                var iData = toAdd.ToResolve;

                fResolverLock.EnterReadLock();

                    // first try the items that are currently being resolved. Do this before the cache, so there can't slip any items between the net (after resolve, first comes add to cache, then remove from resolve list).
                try
                {
                    foreach (var iLoading in fLoading)
                    {
                        if (iLoading.Key != iData.ID)
                        {
                            // no need to resolve self references, these have already been handled.
                            System.Collections.Generic.Dictionary<ulong, int> iSubIndex;
                            if (toAdd.LinksOut.Index.TryGetValue(iLoading.Key, out iSubIndex))
                            {
                                ReplaceLinkOut(toAdd, iLoading.Value, iSubIndex);
                            }
                        }
                    }
                }
                finally
                {
                    fResolverLock.ExitReadLock();
                }

                var iTrackData = Factories.Default.NLists.GetBuffer();
                try
                {
                    for (var i = 0; i < Settings.NrCacheTracks; i++)
                    {
                        // walk through each track in the cache.
                        iTrackData.Clear();
                        LockManager.Current.RequestCacheLock((ulong)i, true);
                        try
                        {
                            cache.GetTrackData(i, iTrackData);
                        }
                        finally
                        {
                            LockManager.Current.ReleaseCacheLock((ulong)i, true);

                                // we close before processing the list, so as not to create any deadlocks due to 2 different locks inside of each other.
                        }

                        foreach (var iCacheItem in iTrackData)
                        {
                            // walk through each neuron in the track.
                            System.Collections.Generic.Dictionary<ulong, int> iSubIndex;
                            if (toAdd.LinksOut.Index.TryGetValue(iCacheItem.ID, out iSubIndex))
                            {
                                ReplaceLinkOut(toAdd, iCacheItem, iSubIndex);
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iTrackData);
                }

                iData.LinksOutIdentifier = toAdd.LinksOut.Items;
            }

            /// <summary>Gets the data for for the specified neuron. This is to support the odler 'xml' storage format.</summary>
            /// <param name="toFind">To find.</param>
            /// <returns>The <see cref="LinkResolverData"/>.</returns>
            internal LinkResolverData GetDataFor(Neuron toFind)
            {
                LinkResolverData iData = null;
                fResolverLock.EnterReadLock();
                try
                {
                    if (fLoading.TryGetValue(toFind.ID, out iData) == false)
                    {
                        throw new System.InvalidOperationException("Internal error: the Linkresolver is out of sync.");
                    }
                }
                finally
                {
                    fResolverLock.ExitReadLock();
                }

                return iData;
            }
        }

        #endregion

        #region Fields

        ///// <summary>
        ///// the value that is used to indicate that the next data in the stream is an ulong, the id of a neuron.
        ///// </summary>
        // const sbyte SHORTID = -1;
        ///// <summary>
        ///// the value that is used to indicate that there should be an id to read, but it was 0, so no data to read for the id.
        ///// </summary>
        // const sbyte NOID = 0;
        ///// <summary>
        ///// the value that is used to indicate that the next data in the stream is a ModuleIndex, the id of a neuron.
        ///// </summary>
        // const sbyte LONGID = 1;

        /// <summary>The f info usage count.</summary>
        private ulong fInfoUsageCount;

        /// <summary>The f meaning usage count.</summary>
        private ulong fMeaningUsageCount;

        /// <summary>The f id.</summary>
        private ulong fID;

        /// <summary>The f sorted links in.</summary>
        internal System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> fSortedLinksIn; // for fast,indexed access tot he incomming links (when there are more than specified nr).

        /// <summary>The f sorted links out.</summary>
        internal System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> fSortedLinksOut;

        /// <summary>The f clusterd by.</summary>
        private ClusterList fClusterdBy;

                            // we need this for locking, external callers will also lock this field during looping.

        /// <summary>The f buffered clustered by.</summary>
        private System.Collections.Generic.List<NeuronCluster> fBufferedClusteredBy;

                                                               // for fast access by the processors.

        /// <summary>The f buffered ref count.</summary>
        private int fBufferedRefCount; // so we don't accidentaly recycle the buffered list while it is still being used.

        /// <summary>The f buffered clustered by hash set.</summary>
        private System.Collections.Generic.HashSet<ulong> fBufferedClusteredByHashSet;

        /// <summary>The f buffered hash ref count.</summary>
        private int fBufferedHashRefCount;

        /// <summary>The f frozen for list.</summary>
        private System.Collections.Generic.HashSet<Processor> fFrozenForList;

        /// <summary>The f module ref count.</summary>
        private int fModuleRefCount;

        /// <summary>The f rules list.</summary>
        internal NeuronCluster fRulesList;

                               // a speed optimization: so we don't always need to fetch the 'Actions' cluster, which is usually always the same, so we buffer it.

        /// <summary>
        ///     Identifies the Id that indicates it is not a registered neuron.
        /// </summary>
        public const ulong EmptyId = 0;

        /// <summary>The start id.</summary>
        public const ulong StartId = (ulong)PredefinedNeurons.PopInstruction;

                           // this indicates the start of the neuron id count (which is the first neuron in the PredefinedNeurons List.

        /// <summary>The f access counter.</summary>
        private uint fAccessCounter;

        /// <summary>The f is changed.</summary>
        private volatile bool fIsChanged;

        /// <summary>The f is deleted.</summary>
        private volatile bool fIsDeleted;

        /// <summary>The f is frozen.</summary>
        private volatile bool fIsFrozen;

                              // this is a shortcut, so we don't always have to lock the complete system to check if it is frozen. Instead, we lock the local hasshet.

        /// <summary>
        ///     Identifies the id that indicates it is a temporary neuron which will be registered
        ///     the first time it is used in a link.
        /// </summary>
        /// <remarks>
        ///     This is used for temporary neurons (like <see cref="IntNeurons" /> returned by <see cref="CountInstruction" />).
        /// </remarks>
        public const ulong TempId = ulong.MaxValue;

        #endregion

        #region prop

        /// <summary>
        ///     provides direct access to the links-in list. protected so descendents can also access them + lock them if needed.
        ///     This is a property so we can make certain that the list is only created when required.
        /// </summary>
        protected System.Collections.Generic.List<Link> FLinksIn
        {
            get
            {
                lock (this)
                {
                    // checking + creating needs to be atomic, otherwise duplicate lists.
                    if (LinksInIdentifier == null)
                    {
                        LinksInIdentifier = Factories.Default.LinkLists.GetBuffer();
                    }
                }

                return LinksInIdentifier;
            }
        }

        /// <summary>
        ///     provides direct access to the links-out list. protected so descendents can also access them + lock them if needed.
        ///     This is a property so we can make certain that the list is only created when required.
        /// </summary>
        protected System.Collections.Generic.List<Link> FLinksOut
        {
            get
            {
                lock (this)
                {
                    // checking + creating needs to be atomic, otherwise duplicate lists.
                    if (LinksOutIdentifier == null)
                    {
                        LinksOutIdentifier = Factories.Default.LinkLists.GetBuffer();
                    }
                }

                return LinksOutIdentifier;
            }
        }

        /// <summary>
        ///     so we only create the field when absolutely required
        /// </summary>
        private ClusterList FClusterdBy
        {
            get
            {
                lock (this)
                {
                    // checking + creating needs to be atomic, otherwise duplicate lists.
                    if (fClusterdBy == null)
                    {
                        fClusterdBy = Factories.Default.ClusterLists.GetList(this);
                    }
                }

                return fClusterdBy;
            }
        }

        /// <summary>
        ///     so that the list only gets created when required.
        /// </summary>
        private System.Collections.Generic.HashSet<Processor> FFrozenForList
        {
            get
            {
                lock (this)
                {
                    // checking + creating needs to be atomic, otherwise duplicate lists.
                    if (fFrozenForList == null)
                    {
                        fFrozenForList = Factories.Default.FrozenForLists.GetBuffer();
                    }
                }

                return fFrozenForList;
            }
        }

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.Neuron" />.</value>
        public Neuron TypeOfNeuron
        {
            get
            {
                return Brain.Current[TypeOfNeuronID];
            }
        }

        /// <summary>
        ///     Gets the type of neuron as a neuron ID. This is used by the <see cref="NDB" /> system to figure out the type of
        ///     neuron.
        /// </summary>
        /// <value>The type of neuron ID.</value>
        public virtual ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Neuron;
            }
        }

        #endregion

        #region IsDeleted

        /// <summary>
        ///     Gets the wether this item is in the process of being deleted (the operation is not yet completed).
        /// </summary>
        /// <remarks>
        ///     This is used inernally by the network, to find out if a list still needs updating or not (to keep
        ///     consistency and to prevent deadlocks).
        /// </remarks>
        public bool IsDeleted
        {
            get
            {
                return fID == EmptyId ? true : fIsDeleted;
            }

            internal set
            {
                fIsDeleted = value;
            }
        }

        #endregion

        #region IsChanged

        /// <summary>
        ///     Gets/sets if this neuron was changed since it was last saved, or read.
        /// </summary>
        /// <remarks>
        ///     When this property is true, the neurons are saved when unloaded.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsChanged
        {
            get
            {
                return fIsChanged;
            }

            internal set
            {
                bool iIsChanged;
                lock (this)
                {
                    // make certain that this is thread save.
                    if (fIsChanged != value)
                    {
                        fIsChanged = value;
                        iIsChanged = true;
                    }
                    else
                    {
                        iIsChanged = false;
                    }
                }

                if (iIsChanged)
                {
                    if (ID != EmptyId && ID != TempId)
                    {
                        if (fIsChanged)
                        {
                            Brain.Current.NotifyChanged(this); // the cache is also locked already in this case.
                        }
                        else
                        {
                            Brain.Current.NotifySaved(this);
                        }
                    }
                }

                UnFreeze();

                    // if anything has changed and the neuron is set as frozen, we need to let every processor know that it is no longer frozen.
                if (value)
                {
                    // when saving, don't need to clear the buffers
                    ClearBuffers();
                }
            }
        }

        /// <summary>clears any temp buffers that no longer are valid cause the neuron changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected virtual void ClearBuffers(bool fromGC = false)
        {
            ClearClusteredBy(fromGC);
        }

        /// <summary>only clears the clusteredBy list. So we can clear the ClsuteredBy list when the list is chagned.</summary>
        /// <param name="fromGC">The from GC.</param>
        private void ClearClusteredBy(bool fromGC = false)
        {
            lock (this)
            {
                // do a lock when we clear out the buffers: could be that 2 threads are trying to clear at the same time: double recycled list: oeps.
                var iFac = Factories.Default;
                if (fBufferedClusteredBy != null)
                {
                    if (fBufferedRefCount == 0)
                    {
                        // only recycle if no longer in use.
                        iFac.CLists.Recycle(fBufferedClusteredBy, fromGC);
                    }

                    fBufferedClusteredBy = null; // when changed, always reset this list, this is the fastest way.

                    // do't reset the bufferedByCount, this is done when releasing a previous get, so it will always go back to 0 and will help in the recycling.
                }

                if (fBufferedClusteredByHashSet != null)
                {
                    // iFac.UlongHashSets.Recycle(fBufferedClusteredByHashSet, fromGC);
                    fBufferedClusteredByHashSet = null;
                }
            }
        }

        /// <summary>
        ///     called for a touchmem.
        /// </summary>
        internal void SetIsChangedDirect()
        {
            fIsChanged = true;
        }

        /// <summary>
        ///     sets the isChanged state without locking the cache. This is to make it a more atomic operation: the cache needs to
        ///     be locked together with the rest of the locks.
        /// </summary>
        protected internal void SetChangedUnsave()
        {
            bool iIsChanged;
            lock (this)
            {
                // make certain that this is thread save.
                if (fIsChanged != true)
                {
                    fIsChanged = true;
                    iIsChanged = true;
                }
                else
                {
                    iIsChanged = false;
                }
            }

            if (iIsChanged)
            {
                ClearBuffers();
                if (ID != EmptyId && ID != TempId)
                {
                    Brain.Current.NotifyChangedUnsave(this);
                }
            }

            UnFreeze();

                // if anything has changed and the neuron is set as frozen, we need to let every processor know that it is no longer frozen.
        }

        /// <summary>Sets the IsChanged value without performing an unfreeze.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected internal void SetIsChangedUnsafeNoUnfreeze(bool value)
        {
            bool iIsChanged;
            lock (this)
            {
                // make certain that this is thread save.
                if (fIsChanged != value)
                {
                    fIsChanged = value;
                    iIsChanged = true;
                }
                else
                {
                    iIsChanged = false;
                }
            }

            if (iIsChanged)
            {
                if (value)
                {
                    // when saving, don't need to clear the buffers
                    ClearBuffers();
                }

                if (ID != EmptyId && ID != TempId)
                {
                    if (fIsChanged)
                    {
                        Brain.Current.NotifyChangedUnsave(this); // the cache is also locked already in this case.
                    }
                    else
                    {
                        Brain.Current.NotifySavedUnsafe(this);
                    }
                }
            }
        }

        /// <summary>Sets the IsChanged value without performing an unfreeze.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected internal void SetIsChangedNoUnfreeze(bool value)
        {
            bool iIsChanged;
            lock (this)
            {
                // make certain that this is thread save.
                if (fIsChanged != value)
                {
                    fIsChanged = value;
                    iIsChanged = true;
                }
                else
                {
                    iIsChanged = false;
                }
            }

            if (iIsChanged)
            {
                if (value)
                {
                    // when saving, don't need to clear the buffers
                    ClearBuffers();
                }

                if (ID != EmptyId && ID != TempId)
                {
                    if (fIsChanged)
                    {
                        Brain.Current.NotifyChanged(this); // the cache is also locked already in this case.
                    }
                    else
                    {
                        Brain.Current.NotifySaved(this);
                    }
                }
            }
        }

        /// <summary>Sets the IsChanged value without performing an unfreeze.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected internal void SetIsChangedNoClearBuffers(bool value)
        {
            bool iIsChanged;
            lock (this)
            {
                // make certain that this is thread save.
                if (fIsChanged != value)
                {
                    fIsChanged = value;
                    iIsChanged = true;
                }
                else
                {
                    iIsChanged = false;
                }
            }

            if (iIsChanged)
            {
                if (ID != EmptyId && ID != TempId)
                {
                    if (fIsChanged)
                    {
                        Brain.Current.NotifyChanged(this); // the cache is also locked already in this case.
                    }
                    else
                    {
                        Brain.Current.NotifySaved(this);
                    }
                }
            }

            UnFreeze();

                // if anything has changed and the neuron is set as frozen, we need to let every processor know that it is no longer frozen.
        }

        /// <summary>
        ///     Whenever the neuron is changed, we make certain that this item is no longer frozen. This
        ///     is done by letting each processor that has this item frozen, remove it from it's list and clearing
        ///     ours.
        /// </summary>
        internal void UnFreeze()
        {
            if (fIsFrozen)
            {
                // fIsDeleted == false &&                                                      //don't try to unfreeze if we are in the process of being deleted, no need anymore, and causes recursive lock.
                using (var iList = FrozenForW)
                {
                    fIsFrozen = false;

                        // not completely thread save since the if and assign are not atomic. This is ok, the atomic is done by the using. The field is just a small optimiser, to make it go faster.
                    iList.Clear();
                }

                Brain.Current.OnNeuronUnFrozen(this);
            }
        }

        /// <summary>When a processor is done that had this neuron frozen, it calls this function to remove the processor
        ///     from the list of frozen items.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool UnFreezeFor(Processor processor)
        {
            if (fIsFrozen)
            {
                var iProcs = FrozenForW;
                iProcs.Lock();
                try
                {
                    iProcs.RemoveDirect(processor); // don't need to update the processor, this is done anyway.
                    if (iProcs.IsEmptyUnsafe)
                    {
                        fIsFrozen = false;
                        return true; // when empty, we can be deleted.
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    iProcs.Dispose(); // this will also do the unlock.
                }
            }

            return true;
        }

        /// <summary>Removes the specified processor from the list to which this is neuron is frozen, without updatin the processor
        ///     and without locking anything.
        ///     This is used yb the processor to clear out the list.</summary>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        internal void ChangeFreezeUnsafe(Processor from, Processor to)
        {
            FFrozenForList.Remove(from);
            FFrozenForList.Add(to);
        }

        /// <summary>Adds the processor to the list of already processors for which this is frozen. This is used by the<see cref="PassFrozentooCallerInstruction"/></summary>
        /// <param name="to">To.</param>
        internal void AddFreezeUnsafe(Processor to)
        {
            FFrozenForList.Add(to);
        }

        #endregion

        #region IsFrozen

        /// <summary>
        ///     Gets/sets the wether this neuron is frozen within the context of the current processor (thread local).
        /// </summary>
        /// <remarks>
        ///     When a neuron is frozen, it will be deleted when the processor is removed (at end of split, or of processor
        ///     run when there is no split).
        /// </remarks>
        internal bool IsFrozen
        {
            get
            {
                return fIsFrozen;
            }

            set
            {
                SetIsFrozen(value, Processor.CurrentProcessor);
            }
        }

        /// <summary>Freezes/unfreezes this item for the specified processor. This is used to duplicate globals.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="processor">The processor.</param>
        internal void SetIsFrozen(bool value, Processor processor)
        {
            if (processor != null)
            {
                var iProcs = FrozenForW;
                iProcs.Lock();
                try
                {
                    iProcs.Set = FFrozenForList;

                        // if the list needs to be created, make certain it only gets done 1 time, so inside the lock.
                    if (value)
                    {
                        iProcs.AddUnsafe(processor);
                        fIsFrozen = true;
                    }
                    else
                    {
                        iProcs.RemoveUnsafe(processor);
                        fIsFrozen = false;
                        Brain.Current.OnNeuronUnFrozen(this);
                    }
                }
                finally
                {
                    iProcs.Dispose(); // also unlocks.
                }
            }
            else
            {
                throw new System.InvalidOperationException(
                    "NeuronIsFrozen can only be changed within the context of a processor.");
            }
        }

        #endregion

        // #region FrozenForList
        ///// <summary>
        ///// Gets the set containing all the processors for which this neuron has been frozen. This is used by the
        ///// <see cref="ProcessorSetAccessor"/> to get to the internal information of the neuron that it requires.
        ///// </summary>
        ///// <value>The frozen for list.</value>
        // internal ProcessorSetAccessor FrozenForList
        // {
        // get { return new ProcessorSetAccessor(fFrozenForList, this, false); }
        // } 
        // #endregion
        #region LinksIn

        /// <summary>
        ///     Gets a thread safe accessor for reading the list of links that point to this neuron. (this neuron is in the
        ///     <see cref="Link.To" /> field.
        /// </summary>
        public LinksListAccessor LinksIn
        {
            get
            {
                return Factories.Default.LinksInAccFactory.Get(FLinksIn, this, false);
            }
        }

        /// <summary>
        ///     gives read access to the lock that manages the list of processors for which this neuron si frozen.
        /// </summary>
        internal ProcessorSetAccessor FrozenFor
        {
            get
            {
                return Factories.Default.FrozenForAccFactory.Get(FFrozenForList, this, false);
            }
        }

        /// <summary>
        ///     gives write access to the lock that manages the list of processors for which this neuron si frozen.
        /// </summary>
        internal ProcessorSetAccessor FrozenForW
        {
            get
            {
                return Factories.Default.FrozenForAccFactory.Get(FFrozenForList, this, true);
            }
        }

        /// <summary>
        ///     Gets the real links in list. Only use this when already locked. This creates a list when there isn't one yet.
        /// </summary>
        public System.Collections.Generic.List<Link> LinksInDirect
        {
            get
            {
                return FLinksIn;
            }
        }

        /// <summary>
        ///     Gets a direct ref to the object, without auto creation. Can be used to check if there is a list present, without
        ///     having to lock.
        /// </summary>
        public System.Collections.Generic.List<Link> LinksInIdentifier { get; private set; }

        #endregion

        #region LinksOut

        /// <summary>
        ///     Gets a thread safe accessor for the list of links that originate from this neuron (this neuron is in the
        ///     <see cref="Link.From" /> field.
        /// </summary>
        public LinksListAccessor LinksOut
        {
            get
            {
                return Factories.Default.LinksOutAccFactory.Get(FLinksOut, this, false);
            }
        }

        /// <summary>
        ///     Gets the real links-out list, for duplication.
        /// </summary>
        public System.Collections.Generic.List<Link> LinksOutDirect
        {
            get
            {
                return FLinksOut;
            }
        }

        /// <summary>
        ///     Gets the real links-out list, for duplication.
        /// </summary>
        public System.Collections.Generic.List<Link> LinksOutIdentifier { get; private set; }

        #endregion

        #endregion

        #region ID

        /// <summary>
        ///     Gets the id of the object
        /// </summary>
        public ulong ID
        {
            get
            {
                return fID;
            }

            internal set
            {
                var iPrev = fID;
                LockManager.Current.RequestLock(this, LockLevel.Value, true);
                try
                {
                    SetId(value);
                }
                finally
                {
                    LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                }

                if (iPrev != EmptyId || value != TempId)
                {
                    // when we set the id from 0 to temp, the network hasn't yet changed. only when it is fully added.
                    IsChanged = true;
                }
            }
        }

        /// <summary>Sets the id without notifying of the change to the brain. This is used by the brain  itself when the neuron is
        ///     added.</summary>
        /// <param name="p">The p.</param>
        protected internal virtual void SetId(ulong p)
        {
            fID = p;
        }

        #endregion

        #region InfoUsageCount

        /// <summary>
        ///     Gets/sets the number of times that this neuron is used in the <see cref="Link.Info" /> list.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Brain.Delete" /> function to determin if this neuron can be
        ///     deleted or not (when this neuron is used as info, it can not yet be deleted).
        /// </remarks>
        public ulong InfoUsageCount
        {
            get
            {
                return fInfoUsageCount;
            }

            set
            {
                if (InfoUsageCount != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fInfoUsageCount = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    IsChanged = true;
                }
            }
        }

        /// <summary>
        ///     thread save way to increment the infoUsageCount value.
        /// </summary>
        internal void IncInfoUsageCount()
        {
            LockManager.Current.RequestLock(this, LockLevel.Value, true);

                // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
            try
            {
                fInfoUsageCount++;
            }
            finally
            {
                LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
            }

            IsChanged = true;
        }

        /// <summary>
        ///     thread save way to decrement the infoUsageCount value.
        /// </summary>
        internal void DecInfoUsageCount()
        {
            LockManager.Current.RequestLock(this, LockLevel.Value, true);

                // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
            try
            {
                if (fInfoUsageCount > 0)
                {
                    // threading problems, due to delete
                    fInfoUsageCount--;
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
            }

            IsChanged = true;
        }

        /// <summary>
        ///     Increments the meaning counter by 1 in an unsafe manner. This is a shortcut, used by the link constructor.
        /// </summary>
        internal void IncInfoUnsafe()
        {
            fInfoUsageCount++;
        }

        /// <summary>
        ///     decrmenents the meaning counter by 1 in an unsafe manner. This is a shortcut, used by the link destruction.
        /// </summary>
        internal void DecInfoUnsafe()
        {
            if (fInfoUsageCount > 0)
            {
                // threading problems, due to delete
                fInfoUsageCount--;
            }
        }

        #endregion

        #region MeaningUsageCount

        /// <summary>
        ///     Gets the number of times that this neuron is used as the value for <see cref="Link.Meaning" />.
        /// </summary>
        /// <remarks>
        ///     This is used by the <see cref="Brain.Delete" /> function to determin if this neuron can be
        ///     deleted or not (when this neuron is used as Meaning, it can not yet be deleted).
        /// </remarks>
        public ulong MeaningUsageCount
        {
            get
            {
                return fMeaningUsageCount;
            }

            set
            {
                if (fMeaningUsageCount != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fMeaningUsageCount = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    IsChanged = true;
                }
            }
        }

        /// <summary>
        ///     Increments the meaning counter by 1 in an unsafe manner. This is a shortcut, used by the link constructor.
        /// </summary>
        internal void IncMeaningUnsafe()
        {
            fMeaningUsageCount++;

            // SetChangedUnsave();
        }

        /// <summary>
        ///     decrmenents the meaning counter by 1 in an unsafe manner. This is a shortcut, used by the link destruction.
        /// </summary>
        internal void DecMeaningUnsafe()
        {
            fMeaningUsageCount--;

            // SetChangedUnsave();
        }

        #endregion

        #region ClusteredBy

        /// <summary>
        ///     Gets the thread manager that provides access to the list of NeuronCluster id's that this Neuron is a child of.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ClustersAccessor ClusteredBy
        {
            get
            {
                return Factories.Default.ClustersAccFactory.Get(FClusterdBy, this, false);
            }
        }

        /// <summary>released the list of buffered clusters</summary>
        /// <param name="list"></param>
        internal void ReleaseBufferedCluseteredBy(System.Collections.Generic.List<NeuronCluster> list)
        {
            if (list != null)
            {
                lock (this)
                {
                    fBufferedRefCount--;
                    if (fBufferedRefCount == 0 && fBufferedClusteredBy != list)
                    {
                        // we need to check for refCount == 0, if this is 1 (or more), the 'list' might still be reffed.
                        Factories.Default.CLists.Recycle(list);
                    }
                }
            }
        }

        /// <summary>The try get buffered clustered by.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<NeuronCluster> TryGetBufferedClusteredBy()
        {
            lock (this)
            {
                if (fBufferedClusteredBy != null)
                {
                    fBufferedRefCount++;
                }

                return fBufferedClusteredBy;
            }
        }

        /// <summary>Gets the children, when possible from a buffered list, otherwise, the new list is buffered. This is a speed
        ///     optimizer
        ///     for clusters invlolved with the processor: so that we don't always need to rebuild lists that never change.
        ///     Whenever the cluster is changed, the lsit is cleaned.
        ///     Warning: should only be callled within the context of a processor.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<NeuronCluster> GetBufferedClusteredBy()
        {
            lock (this)
            {
                if (fBufferedClusteredBy != null)
                {
                    fBufferedRefCount++;
                    return fBufferedClusteredBy;
                }
            }

            var iMemFac = Factories.Default;

                // keep a local copy, so we don't waste time each time getting an expensive threadlocal value.
            var iResList = iMemFac.CLists.GetBuffer();
            if (fClusterdBy != null)
            {
                System.Collections.Generic.List<ulong> iIds;
                LockManager.Current.RequestLock(this, LockLevel.Parents, false);
                try
                {
                    iIds = iMemFac.IDLists.GetBuffer(fClusterdBy.Count);
                    iIds.AddRange(fClusterdBy);
                }
                finally
                {
                    LockManager.Current.ReleaseLock(this, LockLevel.Parents, false);
                }

                if (iResList.Capacity < iIds.Count)
                {
                    iResList.Capacity = iIds.Count;
                }

                foreach (var i in iIds)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        var iCluster = iFound as NeuronCluster;

                            // this is still some bogus code. It could be that the cluster got deleted and the id re-used for another neuron during the load period.
                        if (iCluster != null)
                        {
                            iResList.Add(iCluster);
                        }
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }

            lock (this)
            {
                // make certain that this section can only be executed by 1 thread at a time.
                fBufferedRefCount++;
                if (fBufferedClusteredBy == null)
                {
                    // don't need to assign it 2 times, first result is ok.
                    fBufferedClusteredBy = iResList;
                }
                else
                {
                    iMemFac.CLists.Recycle(iResList, false);

                        // another thread beat us in construction and storing of the list, so recollect the list itself for later usage (releases memory pressure).
                    iResList = fBufferedClusteredBy;
                }
            }

            return iResList;
        }

        /// <summary>gets a buffered hashset of the parents. This is used by the various 'GetCommonParents' functions and can speed
        ///     things
        ///     up considerably.
        ///     Can be done unsave cause all the 'getCommonParents' are also unsave.</summary>
        /// <returns>The <see cref="HashSet"/>.</returns>
        internal System.Collections.Generic.HashSet<ulong> GetBufferedClusteredByHashUnsave()
        {
            lock (this)
            {
                if (fBufferedClusteredByHashSet != null)
                {
                    fBufferedHashRefCount++;
                    return fBufferedClusteredByHashSet;
                }
            }

            var iResList = new System.Collections.Generic.HashSet<ulong>();
            if (fClusterdBy != null)
            {
                foreach (var i in fClusterdBy)
                {
                    if (iResList.Contains(i) == false)
                    {
                        // the same parent can be present 2 times, can't have that in a hashset, so check for this
                        iResList.Add(i);
                    }
                }
            }

            lock (this)
            {
                // make certain that this section can only be executed by 1 thread at a time.
                if (fBufferedClusteredByHashSet == null)
                {
                    // don't need to assign it 2 times, first result is ok.
                    fBufferedClusteredByHashSet = iResList;
                }
                else
                {
                    iResList = fBufferedClusteredByHashSet;
                }

                fBufferedHashRefCount++;
            }

            return iResList;
        }

        /// <summary>The release clustered by hash unsave.</summary>
        /// <param name="list">The list.</param>
        internal void ReleaseClusteredByHashUnsave(System.Collections.Generic.HashSet<ulong> list)
        {
            if (list != null)
            {
                lock (this)
                    if (fBufferedHashRefCount > 0)
                    {
                        fBufferedHashRefCount--;
                    }
            }
        }

        #endregion

        #region ClusteredByIdentifier

        /// <summary>
        ///     Gets an object that uniquely identifies the list that was changed.  This can be used in dicationaries for instance,
        ///     like the designer's eventManager.
        /// </summary>
        /// <value>The identifier.</value>
        public object ClusteredByIdentifier
        {
            get
            {
                return fClusterdBy;
            }
        }

        /// <summary>
        ///     makes certain that the clusteredBy list is built. This is not thread safe.
        /// </summary>
        public void ValidateClusteredByList()
        {
            lock (this)
            {
                // need to make certain that this is thread save.
                if (fClusterdBy == null)
                {
                    fClusterdBy = Factories.Default.ClusterLists.GetList(this);
                }
            }
        }

        #endregion

        #region data management

        /// <summary>Thread save remove link. Not thread safe.</summary>
        /// <param name="link">The link.</param>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        internal void RemoveInboundLink(Link link)
        {
            // SetChangedUnsave();
            if (FLinksIn.Remove(link) == false && Settings.ErrorOnInvalidLinkRemove)
            {
                throw new BrainException("Link not stored as inbound link");
            }

            RemoveFromIndex(fSortedLinksIn, link, link.FromID);
        }

        /// <summary>Adds the link to the index.</summary>
        /// <param name="addTo">The add to.</param>
        /// <param name="toAdd">To add.</param>
        /// <param name="secondIndex">Index of the second item (either fromid or toId.</param>
        /// <returns>True if there alrady was a value stored at the specified index, otherwise false.</returns>
        private static bool AddToIndex(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> addTo, 
            Link toAdd, 
            ulong secondIndex)
        {
            if (addTo != null)
            {
                System.Collections.Generic.Dictionary<ulong, Link> iSub;
                if (addTo.TryGetValue(toAdd.MeaningID, out iSub) == false)
                {
                    iSub = new System.Collections.Generic.Dictionary<ulong, Link>();
                    addTo.Add(toAdd.MeaningID, iSub);
                }
                else if (iSub.ContainsKey(secondIndex))
                {
                    return true;

                        // if there already was an item, don't add it but simply let the caller now somethimg was wrong.
                }

                iSub.Add(secondIndex, toAdd);
            }

            return false;
        }

        /// <summary>Removes the link from the index.</summary>
        /// <param name="dict">The dict.</param>
        /// <param name="toRemove">To remove.</param>
        /// <param name="secondIndex">The second Index.</param>
        private static void RemoveFromIndex(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> dict, 
            Link toRemove, 
            ulong secondIndex)
        {
            if (dict != null)
            {
                System.Collections.Generic.Dictionary<ulong, Link> iSub;
                if (dict.TryGetValue(toRemove.MeaningID, out iSub))
                {
                    iSub.Remove(secondIndex);
                    if (iSub.Count == 0)
                    {
                        // if there are no more items, delete the entire dict.
                        dict.Remove(toRemove.MeaningID);
                    }
                }
            }
        }

        /// <summary>Adds the link to the inbound list. Not thread safe.</summary>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        /// <param name="link"></param>
        internal void AddInboundLink(Link link)
        {
            if (fSortedLinksIn == null)
            {
                // if there is an index, we use this to check if there already was a value.
                if (FLinksIn.Contains(link))
                {
                    throw new BrainException("Link already stored as inbound link");
                }
            }

            FLinksIn.Add(link);
            if (AddToIndex(fSortedLinksIn, link, link.FromID))
            {
                throw new BrainException("Link already stored as inbound link");
            }
        }

        /// <summary>Adds the link to the inbound list. Not thread safe.</summary>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        /// <param name="link"></param>
        /// <param name="index">The index.</param>
        internal void InsertInboundLink(Link link, int index)
        {
            if (fSortedLinksIn == null)
            {
                // if there is an index, we use this to check if there already was a value.
                if (FLinksIn.Contains(link))
                {
                    throw new BrainException("Link already stored as inbound link");
                }
            }

            FLinksIn.Insert(index, link);
            if (AddToIndex(fSortedLinksIn, link, link.FromID))
            {
                throw new BrainException("Link already stored as inbound link");
            }
        }

        /// <summary>Removes the link. Not thread safe.</summary>
        /// <param name="link">The link.</param>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        internal void RemoveOutgoingLink(Link link)
        {
            // SetChangedUnsave();
            fRulesList = null; // all cached code needs to be cleared when links out has changed.
            if (FLinksOut.Remove(link) == false && Settings.ErrorOnInvalidLinkRemove)
            {
                throw new BrainException("Link not stored as outgoing link");
            }

            RemoveFromIndex(fSortedLinksOut, link, link.ToID);
        }

        /// <summary>Adds outgoing link. Not thread safe.</summary>
        /// <param name="link">The link.</param>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        internal void AddOutgoingLink(Link link)
        {
            if (fSortedLinksOut == null)
            {
                // SetChangedUnsave();
                if (FLinksOut.Contains(link))
                {
                    throw new BrainException("Link already stored as outgoing link");
                }
            }

            LinksOutIdentifier.Add(link); // can use flinks -> FLink made certain that the list is created.
            if (AddToIndex(fSortedLinksOut, link, link.ToID))
            {
                throw new BrainException("Link already stored as outgoing link");
            }
        }

        /// <summary>Inserts outgoing link. Not thread safe.</summary>
        /// <param name="link">The link.</param>
        /// <param name="index">The index.</param>
        /// <remarks>Internal cause this operation is done through the link object.</remarks>
        internal void InsertOutgoingLink(Link link, int index)
        {
            if (fSortedLinksOut == null)
            {
                if (FLinksOut.Contains(link))
                {
                    throw new BrainException("Link already stored as outgoing link");
                }
            }

            LinksOutIdentifier.Insert(index, link); // can use flink -> FLink made certain that the list is created.
            if (AddToIndex(fSortedLinksOut, link, link.ToID))
            {
                throw new BrainException("Link already stored as outgoing link");
            }
        }

        /// <summary>Deprecated, use <see cref="Link.SetFirstOutTo"/> instead.
        ///     Searches the first link with the specified meaning in the<see cref="Neuron.LinksOut"/> list and changes it's <see cref="Link.To"/>
        ///     reference to the new value. If this value is null, the link will be removed.</summary>
        /// <remarks><para>this is thread safe.</para>
        /// Used by property setters.</remarks>
        /// <param name="meaning">The meaning to search for.</param>
        /// <param name="value">The value to assign to the 'To' property.</param>
        public void SetFirstOutgoingLinkTo(ulong meaning, Neuron value)
        {
            Link.SetFirstOutTo(this, value, meaning);
        }

        /// <summary>Searches the first link with the specified meaning in the<see cref="Neuron.LinksOut"/> list and changes it's <see cref="Link.To"/>
        ///     reference to the new value. If this value is null, the link will be removed.</summary>
        /// <remarks><para>this is thread safe.</para>
        /// Used by property setters.</remarks>
        /// <param name="meaning">The meaning to search for.</param>
        /// <param name="value">The value to assign to the 'To' property.</param>
        public void SetFirstIncommingLinkTo(ulong meaning, Neuron value)
        {
            Link.SetFirstInTo(this, value, meaning);
        }

        #endregion

        #region functions

        /// <summary>
        ///     Called after the data for this neuron was cleared. Some extra, type specific clearing can be done at this time.
        /// </summary>
        /// <remarks>
        ///     This function is automically called when a neuron is deleted.
        ///     <para>
        ///         Descendents can enhance this function and clean more data. No need to make it thread save, the delete function
        ///         locks the entire neuron during a clear.
        ///     </para>
        /// </remarks>
        protected internal virtual void Clear()
        {
            fIsChanged = false;

                // also need to make certain that we don't generate unwanted errors: when the neuron is unloaded and not yet saved, we write an error, but in this case it is not required.
            fRulesList = null; // all cached code needs to be cleared when links out has changed. 
            RecycleData(false);
            ClearBuffers();
        }

        /// <summary>Recycles all the data.</summary>
        /// <param name="fromGC">if set to <c>true</c> , it is called from the GC thread. This allows us to take special care that
        ///     in the GC thread, data gets collected differently.</param>
        protected virtual void RecycleData(bool fromGC)
        {
            var iMemFac = Factories.Default;
            if (fClusterdBy != null)
            {
                iMemFac.ClusterLists.Recycle(fClusterdBy, fromGC);

                    // no need to recycle the IDlist itself, this can be buffered, which saves a little time for reusing it.
                fClusterdBy = null;
            }

            if (fFrozenForList != null)
            {
                iMemFac.FrozenForLists.Recycle(fFrozenForList, fromGC);
                fFrozenForList = null;
            }

            if (LinksInIdentifier != null)
            {
                iMemFac.LinkLists.Recycle(LinksInIdentifier, fromGC);
                LinksInIdentifier = null;
                fSortedLinksIn = null;
            }

            if (LinksOutIdentifier != null)
            {
                iMemFac.LinkLists.Recycle(LinksOutIdentifier, fromGC);
                LinksOutIdentifier = null;
                fSortedLinksOut = null;
            }
        }

        /// <summary>A thread  unsafe way to remove a cluster  from the 'ClusteredBy' list.</summary>
        /// <param name="toRemove">To remove.</param>
        internal void RemoveClusterUnsafe(NeuronCluster toRemove)
        {
            FClusterdBy.RemoveDirect(toRemove);
            lock (this)
            {
                // do a lock when we clear out the buffers: could be that 2 threads are trying to clear at the same time: double recycled list: oeps.
                var iFac = Factories.Default;
                if (fBufferedClusteredBy != null)
                {
                    if (fBufferedRefCount == 0)
                    {
                        // if no one is using the list, don't need to reset, but can simply add. If the list is being used, don't know which thead, so play save and reset.
                        fBufferedClusteredBy.Remove(toRemove);
                    }
                    else
                    {
                        // iFac.CLists.Recycle((List<NeuronCluster>)fBufferedClusteredBy, false);               //don't recycle, it's still being used.
                        fBufferedClusteredBy = null;
                    }
                }

                if (fBufferedClusteredByHashSet != null)
                {
                    if (fBufferedHashRefCount == 0)
                    {
                        // if no one is using the list, don't need to reset, but can simply add. If the list is being used, don't know which thead, so play save and reset.
                        fBufferedClusteredByHashSet.Remove(toRemove.ID);
                    }
                    else
                    {
                        fBufferedClusteredByHashSet = null;
                    }
                }
            }
        }

        /// <summary>Adds the cluster to the clusteredByList in an unsafe manner (without locking anything).
        ///     This is used during a duplication operation of a cluster for fast duplication.</summary>
        /// <param name="cluster">The cluster.</param>
        internal void AddClusterUnsafe(NeuronCluster cluster)
        {
            FClusterdBy.InsertDirect(fClusterdBy.Count, cluster);

                // we do a direct remove, don't need to remove the neuron from our list as well, only in 1 direction.
            lock (this)
            {
                // do a lock when we clear out the buffers: could be that 2 threads are trying to clear at the same time: double recycled list: oeps.
                var iFac = Factories.Default;
                if (fBufferedClusteredBy != null)
                {
                    if (fBufferedRefCount == 0)
                    {
                        // if no one is using the list, don't need to reset, but can simply add. If the list is being used, don't know which thead, so play save and reset.
                        fBufferedClusteredBy.Add(cluster);
                    }
                    else
                    {
                        // iFac.CLists.Recycle((List<NeuronCluster>)fBufferedClusteredBy, false);                  //don't recycle, it's still being used.
                        fBufferedClusteredBy = null;
                    }
                }

                if (fBufferedClusteredByHashSet != null)
                {
                    if (fBufferedHashRefCount == 0)
                    {
                        // if no one is using the list, don't need to reset, but can simply add. If the list is being used, don't know which thead, so play save and reset.
                        fBufferedClusteredByHashSet.Add(cluster.ID);
                    }
                    else
                    {
                        fBufferedClusteredByHashSet = null;
                    }
                }
            }
        }

        // private void ClusteredByAdded(NeuronCluster cluster)
        // {
        // lock (this)                                                                                           //do a lock so we can savely evaluate the buffers.
        // {
        // if (fBufferedClusteredBy != null && fBufferedRefCount == 0)                                        //if no-one is using it, we can savely add the item
        // fBufferedClusteredBy.Add(cluster);
        // if (fBufferedClusteredByHashSet != null  && fBufferedClusteredByHashSet.Contains(cluster.ID) == false)
        // fBufferedClusteredByHashSet.Add(cluster.ID);
        // }
        // }

        /// <summary>Changes the type of this neuron to the new specified type.  This action creates and destroys the object.</summary>
        /// <remarks>Values are copied over when possible.  Links are always copied over.
        ///     The new object is registed with the brain.</remarks>
        /// <param name="type">The requested type.</param>
        /// <returns>The new object that represents the neuron of the new type.</returns>
        public virtual Neuron ChangeTypeTo(System.Type type)
        {
            var iNew = NeuronFactory.Get(type);
            if (iNew != null)
            {
                // lock (Brain.Current)                                                             //we make certain nothing can get deleted during the move.
                iNew.MoveRefs(this);
                Brain.Current.Replace(this, iNew);
                return iNew;
            }

            throw new System.InvalidOperationException();
        }

        /// <summary>
        ///     Creates an exact duplicate of this Neuron so the <see cref="Processor" /> can perform a split.
        /// </summary>
        /// <remarks>
        ///     A new id is created for the neuron cause all neurons should have unique numbers.
        /// </remarks>
        /// <returns>An exact duplicate of the argument, but with a new id.</returns>
        public virtual Neuron Duplicate()
        {
            var iRes = NeuronFactory.Get(GetType());
            Brain.Current.Add(iRes);

            var iDuplicator = new Duplicator();
            iDuplicator.Duplicate(this, iRes);
            return iRes;
        }

        /// <summary>Copies the link objects found in the source into this list without recreating the links.   It also
        ///     moves the list of 'ClustedBy' references. This is used to change the type of a neuron to another one.</summary>
        /// <param name="source">The source.</param>
        private void MoveRefs(Neuron source)
        {
            if (source.LinksInIdentifier != null)
            {
                ListAccessor<Link> iLinksIn = source.LinksIn;

                    // need to make certain that everything is locked while copying.
                iLinksIn.Lock();
                FLinksIn.AddRange(iLinksIn.List);
                iLinksIn.Unlock();
            }

            if (source.LinksOutIdentifier != null)
            {
                ListAccessor<Link> iLinksOut = source.LinksOut;
                iLinksOut.Lock();
                FLinksOut.AddRange(iLinksOut.List);
                iLinksOut.Unlock();
            }

            if (source.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClustered;
                using (var iList = source.ClusteredBy) iClustered = iList.ConvertTo<NeuronCluster>();
                try
                {
                    if (iClustered.Count > 0)
                    {
                        foreach (var i in iClustered)
                        {
                            FClusterdBy.InsertDirect(FClusterdBy.Count, i);

                                // need to do a direct insert, if we don't do this, the items get put in the child list also, which we don't want.
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClustered);
                }
            }
        }

        /// <summary>Copies all the data from this neuron to the argument.</summary>
        /// <remarks><para>Inheriters should reimplement this function and copy any extra information required for their specific type
        ///         of neuron. Important: you can't perform any write locks during this operation cause that would make the
        ///         operation not a singleton anymore (deadlocks could occur). If you need to lock stuff, use the duplicator class.</para>
        /// </remarks>
        /// <param name="copyTo">The object to copy their data to.</param>
        protected internal virtual void CopyTo(Neuron copyTo)
        {
            // don't do anything at this level, this is done by the duplicator class.
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            if (fID < (ulong)PredefinedNeurons.EndOfStatic)
            {
                return ((PredefinedNeurons)fID).ToString();
            }

            return fID.ToString();
        }

        /// <summary>Compares this neuron with anohter neuron using the specified operator.</summary>
        /// <remarks><para>The right part doesn't have to be solved in advance.  So it may be expressions which
        ///         will be solved at the appropriate time, when needed.  This means that for some operators,
        ///         the right part doesn't have to be solved (logical and/or).</para>
        /// <para>In this base implementation, the compare is performed on the id's, descendents
        ///         can change this if they want (to compare numbers or string for instance).</para>
        /// </remarks>
        /// <param name="right">The neuron to compare it with.</param>
        /// <param name="op">The operator to use.</param>
        /// <returns>True if both id's are the same.</returns>
        protected internal virtual bool CompareWith(Neuron right, Neuron op)
        {
            switch (op.ID)
            {
                case (ulong)PredefinedNeurons.Equal:
                    return ID == right.ID;
                case (ulong)PredefinedNeurons.Smaller:
                    return ID < right.ID;
                case (ulong)PredefinedNeurons.SmallerOrEqual:
                    return ID <= right.ID;
                case (ulong)PredefinedNeurons.Bigger:
                    return ID > right.ID;
                case (ulong)PredefinedNeurons.BiggerOrEqual:
                    return ID >= right.ID;
                case (ulong)PredefinedNeurons.Different:
                    return ID != right.ID;
                case (ulong)PredefinedNeurons.And:
                    if (ID == (ulong)PredefinedNeurons.True)
                    {
                        return right.ID == (ulong)PredefinedNeurons.True;
                    }

                    if (ID == (ulong)PredefinedNeurons.False)
                    {
                        return right.ID == (ulong)PredefinedNeurons.False;
                    }

                    return false;
                case (ulong)PredefinedNeurons.Or:
                    if (ID == (ulong)PredefinedNeurons.True)
                    {
                        return right.ID == (ulong)PredefinedNeurons.True || right.ID == (ulong)PredefinedNeurons.False;
                    }

                    if (ID == (ulong)PredefinedNeurons.False)
                    {
                        return right.ID == (ulong)PredefinedNeurons.True;
                    }

                    return false;
                default:
                    LogService.Log.LogError("neuron.CompareWith", string.Format("Invalid operator found: {0}.", op));
                    return false;
            }
        }

        /// <summary>Checks if the specified item is an expression, if so, it solves it, otherwise, it creates a result list
        ///     with the specified item. The list that is returned is freely consumable.</summary>
        /// <remarks>This is static cause toProcess can be null.</remarks>
        /// <param name="toProcess">the item to check, null alowed.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>null if invalid, otherwise a List containing the result of the expression or the argument</returns>
        public static System.Collections.Generic.List<Neuron> SolveResultExp(Neuron toProcess, Processor processor)
        {
            if (toProcess != null)
            {
                var iRes = processor.Mem.ArgumentStack.Push();
                if (iRes.Capacity < 10)
                {
                    iRes.Capacity = 10;

                        // reserve a little space for the results, so that we don't need to resize the list continuasly.
                }

                try
                {
                    var iExp = toProcess as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(processor);
                    }
                    else
                    {
                        iRes.Add(toProcess);
                    }
                }
                finally
                {
                    processor.Mem.ArgumentStack.Release();
                }

                return iRes;

                    // we can return the list from the stack, cause we asked to release it, so it has be be recycled manually later on.
            }

            return null;
        }

        /// <summary>Checks if the specified item is an expression, if so, it solves it, otherwise, it creates a result list
        ///     with the specified item.
        ///     Warning, the result list is part of the stack and should be released from the stack after it has been used.
        ///     this is a speed optimization, so we can directly use the lists on the stack without having to create temp lists.</summary>
        /// <remarks>This is static cause toProcess can be null.</remarks>
        /// <param name="toProcess">the item to check, null alowed.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>null if invalid, otherwise a List containing the result of the expression or the argument</returns>
        public static System.Collections.Generic.List<Neuron> SolveResultExpNoStackChange(
            Neuron toProcess, 
            Processor processor)
        {
            if (toProcess != null)
            {
                var iRes = processor.Mem.ArgumentStack.Push();
                if (iRes.Capacity < 10)
                {
                    iRes.Capacity = 10;

                        // reserve a little space for the results, so that we don't need to resize the list continuasly.
                }

                var iExp = toProcess as ResultExpression;
                if (iExp != null)
                {
                    iExp.GetValue(processor);
                }
                else
                {
                    iRes.Add(toProcess);
                }

                return iRes;

                    // we make a copy of the result list, cause it gets reused by the stack, if we don't do this, we get problems with 'lists changed' during enumeration.
            }

            return null;
        }

        /// <summary>The solve result exp no stack change.</summary>
        /// <param name="toProcess">The to process.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<Neuron> SolveResultExpNoStackChange(
            ResultExpression toProcess, 
            Processor processor)
        {
            if (toProcess != null)
            {
                var iRes = processor.Mem.ArgumentStack.Push();
                if (iRes.Capacity < 10)
                {
                    iRes.Capacity = 10;

                        // reserve a little space for the results, so that we don't need to resize the list continuasly.
                }

                toProcess.GetValue(processor);
                return iRes;

                    // we make a copy of the result list, cause it gets reused by the stack, if we don't do this, we get problems with 'lists changed' during enumeration.
            }

            return null;
        }

        /// <summary>Checks if the specified item is an expression, if so, it solves it and checks it only has 1 result. The first
        ///     result is always
        ///     return. If it is not an expression, the item itself is returned.</summary>
        /// <remarks>This is static cause toProcess can be null.</remarks>
        /// <param name="toProcess">the item to check, null alowed.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>null if invalid, otherwise an enumerator containing the result of the expression or the argument</returns>
        internal static Neuron SolveSingleResultExp(Neuron toProcess, Processor processor)
        {
            if (toProcess != null)
            {
                var iExp = toProcess as ResultExpression;
                if (iExp == null)
                {
                    return toProcess;
                }

                if (iExp is Variable)
                {
                    var iTemp = ((Variable)iExp).ExtractValue(processor);
                    if (iTemp != null && iTemp.Count > 0)
                    {
                        return iTemp[0];
                    }

                    return null;
                }

                var iRes = processor.Mem.ArgumentStack.Push();
                if (iRes.Capacity < 10)
                {
                    iRes.Capacity = 10; // give it a little room to work
                }

                try
                {
                    iExp.GetValue(processor);
                    if (iRes.Count == 1)
                    {
                        return iRes[0];
                    }
                    else if (iRes.Count > 1)
                    {
                        LogService.Log.LogError(
                            "Neuron.SolveSinlgeResult", 
                            string.Format("Expression '{0}' has multiple results, only 1 allowed.", iExp));
                        return iRes[0];
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Neuron.SolveSinlgeResult", 
                            string.Format("Expression '{0}' has no results.", iExp));
                        return null;
                    }
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }
            }

            return null;
        }

        /// <summary>Checks if the specified id points to an empty or temp neuron or not.</summary>
        /// <param name="id">ulong to check.</param>
        /// <returns>true if id is <see cref="Neuron.EmptyId"/> or <see cref="Neuron.TempId"/>.</returns>
        public static bool IsEmpty(ulong id)
        {
            return id == EmptyId || id == TempId;
        }

        #endregion

        #region searching

        /// <summary>Searches and returns all the neurons linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The list of neurons that were found.</returns>
        public System.Collections.Generic.List<Neuron> FindAllOut(ulong meaning)
        {
            System.Collections.Generic.List<Neuron> iRes;
            var iMemFac = Factories.Default;

                // keep a local copy, so we don't waste time each time getting an expensive threadlocal value.
            var iIds = iMemFac.IDLists.GetBuffer();
            iRes = iMemFac.NLists.GetBuffer();
            if (LinksOutIdentifier != null)
            {
                using (var iLinks = LinksOut)
                {
                    if (fSortedLinksOut == null)
                    {
                        foreach (var i in iLinks)
                        {
                            if (i.MeaningID == meaning)
                            {
                                iIds.Add(i.ToID);
                            }
                        }
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iFound;
                        if (fSortedLinksOut.TryGetValue(meaning, out iFound))
                        {
                            foreach (var i in iFound)
                            {
                                iIds.Add(i.Value.ToID);
                            }
                        }
                    }
                }

                if (iRes.Capacity < iIds.Count)
                {
                    iRes.Capacity = iIds.Count;
                }

                foreach (var i in iIds)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        // could have been deleted?
                        iRes.Add(iFound);
                    }
                }
            }

            iMemFac.IDLists.Recycle(iIds);
            return iRes;
        }

        /// <summary>Searches and returns all the neurons linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The list of neurons that were found.</returns>
        public System.Collections.Generic.List<Neuron> FindAllIn(ulong meaning)
        {
            System.Collections.Generic.List<Neuron> iRes;
            var iMemFac = Factories.Default;

                // keep a local copy, so we don't waste time each time getting an expensive threadlocal value.
            var iIds = iMemFac.IDLists.GetBuffer();
            iRes = iMemFac.NLists.GetBuffer();

            if (LinksInIdentifier != null)
            {
                // no need to search if there are no links
                using (var iLinks = LinksIn)
                {
                    if (fSortedLinksIn == null)
                    {
                        foreach (var i in iLinks)
                        {
                            if (i.MeaningID == meaning)
                            {
                                iIds.Add(i.FromID);
                            }
                        }
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iFound;
                        if (fSortedLinksIn.TryGetValue(meaning, out iFound))
                        {
                            foreach (var i in iFound)
                            {
                                iIds.Add(i.Value.FromID);
                            }
                        }
                    }
                }

                if (iRes.Capacity < iIds.Count)
                {
                    iRes.Capacity = iIds.Count;
                }

                foreach (var i in iIds)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        // could have been deleted?
                        iRes.Add(iFound);
                    }
                }
            }

            iMemFac.IDLists.Recycle(iIds);
            return iRes;
        }

        /// <summary>Searches and returns the first neuron linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The neuron that was found.</returns>
        public Neuron FindFirstOut(ulong meaning)
        {
            Link iRes = null;
            if (LinksOutIdentifier != null)
            {
                LockManager.Current.RequestLock(this, LockLevel.LinksOut, false);
                try
                {
                    if (fSortedLinksOut == null)
                    {
                        for (var i = 0; i < LinksOutIdentifier.Count; i++)
                        {
                            if (LinksOutIdentifier[i].MeaningID == meaning)
                            {
                                iRes = LinksOutIdentifier[i];
                            }
                        }
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iFound;
                        if (fSortedLinksOut.TryGetValue(meaning, out iFound))
                        {
                            iRes = Enumerable.First(iFound).Value;
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(this, LockLevel.LinksOut, false);
                }

                if (iRes != null)
                {
                    return iRes.To;
                }

                return null;
            }

            return null;
        }

        /// <summary>Searches and returns the first neuron linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The neuron that was found.</returns>
        public Neuron FindFirstOutEmptyInfo(ulong meaning)
        {
            if (LinksOutIdentifier != null)
            {
                Link iFound = null;
                using (var iLinks = LinksOut)
                {
                    if (fSortedLinksOut == null)
                    {
                        foreach (var i in iLinks)
                        {
                            if (i.MeaningID == meaning && i.Info.CountUnsafe == 0)
                            {
                                iFound = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iSub;
                        if (fSortedLinksOut.TryGetValue(meaning, out iSub))
                        {
                            foreach (var i in iSub)
                            {
                                if (i.Value.Info.CountUnsafe == 0)
                                {
                                    iFound = i.Value;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (iFound != null)
                {
                    return iFound.To;
                }

                return null;
            }

            return null;
        }

        /// <summary>Searches and returns the last neuron linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The neuron that was found.</returns>
        public Neuron FindFirstIn(ulong meaning)
        {
            if (LinksInIdentifier != null)
            {
                Link iFound;

                    // need to make certain that we don't have lock within lock (get result neuron inside 'using)
                if (fSortedLinksIn == null)
                {
                    using (var iLinks = LinksIn) iFound = (from i in iLinks where i.MeaningID == meaning select i).FirstOrDefault();
                }
                else
                {
                    System.Collections.Generic.Dictionary<ulong, Link> iSub;
                    if (fSortedLinksIn.TryGetValue(meaning, out iSub))
                    {
                        iFound = iSub.First().Value;
                    }
                    else
                    {
                        iFound = null;
                    }
                }

                if (iFound != null)
                {
                    return iFound.From;
                }

                return null;
            }

            return null;
        }

        /// <summary>The find next in.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron FindNextIn(ulong meaning, Neuron prev)
        {
            Neuron iRes = null;
            if (LinksInIdentifier != null)
            {
                var iMemFac = Factories.Default;
                var iIds = iMemFac.IDLists.GetBuffer();
                var iFound = false;
                using (var iLinks = LinksIn)
                    foreach (var i in iLinks)
                    {
                        // this does a lock.
                        if (i.MeaningID == meaning)
                        {
                            iIds.Add(i.FromID);
                        }
                    }

                foreach (var i in iIds)
                {
                    if (i == prev.ID)
                    {
                        iFound = true;
                    }
                    else if (iFound)
                    {
                        iRes = Brain.Current[i];
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }

            return iRes; // no item found
        }

        /// <summary>finds the next outgoing neuron after the specified item, for the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <param name="prev"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron FindNextOut(ulong meaning, Neuron prev)
        {
            if (LinksOutIdentifier != null)
            {
                var iMemFac = Factories.Default;
                var iIds = iMemFac.IDLists.GetBuffer();

                using (var iLinks = LinksOut)
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == meaning)
                        {
                            iIds.Add(i.ToID);
                        }
                    }

                var iFound = false;
                Neuron iRes = null;
                foreach (var i in iIds)
                {
                    if (i == prev.ID)
                    {
                        iFound = true;
                    }
                    else if (iFound)
                    {
                        iRes = Brain.Current[i];
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
                return iRes;
            }

            return null;
        }

        /// <summary>Finds the prev in.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <param name="next">The next.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron FindPrevIn(ulong meaning, Neuron next)
        {
            Neuron iRes = null;
            if (LinksInIdentifier != null)
            {
                var iMemFac = Factories.Default;
                var iIds = iMemFac.IDLists.GetBuffer();

                using (var iLinks = LinksIn)
                    foreach (var i in iLinks)
                    {
                        // this does a lock.
                        if (i.MeaningID == meaning)
                        {
                            iIds.Add(i.FromID);
                        }
                    }

                var iFound = false;
                for (var i = iIds.Count - 1; i >= 0; i--)
                {
                    if (iIds[i] == next.ID)
                    {
                        iFound = true;
                    }
                    else if (iFound)
                    {
                        iRes = Brain.Current[iIds[i]];
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }

            return iRes;
        }

        /// <summary>Finds the previous outgoing neuron.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <param name="next">The next.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron FindPrevOut(ulong meaning, Neuron next)
        {
            if (LinksOutIdentifier != null)
            {
                var iMemFac = Factories.Default;

                    // keep a local copy, so we don't waste time each time getting an expensive threadlocal value.
                var iIds = iMemFac.IDLists.GetBuffer();
                using (var iLinks = LinksOut)
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == meaning)
                        {
                            iIds.Add(i.ToID);
                        }
                    }

                Neuron iRes = null;
                var iFound = false;
                for (var i = iIds.Count - 1; i >= 0; i--)
                {
                    if (iIds[i] == next.ID)
                    {
                        iFound = true;
                    }
                    else if (iFound)
                    {
                        iRes = Brain.Current[iIds[i]];
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
                return iRes;
            }

            return null;
        }

        /// <summary>Looks up the list of common paretns that the specified neurons have.</summary>
        /// <param name="children"></param>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<Neuron> FindCommonParents(System.Collections.Generic.IList<Neuron> children)
        {
            var iLock = Instruction.BuildParentsLock(children, false);
            var iRes = new System.Collections.Generic.List<Neuron>();
            LockManager.Current.RequestLocks(iLock);
            try
            {
                GetCommonParentUnsafe(children, iRes);
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
            }

            return iRes;
        }

        /// <summary>Searches and returns the last neuron linked through the <see cref="Neuron.LinksIn"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The neuron that was found.</returns>
        public Neuron FindLastIn(ulong meaning)
        {
            Link iFound; // need to make certain that we don't have lock within lock (get result neuron inside 'using)
            if (LinksInIdentifier != null)
            {
                using (var iLinks = LinksIn)
                {
                    if (fSortedLinksIn == null)
                    {
                        iFound = (from i in iLinks where i.MeaningID == meaning select i).LastOrDefault();
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iSub;
                        if (fSortedLinksIn.TryGetValue(meaning, out iSub))
                        {
                            iFound = iSub.Last().Value;
                        }
                        else
                        {
                            iFound = null;
                        }
                    }
                }

                if (iFound != null)
                {
                    return iFound.From;
                }

                return null;
            }

            return null;
        }

        /// <summary>Searches and returns the last neuron linked through the <see cref="Neuron.LinksOut"/> list
        ///     where the link has the specified meaning.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The neuron that was found.</returns>
        public Neuron FindLastOut(ulong meaning)
        {
            if (LinksOutIdentifier != null)
            {
                using (var iLinks = LinksOut)
                {
                    Link iFound;
                    if (fSortedLinksOut == null)
                    {
                        iFound = (from i in iLinks where i.MeaningID == meaning select i).LastOrDefault();

                            // need to make certain that we don't have lock within lock (get result neuron inside 'using)
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<ulong, Link> iSub;
                        if (fSortedLinksOut.TryGetValue(meaning, out iSub))
                        {
                            iFound = iSub.Last().Value;
                        }
                        else
                        {
                            iFound = null;
                        }
                    }

                    if (iFound != null)
                    {
                        return iFound.To;
                    }

                    return null;
                }
            }

            return null;
        }

        /// <summary>Searches and returns the first cluster found in the <see cref="Neuron.ClusteredBy"/> list
        ///     where the meaning of the cluster is the specified value.  If no cluster can be found, null is returned.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster FindFirstClusteredBy(ulong meaning)
        {
            NeuronCluster iRes = null;
            if (fClusterdBy != null)
            {
                var iMemFac = Factories.Default;
                System.Collections.Generic.List<ulong> iIds;

                using (var iList = ClusteredBy)
                {
                    iIds = iMemFac.IDLists.GetBuffer(iList.Count);
                    iIds.AddRange(iList);
                }

                foreach (var i in iIds)
                {
                    Neuron iParent;
                    if (Brain.Current.TryFindNeuron(i, out iParent) && iParent is NeuronCluster
                        && ((NeuronCluster)iParent).Meaning == meaning)
                    {
                        iRes = (NeuronCluster)iParent;
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }

            return iRes;
        }

        /// <summary>Searches and returns the first cluster found in the <see cref="Neuron.ClusteredBy"/> list
        ///     where the meaning of the cluster is the specified value.  If no cluster can be found, null is returned.
        ///     Always returns a list, if no results, list is empty.</summary>
        /// <param name="meaning">The id of the meaning to look for</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<NeuronCluster> FindAllClusteredBy(ulong meaning)
        {
            var iRes = new System.Collections.Generic.List<NeuronCluster>();

                // isn't called from within the engine (yet), so no optimisation here.
            if (fClusterdBy != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusteredBy;
                using (var iList = ClusteredBy) iClusteredBy = iList.ConvertTo<NeuronCluster>(); // need to keep this lock as simple as possible.
                try
                {
                    foreach (var iParent in iClusteredBy)
                    {
                        if (iParent != null && iParent.Meaning == meaning)
                        {
                            iRes.Add(iParent);
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusteredBy);
                }
            }

            return iRes;
        }

        /// <summary>Finds the link between the specfied neurons in an unsafe manner.
        ///     This should only be called from within an existing lock.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        internal static Link FindLinkUnsafe(Neuron from, Neuron to, Neuron meaning)
        {
            if (to != null && from != null && meaning != null && to.LinksInIdentifier != null
                && from.LinksOutIdentifier != null)
            {
                // if to.fLinksIn == null -> no incomming links, so nothing to find.
                System.Collections.Generic.Dictionary<ulong, Link> iFound;
                if (from.fSortedLinksOut != null)
                {
                    if (@from.fSortedLinksOut.TryGetValue(meaning.ID, out iFound))
                    {
                        Link iRes;
                        if (iFound.TryGetValue(to.ID, out iRes))
                        {
                            return iRes;
                        }
                    }
                }
                else if (to.fSortedLinksIn != null)
                {
                    if (to.fSortedLinksIn.TryGetValue(meaning.ID, out iFound))
                    {
                        Link iRes;
                        if (iFound.TryGetValue(@from.ID, out iRes))
                        {
                            return iRes;
                        }
                    }
                }
                else if (from.FLinksOut.Count < to.FLinksIn.Count)
                {
                    foreach (var i in from.FLinksOut)
                    {
                        if (i.ToID == to.ID && i.MeaningID == meaning.ID)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    foreach (var i in to.FLinksIn)
                    {
                        if (i.FromID == from.ID && i.MeaningID == meaning.ID)
                        {
                            return i;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Reads the class from xml file.</summary>
        /// <param name="reader"></param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fID = XmlStore.ReadElement<ulong>(reader, "ID");
            fInfoUsageCount = XmlStore.ReadElement<ulong>(reader, "InfoUsageCount");
            fMeaningUsageCount = XmlStore.ReadElement<ulong>(reader, "MeaningUsageCount");
            fAccessCounter = XmlStore.ReadElement<uint>(reader, "AccessCounter");
            var iWeight = 0;
            XmlStore.TryReadElement(reader, "Weight", ref iWeight);

                // weight has been removed from the neuron, is now stored by the processor so we can make it processor local.
            var iIndex = 0;
            if (XmlStore.TryReadElement(reader, "Module", ref iIndex))
            {
                // if there is a module index defined, read this as well.
                ModuleRefCount = iIndex;
            }

            var iRes = new LinkResolverData();
            XmlStore.ReadList(reader, "LinksOut", ReadLink, iRes.LinksOut.Items, true);
            XmlStore.ReadList(reader, "LinksIn", ReadLink, iRes.LinksIn.Items, false);
            var iClusteredBy = new System.Collections.Generic.List<ulong>();
            XmlStore.ReadIDList(reader, "Clusters", iClusteredBy);
            if (iClusteredBy.Count > 0)
            {
                if (fClusterdBy == null)
                {
                    fClusterdBy = Factories.Default.ClusterLists.GetList(this);

                        // make the list manually, so that we don't use the factory to create the object, cause data read from disk is usually garbage collected and not deleted, so not recycled.
                }

                fClusterdBy.List = iClusteredBy; // so we only create the ClusteredBY list when there really is data.
            }

            LinkResolver.Default.Add(iRes, this); // we register ourselfs as being in the process of loading links,
        }

        /// <summary>The read link.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        /// <param name="asOut">The as out.</param>
        private void ReadLink(System.Xml.XmlReader reader, System.Collections.Generic.List<Link> list, bool asOut)
        {
            reader.ReadStartElement("Link");

            var iID = XmlStore.ReadElement<ulong>(reader, "ID");
            var iMeaningId = XmlStore.ReadElement<ulong>(reader, "Meaning");

            // create link
            Link iLink;
            if (asOut)
            {
                iLink = new Link(iID, ID, iMeaningId);
            }
            else
            {
                iLink = new Link(ID, iID, iMeaningId);
            }

            list.Add(iLink);
            if (iLink != null)
            {
                XmlStore.ReadIDList(reader, "ExtraInfo", iLink.InfoDirect.List); // extra info of link.
            }
            else
            {
                // the link is invalid somehow, but we still need to read the info data,
                // so create a temp list and do it like so.
                var iTemp = new System.Collections.Generic.List<ulong>();
                XmlStore.ReadIDList(reader, "ExtraInfo", iTemp);
            }

            reader.ReadEndElement();
        }

        /// <summary>Writes the class to xml files</summary>
        /// <param name="writer">The xml writer to use</param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            WriteXmlHeader(writer);
            XmlStore.WriteElement(writer, "Module", ModuleRefCount);
            WriteXmlChildren(writer);
        }

        /// <summary>The write xml children.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteXmlChildren(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("LinksOut");
            if (LinksOutIdentifier != null)
            {
                foreach (var i in LinksOutIdentifier)
                {
                    WriteLink(writer, i, true);
                }
            }

            writer.WriteEndElement();

            writer.WriteStartElement("LinksIn");
            if (LinksInIdentifier != null)
            {
                foreach (var i in LinksInIdentifier)
                {
                    WriteLink(writer, i, false);
                }
            }

            writer.WriteEndElement();
            if (fClusterdBy != null)
            {
                XmlStore.WriteIDList(writer, "Clusters", fClusterdBy);
            }
        }

        /// <summary>Writes the XML top part.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteXmlHeader(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "ID", fID);
            XmlStore.WriteElement(writer, "InfoUsageCount", InfoUsageCount);
            XmlStore.WriteElement(writer, "MeaningUsageCount", MeaningUsageCount);
            XmlStore.WriteElement(writer, "AccessCounter", AccessCounter);
        }

        /// <summary>Writes the contents of a link to an xml stream.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="link">The link.</param>
        /// <param name="asOut"></param>
        private void WriteLink(System.Xml.XmlWriter writer, Link link, bool asOut)
        {
            writer.WriteStartElement("Link");

            writer.WriteStartElement("ID");
            if (asOut)
            {
                writer.WriteString(link.ToID.ToString());
            }
            else
            {
                writer.WriteString(link.FromID.ToString());
            }

            writer.WriteEndElement();

            XmlStore.WriteElement(writer, "Meaning", link.MeaningID);
            XmlStore.WriteIDList(writer, "ExtraInfo", link.Info);
            writer.WriteEndElement();
        }

        #endregion

        #region INDBStreamable Members

        /// <summary>Reads the object from the specified reader.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        public LinkResolverData Read(CompactBinaryReader reader)
        {
            reader.Version = reader.ReadInt32(); // we first check the version nr, so we can add new fields later on.
            if (reader.Version <= 1)
            {
                return ReadV1(reader);
            }

            if (reader.Version == 2)
            {
                return ReadV2(reader);
            }

            throw new System.InvalidOperationException("Unkown version, can't read neuron from storage.");
        }

        /// <summary>this file format is better adjusted for bigger database set, where some neurons have lots of children/parents/links</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected virtual LinkResolverData ReadV2(CompactBinaryReader reader)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>hte neuron that was read, in a format that can be used to further resolve the links.</returns>
        protected virtual LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            fID = reader.ReadUInt64();
            fInfoUsageCount = reader.ReadUInt64();
            fMeaningUsageCount = reader.ReadUInt64();
            fAccessCounter = reader.ReadUInt32();
            if (reader.ReadBoolean())
            {
                // we always write a bool to indicate if there is a module index or not (this is a left over from the old system that stored a moduleIndex with multiple numbers.
                fModuleRefCount = reader.ReadInt32();
            }

            var iData = new LinkResolverData();
            ReadLinks(reader, iData, true);
            ReadLinks(reader, iData, false);
            ReadClusteredBy(reader);
            LinkResolver.Default.Add(iData, this);

                // we register ourselfs as ready for being resolved. So that other parts can find this during the load/resolve process.
            iData.ToResolve = this;
            return iData;
        }

        /// <summary>Reads a list of ids from the reader and stores them in the list.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadClusteredBy(System.IO.BinaryReader reader)
        {
            var iNrInfo = reader.ReadInt32();
            if (iNrInfo > 0)
            {
                if (fClusterdBy == null)
                {
                    fClusterdBy = Factories.Default.ClusterLists.GetList(this, iNrInfo);

                        // make the list manually, so that we don't use the factory to create the object, cause data read from disk is usually garbage collected and not deleted, so not recycled.
                }

                var list = FClusterdBy.List; // only create clusteredBy when absolutely needed.
                while (iNrInfo > 0)
                {
                    iNrInfo--;
                    var iId = reader.ReadUInt64();
                    list.Add(iId);
                }
            }
        }

        /// <summary>Reads all the links.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="data">The data.</param>
        /// <param name="asOut">if set to <c>true</c> the links should be stored as outgoing links, otherwise as incomming.</param>
        private void ReadLinks(System.IO.BinaryReader reader, LinkResolverData data, bool asOut)
        {
            LinkResolverDataList iList;
            if (asOut)
            {
                iList = data.LinksOut;
            }
            else
            {
                iList = data.LinksIn;
            }

            System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> iNeuronDict
                = null;
            var iNrLinks = reader.ReadInt32();
            if (iList.Items.Capacity < iNrLinks)
            {
                iList.Items.Capacity = iNrLinks;
            }

            if (iNrLinks > Brain.Current.CacheCount)
            {
                // if ther are more links than items in the cache, build an index of the links so that we can do a quick inverse lookup (walk through the cached neurons instead of the links)
                iList.Index =
                    new System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, int>>(
                        );
                if (iNrLinks >= Settings.MinNrOfLinksForIndex)
                {
                    // if there are enough links, build an index for the neuron as well. When doing an inverse link resolve, it is best to build the neuron's index also reight now (fastest), otherwise we have to walk through all the links anyway, in which case it is faster to create the neuron's index during the resolve stage.
                    iNeuronDict =
                        new System.Collections.Generic.Dictionary
                            <ulong, System.Collections.Generic.Dictionary<ulong, Link>>();
                    if (asOut)
                    {
                        fSortedLinksOut = iNeuronDict;
                    }
                    else
                    {
                        fSortedLinksIn = iNeuronDict;
                    }
                }
            }

            while (iNrLinks > 0)
            {
                iNrLinks--;
                var iID = reader.ReadUInt64();
                var iMeaningId = reader.ReadUInt64();
                Link iLink;
                if (asOut)
                {
                    // out always comes first, so simply create a new link, even if it is a link to self, this is resolved during the 'link-in' handling
                    iLink = new Link(iID, ID, iMeaningId);
                }
                else if (iID != ID)
                {
                    // link to other object: regular operation, simple 
                    iLink = new Link(ID, iID, iMeaningId);
                }
                else
                {
                    // it's a self ref, so look up the link object
                    if (data.LinksOut.Index != null)
                    {
                        iLink = data.LinksOut.Items[data.LinksOut.Index[iID][iMeaningId]];
                    }
                    else
                    {
                        iLink =
                            (from i in data.LinksOut.Items
                             where i.FromID == iID && i.ToID == iID && i.MeaningID == iMeaningId
                             select i).FirstOrDefault();
                    }
                }

                if (iLink == null)
                {
                    throw new System.InvalidOperationException();
                }

                if (iList.Index != null)
                {
                    // we need to build an index, so for each meaning, to/from pair, store the index position of the link into the list
                    AddToLinkToIndex(iList, iID, iMeaningId);
                    if (iNeuronDict != null)
                    {
                        AddToLinkToIndex(iNeuronDict, iID, iMeaningId, iLink);
                    }
                }

                iList.Items.Add(iLink);
                ReadLinkInfoList(reader, iLink);
            }
        }

        /// <summary>The add to link to index.</summary>
        /// <param name="dict">The dict.</param>
        /// <param name="iD">The i d.</param>
        /// <param name="meaningId">The meaning id.</param>
        /// <param name="link">The link.</param>
        private void AddToLinkToIndex(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> dict, 
            ulong iD, 
            ulong meaningId, 
            Link link)
        {
            System.Collections.Generic.Dictionary<ulong, Link> iIndex;
            if (dict.TryGetValue(meaningId, out iIndex) == false)
            {
                iIndex = new System.Collections.Generic.Dictionary<ulong, Link>();
                dict.Add(meaningId, iIndex);
            }

            iIndex[iD] = link;
        }

        /// <summary>Adds the link to a temp dictionary, sorted first on 'other side of the link', next on meaning. this
        ///     provides resolve.</summary>
        /// <param name="list">The list.</param>
        /// <param name="iID">The i ID.</param>
        /// <param name="iMeaningId">The i meaning id.</param>
        private void AddToLinkToIndex(LinkResolverDataList list, ulong iID, ulong iMeaningId)
        {
            System.Collections.Generic.Dictionary<ulong, int> iIndex;
            if (list.Index.TryGetValue(iID, out iIndex) == false)
            {
                iIndex = new System.Collections.Generic.Dictionary<ulong, int>();
                list.Index.Add(iID, iIndex);
            }

            iIndex[iMeaningId] = list.Items.Count;
        }

        /// <summary>Reads a list of ids from the reader and stores them in the list.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="link">The list.</param>
        private void ReadLinkInfoList(System.IO.BinaryReader reader, Link link)
        {
            var iNrInfo = reader.ReadInt32();
            if (iNrInfo > 0)
            {
                var iList = link.InfoDirect.List;
                if (iList.Count == 0)
                {
                    // the list can already be populated cause the link object was already loaded (for self refs for instance), when this is the case, make certain we don't try to read it again, this would make the list incorrect.
                    if (iList.Capacity < iNrInfo)
                    {
                        iList.Capacity = iNrInfo;
                    }

                    while (iNrInfo > 0)
                    {
                        iNrInfo--;
                        var iId = reader.ReadUInt64();
                        iList.Add(iId);
                    }
                }
                else
                {
                    while (iNrInfo > 0)
                    {
                        iNrInfo--;
                        var iId = reader.ReadUInt64(); // don't try to compare or anything, no need for this.
                    }
                }
            }
        }

        /// <summary>Writes the object to the specified writer.</summary>
        /// <param name="writer">The writer.</param>
        public void Write(System.IO.BinaryWriter writer)
        {
            WriteV1(writer);
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected virtual void WriteV1(System.IO.BinaryWriter writer)
        {
            writer.Write(1);

                // this is the version of the neuron stream format, so we can add new/change fields later on. 
            writer.Write(fID);
            writer.Write(fInfoUsageCount);
            writer.Write(fMeaningUsageCount);
            writer.Write(fAccessCounter);
            writer.Write(ModuleRefCount != 0);
            if (ModuleRefCount != 0)
            {
                writer.Write(ModuleRefCount);
            }

            WriteLinks(writer, LinksOutIdentifier, true);

                // don't need to lock the list, the entirre neuron is locked during hte operation.
            WriteLinks(writer, LinksInIdentifier, false);
            if (fClusterdBy != null)
            {
                WriteIDList(writer, fClusterdBy.List);
            }
            else
            {
                writer.Write(0); // need to make certain that the system knows there aren't any parent items.
            }
        }

        /// <summary>Writes the ID list.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="list">The list.</param>
        protected void WriteIDList(System.IO.BinaryWriter writer, System.Collections.Generic.IList<ulong> list)
        {
            if (list != null)
            {
                writer.Write(list.Count);
                foreach (var i in list)
                {
                    writer.Write(i);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        /// <summary>Writes the links.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="links">The links.</param>
        /// <param name="asOut">if set to <c>true</c> [as out].</param>
        private void WriteLinks(System.IO.BinaryWriter writer, System.Collections.Generic.IList<Link> links, bool asOut)
        {
            if (links != null)
            {
                writer.Write(links.Count);
                if (asOut)
                {
                    foreach (var i in links)
                    {
                        writer.Write(i.ToID);
                        writer.Write(i.MeaningID);
                        WriteIDList(writer, (System.Collections.Generic.IList<ulong>)i.InfoIdentifier);

                            // don't need to lock, the entire neuron is locked.
                    }
                }
                else
                {
                    foreach (var i in links)
                    {
                        writer.Write(i.FromID);
                        writer.Write(i.MeaningID);
                        WriteIDList(writer, (System.Collections.Generic.IList<ulong>)i.InfoIdentifier);
                    }
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        #endregion

        #region IGetInt Members

        /// <summary>Gets the int value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public virtual int GetInt(Processor processor)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>returns if this object can return an int.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool CanGetInt()
        {
            return false;
        }

        #endregion

        #region IGetBool Members

        /// <summary>gets the bool value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool GetBool(Processor processor)
        {
            return ID == (ulong)PredefinedNeurons.True || ID == (ulong)PredefinedNeurons.False;
        }

        /// <summary>returns if this object can return an bool.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool CanGetBool()
        {
            return ID == (ulong)PredefinedNeurons.True;
        }

        #endregion

        #region IGetDouble Members

        /// <summary>Gets the double value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public virtual double GetDouble(Processor processor)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>returns if this object can return an double.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool CanGetDouble()
        {
            return false;
        }

        #endregion
    }
}