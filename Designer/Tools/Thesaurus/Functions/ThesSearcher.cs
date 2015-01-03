// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesSearcher.cs" company="">
//   
// </copyright>
// <summary>
//   used to determin which items to search: textneurons, compounds or both.
//   This is stored in the app's settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     used to determin which items to search: textneurons, compounds or both.
    ///     This is stored in the app's settings.
    /// </summary>
    public enum DataToSearch
    {
        /// <summary>The text.</summary>
        Text, 

        /// <summary>The compound.</summary>
        Compound, 

        /// <summary>The all.</summary>
        All
    }

    /// <summary>
    ///     Provides async search for the thesaurus.
    /// </summary>
    internal class ThesSearcher : Search.ISearchDataProvider
    {
        /// <summary>The f current.</summary>
        private ulong fCurrent; // so we can track which is the currently selected item

        /// <summary>The f pos rel.</summary>
        private System.Collections.Generic.HashSet<ulong> fPosRel;

        /// <summary>The f process.</summary>
        private Search.SearcherProcess fProcess;

        /// <summary>The f result group.</summary>
        private Search.DisplayPathSetFolder fResultGroup;

                                            // when we search multiple relationships, we group them together.

        /// <summary>The f result sets.</summary>
        private System.Collections.Generic.Dictionary<ulong, Search.DisplayPathSet> fResultSets;

                                                                                    // the list of result sets that contains the search results and whch is displayed to the user.

        /// <summary>The f to search.</summary>
        private string fToSearch; // the text to search, in case we want to start the same search again.

        /// <summary>Initializes a new instance of the <see cref="ThesSearcher"/> class. Initializes a new instance of the<see cref="EditorsOverviewSearcher"/> class.</summary>
        /// <param name="dataSource">The data source.</param>
        public ThesSearcher(Thesaurus dataSource)
        {
            DataSource = dataSource;
        }

        #region DataSource

        /// <summary>
        ///     Gets the root collection that contains all the editor objects.
        /// </summary>
        public Thesaurus DataSource { get; private set; }

        #endregion

        #region PosRel

        /// <summary>
        ///     Gets the list of pos relationship to look for. We buffer this cause
        ///     it's faster.
        /// </summary>
        public System.Collections.Generic.HashSet<ulong> PosRel
        {
            get
            {
                if (fPosRel == null)
                {
                    fPosRel =
                        new System.Collections.Generic.HashSet<ulong>(
                            from i in DataSource.PosFilters where i != null && i.Item != null select i.Item.ID);
                }

                return fPosRel;
            }
        }

        #endregion

        #region Process

        /// <summary>
        ///     Gets/sets the process that performs the actual search, async.
        /// </summary>
        public Search.SearcherProcess Process
        {
            get
            {
                return fProcess;
            }

            set
            {
                if (fProcess != value)
                {
                    fProcess = value;
                }
            }
        }

        #endregion

        /// <summary>The continue object data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public System.Collections.Generic.IEnumerable<object> ContinueObjectData()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get object data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public System.Collections.Generic.IEnumerable<object> GetObjectData()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The start.</summary>
        /// <param name="toSearch">The to search.</param>
        internal void Start(string toSearch)
        {
            fToSearch = toSearch;
            InternalStart();
        }

        /// <summary>The internal start.</summary>
        private void InternalStart()
        {
            fResultSets = new System.Collections.Generic.Dictionary<ulong, Search.DisplayPathSet>();
            if (Properties.Settings.Default.ThesSearchAllRelationships)
            {
                fResultGroup = new Search.DisplayPathSetFolder();
                fResultGroup.Title = fToSearch;
                Search.SearchResults.Default.Items.Add(fResultGroup);
            }
            else
            {
                var iSet = new Search.DisplayPathSet();
                fResultSets.Add(DataSource.SelectedRelationship.ID, iSet);
                iSet.Title = string.Format(
                    "{0} - {1}", 
                    fToSearch, 
                    BrainData.Current.NeuronInfo[DataSource.SelectedRelationship].DisplayTitle);
                Search.SearchResults.Default.Items.Add(iSet);
            }

            WindowMain.Current.ActivateTool(ToolsList.Default.SearchResultsTool);

                // make certain it is visible and active
            Search.SearchResults.Default.SelectedIndex = Search.SearchResults.Default.Items.Count - 1;
            Process = Search.ProcessTracker.Default.InitSearch(fToSearch, this);
            Process.SearchID += Process_SearchID;
            CalculateTotal();
            Process.FindAllUlong();
        }

        /// <summary>
        ///     Continues this instance.
        /// </summary>
        internal void Continue()
        {
            if (Process != null)
            {
                CalculateTotal(); // recalculate the total, it could have changed since the last search.
                Process.ContinueUlong();
            }
            else
            {
                InternalStart();
            }
        }

        /// <summary>
        ///     Calculates the total nr of items that need to be searched and assigns
        ///     this to the process.
        /// </summary>
        private void CalculateTotal()
        {
            if (Properties.Settings.Default.ThesSearchNeedsExactMatch == false)
            {
                Process.TotalCount = TextSin.Words.Count;
            }
            else
            {
                Process.TotalCount = 1;
            }

            // if (Properties.Settings.Default.ThesSearchAllRelationships
        }

        #region internal types

        /// <summary>
        ///     <see cref="SelectPathsFor" /> is normally recursive, but because some
        ///     paths can be very deep, it's best to not rely on function recursion,
        ///     but implement the recursion self.
        /// </summary>
        private struct ResponseForStackItem
        {
            /// <summary>
            ///     the pos that was found for this display path. So we can build a
            ///     correct result string.
            /// </summary>
            public Neuron POS;

            /// <summary>The alraedy processed.</summary>
            public readonly System.Collections.Generic.HashSet<Neuron> AlraedyProcessed;

            /// <summary>The list.</summary>
            public readonly NeuronCluster List;

            /// <summary>The source.</summary>
            public readonly Search.DisplayPath Source;

            /// <summary>Initializes a new instance of the <see cref="ResponseForStackItem"/> struct. Initializes a new instance of the<see cref="ThesSearcher.ResponseForStackItem"/> struct.</summary>
            /// <param name="source">The source.</param>
            /// <param name="list">The list.</param>
            /// <param name="alraedyProcessed">The alraedy Processed.</param>
            public ResponseForStackItem(
                Search.DisplayPath source, 
                NeuronCluster list, System.Collections.Generic.HashSet<Neuron> alraedyProcessed)
            {
                Source = source;
                List = list;
                AlraedyProcessed = alraedyProcessed;
                POS = null;
            }
        }

        #endregion

        #region ISearchDataProvider Members

        /// <summary>Gets the data that needs to be searched in the form of neuron id's.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> GetData()
        {
            if (Properties.Settings.Default.ThesSearchNeedsExactMatch == false)
            {
                foreach (var i in TextSin.Words.Values)
                {
                    // all the textneurons.
                    var iText = Brain.Current[i] as TextNeuron;
                    System.Diagnostics.Debug.Assert(iText != null);
                    if (Properties.Settings.Default.ThesSearchMapCriterium != DataToSearch.Text)
                    {
                        var iParents = iText.FindAllClusteredBy((ulong)PredefinedNeurons.CompoundWord);

                            // also need to return the compounds. for each textneuron, we ony take the compounds for which it is the first word. This makes certain that we don't return duplicate compounds.
                        Process.TotalCount += iParents.Count;

                            // extra items, so increase the total count, otherwise we report an invalid value.
                        foreach (var u in iParents)
                        {
                            using (var iList = u.Children)
                                if (iList.Count >= 1 && iList[0] == i
                                    && BrainData.Current.NeuronInfo.GetDisplayTitleFor(u.ID).Contains(fToSearch))
                                {
                                    fCurrent = u.ID;
                                    yield return fCurrent;
                                }
                        }
                    }

                    if (Properties.Settings.Default.ThesSearchMapCriterium != DataToSearch.Compound
                        && iText.Text.Contains(fToSearch))
                    {
                        // if the search is not set to only search in compounds, also return the single word.
                        fCurrent = i;
                        yield return fCurrent;
                    }
                }
            }
            else
            {
                var iToSearch = Parsers.Tokenizer.Split(fToSearch);
                ulong iFirstID;
                if (TextSin.Words.TryGetID(iToSearch[0], out iFirstID))
                {
                    if (Properties.Settings.Default.ThesSearchMapCriterium != DataToSearch.Compound)
                    {
                        fCurrent = iFirstID;
                        yield return iFirstID;
                    }

                    if (Properties.Settings.Default.ThesSearchMapCriterium != DataToSearch.Text)
                    {
                        var iAllOk = true;
                        var iItems = new System.Collections.Generic.List<Neuron>();
                        foreach (var iText in iToSearch)
                        {
                            ulong iId;
                            if (TextSin.Words.TryGetID(iToSearch[0], out iId))
                            {
                                iItems.Add(Brain.Current[iId]);
                            }
                            else
                            {
                                iAllOk = false;
                                break;
                            }
                        }

                        if (iAllOk)
                        {
                            var iParents = Neuron.FindCommonParents(iItems);
                            Process.TotalCount += iParents.Count;

                                // extra items, so increase the total count, otherwise we report an invalid value.
                            foreach (var i in iParents)
                            {
                                if (((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord)
                                {
                                    fCurrent = i.ID;
                                    yield return i.ID;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>does the string compare to check if we should try to selected the item
        ///     or not. the <see cref="GetData"/> already did the filtering so we
        ///     always return true.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool Process_SearchID(ulong id)
        {
            return true;
        }

        /// <summary>Continues to get the data from when we stopped at the previous run.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> ContinueData()
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        ///     Tries to build a displayPath for the
        /// </summary>
        public void SelectCurrent()
        {
            var iCur = Brain.Current[fCurrent];
            var iObjects = iCur.FindAllClusteredBy((ulong)PredefinedNeurons.Object);
            if (Properties.Settings.Default.ThesSearchAllRelationships == false)
            {
                SelectCurrentFor(DataSource.SelectedRelationship.ID, iObjects, iCur);
            }
            else
            {
                foreach (var i in DataSource.Relationships)
                {
                    SelectCurrentFor(i.Item.ID, iObjects, iCur);
                }
            }
        }

        /// <summary>The select current for.</summary>
        /// <param name="rel">The rel.</param>
        /// <param name="iObjects">The i objects.</param>
        /// <param name="cur">The cur.</param>
        private void SelectCurrentFor(ulong rel, System.Collections.Generic.List<NeuronCluster> iObjects, Neuron cur)
        {
            foreach (var iObj in iObjects)
            {
                if (Process.IsCanceled)
                {
                    return;
                }

                CheckTerminal(iObj, rel);
                SelectPathsFor(iObj, rel);
                foreach (var iRel in DataSource.NoRecursiveRelationships)
                {
                    if (Process.IsCanceled)
                    {
                        return;
                    }

                    SelectNonRecursiveFor(iObj, iRel.Item, rel);
                }
            }

            foreach (var iRel in DataSource.NoRecursiveRelationships)
            {
                if (Process.IsCanceled)
                {
                    return;
                }

                SelectNonRecursiveFor(cur, iRel.Item, rel);
            }

            SelectLinkRelatedFor(cur, rel);
        }

        /// <summary>Checks if there are any incomming links for the specified<paramref name="item"/> where the linkmeaning is an object, for all
        ///     the to's of those links, get a displaypath.</summary>
        /// <param name="item"></param>
        /// <param name="relationship">The relationship.</param>
        private void SelectLinkRelatedFor(Neuron item, ulong relationship)
        {
            if (Process.IsCanceled)
            {
                return;
            }

            if (item.LinksInIdentifier != null)
            {
                using (var iLinks = item.LinksIn)
                {
                    foreach (var i in iLinks)
                    {
                        if (PosRel.Contains(i.MeaningID) || IsObject(i.MeaningID))
                        {
                            var iItem = new Search.DPIThesaurusRoot();
                            iItem.DataSource = DataSource;
                            iItem.Relationship = relationship;
                            var iDPIPath = new Search.DisplayPath(iItem);
                            iDPIPath.Title = "->" + BrainData.Current.NeuronInfo[item].DisplayTitle;

                                // need to build the title for this path while building the actual path.
                            var iOut = new Search.DPILinkOut(i.MeaningID);
                            iItem.Items.Add(iOut);
                            SelectPathsFor(i.From as NeuronCluster, relationship, iDPIPath);
                        }
                    }
                }
            }
        }

        /// <summary>Determines whether the specified <paramref name="p"/> is an object.</summary>
        /// <param name="p">The p.</param>
        /// <returns><c>true</c> if the specified <paramref name="p"/> is object;
        ///     otherwise, <c>false</c> .</returns>
        private static bool IsObject(ulong p)
        {
            var iCluster = Brain.Current[p] as NeuronCluster;
            return iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Object;
        }

        /// <summary>The select non recursive for.</summary>
        /// <param name="item">The item.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="recursiveRel">The recursive rel.</param>
        private void SelectNonRecursiveFor(Neuron item, Neuron relationship, ulong recursiveRel)
        {
            foreach (var i in item.FindAllClusteredBy(relationship.ID))
            {
                var iItem = new Search.DPIThesaurusRoot();
                iItem.DataSource = DataSource;
                iItem.Relationship = recursiveRel;
                var iDPIPath = new Search.DisplayPath(iItem);
                int iIndex;
                using (var iList = i.Children) iIndex = iList.IndexOf(item.ID);
                var iChild = new Search.DPIChild(iIndex);
                iItem.Items.Add(iChild);
                iDPIPath.Title = string.Format("->{0}[{1}]", BrainData.Current.NeuronInfo[item].DisplayTitle, iIndex);

                    // need to build the title for this path while building the actual path.
                Search.DisplayPath iPath;
                var iNext = i.FindAllIn(relationship.ID);
                foreach (var u in iNext)
                {
                    if (u != iNext[iNext.Count - 1])
                    {
                        iPath = iDPIPath.Duplicate();
                    }
                    else
                    {
                        iPath = iDPIPath;
                    }

                    var iOut = new Search.DPILinkOut(relationship.ID);
                    iItem.Items.Add(iOut);
                    SelectPathsFor(u as NeuronCluster, recursiveRel, iDPIPath);
                }
            }
        }

        /// <summary>this is the starting point for selecting recursive paths. It starts at
        ///     the root. You can supply a <paramref name="start"/> object in case
        ///     that there were non-recursive path items selected first.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="start">The start.</param>
        private void SelectPathsFor(NeuronCluster obj, ulong relationship, Search.DisplayPath start = null)
        {
            if (obj != null)
            {
                var iParents = obj.FindAllClusteredBy(relationship);
                foreach (var i in iParents)
                {
                    if (Process.IsCanceled)
                    {
                        return;
                    }

                    Search.DisplayPath iDPIPath;
                    Search.DPIThesaurusRoot iItem;
                    if (start == null)
                    {
                        iItem = new Search.DPIThesaurusRoot();
                        iItem.DataSource = DataSource;
                        iItem.Relationship = relationship;
                        iDPIPath = new Search.DisplayPath(iItem);
                        iDPIPath.Title = BrainData.Current.NeuronInfo[obj].DisplayTitle;

                            // need to build the title for this path while building the actual path.
                    }
                    else
                    {
                        iItem = (Search.DPIThesaurusRoot)start.Root;
                        iDPIPath = start;
                        iDPIPath.Title = BrainData.Current.NeuronInfo[obj].DisplayTitle + iDPIPath.Title;
                    }

                    Search.DPIChild iChild;
                    using (var iList = i.Children) iChild = new Search.DPIChild(iList.IndexOf(obj.ID));
                    iItem.Items.Add(iChild);
                    var iAlraedyProcessed = new System.Collections.Generic.HashSet<Neuron>();
                    iAlraedyProcessed.Add(obj);
                    var iArgs = new ResponseForStackItem(iDPIPath, i, iAlraedyProcessed);
                    SelectPathsFor(iArgs, relationship);
                }
            }
        }

        /// <summary>this is the continue function, for searching path items other then the
        ///     root.</summary>
        /// <param name="args">The args.</param>
        /// <param name="relationship">The relationship.</param>
        private void SelectPathsFor(ResponseForStackItem args, ulong relationship)
        {
            var iStack = new System.Collections.Generic.Stack<ResponseForStackItem>();
            iStack.Push(args);

            while (iStack.Count > 0)
            {
                var iCur = iStack.Pop();
                var iObj = iCur.List.FindFirstIn(relationship);
                if (iCur.AlraedyProcessed.Contains(iObj))
                {
                    // need to make certain we don't get stuck in a loop cause there is a backreference in the tree.
                    continue;
                }

                iCur.AlraedyProcessed.Add(iObj);
                iCur.Source.Title = string.Format(
                    "{0}.{1}", 
                    BrainData.Current.NeuronInfo[iObj].DisplayTitle, 
                    iCur.Source.Title);

                if (iCur.POS == null)
                {
                    iCur.POS = BrainHelper.GetPosFor(iObj, null);

                        // get the pos value, we dont' need to search a relationship anymore, this is a terminus, so no need for further searching.
                }

                CheckTerminal(iObj, iCur, relationship); // check if this is a terminator. 

                var iParents = iObj.FindAllClusteredBy(relationship);
                foreach (var i in iParents)
                {
                    ResponseForStackItem iNew;
                    if (i == iParents[iParents.Count - 1])
                    {
                        // the last item can reuse the source again, all others need to make a duplciate cause they form a new path.
                        iNew = new ResponseForStackItem(iCur.Source, i, iCur.AlraedyProcessed);
                    }
                    else
                    {
                        iNew = new ResponseForStackItem(
                            iCur.Source.Duplicate(), 
                            i, 
                            new System.Collections.Generic.HashSet<Neuron>(iCur.AlraedyProcessed));
                    }

                    Search.DPIChild iChild;
                    using (var iList = i.Children) iChild = new Search.DPIChild(iList.IndexOf(iObj.ID));
                    iNew.Source.Root.Items.Add(iChild);
                    iStack.Push(iNew);
                }

                if (Process.IsCanceled)
                {
                    return;
                }
            }
        }

        /// <summary>check if the object is a terminal, if so, store the specified
        ///     displayPath as a result.</summary>
        /// <param name="obj"></param>
        /// <param name="source"></param>
        /// <param name="relationship">The relationship.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckTerminal(Neuron obj, ResponseForStackItem source, ulong relationship)
        {
            var iList = DataSource.Data[relationship];
            if (iList != null)
            {
                if (iList.Contains(obj.ID))
                {
                    var iSource = source.Source.Duplicate();

                        // make a copy of the displayPath, cause the item could still be used by other items as well, but can't mix an already valid result with posssible new data.
                    if (source.POS != null)
                    {
                        iSource.Title = string.Format(
                            "{2} : {0}.{1}", 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(source.POS.ID), 
                            iSource.Title, 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(fCurrent));
                    }
                    else
                    {
                        iSource.Title = string.Format(
                            "{0} : {1}", 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(fCurrent), 
                            iSource.Title);
                    }

                    var iRoot = (Search.DPIThesaurusRoot)iSource.Root;
                    iRoot.Item = obj;

                        // also need to save the starting object, otherwise we can't properly build the result again.
                    if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                    {
                        GetResultSet(relationship).Items.Add(iSource);
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Action<Search.DisplayPath>(GetResultSet(relationship).Items.Add), 
                            iSource);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>The check terminal.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="relationship">The relationship.</param>
        private void CheckTerminal(NeuronCluster obj, ulong relationship)
        {
            var iList = DataSource.Data[relationship];
            if (iList != null)
            {
                if (iList.Contains(obj.ID))
                {
                    var iItem = new Search.DPIThesaurusRoot();
                    iItem.DataSource = DataSource;
                    iItem.Relationship = relationship;
                    var iDPIPath = new Search.DisplayPath(iItem);
                    iDPIPath.Title = BrainData.Current.NeuronInfo[obj].DisplayTitle;

                        // need to build the title for this path while building the actual path.
                    var iPos = BrainHelper.GetPosFor(obj, null);

                        // get the pos value, we dont' need to search a relationship anymore, this is a terminus, so no need for further searching.
                    if (iPos != null)
                    {
                        iDPIPath.Title = string.Format(
                            "{2} : {0}.{1}", 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(iPos.ID), 
                            iDPIPath.Title, 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(fCurrent));
                    }
                    else
                    {
                        iDPIPath.Title = string.Format(
                            "{0} : {1}", 
                            BrainData.Current.NeuronInfo.GetDisplayTitleFor(fCurrent), 
                            iDPIPath.Title);
                    }

                    iItem.Item = obj;

                        // also need to save the starting object, otherwise we can't properly build the result again.
                    if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                    {
                        GetResultSet(relationship).Items.Add(iDPIPath);
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Action<Search.DisplayPath>(GetResultSet(relationship).Items.Add), 
                            iDPIPath);
                    }
                }
            }
        }

        /// <summary>gets the resultset for the specified relationship. If one doesn't
        ///     exist yet, it is created.</summary>
        /// <param name="relationship"></param>
        /// <returns>The <see cref="DisplayPathSet"/>.</returns>
        private Search.DisplayPathSet GetResultSet(ulong relationship)
        {
            Search.DisplayPathSet iRes;
            if (fResultSets.TryGetValue(relationship, out iRes) == false)
            {
                iRes = new Search.DisplayPathSet();
                iRes.Title = BrainData.Current.NeuronInfo.GetDisplayTitleFor(relationship);
                fResultSets.Add(relationship, iRes);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new System.Action<Search.BaseDisplayPathSet>(fResultGroup.Items.Add), 
                    iRes);
            }

            return iRes;
        }

        /// <summary>The finished.</summary>
        public void Finished()
        {
            Process.TotalCount = 0; // just to be save.
            Process = null;
            fCurrent = 0;
            if (fResultSets.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "End of search reached, no results found.", 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                Search.DisplayPathSet iRes;
                if (fResultSets.TryGetValue(DataSource.SelectedRelationship.ID, out iRes) == false
                    || iRes.Items.Count == 0)
                {
                    // try to get the first result for the currently selected relationship, if this doesn't exist, go for the first int he result list.
                    foreach (var i in fResultSets.Values)
                    {
                        if (i.Items.Count > 0)
                        {
                            iRes = i;
                            break;
                        }
                    }
                }

                if (iRes.Items.Count > 0)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action(iRes.Items[0].SelectPathResult));
                }
            }
        }

        #endregion
    }
}