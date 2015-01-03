// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanesStyleSelector.cs" company="">
//   
// </copyright>
// <summary>
//   for the dockingmanager: to select the style for the container.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     for the dockingmanager: to select the style for the container.
    /// </summary>
    internal class PanesStyleSelector : System.Windows.Controls.StyleSelector
    {
        /// <summary>
        ///     Gets or sets the tool style.
        /// </summary>
        /// <value>
        ///     The tool style.
        /// </value>
        public System.Windows.Style ToolStyle { get; set; }

        /// <summary>
        ///     Gets or sets the document style.
        /// </summary>
        /// <value>
        ///     The document style.
        /// </value>
        public System.Windows.Style DocumentStyle { get; set; }

        /// <summary>When overridden in a derived class, returns a <see cref="System.Windows.Style"/>
        ///     based on custom logic.</summary>
        /// <param name="item">The content.</param>
        /// <param name="container">The element to which the style will be applied.</param>
        /// <returns>Returns an application-specific style to apply; otherwise, null.</returns>
        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ToolViewItem)
            {
                return ToolStyle;
            }

            return DocumentStyle;
        }
    }
}