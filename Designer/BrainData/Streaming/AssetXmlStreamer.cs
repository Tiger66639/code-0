// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   provides xml streaming for assets.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides xml streaming for assets.
    /// </summary>
    public class AssetXmlStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="parseErrors">The parse Errors.</param>
        /// <param name="stillToResolve">The still To Resolve.</param>
        /// <param name="readAssets">The read Assets.</param>
        /// <returns>The <see cref="ObjectEditor"/>.</returns>
        public static ObjectEditor Import(
            string filename, System.Collections.Generic.List<ParsableTextPatternBase> parseErrors, System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> stillToResolve, System.Collections.Generic.Dictionary<string, Neuron> readAssets = null)
        {
            var iStreamer = new AssetXmlStreamer(readAssets, parseErrors, stillToResolve);

            using (var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // XmlSerializer iSer = new XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iStreamer.ReadXml(iReader);
                    }
                }
            }

            return iStreamer.fEditor;
        }

        /// <summary>Exports the specified <paramref name="editor"/> to an xml file with
        ///     the specified name and path.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="filename">The filename.</param>
        public static void Export(ObjectEditor editor, string filename)
        {
            var iStreamer = new AssetXmlStreamer();
            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                // XmlSerializer iSer = new XmlSerializer(typeof(XmlStore));
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings)) iStreamer.WriteAssetEditor(iWriter, editor);
            }
        }

        #region fields

        /// <summary>The f editor.</summary>
        private ObjectEditor fEditor;

        /// <summary>The f read assets.</summary>
        private readonly System.Collections.Generic.Dictionary<string, Neuron> fReadAssets;

                                                                               // stores a reference to all the  assets that have already been handled (root + subs). This is stored, so we can reference assets that have already been imported/exported.

        /// <summary>The f written assets.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, string> fWrittenAssets;

        /// <summary>Initializes a new instance of the <see cref="AssetXmlStreamer"/> class. Use this constructor for importing multiple assets editors that can
        ///     share the same assets.</summary>
        /// <param name="readAssets">The sub assets.</param>
        /// <param name="parseErrors">The parse Errors.</param>
        /// <param name="stillToResolve">The still To Resolve.</param>
        public AssetXmlStreamer(System.Collections.Generic.Dictionary<string, Neuron> readAssets, System.Collections.Generic.List<ParsableTextPatternBase> parseErrors, System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> stillToResolve)
        {
            if (readAssets != null)
            {
                fReadAssets = readAssets;
            }
            else
            {
                fReadAssets = new System.Collections.Generic.Dictionary<string, Neuron>();
            }

            fParseErrors = parseErrors;
            fStillToResolve = stillToResolve;
        }

        /// <summary>Initializes a new instance of the <see cref="AssetXmlStreamer"/> class. Use this constructor for exporting multiple assets editors that can
        ///     share the same assets.</summary>
        /// <param name="writtenAssets">The written assets.</param>
        public AssetXmlStreamer(System.Collections.Generic.Dictionary<Neuron, string> writtenAssets)
        {
            fWrittenAssets = writtenAssets;
        }

        /// <summary>Initializes a new instance of the <see cref="AssetXmlStreamer"/> class. 
        ///     Use this constructor if only 1 asset editor needs to be
        ///     exported/imported.</summary>
        public AssetXmlStreamer()
        {
            fReadAssets = new System.Collections.Generic.Dictionary<string, Neuron>();
            fWrittenAssets = new System.Collections.Generic.Dictionary<Neuron, string>();
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

        #region read

        /// <summary>The read asset.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="asset">The asset.</param>
        internal void ReadAsset(System.Xml.XmlReader reader, ObjectEditor asset)
        {
            fEditor = asset;
            fEditor.Assets.IsActive = false; // don't need ui monitoring while loading.
            fEditor.DeleteAll(DeletionMethod.Delete, DeletionMethod.DeleteIfNoRef);

                // delete all previous asset items so that we can load up the new data.

            // fEditor.IsOpen = true;  not needed for reading                                                          //make certain that the editor is open, otherwise we can't add stuff.
            // fEditor.i
            ReadXml(reader);
        }

        /// <summary>The read asset.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="ObjectEditor"/>.</returns>
        internal ObjectEditor ReadAsset(System.Xml.XmlReader reader)
        {
            ReadXml(reader);
            return fEditor;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            var iId = reader.GetAttribute("ID");
            Neuron iFound;
            if (fReadAssets.TryGetValue(iId, out iFound))
            {
                if (fEditor == null)
                {
                    if (iFound is NeuronCluster && ((NeuronCluster)iFound).Meaning == (ulong)PredefinedNeurons.Asset)
                    {
                        fEditor = new AssetEditor((NeuronCluster)iFound);
                    }
                    else
                    {
                        fEditor = new ObjectEditor(iFound);
                    }

                    // fEditor.IsOpen = true;   not needed for reading
                }
                else if (fEditor is AssetEditor)
                {
                    fEditor.ItemID = iFound.ID;
                }
                else
                {
                    fEditor.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Asset, iFound);

                        // this is for object-textPatterns in the thesaururs.
                }

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                reader.ReadEndElement();
            }
            else
            {
                if (fEditor == null)
                {
                    var iNew = NeuronFactory.GetCluster();
                    Brain.Current.Add(iNew);
                    iNew.Meaning = (ulong)PredefinedNeurons.Asset;
                    fEditor = new AssetEditor(iNew);
                    ((AssetEditor)fEditor).AddItemToAssetsList();
                    fEditor.IsOpen = true; // if we don't do this, there is no initial Items list.
                }

                fReadAssets.Add(iId, fEditor.Item);

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                ReadInfo(reader, fEditor.NeuronInfo);
                ReadItems(reader);
                TryReadSubTopic(reader);
                reader.ReadEndElement();
            }
        }

        /// <summary>The try read sub topic.</summary>
        /// <param name="reader">The reader.</param>
        private void TryReadSubTopic(System.Xml.XmlReader reader)
        {
            if (reader.Name == TOPICEL)
            {
                var wasEmpty = reader.IsEmptyElement;
                if (wasEmpty)
                {
                    reader.Read();
                    return;
                }

                var iPatterns = ((AssetEditor)fEditor).Editors[0] as ObjectTextPatternEditor;

                    // we always use the already assigned editor for sub topics.
                iPatterns.IsOpen = true;
                try
                {
                    var iStreamer = new TopicXmlStreamer(fParseErrors, fStillToResolve);
                    iStreamer.ReadTopic(reader, iPatterns);
                }
                finally
                {
                    iPatterns.IsOpen = false;
                }
            }
        }

        /// <summary>The read items.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadItems(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Items" && reader.EOF == false)
            {
                // if(fEditor.Assets == null || fEditor.Assets.Cluster == null)
                // fEditor.Assets.CreateOwnerLinkIfNeeded(
                ReadAssetItem(reader, fEditor.Assets.Cluster);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read asset item.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadAssetItem(System.Xml.XmlReader reader, NeuronCluster list)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iRes = CreateAssetItem(list);
            var iAttrib = ReadAttrib(reader);
            if (iAttrib != null)
            {
                Link.Create(iRes, iAttrib, (ulong)PredefinedNeurons.Attribute);
            }

            ReadData(reader, iRes);
            reader.ReadEndElement();
        }

        /// <summary>The delete asset value.</summary>
        /// <param name="iPrevVal">The i prev val.</param>
        private void DeleteAssetValue(Neuron iPrevVal)
        {
            if (BrainHelper.HasReferences(iPrevVal) == false)
            {
                var iCluster = iPrevVal as NeuronCluster;
                if (iCluster != null)
                {
                    // if it's a cluster, we delete all non referenced children (it's an object, 'or/and' cluster or asset)
                    var iDel = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
                    iDel.Start(iPrevVal);
                }
                else
                {
                    Brain.Current.Delete(iPrevVal);
                }
            }
        }

        /// <summary>searches the <paramref name="list"/> for a neuron that links out to
        ///     the attribute, with the 'attribute' as meaning. If so , this is
        ///     returned, otherwise a new one is created.</summary>
        /// <param name="list">The list.</param>
        /// <param name="attrib">The attrib.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetAssetItem(NeuronCluster list, Neuron attrib)
        {
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = list.Children) iChildren = iList.ConvertTo<Neuron>();
            try
            {
                foreach (var i in iChildren)
                {
                    if (Link.Exists(i, attrib, (ulong)PredefinedNeurons.Attribute))
                    {
                        return i;
                    }
                }

                var iRes = CreateAssetItem(list);
                Link.Create(iRes, attrib, (ulong)PredefinedNeurons.Attribute);
                return iRes;
            }
            finally
            {
                Factories.Default.NLists.Recycle(iChildren);
            }
        }

        /// <summary>The create asset item.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron CreateAssetItem(NeuronCluster list)
        {
            var iRes = NeuronFactory.GetNeuron();
            Brain.Current.Add(iRes);
            using (var iChildren = list.ChildrenW) iChildren.Add(iRes);
            return iRes;
        }

        /// <summary>The read group.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadGroup(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement; // read group header
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            ulong iFound = 0;
            if (XmlStore.TryReadElement(reader, "ListType", ref iFound))
            {
                iRes.Meaning = iFound;
            }
            else
            {
                LogService.Log.LogError(
                    "Import asset", 
                    "Missing xml element: 'ListType' expected as first child element for groups");
            }

            while (reader.Name != "Group" && reader.EOF == false)
            {
                var iNew = ReadValueContent(reader);
                if (iNew != null)
                {
                    // can be null if something went wrong.
                    using (var iChildren = iRes.ChildrenW) iChildren.Add(iNew);
                }
            }

            reader.ReadEndElement();
            return iRes;
        }

        /// <summary>The read value.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadValue(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            var iRes = ReadTextNeuron(reader);
            reader.ReadEndElement();
            return iRes;
        }

        /// <summary>Reads the list of asset children. If the list of child assets,
        ///     references another asset, this is always returned + if 'list' was
        ///     defined, it will be deleted, but left alone.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadChildren(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            Neuron iRes = null;
            var iId = reader.GetAttribute("ID");
            var iIsRoot = reader.GetAttribute("IsRooted");
            var iName = reader.GetAttribute("Name");

            if (fReadAssets.TryGetValue(iId, out iRes) == false)
            {
                reader.Read();
                if (wasEmpty)
                {
                    return null;
                }

                var iCluster = NeuronFactory.GetCluster();
                iRes = iCluster;
                Brain.Current.Add(iCluster);
                if (string.IsNullOrEmpty(iName) == false)
                {
                    BrainData.Current.NeuronInfo[iRes].DisplayTitle = iName;
                }

                iCluster.Meaning = (ulong)PredefinedNeurons.Asset;
                if (string.IsNullOrEmpty(iIsRoot) == false && iIsRoot.ToLower() == "true")
                {
                    AddToRoot(iCluster);
                }

                fReadAssets.Add(iId, iRes);
                while (reader.Name != "Children" && reader.EOF == false)
                {
                    ReadAssetItem(reader, iCluster);
                }
            }
            else if (!wasEmpty)
            {
                // can't do this if empty, cause then we can't do the endread properly.
                reader.Skip(); // skip the children, we found it in the dict, so it is already read.
            }

            if (wasEmpty)
            {
                reader.Read();
            }
            else
            {
                reader.ReadEndElement();
            }

            return iRes;
        }

        /// <summary>Adds the asset to the root list of assets so that it can't just be
        ///     deleted + we know it's a root.</summary>
        /// <param name="toAdd">To add.</param>
        private void AddToRoot(NeuronCluster toAdd)
        {
            var iRootAssets = Brain.Current[(ulong)PredefinedNeurons.Asset] as NeuronCluster;
            if (iRootAssets != null)
            {
                using (var iChildren = iRootAssets.ChildrenW) iChildren.Add(toAdd);
            }
        }

        /// <summary>The read data.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="assetItem">The asset item.</param>
        private void ReadData(System.Xml.XmlReader reader, Neuron assetItem)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Data" && reader.EOF == false)
            {
                ReadDataValue(reader, assetItem);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read value content.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadValueContent(System.Xml.XmlReader reader)
        {
            Neuron iValue = null;
            if (reader.Name == "Children")
            {
                iValue = ReadChildren(reader);
            }
            else if (reader.Name == "Value")
            {
                iValue = ReadValue(reader);
            }
            else if (reader.Name == "Group")
            {
                iValue = ReadGroup(reader);
            }
            else if (reader.Name == "Time")
            {
                iValue = ReadTime(reader);
            }
            else if (reader.Name == "Int")
            {
                iValue = ReadIntNeuron(reader);
            }
            else if (reader.Name == "Double")
            {
                iValue = ReadDoubleNeuron(reader);
            }
            else if (reader.Name != "Item")
            {
                // could be that there is no value decalred, in which case, we should skip.
                LogService.Log.LogError(
                    "Import asset", 
                    string.Format("Unknown element in asset item: {0}", reader.Name));
                reader.ReadOuterXml(); // skip this value and try to continue
            }

            return iValue;
        }

        /// <summary>The read data value.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="assetItem">The asset item.</param>
        private void ReadDataValue(System.Xml.XmlReader reader, Neuron assetItem)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iMeaningName = XmlStore.ReadElement<string>(reader, "Meaning");

            var iValue = ReadValueContent(reader);

            var iLower = iMeaningName.ToLower();
            if (iValue != null)
            {
                // value and attribute aren't stored int he assetPronounId's list, they are fixed.
                if (iLower != "attribute" && iLower != "value" && iLower != "amount")
                {
                    var iFound = false;
                    foreach (var i in BrainData.Current.AssetPronounIds)
                    {
                        if (BrainData.Current.NeuronInfo[i].DisplayTitle.ToLower() == iLower)
                        {
                            Link.Create(assetItem, iValue, i);
                            iFound = true;
                            break;
                        }
                    }

                    if (iFound == false)
                    {
                        // couldn't find a neuron for the specified pronoun, so create one.
                        var iMeaning = NeuronFactory.GetNeuron();
                        Brain.Current.Add(iMeaning);
                        BrainData.Current.AssetPronounIds.Add(iMeaning.ID);
                        Link.Create(assetItem, iValue, iMeaning);
                    }
                }
                else if (iLower == "value")
                {
                    Link.Create(assetItem, iValue, (ulong)PredefinedNeurons.Value);
                }
                else if (iLower == "amount")
                {
                    Link.Create(assetItem, iValue, (ulong)PredefinedNeurons.Amount);
                }
                else
                {
                    Link.Create(assetItem, iValue, (ulong)PredefinedNeurons.Attribute);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>The read attrib.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadAttrib(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            var iFound = ReadTextNeuron(reader);
            reader.ReadEndElement();
            return iFound;
        }

        #endregion

        #region write

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            string iId;
            if (fWrittenAssets.TryGetValue(fEditor.Item, out iId))
            {
                writer.WriteAttributeString("ID", iId);
            }
            else
            {
                iId = System.Guid.NewGuid().ToString();
                writer.WriteAttributeString("ID", iId); // so we can find it again in the dict while reading
                fWrittenAssets.Add(fEditor.Item, iId);

                    // the cluster is the ref we need, the editor.item can refer to an object, if it was an attaced asset
                WriteInfo(writer, fEditor.NeuronInfo);
                writer.WriteStartElement("Items");
                using (IDListAccessor iLock = fEditor.Assets.Cluster.Children)
                {
                    var iChildren = iLock.ConvertTo<Neuron>();
                    foreach (var i in iChildren)
                    {
                        WriteAssetItem(writer, i);
                    }

                    Factories.Default.NLists.Recycle(iChildren);
                }

                writer.WriteEndElement();
                TryWriteSubTopic(writer);
            }
        }

        /// <summary>Tries to write the sub topic.</summary>
        /// <param name="writer">The writer.</param>
        private void TryWriteSubTopic(System.Xml.XmlWriter writer)
        {
            var iEditor = fEditor as AssetEditor;
            if (iEditor != null && iEditor.Editors.Count > 0)
            {
                var iTextEditor = iEditor.Editors[0] as ObjectTextPatternEditor;
                var iPrevOpen = iTextEditor.IsOpen;
                iTextEditor.IsOpen = true;
                try
                {
                    if (iTextEditor.Items != null && iTextEditor.Items.Item.ID != Neuron.TempId)
                    {
                        var iStreamer = new TopicXmlStreamer();
                        iStreamer.WriteTopic(writer, iTextEditor);
                    }
                }
                finally
                {
                    iTextEditor.IsOpen = iPrevOpen;
                }
            }
        }

        /// <summary>The write asset editor.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="asset">The asset.</param>
        internal void WriteAssetEditor(System.Xml.XmlWriter writer, ObjectEditor asset)
        {
            fEditor = asset;
            writer.WriteStartElement("Asset");
            WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>The write asset item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="asset">The asset.</param>
        private void WriteAssetItem(System.Xml.XmlWriter writer, Neuron asset)
        {
            writer.WriteStartElement("Item");
            WriteAttribute(writer, asset);
            WriteData(writer, asset);
            writer.WriteEndElement();
        }

        /// <summary>The write data.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        private void WriteData(System.Xml.XmlWriter writer, Neuron value)
        {
            writer.WriteStartElement("Data");

            var iData = value.FindFirstOut((ulong)PredefinedNeurons.Value);
            if (iData != null)
            {
                WriteDataItem(writer, (ulong)PredefinedNeurons.Value, iData);
            }

            iData = value.FindFirstOut((ulong)PredefinedNeurons.Amount);
            if (iData != null)
            {
                WriteDataItem(writer, (ulong)PredefinedNeurons.Amount, iData);
            }

            foreach (var i in BrainData.Current.AssetPronounIds)
            {
                iData = value.FindFirstOut(i);
                if (iData != null)
                {
                    WriteDataItem(writer, i, iData);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>The write data item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        private void WriteDataItem(System.Xml.XmlWriter writer, ulong meaning, Neuron value)
        {
            writer.WriteStartElement("DataItem");
            XmlStore.WriteElement(writer, "Meaning", BrainData.Current.NeuronInfo.GetDisplayTitleFor(meaning));
            if (value is NeuronCluster && ((NeuronCluster)value).Meaning == (ulong)PredefinedNeurons.Asset)
            {
                WriteChildren(writer, (NeuronCluster)value);
            }
            else if (value != null)
            {
                // could be no value declared (from editor), so check for this.
                WriteValue(writer, value);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write children.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        private void WriteChildren(System.Xml.XmlWriter writer, NeuronCluster data)
        {
            writer.WriteStartElement("Children");
            string iFound;
            var iIsFound = fWrittenAssets.TryGetValue(data, out iFound);

            if (iIsFound == false)
            {
                var iId = System.Guid.NewGuid().ToString();
                writer.WriteAttributeString("ID", iId); // so we can find it again in the dict while reading
                fWrittenAssets.Add(data, iId);

                    // the cluster is the ref we need, the editor.item can refer to an object, if it was an attaced asset
                writer.WriteAttributeString("IsRoot", IsRoot(data).ToString());

                    // if the asset is a root, store this info, so we can rebuild it.
                var iInfo = BrainData.Current.NeuronInfo[data];
                if (iInfo != null && string.IsNullOrEmpty(iInfo.Title) == false)
                {
                    // if the sub-asset has a title, write this out so that we can build it correctly again.
                    writer.WriteAttributeString("Name", iInfo.Title);
                }

                using (IDListAccessor iLock = data.Children)
                {
                    var iChildren = iLock.ConvertTo<Neuron>();
                    foreach (var i in iChildren)
                    {
                        WriteAssetItem(writer, i);
                    }

                    Factories.Default.NLists.Recycle(iChildren);
                }
            }
            else
            {
                writer.WriteAttributeString("ID", iFound);
            }

            writer.WriteEndElement();
        }

        /// <summary>Determines whether the specified <paramref name="asset"/> is a root
        ///     (contained by the <see cref="JaStDev.HAB.PredefinedNeurons.Asset"/>
        ///     cluster).</summary>
        /// <param name="asset">The asset.</param>
        /// <returns><c>true</c> if the specified <paramref name="asset"/> is root;
        ///     otherwise, <c>false</c> .</returns>
        private bool IsRoot(Neuron asset)
        {
            var iRootAssets = Brain.Current[(ulong)PredefinedNeurons.Asset] as NeuronCluster;
            if (iRootAssets != null && iRootAssets.ChildrenIdentifier != null)
            {
                using (var iList = iRootAssets.Children) return iList.Contains(asset);
            }

            return false;
        }

        /// <summary>The write value.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        private void WriteValue(System.Xml.XmlWriter writer, Neuron value)
        {
            var iCluster = value as NeuronCluster;
            if (iCluster != null)
            {
                if (iCluster.Meaning == (ulong)PredefinedNeurons.Or || iCluster.Meaning == (ulong)PredefinedNeurons.And
                    || iCluster.Meaning == (ulong)PredefinedNeurons.List
                    || iCluster.Meaning == (ulong)PredefinedNeurons.Argument)
                {
                    writer.WriteStartElement("Group");
                    WriteValueGroup(writer, iCluster);
                    writer.WriteEndElement();
                }
                else if (iCluster.Meaning == (ulong)PredefinedNeurons.Time)
                {
                    WriteTime(writer, iCluster);
                }
                else
                {
                    WriteTextValue(writer, value);
                }
            }
            else if (value is IntNeuron)
            {
                WriteIntNeuron(writer, (IntNeuron)value);
            }
            else if (value is DoubleNeuron)
            {
                WriteDoubleNeuron(writer, (DoubleNeuron)value);
            }
            else
            {
                WriteTextValue(writer, value);
            }
        }

        /// <summary>The write text value.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        private void WriteTextValue(System.Xml.XmlWriter writer, Neuron value)
        {
            writer.WriteStartElement("Value");
            WriteTextNeuron(writer, value);
            writer.WriteEndElement();
        }

        /// <summary>The write value group.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        private void WriteValueGroup(System.Xml.XmlWriter writer, NeuronCluster value)
        {
            XmlStore.WriteElement(writer, "ListType", value.Meaning);
            System.Collections.Generic.List<Neuron> iChildren;
            if (value.ChildrenIdentifier != null)
            {
                using (var iList = value.Children) iChildren = value.Children.ConvertTo<Neuron>();
                foreach (var i in iChildren)
                {
                    WriteValue(writer, i);
                }

                Factories.Default.NLists.Recycle(iChildren);
            }
        }

        /// <summary>The write attribute.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="neuron">The neuron.</param>
        private void WriteAttribute(System.Xml.XmlWriter writer, Neuron neuron)
        {
            var iAttrib = neuron.FindFirstOut((ulong)PredefinedNeurons.Attribute);
            writer.WriteStartElement("Attribute");
            if (iAttrib != null)
            {
                WriteTextNeuron(writer, iAttrib);
            }

            writer.WriteEndElement();
        }

        #endregion

        #endregion
    }
}