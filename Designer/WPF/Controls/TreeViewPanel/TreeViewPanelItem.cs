// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeViewPanelItem.cs" company="">
//   
// </copyright>
// <summary>
//   The tree view panel item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>The tree view panel item.</summary>
    public class TreeViewPanelItem : System.Windows.Controls.ContentControl
    {
        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <remarks>When the user clicks on the item, focus.</remarks>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     This event data reports details about the mouse button that was
        ///     pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            var iData = (ITreeViewPanelItem)Content;
            System.Diagnostics.Debug.Assert(iData != null);
            iData.IsSelected = true;
            e.Handled = true;
        }

        /// <summary>The on key down.</summary>
        /// <param name="e">The e.</param>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Home)
            {
                var iPanel = Parent as TreeViewPanel;
                if (iPanel != null)
                {
                    iPanel.SelectFirst();
                }

                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.End)
            {
                var iPanel = Parent as TreeViewPanel;
                if (iPanel != null)
                {
                    iPanel.SelectLast();
                }

                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                var iPanel = Parent as TreeViewPanel;
                var iData = (ITreeViewPanelItem)Content;
                iPanel.SelectPrev(iData);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                var iPanel = Parent as TreeViewPanel;
                var iData = (ITreeViewPanelItem)Content;
                iPanel.SelectNext(iData);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Left)
            {
                var iData = (ITreeViewPanelItem)Content;
                if (iData.IsExpanded)
                {
                    iData.IsExpanded = false;
                }
                else
                {
                    var iPanel = Parent as TreeViewPanel;
                    iPanel.SelectPrev(iData);
                }

                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                var iData = (ITreeViewPanelItem)Content;
                if (iData.IsExpanded == false)
                {
                    iData.IsExpanded = true;
                }
                else
                {
                    var iPanel = Parent as TreeViewPanel;
                    iPanel.SelectNext(iData);
                }

                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        #region Level

        /// <summary>
        ///     <see cref="Level" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LevelProperty =
            System.Windows.DependencyProperty.Register(
                "Level", 
                typeof(int), 
                typeof(TreeViewPanelItem), 
                new System.Windows.FrameworkPropertyMetadata(0));

        /// <summary>
        ///     Gets or sets the <see cref="Level" /> property. This dependency
        ///     property indicates the level of the item within the tree. This is used
        ///     by the panel to render it at the correct horizontal position.
        /// </summary>
        public int Level
        {
            get
            {
                return (int)GetValue(LevelProperty);
            }

            set
            {
                SetValue(LevelProperty, value);
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     <see cref="IsSelected" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsSelectedProperty =
            System.Windows.DependencyProperty.Register(
                "IsSelected", 
                typeof(bool), 
                typeof(TreeViewPanelItem), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IsSelected" /> property. This dependency
        ///     property indicates if this item is selected or not.
        /// </summary>
        /// <remarks>
        ///     It's just easier to have this prop and bind against, so we can use it
        ///     in normal triggers (instead of datatriggers.
        /// </remarks>
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
            ((TreeViewPanelItem)d).OnIsSelectedChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="IsSelected"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIsSelectedChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // when we get selected, make certain that we focus the item.
                Focus();
            }
        }

        #endregion
    }
}