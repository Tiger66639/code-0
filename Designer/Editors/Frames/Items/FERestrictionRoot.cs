// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionRoot.cs" company="">
//   
// </copyright>
// <summary>
//   the root of all the restrictions (the cluster), which also defines the
//   initial logical operator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     the root of all the restrictions (the cluster), which also defines the
    ///     initial logical operator.
    /// </summary>
    public class FERestrictionRoot : FERestrictionGroup
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionRoot"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestrictionRoot(NeuronCluster toWrap)
            : base(toWrap)
        {
        }
    }
}