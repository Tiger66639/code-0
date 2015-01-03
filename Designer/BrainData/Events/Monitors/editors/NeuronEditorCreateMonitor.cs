// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronEditorCreateMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   When an asset is still a temp, it monitors for changes until it becomes a
//   real neuron, so that it can create a possible sub
//   <see cref="TextPatternEditor" /> and register itself in the list of
//   global assets (so that it doesn't get deleted accidentally).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     When an asset is still a temp, it monitors for changes until it becomes a
    ///     real neuron, so that it can create a possible sub
    ///     <see cref="TextPatternEditor" /> and register itself in the list of
    ///     global assets (so that it doesn't get deleted accidentally).
    /// </summary>
    internal class NeuronEditorCreateMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronEditorCreateMonitor"/> class. Initializes a new instance of the<see cref="LargeIDCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public NeuronEditorCreateMonitor(NeuronEditor toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public NeuronEditor Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronEditor)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>Called when any neuron has changed, when it's the item that got
        ///     created, let the editor know and unregister the event monitor, cause
        ///     it's work is done.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action == BrainAction.Created && e.OriginalSource == Item.Item)
            {
                Item.ItemCreated();
                EventManager.Current.RemoveAnyChangedMonitor(this);
            }
        }
    }
}