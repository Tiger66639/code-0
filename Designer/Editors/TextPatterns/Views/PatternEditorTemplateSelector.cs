// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternEditorTemplateSelector.cs" company="">
//   
// </copyright>
// <summary>
//   a template selector
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a template selector
    /// </summary>
    public class PatternEditorTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>
        ///     Gets or sets the template to use for rules.
        /// </summary>
        /// <value>
        ///     The rule template.
        /// </value>
        public System.Windows.DataTemplate RuleTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the template to use for questions.
        /// </summary>
        /// <value>
        ///     The question template.
        /// </value>
        public System.Windows.DataTemplate QuestionTemplate { get; set; }

        /// <summary>When overridden in a derived class, returns a<see cref="System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="System.Windows.DataTemplate"/> or null. The default value is
        ///     null.</returns>
        public override System.Windows.DataTemplate SelectTemplate(
            object item, 
            System.Windows.DependencyObject container)
        {
            if (item is PatternRule)
            {
                return RuleTemplate;
            }

            return QuestionTemplate;
        }
    }
}