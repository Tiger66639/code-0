// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridImage.cs" company="">
//   
// </copyright>
// <summary>
//   The grid image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The grid image.</summary>
    public class GridImage
    {
        /// <summary>
        ///     Determins the maximum width and height of the input grid. This is to keep the ui working somewhat.
        /// </summary>
        public const int MAXGRIDSIZE = 50;

        /// <summary>The tilesize.</summary>
        public const double TILESIZE = 10;

        /// <summary>The bytesperpixel.</summary>
        public const int BYTESPERPIXEL = 3; // PixelFormats.Bgr24.BitsPerPixel / 8;

        /// <summary>The f grid pixels.</summary>
        private byte[] fGridPixels;

        /// <summary>The f image channel.</summary>
        private readonly ImageChannel fImageChannel;

        /// <summary>The update width.</summary>
        /// <param name="iPrev">The i prev.</param>
        /// <param name="value">The value.</param>
        internal void UpdateWidth(int iPrev, int value)
        {
            if (value <= MAXGRIDSIZE)
            {
                GridEditor.Columns = value;
                if (iPrev > value)
                {
                    RemoveCols(iPrev - value, iPrev);
                }
                else
                {
                    AddCols(value - iPrev, iPrev);
                }
            }
        }

        /// <summary>The remove cols.</summary>
        /// <param name="amount">The amount.</param>
        /// <param name="oldWidth">The old width.</param>
        private void RemoveCols(int amount, int oldWidth)
        {
            var iNew = new byte[fImageChannel.Width * fImageChannel.Height * BYTESPERPIXEL];

                // don't need to init pixels since we are removing, so all others will be copied from the prev.
            var iPos = 0;
            for (var iRow = 0; iRow < fImageChannel.Height; iRow++)
            {
                for (var iNrItems = amount; iNrItems > 0; iNrItems--)
                {
                    GridEditor.Children.RemoveAt(iRow * fImageChannel.Width);
                }

                for (var i = 0; i < fImageChannel.Width * BYTESPERPIXEL; i++)
                {
                    // copy the pixels from old to new
                    iNew[iRow * fImageChannel.Width + i] = fGridPixels[(iRow * oldWidth) + i + (amount * BYTESPERPIXEL)];

                        // items are reomved from the front.
                }

                iPos += fImageChannel.Width;
            }

            fGridPixels = iNew;
        }

        /// <summary>The add cols.</summary>
        /// <param name="amount">The amount.</param>
        /// <param name="oldWidth">The old width.</param>
        private void AddCols(int amount, int oldWidth)
        {
            var iNew = GetWhitespace(fImageChannel.Height * fImageChannel.Width * BYTESPERPIXEL);
            var iPos = 0;
            for (var iRow = 0; iRow < fImageChannel.Height; iRow++)
            {
                for (var iNrItems = amount; iNrItems > 0; iNrItems--)
                {
                    InsertRectangle(iPos);
                }

                if (oldWidth > 0)
                {
                    // nothing to copy if this is the first new column
                    for (var i = 0; i < oldWidth * BYTESPERPIXEL; i++)
                    {
                        // copy the pixels from old to new
                        var iOffset = i + (amount * BYTESPERPIXEL);
                        iNew[iRow * (fImageChannel.Width * BYTESPERPIXEL) + iOffset] = fGridPixels[iRow * oldWidth + i];

                            // empty items are added in the front.
                    }
                }

                iPos += oldWidth + amount;
            }

            fGridPixels = iNew;
        }

        /// <summary>The update height.</summary>
        /// <param name="iPrev">The i prev.</param>
        /// <param name="value">The value.</param>
        internal void UpdateHeight(int iPrev, int value)
        {
            if (value <= MAXGRIDSIZE)
            {
                GridEditor.Rows = value;
                if (iPrev > fImageChannel.Height)
                {
                    RemoveRows(iPrev - fImageChannel.Height);
                }
                else
                {
                    AddRows(fImageChannel.Height - iPrev);
                }
            }
        }

        /// <summary>The add rows.</summary>
        /// <param name="amount">The amount.</param>
        private void AddRows(int amount)
        {
            var iPrevEnd = fGridPixels.Length;
            System.Array.Resize(ref fGridPixels, fImageChannel.Width * fImageChannel.Height * BYTESPERPIXEL);
            var iToAdd = amount * fImageChannel.Width;
            for (var i = 0; i < iToAdd; i++)
            {
                fGridPixels[iPrevEnd + (i * BYTESPERPIXEL)] = byte.MaxValue;
                fGridPixels[iPrevEnd + (i * BYTESPERPIXEL) + 1] = byte.MaxValue;
                fGridPixels[iPrevEnd + (i * BYTESPERPIXEL) + 2] = byte.MaxValue;

                AddRectangle();
            }
        }

        /// <summary>The remove rows.</summary>
        /// <param name="amount">The amount.</param>
        private void RemoveRows(int amount)
        {
            for (var i = 0; i < amount * fImageChannel.Width; i++)
            {
                GridEditor.Children.RemoveAt(GridEditor.Children.Count - 1);
            }

            System.Array.Resize(ref fGridPixels, fImageChannel.Width * fImageChannel.Height * BYTESPERPIXEL);
        }

        /// <summary>The create rect.</summary>
        /// <returns>The <see cref="Rectangle"/>.</returns>
        private System.Windows.Shapes.Rectangle CreateRect()
        {
            var iRect = new System.Windows.Shapes.Rectangle();
            iRect.Width = TILESIZE;
            iRect.Height = TILESIZE;
            iRect.Stroke = System.Windows.Media.Brushes.Gray;
            iRect.Fill = System.Windows.Media.Brushes.Transparent;
            iRect.StrokeThickness = 0.3;
            iRect.SnapsToDevicePixels = true;
            iRect.IsHitTestVisible = true;
            return iRect;
        }

        /// <summary>The add rectangle.</summary>
        private void AddRectangle()
        {
            var iRect = CreateRect();
            GridEditor.Children.Add(iRect);
        }

        /// <summary>The insert rectangle.</summary>
        /// <param name="pos">The pos.</param>
        private void InsertRectangle(int pos)
        {
            var iRect = CreateRect();
            GridEditor.Children.Insert(pos, iRect);
        }

        /// <summary>Handles the MouseMove event of the iRect control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void iRect_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                UpdateTile(e.OriginalSource, System.Windows.Input.MouseButton.Left);
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                UpdateTile(e.OriginalSource, System.Windows.Input.MouseButton.Right);
            }
        }

        /// <summary>The update tile.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="button">The button.</param>
        private void UpdateTile(object sender, System.Windows.Input.MouseButton button)
        {
            var iSender = sender as System.Windows.Shapes.Rectangle;
            if (iSender != null)
            {
                var iIndex = GridEditor.Children.IndexOf(iSender) * 3; // 3 bytes in 24 bits.
                if (button == System.Windows.Input.MouseButton.Left)
                {
                    fGridPixels[iIndex] = System.Windows.Media.Colors.Black.B;
                    fGridPixels[iIndex + 1] = System.Windows.Media.Colors.Black.G;
                    fGridPixels[iIndex + 2] = System.Windows.Media.Colors.Black.R;
                    iSender.Fill = System.Windows.Media.Brushes.Black;
                }
                else if (button == System.Windows.Input.MouseButton.Right)
                {
                    fGridPixels[iIndex] = System.Windows.Media.Colors.White.B;
                    fGridPixels[iIndex + 1] = System.Windows.Media.Colors.White.G;
                    fGridPixels[iIndex + 2] = System.Windows.Media.Colors.White.R;
                    iSender.Fill = System.Windows.Media.Brushes.White;
                }
            }
        }

        /// <summary>The i rect_ mouse down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iRect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateTile(e.OriginalSource, e.ChangedButton);
        }

        #region ctor/~

        /// <summary>Initializes a new instance of the <see cref="GridImage"/> class.</summary>
        /// <param name="imageChannel">The image channel.</param>
        public GridImage(ImageChannel imageChannel)
        {
            fImageChannel = imageChannel;
            fGridPixels = new byte[0]; // we need a ref, but don't know the size yet, so set to null.
        }

        /// <summary>
        ///     Inits the array so that it can be used.
        /// </summary>
        /// <remarks>
        ///     Init should always be done from the UI thread cause a Ui object gets created.
        /// </remarks>
        internal void Init()
        {
            GridEditor = new System.Windows.Controls.Primitives.UniformGrid();
            GridEditor.MouseMove += iRect_MouseMove;
            GridEditor.PreviewMouseDown += iRect_MouseDown;
            fGridPixels = GetWhitespace(fImageChannel.Height * fImageChannel.Width * BYTESPERPIXEL);
            GridEditor.Rows = fImageChannel.Height;
            GridEditor.Columns = fImageChannel.Width;
            for (var i = 0; i < fGridPixels.Length / 3; i++)
            {
                // fill with the appropriate items.
                var iRect = CreateRect();
                GridEditor.Children.Add(iRect);
            }
        }

        /// <summary>creates a new whitespace array.</summary>
        /// <param name="size">The size.</param>
        /// <returns>The <see cref="byte[]"/>.</returns>
        private static byte[] GetWhitespace(int size)
        {
            var iRes = new byte[size];
            for (var i = 0; i < iRes.Length; i++)
            {
                // init to max value = white
                iRes[i] = byte.MaxValue;
            }

            return iRes;
        }

        /// <summary>The release data.</summary>
        internal void ReleaseData()
        {
            fGridPixels = new byte[0];
            GridEditor.Children.Clear();
        }

        #endregion

        #region prop

        /// <summary>
        ///     Gets the pixels.
        /// </summary>
        public byte[] Pixels
        {
            get
            {
                return fGridPixels;
            }
        }

        #region GridEditor

        /// <summary>
        ///     Gets the visual that can be used to edit the data.
        /// </summary>
        /// <remarks>
        ///     We need to update it, that's why we provide it from here.
        /// </remarks>
        public System.Windows.Controls.Primitives.UniformGrid GridEditor { get; private set; }

        #endregion

        #endregion
    }
}