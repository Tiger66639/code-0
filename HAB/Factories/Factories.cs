// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Factories.cs" company="">
//   
// </copyright>
// <summary>
//   maintains a collection of factories used by the network, for each thread,
//   a set of factories is provided.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     maintains a collection of factories used by the network, for each thread,
    ///     a set of factories is provided.
    /// </summary>
    public class Factories
    {
        /// <summary>The f default.</summary>
        [System.ThreadStatic]
        private static Factories fDefault;

        /// <summary>The f call frames.</summary>
        private readonly CallFrameFactory<CallFrame> fCallFrames = new CallFrameFactory<CallFrame>();

        /// <summary>The f call inst frames.</summary>
        private readonly CallFrameFactory<CallInstCallFrame> fCallInstFrames = new CallFrameFactory<CallInstCallFrame>();

        /// <summary>The f case frames.</summary>
        private readonly CallFrameFactory<CaseFrame> fCaseFrames = new CallFrameFactory<CaseFrame>();

        /// <summary>The f case loop frames.</summary>
        private readonly CallFrameFactory<CaseLoopedCallFrame> fCaseLoopFrames =
            new CallFrameFactory<CaseLoopedCallFrame>();

        /// <summary>The f child lists.</summary>
        private readonly ChildListFactory fChildLists = new ChildListFactory();

        /// <summary>The f children acc factory.</summary>
        private readonly ListAccessorsFactory<ChildrenAccessor, ulong> fChildrenAccFactory =
            new ListAccessorsFactory<ChildrenAccessor, ulong>();

        /// <summary>The f c lists.</summary>
        private readonly ObjectListFactory<NeuronCluster> fCLists = new ObjectListFactory<NeuronCluster>(300, 100);

        /// <summary>The f cluster lists.</summary>
        private readonly ClusterListFactory fClusterLists = new ClusterListFactory();

        /// <summary>The f clusters acc factory.</summary>
        private readonly ListAccessorsFactory<ClustersAccessor, ulong> fClustersAccFactory =
            new ListAccessorsFactory<ClustersAccessor, ulong>();

        /// <summary>The f expression frames.</summary>
        private readonly CallFrameFactory<ExpressionBlockFrame> fExpressionFrames =
            new CallFrameFactory<ExpressionBlockFrame>();

        /// <summary>The f for each frames.</summary>
        private readonly CallFrameFactory<ForEachCallFrame> fForEachFrames = new CallFrameFactory<ForEachCallFrame>();

        /// <summary>The f for query frames.</summary>
        private readonly CallFrameFactory<ForQueryCallFrame> fForQueryFrames = new CallFrameFactory<ForQueryCallFrame>();

        /// <summary>The f frozen for acc factory.</summary>
        private readonly ProcAcessorsFactory fFrozenForAccFactory = new ProcAcessorsFactory();

        /// <summary>The f frozen for lists.</summary>
        private readonly HashSetFactory<Processor> fFrozenForLists = new HashSetFactory<Processor>();

        /// <summary>The f id lists.</summary>
        private readonly IDListFactory fIDLists = new IDListFactory();

        /// <summary>The f if frames.</summary>
        private readonly CallFrameFactory<IfFrame> fIfFrames = new CallFrameFactory<IfFrame>();

        /// <summary>The f link info acc factory.</summary>
        private readonly ListAccessorsFactory<LinkInfoAccessor, ulong> fLinkInfoAccFactory =
            new ListAccessorsFactory<LinkInfoAccessor, ulong>();

        /// <summary>The f link lists.</summary>
        private readonly ObjectListFactory<Link> fLinkLists = new ObjectListFactory<Link>(300);

        /// <summary>The f links in acc factory.</summary>
        private readonly ListAccessorsFactory<LinksListAccessor, Link> fLinksInAccFactory =
            new ListAccessorsFactory<LinksListAccessor, Link>(LockLevel.LinksIn);

        /// <summary>The f links out acc factory.</summary>
        private readonly ListAccessorsFactory<LinksListAccessor, Link> fLinksOutAccFactory =
            new ListAccessorsFactory<LinksListAccessor, Link>(LockLevel.LinksOut);

        /// <summary>The f lock request dicts.</summary>
        private readonly DictionaryFactory<ulong, LockRequestInfo> fLockRequestDicts =
            new DictionaryFactory<ulong, LockRequestInfo>();

        /// <summary>The f loop frames.</summary>
        private readonly CallFrameFactory<LoopedCallFrame> fLoopFrames = new CallFrameFactory<LoopedCallFrame>();

        /// <summary>The f n hash sets.</summary>
        private readonly HashSetFactory<Neuron> fNHashSets = new HashSetFactory<Neuron>();

        /// <summary>The f n lists.</summary>
        private readonly ObjectListFactory<Neuron> fNLists = new ObjectListFactory<Neuron>();

        /// <summary>The f ulong hash sets.</summary>
        private readonly HashSetFactory<ulong> fUlongHashSets = new HashSetFactory<ulong>();

        /// <summary>The f until frames.</summary>
        private readonly CallFrameFactory<UntilCallFrame> fUntilFrames = new CallFrameFactory<UntilCallFrame>();

        /// <summary>
        ///     Gets the default set of factories for this thread.
        /// </summary>
        public static Factories Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new Factories();
                }

                return fDefault;
            }
        }

        #region ClusterLists

        /// <summary>
        ///     Gets the clusterList factory.
        /// </summary>
        public ClusterListFactory ClusterLists
        {
            get
            {
                return fClusterLists;
            }
        }

        #endregion

        #region ClustersAccFactory

        /// <summary>
        ///     Gets the factory for clustersList accessor objects.
        /// </summary>
        public ListAccessorsFactory<ClustersAccessor, ulong> ClustersAccFactory
        {
            get
            {
                return fClustersAccFactory;
            }
        }

        #endregion

        #region ChildLists

        /// <summary>
        ///     Gets the clusterList factory.
        /// </summary>
        public ChildListFactory ChildLists
        {
            get
            {
                return fChildLists;
            }
        }

        #endregion

        #region ChildrenAccFactory

        /// <summary>
        ///     Gets the factory for clustersList accessor objects.
        /// </summary>
        public ListAccessorsFactory<ChildrenAccessor, ulong> ChildrenAccFactory
        {
            get
            {
                return fChildrenAccFactory;
            }
        }

        #endregion

        #region IDLists

        /// <summary>
        ///     Gets the IdList factory
        /// </summary>
        public IDListFactory IDLists
        {
            get
            {
                return fIDLists;
            }
        }

        #endregion

        #region NLists

        /// <summary>
        ///     Gets/sets the neuronlist factory
        /// </summary>
        public ObjectListFactory<Neuron> NLists
        {
            get
            {
                return fNLists;
            }
        }

        #endregion

        #region CLists

        /// <summary>
        ///     Gets the NeuronClusterLists factory
        /// </summary>
        public ObjectListFactory<NeuronCluster> CLists
        {
            get
            {
                return fCLists;
            }
        }

        #endregion

        #region LinkLists

        /// <summary>
        ///     Gets the linkList factory
        /// </summary>
        public ObjectListFactory<Link> LinkLists
        {
            get
            {
                return fLinkLists;
            }
        }

        #endregion

        #region CallInstFrames

        /// <summary>
        ///     Gets the callInst fame factory
        /// </summary>
        internal CallFrameFactory<CallInstCallFrame> CallInstFrames
        {
            get
            {
                return fCallInstFrames;
            }
        }

        #endregion

        #region ExpressionFrames

        /// <summary>
        ///     Gets the Expresions frame factory
        /// </summary>
        internal CallFrameFactory<ExpressionBlockFrame> ExpressionFrames
        {
            get
            {
                return fExpressionFrames;
            }
        }

        #endregion

        #region UntilFrames

        /// <summary>
        ///     Gets the until frame factory
        /// </summary>
        internal CallFrameFactory<UntilCallFrame> UntilFrames
        {
            get
            {
                return fUntilFrames;
            }
        }

        #endregion

        #region ForQueryFrames

        /// <summary>
        ///     Gets the forQueryFrame factory
        /// </summary>
        internal CallFrameFactory<ForQueryCallFrame> ForQueryFrames
        {
            get
            {
                return fForQueryFrames;
            }
        }

        #endregion

        #region ForEachFrames

        /// <summary>
        ///     Gets the <see langword="foreach" /> callframe factory
        /// </summary>
        internal CallFrameFactory<ForEachCallFrame> ForEachFrames
        {
            get
            {
                return fForEachFrames;
            }
        }

        #endregion

        #region CaseLoopFrames

        /// <summary>
        ///     Gets the caseloopframes factory
        /// </summary>
        internal CallFrameFactory<CaseLoopedCallFrame> CaseLoopFrames
        {
            get
            {
                return fCaseLoopFrames;
            }
        }

        #endregion

        #region LoopFrames

        /// <summary>
        ///     Gets the loopframe factory
        /// </summary>
        internal CallFrameFactory<LoopedCallFrame> LoopFrames
        {
            get
            {
                return fLoopFrames;
            }
        }

        #endregion

        #region CallFrames

        /// <summary>
        ///     Gets the callframes factory
        /// </summary>
        internal CallFrameFactory<CallFrame> CallFrames
        {
            get
            {
                return fCallFrames;
            }
        }

        #endregion

        #region IfFrames

        /// <summary>
        ///     Gets the if frames factory
        /// </summary>
        internal CallFrameFactory<IfFrame> IfFrames
        {
            get
            {
                return fIfFrames;
            }
        }

        #endregion

        #region CaseFrames

        /// <summary>
        ///     Gets the case frame factory
        /// </summary>
        internal CallFrameFactory<CaseFrame> CaseFrames
        {
            get
            {
                return fCaseFrames;
            }
        }

        #endregion

        #region FrozenForLists

        /// <summary>
        ///     Gets the frozenFor lists factory
        /// </summary>
        public HashSetFactory<Processor> FrozenForLists
        {
            get
            {
                return fFrozenForLists;
            }
        }

        #endregion

        #region NHashSets

        /// <summary>
        ///     Gets the frozenFor lists factory
        /// </summary>
        public HashSetFactory<Neuron> NHashSets
        {
            get
            {
                return fNHashSets;
            }
        }

        #endregion

        #region UlongHashSets

        /// <summary>
        ///     Gets the frozenFor lists factory
        /// </summary>
        public HashSetFactory<ulong> UlongHashSets
        {
            get
            {
                return fUlongHashSets;
            }
        }

        #endregion

        #region ChildrenAccFactory

        /// <summary>
        ///     Gets the factory for clustersList accessor objects.
        /// </summary>
        public ProcAcessorsFactory FrozenForAccFactory
        {
            get
            {
                return fFrozenForAccFactory;
            }
        }

        #endregion

        #region LockRequestDicts

        /// <summary>
        ///     Gets the Lock request dictionaries factory.
        /// </summary>
        public DictionaryFactory<ulong, LockRequestInfo> LockRequestDicts
        {
            get
            {
                return fLockRequestDicts;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the link-info accessors factory.
        /// </summary>
        public ListAccessorsFactory<LinkInfoAccessor, ulong> LinkInfoAccFactory
        {
            get
            {
                return fLinkInfoAccFactory;
            }
        }

        /// <summary>
        ///     Gets the links-in accessors factory.
        /// </summary>
        public ListAccessorsFactory<LinksListAccessor, Link> LinksInAccFactory
        {
            get
            {
                return fLinksInAccFactory;
            }
        }

        /// <summary>
        ///     Gets the links-out accessors factory.
        /// </summary>
        public ListAccessorsFactory<LinksListAccessor, Link> LinksOutAccFactory
        {
            get
            {
                return fLinksOutAccFactory;
            }
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fClusterLists.Clear();
            fChildLists.Clear();
            fIDLists.Clear();
            fNLists.Clear();
            fCLists.Clear();
            fLinkLists.Clear();
            fCallFrames.Clear();
            fIfFrames.Clear();
            fCaseFrames.Clear();
            fLoopFrames.Clear();
            fCaseLoopFrames.Clear();
            fForEachFrames.Clear();
            fForQueryFrames.Clear();
            fUntilFrames.Clear();
            fExpressionFrames.Clear();
            fCallInstFrames.Clear();
            fFrozenForLists.Clear();
            fNHashSets.Clear();
            fUlongHashSets.Clear();
            fLockRequestDicts.Clear();
            fClustersAccFactory.Clear();
            fChildrenAccFactory.Clear();
            fFrozenForAccFactory.Clear();
            fLinkInfoAccFactory.Clear();
            fLinksInAccFactory.Clear();
            fLinksOutAccFactory.Clear();
        }
    }
}