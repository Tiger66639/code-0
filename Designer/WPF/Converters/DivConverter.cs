// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DivConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Takes a <see langword="double" /> and returns half of the value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Takes a <see langword="double" /> and returns half of the value.
    /// </summary>
    public class DivConverter : System.Windows.Data.IValueConverter
    {
        #region IValueConverter Members

        /// <summary>The convert.</summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public object Convert(
            object value, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            return (double)value / 2;
        }

        /// <summary>The convert back.</summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public object ConvertBack(
            object value, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            return (double)value * 2;
        }

        #endregion
    }
}