// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for QueryEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for QueryEditorView.xaml
    /// </summary>
    public partial class QueryEditorView : System.Windows.Controls.UserControl
    {
        /// <summary>
        ///     Minimum drag distance that needs to be preserved.
        /// </summary>
        private const double DRAGMIN = 12.0;

        /// <summary>Initializes a new instance of the <see cref="QueryEditorView"/> class.</summary>
        public QueryEditorView()
        {
            InitializeComponent();
        }

        /// <summary>Called when a column was moved.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnColMoved(object sender, System.EventArgs args)
        {
            var iEditor = DataContext as QueryEditor;
            if (iEditor != null)
            {
                iEditor.RefreshOutput();
            }
        }

        /// <summary>The thumb col_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbCol_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iEditor = DataContext as QueryEditor;
            var iSender = (System.Windows.Controls.Primitives.Thumb)sender;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iCol = iSender.DataContext as QueryColumn;
            if (iCol != null && iEditor != null)
            {
                var iChangeVal = e.HorizontalChange;
                var iExpected = iCol.Width + e.HorizontalChange;
                if (iExpected > DRAGMIN)
                {
                    // we set a limit to the min size of a column
                    iCol.Width += iChangeVal;
                }
                else
                {
                    iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                    iCol.Width = DRAGMIN;
                }

                LstHeader.InvalidateChildren();
                LstOutput.InvalidateChildren();
            }
        }

        /// <summary>Handles the CanExecute event of the RunQuery control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void RunQuery_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = DataContext as QueryEditor;
            if (iEditor != null)
            {
                var iQuery = (Queries.Query)iEditor.Item;
                e.CanExecute = iQuery != null && string.IsNullOrEmpty(iEditor.Source) == false;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the RunQuery control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void RunQuery_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = DataContext as QueryEditor;
            if (iEditor != null)
            {
                iEditor.Run();
            }
        }

        /// <summary>The extra file_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ExtraFile_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                var iEditor = DataContext as QueryEditor;
                var iSender = sender as System.Windows.Controls.ListBoxItem;
                if (iSender != null && iEditor != null)
                {
                    iEditor.AdditionalFiles.Remove((string)iSender.DataContext);
                }

                e.Handled = true;

                    // if we don't do this, the delete is handled higher up the logical-tree as well, causing the query-neuron to be requested for delete, which we don't want.
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the NewAddtionalFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void NewAddtionalFile_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iEditor = DataContext as QueryEditor;
            if (iEditor != null)
            {
                var iDlg = new Microsoft.Win32.OpenFileDialog();
                iDlg.Filter = Properties.Resources.SourceFilesFilter;
                iDlg.FilterIndex = 1;
                iDlg.Multiselect = true;

                var iRes = iDlg.ShowDialog();
                if (iRes.HasValue && iRes.Value)
                {
                    foreach (var i in iDlg.FileNames)
                    {
                        string iToAdd;
                        if (string.IsNullOrEmpty(Brain.Current.Storage.DataPath) == false)
                        {
                            // try to make the path relative to the project.
                            iToAdd = PathUtil.RelativePathTo(PathUtil.VerifyPathEnd(Brain.Current.Storage.DataPath), i);
                        }
                        else
                        {
                            iToAdd = i;
                        }

                        iEditor.AdditionalFiles.Add(iToAdd);
                    }
                }
            }
        }

        /// <summary>The btn compile_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnCompile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEditor = DataContext as QueryEditor;
            iEditor.Compile();
        }
    }
}