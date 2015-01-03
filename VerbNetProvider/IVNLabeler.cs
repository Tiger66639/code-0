// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVNLabeler.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> used by the <see cref="VerbNet" /> class
//   to have a callback <see langword="interface" /> for providing labels to
//   the imported data and previously rendered data, like roles that have
//   already been created in the project.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VerbNetProvider
{
    /// <summary>
    ///     An <see langword="interface" /> used by the <see cref="VerbNet" /> class
    ///     to have a callback <see langword="interface" /> for providing labels to
    ///     the imported data and previously rendered data, like roles that have
    ///     already been created in the project.
    /// </summary>
    public interface IVNLabeler
    {
        /// <summary>Assigns the title to the specified neuron.</summary>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        void SetTitle(JaStDev.HAB.Neuron item, string value);

        /// <summary>Assigns the <paramref name="title"/> and<paramref name="description"/> to the specified neuron.</summary>
        /// <param name="item">The item.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        void SetInfo(JaStDev.HAB.Neuron item, string title, string description);

        /// <summary>Stores a new role as a root in the thesaurus, so that it can be used
        ///     and accessed in the designer.</summary>
        /// <param name="value">The value.</param>
        void StoreRoleRoot(ulong value);

        /// <summary>Indicates that the <paramref name="item"/> should get a title that
        ///     indicates the object references the second argument. This is used to
        ///     provide a label for Frame elements.</summary>
        /// <param name="item">The item.</param>
        /// <param name="basedOn">The based on.</param>
        void SetRefToTitle(JaStDev.HAB.Neuron item, JaStDev.HAB.Neuron basedOn);
    }
}