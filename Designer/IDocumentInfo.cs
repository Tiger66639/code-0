// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDocumentInfo.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by all objects
//   that are displayed as documents. It provides access to more info of the
//   document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by all objects
    ///     that are displayed as documents. It provides access to more info of the
    ///     document.
    /// </summary>
    public interface IDocumentInfo
    {
        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        string DocumentTitle { get; }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        string DocumentInfo { get; }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        string DocumentType { get; }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        object DocumentIcon { get; }
    }
}