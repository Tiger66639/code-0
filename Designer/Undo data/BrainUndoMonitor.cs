// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainUndoMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   This class is responsible for keeping track of changes to the brain and
//   syncing this with the undo data system. This is a singleton class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    // To Finish: (this needs simplification, simply put the event handler in window main.
    /// <summary>
    ///     This class is responsible for keeping track of changes to the brain and
    ///     syncing this with the undo data system. This is a singleton class.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When the <see cref="Brain" /> has <see cref="Processor" /> s running, it
    ///         produces a lot of events with all the changes to the brain. In response
    ///         to those changes, some <see cref="MindMap" /> s,
    ///         <see cref="CodeEditor" /> s or others can produce undo data because, for
    ///         instance a neuron was removed that they also depicted. These changes are
    ///         normally also recorded by the undo system cause they go the normal
    ///         channels. However, since the actual brain changes are not recorded since
    ///         they aren't triggered by the user, so this undo data becomes invalid.
    ///     </para>
    ///     <para>
    ///         At the same time it is also intresting to be able to undo everything a
    ///         processor (user input sent to the brain) changed. This can be done since
    ///         all the changes to the brainare sent as events.
    ///     </para>
    ///     <para>
    ///         So that's what this class does: it allows undoing of a 'thought' and it
    ///         makes certain that user actions are properly recorded, even if a
    ///         processor is running (and also producing undo data through this class)
    ///     </para>
    /// </remarks>
    internal class BrainUndoMonitor
    {
        /// <summary>The f current.</summary>
        private static BrainUndoMonitor fCurrent;

        /// <summary>The f undo data.</summary>
        private System.Collections.Generic.Dictionary<Processor, System.Collections.Generic.List<UndoSystem.UndoItem>>
            fUndoData =
                new System.Collections.Generic.Dictionary
                    <Processor, System.Collections.Generic.List<UndoSystem.UndoItem>>();

            // stores all the undo data generated per processor.

        /// <summary>
        ///     Gets the current instance.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public static BrainUndoMonitor Current
        {
            get
            {
                if (fCurrent == null)
                {
                    throw new System.InvalidOperationException(
                        "BrainUndoMonitor is a singleton class that needs to be instantiated before it can be accessed.");
                }

                return fCurrent;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is instantiated or not.
        /// </summary>
        /// <remarks>
        ///     This is usefull to determin since you can't access
        ///     <see cref="Current" /> untill it is valid.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if this instance is instantiated; otherwise, <c>false</c>
        ///     .
        /// </value>
        public static bool IsInstantiated
        {
            get
            {
                return fCurrent != null;
            }
        }

        #region ctor

        /// <summary>
        ///     Creates and initializes the singelton.
        /// </summary>
        /// <remarks>
        ///     This function must be called before the singleton can be used. This is
        ///     to force a specific point in time for it's creation, which is
        ///     important to make certain that the events are registered as soon as
        ///     possible.
        /// </remarks>
        public static void Instantiate()
        {
            fCurrent = new BrainUndoMonitor();
        }

        /// <summary>Prevents a default instance of the <see cref="BrainUndoMonitor"/> class from being created. 
        ///     Initializes a new instance of the <see cref="BrainUndoMonitor"/>
        ///     class.</summary>
        private BrainUndoMonitor()
        {
            WindowMain.UndoStore.PreviewUndoStoreChanged += UndoStore_PreviewUndoStoreChanged;
        }

        /// <summary>Handles the PreviewUndoStoreChanged event of the<see cref="UndoSystem.UndoStore"/> control.</summary>
        /// <remarks>We cancel the operation if the undo item is generated from a thread
        ///     that has a processor assigned (from the execution engine). We do this
        ///     cause they might cause some problems otherwise. undoing a run will
        ///     probably be done by recording these items some other way?</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UndoSystem.UndoStoreEventArgs"/> instance containing the event
        ///     data.</param>
        private void UndoStore_PreviewUndoStoreChanged(object sender, UndoSystem.UndoStoreEventArgs e)
        {
            e.IsCanceled = Processor.CurrentProcessor != null;
        }

        #endregion
    }
}