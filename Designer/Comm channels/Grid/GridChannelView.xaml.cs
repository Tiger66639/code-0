// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for GridChannelView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for GridChannelView.xaml
    /// </summary>
    public partial class GridChannelView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="GridChannelView"/> class.</summary>
        public GridChannelView()
        {
            InitializeComponent();
        }

        #region Up/down buttons.

        /// <summary>Handles the Click event of the RepeatNrWidthUp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatNrWidthUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (GridChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Width++;
            }
        }

        /// <summary>Handles the Click event of the RepeatNrWidthDown control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatNrWidthDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (GridChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Width--;
            }
        }

        /// <summary>Handles the Click event of the RepeatNrHeightUp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatNrHeightUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (GridChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Height++;
            }
        }

        /// <summary>Handles the Click event of the RepeatNrHeightDown control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RepeatNrHeightDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (GridChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Height--;
            }
        }

        #endregion
    }
}