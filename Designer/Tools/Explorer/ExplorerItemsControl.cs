// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerItemsControl.cs" company="">
//   
// </copyright>
// <summary>
//   An itemscontrol that provides a different wrapper than an
//   ContentPResenteer, so that we can assign some properties easier, like the
//   forground.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An itemscontrol that provides a different wrapper than an
    ///     ContentPResenteer, so that we can assign some properties easier, like the
    ///     forground.
    /// </summary>
    /// <remarks>
    ///     uses a contentControl instead of an ContentPResenteer
    /// </remarks>
    public class ExplorerItemsControl : System.Windows.Controls.ItemsControl
    {
        /// <summary>
        ///     Creates or identifies the element that is used to display the given
        ///     item.
        /// </summary>
        /// <returns>
        ///     The element that is used to display the given item.
        /// </returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            var iRes = new System.Windows.Controls.ContentControl();
            return iRes;
        }

        /// <summary>Determines if the specified <paramref name="item"/> is (or is eligible
        ///     to be) its own container.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="item"/> is (or is
        ///     eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is System.Windows.Controls.ContentControl;
        }
    }
}