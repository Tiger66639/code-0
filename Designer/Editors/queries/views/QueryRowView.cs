// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryRowView.cs" company="">
//   
// </copyright>
// <summary>
//   An itemscontrol that represents a single row of the query result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     An itemscontrol that represents a single row of the query result.
    /// </summary>
    public class QueryRowView : System.Windows.Controls.ItemsControl
    {
        /// <summary>Determines if the specified <paramref name="item"/> is (or is eligible
        ///     to be) its own container.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="item"/> is (or is
        ///     eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is QueryDataCellControl;
        }

        /// <summary>
        ///     Creates or identifies the element that is used to display the given
        ///     item.
        /// </summary>
        /// <returns>
        ///     The element that is used to display the given item.
        /// </returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new QueryDataCellControl();
        }

        /// <summary>
        ///     re-renders all the children. Only works if the itemssource is an IList
        /// </summary>
        internal void InvalidateChildren()
        {
            var iCount = 0;
            var iSource = ItemsSource as System.Collections.IList;
            if (iSource != null)
            {
                iCount = iSource.Count;
            }
            else if (ItemsSource is System.Windows.Data.ListCollectionView)
            {
                iCount = ((System.Windows.Data.ListCollectionView)ItemsSource).Count;
            }
            else if (ItemsSource is System.Collections.ICollection)
            {
                iCount = ((System.Collections.ICollection)ItemsSource).Count;
            }

            for (var i = 0; i < iCount; i++)
            {
                var iCtrl = (System.Windows.Controls.Control)ItemContainerGenerator.ContainerFromIndex(i);
                iCtrl.InvalidateMeasure();
            }
        }
    }
}