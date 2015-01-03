// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDisplayPathBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by objects that
//   are able to return a 'displayPath' for themselves.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by objects that
    ///     are able to return a 'displayPath' for themselves.
    /// </summary>
    public interface IDisplayPathBuilder
    {
        /// <summary>Gets the display path that points to the current object.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        DisplayPath GetDisplayPathFromThis();
    }
}