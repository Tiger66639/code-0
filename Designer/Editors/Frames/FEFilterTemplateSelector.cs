// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FEFilterTemplateSelector.cs" company="">
//   
// </copyright>
// <summary>
//   The fe filter template selector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The fe filter template selector.</summary>
    public class FEFilterTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>
        ///     Gets or sets the template for restrictions.
        /// </summary>
        /// <value>
        ///     The restrictions.
        /// </value>
        public System.Windows.DataTemplate Restrictions { get; set; }

        /// <summary>
        ///     Gets or sets the tempalte for custom filters.
        /// </summary>
        /// <value>
        ///     The custom filter.
        /// </value>
        public System.Windows.DataTemplate CustomFilter { get; set; }

        /// <summary>
        ///     Gets or sets the template for filter groups.
        /// </summary>
        /// <value>
        ///     The filter group.
        /// </value>
        public System.Windows.DataTemplate FilterGroup { get; set; }

        /// <summary>
        ///     Gets or sets the root filter.
        /// </summary>
        /// <value>
        ///     The root filter.
        /// </value>
        public System.Windows.DataTemplate RootFilter { get; set; }

        /// <summary>
        ///     Gets or sets the template to use for semantic restriction.
        /// </summary>
        /// <value>
        ///     The semantic restriction.
        /// </value>
        public System.Windows.DataTemplate SemanticRestriction { get; set; }

        /// <summary>
        ///     Gets or sets the <see langword="bool" /> filter.
        /// </summary>
        /// <value>
        ///     The root filter.
        /// </value>
        public System.Windows.DataTemplate BoolFilter { get; set; }

        /// <summary>When overridden in a derived class, returns a<see cref="System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="System.Windows.DataTemplate"/> or null. The default value is
        ///     null.</returns>
        public override System.Windows.DataTemplate SelectTemplate(
            object item, 
            System.Windows.DependencyObject container)
        {
            if (item is FECustomRestriction)
            {
                return CustomFilter;
            }

            if (item is FERestrictionRoot)
            {
                return RootFilter;
            }

            if (item is FERestrictionGroup)
            {
                return FilterGroup;
            }

            if (item is FERestrictionBool)
            {
                return BoolFilter;
            }

            if (item is FERestriction)
            {
                var iItem = (FERestriction)item;
                var iEl = iItem.Root;
                if (iEl != null)
                {
                    var iFrame = iEl.Owner as Frame;
                    if (iFrame.Owner is FrameEditor)
                    {
                        return Restrictions;
                    }

                    return SemanticRestriction;
                }

                return Restrictions;
            }

            return base.SelectTemplate(item, container);
        }
    }
}