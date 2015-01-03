// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyTreeView.cs" company="">
//   
// </copyright>
// <summary>
//   A treeView that fixes a Bug in the WPF treeview, when Virtualization is
//   turned on. This was for WPF Version 3.5.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A treeView that fixes a Bug in the WPF treeview, when Virtualization is
    ///     turned on. This was for WPF Version 3.5.
    /// </summary>
    public class MyTreeView : System.Windows.Controls.TreeView
    {
        /// <summary>
        ///     Creates a <see cref="System.Windows.Controls.TreeViewItem" /> to use to display content.
        /// </summary>
        /// <returns>
        ///     A new <see cref="System.Windows.Controls.TreeViewItem" /> to use as a container for content.
        /// </returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new MyTreeViewItem();
        }

        /// <summary>Determines whether the specified <paramref name="item"/> is its own
        ///     container or can be its own container.</summary>
        /// <param name="item">The object to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is a<see cref="System.Windows.Controls.TreeViewItem"/> ; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MyTreeViewItem;
        }
    }

    /// <summary>
    ///     The treeview item that actually fixes the bug.
    /// </summary>
    public class MyTreeViewItem : System.Windows.Controls.TreeViewItem
    {
        /// <summary>Provides class handling for the<see cref="System.Windows.UIElement.GotFocus"/> event.</summary>
        /// <param name="e">The event data.</param>
        protected override void OnGotFocus(System.Windows.RoutedEventArgs e)
        {
            IsSelected = true;
            RaiseEvent(e);
        }

        /// <summary>
        ///     Creates a new <see cref="System.Windows.Controls.TreeViewItem" /> to use to display the
        ///     object.
        /// </summary>
        /// <returns>
        ///     A new <see cref="System.Windows.Controls.TreeViewItem" /> .
        /// </returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new MyTreeViewItem();
        }

        /// <summary>Determines whether an object is a <see cref="System.Windows.Controls.TreeViewItem"/> .</summary>
        /// <param name="item">The object to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is a<see cref="System.Windows.Controls.TreeViewItem"/> ; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MyTreeViewItem;
        }
    }
}