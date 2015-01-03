// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeToResourceConverter.cs" company="">
//   
// </copyright>
// <summary>
//   return the resource with key: 'Img' + the name of the type (passed in
//   through the value). If not found, tries to go up the inheritence tree of
//   the type untill it finds a resource. Very similar to
//   <see cref="NeuronToImageConverter" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     return the resource with key: 'Img' + the name of the type (passed in
    ///     through the value). If not found, tries to go up the inheritence tree of
    ///     the type untill it finds a resource. Very similar to
    ///     <see cref="NeuronToImageConverter" />
    /// </summary>
    /// <remarks>
    ///     convert from: Type convert to: ojbect
    /// </remarks>
    public class TypeToResourceConverter : System.Windows.Data.IValueConverter
    {
        #region IValueConverter Members;

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
            var iVal = value as System.Type;
            if (iVal != null)
            {
                var iFound = System.Windows.Application.Current.TryFindResource("Img" + iVal.Name);
                if (iFound == null)
                {
                    var iBase = iVal.BaseType;
                    while (iBase != null)
                    {
                        iFound = System.Windows.Application.Current.TryFindResource("Img" + iBase.Name);
                        if (iFound != null)
                        {
                            break;
                        }

                        iBase = iBase.BaseType;
                    }
                }

                return iFound;
            }

            return null;
        }

        /// <summary>The convert back.</summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
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