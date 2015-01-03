// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapLink.cs" company="">
//   
// </copyright>
// <summary>
//   An item that can be displayed on a <see cref="MindMap" /> and that
//   represents a link between 2 <see cref="Neuron" /> s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// using System.Windows.Threading;
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /*
    * check: http://www.dev102.com/2009/05/25/creating-gapped-and-bulleted-shapes-in-wpfsilverlight/
    * for implementing bullets on the lines in WPF.
    * 
    */

    /// <summary>
    ///     An item that can be displayed on a <see cref="MindMap" /> and that
    ///     represents a link between 2 <see cref="Neuron" /> s.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For the link start or link end to be draggable, the items in the data
    ///         template need to have a tag value of 'start' or 'end'.
    ///     </para>
    ///     <para>
    ///         This class can be optimized if some calculations were cashed, and only
    ///         recalculated if invalidated by a change in the points. Best candiates are
    ///         the angle calculations.
    ///     </para>
    /// </remarks>
    [System.Xml.Serialization.XmlInclude(typeof(MindMapNeuron))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapNote))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapLink))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapCluster))]
    public class MindMapLink : MindMapItem, System.Xml.Serialization.IXmlSerializable
    {
        // don't implement iNeuronInfo for meaning ref cause this screws up the descriptio editor.
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMapLink"/> class.</summary>
        public MindMapLink()
        {
            fFromChanged = from_PropertyChanged;
            fToChanged = to_PropertyChanged;
        }

        #endregion

        /// <summary>
        ///     Destroys this instance, thereby also destroying the link between the 2
        ///     neurons while generating undo data.
        /// </summary>
        public void Destroy()
        {
            var iUndoData = new LinkUndoItem(Link, BrainAction.Removed);
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                Link.Destroy(); // this will indirectly also remove the item from the mindmap.
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(WindowMain.UndoStore.EndUndoGroup));

                    // we call async cause the Link.Destroy() raises an event which is handled async as well.
            }
        }

        /// <summary>The remove.</summary>
        internal void Remove()
        {
            if (Owner != null)
            {
                Owner.Items.Remove(this);
            }
        }

        #region Fields

        /// <summary>The f points.</summary>
        private System.Windows.Media.PointCollection fPoints;

        /// <summary>The f points as xaml.</summary>
        private string fPointsAsXaml;

                       // so we can load the points collection as a string, which is parsed when the points collection is used for the first time, in the UI thread. We can't create the collection in another thread, which happens during loading from file.

        /// <summary>The f to.</summary>
        private ulong fTo;

                      // fto and fFrom store the actual value in such a way that we are not dependent of any MindMap objects or neuron objects, just the reference, most flexible, best for storage.

        /// <summary>The f from.</summary>
        private ulong fFrom;

        /// <summary>The f to mind map item.</summary>
        private MindMapNeuron fToMindMapItem;

                              // stores the actual mindmap item that we link to, for fast access, not persisted.

        /// <summary>The f from mind map item.</summary>
        private MindMapNeuron fFromMindMapItem;

                              // stores the actual mindmap item that we link to, for fast access, not persisted.

        /// <summary>The f end angle.</summary>
        private double fEndAngle;

        /// <summary>The f start angle.</summary>
        private double fStartAngle;

        /// <summary>The f internal change.</summary>
        private bool fInternalChange;

                     // stores who changes the list of points, this object or someone else (the overlay).

        /// <summary>The f bounding rect.</summary>
        private System.Windows.Rect? fBoundingRect;

        /// <summary>The f center text pos.</summary>
        private System.Windows.Point fCenterTextPos;

        /// <summary>The f text pos offset.</summary>
        private System.Windows.Point fTextPosOffset;

        /// <summary>The f points visibility.</summary>
        private System.Windows.Visibility fPointsVisibility = System.Windows.Visibility.Visible;

        /// <summary>The f margin.</summary>
        private static double fMargin = 16; // the number of points to stay away from the border.

        /// <summary>The f from changed.</summary>
        private readonly System.ComponentModel.PropertyChangedEventHandler fFromChanged;

        /// <summary>The f to changed.</summary>
        private readonly System.ComponentModel.PropertyChangedEventHandler fToChanged;

        #endregion

        #region prop

        // to do: calculate and refresh the rect: when a point is changed, the rect needs to be recalculated.
        #region BoundingRect

        /// <summary>
        ///     Gets the rectangle that determins the boundery of the mindmaplink.
        /// </summary>
        public override System.Windows.Rect BoundingRect
        {
            get
            {
                if (fBoundingRect.HasValue == false)
                {
                    fBoundingRect = new System.Windows.Rect(StartPoint, EndPoint);
                    var iPoints = InternalPoints;
                    if (iPoints.Count > 2)
                    {
                        for (var i = 1; i < iPoints.Count - 1; i++)
                        {
                            fBoundingRect.Value.Union(iPoints[i]);
                        }
                    }
                }

                return fBoundingRect.Value;
            }
        }

        /// <summary>
        ///     Resets the bounding rect so that it can be recalculated if necessary.
        /// </summary>
        private void ResetBoundingRect()
        {
            fBoundingRect = null;
            OnPropertyChanged("BoundingRect");
        }

        #endregion

        #region Points

        /// <summary>
        ///     Gets/sets the list of points that represent this link.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The first and last item are fixed, and determined by the starting and
        ///         ending point.
        ///     </para>
        ///     <para>
        ///         Note: makes a clone of the points, otherwise the linke wont update
        ///         when this property value changes. Probably because the UI freezes it.
        ///     </para>
        /// </remarks>
        public System.Windows.Media.PointCollection Points
        {
            get
            {
                var iClone = InternalPoints.Clone();
                iClone.Freeze();
                return iClone; // we make a clone, otherwise the linke wont update when this property value changes.
            }

            set
            {
                OnPropertyChanging("Points", InternalPoints, value);
                fPoints = value;
                OnPropertyChanged("Points");
                ResetBoundingRect();
            }
        }

        /// <summary>
        ///     Gets the actual points, not a clone. Used for editing the points
        ///     directly.
        /// </summary>
        /// <remarks>
        ///     will also create the points, if this has not yet been done. This way,
        ///     we can create link objects in other threads but have the
        ///     <see cref="Points" /> list created in the ui thread (through 'changed'
        ///     callbacks).
        /// </remarks>
        internal System.Windows.Media.PointCollection InternalPoints
        {
            get
            {
                if (fPoints == null)
                {
                    if (fPointsAsXaml != null && fPointsAsXaml != "<Points />" && fPointsAsXaml != "<Point />")
                    {
                        // also skip an empty entry, otherwise we get an error.
                        var iReader = new System.Xml.XmlTextReader(new System.IO.StringReader(fPointsAsXaml));
                        fPoints = System.Windows.Markup.XamlReader.Load(iReader) as System.Windows.Media.PointCollection;
                        var iCenter = (InternalPoints.Count - 1) / 2;
                        CenterTextPos = new System.Windows.Point(
                            (fPoints[iCenter].X + fPoints[iCenter + 1].X) / 2, 

                            // when we load the points, we need to make certain that the center is recalculated.
                            (fPoints[iCenter].Y + fPoints[iCenter + 1].Y) / 2);
                        fPointsAsXaml = null;
                    }
                    else
                    {
                        fPoints = new System.Windows.Media.PointCollection();
                        if (fInternalChange == false)
                        {
                            // when this is true, InternalPoints is called from CalculatePoints, which would be a problem.
                            CalculatePoints();
                        }
                    }

                    fPoints.Changed += fPoints_Changed;
                }

                return fPoints;
            }

            set
            {
                if (fPoints != null)
                {
                    fPoints.Changed -= fPoints_Changed;
                }

                fPoints = value;
                fPoints.Changed += fPoints_Changed;
                ResetBoundingRect();
            }
        }

        #endregion

        #region PointsVisibility

        /// <summary>
        ///     Gets/sets the value that determins if the points shoulc be visible or
        ///     not. When we display an overlay, we hide the actual points and display
        ///     an overly line, whic provides different functionality. During this
        ///     operaiotn it is best to hide our line, to make it clearer.
        /// </summary>
        public System.Windows.Visibility PointsVisibility
        {
            get
            {
                return fPointsVisibility;
            }

            set
            {
                fPointsVisibility = value;
                OnPropertyChanged("PointsVisibility");
            }
        }

        #endregion

        /// <summary>
        ///     Gets the link that this object wraps.
        /// </summary>
        public Link Link { get; private set; }

        #region Margin

        /// <summary>
        ///     Gets/sets the margin that should be applied between the geometry of
        ///     the item and the link.
        /// </summary>
        /// <remarks>
        ///     This is a <see langword="static" /> because it should normally not be
        ///     changed, but this way, it is still possible
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public static double Margin
        {
            get
            {
                return fMargin;
            }

            set
            {
                fMargin = value;
            }
        }

        #endregion

        #region StartPoint

        /// <summary>
        ///     Gets the start point of the point collection, which is the center of
        ///     the 'from' neuron.
        /// </summary>
        /// <remarks>
        ///     This is a convenience prop for wpf so a custom images can be used to
        ///     indicate the End.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Windows.Point StartPoint
        {
            get
            {
                var iPoints = InternalPoints;
                if (iPoints != null && iPoints.Count > 0)
                {
                    return iPoints[0];
                }

                throw new System.InvalidOperationException();
            }
        }

        #endregion

        #region StartAngle

        /// <summary>
        ///     Gets the angle that should be applied to the start point in a
        ///     datatemplate so that it appears as if it is following the direction of
        ///     the link line. This is a calculated value.
        /// </summary>
        /// <remarks>
        ///     This value is the inverse of the angle between the 'start'
        ///     (centerpoint) and 'end'. This is done cause the arrow needs the
        ///     inverse value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double StartAngle
        {
            get
            {
                return fStartAngle;
            }

            internal set
            {
                if (value != fStartAngle)
                {
                    fStartAngle = value;
                    OnPropertyChanged("StartAngle");
                }
            }
        }

        #endregion

        #region EndPoint

        /// <summary>
        ///     Gets the End point of the point collection, which is the center of the
        ///     'to' neuron.
        /// </summary>
        /// <remarks>
        ///     This is a convenience prop for wpf so a custom images can be used to
        ///     indicate the End.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Windows.Point EndPoint
        {
            get
            {
                var iPoints = InternalPoints;
                if (iPoints != null && iPoints.Count > 0)
                {
                    return iPoints[iPoints.Count - 1];
                }

                throw new System.InvalidOperationException();
            }
        }

        #endregion

        #region EndAngle

        /// <summary>
        ///     Gets the angle that should be applied to the end point in a
        ///     datatemplate so that it appears as if it is following the direction of
        ///     the link line. This is a calculated value.
        /// </summary>
        /// <remarks>
        ///     This value is the inverse of the angle between the 'end' (centerpoint)
        ///     and 'start'. This is done cause the arrow needs the inverse value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double EndAngle
        {
            get
            {
                return fEndAngle;
            }

            internal set
            {
                if (value != fEndAngle)
                {
                    fEndAngle = value;
                    OnPropertyChanged("EndAngle");
                }
            }
        }

        #endregion

        #region To

        /// <summary>
        ///     Gets/sets the neuron id from where the link starts.
        /// </summary>
        /// <remarks>
        ///     Only used for streaming or setting up a link when not yet resolved.
        /// </remarks>
        public ulong To
        {
            get
            {
                return fTo;
            }

            set
            {
                if (ToMindMapItem != null)
                {
                    throw new System.InvalidOperationException(
                        "Please use the ToMindMapItem property when the link has been resolved for setting this value.");
                }

                OnPropertyChanging("To", fTo, value);
                fTo = value;
                if (Link != null)
                {
                    // when the object is being read from xml, link is not yet set, so this doesn't change the vals
                    Link.To = Brain.Current[value];
                }

                OnPropertyChanged("To");
            }
        }

        #endregion

        #region From

        /// <summary>
        ///     Gets/sets the neuron id to where the link goes.
        /// </summary>
        public ulong From
        {
            get
            {
                return fFrom;
            }

            set
            {
                if (FromMindMapItem != null)
                {
                    throw new System.InvalidOperationException(
                        "Please use the FromMindMapItem property when the link has been resoved for setting this value.");
                }

                OnPropertyChanging("From", fFrom, value);
                fFrom = value;
                if (Link != null)
                {
                    // when the object is being read from xml, link is not yet set, so this doesn't change the val
                    Link.From = Brain.Current[value];
                }

                OnPropertyChanged("From");
            }
        }

        #endregion

        #region ToMindMapItem

        /// <summary>
        ///     Gets/sets the mind map item from which this link starts.
        /// </summary>
        /// <remarks>
        ///     Only available when link is resolved. Use this property to change this
        ///     value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public MindMapNeuron ToMindMapItem
        {
            get
            {
                return fToMindMapItem;
            }

            set
            {
                if (fToMindMapItem != value)
                {
                    OnPropertyChanging("ToMindMapItem", fToMindMapItem, value);
                    UpdateToMindMapItem(value);
                    if (fToMindMapItem != null)
                    {
                        if (Link != null)
                        {
                            // when the object is being read from xml, link is not yet set, so this doesn't change the vals
                            Link.To = value.Item;
                        }

                        fTo = value.ItemID;
                    }
                    else if (Link != null)
                    {
                        Link.To = null;
                    }
                }
            }
        }

        /// <summary>The update to mind map item.</summary>
        /// <param name="value">The value.</param>
        internal void UpdateToMindMapItem(MindMapNeuron value)
        {
            if (fToMindMapItem != null)
            {
                fToMindMapItem.PropertyChanged -= fToChanged;
            }

            fToMindMapItem = value;
            if (fToMindMapItem != null)
            {
                fToMindMapItem.PropertyChanged += fToChanged;
                to_PropertyChanged(fToMindMapItem, new System.ComponentModel.PropertyChangedEventArgs("X"));

                    // we call this function to update the new pos
                fTo = value.ItemID;
            }
            else if (Link != null)
            {
                fTo = Neuron.EmptyId;
            }

            OnPropertyChanged("ToMindMapItem");
            OnPropertyChanged("To");
        }

        #endregion

        #region FromMindMapItem

        /// <summary>
        ///     Gets/sets the mind map item to which this link goes to.
        /// </summary>
        /// <remarks>
        ///     Only available when link is resolved. Use this property to change this
        ///     value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public MindMapNeuron FromMindMapItem
        {
            get
            {
                return fFromMindMapItem;
            }

            set
            {
                if (fFromMindMapItem != value)
                {
                    OnPropertyChanging("FromMindMapItem", fFromMindMapItem, value);
                    UpdateFromMindMapItem(value);
                    if (fFromMindMapItem != null)
                    {
                        if (Link != null)
                        {
                            // when the object is being read from xml, link is not yet set, so this doesn't change the vals
                            Link.From = value.Item;
                        }
                    }
                    else if (Link != null)
                    {
                        Link.From = null;
                    }
                }
            }
        }

        /// <summary>Updates the <see cref="From"/> part without triggering the undo
        ///     system. This is used as a callback to update a change in the network.</summary>
        /// <param name="value">The value.</param>
        internal void UpdateFromMindMapItem(MindMapNeuron value)
        {
            if (fFromMindMapItem != null)
            {
                fFromMindMapItem.PropertyChanged -= fFromChanged;
            }

            fFromMindMapItem = value;
            if (fFromMindMapItem != null)
            {
                fFromMindMapItem.PropertyChanged += fFromChanged;
                from_PropertyChanged(fToMindMapItem, new System.ComponentModel.PropertyChangedEventArgs("X"));

                    // we call this function to update the new pos
                fFrom = fFromMindMapItem.ItemID;
            }
            else if (Link != null)
            {
                fFrom = Neuron.EmptyId;
            }

            OnPropertyChanged("FromMindMapItem");
            OnPropertyChanged("From");
        }

        #endregion

        #region Meaning

        /// <summary>
        ///     Gets/sets the meaning of this relationship.
        /// </summary>
        /// <remarks>
        ///     This property is only known/usable when the link is resolved. It is
        ///     not stored in xml cause it is retrieved /assigned directly to the link
        ///     object.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Meaning
        {
            get
            {
                if (Link != null)
                {
                    return Link.Meaning;
                }

                throw new System.InvalidOperationException("Can't retrieve meaning, link not yet resolved.");
            }

            set
            {
                if (Link != null)
                {
                    if (Link.Meaning != value)
                    {
                        OnPropertyChanging("Meaning", Link.Meaning, value);
                        Link.Meaning = value;
                        OnPropertyChanged("Meaning");
                        OnPropertyChanged("MeaningInfo");
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("Can't assign a new meaning, link not yet resolved.");
                }
            }
        }

        /// <summary>Gets the meaning info.</summary>
        public NeuronData MeaningInfo
        {
            get
            {
                if (Link != null)
                {
                    return BrainData.Current.NeuronInfo[Link.MeaningID];
                }

                return null;
            }
        }

        #endregion

        #region TextPos

        /// <summary>
        ///     Gets/sets the position of the meaning and info of the link.
        /// </summary>
        public System.Windows.Point TextPos
        {
            get
            {
                return new System.Windows.Point(CenterTextPos.X + TextPosOffset.X, CenterTextPos.Y + TextPosOffset.Y);
            }
        }

        #endregion

        #region CenterTextPos

        /// <summary>
        ///     Gets/sets the center position for the text point. This is the normal
        ///     position, which can still be ofsset (through a drag), which is
        ///     added/substracted from this value.
        /// </summary>
        public System.Windows.Point CenterTextPos
        {
            get
            {
                return fCenterTextPos;
            }

            set
            {
                fCenterTextPos = value;
                OnPropertyChanged("CenterTextPos");
                OnPropertyChanged("TextPos");
            }
        }

        #endregion

        #region TextPosOffset

        /// <summary>
        ///     Gets/sets the offset that should be applied to the center text pos, to
        ///     get the final position. This property allows us to move the text,
        ///     relative to it's center position.
        /// </summary>
        public System.Windows.Point TextPosOffset
        {
            get
            {
                return fTextPosOffset;
            }

            set
            {
                fTextPosOffset = value;
                OnPropertyChanged("TextPosOffset");
                OnPropertyChanged("TextPos");
            }
        }

        #endregion

        #region Description Title

        /// <summary>
        ///     Gets the description title.
        /// </summary>
        /// <value>
        ///     The description title.
        /// </value>
        public override string DescriptionTitle
        {
            get
            {
                return "Mindmap link";
            }
        }

        #endregion

        /// <summary>
        ///     this is used to determin a custom ordening for saving the items in the
        ///     list. This is used to make certain that link objects are at the back
        ///     of the list (so that they can load properly).
        /// </summary>
        /// <value>
        ///     The index of the type.
        /// </value>
        protected internal override int TypeIndex
        {
            get
            {
                return int.MaxValue;
            }
        }

        #endregion

        #region functions

        /// <summary>Finds the link object and registers the event handlers so we can
        ///     monitor a move of objets</summary>
        /// <param name="list">The list.</param>
        /// <returns>True if the operation succeeded, otherwise false.</returns>
        internal bool ResolveLink(MindMapItemCollection list)
        {
            if (fToMindMapItem != null)
            {
                fToMindMapItem.PropertyChanged -= fToChanged;
            }

            if (fFromMindMapItem != null)
            {
                fFromMindMapItem.PropertyChanged -= fFromChanged;
            }

            fToMindMapItem = null;
            fFromMindMapItem = null;
            foreach (var i in list)
            {
                var iNeuron = i as MindMapNeuron;
                if (iNeuron != null)
                {
                    if (iNeuron.ItemID == fTo && fToMindMapItem == null)
                    {
                        // don't want double event handlers.
                        iNeuron.PropertyChanged += fToChanged;
                        fToMindMapItem = iNeuron;
                    }
                    else if (iNeuron.ItemID == fFrom && fFromMindMapItem == null)
                    {
                        iNeuron.PropertyChanged += fFromChanged;
                        fFromMindMapItem = iNeuron;
                    }

                    if (fFromMindMapItem != null && fToMindMapItem != null)
                    {
                        break;
                    }
                }
            }

            if (fFromMindMapItem != null && fToMindMapItem != null)
            {
                if (fFromMindMapItem.Item.LinksOutIdentifier != null)
                {
                    using (var iLinks = fFromMindMapItem.Item.LinksOut) Link = (from i in iLinks where i.ToID == fTo select i).FirstOrDefault();
                }
                else
                {
                    Link = null;
                }

                if (Link == null)
                {
                    var iFromId = fFromMindMapItem != null ? fFromMindMapItem.ItemID : 0;
                    var iToId = fToMindMapItem != null ? fFromMindMapItem.ItemID : 0;
                    LogService.Log.LogWarning(
                        "MindMapLink.ResolveLink", 
                        string.Format("failed to find link from '{0}', to '{1}'!", iFromId, iToId));
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes any event handlers from the to and from mind map items +
        ///     resets the link and mindmap item references.
        /// </summary>
        internal void DisconnectLink()
        {
            if (fFromMindMapItem != null)
            {
                fFromMindMapItem.PropertyChanged -= fFromChanged;
            }

            if (fToMindMapItem != null)
            {
                fToMindMapItem.PropertyChanged -= fToChanged;
            }

            fFromMindMapItem = null;
            fToMindMapItem = null;
            Link = null;
        }

        /// <summary>The duplicate.</summary>
        /// <returns>A deep copy of this object.</returns>
        public override MindMapItem Duplicate()
        {
            var iRes = (MindMapLink)base.Duplicate();
            iRes.fPoints = new System.Windows.Media.PointCollection(InternalPoints);
            iRes.fTo = fTo;
            iRes.fFrom = fFrom;
            return iRes;
        }

        /// <summary>Sets up everything so that the <paramref name="link"/> can be
        ///     displayed between the specified 2 points + generates undo data for the
        ///     event.</summary>
        /// <param name="link">The link object between the 2 neurons.</param>
        /// <param name="from">The mindmap item that represents the from part.</param>
        /// <param name="to">The mind map item that represents the to part.</param>
        internal void CreateLink(Link link, MindMapNeuron from, MindMapNeuron to)
        {
            fTo = link.ToID;
            fFrom = link.FromID;
            Link = link;
            fToMindMapItem = to;
            fFromMindMapItem = from;
            CalculatePoints();

            fFromMindMapItem.PropertyChanged += fFromChanged;
            fToMindMapItem.PropertyChanged += fToChanged;
        }

        /// <summary>
        ///     Calculates the points for the line, based on the location of the
        ///     mindmap neurons that it connects, to provide an initial state.
        /// </summary>
        private void CalculatePoints()
        {
            var iStart = new System.Windows.Point(
                fFromMindMapItem.X + (fFromMindMapItem.Width / 2), 
                fFromMindMapItem.Y + (fFromMindMapItem.Height / 2));
            var iEnd = new System.Windows.Point(
                fToMindMapItem.X + (fToMindMapItem.Width / 2), 
                fToMindMapItem.Y + (fToMindMapItem.Height / 2));
            var iStartAngle = Helper.GetAnlge(iStart, iEnd);
            var iEndAngle = Helper.GetAnlge(iEnd, iStart);

            fInternalChange = true;
            try
            {
                InternalPoints.Clear();
                InternalPoints.Add(GetGeometryPoint(fFromMindMapItem, iStartAngle));
                InternalPoints.Add(GetGeometryPoint(fToMindMapItem, iEndAngle));
                StartAngle = 360 - iStartAngle; // the arrow in the UI acutally needs the inverse value.
                EndAngle = 360 - iEndAngle;

                CenterTextPos = new System.Windows.Point(
                    (InternalPoints[0].X + InternalPoints[1].X) / 2, 

                    // 25+.. -> we need to put the text above th line a little
                    (InternalPoints[0].Y + InternalPoints[1].Y) / 2);
            }
            finally
            {
                fInternalChange = false;
            }
        }

        /// <summary>whenever the mindmaap neurom moves, we need to move as well.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>Note: this function is also called to change the start when the to
        ///     neuron is changed.</remarks>
        private void to_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ToChanged));        //we call async, cause otherwise, the angle doesn't always get calculated correctly, this solves some, not always.
            if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width"
                || e.PropertyName == "Height")
            {
                ToChanged();
            }
        }

        /// <summary>The to changed.</summary>
        private void ToChanged()
        {
            if (Link != null)
            {
                fInternalChange = true;
                try
                {
                    var iEnd = new System.Windows.Point(
                        ToMindMapItem.X + (ToMindMapItem.Width / 2), 
                        ToMindMapItem.Y + (ToMindMapItem.Height / 2));
                    System.Windows.Point iStart;
                    if (InternalPoints.Count == 2)
                    {
                        iStart = new System.Windows.Point(
                            FromMindMapItem.X + (FromMindMapItem.Width / 2), 
                            FromMindMapItem.Y + (FromMindMapItem.Height / 2));
                        var iStartAngle = Helper.GetAnlge(iStart, iEnd);
                        InternalPoints[0] = GetGeometryPoint(FromMindMapItem, iStartAngle);
                        StartAngle = 360 - iStartAngle; // the arrow in the UI acutally needs the inverse value.
                    }
                    else
                    {
                        iStart = InternalPoints[InternalPoints.Count - 2];
                    }

                    var iEndAngle = Helper.GetAnlge(iEnd, iStart);
                    InternalPoints[InternalPoints.Count - 1] = GetGeometryPoint(ToMindMapItem, iEndAngle);
                    EndAngle = 360 - iEndAngle;
                }
                finally
                {
                    fInternalChange = false;
                }
            }
        }

        /// <summary>whenever the mindmaap neurom moves, we need to move as well.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>Note: this function is also called to change the start when the from
        ///     neuron is changed.</remarks>
        private void from_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(FromChanged));        //we call async, cause otherwise, the angle doesn't always get calculated correctly, this solves some, not always.
            if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width"
                || e.PropertyName == "Height")
            {
                FromChanged();
            }
        }

        /// <summary>The from changed.</summary>
        private void FromChanged()
        {
            if (Link != null)
            {
                fInternalChange = true;
                try
                {
                    System.Windows.Point iEnd;
                    var iStart = new System.Windows.Point(
                        FromMindMapItem.X + (FromMindMapItem.Width / 2), 
                        FromMindMapItem.Y + (FromMindMapItem.Height / 2));

                    if (InternalPoints.Count == 2)
                    {
                        iEnd = new System.Windows.Point(
                            ToMindMapItem.X + (ToMindMapItem.Width / 2), 
                            ToMindMapItem.Y + (ToMindMapItem.Height / 2));
                        var iEndAngle = Helper.GetAnlge(iEnd, iStart);
                        InternalPoints[1] = GetGeometryPoint(ToMindMapItem, iEndAngle);
                        EndAngle = 360 - iEndAngle;
                    }
                    else
                    {
                        iEnd = InternalPoints[1];
                    }

                    var iStartAngle = Helper.GetAnlge(iStart, iEnd);
                    InternalPoints[0] = GetGeometryPoint(FromMindMapItem, iStartAngle);
                    StartAngle = 360 - iStartAngle; // the arrow in the UI acutally needs the inverse value.
                }
                finally
                {
                    fInternalChange = false;
                }
            }
        }

        /// <summary>If the first, last or the one after first/before last is changed, we
        ///     need to update. For first and last: the startPoint and<see cref="EndPoint"/> props, for the others, we need to calculate the
        ///     rotation that needs to be applied to any object so that it looks as if
        ///     it follows the line.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>Unfortunatly, we don't get info on which item is changed, so update
        ///     all.</remarks>
        private void fPoints_Changed(object sender, System.EventArgs e)
        {
            if (fInternalChange == false)
            {
                ToChanged();
                FromChanged();
            }

            if (InternalPoints.Count > 1)
            {
                var iCenter = (InternalPoints.Count - 1) / 2;
                CenterTextPos =
                    new System.Windows.Point(
                        (InternalPoints[iCenter].X + InternalPoints[iCenter + 1].X) / 2, 

                        // 25+.. -> we need to put the text above th line a little
                        (InternalPoints[iCenter].Y + InternalPoints[iCenter + 1].Y) / 2);
            }

            OnPropertyChanged("StartPoint");
            OnPropertyChanged("EndPoint");
            OnPropertyChanged("Points");
            ResetBoundingRect();
        }

        /// <summary>Gets the point that is located at the specified<paramref name="angle"/> relative to the top of the neuron.</summary>
        /// <remarks><para>All mindmap neurons defines a path that represents their outer border.
        ///         if the start of this path would be at the <paramref name="angle"/> 0
        ///         degrees, this function returns the point at the specified angle.</para>
        /// <para>There is no direct function to do this, so instead we create a temp
        ///         list of points at a fixed distance of each other (360 -&gt; 1 for each
        ///         degree), calculate the <paramref name="angle"/> of this point and we
        ///         return find the closest match.</para>
        /// </remarks>
        /// <param name="item"></param>
        /// <param name="angle"></param>
        /// <returns>The <see cref="Point"/>.</returns>
        private System.Windows.Point GetGeometryPoint(MindMapNeuron item, double angle)
        {
            EdgePoint iResult = null;
            var iResDif = 0.0;

            foreach (var i in item.EdgePoints)
            {
                if (iResult != null)
                {
                    var iNewDif = System.Math.Abs(angle - i.Angle);
                    if (iNewDif < iResDif)
                    {
                        iResDif = iNewDif;
                        iResult = i;
                    }
                }
                else
                {
                    iResult = i;
                    iResDif = System.Math.Abs(angle - i.Angle);
                }
            }

            return iResult.Point;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iZIndex = 0;
            if (XmlStore.TryReadElement(reader, "ZIndex", ref iZIndex))
            {
                ZIndex = iZIndex;
            }

            reader.ReadStartElement("To");
            To = ulong.Parse(reader.ReadString());
            reader.ReadEndElement();

            reader.ReadStartElement("From");
            From = ulong.Parse(reader.ReadString());
            reader.ReadEndElement();

            if (reader.Name == "Description")
            {
                // if the tag is 'description', there is no actual data, so read the end tag to advance.
                reader.Read();
            }
            else
            {
                DescriptionText = reader.ReadOuterXml();
            }

            fPointsAsXaml = reader.ReadOuterXml();

            if (reader.Name == "TextOffset")
            {
                reader.Read(); // need to advance to behind the textoffset.
                fTextPosOffset.X = XmlStore.ReadElement<double>(reader, "X");
                fTextPosOffset.Y = XmlStore.ReadElement<double>(reader, "Y");
                reader.ReadEndElement();
            }

            var iTemp = 0.0;
            if (XmlStore.TryReadElement(reader, "Angle", ref iTemp))
            {
                fEndAngle = iTemp;
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // writer.WriteAttributeString("xsi:type","MindMapLink");                                 //need to identify this neuron for the mindmap.
            XmlStore.WriteElement<double>(writer, "ZIndex", ZIndex);

            writer.WriteStartElement("To");
            writer.WriteString(To.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("From");
            writer.WriteString(From.ToString());
            writer.WriteEndElement();

            if (DescriptionText != null)
            {
                writer.WriteRaw(DescriptionText);
            }
            else
            {
                writer.WriteStartElement("Description");
                writer.WriteEndElement();
            }

            if (fPoints != null)
            {
                System.Windows.Markup.XamlWriter.Save(fPoints, writer);
            }
            else
            {
                writer.WriteStartElement("Points");
                writer.WriteEndElement();
            }

            writer.WriteStartElement("TextOffset");
            XmlStore.WriteElement(writer, "X", TextPosOffset.X);
            XmlStore.WriteElement(writer, "Y", TextPosOffset.Y);
            writer.WriteEndElement();

            XmlStore.WriteElement(writer, "Angle", EndAngle);

                // we store this, so we don't need to recalculate it when we open the view (we also don't recalculate the start and endpoints).
        }

        #endregion
    }
}