// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventManager.cs" company="">
//   
// </copyright>
// <summary>
//   This class handles, filters and delegates all the events comming from the brain. This provides a single entry
//   point for events from the brain which can be handled in a different thread.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     This class handles, filters and delegates all the events comming from the brain. This provides a single entry
    ///     point for events from the brain which can be handled in a different thread.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        ///     The timer period used by the threaded timer to clean up the unused links.
        /// </summary>
        private const int TIMERPERIOD = 60000; // 1 min * 5

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventManager" /> class.
        /// </summary>
        public EventManager()
        {
            Brain.Current.Cleared += Brain_Cleared;
            Brain.Current.LinkChanged += Brain_LinkChanged;
            Brain.Current.NeuronListChanged += Brain_NeuronListChanged;
            Brain.Current.NeuronChanged += Current_NeuronChanged;
            fTimer = new System.Threading.Timer(Clean, null, TIMERPERIOD, TIMERPERIOD);
        }

        #endregion

        #region Prop

        #region Current

        /// <summary>
        ///     Gets the event manager that is currently loaded for this app.
        /// </summary>
        public static EventManager Current
        {
            get
            {
                return fCurrent;
            }
        }

        #endregion

        #endregion

        #region Fields

        /// <summary>The f current.</summary>
        private static readonly EventManager fCurrent = new EventManager();

        /// <summary>
        ///     This dictionary stores all the objects that monitor a specific link.  We use a dict so that the
        ///     event handler can quickly find all the monitors for a specific link.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<Link, System.Collections.Generic.List<EventMonitor>> fLinkMonitors =
                new System.Collections.Generic.Dictionary<Link, System.Collections.Generic.List<EventMonitor>>();

        /// <summary>
        ///     This dictionary stores all the objects that monitor any link activity for a specific neuron.  We use a dict so that
        ///     the
        ///     event handler can quickly find all the monitors for a specific neuron when a link is changed.
        ///     activity needs to be on the to part of the link.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>> fLinkPartInMonitors =
                new System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>>();

        /// <summary>
        ///     This dictionary stores all the objects that monitor any link activity for a specific neuron.  We use a dict so that
        ///     the
        ///     event handler can quickly find all the monitors for a specific neuron when a link is changed.
        ///     activity needs to be on the to part of the link.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>> fLinkPartOutMonitors =
                new System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>>();

        /// <summary>
        ///     This dictionary stores all the objects that monitor a specific neuron.  We use a dict so that the
        ///     event handler can quickly find all the monitors for a specific neuron.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>> fNeuronMonitors =
                new System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>>();

        /// <summary>
        ///     This dictionary stores all the objects that monitor a specific BrainList.  We use a dict so that the
        ///     event handler can quickly find all the monitors for a specific BrainList.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<object, System.Collections.Generic.List<EventMonitor>> fListMonitors =
                new System.Collections.Generic.Dictionary<object, System.Collections.Generic.List<EventMonitor>>();

        /// <summary>
        ///     This list contains all the objects that always get warned, they don't monitor a specific subset of items,
        ///     or they do there own filtering (like the NeuronInfoDict).
        /// </summary>
        private readonly System.Collections.Generic.List<EventMonitor> fAnyNeuronMonitors =
            new System.Collections.Generic.List<EventMonitor>();

        /// <summary>The f lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fLock = new System.Threading.ReaderWriterLockSlim();

        /// <summary>The f timer.</summary>
        private System.Threading.Timer fTimer;

        #endregion

        #region Functions

        #region Register

        #region fAnyNeuronMonitors

        /// <summary>Adds the monitor to the list in a thread safe manner so that it will be warned when any neuron has changed.</summary>
        /// <param name="toAdd">To add.</param>
        internal void AddAnyChangedMonitor(EventMonitor toAdd)
        {
            fLock.EnterWriteLock();
            try
            {
                fAnyNeuronMonitors.Add(toAdd);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Adds the monitor to the list in a thread safe manner so that it will be warned when any neuron has changed.</summary>
        /// <param name="toRemove">The to Remove.</param>
        internal void RemoveAnyChangedMonitor(EventMonitor toRemove)
        {
            fLock.EnterWriteLock();
            try
            {
                fAnyNeuronMonitors.Remove(toRemove);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region Clustercollection

        /// <summary>Registers the cluster Collection for monitoring if anything in it's list changed.</summary>
        /// <param name="item">The item.</param>
        public void RegisterClusterCollection<T>(ClusterCollection<T> item) where T : INeuronWrapper
        {
            var iNew = new ClusterCollectionEventmonitor<T>(item);
            System.Collections.Generic.List<ulong> iChildrenToRegister = null;
            if (item.Cluster.ID != Neuron.TempId && item.Cluster.ChildrenIdentifier != null)
            {
                // only try to load when the cluster is not a temp list, only to be created when it gets items.
                using (var iList = item.Cluster.Children)
                {
                    iChildrenToRegister = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);

                        // we only need a get, can be done unsave without lock, cause we only need a guide.
                    iChildrenToRegister.AddRange(iList);
                }
            }

            fLock.EnterWriteLock();
            try
            {
                System.Collections.Generic.List<EventMonitor> iFound;
                AddToListDict(iNew, item.Cluster.ChildrenDirect);
                var iOwnerItem = item.Owner != null ? ((INeuronWrapper)item.Owner).Item : null;
                if (iOwnerItem != null && item.Item.ID != iOwnerItem.ID)
                {
                    // when iOwnerItem is still null, it's usually because it maps back to the cluster and is not yet set because the collection (with the cluster) is not yet completely created. In this case, we don't need to monitor the owner's item.
                    AddToLinksOutDict(iNew, iOwnerItem.ID);

                        // we also monitor outgoing links on the owner, when the owner and the cluster are different, see if the item that owns the collection, changes it's pointer to a a new cluster, in which case we must reload the data in the collection.
                }

                if (iChildrenToRegister != null)
                {
                    // only try to load when the cluster is not a temp list, only to be created when it gets items.
                    foreach (var i in iChildrenToRegister)
                    {
                        // also monitor each child for changes, so that the items in the list can be updated.
                        if (fNeuronMonitors.TryGetValue(i, out iFound) == false)
                        {
                            iFound = new System.Collections.Generic.List<EventMonitor>();
                            iFound.Add(iNew);
                            fNeuronMonitors.Add(i, iFound);
                        }
                        else if (iFound.Contains(iNew) == false)
                        {
                            // a list can contain the same item 2 times, only need a monitor 1 time.
                            iFound.Add(iNew);
                        }
                    }

                    Factories.Default.IDLists.Recycle(iChildrenToRegister);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>The un register cluster collection.</summary>
        /// <param name="item">The item.</param>
        /// <typeparam name="T"></typeparam>
        public void UnRegisterClusterCollection<T>(ClusterCollection<T> item) where T : INeuronWrapper
        {
            fLock.EnterWriteLock();
            try
            {
                object iId = item.Cluster.ChildrenIdentifier;
                if (iId != null)
                {
                    // this sometimes happens for some reasons. don't know why.
                    RemoveFromListsDict(item, iId);
                }

                var iOwnerItem = item.Owner != null ? ((INeuronWrapper)item.Owner).Item : null;
                if (iOwnerItem != null && item.Item.ID != iOwnerItem.ID)
                {
                    RemoveFromLinkOutDict(item, iOwnerItem.ID);
                }

                foreach (INeuronWrapper i in item)
                {
                    // also monitor each child for changes, so that the items in the list can be updated. don't need to run through the children of the cluster, can run through the items, is faster, no lock required.
                    RemoveFromNeuronsDict(item, i.Item.ID);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region EditorItem

        /// <summary>Registers the editor item to be monitored for changes with regards to incomming or outgoing links and clustedBy.</summary>
        /// <param name="item">The item.</param>
        public void RegisterEditorItem(EditorItem item)
        {
            var iNew = new EditorItemEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                item.Item.ValidateClusteredByList();

                    // need to make certain that there is a list to monitor, othrewise we can get out of sync.
                AddToListDict(iNew, item.Item.ClusteredByIdentifier);
                System.Diagnostics.Debug.Assert(item.Item.ID != Neuron.TempId);
                AddToLinksInDict(iNew, item.Item.ID);
                AddToLinksOutDict(iNew, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>The un register editor item.</summary>
        /// <param name="item">The item.</param>
        public void UnRegisterEditorItem(EditorItem item)
        {
            fLock.EnterWriteLock();
            try
            {
                if (item.Item.ClusteredByIdentifier != null)
                {
                    RemoveFromListsDict(item, item.Item.ClusteredByIdentifier);
                }

                RemoveFromLinkInDict(item, item.Item.ID);
                RemoveFromLinkOutDict(item, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region EditorItem

        /// <summary>Registers the neuron-data to be monitored for changes with regards to outgoing links.</summary>
        /// <param name="item">The item.</param>
        public void RegisterNeuronData(NeuronData item)
        {
            var iNew = new NeuronDataEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                // Debug.Assert(item.ID != Neuron.TempId);
                AddToLinksOutDict(iNew, item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Removes the registration of any event monitors that were registered for the specified item.</summary>
        /// <param name="item">The item.</param>
        public void UnRegisterNeuronData(NeuronData item)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromLinkOutDict(item, item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region DebugNeuron

        /// <summary>Registers the debug neuron to be monitored for changes.</summary>
        /// <param name="item">The item.</param>
        public void RegisterDebugNeuron(DebugNeuron item)
        {
            var iNew = new DebugNeuronEventMonitor(item);

                // only the neuron change monitoring part is registered by default. The rest is done, on demand.
            fLock.EnterWriteLock();
            try
            {
                AddToNeuronsDict(iNew, item.Item.ID);
                item.Item.ValidateClusteredByList();

                    // we need to make certain that the cluster list exists otherwise, if it stll needs to be created when the first item is added, we aren't monitoring correctly.
                AddToListDict(iNew, item.Item.ClusteredByIdentifier);
                var iCluster = item.Item as NeuronCluster;
                if (iCluster != null)
                {
                    AddToListDict(iNew, iCluster.ChildrenDirect);
                }

                AddToLinksOutDict(iNew, item.Item.ID);
                AddToLinksInDict(iNew, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>UnRegisters the debug neuron to be monitored for changes.</summary>
        /// <param name="item">The item.</param>
        public void UnRegisterDebugNeuron(DebugNeuron item)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromNeuronsDict(item, item.Item.ID);
                if (item.Item.ClusteredByIdentifier != null)
                {
                    RemoveFromListsDict(item, item.Item.ClusteredByIdentifier);
                }

                var iCluster = item.Item as NeuronCluster;
                if (iCluster != null)
                {
                    RemoveFromListsDict(item, iCluster.ChildrenIdentifier);
                }

                RemoveFromLinkInDict(item, item.Item.ID);
                RemoveFromLinkOutDict(item, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region RegisterForChildChange

        /// <summary>Registers the specified monitor to check for changes in any child list of the specified cluster.
        ///     This is thread safe.</summary>
        /// <param name="toCheck">The list of clusters to check.</param>
        /// <param name="monitor">The monitor to use as a callback when something changed.</param>
        internal void RegisterForChildChange(System.Collections.Generic.IEnumerable<NeuronCluster> toCheck, 
            EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                foreach (var i in toCheck)
                {
                    AddToListDict(monitor, i.ChildrenDirect);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Registers the specified monitor to check for changes in any of the outgoing links on the neuron with the specified
        ///     id.</summary>
        /// <param name="monitor">The monitor.</param>
        /// <param name="id">The id.</param>
        internal void RegisterForLinksOut(EventMonitor monitor, ulong id)
        {
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(monitor, id);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>UnRegisters the specified monitor to check for changes in any of the outgoing links on the neuron with the
        ///     specified id.</summary>
        /// <param name="monitor">The monitor.</param>
        /// <param name="id">The id.</param>
        internal void UnRegisterForLinksOut(EventMonitor monitor, ulong id)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromLinkOutDict(monitor, id);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Unregister the monitor for all the clusters' child lists. This is thread safe.</summary>
        /// <param name="toRemove">To remove.</param>
        /// <param name="monitor">The monitor.</param>
        internal void UnRegisterForChildChange(System.Collections.Generic.IEnumerable<NeuronCluster> toRemove, 
            EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                foreach (var i in toRemove)
                {
                    RemoveFromListsDict(monitor, i.ChildrenIdentifier);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region RegisterForClusteredByChange

        /// <summary>Registers the specified monitor to check for changes in the 'ClusteredBy' list of the neuron.
        ///     This is thread safe.</summary>
        /// <param name="toCheck">The list of clusters to check.</param>
        /// <param name="monitor">The monitor to use as a callback when something changed.</param>
        internal void RegisterForClusteredByChange(Neuron toCheck, EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                toCheck.ValidateClusteredByList();
                AddToListDict(monitor, toCheck.ClusteredByIdentifier);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Removes the specified monitor so that it stops checking for any changes to 'ClusteredBy' list of the neuron.
        ///     This is thread safe.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="monitor">The monitor.</param>
        internal void UnRegisterForChildChange(Neuron toCheck, EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                if (toCheck.ClusteredByIdentifier != null)
                {
                    RemoveFromListsDict(monitor, toCheck.ClusteredByIdentifier);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region RegisterForLinkChange

        /// <summary>Registers the monitor to track any changes to the specified links. This is thread safe.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="monitor">The monitor.</param>
        internal void RegisterForLinkChange(System.Collections.Generic.IEnumerable<Link> toCheck, EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                foreach (var i in toCheck)
                {
                    AddToLinksDict(monitor, i);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>UnRegisters the monitor so that it stops tracking any changes to the specified links. This is thread safe.</summary>
        /// <param name="toRemove">The to Remove.</param>
        /// <param name="monitor">The monitor.</param>
        internal void UnRegisterForLinkChange(System.Collections.Generic.IEnumerable<Link> toRemove, 
            EventMonitor monitor)
        {
            fLock.EnterWriteLock();
            try
            {
                foreach (var i in toRemove)
                {
                    RemoveFromLinksDict(monitor, i);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region Frame

        /// <summary>Registers the frame to monitor for changes  and returns the monitor to use.</summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The <see cref="FrameEventMonitor"/>.</returns>
        internal FrameEventMonitor RegisterFrame(Frame frame)
        {
            var iRes = new FrameEventMonitor(frame);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, frame.Item.ID);
                AddToNeuronsDict(iRes, frame.Item.ID);
                AddToNeuronsDict(iRes, frame.Elements.Cluster.ID);
                if (frame.HasSequences)
                {
                    AddToNeuronsDict(iRes, frame.Sequences.Cluster.ID);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>Unregisters a frame.</summary>
        /// <param name="frame">The frame.</param>
        internal void UnRegisterFrame(Frame frame)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromLinkOutDict(frame, frame.Item.ID);
                RemoveFromNeuronsDict(frame, frame.Item.ID);
                if (frame.Elements != null && frame.Elements.Cluster != null)
                {
                    RemoveFromNeuronsDict(frame, frame.Elements.Cluster.ID);
                }

                if (frame.Sequences != null && frame.Sequences.Cluster != null)
                {
                    RemoveFromNeuronsDict(frame, frame.Sequences.Cluster.ID);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>The register frame sequence.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FrameSequenceEventMonitor"/>.</returns>
        internal FrameSequenceEventMonitor RegisterFrameSequence(FrameSequence item)
        {
            var iRes = new FrameSequenceEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, item.Item.ID);
                var iResrictionsCluster =
                    item.Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions) as NeuronCluster;
                if (iResrictionsCluster != null)
                {
                    AddToLinksOutDict(iRes, iResrictionsCluster.ID);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>The register frame sequence item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FSItemEventMonitor"/>.</returns>
        internal FSItemEventMonitor RegisterFrameSequenceItem(FrameSequenceItem item)
        {
            var iRes = new FSItemEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>The register fe restriction group.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FERestrictionGroupEventMonitor"/>.</returns>
        internal FERestrictionGroupEventMonitor RegisterFERestrictionGroup(FERestrictionGroup item)
        {
            var iRes = new FERestrictionGroupEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>The register fe restriction segment.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FERestrictionSegmentEventMonitor"/>.</returns>
        internal FERestrictionSegmentEventMonitor RegisterFERestrictionSegment(FERestrictionSegment item)
        {
            var iRes = new FERestrictionSegmentEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        #endregion

        #region VFrames

        /// <summary>Registers the frame to monitor for changes  and returns the monitor to use.</summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The <see cref="VisualEventMonitor"/>.</returns>
        internal VisualEventMonitor RegisterVisualF(VisualFrame frame)
        {
            var iRes = new VisualEventMonitor(frame);
            fLock.EnterWriteLock();
            try
            {
                // AddToLinksOutDict(iRes, frame.Item.ID);
                AddToNeuronsDict(iRes, frame.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>Unregisters a frame.</summary>
        /// <param name="frame">The frame.</param>
        internal void UnRegisterVisualF(VisualFrame frame)
        {
            fLock.EnterWriteLock();
            try
            {
                // RemoveFromLinkOutDict(frame, frame.Item.ID);
                RemoveFromNeuronsDict(frame, frame.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region flow

        /// <summary>The register flow.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="FlowEventMonitor"/>.</returns>
        internal FlowEventMonitor RegisterFlow(Flow item)
        {
            var iRes = new FlowEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToLinksOutDict(iRes, item.ItemID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>The unregister flow.</summary>
        /// <param name="flow">The flow.</param>
        internal void UnregisterFlow(Flow flow)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromLinkOutDict(flow, flow.ItemID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region NeuronEditor

        /// <summary>Removes the NeuronEditor so we can start monitoring for changes that concern him.</summary>
        /// <param name="item">The item.</param>
        internal void UnRegisterNeuronEditor(NeuronEditor item)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromNeuronsDict(item, item.Item.ID);
                RemoveFromLinkOutDict(item, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Registers the NeuronEditor so we can start monitoring for changes that concern him.</summary>
        /// <param name="item">The item.</param>
        internal void RegisterNeuronEditor(NeuronEditor item)
        {
            var iRes = new NeuronEditorEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToNeuronsDict(iRes, item.Item.ID);
                AddToLinksOutDict(iRes, item.Item.ID);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #region MindMapCluster

        /// <summary>The register mind map cluter.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="MindMapClusterEventMonitor"/>.</returns>
        internal MindMapClusterEventMonitor RegisterMindMapCluter(MindMapCluster item)
        {
            var iRes = new MindMapClusterEventMonitor(item);
            fLock.EnterWriteLock();
            try
            {
                AddToNeuronsDict(iRes, item.Item.ID);
                AddToListDict(iRes, ((NeuronCluster)item.Item).ChildrenDirect);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>The unregister mind map cluter.</summary>
        /// <param name="item">The item.</param>
        internal void UnregisterMindMapCluter(MindMapCluster item)
        {
            fLock.EnterWriteLock();
            try
            {
                RemoveFromNeuronsDict(item, item.Item.ID);
                RemoveFromListsDict(item, ((NeuronCluster)item.Item).ChildrenIdentifier);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion

        #endregion

        #region Helpers

        /// <summary>Cleans out all unreferenced items.</summary>
        /// <param name="forCallBack">The for Call Back.</param>
        public void Clean(object forCallBack)
        {
            if (fLock.IsWriteLockHeld == false)
            {
                // make the function a bit more rentrent, so that it doesn't block if a previous call is still buzy.
                try
                {
                    fLock.EnterWriteLock();
                    try
                    {
                        CleanAnyNeuronMonitors();
                        CleanDict(fListMonitors);
                        CleanDict(fNeuronMonitors);
                        CleanDict(fLinkPartInMonitors);
                        CleanDict(fLinkPartOutMonitors);
                        CleanDict(fLinkMonitors);
                    }
                    finally
                    {
                        fLock.ExitWriteLock();
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("EventMonitor.Clean", e.ToString());
                }
            }
        }

        /// <summary>
        ///     Removes all monitors that are no longer referenced. this is not thread safe.
        /// </summary>
        private void CleanAnyNeuronMonitors()
        {
            var i = 0;
            while (i < fAnyNeuronMonitors.Count)
            {
                var iMon = fAnyNeuronMonitors[i];
                if (iMon.Reference.IsAlive == false)
                {
                    fAnyNeuronMonitors.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>The clean dict.</summary>
        /// <param name="dict">The dict.</param>
        /// <typeparam name="T"></typeparam>
        private void CleanDict<T>(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<EventMonitor>> dict)
        {
            var iKeys = new System.Collections.Generic.List<T>(dict.Keys);
            foreach (var iKey in iKeys)
            {
                var iList = dict[iKey];
                var i = 0;
                while (i < iList.Count)
                {
                    var iMon = iList[i];
                    if (iMon.Reference.IsAlive == false)
                    {
                        iList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                if (iList.Count == 0)
                {
                    dict.Remove(iKey);
                }
            }
        }

        /// <summary>Calls all the registered monitores from the dict for the specified id in a thread save manner.</summary>
        /// <remarks>Performs an automatic clean-up of all the invalid items for the specified id.</remarks>
        /// <typeparam name="T">The type of the id in the dict</typeparam>
        /// <param name="dict">The dict that contains all the registered monitors</param>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.IEnumerable ProcessDict<T>(System.Collections.Generic.Dictionary<T, System.Collections.Generic.List<EventMonitor>> dict, 
            T id)
        {
            var iToRemove = new System.Collections.Generic.List<EventMonitor>();
            System.Collections.Generic.List<EventMonitor> iFound;
            fLock.EnterReadLock();
            try
            {
                if (dict.TryGetValue(id, out iFound))
                {
                    iFound = new System.Collections.Generic.List<EventMonitor>(iFound);

                        // we make a local copy of the list so we can change it by other threads while looping through it.
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }

            if (iFound != null)
            {
                // we do the yield outside of the read lock so that the event handlers can update the dicts.
                foreach (var i in iFound)
                {
                    if (i.Reference.IsAlive)
                    {
                        yield return i;
                    }
                    else
                    {
                        iToRemove.Add(i);
                    }
                }
            }

            if (iToRemove.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    foreach (var i in iToRemove)
                    {
                        iFound.Remove(i);
                    }

                    if (iFound.Count == 0)
                    {
                        dict.Remove(id);
                    }
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>Calls all the registered monitores from the dict for the specified id in a thread save manner.
        ///     And while moving all the event handles from the old id to the new one. This is used to solve the 'temp' id problem:
        ///     they get registered with the temp, but once truely used, they are changed, so the dict also needs to move,
        ///     otherwise,
        ///     this list would grow and grow and point to invalid items.</summary>
        /// <remarks>Performs an automatic clean-up of all the invalid items for the specified id.</remarks>
        /// <typeparam name="T">The type of the id in the dict</typeparam>
        /// <param name="dict">The dict that contains all the registered monitors</param>
        /// <param name="id">The id.</param>
        /// <param name="newId">The new Id.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.IEnumerable ProcessDictWithMove(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.List<EventMonitor>> dict, 
            ulong id, 
            ulong newId)
        {
            var iToRemove = new System.Collections.Generic.List<EventMonitor>();
            System.Collections.Generic.List<EventMonitor> iFound;
            fLock.EnterReadLock();
            try
            {
                if (dict.TryGetValue(id, out iFound))
                {
                    iFound = new System.Collections.Generic.List<EventMonitor>(iFound);

                        // we make a local copy of the list so we can change it by other threads while looping through it.
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }

            if (iFound != null)
            {
                // we do the yield outside of the read lock so that the event handlers can update the dicts.
                foreach (var i in iFound)
                {
                    if (i.Reference.IsAlive)
                    {
                        if (i.WrapsID(newId))
                        {
                            iToRemove.Add(i);
                            yield return i;
                        }
                    }
                    else
                    {
                        iToRemove.Add(i);
                    }
                }
            }

            if (iToRemove.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    foreach (var i in iToRemove)
                    {
                        iFound.Remove(i);
                    }

                    if (iFound.Count == 0)
                    {
                        dict.Remove(id);
                    }
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>The process full neurons.</summary>
        /// <param name="e">The e.</param>
        private void ProcessFullNeurons(NeuronChangedEventArgs e)
        {
            System.Collections.Generic.List<EventMonitor> iTemp;
            var iToRemove = new System.Collections.Generic.List<EventMonitor>();
            fLock.EnterReadLock();
            try
            {
                iTemp = new System.Collections.Generic.List<EventMonitor>(fAnyNeuronMonitors);

                    // we make a local copy to make the event call thread safe without blocking changes to the lists.
            }
            finally
            {
                fLock.ExitReadLock();
            }

            foreach (var i in iTemp)
            {
                if (i.Reference.IsAlive)
                {
                    i.NeuronChanged(e);
                }
                else
                {
                    iToRemove.Add(i);
                }
            }

            if (iToRemove.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    foreach (var i in iToRemove)
                    {
                        fAnyNeuronMonitors.Remove(i);
                    }
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>Removes the event monitor from th dict.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        internal void RemoveNeuronChangedMonitor(EventMonitor item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            fLock.EnterWriteLock();
            try
            {
                if (fNeuronMonitors.TryGetValue(id, out iFound))
                {
                    iFound.Remove(item);
                    if (iFound.Count == 0)
                    {
                        fNeuronMonitors.Remove(id);
                    }
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Removes the event monitor from th dict.</summary>
        /// <param name="item">The item.</param>
        /// <param name="ids">The ids.</param>
        internal void RemoveNeuronChangedMonitor(EventMonitor item, System.Collections.Generic.IEnumerable<ulong> ids)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            fLock.EnterWriteLock();
            try
            {
                foreach (var i in ids)
                {
                    if (fNeuronMonitors.TryGetValue(i, out iFound))
                    {
                        iFound.Remove(item);
                        if (iFound.Count == 0)
                        {
                            fNeuronMonitors.Remove(i);
                        }
                    }
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>The add neuron changed monitor.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        internal void AddNeuronChangedMonitor(EventMonitor item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            fLock.EnterWriteLock();
            try
            {
                if (fNeuronMonitors.TryGetValue(id, out iFound) == false)
                {
                    iFound = new System.Collections.Generic.List<EventMonitor>();
                    fNeuronMonitors.Add(id, iFound);
                }

                if (iFound.Contains(item) == false)
                {
                    // make certain it isn't registered 2 times.
                    iFound.Add(item);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Adds an eventMonitor to Neuorn-change dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void AddToNeuronsDict(EventMonitor item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fNeuronMonitors.TryGetValue(id, out iFound) == false)
            {
                iFound = new System.Collections.Generic.List<EventMonitor>();
                fNeuronMonitors.Add(id, iFound);
            }

            if (iFound.Contains(item) == false)
            {
                // make certain it isn't registered 2 times.
                iFound.Add(item);
            }
        }

        /// <summary>Removes the specified wrapper object from the NeuronsDict. This is not a thread safe function.</summary>
        /// <param name="toRemove">To remove.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromNeuronsDict(object toRemove, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fNeuronMonitors.TryGetValue(id, out iFound))
            {
                // add for the children list.
                var iEventMons =
                    (from i in iFound where i.Reference.IsAlive == false || i.Reference.Target == toRemove select i)
                        .ToList();
                foreach (var i in iEventMons)
                {
                    // should always be 1, but just in case.
                    iFound.Remove(i);
                }

                if (iFound.Count == 0)
                {
                    fNeuronMonitors.Remove(id);
                }
            }
        }

        /// <summary>Adds an eventMonitor to lists-change dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void AddToListDict(EventMonitor item, object id)
        {
            System.Collections.Generic.List<EventMonitor> iFound = null;
            if (id != null && fListMonitors.TryGetValue(id, out iFound) == false)
            {
                // add for the children list.
                iFound = new System.Collections.Generic.List<EventMonitor>();
                fListMonitors.Add(id, iFound);
            }

            if (iFound != null)
            {
                iFound.Add(item);
            }
        }

        /// <summary>The remove from lists dict.</summary>
        /// <param name="toRemove">The to remove.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromListsDict(object toRemove, object id)
        {
            System.Diagnostics.Debug.Assert(id != null);
            System.Collections.Generic.List<EventMonitor> iFound;
            if (id != null && fListMonitors.TryGetValue(id, out iFound))
            {
                var iEventMons =
                    (from i in iFound where i.Reference.IsAlive == false || i.Reference.Target == toRemove select i)
                        .ToList();
                foreach (var i in iEventMons)
                {
                    // should always be 1, but just in case.
                    iFound.Remove(i);
                }

                if (iFound.Count == 0)
                {
                    fListMonitors.Remove(id);
                }
            }
        }

        /// <summary>The remove from lists dict.</summary>
        /// <param name="toRemove">The to remove.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromListsDict(EventMonitor toRemove, object id)
        {
            System.Diagnostics.Debug.Assert(id != null);
            System.Collections.Generic.List<EventMonitor> iFound;
            if (id != null && fListMonitors.TryGetValue(id, out iFound))
            {
                var iEventMons =
                    (from i in iFound where i.Reference.IsAlive == false || i == toRemove select i).ToList();
                foreach (var i in iEventMons)
                {
                    // should always be 1, but just in case.
                    iFound.Remove(i);
                }

                if (iFound.Count == 0)
                {
                    fListMonitors.Remove(id);
                }
            }
        }

        /// <summary>Adds an event monitor to the Links changed dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void AddToLinksDict(EventMonitor item, Link id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkMonitors.TryGetValue(id, out iFound) == false)
            {
                // add for the children list.
                iFound = new System.Collections.Generic.List<EventMonitor>();
                fLinkMonitors.Add(id, iFound);
            }

            iFound.Add(item);
        }

        /// <summary>Removes an event monitor fro the Links changed dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromLinksDict(EventMonitor item, Link id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkMonitors.TryGetValue(id, out iFound))
            {
                var iEventMons = (from i in iFound where i.Reference.IsAlive == false || i == item select i).ToList();
                foreach (var i in iEventMons)
                {
                    // should always be 1, but just in case.
                    iFound.Remove(i);
                }

                if (iFound == null || iFound.Count == 0)
                {
                    fLinkMonitors.Remove(id);
                }
            }
        }

        /// <summary>Adds an eventMonitor to LinksIn-change dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void AddToLinksInDict(EventMonitor item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkPartInMonitors.TryGetValue(id, out iFound) == false)
            {
                // add for the children list.
                iFound = new System.Collections.Generic.List<EventMonitor>();
                fLinkPartInMonitors.Add(id, iFound);
            }

            iFound.Add(item);
        }

        /// <summary>The remove from link in dict.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromLinkInDict(object item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkPartInMonitors.TryGetValue(id, out iFound))
            {
                // add for the children list.
                var iEventMons =
                    (from i in iFound where i.Reference.IsAlive == false || i.Reference.Target == item select i).ToList(
                        );
                foreach (var i in iEventMons)
                {
                    // should always be 1, but just in case.
                    iFound.Remove(i);
                }

                if (iFound.Count == 0)
                {
                    fLinkPartInMonitors.Remove(id);
                }
            }
        }

        /// <summary>Adds an eventMonitor to LinksOut-change dict, in a thread unsafe manner.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void AddToLinksOutDict(EventMonitor item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkPartOutMonitors.TryGetValue(id, out iFound) == false)
            {
                // add for the children list.
                iFound = new System.Collections.Generic.List<EventMonitor>();
                fLinkPartOutMonitors.Add(id, iFound);
            }

            iFound.Add(item);
        }

        /// <summary>The remove from link out dict.</summary>
        /// <param name="item">The item.</param>
        /// <param name="id">The id.</param>
        private void RemoveFromLinkOutDict(object item, ulong id)
        {
            System.Collections.Generic.List<EventMonitor> iFound;
            if (fLinkPartOutMonitors.TryGetValue(id, out iFound))
            {
                // add for the children list.
                if (iFound != null)
                {
                    // should not happen, but just did, so check.
                    var iEventMons =
                        (from i in iFound where i.Reference.IsAlive == false || i.Reference.Target == item select i)
                            .ToList();
                    foreach (var i in iEventMons)
                    {
                        // should always be 1, but just in case.
                        iFound.Remove(i);
                    }
                }

                if (iFound == null || iFound.Count == 0)
                {
                    fLinkPartOutMonitors.Remove(id);
                }
            }
        }

        #endregion

        #region Event handlers

        /// <summary>Handles the NeuronChanged event of the Current control.</summary>
        /// <remarks>When neurons get removed, the monitor lists are cleaned out automatically for the specific operation.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.NeuronChangedEventArgs"/> instance containing the event data.</param>
        private void Current_NeuronChanged(object sender, NeuronChangedEventArgs e)
        {
            ProcessFullNeurons(e);
            if (e.Action == BrainAction.Created && e.OriginalSourceID == Neuron.TempId)
            {
                foreach (EventMonitor i in ProcessDictWithMove(fNeuronMonitors, e.OriginalSourceID, e.NewValue.ID))
                {
                    i.NeuronChanged(e);
                }
            }
            else
            {
                foreach (EventMonitor i in ProcessDict(fNeuronMonitors, e.OriginalSourceID))
                {
                    i.NeuronChanged(e);
                }
            }

            if (e.Action == BrainAction.Removed)
            {
                // if the neuron is removed, we can also remove all the monitors for the neuron and possibly any lists as well. This is good way to keep things cleaned
                fLock.EnterWriteLock();
                try
                {
                    fNeuronMonitors.Remove(e.OriginalSourceID);
                    if (e.OriginalSource.ClusteredByIdentifier != null)
                    {
                        fListMonitors.Remove(e.OriginalSource.ClusteredByIdentifier);
                    }

                    var iCluster = e.OriginalSource as NeuronCluster;
                    if (iCluster != null && iCluster.ChildrenIdentifier != null)
                    {
                        fListMonitors.Remove(iCluster.ChildrenIdentifier);
                    }
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }

            ProcessorManager.Current.AtttachedDict.NeuronChanged(e);
            ProjectManager.Default.ProjectChanged = true;
        }

        /// <summary>The brain_ neuron list changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Brain_NeuronListChanged(object sender, NeuronListChangedEventArgs e)
        {
            foreach (EventMonitor i in ProcessDict(fListMonitors, e.Identifier))
            {
                i.ListChanged(e);
            }

            ProcessorManager.Current.AtttachedDict.ListChanged(e);
        }

        /// <summary>Handles the Cleared event of the Brain.</summary>
        /// <remarks>When the brain is cleared, we also clear all monitor lists, cause all items have become invalid anyway.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Brain_Cleared(object sender, System.EventArgs e)
        {
            fLock.EnterWriteLock();
            try
            {
                fLinkPartInMonitors.Clear();
                fLinkPartOutMonitors.Clear();
                fNeuronMonitors.Clear();
                fListMonitors.Clear();
                fLinkMonitors.Clear();
                fAnyNeuronMonitors.Clear();
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            ProcessorManager.Current.AtttachedDict.NetworkCleared();
        }

        /// <summary>The brain_ link changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Brain_LinkChanged(object sender, LinkChangedEventArgs e)
        {
            foreach (EventMonitor i in ProcessDict(fLinkMonitors, e.OriginalSource))
            {
                i.LinkChanged(e);
            }

            switch (e.Action)
            {
                case BrainAction.Created:
                    foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.NewFrom))
                    {
                        i.FromLinkCreated(e.OriginalSource);
                    }

                    foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.NewTo))
                    {
                        i.ToLinkCreated(e.OriginalSource);
                    }

                    ProcessorManager.Current.AtttachedDict.LinkCreated(e);
                    break;
                case BrainAction.Changed:
                    if (e.NewTo != e.OldTo)
                    {
                        foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.OldTo))
                        {
                            i.ToLinkRemoved(e.OriginalSource);
                        }

                        foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.NewTo))
                        {
                            i.ToLinkCreated(e.OriginalSource);
                        }

                        foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.OldFrom))
                        {
                            i.ToChanged(e.OriginalSource, e.OldTo);
                        }
                    }
                    else if (e.NewFrom != e.OldFrom)
                    {
                        foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.OldFrom))
                        {
                            i.FromLinkRemoved(e.OriginalSource, e.OldFrom);
                        }

                        foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.NewFrom))
                        {
                            i.FromLinkCreated(e.OriginalSource);
                        }

                        foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.OldTo))
                        {
                            i.FromChanged(e.OriginalSource, e.OldFrom);
                        }
                    }
                    else
                    {
                        if (e.NewMeaning != e.OldMeaning)
                        {
                            foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.OldFrom))
                            {
                                i.FromMeaningChanged(e.OriginalSource, e.OldMeaning);
                            }

                            foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.OldTo))
                            {
                                i.ToMeaningChanged(e.OriginalSource, e.OldMeaning);
                            }
                        }

                        foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.OldTo))
                        {
                            // think this is wrong, not needed.
                            i.FromChanged(e.OriginalSource, e.OldFrom);
                        }
                    }

                    ProcessorManager.Current.AtttachedDict.LinkChanged(e);
                    break;
                case BrainAction.Removed:
                    foreach (EventMonitor i in ProcessDict(fLinkPartOutMonitors, e.OldFrom))
                    {
                        i.FromLinkRemoved(e.OriginalSource, e.OldTo);
                    }

                    foreach (EventMonitor i in ProcessDict(fLinkPartInMonitors, e.OldTo))
                    {
                        i.ToLinkRemoved(e.OriginalSource);
                    }

                    fLock.EnterWriteLock();
                    try
                    {
                        fLinkMonitors.Remove(e.OriginalSource);
                        if (e.OriginalSource.InfoIdentifier != null)
                        {
                            fListMonitors.Remove(e.OriginalSource.InfoIdentifier);
                        }
                    }
                    finally
                    {
                        fLock.ExitWriteLock();
                    }

                    ProcessorManager.Current.AtttachedDict.LinkRemoved(e);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #endregion
    }
}