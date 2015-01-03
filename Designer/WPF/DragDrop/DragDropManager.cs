// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragDropManager.cs" company="">
//   
// </copyright>
// <summary>
//   Drag drop engine class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DnD
{
    using System.Linq;

    /// <summary>
    ///     Drag drop engine class.
    /// </summary>
    public static class DragDropManager
    {
        /// <summary>The tolerance.</summary>
        public const double TOLERANCE = 2.0;

        /// <summary>
        ///     This constant is added to the height and width of the preview rectangles, to make certain that they contain a
        ///     possible
        ///     dropsadow and the likes. This value needs to be added to the drop location, to get the real position.
        /// </summary>
        public const double PREVIEWADORNERBORDER = 10.0;

        /// <summary>The get offset point.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="Point"/>.</returns>
        private static System.Windows.Point GetOffsetPoint(System.Windows.IDataObject obj)
        {
            return (System.Windows.Point)obj.GetData("OffsetPoint");
        }

        /// <summary>The update effects.</summary>
        /// <param name="uiObject">The ui object.</param>
        /// <param name="e">The e.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool UpdateEffects(object uiObject, System.Windows.DragEventArgs e)
        {
            var advisor = GetDropTarget(uiObject as System.Windows.DependencyObject);
            advisor.TargetUI = uiObject as System.Windows.UIElement;

                // we must reassign it cause this can be a shared advisor, for which this value switches
            if (advisor.IsValidDataObject(e.Data) == false)
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return false;
            }

            if ((e.AllowedEffects & System.Windows.DragDropEffects.Move) == 0
                && (e.AllowedEffects & System.Windows.DragDropEffects.Copy) == 0)
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return true;
            }

            if ((e.AllowedEffects & System.Windows.DragDropEffects.Move) != 0
                && (e.AllowedEffects & System.Windows.DragDropEffects.Copy) != 0)
            {
                e.Effects = advisor.GetEffect(e);
            }

            return true;
        }

        /// <summary>The is drag gesture.</summary>
        /// <param name="point">The point.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool IsDragGesture(System.Windows.Point point)
        {
            var hGesture = System.Math.Abs(point.X - fDragStartPoint.X)
                           > System.Windows.SystemParameters.MinimumHorizontalDragDistance;
            var vGesture = System.Math.Abs(point.Y - fDragStartPoint.Y)
                           > System.Windows.SystemParameters.MinimumVerticalDragDistance;

            return hGesture | vGesture;
        }

        /* ____________________________________________________________________
		 *		Utility functions
		 * ____________________________________________________________________
		 */

        /// <summary>The get top container.</summary>
        /// <param name="from">The from.</param>
        /// <returns>The <see cref="UIElement"/>.</returns>
        private static System.Windows.UIElement GetTopContainer(System.Windows.UIElement from)
        {
            // VisualTreeHelper.GetChild(Window.GetWindow(from), 0)
            var iWindow = System.Windows.Window.GetWindow(from);
            if (iWindow != null)
            {
                return iWindow.Content as System.Windows.UIElement;
            }

            return null;
        }

        /// <summary>The create preview adorner.</summary>
        /// <param name="adornedElt">The adorned elt.</param>
        /// <param name="feedbackUI">The feedback ui.</param>
        private static void CreatePreviewAdorner(
            System.Windows.UIElement adornedElt, 
            System.Windows.UIElement feedbackUI)
        {
            // Clear if there is an existing preview adorner
            RemovePreviewAdorner();
            fPrevAdorned = adornedElt;
            fPrevTopContainer = GetTopContainer(adornedElt);
            var layer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(fPrevTopContainer);
            fOverlayElt = new DropPreviewAdorner(feedbackUI, adornedElt);

            var position = System.Windows.Input.Mouse.GetPosition(adornedElt);
            fOverlayElt.Left = position.X - fOffsetPoint.X;
            fOverlayElt.Top = position.Y - fOffsetPoint.Y;

            layer.Add(fOverlayElt);
        }

        /// <summary>The remove preview adorner.</summary>
        private static void RemovePreviewAdorner()
        {
            if (fOverlayElt != null)
            {
                System.Windows.Documents.AdornerLayer.GetAdornerLayer(fPrevTopContainer).Remove(fOverlayElt);
                fOverlayElt = null;
            }
        }

        /// <summary>Creates a rectangle and assigns it a visual brush containing the arg to use for Fill.</summary>
        /// <param name="item">The item.</param>
        /// <returns>A new rectangle that represents the item.</returns>
        public static System.Windows.Shapes.Rectangle GetRectangleFor(System.Windows.UIElement item)
        {
            var iType = item.GetType();
            var iRect = new System.Windows.Shapes.Rectangle();
            iRect.Width = (double)iType.GetProperty("ActualWidth").GetValue(item, null) + PREVIEWADORNERBORDER;
            iRect.Height = (double)iType.GetProperty("ActualHeight").GetValue(item, null) + PREVIEWADORNERBORDER;
            var iBrush = new System.Windows.Media.VisualBrush(item);
            iBrush.Stretch = System.Windows.Media.Stretch.None;

                // we need to turn of stretching, otherwise, some items can get disfigured because of bitmapeffects.
            iRect.Fill = iBrush;
            var iEffect = GetDragEffect(item);
            if (iEffect != null)
            {
                iRect.Effect = iEffect;
            }

            return iRect;
        }

        /// <summary>The f dragged elt.</summary>
        private static System.Windows.UIElement fDraggedElt;

        /// <summary>The f is mouse down.</summary>
        private static bool fIsMouseDown;

        /// <summary>The f drag start point.</summary>
        private static System.Windows.Point fDragStartPoint;

        /// <summary>The f offset point.</summary>
        private static System.Windows.Point fOffsetPoint;

        /// <summary>The f current drag source.</summary>
        private static DragSourceBase fCurrentDragSource;

        /// <summary>The f overlay elt.</summary>
        private static DropPreviewAdorner fOverlayElt;

        /// <summary>The f source creates drag visual.</summary>
        private static bool fSourceCreatesDragVisual = true;

        /// <summary>The f prev top container.</summary>
        private static System.Windows.UIElement fPrevTopContainer;

                                                // so that we can remove from the correct top container.

        /// <summary>The f prev adorned.</summary>
        private static System.Windows.UIElement fPrevAdorned;

                                                // so we don't remove the adorner layer if we are leaving something other then the corruntly adorned element.
        #region prop

        #region Drag source

        /// <summary>The drag source property.</summary>
        public static readonly System.Windows.DependencyProperty DragSourceProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "DragSource", 
                typeof(DragSourceBase), 
                typeof(DragDropManager), 
                new System.Windows.FrameworkPropertyMetadata(OnDragSourceChanged));

        /// <summary>Gets the drag source.</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <returns>The <see cref="DragSourceBase"/>.</returns>
        public static DragSourceBase GetDragSource(System.Windows.DependencyObject depObj)
        {
            return depObj.GetValue(DragSourceProperty) as DragSourceBase;
        }

        /// <summary>Sets the drag source.</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <param name="isSet">if set to <c>true</c> [is set].</param>
        public static void SetDragSource(System.Windows.DependencyObject depObj, bool isSet)
        {
            depObj.SetValue(DragSourceProperty, isSet);
        }

        #endregion

        #region DropTarget

        /// <summary>The drop target property.</summary>
        public static readonly System.Windows.DependencyProperty DropTargetProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "DropTarget", 
                typeof(DropTargetBase), 
                typeof(DragDropManager), 
                new System.Windows.FrameworkPropertyMetadata(OnDropTargetChanged));

        /// <summary>The set drop target.</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <param name="isSet">The is set.</param>
        public static void SetDropTarget(System.Windows.DependencyObject depObj, bool isSet)
        {
            depObj.SetValue(DropTargetProperty, isSet);
        }

        /// <summary>The get drop target.</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <returns>The <see cref="DropTargetBase"/>.</returns>
        public static DropTargetBase GetDropTarget(System.Windows.DependencyObject depObj)
        {
            return depObj.GetValue(DropTargetProperty) as DropTargetBase;
        }

        #endregion

        #region IsVisualFor

        /// <summary>
        ///     IsVisualFor Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsVisualForProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "IsVisualFor", 
                typeof(System.Windows.Media.Visual), 
                typeof(DragDropManager), 
                new System.Windows.FrameworkPropertyMetadata(
                    null, 
                    System.Windows.FrameworkPropertyMetadataOptions.None, 
                    OnIsVisualForChanged));

        /// <summary>Gets the IsVisualFor property.  This attached property
        ///     indicates that the object is a visual to be displayed when the value is being dragged.
        ///     The value should have a drag source manager attached.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="Visual"/>.</returns>
        public static System.Windows.Media.Visual GetIsVisualFor(System.Windows.DependencyObject d)
        {
            return (System.Windows.Media.Visual)d.GetValue(IsVisualForProperty);
        }

        /// <summary>Sets the IsVisualFor property.  This attached property
        ///     indicates that the object is a visual to be displayed when the value is being dragged.
        ///     The value should have a drag source manager attached.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetIsVisualFor(System.Windows.DependencyObject d, System.Windows.Media.Visual value)
        {
            d.SetValue(IsVisualForProperty, value);
        }

        /// <summary>Handles changes to the IsVisualFor property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        /// <remarks>We need to let the value know that it has a visual. Or remove it.</remarks>
        private static void OnIsVisualForChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var iOld = (System.Windows.DependencyObject)e.OldValue;
                var iSource = GetDragSource(iOld);
                if (iSource != null)
                {
                    var iList = GetVisuals(iOld);
                    if (iList != null)
                    {
                        var iFound = (from i in iList where i.IsAlive == false || i.Target == d select i).ToArray();
                        foreach (var i in iFound)
                        {
                            iList.Remove(i);
                        }

                        if (iList.Count == 0)
                        {
                            iOld.ClearValue(VisualsProperty);
                        }
                    }
                }
            }

            if (e.NewValue != null)
            {
                var iNew = (System.Windows.DependencyObject)e.NewValue;
                var iSource = GetDragSource(iNew);
                if (iSource != null)
                {
                    var iList = GetVisuals(iNew);
                    if (iList == null)
                    {
                        iList = new System.Collections.Generic.List<System.WeakReference>();
                        SetVisuals(iNew, iList);
                    }

                    iList.Add(new System.WeakReference(d));
                }
            }
        }

        #endregion

        #region Visuals

        /// <summary>
        ///     Visuals Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty VisualsProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "Visuals", 
                typeof(System.Collections.Generic.List<System.WeakReference>), 
                typeof(DragDropManager), 
                new System.Windows.FrameworkPropertyMetadata(
                    (System.Collections.Generic.List<System.WeakReference>)null));

        /// <summary>Gets the Visuals property.  This dependency property
        ///     indicates the list of visuals that an object should use to display while dragging, instead of the acutal object
        ///     being dragged.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<System.WeakReference> GetVisuals(
            System.Windows.DependencyObject d)
        {
            return (System.Collections.Generic.List<System.WeakReference>)d.GetValue(VisualsProperty);
        }

        /// <summary>Sets the Visuals property.  This dependency property
        ///     indicates the list of visuals that an object should use to display while dragging, instead of the acutal object
        ///     being dragged.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetVisuals(
            System.Windows.DependencyObject d, System.Collections.Generic.List<System.WeakReference> value)
        {
            d.SetValue(VisualsProperty, value);
        }

        #endregion

        #region DragEffect

        /// <summary>
        ///     DragEffect Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty DragEffectProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "DragEffect", 
                typeof(System.Windows.Media.Effects.Effect), 
                typeof(DragDropManager), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.Media.Effects.Effect)null));

        /// <summary>Gets the DragEffect property.  This dependency property
        ///     indicates the effect that should be applied to the drag visual. This can be used to invert the color of a textblock
        ///     back
        ///     to black, if it white because of the selection.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="Effect"/>.</returns>
        public static System.Windows.Media.Effects.Effect GetDragEffect(System.Windows.DependencyObject d)
        {
            return (System.Windows.Media.Effects.Effect)d.GetValue(DragEffectProperty);
        }

        /// <summary>Sets the DragEffect property.  This dependency property
        ///     indicates the effect that should be applied to the drag visual.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetDragEffect(System.Windows.DependencyObject d, System.Windows.Media.Effects.Effect value)
        {
            d.SetValue(DragEffectProperty, value);
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether the sourceUI creates visual that is shown during drag,
        ///     or if it is the drop targer who has this repsonsibility. (soure UI is the default).
        /// </summary>
        /// <remarks>
        ///     Use the drop target if you want to have drag drop support accros multiple applications, source UI
        ///     is faster.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if [source creates drag visual]; otherwise, <c>false</c>.
        /// </value>
        public static bool SourceCreatesDragVisual
        {
            get
            {
                return fSourceCreatesDragVisual;
            }

            set
            {
                fSourceCreatesDragVisual = value;
            }
        }

        #endregion

        #region Property Change handlers

        /// <summary>Called when the drag source is changed.</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private static void OnDragSourceChanged(
            System.Windows.DependencyObject depObj, 
            System.Windows.DependencyPropertyChangedEventArgs args)
        {
            var sourceElt = depObj as System.Windows.UIElement;
            if (args.OldValue != null)
            {
                var advisor = args.OldValue as DragSourceBase;
                if (advisor.UsePreviewEvents)
                {
                    sourceElt.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
                    sourceElt.PreviewMouseMove -= DragSource_PreviewMouseMove;
                    sourceElt.PreviewMouseUp -= DragSource_PreviewMouseUp;
                }
                else
                {
                    sourceElt.MouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
                    sourceElt.MouseMove -= DragSource_PreviewMouseMove;
                    sourceElt.MouseUp -= DragSource_PreviewMouseUp;
                }
            }

            if (args.NewValue != null)
            {
                // Set the Drag source UI
                var advisor = args.NewValue as DragSourceBase;

                // advisor.SourceUI = sourceElt;                                                                 //don't do this, this can cause items to be kept in memory.  This is assigned dynamically as needed.
                if (advisor.UsePreviewEvents)
                {
                    sourceElt.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
                    sourceElt.PreviewMouseMove += DragSource_PreviewMouseMove;
                    sourceElt.PreviewMouseUp += DragSource_PreviewMouseUp;
                }
                else
                {
                    sourceElt.MouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;

                        // we use mousedown instead of MouseLeftButton cause mouseleftbutton is a direct event and doesn't except bubbling events which mousedown, mousemove,.. do support and which we want.
                    sourceElt.MouseMove += DragSource_PreviewMouseMove;
                    sourceElt.MouseUp += DragSource_PreviewMouseUp;
                }
            }
        }

        /// <summary>Called when [drop target changed].</summary>
        /// <param name="depObj">The dep obj.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private static void OnDropTargetChanged(
            System.Windows.DependencyObject depObj, 
            System.Windows.DependencyPropertyChangedEventArgs args)
        {
            var targetElt = depObj as System.Windows.UIElement;

            if (args.OldValue != null)
            {
                var advisor = args.OldValue as DropTargetBase;
                if (advisor.UsePreviewEvents)
                {
                    targetElt.PreviewDragEnter -= DropTarget_PreviewDragEnter;
                    targetElt.PreviewDragOver -= DropTarget_PreviewDragOver;
                    targetElt.PreviewDragLeave -= DropTarget_PreviewDragLeave;
                    targetElt.PreviewDrop -= DropTarget_PreviewDrop;
                }
                else
                {
                    targetElt.DragEnter -= DropTarget_PreviewDragEnter;
                    targetElt.DragOver -= DropTarget_PreviewDragOver;
                    targetElt.DragLeave -= DropTarget_PreviewDragLeave;
                    targetElt.Drop -= DropTarget_PreviewDrop;
                }
            }

            if (args.NewValue != null)
            {
                // Set the Drag source UI
                var advisor = args.NewValue as DropTargetBase;

                if (advisor.UsePreviewEvents)
                {
                    targetElt.PreviewDragEnter += DropTarget_PreviewDragEnter;
                    targetElt.PreviewDragOver += DropTarget_PreviewDragOver;
                    targetElt.PreviewDragLeave += DropTarget_PreviewDragLeave;
                    targetElt.PreviewDrop += DropTarget_PreviewDrop;
                }
                else
                {
                    targetElt.DragEnter += DropTarget_PreviewDragEnter;
                    targetElt.DragOver += DropTarget_PreviewDragOver;
                    targetElt.DragLeave += DropTarget_PreviewDragLeave;
                    targetElt.Drop += DropTarget_PreviewDrop;
                }

                targetElt.AllowDrop = true;
            }
            else
            {
                targetElt.AllowDrop = false;
            }
        }

        #endregion

        #region Drop target

        /// <summary>Handles the PreviewDrop event of the DropTarget control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        private static void DropTarget_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (fOverlayElt != null)
            {
                var iDropTarget = GetDropTarget(sender as System.Windows.DependencyObject);
                if (iDropTarget != null)
                {
                    try
                    {
                        if (UpdateEffects(sender, e) == false)
                        {
                            return;
                        }

                        var dropPoint = e.GetPosition(sender as System.Windows.UIElement);

                        // Calculate displacement for (Left, Top)
                        // Point offset = e.GetPosition(fOverlayElt);
                        dropPoint.X = dropPoint.X - fOffsetPoint.X + (PREVIEWADORNERBORDER / 2) - TOLERANCE;
                        dropPoint.Y = dropPoint.Y - fOffsetPoint.Y + (PREVIEWADORNERBORDER / 2) - TOLERANCE;

                        var iSource =
                            e.Data.GetData(JaStDev.HAB.Designer.Properties.Resources.DelayLoadFormat) as DragSourceBase;
                        if (iSource != null)
                        {
                            iSource.DelayLoad(e.Data);
                        }

                        iDropTarget.OnDropCompleted(e, dropPoint);
                        RemovePreviewAdorner();
                        fOffsetPoint = new System.Windows.Point(0, 0);
                        e.Handled = true;
                    }
                    finally
                    {
                        iDropTarget.TargetUI = null;

                            // reset, so that we don't keep stuff in mem unwanted (drop manager can be located at app level).
                    }
                }
            }
        }

        /// <summary>Handles the PreviewDragLeave event of the DropTarget control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        private static void DropTarget_PreviewDragLeave(object sender, System.Windows.DragEventArgs e)
        {
            var advisor = GetDropTarget(sender as System.Windows.DependencyObject);
            if (advisor != null)
            {
                try
                {
                    if (fOverlayElt != null)
                    {
                        if (fPrevAdorned == sender)
                        {
                            // don't try to remove the adornerlayer if it has already been rendered for another object.
                            RemovePreviewAdorner();
                        }

                        if (UpdateEffects(sender, e) == false)
                        {
                            if (advisor.UsePreviewEvents == false)
                            {
                                e.Handled = true;
                            }

                            return;
                        }

                        advisor.OnDragLeave(e); // let the droptarget do some extra stuff, when the drag enter starts.
                        e.Handled = true;
                    }
                }
                finally
                {
                    advisor.TargetUI = null;

                        // reset, so that we don't keep stuff in mem unwanted (drop manager can be located at app level).
                }
            }
        }

        /// <summary>Handles the PreviewDragOver event of the DropTarget control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        private static void DropTarget_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            var iDropTarget = GetDropTarget(sender as System.Windows.DependencyObject);

            if (iDropTarget != null)
            {
                if (UpdateEffects(sender, e) == false)
                {
                    if (iDropTarget.UsePreviewEvents == false)
                    {
                        e.Handled = true;
                    }

                    return;
                }

                // Update position of the preview Adorner
                var position = e.GetPosition(sender as System.Windows.UIElement);
                System.Diagnostics.Debug.Assert(position.X >= 0.0);

                iDropTarget.OnDragOver(ref position, e.Data);

                    // let the drop advisor know something is being dragged over it.
                fOverlayElt.Left = position.X - fOffsetPoint.X;
                fOverlayElt.Top = position.Y - fOffsetPoint.Y;

                e.Handled = true;
            }
        }

        /// <summary>Handles the PreviewDragEnter event of the DropTarget control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        private static void DropTarget_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            var iDropTarget = GetDropTarget(sender as System.Windows.DependencyObject);

            if (iDropTarget != null)
            {
                if (UpdateEffects(sender, e) == false)
                {
                    if (iDropTarget.UsePreviewEvents == false)
                    {
                        e.Handled = true;
                    }

                    return;
                }

                iDropTarget.TargetUI = (System.Windows.UIElement)sender; // drop target is variable
                iDropTarget.OnDragEnter(e); // let the droptarget do some extra stuff, when the drag enter starts.

                // Setup the preview Adorner if appropriate
                System.Windows.UIElement feedbackUI;
                if (SourceCreatesDragVisual == false)
                {
                    feedbackUI = iDropTarget.GetVisualFeedback(e.Data);
                }
                else
                {
                    feedbackUI = fCurrentDragSource.DragVisual;
                }

                fOffsetPoint = GetOffsetPoint(e.Data);
                CreatePreviewAdorner(iDropTarget.TargetUI, feedbackUI);
                e.Handled = true;
            }
        }

        #endregion

        #region Drag source event handlers

        /// <summary>Handles the PreviewMouseLeftButtonDown event of the DragSource control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private static void DragSource_PreviewMouseLeftButtonDown(
            object sender, 
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (fIsMouseDown == false && e.ChangedButton == System.Windows.Input.MouseButton.Left
                && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 1)
            {
                // this allows us to put several drag sources on top of each other.  The topmost dragsource sets this flag, preventing lower drag sources from steeling the handle.
                // Make this the new drag source
                var advisor = GetDragSource(sender as System.Windows.DependencyObject);
                advisor.SourceUI = (System.Windows.UIElement)sender;

                fCurrentDragSource = advisor;

                    // don't try to determin if the item IsDraggable, wait till the drag move is completed. This saves calculation and provides better drag experience.
                fDraggedElt = e.Source as System.Windows.UIElement;
                fDragStartPoint = e.GetPosition(GetTopContainer(fDraggedElt));

                fOffsetPoint = e.GetPosition(fDraggedElt);
                fIsMouseDown = true;
                System.Windows.Input.Mouse.Capture(advisor.SourceUI);

                    // capture the mouse asap, so we don't have strange drag behaviour
            }
        }

        /// <summary>The drag source_ preview mouse move.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void DragSource_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (fIsMouseDown)
            {
                var advisor = GetDragSource(sender as System.Windows.DependencyObject);

                if (fCurrentDragSource == advisor && IsDragGesture(e.GetPosition(GetTopContainer(fDraggedElt))))
                {
                    fIsMouseDown = false;
                    if (advisor.IsDraggable(e.Source as System.Windows.UIElement))
                    {
                        DragStarted(sender as System.Windows.UIElement, advisor);
                    }

                    fCurrentDragSource.SourceUI = null;

                        // this can also keep stuff in mem, cause the drag managers are usually at the level off the app's resources, so they stay in mem.
                    fCurrentDragSource = null; // reset these values, so nothing remains in memory.
                    fDraggedElt = null;
                }
            }
        }

        /// <summary>Handles the PreviewMouseUp event of the DragSource control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private static void DragSource_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            fIsMouseDown = false;
            if (fCurrentDragSource != null)
            {
                if (fCurrentDragSource.SourceUI.IsMouseCaptured)
                {
                    fCurrentDragSource.SourceUI.ReleaseMouseCapture();
                }

                fCurrentDragSource.SourceUI = null;

                    // this can also keep stuff in mem, cause the drag managers are usually at the level off the app's resources, so they stay in mem.
                fCurrentDragSource = null; // reset these values, so nothing remains in memory.
            }

            fDraggedElt = null;
        }

        /// <summary>The drag started.</summary>
        /// <param name="uiElt">The ui elt.</param>
        /// <param name="advisor">The advisor.</param>
        private static void DragStarted(System.Windows.UIElement uiElt, DragSourceBase advisor)
        {
            var data = advisor.GetDataObject(fDraggedElt);
            if (data != null)
            {
                // if the advisor doesn't give a data object, we can't start a drag.
                data.SetData("OffsetPoint", fOffsetPoint);
                var supportedEffects = advisor.SupportedEffects;
                try
                {
                    JaStDev.HAB.Designer.WindowMain.UndoStore.BeginUndoGroup(false);

                        // we put a drag in an undo group so cause a drag is a single maneuvre.
                    try
                    {
                        var effects = System.Windows.DragDrop.DoDragDrop(fDraggedElt, data, supportedEffects);

                            // Perform DragDrop
                        advisor.FinishDrag(fDraggedElt, effects);
                    }
                    finally
                    {
                        JaStDev.HAB.Designer.WindowMain.UndoStore.EndUndoGroup();
                    }
                }
                catch (System.Exception e)
                {
                    System.Windows.MessageBox.Show(
                        e.ToString(), 
                        "Drag operation failed", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                    JaStDev.LogService.Log.LogError("DragDropManager", e.ToString());
                }

                // Clean up
                RemovePreviewAdorner();
                System.Windows.Input.Mouse.Capture(null);
                fDraggedElt = null;
            }
        }

        #endregion
    }

    /// <summary>The mouse utilities.</summary>
    public class MouseUtilities
    {
        /// <summary>The get cursor pos.</summary>
        /// <param name="pt">The pt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        /// <summary>The screen to client.</summary>
        /// <param name="hwnd">The hwnd.</param>
        /// <param name="pt">The pt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ScreenToClient(System.IntPtr hwnd, ref Win32Point pt);

        /// <summary>The get mouse position.</summary>
        /// <param name="relativeTo">The relative to.</param>
        /// <returns>The <see cref="Point"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static System.Windows.Point GetMousePosition(System.Windows.Media.Visual relativeTo)
        {
            var mouse = new Win32Point();
            GetCursorPos(ref mouse);

            var presentationSource =
                (System.Windows.Interop.HwndSource)System.Windows.PresentationSource.FromVisual(relativeTo);
            if (presentationSource != null)
            {
                ScreenToClient(presentationSource.Handle, ref mouse);
            }
            else
            {
                throw new System.InvalidOperationException("Failed to find main application handle.");
            }

            var transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);

            var offset = transform.Transform(new System.Windows.Point(0, 0));

            return new System.Windows.Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }

        /// <summary>The win 32 point.</summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct Win32Point
        {
            /// <summary>The x.</summary>
            public readonly int X;

            /// <summary>The y.</summary>
            public readonly int Y;
        };
    }
}