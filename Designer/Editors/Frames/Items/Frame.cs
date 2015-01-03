// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Frame.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for a <see cref="NeuronCluster" /> that represents a Frame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for a <see cref="NeuronCluster" /> that represents a Frame.
    /// </summary>
    /// <remarks>
    ///     A frame contains information about a single sentence: what are it's
    ///     possible elements, the allowed order of elements + triggers.
    /// </remarks>
    public class Frame : Data.OwnedObject<IFrameOwner>, INeuronWrapper, INeuronInfo
    {
        /// <summary>The f event monitor.</summary>
        private FrameEventMonitor fEventMonitor;

        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f selected element.</summary>
        private FrameElement fSelectedElement;

        /// <summary>The f selected sequence.</summary>
        private FrameSequence fSelectedSequence;

        /// <summary>The f sequences.</summary>
        private FrameOrderCollection fSequences;

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="Frame"/> class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        public Frame(NeuronCluster toWrap)
        {
            System.Diagnostics.Debug.Assert(toWrap != null);
            fItem = toWrap;
        }

        #endregion

        /// <summary>Loads the neuron.</summary>
        /// <param name="toWrap">To wrap.</param>
        internal void LoadNeuron(NeuronCluster toWrap)
        {
            Elements = new FrameElementCollection(this, toWrap);

            var iFound = toWrap.FindFirstOut((ulong)PredefinedNeurons.FrameSequences) as NeuronCluster;
            if (iFound != null)
            {
                fSequences = new FrameOrderCollection(this, iFound);
            }
            else
            {
                fSequences = null;
            }

            fEventMonitor = EventManager.Current.RegisterFrame(this);
        }

        /// <summary>
        ///     Unloads the UI data associated with this frame, that includes the
        ///     elements and sequences (child data).
        /// </summary>
        private void Unload()
        {
            EventManager.Current.UnRegisterFrame(this);
            Elements = null;
            fSequences = null;
        }

        #region prop

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
                        Owner.SelectedFrame = this;
                    }
                    else
                    {
                        Owner.SelectedFrame = null;
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

        #region Elements

        /// <summary>
        ///     Gets the list of frame elements declared in the frame.
        /// </summary>
        public FrameElementCollection Elements { get; private set; }

        #endregion

        #region SelectedElement

        /// <summary>
        ///     Gets/sets the currently selected element. Used for editing the
        ///     currently selected element.
        /// </summary>
        public FrameElement SelectedElement
        {
            get
            {
                return fSelectedElement;
            }

            set
            {
                if (value != fSelectedElement)
                {
                    if (fSelectedElement != null)
                    {
                        fSelectedElement.SetSelected(false);
                    }

                    fSelectedElement = value;
                    if (fSelectedElement != null && fSelectedElement.IsSelected == false)
                    {
                        // if the new item doesn't yet know it is selected, let it know now.
                        fSelectedElement.SetSelected(true);
                    }

                    OnPropertyChanged("SelectedElement");
                }
            }
        }

        #endregion

        #region Sequences

        /// <summary>
        ///     Gets the list of FrameOrder objects which represent known frame
        ///     element sequences.
        /// </summary>
        public FrameOrderCollection Sequences
        {
            get
            {
                if (fSequences == null && IsLoaded)
                {
                    fSequences = new FrameOrderCollection(this, (ulong)PredefinedNeurons.FrameSequences);
                    fEventMonitor.RegisterSequences();
                }

                return fSequences;
            }
        }

        #endregion

        #region HasSequences

        /// <summary>
        ///     Gets a value indicating whether this instance has a list of sequences
        ///     or not yet.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has sequences; otherwise, <c>false</c> .
        /// </value>
        public bool HasSequences
        {
            get
            {
                return fSequences != null;
            }
        }

        #endregion

        #region SelectedSequence

        /// <summary>
        ///     Gets/sets the sequence that is currently selected.
        /// </summary>
        /// <remarks>
        ///     This is set from the UI through binding.
        /// </remarks>
        public FrameSequence SelectedSequence
        {
            get
            {
                return fSelectedSequence;
            }

            set
            {
                fSelectedSequence = value;
                OnPropertyChanged("SelectedSequence");
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item
        {
            get
            {
                return fItem;
            }
        }

        #endregion

        #region NeuronInfo(INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[fItem];
            }
        }

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

        #region Functions

        /// <summary>The pos changed.</summary>
        internal void PosChanged()
        {
            OnPropertyChanged("POS");
        }

        /// <summary>Called when any of the monitored neurons is changed. We check what and
        ///     update accordingly.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        internal void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.OriginalSource == Item)
            {
                // this item is deleted
                if (e.Action == BrainAction.Removed)
                {
                    // note: don't change the owner's list, it does this automatically.
                    OnRemoved();
                }
                else if (e.Action == BrainAction.Changed && !(e is NeuronPropChangedEventArgs))
                {
                    Elements = new FrameElementCollection(this, (NeuronCluster)e.NewValue);
                }

                OnPropertyChanged("Elements");
            }
            else if (e.OriginalSource == Sequences.Cluster)
            {
                if (e.Action == BrainAction.Removed)
                {
                    fSequences = null;
                }
                else if (e.Action == BrainAction.Changed)
                {
                    fSequences = new FrameOrderCollection(this, (NeuronCluster)e.NewValue);
                }

                OnPropertyChanged("Sequences");
            }
        }

        /// <summary>
        ///     Called when the neuron that is wrapped, got removed. Makes certain
        ///     that this frame is removed from the project + render undo data when
        ///     required (in the UI thread).
        /// </summary>
        private void OnRemoved()
        {
            object iFramesOwner = null;
            if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
            {
                // we only register the undo event when it comes from the user thread cause when it comes from a processor, the undo event for the actual neuron isn't recorded and there is no undo possible, only when it comes from the ui thread.
                var iUndo = new FrameUndoItem();

                    // we do generate some undo data for this action to allow us to restore the item correctly. Whenever the neuron gets deleted, everything of the frame gets removed, but an undo will restore the same frame object, so it needs a way to restore the link with the neuron.
                iUndo.ToWrap = Elements.Cluster;
                iUndo.Frame = this;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo); // we generate undo data when the neuron gets deleted, 
            }
            else if (Owner != null)
            {
                // an unload, triggered by some change in the network can't generate any undo data.
                iFramesOwner = Owner.Frames.Owner; // we need to restor this value later, after the operation.
                Owner.Frames.Owner = null;

                    // reset the owner of the list so that it doesn't generate events that cause undo data 
            }

            IsLoaded = false;

                // we need to unregiser, cause we add custom undo data to restore the frame correctly again, which registers the frame with the eventmanager again. This is required because an undo might give a new id to the neuron.
            if (Owner != null)
            {
                // if there is an editor we belong to, let him know we are no longer valid.
                Owner.Frames.Remove(this);
            }

            if (iFramesOwner != null)
            {
                // if we removed the owner of the frames list, restore it.
                Owner.Frames.Owner = iFramesOwner;
            }
        }

        #endregion
    }
}