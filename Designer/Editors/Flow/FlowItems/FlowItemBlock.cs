// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemBlock.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for flow items that contain other flow items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for flow items that contain other flow items.
    /// </summary>
    /// <typeparam name="T">The type of children allowed.</typeparam>
    public class FlowItemBlock : FlowItem
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItemBlock"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FlowItemBlock(NeuronCluster toWrap)
            : base(toWrap)
        {
            Items = new FlowItemCollection(this, toWrap);
        }

        #region Items

        /// <summary>
        ///     Gets the list of flow items defined in this flow.
        /// </summary>
        public FlowItemCollection Items
        {
            get
            {
                return fItems;
            }

            internal set
            {
                fItems = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        #region Orientation

        /// <summary>
        ///     Gets/sets the orientation of the wrappanel, which determins how the
        ///     items are displayed: horizontally-wrapped or vertically stacked.
        /// </summary>
        public System.Windows.Controls.Orientation Orientation
        {
            get
            {
                return fOrientation;
            }

            set
            {
                OnPropertyChanging("Orientation", fOrientation, value);
                fOrientation = value;
                OnPropertyChanged("Orientation");
            }
        }

        #endregion

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            Items = new FlowItemCollection(this, (NeuronCluster)value);
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iChild = (FlowItem)child;
            if (Items.Remove(iChild) == false)
            {
                base.RemoveChild(child);
            }
        }

        #region fields

        /// <summary>The f items.</summary>
        private FlowItemCollection fItems;

        /// <summary>The f orientation.</summary>
        private System.Windows.Controls.Orientation fOrientation;

        #endregion
    }
}