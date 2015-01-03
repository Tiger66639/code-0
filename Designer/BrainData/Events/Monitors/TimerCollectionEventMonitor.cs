// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   An event Monitor for the <see cref="TimerCollection" /> . It makes
//   certain that the collection always contains all the timer sins that are
//   available throughout the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     An event Monitor for the <see cref="TimerCollection" /> . It makes
    ///     certain that the collection always contains all the timer sins that are
    ///     available throughout the network.
    /// </summary>
    internal class TimerCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="TimerCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public TimerCollectionEventMonitor(TimerCollection toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>Gets the item.</summary>
        public TimerCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (TimerCollection)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.OriginalSource is TimerNeuron)
            {
                switch (e.Action)
                {
                    case BrainAction.Created:
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<TimerNeuron>(AddTimer), 
                            e.OriginalSource);
                        break;
                    case BrainAction.Changed:
                        var iArgs = e as NeuronPropChangedEventArgs;
                        if (iArgs != null)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<TimerNeuron, string>(PropChanged), 
                                e.OriginalSource, 
                                iArgs.Property);
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<TimerNeuron>(RemoveTimer), 
                                e.OriginalSource);
                            if (e.NewValue is TimerNeuron)
                            {
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<TimerNeuron>(AddTimer), 
                                    e.NewValue);
                            }
                        }

                        break;
                    case BrainAction.Removed:
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<TimerNeuron>(RemoveTimer), 
                            e.OriginalSource);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>The prop changed.</summary>
        /// <param name="value">The value.</param>
        /// <param name="prop">The prop.</param>
        private void PropChanged(TimerNeuron value, string prop)
        {
            var iFound = (from i in Item where i.Item == value select i).FirstOrDefault();
            if (iFound != null)
            {
                iFound.DoPropertyChagned(prop);
            }
        }

        /// <summary>The add timer.</summary>
        /// <param name="value">The value.</param>
        private void AddTimer(TimerNeuron value)
        {
            var iFound = (from i in Item where i.Item == value select i).FirstOrDefault();

                // we check if it isn't already stored.
            if (iFound == null)
            {
                Item.Add(new NeuralTimer(value));
            }
        }

        /// <summary>The remove timer.</summary>
        /// <param name="value">The value.</param>
        private void RemoveTimer(TimerNeuron value)
        {
            var iFound = (from i in Item where i.Item == value select i).FirstOrDefault();
            if (iFound != null)
            {
                Item.Remove(iFound);
            }
        }
    }
}