// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronInfoEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The event monitor for the <see cref="NeuronDataDictionary" /> . This clas
//   allows for a weak reference from within the event system to this dict,
//   which is project local.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The event monitor for the <see cref="NeuronDataDictionary" /> . This clas
    ///     allows for a weak reference from within the event system to this dict,
    ///     which is project local.
    /// </summary>
    internal class NeuronInfoEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronInfoEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronInfoEventMonitor(NeuronDataDictionary toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>Gets the item.</summary>
        public NeuronDataDictionary Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronDataDictionary)Reference.Target;
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
            NeuronData iRes = null;
            if (iItem != null)
            {
                if (e.Action == BrainAction.Removed)
                {
                    iItem.DeleteItem(e.OriginalSourceID);
                }
                else if (e.Action == BrainAction.Changed)
                {
                    iRes = iItem.GetCached(e.OriginalSourceID);
                    var iPropArgs = e as NeuronPropChangedEventArgs;
                    if (iPropArgs != null)
                    {
                        if (iRes != null)
                        {
                            iRes.NeuronPropChanged(iPropArgs.Property);

                                // propety changed can be triggered from another thread
                        }
                    }
                    else if (iRes != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Neuron>(iRes.NeuronChanged), 
                            e.NewValue);
                    }
                }
            }
        }
    }
}