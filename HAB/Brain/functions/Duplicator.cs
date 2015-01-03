// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Duplicator.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="internal" /> helper class for duplicating neurons and
//   clusters in a thread save way, so that no deadlocks can occur. This class
//   also makes certain that a duplication is a single instruction: while
//   retrieving the data from the source, nothing can happen to it and while
//   updating the target, it is also save. Once the source's data is
//   retrieved, it can be modified again (before the target is updated).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="internal" /> helper class for duplicating neurons and
    ///     clusters in a thread save way, so that no deadlocks can occur. This class
    ///     also makes certain that a duplication is a single instruction: while
    ///     retrieving the data from the source, nothing can happen to it and while
    ///     updating the target, it is also save. Once the source's data is
    ///     retrieved, it can be modified again (before the target is updated).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The parent's list is not duplicated. The list of children is maintained
    ///         by a cluster and not the children. Also, a children's list is index
    ///         position sensitve: we wouldn't know where to put the new duplicates since
    ///         we can't put them on the same index position and usually a lot of meaning
    ///         is lost in this case, so no parent list duplication.
    ///     </para>
    ///     <para>
    ///         We first take a quick snapshot of the neuron to duplicate: all links and
    ///         children are copied to a temp list. During this process, the source is
    ///         locked. Any 'values' are already copied over to the target during the
    ///         lock. These don't require a change in any other neurons and can therefor
    ///         be done in the lock. The links and children are copied outside of the
    ///         source lock. This is to prevent deadlocks (if 1 thread wants to
    ///         write-lock our source, while this wants to write-lock the source of the
    ///         other thread: oeps).
    ///     </para>
    /// </remarks>
    internal class Duplicator : NeuronFunction
    {
        #region Fields

        /// <summary>The f target.</summary>
        protected Neuron fTarget;

        #endregion

        /// <summary>Retrieves the locks.</summary>
        /// <returns>The <see cref="DuplicationData"/>.</returns>
        internal override DuplicationData RetrieveLocksNoParents()
        {
            var iRes = base.RetrieveLocksNoParents();
            AddTargetLocks();
            return iRes;
        }

        /// <summary>set all the neurons in the <paramref name="list"/> to changed.
        ///     override, cause a duplicate doesn't cause an unfreeze, which normally
        ///     does happen.</summary>
        /// <param name="list"></param>
        protected override void SetIsChanged(System.Collections.Generic.List<LinkData> list)
        {
            foreach (var i in list)
            {
                i.Neuron.SetIsChangedNoUnfreeze(true);
                i.Meaning.SetIsChangedNoUnfreeze(true);
                if (i.Info != null)
                {
                    foreach (var u in i.Info)
                    {
                        u.SetIsChangedNoUnfreeze(true);
                    }
                }
            }
        }

        /// <summary>Sets the is changed for the <paramref name="list"/> of items.</summary>
        /// <param name="list">The list.</param>
        protected override void SetIsChanged(System.Collections.Generic.List<Neuron> list)
        {
            foreach (var i in fData.fChildren)
            {
                i.SetIsChangedNoUnfreeze(true);
            }
        }

        /// <summary>The set is changed.</summary>
        /// <param name="item">The item.</param>
        protected override void SetIsChanged(Neuron item)
        {
            item.SetIsChangedNoUnfreeze(true);
        }

        /// <summary>
        ///     Used while locking all the neurons involved in a dupliation process.
        ///     This funciton is responsible for adding the lockdata on the target
        ///     neurons.
        /// </summary>
        /// <param name="iRes">The res.</param>
        /// <param name="targets">The targets.</param>
        protected virtual void AddTargetLocks()
        {
            LockRequestList iList;
            if (fData.LockRequestsDict.TryGetValue(fTarget, out iList))
            {
                // if there are already other locks on the targets (should not be possible), then we remove them all and replace it with 1 big 'all' lock, cause that's what we need.
                var iProc = Processor.CurrentProcessor;
                if (iProc != null)
                {
                    foreach (var i in iList)
                    {
                        if (fData.LockRequests.Remove(i))
                        {
                            iProc.Mem.LocksFactory.ReleaseLock(i); // release the lock object when possible
                        }
                    }
                }
                else
                {
                    foreach (var i in iList)
                    {
                        fData.LockRequests.Remove(i);
                    }
                }
            }

            var iReq = LockRequestInfo.Create();
            iReq.Level = LockLevel.All;
            iReq.Neuron = fTarget;
            iReq.Writeable = true;
            fData.LockRequests.Add(iReq);

            if (iList != null)
            {
                iList.Clear();
            }
            else
            {
                iList = LockRequestList.Create();
                fData.LockRequestsDict.Add(fTarget, iList);
            }

            iList.Add(iReq);
        }

        #region functions

        /// <summary>Duplicates all the data from the specified <paramref name="source"/>
        ///     to the target.</summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        internal void Duplicate(Neuron source, Neuron target)
        {
            try
            {
                fTarget = target;
                fSource = source;
                fData = RetrieveLocksNoParents();
                LockManager.Current.RequestLocks(fData.LockRequests);
                try
                {
                    source.CopyTo(target);

                        // copy the simple, thread save data from within the lock, so it becomes a singleton operation.
                    StoreData(target); // while everything is locked, we also make the copy: thread save.
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(fData.LockRequests, true);
                    ReleaseObjects();
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "Duplicator.Duplicate", 
                    string.Format("Failed to dupliate neuron {0} to {1}, error: {2}", source, target, e));
            }
        }

        /// <summary>The store data.</summary>
        /// <param name="target">The target.</param>
        private void StoreData(Neuron target)
        {
            foreach (var i in fData.fLinksIn)
            {
                if (i.Neuron.IsDeleted == false && i.Meaning.IsDeleted == false)
                {
                    // need to check that they weren't deletd during copy.
                    var iNew = Link.CreateLinkUnsafe(i.Neuron, target, i.Meaning);
                    if (i.Info != null && i.Info.Count > 0)
                    {
                        iNew.InfoDirect.AddRange(i.Info);
                    }
                }
            }

            foreach (var i in fData.fLinksOut)
            {
                if (i.Neuron.IsDeleted == false && i.Meaning.IsDeleted == false)
                {
                    // need to check that they weren't deletd during copy.
                    var iNew = Link.CreateLinkUnsafe(target, i.Neuron, i.Meaning);
                    if (i.Info != null && i.Info.Count > 0)
                    {
                        iNew.InfoDirect.AddRange(i.Info);
                    }
                }
            }

            if (fData.Iscluster)
            {
                var iCopyTo = (NeuronCluster)target;
                System.Diagnostics.Debug.Assert(iCopyTo != null);
                if (fData.fMeaning != null && fData.fMeaning.IsDeleted == false)
                {
                    iCopyTo.SetMeaningUnsafe(fData.fMeaning);
                }

                if (fData.fChildren != null)
                {
                    // can be null, if there were no children
                    iCopyTo.AddChildrenUnsafe(fData.fChildren);
                }
            }
        }

        #endregion
    }
}