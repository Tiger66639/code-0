// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection for <see cref="EditorBase" /> objects able to stream
//   properly.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection for <see cref="EditorBase" /> objects able to stream
    ///     properly.
    /// </summary>
    /// <remarks>
    ///     this is required cause <see cref="EditorBase" /> implements
    ///     <see cref="System.Xml.Serialization.IXmlSerializable" /> which causes the serialization of the
    ///     items to fail: always use base type as name of xml element instead of
    ///     actual one.
    /// </remarks>
    public class EditorCollection : Data.ObservedCollection<EditorBase>, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>Initializes a new instance of the <see cref="EditorCollection"/> class. 
        ///     Initializes a new instance of the <see cref="EditorCollection"/>
        ///     class.</summary>
        public EditorCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditorCollection"/> class. Initializes a new instance of the <see cref="EditorCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public EditorCollection(object owner)
            : base(owner)
        {
        }

        /// <summary>
        ///     Gets an object that allows a <see cref="DropDownNNSelector" /> to
        ///     browse through the data of all the outputs of the topics in the
        ///     project.
        /// </summary>
        public System.Collections.IEnumerator BrowsableOutputs
        {
            get
            {
                return new BrowsableOutputPatternEditorsEnumerator(this);
            }
        }

        /// <summary>
        ///     Gets an object that allows a <see cref="DropDownNNSelector" /> to
        ///     browse through the data of all the assets in the project.
        /// </summary>
        public System.Collections.IEnumerator BrowsableAssets
        {
            get
            {
                return new BrowsableAssetsEnumerator(this);
            }
        }

        /// <summary>
        ///     Gets an object that allows a <see cref="DropDownNNSelector" /> to
        ///     browse through the data of all the topics in the project.
        /// </summary>
        public System.Collections.IEnumerator BrowsableTopics
        {
            get
            {
                return new BrowsableTopicsEnumerator(this);
            }
        }

        /// <summary>Removes the item recursivelly, that is, when the item is not in this
        ///     list, all child editor folders are searched as well.</summary>
        /// <param name="toRemove">To remove.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool RemoveRecursive(EditorBase toRemove)
        {
            if (Remove(toRemove) == false)
            {
                var iFolders = from i in this where i is EditorFolder select (EditorFolder)i;
                foreach (var i in iFolders)
                {
                    if (i.Items.RemoveRecursive(toRemove))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     Gets all the <see cref="NeuronEditor" /> s.
        /// </summary>
        /// <remarks>
        ///     Use this to quickly access all the editors that derive from
        ///     <see cref="NeuronEditor" /> .
        /// </remarks>
        /// <returns>
        ///     All the neuron editors.
        /// </returns>
        public System.Collections.Generic.IEnumerable<NeuronEditor> AllNeuronEditors()
        {
            var iToProcess = new System.Collections.Generic.Stack<EditorCollection>();
            iToProcess.Push(this);
            while (iToProcess.Count > 0)
            {
                var iCol = iToProcess.Pop();
                foreach (var i in iCol)
                {
                    if (i is NeuronEditor)
                    {
                        yield return (NeuronEditor)i;
                    }
                    else if (i is FlowEditor)
                    {
                        foreach (var ii in ((FlowEditor)i).Flows)
                        {
                            yield return ii;
                        }
                    }
                    else if (i is IEditorsOwner)
                    {
                        iToProcess.Push(((IEditorsOwner)i).Editors);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all the <see cref="CodeEditor" /> s.
        /// </summary>
        /// <returns>
        ///     All the code editors.
        /// </returns>
        public System.Collections.Generic.IEnumerable<CodeEditor> AllCodeEditors()
        {
            var iToProcess = new System.Collections.Generic.Stack<EditorCollection>();
            iToProcess.Push(this);
            while (iToProcess.Count > 0)
            {
                var iCol = iToProcess.Pop();
                foreach (var i in iCol)
                {
                    if (i is CodeEditor)
                    {
                        yield return (CodeEditor)i;
                    }
                    else if (i is IEditorsOwner)
                    {
                        iToProcess.Push(((IEditorsOwner)i).Editors);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all the <see cref="TextPatternEditor" /> s defined in the
        ///     project. Opens each editor before returning it. This is more suited
        ///     for UI's and stuff
        /// </summary>
        /// <returns>
        ///     All the code editors.
        /// </returns>
        public System.Collections.Generic.IEnumerable<TextPatternEditor> AllTextPatternEditors()
        {
            var iToProcess = new System.Collections.Generic.Stack<EditorCollection>();
            iToProcess.Push(this);
            while (iToProcess.Count > 0)
            {
                var iCol = iToProcess.Pop();
                foreach (var i in iCol)
                {
                    if (i is TextPatternEditor)
                    {
                        var iEditor = (TextPatternEditor)i;
                        var iIsOpen = iEditor.IsOpen;

                            // need to make certain that the data is loaded, otherwise we can't see if it needs to be written or not.
                        iEditor.IsOpen = true;
                        try
                        {
                            if (iEditor.Items != null && iEditor.Items.Cluster.ID != Neuron.TempId)
                            {
                                // assets generate temp patterns, they don't need to be listed if they are still temp, cause they are just there to display a starting point for the editor, they are not yet real topics.
                                yield return iEditor;
                            }
                        }
                        finally
                        {
                            iEditor.IsOpen = iIsOpen;
                        }
                    }
                    else if (i is IEditorsOwner)
                    {
                        iToProcess.Push(((IEditorsOwner)i).Editors);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all the <see cref="TextPatternEditor" /> s defined in the project
        ///     but doesn't open them before returning. This is more suitable for
        ///     retrieving and storing the entire list.
        /// </summary>
        /// <returns>
        ///     All the code editors.
        /// </returns>
        public System.Collections.Generic.IEnumerable<TextPatternEditor> AllTextPatternEditorsClosed()
        {
            var iToProcess = new System.Collections.Generic.Stack<EditorCollection>();
            iToProcess.Push(this);
            while (iToProcess.Count > 0)
            {
                var iCol = iToProcess.Pop();
                foreach (var i in iCol)
                {
                    if (i is TextPatternEditor)
                    {
                        var iEditor = (TextPatternEditor)i;
                        yield return iEditor;
                    }
                    else if (i is IEditorsOwner)
                    {
                        iToProcess.Push(((IEditorsOwner)i).Editors);
                    }
                }
            }
        }

        #region overrides

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                // when a NeuronEditor is no longer managed by us, we don't need to lock it anymore, it can be deleted as far as we are concerned.
                var iEditor = i as NeuronEditor;
                if (iEditor != null)
                {
                    iEditor.NeuronInfo.IsLocked = false;
                }
            }

            base.ClearItems();
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            foreach (var i in this)
            {
                // when a NeuronEditor is no longer managed by us, we don't need to lock it anymore, it can be deleted as far as we are concerned.
                var iEditor = i as NeuronEditor;
                if (iEditor != null)
                {
                    iEditor.NeuronInfo.IsLocked = false;
                }
            }

            base.ClearItemsDirect();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, EditorBase item)
        {
            var iEditor = item as NeuronEditor;
            if (iEditor != null && iEditor.NeuronInfo != null)
            {
                // while loading from disk, neuronInfo is not yet valid. These items are locked when they get registered (after loading).
                iEditor.NeuronInfo.IsLocked = true;
            }

            base.InsertItem(index, item);
            ExpandOwner();

                // when we add data to the editors collection, make certain that it is expanded, so the user can see the change.
        }

        /// <summary>The expand owner.</summary>
        private void ExpandOwner()
        {
            var iOwner = Owner as EditorFolder;
            if (iOwner != null)
            {
                iOwner.IsExpanded = true;
            }
        }

        /// <summary>Inserts the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItemDirect(int index, EditorBase item)
        {
            var iEditor = item as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = true;
            }

            base.InsertItemDirect(index, item);
            ExpandOwner();

                // when we add data to the editors collection, make certain that it is expanded, so the user can see the change.
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iEditor = this[index] as NeuronEditor;
            if (iEditor != null && iEditor.NeuronInfo != null)
            {
                iEditor.NeuronInfo.IsLocked = false;
            }

            base.RemoveItem(index);
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            var iEditor = this[index] as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = false;
            }

            base.RemoveItemDirect(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, EditorBase item)
        {
            var iEditor = this[index] as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = false;
            }

            iEditor = item as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = true;
            }

            base.SetItem(index, item);
        }

        /// <summary>Sets the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItemDirect(int index, EditorBase item)
        {
            var iEditor = this[index] as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = false;
            }

            iEditor = item as NeuronEditor;
            if (iEditor != null)
            {
                iEditor.NeuronInfo.IsLocked = true;
            }

            base.SetItemDirect(index, item);
        }

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
        public void ReadXml(System.Xml.XmlReader reader)
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
                if (ReadXmlContent(reader) == false)
                {
                    // if for some reason, we failed to read the item, log an error, and advance to the next item so that we don't get in a loop.
                    LogService.Log.LogError(
                        "EditorCollection.ReadXml", 
                        string.Format("Failed to read xml element {0} in stream.", reader.Name));
                    reader.Skip();
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads the content of the XML.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ReadXmlContent(System.Xml.XmlReader reader)
        {
            EditorBase iNode = null;
            if (reader.Name == "MindMap")
            {
                var iNoteSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMap));
                iNode = (MindMap)iNoteSer.Deserialize(reader);
            }
            else if (reader.Name == "FrameEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(FrameEditor));
                iNode = (FrameEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "VisualEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(VisualEditor));
                iNode = (VisualEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "ObjectFramesEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectFramesEditor));
                iNode = (ObjectFramesEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "Flow")
            {
                var iNeuronSer = new System.Xml.Serialization.XmlSerializer(typeof(Flow));
                iNode = (Flow)iNeuronSer.Deserialize(reader);
            }
            else if (reader.Name == "CodeEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(CodeEditor));
                iNode = (CodeEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "FlowEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(FlowEditor));
                iNode = (FlowEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "AssetEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(AssetEditor));
                iNode = (AssetEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "ObjectEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectEditor));
                iNode = (ObjectEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "TextPatternEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(TextPatternEditor));
                iNode = (TextPatternEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "ObjectTextPatternEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectTextPatternEditor));
                iNode = (ObjectTextPatternEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "QueryEditor")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(QueryEditor));
                iNode = (QueryEditor)iLinkSer.Deserialize(reader);
            }
            else if (reader.Name == "EditorFolder")
            {
                var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(EditorFolder));
                iNode = (EditorFolder)iLinkSer.Deserialize(reader);
            }

            if (iNode != null)
            {
                Add(iNode);
                return true;
            }

            return false;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var iMindMapSer = new System.Xml.Serialization.XmlSerializer(typeof(MindMap));
            var iFrameSer = new System.Xml.Serialization.XmlSerializer(typeof(FrameEditor));
            var iFlowSer = new System.Xml.Serialization.XmlSerializer(typeof(Flow));
            var iFlowEdSer = new System.Xml.Serialization.XmlSerializer(typeof(FlowEditor));
            var iCodeSer = new System.Xml.Serialization.XmlSerializer(typeof(CodeEditor));
            var iFolderSer = new System.Xml.Serialization.XmlSerializer(typeof(EditorFolder));
            var iAssetSer = new System.Xml.Serialization.XmlSerializer(typeof(AssetEditor));
            var iObjectSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectEditor));
            var iQuerySer = new System.Xml.Serialization.XmlSerializer(typeof(QueryEditor));
            var iTextPatternSer = new System.Xml.Serialization.XmlSerializer(typeof(TextPatternEditor));
            var iObjectTextPatternSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectTextPatternEditor));

            var iObjectFramesSer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectFramesEditor));
            var iVFrameSer = new System.Xml.Serialization.XmlSerializer(typeof(VisualEditor));
            foreach (var item in this)
            {
                if (item is MindMap)
                {
                    iMindMapSer.Serialize(writer, item);
                }
                else if (item is VisualEditor)
                {
                    iVFrameSer.Serialize(writer, item);
                }
                else if (item is FrameEditor)
                {
                    iFrameSer.Serialize(writer, item);
                }
                else if (item is ObjectFramesEditor)
                {
                    iObjectFramesSer.Serialize(writer, item);
                }
                else if (item is Flow)
                {
                    iFlowSer.Serialize(writer, item);
                }
                else if (item is CodeEditor)
                {
                    iCodeSer.Serialize(writer, item);
                }
                else if (item is FlowEditor)
                {
                    iFlowEdSer.Serialize(writer, item);
                }
                else if (item is AssetEditor)
                {
                    iAssetSer.Serialize(writer, item);
                }
                else if (item is ObjectEditor)
                {
                    iObjectSer.Serialize(writer, item);
                }
                else if (item is TextPatternEditor)
                {
                    iTextPatternSer.Serialize(writer, item);
                }
                else if (item is ObjectTextPatternEditor)
                {
                    iObjectTextPatternSer.Serialize(writer, item);
                }
                else if (item is EditorFolder)
                {
                    iFolderSer.Serialize(writer, item);
                }
                else if (item is QueryEditor)
                {
                    iQuerySer.Serialize(writer, item);
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }
        }

        #endregion
    }
}