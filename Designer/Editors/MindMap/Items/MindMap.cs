// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMap.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the information to draw a graph of part of the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Contains all the information to draw a graph of part of the brain.
    /// </summary>
    public class MindMap : EditorBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMap"/> class.</summary>
        public MindMap()
        {
            Items = new MindMapItemCollection(this);
        }

        #endregion

        #region Fields

        /// <summary>The f height.</summary>
        private double fHeight;

        /// <summary>The f width.</summary>
        private double fWidth;

        /// <summary>The f zoom.</summary>
        private double fZoom = 1.0;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        /// <summary>The f new neuron width.</summary>
        private const double fNewNeuronWidth = 40; // default width for a new neuron.

        /// <summary>The f letter length.</summary>
        private const double fLetterLength = 4; // average length of letter that is used to calculate the width.

        /// <summary>The f new neuron height.</summary>
        private const double fNewNeuronHeight = 25;

                             // when new neurons are created for showing links or children, this is the default height.

        /// <summary>The f selected items.</summary>
        private readonly MindMapItemSelectionList fSelectedItems = new MindMapItemSelectionList();

        #endregion

        #region Prop

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific.
        /// </summary>
        /// <value>
        /// </value>
        public override string Icon
        {
            get
            {
                return "/Images/NewMindMap_Enabled.png";
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items visialble on this mind map.
        /// </summary>
        public MindMapItemCollection Items { get; private set; }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     Gets the list of selected objects
        /// </summary>
        /// <value>
        ///     The selected items internal.
        /// </value>
        public MindMapItemSelectionList SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the first item inte selectedItems list, if there is any. This is
        ///     used to bin to in wpf from contextmenus
        /// </summary>
        /// <value>
        ///     The selected item.
        /// </value>
        public MindMapItem SelectedItem
        {
            get
            {
                if (fSelectedItems.Count > 0)
                {
                    return fSelectedItems[0];
                }

                return null;
            }
        }

        #region Height

        /// <summary>
        ///     Gets the height of the work area.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is important for scrolling. it is set by the
        ///         <see cref="MindMapItems" /> whenever a property is changed.
        ///     </para>
        ///     <para>
        ///         We allow an <see langword="internal" /> setter cause the view must also
        ///         be able to set this value.
        ///     </para>
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double Height
        {
            get
            {
                return fHeight;
            }

            internal set
            {
                fHeight = value;
                OnPropertyChanged("Height");
                OnPropertyChanged("ScrollHeight");
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the widht of the work area.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is important for scrolling. it is set by the
        ///         <see cref="MindMapItems" /> whenever a property is changed.
        ///     </para>
        ///     <para>
        ///         We allow an <see langword="internal" /> setter cause the view must also
        ///         be able to set this value.
        ///     </para>
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double Width
        {
            get
            {
                return fWidth;
            }

            internal set
            {
                fWidth = value;
                OnPropertyChanged("Width");
                OnPropertyChanged("ScrollWidth");
            }
        }

        #endregion

        #region ScrollWidth

        /// <summary>
        ///     Gets the width of the work area, adjusted by the zoom value.
        /// </summary>
        /// <remarks>
        ///     This is provided so the view doesn't have to use complex bindings for
        ///     this value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double ScrollWidth
        {
            get
            {
                return fWidth * fZoom;
            }
        }

        #endregion

        #region ScrollHeight

        /// <summary>
        ///     Gets the width of the work area, adjusted by the zoom value.
        /// </summary>
        /// <remarks>
        ///     This is provided so the view doesn't have to use complex bindings for
        ///     this value.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double ScrollHeight
        {
            get
            {
                return fHeight * fZoom;
            }
        }

        #endregion

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll position
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                if (value < 0)
                {
                    // can't have values smaller than 0.
                    value = 0;
                }

                if (fHorScrollPos != value)
                {
                    fHorScrollPos = value;
                    OnPropertyChanged("HorScrollPos");
                }
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scroll position
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                if (value < 0)
                {
                    // can't have values smaller than 0.
                    value = 0;
                }

                if (value != fVerScrollPos)
                {
                    fVerScrollPos = value;
                    OnPropertyChanged("VerScrollPos");
                }
            }
        }

        #endregion

        #region Zoom

        /// <summary>
        ///     Gets/sets the zoom factor that should be applied, expressed in procent
        ///     values.
        /// </summary>
        public double ZoomProcent
        {
            get
            {
                return fZoom * 100;
            }

            set
            {
                var iVal = value / 100;
                if (fZoom != iVal)
                {
                    fZoom = iVal;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                    OnPropertyChanged("ScrollHeight");
                    OnPropertyChanged("ScrollWidth");
                }
            }
        }

        /// <summary>
        ///     Gets/sets the zoom factor that should be applied.
        /// </summary>
        public double Zoom
        {
            get
            {
                return fZoom;
            }

            set
            {
                if (value < 0.001)
                {
                    // need to make certain we don't make it to small.
                    value = 0.001;
                }

                if (fZoom != value)
                {
                    fZoom = value;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                    OnPropertyChanged("ScrollHeight");
                    OnPropertyChanged("ScrollWidth");
                }
            }
        }

        /// <summary>
        ///     Gets/sets the inverse value of the zoom factor that should be applied.
        ///     This is used to re-adjust zoom values for overlays (bummer, need to
        ///     work this way for wpf).
        /// </summary>
        public double ZoomInverse
        {
            get
            {
                return 1 / fZoom;
            }
        }

        #endregion

        /// <summary>
        ///     Gets a title that the description editor can use to display in the
        ///     header.
        /// </summary>
        /// <value>
        /// </value>
        public override string DescriptionTitle
        {
            get
            {
                return Name + " - Mind map";
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public override string DocumentInfo
        {
            get
            {
                return "Mind map: " + Name;
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public override string DocumentType
        {
            get
            {
                return "Mind map";
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <remarks>This function is called for each element that is found, so this
        ///     function should check which element it is and only read that element
        ///     accordingly.</remarks>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Zoom")
            {
                fZoom = XmlStore.ReadElement<double>(reader, "Zoom");
                return true;
            }

            if (reader.Name == "XmlObjectStore")
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(XmlObjectStore));
                var iNode = (XmlObjectStore)valueSerializer.Deserialize(reader);
                var value = iNode.Data as MindMapItem;
                if (value != null)
                {
                    Items.Add(value);
                }

                reader.MoveToContent();
                return true;
            }

            if (reader.Name == "MindMapNote")
            {
                var iNoteSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapNote));
                var iNode = (MindMapNote)iNoteSer.Deserialize(reader);
                if (iNode != null)
                {
                    Items.Add(iNode);
                }

                return true;
            }

            if (reader.Name == "MindMapLink")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapLink));
                var iNode = (MindMapLink)iLinkSer.Deserialize(reader);
                if (iNode != null)
                {
                    Items.Add(iNode);
                }

                return true;
            }

            if (reader.Name == "MindMapNeuron")
            {
                var iNeuronSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapNeuron));
                var iNode = (MindMapNeuron)iNeuronSer.Deserialize(reader);
                if (iNode != null && iNode.ItemID != Neuron.EmptyId)
                {
                    // The emptyID is to filter out items that failed to load.
                    Items.Add(iNode);
                }

                return true;
            }

            if (reader.Name == "MindMapCluster")
            {
                var iClusterSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapCluster));
                var iNode = (MindMapCluster)iClusterSer.Deserialize(reader);
                if (iNode != null && iNode.ItemID != Neuron.EmptyId)
                {
                    Items.Add(iNode);
                }

                return true;
            }

            return base.ReadXmlInternal(reader);
        }

        /// <summary>Reads the XML.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            var iClusters = from i in Items where i is MindMapCluster select (MindMapCluster)i;
            foreach (var i in iClusters)
            {
                i.PrepareChildren();
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "Zoom", Zoom);

            var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(XmlObjectStore));
            var iNoteSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapNote));
            var iNeuronSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapNeuron));
            var iClusterSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapCluster));
            var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMapLink));
            var iSorted = from i in Items orderby i.TypeIndex select i;

                // we sort to make certain that all the links are at the back, which is required for loading.
            foreach (var item in iSorted)
            {
                if (item is MindMapNote)
                {
                    iNoteSer.Serialize(writer, item);
                }
                else if (item is MindMapLink)
                {
                    iLinkSer.Serialize(writer, item);
                }
                else if (item is MindMapCluster)
                {
                    iClusterSer.Serialize(writer, item);
                }
                else if (item is MindMapNeuron)
                {
                    iNeuronSer.Serialize(writer, item);
                }
                else
                {
                    var iNode = new XmlObjectStore { Data = item };
                    valueSerializer.Serialize(writer, iNode);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>Shows the children of the specified neuron.</summary>
        /// <param name="cluster">The cluster.</param>
        public void ShowChildren(MindMapCluster cluster)
        {
            var iMMs = (from i in Items

                        // all the mindmap neurons
                        where i is MindMapNeuron
                        select (MindMapNeuron)i).ToList();
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = cluster.Cluster.Children) iChildren = iList.ConvertTo<Neuron>(); // we need to have a list cause we need to have the count.
            var iAllNeurons = (from i in iMMs select i.Item).ToList();

            var iListToAdd = (from i in iChildren where iAllNeurons.Contains(i) == false select i).ToList();
            var iExistingMM = (from i in iMMs where iChildren.Contains(i.Item) select i).ToList();
            ShowChildren(cluster, iListToAdd, iExistingMM);
        }

        /// <summary>The show children.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="newItems">The new items.</param>
        /// <param name="existing">The existing.</param>
        internal void ShowChildren(
            MindMapCluster cluster, System.Collections.Generic.List<Neuron> newItems, System.Collections.Generic.List<MindMapNeuron> existing)
        {
            var iOffsetX = 16.0; // stores the offset to use for putting new neurons on the ui.
            var iOffsetY = 56.0;

            var iTotal = newItems == null ? 0 : newItems.Count;
            iTotal += existing == null ? 0 : existing.Count;
            var iNrRows = (int)System.Math.Ceiling(System.Math.Sqrt(iTotal));

                // we lay the items out in a square.  To get the nr of rows, there are as many on X as on Y, so use square root.
            var iCols = iNrRows; // so we can reassign the value after each row.
            if (newItems != null)
            {
                foreach (var i in newItems)
                {
                    var iRes = CreateNewFor(i, cluster.X + iOffsetX, cluster.Y + iOffsetY);
                    iOffsetX += iRes.Width + 4;
                    iCols--;
                    if (iCols == 0)
                    {
                        iCols = iNrRows;
                        iOffsetY += fNewNeuronHeight + 4;
                        iOffsetX = 20.0;
                    }
                }
            }

            if (existing != null)
            {
                foreach (var i in existing)
                {
                    i.X = cluster.X + iOffsetX;
                    i.Y = cluster.Y + iOffsetY;
                    iOffsetX += i.Width + 4;
                    iCols--;
                    if (iCols == 0)
                    {
                        iCols = iNrRows;
                        iOffsetY += fNewNeuronHeight + 4;
                        iOffsetX = 20.0;
                    }
                }
            }

            cluster.Width += 16.0; // we increase the width a bit to create a border.
        }

        /// <summary>Shows all the clusters that own a neuron on the mindmap.</summary>
        /// <param name="item">The item for whom to show all the clusters that own it.</param>
        public void ShowOwners(MindMapNeuron item)
        {
            var iExisting = (from i in Items

                             // all the clusters on the mindmap
                             where i is MindMapCluster
                             select ((MindMapCluster)i).Cluster).ToList();

            if (item.Item.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = item.Item.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iToCreate = (from i in iClusters

                                     // all the clusters that need to be created.
                                     where iExisting.Contains(i) == false
                                     select i).ToList();
                    ShowClustersRound(item, iToCreate);
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusters);
                }
            }
        }

        /// <summary>Creates new <see cref="MindMapClusters"/> for all the clusters in the<paramref name="list"/> and arranges them round the item.</summary>
        /// <param name="item">The item to arrange all the items in the <paramref name="list"/>
        ///     round.</param>
        /// <param name="list">The list.</param>
        private void ShowClustersRound(MindMapNeuron item, System.Collections.Generic.List<NeuronCluster> list)
        {
            double iOffsetX = list.Count * -5; // we do *10 so that we can go up and down a range with + 10
            double iOffsetY = list.Count * 5;
            foreach (var i in list)
            {
                MindMapNeuron iRes;
                if (i != null)
                {
                    // just a sanity check, if we don't do this and the db is corrupted, we could get an error here.
                    iRes = CreateNewFor(i, item.X + iOffsetX, item.Y + iOffsetY);
                }

                // For corrupt db checking: but an else statement here
                iOffsetX += 10;
                iOffsetY -= 10;
            }
        }

        /// <summary>Shows all the links of the specified neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        public void ShowLinksIn(MindMapNeuron neuron)
        {
            var iOffset = 0.0; // stores the offset to use for putting new neurons on the ui.
            if (neuron.Item.LinksInIdentifier != null)
            {
                using (var iLinks = neuron.Item.LinksIn)
                {
                    foreach (var iLink in iLinks)
                    {
                        ShowLinkBetween(neuron, iLink, true, ref iOffset);
                    }
                }
            }
        }

        /// <summary>Shows the outgoing links of the argument</summary>
        /// <param name="neuron">The neuron.</param>
        public void ShowLinksOut(MindMapNeuron neuron)
        {
            var iOffset = 0.0; // stores the offset to use for putting new neurons on the ui.
            using (var iLinks = neuron.Item.LinksOut)
                foreach (var iLink in iLinks)
                {
                    ShowLinkBetween(neuron, iLink, false, ref iOffset);
                }
        }

        /// <summary>The show link between.</summary>
        /// <param name="from">The from.</param>
        /// <param name="link">The link.</param>
        /// <param name="asIncomming">The as incomming.</param>
        /// <param name="offset">The offset.</param>
        public void ShowLinkBetween(MindMapNeuron from, Link link, bool asIncomming, ref double offset)
        {
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                Neuron iTo;
                if (asIncomming)
                {
                    iTo = link.From;
                }
                else
                {
                    iTo = link.To;
                }

                // first check if the requested neuron is already visible, if not, creating it will automatically create the link.
                var iFound =
                    (from i in Items
                     where i is MindMapNeuron && ((MindMapNeuron)i).ItemID == iTo.ID
                     select (MindMapNeuron)i).FirstOrDefault();
                if (iFound == null)
                {
                    var iRes = CreateNewFor(iTo, from.X + offset, from.Y + (from.Height * 2));
                    offset += iRes.Width + 16;
                }
                else
                {
                    var iNew = new MindMapLink();
                    if (asIncomming)
                    {
                        iNew.CreateLink(link, iFound, from);
                    }
                    else
                    {
                        iNew.CreateLink(link, from, iFound);
                    }

                    Items.Add(iNew);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Creates a new correct <see cref="MindMapNeuron"/> /<see cref="MindMapCluster"/> for the neuron to wrap.</summary>
        /// <param name="toWrap">Item to wrap.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The <see cref="MindMapNeuron"/>.</returns>
        internal MindMapNeuron CreateNewFor(Neuron toWrap, double x, double y)
        {
            var iRes = MindMapNeuron.CreateFor(toWrap);
            iRes.Y = y;
            iRes.X = x;
            iRes.Width = fNewNeuronWidth + (fLetterLength * iRes.NeuronInfo.DisplayTitle.Length);
            iRes.Height = fNewNeuronHeight;
            Items.Add(iRes);

                // don't need to create the link object, this is done automatically when it is added to the list
            return iRes;
        }

        /// <summary>Shows all the <paramref name="existing"/> incomming relations</summary>
        /// <param name="neuron">The neuron for whom to show the links.</param>
        /// <param name="existing">The list of neurons that are existing on the mindmap. This is passed
        ///     as a param for optimazation, sometimes showExistingIn and Out is
        ///     called after each other this is offcourse the same list for both, so
        ///     it is best to reuse them.</param>
        /// <param name="checkExistence">True if we should check if any of the links are already visible for
        ///     the neuron, or all should be created blindly without checking.</param>
        internal void ShowExistingIn(
            MindMapNeuron neuron, System.Collections.Generic.IList<ulong> existing, 
            bool checkExistence)
        {
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                if (neuron.Item.LinksInIdentifier != null)
                {
                    using (var iLinks = neuron.Item.LinksIn)
                    {
                        var iLinksIn = from i in iLinks where existing.Contains(i.FromID) select i;
                        foreach (var iLink in iLinksIn)
                        {
                            if (checkExistence)
                            {
                                var iFound =
                                    (from i in Items where i is MindMapLink && ((MindMapLink)i).Link == iLink select i)
                                        .FirstOrDefault();
                                if (iFound != null)
                                {
                                    // the link already exists, don't try to create it.
                                    break;
                                }
                            }

                            var iNew = new MindMapLink();
                            var iFrom =
                                (from i in Items
                                 where i is MindMapNeuron && ((MindMapNeuron)i).ItemID == iLink.FromID
                                 select (MindMapNeuron)i).FirstOrDefault();
                            System.Diagnostics.Debug.Assert(iFrom != null);
                            iNew.CreateLink(iLink, iFrom, neuron);
                            Items.Add(iNew);
                        }
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Shows all the <paramref name="existing"/> outgoing relations</summary>
        /// <param name="neuron">The neuron for whom to show the links.</param>
        /// <param name="existing">The list of neurons that are existing on the mindmap. This is passed
        ///     as a param for optimazation, sometimes showExistingIn and Out is
        ///     called after each other this is offcourse the same list for both, so
        ///     it is best to reuse them.</param>
        /// <param name="checkExistence">True if we should check if any of the links are already visible for
        ///     the neuron, or all should be created blindly without checking.</param>
        internal void ShowExistingOut(
            MindMapNeuron neuron, System.Collections.Generic.IList<ulong> existing, 
            bool checkExistence)
        {
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                ListAccessor<Link> iLinksAccess = neuron.Item.LinksOut;
                iLinksAccess.Lock();
                try
                {
                    var iLinksOut = from i in iLinksAccess.List where existing.Contains(i.ToID) select i;
                    foreach (var iLink in iLinksOut)
                    {
                        if (checkExistence)
                        {
                            var iFound =
                                (from i in Items where i is MindMapLink && ((MindMapLink)i).Link == iLink select i)
                                    .FirstOrDefault();
                            if (iFound != null)
                            {
                                // the link already exists, don't try to create it.
                                break;
                            }
                        }

                        var iNew = new MindMapLink();
                        var iTo =
                            (from i in Items
                             where i is MindMapNeuron && ((MindMapNeuron)i).ItemID == iLink.ToID
                             select (MindMapNeuron)i).FirstOrDefault();
                        iNew.CreateLink(iLink, neuron, iTo);
                        Items.Add(iNew);
                    }
                }
                finally
                {
                    iLinksAccess.Dispose(); // also unlocks
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region ZIndex manip

        /// <summary>Moves the specified mindmap items 1 up the ZIndex (visually).</summary>
        /// <remarks>warning: modifies the argument list.</remarks>
        /// <param name="toMove"><see cref="Items"/> to move. Warning: this list is empty when the
        ///     function retursn.</param>
        public void MoveUp(System.Collections.Generic.List<MindMapItem> toMove)
        {
            while (toMove.Count > 0)
            {
                var iCurLevel = (from i in toMove where i.ZIndex == toMove[0].ZIndex select i).ToList();

                    // get all the items at the same level as the first item and move them together by switching with all the items at the new level
                var iPrevOnNewPos = from i in Items where i.ZIndex == toMove[0].ZIndex + 1 select i;

                    // these are all the items currently at the level of the items we are trying to move, so switch these levels.
                foreach (var i in iPrevOnNewPos)
                {
                    i.ZIndex--;
                }

                foreach (var i in iCurLevel)
                {
                    i.ZIndex++;
                    toMove.Remove(i);
                }
            }
        }

        /// <summary>Moves the specified mindmap items 1 down the ZIndex (visually).</summary>
        /// <param name="toMove">The to Move.</param>
        public void MoveDown(System.Collections.Generic.List<MindMapItem> toMove)
        {
            while (toMove.Count > 0)
            {
                var iCurLevel = (from i in toMove where i.ZIndex == toMove[0].ZIndex select i).ToList();

                    // get all the items at the same level as the first item and move them together by switching with all the items at the new level
                var iPrevOnNewPos = from i in Items where i.ZIndex == toMove[0].ZIndex - 1 select i;

                    // these are all the items currently at the level of the items we are trying to move, so switch these levels.
                foreach (var i in iPrevOnNewPos)
                {
                    i.ZIndex++;
                }

                foreach (var i in iCurLevel)
                {
                    i.ZIndex--;
                    toMove.Remove(i);
                }
            }
        }

        /// <summary>Moves the specified mindmap items to the back of the ZIndex
        ///     (visually).</summary>
        /// <param name="list">The list</param>
        public void MoveToBack(System.Collections.Generic.IList<MindMapItem> list)
        {
            foreach (var iSelected in list)
            {
                foreach (var iItem in Items)
                {
                    if (list.Contains(iItem) == false)
                    {
                        if (iItem.ZIndex < iSelected.ZIndex)
                        {
                            iItem.ZIndex++;
                        }
                    }
                }

                iSelected.ZIndex = 0;
            }
        }

        /// <summary>Moves the specified mindmap items to the front of the ZIndex
        ///     (visually).</summary>
        /// <param name="list">The list.</param>
        public void MoveToFront(System.Collections.Generic.IList<MindMapItem> list)
        {
            foreach (var iSelected in list)
            {
                foreach (var iItem in Items)
                {
                    if (list.Contains(iItem) == false)
                    {
                        if (iItem.ZIndex > iSelected.ZIndex)
                        {
                            iItem.ZIndex--;
                        }
                    }
                }

                iSelected.ZIndex = Items.Count - 1;
            }
        }

        /// <summary>Moves the specified item down the ZIndex so that it visually behind
        ///     the other item.</summary>
        /// <param name="toMove">To mind map item to move.</param>
        /// <param name="after">The mind map after that should be in front of the item that is being
        ///     moved.</param>
        public void MoveAfter(MindMapItem toMove, MindMapItem after)
        {
            while (toMove.ZIndex >= after.ZIndex)
            {
                var iTemp = new System.Collections.Generic.List<MindMapItem> { toMove };

                    // we need to remake the list each time, cause the item gets removed in the MoveDown.
                MoveDown(iTemp);
            }
        }

        #endregion

        #region Clipboard

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The can copy to clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool CanCopyToClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The can paste special from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool CanPasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The paste special from clipboard.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void PasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The can paste from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool CanPasteFromClipboard()
        {
            if (base.CanPasteFromClipboard())
            {
            }

            throw new System.NotImplementedException();
        }

        /// <summary>The paste from clipboard.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void PasteFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Delete

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        /// <remarks>
        ///     Try to delete all neurons on the mindmap that aren't referenced
        ///     anywhere else. We also delete all the links that are left over after
        ///     all the neurons have been removed (or tried to). This is required
        ///     because if there were neurons that were used somewhere else, we would
        ///     at least remove the links related to this editor.
        /// </remarks>
        public override void DeleteEditor()
        {
            var iLinks = (from i in Items where i is MindMapLink select (MindMapLink)i).ToList();
            foreach (var i in iLinks)
            {
                EditorsHelper.DeleteLink(i.Link);
            }

            var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
            var iNeurons = (from i in Items where i is MindMapNeuron select ((MindMapNeuron)i).Item).ToList();
            iDeleter.Start(iNeurons);
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            var iNeurons = (from i in Items where i is MindMapNeuron select ((MindMapNeuron)i).Item).ToList();
            iDeleter.Start(iNeurons);

            if (deletionMethod == DeletionMethod.Delete || deletionMethod == DeletionMethod.DeleteIfNoRef)
            {
                // we only delete the links if the user requested so.
                var iLinks = (from i in Items where i is MindMapLink select (MindMapLink)i).ToList();
                foreach (var i in iLinks)
                {
                    EditorsHelper.DeleteLink(i.Link);
                }
            }
        }

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanDeleteSpecial()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region functions

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <remarks>This is used to determin which neurons need to be exported when an
        ///     editor is selected for export.</remarks>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            foreach (var i in Items)
            {
                var iNeuron = i as MindMapNeuron;
                if (iNeuron != null)
                {
                    yield return iNeuron.Item;
                }
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        /// <remarks>
        ///     We make certain that each item unloads all the data that it doesn't
        ///     require.
        /// </remarks>
        protected override void UnloadUIData()
        {
            SelectedItems.Clear();

                // when we get unloaded, we remove the selected info. This is important for links, which make themselves invisible when selected.
            foreach (var i in Items)
            {
                var iNeuron = i as MindMapNeuron;
                if (iNeuron != null)
                {
                    iNeuron.UnloadUIData();
                }
            }

            base.UnloadUIData();
        }

        #endregion
    }
}