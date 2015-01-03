// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorFolder.cs" company="">
//   
// </copyright>
// <summary>
//   An editor object that contains other editor objects, it functions as a
//   folder in the Editors-overview.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An editor object that contains other editor objects, it functions as a
    ///     folder in the Editors-overview.
    /// </summary>
    public class EditorFolder : EditorBase, IEditorsOwner
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditorFolder" /> class.
        /// </summary>
        public EditorFolder()
        {
            fItems = new EditorCollection(this);
        }

        #endregion

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
                return "Folder: " + Name;
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
                return "Folder";
            }
        }

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        public override void DeleteEditor()
        {
            foreach (var i in Items)
            {
                i.DeleteEditor();
            }
        }

        /// <summary>Returns <see langword="false"/> if the editor can't be deleted for
        ///     some reason + the <paramref name="error"/> message why it can't be
        ///     deleted. By default, returnst true.</summary>
        /// <param name="error">The error.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool EditorCanBeDeleted(out string error)
        {
            foreach (var i in Items)
            {
                if (i.EditorCanBeDeleted(out error) == false)
                {
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            foreach (var i in Items)
            {
                i.DeleteAll(deletionMethod, branchHandling);
            }
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

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <remarks>This is used to determin which neurons need to be exported when an
        ///     editor is selected for export.</remarks>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            throw new System.InvalidOperationException("Project folders don't have root neurons.");
        }

        #region fields

        /// <summary>The f items.</summary>
        private readonly EditorCollection fItems;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

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
                return "/Images/ClosedFolder.png";
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the editor is currently selected in the overview
        ///     list.
        /// </summary>
        /// <remarks>
        ///     This is provided so that a folder can make itself 'active' when
        ///     selected.
        /// </remarks>
        /// <value>
        /// </value>
        public override bool IsSelected
        {
            get
            {
                return base.IsSelected;
            }

            set
            {
                base.IsSelected = value;
                if (value)
                {
                    BrainData.Current.CurrentEditorsList = Items;
                }
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of editor objects that are stored in this folder.
        /// </summary>
        public EditorCollection Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region IEditorsOwner Members

        /// <summary>
        ///     Gets the list of editor objects that are stored in this item.
        /// </summary>
        public EditorCollection Editors
        {
            get
            {
                return Items;
            }
        }

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

        #region ContainsAssets

        /// <summary>
        ///     Gets a value indicating whether this folder contains any asset editors
        ///     (recursivelly).
        /// </summary>
        /// <value>
        ///     <c>true</c> if [contains assets]; otherwise, <c>false</c> .
        /// </value>
        public bool ContainsAssets
        {
            get
            {
                foreach (var i in Items)
                {
                    if (i is AssetEditor)
                    {
                        return true;
                    }

                    if (i is EditorFolder)
                    {
                        return ((EditorFolder)i).ContainsAssets;
                    }
                }

                return false;
            }
        }

        #endregion

        #region ContainsTopics

        /// <summary>
        ///     Gets a value indicating whether this folder contains any topic editors
        ///     (recursivelly).
        /// </summary>
        /// <value>
        ///     <c>true</c> if [contains assets]; otherwise, <c>false</c> .
        /// </value>
        public bool ContainsTopics
        {
            get
            {
                foreach (var i in Items)
                {
                    if (i is TextPatternEditor)
                    {
                        return true;
                    }

                    if (i is EditorFolder)
                    {
                        return ((EditorFolder)i).ContainsTopics;
                    }
                }

                return false;
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
                return Name + " - Folder";
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Called when the editor is loaded from stream. Allows The editor to
        ///     register things, like neurons it monitors. Let all the children
        ///     register.
        /// </summary>
        public override void Register()
        {
            foreach (var i in Items)
            {
                i.Register();
            }
        }

        /// <summary>Removes the editor from the specified list.</summary>
        /// <remarks>This is a <see langword="virtual"/> function so that some editors can
        ///     have multiple open documents (like a folder).</remarks>
        /// <param name="list">The list.</param>
        public override void EditorRemoved(System.Collections.IList list)
        {
            foreach (var i in Items)
            {
                i.EditorRemoved(list);
            }
        }

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <remarks>This function is called for each element that is found, so this
        ///     function should check which element it is and only read that element
        ///     accordingly.</remarks>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (reader.Name == "EditorCollection")
            {
                fItems.Clear();
                fItems.ReadXml(reader);
                return true;
            }

            if (reader.Name == "IsExpanded")
            {
                IsExpanded = XmlStore.ReadElement<bool>(reader, "IsExpanded");
                return true;
            }

            return base.ReadXmlInternal(reader);
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            var iCol = new System.Xml.Serialization.XmlSerializer(typeof(EditorCollection));
            iCol.Serialize(writer, Items);
            XmlStore.WriteElement(writer, "IsExpanded", IsExpanded);

                // important: needs to be done after the items, so that when reading, the collection is in a correct state, cause adding items to an EditorCollection automatically expands it.
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
    }
}