// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanesTemplateSelector.cs" company="">
//   
// </copyright>
// <summary>
//   selects the template for the main window's dockingcontrol
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     selects the template for the main window's dockingcontrol
    /// </summary>
    internal class PanesTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>When overridden in a derived class, returns a<see cref="System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="System.Windows.DataTemplate"/> or null. The default value is
        ///     null.</returns>
        public override System.Windows.DataTemplate SelectTemplate(
            object item, 
            System.Windows.DependencyObject container)
        {
            if (item is ToolViewItem)
            {
                return ((ToolViewItem)item).DataTemplate;
            }

            return null;
        }
    }
}