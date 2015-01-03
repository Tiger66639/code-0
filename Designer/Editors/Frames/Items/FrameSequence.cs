// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequence.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for a <see cref="NeuronCluster" /> that describes a known
//   order of <see cref="FrameElement" /> s in a <see cref="Frame" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A wrapper class for a <see cref="NeuronCluster" /> that describes a known
    ///     order of <see cref="FrameElement" /> s in a <see cref="Frame" /> .
    /// </summary>
    public class FrameSequence : FrameItemBase, System.Windows.IWeakEventListener
    {
        #region fields

        /// <summary>The f not used items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<FrameElement> fNotUsedItems =
            new System.Collections.ObjectModel.ObservableCollection<FrameElement>();

        // FrameSequenceEventMonitor fEventMonitor;
        // FERestrictionBase fSelectedRestriction;
        // RestrictionsCollection fRestrictions;
        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameSequence"/> class. Initializes a new instance of the <see cref="FrameOrder"/> class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        public FrameSequence(Neuron toWrap)
            : base(toWrap)
        {
            Items = new FrameSequenceItemCollection(null, toWrap as NeuronCluster);

                // we can't own this list, cause that would cause us to also own the frame elements, which we don't, the frame does, so no owner.

            // fEventMonitor = EventManager.Current.RegisterFrameSequence(this);
        }

        #endregion

        /// <summary>The copy to clipboard.</summary>
        /// <returns>The <see cref="DataObject"/>.</returns>
        internal System.Windows.DataObject CopyToClipboard()
        {
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.FrameSequenceFormat, Item.ID, false);
            iData.SetData(Properties.Resources.FrameOriginFormat, Owner.Item.ID, false);

                // we also put the id of the frame on the clipboard so that we can check that,during a paste, the data comes from the same frame.
            return iData;
        }

        #region Prop

        #region Items

        /// <summary>
        ///     Gets the list of frame elements that are declared in this sequence.
        /// </summary>
        public FrameSequenceItemCollection Items { get; private set; }

        #endregion

        #region NotUsedItems

        /// <summary>
        ///     Gets the list of frame elements not yet used by this item.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<FrameElement> NotUsedItems
        {
            get
            {
                return fNotUsedItems;
            }
        }

        #endregion

        #region Owner

        /// <summary>
        ///     Gets or sets the owner.
        /// </summary>
        /// <remarks>
        ///     When changed, we need to monitor the list of possible 'FrameElements'.
        /// </remarks>
        /// <value>
        ///     The owner.
        /// </value>
        public override INeuronWrapper Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                if (value != base.Owner)
                {
                    var iOwner = base.Owner as Frame;
                    if (iOwner != null)
                    {
                        System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(
                            iOwner.Elements, 
                            this);
                        NotUsedItems.Clear();
                    }

                    base.Owner = value;
                    iOwner = base.Owner as Frame; // need to recalculate the owner cause it has changed.
                    if (iOwner != null)
                    {
                        foreach (var i in iOwner.Elements)
                        {
                            NotUsedItems.Add(i);
                        }

                        System.Collections.Specialized.CollectionChangedEventManager.AddListener(iOwner.Elements, this);
                    }
                }
            }
        }

        #endregion

        // #region Restrictions

        ///// <summary>
        ///// Gets the restrictions that apply to this frame element.
        ///// </summary>
        // public RestrictionsCollection Restrictions
        // {
        // get
        // {
        // if (fRestrictions == null)
        // {
        // NeuronCluster iFound = Item.FindFirstOut((ulong)PredefinedNeurons.SequenceResultRestrictions) as NeuronCluster;
        // if (iFound != null)
        // fRestrictions = new RestrictionsCollection(this, iFound);
        // else
        // fRestrictions = new RestrictionsCollection(this, (ulong)PredefinedNeurons.SequenceResultRestrictions);
        // }
        // return fRestrictions;
        // }
        // internal set
        // {
        // //we allow an internal set, so the event monitor can reset the list when the link has changed.
        // if (value != fRestrictions)
        // {
        // fRestrictions = value;
        // OnPropertyChanged("Restrictions");
        // }
        // }
        // }

        // #endregion

        // #region SelectedRestriction

        ///// <summary>
        ///// Gets/sets the restriction that is currently selected. used for editing.
        ///// </summary>
        // public FERestrictionBase SelectedRestriction
        // {
        // get
        // {
        // return fSelectedRestriction;
        // }
        // set
        // {
        // if (value != fSelectedRestriction)
        // {
        // fSelectedRestriction = value;
        // OnPropertyChanged("SelectedRestriction");
        // }
        // }
        // }

        // #endregion

        // #region LogicOperator

        ///// <summary>
        ///// Gets/sets the neuron used to indicate the logical operator that should be used in case the restrictions collection contains multiple items.
        ///// </summary>
        // public Neuron LogicOperator
        // {
        // get
        // {
        // NeuronCluster iCluster = Restrictions.Cluster;
        // if (iCluster != null)
        // return iCluster.FindFirstOut((ulong)PredefinedNeurons.VerbNetLogicValue);
        // else
        // return null;
        // }
        // set
        // {
        // Neuron iCur = LogicOperator;
        // if (iCur != value)
        // {
        // OnPropertyChanging("LogicOperator", iCur, value);
        // fRestrictions.Cluster.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.VerbNetLogicValue, value); //if there is a prev val, there is a restrictions collection.
        // }
        // }
        // }

        // #endregion
        #endregion

        #region Functions

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
            if (managerType == typeof(System.Collections.Specialized.CollectionChangedEventManager))
            {
                Elements_CollectionChanged(sender, (System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Handles the CollectionChanged event of the fItems control.</summary>
        /// <remarks>Keeps the frame elements of the owner (frame) in sync with our 2
        ///     lists.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void Elements_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (FrameElement i in e.NewItems)
                    {
                        NotUsedItems.Add(i);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (FrameElement i in e.OldItems)
                    {
                        NotUsedItems.Remove(i);
                        var iFound = from u in Items where u.Element == i.Item select u;
                        foreach (var u in iFound)
                        {
                            Items.Remove(u);
                        }
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    var iIndex = 0;
                    foreach (FrameElement i in e.OldItems)
                    {
                        var iPos = NotUsedItems.IndexOf(i);
                        if (iPos != -1)
                        {
                            NotUsedItems[iPos] = (FrameElement)e.NewItems[iIndex];
                        }

                        var iToReplace = from u in Items where u.Element == i.Item select u;
                        foreach (var u in iToReplace)
                        {
                            u.Element = ((FrameElement)e.NewItems[iIndex]).Item;
                        }

                        iIndex++;
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    NotUsedItems.Clear();
                    Items.Clear();
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}