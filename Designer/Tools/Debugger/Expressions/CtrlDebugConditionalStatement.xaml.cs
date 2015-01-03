// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlDebugConditionalStatement.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlDebugConditionalStatement.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlDebugConditionalStatement.xaml
    /// </summary>
    public partial class CtrlDebugConditionalStatement : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlDebugConditionalStatement"/> class.</summary>
        public CtrlDebugConditionalStatement()
        {
            InitializeComponent();
        }

        /// <summary>The on mouse left button down.</summary>
        /// <param name="e">The e.</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
        }
    }
}