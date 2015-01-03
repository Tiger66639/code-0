// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlBoolExpression.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlBoolExpression.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlBoolExpression.xaml
    /// </summary>
    public partial class CtrlBoolExpression : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlBoolExpression"/> class.</summary>
        public CtrlBoolExpression()
        {
            InitializeComponent();
        }

        /// <summary>The operator_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Operator_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var iItem = (CodeItem)DataContext;
                var iOp = e.AddedItems[0] as Neuron;
                OperatorPart.Content = new CodeItemStatic(iOp, iItem.IsActive);
            }
            else if (e.RemovedItems.Count > 0)
            {
                OperatorPart.Content = null;
            }
        }
    }
}