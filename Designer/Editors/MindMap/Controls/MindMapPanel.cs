// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapPanel.cs" company="">
//   
// </copyright>
// <summary>
//   displays a mindmap object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     displays a mindmap object.
    /// </summary>
    public class MindMapPanel : ScrollablePanel, System.Windows.IWeakEventListener
    {
        #region Fields

        /// <summary>The f required.</summary>
        private System.Windows.Size fRequired; // the size, required by the objects.

        /// <summary>The f selection box.</summary>
        private SelectionAdorner fSelectionBox; // so we know when we are drawing a selection box.

        #endregion

        #region Prop

        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(MindMap), 
                typeof(MindMapPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates which mindmap that needs to be displayed.
        /// </summary>
        public MindMap ItemsSource
        {
            get
            {
                return (MindMap)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ItemsSource"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemsSourceChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((MindMapPanel)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ItemsSource"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            InternalChildren.Clear();
            var iSource = e.OldValue as MindMap;
            if (iSource != null)
            {
                System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(iSource.Items, this);
                System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(iSource.SelectedItems, this);
                Data.EventManagers.PropertyChangingEventManager.RemoveListener(iSource, this);
            }

            iSource = e.NewValue as MindMap;
            if (iSource != null)
            {
                System.Collections.Specialized.CollectionChangedEventManager.AddListener(iSource.Items, this);
                System.Collections.Specialized.CollectionChangedEventManager.AddListener(iSource.SelectedItems, this);

                    // important: needs to be the actual list, not a temp generated, cause than it don't work (receive weak event won't know which list it comes from).
                Data.EventManagers.PropertyChangingEventManager.AddListener(iSource, this);
                BuildChildList();
            }
        }

        #endregion

        #region NeuronTemplate

        /// <summary>
        ///     <see cref="NeuronTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty NeuronTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "NeuronTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(MindMapPanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the <see cref="NeuronTemplate" /> property. This
        ///     dependency property indicates The template to use for neurons.
        /// </summary>
        public System.Windows.DataTemplate NeuronTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(NeuronTemplateProperty);
            }

            set
            {
                SetValue(NeuronTemplateProperty, value);
            }
        }

        #endregion

        #region ClusterTemplate

        /// <summary>
        ///     <see cref="ClusterTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ClusterTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "ClusterTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(MindMapPanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the <see cref="ClusterTemplate" /> property. This
        ///     dependency property indicates the template to use for clusters.
        /// </summary>
        public System.Windows.DataTemplate ClusterTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(ClusterTemplateProperty);
            }

            set
            {
                SetValue(ClusterTemplateProperty, value);
            }
        }

        #endregion

        #region TextTemplate

        /// <summary>
        ///     <see cref="TextTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty TextTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "TextTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(MindMapPanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the <see cref="TextTemplate" /> property. This dependency
        ///     property indicates the template to use for textneurons.
        /// </summary>
        public System.Windows.DataTemplate TextTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(TextTemplateProperty);
            }

            set
            {
                SetValue(TextTemplateProperty, value);
            }
        }

        #endregion

        #region LinkTemplate

        /// <summary>
        ///     <see cref="LinkTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LinkTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "LinkTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(MindMapPanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the <see cref="LinkTemplate" /> property. This dependency
        ///     property indicates the template to use for links.
        /// </summary>
        public System.Windows.DataTemplate LinkTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(LinkTemplateProperty);
            }

            set
            {
                SetValue(LinkTemplateProperty, value);
            }
        }

        #endregion

        #endregion

        #region overrides

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iXOffset = -HorizontalOffset;
            var iYOffset = -VerticalOffset;
            foreach (System.Windows.Controls.UserControl child in InternalChildren)
            {
                var iItem = child.Content as MindMapItem;
                var iOr = iItem.BoundingRect;
                System.Windows.Rect iRect;
                if (iItem is MindMapLink)
                {
                    iRect = new System.Windows.Rect(iOr.Size);

                        // mindmaplinks need to be drawn at 0,0 (left-top) since they have exact points, not relative points.
                }
                else
                {
                    iRect = new System.Windows.Rect(iOr.Location, iOr.Size);
                }

                iRect.Offset(iXOffset, iYOffset);
                child.Arrange(iRect);
            }

            return finalSize;
        }

        /// <summary>When overridden in a derived class, measures the size in layout
        ///     required for child elements and determines a size for the<see cref="System.Windows.FrameworkElement"/> -derived class.</summary>
        /// <param name="availableSize">The available size that this element can give to child elements.
        ///     Infinity can be specified as a value to indicate that the element will
        ///     size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on
        ///     its calculations of child element sizes.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var resultSize = new System.Windows.Size(0, 0);

            var iAvailable = new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity);

                // we allow for an infinite size.
            foreach (System.Windows.Controls.UserControl child in InternalChildren)
            {
                var iItem = child.Content as MindMapItem;
                iAvailable = iItem.BoundingRect.Size;
                child.Measure(iAvailable);
                resultSize.Width = System.Math.Max(resultSize.Width, child.DesiredSize.Width);
                resultSize.Height = System.Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? resultSize.Width : availableSize.Width;
            resultSize.Height = double.IsPositiveInfinity(availableSize.Height)
                                    ? resultSize.Height
                                    : availableSize.Height;
            return resultSize;
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.PreviewMouseDown"/> attached
        ///     routed event reaches an element in its route that is derived from this
        ///     class. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that one or more mouse buttons were pressed.</param>
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSource = e.OriginalSource as System.Windows.FrameworkElement;
            if (iSource is MindMapPanel)
            {
                // we check if the mouse event was done on the canvas used by the listbox, just to make certain we are not on something else.
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    var iMap = ItemsSource;
                    System.Diagnostics.Debug.Assert(iMap != null);
                    var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
                    System.Diagnostics.Debug.Assert(iLayer != null);
                    fSelectionBox = new SelectionAdorner(
                        System.Windows.Input.Mouse.GetPosition(this), 
                        this, 
                        iMap.ZoomInverse);
                    if (CaptureMouse())
                    {
                        iLayer.Add(fSelectionBox);
                        foreach (var i in Enumerable.ToList(iMap.SelectedItems))
                        {
                            // clear doesn't work, so do every item seperatly. need list copy, for the iterator to work.
                            i.IsSelected = false;
                        }
                    }
                }
            }
            else if (iSource != null)
            {
                var iItem = iSource.DataContext as MindMapItem;
                if (iItem != null)
                {
                    iItem.IsSelected = true;
                }
            }

            base.OnPreviewMouseDown(e);
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.PreviewMouseMove"/> attached
        ///     event reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (fSelectionBox != null)
            {
                fSelectionBox.EndPoint = System.Windows.Input.Mouse.GetPosition(this);
            }

            base.OnPreviewMouseMove(e);
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseUp"/> routed event reaches
        ///     an element in its route that is derived from this class. Implement
        ///     this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that the mouse button was released.</param>
        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (fSelectionBox != null)
            {
                ReleaseMouseCapture();
                var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
                System.Diagnostics.Debug.Assert(iLayer != null);
                iLayer.Remove(fSelectionBox);

                var iSelectArea = fSelectionBox.GetSelectArea();
                iSelectArea.Offset(HorizontalOffset, VerticalOffset);

                    // we need to adjust the rect for scrolling before calculating which items are within the frame.
                var iMap = ItemsSource;
                iMap.SelectedItems.DoGroupSelect = true;
                try
                {
                    foreach (var iItem in iMap.Items)
                    {
                        var i = iItem as PositionedMindMapItem;
                        if (i != null)
                        {
                            if (iSelectArea.Contains(new System.Windows.Rect(i.X, i.Y, i.Width, i.Height)))
                            {
                                i.IsSelected = true;
                            }
                        }
                    }
                }
                finally
                {
                    iMap.SelectedItems.DoGroupSelect = false;
                    fSelectionBox = null;
                }
            }

            base.OnMouseUp(e);
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.PreviewMouseWheel"/> attached
        ///     event reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <remarks>When control is pressed-&gt; zoom, otherwise scroll. Don't use preview,
        ///     than we can't scroll children anymore like comboboxes.</remarks>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var iMap = ItemsSource;
            System.Diagnostics.Debug.Assert(iMap != null);
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                // we do a zoom.
                if (e.Delta > 0)
                {
                    iMap.Zoom += iMap.Zoom * 0.1; // we make the zoom relative to the current value: 10 %
                }
                else
                {
                    iMap.Zoom -= iMap.Zoom * 0.1;
                }
            }
            else
            {
                iMap.VerScrollPos -= e.Delta / 3.0;
            }

            base.OnMouseWheel(e);
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            var iMap = ItemsSource;
            if (managerType == typeof(System.Collections.Specialized.CollectionChangedEventManager))
            {
                if (sender == iMap.Items)
                {
                    CollectionChanged((System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                }
                else if (sender == iMap.SelectedItems)
                {
                    SelectionChanged((System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                }
                else
                {
                    throw new System.InvalidOperationException("Internal error: Invalid source list");
                }

                return true;
            }

            if (managerType == typeof(Data.EventManagers.PropertyChangingEventManager))
            {
                MindMapPropertyChanged((UndoSystem.Interfaces.PropertyChangingEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Called when a prop of the mindmap or one of it's children has changed.
        ///     We use this to redraw the canvas when required.</summary>
        /// <param name="e">The <see cref="UndoSystem.Interfaces.PropertyChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void MindMapPropertyChanged(UndoSystem.Interfaces.PropertyChangingEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width"
                || e.PropertyName == "Height" || e.PropertyName == "Points")
            {
                InvalidateMeasure();
            }
        }

        /// <summary>Called when the Selection list has changed. Draw/remove selection -
        ///     resize boxes when the selection changes.</summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void SelectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddResizeAdorner((MindMapItem)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RemoveResizeAdorner((MindMapItem)e.OldItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RemoveResizeAdorner((MindMapItem)e.OldItems[0]);
                    AddResizeAdorner((MindMapItem)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearChildAdorners();
                    break;
                default:
                    break;
            }
        }

        /// <summary>Called when the items Collection changed.</summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void CollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddChild((MindMapItem)e.NewItems[0], -1);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var iToMove = InternalChildren[e.OldStartingIndex];
                    InternalChildren.RemoveAt(e.OldStartingIndex);
                    InternalChildren.Insert(e.NewStartingIndex, iToMove);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    InternalChildren.RemoveAt(e.OldStartingIndex);
                    CalculateRequiredSize();
                    CalculateScrollValues();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    InternalChildren.RemoveAt(e.NewStartingIndex);
                    CalculateRequiredSize();

                        // don't need to recalcualte the MaxScrolls, this is done by the add, this way, we don't do to much double work.
                    AddChild((MindMapItem)e.NewItems[0], e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    fRequired = new System.Windows.Size();
                    CalculateScrollValues();
                    InternalChildren.Clear();
                    break;
                default:
                    break;
            }
        }

        /// <summary>Adds a new ui <paramref name="item"/> for a single mindmap item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        private void AddChild(MindMapItem item, int index)
        {
            if (index > -1)
            {
                InternalChildren.Insert(index, GetMindMapUI(item));
            }
            else
            {
                InternalChildren.Add(GetMindMapUI(item));
            }

            var iRect = item.BoundingRect;
            if (fRequired.Height < iRect.Bottom)
            {
                fRequired.Height = iRect.Bottom;
            }

            if (fRequired.Width < iRect.Right)
            {
                fRequired.Width = iRect.Right;
            }

            CalculateScrollValues();
        }

        /// <summary>
        ///     Calculates both horizontal and vertical max scroll values.
        /// </summary>
        protected internal override void CalculateScrollValues()
        {
            var iMap = ItemsSource;
            if (iMap != null)
            {
                var iRequired =
                    new System.Windows.Size(
                        fRequired.Width * iMap.Zoom + ScrollAdded.Width * iMap.Zoom + BORDERSIZE, 
                        fRequired.Height * iMap.Zoom + ScrollAdded.Height * iMap.Zoom + BORDERSIZE);
                var iHeight = ActualHeight + BORDERSIZE;
                var iWidth = ActualWidth + BORDERSIZE;

                if (iRequired.Height < iHeight)
                {
                    SetVerticalMax(System.Math.Max(iMap.VerScrollPos, iHeight));
                }
                else
                {
                    SetVerticalMax(System.Math.Max(iMap.VerScrollPos, iRequired.Height));
                }

                if (iRequired.Width < iWidth)
                {
                    SetHorizontalMax(System.Math.Max(iMap.HorScrollPos, iWidth));
                }
                else
                {
                    SetHorizontalMax(System.Math.Max(iMap.HorScrollPos, iRequired.Width));
                }
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Builds the child list, based on the mindmap objects.
        /// </summary>
        private void BuildChildList()
        {
            var iHMax = 0.0;
            var iVMax = 0.0;

            var iMap = ItemsSource;
            foreach (var i in iMap.Items)
            {
                var iEl = GetMindMapUI(i);
                InternalChildren.Add(iEl);
                var iRect = i.BoundingRect;
                if (iRect.Right > iHMax)
                {
                    iHMax = iRect.Right;
                }

                if (iRect.Bottom > iVMax)
                {
                    iVMax = iRect.Bottom;
                }
            }

            fRequired = new System.Windows.Size(iHMax, iVMax);
            CalculateScrollValues();
        }

        /// <summary>
        ///     Calculates the size of the required space, based on the items in the
        ///     mindmap.
        /// </summary>
        private void CalculateRequiredSize()
        {
            var iHMax = 0.0;
            var iVMax = 0.0;
            var iMap = ItemsSource;
            foreach (var i in iMap.Items)
            {
                var iRect = i.BoundingRect;
                if (iRect.Right > iHMax)
                {
                    iHMax = iRect.Right;
                }

                if (iRect.Bottom > iVMax)
                {
                    iVMax = iRect.Bottom;
                }
            }

            fRequired = new System.Windows.Size(iHMax, iVMax);
        }

        /// <summary>Gets the UI element for the mind map item.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        private System.Windows.FrameworkElement GetMindMapUI(MindMapItem i)
        {
            System.Windows.Controls.UserControl iEl = null;
            if (i is MindMapNote)
            {
                iEl = new MindMapNoteView();
                iEl.DataContext = i;
            }
            else
            {
                iEl = new System.Windows.Controls.UserControl();
                if (i is MindMapLink)
                {
                    iEl.ContentTemplate = LinkTemplate;
                }
                else if (i is MindMapCluster)
                {
                    iEl.ContentTemplate = ClusterTemplate;
                }
                else if (i is MindMapNeuron)
                {
                    if (((MindMapNeuron)i).Item is TextNeuron)
                    {
                        iEl.ContentTemplate = TextTemplate;
                    }
                    else
                    {
                        iEl.ContentTemplate = NeuronTemplate;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }

            iEl.Content = i;
            iEl.DataContext = i;
            iEl.Focusable = true;
            iEl.PreviewMouseDown += Child_PreviewMouseDown;
            var iBind = new System.Windows.Data.Binding("ZIndex");

                // we need to make certain that the zindex works, since we have no style to assign to the elements.
            iBind.Source = i;
            iEl.SetBinding(ZIndexProperty, iBind);
            return iEl;
        }

        /// <summary>Handles the PreviewMouseDown event of the iEl control. We need to
        ///     focus the object.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private static void Child_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = (System.Windows.FrameworkElement)sender;
            iSender.Focus();
        }

        #region Adorners

        /// <summary>
        ///     Deactivates the overlays so that the drop advisor can properly work.
        /// </summary>
        internal void DeactivateAdorners()
        {
            var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (iLayer != null)
            {
                foreach (System.Windows.UIElement i in InternalChildren)
                {
                    var iAdorners = iLayer.GetAdorners(i);
                    if (iAdorners != null)
                    {
                        foreach (var u in iAdorners)
                        {
                            u.IsHitTestVisible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Activates the overlays so that the drop advisor can properly work.
        /// </summary>
        internal void ActivateAdorners()
        {
            var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (iLayer != null)
            {
                foreach (System.Windows.UIElement i in InternalChildren)
                {
                    var iAdorners = iLayer.GetAdorners(i);
                    if (iAdorners != null)
                    {
                        foreach (var u in iAdorners)
                        {
                            u.IsHitTestVisible = true;
                        }
                    }
                }
            }
        }

        /// <summary>Removes the resize adorner for the specified item.</summary>
        /// <param name="toRemove">The mind map item.</param>
        private void RemoveResizeAdorner(MindMapItem toRemove)
        {
            var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (iLayer != null)
            {
                var iMap = ItemsSource;
                var iControl = InternalChildren[iMap.Items.IndexOf(toRemove)];
                var iAdorners = iLayer.GetAdorners(iControl);
                if (iAdorners != null)
                {
                    foreach (var i in iAdorners)
                    {
                        RemoveAdorner(iLayer, i, toRemove); // this only removes if it is a resize adorner.
                    }
                }
            }
        }

        /// <summary>Removes a single <paramref name="adorner"/> from the<paramref name="layer"/> if it is an itemresizer or
        ///     linkoverlayresizer.</summary>
        /// <param name="layer">The layer.</param>
        /// <param name="adorner">The adorner.</param>
        /// <param name="item">The mindmap item that was adorned.</param>
        private void RemoveAdorner(
            System.Windows.Documents.AdornerLayer layer, 
            System.Windows.Documents.Adorner adorner, 
            MindMapItem item)
        {
            if (adorner is ItemResizeAdorner)
            {
                layer.Remove(adorner);
            }
            else
            {
                var iLinkOverlay = adorner as LinkOverlayAdorner;
                if (iLinkOverlay != null)
                {
                    layer.Remove(adorner);
                    iLinkOverlay.Clear();
                    ((MindMapLink)item).PointsVisibility = System.Windows.Visibility.Visible;

                        // has to be link, otherwise no linkoverlay
                }
            }
        }

        /// <summary>Adds an resize adorner to the item.</summary>
        /// <param name="toAdorn">To adorn.</param>
        private void AddResizeAdorner(MindMapItem toAdorn)
        {
            var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (iLayer != null)
            {
                var iMap = ItemsSource;
                System.Diagnostics.Debug.Assert(iMap != null);
                var iLink = toAdorn as MindMapLink;
                if (iLink != null)
                {
                    var iItem = InternalChildren[iMap.Items.IndexOf(toAdorn)] as System.Windows.Controls.UserControl;
                    System.Diagnostics.Debug.Assert(iItem != null);
                    var iOverlay = new LinkOverlayAdorner(iLink, iItem);
                    iLayer.Add(iOverlay);
                    iLink.PointsVisibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(toAdorn is PositionedMindMapItem);

                        // only know links and positional items.
                    var iItem = InternalChildren[iMap.Items.IndexOf(toAdorn)] as System.Windows.Controls.UserControl;
                    var iResizer = new ItemResizeAdorner((PositionedMindMapItem)toAdorn, iItem);
                    iLayer.Add(iResizer);
                }
            }
        }

        /// <summary>
        ///     Removes the resize adorners of all the child objects.
        /// </summary>
        private void ClearChildAdorners()
        {
            var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            if (iLayer != null)
            {
                foreach (System.Windows.Controls.UserControl i in InternalChildren)
                {
                    var iAdorners = iLayer.GetAdorners(i);
                    if (iAdorners != null)
                    {
                        foreach (var u in iAdorners)
                        {
                            RemoveAdorner(iLayer, u, (MindMapItem)i.Content);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}