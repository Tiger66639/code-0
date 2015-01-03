// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlDropTarget.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlDropTarget.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for CtrlDropTarget.xaml
    /// </summary>
    public partial class CtrlDropTarget : System.Windows.Controls.Border
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlDropTarget"/> class.</summary>
        public CtrlDropTarget()
        {
            InitializeComponent();
        }

        /// <summary>The on mouse left button down.</summary>
        /// <param name="e">The e.</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();
            e.Handled = true;
        }
    }
}