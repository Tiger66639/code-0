// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestriction.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that contains restrictions for <see cref="FrameElement" />
//   s, called <see cref="FrameElementRestrictions" /> , which help determin
//   some limitations on the frame elements by checking the content of the
//   frame element with the filter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection that contains restrictions for <see cref="FrameElement" />
    ///     s, called <see cref="FrameElementRestrictions" /> , which help determin
    ///     some limitations on the frame elements by checking the content of the
    ///     frame element with the filter.
    /// </summary>
    public class FERestriction : FERestrictionBase
    {
        #region Fields

        /// <summary>The f event monitor.</summary>
        private FERestrictionEventMonitor fEventMonitor;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="FERestriction"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestriction(Neuron toWrap)
            : base(toWrap)
        {
            System.Diagnostics.Debug.Assert(toWrap is NeuronCluster);
            fEventMonitor = new FERestrictionEventMonitor(this);

                // this only registers for link changes, the segment changes (which are also children of the restriction)
            Segments = new FERestrictionSegmentCollection(this, (NeuronCluster)toWrap);
            Segments.CollectionChanged += fSegments_CollectionChanged;
        }

        #region common

        #region InclusionModifier

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element
        ///     has.
        /// </summary>
        public Neuron InclusionModifier
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictionModifier);
            }

            set
            {
                var iCur = InclusionModifier;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(
                        Item, 
                        (ulong)PredefinedNeurons.VerbNetRestrictionModifier, 
                        value); // this generats more correctly the undo data.
                }
            }
        }

        #endregion

        #endregion

        #region Normal restriction

        #region Segments

        /// <summary>
        ///     Gets the list of segments
        /// </summary>
        public FERestrictionSegmentCollection Segments { get; private set; }

        #endregion

        /// <summary>Handles the CollectionChanged event of the fSegments control.</summary>
        /// <remarks>When the segment collection changes, we need to ask the last segments
        ///     to update wether they need a restriction value or only a direction
        ///     (only the last one always needs a restriction + recursive ones).</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void fSegments_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex >= Segments.Count - 2)
                    {
                        if (Segments.Count > 1)
                        {
                            Segments[Segments.Count - 2].UpdateRestrictionRequirement();
                        }

                        Segments[Segments.Count - 1].UpdateRestrictionRequirement();

                            // this should always work cause we did an add.
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    if (e.NewStartingIndex >= Segments.Count - 2 || e.OldStartingIndex >= Segments.Count - 2)
                    {
                        Segments[Segments.Count - 2].UpdateRestrictionRequirement();
                        Segments[Segments.Count - 1].UpdateRestrictionRequirement();
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex == Segments.Count && Segments.Count > 0)
                    {
                        Segments[Segments.Count - 1].UpdateRestrictionRequirement();
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex == Segments.Count - 1)
                    {
                        Segments[Segments.Count - 1].UpdateRestrictionRequirement();
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region semantic restriction

        #region ValueToInspect

        /// <summary>
        ///     Gets/sets the neuron that identifies which part of the semantic value
        ///     should be inspected. This relates to the 'variable' that should be
        ///     inspected, or better yet, the outgoing link on the semantic neuron.
        /// </summary>
        public Neuron ValueToInspect
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.ValueToInspect);
            }

            set
            {
                var iCur = ValueToInspect;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.ValueToInspect, value);

                        // this generats more correctly the undo data.
                }
            }
        }

        #endregion

        #region SearchDirection

        /// <summary>
        ///     Gets/sets the neuron that defines the link to follow when searching
        ///     the thesaurus if an item is allowed for the filter.
        /// </summary>
        public Neuron SearchDirection
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection);
            }

            set
            {
                var iCur = SearchDirection;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(
                        Item, 
                        (ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection, 
                        value); // this generats more correctly the undo data.
                }
            }
        }

        #endregion

        #region RequiresRestriction

        /// <summary>
        ///     Gets/sets the wether a restriction neuron is used to go with the
        ///     searchDirection.
        /// </summary>
        public bool RequiresRestriction
        {
            get
            {
                var iDirection = Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection);

                    // load some default values.
                if (iDirection != null)
                {
                    var iFound = iDirection.FindFirstOut((ulong)PredefinedNeurons.IsRecursive);
                    return iFound != null && iFound.ID == (ulong)PredefinedNeurons.True;
                }

                return true;

                // return false;
            }
        }

        #endregion

        #region Restriction

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element
        ///     has.
        /// </summary>
        public Neuron Restriction
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestriction);
            }

            set
            {
                var iCur = Restriction;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.VerbNetRestriction, value);

                        // this generats more correctly the undo data.
                }
            }
        }

        #endregion

        #endregion
    }
}