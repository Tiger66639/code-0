// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionBase.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for all types of restrictions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class for all types of restrictions.
    /// </summary>
    public class FERestrictionBase : FrameItemBase, 
                                     WPF.Controls.ITreeViewPanelItem, 
                                     Data.INotifyCascadedPropertyChanged, 
                                     Data.ICascadedNotifyCollectionChanged, 
                                     Data.IOnCascadedChanged
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FERestrictionBase"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FERestrictionBase(Neuron toWrap)
            : base(toWrap)
        {
        }

        #endregion

        #region fields

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded = true; // always initialy show expanded.

        /// <summary>The f needs bring into view.</summary>
        private bool fNeedsBringIntoView;

        #endregion

        #region Events  (NotifyCascadedPropertyChanged Members + ICascadedNotifyCollectionChanged Members)

        /// <summary>
        ///     Occurs when a property was changed in one of the thesaurus items. This is used for the tree display (only root
        ///     objects get events).
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when a collection was changed in one of the child items or the root list. This is used for the tree display
        ///     (only root objects get events).
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region prop

        #region Root

        /// <summary>
        ///     Gets the root frame element that owns the restriction.
        /// </summary>
        /// <value>The root.</value>
        public FrameElement Root
        {
            get
            {
                var iOwner = Owner as Data.IOwnedObject;
                while (iOwner != null)
                {
                    var iEl = iOwner as FrameElement;
                    if (iEl != null)
                    {
                        return iEl;
                    }

                    iOwner = iOwner.Owner as Data.IOwnedObject;
                }

                return null;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the current item is selected or not. This property allows us to select the item in the  UI
        ///     from code behind.
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
                    if (value)
                    {
                        var iRoot = Root;
                        if (iRoot != null)
                        {
                            iRoot.SetSelectedRestriction(this);
                        }
                    }

                    SetSelected(value);
                }
            }
        }

        /// <summary>Sets the selected value.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region DefinesFullContent

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element has.
        /// </summary>
        public bool DefinesFullContent
        {
            get
            {
                var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.RestrictionDefinesFullContent);
                return iFound != null && iFound.ID == (ulong)PredefinedNeurons.True;
            }

            set
            {
                var iCur = Item.FindFirstOut((ulong)PredefinedNeurons.RestrictionDefinesFullContent);
                if ((iCur != null && iCur.ID == (ulong)PredefinedNeurons.True) != value)
                {
                    OnPropertyChanging("DefinesFullContent", iCur, value);
                    if (value)
                    {
                        Item.SetFirstOutgoingLinkTo(
                            (ulong)PredefinedNeurons.RestrictionDefinesFullContent, 
                            Brain.Current.TrueNeuron);
                    }
                    else
                    {
                        Item.SetFirstOutgoingLinkTo(
                            (ulong)PredefinedNeurons.RestrictionDefinesFullContent, 
                            Brain.Current.FalseNeuron);
                    }
                }
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (value != fIsExpanded)
                {
                    fIsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                    var iArgs = new Data.CascadedPropertyChangedEventArgs(
                        this, 
                        new System.ComponentModel.PropertyChangedEventArgs("IsExpanded"));
                    Data.EventEngine.OnPropertyChanged(this, iArgs);
                }
            }
        }

        #endregion

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not. When the
        ///     list of children changes (becomes empty or gets the first item), this should
        ///     be raised when appropriate through a propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasChildren
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region TreeItems (for ITreeViewPanelItem)

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>The tree items.</value>
        public virtual System.Collections.IList TreeItems
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region NeedsBringIntoView

        /// <summary>
        ///     Gets or sets a value indicating whether this object needs to be brought into view.
        /// </summary>
        /// <value><c>true</c> if [needs bring into view]; otherwise, <c>false</c>.</value>
        /// <remarks>
        ///     This needs to be a simple field set/get with propertyChanged call. The treeViewPanel uses this
        ///     to respond to it. It will also toglle it back off again when the operation is done.
        /// </remarks>
        public bool NeedsBringIntoView
        {
            get
            {
                return fNeedsBringIntoView;
            }

            set
            {
                if (fNeedsBringIntoView != value)
                {
                    fNeedsBringIntoView = value;
                    OnPropertyChanged("NeedsBringIntoView");
                    var iArgs = new Data.CascadedPropertyChangedEventArgs(
                        this, 
                        new System.ComponentModel.PropertyChangedEventArgs("NeedsBringIntoView"));
                    Data.EventEngine.OnPropertyChanged(this, iArgs);
                }
            }
        }

        #endregion

        #region ParentTreeItem

        /// <summary>
        ///     Gets the parent tree item.
        /// </summary>
        /// <value>The parent tree item.</value>
        public WPF.Controls.ITreeViewPanelItem ParentTreeItem
        {
            get
            {
                return Owner as WPF.Controls.ITreeViewPanelItem;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Copies this object to the clipboard.</summary>
        /// <returns>The <see cref="DataObject"/>.</returns>
        public System.Windows.DataObject CopyToClipboard()
        {
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.NeuronIDFormat, Item.ID, false);
            return iData;
        }

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion

        #endregion
    }
}