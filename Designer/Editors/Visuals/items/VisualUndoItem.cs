// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   Undo data for Visuals.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Undo data for Visuals.
    /// </summary>
    public class VisualUndoItem : UndoSystem.UndoItem
    {
        /// <summary>
        ///     Gets or sets then that was wrapped
        /// </summary>
        /// <value>
        ///     To wrap.
        /// </value>
        public NeuronCluster ToWrap { get; set; }

        /// <summary>Gets or sets the visual.</summary>
        public VisualFrame Visual { get; set; }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            Visual.LoadNeuron(ToWrap);
        }

        /// <summary>Checks if this <see cref="UndoSystem.UndoItem"/> has the same target(s) as the
        ///     undo item to compare.</summary>
        /// <remarks>This is used by the undo system to check for duplicate entries which
        ///     don't need to be stored if they are within the<see cref="JaStDev.UndoSystem.UndoStore.MaxSecBetweenUndo"/> limit.</remarks>
        /// <param name="toCompare">An undo item to compare this undo item with.</param>
        /// <returns>True, if they have the same target, otherwise false.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            return false;

                // can't group changes to a frame together like this (like a texteditor that has multiple keys typed fast afte each other).
        }

        /// <summary>Checks if this undo item contains data from the specified<paramref name="source"/> object.</summary>
        /// <remarks>This function is used by the undo system to clean up undo data from
        ///     items that are no longer available. This can happen for undo data
        ///     generated from user <see langword="interface"/> controls which are no
        ///     longer valid (loaded).</remarks>
        /// <param name="source">The object to check for.</param>
        /// <returns>True if this object contains undo data for the specified object.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false; // we aren't ui undo, so can never happen.
        }
    }
}