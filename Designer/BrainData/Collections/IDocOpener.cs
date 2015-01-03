// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDocOpener.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by all objects
//   that can be 'opened' as a document (added to the 'openDocsCollection).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by all objects
    ///     that can be 'opened' as a document (added to the 'openDocsCollection).
    /// </summary>
    public interface IDocOpener
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is open.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c> .
        /// </value>
        bool IsOpen { get; set; }
    }
}