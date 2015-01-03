// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronToInfoConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a neuron or it's id to it's <see cref="NeuronData" /> object so
//   we can display a proper title + get warned when it is changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Converts a neuron or it's id to it's <see cref="NeuronData" /> object so
    ///     we can display a proper title + get warned when it is changed.
    /// </summary>
    /// <remarks>
    ///     <para>This converter works in 2 ways.</para>
    ///     <para>
    ///         Major defect: ToString of <see cref="NeuronData" /> return the
    ///         displayTitle. When this is updated, UI's using this converter aren't
    ///         updated, so only use in situations where no display names can be changed.
    ///     </para>
    /// </remarks>
    internal class NeuronToInfoConverter : System.Windows.Data.IValueConverter
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
            var iVal = value as Neuron;
            if (iVal != null)
            {
                return BrainData.Current.NeuronInfo[iVal.ID];
            }

            return value;
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
            var iVal = value as NeuronData;
            if (iVal != null)
            {
                return Brain.Current[iVal.ID];
            }

            return value;
        }

        #endregion
    }
}