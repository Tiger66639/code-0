// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearcherProcess.cs" company="">
//   
// </copyright>
// <summary>
//   delegate used for callback to do a search on the specified id. This allows for custom search algorithms.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{

    #region SearchID event types

    /// <summary>
    ///     delegate used for callback to do a search on the specified id. This allows for custom search algorithms.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate bool SearchIDHandler(ulong id);

    #endregion

    /// <summary>
    ///     Performs a single search run on the data.
    /// </summary>
    public class SearcherProcess : ProcessTrackerItem
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="SearcherProcess"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="dataSource">The data source to search.</param>
        internal SearcherProcess(ProcessTracker owner, ISearchDataProvider dataSource)
            : base(owner)
        {
            DataSource = dataSource;
        }

        #endregion

        /// <summary>
        ///     Raised when the id needs to be evaluated. The callback needs to return true if the id has to be inclued in the
        ///     result set,
        ///     otherwise false.
        ///     Warning: implementers are responsible for incrementing the current position themseslves.
        /// </summary>
        /// <remarks>
        ///     If no callback is specified, a default function is used that will do a simple substring compare (lowercase).
        /// </remarks>
        public event SearchIDHandler SearchID;

        /// <summary>
        ///     Cancels this search.
        /// </summary>
        internal void Cancel()
        {
            if (IsRunning)
            {
                IsCanceled = true;
            }
            else
            {
                Owner.Searches.Remove(this); // if the search wasn't running, simply remove it from the searches list.
            }
        }

        /// <summary>
        ///     Starts a new search on this datasource in an async way, treating every
        ///     data source as ulong (uses <see cref="ISearchDataProvider.GetData" /> and
        ///     <see cref="ISearchDataProvider.ContinueData" />.
        ///     Once an item is found, the search stops. It can be continued with <see cref="SearcherProcess.ContinueLong" /> .
        /// </summary>
        public void StartUlong()
        {
            if (IsRunning == false)
            {
                // don't start if already running.
                System.Action iAction = InternalStart;
                iAction.BeginInvoke(null, null); // do the search async.
            }
        }

        /// <summary>The find all ulong.</summary>
        public void FindAllUlong()
        {
            if (IsRunning == false)
            {
                // don't start if already running.
                System.Action iAction = InternalFindAll;
                iAction.BeginInvoke(null, null); // do the search async.
            }
        }

        /// <summary>
        ///     Continues to search for the next occurance from where we previously halted, treating every
        ///     data source as ulong (uses <see cref="ISearchDataProvider.GetData" /> and
        ///     <see cref="ISearchDataProvider.ContinueData" />.
        /// </summary>
        public void ContinueUlong()
        {
            if (IsRunning == false)
            {
                // don't start if already running.
                System.Action iAction = InternalContinue;
                iAction.BeginInvoke(null, null); // do the search async.
            }
        }

        /// <summary>
        ///     Starts a new search on this datasource in an async way, treating the data source as either
        ///     ulong  or string (uses <see cref="ISearchDataProvider.GetObjectData" /> and
        ///     <see cref="ISearchDataProvider.ContinueObjectData" />.
        ///     This is a bit slower, but it allows us to pass along custom strings that also need to be matched.
        /// </summary>
        public void StartObjectSearch()
        {
            if (IsRunning == false)
            {
                // don't start if already running.
                System.Action iAction = InternalObjectStart;
                iAction.BeginInvoke(null, null); // do the search async.
            }
        }

        /// <summary>
        ///     Continues to search for the next occurance from where we previously halted, treating every
        ///     data source as ulong (uses <see cref="ISearchDataProvider.GetGetObjectDataData" /> and
        ///     <see cref="ISearchDataProvider.ContinueObjectData" />.
        /// </summary>
        public void ContinueObjectSearch()
        {
            if (IsRunning == false)
            {
                // don't start if already running.
                System.Action iAction = InternalObjectContinue;
                iAction.BeginInvoke(null, null); // do the search async.
            }
        }

        /// <summary>The internal start.</summary>
        private void InternalStart()
        {
            try
            {
                if (IsRunning == false)
                {
                    IsRunning = true;
                    try
                    {
                        CurrentPos = 0;
                        foreach (var i in DataSource.GetData())
                        {
                            if (IsCanceled)
                            {
                                // when we cancel, we need to signal the datasource that we are finished, so only use a break.
                                break;
                            }

                            if (SearchID(i))
                            {
                                DataSource.SelectCurrent();
                                return;
                            }
                        }

                        DataSource.Finished();
                    }
                    finally
                    {
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<SearcherProcess, bool>(Owner.Searches.Remove), 
                            this);
                        IsRunning = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    e.ToString(), 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                DataSource.Finished();
            }
        }

        /// <summary>The internal continue.</summary>
        private void InternalContinue()
        {
            try
            {
                if (IsRunning == false)
                {
                    IsRunning = true;
                    try
                    {
                        foreach (var i in DataSource.ContinueData())
                        {
                            if (IsCanceled)
                            {
                                // when we cancel, we need to signal the datasource that we are finished, so only use a break.
                                break;
                            }

                            if (SearchID(i))
                            {
                                DataSource.SelectCurrent();
                                return;
                            }
                        }

                        DataSource.Finished();
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                    }
                    finally
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<SearcherProcess, bool>(Owner.Searches.Remove), 
                            this);
                        IsRunning = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    e.ToString(), 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                DataSource.Finished();
                TotalCount = 0;

                    // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                CurrentPos = 0;
            }
        }

        /// <summary>The internal find all.</summary>
        private void InternalFindAll()
        {
            try
            {
                if (IsRunning == false)
                {
                    IsRunning = true;
                    try
                    {
                        CurrentPos = 0;
                        foreach (var i in DataSource.GetData())
                        {
                            if (IsCanceled)
                            {
                                // when we cancel, we need to signal the datasource that we are finished, so only use a break.
                                break;
                            }

                            if (SearchID(i))
                            {
                                DataSource.SelectCurrent();
                            }
                        }

                        DataSource.Finished();
                    }
                    finally
                    {
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<SearcherProcess, bool>(Owner.Searches.Remove), 
                            this);
                        IsRunning = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    e.ToString(), 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                DataSource.Finished();
            }
        }

        /// <summary>The internal object start.</summary>
        private void InternalObjectStart()
        {
            try
            {
                if (IsRunning == false)
                {
                    IsRunning = true;
                    try
                    {
                        CurrentPos = 0;
                        foreach (var i in DataSource.GetObjectData())
                        {
                            if (IsCanceled)
                            {
                                // when we cancel, we need to signal the datasource that we are finished, so only use a break.
                                break;
                            }

                            if (i is ulong)
                            {
                                if (SearchID((ulong)i))
                                {
                                    DataSource.SelectCurrent();
                                    return;
                                }
                            }
                            else if (i is string)
                            {
                                if (CompareTo((string)i))
                                {
                                    DataSource.SelectCurrent();
                                    return;
                                }
                            }
                        }

                        DataSource.Finished();
                    }
                    finally
                    {
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<SearcherProcess, bool>(Owner.Searches.Remove), 
                            this);
                        IsRunning = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    e.ToString(), 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                DataSource.Finished();
            }
        }

        /// <summary>The internal object continue.</summary>
        private void InternalObjectContinue()
        {
            try
            {
                if (IsRunning == false)
                {
                    IsRunning = true;
                    try
                    {
                        foreach (var i in DataSource.ContinueObjectData())
                        {
                            if (IsCanceled)
                            {
                                // when we cancel, we need to signal the datasource that we are finished, so only use a break.
                                break;
                            }

                            if (i is ulong)
                            {
                                if (SearchID((ulong)i))
                                {
                                    DataSource.SelectCurrent();
                                    return;
                                }
                            }
                            else if (i is string)
                            {
                                if (CompareTo((string)i))
                                {
                                    DataSource.SelectCurrent();
                                    return;
                                }
                            }
                        }

                        DataSource.Finished();
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                    }
                    finally
                    {
                        TotalCount = 0;

                            // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                        CurrentPos = 0;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<SearcherProcess, bool>(Owner.Searches.Remove), 
                            this);
                        IsRunning = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    e.ToString(), 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                DataSource.Finished();
                TotalCount = 0;

                    // always need to rest the total count when the processing is finished, otherwise the totalCounter keeps increasing. Do before finished, cause this can reset the Process prop
                CurrentPos = 0;
            }
        }

        /// <summary>Performs a search for the text on the object with the specified id.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool InternalSearchId(ulong i)
        {
            CurrentPos++;
            var iText = BrainData.Current.NeuronInfo.GetDisplayTitleFor(i);
            if (iText != null)
            {
                return CompareTo(iText);
            }

            return false;
        }

        /// <summary>Compares the text to search to the argument and tells the datasource to select the current item if there is a
        ///     match.</summary>
        /// <param name="text">The text.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareTo(string text)
        {
            if (text != null)
            {
                text = text.ToLower();
                if (text.Contains(TextToSearch))
                {
                    return true;
                }
            }

            return false;
        }

        #region Prop

        #region DataSource

        /// <summary>
        ///     Gets the datasource that provides the id's who need to be searched.
        /// </summary>
        public ISearchDataProvider DataSource { get; internal set; }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets the value that indicates if this search is still running or not.
        /// </summary>
        public override bool IsRunning
        {
            get
            {
                return base.IsRunning;
            }

            internal set
            {
                if (value != base.IsRunning)
                {
                    base.IsRunning = value;
                    if (value && SearchID == null)
                    {
                        SearchID += InternalSearchId;
                    }
                }
            }
        }

        #endregion

        #region TextToSearch

        /// <summary>
        ///     Gets/sets the text to search for.
        /// </summary>
        public string TextToSearch { get; set; }

        #endregion

        #endregion
    }
}