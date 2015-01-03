// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticFlowItemContentSelector.cs" company="">
//   
// </copyright>
// <summary>
//   Provides custom logic for the <see cref="FlowItemStaticView" /> to select
//   the appropriate template
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides custom logic for the <see cref="FlowItemStaticView" /> to select
    ///     the appropriate template
    /// </summary>
    public class StaticFlowItemContentSelector : System.Windows.Controls.DataTemplateSelector
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
            var iCont = container as System.Windows.FrameworkElement;
            if (Properties.Settings.Default.FlowItemDisplayMode == 0)
            {
                return iCont.TryFindResource("FlowItemStaticNormalView") as System.Windows.DataTemplate;
            }

            if (Properties.Settings.Default.FlowItemDisplayMode == 1)
            {
                return iCont.TryFindResource("FlowItemStaticLeftIcon") as System.Windows.DataTemplate;
            }

            if (Properties.Settings.Default.FlowItemDisplayMode == 2)
            {
                return iCont.TryFindResource("FlowItemStaticUnderIcon") as System.Windows.DataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}