// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetChannelEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides event monitoring for all the loaded <see cref="WordNetItem" />
//   objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides event monitoring for all the loaded <see cref="WordNetItem" />
    ///     objects.
    /// </summary>
    internal class WordNetChannelEventMonitor : EventMonitor
    {
        /// <summary>The f ids.</summary>
        private readonly System.Collections.Generic.List<ulong> fIds = new System.Collections.Generic.List<ulong>();

                                                                // stores all the registered id's, for the wordnetchannel, so that we can clear them.

        /// <summary>Initializes a new instance of the <see cref="WordNetChannelEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public WordNetChannelEventMonitor(WordNetChannel toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public WordNetChannel Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (WordNetChannel)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a neuron is changed. Is only called when one of the loaded<see cref="WordNetItem"/> s is changed.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action == BrainAction.Removed)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<ulong>(Item.OnNeuronRemoved), 
                    e.OriginalSourceID);
            }

            // don't need to do anything with a change cause these objects only store the id, all the rest is internal data of wordnet.
        }

        /// <summary>Adds an <paramref name="id"/> to monitor.</summary>
        /// <param name="id">The id.</param>
        internal void AddId(ulong id)
        {
            EventManager.Current.AddNeuronChangedMonitor(this, id);
            fIds.Add(id);
        }

        /// <summary>
        ///     Removes all the registerd id's for this monitor.
        /// </summary>
        internal void Clear()
        {
            foreach (var i in fIds)
            {
                EventManager.Current.RemoveNeuronChangedMonitor(this, i);
            }
        }
    }
}