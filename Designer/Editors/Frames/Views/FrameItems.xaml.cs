// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameItems.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FrameItems.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for FrameItems.xaml
    /// </summary>
    public partial class FrameItems : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="FrameItems"/> class.</summary>
        public FrameItems()
        {
            InitializeComponent();
        }

        /// <summary>Handles the MouseDoubleClick event of the CustomFilter control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void CustomFilter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iEl = sender as System.Windows.FrameworkElement;
            if (iEl != null)
            {
                var iRestriction = iEl.DataContext as FERestrictionBase;
                if (iRestriction != null)
                {
                    ((WindowMain)System.Windows.Application.Current.MainWindow).ViewCodeForNeuron(iRestriction.Item);
                }
            }
        }
    }
}