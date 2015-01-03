// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataToSearchConverter.cs" company="">
//   
// </copyright>
// <summary>
//   converts the <see cref="DataToSearch" /> <see langword="enum" /> to a
//   bool.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     converts the <see cref="DataToSearch" /> <see langword="enum" /> to a
    ///     bool.
    /// </summary>
    public class DataToSearchConverter : System.Windows.Data.IValueConverter
    {
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
            return (DataToSearch)value == (DataToSearch)parameter;
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
            return parameter;

                // don't matter if the user selects true or false. When the item gets clicked, we return the parameter. This prevents the user from unselecting any item.
        }

        #endregion
    }
}