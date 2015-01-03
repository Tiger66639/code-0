// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStoragePropertiesUser.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by all types
//   that plan to use properties of an <see cref="ILongTermMem" /> type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by all types
    ///     that plan to use properties of an <see cref="ILongTermMem" /> type.
    /// </summary>
    internal interface IStoragePropertiesUser
    {
        /// <summary>Rerturns the type of the specified property. This is used so we can
        ///     read all the property types from the storage at once for converting
        ///     storage type.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        System.Type GetTypeForProperty(string name);
    }
}