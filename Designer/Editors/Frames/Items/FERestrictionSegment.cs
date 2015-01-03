// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionSegment.cs" company="">
//   
// </copyright>
// <summary>
//   Defines all the data for a single segment in a frame restriction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Defines all the data for a single segment in a frame restriction.
    /// </summary>
    public class FERestrictionSegment : FrameItemBase
    {
        /// <summary>The f event monitor.</summary>
        private FERestrictionSegmentEventMonitor fEventMonitor;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FERestrictionSegment"/> class. Initializes a new instance of the <see cref="FERestrictionSegment"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FERestrictionSegment(Neuron toWrap)
            : base(toWrap)
        {
            fEventMonitor = EventManager.Current.RegisterFERestrictionSegment(this);
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
                var iOwner = (FERestriction)Owner;
                if (iOwner != null)
                {
                    var iDirection = Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection);

                        // load some default values.
                    if (iDirection != null)
                    {
                        var iFound = iDirection.FindFirstOut((ulong)PredefinedNeurons.IsRecursive);
                        return (iFound != null && iFound.ID == (ulong)PredefinedNeurons.True)
                               || Enumerable.Last(iOwner.Segments) == this;
                    }

                    return Enumerable.Last(iOwner.Segments) == this;
                }

                return false;
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

        /// <summary>
        ///     Checks if there is a restriction requirement and if this has changed
        ///     since the previous time. If there is no longer a restriction required,
        ///     this is removed.
        /// </summary>
        internal void UpdateRestrictionRequirement()
        {
            var iRestriction = Restriction;
            if (RequiresRestriction == false && iRestriction != null)
            {
                Restriction = null;
                OnPropertyChanged("RequiresRestriction");
            }
        }
    }
}