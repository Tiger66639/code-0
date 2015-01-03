// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryColDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   provides drop support for moving the column headers around on the  editors.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides drop support for moving the column headers around on the  editors.
    /// </summary>
    public class QueryColDropAdvisor : DnD.DropTargetBase
    {
        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub lists don't get used but only the main list cause this gets the events
        ///     first,
        ///     while we usually want to drop in a sublist.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        #endregion

        /// <summary>Gets the target.</summary>
        public EditorColumn Target
        {
            get
            {
                return (EditorColumn)((System.Windows.FrameworkElement)TargetUI).DataContext;
            }
        }

        /// <summary>
        ///     Called when a column got moved. Allows the ui to update further.
        /// </summary>
        public event System.EventHandler ColumnMoved;

        #region Overrides

        /// <summary>Raises the <see cref="E:DropCompleted"/> event.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iIndex = (int)arg.Data.GetData(Properties.Resources.ASSETCOLUMN);

            var iCol = Target;
            var iEdior = iCol.Owner as QueryEditor;
            if (iEdior != null)
            {
                var iNewIndex = iCol.Index;
                if (iCol.Index < iIndex)
                {
                    foreach (var i in iEdior.Columns)
                    {
                        if (i.Index >= iCol.Index && i.Index < iIndex)
                        {
                            i.Index++;
                        }
                        else if (i.Index == iIndex)
                        {
                            i.Index = iNewIndex;
                        }
                    }
                }
                else
                {
                    foreach (var i in iEdior.Columns)
                    {
                        if (i.Index > iIndex && i.Index <= iCol.Index)
                        {
                            i.Index--;
                        }
                        else if (i.Index == iIndex)
                        {
                            i.Index = iNewIndex;
                        }
                    }
                }
            }

            if (ColumnMoved != null)
            {
                ColumnMoved(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Determines whether [is valid data object] [the specified obj].</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise, <c>false</c>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return Target != null && Target.Index > 0 && obj.GetDataPresent(Properties.Resources.ASSETCOLUMN);
        }

        #endregion
    }
}