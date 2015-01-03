// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessTracker.cs" company="">
//   
// </copyright>
// <summary>
//   Provides advanced search capabilities and monitoring for the designer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Provides advanced search capabilities and monitoring for the designer.
    /// </summary>
    public class ProcessTracker : Data.ObservableObject
    {
        /// <summary>Attaches the search to this overview.</summary>
        /// <param name="toSearch">To search.</param>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The <see cref="SearcherProcess"/>.</returns>
        public SearcherProcess InitSearch(string toSearch, ISearchDataProvider dataSource)
        {
            var iRes = new SearcherProcess(this, dataSource);
            iRes.TextToSearch = toSearch;
            fSearches.Add(iRes);
            return iRes;
        }

        /// <summary>Attaches a new general purpose process tracker to the the system.</summary>
        /// <returns>The <see cref="ProcessTrackerItem"/>.</returns>
        public ProcessTrackerItem InitProcess()
        {
            var iNew = new ProcessTrackerItem(this);
            fSearches.Add(iNew);
            return iNew;
        }

        /// <summary>Attaches the search to this overview.</summary>
        /// <param name="callback">The callback.</param>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The <see cref="SearcherProcess"/>.</returns>
        public SearcherProcess InitSearch(SearchIDHandler callback, ISearchDataProvider dataSource)
        {
            var iRes = new SearcherProcess(this, dataSource);
            iRes.SearchID += callback;
            fSearches.Add(iRes);
            return iRes;
        }

        #region fields

        /// <summary>The f total count.</summary>
        private double fTotalCount;

        /// <summary>The f total lock.</summary>
        private readonly object fTotalLock = new object();

        /// <summary>The f current pos.</summary>
        private double fCurrentPos;

        /// <summary>The f current lock.</summary>
        private readonly object fCurrentLock = new object();

        /// <summary>The f run count.</summary>
        private int fRunCount; // keeps track of the number of searches currently running (doing a search).

        /// <summary>The f run count lock.</summary>
        private readonly object fRunCountLock = new object();

        /// <summary>The f searches.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<ProcessTrackerItem> fSearches =
            new System.Collections.ObjectModel.ObservableCollection<ProcessTrackerItem>();

        /// <summary>The f default.</summary>
        private static readonly ProcessTracker fDefault = new ProcessTracker();

        #endregion

        #region prop

        #region Default

        /// <summary>
        ///     Gets the default searcher object.
        /// </summary>
        public static ProcessTracker Default
        {
            get
            {
                return fDefault;
            }
        }

        #endregion

        #region TotalCount

        /// <summary>
        ///     Gets the total nr of items that still need to be processed, for all
        ///     currently running searches.
        /// </summary>
        public double TotalCount
        {
            get
            {
                return fTotalCount;
            }

            private set
            {
                fTotalCount = value;
                OnPropertyChanged("TotalCount");
            }
        }

        #endregion

        #region CurrentPos

        /// <summary>
        ///     Gets the position of the current item that is being/has been
        ///     processed.
        /// </summary>
        public double CurrentPos
        {
            get
            {
                return fCurrentPos;
            }

            private set
            {
                fCurrentPos = value;
                OnPropertyChanged("CurrentPos");
            }
        }

        #endregion

        #region Searches

        /// <summary>
        ///     Gets the list of all the searches that are currently running.
        /// </summary>
        /// <value>
        ///     The searches.
        /// </value>
        public System.Collections.ObjectModel.ObservableCollection<ProcessTrackerItem> Searches
        {
            get
            {
                return fSearches;
            }
        }

        #endregion

        #region IsSearching

        /// <summary>
        ///     Gets a value indicating whether the searcher is currently searching.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is searching; otherwise, <c>false</c> .
        /// </value>
        public bool IsSearching
        {
            get
            {
                return fRunCount > 0;
            }
        }

        /// <summary>The inc run count.</summary>
        internal void IncRunCount()
        {
            lock (fRunCountLock)
            {
                fRunCount++;
                if (fRunCount == 1)
                {
                    OnPropertyChanged("IsSearching");
                }
                else if (fRunCount == 2)
                {
                    OnPropertyChanged("HasMultipleSearches");
                }
            }
        }

        /// <summary>The dec run count.</summary>
        internal void DecRunCount()
        {
            lock (fRunCountLock)
            {
                fRunCount--;
                if (fRunCount == 1)
                {
                    OnPropertyChanged("HasMultipleSearches");
                }
                else if (fRunCount == 0)
                {
                    OnPropertyChanged("IsSearching");
                }
            }
        }

        #endregion

        #region HasMultipleSearches

        /// <summary>
        ///     Gets a value indicating whether this instance has multiple searches
        ///     running at the same time.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has multiple searches; otherwise,
        ///     <c>false</c> .
        /// </value>
        public bool HasMultipleSearches
        {
            get
            {
                return fSearches.Count > 1;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Cancels all the currently running searches.
        /// </summary>
        public void CancelAll()
        {
            foreach (SearcherProcess i in Searches)
            {
                i.Cancel();
            }
        }

        /// <summary>Updates the total count in a thread save way.</summary>
        /// <param name="toAdd">To value to add. Provide a negative value to do a substraction.</param>
        internal void UpdateTotalCount(double toAdd)
        {
            lock (fTotalLock) TotalCount += toAdd;
        }

        /// <summary>Updates the Current pos in a thread save way.</summary>
        /// <param name="toAdd">To value to add. Provide a negative value to do a substraction.</param>
        internal void UpdateCurrentPos(double toAdd)
        {
            lock (fCurrentLock) CurrentPos += toAdd;
        }

        #endregion
    }
}