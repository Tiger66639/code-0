// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegWithTopicsDictionaryUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   The reg with topics dictionary undo item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The reg with topics dictionary undo item.</summary>
    public class RegWithTopicsDictionaryUndoItem : UndoSystem.UndoItem
    {
        /// <summary>The f item.</summary>
        protected Neuron fItem;

        /// <summary>Initializes a new instance of the <see cref="RegWithTopicsDictionaryUndoItem"/> class.</summary>
        /// <param name="item">The item.</param>
        public RegWithTopicsDictionaryUndoItem(Neuron item)
        {
            fItem = item;
        }

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new UnRegWithTopicsDictionaryUndoItem(fItem);
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            Parsers.TopicsDictionary.Add(BrainData.Current.NeuronInfo[fItem].DisplayTitle, fItem);
        }

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