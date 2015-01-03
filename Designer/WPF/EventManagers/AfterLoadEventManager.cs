// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AfterLoadEventManager.cs" company="">
//   
// </copyright>
// <summary>
//   The event manager for the weak event pattern used for the
//   <see cref="JaStDev.HAB.Designer.BrainData.AfterLoad" /> event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The event manager for the weak event pattern used for the
    ///     <see cref="JaStDev.HAB.Designer.BrainData.AfterLoad" /> event.
    /// </summary>
    /// <remarks>
    ///     use this class to prevent mem leaks.
    /// </remarks>
    public class AfterLoadEventManager : System.Windows.WeakEventManager
    {
        /// <summary>
        ///     Gets the current manager.
        /// </summary>
        /// <value>
        ///     The current manager.
        /// </value>
        private static AfterLoadEventManager CurrentManager
        {
            get
            {
                var iType = typeof(AfterLoadEventManager);
                var currentManager = (AfterLoadEventManager)GetCurrentManager(iType);
                if (currentManager == null)
                {
                    currentManager = new AfterLoadEventManager();
                    SetCurrentManager(iType, currentManager);
                }

                return currentManager;
            }
        }

        /// <summary>Adds the listener.</summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void AddListener(BrainData source, System.Windows.IWeakEventListener listener)
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                CurrentManager.ProtectedAddListener(source, listener);
            }
        }

        /// <summary>Removes the listener.</summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener(BrainData source, System.Windows.IWeakEventListener listener)
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                CurrentManager.ProtectedRemoveListener(source, listener);
            }
        }

        /// <summary>When overridden in a derived class, starts listening for the event
        ///     being managed. After <see cref="System.Windows.WeakEventManager.StartListening"/> is
        ///     first called, the manager should be in the state of calling<see cref="System.Windows.WeakEventManager.DeliverEvent"/> or<see cref="System.Windows.WeakEventManager.DeliverEventToList"/> whenever the
        ///     relevant event from the provided <paramref name="source"/> is handled.</summary>
        /// <param name="source">The source to begin listening on.</param>
        protected override void StartListening(object source)
        {
            var iSource = (BrainData)source;
            iSource.AfterLoad += Do_AfterLoad;
        }

        /// <summary>Handles the AfterLoad event of the Do control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Do_AfterLoad(object sender, System.EventArgs e)
        {
            DeliverEvent(sender, e);
        }

        /// <summary>When overridden in a derived class, stops listening on the provided<paramref name="source"/> for the event being managed.</summary>
        /// <param name="source">The source to stop listening on.</param>
        protected override void StopListening(object source)
        {
            var iSource = (BrainData)source;
            iSource.AfterLoad -= Do_AfterLoad;
        }
    }
}