// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LeftMarginConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a <see langword="double" /> to a margin where only the left part
//   is filled in.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Converts a <see langword="double" /> to a margin where only the left part
    ///     is filled in.
    /// </summary>
    public class LeftMarginConverter : System.Windows.Data.IValueConverter
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
            var sliderValue = (double)value;
            return new System.Windows.Thickness(sliderValue, 0, 0, 0);
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
            throw new System.Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}