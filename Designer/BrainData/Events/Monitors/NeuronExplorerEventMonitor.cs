// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronExplorerEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The neuron explorer event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The neuron explorer event monitor.</summary>
    internal class NeuronExplorerEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronExplorerEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronExplorerEventMonitor(NeuronExplorer toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>
        ///     Gets or sets the current scroll pos of the item, we keep a copy since
        ///     this is a dp prop, which is only accesssible from the ui thread
        /// </summary>
        /// <value>
        ///     The current scroll pos.
        /// </value>
        public ulong CurrentScrollPos { get; set; }

        /// <summary>
        ///     Gets or sets the max visible items, we keep a copy since this is a dp
        ///     prop, which is only accesssible from the ui thread
        /// </summary>
        /// <value>
        ///     The max visible.
        /// </value>
        public ulong MaxVisible { get; set; }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public NeuronExplorer Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronExplorer)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            EventManager.Current.RemoveAnyChangedMonitor(this);
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action != BrainAction.Changed)
            {
                Item.ItemCount = Brain.Current.NextID; // regular prop, can be done from other thread.
                if (e.Action == BrainAction.Created)
                {
                    var iNeuron = e.OriginalSource;
                    if (iNeuron.ID >= CurrentScrollPos && iNeuron.ID < (CurrentScrollPos + MaxVisible))
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Neuron>(Item.InsertNeuron), 
                            iNeuron);
                    }
                }
                else if (e.Action == BrainAction.Removed)
                {
                    var iItem = Item;
                    iItem.Selection.AdjustForDelete(e.OriginalSourceID);
                }
            }
        }
    }
}