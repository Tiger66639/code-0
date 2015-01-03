// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   provides monitoring capabilities for the <see cref="Thesaurus" /> root
//   items, and other root dictionaries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides monitoring capabilities for the <see cref="Thesaurus" /> root
    ///     items, and other root dictionaries.
    /// </summary>
    internal class ThesaurusEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="ThesaurusEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ThesaurusEventMonitor(Thesaurus toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>Gets the item.</summary>
        public Thesaurus Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (Thesaurus)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            var iItem = Item;
            var iSender = e.OriginalSource;
            if (iItem != null && e.Action == BrainAction.Removed)
            {
                if (iItem.Data.ContainsKey(e.OriginalSourceID))
                {
                    iItem.RemoveFromData(e.OriginalSourceID);

                        // don't call async, need to do as much as possible in the calling thread.
                }

                if (iItem.SelectedItem != null && iItem.SelectedItem.ID == e.OriginalSourceID)
                {
                    iItem.SelectedItem = null;
                }
            }
        }
    }
}