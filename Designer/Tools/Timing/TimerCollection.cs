// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerCollection.cs" company="">
//   
// </copyright>
// <summary>
//   An <see cref="System.Collections.ObjectModel.ObservableCollection`1" />
//   that keeps it's list synchronized with the brain for all
//   <see cref="SinTimer" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



// -----------------------------------------------------------------------

// <copyright file="TimerCollection.cs">

// Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.

// </copyright> 

// <authorJan Bogaerts</author>

// <email>Jan.Bogaerts@telenet.be</email>

// <date>01/07/2011</date>

// -----------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see cref="System.Collections.ObjectModel.ObservableCollection`1" />
    ///     that keeps it's list synchronized with the brain for all
    ///     <see cref="SinTimer" /> objects.
    /// </summary>
    public class TimerCollection : System.Collections.ObjectModel.ObservableCollection<NeuralTimer>
    {
        /// <summary>The timersnamespace.</summary>
        private const string TIMERSNAMESPACE = "Timers";

        /// <summary>The f event monitor.</summary>
        private TimerCollectionEventMonitor fEventMonitor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimerCollection" /> class
        ///     and automatically fills the list with all the known timers.
        /// </summary>
        public TimerCollection()
        {
            fEventMonitor = new TimerCollectionEventMonitor(this);
            LoadTimers();
            BrainData.Current.AfterLoad += Current_AfterLoad;
        }

        /// <summary>The load timers.</summary>
        private void LoadTimers()
        {
            foreach (var i in Brain.Current.Timers)
            {
                try
                {
                    var iTimer = Brain.Current[i] as TimerNeuron;
                    Add(new NeuralTimer(iTimer));
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "TimerCollection", 
                        string.Format("Neuron {0} has been removed from the list because: {1}", i, e));
                }
            }
        }

        /// <summary>Handles the AfterLoad event of the Current control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Current_AfterLoad(object sender, System.EventArgs e)
        {
            LoadTimers();
        }

        /// <summary>
        ///     Reregisters the event monitor, should be called in case the project
        ///     got reloaded
        /// </summary>
        internal void Reregister()
        {
            fEventMonitor = new TimerCollectionEventMonitor(this);
        }

        /// <summary>
        ///     removes all the timers but doesn't consider it a delete from the list.
        ///     This should only be called when the project is reset. It simply clears
        ///     the list but doens't change the 'isLocked' property of all the items.
        /// </summary>
        public void Reset()
        {
            base.ClearItems();
        }

        #region Overrides

        /// <summary>Inserts an <paramref name="item"/> into the collection at the
        ///     specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, NeuralTimer item)
        {
            base.InsertItem(index, item);
            var iData = item.NeuronInfo;
            iData.IsLocked = true;

                // timers that are on the lsit are locked, so we don't accidently delete them during a deleteRecurrsive on another object.
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                var iData = i.NeuronInfo;
                iData.IsLocked = false;
            }

            base.ClearItems();
        }

        /// <summary>Removes the item at the specified <paramref name="index"/> of the
        ///     collection.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var iData = this[index].NeuronInfo;
            iData.IsLocked = false;
            base.RemoveItem(index);
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, NeuralTimer item)
        {
            var iData = this[index].NeuronInfo;
            iData.IsLocked = false;

            iData = item.NeuronInfo;
            iData.IsLocked = true;

                // timers that are on the lsit are locked, so we don't accidently delete them during a deleteRecurrsive on another object.
            base.SetItem(index, item);
        }

        #endregion
    }
}