// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearedEventManager.cs" company="">
//   
// </copyright>
// <summary>
//   The event manager for the weak event pattern used for the
//   <see cref="JaStDev.HAB.Brain.Cleared" /> event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The event manager for the weak event pattern used for the
    ///     <see cref="JaStDev.HAB.Brain.Cleared" /> event.
    /// </summary>
    /// <remarks>
    ///     use this class to prevent mem leaks.
    /// </remarks>
    public class ClearedEventManager : System.Windows.WeakEventManager
    {
        /// <summary>
        ///     Gets the current manager.
        /// </summary>
        /// <value>
        ///     The current manager.
        /// </value>
        private static ClearedEventManager CurrentManager
        {
            get
            {
                var iType = typeof(ClearedEventManager);
                var currentManager = (ClearedEventManager)GetCurrentManager(iType);
                if (currentManager == null)
                {
                    currentManager = new ClearedEventManager();
                    SetCurrentManager(iType, currentManager);
                }

                return currentManager;
            }
        }

        /// <summary>Adds the listener.</summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void AddListener(Brain source, System.Windows.IWeakEventListener listener)
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                CurrentManager.ProtectedAddListener(source, listener);
            }
        }

        /// <summary>Removes the listener.</summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener(Brain source, System.Windows.IWeakEventListener listener)
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
            var iSource = (Brain)source;
            iSource.Cleared += OnCleared;
        }

        /// <summary>Handles the Cleared event of the iSource control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCleared(object sender, System.EventArgs e)
        {
            DeliverEvent(sender, e);
        }

        /// <summary>When overridden in a derived class, stops listening on the provided<paramref name="source"/> for the event being managed.</summary>
        /// <param name="source">The source to stop listening on.</param>
        protected override void StopListening(object source)
        {
            var iSource = (Brain)source;
            iSource.Cleared -= OnCleared;
        }
    }
}