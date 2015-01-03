// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronEditor.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all editor types that also wrap a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Base class for all editor types that also wrap a neuron.
    /// </summary>
    /// <remarks>
    ///     When read from xml, use <see cref="NeuronEditor.RegisterItem" /> to properly attach the item.
    /// </remarks>
    public abstract class NeuronEditor : EditorBase, 
                                         INeuronWrapper, 
                                         INeuronInfo, 
                                         System.Windows.IWeakEventListener, 
                                         IEditorsOwner
    {
        #region Fields

        /// <summary>The f item.</summary>
        private Neuron fItem;

        /// <summary>The f show in project.</summary>
        private bool? fShowInProject;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="NeuronEditor" /> class.
        /// </summary>
        public NeuronEditor()
        {
            Editors = new EditorCollection(this);
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronEditor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public NeuronEditor(Neuron toWrap)
        {
            if (toWrap == null)
            {
                throw new System.ArgumentNullException();
            }

            Editors = new EditorCollection(this);
            Item = toWrap;
        }

        #endregion

        #region Prop

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the Neuron that this object provides a wraper for.
        /// </summary>
        /// <value>The item.</value>
        public virtual Neuron Item
        {
            get
            {
                return fItem;
            }

            internal set
            {
                // OnPropertyChanging("Item", fItem, value);                                    //we need to store undo data for this item because it gets reset before a delete (to free the lock), offcourse, the lock and ID need to be assigned again during an undo.
                if (NeuronInfo != null)
                {
                    System.ComponentModel.PropertyChangedEventManager.RemoveListener(NeuronInfo, this, "DisplayTitle");
                }

                if (fItem != null)
                {
                    EventManager.Current.UnRegisterNeuronEditor(this);
                    if (IsOpen)
                    {
                        UnloadUIData();
                    }
                }

                Editors.Clear();
                fItem = value;
                if (fItem != null && fItem.ID != Neuron.TempId)
                {
                    EventManager.Current.RegisterNeuronEditor(this);

                        // the event monitor is only useful if there is a neuron to wrap.
                    if (BrainData.Current != null && BrainData.Current.NeuronInfo != null)
                    {
                        // NeuronInfo == null while loading a project, in that case, RegisterItem is called.
                        RegisterItem();
                    }
                    else
                    {
                        NeuronInfo = null;
                    }
                }
                else
                {
                    var monitor = new NeuronEditorCreateMonitor(this);

                        // this will register the monitor to the eventManager. When the item gets created, the ItemCreated gets called.
                    NeuronInfo = null;
                }

                if (fItem != null && IsOpen)
                {
                    LoadUIData();
                }

                OnPropertyChanged("Item");
                OnPropertyChanged("NeuronInfo");
                OnPropertyChanged("ItemID");
            }
        }

        #endregion

        #region ItemID

        /// <summary>
        ///     Gets or sets the ID of the item that this object provides an editor for.
        /// </summary>
        /// <remarks>
        ///     When read from xml, use <see cref="NeuronEditor.RegisterItem" /> to properly attach the item.
        /// </remarks>
        /// <value>The ID of the item.</value>
        public virtual ulong ItemID
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
                if (value != ItemID)
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
                                "NeuronEditor.ItemID", 
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
        }

        /// <summary>
        ///     Registers the item that was read from xml.
        /// </summary>
        /// <remarks>
        ///     This must be called when the editor is read from xml.  In that situation, the
        ///     brainData isn't always loaded properly yet.  At this point, this can be resolved.
        ///     It is called by the brainData.
        /// </remarks>
        public override void Register()
        {
            UnloadNeuronData();
            if (Item != null)
            {
                RegisterItem();
            }
            else
            {
                NeuronInfo = null;
            }

            OnPropertyChanged("Item");
            OnPropertyChanged("NeuronInfo");
            OnPropertyChanged("ItemID");
        }

        #endregion

        #region NeuronInfo (INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron.  Can be null.
        /// </summary>
        /// <value></value>
        public NeuronData NeuronInfo { get; private set; }

        /// <summary>
        ///     Loads the neuron info.
        /// </summary>
        protected virtual void LoadNeuronInfo()
        {
            SetNeuronInfo(BrainData.Current.NeuronInfo[Item.ID]);
        }

        /// <summary>so descendents can overwrite this.</summary>
        /// <param name="value">The value.</param>
        protected void SetNeuronInfo(NeuronData value)
        {
            NeuronInfo = value;
            if (NeuronInfo != null)
            {
                // can be null for ObjectTextPatternEditors that are still working with a temp topic cluster -> they use fData from topic cluster, which is not yet existing.
                System.ComponentModel.PropertyChangedEventManager.AddListener(NeuronInfo, this, "DisplayTitle");
            }
        }

        #endregion

        #region ShowInProject

        /// <summary>
        ///     Gets/sets wether this code editor is displayed as a shortcut in the project tree.
        /// </summary>
        public bool ShowInProject
        {
            get
            {
                if (fShowInProject.HasValue == false)
                {
                    fShowInProject = Enumerable.Contains(BrainData.Current.Editors.AllNeuronEditors(), this);
                }

                return fShowInProject.Value;
            }

            set
            {
                if (value != fShowInProject)
                {
                    fShowInProject = value;
                    if (value)
                    {
                        BrainData.Current.CurrentEditorsList.Add(this);
                    }
                    else
                    {
                        BrainData.Current.Editors.RemoveRecursive(this);
                    }

                    OnPropertyChanged("ShowInProject");
                }
            }
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                if (value != base.Name)
                {
                    base.Name = value;
                    if (base.Name == value && NeuronInfo != null && NeuronInfo.DisplayTitle != value)
                    {
                        // we check for base.name == value, so we are certain that the change took place and wasn't canceled. If it is canceled, we don't wan to go change the displaytitle as well.
                        NeuronInfo.DisplayTitle = value;
                    }
                }
            }
        }

        #endregion

        #region Editors

        /// <summary>
        ///     Gets the editors that are linked to this asset. This is used to provide access sub editors, like the
        ///     <see cref="TextPatternEditor" />
        ///     that can be attached to an asset. This allows the UI to make them visible.
        /// </summary>
        public EditorCollection Editors { get; private set; }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the a value indicating that the item is expanded or not.
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

        #region virtuals

        /// <summary>
        ///     sets everything up for the neuron info.
        /// </summary>
        protected virtual void RegisterItem()
        {
            LoadNeuronInfo();
            LoadSubEditors();
        }

        /// <summary>
        ///     Lets inheriters load possible sub editors for this editor. By default, doesn't load anything.
        /// </summary>
        protected virtual void LoadSubEditors()
        {
        }

        /// <summary>
        ///     Called when the item got created (from a temp), so we can add the textPatternEditor and register the item to the
        ///     global list of assets.
        /// </summary>
        protected internal virtual void ItemCreated()
        {
            EventManager.Current.RegisterNeuronEditor(this);

                // the event monitor is only useful if there is a neuron to wrap.
            LoadNeuronInfo();
            NeuronInfo.DisplayTitle = Name; // also store the name correctly
            LoadSubEditors();
        }

        #endregion

        #region overrides

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere else, if appropriate for the editor.
        ///     This is called when the editor is deleted from the project. Usually, the user will expect unused data to get
        ///     removed as well. The neuron that the editor wraps, is always deleted.
        /// </summary>
        /// <remarks>
        ///     Tries to delete the neuron that this editor wraps.
        /// </remarks>
        public override void DeleteEditor()
        {
            var iDeleter = new NeuronDeleter(DeletionMethod.Delete, DeletionMethod.DeleteIfNoRef);
            iDeleter.Start(Item);
        }

        /// <summary>Called when the editor is removed from the project. The editor should remove itself (or items that it controls),
        ///     from the specified list (usually the list of open documents). It also allows descendents to do some cleanup when
        ///     they get removed from the project.</summary>
        /// <param name="list">The list.</param>
        /// <remarks>This is a virtual function so that some editors can have multiple open documents (like a folder).</remarks>
        public override void EditorRemoved(System.Collections.IList list)
        {
            base.EditorRemoved(list);
            UnloadNeuronData();
        }

        /// <summary>
        ///     if there is an fData object, it gets reset, so that the neuronData object is no longer aware of the frame.
        ///     Doesn't assign null to the fData.
        /// </summary>
        private void UnloadNeuronData()
        {
            if (NeuronInfo != null)
            {
                System.ComponentModel.PropertyChangedEventManager.RemoveListener(NeuronInfo, this, "DisplayTitle");
                NeuronInfo.IsLocked = false;
            }
        }

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        /// <remarks>This is used to determin which neurons need to be exported when an editor is selected for export.</remarks>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            yield return Item;
        }

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        /// <remarks>This function is called for each element that is found, so this function should check which element it is
        ///     and only read that element accordingly.</remarks>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (base.ReadXmlInternal(reader) == false)
            {
                if (reader.Name == "Item")
                {
                    ulong iVal;
                    iVal = XmlStore.ReadElement<ulong>(reader, "Item");
                    if (iVal != Neuron.EmptyId)
                    {
                        Neuron iFound;
                        if (Brain.Current.TryFindNeuron(iVal, out iFound))
                        {
                            Item = iFound;
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "Editor.ReadXml", 
                                string.Format("Failed to find backing neuron for editor: {0}", Name));
                            Item = null;
                        }
                    }
                    else
                    {
                        Item = null;
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Name", Name);

                // we don't write out the complete base cause this also generates the description, but we don't store this, it comes from the NeuronData object that we ref.
            XmlStore.WriteElement(writer, "Item", ItemID);
        }

        #endregion

        #region IWeakEventListener

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>true if the listener handled the event. It is considered an error by the<see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the
        ///     listener does not handle. Regardless, the method should return false if it receives an event that it does not
        ///     recognize or handle.</returns>
        public virtual bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.ComponentModel.PropertyChangedEventManager))
            {
                fData_PropertyChanged(sender, (System.ComponentModel.PropertyChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Called when the wrapped item is changed. When removed, makes certain that the <see cref="BrainData.CodeEditors"/>
        ///     list is updated.</summary>
        /// <param name="e">The <see cref="JaStDev.HAB.NeuronChangedEventArgs"/> instance containing the event data.</param>
        protected internal virtual void Item_NeuronChanged(NeuronChangedEventArgs e)
        {
            if (!(e is NeuronPropChangedEventArgs))
            {
                // don't need to respond to a prop changed event.
                switch (e.Action)
                {
                    case BrainAction.Changed:
                        Item = e.NewValue;
                        break;
                    case BrainAction.Removed:
                        BrainData.Current.Editors.RemoveRecursive(this);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>Called when an outgoing link got removed. Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        protected internal virtual void OutgoingLinkRemoved(Link link)
        {
        }

        /// <summary>Called when an outgoing link got created. Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        protected internal virtual void OutgoingLinkCreated(Link link)
        {
        }

        /// <summary>Called when an outgoing link got removed. Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        protected internal virtual void OutgoingLinkChanged(Link link, ulong oldVal)
        {
        }

        /// <summary>Handles the PropertyChanged event of the fData control.</summary>
        /// <remarks>Need to update the title of the code Editor if appropriate.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void fData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Name != NeuronInfo.DisplayTitle)
            {
                // name != displayTitle cause when name is changed,sometimes displaytitle might also get
                ChangeName(NeuronInfo.DisplayTitle); // can't use prop setter, cause it generates undo data
                if (NeuronInfo.DisplayTitle != Name)
                {
                    // if the name change didn't get accepted, also roll back the displaytitle
                    NeuronInfo.DisplayTitle = Name;
                }
            }
        }

        #endregion
    }
}