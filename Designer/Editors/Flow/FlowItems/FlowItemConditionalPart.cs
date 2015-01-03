// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemConditionalPart.cs" company="">
//   
// </copyright>
// <summary>
//   A single list of items within a loop or option.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A single list of items within a loop or option.
    /// </summary>
    public class FlowItemConditionalPart : FlowItemBlock
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItemConditionalPart"/> class. Initializes a new instance of the<see cref="FlowItemConditionalPart"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FlowItemConditionalPart(NeuronCluster toWrap)
            : base(toWrap)
        {
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.FlowPanel"/> object.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        /// <returns>The <see cref="FlowPanelItemBase"/>.</returns>
        protected internal override WPF.Controls.FlowPanelItemBase CreateDefaultUI(
            WPF.Controls.FlowPanelItemBase owner, 
            WPF.Controls.FlowPanel panel)
        {
            var iNew = new WPF.Controls.OverlayedFlowPanelItemList(owner, panel);
            iNew.IsExpanded = true;
            iNew.ItemsSource = Items;
            iNew.Orientation = System.Windows.Controls.Orientation.Horizontal;

            var iBind = new System.Windows.Data.Binding("IsSelected")
                            {
                                Source = this, 
                                Mode = System.Windows.Data.BindingMode.OneWay
                            };
            System.Windows.Data.BindingOperations.SetBinding(
                iNew, 
                WPF.Controls.FlowPanelItemBase.IsSelectedProperty, 
                iBind);

                // so the flowpanel item knows when our selected is chagned: loose coupling, we can have multiple views
            System.Windows.Controls.Border iBackground = new WPF.Controls.FlowItemCondionalPartBorder();
            iBackground.DataContext = this;
            iBackground.Style = panel.ConditionalPartBackgroundStyle;
            iNew.ListBackground = iBackground;

            var iOverlayData = new FlowItemConditionalPartView();
            iOverlayData.DataContext = this;
            iNew.Overlay = iOverlayData;

            return iNew;
        }
    }
}