// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropTargetBase.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DropTargetBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DnD
{
    using System.Linq;

    /// <summary>
    /// </summary>
    public abstract class DropTargetBase
    {
        /// <summary>The f supported format.</summary>
        private string fSupportedFormat = "SampleFormat";

        /// <summary>Gets or sets the target ui.</summary>
        public virtual System.Windows.UIElement TargetUI { get; set; }

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        public virtual bool UsePreviewEvents
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets or sets the supported format.</summary>
        public virtual string SupportedFormat
        {
            get
            {
                return fSupportedFormat;
            }

            set
            {
                fSupportedFormat = value;
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return obj.GetDataPresent(fSupportedFormat);
        }

        /// <summary>Called when a drop is performed.</summary>
        /// <remarks>modified from original, changed IDataobject to DragEventArgs so that
        ///     we have more info about the drop, like on who we are dropping.</remarks>
        /// <param name="obj"></param>
        /// <param name="dropPoint"></param>
        public abstract void OnDropCompleted(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint);

        /// <summary>Gets the visual feedback.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="UIElement"/>.</returns>
        public virtual System.Windows.UIElement GetVisualFeedback(System.Windows.IDataObject obj)
        {
            var iEl = ExtractElement(obj);
            if (iEl != null)
            {
                iEl.Opacity = 0.5;
                iEl.IsHitTestVisible = false;
                return iEl;
            }

            return null;
        }

        /// <summary>Extracts the element.</summary>
        /// <remarks>When there are multiple items on the list, we must construct a new
        ///     element (canvas) containing all the items.</remarks>
        /// <param name="obj">The data object from which to extract the visual element.</param>
        /// <returns>The <see cref="UIElement"/>.</returns>
        public virtual System.Windows.UIElement ExtractElement(System.Windows.IDataObject obj)
        {
            var iRes =
                obj.GetData(JaStDev.HAB.Designer.Properties.Resources.UIElementFormat) as System.Windows.UIElement;
            if (iRes != null)
            {
                return DragDropManager.GetRectangleFor(iRes);
            }

            var iList =
                obj.GetData(JaStDev.HAB.Designer.Properties.Resources.MultiUIElementFormat) as
                System.Collections.Generic.List<System.Windows.UIElement>;
            if (iList != null && iList.Count > 0)
            {
                var iFirst = iList[0];
                var iMetrics = from i in iList

                               // extract some info for each item in the list
                               select new
                                          {
                                              Point = i.TranslatePoint(new System.Windows.Point(0, 0), iFirst), 

                                              // this gets the top corner point relative to the first item in the list (which is the item being dragged).  This is used to calculate the total size of all the item.
                                              Width = i.GetValue(System.Windows.FrameworkElement.ActualWidthProperty), 
                                              Height = i.GetValue(System.Windows.FrameworkElement.ActualHeightProperty)
                                          };
                var iSum = new { MinX = iMetrics.Min(i => i.Point.X), // the smallest value (to the left)
                                 MaxX = iMetrics.Max(i => i.Point.X + (double)i.Width), 

                                 // the biggest value (to the right)
                                 MinY = iMetrics.Min(i => i.Point.Y), // top
                                 MaxY = iMetrics.Max(i => i.Point.Y + (double)i.Height)

                                 // the biggest value (to the bottom)
                               };

                var iIndex = 0;
                var iCanvas = new System.Windows.Controls.Canvas();
                iCanvas.Width = System.Math.Abs(iSum.MinX) + System.Math.Abs(iSum.MaxX);

                    // we must assign a fixed widht/height to the canvas cause this is a top item.  MinX contains the left most pos, MaxX contains the biggest pos + size 
                iCanvas.Height = System.Math.Abs(iSum.MinY) + System.Math.Abs(iSum.MaxY);
                var iTransform = new System.Windows.Media.TranslateTransform(iSum.MinX, iSum.MinY);

                    // we also need to add a transform so that the canvas is positioned correctly over the items.
                iCanvas.RenderTransform = iTransform;
                foreach (var i in iMetrics)
                {
                    var iRect = DragDropManager.GetRectangleFor(iList[iIndex]);

                        // we create a rect for each UI item and put it in the canvas.
                    System.Windows.Controls.Canvas.SetTop(iRect, i.Point.Y - iSum.MinY);
                    System.Windows.Controls.Canvas.SetLeft(iRect, i.Point.X - iSum.MinX);
                    iCanvas.Children.Add(iRect);
                    iIndex++;
                }

                iRes = iCanvas;
            }

            return iRes;
        }

        /// <summary>Called whenever an item is being moved over the drop target. By
        ///     default doesn't do anything.</summary>
        /// <remarks>Can be used to adjust the <paramref name="position"/> (for snapping)
        ///     or draw extra placement information.</remarks>
        /// <param name="position"></param>
        /// <param name="data">The data being dragged.</param>
        public virtual void OnDragOver(ref System.Windows.Point position, System.Windows.IDataObject data)
        {
        }

        /// <summary>Called whenever an item is dragged into the drop target. By default,
        ///     doesn't do anything. Allows descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public virtual void OnDragEnter(System.Windows.DragEventArgs e)
        {
        }

        /// <summary>Called whenever an item is dragged out of the drop target. By default,
        ///     doesn't do anything. Allows descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public virtual void OnDragLeave(System.Windows.DragEventArgs e)
        {
        }

        /// <summary>Gets the effect that should be used for the drop operation.</summary>
        /// <remarks>By default, this function checks the control key, wen pressed, a copy
        ///     is done, otherwise a move.</remarks>
        /// <param name="e">The drag event arguments.</param>
        /// <returns>The prefered effect to use.</returns>
        public virtual System.Windows.DragDropEffects GetEffect(System.Windows.DragEventArgs e)
        {
            return ((e.KeyStates & System.Windows.DragDropKeyStates.ControlKey) != 0)
                       ? System.Windows.DragDropEffects.Copy
                       : System.Windows.DragDropEffects.Move;
        }
    }
}