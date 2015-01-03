// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlVariable.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlVariable.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlVariable.xaml
    /// </summary>
    public partial class CtrlVariable : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlVariable"/> class.</summary>
        public CtrlVariable()
        {
            InitializeComponent();
        }

        /// <summary>The split reaction_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SplitReaction_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var iItem = (CodeItem)DataContext;
                var iInfo = e.AddedItems[0] as NeuronInfo;
                SplitReactionPart.Content = new CodeItemStatic(iInfo.Item, iItem.IsActive);
            }
            else if (e.RemovedItems.Count > 0)
            {
                SplitReactionPart.Content = null;
            }
        }
    }
}