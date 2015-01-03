// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectDevice.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgSelectDevice.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgSelectDevice.xaml
    /// </summary>
    public partial class DlgSelectDevice : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgSelectDevice"/> class.</summary>
        /// <param name="values">The values.</param>
        public DlgSelectDevice(System.Collections.Generic.List<string> values)
        {
            InitializeComponent();
            LstItems.ItemsSource = values;
        }

        #region SelectedDevice

        /// <summary>
        ///     Gets the currently selected device name.
        /// </summary>
        public string SelectedDevice { get; internal set; }

        #endregion

        /// <summary>Handles the SelectionChanged event of the ListBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LstItems.SelectedItem != null)
            {
                SelectedDevice = (string)LstItems.SelectedItem;
            }
        }

        /// <summary>Handles the Click event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}