// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for custom undo items for the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class for custom undo items for the brain.
    /// </summary>
    public abstract class BrainUndoItem : UndoSystem.UndoItem
    {
        /// <summary>
        ///     Gets or sets the action that was undertaken: a delete or create.
        /// </summary>
        /// <remarks>
        ///     When undoing the action, the reverse of this should be done.
        /// </remarks>
        /// <value>
        ///     The action.
        /// </value>
        public BrainAction Action { get; set; }

        /// <summary>The stores data from.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false;

                // can never group multiple changes together that are executed faster after each other (like text editor).
        }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            return false;

                // we don't need to delete cluster undo data because ui elements are invalid, this is always valid data.
        }
    }
}