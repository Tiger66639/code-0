// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgBuildThesaurus.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgBuildThesaurus.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgBuildThesaurus.xaml
    /// </summary>
    public partial class DlgBuildThesaurus : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgBuildThesaurus"/> class.</summary>
        public DlgBuildThesaurus()
        {
            InitializeComponent();
        }

        /// <summary>Called when start is clicked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickStart(object sender, System.Windows.RoutedEventArgs e)
        {
            var iBuilder = (ThesaurusBuilder)DataContext;
            System.Diagnostics.Debug.Assert(iBuilder != null);
            iBuilder.Finished += iBuilder_OnFinished;
            iBuilder.Build();
        }

        /// <summary>Handles the OnFinished event of the iBuilder control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void iBuilder_OnFinished(object sender, System.EventArgs e)
        {
            BtnOk.IsEnabled = true;
        }

        /// <summary>Handles the Click event of the BtnOk control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iBuilder = (ThesaurusBuilder)DataContext;
            iBuilder.StoreResult();
            DialogResult = true;
        }

        /// <summary>Handles the Click event of the BtnClose control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iBuilder = (ThesaurusBuilder)DataContext;
            if (iBuilder.IsRunning)
            {
                iBuilder.StopRequested = true;
            }
        }
    }
}