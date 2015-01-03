// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiBoolConverter.cs" company="">
//   
// </copyright>
// <summary>
//   A converter that expects all <see langword="bool" /> values and returns
//   wether all are <see langword="true" /> or not. this converter works 2
//   ways: when converting back, it returns all 'true' or all 'false'
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    // not used
    /// <summary>
    ///     A converter that expects all <see langword="bool" /> values and returns
    ///     wether all are <see langword="true" /> or not. this converter works 2
    ///     ways: when converting back, it returns all 'true' or all 'false'
    /// </summary>
    public class MultiBoolConverter : System.Windows.Data.IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>The convert.</summary>
        /// <param name="values">The values.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public object Convert(
            object[] values, 
            System.Type targetType, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            foreach (var i in values)
            {
                if (!(i is bool) || (bool)i == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The convert back.</summary>
        /// <param name="value">The value.</param>
        /// <param name="targetTypes">The target types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="object[]"/>.</returns>
        public object[] ConvertBack(
            object value, 
            System.Type[] targetTypes, 
            object parameter, 
            System.Globalization.CultureInfo culture)
        {
            var iRes = new object[targetTypes.Length];
            for (var i = 0; i < targetTypes.Length; i++)
            {
                iRes[i] = value;
            }

            return iRes;
        }

        #endregion
    }
}