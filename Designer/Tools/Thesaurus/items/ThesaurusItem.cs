// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusItem.cs" company="">
//   
// </copyright>
// <summary>
//   A single item in the thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A single item in the thesaurus.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A thesaurus item finds it's children in the following manner:
    ///         - check if the wrapped item is owned by clusters that have the meaning = relations, if so, for all clusters:
    ///         - look for incomming links on those cluster with the specified meaning,
    ///         all the starting points form the children.
    ///         This algorithm comes from the fact that the relationships are stored in reverse order: an object points to a
    ///         cluster with
    ///         all the related items for 1 relationship, so in reverse order can an object be located in multiple lists.
    ///     </para>
    ///     <para>
    ///         For this reason, we need to do some custom monitoring of links and clusters in order to keep it all in sync.
    ///         We need to keep an eye on the cluster content changes to see if our wrapped object is added/removed to items.
    ///         We also need to keep an eye on all the links to see if they are changed/deleted. This will also keep track if
    ///         any objects are deleted: the links will also be deleted.
    ///     </para>
    /// </remarks>
    public class ThesaurusItem : Data.OwnedObject<IThesaurusData>, 
                                 IThesaurusData, 
                                 INeuronWrapper, 
                                 INeuronInfo, 
                                 WPF.Controls.ITreeViewPanelItem, 
                                 Data.INotifyCascadedPropertyChanged, 
                                 Data.ICascadedNotifyCollectionChanged, 
                                 Data.IOnCascadedChanged
    {
        #region inner types

        /// <summary>
        ///     stores the mapping of a single personal mapping item (I, you, he, she,...) and wheter it was active or not for this
        ///     item. This
        ///     is used by the context menu, so you can turn it on, off.
        /// </summary>
        public class PersonMapping : Data.ObservableObject, INeuronWrapper, INeuronInfo
        {
            #region ctor

            /// <summary>Initializes a new instance of the <see cref="PersonMapping"/> class.</summary>
            /// <param name="map">The map.</param>
            /// <param name="item">The item.</param>
            public PersonMapping(ulong map, NeuronCluster item)
            {
                fItem = item;
                fMap = map;
                if (fItem.ChildrenIdentifier != null)
                {
                    using (var iChildren = fItem.Children) fIsSelected = iChildren.Contains(fMap);
                }
                else
                {
                    fIsSelected = false;
                }
            }

            #endregion

            #region IsSelected

            /// <summary>
            ///     Gets/sets the value that indicates of this personal mapping is active or not. When the value changes, the
            ///     underlying object is updated.
            /// </summary>
            public bool IsSelected
            {
                get
                {
                    return fIsSelected;
                }

                set
                {
                    if (value != fIsSelected)
                    {
                        fIsSelected = value;
                        using (var iList = fItem.ChildrenW)
                            if (value)
                            {
                                iList.Add(fMap);
                            }
                            else
                            {
                                iList.Remove(fMap);
                            }

                        OnPropertyChanged("IsSelected");
                    }
                }
            }

            #endregion

            #region INeuronInfo Members

            /// <summary>
            ///     Gets the extra info for the specified neuron.  Can be null.
            /// </summary>
            public NeuronData NeuronInfo
            {
                get
                {
                    return BrainData.Current.NeuronInfo[fMap];
                }
            }

            #endregion

            #region INeuronWrapper Members

            /// <summary>
            ///     Gets the item.
            /// </summary>
            public Neuron Item
            {
                get
                {
                    return Brain.Current[fMap];
                }
            }

            #endregion

            #region fields

            /// <summary>The f is selected.</summary>
            private bool fIsSelected;

            /// <summary>The f map.</summary>
            private readonly ulong fMap;

            /// <summary>The f item.</summary>
            private readonly NeuronCluster fItem;

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>The f items.</summary>
        private ThesChildItemsCollection fItems;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f has items.</summary>
        private bool fHasItems;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f needs bring into view.</summary>
        private bool fNeedsBringIntoView;

        /// <summary>The f is place holder.</summary>
        private bool? fIsPlaceHolder;

        /// <summary>The f has text pattern.</summary>
        private bool? fHasTextPattern;

        /// <summary>The f is attribute.</summary>
        private bool? fIsAttribute;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ThesaurusItem"/> class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        /// <param name="relation">The relation.</param>
        public ThesaurusItem(ulong toWrap, Neuron relation)
        {
            ID = toWrap;
            CheckHasItems(relation);
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaurusItem"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        /// <param name="relation">The relation.</param>
        public ThesaurusItem(Neuron toWrap, Neuron relation)
        {
            ID = toWrap.ID;
            CheckHasItems(relation);
        }

        #endregion

        #region Events  (NotifyCascadedPropertyChanged Members + ICascadedNotifyCollectionChanged Members)

        /// <summary>
        ///     Occurs when a property was changed in one of the thesaurus items. This is used for the tree display (only root
        ///     objects get events).
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when a collection was changed in one of the child items or the root list. This is used for the tree display
        ///     (only root objects get events).
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region Prop

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the list is loaded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (value != fIsExpanded)
                {
                    // OnPropertyChanging("IsExpanded", fIsExpanded, value);                               //we need this for the treeviews.
                    if (value)
                    {
                        LoadItems(Root.SelectedRelationship);
                    }
                    else
                    {
                        UnloadItems();
                    }
                }
            }
        }

        /// <summary>Called when the IsExpanded prop is changed.</summary>
        /// <param name="value">The value.</param>
        private void OnIsExpandedChanged(bool value)
        {
            fIsExpanded = value;

                // do before raising event, otherwise things go wrong (It's a changed event, so happens after assign).
            OnPropertyChanged("IsExpanded");
            var iArgs = new Data.CascadedPropertyChangedEventArgs(
                this, 
                new System.ComponentModel.PropertyChangedEventArgs("IsExpanded"));
            Data.EventEngine.OnPropertyChanged(this, iArgs);
        }

        #endregion

        #region IsAttribute

        /// <summary>
        ///     Gets/sets the value that indicates if this thesaurus item can be used as a default attribute value or not.
        /// </summary>
        public bool IsAttribute
        {
            get
            {
                if (fIsAttribute.HasValue == false)
                {
                    CheckIsAttribute();
                }

                return fIsAttribute.Value;
            }

            set
            {
                if (value != fIsAttribute)
                {
                    if (value)
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Attribute, Item);
                    }
                    else
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Attribute, (Neuron)null);
                    }
                }
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets wether this item is currently the selected one or not.
        /// </summary>
        /// <remarks>
        ///     Also updates the <see cref="Thesaurus.SelectedItem" />
        /// </remarks>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                    var iRoot = Root;
                    if (iRoot != null)
                    {
                        if (iRoot.SelectedItem != null && iRoot.SelectedItem != this)
                        {
                            iRoot.SelectedItem.IsSelected = false;
                        }

                        iRoot.SelectedItem = value ? this : null;
                    }
                }
            }
        }

        #endregion

        #region NeedsBringIntoView

        /// <summary>
        ///     Gets/sets the wether this item needs to be brought into view. This is a way to communicate with wpf.
        /// </summary>
        public bool NeedsBringIntoView
        {
            get
            {
                return fNeedsBringIntoView;
            }

            set
            {
                if (fNeedsBringIntoView != value)
                {
                    fNeedsBringIntoView = value;
                    OnPropertyChanged("NeedsBringIntoView");
                    var iArgs = new Data.CascadedPropertyChangedEventArgs(
                        this, 
                        new System.ComponentModel.PropertyChangedEventArgs("NeedsBringIntoView"));
                    Data.EventEngine.OnPropertyChanged(this, iArgs);
                }
            }
        }

        #endregion

        #region HasItems

        /// <summary>
        ///     Gets the wether the item has any children or not. Similar as <see cref="ThesaurusItem.HasChildren" />
        /// </summary>
        public bool HasItems
        {
            get
            {
                return fHasItems;
            }

            internal set
            {
                if (fHasItems != value)
                {
                    fHasItems = value;
                    OnPropertyChanged("HasItems");
                    OnPropertyChanged("HasChildren");
                }
            }
        }

        #endregion

        #region IsPlaceHolder

        /// <summary>
        ///     Gets/sets the value that indicates if this thesaurus is a placeholder or not.
        ///     Placeholders
        /// </summary>
        public bool IsPlaceHolder
        {
            get
            {
                if (fIsPlaceHolder.HasValue == false)
                {
                    CheckIsPlaceHolder();
                }

                return fIsPlaceHolder.Value;
            }

            internal set
            {
                fIsPlaceHolder = value;
                OnPropertyChanged("IsPlaceHolder");
            }
        }

        #endregion

        #region IThesaurusData Members

        #region Items

        /// <summary>
        ///     Gets the list of items
        /// </summary>
        /// <value></value>
        public System.Collections.Generic.IList<ThesaurusItem> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region Root

        /// <summary>
        ///     Gets the root of the thesaurus
        /// </summary>
        /// <value>The root.</value>
        public Thesaurus Root
        {
            get
            {
                var iOwner = Owner;
                while (iOwner != null)
                {
                    // orinally used recursion here, we need to unrap this since it caused stack overflows.
                    if (iOwner is Thesaurus)
                    {
                        return (Thesaurus)iOwner;
                    }

                    iOwner = ((ThesaurusItem)iOwner).Owner;
                }

                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the parent tree item.
        /// </summary>
        /// <value>The parent tree item.</value>
        public WPF.Controls.ITreeViewPanelItem ParentTreeItem
        {
            get
            {
                return Owner as WPF.Controls.ITreeViewPanelItem;
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public Neuron Item
        {
            get
            {
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(ID, out iFound))
                {
                    return iFound;
                }

                return null;
            }
        }

        #endregion

        #region ID

        /// <summary>
        ///     Gets the id of the neuron that we are wrapping.
        /// </summary>
        public ulong ID { get; private set; }

        #endregion

        #region NeuronInfo(INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron.  Can be null.
        /// </summary>
        /// <value></value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (ID != Neuron.EmptyId)
                {
                    return BrainData.Current.NeuronInfo[ID];
                }

                return null;
            }
        }

        #endregion

        #region ITreeViewPanelItem Members

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not. When the list changes, this should be
        ///     raised when appropriate through a propertyChanging event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren
        {
            get
            {
                return HasItems;
            }
        }

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>The tree items.</value>
        public System.Collections.IList TreeItems
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region POS

        /// <summary>
        ///     Gets the POS for the thesaurus item. This is extracted from the neuron.
        /// </summary>
        /// <value>The POS.</value>
        public Neuron POS
        {
            get
            {
                var iItem = Item;
                var iFound = iItem.FindFirstOut((ulong)PredefinedNeurons.POS);
                if (iFound != null)
                {
                    return iFound;
                }

                var iParent = iItem.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);
                if (iParent != null)
                {
                    return iParent.FindFirstOut((ulong)PredefinedNeurons.POS);
                }

                var iOwner = Owner as ThesaurusItem;

                    // the pos can also be stored recursively, so ask the parent if it has a pos (which will do it recursivelly untill there is no more parent).
                if (iOwner != null && iOwner.POS != null)
                {
                    return iOwner.POS;
                }

                return null;
            }
        }

        #endregion

        #region HasTextPattern

        /// <summary>
        ///     Gets the value that indicates if there is a textpattern-editor present for this thesaurus item.
        /// </summary>
        public bool HasTextPattern
        {
            get
            {
                if (fHasTextPattern.HasValue == false)
                {
                    CheckEditors();
                }

                return fHasTextPattern.Value;
            }

            internal set
            {
                fHasTextPattern = value;
                OnPropertyChanged("HasTextPattern");
            }
        }

        #endregion

        #region PersonalMappings

        /// <summary>
        ///     Gets the name of the object
        /// </summary>
        public System.Collections.Generic.List<PersonMapping> PersonalMappings
        {
            get
            {
                var iRes = new System.Collections.Generic.List<PersonMapping>();
                foreach (var i in BrainData.Current.PersonMapIds)
                {
                    iRes.Add(new PersonMapping(i, (NeuronCluster)Item));
                }

                return iRes;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Checks if there are any  editors present for this item that need to be depicted.
        /// </summary>
        internal void CheckEditors()
        {
            var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic);
            HasTextPattern = iFound != null; // have to assign attrib, cause this call is also used to update the prop.
        }

        /// <summary>
        ///     Checks if there are any  editors present for this item that need to be depicted.
        /// </summary>
        internal void CheckIsAttribute()
        {
            var iItem = Item;
            fIsAttribute = Link.Exists(iItem, iItem, (ulong)PredefinedNeurons.Attribute);
        }

        /// <summary>The reset attrib.</summary>
        internal void ResetAttrib()
        {
            fIsAttribute = null;
            OnPropertyChanged("IsAttribute");
        }

        /// <summary>
        ///     Checks if this is a place holder.
        /// </summary>
        private void CheckIsPlaceHolder()
        {
            var iCluster = Item as NeuronCluster;
            if (iCluster != null && iCluster.ChildrenIdentifier != null)
            {
                System.Collections.Generic.List<Neuron> iList;
                using (var iChildren = iCluster.ChildrenW) iList = iChildren.ConvertTo<Neuron>();
                foreach (var i in iList)
                {
                    if (i is TextNeuron)
                    {
                        IsPlaceHolder = false;
                        return;
                    }

                    var iChild = i as NeuronCluster;
                    if (iChild != null && iChild.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                    {
                        IsPlaceHolder = false;
                        return;
                    }
                }

                Factories.Default.NLists.Recycle(iList);
            }

            fIsPlaceHolder = true;
        }

        /// <summary>Checks if the item to wrap has any children.</summary>
        /// <param name="relation">The relation.</param>
        internal void CheckHasItems(Neuron relation)
        {
            var iItem = Item;
            var iFound = iItem.FindFirstOut(relation.ID) as NeuronCluster;
            if (iFound == null)
            {
                IsExpanded = false;
                HasItems = false;
            }
            else
            {
                HasItems = true;
            }
        }

        /// <summary>searches for and loads all the items that are related to the wrapped item according to the specified relationship.</summary>
        /// <param name="relation">The relation.</param>
        internal void LoadItems(Neuron relation)
        {
            var iItem = Item;
            var iFound = iItem.FindFirstOut(relation.ID) as NeuronCluster;
            if (iFound != null)
            {
                fItems = new ThesChildItemsCollection(this, iFound);
            }
            else
            {
                fItems = new ThesChildItemsCollection(this, relation.ID);
            }

            ThesaurusItemCollectionEventMonitor.Default.AddList(fItems);
            OnPropertyChanged("Items");
            OnPropertyChanged("TreeItems");
            OnIsExpandedChanged(true);

                // important: needs to be called after loading items, otherwise the UI can't calculate the amount of items to remove + which items to render.
        }

        /// <summary>
        ///     Unloads the list with child items.
        /// </summary>
        internal void UnloadItems()
        {
            OnIsExpandedChanged(false);

                // important: needs to be called before unloading items, otherwise the UI can't calculate the amount of items to remove.
            ThesaurusItemCollectionEventMonitor.Default.RemoveList(fItems);
            fItems = null;
            OnPropertyChanged("Items");

                // make certain that when the items are unloaded, we let the ui know there are no items anymore.
            OnPropertyChanged("TreeItems");
        }

        /// <summary>Raises the <see cref="E:CollectionChanging"/> event.</summary>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.CollectionChangingEventArgs"/> instance containing the
        ///     event data.</param>
        protected override void OnCollectionChanging(UndoSystem.Interfaces.CollectionChangingEventArgs e)
        {
            base.OnCollectionChanging(e);
        }

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }

            var iSource = args.OriginalSource as System.Collections.IList;
            if (iSource != null)
            {
                if (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                        && iSource.Count == 1)
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                        && iSource.Count == 0))
                {
                    OnPropertyChanged("HasChildren");
                }
            }
            else
            {
                OnPropertyChanged("HasChildren");

                    // if we can't check agains the nr of existing children, always let it update.
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion

        #endregion
    }
}