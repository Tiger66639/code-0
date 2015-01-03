// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetEditor.cs" company="">
//   
// </copyright>
// <summary>
//   An asset editor for assets that aren't linked to an object (like the
//   assets that represent people).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An asset editor for assets that aren't linked to an object (like the
    ///     assets that represent people).
    /// </summary>
    public class AssetEditor : ObjectEditor
    {
        /// <summary>
        ///     <para>
        ///         Changes the name. Need to update the name of the pattern editor.
        ///     </para>
        ///     <para>
        ///         Gets the resource path to the icon that should be used for this
        ///         editor. This is usually class specific. start with /
        ///     </para>
        /// </summary>
        /// <value>
        /// </value>
        /// <param name="value">The value.</param>
        public override string Icon
        {
            get
            {
                return "/Images/Asset/asset_Enabled.png";
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
                return "Asset editor: " + Name;
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
                return "Asset editor";
            }
        }

        /// <summary>The load sub editors.</summary>
        protected override void LoadSubEditors()
        {
            var iSubEditor = new ObjectTextPatternEditor(Item);
            iSubEditor.Name = Name;
            Editors.Add(iSubEditor);
        }

        /// <summary>
        ///     Called when the item got created (from a temp), so we can add the
        ///     textPatternEditor and register the item to the global list of assets.
        /// </summary>
        protected internal override void ItemCreated()
        {
            base.ItemCreated();
            AddItemToAssetsList();
        }

        /// <summary>
        ///     Adds the item to the assets cluster, so that the system knows it's a
        ///     root asset. (this is to prevent assets from being deleted
        ///     automatically by the chatbot when it cleans up the previous value of
        ///     an asset operation.
        /// </summary>
        internal void AddItemToAssetsList()
        {
            var iAssetsGroup = Brain.Current[(ulong)PredefinedNeurons.Asset] as NeuronCluster;
            if (iAssetsGroup != null)
            {
                using (var iList = iAssetsGroup.ChildrenW) iList.Add(Item);
            }
        }

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            var iItem = Item as NeuronCluster;
            if (iItem != null)
            {
                fAssets = new AssetCollection(this, iItem);
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
            if (Editors.Count > 0)
            {
                BrainData.Current.OpenDocuments.Remove(Editors[0]);

                    // need to make certain that this editor is closed, normally, the caller of DeleteEditor is responsible for this, but he doesn't know about the sub editor, so need to do this manually.
                Editors[0].DeleteEditor();
            }

            base.DeleteEditor();
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="AssetEditor"/> class.</summary>
        /// <param name="asset">The asset.</param>
        public AssetEditor(NeuronCluster asset)
            : base(asset)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AssetEditor"/> class.</summary>
        public AssetEditor()
        {
        }

        #endregion
    }
}