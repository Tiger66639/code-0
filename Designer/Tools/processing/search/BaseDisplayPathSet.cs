// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseDisplayPathSet.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for DisplayPathSets. It allows us to add an extra layer
//   (folders), betwen the display paths and a single reset set. To group by
//   'editors' for instance.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     A base class for DisplayPathSets. It allows us to add an extra layer
    ///     (folders), betwen the display paths and a single reset set. To group by
    ///     'editors' for instance.
    /// </summary>
    public class BaseDisplayPathSet : Data.ObservableObject
    {
        #region fields

        /// <summary>The f title.</summary>
        private string fTitle;

        #endregion

        #region Title

        /// <summary>
        ///     Gets/sets the title of the search.
        /// </summary>
        public string Title
        {
            get
            {
                return fTitle;
            }

            set
            {
                fTitle = value;
                OnPropertyChanged("Title");
            }
        }

        #endregion
    }
}