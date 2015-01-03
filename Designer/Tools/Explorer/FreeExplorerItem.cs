// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FreeExplorerItem.cs" company="">
//   
// </copyright>
// <summary>
//   identifies an id in the brain that is free, it's corresponding neuron has
//   been deleted and the id has not yet been recycled.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     identifies an id in the brain that is free, it's corresponding neuron has
    ///     been deleted and the id has not yet been recycled.
    /// </summary>
    public class FreeExplorerItem : ExplorerItem
    {
        /// <summary>Initializes a new instance of the <see cref="FreeExplorerItem"/> class. Initializes a new instance of the <see cref="FreeExplorerItem"/>
        ///     class.</summary>
        /// <param name="id"></param>
        public FreeExplorerItem(ulong id)
            : base(id)
        {
        }
    }
}