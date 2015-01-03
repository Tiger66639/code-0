// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetsDictionary.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for AssetsDictionary.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for AssetsDictionary.xaml
    /// </summary>
    public partial class AssetsDictionary : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="AssetsDictionary"/> class.</summary>
        public AssetsDictionary()
        {
            InitializeComponent();
        }

        /// <summary><para>Handles the Click event of the ButtonAddNew control.</para>
        /// <para>Handles the Click event of the BtnAddExisting control.</para>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnAddExisting_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iData = iSender.DataContext as AssetData;
            if (iData != null)
            {
                var iCluster = iData.Value as NeuronCluster;
                System.Diagnostics.Debug.Assert(iCluster != null);
                var iFound = TextNeuron.GetFor(" ");

                    // we get an empty text neuron, so we have something to add to tthe list, but don't show to much.
                using (var iList = iCluster.ChildrenW) iList.Add(iFound);
            }
        }
    }
}