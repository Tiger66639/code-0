// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternsItemsSourceConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Used to convert the ItemsSource list to a composite collection containing
//   the itemsSource
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to convert the ItemsSource list to a composite collection containing
    ///     the itemsSource
    /// </summary>
    public class PatternsItemsSourceConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        ///     Gets or sets the template to use for creating new rules.
        /// </summary>
        /// <value>
        ///     The new rule template.
        /// </value>
        public System.Windows.Controls.ControlTemplate NewRuleTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the template to use for creating new rules.
        /// </summary>
        /// <value>
        ///     The new rule template.
        /// </value>
        public System.Windows.DataTemplate RuleTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the template to use for questions.
        /// </summary>
        /// <value>
        ///     The new question template.
        /// </value>
        public System.Windows.Controls.ControlTemplate NewQuestionTemplate { get; set; }

        /// <summary>
        ///     the template that should be used for the topic details
        /// </summary>
        public System.Windows.Controls.ControlTemplate TopicDetailsTemplate { get; set; }

        #region IValueConverter Members

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid<see langword="null"/> <paramref name="value"/> is used.</returns>
        public object Convert(
            object value, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            var iRes = new System.Windows.Data.CompositeCollection();
            if (value != null)
            {
                if (value is TopicFilterCollection)
                {
                    var iAdder = new System.Windows.Controls.ContentControl();
                    iAdder.Template = TopicDetailsTemplate;
                    iRes.Add(iAdder);
                }
                else if (value is PatternRule)
                {
                    // for master-detail view.
                    // ContentControl iAdder = new ContentControl();
                    // iAdder.ContentTemplate = RuleTemplate;
                    // iAdder.Content = value;
                    iRes.Add(value);

                    var iAdder = new System.Windows.Controls.ContentControl();
                    iAdder.Template = NewRuleTemplate;
                    iRes.Add(iAdder);
                }
                else
                {
                    var iCol = new System.Windows.Data.CollectionContainer();
                    var iBind = new System.Windows.Data.Binding();
                    iBind.Source = value;
                    System.Windows.Data.BindingOperations.SetBinding(
                        iCol, 
                        System.Windows.Data.CollectionContainer.CollectionProperty, 
                        iBind);
                    iRes.Add(iCol);

                    var iAdder = new System.Windows.Controls.ContentControl();
                    if (value is PatternDefCollection)
                    {
                        iAdder.Template = NewRuleTemplate;
                    }
                    else
                    {
                        iAdder.Template = NewQuestionTemplate;
                    }

                    iRes.Add(iAdder);
                }
            }
            else
            {
                var iAdder = new System.Windows.Controls.ContentControl();
                iAdder.Template = NewRuleTemplate;
                iRes.Add(iAdder);
            }

            return iRes;
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid<see langword="null"/> <paramref name="value"/> is used.</returns>
        public object ConvertBack(
            object value, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}