// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBrowserItemSelector.cs" company="">
//   
// </copyright>
// <summary>
//   A DataTemplate selector for the treeview in the
//   <see cref="NeuronDataBrowser" /> control
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A DataTemplate selector for the treeview in the
    ///     <see cref="NeuronDataBrowser" /> control
    /// </summary>
    public class NDBrowserItemSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>
        ///     Gets or sets the template to assign to neuronData.
        /// </summary>
        /// <value>
        ///     The neuron data template.
        /// </value>
        public System.Windows.DataTemplate NeuronDataTemplate { get; set; }

        /// <summary>Gets or sets the neuron info template.</summary>
        public System.Windows.DataTemplate NeuronInfoTemplate { get; set; }

        /// <summary>When overridden in a derived class, returns a<see cref="System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="System.Windows.DataTemplate"/> or null. The default value is
        ///     null.</returns>
        public override System.Windows.DataTemplate SelectTemplate(
            object item, 
            System.Windows.DependencyObject container)
        {
            if (item is INeuronInfo)
            {
                return NeuronInfoTemplate;
            }

            if (item is NeuronData)
            {
                return NeuronDataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}