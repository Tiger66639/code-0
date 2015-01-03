// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlExpander.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   The little expander/collapser in front of code items This has the
//   <see cref="CPPItemList" /> as content, so the expand/collaps can be
//   controlled.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     The little expander/collapser in front of code items This has the
    ///     <see cref="CPPItemList" /> as content, so the expand/collaps can be
    ///     controlled.
    /// </summary>
    public partial class CtrlExpander : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlExpander"/> class.</summary>
        public CtrlExpander()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the header that we need to control. This actually needs
        ///     to be collapsed/expanded to get a visual response. The dataContext is
        ///     for the backend storage.
        /// </summary>
        /// <value>
        ///     The header.
        /// </value>
        public CPPHeaderedItemList Header { get; set; }

        /// <summary>Handles the Unchecked event of the Toggle control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Toggle_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Header != null)
            {
                Header.List.IsExpanded = false;
                Header.List.ItemsSource = null;

                    // we reset this, the code item also destroys this, so we remove the ref so that it can be  garbage collected.
            }

            e.Handled = true;
        }

        /// <summary>Handles the Checked event of the Toggle control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Toggle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Header != null)
            {
                var iData = DataContext as ICodeItemsOwner;
                if (iData != null)
                {
                    Header.List.ItemsSource = iData.Items;

                        // most expandable code items destroy their list with children when collapsed, to save on resources. So we need to assign this collection when we expand.
                    Header.List.IsExpanded = true;
                }
            }

            e.Handled = true;
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     This event data reports details about the mouse button that was
        ///     pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var iContext = DataContext as CodeItem;
            Focus();
            if (iContext != null)
            {
                iContext.IsSelected = true;
            }
        }
    }
}