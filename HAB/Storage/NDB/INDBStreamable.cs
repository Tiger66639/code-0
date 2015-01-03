// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INDBStreamable.cs" company="">
//   
// </copyright>
// <summary>
//   This <see langword="interface" /> is implemented by neurons so that they
//   can be read/written to an <see cref="JaStDev.HAB.Storage.NDB" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Storage.NDB
{
    /// <summary>
    ///     This <see langword="interface" /> is implemented by neurons so that they
    ///     can be read/written to an <see cref="JaStDev.HAB.Storage.NDB" /> .
    /// </summary>
    public interface INDBStreamable
    {
        /// <summary>Reads the object from the specified reader.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        LinkResolverData Read(CompactBinaryReader reader);

        /// <summary>Writes the object to the specified writer.</summary>
        /// <param name="writer">The writer.</param>
        void Write(System.IO.BinaryWriter writer);
    }
}