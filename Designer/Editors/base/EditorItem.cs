// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorItem.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for items used by editors. They can be nested.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for items used by editors. They can be nested.
    /// </summary>
    public class EditorItem : Data.OwnedObject<Data.IOwnedObject>, 
                              INeuronWrapper, 
                              INeuronInfo, 
                              Search.IDisplayPathBuilder, 
                              IInternalChange
    {
        // we inherit from OwnedObject<IOwnedObject> cause this is the generic version that provides functions for searching the owner tree using generics. 
        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object.</summary>
        /// <remarks>by default, not implemented. Simply returns a <see langword="null"/></remarks>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public virtual Search.DisplayPath GetDisplayPathFromThis()
        {
            return null;
        }

        #endregion

        /// <summary>Sets the first outgoing link to of the <see cref="Neuron"/> being
        ///     wrapped by this editor item, to the new value. During this process,
        ///     the proper undo data is generated.</summary>
        /// <remarks>Note, we can't rely on OnPropertyChanging event handling of the undo
        ///     system, to handle link changes, cause the editor item that generated
        ///     the event can be replaced. Instead, we must use<see cref="LinkUndoItem"/> data.</remarks>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        protected void SetFirstOutgoingLinkTo(ulong meaning, EditorItem value)
        {
            EditorsHelper.SetFirstOutgoingLinkTo(Item, meaning, value);
        }

        /// <summary>Sets the first outgoing link of the <see cref="Neuron"/> being wrapped
        ///     by this editor item, to the new value. During this process, the proper
        ///     undo data is generated.</summary>
        /// <remarks>Note, we can't rely on OnPropertyChanging event handling of the undo
        ///     system, to handle link changes, cause the editor item that generated
        ///     the event can be replaced. Instead, we must use<see cref="LinkUndoItem"/> data.</remarks>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value"><para>The value as a bool, this is resolved to<see cref="JaStDev.HAB.PredefinedNeurons.True"/></para>
        /// <para>or <see cref="JaStDev.HAB.PredefinedNeurons.True"/> .</para>
        /// </param>
        protected void SetFirstOutgoingLinkTo(ulong meaning, bool value)
        {
            EditorsHelper.SetFirstOutgoingLinkTo(Item, meaning, value);
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child">The child.</param>
        public virtual void RemoveChild(EditorItem child)
        {
            System.Windows.MessageBox.Show("Unable to remove the child!");
        }

        /// <summary>
        ///     Called when all the data kept in memory for the UI section can be
        ///     unloaded.
        /// </summary>
        internal virtual void UnloadUIData()
        {
            fNeuronInfo = null; // neuroninfo can be released if no longer open.
            IsActive = false;
            IsSelected = false;

                // when unloaded, also make certain that the item isn't selected, otherwise, some deleted items remain seleced, which we don't want.
        }

        #region fields

        /// <summary>The f item.</summary>
        private Neuron fItem;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is multi used.</summary>
        private bool fIsMultiUsed;

        /// <summary>The f neuron info.</summary>
        private NeuronData fNeuronInfo;

        /// <summary>The f is active.</summary>
        private bool fIsActive = true;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="EditorItem"/> class. Initializes a new instance of the <see cref="CodeItem"/> class.</summary>
        /// <remarks>By default, this sets<see cref="JaStDev.HAB.Designer.EditorItem.IsActive"/> to true.</remarks>
        /// <param name="toWrap">The item to wrap.</param>
        public EditorItem(Neuron toWrap)
        {
            if (toWrap == null)
            {
                throw new System.ArgumentNullException();
            }

            Item = toWrap;
            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="EditorItem"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        /// <param name="isActive">if set to <c>true</c> The item will be<see cref="JaStDev.HAB.Designer.EditorItem.IsActive"/> .</param>
        public EditorItem(Neuron toWrap, bool isActive)
        {
            if (toWrap == null)
            {
                throw new System.ArgumentNullException();
            }

            fIsActive = isActive;
            Item = toWrap;
            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="EditorItem"/> class. 
        ///     For <see langword="static"/> code item</summary>
        public EditorItem()
        {
            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="EditorItem"/> class.</summary>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public EditorItem(bool isActive)
        {
            fIsActive = isActive;
            InternalCreate();
        }

        /// <summary>The internal create.</summary>
        private void InternalCreate()
        {
            InternalChange = false;
        }

        #endregion

        #region prop

        /// <summary>
        ///     a <see langword="switch" /> that determins if the class is doing an
        ///     <see langword="internal" /> change or not. This is used by the
        ///     LinkChanged event manager to see if there needs to be an update in
        ///     response to a linkchange or not.
        /// </summary>
        protected internal bool InternalChange { get; set; }

        #region IInternalChange Members

        /// <summary>
        ///     a <see langword="switch" /> that determins if the class is doing an
        ///     <see langword="internal" /> change or not. This is used by the event
        ///     system to make certain that some things don't get updated 2 times.
        /// </summary>
        /// <remarks>
        ///     We need to keep this hidden, so an explicit interface.
        /// </remarks>
        bool IInternalChange.InternalChange
        {
            get
            {
                return InternalChange;
            }

            set
            {
                InternalChange = value;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets if this item is selected or not.
        /// </summary>
        /// <remarks>
        ///     When changed, stores the value and raises the event but also updates
        ///     the root list.
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
                    UpdateIsSelected(value);

                        // we don't need to call setSelected again, this is done by the root when it adds the item to the list.
                }
            }
        }

        /// <summary>Updates the root object's<see cref="JaStDev.HAB.Designer.IEditorSelection.SelectedItems"/>
        ///     list so that everything is up to date.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void UpdateIsSelected(bool value)
        {
            var iRoot = Root;
            if (iRoot != null)
            {
                if (value == false)
                {
                    Root.SelectedItems.Remove(this);
                }
                else
                {
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) == false
                        && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl) == false)
                    {
                        Root.SelectedItems.Clear();
                    }

                    Root.SelectedItems.Add(this);
                }
            }
        }

        /// <summary>Stores the selected <paramref name="value"/> and raises the event.</summary>
        /// <param name="value"></param>
        protected internal virtual void SetSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the expression (or neuron in case of a static) item that is
        ///     wrapped by this one.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Item
        {
            get
            {
                return fItem;
            }

            protected set
            {
                if (fItem != value)
                {
                    if (fItem != null && IsActive)
                    {
                        EventManager.Current.UnRegisterEditorItem(this);
                    }

                    fItem = value;
                    if (value != null)
                    {
                        OnItemChanged(value);
                        if (IsActive)
                        {
                            EventManager.Current.RegisterEditorItem(this);
                        }
                    }
                    else if (IsActive)
                    {
                        // only need unregister if the item is still active
                        EventManager.Current.UnRegisterEditorItem(this);
                    }

                    OnPropertyChanged("Item");
                }
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected virtual void OnItemChanged(Neuron value)
        {
            var iCount = 0;

            // this can be made saver: use a single lock and perform all the calculations inside the single lock.
            if (value.LinksInIdentifier != null)
            {
                using (var iLinks = value.LinksIn)
                    iCount = iLinks.Count;

                        // if an expression belongs to multiple groups, we presume it is in multiple code sets.
            }

            if (value.ClusteredByIdentifier != null)
            {
                using (var iList = value.ClusteredBy) iCount += iList.Count;
            }

            IsMultiUsed = iCount > 1;

                // if an expression belongs to multiple groups, we presume it is in multiple code sets.
        }

        #endregion

        #region Root

        /// <summary>
        ///     Gets the root page, the <see cref="IEditorSelection" /> object that
        ///     contains this code.
        /// </summary>
        /// <remarks>
        ///     This is used to find out if 2 code Items belong to the same function.
        /// </remarks>
        public IEditorSelection Root
        {
            get
            {
                return FindFirstOwner<IEditorSelection>();
            }
        }

        #endregion

        #region IsMultiUsed

        /// <summary>
        ///     Queries the underlying <see cref="Expression" /> to see if it used in
        ///     more locations than 1.
        /// </summary>
        /// <remarks>
        ///     It is possible for an expression (block) to be used by more than 1
        ///     function in more than 1 place. This property returns if this is the
        ///     case or not.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsMultiUsed
        {
            get
            {
                return fIsMultiUsed;
            }

            private set
            {
                fIsMultiUsed = value;
                OnPropertyChanged("IsMultiUsed");
            }
        }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets wether this object monitors changes in the database or not.
        ///     When this is false, it uses less resources.
        /// </summary>
        /// <remarks>
        ///     This is <see langword="virtual" /> so that any changes to this property
        ///     can also be propegated to other objects.
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
                    OnPropertyChanged("IsActive");
                    if (value)
                    {
                        EventManager.Current.RegisterEditorItem(this);
                    }
                    else if (Item != null)
                    {
                        // can't unregister is this is nul.
                        EventManager.Current.UnRegisterEditorItem(this);
                    }
                }
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

        #region IWeakEventListener Members

        /// <summary>Called when the number of times that the neuron is referenced, has
        ///     changed. This allows us to quickly update the IsMultiUsedValue.</summary>
        /// <param name="clusterCount">The cluster count.</param>
        /// <param name="linkCount">The link count.</param>
        internal void UpdateRefCount(int clusterCount, int linkCount)
        {
            IsMultiUsed = clusterCount + linkCount > 1;
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this EditorItem no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal virtual void OutgoingLinkRemoved(Link link)
        {
            // don't do anything by default
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this EditorItem wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal virtual void OutgoingLinkCreated(Link link)
        {
            // don't do anything by default
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (fNeuronInfo == null)
                {
                    // we store a local copy so that, if it is a weak ref in the BrainData, the object remains valid for as long as this item exists (otherwise, we can't edit the displaytitle because the object is garbage collected).
                    if (fItem != null && Neuron.IsEmpty(fItem.ID) == false)
                    {
                        fNeuronInfo = BrainData.Current.NeuronInfo[fItem.ID];
                    }
                }

                return fNeuronInfo;
            }
        }

        /// <summary>doesn't try to create a new item, just returns the currently loaded
        ///     item. This is used during unload, to see if there is a callback</summary>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        public NeuronData GetCurrentNeuronInfo()
        {
            return fNeuronInfo;
        }

        #endregion
    }
}