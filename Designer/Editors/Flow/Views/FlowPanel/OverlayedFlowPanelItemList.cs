// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayedFlowPanelItemList.cs" company="">
//   
// </copyright>
// <summary>
//   a <see cref="FlowPanelItemList" /> that provides some extra space for an
//   overlay list at the front of the background.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a <see cref="FlowPanelItemList" /> that provides some extra space for an
    ///     overlay list at the front of the background.
    /// </summary>
    public class OverlayedFlowPanelItemList : FlowPanelItemList
    {
        /// <summary>The f overlay.</summary>
        private System.Windows.FrameworkElement fOverlay;

        /// <summary>Initializes a new instance of the <see cref="OverlayedFlowPanelItemList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public OverlayedFlowPanelItemList(FlowPanelItemBase owner, FlowPanel panel)
            : base(owner, panel)
        {
        }

        #region Overlay

        /// <summary>
        ///     Gets/sets the overlay to display.
        /// </summary>
        public System.Windows.FrameworkElement Overlay
        {
            get
            {
                return fOverlay;
            }

            set
            {
                if (value != fOverlay)
                {
                    if (fOverlay != null)
                    {
                        Panel.Children.Remove(fOverlay);
                    }

                    fOverlay = value;
                    if (fOverlay != null && IsExpanded)
                    {
                        // if we are not expanded, we will add the listbackground again when expanding, we don't want that.
                        Panel.Children.Add(fOverlay);
                    }
                }
            }
        }

        #endregion

        #region Overrides

        /// <summary>Measures this instance.</summary>
        /// <param name="available">The available.</param>
        /// <remarks>The list doesn't have to calculate it's zindex, it gets assigned by
        ///     the header object, and the first has a default of 0.</remarks>
        public override void Measure(System.Windows.Size available)
        {
            SetZIndex();
            var iSize = new System.Windows.Size(0, 0);
            MeasureChildren(ref iSize, available);
            MeasureOverlay(ref iSize);
            AddMargins(ref iSize);
            MeasureBackground(ref iSize);
            Size = iSize;
        }

        /// <summary>The measure overlay.</summary>
        /// <param name="iSize">The i size.</param>
        private void MeasureOverlay(ref System.Windows.Size iSize)
        {
            if (Overlay != null)
            {
                System.Windows.Controls.Panel.SetZIndex(Overlay, ZIndex);
                System.Windows.Size iOverlaySize;
                if (Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    if (ListBackground != null && IsExpanded)
                    {
                        iOverlaySize = new System.Windows.Size(
                            double.PositiveInfinity, 
                            System.Math.Max(ListBackground.MinHeight, iSize.Height));
                    }
                    else
                    {
                        iOverlaySize = new System.Windows.Size(double.PositiveInfinity, iSize.Height);
                    }
                }
                else
                {
                    if (ListBackground != null && IsExpanded)
                    {
                        iOverlaySize = new System.Windows.Size(
                            System.Math.Max(ListBackground.MinWidth, iSize.Width), 
                            double.PositiveInfinity);
                    }
                    else
                    {
                        iOverlaySize = new System.Windows.Size(iSize.Width, double.PositiveInfinity);
                    }
                }

                Overlay.Measure(iOverlaySize);
                UpdateSize(ref iSize, Overlay.DesiredSize);
            }
        }

        /// <summary>Arranges verticallly.</summary>
        /// <param name="size">The size.</param>
        /// <param name="offset">The offset.</param>
        public override void Arrange(System.Windows.Size size, System.Windows.Point offset)
        {
            var iSubOffset = offset;
            if (Overlay != null)
            {
                if (Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    var iStart = new System.Windows.Point(
                        offset.X, 
                        offset.Y + (Size.Height - Overlay.DesiredSize.Height));
                    var iRect = new System.Windows.Rect(iStart, Overlay.DesiredSize);
                    Overlay.Arrange(iRect);
                    iSubOffset.Offset(Overlay.DesiredSize.Width, 0);
                }
                else
                {
                    var iRect = new System.Windows.Rect(offset, Overlay.DesiredSize);
                    Overlay.Arrange(iRect);
                    iSubOffset.Offset(0, Overlay.DesiredSize.Height);
                }
            }

            Arrange(size, offset, iSubOffset);
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            base.Release();
            if (Overlay != null)
            {
                Panel.Children.Remove(Overlay);
            }
        }

        #endregion
    }
}