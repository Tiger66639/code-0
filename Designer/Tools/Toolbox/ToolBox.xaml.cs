// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolBox.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ToolBox.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for ToolBox.xaml
    /// </summary>
    public partial class ToolBox : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ToolBox"/> class.</summary>
        public ToolBox()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the BtnReset control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnReset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Brain.Current.ReloadLoadDefaultNeurons();

                // before we try to reload all the default toolboxitems, we make certain that all the new default neurons are also loaded.
            BrainData.Current.ReloadToolboxItems();
        }

        /// <summary>The mnu items collaps items_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuItemsCollapsItems_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AllItemsOpen = null; // to make certain prev value is reset
            AllItemsOpen = false;
        }

        /// <summary>The mnu items expand items_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuItemsExpandItems_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AllItemsOpen = null; // to make certain prev value is reset
            AllItemsOpen = true;
        }

        /// <summary>The mnu instructions collaps items_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuInstructionsCollapsItems_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AllInstructionsOpen = null; // to make certain prev value is reset
            AllInstructionsOpen = false;
        }

        /// <summary>The mnu instructions expand items_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuInstructionsExpandItems_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AllInstructionsOpen = null; // to make certain prev value is reset
            AllInstructionsOpen = true;
        }

        /// <summary>The export defaults_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ExportDefaults_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BrainData.Current.NeuronInfo.SaveDefaultInfo();
        }

        #region AllInstructionsOpen

        /// <summary>
        ///     <see cref="AllInstructionsOpen" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AllInstructionsOpenProperty =
            System.Windows.DependencyProperty.Register(
                "AllInstructionsOpen", 
                typeof(bool?), 
                typeof(ToolBox), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets the <see cref="AllInstructionsOpen" /> property. This
        ///     dependency property indicates wether all groups on the instructions
        ///     list are collapsed/opened or mixed.
        /// </summary>
        public bool? AllInstructionsOpen
        {
            get
            {
                return (bool?)GetValue(AllInstructionsOpenProperty);
            }

            set
            {
                SetValue(AllInstructionsOpenProperty, value);
            }
        }

        #endregion

        #region AllItemsOpen

        /// <summary>
        ///     <see cref="AllItemsOpen" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AllItemsOpenProperty =
            System.Windows.DependencyProperty.Register(
                "AllItemsOpen", 
                typeof(bool?), 
                typeof(ToolBox), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets the <see cref="AllItemsOpen" /> property. This dependency
        ///     property indicates wether all the groups on the items list are
        ///     collapsed/opened or mixed.
        /// </summary>
        public bool? AllItemsOpen
        {
            get
            {
                return (bool?)GetValue(AllItemsOpenProperty);
            }

            set
            {
                SetValue(AllItemsOpenProperty, value);
            }
        }

        #endregion
    }
}