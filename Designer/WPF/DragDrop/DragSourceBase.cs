// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragSourceBase.cs" company="">
//   
// </copyright>
// <summary>
//   a base clas for drag functionality providers
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DnD
{
    using System.Linq;

    /// <summary>
    ///     a base clas for drag functionality providers
    /// </summary>
    public abstract class DragSourceBase
    {
        #region SourceUI

        /// <summary>
        ///     Gets or sets the source UI that the dragsource is currently servising.
        /// </summary>
        /// <value>
        ///     The source UI handling the events.
        /// </value>
        public virtual System.Windows.UIElement SourceUI
        {
            get
            {
                return fSourceElt;
            }

            set
            {
                fSourceElt = value;
                if (value == null)
                {
                    // when this dragsource gets reset, don't forget to free the mem for the visual that was used..
                    DragVisual = null;
                }
            }
        }

        #endregion

        #region UsePreviewEvents

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

        #endregion

        #region SupportedEffects

        /// <summary>
        ///     Gets the supported effects.
        /// </summary>
        /// <value>
        ///     The supported effects.
        /// </value>
        public virtual System.Windows.DragDropEffects SupportedEffects
        {
            get
            {
                return System.Windows.DragDropEffects.Copy | System.Windows.DragDropEffects.Move;
            }
        }

        #endregion

        #region SupportedFormat

        /// <summary>
        ///     Gets or sets the supported format.
        /// </summary>
        /// <value>
        ///     The supported format.
        /// </value>
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

        #endregion

        #region DragVisual

        /// <summary>
        ///     Gets the UIElement or list of UIElements that should be displayed.
        /// </summary>
        public System.Windows.UIElement DragVisual { get; internal set; }

        #endregion

        /// <summary>The extract element.</summary>
        /// <param name="toExtract">The to extract.</param>
        /// <returns>The <see cref="UIElement"/>.</returns>
        private System.Windows.UIElement ExtractElement(object toExtract)
        {
            var iRes = toExtract as System.Windows.UIElement;
            if (iRes != null)
            {
                return DragDropManager.GetRectangleFor(iRes);
            }

            var iList = toExtract as System.Collections.Generic.List<System.Windows.UIElement>;
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

        #region Fields

        /// <summary>The f supported format.</summary>
        private string fSupportedFormat = "SampleFormat";

        /// <summary>The f source elt.</summary>
        private System.Windows.UIElement fSourceElt;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DragSourceBase" /> class.
        /// </summary>
        public DragSourceBase()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DragSourceBase"/> class.</summary>
        /// <param name="supportedFormat">The default supported format.</param>
        public DragSourceBase(string supportedFormat)
        {
            fSupportedFormat = supportedFormat;
        }

        #endregion

        #region functions

        /// <summary>Gets the data object containing all the data required to perform the
        ///     drag operation.</summary>
        /// <remarks>To add multiple objects to the data object, use<see cref="Properties.Resources.MultiUIElementFormat"/> as data type
        ///     for the visual object, with a list containing all the ui elements. The
        ///     drop target base will make it into a proper composite image. The first
        ///     item in the list must be the item actually being dragged.</remarks>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <returns>The <see cref="DataObject"/>.</returns>
        public virtual System.Windows.DataObject GetDataObject(System.Windows.UIElement draggedElt)
        {
            var serializedElt = System.Windows.Markup.XamlWriter.Save(draggedElt);
            var obj = new System.Windows.DataObject();
            obj.SetData(fSupportedFormat, serializedElt);

            // obj.SetData(DataFormats.Xaml, serializedElt);
            return obj;
        }

        #region drag

        /// <summary>Called when the drag is finished</summary>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <param name="finalEffects">The final effects.</param>
        public abstract void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects);

        /// <summary>Determines whether the specified drag elt is draggable.</summary>
        /// <param name="dragElt">The drag elt.</param>
        /// <returns><c>true</c> if the specified drag elt is draggable; otherwise,<c>false</c> .</returns>
        public abstract bool IsDraggable(System.Windows.UIElement dragElt);

        #endregion

        #region delay load

        /// <summary>If this drag source object supports delay loading of the data,
        ///     descendents should reimplement this function.</summary>
        /// <param name="data">The data.</param>
        public virtual void DelayLoad(System.Windows.IDataObject data)
        {
        }

        /// <summary>If this drag source supports delay loading of the data, descendents
        ///     should reimplement this function to let the system know the type of<paramref name="data"/> the drag source will provide.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        public virtual System.Type GetDelayLoadResultType(System.Windows.IDataObject data)
        {
            return null;
        }

        #endregion

        #region visual feedback

        /// <summary>Adds the UI element to the data object, keeping acount of the fact
        ///     that there might be a list of other visuals assigned to the ui
        ///     elements that need to be used during dragging.</summary>
        /// <param name="obj">The data obj.</param>
        /// <param name="items">The list of UI elements.</param>
        protected void AddUIElementToData(
            System.Windows.DataObject obj, System.Collections.Generic.List<System.Windows.UIElement> items)
        {
            var iRes = new System.Collections.Generic.List<System.Windows.UIElement>();
            var iCount = 0;
            while (iCount < items.Count)
            {
                var iFound = DragDropManager.GetVisuals(items[iCount]);
                if (iFound != null)
                {
                    foreach (var i in iFound)
                    {
                        if (i.IsAlive)
                        {
                            var iNew = i.Target as System.Windows.UIElement;
                            if (iNew != null)
                            {
                                iRes.Add(iNew);
                            }
                        }
                    }
                }
                else
                {
                    iRes.Add(items[iCount]);
                    iCount++;
                }
            }

            StoreDragVisual(obj, JaStDev.HAB.Designer.Properties.Resources.MultiUIElementFormat, iRes);
        }

        /// <summary>Adds the UI element to the data object, keeping acount of the fact
        ///     that there might be a list of other visuals assigned to the ui
        ///     elements that need to be used during dragging.</summary>
        /// <param name="obj">The data obj.</param>
        /// <param name="item">The UI element.</param>
        protected void AddUIElementToData(System.Windows.DataObject obj, System.Windows.UIElement item)
        {
            var iRes = new System.Collections.Generic.List<System.Windows.UIElement>();
            var iFound = DragDropManager.GetVisuals(item);

                // first try the item bein dragged itself, see if it has anything defined that needs to be used as dag image.
            if (iFound == null)
            {
                iFound = DragDropManager.GetVisuals(SourceUI);

                    // the item might be a focusable child on the actual control that needs to be dragged (has the manager). Perhaps this has other visuals defined.
            }

            if (iFound != null)
            {
                foreach (var i in iFound)
                {
                    if (i.IsAlive)
                    {
                        var iNew = i.Target as System.Windows.UIElement;
                        if (iNew != null)
                        {
                            iRes.Add(iNew);
                        }
                    }
                }

                if (iRes.Count > 1)
                {
                    StoreDragVisual(obj, JaStDev.HAB.Designer.Properties.Resources.MultiUIElementFormat, iRes);
                }
                else
                {
                    StoreDragVisual(obj, JaStDev.HAB.Designer.Properties.Resources.UIElementFormat, iRes[0]);
                }
            }
            else
            {
                StoreDragVisual(obj, JaStDev.HAB.Designer.Properties.Resources.UIElementFormat, item);
            }
        }

        /// <summary>The store drag visual.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void StoreDragVisual(System.Windows.DataObject obj, string key, object value)
        {
            if (DragDropManager.SourceCreatesDragVisual)
            {
                SetDragVisual(value);
            }
            else
            {
                obj.SetData(key, value);
            }
        }

        /// <summary>The set drag visual.</summary>
        /// <param name="value">The value.</param>
        private void SetDragVisual(object value)
        {
            var iEl = ExtractElement(value);
            if (iEl != null)
            {
                iEl.Opacity = 0.5;
                iEl.IsHitTestVisible = false;
                DragVisual = iEl;
            }
            else
            {
                DragVisual = null;
            }
        }

        #endregion

        #endregion
    }
}