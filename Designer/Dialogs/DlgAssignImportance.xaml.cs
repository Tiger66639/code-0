// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgAssignImportance.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgAssignImportance.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Dialogs
{
    /// <summary>
    ///     Interaction logic for DlgAssignImportance.xaml
    /// </summary>
    public partial class DlgAssignImportance : System.Windows.Window
    {
        /// <summary>The f cluster.</summary>
        private NeuronCluster fCluster; // the pos cluster, if there was any.

        /// <summary>The f value.</summary>
        private IntNeuron fValue; // in case there was a previous value.

        /// <summary>The fto edit.</summary>
        private readonly Neuron ftoEdit;

        /// <summary>Initializes a new instance of the <see cref="DlgAssignImportance"/> class.</summary>
        /// <param name="toEdit">The to edit.</param>
        public DlgAssignImportance(Neuron toEdit)
        {
            System.Diagnostics.Debug.Assert(toEdit != null);
            ftoEdit = toEdit;
            InitializeComponent();
            Initvalue();
        }

        /// <summary>
        ///     Checks if there is a posgroup available to assign the value to +
        ///     checks if there is a previous value.
        /// </summary>
        private void Initvalue()
        {
            fValue = ftoEdit.FindFirstOut((ulong)PredefinedNeurons.ImportanceLevel) as IntNeuron;
            if (fValue != null)
            {
                TxtValue.Text = fValue.Value.ToString();
            }

            fCluster = ftoEdit.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);
            if (fCluster != null)
            {
                if (fValue == null)
                {
                    fValue = fCluster.FindFirstOut((ulong)PredefinedNeurons.ImportanceLevel) as IntNeuron;
                }
            }
            else
            {
                ChkPosGrp.IsEnabled = false;
            }
        }

        /// <summary>Called when the user clicked ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            int iValue;
            if (int.TryParse(TxtValue.Text, out iValue))
            {
                if (fValue != null)
                {
                    fValue.Value = iValue;
                }
                else
                {
                    fValue = NeuronFactory.GetInt(iValue);
                    Brain.Current.Add(fValue);
                    if (fCluster != null && ChkPosGrp.IsChecked == true)
                    {
                        fCluster.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ImportanceLevel, fValue);
                    }
                    else
                    {
                        ftoEdit.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ImportanceLevel, fValue);
                    }
                }

                DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Please provide an integer value!", 
                    "Input error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}