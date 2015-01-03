// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesRelItemUndoData.cs" company="">
//   
// </copyright>
// <summary>
//   an undo object for a relationship neuron in a thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an undo object for a relationship neuron in a thesaurus.
    /// </summary>
    public class ThesRelItemUndoData : BrainUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="ThesRelItemUndoData"/> class.</summary>
        /// <param name="action">The action.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="data">The data.</param>
        public ThesRelItemUndoData(BrainAction action, ulong relationship, LargeIDCollection data)
        {
            Relationship = relationship;
            Data = data;
            Action = action;
        }

        /// <summary>
        ///     Gets or sets the relationship of the root item.
        /// </summary>
        /// <value>
        ///     The relationship.
        /// </value>
        public ulong Relationship { get; set; }

        /// <summary>Gets or sets the data.</summary>
        public LargeIDCollection Data { get; set; }

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            ThesRelItemUndoData iUndoData;
            switch (Action)
            {
                case BrainAction.Created: // we need to remove it from the brain.
                    iUndoData = new ThesRelItemUndoData(BrainAction.Removed, Relationship, Data);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    BrainData.Current.Thesaurus.RemoveRelationship(Relationship);
                    break;
                case BrainAction.Removed:
                    iUndoData = new ThesRelItemUndoData(BrainAction.Created, Relationship, Data);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    var iThes = BrainData.Current.Thesaurus;
                    iThes.RecreateRelationship(Relationship, Data);
                    iThes.SelectedRelationshipIndex = iThes.Relationships.Count - 1;

                        // make it active, so the user can se the undo did something.
                    break;
                default:
                    throw new System.InvalidOperationException(
                        string.Format("Unsuported BrainAction type: {0}.", Action));
            }
        }
    }
}