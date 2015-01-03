// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for the <see cref="Neuron" /> that provides support for the
//   <see cref="System.ComponentModel.INotifyPropertyChanged" /> event and provides observable
//   collections for the links.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for the <see cref="Neuron" /> that provides support for the
    ///     <see cref="System.ComponentModel.INotifyPropertyChanged" /> event and provides observable
    ///     collections for the links.
    /// </summary>
    public class DebugNeuron : DebugItem, INeuronInfo, INeuronWrapper
    {
        #region EventMonitor Members

        /// <summary>Caled when the <paramref name="neuron"/> is Replaced the neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void ReplaceNeuron(Neuron neuron)
        {
            Item = neuron;
        }

        ///// <summary>
        ///// Internal handler for the children changed event.
        ///// </summary>
        ///// <param name="e">The <see cref="JaStDev.HAB.NeuronListChangedEventArgs"/> instance containing the event data.</param>
        // internal void ChildrenChanged(NeuronListChangedEventArgs e)
        // {
        // if (Children != null)
        // {
        // if (Children.IsLoaded == true)
        // {
        // DebugRef iFound = (from i in Children.Children where i.PointsTo != null && i.PointsTo.Item == e.Item select i).FirstOrDefault();
        // switch (e.Action)
        // {
        // case NeuronListChangeAction.Insert:
        // if (iFound == null)
        // Children.Children.Insert(e.Index, new DebugChild(e.Item, this));
        // break;
        // case NeuronListChangeAction.Remove:
        // if (iFound != null)
        // Children.Children.Remove(iFound);
        // break;
        // default:
        // break;
        // }
        // }
        // else
        // Children.UpdateHasChildren();
        // }
        // }

        ///// <summary>
        ///// Internal handler for the Clusteredby changed event (is ChildrenChanged, but from the other perspective).
        ///// </summary>
        ///// <param name="e">The <see cref="JaStDev.HAB.NeuronListChangedEventArgs"/> instance containing the event data.</param>
        // internal void ClusteredByChanged(NeuronListChangedEventArgs e)
        // {
        // if (ClusteredBy.IsLoaded == true && e.Item == Item)
        // {
        // DebugRef iFound = (from i in ClusteredBy.Children
        // where i.PointsTo != null && e.IsListFrom((NeuronCluster)i.PointsTo.Item) == true
        // select i).FirstOrDefault();
        // switch (e.Action)
        // {
        // case NeuronListChangeAction.Insert:
        // if (iFound == null)
        // Children.Children.Insert(e.Index, new DebugChild(e.Item, this));
        // break;
        // case NeuronListChangeAction.Remove:
        // if (iFound != null)
        // Children.Children.Remove(iFound);
        // break;
        // default:
        // break;
        // }
        // }
        // else
        // ClusteredBy.UpdateHasChildren();

        // }

        // internal void RemoveFromLinksOut(Link link)
        // {
        // if (LinksOut != null)
        // {
        // if (LinksOut.IsLoaded == true)
        // {
        // DebugRef iFound = (from i in LinksOut.Children where ((DebugLink)i).Item == link select i).FirstOrDefault();
        // if (iFound != null)
        // LinksOut.Children.Remove(iFound);
        // }
        // else
        // LinksOut.UpdateHasChildren();
        // }
        // }

        // internal void AddToLinksOut(Link link)
        // {
        // if (LinksOut != null  && link.IsValid == true)                                         //this funtion is called async, it could be that the link has since already been deleted again. check for this.
        // {
        // if (LinksOut.IsLoaded == true)
        // {
        // DebugRef iFound = (from i in LinksOut.Children where ((DebugLink)i).Item == link select i).FirstOrDefault();
        // if (iFound == null)
        // LinksOut.Children.Add(new DebugLink(link, this));
        // }
        // else
        // LinksOut.UpdateHasChildren();
        // }
        // }

        // internal void RemoveFromLinksIn(Link link)
        // {
        // if (LinksIn != null)
        // {
        // if (LinksIn.IsLoaded == true)
        // {
        // DebugRef iFound = (from i in LinksIn.Children where ((DebugLink)i).Item == link select i).FirstOrDefault();
        // if (iFound != null)
        // LinksIn.Children.Remove(iFound);
        // }
        // else
        // LinksIn.UpdateHasChildren();
        // }
        // }

        // internal void AddToLinksIn(Link link)
        // {
        // if (LinksIn != null && link.IsValid == true)                                           //this funtion is called async, it could be that the link has since already been deleted again. check for this.
        // {
        // if (LinksIn.IsLoaded == true)
        // {
        // DebugRef iFound = (from i in LinksIn.Children where ((DebugLink)i).Item == link select i).FirstOrDefault();
        // if (iFound == null)
        // LinksIn.Children.Add(new DebugLink(link, this));
        // }
        // else
        // LinksIn.UpdateHasChildren();
        // }
        // }
        #endregion

        /// <summary>Creates a duplicate of this DebugNeuron, using the specified<paramref name="neuron"/> as the new wrapped item. This is used to
        ///     create new wrappers for duplicated neurons (by the<see cref="DebugProcessor"/> ).</summary>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="DebugNeuron"/>.</returns>
        internal DebugNeuron Duplicate(Neuron neuron)
        {
            if (neuron != null)
            {
                var iRes = new DebugNeuron();
                iRes.fItem = neuron;
                if (neuron.ID != Neuron.TempId)
                {
                    iRes.NeuronInfo = BrainData.Current.NeuronInfo[neuron];

                        // we use the object and not the id, this is thread saver, but can result in a nul value.
                }
                else
                {
                    iRes.NeuronInfo = new NeuronData(neuron);
                }

                iRes.LinksIn = new DebugNeuronChildren(this, "In");
                iRes.LinksOut = new DebugNeuronChildren(this, "Out");
                iRes.ClusteredBy = new DebugNeuronChildren(this, "ClusteredBy");
                iRes.ChildLists.Add(LinksIn);
                iRes.ChildLists.Add(LinksOut);
                iRes.ChildLists.Add(ClusteredBy);
                if (neuron is NeuronCluster)
                {
                    if (((NeuronCluster)neuron).Meaning != Neuron.EmptyId)
                    {
                        iRes.fClusterMeaning = fClusterMeaning;
                    }

                    iRes.Children = new DebugNeuronChildren(iRes, "Children");
                    iRes.ChildLists.Add(Children);
                }

                iRes.IsMonitorEnabled = IsMonitorEnabled;
                iRes.IsTemp = IsTemp;
                return iRes;
            }

            return null;
        }

        #region Fields

        /// <summary>The f item.</summary>
        private Neuron fItem;

        /// <summary>The f is temp.</summary>
        private bool fIsTemp;

        /// <summary>The f is deleted.</summary>
        private bool fIsDeleted;

        /// <summary>The f neuron info.</summary>
        private NeuronData fNeuronInfo;

        /// <summary>The f links out.</summary>
        private DebugNeuronChildren fLinksOut;

        /// <summary>The f links in.</summary>
        private DebugNeuronChildren fLinksIn;

        /// <summary>The f children.</summary>
        private DebugNeuronChildren fChildren;

        /// <summary>The f cluster meaning.</summary>
        private Neuron fClusterMeaning;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f clustered by.</summary>
        private DebugNeuronChildren fClusteredBy;

        /// <summary>The f child lists.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<DebugNeuronChildren> fChildLists =
            new System.Collections.ObjectModel.ObservableCollection<DebugNeuronChildren>();

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f is monitor enabled.</summary>
        private bool fIsMonitorEnabled;

        #endregion

        #region ctor

        /// <summary>Prevents a default instance of the <see cref="DebugNeuron"/> class from being created. 
        ///     Initializes a new instance of the <see cref="DebugNeuron"/> class.
        ///     This is <see langword="private"/> since it is only used by the<see cref="DebugNeuron.Duplicate"/> method.</summary>
        private DebugNeuron()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DebugNeuron"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public DebugNeuron(Neuron toWrap)
        {
            System.Diagnostics.Debug.Assert(toWrap != null);
            fIsMonitorEnabled = true;
            fIsTemp = toWrap.ID == Neuron.TempId;
            Item = toWrap;
        }

        /// <summary>Initializes a new instance of the <see cref="DebugNeuron"/> class.</summary>
        /// <remarks>When <paramref name="isMonitorEnabled"/> is false, all the direct
        ///     children of the debugNeuron (linksIn, LinksOut, children, parents) are
        ///     immediatly loaded, so that they represent a snapshot picture of this
        ///     moment in time.</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isMonitorEnabled">if set to <c>true</c> [is monitor enabled].</param>
        public DebugNeuron(Neuron toWrap, bool isMonitorEnabled)
        {
            System.Diagnostics.Debug.Assert(toWrap != null);
            fIsMonitorEnabled = isMonitorEnabled;
            fIsTemp = toWrap.ID == Neuron.TempId;
            Item = toWrap;
            if (isMonitorEnabled)
            {
                foreach (var iChildren in ChildLists)
                {
                    iChildren.IsLoaded = true;
                }
            }
        }

        #endregion

        #region prop

        #region Item

        /// <summary>
        ///     Gets the neuron we provide a wrapper for.
        /// </summary>
        public Neuron Item
        {
            get
            {
                return fItem;
            }

            internal set
            {
                if (fItem != value)
                {
                    if (fItem != null)
                    {
                        // if was set to null, we are unregisered, so register again.
                        EventManager.Current.UnRegisterDebugNeuron(this);
                    }

                    fItem = value;
                    if (fItem != null)
                    {
                        if (fItem.ID != Neuron.TempId)
                        {
                            NeuronInfo = BrainData.Current.NeuronInfo[fItem];

                                // retrieving through the item instead of id should be more trhead save.
                        }
                        else
                        {
                            NeuronInfo = new NeuronData(fItem);
                        }

                        LinksIn = new DebugNeuronChildren(this, "In");
                        LinksOut = new DebugNeuronChildren(this, "Out");
                        ClusteredBy = new DebugNeuronChildren(this, "ClusteredBy");
                        ChildLists.Add(LinksIn);
                        ChildLists.Add(LinksOut);
                        ChildLists.Add(ClusteredBy);
                        if (value is NeuronCluster)
                        {
                            var iMeaning = ((NeuronCluster)value).Meaning;
                            if (iMeaning != Neuron.EmptyId)
                            {
                                SetClusterMeaning(Brain.Current[iMeaning]);
                            }

                            Children = new DebugNeuronChildren(this, "Children");
                            ChildLists.Add(Children);
                        }

                        if (IsMonitorEnabled)
                        {
                            EventManager.Current.RegisterDebugNeuron(this);
                        }
                    }
                    else
                    {
                        NeuronInfo = null;
                        LinksIn = null;
                        LinksOut = null;
                        ClusteredBy = null;
                        Children = null;
                        ChildLists.Clear();
                    }

                    OnPropertyChanged("Item");
                }
            }
        }

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

        #region NeuronTypeName

        /// <summary>
        ///     Gets the name of the type of neuron we wrap.
        /// </summary>
        /// <remarks>
        ///     Used as the tooltip of the image
        /// </remarks>
        public string NeuronTypeName
        {
            get
            {
                if (fItem != null)
                {
                    return fItem.GetType().Name;
                }

                return null;
            }
        }

        #endregion

        #region ChildLists

        /// <summary>
        ///     Gets the lists of all the Child containers for this object.
        /// </summary>
        /// <remarks>
        ///     This is a list containing
        ///     <see cref="JaStDev.HAB.Designer.DebugNeuron.LinksIn" /> ,
        ///     <see cref="JaStDev.HAB.Designer.DebugNeuron.LinksOut" /> and/or
        ///     <see cref="JaStDev.HAB.Designer.DebugNeuron.Children" /> . It is used
        ///     to fascilitate the WPF treeview control.
        /// </remarks>
        public System.Collections.ObjectModel.ObservableCollection<DebugNeuronChildren> ChildLists
        {
            get
            {
                return fChildLists;
            }
        }

        #endregion

        #region NeuronInfo

        /// <summary>
        ///     Gets the extra info that is stored for the neuron in the
        ///     <see cref="BrainData" /> .
        /// </summary>
        /// <remarks>
        ///     We use a field and don't dynamically retrieve this info when we need
        ///     it cause a DebugNeuron can exist for temporary neurons (have no valid
        ///     id yet), which don't have neuronData registered yet. We solve this by
        ///     keeping track of the temp status and using a temp neuron info object.
        /// </remarks>
        public NeuronData NeuronInfo
        {
            get
            {
                return fNeuronInfo;
            }

            internal set
            {
                fNeuronInfo = value;
                OnPropertyChanged("NeuronInfo");
            }
        }

        #endregion

        #region LinksOut

        /// <summary>
        ///     Gets the list of outgoing links.
        /// </summary>
        public DebugNeuronChildren LinksOut
        {
            get
            {
                return fLinksOut;
            }

            internal set
            {
                if (fLinksOut != value)
                {
                    if (value == null)
                    {
                        fLinksOut.UnloadChildren();
                    }

                    fLinksOut = value;
                }
            }
        }

        #endregion

        #region LinksIn

        /// <summary>
        ///     Gets the list of incomming links.
        /// </summary>
        public DebugNeuronChildren LinksIn
        {
            get
            {
                return fLinksIn;
            }

            internal set
            {
                if (fLinksIn != value)
                {
                    if (value == null)
                    {
                        fLinksIn.UnloadChildren();
                    }

                    fLinksIn = value;
                }
            }
        }

        #endregion

        #region ClusteredBy

        /// <summary>
        ///     Gets the list of clusters that own this neuron.
        /// </summary>
        public DebugNeuronChildren ClusteredBy
        {
            get
            {
                return fClusteredBy;
            }

            internal set
            {
                if (fClusteredBy != value)
                {
                    if (value == null)
                    {
                        fClusteredBy.UnloadChildren();
                    }

                    fClusteredBy = value;
                }
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children of this cluster.
        /// </summary>
        public DebugNeuronChildren Children
        {
            get
            {
                return fChildren;
            }

            internal set
            {
                if (fChildren != value)
                {
                    if (value == null)
                    {
                        fChildren.UnloadChildren();
                    }

                    fChildren = value;
                }
            }
        }

        #endregion

        #region IsNeuronCluster

        /// <summary>
        ///     Gets the wether this neuron is a cluster.
        /// </summary>
        public bool IsNeuronCluster
        {
            get
            {
                return fChildren != null;
            }
        }

        #endregion

        #region ClusterMeaning

        /// <summary>
        ///     Gets the neuron that represents the meaning of the cluster.
        /// </summary>
        public DebugNeuron ClusterMeaning
        {
            get
            {
                if (fClusterMeaning != null)
                {
                    return new DebugNeuron(fClusterMeaning);
                }

                return null;
            }
        }

        /// <summary>Sets the cluster meaning.</summary>
        /// <param name="value">The value.</param>
        internal void SetClusterMeaning(Neuron value)
        {
            fClusterMeaning = value;
            OnPropertyChanged("ClusterMeaning");
        }

        #endregion

        #region IsMonitorEnabled

        /// <summary>
        ///     Gets/sets a value indicating if the DebugNeuron should monitor any
        ///     changes to it's underlying neuron or not. Sometimes it is usefull to
        ///     maintain the state of the item at a specific point it time (like the
        ///     ProcessorOverview, which depicts the neurons as they were when they
        ///     were put on the stack.
        /// </summary>
        public bool IsMonitorEnabled
        {
            get
            {
                return fIsMonitorEnabled;
            }

            set
            {
                if (value != fIsMonitorEnabled)
                {
                    fIsMonitorEnabled = value;
                    if (value)
                    {
                        EventManager.Current.RegisterDebugNeuron(this);
                    }
                    else
                    {
                        EventManager.Current.UnRegisterDebugNeuron(this);
                    }
                }
            }
        }

        #endregion

        #region IsTemp

        /// <summary>
        ///     Gets/sets the if this neuron still has a temp id, or if it has been
        ///     fully registered with the brain.
        /// </summary>
        public bool IsTemp
        {
            get
            {
                return fIsTemp;
            }

            internal set
            {
                if (fIsTemp != value)
                {
                    fIsTemp = value;
                    if (fIsTemp == false)
                    {
                        // when turned off, we can stop monitoring the event + need to register the neurondata with the braindata manager.
                        var iNew = BrainData.Current.NeuronInfo[Item.ID];

                            // we copy over the data from the old item to the new (so we save any data that the user entered). we need to use the new item, to be in correlation with the rest of the system
                        if (NeuronInfo != null)
                        {
                            // if the neuron was deleted before we could render it.
                            iNew.CopyFrom(NeuronInfo);
                        }

                        NeuronInfo = iNew;
                    }

                    OnPropertyChanged("IsTemp");
                }
            }
        }

        #endregion

        #region IsDeleted

        /// <summary>
        ///     Gets wether the neuron has been deleted or not.
        /// </summary>
        public bool IsDeleted
        {
            get
            {
                return fIsDeleted;
            }

            internal set
            {
                fIsDeleted = value;
                OnPropertyChanged("IsDeleted");
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets wether this item is selected or not.
        /// </summary>
        /// <remarks>
        ///     This property value is controlled from within the
        ///     <see cref="DebugProcessor" /> .
        /// </remarks>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            internal set
            {
                fIsSelected = value;

                // Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(OnPropertyChanged), "IsSelected");
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets wether the UI tree item is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                fIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>Creates the children for the specified list.</summary>
        /// <param name="list">The list to fill with child elements</param>
        internal void CreateChildrenFor(DebugNeuronChildren list)
        {
            if (Item != null)
            {
                if (list == Children)
                {
                    if (((NeuronCluster)Item).ChildrenIdentifier != null)
                    {
                        // don't create list if not needed.
                        using (var iList = ((NeuronCluster)Item).Children)
                            foreach (var i in iList)
                            {
                                list.Children.Add(new DebugChild(i, this));
                            }
                    }
                }
                else if (list == LinksIn)
                {
                    if (Item.LinksInIdentifier != null)
                    {
                        using (var iLinks = Item.LinksIn)
                            foreach (var i in iLinks)
                            {
                                list.Children.Add(new DebugLink(i, this));
                            }
                    }
                }
                else if (list == LinksOut)
                {
                    if (Item.LinksOutIdentifier != null)
                    {
                        using (var iLinks = Item.LinksOut)
                            foreach (var i in iLinks)
                            {
                                list.Children.Add(new DebugLink(i, this));
                            }
                    }
                }
                else if (list == ClusteredBy && Item.ClusteredByIdentifier != null)
                {
                    using (var iList = Item.ClusteredBy)
                        foreach (var i in iList)
                        {
                            list.Children.Add(new DebugChild(i, this));
                        }
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("list");
                }
            }
        }

        /// <summary>Determines whether the specified <paramref name="list"/> has children.</summary>
        /// <param name="list">The list.</param>
        /// <returns><c>true</c> if the specified <paramref name="list"/> has children;
        ///     otherwise, <c>false</c> .</returns>
        internal bool HasChildren(DebugNeuronChildren list)
        {
            if (Item != null)
            {
                if (list == Children)
                {
                    if (((NeuronCluster)Item).ChildrenIdentifier != null)
                    {
                        using (var iList = ((NeuronCluster)Item).Children) return iList.Count > 0;
                    }

                    return false;
                }

                if (list == LinksIn)
                {
                    if (Item.LinksInIdentifier != null)
                    {
                        using (var iList = Item.LinksIn) return iList.Count > 0;
                    }

                    return false;
                }

                if (list == LinksOut)
                {
                    if (Item.LinksOutIdentifier != null)
                    {
                        using (var iList = Item.LinksOut) return iList.Count > 0;
                    }

                    return false;
                }

                if (list == ClusteredBy)
                {
                    if (Item.ClusteredByIdentifier != null)
                    {
                        using (var iList = Item.ClusteredBy) return iList.Count > 0;
                    }

                    return false;
                }

                throw new System.ArgumentOutOfRangeException("list");
            }

            return false;
        }

        #endregion
    }
}