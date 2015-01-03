// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemStatic.cs" company="">
//   
// </copyright>
// <summary>
//   The code item static.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item static.</summary>
    public class CodeItemStatic : CodeItemResult
    {
        /// <summary>Initializes a new instance of the <see cref="CodeItemStatic"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemStatic(Neuron toWrap, bool isActive)
        {
            IsActive = isActive;
            Item = toWrap;
        }
    }
}