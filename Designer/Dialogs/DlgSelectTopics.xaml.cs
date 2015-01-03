// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectTopics.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgSelectTopics.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Dialogs
{
    /// <summary>
    ///     Interaction logic for DlgSelectTopics.xaml
    /// </summary>
    public partial class DlgSelectTopics : System.Windows.Window
    {
        /// <summary>The f selected.</summary>
        private readonly System.Collections.Generic.HashSet<TextPatternEditor> fSelected =
            new System.Collections.Generic.HashSet<TextPatternEditor>();

        /// <summary>Initializes a new instance of the <see cref="DlgSelectTopics"/> class.</summary>
        public DlgSelectTopics()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets the items that should be displyaed.
        /// </summary>
        public System.Collections.IEnumerator Items
        {
            get
            {
                return new BrowsableTopicsEnumerator(BrainData.Current.Editors, true);
            }
        }

        /// <summary>
        ///     Gets the selected items.
        /// </summary>
        public System.Collections.Generic.IEnumerable<TextPatternEditor> SelectedItems
        {
            get
            {
                return fSelected;
            }
        }

        /// <summary>Handles the Selected event of the TreeView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TreeView_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iItem = e.OriginalSource as System.Windows.Controls.CheckBox;
            fSelected.Add((TextPatternEditor)iItem.DataContext);
        }

        /// <summary>Handles the Unselected event of the TreeView control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TreeView_Unselected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iItem = e.OriginalSource as System.Windows.Controls.CheckBox;
            fSelected.Remove((TextPatternEditor)iItem.DataContext);
        }

        /// <summary>Called when [click ok].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}