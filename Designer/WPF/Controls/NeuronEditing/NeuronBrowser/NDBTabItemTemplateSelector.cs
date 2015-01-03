// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBTabItemTemplateSelector.cs" company="">
//   
// </copyright>
// <summary>
//   The ndb tab item template selector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The ndb tab item template selector.</summary>
    public class NDBTabItemTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>
        ///     Gets or sets the template to use when the data needs to be displayed
        ///     as a list.
        /// </summary>
        /// <value>
        ///     The list template.
        /// </value>
        public System.Windows.DataTemplate ListTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the template to use when the data needs to be displayed
        ///     as a tree.
        /// </summary>
        /// <value>
        ///     The tree template.
        /// </value>
        public System.Windows.DataTemplate TreeTemplate { get; set; }

        /// <summary>
        ///     The template that should be used for a date selection page.
        /// </summary>
        public System.Windows.DataTemplate DateTemplate { get; set; }

        /// <summary>When overridden in a derived class, returns a<see cref="System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="System.Windows.DataTemplate"/> or null. The default value is
        ///     null.</returns>
        public override System.Windows.DataTemplate SelectTemplate(
            object item, 
            System.Windows.DependencyObject container)
        {
            var iItem = item as WPF.Controls.BrowserDataSource.BDSWrapper;
            if (iItem.Data.AsDate)
            {
                return DateTemplate;
            }

            if (iItem.Data.AsTree)
            {
                return TreeTemplate;
            }

            return ListTemplate;
        }
    }
}