// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenDocsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that is able to store all the documents that are curently
//   open.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection that is able to store all the documents that are curently
    ///     open.
    /// </summary>
    /// <remarks>
    ///     Makes certaint that any code editors are properly loaded when displayed +
    ///     unloaded when not visual.
    /// </remarks>
    public class OpenDocsCollection : System.Collections.ObjectModel.ObservableCollection<object>, 
                                      System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f prev open docs.</summary>
        private object fPrevOpenDocs;

                       // when in viewer mode, we don't open previous docs, instead, we store which one were open. This can be the raw xml.

        /// <summary>
        ///     When loaded from xml, we stil need to open all the items after the
        ///     project is loaded. This is the place to do it.
        /// </summary>
        public void OpenLoaded()
        {
            var iOpen = fPrevOpenDocs as System.Collections.Generic.List<object>;
            if (iOpen != null)
            {
                foreach (var i in iOpen)
                {
                    if (i is EditorInfo)
                    {
                        OpenEditor((EditorInfo)i);
                    }
                    else
                    {
                        OpenEditor((System.Collections.Generic.List<int>)i);
                    }
                }

                fPrevOpenDocs = null;

                    // reset to null, otherwise we will try to stream a raw list to xml, which don't work.
            }
        }

        /// <summary>Opens the editor found in the project at the specified path.</summary>
        /// <param name="list">The list.</param>
        private void OpenEditor(System.Collections.Generic.List<int> list)
        {
            try
            {
                var iCol = BrainData.Current.Editors;
                for (var i = 0; i < list.Count - 1; i++)
                {
                    var iListOwner = iCol[list[i]] as IEditorsOwner;
                    if (iListOwner != null)
                    {
                        iCol = iListOwner.Editors;
                    }
                    else
                    {
                        return; // something went wrong, don't try to open it.
                    }
                }

                if (iCol != null && iCol.Count > list[list.Count - 1])
                {
                    // sanity check, make certain all is within range, otherwise don't try to open it.
                    var iToOpen = iCol[list[list.Count - 1]];
                    Add(iToOpen); // this opens it, adds it to the OpendDocs list as well.
                }
            }
            catch
            {
                // if something went wrong trying to show a previously opened item, don't cry about it and simply continue.
            }
        }

        /// <summary>Opens the editor by simply creating a new editor object and opening
        ///     it.</summary>
        /// <param name="toOpen">To open.</param>
        private void OpenEditor(EditorInfo toOpen)
        {
            var iEditor = System.Activator.CreateInstance(System.Type.GetType(toOpen.TypeName)) as NeuronEditor;
            iEditor.ItemID = toOpen.ID;
            iEditor.Name = iEditor.NeuronInfo.DisplayTitle;

                // normally, the name is stored in xml, but since this is dynamically gnereated, we need to assign it.
            Add(iEditor);
        }

        #region internal types

        /// <summary>
        ///     stores some info to reload an editor from the network that wasn't in
        ///     the project.
        /// </summary>
        private class EditorInfo
        {
            /// <summary>Gets or sets the type name.</summary>
            public string TypeName { get; set; }

            /// <summary>Gets or sets the id.</summary>
            public ulong ID { get; set; }
        }

        #endregion

        #region overwrites

        /// <summary>Inserts an <paramref name="item"/> into the collection at the
        ///     specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, object item)
        {
            var iItem = item as IDocOpener;
            if (iItem != null)
            {
                iItem.IsOpen = true;
            }

            base.InsertItem(index, item);

            // if (WindowMain.Current != null)                                   //need to check for this cause when loading app, we add items when WindowMain.Current not yet loaded).
            // WindowMain.Current.AddDocument(item, index);
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                var iItem = i as IDocOpener;
                if (iItem != null)
                {
                    iItem.IsOpen = false;
                }
            }

            base.ClearItems();

            // while (WindowMain.Current.AvalonDocs.Count > 0)
            // WindowMain.Current.AvalonDocs.RemoveAt(WindowMain.Current.AvalonDocs.Count - 1);
            // WindowMain.Current.AvalonDocs.Clear();
            BrainData.Current.ChatbotProps = null;
        }

        /// <summary>Replaces the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, object item)
        {
            var iPrev = this[index] as IDocOpener;
            if (iPrev != null)
            {
                iPrev.IsOpen = false;
            }

            var iItem = item as IDocOpener;
            if (iItem != null)
            {
                iItem.IsOpen = true;
            }

            base.SetItem(index, item);
        }

        /// <summary>Removes the item at the specified <paramref name="index"/> of the
        ///     collection.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            base.RemoveItem(index);
            var iPrev = iItem as IDocOpener;
            if (iPrev != null)
            {
                iPrev.IsOpen = false;
            }
            else
            {
                var iComm = iItem as CommChannel;
                if (iComm != null)
                {
                    iComm.IsVisible = false;

                        // we need to do this to make certain  that the commchannel knows it is gone.
                }
            }

            // WindowMain.Current.AvalonDocs.RemoveAt(index);
            if (iItem == BrainData.Current.ChatbotProps)
            {
                // need to reset the field if the props view is closed.
                BrainData.Current.ChatbotProps = null;
            }
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

        /// <summary>Generates an object from its XML representation. Reads all the open
        ///     editors and stores them in a temp location until the entire project is
        ///     loaded and the editors can be opened.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iRes = new System.Collections.Generic.List<object>();
            fPrevOpenDocs = iRes;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            if (ProjectManager.Default.IsViewerVisibility == System.Windows.Visibility.Collapsed)
            {
                fPrevOpenDocs = reader.ReadOuterXml();
            }
            else
            {
                while (reader.EOF == false && reader.Name != "OpenDocuments")
                {
                    ReadEditor(reader, iRes);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads all the info to open a single editor.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadEditor(System.Xml.XmlReader reader, System.Collections.Generic.List<object> result)
        {
            if (reader.Name == "ProjectPath")
            {
                var iRes = new System.Collections.Generic.List<int>();
                var iPath = XmlStore.ReadElement<string>(reader, "ProjectPath");
                foreach (var i in iPath.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    iRes.Add(int.Parse(i));
                }

                result.Add(iRes);
            }
            else
            {
                var iNew = new EditorInfo();
                iNew.TypeName = reader.Name;

                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iVal = reader.ReadString();
                ulong iId;
                if (ulong.TryParse(iVal, out iId))
                {
                    // could be that something goes wrong while trying to convert the id.
                    iNew.ID = iId;
                    result.Add(iNew);
                }
                else
                {
                    LogService.Log.LogError("Read editor", string.Format("Failed to convert: '{0}' to an id", iVal));
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (fPrevOpenDocs != null)
            {
                writer.WriteRaw((string)fPrevOpenDocs);
            }
            else
            {
                foreach (var i in this)
                {
                    var iEditor = i as EditorBase;
                    if (iEditor != null)
                    {
                        WriteEditor(writer, iEditor);
                    }
                }
            }
        }

        /// <summary>The write editor.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        private void WriteEditor(System.Xml.XmlWriter writer, EditorBase editor)
        {
            if (editor.Owner != null)
            {
                // the editor is stored in the project, so use a path into the project
                WriteEditorPath(writer, editor);
            }
            else
            {
                WriteEditorRef(writer, editor as NeuronEditor);
            }
        }

        /// <summary>Writes a reference to an editor: the type of <paramref name="editor"/>
        ///     + the neuron (this can only be for editors derived from NeuronEditor).</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        private void WriteEditorRef(System.Xml.XmlWriter writer, NeuronEditor editor)
        {
            if (editor != null)
            {
                XmlStore.WriteElement(writer, editor.GetType().FullName, editor.ItemID);
            }
        }

        /// <summary>Writes out a list of integers that form a path into the project which
        ///     can be opened again.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        private void WriteEditorPath(System.Xml.XmlWriter writer, EditorBase editor)
        {
            var fIndexes = new System.Collections.Generic.List<int>();

            var iCur = editor;
            var iOwner = editor.Owner as IEditorsOwner;
            while (iOwner != null && iOwner != BrainData.Current)
            {
                // build the path in reverse
                fIndexes.Add(iOwner.Editors.IndexOf(iCur));
                iCur = iOwner as EditorBase;
                var iTemp = (Data.OwnedObject)iOwner;
                if (iTemp != null)
                {
                    iOwner = iTemp.Owner as IEditorsOwner;
                }
                else
                {
                    iOwner = null;
                }
            }

            fIndexes.Add(BrainData.Current.Editors.IndexOf(iCur));

                // last item doesn't get added cause brainData isn't an OwnedObject.
            var iStr = new System.Text.StringBuilder();

                // write out the path with a string builder, this should be fastest.
            for (var i = fIndexes.Count - 1; i >= 0; i--)
            {
                if (i != fIndexes.Count - 1)
                {
                    iStr.Append(",");
                }

                iStr.Append(fIndexes[i]);
            }

            XmlStore.WriteElement(writer, "ProjectPath", iStr.ToString());
        }

        #endregion
    }
}