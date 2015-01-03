// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuralTimer.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for the <see cref="TimerNeuron" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for the <see cref="TimerNeuron" /> .
    /// </summary>
    /// <remarks>
    ///     Also makes certain that the <see cref="TimerNeuron" /> has a correct
    ///     factory assigned for creating processors.
    /// </remarks>
    public class NeuralTimer : Data.ObservableObject, INeuronWrapper, INeuronInfo
    {
        #region Fields

        /// <summary>The f item.</summary>
        private TimerNeuron fItem;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuralTimer"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        public NeuralTimer(TimerNeuron toWrap)
        {
            if (toWrap == null)
            {
                throw new System.ArgumentNullException();
            }

            Item = toWrap;
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item.ID];
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        Neuron INeuronWrapper.Item
        {
            get
            {
                return Item;
            }
        }

        #endregion

        /// <summary>Called by the event Monitor of the <see cref="TimerCollection"/> (<see cref="TimerCollectionEventMonitor"/> ) when the property of the
        ///     timer sin was changed.</summary>
        /// <param name="prop">The prop.</param>
        internal void DoPropertyChagned(string prop)
        {
            OnPropertyChanged(prop);
        }

        #region Prop

        #region Item

        /// <summary>
        ///     Gets/sets the <see cref="Neuron" /> that is wrapped.
        /// </summary>
        public TimerNeuron Item
        {
            get
            {
                return fItem;
            }

            set
            {
                if (fItem != value)
                {
                    fItem = value;
                    if (value != null)
                    {
                        IsActive = value.IsActive;
                        Interval = value.Interval;
                    }
                    else
                    {
                        IsActive = false;
                        Interval = double.NaN;
                    }

                    OnPropertyChanged("Item");
                }
            }
        }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets the if the timer is active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return fItem.IsActive;
            }

            set
            {
                if (value != fItem.IsActive)
                {
                    OnPropertyChanging("IsActive", fItem.IsActive, value);
                    if (Item != null)
                    {
                        Item.IsActive = value;
                    }

                    // OnProperty is called by event handler of brain.
                }
            }
        }

        #endregion

        #region Interval

        /// <summary>
        ///     Gets/sets the timing interval used by the timer between 2 ticks,
        ///     expressed in milliseconds.
        /// </summary>
        public double Interval
        {
            get
            {
                return fItem.Interval;
            }

            set
            {
                if (fItem.Interval != value)
                {
                    OnPropertyChanging("Interval", fItem.Interval, value);
                    if (Item != null)
                    {
                        Item.Interval = value;
                    }

                    // OnProperty is called by event handler of brain.
                }
            }
        }

        #endregion

        #endregion
    }
}