// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemprofilerView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for MemprofilerView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Profiler
{
    /// <summary>
    ///     Interaction logic for MemprofilerView.xaml
    /// </summary>
    public partial class MemprofilerView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="MemprofilerView"/> class.</summary>
        public MemprofilerView()
        {
            InitializeComponent();
        }

        #region Store split path

        /// <summary>Handles the Executed event of the StoreSplitPath control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void StoreSplitPath_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = Trvleaks.SelectedItem as MemProfiledProcessor;
            if (iProc == null)
            {
                var iItem = Trvleaks.SelectedItem as MemProfilerItem;
                if (iItem != null)
                {
                    iProc = iItem.Owner;
                }
            }

            if (iProc != null)
            {
                WindowMain.Current.StoreSplitPath(iProc.SplitPath, iProc.Name);
            }
        }

        /// <summary>The store split path_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StoreSplitPath_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Trvleaks != null
                           && (Trvleaks.SelectedItem != null && !(Trvleaks.SelectedItem is MemProfiledVar));
        }

        #endregion

        #region GotoStart

        /// <summary>Shows the code editor for the specified <see cref="Neuron"/> . If
        ///     there was already an editor opened for this item, it is made active.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoStart_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = Trvleaks.SelectedItem as MemProfilerItem;
            if (iItem != null)
            {
                iItem.CreatedAt.SelectPathResult();
            }
        }

        /// <summary>Handles the CanExecute event of the GotoSTart control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoStart_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Trvleaks != null && Trvleaks.SelectedItem is MemProfilerItem;
        }

        #endregion

        #region GotoUnfreeze 

        /// <summary>Handles the CanExecute event of the GotoUnfreeze control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoUnfreeze_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (Trvleaks != null)
            {
                var iItem = Trvleaks.SelectedItem as MemProfilerItem;
                e.CanExecute = iItem != null && iItem.UnfrozenAt != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the GotoUnfreeze control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoUnfreeze_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = Trvleaks.SelectedItem as MemProfilerItem;
            if (iItem != null)
            {
                iItem.UnfrozenAt.SelectPathResult();
            }
        }

        #endregion

        #region GotoEnd

        /// <summary>Shows the code editor for the specified <see cref="Neuron"/> . If
        ///     there was already an editor opened for this item, it is made active.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoEnd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = Trvleaks.SelectedItem as MemProfiledProcessor;
            if (iProc == null)
            {
                var iItem = Trvleaks.SelectedItem as MemProfilerItem;
                if (iItem != null)
                {
                    iProc = iItem.Owner;
                }
            }

            if (iProc != null && iProc.StoppedAd != null)
            {
                iProc.StoppedAd.SelectPathResult();
            }
        }

        /// <summary>Handles the CanExecute event of the GotoEnd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void GotoEnd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Trvleaks != null
                           && (Trvleaks.SelectedItem != null && !(Trvleaks.SelectedItem is MemProfiledVar));
        }

        #endregion
    }
}