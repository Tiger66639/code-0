// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentsItemsControl.cs" company="">
//   
// </copyright>
// <summary>
//   a customized items control for the argumnets of
//   (resultStatement)statements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a customized items control for the argumnets of
    ///     (resultStatement)statements.
    /// </summary>
    public class ArgumentsItemsControl : System.Windows.Controls.ItemsControl
    {
        /// <summary>Determines if the specified <paramref name="item"/> is (or is eligible
        ///     to be) its own container.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="item"/> is (or is
        ///     eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is System.Windows.Controls.ContentControl;
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
            return new System.Windows.Controls.ContentControl();
        }
    }
}