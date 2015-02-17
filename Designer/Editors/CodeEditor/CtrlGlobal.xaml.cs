﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlGlobal.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlGlobal.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlGlobal.xaml
    /// </summary>
    public partial class CtrlGlobal : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlGlobal"/> class.</summary>
        public CtrlGlobal()
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