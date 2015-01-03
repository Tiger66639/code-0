// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CPPItemBase.cs" company="">
//   
// </copyright>
// <summary>
//   the base class for all the control wrappers on a
//   <see cref="CodePagePanel" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     the base class for all the control wrappers on a
    ///     <see cref="CodePagePanel" /> .
    /// </summary>
    public abstract class CPPItemBase
    {
        /// <summary>Initializes a new instance of the <see cref="CPPItemBase"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public CPPItemBase(CPPItemBase owner, CodePagePanel panel)
        {
            Owner = owner;
            Panel = panel;
        }

        #region Owner

        /// <summary>
        ///     Gets the direct owner of this object.
        /// </summary>
        public CPPItemBase Owner { get; private set; }

        #endregion

        #region Panel

        /// <summary>
        ///     Gets the panel that manages this item.
        /// </summary>
        public CodePagePanel Panel { get; private set; }

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
        public abstract void Arrange(System.Windows.Size size, ref System.Windows.Point offset);

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
    }
}