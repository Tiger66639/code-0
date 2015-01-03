// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemPanelDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for <see cref="CodeItemCollection" /> objects housed on a CodePagePanel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for <see cref="CodeItemCollection" /> objects housed on a CodePagePanel.
    /// </summary>
    /// <remarks>
    ///     This object should be attached to a root CodePagePanel. It provides scrolling when the border is reached.
    ///     <para>
    ///         Provides only add functionality for the list, no inserts.  This has to be done with
    ///         <see cref="CodeListItemDropAdvisor" />
    ///     </para>
    /// </remarks>
    internal class CodeItemPanelDropAdvisor : CodeListDropAdvisor
    {
        /// <summary>
        ///     when entering this border, we start scrolling.
        /// </summary>
        private const double SCROLLBORDERSIZE = 10.0;

        /// <summary>The f prev pos.</summary>
        private System.Windows.Point? fPrevPos; // so we know how much we need to move the panel, when on the edge.

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is attached too, displays data for.
        /// </summary>
        /// <value></value>
        public override CodeItemCollection CodeList
        {
            get
            {
                return ((WPF.Controls.CodePagePanel)TargetUI).ItemsSource.Items;
            }
        }

        /// <summary>The on drag leave.</summary>
        /// <param name="e">The e.</param>
        public override void OnDragLeave(System.Windows.DragEventArgs e)
        {
            base.OnDragLeave(e);
        }

        /// <summary>Called whenever an item is being moved over the drop target.  By default doesn't do anything.</summary>
        /// <param name="position"></param>
        /// <param name="data">The data being dragged.</param>
        /// <remarks>Can be used to adjust the position (for snapping) or draw extra placement information.</remarks>
        public override void OnDragOver(ref System.Windows.Point position, System.Windows.IDataObject data)
        {
            var iPanel = (WPF.Controls.CodePagePanel)TargetUI;
            System.Diagnostics.Debug.Assert(iPanel != null);
            if (fPrevPos.HasValue)
            {
                var iDivX = fPrevPos.Value.X - position.X;

                    // when we are moving in the opposite direction, don't scroll, so we need  to check the direction.
                var iDivY = fPrevPos.Value.Y - position.Y;

                var iVal = iPanel.ActualWidth;
                if (position.X + SCROLLBORDERSIZE > iVal && iDivX >= 0)
                {
                    iPanel.HorizontalOffset += (position.X - fPrevPos.Value.X) + (position.X + SCROLLBORDERSIZE - iVal);
                }
                else if (position.X <= SCROLLBORDERSIZE && iDivX <= 0)
                {
                    iPanel.HorizontalOffset -= SCROLLBORDERSIZE - position.X;
                }

                iVal = iPanel.ActualHeight;
                if (position.Y + SCROLLBORDERSIZE > iVal && iDivY >= 0)
                {
                    iPanel.VerticalOffset += (position.Y - fPrevPos.Value.Y) + (position.Y + SCROLLBORDERSIZE - iVal);
                }
                else if (position.Y <= SCROLLBORDERSIZE && iDivY <= 0)
                {
                    iPanel.VerticalOffset -= SCROLLBORDERSIZE - position.Y;
                }
            }

            fPrevPos = position;
        }
    }
}