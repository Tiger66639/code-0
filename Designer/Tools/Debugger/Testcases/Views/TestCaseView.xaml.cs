// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCaseView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for TestCaseView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     Interaction logic for TestCaseView.xaml
    /// </summary>
    public partial class TestCaseView : System.Windows.Controls.UserControl
    {
        /// <summary>
        ///     Size of the expand/collaps button.
        /// </summary>
        private const double TOGGLEBUTTONSIZE = 19.0;

        /// <summary>The splitsize.</summary>
        private const double SPLITSIZE = 3.0;

        /// <summary>
        ///     Minimum drag distance that needs to be preserved.
        /// </summary>
        private const double DRAGMIN = 12.0;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TestCaseView"/> class.</summary>
        public TestCaseView()
        {
            InitializeComponent();
        }

        #endregion

        #region Thumbs

        /// <summary>The thumb result_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbResult_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                var iChangeVal = e.HorizontalChange;
                var iExpected = iEditor.ResultWidth + e.HorizontalChange;
                if (iExpected > DRAGMIN)
                {
                    iEditor.ResultWidth += iChangeVal;
                }
                else
                {
                    iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                    iEditor.ResultWidth = DRAGMIN;
                }

                foreach (System.Windows.FrameworkElement i in TestCasePanel.Children)
                {
                    // all the visible elements also need to be adjusted. doing it this way, saves CPU -> easier binding (can't easely bind to 'root' object).
                    var iRec = i.DataContext as TestCaseItem;
                    System.Diagnostics.Debug.Assert(iRec != null);
                    iRec.ResultWidth += e.HorizontalChange;
                    iRec.RunWidth -= e.HorizontalChange;
                    i.InvalidateMeasure();
                }
            }
        }

        /// <summary>The thumb is enabled_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbIsEnabled_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                var iChangeVal = e.HorizontalChange;
                var iExpected = iEditor.IsEnabledWidth + e.HorizontalChange;
                if (iExpected > DRAGMIN)
                {
                    iEditor.IsEnabledWidth += iChangeVal;
                }
                else
                {
                    iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                    iEditor.IsEnabledWidth = DRAGMIN;
                }

                foreach (System.Windows.FrameworkElement i in TestCasePanel.Children)
                {
                    // all the visible elements also need to be adjusted. doing it this way, saves CPU -> easier binding (can't easely bind to 'root' object).
                    var iRec = i.DataContext as TestCaseItem;
                    System.Diagnostics.Debug.Assert(iRec != null);
                    iRec.IsEnabledWidth += iChangeVal;
                    iRec.RunWidth -= iChangeVal;
                    i.InvalidateMeasure();
                }
            }
        }

        /// <summary>The thumb test_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbTest_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                var iChangeVal = e.HorizontalChange;
                var iExpected = iEditor.TestWidth + e.HorizontalChange;
                if (iExpected > DRAGMIN)
                {
                    iEditor.TestWidth += iChangeVal;
                }
                else
                {
                    iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                    iEditor.TestWidth = DRAGMIN;
                }

                foreach (System.Windows.FrameworkElement i in TestCasePanel.Children)
                {
                    // all the visible elements also need to be adjusted. doing it this way, saves CPU -> easier binding (can't easely bind to 'root' object).
                    var iRec = i.DataContext as TestCaseItem;
                    System.Diagnostics.Debug.Assert(iRec != null);
                    iRec.TestWidth += e.HorizontalChange;
                    iRec.RunWidth -= e.HorizontalChange;
                    i.InvalidateMeasure();
                }
            }
        }

        /// <summary>The thumb verify_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ThumbVerify_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                var iChangeVal = e.HorizontalChange;
                var iExpected = iEditor.VerifyWidth + e.HorizontalChange;
                if (iExpected > DRAGMIN)
                {
                    iEditor.VerifyWidth += iChangeVal;
                }
                else
                {
                    iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                    iEditor.VerifyWidth = DRAGMIN;
                }

                foreach (System.Windows.FrameworkElement i in TestCasePanel.Children)
                {
                    // all the visible elements also need to be adjusted. doing it this way, saves CPU -> easier binding (can't easely bind to 'root' object).
                    var iRec = i.DataContext as TestCaseItem;
                    System.Diagnostics.Debug.Assert(iRec != null);
                    iRec.VerifyWidth += e.HorizontalChange;
                    iRec.RunWidth -= e.HorizontalChange;
                    i.InvalidateMeasure();
                }
            }
        }

        #endregion

        #region TestcasePanel

        /// <summary>The test case item_ data context changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TestCaseItem_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UpdateGridRow((System.Windows.FrameworkElement)sender, (TestCase)DataContext);
        }

        /// <summary>The test case item_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TestCaseItem_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateGridRow((System.Windows.FrameworkElement)sender, (TestCase)DataContext);
        }

        /// <summary>The update grid row.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="testCase">The test case.</param>
        private void UpdateGridRow(System.Windows.FrameworkElement sender, TestCase testCase)
        {
            var iTrvItem = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.TreeViewPanelItem>(sender);
            if (iTrvItem != null)
            {
                var iTest = sender.DataContext as TestCaseItem;
                if (iTest != null && testCase != null)
                {
                    double iTotalUsed = 0;
                    var iOffset = TOGGLEBUTTONSIZE + SPLITSIZE + (iTrvItem.Level * TestCasePanel.LevelDepth);

                    if (iOffset < testCase.IsEnabledWidth)
                    {
                        // could be that the collaps  is further then the col width, in which case we spread  over multiple cols.
                        iTest.IsEnabledWidth = testCase.IsEnabledWidth - iOffset;
                        iOffset = 0;
                    }
                    else
                    {
                        iOffset -= testCase.IsEnabledWidth;
                        iTest.IsEnabledWidth = 0;
                    }

                    iTotalUsed += iTest.IsEnabledWidth;

                    if (iOffset < testCase.TestWidth)
                    {
                        iTest.TestWidth = testCase.TestWidth - iOffset;
                        iOffset = 0;
                    }
                    else
                    {
                        iOffset -= testCase.TestWidth;
                        iTest.TestWidth = 0;
                    }

                    iTotalUsed += iTest.TestWidth;

                    if (iOffset < testCase.VerifyWidth)
                    {
                        iTest.VerifyWidth = testCase.VerifyWidth - iOffset;
                        iOffset = 0;
                    }
                    else
                    {
                        iOffset -= testCase.VerifyWidth;
                        iTest.VerifyWidth = 0;
                    }

                    iTotalUsed += iTest.VerifyWidth;

                    if (iOffset < testCase.ResultWidth)
                    {
                        iTest.ResultWidth = testCase.ResultWidth - iOffset;
                        iOffset = 0;
                    }
                    else
                    {
                        iOffset -= testCase.ResultWidth;
                        iTest.ResultWidth = 0;
                    }

                    iTotalUsed += iTest.ResultWidth;
                    iTest.RunWidth = TestCasePanel.ActualWidth - iTotalUsed - TOGGLEBUTTONSIZE - (SPLITSIZE * 2)
                                     - (iTrvItem.Level * TestCasePanel.LevelDepth);

                    // iTrvItem.InvalidateMeasure();
                }
            }
        }

        /// <summary>Handles the MouseDown event of the AssetPanel control.
        ///     We need to make certain that the commands get activated and the list of selected items can be cleared.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TestCasePanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == TestCasePanel)
            {
                // only try to handle when the user clicked on the background, and nothing else
                if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
                {
                    var iTest = (TestCase)DataContext;
                    if (iTest != null)
                    {
                        iTest.SelectedItems.Clear();
                    }
                }

                TestCasePanel.Focus();
                e.Handled = true;
            }
        }

        /// <summary>Handles the TiltWheel event of the ThesPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.Designer.WPF.Controls.MouseTiltEventArgs"/> instance containing the event
        ///     data.</param>
        private void TestCasePanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            var iThes = (TestCase)DataContext;
            if (iThes != null)
            {
                iThes.HorScrollPos += e.Tilt;
            }
        }

        #endregion

        #region Commands

        #region AddSibling

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void AddSibling_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            var iSelected = iTest.SelectedItem;
            iTest.SelectedItems.Clear();

                // we manually clear out the selected list, cause the keybinding uses the control, which is also used by the selection list, to add instead of replace the first selected item, which we don't want during this operation.
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iNew = new TestCaseItem();
                iSelected.Items.Add(iNew);
                iSelected.IsExpanded = true;
                iTest.SelectedItem = iNew;
                iNew.NeedsBringIntoView = true;
                iNew.NeedsFocus = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void AddSibling_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                e.CanExecute = iEditor.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        #endregion

        #region AddChild

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void AdddChild_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            var iSelected = iTest.SelectedItem;

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iNew = new TestCaseItem();
                if (iSelected == null || iSelected.Owner is TestCase)
                {
                    iTest.Items.Add(iNew);
                }
                else
                {
                    iSelected = iSelected.Owner as TestCaseItem;
                    iSelected.Items.Add(iNew);
                    iSelected.IsExpanded = true;
                }

                iTest.SelectedItem = iNew;
                iNew.NeedsBringIntoView = true;
                iNew.NeedsFocus = true;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void AdddChild_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            e.CanExecute = iEditor != null;
            e.Handled = true;
        }

        #endregion

        #region Run

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Run_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            iTest.Run();
            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Run_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            e.CanExecute = iTest != null && iTest.RunOn != null;
            e.Handled = true;
        }

        #endregion

        #region Stop

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Stop_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            iTest.Stop();
            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Stop_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            e.CanExecute = iEditor != null && iEditor.IsRunning;
            e.Handled = true;
        }

        #endregion

        #region Delete

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTest = (TestCase)DataContext;
            var iSelected = iTest.SelectedItem;

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                TestCaseItem iNext = null;
                int iIndex;
                if (iSelected.Owner is TestCase)
                {
                    iIndex = iTest.Items.IndexOf(iSelected);
                    iTest.Items.RemoveAt(iIndex);
                    if (iIndex > 0)
                    {
                        if (iIndex < iTest.Items.Count - 1)
                        {
                            iNext = iTest.Items[iIndex];
                        }
                        else
                        {
                            iNext = iTest.Items[iIndex - 1];
                        }
                    }
                    else if (iTest.Items.Count > 0)
                    {
                        iNext = iTest.Items[0];
                    }
                }
                else
                {
                    var iOwner = iSelected.Owner as TestCaseItem;
                    iIndex = iOwner.Items.IndexOf(iSelected);
                    iOwner.Items.RemoveAt(iIndex);
                    if (iIndex > 0)
                    {
                        if (iIndex < iOwner.Items.Count - 1)
                        {
                            iNext = iOwner.Items[iIndex];
                        }
                        else
                        {
                            iNext = iOwner.Items[iIndex - 1];
                        }
                    }
                    else if (iOwner.Items.Count > 0)
                    {
                        iNext = iOwner.Items[0];
                    }
                    else
                    {
                        iNext = iOwner;
                    }
                }

                Dispatcher.BeginInvoke(
                    new System.Action<TestCaseItem>(SelectAsync), 
                    System.Windows.Threading.DispatcherPriority.Background, 
                    iNext);

                    // do this async otherwise, we might get a bad interaction with  the delete and the UI repaints badly.
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>The select async.</summary>
        /// <param name="iNext">The i next.</param>
        private void SelectAsync(TestCaseItem iNext)
        {
            if (iNext != null)
            {
                iNext.NeedsBringIntoView = true;
                iNext.IsSelected = true; // this also sets iTest.SelectedItem.
            }
            else
            {
                var iTest = (TestCase)DataContext;
                iTest.SelectedItem = null;
            }
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (TestCase)DataContext;
            if (iEditor != null)
            {
                e.CanExecute = iEditor.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        #endregion

        /// <summary>The text box_ got focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.TextBox;
            if (iSender != null)
            {
                var iItem = iSender.DataContext as TestCaseItem;
                if (iItem != null)
                {
                    iItem.IsSelected = true;
                }
            }
        }

        #endregion
    }
}