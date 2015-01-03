// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdditionalFilesSourceConverter.cs" company="">
//   
// </copyright>
// <summary>
//   converts the list to a composite (doesn't work from xaml).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     converts the list to a composite (doesn't work from xaml).
    /// </summary>
    public class AdditionalFilesSourceConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        ///     Gets or sets the template to use for creating new rules.
        /// </summary>
        /// <value>
        ///     The new rule template.
        /// </value>
        public System.Windows.Controls.ListBoxItem ExtraItem { get; set; }

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
            var iCol = new System.Windows.Data.CollectionContainer();
            var iBind = new System.Windows.Data.Binding();
            iBind.Source = value;
            System.Windows.Data.BindingOperations.SetBinding(
                iCol, 
                System.Windows.Data.CollectionContainer.CollectionProperty, 
                iBind);
            iRes.Add(iCol);
            if (ExtraItem != null)
            {
                iRes.Add(ExtraItem);
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