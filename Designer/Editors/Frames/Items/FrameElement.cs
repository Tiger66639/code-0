// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElement.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for frame elements contained in a <see cref="Frame" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A wrapper class for frame elements contained in a <see cref="Frame" />.
    /// </summary>
    public class FrameElement : FrameItemBase, WPF.Controls.ITreeViewRoot, Data.IOnCascadedChanged
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameElement"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FrameElement(Neuron toWrap)
            : base(toWrap)
        {
            fEventMonitor = new FrameElementEventMonitor(this);
            var iFound = toWrap.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions) as NeuronCluster;
            FERestrictionRoot iRoot;
            if (iFound != null)
            {
                iRoot = new FERestrictionRoot(iFound);
                iRoot.Items = new RestrictionsCollection(iRoot, iFound);
            }
            else
            {
                var iCluster = NeuronFactory.GetCluster();
                Brain.Current.MakeTemp(iCluster);
                iRoot = new FERestrictionRoot(iCluster);
                iRoot.Items = new RestrictionsCollection(
                    iRoot, 
                    iCluster, 
                    (ulong)PredefinedNeurons.VerbNetRestrictions, 
                    toWrap);
            }

            fRestrictionsRoot = new Data.CascadedObservedCollection<FERestrictionRoot>(this);
            fRestrictionsRoot.Add(iRoot);
        }

        #endregion

        /// <summary>The reset allow multi.</summary>
        internal void ResetAllowMulti()
        {
            fAllowMulti = null;
            OnPropertyChanged("AllowMulti");
        }

        #region fields

        /// <summary>The f event monitor.</summary>
        private FrameElementEventMonitor fEventMonitor;

        // RestrictionsCollection fRestrictions;
        /// <summary>The f restrictions root.</summary>
        private readonly Data.CascadedObservedCollection<FERestrictionRoot> fRestrictionsRoot;

        /// <summary>The f is evoker.</summary>
        private bool? fIsEvoker;

        /// <summary>The f allow multi.</summary>
        private bool? fAllowMulti;

        /// <summary>The f selected restriction.</summary>
        private FERestrictionBase fSelectedRestriction;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        #endregion

        #region Events  (ITreeViewRoot: NotifyCascadedPropertyChanged Members + ICascadedNotifyCollectionChanged Members)

        /// <summary>
        ///     Occurs when a property was changed in one of the thesaurus items. This is used for the tree display.
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when a collection was changed in one of the child items or the root list. This is used for the tree display.
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region Prop

        #region Importance

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element has.
        /// </summary>
        public Neuron Importance
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.FrameImportance);
            }

            set
            {
                EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.FrameImportance, value);
            }
        }

        #endregion

        #region VerbNetRole

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element has.
        /// </summary>
        public Neuron VerbNetRole
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRole);
            }

            set
            {
                var iCur = VerbNetRole;
                if (iCur != value)
                {
                    WindowMain.UndoStore.BeginUndoGroup();

                        // when the verbnetrole is changed, we also update the displaytitle of the frame element (sometimes), so there are multiple data elements involved with this change, which we need to group.
                    try
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.VerbNetRole, value);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region ElementResultType

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element has.
        /// </summary>
        public Neuron ElementResultType
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.FrameElementResultType);
            }

            set
            {
                WindowMain.UndoStore.BeginUndoGroup();

                    // when the verbnetrole is changed, we also update the displaytitle of the frame element (sometimes), so there are multiple data elements involved with this change, which we need to group.
                try
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.FrameElementResultType, value);
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion

        #region VerbNetRoleVisual

        /// <summary>
        ///     Gets the neuron used to indicate which importance a frame element has, as a debugNeuron.
        /// </summary>
        public DebugNeuron VerbNetRoleVisual
        {
            get
            {
                var iRole = VerbNetRole;
                if (iRole != null)
                {
                    return new DebugNeuron(VerbNetRole);
                }

                return null;
            }
        }

        #endregion

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll pos of the resctrictions editor view.
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                fHorScrollPos = value;
                OnPropertyChanged("HorScrollPos");
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scroll pos of the restrictions editor view.
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                fVerScrollPos = value;
                OnPropertyChanged("VerScrollPos");
            }
        }

        #endregion

        /// <summary>
        ///     Gets the root object that contains all the restrictions on this element.
        /// </summary>
        /// <value>The restrictions root.</value>
        public FERestrictionRoot RestrictionsRoot
        {
            get
            {
                return fRestrictionsRoot[0];
            }
        }

        #region TreeItems

        /// <summary>
        ///     Gets a list to all the children of the tree root.
        /// </summary>
        /// <value>The tree items.</value>
        public System.Collections.IList TreeItems
        {
            get
            {
                return fRestrictionsRoot;
            }
        }

        #endregion

        #region SelectedRestriction

        /// <summary>
        ///     Gets/sets the restriction that is currently selected. used for editing.
        /// </summary>
        public FERestrictionBase SelectedRestriction
        {
            get
            {
                return fSelectedRestriction;
            }

            set
            {
                if (value != fSelectedRestriction)
                {
                    SetSelectedRestriction(value);
                    if (value != null)
                    {
                        value.SetSelected(true);
                    }
                }
            }
        }

        /// <summary>Sets the selected restriction.</summary>
        /// <param name="value">The value.</param>
        internal void SetSelectedRestriction(FERestrictionBase value)
        {
            if (fSelectedRestriction != null)
            {
                fSelectedRestriction.SetSelected(false);
            }

            fSelectedRestriction = value;
            OnPropertyChanged("SelectedRestriction");
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the if this item is currently selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    SetSelected(value);
                    var iFrame = Owner as Frame;
                    if (iFrame != null)
                    {
                        iFrame.SelectedElement = value ? this : null;
                    }
                }
            }
        }

        /// <summary>The set selected.</summary>
        /// <param name="value">The value.</param>
        internal void SetSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region IsEvoker

        /// <summary>
        ///     Gets/sets wether this is an evoker or not. An evoker is stored as a link between the element and the frame
        ///     taht it evokes.
        /// </summary>
        public bool IsEvoker
        {
            get
            {
                if (fIsEvoker.HasValue == false)
                {
                    if (Item.LinksOutIdentifier != null)
                    {
                        using (var iList = Item.LinksOut)
                        {
                            var iFound =
                                (from i in iList
                                 where i.MeaningID == (ulong)PredefinedNeurons.IsFrameEvoker && i.ToID == Owner.Item.ID
                                 select i).FirstOrDefault();
                            fIsEvoker = iFound != null;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return fIsEvoker.Value;
            }

            set
            {
                Link iFound;
                if (Item.LinksOutIdentifier != null)
                {
                    using (var iList = Item.LinksOut)
                        iFound =
                            (from i in iList
                             where i.MeaningID == (ulong)PredefinedNeurons.IsFrameEvoker && i.ToID == Owner.Item.ID
                             select i).FirstOrDefault();
                }
                else
                {
                    iFound = null;
                }

                if (value != (iFound != null))
                {
                    LinkUndoItem iUndo;
                    if (value)
                    {
                        var iNew = Link.Create(Item, Owner.Item, (ulong)PredefinedNeurons.IsFrameEvoker);
                        iUndo = new LinkUndoItem(iNew, BrainAction.Created);
                    }
                    else
                    {
                        iUndo = new LinkUndoItem(iFound, BrainAction.Removed);
                        iFound.Destroy();
                    }

                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                }
            }
        }

        #endregion

        #region AllowMulti

        /// <summary>
        ///     Gets or sets a value indicating whether multiple result values are allowed for this frame element.
        ///     These will have to follow each other in the frame sequence.
        /// </summary>
        /// <value><c>true</c> if [allow multi]; otherwise, <c>false</c>.</value>
        public bool AllowMulti
        {
            get
            {
                if (fAllowMulti.HasValue == false)
                {
                    if (Item.LinksOutIdentifier != null)
                    {
                        using (var iList = Item.LinksOut)
                        {
                            var iFound =
                                (from i in iList
                                 where i.MeaningID == (ulong)PredefinedNeurons.FrameElementAllowMulti
                                 select i).FirstOrDefault();
                            fAllowMulti = iFound != null && iFound.ToID == (ulong)PredefinedNeurons.True;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return fAllowMulti.Value;
            }

            set
            {
                {
                    Link iFound;
                    if (Item.LinksOutIdentifier != null)
                    {
                        using (var iList = Item.LinksOut)
                            iFound =
                                (from i in iList
                                 where i.MeaningID == (ulong)PredefinedNeurons.FrameElementAllowMulti
                                 select i).FirstOrDefault();
                    }
                    else
                    {
                        iFound = null;
                    }

                    var iPrev = iFound != null && iFound.ToID == (ulong)PredefinedNeurons.True;
                    if (value != iPrev)
                    {
                        LinkUndoItem iUndo;
                        if (value)
                        {
                            var iNew = Link.Create(
                                Item, 
                                Brain.Current.TrueNeuron, 
                                (ulong)PredefinedNeurons.FrameElementAllowMulti);
                            iUndo = new LinkUndoItem(iNew, BrainAction.Created);
                        }
                        else
                        {
                            iUndo = new LinkUndoItem(iFound, BrainAction.Removed);
                            iFound.Destroy();
                        }

                        WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Copies the frame element to the clipboard.</summary>
        /// <returns>The <see cref="DataObject"/>.</returns>
        internal System.Windows.DataObject CopyToClipboard()
        {
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.NeuronIDFormat, Item.ID);
            return iData;
        }

        /// <summary>
        ///     Resets the IsEvoker value and raises the prop changed event. this allows us to update this value
        ///     when the backing network is changed.
        /// </summary>
        internal void ResetEvoker()
        {
            fIsEvoker = null;
            OnPropertyChanged("IsEvoker");
        }

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion
    }
}