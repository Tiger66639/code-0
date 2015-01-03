// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResManToolBar.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ResManToolBar.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for ResManToolBar.xaml
    /// </summary>
    public partial class ResManToolBar : System.Windows.Controls.ToolBar
    {
        /// <summary>Initializes a new instance of the <see cref="ResManToolBar"/> class.</summary>
        public ResManToolBar()
        {
            InitializeComponent();
        }

        #region CommandTarget

        /// <summary>
        ///     <see cref="CommandTarget" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CommandTargetProperty =
            System.Windows.DependencyProperty.Register(
                "CommandTarget", 
                typeof(System.Windows.DependencyObject), 
                typeof(ResManToolBar), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DependencyObject)null));

        /// <summary>
        ///     Gets or sets the <see cref="CommandTarget" /> property. This dependency
        ///     property indicates the target that the commands should be applied to.
        ///     This should reference a ResourceManager object.
        /// </summary>
        public System.Windows.DependencyObject CommandTarget
        {
            get
            {
                return (System.Windows.DependencyObject)GetValue(CommandTargetProperty);
            }

            set
            {
                SetValue(CommandTargetProperty, value);
            }
        }

        #endregion
    }
}