// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Deleter.cs" company="">
//   
// </copyright>
// <summary>
//   provides functionality to delete a neuron in a thread safe manner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides functionality to delete a neuron in a thread safe manner.
    /// </summary>
    internal class Deleter : NeuronFunction
    {
        /// <summary>The delete.</summary>
        /// <param name="toDelete">The to delete.</param>
        public void Delete(Neuron toDelete)
        {
            try
            {
                fSource = toDelete;
                fData = RetrieveLocks();
                LockManager.Current.RequestLocks(fData.LockRequests);
                try
                {
                    ClearData();
                    fSource.Clear();
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
                    "Deleter.Delete", 
                    string.Format("Failed to delete neuron {0}, error: {1}", toDelete, e));
            }
        }

        /// <summary>
        ///     if there is a context, we need to return all the objects back to the
        ///     factory. this is used to release presure from gc. Also makes certain
        ///     that everything is set to changed.
        /// </summary>
        protected override void ReleaseObjects()
        {
            var iData = (DeletionData)fData;
            if (iData.fParents != null)
            {
                foreach (var i in iData.fParents)
                {
                    i.IsChanged = true;
                }
            }

            base.ReleaseObjects();
        }

        /// <summary>The clear data.</summary>
        private void ClearData()
        {
            ClearLinksIn();
            ClearLinksOut();
            ClearParents();
            ClearChildren();
            if (fData.fMeaning != null)
            {
                fData.fMeaning.DecMeaningUnsafe();
            }
        }

        /// <summary>The clear children.</summary>
        private void ClearChildren()
        {
            var iSource = fSource as NeuronCluster;
            if (iSource != null && fData.fChildren != null)
            {
                foreach (var i in fData.fChildren)
                {
                    if (i != iSource)
                    {
                        // cluster that has itself as child has already been handled: the parent list got cleaned.
                        i.RemoveClusterUnsafe(iSource);
                    }
                }

                iSource.ChildrenDirect.ClearDirect();
            }
        }

        /// <summary>The clear parents.</summary>
        private void ClearParents()
        {
            var iData = (DeletionData)fData;
            if (iData.fParents != null)
            {
                foreach (var i in iData.fParents)
                {
                    i.RemoveAllChildrenUnsafe(fSource);
                }

                if (fSource.ClusteredByIdentifier != null)
                {
                    fSource.ClusteredByDirect.ClearDirect();
                }
            }
        }

        /// <summary>The clear links out.</summary>
        private void ClearLinksOut()
        {
            foreach (var i in fData.fLinksOut)
            {
                try
                {
                    if (i.Neuron.IsDeleted == false && i.Neuron.ID != Neuron.EmptyId && i.Meaning.IsDeleted == false
                        && i.Meaning.ID != Neuron.EmptyId && i.Link.FromID != i.Link.ToID)
                    {
                        // links to self are already deleted cause linksOut is done after linksIn. So don't try to do this again.
                        i.Neuron.RemoveInboundLink(i.Link);
                        i.Meaning.DecMeaningUnsafe();
                        if (i.Info != null)
                        {
                            foreach (var u in i.Info)
                            {
                                u.DecInfoUnsafe();
                            }

                            if (i.Link.InfoIdentifier != null)
                            {
                                i.Link.InfoDirect.Clear();
                            }
                        }

                        i.Link.RaiseDestroyEvent();

                            // we need to do this cause the system doesn't get warned about link destruction otherwise.
                        i.Link.Reset(); // do this so that everyone knows this link is no longer valid.
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Deleter.ClearLinksOut", e.ToString());
                }
            }

            if (fSource.LinksOutIdentifier != null)
            {
                fSource.LinksOutIdentifier.Clear();
            }
        }

        /// <summary>
        ///     removes the link in a thread ansafe way. All neurons are retrieved
        ///     from fdata without locking anything.
        /// </summary>
        private void ClearLinksIn()
        {
            foreach (var i in fData.fLinksIn)
            {
                try
                {
                    if (i.Neuron.ID != Neuron.EmptyId && i.Meaning.ID != Neuron.EmptyId)
                    {
                        i.Neuron.RemoveOutgoingLink(i.Link);
                        i.Meaning.DecMeaningUnsafe();
                        if (i.Info != null)
                        {
                            foreach (var u in i.Info)
                            {
                                u.DecInfoUnsafe();
                            }

                            if (i.Link.InfoIdentifier != null)
                            {
                                i.Link.InfoDirect.Clear();
                            }
                        }

                        i.Link.RaiseDestroyEvent();

                            // we need to do this cause the system doesn't get warned about link destruction otherwise.
                        i.Link.Reset(); // do this so that everyone knows this link is no longer valid.
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Deleter.ClearLinksIn", e.ToString());
                }
            }

            if (fSource.LinksInIdentifier != null)
            {
                // when null -> nothing to clear.
                fSource.LinksInIdentifier.Clear();
            }
        }
    }
}