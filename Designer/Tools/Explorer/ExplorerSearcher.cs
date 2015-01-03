// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerSearcher.cs" company="">
//   
// </copyright>
// <summary>
//   The explorer searcher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The explorer searcher.</summary>
    internal class ExplorerSearcher : Search.ISearchDataProvider
    {
        /// <summary>The f current.</summary>
        private ulong fCurrent;

        /// <summary>The f to search.</summary>
        private string fToSearch; // the text to search, in case we want to start the same search again.

        /// <summary>Initializes a new instance of the <see cref="ExplorerSearcher"/> class. Initializes a new instance of the<see cref="EditorsOverviewSearcher"/> class.</summary>
        /// <param name="dataSource">The data source.</param>
        public ExplorerSearcher(NeuronExplorer dataSource)
        {
            DataSource = dataSource;
        }

        #region DataSource

        /// <summary>
        ///     Gets the root collection that contains all the editor objects.
        /// </summary>
        public NeuronExplorer DataSource { get; private set; }

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
            Process.TotalCount = Brain.Current.NextID;
            fCurrent = 0;
            Process.StartUlong();
        }

        /// <summary>
        ///     Continues this instance.
        /// </summary>
        internal void Continue()
        {
            if (Process != null)
            {
                Process.TotalCount = Brain.Current.NextID;

                    // recalculate the total, it could have changed since the last search.
                Process.ContinueUlong();
            }
            else
            {
                InternalStart();
            }
        }

        #region ISearchDataProvider Members

        /// <summary>Gets the data that needs to be searched in the form of neuron id's.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> GetData()
        {
            return InternalGetData();
        }

        /// <summary>Continues to get the data from when we stopped at the previous run.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<ulong> ContinueData()
        {
            fCurrent++; // we continue from a prev pos, so advance 1 before we continue.
            return InternalGetData();
        }

        /// <summary>The internal get data.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.Generic.IEnumerable<ulong> InternalGetData()
        {
            while (fCurrent < Brain.Current.NextID)
            {
                if (Brain.Current.IsExistingID(fCurrent))
                {
                    yield return fCurrent;
                }
                else
                {
                    Process.CurrentPos++;

                        // we advance the pos manually cause we aren't returning a value, but we did include it with the total count.
                }

                fCurrent++;
            }
        }

        /// <summary>
        ///     This function is called when a valid search result is found and the
        ///     object at the current pointer position should be selected.
        /// </summary>
        public void SelectCurrent()
        {
            System.Action iAsync = InternalSetlectCurrent;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(iAsync);
        }

        /// <summary>The internal setlect current.</summary>
        private void InternalSetlectCurrent()
        {
            DataSource.Selection.SelectedID = fCurrent;
        }

        /// <summary>The finished.</summary>
        public void Finished()
        {
            Process.TotalCount = 0;

                // reset, otherwise the count remains up, giving errors in the process-tracker for next items.
            Process = null;
            System.Windows.MessageBox.Show(
                "End of search reached.", 
                "Search", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Asterisk);
        }

        #endregion
    }
}