// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimersView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for TimersView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for TimersView.xaml
    /// </summary>
    public partial class TimersView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="TimersView"/> class.</summary>
        public TimersView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the BtnNewTimer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnNewTimer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                var iNew = new TimerNeuron();
                WindowMain.AddItemToBrain(iNew);

                    // don't need to add it to our list, this is done through the event handling
                BrainData.Current.NeuronInfo[iNew].DisplayTitle = "Timer";
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(WindowMain.UndoStore.EndUndoGroup)); // not really needed, just for safety.
            }
        }

        /// <summary>The data timers_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DataTimers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iTimers = DataContext as Timers;
            if (iTimers != null && e.AddedItems.Count > 0)
            {
                iTimers.SelectedItem = e.AddedItems[0] as NeuralTimer;
            }
        }

        /// <summary>The d grid row_ preview mouse right button down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DGridRow_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.DataGridRow;
            if (item != null)
            {
                item.Focus();
            }
        }

        #region Delete

        /// <summary>The delete neuron_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteNeuron_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iTimers = DataContext as Timers;
            e.CanExecute = iTimers != null && iTimers.SelectedItem != null;
        }

        /// <summary>The delete neuron_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iTimers = DataContext as Timers;
            var iItem = iTimers.SelectedItem;
            if (iItem != null)
            {
                if (iItem.Item.ModuleRefCount > 0)
                {
                    System.Windows.MessageBox.Show(
                        "Timer was created by a module, can only delete it by unloading the module", 
                        "Delete timer", 
                        System.Windows.MessageBoxButton.OK);
                }
                else
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            string.Format("Delete timer: '{0}'?", iItem.NeuronInfo.DisplayTitle), 
                            "Delete timer", 
                            System.Windows.MessageBoxButton.OKCancel, 
                            System.Windows.MessageBoxImage.Question, 
                            System.Windows.MessageBoxResult.No);
                    if (iRes == System.Windows.MessageBoxResult.OK)
                    {
                        WindowMain.UndoStore.BeginUndoGroup(false);

                            // we group all the data together so a single undo command cand restore the previous state.
                        try
                        {
                            while (iItem.NeuronInfo.IsLocked)
                            {
                                // we force the delete, so make certain it isn't locked anymore.
                                iItem.NeuronInfo.IsLocked = false;

                                    // we need to manually unlock cause it is no longer a root, if we don't do this, can't delete it.
                            }

                            Brain.Current.Delete(iItem.Item);
                        }
                        finally
                        {
                            Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Background, 
                                new System.Action(WindowMain.UndoStore.EndUndoGroup));

                                // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                        }
                    }
                }
            }
        }

        #endregion
    }
}