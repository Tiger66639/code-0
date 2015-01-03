// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgAssemblyFromGac.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgAssemblyFromGac.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgAssemblyFromGac.xaml
    /// </summary>
    public partial class DlgAssemblyFromGac : System.Windows.Window
    {
        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<string> fItems;

        /// <summary>The f loaded items.</summary>
        private System.Collections.Generic.List<string> fLoadedItems;

        /// <summary>Initializes a new instance of the <see cref="DlgAssemblyFromGac"/> class. 
        ///     Initializes a new instance of the <see cref="DlgAssemblyFromGac"/>
        ///     class.</summary>
        public DlgAssemblyFromGac()
        {
            InitializeComponent();
        }

        #region Items

        /// <summary>
        ///     Gets the list of assembly names in the gac.
        /// </summary>
        public System.Collections.Generic.List<string> Items
        {
            get
            {
                if (fItems == null)
                {
                    fItems = new System.Collections.Generic.List<string>();
                    var iTemp = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System);
                    var iPos = iTemp.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                    iTemp = iTemp.Substring(0, iPos);
                    var strGacDir = System.IO.Path.Combine(iTemp, "assembly");
                    var iGacDirs = System.IO.Directory.GetDirectories(strGacDir);
                    foreach (var iGacDir in iGacDirs)
                    {
                        var strDirs1 = System.IO.Directory.GetDirectories(iGacDir);
                        string[] strDirs2;

                        string[] iFiles;

                        foreach (var strDir1 in strDirs1)
                        {
                            strDirs2 = System.IO.Directory.GetDirectories(strDir1);

                            foreach (var strDir2 in strDirs2)
                            {
                                iFiles = System.IO.Directory.GetFiles(strDir2, "*.dll");
                                foreach (var iDll in iFiles)
                                {
                                    fItems.Add(iDll);
                                }
                            }
                        }
                    }

                    fItems.Sort();
                }

                return fItems;
            }
        }

        #endregion

        #region LoadedItems

        /// <summary>
        ///     Gets the list of loaded assemblies.
        /// </summary>
        public System.Collections.Generic.List<string> LoadedItems
        {
            get
            {
                if (fLoadedItems == null)
                {
                    fLoadedItems = new System.Collections.Generic.List<string>();
                    foreach (var i in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        fLoadedItems.Add(i.CodeBase);
                    }

                    fLoadedItems.Sort();
                }

                return fLoadedItems;
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets the assembly that is currently selected.
        /// </summary>
        public string SelectedItem { get; internal set; }

        #endregion

        /// <summary>Called when [click ok].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TabLoaded.IsSelected)
            {
                SelectedItem = (string)LstLoaded.SelectedItem;
            }
            else
            {
                SelectedItem = (string)LstGAC.SelectedItem;
            }

            DialogResult = true;
        }
    }
}