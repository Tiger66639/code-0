// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlCustomConduitSelector.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlCustomConduitSelector.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for CtrlCustomConduitSelector.xaml
    /// </summary>
    public partial class CtrlCustomConduitSelector : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlCustomConduitSelector"/> class.</summary>
        public CtrlCustomConduitSelector()
        {
            InitializeComponent();
        }

        /// <summary>The btn more_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnMore_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.Controls.Button)sender;
            var iDlg = new Microsoft.Win32.OpenFileDialog();

            if ((string)iSender.Tag == "dll")
            {
                iDlg.Filter = "dll files (*.dll)|*.dll";
                iDlg.FileName = TxtCustomDll.Text;
            }
            else
            {
                iDlg.Filter = "all files (*.*)|*.*";
                iDlg.FileName = TxtSource.Text;
            }

            iDlg.FilterIndex = 1;
            iDlg.Multiselect = false;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                if ((string)iSender.Tag == "dll")
                {
                    TxtCustomDll.Text = iDlg.FileName;
                }
                else if ((string)iSender.Tag == "source")
                {
                    TxtSource.Text = iDlg.FileName;
                }
                else
                {
                    TxtDest.Text = iDlg.FileName;
                }
            }
        }
    }
}