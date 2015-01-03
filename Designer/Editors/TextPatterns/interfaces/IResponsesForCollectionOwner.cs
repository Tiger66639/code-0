// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IResponsesForCollectionOwner.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that owners of
//   <see cref="ResponseForOutput" /> collections should implement so that the
//   <see cref="ResponseForOutput" /> object can manipulate it's owner
//   correctly.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an <see langword="interface" /> that owners of
    ///     <see cref="ResponseForOutput" /> collections should implement so that the
    ///     <see cref="ResponseForOutput" /> object can manipulate it's owner
    ///     correctly.
    /// </summary>
    internal interface IResponseForOutputOwner
    {
        /// <summary>
        ///     Gets/sets the list of outputs for which this can be a response.
        /// </summary>
        ResponsesForCollection ResponseFor { get; }
    }
}