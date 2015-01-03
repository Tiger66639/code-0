// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorsOverview.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ProcessorsOverview.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for ProcessorsOverview.xaml
    /// </summary>
    public partial class ProcessorsOverview : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ProcessorsOverview"/> class.</summary>
        public ProcessorsOverview()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Checked event of the RBtnVariable control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RBtnVariable_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = ((System.Windows.Controls.Primitives.ButtonBase)sender).DataContext as Watch;
            if (iData != null)
            {
                ProcessorManager.Current.SelectedWatchIndex = ProcessorManager.Current.Watches.IndexOf(iData);

                    // this is required so it knows which one to use for updating values after processing.
                var iVar = Brain.Current[iData.ID] as Variable;
                if (iVar == null)
                {
                    throw new System.InvalidOperationException(
                        string.Format("Variables expected for watches, found invalid type for neuron {0}.", iData.ID));
                }

                ProcessorManager.Current.DisplayValuesFor(iVar);
            }
        }

        /// <summary>The toggle vars click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleVarsClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ProcessorManager.Current.Displaymode = ProcessorManager.DisplayMode.Variables;
            DataVarProcWatches.Visibility = System.Windows.Visibility.Collapsed;
            ScrollVarWatches.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>The toggle proc click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleProcClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ProcessorManager.Current.Displaymode = ProcessorManager.DisplayMode.Processors;
            DataVarProcWatches.Visibility = System.Windows.Visibility.Visible;
            ScrollVarWatches.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>The trv values_ selected item changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TrvValues_SelectedItemChanged(
            object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            ProcessorManager.Current.SelectedProcessor = e.NewValue as ProcItem;
        }

        /// <summary>Handles the Click event of the MnuClear control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ProcessorManager.Current.Watches.Clear();
        }

        /// <summary>The delete break point_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteBreakPoint_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExp = e.Parameter as Expression;
            if (iExp == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                iExp = iFocused.DataContext as Expression;
            }

            BrainData.Current.BreakPoints.Remove(iExp);
        }

        /// <summary>The delete break point_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteBreakPoint_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = e.Parameter is Expression || (iFocused != null && iFocused.DataContext is Expression);
        }

        #region Delete watch

        /// <summary>The delete watch_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteWatch_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = e.Parameter is Watch || (iFocused != null && iFocused.DataContext is Watch);
        }

        /// <summary>The delete watch_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteWatch_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iWatch = e.Parameter as Watch;
            if (iWatch == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                iWatch = iFocused.DataContext as Watch;
            }

            if (iWatch != null)
            {
                ProcessorManager.Current.Watches.Remove(iWatch);
            }
        }

        #endregion
    }
}