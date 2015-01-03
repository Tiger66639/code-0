// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ImageChannelView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for ImageChannelView.xaml
    /// </summary>
    public partial class ImageChannelView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ImageChannelView"/> class.</summary>
        public ImageChannelView()
        {
            InitializeComponent();
            DataContextChanged += ImageChannelView_DataContextChanged;
        }

        /// <summary>Handles the DataContextChanged event of the ImageChannelView control.
        ///     When we get an ImageChannel, need to hook up the UniFormGrid that is
        ///     provided by the GridImage, so that it is displayed.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void ImageChannelView_DataContextChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iChannel = DataContext as ImageChannel;
            if (iChannel != null)
            {
                if (GrdPixels.Children.Contains(iChannel.GridImage.GridEditor) == false
                    && iChannel.GridImage.GridEditor != null)
                {
                    GrdPixels.Children.Add(iChannel.GridImage.GridEditor);
                }
            }

            iChannel = e.OldValue as ImageChannel; // need to unhook, just in case, to prevent leaks.
            if (iChannel != null)
            {
                GrdPixels.Children.Remove(iChannel.GridImage.GridEditor);
            }
        }

        /// <summary>
        ///     makes certain that the grid input is activated and not in error
        ///     display.
        /// </summary>
        private void ReleaseGridInputToLarge()
        {
            GrdPixels.Visibility = System.Windows.Visibility.Visible;
            TxtGridError.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        ///     Hide the grid input and shows an error.
        /// </summary>
        private void SetGridInputToLarge()
        {
            GrdPixels.Visibility = System.Windows.Visibility.Collapsed;
            TxtGridError.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>Handles the SelectionChanged event of the TabInput control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void TabInput_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var iChannel = (ImageChannel)DataContext;
                iChannel.SelectedInputView = TabInput.SelectedIndex;
            }
        }

        #region event handlers

        #region Up/down buttons.

        /// <summary>The repeat nr width up_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrWidthUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Width++;
            }
        }

        /// <summary>The repeat nr width down_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrWidthDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Width--;
            }
        }

        /// <summary>The repeat nr height up_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrHeightUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Height++;
            }
        }

        /// <summary>The repeat nr height down_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatNrHeightDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.Height--;
            }
        }

        /// <summary>The repeat pen height down_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatPenHeightDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InkInput.DefaultDrawingAttributes.Height--;
        }

        /// <summary>The repeat pen height up_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatPenHeightUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InkInput.DefaultDrawingAttributes.Height++;
        }

        /// <summary>The repeat pen width down_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatPenWidthDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InkInput.DefaultDrawingAttributes.Width--;
        }

        /// <summary>The repeat pen width up_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RepeatPenWidthUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InkInput.DefaultDrawingAttributes.Width++;
        }

        #endregion

        /// <summary>The btn send_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                if (TabInput.SelectedIndex == 0)
                {
                    iChannel.SendGrid();
                }
                else
                {
                    iChannel.SendImage(InkInput);
                }
            }
        }

        /// <summary>Handles the TargetUpdated event of the TxtWidth control.</summary>
        /// <remarks>need to recalculate the grid if it is visible.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Data.DataTransferEventArgs"/> instance containing the event
        ///     data.</param>
        private void TxtWidth_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                if (iChannel.Height > 0)
                {
                    if (iChannel.Width <= GridImage.MAXGRIDSIZE)
                    {
                        ReleaseGridInputToLarge();
                    }
                    else
                    {
                        SetGridInputToLarge();
                    }

                    GridScaler.CenterX = (iChannel.Width * GridImage.TILESIZE) / 2;

                        // whenever the nr of items changes, we need to recalculate the center of the scaling object, othewise the zoom works funny.                                
                    StylusScaler.CenterX = GridScaler.CenterX;
                }
            }
        }

        /// <summary>The txt height_ target updated.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtHeight_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var iChannel = (ImageChannel)DataContext;
            if (iChannel != null)
            {
                // shortcut for checking of the grid is visible.
                if (iChannel.Width > 0)
                {
                    if (iChannel.Height < GridImage.MAXGRIDSIZE)
                    {
                        ReleaseGridInputToLarge();
                    }
                    else
                    {
                        SetGridInputToLarge();
                    }
                }

                GridScaler.CenterY = (iChannel.Width * GridImage.TILESIZE) / 2;
                StylusScaler.CenterY = GridScaler.CenterY;
            }
        }

        #endregion
    }
}