// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusRelItem.cs" company="">
//   
// </copyright>
// <summary>
//   The thesaurus rel item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The thesaurus rel item.</summary>
    public class ThesaurusRelItem : Data.OwnedObject<ThesaurusItem>, INeuronWrapper, INeuronInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ThesaurusRelItem"/> class. Initializes a new instance of the <see cref="SynonymItem"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ThesaurusRelItem(Neuron toWrap)
        {
            // Debug.Assert(toWrap != null); we alllow null as wrapped: to allow for 'no filter'.
            Item = toWrap;
        }

        #region NeuronInfo (INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (Item != null && BrainData.Current.NeuronInfo != null)
                {
                    // when doing 'new' the neuronInfo is sometimes not yet set.
                    return BrainData.Current.NeuronInfo[Item.ID];
                }

                return null;
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; internal set; }

        #endregion
    }
}