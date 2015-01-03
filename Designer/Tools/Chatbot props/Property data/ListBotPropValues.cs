// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListBotPropValues.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the values assigned to a <see cref="ListBotPropDecl" /> . It
//   wraps around a cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the values assigned to a <see cref="ListBotPropDecl" /> . It
    ///     wraps around a cluster.
    /// </summary>
    public class ListBotPropValues : ClusterCollection<ListBotPropValue>
    {
        /// <summary>Initializes a new instance of the <see cref="ListBotPropValues"/> class. Initializes a new instance of the <see cref="ListBotPropValues"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toWrap">To wrap.</param>
        public ListBotPropValues(INeuronWrapper owner, NeuronCluster toWrap)
            : base(owner, toWrap)
        {
        }

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ListBotPropValue"/>.</returns>
        public override ListBotPropValue GetWrapperFor(Neuron toWrap)
        {
            return new ListBotPropValue(toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.List;
        }
    }
}