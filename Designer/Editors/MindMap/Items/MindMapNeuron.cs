// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   An item that can be displayed on a <see cref="MindMap" /> and that
//   represents a <see cref="Neuron" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An item that can be displayed on a <see cref="MindMap" /> and that
    ///     represents a <see cref="Neuron" /> .
    /// </summary>
    public class MindMapNeuron : PositionedMindMapItem, INeuronInfo, INeuronWrapper, System.Windows.IWeakEventListener
    {
        #region fields

        /// <summary>The f item.</summary>
        private Neuron fItem;

        /// <summary>The f item data.</summary>
        private NeuronData fItemData;

        /// <summary>The f child count.</summary>
        private int fChildCount;

        /// <summary>The f edge points.</summary>
        private System.Collections.Generic.List<EdgePoint> fEdgePoints; // stores the edgepoints calculated on the shape.

        /// <summary>The f outliner.</summary>
        private static NeuronToResourceConverter fOutliner;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMapNeuron"/> class.</summary>
        /// <param name="item">The item.</param>
        /// <remarks>This is a <see langword="private"/> function so that the Create
        ///     function needs to be used.</remarks>
        internal MindMapNeuron(Neuron item)
        {
            Item = item;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MindMapNeuron" /> class.
        /// </summary>
        /// <remarks>
        ///     This constructor is provided for streaming, should be fixed later on
        /// </remarks>
        public MindMapNeuron()
        {
        }

        /// <summary>The create for.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="MindMapNeuron"/>.</returns>
        public static MindMapNeuron CreateFor(Neuron item)
        {
            if (item is NeuronCluster)
            {
                return new MindMapCluster((NeuronCluster)item);
            }

            return new MindMapNeuron(item);
        }

        #endregion

        #region prop

        #region NeuronInfo

        /// <summary>
        ///     Gets the extra info stored about the
        ///     <see cref="JaStDev.HAB.Designer.MindMapNeuron.Item" /> .
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronData NeuronInfo
        {
            get
            {
                if (fItemData == null && Brain.Current != null && fItem != null)
                {
                    fItemData = BrainData.Current.NeuronInfo[fItem.ID];
                    if (fItemData != null)
                    {
                        System.ComponentModel.PropertyChangedEventManager.AddListener(
                            fItemData, 
                            this, 
                            string.Empty);
                    }
                }

                return fItemData;
            }

            internal set
            {
                fItemData = value;
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets/sets the neuron that this mind map item represents.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public virtual Neuron Item
        {
            get
            {
                return fItem;
            }

            set
            {
                if (fItemData != null)
                {
                    System.ComponentModel.PropertyChangedEventManager.RemoveListener(
                        fItemData, 
                        this, 
                        string.Empty);
                }

                OnPropertyChanging("Item", fItem, value);
                fItem = value;
                fItemData = null;
                fEdgePoints = null;
                OnPropertyChanged("Item");
                OnPropertyChanged("Description");

                    // whenever ItemData changes, these 2 props are also triggered, cause we use the global data.
                OnPropertyChanged("DescriptionTitle");
            }
        }

        #endregion

        #region ItemId

        /// <summary>
        ///     Gets/sets the id of the neuron that this item represents.
        /// </summary>
        /// <remarks>
        ///     This is primarely provided so we can easely stream this info.
        /// </remarks>
        [System.Xml.Serialization.XmlElement("Neuron")]
        public ulong ItemID
        {
            get
            {
                if (fItem != null)
                {
                    return fItem.ID;
                }

                return Neuron.EmptyId;
            }

            set
            {
                if (value != Neuron.EmptyId)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(value, out iFound))
                    {
                        Item = iFound;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MindMapNeuron.ItemID", 
                            string.Format("No neuron found with id: {0}", value));
                        ProjectManager.Default.DataError = true;
                        Item = null;
                    }
                }
                else
                {
                    Item = null;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the converter to retrieve an outline for a neuron.
        /// </summary>
        /// <value>
        ///     The outliner.
        /// </value>
        public static NeuronToResourceConverter Outliner
        {
            get
            {
                if (fOutliner == null)
                {
                    fOutliner = WindowMain.Current.FindResource("NeuronToOutlineConv") as NeuronToResourceConverter;
                }

                return fOutliner;
            }
        }

        #region Outline

        /// <summary>
        ///     Gets/sets the outline of this neuron. This is used to calculate the
        ///     edge for placing links.
        /// </summary>
        /// <remarks>
        ///     This is just the basic outline, it still needs to be adjusted in size
        ///     and offset, to correspond to the actual object.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public virtual System.Windows.Media.Geometry Outline
        {
            get
            {
                System.Windows.Media.Geometry iRes = null;
                var iFound = Outliner.Convert(Item, typeof(System.Windows.Media.Geometry), null, null) as string;
                if (iFound != null)
                {
                    iRes = System.Windows.Media.Geometry.Parse(iFound);
                }

                if (iRes == null)
                {
                    iRes = new System.Windows.Media.PathGeometry();
                }

                return iRes;
            }
        }

        #endregion

        #region EdgePoints

        /// <summary>
        ///     Gets the list of all the edgepoints for this object.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<EdgePoint> EdgePoints
        {
            get
            {
                if (fEdgePoints == null)
                {
                    fEdgePoints = new System.Collections.Generic.List<EdgePoint>(360);
                    var iPath = Outline.GetOutlinedPathGeometry(1, System.Windows.Media.ToleranceType.Absolute);

                        // need the outline of the object.
                    var iOutLineBounds = iPath.Bounds;

                        // we need to know the size of the thing, so we can adjust it to the wanted size: we need to scale with a relative number to get from this rect to the width/height.
                    var iTransform = new System.Windows.Media.TransformGroup();

                        // need to scale it a bit (bigger) for the margin + adjust for size and offset.
                    iTransform.Children.Add(
                        new System.Windows.Media.TranslateTransform(-iOutLineBounds.X, -iOutLineBounds.Y));

                        // first make certain that the image is completely left/top, otherwise the stretch don't work properly
                    var iScale = new System.Windows.Media.ScaleTransform(
                        Width / iOutLineBounds.Width, 
                        Height / iOutLineBounds.Height);
                    iTransform.Children.Add(iScale);
                    iScale = new System.Windows.Media.ScaleTransform(
                        (Width + MindMapLink.Margin) / Width, 
                        (Height + MindMapLink.Margin) / Height);
                    iScale.CenterX = (Width + MindMapLink.Margin) / 2;
                    iScale.CenterY = (Height + MindMapLink.Margin) / 2;
                    iTransform.Children.Add(iScale);
                    iTransform.Children.Add(new System.Windows.Media.TranslateTransform(X, Y));

                    iPath.Transform = iTransform;

                    System.Windows.Point iTangent;
                    System.Windows.Point iFound;
                    iOutLineBounds = iPath.Bounds; // recalculate the value cause it has changed.
                    var iCenter = new System.Windows.Point(
                        iOutLineBounds.X + (iOutLineBounds.Width / 2), 
                        iOutLineBounds.Y + (iOutLineBounds.Height / 2));
                    for (var i = 0; i < 360; i++)
                    {
                        var iEdge = new EdgePoint();
                        fEdgePoints.Add(iEdge);
                        iPath.GetPointAtFractionLength(i / 360.0, out iFound, out iTangent);
                        iEdge.Point = iFound;
                        iEdge.Angle = Helper.GetAnlge(iCenter, iEdge.Point);
                    }
                }

                return fEdgePoints;
            }

            set
            {
                fEdgePoints = value;
                OnPropertyChanged("EdgePoints");
            }
        }

        #endregion

        #region Description

        /// <summary>
        ///     We always use the global description data for neurons, so we don't
        ///     store it and get the data from the braindata.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override System.Windows.Documents.FlowDocument Description
        {
            get
            {
                if (NeuronInfo != null)
                {
                    return NeuronInfo.Description;
                }

                return null;
            }

            set
            {
                if (NeuronInfo != null)
                {
                    NeuronInfo.Description = value;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the description as xaml text.
        /// </summary>
        /// <value>
        ///     The description text.
        /// </value>
        public override string DescriptionText
        {
            get
            {
                if (NeuronInfo != null)
                {
                    return NeuronInfo.DescriptionText;
                }

                return null;
            }

            set
            {
                if (NeuronInfo != null)
                {
                    NeuronInfo.DescriptionText = value;
                }
            }
        }

        #region DescriptionTitle

        /// <summary>
        ///     used as the header of the description editor frame.
        /// </summary>
        public override string DescriptionTitle
        {
            get
            {
                if (NeuronInfo != null && string.IsNullOrEmpty(NeuronInfo.DisplayTitle) == false)
                {
                    return NeuronInfo.DisplayTitle;
                }

                if (fItem != null)
                {
                    return fItem.ToString();
                }

                return null;
            }
        }

        #endregion

        #region ChildCount

        /// <summary>
        ///     Gets the nr of times this neuron is a child of a cluster.
        /// </summary>
        /// <remarks>
        ///     This property is automatically managed by the
        ///     <see cref="MindMapCluster" /> objects. It can be used in the UI to
        ///     indicate if an item is directly on the design surface or not.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public int ChildCount
        {
            get
            {
                return fChildCount;
            }

            internal set
            {
                fChildCount = value;
                OnPropertyChanged("ChildCount");
            }
        }

        #endregion

        #region X

        /// <summary>
        ///     Gets/sets the horizontal offset of the item on the graph.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children. need to reset
        ///     the edgepoints
        /// </remarks>
        /// <value>
        /// </value>
        public override double X
        {
            get
            {
                return base.X;
            }

            set
            {
                EdgePoints = null; // we must make certain that edgepoints are recalculated.
                base.X = value;
            }
        }

        #endregion

        #region Y

        /// <summary>
        ///     Gets/sets the vertical offset of the item on the graph.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children. need to reset
        ///     the edgepoints
        /// </remarks>
        /// <value>
        /// </value>
        public override double Y
        {
            get
            {
                return base.Y;
            }

            set
            {
                EdgePoints = null; // we must make certain that edgepoints are recalculated.
                base.Y = value;
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the height of the item.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children. need to reset
        ///     the edgepoints
        /// </remarks>
        /// <value>
        /// </value>
        public override double Height
        {
            get
            {
                return base.Height;
            }

            set
            {
                EdgePoints = null; // we must make certain that edgepoints are recalculated.
                base.Height = value;
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width of the item.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children. need to reset
        ///     the edgepoints
        /// </remarks>
        /// <value>
        ///     The width.
        /// </value>
        public override double Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                EdgePoints = null; // we must make certain that edgepoints are recalculated.
                base.Width = value;
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        Neuron INeuronWrapper.Item
        {
            get
            {
                return Item;
            }
        }

        #endregion

        #endregion

        #region Xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement<double>(writer, "Neuron", ItemID);
        }

        /// <summary>Reads the content of the XML file (all the properties)</summary>
        /// <param name="reader">The reader.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            ItemID = XmlStore.ReadElement<ulong>(reader, "Neuron");
        }

        #endregion

        #region Functions

        /// <summary>The duplicate.</summary>
        /// <returns>A deep copy of this object.</returns>
        public override MindMapItem Duplicate()
        {
            var iRes = (MindMapNeuron)base.Duplicate();
            iRes.fItem = fItem;
            return iRes;
        }

        /// <summary>Copies the field values.</summary>
        /// <param name="from">from where to copy the data.</param>
        internal void CopyValues(MindMapNeuron from)
        {
            X = from.X;
            Y = from.Y;
            Width = from.Width;
            Height = from.Height;
            ZIndex = from.ZIndex;
            ChildCount = from.ChildCount;
        }

        /// <summary>Assigns this neuron to all the mindmap <paramref name="clusters"/> in
        ///     the list.</summary>
        /// <param name="clusters">The clusters.</param>
        internal void AssignToClusters(System.Collections.Generic.IEnumerable<MindMapCluster> clusters)
        {
            foreach (var i in clusters)
            {
                using (var iList = i.Cluster.Children)
                    if (iList.Contains(Item))
                    {
                        i.MonitorChild(this);
                        i.Children.Add(this);
                    }
            }
        }

        /// <summary>
        ///     Unloads the UI data. Called when the editor is unloaded.
        /// </summary>
        internal void UnloadUIData()
        {
            fEdgePoints = null;
        }

        #endregion

        #region WeakEventListener

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public virtual bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.ComponentModel.PropertyChangedEventManager))
            {
                ItemData_PropertyChanged(sender, (System.ComponentModel.PropertyChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>need to pass along the prop change, cause this is where we get the
        ///     values for titel and description.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #endregion
    }
}