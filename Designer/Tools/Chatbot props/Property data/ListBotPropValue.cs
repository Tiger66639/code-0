// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListBotPropValue.cs" company="">
//   
// </copyright>
// <summary>
//   wraps round a single value that is part of a
//   <see cref="ListBotPropDecl" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     wraps round a single value that is part of a
    ///     <see cref="ListBotPropDecl" /> .
    /// </summary>
    public class ListBotPropValue : Data.ObservableObject, INeuronInfo, INeuronWrapper
    {
        /// <summary>Initializes a new instance of the <see cref="ListBotPropValue"/> class. Initializes a new instance of the <see cref="ListBotPropValue"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ListBotPropValue(Neuron toWrap)
        {
            Item = toWrap;
            NeuronInfo = BrainData.Current.NeuronInfo[toWrap];
        }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo { get; private set; }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion
    }
}