// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgPasteSpecial.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgPasteSpecial.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgPasteSpecial.xaml
    /// </summary>
    public partial class DlgPasteSpecial : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgPasteSpecial"/> class.</summary>
        public DlgPasteSpecial()
        {
            Duplicate = false;
            InitializeComponent();
        }

        #region Duplicate

        /// <summary>
        ///     Gets the wether a duplicate should be created or a reference,
        ///     according to the user's choises.
        /// </summary>
        public bool Duplicate { get; internal set; }

        #endregion

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>The duplicate_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Duplicate_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Duplicate = true;
        }

        /// <summary>The reference_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Reference_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Duplicate = false;
        }
    }
}