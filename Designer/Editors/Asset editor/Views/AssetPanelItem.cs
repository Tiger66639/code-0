// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetPanelItem.cs" company="">
//   
// </copyright>
// <summary>
//   This <see cref="TreeViewPanelItem" /> manages the cells in the row.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     This <see cref="TreeViewPanelItem" /> manages the cells in the row.
    /// </summary>
    public class AssetPanelItem : TreeViewPanelItem
    {
        /// <summary>The get row.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        private System.Windows.FrameworkElement GetRow()
        {
            var iTemplate = Template;
            if (iTemplate != null)
            {
                var iContent = iTemplate.FindName("PART_CONTENT", this) as System.Windows.Controls.ContentPresenter;
                if (iContent != null)
                {
                    var iDataTemplate = iContent.ContentTemplate;
                    return iDataTemplate.FindName("PART_ROW", iContent) as System.Windows.FrameworkElement;
                }
            }

            return null;
        }

        /// <summary>Changes the with of the column at the specified index, if there are
        ///     any.</summary>
        /// <param name="index">The index.</param>
        /// <param name="amount">The amount.</param>
        public void ChangeColumn(int index, double amount)
        {
            var iRow = GetRow();
            if (iRow != null)
            {
                iRow.InvalidateArrange();
            }
        }
    }
}