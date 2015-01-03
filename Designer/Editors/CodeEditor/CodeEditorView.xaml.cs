// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CodeEditor.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditorView : CtrlEditorBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeEditorView" /> class.
        /// </summary>
        public CodeEditorView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the MnuAddCodePage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuAddCodePage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                var iData = iSender.DataContext as NeuronData;
                AddNewCodePage(iData);
            }
        }

        /// <summary>Adds the new code page.</summary>
        /// <param name="iData">The i data.</param>
        private void AddNewCodePage(NeuronData iData)
        {
            if (iData != null)
            {
                var iEditor = (CodeEditor)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                iEditor.EntryPoints.Add(new CodeEditorPage(iData.DisplayTitle, iEditor.Item, iData.ID));

                    // only initiate the cluster when items are assigned to it.
                iEditor.SelectedIndex = iEditor.EntryPoints.Count - 1;
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the LstPossiblePages control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void LstPossiblePages_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            AddNewCodePage(LstPossiblePages.SelectedItem as NeuronData);
            PopupAddPage.IsOpen = false;
        }

        #region RunNeuron

        /// <summary>Handles the Executed event of the RunNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RunNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = ProcessorFactory.GetProcessor();
            var iPage = TabPages.SelectedItem as CodeEditorPage;
            iProc.CallSingle(iPage.Items.Cluster);

                // this is async, otherwise the ui can get stuck because the debugger is doing a thread sync by waiting.
        }

        /// <summary>Handles the CanExecute event of the RunNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RunNeuron_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        #region Delete

        /// <summary>Handles the Click event of the MnuItemRemovePage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuItemRemovePage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iRes = System.Windows.MessageBox.Show(
                "Remove the entire code page, and delete when no longer used?", 
                "Delete page", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);
            if (iRes == System.Windows.MessageBoxResult.Yes)
            {
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iEditor = (CodeEditor)DataContext;
                    var iPage = TabPages.SelectedItem as CodeEditorPage;
                    Neuron iCluster = iPage.Items.Cluster;
                    iEditor.EntryPoints.Remove(iPage);
                    var iFound = Link.Find(iEditor.Item, iCluster, Brain.Current[iPage.LinkMeaning]);

                        // remove the reference between the neuron and the cluster
                    if (iFound != null)
                    {
                        var iUndo = new LinkUndoItem(iFound, BrainAction.Removed);
                        WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                        iFound.Destroy();
                    }

                    if (iCluster.ID != Neuron.TempId)
                    {
                        // if the code page is empty and the cluster is just a temp object, don't ask it to delete cause it hasn't been registered yet.
                        var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
                        iDeleter.Start(iCluster);
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>Handles the Opened event of the CodeEditorViewContextMenu control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CodeEditorViewContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEditor = (CodeEditor)DataContext;
            MnuItemRemovePage.IsEnabled = iEditor.SelectedIndex >= iEditor.FixedNrEntryPoints;
        }

        #endregion
    }
}