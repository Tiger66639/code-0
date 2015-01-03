// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnRegWithTopicsDictionaryUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   The un reg with topics dictionary undo item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The un reg with topics dictionary undo item.</summary>
    internal class UnRegWithTopicsDictionaryUndoItem : RegWithTopicsDictionaryUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="UnRegWithTopicsDictionaryUndoItem"/> class.</summary>
        /// <param name="item">The item.</param>
        public UnRegWithTopicsDictionaryUndoItem(Neuron item)
            : base(item)
        {
        }

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new RegWithTopicsDictionaryUndoItem(fItem);
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            Parsers.TopicsDictionary.Remove(BrainData.Current.NeuronInfo[fItem].DisplayTitle, fItem);
        }
    }
}