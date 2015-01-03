// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowPanelItemBase.cs" company="">
//   
// </copyright>
// <summary>
//   The root class for the container elements used by the
//   <see cref="FlowPanel" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     The root class for the container elements used by the
    ///     <see cref="FlowPanel" />
    /// </summary>
    /// <remarks>
    ///     This is a dependency object so we can create an 'IsSelected' property to
    ///     which we can bind the flowItem's IsSelected. This allows us to track
    ///     which items are selected, so we can calculate the position of the
    ///     selected item, without creating any references to the flowitems
    ///     themselves.
    /// </remarks>
    public abstract class FlowPanelItemBase : System.Windows.DependencyObject
    {
        /// <summary>Initializes a new instance of the <see cref="FlowPanelItemBase"/> class. Initializes a new instance of the <see cref="FlowPanelItemBase"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public FlowPanelItemBase(FlowPanelItemBase owner, FlowPanel panel)
        {
            Owner = owner;
            Panel = panel;
        }

        #region Owner

        /// <summary>
        ///     Gets the direct owner of this object.
        /// </summary>
        public FlowPanelItemBase Owner { get; private set; }

        #endregion

        #region Panel

        /// <summary>
        ///     Gets the panel that manages this item.
        /// </summary>
        public FlowPanel Panel { get; private set; }

        #endregion

        /// <summary>
        ///     Gets or sets the size of the content of this object.
        /// </summary>
        /// <value>
        ///     The rect.
        /// </value>
        public System.Windows.Size Size { get; set; }

        /// <summary>
        ///     Gets or sets the Zindex to apply to this element. This is calculated
        ///     during the measure pass. It is used to make certain that everything is
        ///     drawn in the correct order (top items on top, like lines and stuff).
        /// </summary>
        /// <value>
        ///     The index of the Z.
        /// </value>
        public int ZIndex { get; set; }

        /// <summary>Arranges the items.</summary>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public abstract void Arrange(System.Windows.Size size, System.Windows.Point offset);

        /// <summary>Measures this instance.</summary>
        /// <param name="available">The available.</param>
        public abstract void Measure(System.Windows.Size available);

        /// <summary>The update measure.</summary>
        protected void UpdateMeasure()
        {
            Panel.InvalidateMeasure();
            Panel.CalculateScrollValues();

            // Size iPrev = Size;

            // Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));                            //we do a remeasure of this list, so we reset the size.
            // Panel.InvalidateArrange();
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal abstract void Release();

        /// <summary>Gets the last UI element of the <see cref="FlowPanel"/> item, to be
        ///     used for calculating the position when displaying the drop down.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        protected internal abstract System.Windows.FrameworkElement GetLastUI();

        /// <summary>
        ///     Moves focus to the left.
        /// </summary>
        protected internal abstract void MoveLeft();

        /// <summary>
        ///     Moves focus to the right.
        /// </summary>
        protected internal abstract void MoveRight();

        /// <summary>
        ///     Moves focus up.
        /// </summary>
        protected internal abstract void MoveUp();

        /// <summary>
        ///     Moves focus down.
        /// </summary>
        protected internal abstract void MoveDown();

        /// <summary>
        ///     Moves focuses to the ui element appropriate for this item.
        /// </summary>
        protected internal abstract void Focus();

        #region IsSelected

        /// <summary>
        ///     <see cref="IsSelected" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsSelectedProperty =
            System.Windows.DependencyProperty.Register(
                "IsSelected", 
                typeof(bool), 
                typeof(FlowPanelItemBase), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IsSelected" /> property. This dependency
        ///     property indicates wether the flowItem for which this object is a
        ///     contains, is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }

            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="IsSelected"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsSelectedChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowPanelItemBase)d).OnIsSelectedChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="IsSelected"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIsSelectedChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (Panel != null)
            {
                if ((bool)e.NewValue)
                {
                    Panel.AddSelected(this);
                }
                else
                {
                    Panel.RemoveSelected(this);
                }
            }
        }

        #endregion
    }
}