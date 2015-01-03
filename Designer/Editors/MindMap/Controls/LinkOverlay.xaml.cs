// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkOverlay.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for LinkOverlay.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for LinkOverlay.xaml
    /// </summary>
    public partial class LinkOverlay : System.Windows.Controls.UserControl
    {
        /// <summary>The f internal edit.</summary>
        private bool fInternalEdit;

                     // keeps track if a change in the points collection was done by this class or not (for updating items).

        /// <summary>The f link.</summary>
        private MindMapLink fLink;

        /// <summary>The f menu opened at.</summary>
        private System.Windows.Point fMenuOpenedAt; // stores the location where the menu opened, used to add points.

        /// <summary>The f editable points.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<LinkOverlayItem> fEditablePoints =
            new System.Collections.ObjectModel.ObservableCollection<LinkOverlayItem>();

        /// <summary>Initializes a new instance of the <see cref="LinkOverlay"/> class.</summary>
        public LinkOverlay()
        {
            InitializeComponent();
        }

        #region EditablePoints

        /// <summary>
        ///     Gets the list of points that can be dragged around (who's values can
        ///     change) -> all but first and last.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<LinkOverlayItem> EditablePoints
        {
            get
            {
                return fEditablePoints;
            }
        }

        #endregion

        #region Link

        /// <summary>
        ///     Gets/sets the link for which we provide an overlay.
        /// </summary>
        /// <remarks>
        ///     This is required for editing it's properties and deleting it.
        /// </remarks>
        public MindMapLink ToControl
        {
            get
            {
                return fLink;
            }

            set
            {
                fLink = value;
                Points = fLink.InternalPoints;
                var iPos = fLink.TextPos;
                TextX = iPos.X - 12; // to get in the center of the text, it's a guess.
                TextY = iPos.Y + 4; // do offset of 4, to get it nicely in the middel (thumb is 8 in size).
            }
        }

        #endregion

        /// <summary>The thumb_ drag delta.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            fInternalEdit = true;
            try
            {
                var iMap = ToControl.Owner;
                var fItem = ((System.Windows.Controls.Primitives.Thumb)sender).DataContext as LinkOverlayItem;
                if (fItem != null)
                {
                    fItem.ChangeValues(e.HorizontalChange * iMap.ZoomInverse, e.VerticalChange * iMap.ZoomInverse);
                }
            }
            finally
            {
                fInternalEdit = false;
            }

            TextX = ToControl.TextPos.X - 4;

                // we update tte text pos when points are changed, since the center has changed and we want our thumb to remain close.
            TextY = ToControl.TextPos.Y - 4;
        }

        /// <summary>Inserts a point in the collection.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fInternalEdit = true;
            try
            {
                var iPoints = Points;
                var iInsertAt = iPoints.Count - 1; // default insert is just before last.

                // need to find where to insert. this is done by finding the 2 points that are just bigger and smaller.
                for (var i = 0; i < iPoints.Count - 1; i++)
                {
                    var iLeft = iPoints[i];
                    var iRight = iPoints[i + 1];
                    if ((iLeft.X <= fMenuOpenedAt.X && iRight.X >= fMenuOpenedAt.X && iLeft.Y <= fMenuOpenedAt.Y
                         && iRight.Y >= fMenuOpenedAt.Y)
                        || (iLeft.X >= fMenuOpenedAt.X && iRight.X <= fMenuOpenedAt.X && iLeft.Y >= fMenuOpenedAt.Y
                            && iRight.Y <= fMenuOpenedAt.Y))
                    {
                        iInsertAt = i + 1;
                        break;
                    }
                }

                iPoints.Insert(iInsertAt, fMenuOpenedAt);
                foreach (var i in fEditablePoints)
                {
                    if (i.Index >= iInsertAt)
                    {
                        i.Index++;
                    }
                }

                fEditablePoints.Add(new LinkOverlayItem(iInsertAt, iPoints));

                TextX = ToControl.TextPos.X - 4;

                    // we update tte text pos when points are changed, since the center has changed and we want our thumb to remain close.
                TextY = ToControl.TextPos.Y - 4;
            }
            finally
            {
                fInternalEdit = false;
            }
        }

        /// <summary>Remvoes the selected point from the list</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RemovePoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to get the LinkOverlayItem for the index nr of the point to remove.
            var iMenu =
                ControlFramework.Utility.TreeHelper.FindInTree<System.Windows.Controls.ContextMenu>(
                    sender as System.Windows.DependencyObject);
            if (iMenu != null)
            {
                var iThumb = iMenu.PlacementTarget as System.Windows.Controls.Primitives.Thumb;
                if (iThumb != null)
                {
                    var iItem = iThumb.DataContext as LinkOverlayItem;
                    if (iItem != null)
                    {
                        Points.RemoveAt(iItem.Index);
                        fEditablePoints.Remove(iItem);

                        TextX = ToControl.TextPos.X - 4;

                            // we update tte text pos when points are changed, since the center has changed and we want our thumb to remain close.
                        TextY = ToControl.TextPos.Y - 4;
                    }
                }
            }
        }

        /// <summary>Need to keep track of where the mouse was when the context menu
        ///     opened, for adding points.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            fMenuOpenedAt = System.Windows.Input.Mouse.GetPosition(MainContainer);
        }

        /// <summary>Handles the Click event of the ShowProp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ShowProp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iMap = ToControl.Owner;
                var iDlg = new DlgLink();
                iDlg.Owner = System.Windows.Window.GetWindow(this);
                iDlg.ToList =
                    (from i in iMap.Items
                     where i is MindMapNeuron
                     orderby ((MindMapNeuron)i).NeuronInfo.DisplayTitle
                     select (MindMapNeuron)i).ToList();

                    // extract all the neurons because we can make connections between those.
                iDlg.FromList = iDlg.ToList; // from list and to list is the same.
                iDlg.SelectedFrom = ToControl.FromMindMapItem;
                iDlg.SelectedTo = ToControl.ToMindMapItem;
                iDlg.SelectedMeaning = ToControl.Meaning;

                var iRes = iDlg.ShowDialog();
                if (iRes.HasValue && iRes.Value)
                {
                    WindowMain.UndoStore.BeginUndoGroup(true);

                        // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                    try
                    {
                        ToControl.FromMindMapItem = iDlg.SelectedFrom;
                        ToControl.ToMindMapItem = iDlg.SelectedTo; // guaranteed to be valid if dialog closed with ok.;
                        ToControl.Meaning = iDlg.SelectedMeaning;
                        
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception iEx)
            {
                var iMsg = iEx.ToString();
                System.Windows.MessageBox.Show(
                    "failed to update the link:\n" + iMsg, 
                    "Link", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Handles the Click event of the DeleteLink control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (fLink != null)
            {
                fLink.Destroy();
            }
            else
            {
                LogService.Log.LogError("LinkOverlay.DeleteLink_Click", "Internal error: no link assigned");
            }
        }

        /// <summary>Handles the Click event of the RemoveLink control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RemoveLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (fLink != null)
            {
                fLink.Remove();
            }
            else
            {
                LogService.Log.LogError("LinkOverlay.RemoveLink_Click", "Internal error: no link assigned");
            }
        }

        /// <summary>Handles the DragDelta event of the TextThumb control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event
        ///     data.</param>
        private void TextThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iMap = ToControl.Owner;
            var iLink = ToControl;
            iLink.TextPosOffset = new System.Windows.Point(
                iLink.TextPosOffset.X + e.HorizontalChange, 
                iLink.TextPosOffset.Y + e.VerticalChange);

                // we need to assign, can't do offset cause that doesn't trigger an  update.
            TextX += e.HorizontalChange;
            TextY += e.VerticalChange;
        }

        #region TextX

        /// <summary>
        ///     <see cref="TextX" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty TextXProperty =
            System.Windows.DependencyProperty.Register(
                "TextX", 
                typeof(double), 
                typeof(LinkOverlay), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the <see cref="TextX" /> property. This dependency
        ///     property indicates the X pos of the text decorator.
        /// </summary>
        public double TextX
        {
            get
            {
                return (double)GetValue(TextXProperty);
            }

            set
            {
                SetValue(TextXProperty, value);
            }
        }

        #endregion

        #region TextY

        /// <summary>
        ///     <see cref="TextY" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty TextYProperty =
            System.Windows.DependencyProperty.Register(
                "TextY", 
                typeof(double), 
                typeof(LinkOverlay), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the <see cref="TextY" /> property. This dependency
        ///     property indicates the Y pos of the text decorator.
        /// </summary>
        public double TextY
        {
            get
            {
                return (double)GetValue(TextYProperty);
            }

            set
            {
                SetValue(TextYProperty, value);
            }
        }

        #endregion

        #region Points

        /// <summary>
        ///     <see cref="Points" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PointsProperty =
            System.Windows.DependencyProperty.Register(
                "Points", 
                typeof(System.Windows.Media.PointCollection), 
                typeof(LinkOverlay), 
                new System.Windows.FrameworkPropertyMetadata(null, OnPointsChanged));

        /// <summary>
        ///     Gets or sets the <see cref="Points" /> property. This dependency
        ///     property indicates the list of points that we are providing an overlay
        ///     for.
        /// </summary>
        public System.Windows.Media.PointCollection Points
        {
            get
            {
                return (System.Windows.Media.PointCollection)GetValue(PointsProperty);
            }

            set
            {
                SetValue(PointsProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="Points"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnPointsChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((LinkOverlay)d).OnPointsChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="Points"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPointsChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fInternalEdit == false && e.NewValue != null)
            {
                fEditablePoints.Clear();
                var iNew = (System.Windows.Media.PointCollection)e.NewValue;
                for (var i = 1; i < iNew.Count - 1; i++)
                {
                    fEditablePoints.Add(new LinkOverlayItem(i, iNew));
                }
            }
        }

        #endregion
    }

    /// <summary>The link overlay adorner.</summary>
    public class LinkOverlayAdorner : System.Windows.Documents.Adorner
    {
        /// <summary>The f content.</summary>
        private LinkOverlay fContent;

        /// <summary>The visual children.</summary>
        private readonly System.Windows.Media.VisualCollection visualChildren;

        /// <summary>Initializes a new instance of the <see cref="LinkOverlayAdorner"/> class.</summary>
        /// <param name="toControl">The to control.</param>
        /// <param name="adornedElt">The adorned elt.</param>
        public LinkOverlayAdorner(MindMapLink toControl, System.Windows.UIElement adornedElt)
            : base(adornedElt)
        {
            visualChildren = new System.Windows.Media.VisualCollection(this);

            fContent = new LinkOverlay();
            fContent.ToControl = toControl;
            visualChildren.Add(fContent);
        }

        /// <summary>Gets the visual children count.</summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>The measure override.</summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            fContent.Measure(constraint);
            return fContent.DesiredSize;
        }

        /// <summary>The arrange override.</summary>
        /// <param name="finalSize">The final size.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            fContent.Arrange(new System.Windows.Rect(finalSize));
            return finalSize;
        }

        /// <summary>The get visual child.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="Visual"/>.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return fContent;
        }

        /// <summary>The clear.</summary>
        public void Clear()
        {
            fContent.Points = null;

                // need to remove this ref,otherwise it keeps getting updated, even if the overlay is gone.
            visualChildren.Clear();
            fContent = null;
        }
    }

    /// <summary>
    ///     A wrapper for a point in the link line that can be edited.s
    /// </summary>
    public class LinkOverlayItem : Data.ObservableObject
    {
        /// <summary>The f list.</summary>
        private readonly System.Windows.Media.PointCollection fList;

                                                              // the list that contains the points that need editing (backref).

        /// <summary>Initializes a new instance of the <see cref="LinkOverlayItem"/> class.</summary>
        /// <param name="index">The index.</param>
        /// <param name="list">The list.</param>
        public LinkOverlayItem(int index, System.Windows.Media.PointCollection list)
        {
            Index = index;
            fList = list;
        }

        /// <summary>
        ///     Gets/sets the index that this object encapsulates.
        /// </summary>
        public int Index { get; set; }

        #region X

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public double X
        {
            get
            {
                return fList[Index].X;
            }

            set
            {
                fList[Index] = new System.Windows.Point(value, Y);
                OnPropertyChanged("X");
            }
        }

        #endregion

        #region Y

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public double Y
        {
            get
            {
                return fList[Index].Y;
            }

            set
            {
                fList[Index] = new System.Windows.Point(X, value);
                OnPropertyChanged("Y");
            }
        }

        #endregion

        /// <summary>The change values.</summary>
        /// <param name="horChange">The hor change.</param>
        /// <param name="verChange">The ver change.</param>
        internal void ChangeValues(double horChange, double verChange)
        {
            var iPrev = fList[Index];
            fList[Index] = new System.Windows.Point(iPrev.X + horChange, iPrev.Y + verChange);
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
        }
    }
}