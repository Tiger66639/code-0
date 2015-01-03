// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDeleter.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that can be implemented by objects for
//   doing a special type of remove from the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>an <see langword="interface"/> that can be implemented by objects for
    ///     doing a special type of remove from the network.</summary>
    /// <typeparam name="U"></typeparam>
    public interface INeuronRemoveable<U>
        where U : INeuronWrapper
    {
        /// <summary>The remove child.</summary>
        /// <param name="child">The child.</param>
        void RemoveChild(U child);
    }

    /// <summary>The base neuron deleter.</summary>
    internal abstract class BaseNeuronDeleter : ProjectOperation
    {
        #region internal types

        /// <summary>
        ///     a wrapper for branch neurons that weren't deleted because they still
        ///     had refs or meaningCount. This could have changed, so it needs to be
        ///     <see langword="checked" /> again.
        /// </summary>
        internal class BranchDeletionRecord
        {
            /// <summary>Gets or sets the ref checker.</summary>
            public System.Func<Neuron, Neuron, bool> RefChecker { get; set; }

            /// <summary>Gets or sets the item.</summary>
            public Neuron Item { get; set; }

            /// <summary>Gets or sets the branch owner.</summary>
            public Neuron BranchOwner { get; set; }
        }

        #endregion

        #region Fields

        /// <summary>
        ///     All the branches that are still candidates for deletion, if they no
        ///     longer have any refs (which might have changed when all items are
        ///     processed).
        /// </summary>
        protected System.Collections.Generic.List<BranchDeletionRecord> fBranchesToTryAgain =
            new System.Collections.Generic.List<BranchDeletionRecord>();

        /// <summary>
        ///     Stores all the neurons that have already been handled. This is to
        ///     prevent as getting in an ethernal loop because of circular references
        ///     in the neurons.
        /// </summary>
        protected System.Collections.Generic.HashSet<Neuron> fAlreadyProcessed =
            new System.Collections.Generic.HashSet<Neuron>();

        #endregion

        #region Prop

        /// <summary>
        ///     Gets or sets the deletion method that should be applied to the root.
        /// </summary>
        /// <value>
        ///     The neuron deletion method.
        /// </value>
        public DeletionMethod NeuronDeletionMethod { get; set; }

        /// <summary>
        ///     Gets or sets the deletion method that should be applied to the
        ///     children of the root (links out and children of clusters).
        /// </summary>
        /// <value>
        ///     The branch handling.
        /// </value>
        public DeletionMethod BranchHandling { get; set; }

        #endregion

        #region Functions

        /// <summary>The process roots again.</summary>
        protected abstract void ProcessRootsAgain();

        /// <summary>The process branches again.</summary>
        protected void ProcessBranchesAgain()
        {
            System.Collections.Generic.List<BranchDeletionRecord> iBranches;
            var iContinue = true;
            while (iContinue)
            {
                fAlreadyProcessed.Clear(); // all items can be processed again.
                iContinue = false;
                iBranches = fBranchesToTryAgain;
                fBranchesToTryAgain = new System.Collections.Generic.List<BranchDeletionRecord>();
                foreach (var i in iBranches)
                {
                    if (i.Item.ID != Neuron.EmptyId)
                    {
                        // skip if already deleted.
                        ProcessBranchItem(i.Item, i.BranchOwner, i.RefChecker);
                    }
                }

                ProcessRootsAgain();

                    // after we have done all the branches again, we try to do the roots again, perhaps something has changed, which might trigger new branch changes.
                if (iBranches.Count == fBranchesToTryAgain.Count)
                {
                    // only do a deep check for change if the count is the same, otherwise we know something changed, and we need to go again.
                    foreach (var i in fBranchesToTryAgain)
                    {
                        // we need to check if there was a new item added or if we need to process all the same.
                        var iFound =
                            (from u in iBranches where u.Item == i.Item && u.BranchOwner == i.BranchOwner select u)
                                .FirstOrDefault();
                        if (iFound == null)
                        {
                            iContinue = true;
                        }
                    }
                }
                else
                {
                    iContinue = fBranchesToTryAgain.Count != 0;
                }
            }
        }

        /// <summary>The handle branch.</summary>
        /// <param name="root">The root.</param>
        protected void HandleBranch(Neuron root)
        {
            if (BranchHandling != DeletionMethod.Nothing)
            {
                // if we don't need to handle the branch, don't spend time running through it.
                ProcessLinksOut(root);
                var iToDelete = root as NeuronCluster;
                if (iToDelete != null)
                {
                    ProcessChildren(iToDelete);
                }
            }
        }

        /// <summary>The process links out.</summary>
        /// <param name="neuron">The neuron.</param>
        protected void ProcessLinksOut(Neuron neuron)
        {
            var iLinksOut = Factories.Default.NLists.GetBuffer();
            try
            {
                using (var iLinks = neuron.LinksOut)
                    iLinksOut.AddRange(from i in iLinks select i.To);

                        // we make a copy cause we are going to delete items, if we don't make a copy, we would get an error that we are trying to modify the list inside a foreach.
                foreach (var iTo in iLinksOut)
                {
                    ProcessBranchItem(iTo, neuron, HasRefsFromLink);
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iLinksOut);
            }
        }

        /// <summary>Processes all the children of a branch item.</summary>
        /// <param name="toProcess">To process.</param>
        protected void ProcessChildren(NeuronCluster toProcess)
        {
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = toProcess.Children) iChildren = iList.ConvertTo<Neuron>();
            foreach (var i in iChildren)
            {
                ProcessBranchItem(i, toProcess, HasRefsFromCluster);
            }
        }

        /// <summary>Processes a single branch item.</summary>
        /// <param name="i">The i.</param>
        /// <param name="branchOwner">The branch Owner.</param>
        /// <param name="hasRefChecker">The has <see langword="ref"/> checker.</param>
        protected void ProcessBranchItem(Neuron i, Neuron branchOwner, System.Func<Neuron, Neuron, bool> hasRefChecker)
        {
            if (fAlreadyProcessed.Contains(i) == false)
            {
                fAlreadyProcessed.Add(i);
                if (BrainData.Current.NeuronInfo[i.ID].IsLocked == false && i.ModuleRefCount == 0)
                {
                    // if the item is referecend by a module, can't delete it or it's children.
                    if (BranchHandling == DeletionMethod.DeleteIfNoRef)
                    {
                        if (hasRefChecker(i, branchOwner) == false)
                        {
                            HandleBranch(i);
                            WindowMain.DeleteItemFromBrain(i);
                        }
                        else if (i.ID >= (ulong)PredefinedNeurons.Dynamic)
                        {
                            // we don't try to process statics any further, they will always fail anyway.
                            var iNew = new BranchDeletionRecord
                                           {
                                               Item = i, 
                                               RefChecker = hasRefChecker, 
                                               BranchOwner = branchOwner
                                           };
                            fBranchesToTryAgain.Add(iNew);
                        }
                    }
                    else if (BranchHandling == DeletionMethod.Delete)
                    {
                        if (i.CanBeDeleted)
                        {
                            HandleBranch(i);
                            WindowMain.DeleteItemFromBrain(i);
                        }
                        else
                        {
                            var iNew = new BranchDeletionRecord
                                           {
                                               Item = i, 
                                               RefChecker = hasRefChecker, 
                                               BranchOwner = branchOwner
                                           };

                            // if the item can't yet be deleted because there is a meaning or info refcount, then we need to try again cause deletions after this might change the situation.
                            fBranchesToTryAgain.Add(iNew);
                        }
                    }
                }
            }
        }

        /// <summary>Determines whether the item has any incomming links or is still
        ///     clustered by more than 1 item.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="owner">The owner.</param>
        /// <returns><c>true</c> if [has refs from cluster] [the specified to check];
        ///     otherwise, <c>false</c> .</returns>
        protected bool HasRefsFromCluster(Neuron toCheck, Neuron owner)
        {
            var iDelete = toCheck.CanBeDeleted;
            if (iDelete)
            {
                if (toCheck.LinksInIdentifier != null)
                {
                    ListAccessor<Link> iLinksIn = toCheck.LinksIn;
                    iLinksIn.Lock();
                    try
                    {
                        iDelete = CheckHasClusteredBy(toCheck, owner, iLinksIn);
                    }
                    finally
                    {
                        iLinksIn.Unlock();
                        iLinksIn.Dispose();
                    }
                }
                else
                {
                    iDelete = CheckHasClusteredBy(toCheck, owner, null);
                }
            }

            return !iDelete;
        }

        /// <summary>The check has clustered by.</summary>
        /// <param name="toCheck">The to check.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="iLinksIn">The i links in.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckHasClusteredBy(Neuron toCheck, Neuron owner, ListAccessor<Link> iLinksIn)
        {
            var iDelete = true;
            if (toCheck.ClusteredByIdentifier != null)
            {
                ListAccessor<ulong> iClusteredBy = toCheck.ClusteredBy;
                iClusteredBy.Lock();

                    // should be in a single lock, otherwise it's possible to have deadlocks. But this should be very rare.
                try
                {
                    var iCount = iClusteredBy.CountUnsafe;
                    if (iCount == 0 || (iCount == 1 && (owner == null || iClusteredBy.GetUnsafe(0) == owner.ID)))
                    {
                        if (iLinksIn != null)
                        {
                            iDelete = iLinksIn.CountUnsafe == 0;
                        }
                    }
                    else
                    {
                        iDelete = false; // clusterByCount = 1 cause we are still clustered by the owner.
                    }
                }
                finally
                {
                    iClusteredBy.Unlock();
                    iClusteredBy.Dispose();
                }
            }

            return iDelete;
        }

        /// <summary>Determines whether the item to check has any refs left, other then an
        ///     incomming link.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="otherSide">The other Side.</param>
        /// <returns><c>true</c> if [has refs from link] [the specified to check];
        ///     otherwise, <c>false</c> .</returns>
        protected bool HasRefsFromLink(Neuron toCheck, Neuron otherSide)
        {
            var iDelete = toCheck.CanBeDeleted;
            if (iDelete)
            {
                ListAccessor<Link> iLinksIn = toCheck.LinksIn;
                iLinksIn.Lock();
                try
                {
                    ListAccessor<ulong> iClusteredBy = toCheck.ClusteredBy;
                    iClusteredBy.Lock();

                        // should be in a single lock, otherwise it's possible to have deadlocks. But this should be very rare.
                    try
                    {
                        var iCount = iLinksIn.CountUnsafe;
                        if (iCount == 0 || (iCount == 1 && iLinksIn.GetUnsafe(0).FromID == otherSide.ID))
                        {
                            iDelete = iClusteredBy.CountUnsafe == 0;
                        }
                        else
                        {
                            iDelete = false;
                        }
                    }
                    finally
                    {
                        iClusteredBy.Unlock();
                        iClusteredBy.Dispose();
                    }
                }
                finally
                {
                    iLinksIn.Unlock();
                    iLinksIn.Dispose();
                }
            }

            return !iDelete;
        }

        /// <summary>The perform remove.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toRemove">The to remove.</param>
        /// <param name="index">The index.</param>
        protected void PerformRemove(NeuronCluster owner, Neuron toRemove, int index)
        {
            if (owner != null)
            {
                var iUndo = new RemoveClusterUndoItem();
                iUndo.Cluster = owner;
                iUndo.Items = new System.Collections.Generic.List<Neuron> { toRemove };
                iUndo.Index = index;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                using (var iChildren = owner.ChildrenW) iChildren.RemoveAt(index);
            }
        }

        #endregion
    }

    /// <summary>
    ///     A general purpose deleter (no type specific support). Warning: doesn't
    ///     delete any link-Info data.
    /// </summary>
    internal class NeuronDeleter : BaseNeuronDeleter
    {
        #region Fields

        /// <summary>The f roots.</summary>
        private System.Collections.Generic.List<Neuron> fRoots = new System.Collections.Generic.List<Neuron>();

        #endregion

        /// <summary>Deletes the specified neuron according to the specified deletion
        ///     rules.</summary>
        /// <param name="toDelete">To delete.</param>
        public void Start(Neuron toDelete)
        {
            var iToDel = new System.Collections.Generic.List<Neuron>();
            iToDel.Add(toDelete);
            Start(iToDel);
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="NeuronDeleter" /> class.
        /// </summary>
        public NeuronDeleter()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronDeleter"/> class.</summary>
        /// <param name="rootDelMethod">The root deletion method.</param>
        /// <param name="branchDelMethod">The branch deletion method.</param>
        public NeuronDeleter(DeletionMethod rootDelMethod, DeletionMethod branchDelMethod)
        {
            BranchHandling = branchDelMethod;
            NeuronDeletionMethod = rootDelMethod;
        }

        #endregion

        #region Functions

        /// <summary>The process roots again.</summary>
        protected override void ProcessRootsAgain()
        {
            if (fRoots.Count > 0)
            {
                System.Collections.Generic.List<Neuron> iRoots;
                do
                {
                    fAlreadyProcessed.Clear(); // all items can be processed again.
                    iRoots = fRoots;
                    fRoots = new System.Collections.Generic.List<Neuron>();
                    ProcessList(iRoots);
                }
                while (iRoots.Count != fRoots.Count);
            }
        }

        /// <summary>Deletes the specified list of neurons in 1 operation.</summary>
        /// <param name="toDelete">To delete.</param>
        public void Start(System.Collections.Generic.IEnumerable<Neuron> toDelete)
        {
            ProcessList(toDelete);
            ProcessRootsAgain();
            ProcessBranchesAgain();
        }

        /// <summary>The process list.</summary>
        /// <param name="toDelete">The to delete.</param>
        private void ProcessList(System.Collections.Generic.IEnumerable<Neuron> toDelete)
        {
            foreach (var i in toDelete)
            {
                fAlreadyProcessed.Add(i);

                    // root items are always processed, so we add to the alreadyProcessed list for the branchItems, but in this routine, we don't check if the item is already processed or not (it won't create a circular loop).
                if (BrainData.Current.NeuronInfo[i.ID].IsLocked == false && i.ModuleRefCount == 0)
                {
                    if (i.ID != Neuron.EmptyId)
                    {
                        // don't try to delete it if it is already deleted.
                        switch (NeuronDeletionMethod)
                        {
                            case DeletionMethod.Nothing:
                                break;
                            case DeletionMethod.Remove:

                                // this doesn't do anything as well cause we don't know from where to remove the root item.
                                break;
                            case DeletionMethod.DeleteIfNoRef:
                                if (HasRefsFromCluster(i, null))
                                {
                                    if (i.ID >= (ulong)PredefinedNeurons.Dynamic)
                                    {
                                        // root items that are static aren't tried again.
                                        fRoots.Add(i);
                                    }
                                }
                                else
                                {
                                    HandleBranch(i);
                                    WindowMain.DeleteItemFromBrain(i);
                                }

                                break;
                            case DeletionMethod.Delete:
                                if (i.CanBeDeleted)
                                {
                                    HandleBranch(i);
                                    WindowMain.DeleteItemFromBrain(i);
                                }
                                else
                                {
                                    fRoots.Add(i);
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>A helper class that provides advanced deletion with type specific
    ///     support. 'No ref' is interpreted as 'no parent or incomming links. this
    ///     is useful for code mostly, but not for asset data. Use the<see cref="AsetDataDeleter"/> for this purpose. Warning: doesn't delete
    ///     any link-Info data.</summary>
    /// <typeparam name="T"></typeparam>
    internal class NeuronDeleter<T> : BaseNeuronDeleter
        where T : INeuronWrapper
    {
        #region Fields

        /// <summary>
        ///     this list contains all the records of items that should be
        ///     <see langword="checked" /> next time, to see if they still have any
        ///     links or clusters that prevent the deletion.
        /// </summary>
        private System.Collections.Generic.List<RootDeletionRecord> fToTryNext =
            new System.Collections.Generic.List<RootDeletionRecord>();

        #endregion

        #region internal types

        /// <summary>
        ///     Allows the grouping of several items that have different pareents
        ///     together into 1 delete operation.
        /// </summary>
        public class DeleteRequestRecord
        {
            /// <summary>
            ///     Gets or sets the item that needs to be deleted or removed from the
            ///     ownign list.
            /// </summary>
            /// <value>
            ///     The item.
            /// </value>
            public T Item { get; set; }

            /// <summary>
            ///     Gets or sets the collection that owns the item that needs to be
            ///     deleted. This allows us to only remove the item from the
            ///     appropriate list, if this is the required operation.
            /// </summary>
            /// <value>
            ///     The owner.
            /// </value>
            public ClusterCollection<T> Owner { get; set; }

            /// <summary>
            ///     Gets or sets the other side of the link, when a code item needs to
            ///     be removed that was reffed through a link and not a parent - child
            ///     relationship.
            /// </summary>
            /// <value>
            ///     The other side.
            /// </value>
            public T OtherSide { get; set; }
        }

        /// <summary>The root deletion record.</summary>
        private class RootDeletionRecord
        {
            /// <summary>Gets or sets the cluster.</summary>
            public ClusterCollection<T> Cluster { get; set; }

            /// <summary>Gets or sets the other side.</summary>
            public T OtherSide { get; set; }

            /// <summary>Gets or sets the item.</summary>
            public T Item { get; set; }

            /// <summary>Gets or sets the delete method.</summary>
            public DeletionMethod DeleteMethod { get; set; }
        }

        #endregion

        #region Functions

        /// <summary>Starts the deletion procedure for the specified neuron wrapper, using
        ///     the specified owner. The <paramref name="owner"/> is required for in
        ///     case a remove is needed, which is usually from only 1 specific owner.
        ///     use this if there is only 1 item to delete.</summary>
        /// <param name="owner">The owner to remove the item from (as child).</param>
        /// <param name="toDelete">The neuron to delete (or remove from the owner).</param>
        public void Start(ClusterCollection<T> owner, T toDelete)
        {
            var iToProcess = new DeleteRequestRecord();
            iToProcess.Owner = owner;
            iToProcess.Item = toDelete;
            ProcessDeleteRequest(iToProcess);

            ProcessRootsAgain();
            ProcessBranchesAgain();
        }

        /// <summary>Starts the deletion procedure for the specified neuron wrapper, using
        ///     the specified owner. The <paramref name="owner"/> is required for in
        ///     case a remove is needed, which is usually from only 1 specific owner.
        ///     use this if there are multiple items to delete. Note: all the items
        ///     that need to be deleted, need to be in the specified cluster
        ///     collection.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toDelete">To delete.</param>
        public void Start(ClusterCollection<T> owner, System.Collections.Generic.IList<T> toDelete)
        {
            var iToProcess = new DeleteRequestRecord();
            iToProcess.Owner = owner;
            foreach (var i in toDelete)
            {
                if (i.Item.ID != Neuron.EmptyId)
                {
                    // could already be deleted.
                    iToProcess.Item = i;
                    ProcessDeleteRequest(iToProcess);
                }
            }

            ProcessRootsAgain();
            ProcessBranchesAgain();
        }

        /// <summary>Starts the deletion operation for multiple items, which can be in
        ///     multiple owners. the delete is done in 1 operation.</summary>
        /// <param name="toDelete">To delete.</param>
        public void Start(System.Collections.Generic.IEnumerable<DeleteRequestRecord> toDelete)
        {
            foreach (var i in toDelete)
            {
                if (i.Item.Item.ID != Neuron.EmptyId)
                {
                    ProcessDeleteRequest(i);
                }
            }

            ProcessRootsAgain();
            ProcessBranchesAgain();
        }

        /// <summary>The process delete request.</summary>
        /// <param name="i">The i.</param>
        private void ProcessDeleteRequest(DeleteRequestRecord i)
        {
            fAlreadyProcessed.Add(i.Item.Item);

                // root items are always processed, so we add to the alreadyProcessed list for the branchItems, but in this routine, we don't check if the item is already processed or not (it won't create a circular loop).
            switch (NeuronDeletionMethod)
            {
                case DeletionMethod.Nothing:
                    break;
                case DeletionMethod.Remove:
                    HandleBranch(i.Item.Item);
                    if (i.Owner != null)
                    {
                        PerformRemove(i.Owner.Cluster, i.Item.Item, i.Owner.IndexOf(i.Item));
                    }
                    else
                    {
                        PerformRefRemove(i);
                    }

                    break;
                case DeletionMethod.DeleteIfNoRef:
                    if (i.Owner != null)
                    {
                        RemoveOrDeleteFromCluster(i);
                    }
                    else
                    {
                        RemoveOrDeleteFromRef(i);
                    }

                    break;
                case DeletionMethod.Delete:
                    TryDeleteItemFromBrain(i.Owner, i.Item);
                    break;
                default:
                    break;
            }
        }

        /// <summary>The remove or delete from ref.</summary>
        /// <param name="i">The i.</param>
        private void RemoveOrDeleteFromRef(DeleteRequestRecord i)
        {
            if (HasRefsFromLink(i.Item.Item, i.OtherSide.Item))
            {
                PerformRefRemove(i);
                var iDel = new RootDeletionRecord
                               {
                                   OtherSide = i.OtherSide, 
                                   Item = i.Item, 
                                   DeleteMethod = DeletionMethod.DeleteIfNoRef
                               };
                fToTryNext.Add(iDel);
            }
            else
            {
                TryDeleteItemFromBrain(i.OtherSide, i.Item);
            }
        }

        /// <summary>The perform ref remove.</summary>
        /// <param name="i">The i.</param>
        private void PerformRefRemove(DeleteRequestRecord i)
        {
            var iRemoveable = i.OtherSide as INeuronRemoveable<T>;
            System.Diagnostics.Debug.Assert(iRemoveable != null);
            iRemoveable.RemoveChild(i.Item);
        }

        /// <summary>The remove or delete from cluster.</summary>
        /// <param name="i">The i.</param>
        private void RemoveOrDeleteFromCluster(DeleteRequestRecord i)
        {
            var iCluster = i.Owner.Cluster;
            if (HasRefsFromCluster(i.Item.Item, iCluster))
            {
                var iIndex = i.Owner.IndexOf(i.Item);
                PerformRemove(iCluster, i.Item.Item, iIndex);
                var iDel = new RootDeletionRecord
                               {
                                   Cluster = i.Owner, 
                                   Item = i.Item, 
                                   DeleteMethod = DeletionMethod.DeleteIfNoRef
                               };
                fToTryNext.Add(iDel);
            }
            else
            {
                TryDeleteItemFromBrain(i.Owner, i.Item);
            }
        }

        /// <summary>The try delete item from brain.</summary>
        /// <param name="otherSide">The other side.</param>
        /// <param name="toDelete">The to delete.</param>
        private void TryDeleteItemFromBrain(T otherSide, T toDelete)
        {
            if (toDelete.Item.CanBeDeleted)
            {
                HandleBranch(toDelete.Item);
                if (BrainData.Current.NeuronInfo[toDelete.Item.ID].IsLocked == false)
                {
                    WindowMain.DeleteItemFromBrain(toDelete.Item);
                }
                else if (otherSide != null)
                {
                    var iRemoveable = otherSide as INeuronRemoveable<T>;
                    System.Diagnostics.Debug.Assert(iRemoveable != null);
                    iRemoveable.RemoveChild(toDelete);

                        // locked in editor, so don't delete and don't try again, this will not change anyway.
                }
            }
            else
            {
                var iRemoveable = otherSide as INeuronRemoveable<T>;
                System.Diagnostics.Debug.Assert(iRemoveable != null);
                iRemoveable.RemoveChild(toDelete);
                if (BrainData.Current.NeuronInfo[toDelete.Item.ID].IsLocked == false)
                {
                    var iDel = new RootDeletionRecord
                                   {
                                       OtherSide = otherSide, 
                                       Item = toDelete, 
                                       DeleteMethod = DeletionMethod.Delete
                                   };
                    fToTryNext.Add(iDel);

                        // it's locked because of meaning usage, this can change as other objects are deleted, so check later on again.
                }
            }
        }

        /// <summary>The try delete item from brain.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toDelete">The to delete.</param>
        private void TryDeleteItemFromBrain(ClusterCollection<T> owner, T toDelete)
        {
            if (toDelete.Item.CanBeDeleted)
            {
                if (BrainData.Current.NeuronInfo[toDelete.Item.ID].IsLocked == false)
                {
                    HandleBranch(toDelete.Item); // don't delete the branch if the root is locked.
                    WindowMain.DeleteItemFromBrain(toDelete.Item);
                }
                else if (owner != null)
                {
                    PerformRemove(owner.Cluster, toDelete.Item, owner.IndexOf(toDelete));

                        // locked in editor, so don't delete and don't try again, this will not change anyway.
                }
            }
            else
            {
                if (owner != null && owner.Cluster != null)
                {
                    var iIndex = owner.IndexOf(toDelete);
                    PerformRemove(owner.Cluster, toDelete.Item, iIndex);
                }

                if (BrainData.Current.NeuronInfo[toDelete.Item.ID].IsLocked == false)
                {
                    var iDel = new RootDeletionRecord
                                   {
                                       Cluster = owner, 
                                       Item = toDelete, 
                                       DeleteMethod = DeletionMethod.Delete
                                   };
                    fToTryNext.Add(iDel);

                        // it's locked because of meaning usage, this can change as other objects are deleted, so check later on again.
                }
            }
        }

        /// <summary>The process roots again.</summary>
        protected override void ProcessRootsAgain()
        {
            if (fToTryNext.Count > 0)
            {
                System.Collections.Generic.List<RootDeletionRecord> iRoots;
                do
                {
                    fAlreadyProcessed.Clear(); // all items can be processed again.
                    iRoots = fToTryNext;
                    fToTryNext = new System.Collections.Generic.List<RootDeletionRecord>();
                    foreach (var i in iRoots)
                    {
                        if (i.Item.Item != null && i.Item.Item.ID != Neuron.EmptyId)
                        {
                            // check that all of the items that need to be recheked, still exist.
                            if (i.Cluster != null)
                            {
                                if (HasRefsFromCluster(i.Item.Item, i.Cluster.Cluster) == false)
                                {
                                    TryDeleteItemFromBrain(i.Cluster, i.Item);
                                }
                                else
                                {
                                    fToTryNext.Add(i);
                                }
                            }
                            else if (HasRefsFromLink(i.Item.Item, i.OtherSide.Item) == false)
                            {
                                TryDeleteItemFromBrain(i.Cluster, i.Item);
                            }
                            else
                            {
                                fToTryNext.Add(i);
                            }
                        }
                    }
                }
                while (iRoots.Count != fToTryNext.Count);
            }
        }

        #endregion
    }
}