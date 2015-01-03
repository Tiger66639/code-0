// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetDouble.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that can be implemented by objects that
//   are able to return a <see langword="double" /> value. This is used for
//   faster calculations (in the bool-expression) without having to create
//   temp ints all the time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that can be implemented by objects that
    ///     are able to return a <see langword="double" /> value. This is used for
    ///     faster calculations (in the bool-expression) without having to create
    ///     temp ints all the time.
    /// </summary>
    internal interface IGetDouble
    {
        /// <summary>Gets the <see langword="double"/> value.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="double"/>.</returns>
        double GetDouble(Processor processor);

        /// <summary>returns if this object can return an double.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanGetDouble();
    }
}