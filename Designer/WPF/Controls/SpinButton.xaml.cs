// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpinButton.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SpinButton.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for SpinButton.xaml
    /// </summary>
    public partial class SpinButton : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="SpinButton"/> class.</summary>
        public SpinButton()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the RepeatUp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Value++;
        }

        /// <summary>Handles the Click event of the RepeatDown control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Value--;
        }

        #region Value

        /// <summary>
        ///     <see cref="Value" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ValueProperty =
            System.Windows.DependencyProperty.Register(
                "Value", 
                typeof(int), 
                typeof(SpinButton), 
                new System.Windows.FrameworkPropertyMetadata(
                    0, 
                    System.Windows.FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     Gets or sets the <see cref="Value" /> property. This dependency
        ///     property indicates the current value.
        /// </summary>
        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }

            set
            {
                SetValue(ValueProperty, value);
            }
        }

        #endregion
    }
}