// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReservedExplorerItem.cs" company="">
//   
// </copyright>
// <summary>
//   This class repressents an id that has been reserved by the system for
//   later use, but currently has no meaning yet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This class repressents an id that has been reserved by the system for
    ///     later use, but currently has no meaning yet.
    /// </summary>
    public class ReservedExplorerItem : FreeExplorerItem
    {
        /// <summary>Initializes a new instance of the <see cref="ReservedExplorerItem"/> class.</summary>
        /// <param name="id">The id.</param>
        public ReservedExplorerItem(ulong id)
            : base(id)
        {
        }
    }
}