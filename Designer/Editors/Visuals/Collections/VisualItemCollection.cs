// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The visual item collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The visual item collection.</summary>
    public class VisualItemCollection : ClusterCollection<VisualItem>
    {
        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="VisualItem"/>.</returns>
        public override VisualItem GetWrapperFor(Neuron toWrap)
        {
            var iInt = toWrap as IntNeuron;
            if (iInt != null)
            {
                return new VisualItem(iInt);
            }

            throw new System.InvalidOperationException("Int values expected as content for visual frames");
        }

        /// <summary>The get list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.Frame;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="VisualItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public VisualItemCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="VisualItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public VisualItemCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
        }

        #endregion
    }
}