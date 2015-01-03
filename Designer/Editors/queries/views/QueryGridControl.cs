// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryGridControl.cs" company="">
//   
// </copyright>
// <summary>
//   a custom listbox that uses <see cref="QueryRowView" /> s as children.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a custom listbox that uses <see cref="QueryRowView" /> s as children.
    /// </summary>
    public class QueryGridControl : System.Windows.Controls.ListBox
    {
        /// <summary>Determines if the specified <paramref name="item"/> is (or is eligible
        ///     to be) its own container.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="item"/> is (or is
        ///     eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is QueryRowView;
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
            return new QueryRowView();
        }

        /// <summary>
        ///     re-renders all the children. Only works if the itemssource is an IList
        /// </summary>
        internal void InvalidateChildren()
        {
            var iSource = ItemsSource as System.Collections.IList;
            if (iSource != null)
            {
                var iFoundStart = false;
                for (var i = 0; i < iSource.Count; i++)
                {
                    var iCtrl = (QueryRowView)ItemContainerGenerator.ContainerFromIndex(i);
                    if (iCtrl != null)
                    {
                        iCtrl.InvalidateChildren();
                        iCtrl.InvalidateMeasure();
                        iFoundStart = true;
                    }
                    else if (iFoundStart)
                    {
                        break;
                    }
                }
            }
        }
    }
}