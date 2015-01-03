// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Timers.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data for the <see cref="Timers" /> view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the data for the <see cref="Timers" /> view.
    /// </summary>
    public class Timers : Data.ObservableObject, System.Windows.IWeakEventListener
    {
        /// <summary>The f selected item.</summary>
        private NeuralTimer fSelectedItem;

        /// <summary>The f timer list.</summary>
        private readonly TimerCollection fTimerList = new TimerCollection();

        /// <summary>Initializes a new instance of the <see cref="Timers"/> class.</summary>
        public Timers()
        {
            fTimerList.CollectionChanged += fTimerList_CollectionChanged;
            ClearedEventManager.AddListener(Brain.Current, this);

                // we check for clear instructions, cause we need to reregister the timers collection again when that happens.
        }

        #region TimerList

        /// <summary>
        ///     Gets the list of all the timers in the brain.
        /// </summary>
        public TimerCollection TimerList
        {
            get
            {
                return fTimerList;
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets/sets the currently selected timervalue.
        /// </summary>
        public NeuralTimer SelectedItem
        {
            get
            {
                return fSelectedItem;
            }

            set
            {
                if (value != fSelectedItem)
                {
                    fSelectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        #endregion

        /// <summary>When the selected item is removed, we need to reset it to null.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void fTimerList_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems[0] == fSelectedItem)
                    {
                        SelectedItem = null;
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(ClearedEventManager))
            {
                Brain_Cleared(sender, e);
                return true;
            }

            return false;
        }

        /// <summary>Handles the Cleared event of the <see cref="Brain"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Brain_Cleared(object sender, System.EventArgs e)
        {
            fTimerList.Reset(); // need to make certain that the list is cleared again.
            fTimerList.Reregister();
        }

        #endregion
    }
}