// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FirstAssetColWidthConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Makes certain that the first column of the asset editor always ends at
//   the same location. if we don't do this, nested rows will always jump more
//   and more to the left because the of the expander boxes. This adjusts for
//   that.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Makes certain that the first column of the asset editor always ends at
    ///     the same location. if we don't do this, nested rows will always jump more
    ///     and more to the left because the of the expander boxes. This adjusts for
    ///     that.
    /// </summary>
    internal class FirstAssetColWidthConverter : System.Windows.Data.IMultiValueConverter
    {
        #region IValueConverter Members

        /// <summary>Converts source <paramref name="values"/> to a value for the binding
        ///     target. The data binding engine calls this method when it propagates
        ///     the <paramref name="values"/> from source bindings to the binding
        ///     target. -binding 1: the width of the column -binding 2: the level of
        ///     the current row -binding 3: the width for a single offset level.</summary>
        /// <param name="values">The array of values that the source bindings in the<see cref="System.Windows.Data.MultiBinding"/> produces. The value<see cref="System.Windows.DependencyProperty.UnsetValue"/> indicates
        ///     that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value.If the method returns null, the valid<see langword="null"/> value is used.A return value of<see cref="System.Windows.DependencyProperty"/> .<see cref="System.Windows.DependencyProperty.UnsetValue"/> indicates
        ///     that the converter did not produce a value, and that the binding will
        ///     use the <see cref="System.Windows.Data.BindingBase.FallbackValue"/>
        ///     if it is available, or else will use the default value.A return value
        ///     of <see cref="System.Windows.Data.Binding"/> .<see cref="System.Windows.Data.Binding.DoNothing"/> indicates that
        ///     the binding does not transfer the value or use the<see cref="System.Windows.Data.BindingBase.FallbackValue"/> or the
        ///     default value.</returns>
        public object Convert(
            object[] values, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            var iRes = (double)values[0] - ((int)values[1] * (double)values[2]);
            if (iRes >= 0)
            {
                return iRes;
            }

            return 0;
        }

        #endregion

        #region IMultiValueConverter Members

        /// <summary>Converts a binding target <paramref name="value"/> to the source
        ///     binding values.</summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the
        ///     number and types of values that are suggested for the method to
        ///     return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>An array of values that have been converted from the target<paramref name="value"/> back to the source values.</returns>
        public object[] ConvertBack(
            object value, 
            System.Type[] targetTypes, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}