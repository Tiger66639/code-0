// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for WordNetChannelView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for WordNetChannelView.xaml
    /// </summary>
    public partial class WordNetChannelView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="WordNetChannelView"/> class.</summary>
        public WordNetChannelView()
        {
            InitializeComponent();
        }

        /// <summary>The txt input_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchInWordNetCmd_Executed(sender, null);
            }
        }

        /// <summary>Handles the CanExecute event of the ImportFromWordNetCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportFromWordNetCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iChannel = (WordNetChannel)DataContext;
            e.CanExecute = iChannel != null && !string.IsNullOrEmpty(iChannel.CurrentText);
        }

        /// <summary>Handles the Executed event of the ImportFromWordNetCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportFromWordNetCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMRes =
                System.Windows.MessageBox.Show(
                    string.Format("Load all available WordNet data into the network for '{0}'?", TxtInput.Text), 
                    "Import from WordNet", 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Question);
            if (iMRes == System.Windows.MessageBoxResult.Yes)
            {
                var iChannel = DataContext as WordNetChannel;
                if (iChannel != null)
                {
                    iChannel.ImportWord(iChannel.CurrentText);
                }
            }
        }

        /// <summary>The import all from word net cmd_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ImportAllFromWordNetCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iChannel = (WordNetChannel)DataContext;
            e.CanExecute = iChannel != null && iChannel.Importer == null;

                // can only start an import when there is no prec running.
        }

        /// <summary>The import all from word net cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ImportAllFromWordNetCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMRes =
                System.Windows.MessageBox.Show(
                    "Load the entire WordNet database into the network?\nWarning during this process, the network is automatically streamed to disk!", 
                    "Import from WordNet", 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Warning);
            if (iMRes == System.Windows.MessageBoxResult.Yes)
            {
                var iChannel = (WordNetChannel)DataContext;
                iChannel.Importer = new WordNetImporter();
                iChannel.Importer.Channel = iChannel;
                iChannel.Importer.Import(AfterWordNetImport, iChannel.LastImportedWord);
            }
        }

        /// <summary>Called when the import of the wordnet db is over: used to hide the ui
        ///     section.</summary>
        /// <param name="importer">The importer.</param>
        private void AfterWordNetImport(WordNetImporter importer)
        {
            var iChannel = importer.Channel;
            if (importer.StopRequested)
            {
                iChannel.LastImportedWord = importer.CurrentPosition;
            }
            else
            {
                iChannel.LastImportedWord = importer.CurrentPosition + 1;

                    // make certain we can't import anything,when all was imported.
            }

            iChannel.Importer = null;
        }

        /// <summary>Handles the Executed event of the SearchInWordNetCmd control.</summary>
        /// <remarks>Displays all the available data frm wordnet for the current word in
        ///     the view for the selected relationship.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void SearchInWordNetCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iChannel = (WordNetChannel)DataContext;
            iChannel.LoadDataFor(iChannel.CurrentText);
        }

        /// <summary>The stop import all_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StopImportAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iChannel = (WordNetChannel)DataContext;
            var iImporter = iChannel.Importer;
            System.Diagnostics.Debug.Assert(iImporter != null);
            iImporter.Stop();
        }

        /// <summary>Handles the Click event of the ChkIsLoaded control.</summary>
        /// <remarks>We need to change the order of checkstate, we need: null, true, false,
        ///     while it normally does: null, false, true.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkIsLoaded_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        /// <summary>The check box_ preview mouse down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CheckBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iCheck = sender as System.Windows.Controls.CheckBox;
            System.Diagnostics.Debug.Assert(iCheck != null);
            var iItem = iCheck.DataContext as WordNetItem;
            System.Diagnostics.Debug.Assert(iItem != null);
            var iIsLoaded = iItem.IsLoaded;
            if (iIsLoaded.HasValue == false)
            {
                iItem.IsLoaded = true;
            }
            else
            {
                iItem.IsLoaded = !iIsLoaded.Value;
            }

            e.Handled = true;
        }
    }
}