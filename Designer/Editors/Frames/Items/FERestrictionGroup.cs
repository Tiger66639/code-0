// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionGroup.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for a restriction group (a cluster). This contains the
//   Restrictions collection which represents the items. The collection wrapps
//   the same item as this class. This class provides undo capability for the
//   logicOperator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for a restriction group (a cluster). This contains the
    ///     Restrictions collection which represents the items. The collection wrapps
    ///     the same item as this class. This class provides undo capability for the
    ///     logicOperator.
    /// </summary>
    public class FERestrictionGroup : FERestrictionBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FERestrictionGroup"/> class. Initializes a new instance of the <see cref="FERestrictionGroup"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FERestrictionGroup(Neuron toWrap)
            : base(toWrap)
        {
            fEventMonitor = EventManager.Current.RegisterFERestrictionGroup(this);
        }

        #endregion

        #region Fields

        /// <summary>The f items.</summary>
        private RestrictionsCollection fItems;

        /// <summary>The f event monitor.</summary>
        private FERestrictionGroupEventMonitor fEventMonitor;

        #endregion

        #region Prop

        #region LogicOperator

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element
        ///     has.
        /// </summary>
        public Neuron LogicOperator
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetLogicValue);
            }

            set
            {
                var iCur = LogicOperator;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.VerbNetLogicValue, value);

                        // we use this one, which also generates undo data.
                }
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of sub restrictions in this group.
        /// </summary>
        public RestrictionsCollection Items
        {
            get
            {
                if (fItems == null)
                {
                    fItems = new RestrictionsCollection(this, (NeuronCluster)Item);
                }

                return fItems;
            }

            internal set
            {
                // we allow an internal setter so that the event monitor can reset the value of this prop when it needs to be recalculated.
                fItems = value;
                OnPropertyChanged("Items");
                OnPropertyChanged("TreeItems");
            }
        }

        #endregion

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>
        ///     The tree items.
        /// </value>
        public override System.Collections.IList TreeItems
        {
            get
            {
                return Items;
            }
        }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not.
        ///     When the list of children changes (becomes empty or gets the first
        ///     item), this should be raised when appropriate through a
        ///     propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public override bool HasChildren
        {
            get
            {
                return Items.Count > 0;
            }
        }

        #endregion

        #endregion
    }
}