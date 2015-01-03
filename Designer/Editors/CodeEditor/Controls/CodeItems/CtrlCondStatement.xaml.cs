// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlCondStatement.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlCondStatement.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for CtrlCondStatement.xaml
    /// </summary>
    public partial class CtrlCondStatement : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlCondStatement"/> class.</summary>
        public CtrlCondStatement()
        {
            InitializeComponent();
        }

        /// <summary>Handles the SelectionChanged event of the LoopStyle control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void LoopStyle_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var iItem = (CodeItem)DataContext;
                var iInfo = e.AddedItems[0] as NeuronInfo;
                LoopStylePart.Content = new CodeItemStatic(iInfo.Item, iItem.IsActive);
            }
            else if (e.RemovedItems.Count > 0)
            {
                LoopStylePart.Content = null;
            }
        }
    }
}