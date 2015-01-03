// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItem.cs" company="">
//   
// </copyright>
// <summary>
//   The base class for flow items like statics, and conditional items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The base class for flow items like statics, and conditional items.
    /// </summary>
    public class FlowItem : EditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItem"/> class.</summary>
        /// <param name="towrap">The towrap.</param>
        public FlowItem(Neuron towrap)
            : base(towrap)
        {
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.FlowPanel"/> object.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        /// <returns>The <see cref="FlowPanelItemBase"/>.</returns>
        protected internal virtual WPF.Controls.FlowPanelItemBase CreateDefaultUI(
            WPF.Controls.FlowPanelItemBase owner, 
            WPF.Controls.FlowPanel panel)
        {
            var iNew = new WPF.Controls.FlowPanelItem(owner, panel);
            iNew.Data = this;
            iNew.Element = new FlowItemStaticView();
            return iNew;
        }
    }
}