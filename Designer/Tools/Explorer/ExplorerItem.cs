// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerItem.cs" company="">
//   
// </copyright>
// <summary>
//   Represents all the info known for an item in the
//   <see cref="NeuronExplorer" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Represents all the info known for an item in the
    ///     <see cref="NeuronExplorer" /> .
    /// </summary>
    /// <remarks>
    ///     This object is never persisted, so it uses the object reference of the
    ///     neuron directly, without bothering about the id.
    /// </remarks>
    public class ExplorerItem : Data.ObservableObject
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerItem"/> class.</summary>
        /// <param name="id">The id.</param>
        public ExplorerItem(ulong id)
        {
            ID = id;
        }

        #region ID

        /// <summary>
        ///     Gets the index nr of this item in the neuron list. If this item
        ///     represents an invalid item, the index is also stored here.
        /// </summary>
        public ulong ID { get; internal set; }

        #endregion
    }
}