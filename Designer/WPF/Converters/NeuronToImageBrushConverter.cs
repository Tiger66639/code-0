// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronToImageBrushConverter.cs" company="">
//   
// </copyright>
// <summary>
//   converts a neuron to a BitmapCacheBrush (which gets cached for future
//   use). This is used for high-performance image rendering like asset
//   editors.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     converts a neuron to a BitmapCacheBrush (which gets cached for future
    ///     use). This is used for high-performance image rendering like asset
    ///     editors.
    /// </summary>
    public class NeuronToImageBrushConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>The f cache.</summary>
        private static readonly System.Collections.Generic.Dictionary<string, System.Windows.Media.BitmapCacheBrush>
            fCache = new System.Collections.Generic.Dictionary<string, System.Windows.Media.BitmapCacheBrush>();

            // contains all the cached brushes. this is static so that every converter uses the same cache.

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
                var iFound = TryGetBrush(string.Format("{0}{1}{2}", Prefix, iVal.ID, Suffix));
                if (iFound == null)
                {
                    iFound = GetResForCluster(iVal as NeuronCluster);
                    if (iFound == null)
                    {
                        iFound = TryGetBrush(string.Format("{0}{1}{2}", Prefix, iVal.GetType().Name, Suffix));
                        if (iFound == null)
                        {
                            var iBase = iVal.GetType().BaseType;
                            while (iBase != null)
                            {
                                iFound = TryGetBrush(string.Format("{0}{1}{2}", Prefix, iBase.Name, Suffix));
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

        /// <summary>Tries the find a brush with the specified name. If there is no brush,
        ///     but there is a resource, make a brush and cache it.</summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>The <see cref="object"/>.</returns>
        private object TryGetBrush(string name)
        {
            System.Windows.Media.BitmapCacheBrush iRes = null;
            if (fCache.TryGetValue(name, out iRes))
            {
                return iRes;
            }

            var iFound = System.Windows.Application.Current.TryFindResource(name) as System.Windows.Media.ImageSource;
            if (iFound != null)
            {
                var iImg = new System.Windows.Controls.Image();
                iImg.Source = iFound;
                iImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                iImg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                iImg.Stretch = System.Windows.Media.Stretch.Uniform;

                var iGrid = new System.Windows.Controls.Grid(); // put the image in a grid so that it scales correctly.
                iGrid.Background = System.Windows.Media.Brushes.Transparent;
                iGrid.Height = 16;

                    // we provide a fixed size so that the inner image is layed out correctly (otherwise the image gets stretched out.
                iGrid.Width = 16;
                iGrid.Children.Add(iImg);

                iRes = new System.Windows.Media.BitmapCacheBrush(iGrid);
                if (iRes.CanFreeze)
                {
                    iRes.Freeze();
                }

                fCache.Add(name, iRes);
            }

            return iRes;
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
                                TryGetBrush(
                                    string.Format(
                                        "{0}Cluster{1}{2}", 
                                        Prefix, 
                                        System.Enum.GetName(typeof(PredefinedNeurons), iPos.ID), 
                                        Suffix));
                        }

                        return
                            TryGetBrush(
                                string.Format(
                                    "{0}Cluster{1}{2}", 
                                    Prefix, 
                                    System.Enum.GetName(typeof(PredefinedNeurons), iMeaning), 
                                    Suffix));
                    }

                    return
                        TryGetBrush(
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