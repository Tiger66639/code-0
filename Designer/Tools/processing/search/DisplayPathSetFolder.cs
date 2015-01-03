// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayPathSetFolder.cs" company="">
//   
// </copyright>
// <summary>
//   Used to group together sets of displaypaths. This can be used to create
//   an extra layer in the result set and group them together for the same
//   editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Used to group together sets of displaypaths. This can be used to create
    ///     an extra layer in the result set and group them together for the same
    ///     editor.
    /// </summary>
    internal class DisplayPathSetFolder : BaseDisplayPathSet
    {
        #region fields

        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<BaseDisplayPathSet> fItems =
            new System.Collections.ObjectModel.ObservableCollection<BaseDisplayPathSet>();

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
    }
}