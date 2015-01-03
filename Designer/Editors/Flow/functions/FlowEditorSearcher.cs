// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowEditorSearcher.cs" company="">
//   
// </copyright>
// <summary>
//   The flow editor searcher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The flow editor searcher.</summary>
    internal class FlowEditorSearcher : Search.ISearchDataProvider
    {
        /// <summary>The f cur flow.</summary>
        private int fCurFlow; // the index of the current flow we are handling.

        /// <summary>The f next root.</summary>
        private NeuronCluster fNextRoot; // if GetCurrent needs to go to a new root, this is it.

        /// <summary>The f path.</summary>
        private System.Collections.Generic.List<int> fPath;

                                                     // stores the indexes of the current position in each of the collections (the treepath for current index).

        /// <summary>The f path sources.</summary>
        private System.Collections.Generic.List<NeuronCluster> fPathSources;

                                                               // so we can revert to the parent source again when a child list has been handled.

        /// <summary>The f to search.</summary>
        private string fToSearch; // the text to search, in case we want to start the same search again.

        /// <summary>Initializes a new instance of the <see cref="FlowEditorSearcher"/> class. Initializes a new instance of the<see cref="EditorsOverviewSearcher"/> class.</summary>
        /// <param name="dataSource">The data source.</param>
        public FlowEditorSearcher(FlowEditor dataSource)
        {
            DataSource = dataSource;
        }

        #region DataSource

        /// <summary>
        ///     Gets the root collection that contains all the editor objects.
        /// </summary>
        public FlowEditor DataSource { get; private set; }

        #endregion

        #region Process

        /// <summary>
        ///     Gets/sets the process that performs the actual search, async.
        /// </summary>
        public Search.SearcherProcess Process { get; set; }

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
            Process = Search.ProcessTracker.Default.InitSearch(fToSearch, this);
            CalculateTotal();
            Process.StartUlong();
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
            Process.TotalCount = DataSource.Flows.Count;
        }

        #region ISearchDataProvider Members

        /// <summary>The get data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> GetData()
        {
            fNextRoot = null;
            fPath = new System.Collections.Generic.List<int>();
            fPathSources = new System.Collections.Generic.List<NeuronCluster>();
            fCurFlow = -1; // we increment just before we get the item, so init to -1 to start with item at 0.
            return InternalGetData();
        }

        /// <summary>The continue data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> ContinueData()
        {
            return InternalGetData();
        }

        /// <summary>The internal get data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.Generic.IEnumerable<ulong> InternalGetData()
        {
            var iCurrent = GetCurrent();
            while (iCurrent == null && fPath.Count > 0)
            {
                fPath.RemoveAt(fPath.Count - 1);
                fPathSources.RemoveAt(fPathSources.Count - 1);
                iCurrent = GetCurrent();
            }

            while (iCurrent != null)
            {
                fNextRoot = iCurrent as NeuronCluster;
                if (fPath.Count > 0 && fNextRoot != null && fNextRoot.Meaning == (ulong)PredefinedNeurons.Flow)
                {
                    // we don't process the children of flows when they are used as statics, only when they are roots.
                    fNextRoot = null;
                }

                yield return iCurrent.ID;
                iCurrent = GetCurrent();
                while (iCurrent == null && fPath.Count > 0)
                {
                    fPath.RemoveAt(fPath.Count - 1);
                    fPathSources.RemoveAt(fPathSources.Count - 1);
                    iCurrent = GetCurrent();
                }
            }
        }

        /// <summary>Gets the neuron at the current position. After the call, we have
        ///     incremented the pos to the next current.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetCurrent()
        {
            Neuron iCurrent = null;
            if (fPath.Count == 0 && fNextRoot == null)
            {
                fCurFlow++;
                if (fCurFlow < DataSource.Flows.Count)
                {
                    iCurrent = DataSource.Flows[fCurFlow].Item;
                }
            }
            else
            {
                if (fNextRoot != null)
                {
                    // add the next position before we return the current value.
                    using (var iList = fNextRoot.Children)
                        Process.TotalCount += iList.Count;

                            // we need to add the children for processing, otherwise the currentpos gets screwed up.
                    if (fPathSources.Contains(fNextRoot) == false)
                    {
                        // try to prevent a circular ref otherwise we are fucked.
                        fPathSources.Add(fNextRoot);
                        fPath.Add(0);
                    }

                    fNextRoot = null;
                }

                var iCluster = fPathSources[fPathSources.Count - 1];
                var iChildren = iCluster.Children;
                iChildren.Lock();
                try
                {
                    var iPos = fPath.Count - 1;
                    if (iChildren.CountUnsafe > fPath[iPos])
                    {
                        iCurrent = Brain.Current[iChildren.GetUnsafe(fPath[iPos])];
                        fPath[iPos]++; // when we have the current pos, increment to the next.
                    }
                }
                finally
                {
                    iChildren.Dispose(); // also unlocks.
                }
            }

            return iCurrent;
        }

        /// <summary>The select current.</summary>
        public void SelectCurrent()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(SelectCurrentAsync));

                // the IsSelected needs to be set from the main thread, otherwise we might get errors from ICollectionChanged objects.
        }

        /// <summary>The select current async.</summary>
        private void SelectCurrentAsync()
        {
            FlowItem iCur = null;
            var iFlow = DataSource.Flows[fCurFlow];
            iFlow.IsSelected = true;
            if (fPath.Count != 0)
            {
                var iSource = iFlow.Items;
                foreach (var i in fPath)
                {
                    iCur = iSource[i - 1]; // -1 cause the index has already been incremented to point to the next item.
                    var iblock = iCur as FlowItemBlock;
                    if (iblock != null)
                    {
                        iSource = iblock.Items;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (iCur != null)
            {
                iCur.IsSelected = true; // when selecting, the item is automatically scrolled into view.
            }
        }

        /// <summary>The finished.</summary>
        public void Finished()
        {
            Process.TotalCount = 0;

                // reset, otherwise the count remains up, giving errors in the process-tracker for next items.
            Process = null;
            fPath = null;
            fPathSources = null;
            System.Windows.MessageBox.Show(
                "End of search reached.", 
                "Search", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Asterisk);
        }

        #endregion
    }
}