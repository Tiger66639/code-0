// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemResult.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for all code items that return a result like a variable or
//   search expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class for all code items that return a result like a variable or
    ///     search expression.
    /// </summary>
    public class CodeItemResult : CodeItem
    {
        /// <summary>Initializes a new instance of the <see cref="CodeItemResult"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        /// <param name="isActive">The is active.</param>
        public CodeItemResult(ResultExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeItemResult"/> class.</summary>
        public CodeItemResult()
        {
        }
    }
}