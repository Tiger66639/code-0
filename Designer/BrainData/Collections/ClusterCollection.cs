// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for all Collections that wrap a <see cref="NeuronCluster" />
//   . This collection will automatically keep sync with the content of a
//   cluster. The actual item in the collection is generic, as long at it's a
//   neuron wrapper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>A base class for all Collections that wrap a <see cref="NeuronCluster"/>
    ///     . This collection will automatically keep sync with the content of a
    ///     cluster. The actual item in the collection is generic, as long at it's a
    ///     neuron wrapper.</summary>
    /// <remarks>doesn't raise events when the list is changed cause this confuses the
    ///     undo system. Instead, drag handlers and functions generate their own undo
    ///     data.</remarks>
    /// <typeparam name="T">The object type used to wrap the children of the cluster with.</typeparam>
    public abstract class ClusterCollection<T> : Data.ObservedCollection<T>, INeuronWrapper
        where T : INeuronWrapper
    {
        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <remarks>
        ///     Returns the same value as
        ///     <see cref="JaStDev.HAB.Designer.ClusterCollection`1.Cluster" />
        /// </remarks>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item
        {
            get
            {
                return Cluster;
            }
        }

        #endregion

        #region abstract

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public abstract T GetWrapperFor(Neuron toWrap);

        #endregion

        /// <summary>Called when the link between the owner and the cluster was changed.
        ///     This will reload the data in the collection.</summary>
        /// <remarks>Note: doesn't check that the new <paramref name="value"/> is the same
        ///     as the old cluster. Can not be null, use<see cref="ClusterCollection.RemoveCollection"/> for this.</remarks>
        /// <param name="value">The value.</param>
        internal void UpdateCluster(NeuronCluster value)
        {
            System.Diagnostics.Debug.Assert(value != null);
            if (Cluster.ID != Neuron.TempId)
            {
                // if there was a previous cluster, clear out all the data.
                ClearItemsDirect(); // we clear without undo event data
                if (IsActive)
                {
                    UnloadEventMonitor();
                }
            }

            LoadList(value);
            if (IsActive)
            {
                LoadEventMonitor();
            }
        }

        /// <summary>Removes the cluster, unloads all the data associated with it and
        ///     creates a new temp cluster.</summary>
        /// <param name="meaning">The meaning.</param>
        internal void RemoveCluster(ulong meaning)
        {
            if (IsActive)
            {
                // we unload the event monitor, cause the cluster object will change, which means a new internal list to monitor.
                UnloadEventMonitor();
            }

            if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
            {
                RemoveCusterInternal(meaning);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new System.Action<ulong>(RemoveCusterInternal), 
                    meaning); // if not on ui thread, do async cause we are modifying a UI list.
            }
        }

        /// <summary>The remove custer internal.</summary>
        /// <param name="meaning">The meaning.</param>
        private void RemoveCusterInternal(ulong meaning)
        {
            ClearItemsDirect();
            CreateTempList(meaning, Cluster.Meaning);
        }

        #region internal types

        /// <summary>
        ///     Stores the link and cluster meaning for a temp neuron for as long as
        ///     it isn't created (assigning cluster meaning to temp will register it)
        /// </summary>
        protected internal class MeaningInfo
        {
            /// <summary>Gets or sets the link meaning.</summary>
            public ulong LinkMeaning { get; set; }

            /// <summary>Gets or sets the cluster meaning.</summary>
            public ulong ClusterMeaning { get; set; }
        }

        #endregion

        #region fields

        /// <summary>The f link meaning.</summary>
        protected MeaningInfo fLinkMeaning;

                              // if it is a temp cluster, it means we need to add the NeuronCluster to the owner.Item.  null means that it is already added or not needed.

        /// <summary>The f is active.</summary>
        private bool fIsActive = true;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ClusterCollection{T}"/> class. Default constructor to use with a <see cref="NeuronCluster"/> that is
        ///     already registered with the owner, and therefor possibly already has
        ///     code. When you need to do something before loading the children during
        ///     construction, use the <see langword="protected"/> version of this
        ///     constructor (just below this one).</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public ClusterCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner)
        {
            IsInternalChange = false;
            LoadList(childList);
            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="ClusterCollection{T}"/> class. The <see langword="protected"/> version of this constructor. It has
        ///     the list of arguments inverted so that there is a difference in
        ///     signature. It doesn't load the children or init the list, this needs
        ///     to be done by the inheriters.</summary>
        /// <param name="childList"></param>
        /// <param name="owner"></param>
        protected ClusterCollection(NeuronCluster childList, INeuronWrapper owner)
            : base(owner)
        {
            IsInternalChange = false;
        }

        /// <summary>Loads all the items This is <see langword="protected"/> so that
        ///     inheriters can do stuff before loading the children. To do this, use
        ///     the <see langword="protected"/> version of the constructor</summary>
        /// <param name="childList">The child list.</param>
        protected void LoadList(NeuronCluster childList)
        {
            System.Diagnostics.Debug.Assert(childList != null);
            Cluster = childList;
            System.Collections.Generic.List<Neuron> iItems;
            using (var iList = childList.Children) iItems = iList.ConvertTo<Neuron>();
            if (iItems != null)
            {
                foreach (var i in iItems)
                {
                    InsertItemDirect(Count, GetWrapperFor(i)); // we take the base, otherwise we would reinsert it again.
                }
            }
            else
            {
                ProjectManager.Default.DataError = true;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ClusterCollection{T}"/> class. Default constructor to use with a <see cref="NeuronCluster"/> that is
        ///     already registered with the owner, and therefor possibly already has
        ///     code.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        /// <param name="isActive">if set to <c>true</c> the collection should be active, so it should
        ///     monitor any changes. Otherwise, the list wont monitor any changes.</param>
        public ClusterCollection(INeuronWrapper owner, NeuronCluster childList, bool isActive)
            : base(owner)
        {
            IsInternalChange = false;
            System.Diagnostics.Debug.Assert(childList != null);
            Cluster = childList;

            fIsActive = isActive;
            System.Collections.Generic.List<Neuron> iItems;
            using (var iList = childList.Children) iItems = iList.ConvertTo<Neuron>();
            if (iItems != null)
            {
                try
                {
                    foreach (var i in iItems)
                    {
                        InsertItemDirect(Count, GetWrapperFor(i));

                            // we take the base, otherwise we would reinsert it again.
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iItems);
                }
            }

            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="ClusterCollection{T}"/> class. Default constructor to use for a <see cref="NeuronCluster"/> that is
        ///     not yet declared (empty, and therefor still needs to be created,
        ///     specifically for code or argument lists (will automatically generate a
        ///     cluster meaning depending on the linkmeaning).</summary>
        /// <remarks>This cluster will only be registered if data is added. This prevents
        ///     us from creating clusters that only to view the code.</remarks>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="linkMeaning">The id that should be used as meaning if data is added to the list.</param>
        public ClusterCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner)
        {
            IsInternalChange = false;
            CreateTempList(linkMeaning, Neuron.EmptyId);
        }

        /// <summary>Initializes a new instance of the <see cref="ClusterCollection{T}"/> class. Default constructor to use for a <see cref="NeuronCluster"/> that is
        ///     not yet declared (empty, and therefor still needs to be created.</summary>
        /// <remarks>This cluster will only be registered if data is added. This prevents
        ///     us from creating clusters that only to view the code.</remarks>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="linkMeaning">The id that should be used as meaning if data is added to the list.</param>
        /// <param name="clusterMeaning">The id that should be used as the meaning for the cluster.</param>
        public ClusterCollection(INeuronWrapper owner, ulong linkMeaning, ulong clusterMeaning)
            : base(owner)
        {
            IsInternalChange = false;
            CreateTempList(linkMeaning, clusterMeaning);
        }

        /// <summary>Creates a temp cluster to store any data that is added to the
        ///     collection.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <param name="meaning">The meaning.</param>
        private void CreateTempList(ulong linkMeaning, ulong meaning)
        {
            Cluster = NeuronFactory.GetCluster();
            Brain.Current.MakeTemp(Cluster);

                // if we don't do this, the cluster has the 0 id, which is not registered with the brain, this will register the neuron, the first time it is used in a link.
            fLinkMeaning = new MeaningInfo { LinkMeaning = linkMeaning };
            fLinkMeaning.ClusterMeaning = (meaning != Neuron.EmptyId) ? meaning : GetListMeaning(linkMeaning);
            InternalCreate();
        }

        /// <summary>
        ///     Internally creates the item. This is <see langword="protected" /> so
        ///     that inheriters can do stuff before loading the children. To do this,
        ///     use the <see langword="protected" /> version of the constructor
        /// </summary>
        protected void InternalCreate()
        {
            if (IsActive)
            {
                LoadEventMonitor();
            }
        }

        /// <summary>
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="CodeItemCollection" /> is reclaimed by garbage
        ///     collection.
        /// </summary>
        private void UnloadEventMonitor()
        {
            EventManager.Current.UnRegisterClusterCollection(this);
        }

        /// <summary>The load event monitor.</summary>
        private void LoadEventMonitor()
        {
            EventManager.Current.RegisterClusterCollection(this);
        }

        #endregion

        #region prop

        #region Cluster

        /// <summary>
        ///     Gets the cluster used to store all the code items in the brain.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public NeuronCluster Cluster { get; private set; }

        /// <summary>Replaces the cluster in case that the object got replaced (should
        ///     normally not happen, but can technically be executed). The content of
        ///     the list should not be changed.</summary>
        /// <param name="value">The value.</param>
        internal void ReplaceCluster(NeuronCluster value)
        {
            Cluster = value;
        }

        #endregion

        #region MeaningID

        /// <summary>
        ///     Gets the ID of the meaning that is assigned to the cluster.
        /// </summary>
        /// <remarks>
        ///     This property is provided so that you can easely access this value.
        ///     <see cref="JaStDev.HAB.Designer.ClusterCollection`1.Cluster" /> only
        ///     gets created when there is data, but the collection already knows
        ///     which list type to use, so it can return this value.
        /// </remarks>
        /// <value>
        ///     The meaning.
        /// </value>
        public ulong MeaningID
        {
            get
            {
                if (fLinkMeaning != null)
                {
                    // this field is also used as a switch, when it is null, it means that there is a cluster created.
                    return fLinkMeaning.ClusterMeaning;
                }

                return Cluster.Meaning;
            }
        }

        #endregion

        #region IsInternalChange

        /// <summary>
        ///     Gets a value indicating whether the current action is trigged through
        ///     an <see langword="internal" /> change or not. This is used by the event
        ///     monitor to check (in the correct thread) if the item needs to receive
        ///     an event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is <see langword="internal" /> change;
        ///     otherwise, <c>false</c> .
        /// </value>
        internal bool IsInternalChange { get; private set; }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets wether the collection is monitoring the network for changes.
        ///     This is also valid for all child items.
        /// </summary>
        /// <remarks>
        ///     When true, the collection uses more system resources. False is used
        ///     for displaying the code being executed, this can not change.
        /// </remarks>
        public virtual bool IsActive
        {
            get
            {
                return fIsActive;
            }

            set
            {
                if (value != fIsActive)
                {
                    fIsActive = value;
                    if (value)
                    {
                        LoadEventMonitor();
                    }
                    else
                    {
                        UnloadEventMonitor();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether to use the first outgoing link to find
        ///     the code cluster or a specific link.
        /// </summary>
        /// <remarks>
        ///     By default, we use the first outgoing link from an owner to a cluster
        ///     for generating this collection. If you want to use a specific link,
        ///     overwrite this prop.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if [use first out]; otherwise, <c>false</c> .
        /// </value>
        public virtual bool UseFirstOut
        {
            get
            {
                return true;
            }
        }

        /*
 * Possible future extention for virtualization.
 * 
      #region ChildrenLoaded

      /// <summary>
      /// Gets/sets if the <see cref="CodeItem.Children"/> list is loaded or not.
      /// </summary>
      /// <remarks>
      /// This allows for dynamic UI loading.
      /// </remarks>
      public bool ChildrenLoaded
      {
         get
         {
            return fChildrenLoaded;
         }
         set
         {
            if (fChildrenLoaded != value)
            {
               fChildrenLoaded = value;
               if (fChildren != null)                                            //only try to do something if there is a list to work on, this can be null if HasChildren is false.
               {
                  if (value == true)
                     LoadChildren();
                  else
                     fChildren.Clear();
               }
               OnPropertyChanged("ChildrenLoaded");
            }
         }
      }

      private void LoadChildren()
      {
         ConditionalStatement iGroup = (ConditionalStatement)fItem;
         foreach (ConditionalExpression i in iGroup.Conditions)
            fChildren.Add(new CodeItem(i));
      }

      #endregion

      #region HasChildren

      /// <summary>
      /// Gets if this <see cref="CodeItem.Expression"/> has child expressions.
      /// </summary>
      /// <remarks>
      /// This property is automatically set to true when a <see cref="conditionalGroup"/> or
      /// <see cref="conditionalExpression"/> is wrapped.
      /// </remarks>
      public bool HasChildren
      {
         get
         {
            NeuronCluster iCluster = ((ConditionalStatement)Item).ConditionsCluster;
            return iCluster != null && iCluster.Children.Count > 0;
         }
      }

      #endregion
 */
        #endregion

        #region Functions

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected abstract ulong GetListMeaning(ulong linkMeaning);

        /// <summary>Called when the underlying list (fChildList.Children) is changed.
        ///     Makes certain that changes are reflected in this list as well.</summary>
        /// <param name="e">the events arguments</param>
        internal void NeuronlistChanged(NeuronListChangedEventArgs e)
        {
            if (Cluster != null)
            {
                // when calling async, the cluster could already ahve been unloaded, if so, don't try to sync anymore.
                if (e.ListOwner == Cluster)
                {
                    // when a branch of code gets deleted, the list could be cleared and than deleted, or the link between owner and list could be destroyed which calls 'RemoveCluster', which resets the content, afterwhich the async call of the remove gets in. This needs to be caught and filtered out.
                    switch (e.Action)
                    {
                        case NeuronListChangeAction.Insert:
                            InsertItemDirect(e.Index, GetWrapperFor(e.Item));
                            break;
                        case NeuronListChangeAction.Remove:
                            if (e.Index >= 0 && e.Index < Count)
                            {
                                RemoveItemDirect(e.Index);
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "ClusterCollection.NeuronListChanged", 
                                    "Failed to sync the collection with the cluster: index out of range");
                            }

                            break;
                        default:
                            throw new System.InvalidOperationException("Unkown list change.");
                    }
                }
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="original">The original.</param>
        /// <param name="neuron">The neuron.</param>
        internal void NeuronChanged(Neuron original, Neuron neuron)
        {
            for (var i = 0; i < Count; i++)
            {
                // 1 neuron can be contained many times in a
                if (this[i].Item == original)
                {
                    this[i] = GetWrapperFor(neuron);
                }
            }
        }

        /// <summary>
        ///     We don't call base, this is done by the event handler that monitors
        ///     changes to the list. If we don't use this technique, actions performed
        ///     by the user will be done 2 times: once normally, once in response to
        ///     the list change in the neuron.
        /// </summary>
        protected override void ClearItems()
        {
            var iList = Cluster.ChildrenW;

                // we lock before we set fInternalChange to make certain no one can change something in between calls
            var iAlsoLocked = iList.LockAll();
            IsInternalChange = true;
            try
            {
                var iUndo = new ResetClusterUndoItem();
                iUndo.Cluster = Cluster;
                iUndo.Items = iAlsoLocked; // we don't recycle the list so we can assign it directly.
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                iList.ClearUnsafe(iAlsoLocked);
                ClearItemsDirect();
            }
            finally
            {
                IsInternalChange = false;
                iList.Unlock(iAlsoLocked);
                iList.Dispose();

                // -> don't recycle the list, it's used in the undo data. Factories.Default.NLists.Recycle(iAlsoLocked);
            }
        }

        ///// <summary>
        ///// Adds the item without raising events.
        ///// </summary>
        ///// <param name="item">The item.</param>
        // public void AddWithoutEvents(T item)
        // {
        // using (ChildrenAccessor iList = fChildList.ChildrenW)                                                                //we lock before we set fInternalChange to make certain no one can change something in between calls
        // {
        // fInternalChange = true;
        // try
        // {
        // if (fLinkMeaning != Neuron.EmptyId)                                                                //need to register the NeuronCluster
        // {
        // Link iLink = new Link(fChildList, ((INeuronWrapper)Owner).Item, fLinkMeaning);
        // fLinkMeaning = Neuron.EmptyId;                                                                  //need to indicate that the action is completed.
        // }
        // iList.Add(item.Item);
        // base.InsertItemDirect(Count, item);
        // }
        // finally
        // {
        // fInternalChange = false;
        // }
        // }
        // }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, T item)
        {
            var iList = Cluster.ChildrenW;
            iList.Lock(item.Item);

                // we lock before we set fInternalChange to make certain no one can change something in between calls
            IsInternalChange = true;
            try
            {
                if (fLinkMeaning != null)
                {
                    // need to register the NeuronCluster
                    CreateOwnerLink();
                }
                else if (Cluster.ID == Neuron.TempId)
                {
                    Brain.Current.Add(Cluster);
                    Cluster.Meaning = GetListMeaning(Neuron.EmptyId);

                        // assign the meaning if possible, to a stand alone cluster.
                }

                var iUndo = new AddClusterUndoItem();
                iUndo.Cluster = Cluster;
                iUndo.Items = new System.Collections.Generic.List<Neuron> { item.Item };
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                iList.InsertUnsafe(index, item.Item);
                InsertItemDirect(index, item);
            }
            finally
            {
                IsInternalChange = false;
                iList.Unlock(item.Item);
                iList.Dispose();
            }
        }

        /// <summary>
        ///     Makes certain that the link with the owner is created. This can be
        ///     used if the cluster was added to the brain manually, when it was still
        ///     a temp cluster. In this case, the link isn't created yet. This is a
        ///     safe way for doing this.
        /// </summary>
        public void CreateOwnerLinkIfNeeded()
        {
            if (fLinkMeaning != null)
            {
                CreateOwnerLink();
            }
        }

        /// <summary>
        ///     Creates the link between the owner and the cluster. This is a
        ///     <see langword="virtual" /> function so that descendents can change the
        ///     behaviour.
        /// </summary>
        protected virtual void CreateOwnerLink()
        {
            var iIsTemp = Cluster.ID == Neuron.TempId;

                // if it is a temp, we need to create undo data for the neuron, otherwise we need to creat undo data for the link
            var iOwner = Owner as IInternalChange;
            if (iOwner != null)
            {
                iOwner.InternalChange = true;

                    // need to make certain that the owner wont create a new list. If this happens, and there is a fast process happening, some things can get out of sync and lists can get reversed.
            }

            try
            {
                if (iIsTemp)
                {
                    WindowMain.AddItemToBrain(Cluster); // easiest way to add to undo.
                }

                var iLink = new Link(Cluster, ((INeuronWrapper)Owner).Item, fLinkMeaning.LinkMeaning);

                    // simply create link, don't need to check if already created, cause this is caught by the event monitor.
                Cluster.Meaning = fLinkMeaning.ClusterMeaning;
                fLinkMeaning = null; // need to indicate that the action is completed.
                if (iIsTemp == false)
                {
                    var iUndo = new LinkUndoItem(iLink, BrainAction.Created); // we need to create undo data.
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                }
            }
            finally
            {
                if (iOwner != null)
                {
                    iOwner.InternalChange = false;
                }
            }
        }

        ///// <summary>
        ///// Performs an insert which generates events.
        ///// </summary>
        ///// <param name="index">The index.</param>
        ///// <param name="value">The value.</param>
        // public void InsertWithoutEvents(int index, T value)
        // {
        // if (index > -1 && index < Count)
        // {
        // using (ChildrenAccessor iList = fChildList.ChildrenW)                                                                //we lock before we set fInternalChange to make certain no one can change something in between calls
        // {
        // fInternalChange = true;
        // try
        // {
        // if (fLinkMeaning != Neuron.EmptyId)                                                                //need to register the NeuronCluster
        // {
        // Link iLink = new Link(fChildList, ((INeuronWrapper)Owner).Item, fLinkMeaning);
        // fLinkMeaning = Neuron.EmptyId;                                                                  //need to indicate that the action is completed.
        // }
        // iList.Insert(index, value.Item);
        // base.InsertItemDirect(index, value);
        // }
        // finally
        // {
        // fInternalChange = false;
        // }
        // }
        // }
        // else
        // throw new ArgumentOutOfRangeException("index");
        // }

        /// <summary>Moves the item.</summary>
        /// <remarks>Always raises the event, so that undo works correctly (through the
        ///     CodeItemDropAdvisor).</remarks>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var iList = Cluster.ChildrenW;

                // we lock before we set fInternalChange to make certain no one can change something in between calls
            IsInternalChange = true;
            try
            {
                var iToMove = this[oldIndex].Item;
                var iUndo = new MoveClusterUndoItem();
                iUndo.Cluster = Cluster;
                iUndo.Item = iToMove;
                iUndo.Index = oldIndex;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                // iList.RemoveAt(oldIndex);
                iList.Move(oldIndex, newIndex);

                // iList.Insert(newIndex, iToMove);
                base.MoveItem(oldIndex, newIndex);
            }
            finally
            {
                IsInternalChange = false;
                iList.Dispose();
            }
        }

        /// <summary>Removes the item.</summary>
        /// <remarks>Always raises the event, so that undo works correctly (through the
        ///     CodeItemDropAdvisor).</remarks>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iUndo = new RemoveClusterUndoItem();
            var iList = Cluster.ChildrenW;
            iUndo.Items = new System.Collections.Generic.List<Neuron> { this[index].Item };
            iList.Lock(iUndo.Items);

                // we lock before we set fInternalChange to make certain no one can change something in between calls
            try
            {
                IsInternalChange = true;
                try
                {
                    iUndo.Cluster = Cluster;
                    iUndo.Index = index;
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    iList.RemoveUnsafe(this[index].Item, index);
                    RemoveItemDirect(index);
                }
                finally
                {
                    IsInternalChange = false;
                }
            }
            finally
            {
                iList.Unlock(iUndo.Items);
                iList.Dispose();
            }
        }

        /// <summary>Sets the item.</summary>
        /// <remarks>Always raises the event, so that undo works correctly (through the
        ///     CodeItemDropAdvisor).</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, T item)
        {
            var iUndo = new ReplaceClusterUndoItem();
            iUndo.Item = this[index].Item;
            var iList = Cluster.ChildrenW;
            var iToRelease = iList.Lock(item.Item, iUndo.Item);

                // we lock before we set fInternalChange to make certain no one can change something in between calls
            try
            {
                IsInternalChange = true;
                try
                {
                    iUndo.Cluster = Cluster;
                    iUndo.Index = index;
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    iList.RemoveUnsafe(iUndo.Item, index);
                    iList.InsertUnsafe(index, item.Item);
                    SetItemDirect(index, item);
                }
                finally
                {
                    IsInternalChange = false;
                }
            }
            finally
            {
                iList.Unlock(iToRelease);
                iList.Dispose();
                Factories.Default.NLists.Recycle(iToRelease);
            }
        }

        #endregion
    }
}