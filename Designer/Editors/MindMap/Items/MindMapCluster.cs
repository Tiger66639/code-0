// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapCluster.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="MindMapNeuron" /> specific for clusters. Provides some extra
//   functionality to handle children.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A <see cref="MindMapNeuron" /> specific for clusters. Provides some extra
    ///     functionality to handle children.
    /// </summary>
    public class MindMapCluster : MindMapNeuron
    {
        #region Children

        /// <summary>
        ///     Gets/sets the list of children that are displayed in this neuron.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This prop is formatted with a get + as list-ulong- for streaming. This
        ///         way, we can use the list as a lookup. Warning: this should be changed,
        ///         so that the list can't be modified from outside at runtime (only on
        ///         load of the data)
        ///     </para>
        ///     <para>
        ///         We don't need to make an observable list from this cause the children
        ///         are never really managed by this object, only referenced so we can
        ///         monitor changes in them -> however, we need to access this list of
        ///         child MindMapNeurons far to often, so best to keep a sub list.
        ///     </para>
        /// </remarks>
        public SmallIDCollection ChildrenIds
        {
            get
            {
                if (fChildren is SmallIDCollection)
                {
                    return (SmallIDCollection)fChildren;
                }

                var iList = from i in Children select i.ItemID;
                return new SmallIDCollection(iList.ToList());
            }

            set
            {
                if (fChildren == null)
                {
                    fChildren = value;

                        // we store this temporarely, so the first time that the children are accessed, the values are copied.  This is done cause the Owner prop is not yet set when the object is loading from xmla
                }
                else
                {
                    throw new System.InvalidOperationException("object already loaded, can't set childrenId's.");
                }
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children assigned to this cluster.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<MindMapNeuron> Children
        {
            get
            {
                if (fChildren is SmallIDCollection && Owner != null)
                {
                    PrepareChildren();
                }
                else if (fChildren == null)
                {
                    fChildren = new System.Collections.ObjectModel.ObservableCollection<MindMapNeuron>();

                        // if the object is freshly created, and not loaded from disk, we need to init the list
                }

                return (System.Collections.ObjectModel.ObservableCollection<MindMapNeuron>)fChildren;
            }
        }

        #endregion

        #region CircularRefs

        /// <summary>
        ///     Gets the list of circular references found in this neuron.
        /// </summary>
        /// <remarks>
        ///     This is not persisted cause this situation can change during storage,
        ///     so it best to recalculate.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<ulong> CircularRefs
        {
            get
            {
                return fCircularRefs;
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets/sets the neuron that this mind map item represents.
        /// </summary>
        /// <remarks>
        ///     Checks if the item being assigned, is a Cluster.
        /// </remarks>
        /// <value>
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public override Neuron Item
        {
            get
            {
                return base.Item;
            }

            set
            {
                if (value is NeuronCluster || value == null)
                {
                    base.Item = value;
                }
                else
                {
                    throw new System.InvalidOperationException("NeuronCluster expected.");
                }
            }
        }

        #endregion

        #region Cluster

        /// <summary>
        ///     Gets/sets the cluster, same as
        ///     <see cref="JaStDev.HAB.Designer.MindMapCluster.Item" /> , but with c
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronCluster Cluster
        {
            get
            {
                return (NeuronCluster)base.Item;
            }

            set
            {
                Item = value;
            }
        }

        #endregion

        #region Meaning

        /// <summary>
        ///     Gets/sets the meaning that is assigned to the cluster.
        /// </summary>
        /// <remarks>
        ///     This is as a neuron cause the DropDownNSSelector uses this in the UI.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Meaning
        {
            get
            {
                if (Cluster.Meaning != Neuron.EmptyId)
                {
                    return Brain.Current[Cluster.Meaning];
                }

                return null;
            }

            set
            {
                if ((value != null && Cluster.Meaning != value.ID)
                    || (value == null && Cluster.Meaning != Neuron.EmptyId))
                {
                    OnPropertyChanging("Meaning", Meaning, value);
                    if (value != null)
                    {
                        Cluster.Meaning = value.ID;
                    }
                    else
                    {
                        Cluster.Meaning = Neuron.EmptyId;
                    }

                    OnPropertyChanged("Meaning");
                }
            }
        }

        #endregion

        #region X

        /// <summary>
        ///     Gets/sets the horizontal offset of the item on the graph.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children.
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
                if (fIsDragging == false && fChildren != null)
                {
                    // we also check for fChildren != null cause this tells us if the object is loading from xml or not.  when fChildren == null -> still loading, so don't adjust anything.
                    foreach (var i in Children)
                    {
                        if (i.X < value)
                        {
                            // if the new value is to small, recalculate the max allowed change.
                            value = i.X;
                        }
                    }
                }

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
        ///     check and change val as desired, depending on children.
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
                if (fIsDragging == false && fChildren != null)
                {
                    // we also check for fChildren != null cause this tells us if the object is loading from xml or not.  when fChildren == null -> still loading, so don't adjust anything.
                    foreach (var i in Children)
                    {
                        if (i.Y < value)
                        {
                            // if the new value is to small, recalculate the max allowed change.
                            value = i.Y;
                        }
                    }
                }

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
        ///     check and change val as desired, depending on children.
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
                if (fIsDragging == false && fChildren != null)
                {
                    // we also check for fChildren != null cause this tells us if the object is loading from xml or not.  when fChildren == null -> still loading, so don't adjust anything.
                    foreach (var i in Children)
                    {
                        if (i.Y + i.Height > Y + value)
                        {
                            // if the new value is to small, recalculate the max allowed change.
                            value = (i.Y + i.Height) - Y;
                        }
                    }
                }

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
        ///     check and change val as desired, depending on children.
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
                if (fIsDragging == false && fChildren != null)
                {
                    // we also check for fChildren != null cause this tells us if the object is loading from xml or not.  when fChildren == null -> still loading, so don't adjust anything.
                    foreach (var i in Children)
                    {
                        if (i.X + i.Width > X + value)
                        {
                            // if the new value is to small, recalculate the max allowed change.
                            value = (i.X + i.Width) - X;
                        }
                    }
                }

                base.Width = value;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the converter to retrieve an outline for a neuron.
        /// </summary>
        /// <value>
        ///     The outliner.
        /// </value>
        public static NeuronToResourceConverter ClusterOutliner
        {
            get
            {
                if (fOutliner == null)
                {
                    fOutliner =
                        WindowMain.Current.FindResource("NeuronToOutlineConvForCluster") as NeuronToResourceConverter;
                }

                return fOutliner;
            }
        }

        /// <summary>
        ///     Gets/sets the outline of this neuron. This is used to calculate the
        ///     edge for placing links.
        /// </summary>
        /// <remarks>
        ///     We <see langword="override" /> cause a cluster needs a different
        ///     converter. This is just the basic outline, it still needs to be
        ///     adjusted in size and offset, to correspond to the actual object.
        /// </remarks>
        /// <value>
        /// </value>
        public override System.Windows.Media.Geometry Outline
        {
            get
            {
                System.Windows.Media.Geometry iRes = null;
                var iFound = ClusterOutliner.Convert(Item, typeof(System.Windows.Media.Geometry), null, null) as string;
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

        /// <summary>Replaces the child.</summary>
        /// <remarks>When the <see cref="MindMap"/> representation of a neuron is changed,
        ///     and this neuron is a child of this cluster, we need to update the
        ///     mindMap representation of the child.</remarks>
        /// <param name="oldValue">The old child.</param>
        /// <param name="newValue">The new child.</param>
        internal void ReplaceChild(MindMapNeuron oldValue, MindMapNeuron newValue)
        {
            var iIndex = Children.IndexOf(oldValue);
            if (iIndex > -1)
            {
                oldValue.PropertyChanged -= Child_PropertyChanged;
                if (oldValue is MindMapCluster && !(newValue is MindMapCluster))
                {
                    // if the old item was a cluster, it could be there was a circular ref, if the new item is no longer a cluster, this is solved.
                    fCircularRefs.Remove(newValue.ItemID);
                }

                if (newValue != null)
                {
                    newValue.PropertyChanged += Child_PropertyChanged;
                    Children[iIndex] = newValue;
                }
                else
                {
                    Children.Remove(oldValue);
                }
            }
        }

        #region Fields

        /// <summary>The f children.</summary>
        private object fChildren;

        /// <summary>The f circular refs.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<ulong> fCircularRefs =
            new System.Collections.ObjectModel.ObservableCollection<ulong>();

        /// <summary>The f is dragging.</summary>
        private bool fIsDragging;

                     // a switch used by SetPositionFromDrag and X and Y to skip adjusting x,y when they are changed.

        /// <summary>The f event monitor.</summary>
        private MindMapClusterEventMonitor fEventMonitor;

        /// <summary>
        ///     Determins if items are automatically added to the cluster when they
        ///     are not on the mindmap or not.
        /// </summary>
        /// <remarks>
        ///     Should be configurable from options window
        /// </remarks>
        public static bool AutoAddItemsToMindMapCluter = true;

        /// <summary>The f outliner.</summary>
        private static NeuronToResourceConverter fOutliner;

        #endregion

        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="MindMapCluster"/> class.</summary>
        /// <param name="item">The item.</param>
        internal MindMapCluster(NeuronCluster item)
            : base(item)
        {
            fEventMonitor = EventManager.Current.RegisterMindMapCluter(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MindMapCluster" /> class.
        /// </summary>
        /// <remarks>
        ///     Required for serialization, should be fixed later on
        /// </remarks>
        public MindMapCluster()
        {
        }

        #endregion

        #region functions

        /// <summary>Sets the position in 1 call by simulating a drag operation..</summary>
        /// <remarks><para>Normally, a drag operation isn't different from setting the<paramref name="x"/> and <paramref name="y"/> props. However, when a
        ///         cluster is dragged, it's children should also be moved. This is done
        ///         through this call.</para>
        /// <para>This is a <see langword="virtual"/> function so that descendents can
        ///         change some of the behaviour a drop operation does on a positioned
        ///         mindmap item.</para>
        /// </remarks>
        /// <param name="x">The x pos</param>
        /// <param name="y">The y pos.</param>
        /// <param name="alreadyMoved">The already Moved.</param>
        public override void SetPositionFromDrag(
            double x, 
            double y, System.Collections.Generic.List<MindMapNeuron> alreadyMoved)
        {
            fIsDragging = true;
            try
            {
                var iXDiv = x - X;
                var iYDiv = y - Y;
                foreach (var i in Children)
                {
                    if (alreadyMoved.Contains(i) == false)
                    {
                        alreadyMoved.Add(i);
                        i.SetPositionFromDrag(i.X + iXDiv, i.Y + iYDiv, alreadyMoved);
                    }
                }

                base.SetPositionFromDrag(x, y, alreadyMoved);
            }
            finally
            {
                fIsDragging = false;
            }
        }

        /// <summary>
        ///     Loads the children.
        /// </summary>
        public void LoadChildren()
        {
            var iItems = (from i in Owner.Items where i is MindMapNeuron select (MindMapNeuron)i).ToList();
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = Cluster.Children) iChildren = iList.ConvertTo<Neuron>(); // we need to have a list cause we need to have the count.
            var iExistingMM = (from i in iItems where iChildren.Contains(i.Item) select i).ToList();
            if (AutoAddItemsToMindMapCluter == false)
            {
                Owner.ShowChildren(this, null, iExistingMM);
                foreach (var i in iExistingMM)
                {
                    Children.Add(i);
                    MonitorChild(i);
                }
            }
            else
            {
                var iAllNeurons = (from i in iItems select i.Item).ToList(); // just a helper for the next query.
                var iListToAdd = (from i in iChildren

                                  // need to calculate all the neurons that need to be created.
                                  where iAllNeurons.Contains(i) == false
                                  select i).ToList();
                Owner.ShowChildren(this, iListToAdd, iExistingMM);
                foreach (var i in iExistingMM)
                {
                    // don't need to recalculate this list, newly created items are automatically monitored.
                    Children.Add(i);
                    MonitorChild(i);
                }
            }

            if (fEventMonitor == null)
            {
                // if the event monitor is not yet registered (happens during load), create it now.
                fEventMonitor = EventManager.Current.RegisterMindMapCluter(this);
            }
        }

        /// <summary>Creates a new mindmap item for the specified neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <returns>a new mindMapNeuron/Cluster.</returns>
        private MindMapNeuron CreateNewFor(Neuron neuron)
        {
            var iNew = CreateFor(neuron);
            iNew.X = X + 4 + (Children.Count * 44);

                // width of new item = with of this + border + nr of children already added + spacing between each.
            iNew.Width = 40;
            iNew.Height = 30;
            iNew.Y = Y;
            return iNew;
        }

        /// <summary>Called when a property of this cluster is changed (should be already<see langword="checked"/> that it is for this cluster).</summary>
        /// <remarks>Updates the Meaning.</remarks>
        /// <param name="e">The <see cref="NeuronPropChangedEventArgs"/> instance containing the
        ///     event data.</param>
        internal void PropChanged(NeuronPropChangedEventArgs e)
        {
            OnPropertyChanged(e.Property);
        }

        /// <summary>Called when the list of <see cref="Children"/> are changed.</summary>
        /// <remarks>We need to filter, make certain that the event was for the cluster
        ///     that we ar wrapping.</remarks>
        /// <param name="e">The <see cref="NeuronListChangedEventArgs"/> instance containing the
        ///     event data.</param>
        internal void ChildrenChanged(NeuronListChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NeuronListChangeAction.Insert:
                    MonitorChild(e.Item);
                    break;
                case NeuronListChangeAction.Remove:
                    UnMonitorChild(e.Item);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Prepares the children after a load from stream.
        /// </summary>
        /// <remarks>
        ///     Throws an error if called at other time than after load
        /// </remarks>
        internal void PrepareChildren()
        {
            var iChildren = (SmallIDCollection)fChildren; // this will fail if not just loaded from xml file.
            fChildren = new System.Collections.ObjectModel.ObservableCollection<MindMapNeuron>();
            var iContent = from i in Owner.Items
                           where i is MindMapNeuron && iChildren.Contains(((MindMapNeuron)i).ItemID)
                           select (MindMapNeuron)i;
            foreach (var i in iContent)
            {
                ((System.Collections.ObjectModel.ObservableCollection<MindMapNeuron>)fChildren).Add(i);
                MonitorChild(i);
            }

            if (fEventMonitor == null)
            {
                // if the event monitor is not yet registered (happens during load), create it now.
                fEventMonitor = EventManager.Current.RegisterMindMapCluter(this);
            }
        }

        /// <summary>Stops montiroing the child.</summary>
        /// <param name="neuron">The neuron.</param>
        private void UnMonitorChild(Neuron neuron)
        {
            var iFound = (from i in Children where i.Item == neuron select i).FirstOrDefault();
            if (iFound != null)
            {
                iFound.ChildCount--;
                if (iFound != null)
                {
                    iFound.PropertyChanged -= Child_PropertyChanged;
                    if (iFound is MindMapCluster)
                    {
                        // make certain any circular refs are broken.
                        ((MindMapCluster)iFound).fCircularRefs.Remove(ItemID);
                    }
                }

                Children.Remove(iFound);
                fCircularRefs.Remove(neuron.ID); // also make certain that any circular ref is broken.
            }
        }

        /// <summary>starts Monitoring the child.</summary>
        /// <param name="neuron">The neuron.</param>
        private void MonitorChild(Neuron neuron)
        {
            if (Owner != null)
            {
                var iFound =
                    (from i in Owner.Items
                     where i is MindMapNeuron && ((MindMapNeuron)i).Item == neuron
                     select (MindMapNeuron)i).FirstOrDefault();
                if (iFound == null && AutoAddItemsToMindMapCluter)
                {
                    iFound = CreateNewFor(neuron);
                    Owner.Items.Add(iFound);
                }

                if (iFound != null)
                {
                    // we only add, if there is a mindmap item found.
                    Children.Add(iFound);
                    MonitorChild(iFound);
                }
            }
        }

        /// <summary>starts Monitoring the child.</summary>
        /// <remarks>Also checks for circular references.</remarks>
        /// <param name="child">The child.</param>
        internal void MonitorChild(MindMapNeuron child)
        {
            AdjustRectForChild(child);
            child.PropertyChanged += Child_PropertyChanged;
            child.ChildCount++;

            var iCluster = child.Item as NeuronCluster;
            if (iCluster != null)
            {
                bool iItemInCluster;
                using (var iList = iCluster.Children) iItemInCluster = iList.Contains(Item);
                if (iItemInCluster && fCircularRefs.Contains(iCluster.ID) == false)
                {
                    fCircularRefs.Add(iCluster.ID);
                    ((MindMapCluster)child).CircularRefs.Add(ItemID);

                        // if child.Item is NeuronCluster, child must be MindMapCluster
                }
            }
        }

        /// <summary>Adjusts the rect of this control so that the child fits into it +
        ///     ZIndex is adjusted.</summary>
        /// <param name="item">The item we need to adjust to.</param>
        private void AdjustRectForChild(MindMapNeuron item)
        {
            if (X > item.X)
            {
                X = item.X;
            }

            var iItemVal = item.X + item.Width;
            var iVal = X + Width;
            if (iVal < iItemVal)
            {
                Width += iItemVal - iVal;
            }

            if (Y > item.Y)
            {
                Y = item.Y;
            }

            iItemVal = item.Y + item.Height;
            iVal = Y + Height;
            if (iVal < iItemVal)
            {
                Height += iItemVal - iVal;
            }

            Owner.MoveAfter(this, item);
        }

        /// <summary>Handles the PropertyChanged event of the Child control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Child_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (fIsDragging == false && Owner != null)
            {
                // we also check for owner cause this could get called because the item is being removed because of an undo, in which case this gets triggered because MindMapNeuron.Shape is being reset to null.
                AdjustRectForChild((MindMapNeuron)sender);
            }
        }

        #endregion

        #region Xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteIDList(writer, "ChildrenIds", ChildrenIds);
        }

        /// <summary>Reads the content of the XML file (all the properties)</summary>
        /// <param name="reader">The reader.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            var iNew = new SmallIDCollection();
            XmlStore.ReadIDList(reader, "ChildrenIds", iNew);
            string iTemp = null;
            XmlStore.TryReadElement(reader, "Meaning", ref iTemp);

                // this is to catch an old bug: original format wrote this value, which is not required. So for reading old formats, we still need this.
            ChildrenIds = iNew;
        }

        #endregion
    }
}