// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Displays the data for 1 <see cref="DebugProcessor" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Displays the data for 1 <see cref="DebugProcessor" /> .
    /// </summary>
    public partial class ProcessorView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ProcessorView"/> class.</summary>
        public ProcessorView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Loaded event of the UserControl control.</summary>
        /// <remarks>need to make the proc that we wrap, the selected proc, so that the
        ///     commands work properly.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iProc = (ProcItem)DataContext;
            ProcessorManager.Current.SelectedProcessor = iProc;
        }

        /// <summary>The open debugger in editor_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OpenDebuggerInEditor_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = e.Parameter is Neuron || (iFocused != null && iFocused.DataContext is INeuronWrapper);
        }

        /// <summary>The open debugger in editor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OpenDebuggerInEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = (ProcItem)DataContext;
            var iPath = Search.DisplayPath.CreateFromSelectedCode(iProc.Processor);
            iPath.SelectPathResult();
        }
    }
}