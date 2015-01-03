// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonCodeItemsControls.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   The common code items controls.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The common code items controls.</summary>
    public partial class CommonCodeItemsControls : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="CommonCodeItemsControls"/> class. 
        ///     Initializes a new instance of the<see cref="CommonCodeItemsControls"/> class.</summary>
        public CommonCodeItemsControls()
        {
            InitializeComponent();
        }

        /// <summary>Handles the MouseLeftButtonDown event of the CodeItemDropTargetGrid
        ///     control.</summary>
        /// <remarks>We need to move and keep focus to the item that raised the mouse down
        ///     event so that a paste works. This needs a specific type of object to
        ///     be focused.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void CodeItemDropTargetGrid_MouseLeftButtonDown(
            object sender, 
            System.Windows.Input.MouseButtonEventArgs e)
        {
            var iOrSource = sender as System.Windows.FrameworkElement;
            if (iOrSource != null)
            {
                var iPresenter = iOrSource.Tag as System.Windows.Controls.ContentPresenter;

                    // only try to capture the focus if there is no content.  Otherwise we leave the content item to capture it.Otherwise we can't see the debug data.
                if (iPresenter == null || iPresenter.Content == null)
                {
                    iOrSource.Focus();
                    e.Handled = true;

                        // if we don't do this, the event goes up to the editor item, which will also try to get focus.
                }
            }
        }
    }
}