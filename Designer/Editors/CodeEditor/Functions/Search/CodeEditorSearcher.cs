// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeEditorSearcher.cs" company="">
//   
// </copyright>
// <summary>
//   Provides search functionality for a code editor. It can search for a
//   single neuron and display all the code editors that it is used in (or for
//   the leef, if no editor is found for the actual item).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    using System.Linq;

    /// <summary>
    ///     Provides search functionality for a code editor. It can search for a
    ///     single neuron and display all the code editors that it is used in (or for
    ///     the leef, if no editor is found for the actual item).
    /// </summary>
    internal class CodeEditorSearcher
    {
        /// <summary>The f already in path.</summary>
        private System.Collections.Generic.HashSet<Neuron> fAlreadyInPath =
            new System.Collections.Generic.HashSet<Neuron>(); // so we don't get stuck in ethernal loops.

        /// <summary>The f results.</summary>
        private readonly System.Collections.Generic.List<CodeSearchResult> fResults =
            new System.Collections.Generic.List<CodeSearchResult>();

        /// <summary>The f to search.</summary>
        private readonly Neuron fToSearch;

        /// <summary>Initializes a new instance of the <see cref="CodeEditorSearcher"/> class. Initializes a new instance of the <see cref="CodeEditorSearcher"/>
        ///     class.</summary>
        /// <param name="toSearch">To search.</param>
        public CodeEditorSearcher(Neuron toSearch)
        {
            fToSearch = toSearch;
        }

        /// <summary>The display results.</summary>
        internal void DisplayResults()
        {
            BuildResults();
            var iResultSet = new DisplayPathSet();
            iResultSet.Title = BrainData.Current.NeuronInfo[fToSearch].DisplayTitle;
            foreach (var i in fResults)
            {
                if (i.HasPath && i.IsClosed)
                {
                    iResultSet.Items.Add(i.Item);
                }
            }

            SearchResults.Default.Items.Add(iResultSet);
            WindowMain.Current.ActivateTool(ToolsList.Default.SearchResultsTool);

                // make certain it is visible and active
            SearchResults.Default.SelectedIndex = SearchResults.Default.Items.Count - 1;
        }

        /// <summary>The build results.</summary>
        private void BuildResults()
        {
            var iResults = new CodeSearchResult();
            fResults.Add(iResults);
            ContinueSearch(iResults, fToSearch);
        }

        /// <summary>Continues the search from the specified item to the next one. Warning:
        ///     this is a recursive function.</summary>
        /// <param name="results">The results.</param>
        /// <param name="neuron">The neuron.</param>
        private void ContinueSearch(CodeSearchResult results, Neuron neuron)
        {
            System.Collections.Generic.List<Link> iLinks;
            using (var iList = neuron.LinksIn)
                iLinks = new System.Collections.Generic.List<Link>(iList);

                    // make a copy, so we don't keep the list locked while processing it all.
            for (var i = 1; i < iLinks.Count; i++)
            {
                var fPrevPath = new System.Collections.Generic.HashSet<Neuron>(fAlreadyInPath);

                    // make a backup of the hashset, so we can recover. different paths, don't need to check for duplicates in other paths.
                try
                {
                    var iSub = new CodeSearchResult(results);
                    iSub.InsertOutgoingLink(iLinks[i].MeaningID);
                    fResults.Add(iSub);
                    if (IsTerminus(iSub, iLinks[i].From) == false)
                    {
                        ContinueSearch(iSub, iLinks[i].From);
                    }
                }
                finally
                {
                    fAlreadyInPath = fPrevPath;
                }
            }

            if (iLinks.Count > 0)
            {
                var iSub = new CodeSearchResult(results);

                results.InsertOutgoingLink(iLinks[0].MeaningID);
                if (IsTerminus(results, iLinks[0].From) == false)
                {
                    ContinueSearch(results, iLinks[0].From);
                }

                ContinueSearchInClusters(iSub, neuron, false);

                    // we never want to register this item as a termintator, cause it had links and wasn't assigned to be a terminator.
                fResults.Add(iSub);
            }
            else
            {
                ContinueSearchInClusters(results, neuron, true);
            }
        }

        /// <summary>The continue search in clusters.</summary>
        /// <param name="results">The results.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="allowTerminus">The allow terminus.</param>
        private void ContinueSearchInClusters(CodeSearchResult results, Neuron neuron, bool allowTerminus)
        {
            System.Collections.Generic.List<NeuronCluster> iParents = null;
            if (neuron.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iLocks;
                using (var iList = neuron.ClusteredBy) iLocks = neuron.ClusteredBy.ConvertTo<NeuronCluster>();
                try
                {
                    iParents = (from i in iLocks
                                where
                                    i.Meaning == (ulong)PredefinedNeurons.Code
                                    || i.Meaning == (ulong)PredefinedNeurons.Arguments
                                    || i.Meaning == (ulong)PredefinedNeurons.ArgumentsList
                                select i).ToList();
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iLocks);
                }
            }

            if (iParents != null && iParents.Count > 0)
            {
                var iParentsList = new System.Collections.Generic.Dictionary<NeuronCluster, int>();

                    // we only need to keep track of each unique parent, we will ask each position within the parent.
                int iIndex;
                if (iParents.Count > 1)
                {
                    for (var count = 1; count < iParents.Count; count++)
                    {
                        var fPrevPath = new System.Collections.Generic.HashSet<Neuron>(fAlreadyInPath);

                            // make a backup of the hashset, so we can recover. different paths, don't need to check for duplicates in other paths.
                        try
                        {
                            var iCluster = iParents[count];
                            var iSub = new CodeSearchResult(results);
                            iIndex = GetIndex(iParentsList, iCluster, neuron);
                            if (iIndex > -1)
                            {
                                iSub.InsertChild(iIndex);
                                fResults.Add(iSub);
                                if (IsTerminus(iSub, iCluster) == false)
                                {
                                    ContinueSearch(iSub, iCluster);
                                }
                            }
                        }
                        finally
                        {
                            fAlreadyInPath = fPrevPath;
                        }
                    }
                }

                iIndex = GetIndex(iParentsList, iParents[0], neuron);
                results.InsertChild(iIndex);
                if (IsTerminus(results, iParents[0]) == false)
                {
                    ContinueSearch(results, iParents[0]);
                }
            }
            else if (allowTerminus)
            {
                // sometimes, we already found a 'link' terminus, in which case, we don't need to add 2 times.
                results.InsertCodeRoot(neuron);
            }
        }

        /// <summary>Determines whether the specified item is registered as a code editor
        ///     somewhere, or a root cluster.</summary>
        /// <param name="results">The results.</param>
        /// <param name="toCheck">To check.</param>
        /// <returns><c>true</c> if the specified <paramref name="results"/> is terminus;
        ///     otherwise, <c>false</c> .</returns>
        private bool IsTerminus(CodeSearchResult results, Neuron toCheck)
        {
            var iEnd = false;
            if (toCheck.ClusteredByIdentifier != null)
            {
                using (var iList = toCheck.ClusteredBy) iEnd = iList.Count == 0;
            }
            else
            {
                iEnd = true;
            }

            if (iEnd)
            {
                if (toCheck.LinksInIdentifier != null)
                {
                    using (var iLinks = toCheck.LinksIn) iEnd = iLinks.Count == 0;
                }
                else
                {
                    iEnd = true;
                }

                if (iEnd)
                {
                    results.InsertCodeRoot(toCheck);
                    return true;
                }
            }

            if (toCheck.TypeOfNeuronID == (ulong)PredefinedNeurons.Neuron)
            {
                // if it's an actual neuron, it has to be a root, since it can't be  used as code and there  are no more parents.
                results.InsertCodeRoot(toCheck);
                return true;
            }

            var iFound = (from i in BrainData.Current.CodeEditors where i.Item == toCheck select i).FirstOrDefault();
            if (iFound != null)
            {
                results.InsertCodeRoot(toCheck);
                return true;
            }

            var iCluster = toCheck as NeuronCluster;
            if (iCluster != null)
            {
                var iM = iCluster.Meaning;
                if (iM == (ulong)PredefinedNeurons.Frame || iM == (ulong)PredefinedNeurons.FrameSequence
                    || iM == (ulong)PredefinedNeurons.Flow || iM == (ulong)PredefinedNeurons.FlowItemConditional
                    || iM == (ulong)PredefinedNeurons.FlowItemConditionalPart || iM == (ulong)PredefinedNeurons.Object
                    || iM == (ulong)PredefinedNeurons.CompoundWord || iM == (ulong)PredefinedNeurons.POSGroup)
                {
                    results.InsertCodeRoot(toCheck);
                    return true;
                }

                if (iM == (ulong)PredefinedNeurons.Arguments)
                {
                    // arguments lists aren't checked for recursion, that's allowed.
                    return false;
                }
            }

            if (fAlreadyInPath.Contains(toCheck))
            {
                // in case there is a backref in the code, this prevents us from a stack overflow: ethernal loop.
                results.InsertCodeRoot(toCheck);
                return true;
            }

            fAlreadyInPath.Add(toCheck);
            return false;
        }

        /// <summary>Gets the index of an item within a <paramref name="cluster"/> at the
        ///     x'th occurance, depending on the <paramref name="indexes"/> list.</summary>
        /// <param name="indexes">The indexes.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="child">The child.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetIndex(System.Collections.Generic.Dictionary<NeuronCluster, int> indexes, 
            NeuronCluster cluster, 
            Neuron child)
        {
            int iFound;
            if (indexes.TryGetValue(cluster, out iFound) == false)
            {
                indexes.Add(cluster, 0);
                using (var iChildren = cluster.Children) return iChildren.IndexOf(child.ID);
            }

            iFound++;
            indexes[cluster] = iFound;
            IDListAccessor iChildren = cluster.Children;
            iChildren.Lock();
            try
            {
                for (var i = 0; i < iChildren.CountUnsafe; i++)
                {
                    if (iChildren.GetUnsafe(i) == child.ID)
                    {
                        if (iFound == 0)
                        {
                            return i;
                        }
                        else
                        {
                            iFound--;
                        }
                    }
                }
            }
            finally
            {
                iChildren.Dispose(); // also unlocks
            }

            return -1;
        }
    }
}