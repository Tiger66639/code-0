// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   used by the output, to keep track of the type of uppercasing that needs
//   to happen to the next word that comes through the pipeline.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     used by the output, to keep track of the type of uppercasing that needs
    ///     to happen to the next word that comes through the pipeline.
    /// </summary>
    internal enum UppercaseStyle
    {
        /// <summary>The none.</summary>
        None, 

        /// <summary>The first letter.</summary>
        FirstLetter, 

        /// <summary>The all.</summary>
        All, 

        /// <summary>The map.</summary>
        Map, 

        /// <summary>The all low.</summary>
        AllLow
    }

    /// <summary>
    ///     used to build the output string. This is a wrapper class for the
    ///     stringbuilder that adds support for making words uppercase when needed.
    /// </summary>
    internal class OutputBuilder
    {
        /// <summary>The f uppercase map.</summary>
        private System.Collections.Generic.List<bool> fUppercaseMap;

        /// <summary>The f output.</summary>
        private readonly System.Text.StringBuilder fOutput = new System.Text.StringBuilder();

        #region UppercaseForNext

        /// <summary>
        ///     Gets/sets the uppercase style to use for the next word that gets
        ///     appended.
        /// </summary>
        public UppercaseStyle UppercaseForNext { get; set; }

        #endregion

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return fOutput.ToString();
        }

        /// <summary>Appends the specified <paramref name="value"/> to the output.</summary>
        /// <param name="value">The value.</param>
        internal void Append(string value)
        {
            switch (UppercaseForNext)
            {
                case UppercaseStyle.None:
                    fOutput.Append(value);
                    break;
                case UppercaseStyle.FirstLetter:
                    fOutput.Append(char.ToUpper(value[0]));
                    fOutput.Append(value.Substring(1));
                    break;
                case UppercaseStyle.All:
                    fOutput.Append(value.ToUpper());
                    break;
                case UppercaseStyle.AllLow:
                    fOutput.Append(value.ToLower());
                    break;
                case UppercaseStyle.Map:
                    for (var i = 0; i < value.Length; i++)
                    {
                        if (fUppercaseMap.Count > i && fUppercaseMap[i])
                        {
                            fOutput.Append(char.ToUpper(value[i]));
                        }
                        else
                        {
                            fOutput.Append(value[i]);
                        }
                    }

                    fUppercaseMap = null;
                    break;
                default:
                    fOutput.Append(value);
                    break;
            }

            UppercaseForNext = UppercaseStyle.None;
        }

        /// <summary>Changes the <see cref="UppercaseForNext"/> value to map and builds the
        ///     map to use based on the upper case map.</summary>
        /// <param name="toSend">To send.</param>
        internal void StoreUpperCaseMap(NeuronCluster toSend)
        {
            fUppercaseMap = new System.Collections.Generic.List<bool>();
            System.Collections.Generic.List<IntNeuron> iChildren;
            using (var iList = toSend.Children) iChildren = iList.ConvertTo<IntNeuron>();
            foreach (var i in iChildren)
            {
                while (fUppercaseMap.Count < i.Value)
                {
                    fUppercaseMap.Add(false);
                }

                fUppercaseMap.Add(true);
            }

            UppercaseForNext = UppercaseStyle.Map;
        }
    }
}