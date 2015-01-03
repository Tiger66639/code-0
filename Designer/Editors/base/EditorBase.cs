// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorBase.cs" company="">
//   
// </copyright>
// <summary>
//   Keeps track of the current state of the mindmap, is used to check if the
//   object is being loaded or has already been so.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Keeps track of the current state of the mindmap, is used to check if the
    ///     object is being loaded or has already been so.
    /// </summary>
    public enum EditorState
    {
        /// <summary>The loading.</summary>
        Loading, 

        /// <summary>The loaded.</summary>
        Loaded, 

        /// <summary>The undoing.</summary>
        Undoing, 

        /// <summary>The setting property.</summary>
        SettingProperty
    }

    /// <summary>
    ///     Base class for all editor objects.
    /// </summary>
    public abstract class EditorBase : Data.NamedObject, 
                                       IDescriptionable, 
                                       System.Xml.Serialization.IXmlSerializable, 
                                       IDocumentInfo, 
                                       IDocOpener
    {
        /// <summary>
        ///     Called when the editor is loaded from stream. Allows The editor to
        ///     register things, like neurons it monitors. By default, this does
        ///     nothing.
        /// </summary>
        public virtual void Register()
        {
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        protected virtual void UnloadUIData()
        {
        }

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected virtual void LoadUIData()
        {
        }

        /// <summary>Called when the editor is removed from the project. The editor should
        ///     remove itself (or items that it controls), from the specified<paramref name="list"/> (usually the <paramref name="list"/> of open
        ///     documents). It also allows descendents to do some cleanup when they
        ///     get removed from the project. Note: not called when the project is
        ///     unloaded.</summary>
        /// <remarks>This is a <see langword="virtual"/> function so that some editors can
        ///     have multiple open documents (like a folder).</remarks>
        /// <param name="list">The list.</param>
        public virtual void EditorRemoved(System.Collections.IList list)
        {
            list.Remove(this);
        }

        /// <summary>
        ///     Removes this item from it's owner.
        /// </summary>
        internal void RemoveFromOwner()
        {
            var iFolder = Owner as IEditorsOwner;
            if (iFolder != null)
            {
                iFolder.Editors.Remove(this);
            }
            else
            {
                BrainData.Current.Editors.Remove(this);
            }
        }

        /// <summary>Determines whether this object is the child of the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if [is child of] [the specified item]; otherwise,<c>false</c> .</returns>
        internal bool IsChildOf(EditorBase item)
        {
            var iFolder = Owner as EditorFolder;
            while (iFolder != null)
            {
                if (iFolder == item)
                {
                    return true;
                }

                iFolder = iFolder.Owner as EditorFolder;
            }

            return false;
        }

        /// <summary>Changes the name.</summary>
        /// <param name="value">The value.</param>
        protected override void ChangeName(string value)
        {
            base.ChangeName(value);
            OnPropertyChanged("DocumentTitle");
        }

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <remarks>This is used to determin which neurons need to be exported when an
        ///     editor is selected for export.</remarks>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public abstract System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons();

        #region Fields

        /// <summary>The f current state.</summary>
        private EditorState fCurrentState = EditorState.Loaded; // for when first created.

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is open.</summary>
        private bool fIsOpen;

        // List fSelectedObjects;
        #endregion

        #region Prop

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific. start with /
        /// </summary>
        public abstract string Icon { get; }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the editor is currently selected in the overview
        ///     list. When an editor is selected, the first owning folder is
        ///     automatically assigned to become the
        ///     <see cref="JaStDev.HAB.Designer.BrainData.CurrentEditorsList" />
        /// </summary>
        /// <remarks>
        ///     This is provided so that a folder can make itself 'active' when
        ///     selected.
        /// </remarks>
        public virtual bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    if (value)
                    {
                        var iOwner = Owner as IEditorsOwner;
                        while (iOwner is Data.OwnedObject && !(Owner is EditorFolder))
                        {
                            iOwner = ((Data.OwnedObject)iOwner).Owner as IEditorsOwner;
                        }

                        if (iOwner != null)
                        {
                            BrainData.Current.CurrentEditorsList = iOwner.Editors;
                        }
                        else
                        {
                            BrainData.Current.CurrentEditorsList = BrainData.Current.Editors;
                        }
                    }

                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region CurrentState

        /// <summary>
        ///     Gets/sets the current state of the object. This is used by
        ///     MindMapItemCollection.
        /// </summary>
        internal EditorState CurrentState
        {
            get
            {
                if (fCurrentState == EditorState.Loaded
                    && (WindowMain.UndoStore.CurrentState == UndoSystem.UndoState.Redoing)
                    || WindowMain.UndoStore.CurrentState == UndoSystem.UndoState.Undoing)
                {
                    return EditorState.Undoing;
                }

                return fCurrentState;
            }

            set
            {
                fCurrentState = value;
            }
        }

        #endregion

        #region IDescriptionable Members

        #region Description title

        /// <summary>
        ///     Gets a title that the description editor can use to display in the
        ///     header.
        /// </summary>
        /// <value>
        /// </value>
        public virtual string DescriptionTitle
        {
            get
            {
                return Name + " - Editor";
            }
        }

        #endregion

        #region Description

        /// <summary>
        ///     Gets/sets the description of the graph.
        /// </summary>
        /// <remarks>
        ///     Could be that this prop needs to be streamed manually with
        ///     xmlWriter.WriteRaws.
        /// </remarks>
        public System.Windows.Documents.FlowDocument Description
        {
            get
            {
                if (DescriptionText != null)
                {
                    var stringReader = new System.IO.StringReader(DescriptionText);
                    var xmlReader = System.Xml.XmlReader.Create(stringReader);
                    return System.Windows.Markup.XamlReader.Load(xmlReader) as System.Windows.Documents.FlowDocument;
                }

                return Helper.CreateDefaultFlowDoc();
            }

            set
            {
                var iVal = System.Windows.Markup.XamlWriter.Save(value);
                if (DescriptionText != iVal)
                {
                    DescriptionText = iVal;
                    OnPropertyChanged("Description");
                }
            }
        }

        #endregion

        #region DescriptionText

        /// <summary>
        ///     Gets or sets the description as an xml formatted text.
        /// </summary>
        /// <value>
        ///     The description text.
        /// </value>
        public string DescriptionText { get; set; }

        #endregion

        #endregion

        /// <summary>
        ///     Inheriters return a list of children that can be used to browse
        ///     through the content and select a neuron. This is used by the
        ///     <see cref="NeuronDataBrowser" /> objects.
        /// </summary>
        public virtual System.Collections.IEnumerator BrowsableItems
        {
            get
            {
                return null;
            }
        }

        #region IsOpen

        /// <summary>
        ///     Gets/sets if the Ui editor data is currently loaded for editing or
        ///     not. This is a speed optimisation, to make certain there aren't to
        ///     many event handlers working and data loaded.
        /// </summary>
        /// <remarks>
        ///     When the editor is open for editing, some data needs to remain loaded
        ///     in memory that is not required when it is no longer visible. For this
        ///     reason, the ui part can let the backend know that it is currently
        ///     visualized or not. When no longer visible, some data could be
        ///     unloaded.
        /// </remarks>
        public bool IsOpen
        {
            get
            {
                return fIsOpen;
            }

            set
            {
                if (fIsOpen != value)
                {
                    fIsOpen = value;
                    if (value)
                    {
                        LoadUIData();
                    }
                    else
                    {
                        UnloadUIData();
                    }

                    OnPropertyChanged("IsLoaded");
                }
            }
        }

        #endregion

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

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            CurrentState = EditorState.Loading;
            try
            {
                var iName = reader.Name;
                var wasEmpty = reader.IsEmptyElement;

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != iName)
                {
                    if (ReadXmlInternal(reader) == false)
                    {
                        // if for some reason, we failed to read the item, log an error, and advance to the next item so that we don't get in a loop.
                        LogService.Log.LogError(
                            "EditorBase.ReadXml", 
                            string.Format("Failed to read xml element {0} in stream.", reader.Name));
                        reader.Skip();
                    }
                }

                reader.ReadEndElement();
            }
            finally
            {
                CurrentState = EditorState.Loaded;
            }
        }

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <remarks>This function is called for each element that is found, so this
        ///     function should check which element it is and only read that element
        ///     accordingly.</remarks>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        protected virtual bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (reader.Name == "FlowDocument")
            {
                if (reader.IsEmptyElement == false)
                {
                    DescriptionText = reader.ReadOuterXml();
                }
                else
                {
                    reader.ReadStartElement("FlowDocument");
                }

                return true;
            }

            if (reader.Name == "Name")
            {
                Name = XmlStore.ReadElement<string>(reader, "Name");
                return true;
            }

            return false;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Name", Name);
            if (DescriptionText != null)
            {
                writer.WriteRaw(DescriptionText);
            }
            else
            {
                writer.WriteStartElement("FlowDocument");
                writer.WriteEndElement();
            }
        }

        #endregion

        #region clipboard

        /// <summary>
        ///     Copies the selected data to the clipboard.
        /// </summary>
        public void CopyToClipboard()
        {
            var iData = EditorsHelper.GetDataObject();
            CopyToClipboard(iData);
            System.Windows.Clipboard.SetDataObject(iData, false);
        }

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected abstract void CopyToClipboard(System.Windows.DataObject data);

        /// <summary>
        ///     Determines whether this instance can copy the selected data to the
        ///     clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can copy to the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public abstract bool CanCopyToClipboard();

        /// <summary>
        ///     Determines whether this instance can paste special from the clipboard.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste special from the clipboard;
        ///     otherwise, <c>false</c> .
        /// </returns>
        public abstract bool CanPasteSpecialFromClipboard();

        /// <summary>
        ///     Pastes the data in a special way from the clipboard.
        /// </summary>
        public abstract void PasteSpecialFromClipboard();

        /// <summary>
        ///     Determines whether this instance can cut the selected data to the
        ///     clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can cut to the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public virtual bool CanCutToClipboard()
        {
            return CanCopyToClipboard();
        }

        /// <summary>
        ///     Cuts the selected data to the clipboard. Stores an extra
        ///     <see langword="bool" /> in the data object, to indicate it was a cut,
        ///     so that we know that we don't need to duplicate the data but can use
        ///     the data in the buffer directly.
        /// </summary>
        public virtual void CutToClipboard()
        {
            var iData = EditorsHelper.GetDataObject();
            CopyToClipboard(iData);
            iData.SetData(Properties.Resources.CUTOPERATION, true);

                // we also store the fact that it is a cut operation. This way, a paste knows it doesn't have to perform a duplicate.
            System.Windows.Clipboard.SetDataObject(iData, false);
            Remove();
        }

        /// <summary>
        ///     Determines whether this instance can paste from the clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste from the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public virtual bool CanPasteFromClipboard()
        {
            return EditorsHelper.IsValidCipboardData();
        }

        /// <summary>
        ///     Pastes the data from the clipboard.
        /// </summary>
        public abstract void PasteFromClipboard();

        #endregion

        #region Delete

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public abstract void Remove();

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public abstract void Delete();

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public abstract bool CanDelete();

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public abstract void DeleteSpecial();

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public abstract bool CanDeleteSpecial();

        /// <summary>
        ///     Deletes the editor and all the neurons on the editor that aren't
        ///     referenced anywhere else, if appropriate for the editor. This is
        ///     called when the editor is removed from the project. Usually, the user
        ///     will expect unused data to get removed as well.
        /// </summary>
        /// <remarks>
        ///     Doesn't need to be undo save, this is done by the caller.
        /// </remarks>
        public abstract void DeleteEditor();

        /// <summary>Returns <see langword="false"/> if the editor can't be deleted for
        ///     some reason + the <paramref name="error"/> message why it can't be
        ///     deleted. By default, returnst true.</summary>
        /// <param name="error">The error.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool EditorCanBeDeleted(out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is deleted from the project using the special delete.</summary>
        /// <remarks>Doesn't need to be undo save, this is done by the caller.</remarks>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public abstract void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling);

        #endregion

        #region IDocumentInfo Members

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        public virtual string DocumentTitle
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public abstract string DocumentInfo { get; }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public abstract string DocumentType { get; }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        public object DocumentIcon
        {
            get
            {
                return Icon;
            }
        }

        #endregion
    }
}