// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetBool.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that can be implemented by objects that
//   are able to return a <see langword="bool" /> value. This is used for
//   faster calculations (in the bool-expression) without having to create
//   temp ints all the time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that can be implemented by objects that
    ///     are able to return a <see langword="bool" /> value. This is used for
    ///     faster calculations (in the bool-expression) without having to create
    ///     temp ints all the time.
    /// </summary>
    internal interface IGetBool
    {
        /// <summary>gets the <see langword="bool"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool GetBool(Processor processor);

        /// <summary>returns if this object can return an bool.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanGetBool();
    }
}