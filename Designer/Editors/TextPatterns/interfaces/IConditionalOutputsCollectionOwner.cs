// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConditionalOutputsCollectionOwner.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> for all objects that have a
//   <see cref="ConditionalOutputs" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an <see langword="interface" /> for all objects that have a
    ///     <see cref="ConditionalOutputs" /> .
    /// </summary>
    public interface IConditionalOutputsCollectionOwner
    {
        /// <summary>
        ///     Gets the list of conditional output sets.
        /// </summary>
        ConditionalOutputsCollection Conditionals { get; }
    }
}