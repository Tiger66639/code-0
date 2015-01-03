// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInternalChange.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> used by
//   <see cref="JaStDev.HAB.Designer.ClusterCollection`1" /> to let the owner
//   of the collection know that an <see langword="internal" /> change is about
//   the happen because the collection is going to be instantiated (first item
//   added), in which case the owner doesn't need to create a new cluster
//   object. This is important for the pattern import, cause otherwise some
//   things can happen out of sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an <see langword="interface" /> used by
    ///     <see cref="JaStDev.HAB.Designer.ClusterCollection`1" /> to let the owner
    ///     of the collection know that an <see langword="internal" /> change is about
    ///     the happen because the collection is going to be instantiated (first item
    ///     added), in which case the owner doesn't need to create a new cluster
    ///     object. This is important for the pattern import, cause otherwise some
    ///     things can happen out of sync.
    /// </summary>
    public interface IInternalChange
    {
        /// <summary>
        ///     a <see langword="switch" /> that determins if the class is doing an
        ///     <see langword="internal" /> change or not. This is used by the event
        ///     system to make certain that some things don't get updated 2 times.
        /// </summary>
        bool InternalChange { get; set; }
    }
}