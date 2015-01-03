// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgCreateCluster.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgCreateCluster.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgCreateCluster.xaml
    /// </summary>
    /// <remarks>
    ///     Use the <see cref="DlgCreateCluster.CreateCluster" /> function to display
    ///     the dialog.
    /// </remarks>
    public partial class DlgCreateCluster : System.Windows.Window
    {
        /// <summary>The f meaning.</summary>
        private ulong fMeaning;

        /// <summary>The f title.</summary>
        private string fTitle;

        /// <summary>Prevents a default instance of the <see cref="DlgCreateCluster"/> class from being created.</summary>
        private DlgCreateCluster()
        {
            InitializeComponent();
        }

        /// <summary>Creates a cluster and returns it.</summary>
        /// <remarks>The cluster is added to the brain before it is returned.</remarks>
        /// <param name="name">A possible default value for the name.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>A cluster or <see langword="null"/> if the action was aborted.</returns>
        public static NeuronCluster CreateCluster(string name, System.Windows.Window owner)
        {
            var iDlg = new DlgCreateCluster();
            iDlg.TxtTitle.Text = name;
            iDlg.Owner = owner;
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                var iCluster = NeuronFactory.GetCluster();
                Brain.Current.Add(iCluster);
                iCluster.Meaning = iDlg.fMeaning;
                BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = iDlg.fTitle;
                return iCluster;
            }

            return null;
        }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            fTitle = TxtTitle.Text;
            if (CmbMeaning.SelectedValue != null)
            {
                fMeaning = (ulong)CmbMeaning.SelectedValue;
            }
            else
            {
                fMeaning = Neuron.EmptyId;
            }

            DialogResult = true;
        }
    }
}