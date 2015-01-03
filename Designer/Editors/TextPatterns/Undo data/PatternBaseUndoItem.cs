// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternBaseUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   base class for undo data generated in the pattern editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for undo data generated in the pattern editor.
    /// </summary>
    public abstract class PatternBaseUndoItem : UndoSystem.UndoItem
    {
        /// <summary>
        ///     Gets or sets the pattern to which the action should be applied.
        /// </summary>
        /// <value>
        ///     The pattern.
        /// </value>
        public TextNeuron Pattern { get; set; }

        /// <summary>Checks if this <see cref="UndoSystem.UndoItem"/> has the same target(s) as the
        ///     undo item to compare.</summary>
        /// <param name="toCompare">An undo item to compare this undo item with.</param>
        /// <returns>True, if they have the same target, otherwise false.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            return false;
        }

        /// <summary>Checks if this undo item contains data from the specified<paramref name="source"/> object.</summary>
        /// <param name="source">The object to check for.</param>
        /// <returns>True if this object contains undo data for the specified object.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false;
        }
    }
}