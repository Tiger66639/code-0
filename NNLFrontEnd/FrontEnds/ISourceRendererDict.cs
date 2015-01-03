// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISourceRendererDict.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that shoul be implemented by objects that
//   which to render nnl code. The implementor should provide name mappimgs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     an <see langword="interface" /> that shoul be implemented by objects that
    ///     which to render nnl code. The implementor should provide name mappimgs.
    /// </summary>
    public interface ISourceRendererDict
    {
        /// <summary>Gets the name for the object.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetName(ulong id);

        /// <summary>Checks if the object has a <see langword="static"/> name (non
        ///     rendered).</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool HasName(ulong id);

        /// <summary>Create a <see langword="static"/> name for the object.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        string BuildName(ulong id);

        /// <summary>Returns a description as text (no xml formatting allowed) of the item,
        ///     if there is any.</summary>
        /// <param name="toRender"></param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetDescriptionText(ulong toRender);
    }
}