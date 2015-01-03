// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectMeaning.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   A dialog used to select a neuron from the meanings list. It provides
//   faschilities to create a new neuron to be used as a meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A dialog used to select a neuron from the meanings list. It provides
    ///     faschilities to create a new neuron to be used as a meaning.
    /// </summary>
    public partial class DlgSelectMeaning : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgSelectMeaning"/> class.</summary>
        public DlgSelectMeaning()
        {
            InitializeComponent();
        }

        #region SelectedValue

        /// <summary>
        ///     Gets the value that was selected by the user when the 'ok' button was
        ///     pressed.
        /// </summary>
        public Neuron SelectedValue { get; internal set; }

        #endregion

        /// <summary>The btn add neuron_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnAddNeuron_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgStringQuestion();
            iDlg.Question = "Name: ";
            iDlg.Title = "New meaning";
            iDlg.Answer = "New meaning";
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                var iNew = NeuronFactory.GetNeuron();
                WindowMain.AddItemToBrain(iNew);
                BrainData.Current.NeuronInfo[iNew.ID].DisplayTitle = iDlg.Answer;
                BrainData.Current.DefaultMeaningIds.Add(iNew.ID);
                SelectedValue = iNew;
                DialogResult = true;
            }
        }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedValue = CmbSelectedMeaning.SelectedItem as Neuron;
            DialogResult = true;
        }
    }
}