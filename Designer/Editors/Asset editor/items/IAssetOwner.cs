// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAssetOwner.cs" company="">
//   
// </copyright>
// <summary>
//   The AssetOwner interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The AssetOwner interface.</summary>
    public interface IAssetOwner
    {
        /// <summary>
        ///     Gets the list of items
        /// </summary>
        System.Collections.Generic.IList<AssetBase> Assets { get; }

        /// <summary>
        ///     Gets the root of the thesaurus
        /// </summary>
        /// <value>
        ///     The root.
        /// </value>
        ObjectEditor Root { get; }
    }
}