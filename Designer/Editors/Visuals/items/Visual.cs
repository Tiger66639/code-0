// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Visual.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the VisualFrame type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    /// </summary>
    public class VisualFrame : Data.OwnedObject<VisualEditor>, INeuronWrapper, INeuronInfo
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="VisualFrame"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public VisualFrame(NeuronCluster toWrap)
        {
            fItem = toWrap;
        }

        #endregion

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        internal void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.OriginalSource == Item)
            {
                if (e.Action == BrainAction.Removed)
                {
                    OnRemoved();
                }
            }
        }

        /// <summary>The on removed.</summary>
        private void OnRemoved()
        {
            if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
            {
                // we only register the undo event when it comes from the user thread cause when it comes from a processor, the undo event for the actual neuron isn't recorded and there is no undo possible, only when it comes from the ui thread.
                var iUndo = new VisualUndoItem();

                    // we do generate some undo data for this action to allow us to restore the item correctly. Whenever the neuron gets deleted, everything of the frame gets removed, but an undo will restore the same frame object, so it needs a way to restore the link with the neuron.
                iUndo.ToWrap = Items.Cluster;
                iUndo.Visual = this;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo); // we generate undo data when the neuron gets deleted, 
            }

            IsLoaded = false;

                // we need to unregiser, cause we add custom undo data to restore the frame correctly again, which registers the frame with the eventmanager again. This is required because an undo might give a new id to the neuron.
            var iOwner = Owner;
            if (iOwner != null)
            {
                // if there is an editor we belong to, let him know we are no longer valid.
                iOwner.Visuals.Remove(this);
            }
        }

        #region fields

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        /// <summary>The f event monitor.</summary>
        private VisualEventMonitor fEventMonitor;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        #endregion

        #region Prop

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item
        {
            get
            {
                return fItem;
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[fItem];
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether this frame is currently selected in the frame
        ///     editor that it belongs to.
        /// </summary>
        /// <remarks>
        ///     Will update the <see cref="FrameEditor" /> automaically.
        /// </remarks>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (fIsSelected != value)
                {
                    SetIsSelected(value);
                    if (value)
                    {
                        Owner.SelectedVisual = this;
                    }
                    else
                    {
                        Owner.SelectedVisual = null;
                    }
                }
            }
        }

        /// <summary>Allows the frame editor to update this frame's selected<paramref name="value"/> without creating an ethernal loop cause<see cref="IsSelected"/> will also update the editor.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetIsSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items in this frame.
        /// </summary>
        public VisualItemCollection Items { get; private set; }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets/sets the fact that this frame is loaded or not for UI
        ///     representation
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return fIsLoaded;
            }

            set
            {
                if (fIsLoaded != value)
                {
                    fIsLoaded = value;
                    if (value)
                    {
                        LoadNeuron(fItem);
                    }
                    else
                    {
                        Unload();
                    }
                }
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>Loads the neuron.</summary>
        /// <param name="toWrap">To wrap.</param>
        internal void LoadNeuron(NeuronCluster toWrap)
        {
            Items = new VisualItemCollection(this, toWrap);
            fEventMonitor = EventManager.Current.RegisterVisualF(this);
        }

        /// <summary>
        ///     Unloads the UI data associated with this frame, that includes the
        ///     elements and sequences (child data).
        /// </summary>
        private void Unload()
        {
            EventManager.Current.UnRegisterVisualF(this);
            Items = null;
        }

        #endregion
    }
}