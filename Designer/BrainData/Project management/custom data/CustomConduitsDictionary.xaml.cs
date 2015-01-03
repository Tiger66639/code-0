// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConduitsDictionary.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   The custom conduits dictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The custom conduits dictionary.</summary>
    public partial class CustomConduitsDictionary : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="CustomConduitsDictionary"/> class.</summary>
        public CustomConduitsDictionary()
        {
            InitializeComponent();
        }

        /// <summary>The btn more_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnMore_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.Controls.Button)sender;
            var iText = (System.Windows.Controls.TextBox)iSender.Tag;
            var iDlg = new Microsoft.Win32.SaveFileDialog();

            iDlg.Filter = Properties.Resources.CSVFileFilter;
            iDlg.FileName = iText.Text;
            iDlg.FilterIndex = 1;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                iText.Text = iDlg.FileName;
            }
        }
    }
}