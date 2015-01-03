// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorsOverviewSearcher.cs" company="">
//   
// </copyright>
// <summary>
//   Provides search capabilities to the <see cref="EditorsOverview" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Provides search capabilities to the <see cref="EditorsOverview" />
    /// </summary>
    internal class EditorsOverviewSearcher : Search.ISearchDataProvider
    {
        /// <summary>The f path.</summary>
        private System.Collections.Generic.List<int> fPath;

                                                     // stores the indexes of the current position in each of the collections (the treepath for current index).

        /// <summary>The f path sources.</summary>
        private System.Collections.Generic.List<EditorCollection> fPathSources;

                                                                  // so we can revert to the parent source again when a child list has been handled.

        /// <summary>The f to search.</summary>
        private string fToSearch; // the text to search, in case we want to start the same search again.

        /// <summary>Initializes a new instance of the <see cref="EditorsOverviewSearcher"/> class. Initializes a new instance of the<see cref="EditorsOverviewSearcher"/> class.</summary>
        /// <param name="dataSource">The data source.</param>
        public EditorsOverviewSearcher(EditorCollection dataSource)
        {
            DataSource = dataSource;
        }

        #region DataSource

        /// <summary>
        ///     Gets the root collection that contains all the editor objects.
        /// </summary>
        public EditorCollection DataSource { get; private set; }

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
            var iFolders = (from i in DataSource where i is EditorFolder select (EditorFolder)i).ToArray();
            Process.TotalCount = DataSource.Count - iFolders.Length;
            while (iFolders.Length > 0)
            {
                var iNewSubs = new System.Collections.Generic.List<EditorFolder>();
                foreach (var iSource in iFolders)
                {
                    var iTemp = (from i in iSource.Items where i is EditorFolder select (EditorFolder)i).ToArray();
                    Process.TotalCount += iSource.Items.Count - iTemp.Length;
                    iNewSubs.AddRange(iTemp);
                }

                iFolders = iNewSubs.ToArray();
            }
        }

        #region ISearchDataProvider Members

        /// <summary>The get data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> GetData()
        {
            fPath = new System.Collections.Generic.List<int>();
            fPath.Add(0);
            fPathSources = new System.Collections.Generic.List<EditorCollection>();
            fPathSources.Add(DataSource);
            return InternalGetData(0);
        }

        /// <summary>The continue data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> ContinueData()
        {
            return InternalGetData(fPath.Count - 1);
        }

        /// <summary>The internal get data.</summary>
        /// <param name="iPathIndex">The i path index.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.Generic.IEnumerable<ulong> InternalGetData(int iPathIndex)
        {
            EditorCollection iParents;
            while (iPathIndex >= 0)
            {
                iParents = fPathSources[iPathIndex];
                var i = fPath[iPathIndex];
                while (i < iParents.Count)
                {
                    fPath[iPathIndex] = i + 1;
                    var iFolder = iParents[i] as EditorFolder;
                    if (iFolder != null)
                    {
                        fPath.Add(0); // new folder, so also new item on the path.
                        iPathIndex++;
                        iParents = iFolder.Items;
                        fPathSources.Add(iParents);
                        i = 0;
                    }
                    else
                    {
                        var iNeuronEditor = iParents[i] as NeuronEditor;
                        if (iNeuronEditor != null)
                        {
                            yield return iNeuronEditor.ItemID;
                        }

                        i++;
                    }
                }

                fPath.RemoveAt(iPathIndex);
                fPathSources.RemoveAt(iPathIndex);
                iPathIndex--;
            }
        }

        /// <summary>The select current.</summary>
        public void SelectCurrent()
        {
            var iParent = DataSource;
            var iCur = fPathSources[fPathSources.Count - 1][fPath[fPath.Count - 1] - 1];
            foreach (var i in fPath)
            {
                if (iParent != null)
                {
                    var iFolder = iParent[i - 1] as EditorFolder;
                    if (iFolder != null)
                    {
                        iParent = iFolder.Items;
                        iFolder.IsExpanded = true;
                    }
                    else
                    {
                        iParent = null;
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