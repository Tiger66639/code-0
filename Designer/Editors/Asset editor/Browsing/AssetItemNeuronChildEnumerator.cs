// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetItemNeuronChildEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   terminator for browsing through asset data Wraps round a single neuron's
//   result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     terminator for browsing through asset data Wraps round a single neuron's
    ///     result.
    /// </summary>
    public class AssetItemNeuronChildEnumerator : INeuronInfo, INeuronWrapper
    {
        /// <summary>Initializes a new instance of the <see cref="AssetItemNeuronChildEnumerator"/> class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="title">The title.</param>
        public AssetItemNeuronChildEnumerator(Neuron item, string title)
        {
            Title = title;
            Item = item;
        }

        #region Title

        /// <summary>
        ///     Gets the title to use
        /// </summary>
        public string Title { get; private set; }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item];
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion
    }
}