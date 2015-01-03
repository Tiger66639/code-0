// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronToResourceConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Looks up the corresponding ImageSource for the neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Looks up the corresponding ImageSource for the neuron.
    /// </summary>
    /// <remarks>
    ///     <para>required parameter: non Uses App.Current to find resources.</para>
    ///     <para>
    ///         searches resources for the following key: -first try the 'Img' +
    ///         Neuron.id.ToString() -next: 'Img' + typeof(neuron).ToString()
    ///     </para>
    /// </remarks>
    public class NeuronToResourceConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>The f prefix.</summary>
        private string fPrefix = "Img";

        /// <summary>The f suffix.</summary>
        private string fSuffix = string.Empty;

        #region Prefix

        /// <summary>
        ///     Gets/sets the prefix to use for searching the resource (text to insert
        ///     in the front). default is 'Img'.
        /// </summary>
        public string Prefix
        {
            get
            {
                return fPrefix;
            }

            set
            {
                fPrefix = value;
            }
        }

        #endregion

        #region Suffix

        /// <summary>
        ///     Gets/sets the suffix to use for searching the resource (text to append
        ///     at the end). Default is empty.
        /// </summary>
        public string Suffix
        {
            get
            {
                return fSuffix;
            }

            set
            {
                fSuffix = value;
            }
        }

        #endregion

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
            var iVal = value as Neuron;
            if (iVal != null)
            {
                var iFound =
                    System.Windows.Application.Current.TryFindResource(
                        string.Format("{0}{1}{2}", Prefix, iVal.ID, Suffix));
                if (iFound == null)
                {
                    iFound = GetResForCluster(iVal as NeuronCluster);
                    if (iFound == null)
                    {
                        iFound =
                            System.Windows.Application.Current.TryFindResource(
                                string.Format("{0}{1}{2}", Prefix, iVal.GetType().Name, Suffix));
                        if (iFound == null)
                        {
                            var iBase = iVal.GetType().BaseType;
                            while (iBase != null)
                            {
                                iFound =
                                    System.Windows.Application.Current.TryFindResource(
                                        string.Format("{0}{1}{2}", Prefix, iBase.Name, Suffix));
                                if (iFound != null)
                                {
                                    break;
                                }

                                iBase = iBase.BaseType;
                            }
                        }
                    }
                }

                return iFound;
            }

            return null;
        }

        /// <summary>Neuronclusters like objects can have special images.</summary>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private object GetResForCluster(NeuronCluster val)
        {
            if (val != null)
            {
                var iMeaning = val.Meaning;
                if (iMeaning != Neuron.EmptyId)
                {
                    if (iMeaning == (ulong)PredefinedNeurons.POSGroup)
                    {
                        var iPos = val.FindFirstOut((ulong)PredefinedNeurons.POS);
                        if (iPos != null)
                        {
                            return
                                System.Windows.Application.Current.TryFindResource(
                                    string.Format(
                                        "{0}Cluster{1}{2}", 
                                        Prefix, 
                                        System.Enum.GetName(typeof(PredefinedNeurons), iPos.ID), 
                                        Suffix));
                        }

                        return
                            System.Windows.Application.Current.TryFindResource(
                                string.Format(
                                    "{0}Cluster{1}{2}", 
                                    Prefix, 
                                    System.Enum.GetName(typeof(PredefinedNeurons), iMeaning), 
                                    Suffix));
                    }

                    return
                        System.Windows.Application.Current.TryFindResource(
                            string.Format(
                                "{0}Cluster{1}{2}", 
                                Prefix, 
                                System.Enum.GetName(typeof(PredefinedNeurons), iMeaning), 
                                Suffix));
                }
            }

            return null;
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <exception cref="System.NotImplementedException"></exception>
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