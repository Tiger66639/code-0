// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   povides xml streaming functionality for the thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     povides xml streaming functionality for the thesaurus.
    /// </summary>
    public class ThesaurusXmlStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     Gets the path of parents that are currently open, so we can check for
        ///     recursion.
        /// </summary>
        public System.Collections.Generic.HashSet<Neuron> ThesPath
        {
            get
            {
                if (fThesPath == null)
                {
                    fThesPath = new System.Collections.Generic.HashSet<Neuron>();
                }

                return fThesPath;
            }
        }

        /// <summary>
        ///     used to find the neurons for the non recursive relationships.
        /// </summary>
        internal System.Collections.Generic.Dictionary<string, Neuron> NoRecursiveRelMap
        {
            get
            {
                if (fNoRecursiveRelMap == null)
                {
                    fNoRecursiveRelMap = new System.Collections.Generic.Dictionary<string, Neuron>();
                    foreach (var i in BrainData.Current.Thesaurus.NoRecursiveRelationships)
                    {
                        fNoRecursiveRelMap.Add(i.NeuronInfo.DisplayTitle.ToLower(), i.Item);
                    }
                }

                return fNoRecursiveRelMap;
            }
        }

        /// <summary>
        ///     gets the list of id's that should be used as pos filters. We buffer
        ///     this list for speed.
        /// </summary>
        public System.Collections.Generic.List<ulong> PosFilters
        {
            get
            {
                if (fPosFilters == null)
                {
                    fPosFilters =
                        (from i in BrainData.Current.Thesaurus.PosFilters where i.Item != null select i.Item.ID).ToList(
                            );
                }

                return fPosFilters;
            }
        }

        /// <summary>
        ///     gets the list of id's that should be used as conjugation
        ///     relationships. We buffer this list for speed.
        /// </summary>
        public System.Collections.Generic.List<ulong> Conjugations
        {
            get
            {
                if (fConjugations == null)
                {
                    fConjugations =
                        (from i in BrainData.Current.Thesaurus.ConjugationMeanings where i.Item != null select i.Item.ID)
                            .ToList();
                }

                return fConjugations;
            }
        }

        /// <summary>Gets the non recursive.</summary>
        public System.Collections.Generic.List<ulong> NonRecursive
        {
            get
            {
                if (fNonRecursive == null)
                {
                    fNonRecursive =
                        (from i in BrainData.Current.Thesaurus.NoRecursiveRelationships
                         where i.Item != null
                         select i.Item.ID).ToList();
                }

                return fNonRecursive;
            }
        }

        /// <summary>Writes the description for the Item if there is one. checks if we are
        ///     writing compact, if so, check if the <paramref name="item"/> has a
        ///     link with meaning 'wordnet meaning' (as an object), if so, don't
        ///     render the description, to save space (it's already stored in the
        ///     link.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        protected override void WriteDesc(System.Xml.XmlWriter writer, Neuron item)
        {
            if (fCompact)
            {
                var iMeaning = WordNetSin.Default.WordnetMeaning;
                if (item.FindFirstOut(iMeaning.ID) != null)
                {
                    // only write the description wehn abolutely required.
                    return;
                }
            }

            base.WriteDesc(writer, item);
        }

        #region fields

        /// <summary>The f read assets.</summary>
        private System.Collections.Generic.Dictionary<string, Neuron> fReadAssets;

                                                                      // stores references to the assets that have already been generated/read

        /// <summary>The f read topics.</summary>
        private readonly System.Collections.Generic.Dictionary<string, TextPatternEditor> fReadTopics;

        /// <summary>The f written topics.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, string> fWrittenTopics;

                                                                              // so we can check if a topic is already written by the project or not.

        /// <summary>The f written assets.</summary>
        private System.Collections.Generic.Dictionary<Neuron, string> fWrittenAssets;

        /// <summary>The f current list.</summary>
        private LargeIDCollection fCurrentList;

        /// <summary>The f thes path.</summary>
        private System.Collections.Generic.HashSet<Neuron> fThesPath;

                                                           // when writing the thesaurus, we keep track of all the parents that are currently opened, so that  we  don't get stuck in a loop, when a parent is also added as a child node.

        /// <summary>The f already written.</summary>
        private readonly System.Collections.Generic.HashSet<Neuron> fAlreadyWritten =
            new System.Collections.Generic.HashSet<Neuron>();

                                                                    // we always keep track of the thes items that have already been written, so we only write the non recursive items 1 time.

        /// <summary>The f written for rel.</summary>
        private readonly System.Collections.Generic.HashSet<Neuron> fWrittenForRel =
            new System.Collections.Generic.HashSet<Neuron>();

                                                                    // we also to keep track for each relationship type, which items have already been rendered, so that we only write children 1 time (for items that are referenced multiple times for the same relationship).

        /// <summary>The f current relationship.</summary>
        private Neuron fCurrentRelationship;

                       // so we can read async while not depending on thesaurus object (which is linked to ui and doesn't handle async calls very well)

        /// <summary>The f root.</summary>
        private Neuron fRoot;

                       // identifies the item to import into/export from. When null, the entire thesaurus is expected.

        /// <summary>The f compact.</summary>
        private bool fCompact; // for exporting wordnet db: we compact the data and leave out stuff that's need needed

        /// <summary>The f no recursive rel map.</summary>
        private System.Collections.Generic.Dictionary<string, Neuron> fNoRecursiveRelMap;

        /// <summary>The f pos filters.</summary>
        private System.Collections.Generic.List<ulong> fPosFilters;

        /// <summary>The f conjugations.</summary>
        private System.Collections.Generic.List<ulong> fConjugations;

        /// <summary>The f non recursive.</summary>
        private System.Collections.Generic.List<ulong> fNonRecursive;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ThesaurusXmlStreamer"/> class. 
        ///     constructor for importing/exporting only the thesaurus</summary>
        public ThesaurusXmlStreamer()
        {
        }

        /// <summary>
        ///     Prepares the object for export when it is a stand alone export (not
        ///     part of a project exort).
        /// </summary>
        private void PrepareForExport()
        {
            fWrittenAssets = new System.Collections.Generic.Dictionary<Neuron, string>();
        }

        /// <summary>
        ///     Prepares for import.
        /// </summary>
        private void PrepareForImport()
        {
            fParseErrors = new System.Collections.Generic.List<ParsableTextPatternBase>();
            fReadAssets = new System.Collections.Generic.Dictionary<string, Neuron>();
            fStillToResolve = new System.Collections.Generic.List<ProjectStreamingOperation.ToResolve>();
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaurusXmlStreamer"/> class. Use this constructor for exporting the entire project, so that
        ///     mappings can be done between between all the editors.</summary>
        /// <param name="writtenAssets">The written Assets.</param>
        /// <param name="writtenTopics">The written Topics.</param>
        public ThesaurusXmlStreamer(System.Collections.Generic.Dictionary<Neuron, string> writtenAssets, System.Collections.Generic.Dictionary<ulong, string> writtenTopics)
        {
            fWrittenTopics = writtenTopics;
            fWrittenAssets = writtenAssets;
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaurusXmlStreamer"/> class. Initializes a new instance of the <see cref="ThesaurusXmlStreamer"/>
        ///     class.</summary>
        /// <param name="readAssets">The read assets.</param>
        /// <param name="parseErrors">The parse errors.</param>
        /// <param name="readTopics">The read Topics.</param>
        /// <param name="stillToResolve">The still To Resolve.</param>
        public ThesaurusXmlStreamer(System.Collections.Generic.Dictionary<string, Neuron> readAssets, System.Collections.Generic.List<ParsableTextPatternBase> parseErrors, System.Collections.Generic.Dictionary<string, TextPatternEditor> readTopics, System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> stillToResolve)
        {
            fReadAssets = readAssets;
            fParseErrors = parseErrors;
            fReadTopics = readTopics;
            fStillToResolve = stillToResolve;
        }

        #endregion

        #region functions

        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        public static void Import(string filename)
        {
            Import(filename, null, null);
        }

        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="into">The item to import data into.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="tracker">The tracker.</param>
        public static void Import(
            string filename, 
            ThesaurusItem into, 
            Neuron relationship, 
            Search.ProcessTrackerItem tracker = null)
        {
            try
            {
                var iStreamer = new ThesaurusXmlStreamer();
                if (into != null)
                {
                    iStreamer.fRoot = into.Item;
                }

                iStreamer.fCurrentRelationship = relationship;
                iStreamer.Tracker = new PosTracker { Tracker = tracker };
                iStreamer.PrepareForImport();
                using (
                    var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    if (iStreamer.Tracker != null)
                    {
                        iStreamer.Tracker.Stream = iFile;
                    }

                    // XmlSerializer iSer = new XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iStreamer.ReadXml(iReader);
                            iStreamer.ResolveParseErrors();
                        }
                    }
                }
            }
            finally
            {
                // Placesholders = null;                   
                AlreadyRendered = null; // need to make certain that there are no more buffers left.
                AlreadyRead = null;
            }
        }

        /// <summary>Exports the specified editor to an xml file with the specified name
        ///     and path.</summary>
        /// <param name="filename">The filename.</param>
        public static void Export(string filename)
        {
            Export(filename, null, null);
        }

        /// <summary>Exports the specified editor to an xml file with the specified name
        ///     and path.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="from">the thesaurus item to export the children from. When null, the entire
        ///     thes will be exported</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="compact">The compact.</param>
        public static void Export(string filename, ThesaurusItem from, Neuron relationship, bool compact = false)
        {
            try
            {
                var iStreamer = new ThesaurusXmlStreamer();
                if (from != null)
                {
                    iStreamer.fRoot = from.Item;
                }

                iStreamer.fCurrentRelationship = relationship;
                iStreamer.fCompact = compact;
                iStreamer.PrepareForExport();
                using (
                    var iFile = new System.IO.FileStream(
                        filename, 
                        System.IO.FileMode.Create, 
                        System.IO.FileAccess.ReadWrite))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateWriterSettings();

                        // use proper formatting for the xml, with lines it opesn bettern in text editors
                    iSettings.IndentChars = string.Empty;

                        // do a new line but don't indent the actual items, to save space, but to keep it readable.
                    using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                    {
                        iWriter.WriteStartElement("thesaurus");
                        iStreamer.WriteXml(iWriter);
                        iWriter.WriteEndElement();
                    }
                }
            }
            finally
            {
                AlreadyRendered = null; // need to make certain that these are reset after import/export
                AlreadyRead = null;
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

        #region reading

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // we block the undo system cause we might have to delete some pattern editors/assets from objects that already existed.
            try
            {
                ReadPos(reader);
                if (fCurrentRelationship == null || fRoot == null)
                {
                    ReadRootItems(reader, null);
                }
                else
                {
                    var iCluster = fRoot.FindFirstOut(fCurrentRelationship.ID) as NeuronCluster;
                    if (iCluster == null)
                    {
                        iCluster = NeuronFactory.GetCluster();
                        Brain.Current.Add(iCluster);
                        iCluster.Meaning = fCurrentRelationship.ID;
                        Link.Create(fRoot, iCluster, fCurrentRelationship);
                    }

                    ReadRootItems(reader, iCluster);
                }

                reader.ReadEndElement();
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>reads all the root values. Checks if its a full thesaurus
        ///     (relationships are defined) or just a section.</summary>
        /// <param name="reader"></param>
        /// <param name="iCluster">The i Cluster.</param>
        private void ReadRootItems(System.Xml.XmlReader reader, NeuronCluster iCluster)
        {
            if (reader.Name == "Relationship")
            {
                while (reader.Name != "thesaurus")
                {
                    ReadRelationship(reader);
                }
            }
            else
            {
                while (reader.Name != "thesaurus")
                {
                    ReadThesItem(reader, iCluster);
                }
            }
        }

        /// <summary>The read relationship.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadRelationship(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iName = XmlStore.ReadElement<string>(reader, "name");
            SetRelationship(iName);
            if (fRoot != null)
            {
                var iCluster = fRoot.FindFirstOut(fCurrentRelationship.ID) as NeuronCluster;
                if (iCluster == null)
                {
                    iCluster = NeuronFactory.GetCluster();
                    Brain.Current.Add(iCluster);
                    iCluster.Meaning = fCurrentRelationship.ID;
                    Link.Create(fRoot, iCluster, fCurrentRelationship);
                }

                while (reader.Name != "Relationship")
                {
                    ReadThesItem(reader, iCluster);
                }
            }
            else
            {
                while (reader.Name != "Relationship")
                {
                    ReadThesItem(reader, null);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Makes certain that there is a relationship with the specified<paramref name="name"/> and makes it active, so we can start importing
        ///     items for it.</summary>
        /// <param name="name">The name.</param>
        private void SetRelationship(string name)
        {
            var iName = name.ToLower();
            if (iName == "hyponym")
            {
                // we transform 'hyponym' from wordnet to 'is a', which matches internal usage.
                iName = "is a";
            }

            var iFound = false;
            ThesaurusRelItem iItem;
            for (var i = 0; i < BrainData.Current.Thesaurus.Relationships.Count; i++)
            {
                iItem = BrainData.Current.Thesaurus.Relationships[i];
                if (iItem.NeuronInfo.DisplayTitle.ToLower() == iName)
                {
                    iFound = true;
                    fCurrentList = BrainData.Current.Thesaurus.Data[iItem.Item.ID];
                    fCurrentRelationship = iItem.Item;
                    break;
                }
            }

            if (iFound == false)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                BrainData.Current.NeuronInfo[iNew].DisplayTitle = name;
                BrainData.Current.DefaultMeanings.Add(iNew);
                fCurrentList = BrainData.Current.Thesaurus.CreateRelationship(iNew);
                fCurrentRelationship = iNew;
            }
        }

        /// <summary>Reads all the values for a single part of speech.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadPos(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            // SetPos(iName);
            while (reader.Name != "POS")
            {
                var iName = XmlStore.ReadElement<string>(reader, "name");
                StorePos(iName);
            }

            reader.ReadEndElement();
        }

        /// <summary>Searches for a neuron with the specified <paramref name="name"/> that
        ///     is already registered as a part of speech, if non is found, a new one
        ///     is returned. The pos is made selected in the thesaurus, so that new
        ///     items will come in it's lists.</summary>
        /// <param name="name">The name.</param>
        private void StorePos(string name)
        {
            var iName = name.ToLower();
            var iFound = false;
            ThesaurusRelItem iItem;
            for (var i = 1; i < BrainData.Current.Thesaurus.PosFilters.Count; i++)
            {
                // we start from 1 cause 0 is the null item, to select all.
                iItem = BrainData.Current.Thesaurus.PosFilters[i];
                if (iItem.NeuronInfo.DisplayTitle.ToLower() == iName)
                {
                    iFound = true;
                    break;
                }
            }

            if (iFound == false)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                iItem = new ThesaurusRelItem(iNew);
                iItem.NeuronInfo.DisplayTitle = name;
                BrainData.Current.Thesaurus.PosFilters.Add(iItem);
            }
        }

        /// <summary>Reads a single thesaurus item.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="parent">The parent.</param>
        private void ReadThesItem(System.Xml.XmlReader reader, NeuronCluster parent)
        {
            if (CheckCancelAndPos())
            {
                return;
            }

            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            Neuron iObject;
            if (reader.Name == "Object")
            {
                iObject = ReadObject(reader);
            }
            else if (reader.Name == "PosGroup")
            {
                iObject = ReadPosGroup(reader);
            }
            else
            {
                iObject = ReadText(reader);
            }

            if (iObject == null)
            {
                throw new System.InvalidOperationException("Name of object expected!");
            }

            if (parent != null)
            {
                var iChildLock = parent.ChildrenW;
                iChildLock.Lock(iObject);
                try
                {
                    if (iChildLock.ContainsUnsafe(iObject) == false)
                    {
                        iChildLock.AddUnsafe(iObject);
                    }
                }
                finally
                {
                    iChildLock.Unlock(iObject);
                    iChildLock.Dispose();
                }
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<Neuron, Neuron>(BrainData.Current.Thesaurus.AddRootItem), 
                    fCurrentRelationship, 
                    iObject);
            }

            var iBoolVal = false;
            if (XmlStore.TryReadElement(reader, "IsAttribute", ref iBoolVal))
            {
                if (iBoolVal)
                {
                    iObject.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Attribute, iObject);
                }
            }

            ReadSynsetID(reader, iObject);
            ReadConjugations(reader, iObject);
            ReadPosRelated(reader, iObject);
            ReadObjectRelated(reader, iObject);
            ReadNonRecursive(reader, iObject);
            if (reader.Name == "BrainFile" || reader.Name == TOPICEL)
            {
                // there once was a time when topics were called brainfiles.
                ReadTopic(reader, iObject);
            }
            else if (reader.Name == "TopicID")
            {
                ReadTopicRef(reader, iObject);
            }

            if (reader.Name == "Asset")
            {
                ReadAsset(reader, iObject);
            }

            ReadThesChildren(reader, iObject);

            reader.ReadEndElement();
        }

        /// <summary>The read synset id.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadSynsetID(System.Xml.XmlReader reader, Neuron item)
        {
            var iID = 0;
            if (XmlStore.TryReadElement(reader, "SynsetID", ref iID) && iID != 0)
            {
                var iNew = NeuronFactory.GetInt(iID);
                Brain.Current.Add(iNew);
                item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.SynSetID, iNew);
            }
        }

        /// <summary>The read thes children.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadThesChildren(System.Xml.XmlReader reader, Neuron item)
        {
            if (reader.Name == "Children")
            {
                var wasEmpty = reader.IsEmptyElement;

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iChildren = item.FindFirstOut(fCurrentRelationship.ID) as NeuronCluster;
                if (iChildren == null)
                {
                    iChildren = NeuronFactory.GetCluster();
                    Brain.Current.Add(iChildren);
                    iChildren.Meaning = fCurrentRelationship.ID;
                    Link.Create(item, iChildren, fCurrentRelationship);
                }

                while (reader.Name != "Children")
                {
                    ReadThesItem(reader, iChildren);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Tries to read in the asset values for a thes item. If the thes<paramref name="item"/> already had an asset cluster, this is reused
        ///     (items are appended).</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadAsset(System.Xml.XmlReader reader, Neuron item)
        {
            var wasEmpty = reader.IsEmptyElement;

            var iCluster = item.FindFirstOut((ulong)PredefinedNeurons.Asset) as NeuronCluster;
            if (iCluster == null)
            {
                iCluster = NeuronFactory.GetCluster();
                Brain.Current.Add(iCluster);
                iCluster.Meaning = (ulong)PredefinedNeurons.Asset;
                Link.Create(item, iCluster, (ulong)PredefinedNeurons.Asset);
            }

            var iAsset = new ObjectEditor(item);
            iAsset.IsOpen = true;
            try
            {
                var iStreamer = new AssetXmlStreamer(fReadAssets, fParseErrors, fStillToResolve);
                iStreamer.ReadAsset(reader, iAsset);
            }
            finally
            {
                iAsset.IsOpen = false;
            }
        }

        /// <summary>The read topic.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadTopic(System.Xml.XmlReader reader, Neuron item)
        {
            var wasEmpty = reader.IsEmptyElement;
            if (wasEmpty)
            {
                reader.Read();
                return;
            }

            var iPatterns = new ObjectTextPatternEditor(item);

            // iPatterns.Name = item.NeuronInfo.DisplayTitle;                                            //we need to set the nama manually, otherwise a whole set of things go wrong. the objectTextPattnerEditor doesn't get it's name automatically and
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

        /// <summary>Reads the topic ref, resolves it and replaces the editor with the
        ///     correct own.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iItem">The i item.</param>
        private void ReadTopicRef(System.Xml.XmlReader reader, Neuron iItem)
        {
            var iID = XmlStore.ReadElement<string>(reader, "TopicID");
            TextPatternEditor iFound;

            if (fReadTopics.TryGetValue(iID, out iFound))
            {
                Link.Create(iItem, iFound.Item, (ulong)PredefinedNeurons.TextPatternTopic);
                var iNew = new ObjectTextPatternEditor(iItem);
                if (iFound.Owner is BrainData)
                {
                    var iIndex = ((BrainData)iFound.Owner).Editors.IndexOf(iFound);
                    ((BrainData)iFound.Owner).Editors[iIndex] = iNew;
                }
                else if (iFound.Owner is EditorFolder)
                {
                    var iIndex = ((EditorFolder)iFound.Owner).Items.IndexOf(iFound);
                    ((EditorFolder)iFound.Owner).Items[iIndex] = iNew;
                }
            }
        }

        /// <summary>The read non recursive.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadNonRecursive(System.Xml.XmlReader reader, Neuron item)
        {
            if (reader.Name == "NonRecursive")
            {
                var wasEmpty = reader.IsEmptyElement;

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "NonRecursive" && reader.EOF == false)
                {
                    ReadNonRecursiveItem(reader, item);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read non recursive item.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadNonRecursiveItem(System.Xml.XmlReader reader, Neuron item)
        {
            var wasEmpty = reader.IsEmptyElement;

            if (wasEmpty)
            {
                reader.Read();
                return;
            }

            var iName = reader.GetAttribute("relationship");
            reader.Read();
            var iRelNeuron = GetRelNeuron(iName);
            var iCluster = item.FindFirstOut(iRelNeuron.ID) as NeuronCluster;
            if (iCluster == null)
            {
                iCluster = NeuronFactory.GetCluster();
                Brain.Current.Add(iCluster);
                iCluster.Meaning = iRelNeuron.ID;
                Link.Create(item, iCluster, iRelNeuron.ID);
            }

            System.Collections.Generic.HashSet<ulong> iAlreadyStored;
            using (var iList = iCluster.Children) iAlreadyStored = new System.Collections.Generic.HashSet<ulong>(iList);

            while (reader.Name != "NonRecursiveItem" && reader.EOF == false)
            {
                // read all the objects for this sub relationship
                var iObj = ReadTextNeuron(reader);
                if (iAlreadyStored.Contains(iObj.ID) == false)
                {
                    using (var iChildren = iCluster.ChildrenW) iChildren.Add(iObj);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Gets the neuron for the specifid non recursive relationship.</summary>
        /// <param name="iName">Name of the i.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetRelNeuron(string iName)
        {
            iName = iName.ToLower();
            if (iName == "antonym")
            {
                iName = "opposite";
            }

            Neuron iRel = null;
            if (NoRecursiveRelMap.TryGetValue(iName, out iRel) == false)
            {
                iRel = NeuronFactory.GetNeuron();
                Brain.Current.Add(iRel);
                BrainData.Current.NeuronInfo[iRel].DisplayTitle = iName;
                BrainData.Current.DefaultMeanings.Add(iRel);
                var iThesItem = new ThesaurusRelItem(iRel);
                BrainData.Current.Thesaurus.NoRecursiveRelationships.Add(iThesItem);
                NoRecursiveRelMap.Add(iName, iRel);
                return iRel;
            }

            return iRel;
        }

        /// <summary>The read object related.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadObjectRelated(System.Xml.XmlReader reader, Neuron item)
        {
            if (reader.Name == "ObjectRelated")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "ObjectRelated")
                {
                    ReadObjectRelatedItem(reader, item);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Reads 2 objects that represent 'meaning' and 'to' for a link. They
        ///     form an object-related item.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadObjectRelatedItem(System.Xml.XmlReader reader, Neuron item)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iRelationship = ReadObject(reader);
            var iRelated = ReadTextNeuron(reader);

            var iFound = false;
            if (item.LinksOutIdentifier != null)
            {
                using (var iLinks = item.LinksOut)
                    foreach (var i in iLinks)
                    {
                        if (i.ToID == iRelated.ID && i.MeaningID == iRelationship.ID)
                        {
                            iFound = true;
                            break;
                        }
                    }

                if (iFound == false)
                {
                    Link.Create(item, iRelated, iRelationship);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>The read pos related.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadPosRelated(System.Xml.XmlReader reader, Neuron item)
        {
            if (reader.Name == "PosRelated")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "PosRelated")
                {
                    ReadLinkedItem(reader, item, BrainData.Current.Thesaurus.PosFilters, PosMappings);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Reads the linked item. The <paramref name="dict"/> is for faster
        ///     searching.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        /// <param name="list">The list.</param>
        /// <param name="dict">The dict.</param>
        private void ReadLinkedItem(
            System.Xml.XmlReader reader, 
            Neuron item, 
            ThesaurusRelItemCollection list, System.Collections.Generic.Dictionary<string, Neuron> dict)
        {
            if (reader.IsEmptyElement)
            {
                throw new System.InvalidOperationException();
            }

            Neuron iRelationship;
            var iName = reader.GetAttribute("relationship");
            reader.Read();

            var iLowerName = iName.ToLower();
            var iStripped = iLowerName.Replace(" ", string.Empty);

                // at some point, we had to remove the spaces in the names of the relationships (for the functions)
            if (dict.TryGetValue(iLowerName, out iRelationship) == false
                && dict.TryGetValue(iStripped, out iRelationship) == false)
            {
                ThesaurusRelItem iFound = null;
                iRelationship = NeuronFactory.GetNeuron();
                Brain.Current.Add(iRelationship);
                iFound = new ThesaurusRelItem(iRelationship);
                iFound.NeuronInfo.DisplayTitle = iName;
                list.Add(iFound);
                var iRelated = ReadTextNeuron(reader);
                Link.Create(item, iRelated, iRelationship);
                dict.Add(iLowerName, iRelationship);
            }
            else
            {
                var iRelated = ReadTextNeuron(reader);
                var iLinkExists = false;
                if (item.LinksOutIdentifier != null)
                {
                    using (var iLinks = item.LinksOut)
                        foreach (var i in iLinks)
                        {
                            if (i.ToID == iRelated.ID && i.MeaningID == iRelationship.ID)
                            {
                                iLinkExists = true;
                                break;
                            }
                        }
                }

                if (iLinkExists == false)
                {
                    Link.Create(item, iRelated, iRelationship);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>The read conjugations.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadConjugations(System.Xml.XmlReader reader, Neuron item)
        {
            if (reader.Name == "Conjugations")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "Conjugations" && reader.EOF == false)
                {
                    ReadLinkedItem(reader, item, BrainData.Current.Thesaurus.ConjugationMeanings, ConjugationMeanings);
                }

                reader.ReadEndElement();
            }
        }

        #endregion

        #region writing

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            WritePosValues(writer);
            if (fCurrentRelationship == null || fRoot == null)
            {
                foreach (var i in BrainData.Current.Thesaurus.Relationships)
                {
                    var iName = i.NeuronInfo.DisplayTitle.ToLower();
                    if (fCompact == false || (iName != "hypernym" && iName != "instance hypernym"))
                    {
                        // when compacting: don't write the reverse relationships, these are already known
                        WriteValuesForRelationship(writer, i);
                    }

                    fWrittenForRel.Clear();

                        // after each relationship type, clear the list so that we can render the children again.
                }
            }
            else
            {
                WriteThesItem(writer, fRoot);
            }
        }

        /// <summary>Writes the values for relationship.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="rel">The rel.</param>
        private void WriteValuesForRelationship(System.Xml.XmlWriter writer, ThesaurusRelItem rel)
        {
            fCurrentList = BrainData.Current.Thesaurus.Data[rel.Item.ID];
            fCurrentRelationship = rel.Item;
            writer.WriteStartElement("Relationship");
            XmlStore.WriteElement(writer, "name", rel.NeuronInfo.DisplayTitle);
            foreach (var i in fCurrentList)
            {
                WriteThesItem(writer, Brain.Current[i]);
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes all the thes items for a single pos.</summary>
        /// <param name="writer">The writer.</param>
        private void WritePosValues(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("POS");

            for (var i = 1; i < BrainData.Current.Thesaurus.PosFilters.Count; i++)
            {
                var iItem = BrainData.Current.Thesaurus.PosFilters[i];
                XmlStore.WriteElement(writer, "name", iItem.NeuronInfo.DisplayTitle);
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes all the values for a single thes <paramref name="item"/> + it's
        ///     chidren.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteThesItem(System.Xml.XmlWriter writer, Neuron item)
        {
            // item.IsSelected = true;                                                          //we select the item, so that the thesaurus can load it's other data elements.
            writer.WriteStartElement("Item");
            if (item is NeuronCluster)
            {
                // the thes item could be a textNeuron or posgroup, so check for this.
                var iCluster = (NeuronCluster)item;
                if (iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                {
                    if (fCompact == false)
                    {
                        WriteObjectRef(writer, item as NeuronCluster);
                    }
                    else
                    {
                        WriteObjectRefCompact(writer, item as NeuronCluster);
                    }
                }
                else if (iCluster.Meaning == (ulong)PredefinedNeurons.POSGroup)
                {
                    WritePosGroup(writer, iCluster);
                }
                else
                {
                    throw new System.InvalidOperationException("Unknown thesaurus item");
                }
            }
            else if (item is TextNeuron)
            {
                WriteText(writer, (TextNeuron)item, null);
            }
            else
            {
                throw new System.InvalidOperationException("Unknown thesaurus item");
            }

            WriteIsAttrib(writer, item);
            if (fAlreadyWritten.Contains(item) == false)
            {
                // to save room, we make certain that the non-recursive part of a thes item is only written 1 time. While reading, it only needs to be read once.
                fAlreadyWritten.Add(item);
                WriteSynsetId(writer, item);
                WriteConjugations(writer, item);
                WritePosRelated(writer, item);
                WriteObjectRelated(writer, item);
                WriteNonRecursive(writer, item);
                WriteTopic(writer, item);
                var iAsset = item.FindFirstOut((ulong)PredefinedNeurons.Asset) as NeuronCluster;
                if (iAsset != null)
                {
                    WriteAsset(writer, item);
                }
            }

            if (fWrittenForRel.Contains(item) == false)
            {
                // only write the children if this hasn't been done yet. this saves lots of room in case that an item is referenced 2 times in the same relationship type.
                fWrittenForRel.Add(item);
                WriteThesChildren(writer, item); // thes children are local to the current relationship, so alway write.
            }

            writer.WriteEndElement();
        }

        /// <summary>The write is attrib.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteIsAttrib(System.Xml.XmlWriter writer, Neuron item)
        {
            var iIsAttrib = item.FindFirstOut((ulong)PredefinedNeurons.Attribute) != null;
            XmlStore.WriteElement(writer, "IsAttribute", iIsAttrib);
        }

        /// <summary>Writes the synset id.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteSynsetId(System.Xml.XmlWriter writer, Neuron item)
        {
            var iFound = item.FindFirstOut((ulong)PredefinedNeurons.SynSetID) as IntNeuron;
            if (iFound != null)
            {
                XmlStore.WriteElement(writer, "SynsetID", iFound.Value);
            }
        }

        /// <summary>The write thes children.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteThesChildren(System.Xml.XmlWriter writer, Neuron item)
        {
            // bool iExpanded = item.IsExpanded;
            if (ThesPath.Contains(item) == false)
            {
                // we only write children of the neuron if we aren't rendering it already, otherwise we would get into a loop
                ThesPath.Add(item);
                try
                {
                    var iFound = item.FindFirstOut(fCurrentRelationship.ID) as NeuronCluster;
                    if (iFound != null && iFound.ChildrenIdentifier != null)
                    {
                        System.Collections.Generic.List<Neuron> iToWrite;
                        using (var iList = iFound.Children) iToWrite = iList.ConvertTo<Neuron>();
                        if (iToWrite.Count > 0)
                        {
                            writer.WriteStartElement("Children");
                            foreach (var i in iToWrite)
                            {
                                WriteThesItem(writer, i);
                            }

                            writer.WriteEndElement();
                        }

                        Factories.Default.NLists.Recycle(iToWrite);
                    }
                }
                finally
                {
                    ThesPath.Remove(item);
                }
            }
        }

        /// <summary>The write asset.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="assetFor">The asset for.</param>
        protected void WriteAsset(System.Xml.XmlWriter writer, Neuron assetFor)
        {
            var iAsset = new ObjectEditor(assetFor);

                // we use an asset editor and not an objectEditor since we already retrieved the asset object.
            iAsset.IsOpen = true;
            try
            {
                var iStreamer = new AssetXmlStreamer(fWrittenAssets);
                iStreamer.WriteAssetEditor(writer, iAsset);
            }
            finally
            {
                iAsset.IsOpen = false;
            }
        }

        /// <summary>The write topic.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="neuron">The neuron.</param>
        protected void WriteTopic(System.Xml.XmlWriter writer, Neuron neuron)
        {
            var iTopicCluster = neuron.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic);
            if (iTopicCluster != null)
            {
                if (fWrittenTopics != null)
                {
                    // when we are writing for the project, we don't need to write the complete topic, just a unique id.
                    string iFound;
                    if (fWrittenTopics.TryGetValue(iTopicCluster.ID, out iFound))
                    {
                        XmlStore.WriteElement(writer, "TopicID", iFound);
                        return;
                    }
                }

                var iEditor = new ObjectTextPatternEditor(neuron);
                var iStreamer = new TopicXmlStreamer();
                iStreamer.WriteTopic(writer, iEditor);
            }
        }

        /// <summary>The write non recursive.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteNonRecursive(System.Xml.XmlWriter writer, Neuron item)
        {
            var iToWrite = Factories.Default.LinkLists.GetBuffer();
            try
            {
                if (item.LinksOutIdentifier != null)
                {
                    using (var iLinks = item.LinksOut)
                        foreach (var i in iLinks)
                        {
                            if (NonRecursive.Contains(i.MeaningID))
                            {
                                iToWrite.Add(i);
                            }
                        }
                }

                if (iToWrite.Count > 0)
                {
                    writer.WriteStartElement("NonRecursive");
                    foreach (var i in iToWrite)
                    {
                        WriteNonRecursiveItem(writer, i.MeaningID, i.To as NeuronCluster);
                    }

                    writer.WriteEndElement();
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iToWrite);
            }
        }

        /// <summary>The write object related.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteObjectRelated(System.Xml.XmlWriter writer, Neuron item)
        {
            var iToWrite = Factories.Default.LinkLists.GetBuffer();
            try
            {
                if (item.LinksOutIdentifier != null)
                {
                    using (var iLinks = item.LinksOut)
                        foreach (var i in iLinks)
                        {
                            var iMeaning = i.Meaning as NeuronCluster;
                            if (iMeaning != null && iMeaning.Meaning == (ulong)PredefinedNeurons.Object)
                            {
                                iToWrite.Add(i);
                            }
                        }
                }

                if (iToWrite.Count > 0)
                {
                    writer.WriteStartElement("ObjectRelated");
                    foreach (var i in iToWrite)
                    {
                        WriteObjectLinkedItem(writer, i.Meaning, i.To);
                    }

                    writer.WriteEndElement();
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iToWrite);
            }
        }

        /// <summary>The write pos related.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void WritePosRelated(System.Xml.XmlWriter writer, Neuron item)
        {
            try
            {
                var iRelationships = PosFilters;
                var iToWrite = Factories.Default.LinkLists.GetBuffer();
                try
                {
                    if (item.LinksOutIdentifier != null)
                    {
                        using (var iLinks = item.LinksOut)
                            foreach (var i in iLinks)
                            {
                                if (iRelationships.Contains(i.MeaningID))
                                {
                                    iToWrite.Add(i);
                                }
                            }
                    }

                    if (iToWrite.Count > 0)
                    {
                        writer.WriteStartElement("PosRelated");
                        foreach (var i in iToWrite)
                        {
                            WriteLinkedItem(writer, i.Meaning, i.To);
                        }

                        writer.WriteEndElement();
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iToWrite);
                }
            }
            catch (System.Exception e)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to write Pos related for: {0}", 
                        BrainData.Current.NeuronInfo[item].DisplayTitle), 
                    e);
            }
        }

        /// <summary>The write conjugations.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void WriteConjugations(System.Xml.XmlWriter writer, Neuron item)
        {
            try
            {
                var iRelationships = Conjugations;
                var iToWrite = Factories.Default.LinkLists.GetBuffer();
                try
                {
                    if (item.LinksOutIdentifier != null)
                    {
                        using (var iLinks = item.LinksOut)
                            foreach (var i in iLinks)
                            {
                                if (iRelationships.Contains(i.MeaningID))
                                {
                                    iToWrite.Add(i);
                                }
                            }
                    }

                    if (iToWrite.Count > 0)
                    {
                        writer.WriteStartElement("Conjugations");
                        foreach (var i in iToWrite)
                        {
                            WriteLinkedItem(writer, i.Meaning, i.To);
                        }

                        writer.WriteEndElement();
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iToWrite);
                }
            }
            catch (System.Exception e)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to write conjugation for: {0}", 
                        BrainData.Current.NeuronInfo[item].DisplayTitle), 
                    e);
            }
        }

        /// <summary>The write non recursive item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="related">The related.</param>
        private void WriteNonRecursiveItem(System.Xml.XmlWriter writer, ulong relationship, NeuronCluster related)
        {
            if (related != null)
            {
                writer.WriteStartElement("NonRecursiveItem");
                writer.WriteAttributeString(
                    "relationship", 
                    BrainData.Current.NeuronInfo.GetDisplayTitleFor(relationship));
                System.Collections.Generic.List<Neuron> iList;
                using (var iChildren = related.Children) iList = iChildren.ConvertTo<Neuron>();
                try
                {
                    foreach (var i in iList)
                    {
                        WriteTextNeuron(writer, i, fCompact);
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iList);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write linked item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteLinkedItem(System.Xml.XmlWriter writer, ThesaurusLinkedItem item)
        {
            writer.WriteStartElement("link");
            writer.WriteAttributeString("relationship", BrainData.Current.NeuronInfo[item.Relationship].DisplayTitle);
            WriteTextNeuron(writer, item.Related, fCompact);
            writer.WriteEndElement();
        }

        /// <summary>The write linked item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="related">The related.</param>
        private void WriteLinkedItem(System.Xml.XmlWriter writer, Neuron relationship, Neuron related)
        {
            writer.WriteStartElement("link");
            writer.WriteAttributeString("relationship", BrainData.Current.NeuronInfo[relationship].DisplayTitle);
            WriteTextNeuron(writer, related, fCompact);
            writer.WriteEndElement();
        }

        /// <summary>The write object linked item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="related">The related.</param>
        private void WriteObjectLinkedItem(System.Xml.XmlWriter writer, Neuron relationship, Neuron related)
        {
            writer.WriteStartElement("ObjectLink");
            if (fCompact == false)
            {
                WriteObjectRef(writer, relationship as NeuronCluster);
            }
            else
            {
                WriteObjectRefCompact(writer, relationship as NeuronCluster);
            }

            WriteTextNeuron(writer, related, fCompact);
            writer.WriteEndElement();
        }

        /// <summary>The write object linked item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteObjectLinkedItem(System.Xml.XmlWriter writer, ThesaurusLinkedItem item)
        {
            writer.WriteStartElement("ObjectLink");
            if (fCompact == false)
            {
                WriteObjectRef(writer, item.Relationship as NeuronCluster);
            }
            else
            {
                WriteObjectRefCompact(writer, item.Relationship as NeuronCluster);
            }

            WriteTextNeuron(writer, item.Related, fCompact);
            writer.WriteEndElement();
        }

        #endregion

        #endregion
    }
}