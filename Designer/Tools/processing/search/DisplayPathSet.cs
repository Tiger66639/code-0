// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayPathSet.cs" company="">
//   
// </copyright>
// <summary>
//   Contains a list of <see cref="DisplayPath" /> s and a title. This is used
//   to group together all the search results of 1 search.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Contains a list of <see cref="DisplayPath" /> s and a title. This is used
    ///     to group together all the search results of 1 search.
    /// </summary>
    public class DisplayPathSet : BaseDisplayPathSet
    {
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

        #region Items

        /// <summary>
        ///     Gets the list of display paths.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<DisplayPath> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region Fields

        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<DisplayPath> fItems =
            new System.Collections.ObjectModel.ObservableCollection<DisplayPath>();

        /// <summary>The f selected index.</summary>
        private int fSelectedIndex;

        #endregion
    }
}