// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronFunction.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for functions that perform a task on a neuron and need to
//   lock/retrieve all the neurons directly related (linked/parent/children)
//   to the neuron in question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     A base class for functions that perform a task on a neuron and need to
    ///     lock/retrieve all the neurons directly related (linked/parent/children)
    ///     to the neuron in question.
    /// </summary>
    internal class NeuronFunction
    {
        /// <summary>The f data.</summary>
        protected DuplicationData fData;

        /// <summary>The f mem factory.</summary>
        protected Factories fMemFactory = Factories.Default; // we keep a local copy of this var, for faster access.

        /// <summary>The f source.</summary>
        protected Neuron fSource;

        #region retrive data & locking

        /// <summary>Retrieves the locks but doesn't inlcude any of the parent objects.
        ///     This is for duplication.</summary>
        /// <returns>The <see cref="DuplicationData"/>.</returns>
        internal virtual DuplicationData RetrieveLocksNoParents()
        {
            fData = new DuplicationData();
            System.Collections.Generic.List<Link> iOut = null;
            System.Collections.Generic.List<Link> iIn = null;
            System.Collections.Generic.List<ulong> iChildren = null;
            var iCluster = fSource as NeuronCluster;
            LockManager.Current.RequestLock(fSource, LockLevel.All, false);
            try
            {
                if (fSource.LinksOutIdentifier != null)
                {
                    iOut = fMemFactory.LinkLists.GetBuffer();
                    iOut.AddRange(fSource.LinksOutIdentifier);
                }

                if (fSource.LinksInIdentifier != null)
                {
                    iIn = fMemFactory.LinkLists.GetBuffer();
                    iIn.AddRange(fSource.LinksInIdentifier);
                }

                if (iCluster != null && iCluster.ChildrenIdentifier != null)
                {
                    iChildren = fMemFactory.IDLists.GetBuffer(iCluster.ChildrenIdentifier.Count);
                    iChildren.AddRange(iCluster.ChildrenIdentifier);
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(fSource, LockLevel.All, false);
            }

            AddSourceLock();
            RetrieveLinksIn(iIn);
            RetrieveLinksOut(iOut);
            if (iCluster != null)
            {
                RetrieveChildren(iChildren);
            }

            if (Processor.CurrentProcessor != null)
            {
                if (iOut != null)
                {
                    fMemFactory.LinkLists.Recycle(iOut);
                }

                if (iIn != null)
                {
                    fMemFactory.LinkLists.Recycle(iIn);
                }

                if (iChildren != null)
                {
                    fMemFactory.IDLists.Recycle(iChildren);
                }
            }

            return fData;
        }

        /// <summary>Retrieves the locks and inlcudes the parent objects. This is for
        ///     deletion.</summary>
        /// <returns>The <see cref="DuplicationData"/>.</returns>
        internal virtual DuplicationData RetrieveLocks()
        {
            fData = new DeletionData();
            System.Collections.Generic.List<Link> iOut = null;
            System.Collections.Generic.List<Link> iIn = null;
            System.Collections.Generic.List<ulong> iChildren = null;
            System.Collections.Generic.List<ulong> iParents = null;
            var iCluster = fSource as NeuronCluster;
            LockManager.Current.RequestLock(fSource, LockLevel.All, false);
            try
            {
                if (fSource.LinksOutIdentifier != null)
                {
                    iOut = fMemFactory.LinkLists.GetBuffer();
                    iOut.AddRange(fSource.LinksOutIdentifier);
                }

                if (fSource.LinksInIdentifier != null)
                {
                    iIn = fMemFactory.LinkLists.GetBuffer();
                    iIn.AddRange(fSource.LinksInIdentifier);
                }

                if (fSource.ClusteredByIdentifier != null)
                {
                    iParents = fMemFactory.IDLists.GetBuffer(fSource.ClusteredByDirect.Count);
                    iParents.AddRange((ClusterList)fSource.ClusteredByIdentifier);
                }

                if (iCluster != null && iCluster.ChildrenIdentifier != null)
                {
                    iChildren = fMemFactory.IDLists.GetBuffer(iCluster.ChildrenIdentifier.Count);
                    iChildren.AddRange(iCluster.ChildrenIdentifier);
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(fSource, LockLevel.All, false);
            }

            AddSourceLock();
            RetrieveLinksIn(iIn);
            RetrieveLinksOut(iOut);
            RetrieveParents(iParents);
            if (iCluster != null)
            {
                RetrieveChildren(iChildren);
            }

            if (Processor.CurrentProcessor != null)
            {
                if (iOut != null)
                {
                    fMemFactory.LinkLists.Recycle(iOut);
                }

                if (iIn != null)
                {
                    fMemFactory.LinkLists.Recycle(iIn);
                }

                if (iParents != null)
                {
                    fMemFactory.IDLists.Recycle(iParents);
                }

                if (iChildren != null)
                {
                    fMemFactory.IDLists.Recycle(iChildren);
                }
            }

            return fData;
        }

        /// <summary>The retrieve parents.</summary>
        /// <param name="items">The items.</param>
        private void RetrieveParents(System.Collections.Generic.List<ulong> items)
        {
            if (items != null)
            {
                Neuron iFound;
                var iData = (DeletionData)fData;
                iData.fParents = fMemFactory.CLists.GetBuffer();
                foreach (var i in items)
                {
                    if (Brain.Current.TryFindNeuron(i, out iFound) && iFound.IsDeleted == false)
                    {
                        // could be that it's already removed.
                        var iN = (NeuronCluster)iFound;
                        if (iN != null)
                        {
                            // should not be the case, unless the id has already been recycled?
                            iData.fParents.Add(iN);
                            AddParentToLock(iN);
                        }
                    }
                }
            }
        }

        /// <summary>The retrieve children.</summary>
        /// <param name="items">The items.</param>
        private void RetrieveChildren(System.Collections.Generic.List<ulong> items)
        {
            var iCluster = (NeuronCluster)fSource;
            Neuron iFound;
            fData.Iscluster = true;
            if (iCluster.Meaning != Neuron.EmptyId)
            {
                if (Brain.Current.TryFindNeuron(iCluster.Meaning, out iFound) && iFound.IsDeleted == false)
                {
                    fData.fMeaning = iFound; // in case the meaning neuron got removed, it's id can be reused.
                }

                BuildMeaningLock(fData.fMeaning);
            }

            if (items != null)
            {
                fData.fChildren = fMemFactory.NLists.GetBuffer();
                foreach (var i in items)
                {
                    if (Brain.Current.TryFindNeuron(i, out iFound) && iFound.IsDeleted == false)
                    {
                        // could be that it's already removed.
                        fData.fChildren.Add(iFound);
                        AddChildToLock(iFound);
                    }
                }
            }
        }

        /// <summary>The retrieve links out.</summary>
        /// <param name="items">The items.</param>
        private void RetrieveLinksOut(System.Collections.Generic.List<Link> items)
        {
            if (items != null)
            {
                foreach (var i in items)
                {
                    if (i.IsValid == false)
                    {
                        // if any part of the link is null, the link has already been destroyed, so skip it.
                        continue;
                    }

                    // don't check if i.To is already marked for deletion. We always need to do a substract for the meaning and info.
                    var iNew = LockRequestsFactory.CreateLinkData();
                    iNew.Neuron = i.To;
                    iNew.Meaning = i.Meaning;
                    iNew.Link = i;
                    BuildToLock(iNew.Neuron);
                    BuildMeaningLock(iNew.Meaning);
                    System.Collections.Generic.List<ulong> iInfo;
                    using (IDListAccessor iInfoAcc = i.Info)
                    {
                        iInfo = fMemFactory.IDLists.GetBuffer(iInfoAcc.CountUnsafe);
                        iInfo.AddRange(iInfoAcc);
                    }

                    if (iInfo.Count > 0)
                    {
                        iNew.Info = fMemFactory.NLists.GetBuffer();
                        foreach (var iId in iInfo)
                        {
                            var iItem = Brain.Current[iId];
                            iNew.Info.Add(iItem);
                        }

                        BuildInfoLock(iNew.Info);
                    }

                    fData.fLinksOut.Add(iNew);
                    fMemFactory.IDLists.Recycle(iInfo);
                }
            }
        }

        /// <summary>The retrieve links in.</summary>
        /// <param name="items">The items.</param>
        private void RetrieveLinksIn(System.Collections.Generic.List<Link> items)
        {
            if (items != null)
            {
                foreach (var i in items)
                {
                    if (i.IsValid == false)
                    {
                        // if any part of the link is null, the link has already been destroyed, so skip it.
                        continue;
                    }

                    var iFrom = i.From;
                    if (iFrom.IsDeleted == false)
                    {
                        // we don't unregister the incomming link if the other side is already trying to unregister: it would happen 2 times otherwise.
                        var iNew = LockRequestsFactory.CreateLinkData();
                        iNew.Neuron = i.From;
                        iNew.Meaning = i.Meaning;
                        iNew.Link = i;
                        BuildFromLock(iNew.Neuron);
                        BuildMeaningLock(iNew.Meaning);
                        System.Collections.Generic.List<ulong> iInfo;
                        using (IDListAccessor iInfoAcc = i.Info)
                        {
                            iInfo = fMemFactory.IDLists.GetBuffer(iInfoAcc.CountUnsafe);
                            iInfo.AddRange(iInfoAcc);
                        }

                        if (iInfo != null && iInfo.Count > 0)
                        {
                            iNew.Info = fMemFactory.NLists.GetBuffer();
                            foreach (var iId in iInfo)
                            {
                                var iItem = Brain.Current[iId];
                                iNew.Info.Add(iItem);
                            }

                            BuildInfoLock(iNew.Info);
                        }

                        fData.fLinksIn.Add(iNew);
                        fMemFactory.IDLists.Recycle(iInfo);
                    }
                }
            }
        }

        #region locking

        /// <summary>The add source lock.</summary>
        private void AddSourceLock()
        {
            LockRequestList iList = null;
            if (fData.LockRequestsDict.TryGetValue(fSource, out iList))
            {
                // if there are already other locks on the source, then we remove them all and replace it with 1 big 'all' lock, cause that's what we need.
                foreach (var i in iList)
                {
                    fData.LockRequests.Remove(i);

                        // they need to be removed from the list of requests. They will be recycled in just a bit.
                    if (Processor.CurrentProcessor != null)
                    {
                        // need to release the items so they can be reused later on.
                        Processor.CurrentProcessor.Mem.LocksFactory.ReleaseLock(i);
                    }
                }

                iList.Clear();
            }

            var iReq = LockRequestInfo.Create();
            iReq.Level = LockLevel.All;
            iReq.Neuron = fSource;
            iReq.Writeable = true;
            fData.LockRequests.Add(iReq);

            if (iList == null)
            {
                iList = LockRequestList.Create();
                fData.LockRequestsDict.Add(fSource, iList);
            }

            iList.Add(iReq);
        }

        /// <summary>The add child to lock.</summary>
        /// <param name="i">The i.</param>
        private void AddChildToLock(Neuron i)
        {
            LockRequestList iList;
            var iAdd = true;
            if (fData.LockRequestsDict.TryGetValue(i, out iList))
            {
                // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                foreach (var u in iList)
                {
                    if (u.Level == LockLevel.Parents || u.Level == LockLevel.All)
                    {
                        iAdd = false;
                        break;
                    }
                }
            }

            if (iAdd)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.Parents;
                iReq.Neuron = i;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);

                if (iList == null)
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(i, iList);
                }

                iList.Add(iReq);
            }
        }

        /// <summary>The add parent to lock.</summary>
        /// <param name="i">The i.</param>
        private void AddParentToLock(NeuronCluster i)
        {
            LockRequestList iList;
            var iAdd = true;
            if (fData.LockRequestsDict.TryGetValue(i, out iList))
            {
                // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                foreach (var u in iList)
                {
                    if (u.Level == LockLevel.Children || u.Level == LockLevel.All)
                    {
                        iAdd = false;
                        break;
                    }
                }
            }

            if (iAdd)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.Children;
                iReq.Neuron = i;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);

                if (iList == null)
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(i, iList);
                }

                iList.Add(iReq);
            }
        }

        /// <summary>The build info lock.</summary>
        /// <param name="list">The list.</param>
        private void BuildInfoLock(System.Collections.Generic.IList<Neuron> list)
        {
            foreach (var i in list)
            {
                LockRequestList iList;
                var iAdd = true;
                if (fData.LockRequestsDict.TryGetValue(i, out iList))
                {
                    // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                    var iFound =
                        (from u in iList where u.Level == LockLevel.Value || u.Level == LockLevel.All select u)
                            .FirstOrDefault();
                    iAdd = iFound == null;
                }

                if (iAdd)
                {
                    var iReq = LockRequestInfo.Create();
                    iReq.Level = LockLevel.Value;
                    iReq.Neuron = i;
                    iReq.Writeable = true;
                    fData.LockRequests.Add(iReq);

                    if (iList == null)
                    {
                        iList = LockRequestList.Create();
                        fData.LockRequestsDict.Add(i, iList);
                    }

                    iList.Add(iReq);
                }
            }
        }

        /// <summary>The build meaning lock.</summary>
        /// <param name="neuron">The neuron.</param>
        private void BuildMeaningLock(Neuron neuron)
        {
            LockRequestList iList;
            var iAdd = true;
            if (fData.LockRequestsDict.TryGetValue(neuron, out iList))
            {
                // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                var iFound =
                    (from u in iList where u.Level == LockLevel.Value || u.Level == LockLevel.All select u)
                        .FirstOrDefault();
                iAdd = iFound == null;
            }

            if (iAdd)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.Value;
                iReq.Neuron = neuron;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);

                if (iList == null)
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(neuron, iList);
                }

                iList.Add(iReq);
            }
        }

        /// <summary>The build from lock.</summary>
        /// <param name="neuron">The neuron.</param>
        private void BuildFromLock(Neuron neuron)
        {
            LockRequestList iList;
            var iAdd = true;
            if (fData.LockRequestsDict.TryGetValue(neuron, out iList))
            {
                // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                var iFound =
                    (from u in iList where u.Level == LockLevel.LinksOut || u.Level == LockLevel.All select u)
                        .FirstOrDefault();
                iAdd = iFound == null;
            }

            if (iAdd)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.LinksOut;
                iReq.Neuron = neuron;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);

                if (iList == null)
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(neuron, iList);
                }

                iList.Add(iReq);
            }
        }

        /// <summary>The build to lock.</summary>
        /// <param name="neuron">The neuron.</param>
        private void BuildToLock(Neuron neuron)
        {
            LockRequestList iList;
            var iAdd = true;
            if (fData.LockRequestsDict.TryGetValue(neuron, out iList))
            {
                // if there are already other locks on the item for the same level, don't try to add a new lock, cause it's already done.
                var iFound =
                    (from u in iList where u.Level == LockLevel.LinksIn || u.Level == LockLevel.All select u)
                        .FirstOrDefault();
                iAdd = iFound == null;
            }

            if (iAdd)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.LinksIn;
                iReq.Neuron = neuron;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);
                if (iList == null)
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(neuron, iList);
                }

                iList.Add(iReq);
            }
        }

        #endregion

        #endregion

        #region releaseData

        /// <summary>set all the neurons in the <paramref name="list"/> to changed.</summary>
        /// <param name="list"></param>
        protected virtual void SetIsChanged(System.Collections.Generic.List<LinkData> list)
        {
            foreach (var i in list)
            {
                i.Neuron.IsChanged = true;
                i.Meaning.IsChanged = true; // increment/decrement of count
                if (i.Info != null)
                {
                    foreach (var u in i.Info)
                    {
                        u.IsChanged = true;
                    }
                }
            }
        }

        /// <summary>
        ///     if there is a context, we need to return all the objects back to the
        ///     factory. this is used to release presure from gc. Also makes certain
        ///     that everything is set to changed.
        /// </summary>
        protected virtual void ReleaseObjects()
        {
            SetIsChanged(fData.fLinksIn);
            SetIsChanged(fData.fLinksOut);
            if (Processor.CurrentProcessor != null)
            {
                var iMem = Processor.CurrentProcessor.Mem;
                foreach (var i in fData.LockRequestsDict)
                {
                    iMem.LocksFactory.ReleaseList(i.Value);
                }

                iMem.LocksFactory.Release(fData.LockRequestsDict);
                iMem.LocksFactory.Release(fData.fLinksIn);
                iMem.LocksFactory.Release(fData.fLinksOut);
            }

            if (fData.fChildren != null)
            {
                SetIsChanged(fData.fChildren);
                fMemFactory.NLists.Recycle(fData.fChildren);
                fData.fChildren = null;
                if (fData.fMeaning != null)
                {
                    SetIsChanged(fData.fMeaning);
                }
            }

            if (fData is DeletionData && ((DeletionData)fData).fParents != null)
            {
                fMemFactory.CLists.Recycle(((DeletionData)fData).fParents);
                ((DeletionData)fData).fParents = null;
            }

            SetIsChanged(fSource);
        }

        /// <summary>The set is changed.</summary>
        /// <param name="item">The item.</param>
        protected virtual void SetIsChanged(Neuron item)
        {
            item.IsChanged = true;
        }

        /// <summary>Sets the is changed for the <paramref name="list"/> of items.</summary>
        /// <param name="list">The list.</param>
        protected virtual void SetIsChanged(System.Collections.Generic.List<Neuron> list)
        {
            foreach (var i in fData.fChildren)
            {
                i.IsChanged = true;
            }
        }

        #endregion
    }
}