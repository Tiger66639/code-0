// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgOnlineConfig.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgOnlineConfig.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgOnlineConfig.xaml
    /// </summary>
    public partial class DlgOnlineConfig : System.Windows.Window
    {
        /// <summary>The f data.</summary>
        private readonly OnlineData fData = new OnlineData();

        /// <summary>Initializes a new instance of the <see cref="DlgOnlineConfig"/> class. 
        ///     Initializes a new instance of the <see cref="DlgOnlineConfig"/>
        ///     class.</summary>
        public DlgOnlineConfig()
        {
            if (BrainData.Current.DesignerData.OnlineInfo != null)
            {
                BrainData.Current.DesignerData.OnlineInfo.AssignTo(fData);
            }

            InitializeComponent();
            DataContext = fData;
            if (BrainData.Current.DesignerData.OnlineInfo != null
                && BrainData.Current.DesignerData.OnlineInfo.IsInstalled)
            {
                BtnUpdate.IsEnabled = true;
            }
            else
            {
                BtnOk.IsEnabled = false;
            }
        }

        /// <summary>performs an install.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BrainData.Current.DesignerData.OnlineInfo == null)
            {
                BrainData.Current.DesignerData.OnlineInfo = fData;
            }
            else
            {
                fData.AssignTo(BrainData.Current.DesignerData.OnlineInfo);
            }

            DialogResult = true;
            var iInstaller = new OnlineInstaller();
            iInstaller.Install();
        }

        /// <summary>The prepare_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Prepare_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BrainData.Current.DesignerData.OnlineInfo == null)
            {
                BrainData.Current.DesignerData.OnlineInfo = fData;
            }
            else
            {
                fData.AssignTo(BrainData.Current.DesignerData.OnlineInfo);
            }

            DialogResult = true;
            var iInstaller = new OnlinePreparer();
            iInstaller.Install();
        }

        /// <summary>performs an update</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDest = BrainData.Current.DesignerData.OnlineInfo;
            fData.AssignTo(iDest);
            DialogResult = true;
            var iInstaller = new OnlineUpdater();
            iInstaller.Install();
        }

        /// <summary>loads a filename into the textbox.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Multiselect = false;
            if (sender == BtnCss)
            {
                iDlg.Filter = "Style sheets (*.css)|*.css*|All Files|*.*";
                iDlg.DefaultExt = "css";
            }
            else
            {
                iDlg.Filter = "Html files (*.html)|*.html*|All Files|*.*";
                iDlg.DefaultExt = "html";
            }

            iDlg.FilterIndex = 0;
            iDlg.AddExtension = true;
            if (sender == BtnCss)
            {
                iDlg.FileName = TxtCssFile.Text;
            }
            else
            {
                iDlg.FileName = TxtHtmlTemplate.Text;
            }

            iDlg.CheckFileExists = true;
            var iRes = iDlg.ShowDialog(WindowMain.Current);
            if (iRes.HasValue && iRes.Value)
            {
                if (sender == BtnCss)
                {
                    TxtCssFile.Text = iDlg.FileName;
                }
                else
                {
                    TxtHtmlTemplate.Text = iDlg.FileName;
                }
            }
        }

        /// <summary>when the usename changes, we can only do a new installation.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (BrainData.Current.DesignerData.OnlineInfo != null
                && (TxtUserName.Text != BrainData.Current.DesignerData.OnlineInfo.User
                    || TxtServerPath.Text != BrainData.Current.DesignerData.OnlineInfo.ServerPath
                    || TxtLocation.Text != BrainData.Current.DesignerData.OnlineInfo.FTPLocation))
            {
                // dont't want to disable button at startup, when value gets assigned to textbox.
                BtnUpdate.IsEnabled = false;
                BtnOk.IsEnabled = string.IsNullOrEmpty(TxtUserName.Text) == false
                                  && string.IsNullOrEmpty(TxtLocation.Text) == false
                                  && string.IsNullOrEmpty(TxtServerPath.Text) == false;
            }
            else
            {
                BtnOk.IsEnabled = true;
                BtnUpdate.IsEnabled = BrainData.Current.DesignerData.OnlineInfo != null
                                      && BrainData.Current.DesignerData.OnlineInfo.IsInstalled;
            }

            BtnPrepare.IsEnabled = string.IsNullOrEmpty(TxtServerPath.Text) == false;
        }
    }
}