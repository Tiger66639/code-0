// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRootEventManager.cs" company="">
//   
// </copyright>
// <summary>
//   The new root event manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The new root event manager.</summary>
    public class NewRootEventManager : System.Windows.WeakEventManager
    {
        /// <summary>
        ///     Gets the current manager.
        /// </summary>
        /// <value>
        ///     The current manager.
        /// </value>
        private static NewRootEventManager CurrentManager
        {
            get
            {
                var iType = typeof(NewRootEventManager);
                var currentManager = (NewRootEventManager)GetCurrentManager(iType);
                if (currentManager == null)
                {
                    currentManager = new NewRootEventManager();
                    SetCurrentManager(iType, currentManager);
                }

                return currentManager;
            }
        }

        /// <summary>Adds the listener.</summary>
        /// <param name="sin">The <see cref="WordNetSin"/> to connect to.</param>
        /// <param name="listener">The listener.</param>
        public static void AddListener(WordNetSin sin, System.Windows.IWeakEventListener listener)
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                CurrentManager.ProtectedAddListener(sin, listener);
            }
        }

        /// <summary>Removes the listener.</summary>
        /// <param name="sin">The <see cref="WordNetSin"/> to connect to.</param>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener(WordNetSin sin, System.Windows.IWeakEventListener listener)
        {
            if (System.Environment.HasShutdownStarted == false)
            {
                CurrentManager.ProtectedRemoveListener(sin, listener);
            }
        }

        /// <summary>When overridden in a derived class, starts listening for the event
        ///     being managed. After <see cref="System.Windows.WeakEventManager.StartListening"/> is
        ///     first called, the manager should be in the state of calling<see cref="System.Windows.WeakEventManager.DeliverEvent"/> or<see cref="System.Windows.WeakEventManager.DeliverEventToList"/> whenever the
        ///     relevant event from the provided <paramref name="source"/> is handled.</summary>
        /// <param name="source">The source to begin listening on.</param>
        protected override void StartListening(object source)
        {
            var iSource = (WordNetSin)source;
            iSource.NewRoot += WordNet_NewRoot;
        }

        /// <summary>Handles the NewRoot event of the WordNet control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NewRootEventArgs"/> instance containing the event
        ///     data.</param>
        private void WordNet_NewRoot(object sender, NewRootEventArgs e)
        {
            DeliverEvent(sender, e);
        }

        /// <summary>When overridden in a derived class, stops listening on the provided<paramref name="source"/> for the event being managed.</summary>
        /// <param name="source">The source to stop listening on.</param>
        protected override void StopListening(object source)
        {
            var iSource = (WordNetSin)source;
            iSource.NewRoot -= WordNet_NewRoot;
        }
    }
}