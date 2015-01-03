// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgFixBrokenRefs.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgFixBrokenRefs.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for DlgFixBrokenRefs.xaml
    /// </summary>
    public partial class DlgFixBrokenRefs : System.Windows.Window
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DlgFixBrokenRefs" /> class.
        /// </summary>
        public DlgFixBrokenRefs()
        {
            InitializeComponent();
            CurrentPos = 0;
            Maximum = Brain.Current.NextID;
        }

        #endregion

        /// <summary>The on click start.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickStart(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BtnStartStop.Tag == null)
            {
                LogItems.Clear();
                CurrentPos = 0;
                BtnStartStop.Content = "Stop";
                BtnStartStop.Tag = this; // we use a pointer, this is faster to check instead of boxing the val
                BtnStartStop.ToolTip = "Stop the process.";
                BtnClose.IsEnabled = false;
                System.Action iThread = ProcessItems;
                System.AsyncCallback iDoWhenDone = ProcessFinished;
                var iAsync = iThread.BeginInvoke(iDoWhenDone, null);
            }
            else
            {
                fStopProcessing = true;
            }
        }

        /// <summary>
        ///     Processes all the neurons. This allows us to call this function async.
        /// </summary>
        private void ProcessItems()
        {
            var iErr = false;
            for (var i = Neuron.StartId; i < Brain.Current.NextID; i++)
            {
                try
                {
                    if (fStopProcessing)
                    {
                        // check if a stop was requested.
                        break;
                    }

                    if (i % 40 == 0)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<ulong>(SetCurPos), 
                            i);

                            // we use a high priority so that it will always get updated. only send occationaly, otherwise we don't get to see the update.
                    }

                    Neuron iFound;
                    if (Brain.Current.IsValidID(i))
                    {
                        if (Brain.Current.TryFindNeuron(i, out iFound))
                        {
                            // the neuron exists, so check it is ok.
                            if (iFound.ID == i)
                            {
                                // something can go wrong in the save routine, so that the index of an id points to a database location of an old neuron, that got deleted. If this is the case, we need to delete the reference, it's invalid.
                                CheckNeuron(iFound);
                            }
                            else
                            {
                                HardDelete(iFound, i);
                            }
                        }
                        else
                        {
                            CheckDeletedNeuron(i);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("failed to clean '{0}', error: {1}.", i, e));
                    ProjectManager.Default.DataError = true;
                    iErr = true;
                }
            }

            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send, 
                new System.Action<ulong>(SetCurPos), 
                Brain.Current.NextID - 1); // set last pos
            iErr |= CleanThesaurus();
            ProjectManager.Default.DataError = iErr;

                // we set the error switch off, when all has been cleaned, this way, the user can save the project again without problem.
        }

        /// <summary>
        ///     Fixes any thesaurus problems: if a neuron is set to be a relationship, but not in the wordnetsin, add to wordnetsin
        ///     and visa versa.
        /// </summary>
        /// <returns>true if error</returns>
        private bool CleanThesaurus()
        {
            try
            {
                var iThesRootsToRemove = new System.Collections.Generic.List<ulong>();
                foreach (var iKey in BrainData.Current.Thesaurus.Data)
                {
                    if (Brain.Current.IsExistingID(iKey.Key))
                    {
                        if (WordNetSin.LinkDefs.ContainsValue(iKey.Key) == false)
                        {
                            var iText = BrainData.Current.NeuronInfo[iKey.Key].DisplayTitle;
                            Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Send, 
                                new System.Action<string>(LogItems.Add), 
                                string.Format("WordNetSin was missing Linkdef {0}, added to sin.", iText));
                            WordNetSin.LinkDefs.Add(iText, iKey.Key);
                        }

                        var iToRemove = new System.Collections.Generic.List<ulong>();

                            // we need to work with a delete list cause a largeIdcollection can't be reached through index, which we need if we want to loop and delete at once.
                        foreach (var i in iKey.Value)
                        {
                            if (Brain.Current.IsExistingID(i) == false)
                            {
                                iToRemove.Add(i);
                            }
                        }

                        foreach (var i in iToRemove)
                        {
                            iKey.Value.Remove(i);
                            Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Send, 
                                new System.Action<string>(LogItems.Add), 
                                string.Format(
                                    "thesaurus had invalid neuron ({0}) reference in root data, removed from thesaurus.", 
                                    i));
                        }
                    }
                    else
                    {
                        iThesRootsToRemove.Add(iKey.Key);
                    }

                    foreach (var i in iThesRootsToRemove)
                    {
                        BrainData.Current.Thesaurus.Data.Remove(i);
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "thesaurus had an invalid relationship ({0}) reference in root data, removed from thesaurus.", 
                                i));
                    }
                }

                var iLinkDefsToRemove = new System.Collections.Generic.List<string>();
                foreach (var iKey in WordNetSin.LinkDefs)
                {
                    if (Brain.Current.IsExistingID(iKey.Value) == false)
                    {
                        iLinkDefsToRemove.Add(iKey.Key);
                    }

                    foreach (var i in iLinkDefsToRemove)
                    {
                        WordNetSin.LinkDefs.Remove(i);
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "the WordNetSin had an invalid relationship ({0}) entry point, removed from the sin's entry points.", 
                                i));
                    }
                }

                return false;
            }
            catch (System.Exception e)
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<string>(LogItems.Add), 
                    string.Format("failed to clean thesaurus, error: {1}.", e));
                return true;
            }
        }

        /// <summary>Checks if the specified id is registered as deleted, if not, it's a missing neuron and reregister as deleted.</summary>
        /// <param name="id">The id.</param>
        private void CheckDeletedNeuron(ulong id)
        {
            if (Brain.Current.IsValidID(id))
            {
                // if a deleted neuron is still valid, its' not registered as deleted and within range of being allowed to be deleted.
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<string>(LogItems.Add), 
                    string.Format(
                        "Id {0} was not registered as deleted, but no neuron was found in the storage so the id will be recycled.", 
                        id));
                Brain.Current.Delete(id);
                BrainData.Current.NeuronInfo.DeleteItem(id);

                    // also needs to be removed from the neuronInfoDict, otherwise we get errors all over the place.
            }
            else
            {
                var iFound = BrainData.Current.NeuronInfo.InternalTryGetNeuronData(id); // check if the item
                if (iFound != null)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format(
                            "Id {0} was registered as deleted, but the designer was still storing info about it, which has been removed.", 
                            id));
                    BrainData.Current.NeuronInfo.DeleteItem(id);
                }
            }
        }

        /// <summary>Checks all the data of the neuron.</summary>
        /// <param name="toCheck">The neuron to check.</param>
        private void CheckNeuron(Neuron toCheck)
        {
            System.Collections.Generic.List<Link> iList;
            using (var iLinks = toCheck.LinksIn)
                iList = new System.Collections.Generic.List<Link>(iLinks);

                    // make a copy, otherwise we can't destroy items.
            CheckLinks(iList, "incomming link", toCheck);

                // we check both links in and out, cause we want to verify 'all' the data.
            using (var iLinks = toCheck.LinksOut)
                iList = new System.Collections.Generic.List<Link>(iLinks);

                    // make a copy, otherwise we can't destroy items.
            CheckLinks(iList, "outgoing link", toCheck);
            using (var iClusters = toCheck.ClusteredByW)
            {
                CheckClusteredByList(iClusters, "ClusteredBy", toCheck);
                CheckListType(iClusters, typeof(NeuronCluster), "ClusteredBy", toCheck);
            }

            var iToCheck = toCheck as NeuronCluster;
            if (iToCheck != null)
            {
                using (var iChildren = iToCheck.ChildrenW)
                {
                    CheckList(iChildren, "Children", toCheck);
                    CheckChildren(iToCheck, iChildren);
                }
            }

            CheckClusteredBy(toCheck); // also need to check if we don't have any illegal references to 'clusters'
        }

        /// <summary>Checks if all the references defined in the list are valid and removes the ones that arent.</summary>
        /// <param name="list">The list.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="toCheck">The to Check.</param>
        private void CheckClusteredByList(ListAccessor<ulong> list, string desc, Neuron toCheck)
        {
            var iToRemove = (from i in list where Brain.Current.IsExistingID(i) == false select i).ToList();
            foreach (var i in iToRemove)
            {
                try
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("Removing '{0}' from the {1} in {2}: ID doesn't exist.", i, desc, toCheck.ID));
                    list.Remove(i);
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("failed to delete item from {0}: {1}.", desc, e));
                }
            }
        }

        /// <summary>Checks if the children of a neuronCluster all have the correct back references in their 'ClusteredBy' list.
        ///     We must make certain that items who are included multiple times, have multiple backlinks.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="listToCheck">The list To Check.</param>
        private void CheckChildren(NeuronCluster toCheck, IDListAccessor listToCheck)
        {
            var iToCheck = from i in listToCheck

                           // we need to find out how many times each id occures in the children list
                           group i by i
                           into SameIds
                           select new { ID = SameIds.Key, Count = (from u in SameIds select 1).Count() };
            foreach (var i in iToCheck)
            {
                Neuron iNeuron = null;
                int iCount;
                if (Neuron.IsEmpty(i.ID) == false && Brain.Current.TryFindNeuron(i.ID, out iNeuron)
                    && iNeuron.ClusteredByIdentifier != null)
                {
                    using (var iList = iNeuron.ClusteredBy) iCount = (from u in iList where u == toCheck.ID select 1).Count();
                }
                else
                {
                    iCount = 0; // the item doesn't exist, so remove all references.
                }

                RemoveClusteredBy(toCheck, iNeuron, i.Count, ref iCount, i.ID);
                listToCheck.IsWriteable = true;
                listToCheck.Lock(iNeuron);
                while (iCount < i.Count)
                {
                    // remove any refs in child that are to many
                    try
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "Removing '{0}' from '{1}' in 'Children': missing reference in child.ClusteredBy to cluster.", 
                                i.ID, 
                                toCheck.ID));
                        listToCheck.List.Remove(i.ID);

                            // remove directly on the list, otherwise the other side gets updated, and the imballance remains.
                    }
                    catch (System.Exception e)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format("failed to delete item '{0}' from '{1}': {2}.", i.ID, toCheck.ID, e));
                    }

                    iCount++;
                }

                listToCheck.Unlock(iNeuron); // this takes care of 'isChanged'
            }
        }

        /// <summary>The remove clustered by.</summary>
        /// <param name="toCheck">The to check.</param>
        /// <param name="iNeuron">The i neuron.</param>
        /// <param name="i">The i.</param>
        /// <param name="iCount">The i count.</param>
        /// <param name="id">The id.</param>
        private void RemoveClusteredBy(NeuronCluster toCheck, Neuron iNeuron, int i, ref int iCount, ulong id)
        {
            if (iNeuron != null && i > 0)
            {
                IDListAccessor iClusteredBy = iNeuron.ClusteredByW;
                iClusteredBy.Lock(iNeuron);
                try
                {
                    while (iCount > i)
                    {
                        // remove any refs in child that are to many
                        try
                        {
                            Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Send, 
                                new System.Action<string>(LogItems.Add), 
                                string.Format(
                                    "Removing '{0}' from '{1}' in 'ClusteredBy': to many back references to cluster.", 
                                    toCheck.ID, 
                                    id));
                            iClusteredBy.List.Remove(toCheck.ID);

                                // do a direct remove, otherwise the other side gets updated, which we dont want.
                        }
                        catch (System.Exception e)
                        {
                            Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Send, 
                                new System.Action<string>(LogItems.Add), 
                                string.Format("failed to delete item '{1}' from '{0}': {2}.", id, toCheck.ID, e));
                        }

                        iCount--;
                    }
                }
                finally
                {
                    iClusteredBy.Unlock(iNeuron); // this makes certain taht the 'ischanged' is correctly set.
                    iClusteredBy.Dispose();
                }
            }
        }

        /// <summary>Similar as <see cref="DlgFixBrokenRefs.CheckChildren"/>. Checks if each cluster declared in 'ClusteredBy' of the
        ///     argument,
        ///     also has a backreference.  If not, the reference gets removed.  Keeps count of multiple references to the same
        ///     cluster.</summary>
        /// <param name="toCheck">To check.</param>
        private void CheckClusteredBy(Neuron toCheck)
        {
            if (toCheck.ClusteredByIdentifier != null)
            {
                using (var iList = toCheck.ClusteredBy)
                {
                    var iToCheck = from i in toCheck.ClusteredBy

                                   // we need to find out how many times each id occures in the children list
                                   group i by i
                                   into SameIds
                                   select new { ID = SameIds.Key, Count = (from u in SameIds select 1).Count() };
                    foreach (var i in iToCheck)
                    {
                        Neuron iNeuron;
                        NeuronCluster iCluster = null;
                        var iCount = 0;
                        if (Brain.Current.TryFindNeuron(i.ID, out iNeuron))
                        {
                            iCluster = iNeuron as NeuronCluster;
                            if (iCluster != null)
                            {
                                using (var iChildren = iCluster.Children) iCount = (from u in iChildren where u == toCheck.ID select 1).Count();
                            }
                        }

                        while (iCount > i.Count)
                        {
                            // remove any refs in child that are to many
                            try
                            {
                                Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Send, 
                                    new System.Action<string>(LogItems.Add), 
                                    string.Format(
                                        "Removing '{0}' from '{1}' in 'Children': missing reference in child.ClusteredBy to cluster.", 
                                        i.ID, 
                                        toCheck.ID));
                                iCluster.ChildrenDirect.List.Remove(toCheck.ID);

                                    // direct remove, otherwise the refs get updated and the imbalance remains.
                            }
                            catch (System.Exception e)
                            {
                                Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Send, 
                                    new System.Action<string>(LogItems.Add), 
                                    string.Format("failed to delete item '{1}' from '{0}': {2}.", i.ID, toCheck.ID, e));
                            }

                            iCount--;
                        }

                        while (iCount < i.Count)
                        {
                            // remove any refs in child that are to many
                            try
                            {
                                Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Send, 
                                    new System.Action<string>(LogItems.Add), 
                                    string.Format(
                                        "Removing '{0}' from '{1}' in 'ClusteredBy': to many back references to cluster.", 
                                        toCheck.ID, 
                                        i.ID));
                                toCheck.ClusteredByDirect.List.Remove(i.ID);

                                    // direct remove, otherwise the refs get updated and the imbalance remains.
                            }
                            catch (System.Exception e)
                            {
                                Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Send, 
                                    new System.Action<string>(LogItems.Add), 
                                    string.Format("failed to delete item '{0}' from '{1}': {2}.", i.ID, toCheck.ID, e));
                            }

                            iCount++;
                        }
                    }
                }
            }
        }

        /// <summary>First makes certain all the links are removed before trying to delete the actual neuron.</summary>
        /// <param name="toDelete">To delete.</param>
        /// <param name="id">The id.</param>
        private void HardDelete(Neuron toDelete, ulong id)
        {
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send, 
                new System.Action<string>(LogItems.Add), 
                string.Format(
                    "Found neuron with invalid id, expected: '{0}' found '{1}'. Deleting neuron", 
                    id, 
                    toDelete.ID));
            System.Collections.Generic.List<Link> iList;
            using (var iLinks = toDelete.LinksIn)
                iList = new System.Collections.Generic.List<Link>(iLinks);

                    // make a copy, otherwise we can't destroy items.
            for (var i = 0; i < iList.Count; i++)
            {
                iList[i].DestroyHard();
            }

            using (var iLinks = toDelete.LinksOut)
                iList = new System.Collections.Generic.List<Link>(iLinks);

                    // make a copy, otherwise we can't destroy items.
            for (var i = 0; i < iList.Count; i++)
            {
                iList[i].DestroyHard();
            }

            // if (id < (ulong)PredefinedNeurons.EndOfStatic)
            Brain.Current.ResolveCrossRef(toDelete, id, toDelete.ID);

            // else
        }

        /// <summary>Checks all the links in the list.</summary>
        /// <remarks>We check if the to, from an meaning are valid refs + all the items in the info list.
        ///     We also check if the link is to be found in both from and to neurons (so accessible from both sides, if not the
        ///     case, fix this).</remarks>
        /// <param name="list">The list.</param>
        /// <param name="direction"></param>
        /// <param name="toCheck">The to Check.</param>
        private void CheckLinks(System.Collections.Generic.IEnumerable<Link> list, string direction, Neuron toCheck)
        {
            Neuron iToCheck;
            foreach (var i in list)
            {
                Settings.ErrorOnInvalidLinkRemove = false; // disable this, so we can actually delete invalid links.
                try
                {
                    var iDestroy = Brain.Current.IsExistingID(i.FromID) == false;
                    if (iDestroy == false)
                    {
                        if (Brain.Current.TryFindNeuron(i.FromID, out iToCheck))
                        {
                            if (toCheck.LinksOutIdentifier != null)
                            {
                                using (var iLinks = toCheck.LinksOut)
                                    if (iToCheck.LinksOut.Contains(i) == false)
                                    {
                                        iDestroy = true;
                                    }
                            }
                        }
                        else
                        {
                            iDestroy = true;
                        }
                    }

                    if (iDestroy)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "Deleting {0} defined in {1}: 'from' pointed to invalid neuron: {2}.", 
                                direction, 
                                toCheck.ID, 
                                i.FromID));
                        i.DestroyHard();
                        continue;
                    }

                    iDestroy = Brain.Current.IsExistingID(i.ToID) == false;
                    if (iDestroy == false)
                    {
                        if (Brain.Current.TryFindNeuron(i.ToID, out iToCheck))
                        {
                            if (i.To.LinksInIdentifier != null)
                            {
                                using (var iLinks = i.To.LinksIn)
                                    if (iLinks.Contains(i) == false)
                                    {
                                        iDestroy = true;
                                    }
                            }
                            else
                            {
                                iDestroy = true;
                            }
                        }
                        else
                        {
                            iDestroy = true;
                        }
                    }

                    if (iDestroy)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "Deleting {0} defined in {1}: 'to' pointed to invalid neuron: {2}.", 
                                direction, 
                                toCheck.ID, 
                                i.ToID));
                        i.DestroyHard();
                        continue;
                    }

                    if (Brain.Current.IsExistingID(i.MeaningID) == false
                        || Brain.Current.TryFindNeuron(i.MeaningID, out iToCheck) == false)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(LogItems.Add), 
                            string.Format(
                                "Deleting {0} defined in {1}: 'meaning' pointed to invalid neuron: {2}.", 
                                direction, 
                                toCheck.ID, 
                                i.MeaningID));
                        i.DestroyHard();
                        continue;
                    }

                    CheckList(i.InfoW, "info list", toCheck);
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("failed to delete link: {0}.", e));
                    Settings.ErrorOnInvalidLinkRemove = true;
                }
            }
        }

        /// <summary>Checks if all the references defined in the list are valid and removes the ones that arent.</summary>
        /// <param name="list">The list.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="toCheck">The to Check.</param>
        private void CheckList(System.Collections.Generic.IList<ulong> list, string desc, Neuron toCheck)
        {
            var iToRemove = new System.Collections.Generic.List<ulong>();
            foreach (var i in list)
            {
                if (Brain.Current.IsExistingID(i) == false)
                {
                    iToRemove.Add(i);
                }
                else
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound) == false)
                    {
                        iToRemove.Add(i);
                    }
                }
            }

            foreach (var i in iToRemove)
            {
                try
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("Removing '{0}' from the {1} in {2}: ID doesn't exist.", i, desc, toCheck.ID));
                    list.Remove(i);
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("failed to delete item from {0}: {1}.", desc, e));
                }
            }
        }

        /// <summary>Checks if the type of the items in a list is correct and removes all items from the list that don't match.</summary>
        /// <param name="list">The list with items to check.</param>
        /// <param name="type">The type.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="toCheck">The to Check.</param>
        private void CheckListType(ListAccessor<ulong> list, System.Type type, string desc, Neuron toCheck)
        {
            var iToRemove = new System.Collections.Generic.List<ulong>();
            foreach (var i in list)
            {
                if (Brain.Current.IsExistingID(i) == false)
                {
                    iToRemove.Add(i);
                }
                else
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        if (iFound.GetType() != type)
                        {
                            iToRemove.Add(i);
                        }
                    }
                    else
                    {
                        iToRemove.Add(i);
                    }
                }
            }

            foreach (var i in iToRemove)
            {
                try
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("Removing '{0}' from the {1} in {2}: not a '{3}'.", i, desc, toCheck.ID, type));
                    list.Remove(i);
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(LogItems.Add), 
                        string.Format("failed to delete item from {0}: {1}.", desc, e));
                }
            }
        }

        /// <summary>Called when the async <see cref="DlgFixBrokenRefs.ProcessItems"/> is finished.  Updates the buttons and stuff
        ///     to indicate that we are done.</summary>
        /// <param name="result">The result.</param>
        private void ProcessFinished(System.IAsyncResult result)
        {
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send, 
                new System.Action(InternalProcessFinished));
        }

        /// <summary>The internal process finished.</summary>
        private void InternalProcessFinished()
        {
            BtnStartStop.Content = "Start";
            BtnStartStop.ToolTip = "Start the process.";
            BtnClose.IsEnabled = true;
            BtnStartStop.Tag = null; // always let the system know we canallow a new start.
            LogItems.Add("All neurons processed!");
        }

        /// <summary>Sets the current pos (for async calls).</summary>
        /// <param name="id">The id.</param>
        private void SetCurPos(ulong id)
        {
            CurrentPos = id;
        }

        #region Fields

        /// <summary>The f log items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<string> fLogItems =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>The f stop processing.</summary>
        private bool fStopProcessing; // switch used to stop the processing.

        #endregion

        #region Prop

        #region CurrentPos

        /// <summary>
        ///     CurrentPos Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CurrentPosProperty =
            System.Windows.DependencyProperty.Register(
                "CurrentPos", 
                typeof(ulong), 
                typeof(DlgFixBrokenRefs), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the CurrentPos property.  This dependency property
        ///     indicates the id of the current neuron being processed.
        /// </summary>
        public ulong CurrentPos
        {
            get
            {
                return (ulong)GetValue(CurrentPosProperty);
            }

            set
            {
                SetValue(CurrentPosProperty, value);
            }
        }

        #endregion

        #region Maximum

        /// <summary>
        ///     Maximum Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty MaximumProperty =
            System.Windows.DependencyProperty.Register(
                "Maximum", 
                typeof(ulong), 
                typeof(DlgFixBrokenRefs), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the Maximum property.  This dependency property
        ///     indicates how many neurons need to be processed in total.
        /// </summary>
        public ulong Maximum
        {
            get
            {
                return (ulong)GetValue(MaximumProperty);
            }

            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        #endregion

        #region LogItems

        /// <summary>
        ///     Gets the list of log items that need to shown to the user.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> LogItems
        {
            get
            {
                return fLogItems;
            }
        }

        #endregion

        #endregion
    }
}