// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiDuplicator.cs" company="">
//   
// </copyright>
// <summary>
//   A duplicator that can generate multiple copies at once. Used for the
//   splits.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A duplicator that can generate multiple copies at once. Used for the
    ///     splits.
    /// </summary>
    internal class MultiDuplicator : Duplicator
    {
        #region fields

        /// <summary>The f targets.</summary>
        private readonly System.Collections.Generic.List<Neuron> fTargets;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MultiDuplicator"/> class. 
        ///     Initializes a new instance of the <see cref="MultiDuplicator"/>
        ///     class.</summary>
        /// <param name="keepFrozen">When true, the duplication process wont effect the frozen state</param>
        public MultiDuplicator()
        {
            fTargets = fMemFactory.NLists.GetBuffer();
        }

        #endregion

        /// <summary>Duplicates the specified <paramref name="source"/> x nr of times.</summary>
        /// <param name="source">The source.</param>
        /// <param name="nrItems">The nr times to make a duplicate.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<Neuron> Duplicate(Neuron source, int nrItems)
        {
            try
            {
                fSource = source;
                CreateDuplicates(source.GetType(), nrItems);
                fData = RetrieveLocksNoParents();
                LockManager.Current.RequestLocks(fData.LockRequests);
                try
                {
                    foreach (var i in fTargets)
                    {
                        source.CopyTo(i);
                    }

                    StoreData();
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
                    "MultiDuplicator.Duplicate", 
                    string.Format("Failed to dupliate neuron {0}, error: {1}", source, e));
            }

            return fTargets;
        }

        /// <summary>The create duplicates.</summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="nrItems">The nr items.</param>
        private void CreateDuplicates(System.Type sourceType, int nrItems)
        {
            while (nrItems > 0)
            {
                var iNew = NeuronFactory.Get(sourceType);
                Brain.Current.Add(iNew);
                fTargets.Add(iNew);
                nrItems--;
            }
        }

        /// <summary>
        ///     Used while locking all the neurons involved in a dupliation process.
        ///     This funciton is responsible for adding the lockdata on the target
        ///     neurons.
        /// </summary>
        protected override void AddTargetLocks()
        {
            foreach (var i in fTargets)
            {
                LockRequestList iList;
                if (fData.LockRequestsDict.TryGetValue(i, out iList))
                {
                    // if there are already other locks on the targets (should not be possible), then we remove them all and replace it with 1 big 'all' lock, cause that's what we need.
                    foreach (var u in iList)
                    {
                        fData.LockRequests.Remove(u);
                    }

                    iList.Clear();
                }
                else
                {
                    iList = LockRequestList.Create();
                    fData.LockRequestsDict.Add(i, iList);
                }

                var iReq = LockRequestInfo.Create();
                iReq.Level = LockLevel.All;
                iReq.Neuron = i;
                iReq.Writeable = true;
                fData.LockRequests.Add(iReq);
                iList.Add(iReq);
            }
        }

        /// <summary>The store data.</summary>
        private void StoreData()
        {
            CopyLinksIn();
            CopyLinksOut();
            if (fData.Iscluster)
            {
                CopyClusterData();
            }
        }

        /// <summary>The copy cluster data.</summary>
        private void CopyClusterData()
        {
            foreach (NeuronCluster iCopyTo in fTargets)
            {
                System.Diagnostics.Debug.Assert(iCopyTo != null);
                if (fData.fChildren != null)
                {
                    // can be null, if there were no children
                    iCopyTo.AddChildrenUnsafe(fData.fChildren);
                }

                if (fData.fMeaning != null && fData.fMeaning.IsDeleted == false && fData.fMeaning.ID != Neuron.EmptyId)
                {
                    iCopyTo.SetMeaningUnsafe(fData.fMeaning);
                }
            }
        }

        /// <summary>The copy links out.</summary>
        private void CopyLinksOut()
        {
            foreach (var i in fData.fLinksOut)
            {
                if (i.Neuron.IsDeleted == false && i.Meaning.IsDeleted == false && i.Neuron.ID != Neuron.EmptyId
                    && i.Meaning.ID != Neuron.EmptyId)
                {
                    // need to check that they weren't deletd during copy.
                    var iList = Link.CreateUnsafe(fTargets, i.Neuron, i.Meaning);
                    if (i.Info != null && i.Info.Count > 0)
                    {
                        foreach (var iNew in iList)
                        {
                            iNew.InfoDirect.AddRange(i.Info);
                        }
                    }
                }
            }
        }

        /// <summary>The copy links in.</summary>
        private void CopyLinksIn()
        {
            foreach (var i in fData.fLinksIn)
            {
                if (i.Neuron.IsDeleted == false && i.Meaning.IsDeleted == false && i.Neuron.ID != Neuron.EmptyId
                    && i.Meaning.ID != Neuron.EmptyId)
                {
                    // need to check that they weren't deletd during copy.
                    var iList = Link.CreateUnsafe(i.Neuron, fTargets, i.Meaning);
                    if (i.Info != null && i.Info.Count > 0)
                    {
                        foreach (var iNew in iList)
                        {
                            iNew.InfoDirect.AddRange(i.Info);
                        }
                    }
                }
            }
        }
    }
}