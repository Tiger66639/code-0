// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchResources.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SearchResources.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Interaction logic for SearchResources.xaml
    /// </summary>
    public partial class SearchResources : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="SearchResources"/> class.</summary>
        public SearchResources()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the CloseSearchSet control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseSearchSet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SearchResults.Default.SelectedIndex > -1)
            {
                SearchResults.Default.Items.RemoveAt(SearchResults.Default.SelectedIndex);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Handles the Click event of the CloseAllSearchSetsButActive control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseAllSearchSetsButActive_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SearchResults.Default.SelectedIndex > -1)
            {
                var iSet = SearchResults.Default.Items[SearchResults.Default.SelectedIndex];
                SearchResults.Default.Items.Clear();
                SearchResults.Default.Items.Add(iSet);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Handles the Click event of the CloseAllSearchSets control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseAllSearchSets_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchResults.Default.Items.Clear();
        }

        /// <summary>Handles the MouseDoubleClick event of the <see cref="DisplayPath"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void DisplayPath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            var iTreeItem = iSender as System.Windows.Controls.TreeViewItem;
            if (iTreeItem != null)
            {
                iTreeItem.IsSelected = true;

                    // treeview items and listbox items may be selected when clicked upon. treeviews aren't focusable, so we need to do this for them.
            }

            var iPath = iSender.DataContext as DisplayPath;
            System.Diagnostics.Debug.Assert(iPath != null);
            iPath.SelectPathResult();
        }
    }
}