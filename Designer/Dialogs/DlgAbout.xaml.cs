// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgAbout.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgAbout.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgAbout.xaml
    /// </summary>
    public partial class DlgAbout : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgAbout"/> class.</summary>
        public DlgAbout()
        {
            InitializeComponent();
            if (WindowMain.Current.DesignerVisibility == System.Windows.Visibility.Visible)
            {
                ImgAbout.Source =
                    new System.Windows.Media.Imaging.BitmapImage(
                        new System.Uri("\\Images\\splash\\CBDRedAbout.jpg", System.UriKind.RelativeOrAbsolute));
                LblEdition.Text = "Designer edition";
            }
            else if (WindowMain.Current.ProVisibility == System.Windows.Visibility.Visible)
            {
                ImgAbout.Source =
                    new System.Windows.Media.Imaging.BitmapImage(
                        new System.Uri("\\Images\\splash\\CBDGreenAbout.jpg", System.UriKind.RelativeOrAbsolute));
                LblEdition.Text = "Pro edition";
            }
            else
            {
                ImgAbout.Source =
                    new System.Windows.Media.Imaging.BitmapImage(
                        new System.Uri("\\Images\\splash\\CBDBlueAbout.jpg", System.UriKind.RelativeOrAbsolute));
                LblEdition.Text = "Basic edition";
            }
        }

        #region Version

        /// <summary>
        ///     Gets the application version
        /// </summary>
        public System.Version Version
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return assembly.GetName().Version;
            }
        }

        #endregion

        #region ApplicationName

        /// <summary>
        ///     Gets the name of the application. This allows us to dynamically change
        ///     this, depending on the version (basic, pro, desiger).
        /// </summary>
        public string ApplicationName
        {
            get
            {
                if (WindowMain.Current.DesignerVisibility == System.Windows.Visibility.Visible)
                {
                    return "Neural network designer";
                }

                return ProjectManager.SHORTAPPNAMEBASIC;
            }
        }

        #endregion

        /// <summary>The label_ mouse down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CmdShell.CmdShell.OpenDocument(Properties.Resources.WebSite);

            // Process iExplorer = new Process();
            // iExplorer.StartInfo.FileName = Properties.Resources.WebSite;
            // iExplorer.Start();
        }
    }
}