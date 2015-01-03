// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for VisualEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Editors
{
    /// <summary>
    ///     Interaction logic for VisualEditorView.xaml
    /// </summary>
    public partial class VisualEditorView : CtrlEditorBase
    {
        /// <summary>Initializes a new instance of the <see cref="VisualEditorView"/> class.</summary>
        public VisualEditorView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the SelectionChanged event of the ListBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iEditor = (VisualEditor)DataContext;
            if (iEditor != null && e.AddedItems.Count > 0)
            {
                iEditor.SelectedVisual = e.AddedItems[0] as VisualFrame;
            }
        }

        /// <summary>Handles the Executed event of the AddFrame control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddFrame_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = DataContext as VisualEditor;
            if (iEditor != null)
            {
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iCluster = NeuronFactory.GetCluster();
                    WindowMain.AddItemToBrain(iCluster);
                    iCluster.Meaning = (ulong)PredefinedNeurons.VisualFrame;
                    var iFrame = new VisualFrame(iCluster);
                    iFrame.NeuronInfo.DisplayTitle = "New visual";
                    iFrame.IsLoaded = true;
                    for (var u = 0; u < iEditor.NrVerItems; u++)
                    {
                        for (var i = 0; i < iEditor.NrHorItems; i++)
                        {
                            var iVal = NeuronFactory.GetInt(iEditor.LowValue);
                            WindowMain.AddItemToBrain(iVal);
                            iVal.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, iEditor.LowValOperator);
                            var iItem = new VisualItem(iVal);
                            iFrame.Items.Add(iItem);
                        }
                    }

                    iEditor.Visuals.Add(iFrame);
                    iEditor.SelectedVisual = iFrame;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The repeat nr hor down_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrHorDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEdit = (VisualEditor)DataContext;
            if (iEdit != null)
            {
                iEdit.NrHorItems--;
            }
        }

        /// <summary>The repeat nr hor up_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrHorUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEdit = (VisualEditor)DataContext;
            if (iEdit != null)
            {
                iEdit.NrHorItems++;
            }
        }
    }
}