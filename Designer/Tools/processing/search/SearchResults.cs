// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchResults.cs" company="">
//   
// </copyright>
// <summary>
//   The root object that contains all the search results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     The root object that contains all the search results.
    /// </summary>
    public class SearchResults : Data.ObservableObject
    {
        /// <summary>Initializes a new instance of the <see cref="SearchResults"/> class.</summary>
        public SearchResults()
        {
            Brain.Current.Cleared += Network_Cleared;
        }

        #region Default

        /// <summary>
        ///     Gets the default list of results for the app.
        /// </summary>
        public static SearchResults Default
        {
            get
            {
                return fDefault;
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of display paths.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<BaseDisplayPathSet> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region SelectedIndex

        /// <summary>
        ///     Gets/sets the index of the selected item
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return fSelectedIndex;
            }

            set
            {
                fSelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        #endregion

        /// <summary>Handles the Cleared event of the Brain.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Network_Cleared(object sender, System.EventArgs e)
        {
            fItems.Clear();
        }

        #region Fields

        /// <summary>The f selected index.</summary>
        private int fSelectedIndex;

        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<BaseDisplayPathSet> fItems =
            new System.Collections.ObjectModel.ObservableCollection<BaseDisplayPathSet>();

        /// <summary>The f default.</summary>
        private static readonly SearchResults fDefault = new SearchResults();

        #endregion
    }
}