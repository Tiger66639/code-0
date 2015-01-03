// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FECustomRestriction.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a restriction definition for a frame element, where the
//   restriction consists out of custom code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides a restriction definition for a frame element, where the
    ///     restriction consists out of custom code.
    /// </summary>
    /// <remarks>
    ///     Doesn't have any properties. It simply has a code cluster attached to the
    ///     neuron, no data needs to be visible in the frame element, only the name
    ///     of the neuron.
    /// </remarks>
    public class FECustomRestriction : FERestrictionBase
    {
        /// <summary>Initializes a new instance of the <see cref="FECustomRestriction"/> class. Initializes a new instance of the <see cref="FELinkRestriction"/>
        ///     class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        public FECustomRestriction(Neuron toWrap)
            : base(toWrap)
        {
        }
    }
}