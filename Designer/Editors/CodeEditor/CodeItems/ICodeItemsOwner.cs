// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICodeItemsOwner.cs" company="">
//   
// </copyright>
// <summary>
//   The CodeItemsOwner interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The CodeItemsOwner interface.</summary>
    public interface ICodeItemsOwner
    {
        /// <summary>Gets the items.</summary>
        CodeItemCollection Items { get; }
    }
}