// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectInputDlg.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SelectInputDlg.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for SelectInputDlg.xaml
    /// </summary>
    public partial class SelectInputDlg : System.Windows.Window
    {
        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<InputSelectionItem> fItems;

        /// <summary>Initializes a new instance of the <see cref="SelectInputDlg"/> class.</summary>
        /// <param name="items">The items.</param>
        public SelectInputDlg(System.Collections.Generic.List<InputSelectionItem> items)
        {
            fItems = items;
            InitializeComponent();
            ListItems.ItemsSource = items;
        }

        /// <summary>
        ///     Gets/sets the index of the item that was selected.
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>Handles the Click event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.Button;
            SelectedIndex = (int)iSender.Tag; // the Tag links to the index nr.
            DialogResult = true;
        }
    }
}