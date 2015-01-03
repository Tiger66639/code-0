// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryDataCellControl.cs" company="">
//   
// </copyright>
// <summary>
//   a custom control object used to represent a single cell in the output
//   grid. Provides a fast way to size the cell to the appropriate column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a custom control object used to represent a single cell in the output
    ///     grid. Provides a fast way to size the cell to the appropriate column.
    /// </summary>
    public class QueryDataCellControl : System.Windows.Controls.ContentControl
    {
        /// <summary>The f editor.</summary>
        private QueryEditor fEditor;

        /// <summary>Gets the editor.</summary>
        private QueryEditor Editor
        {
            get
            {
                if (fEditor == null)
                {
                    var iView = ControlFramework.Utility.TreeHelper.FindInTree<QueryEditorView>(this);
                    if (iView != null)
                    {
                        fEditor = (QueryEditor)iView.DataContext;
                    }
                }

                return fEditor;
            }
        }

        /// <summary>Called to remeasure a control.</summary>
        /// <param name="constraint">The maximum size that the method can return.</param>
        /// <returns>The size of the control, up to the maximum specified by<paramref name="constraint"/> .</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            var iEditor = Editor;
            if (iEditor != null)
            {
                var iIndex = System.Windows.Controls.ItemsControl.GetAlternationIndex(this);
                if (iIndex > -1 && iIndex < iEditor.Columns.Count)
                {
                    constraint.Width = iEditor.Columns[iIndex].Width;
                }
            }

            var iRes = base.MeasureOverride(constraint);
            iRes.Width = constraint.Width;
            return iRes;
        }

        /// <summary>The arrange override.</summary>
        /// <param name="arrangeBounds">The arrange bounds.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            var iEditor = Editor;
            if (iEditor != null)
            {
                var iIndex = System.Windows.Controls.ItemsControl.GetAlternationIndex(this);
                if (iIndex > -1 && iIndex < iEditor.Columns.Count)
                {
                    arrangeBounds.Width = iEditor.Columns[iIndex].Width;
                }
            }

            var iRes = base.ArrangeOverride(arrangeBounds);
            return iRes;
        }
    }
}