// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetInt.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that can be implemented by objects that
//   are able to return an <see langword="int" /> value. This is used for
//   faster calculations (in the bool-expression) without having to create
//   temp ints all the time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that can be implemented by objects that
    ///     are able to return an <see langword="int" /> value. This is used for
    ///     faster calculations (in the bool-expression) without having to create
    ///     temp ints all the time.
    /// </summary>
    internal interface IGetInt
    {
        /// <summary>Gets the <see langword="int"/> value.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="int"/>.</returns>
        int GetInt(Processor processor);

        /// <summary>returns if this object can return an int.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanGetInt();
    }
}