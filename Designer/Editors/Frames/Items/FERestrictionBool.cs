// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionBool.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for a <see langword="bool" /> expression, used as frame element
//   restriction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for a <see langword="bool" /> expression, used as frame element
    ///     restriction.
    /// </summary>
    public class FERestrictionBool : FERestrictionBase
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionBool"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestrictionBool(Neuron toWrap)
            : base(toWrap)
        {
            Filter = new CodeItemBoolExpression((BoolExpression)toWrap, true);
        }

        /// <summary>Gets the filter.</summary>
        public CodeItemBoolExpression Filter { get; private set; }
    }
}