// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RootItemUndoData.cs" company="">
//   
// </copyright>
// <summary>
//   an undo object for root neuron in a thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an undo object for root neuron in a thesaurus.
    /// </summary>
    internal class RootItemUndoData : BrainUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="RootItemUndoData"/> class.</summary>
        /// <param name="action">The action.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="item">The item.</param>
        public RootItemUndoData(BrainAction action, Neuron relationship, Neuron item)
        {
            Item = item;
            Relationship = relationship;
            Action = action;
        }

        /// <summary>
        ///     Gets or sets the item root item involved with the operation.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; set; }

        /// <summary>
        ///     Gets or sets the relationship of the root item.
        /// </summary>
        /// <value>
        ///     The relationship.
        /// </value>
        public Neuron Relationship { get; set; }

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            RootItemUndoData iUndoData;
            switch (Action)
            {
                case BrainAction.Created: // we need to remove it from the brain.
                    iUndoData = new RootItemUndoData(BrainAction.Removed, Relationship, Item);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    BrainData.Current.Thesaurus.RemoveRootItem(Relationship, Item);
                    break;
                case BrainAction.Removed:
                    iUndoData = new RootItemUndoData(BrainAction.Created, Relationship, Item);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    BrainData.Current.Thesaurus.AddRootItem(Relationship, Item);
                    break;
                default:
                    throw new System.InvalidOperationException(
                        string.Format("Unsuported BrainAction type: {0}.", Action));
            }
        }
    }
}