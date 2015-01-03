// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   imports, exports the entire project to xml. This is currently primarely
//   used for upgrading
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     imports, exports the entire project to xml. This is currently primarely
    ///     used for upgrading
    /// </summary>
    public class ProjectXmlStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ProjectXmlStreamer"/> class. 
        ///     Initializes a new instance of the <see cref="ProjectXmlStreamer"/>
        ///     class.</summary>
        public ProjectXmlStreamer()
        {
            fReadAssets = new System.Collections.Generic.Dictionary<string, Neuron>();
            fWrittenAssets = new System.Collections.Generic.Dictionary<Neuron, string>();
            fParseErrors = new System.Collections.Generic.List<ParsableTextPatternBase>();
            fWrittenPatterns = new System.Collections.Generic.Dictionary<ulong, string>();
            fReadTopics = new System.Collections.Generic.Dictionary<string, TextPatternEditor>();
        }

        #endregion

        #region fields

        /// <summary>The f read assets.</summary>
        private readonly System.Collections.Generic.Dictionary<string, Neuron> fReadAssets;

                                                                               // stores references to the assets that have already been generated/read

        /// <summary>The f read topics.</summary>
        private readonly System.Collections.Generic.Dictionary<string, TextPatternEditor> fReadTopics;

        /// <summary>The f written patterns.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, string> fWrittenPatterns;

                                                                              // keeps track of the topics that were attached to objects/assets and which were rendered by the editors list, so they don't get rendered 2 times.

        /// <summary>The f written assets.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, string> fWrittenAssets;

        #endregion

        #region functions

        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        public static void Import(string filename)
        {
            var iStreamer = new ProjectXmlStreamer();
            using (var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iStreamer.ReadXml(iReader);
                    }
                }
            }
        }

        /// <summary>The import props.</summary>
        /// <param name="filename">The filename.</param>
        public static void ImportProps(string filename)
        {
            var iStreamer = new ProjectXmlStreamer();
            using (var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        Parsers.ParserBase.BlockLogErrors = true;

                            // so we only report the errors at the end when we want it.
                        try
                        {
                            var wasEmpty = iReader.IsEmptyElement;

                            iReader.Read();
                            if (wasEmpty)
                            {
                                return;
                            }

                            iStreamer.ReadGlobals(iReader);
                            iReader.ReadEndElement();
                        }
                        finally
                        {
                            Parsers.ParserBase.BlockLogErrors = false;
                        }

                        iStreamer.ResolveParseErrors();
                    }
                }
            }
        }

        /// <summary>Exports the project to an xml file with the specified name and path.</summary>
        /// <param name="filename">The filename.</param>
        public static void Export(string filename)
        {
            var iStreamer = new ProjectXmlStreamer();
            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                {
                    iWriter.WriteStartElement("CBDProject");
                    iStreamer.WriteXml(iWriter);
                    iWriter.WriteEndElement();
                }
            }
        }

        /// <summary>Exports the project's global patterns and bot's props to an xml file
        ///     with the specified name and path.</summary>
        /// <param name="filename">The filename.</param>
        internal static void ExportProps(string filename)
        {
            var iStreamer = new ProjectXmlStreamer();
            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                {
                    iWriter.WriteStartElement("CBDGlobals");
                    iStreamer.WriteGlobals(iWriter);
                    iWriter.WriteEndElement();
                }
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
            Parsers.ParserBase.BlockLogErrors = true; // so we only report the errors at the end when we want it.
            try
            {
                var wasEmpty = reader.IsEmptyElement;

                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                ReadGlobals(reader);

                if (reader.Name == "Online")
                {
                    // this is an optional part.
                    BrainData.Current.DesignerData.OnlineInfo = OnlineData.ReadFromXml(reader);
                }

                ReadUserAssets(reader);
                ReadBotAsset(reader);
                ReadTextAssets(reader);
                ReadThes(reader);
                var iList = new System.Collections.Generic.List<EditorBase>();
                ReadEditors(reader, iList);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new System.Action<System.Collections.Generic.List<EditorBase>>(AddRootEditors), 
                    iList);

                    // this must be done async cause it is done from other thread then UI, but root is usually rendered, which would be a problem.
                reader.ReadEndElement();
            }
            finally
            {
                Parsers.ParserBase.BlockLogErrors = false;

                // ThesaurusXmlStreamer.Placesholders = null;                                         
                AlreadyRendered = null; // need to make certain that there are no more buffers left.
                AlreadyRead = null;
            }

            ResolveParseErrors();
        }

        /// <summary>The read globals.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadGlobals(System.Xml.XmlReader reader)
        {
            ReadReflectionData(reader);
            ReadDoOnStartup(reader);
            ReadOpeningStatements(reader);
            ReadFallbacks(reader);
            ReadContext(reader);
            ReadDoStatements(reader);
            ReadRepeatResponses(reader);

            if (reader.Name.ToLower() == "moduleproperties")
            {
                // check if it's a new format of project properties xml file.
                ReadModuleProps(reader);
            }
            else
            {
                ReadDepricated(reader);
            }
        }

        /// <summary>used to read te old format of module properties (fixed form).</summary>
        /// <param name="reader"></param>
        private void ReadDepricated(System.Xml.XmlReader reader)
        {
            var iProps = new ChatbotProperties();
            iProps.UseOutputVar = XmlStore.ReadElement<bool>(reader, "UseOutputVar");
            iProps.AutoResolveSyns = XmlStore.ReadElement<bool>(reader, "AutoResolveSyns");
            var iRead = false;
            if (XmlStore.TryReadElement(reader, "UseSTTWeight", ref iRead))
            {
                iProps.UseSTTWeight = iRead;
            }

            if (XmlStore.TryReadElement(reader, "SingleTopPatternResult", ref iRead))
            {
                iProps.SingleTopPatternResult = iRead;
            }

            iProps = null;
        }

        /// <summary>reads the newer, variable form of module properties.</summary>
        /// <param name="reader"></param>
        private void ReadModuleProps(System.Xml.XmlReader reader)
        {
            var iProps = new ChatbotProperties();

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.EOF == false && reader.Name.ToLower() != "moduleproperties")
            {
                var iName = reader.Name.ToLower();
                var iFound =
                    (from i in iProps.ModuleProps where i.XmlElementName.ToLower() == iName select i).FirstOrDefault();
                if (iFound != null)
                {
                    iFound.ReadXmlValue(reader);
                }
                else
                {
                    LogService.Log.LogError(
                        "Project properties", 
                        "Failed to find module property for xml element: " + reader.Name);
                    reader.ReadOuterXml(); // skip the element.
                }
            }

            reader.ReadEndElement();
            iProps = null;
        }

        /// <summary>The read reflection data.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadReflectionData(System.Xml.XmlReader reader)
        {
            System.Collections.Generic.List<ExportableReflectionSinEntryPoint> iValues;
            if (reader.Name == "ArrayOfExportableReflectionSinEntryPoint")
            {
                // it could be that we are upgrading a project that didn't have any reflection-sin entrypoints.
                var valueSerializer =
                    new System.Xml.Serialization.XmlSerializer(
                        typeof(System.Collections.Generic.List<ExportableReflectionSinEntryPoint>));
                iValues =
                    (System.Collections.Generic.List<ExportableReflectionSinEntryPoint>)
                    valueSerializer.Deserialize(reader);

                var iChannel =
                    (from i in BrainData.Current.CommChannels where i is ReflectionChannel select (ReflectionChannel)i)
                        .FirstOrDefault();
                if (iChannel != null)
                {
                    var iSin = iChannel.Sin as ReflectionSin;

                    LoadEntryPoints(iSin, iValues);
                    iChannel.AssemblyFiles = (from i in iSin.FunctionAssemblies.Keys select i).ToList();

                        // get all the loaded assemblies and display them to the user.
                }
            }
        }

        /// <summary>Loads the entry points that were exported from another project. Makes
        ///     certain that all the libs and functions are loaded.</summary>
        /// <param name="sin">The sin.</param>
        /// <param name="values">The values.</param>
        private static void LoadEntryPoints(
            ReflectionSin sin, System.Collections.Generic.List<ExportableReflectionSinEntryPoint> values)
        {
            foreach (var i in values)
            {
                var iItem = sin.LoadMethod(i);
                if (iItem != null)
                {
                    BrainData.Current.NeuronInfo[iItem].DisplayTitle = i.MappedName;
                    BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.Add(iItem.ID);

                        // also need to register the function, so the parsers can find it.
                }
            }
        }

        /// <summary>The read context.</summary>
        /// <param name="reader">The reader.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReadContext(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iBot = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.BotID];
            if (iBot != null)
            {
                var iContext = iBot.FindFirstOut((ulong)PredefinedNeurons.Context) as NeuronCluster;
                if (iContext == null)
                {
                    iContext = NeuronFactory.GetCluster();
                    Brain.Current.Add(iContext);
                    iContext.Meaning = (ulong)PredefinedNeurons.TextPatternOutputs;
                }

                System.Diagnostics.Debug.Assert(iContext != null);
                ReadClusterContentAsOutputPatterns(reader, iContext, "Context");
            }
            else
            {
                throw new System.InvalidOperationException("Couldn't find bot cluster, can't import context patterns.");
            }

            reader.ReadEndElement();
        }

        /// <summary>The read repeat responses.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadRepeatResponses(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Repetitions")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iReps = Brain.Current[(ulong)PredefinedNeurons.RepeatOutputPatterns] as NeuronCluster;
                System.Diagnostics.Debug.Assert(iReps != null);

                while (reader.Name != "Repetitions" && reader.EOF == false)
                {
                    ReadRepetition(reader, iReps);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read repetition.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iReps">The i reps.</param>
        private void ReadRepetition(System.Xml.XmlReader reader, NeuronCluster iReps)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iNew = NeuronFactory.GetCluster();
            Brain.Current.Add(iNew);
            iNew.Meaning = (ulong)PredefinedNeurons.TextPatternOutputs;
            using (var iChildren = iReps.ChildrenW) iChildren.Add(iNew);
            if (reader.Name == "Condition")
            {
                var iCond = ReadCondition(reader);
                Link.Create(iNew, iCond, (ulong)PredefinedNeurons.Condition);
                var iPattern = new ConditionPattern(iCond);
                iPattern.Parse();
                if (iPattern.HasError)
                {
                    fParseErrors.Add(iPattern);
                }
            }

            ReadClusterContentAsOutputPatterns(reader, iNew, "Repetition");
            ReadDoPatterns(reader, iNew);
            reader.ReadEndElement();
        }

        /// <summary>The read condition.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        private TextNeuron ReadCondition(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName);
            var iNew = ReadExpression(reader);
            var iText = NeuronFactory.GetText(iNew);
            Brain.Current.Add(iText);
            var iData = BrainData.Current.NeuronInfo[iText];
            iData.Title = iName;
            iData.DescriptionText = iDesc;

            reader.ReadEndElement();
            return iText;
        }

        /// <summary>The read do patterns.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="outputCluster">The output cluster.</param>
        private void ReadDoPatterns(System.Xml.XmlReader reader, NeuronCluster outputCluster)
        {
            if (reader.Name == "DoPatterns")
            {
                ReadDoPattern(reader, outputCluster);
            }
        }

        /// <summary>The read do on startup.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadDoOnStartup(System.Xml.XmlReader reader)
        {
            if (reader.Name == "DoOnStartup")
            {
                ReadDoPattern(reader, Brain.Current[(ulong)PredefinedNeurons.DoOnStartup]);
            }
        }

        /// <summary>The read do statements.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadDoStatements(System.Xml.XmlReader reader)
        {
            if (reader.Name == "DoAfterStatement")
            {
                ReadDoPattern(reader, Brain.Current[(ulong)PredefinedNeurons.DoAfterStatement]);
            }
        }

        /// <summary>The add root editors.</summary>
        /// <param name="list">The list.</param>
        private void AddRootEditors(System.Collections.Generic.List<EditorBase> list)
        {
            foreach (var i in list)
            {
                BrainData.Current.Editors.Add(i);
            }
        }

        /// <summary>The read fallbacks.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadFallbacks(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iFallbacks = Brain.Current[(ulong)PredefinedNeurons.ResponsesForEmptyParse] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iFallbacks != null);
            ReadClusterContentAsOutputPatterns(reader, iFallbacks, "Fallbacks");
            reader.ReadEndElement();
        }

        /// <summary>The read opening statements.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadOpeningStatements(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iOpenings = Brain.Current[(ulong)PredefinedNeurons.ConversationStarts] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iOpenings != null);
            ReadClusterContentAsOutputPatterns(reader, iOpenings, "OpeningStatements");
            reader.ReadEndElement();
        }

        /// <summary>The read cluster content as output patterns.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="name">The name.</param>
        private void ReadClusterContentAsOutputPatterns(System.Xml.XmlReader reader, NeuronCluster cluster, string name)
        {
            while (reader.Name != name && reader.EOF == false)
            {
                var iText = ReadOutput(reader);
                if (iText != null)
                {
                    using (var iChildren = cluster.ChildrenW) iChildren.Add(iText);
                    var iPattern = new OutputPattern(iText);
                    iPattern.Parse();
                    if (iPattern.HasError)
                    {
                        fParseErrors.Add(iPattern);
                    }
                }
            }
        }

        /// <summary>The read do pattern.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="attachTo">The attach to.</param>
        private void ReadDoPattern(System.Xml.XmlReader reader, Neuron attachTo)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName);
            var iNew = ReadExpression(reader);
            var iText = NeuronFactory.GetText(iNew);
            Brain.Current.Add(iText);
            var iData = BrainData.Current.NeuronInfo[iText];
            iData.Title = iName;
            iData.DescriptionText = iDesc;

            reader.ReadEndElement();
            if (iText != null)
            {
                Link.Create(attachTo, iText, (ulong)PredefinedNeurons.DoPatterns);
                var iDo = new DoPattern(iText);
                iDo.Parse();
                if (iDo.HasError)
                {
                    fParseErrors.Add(iDo);
                }
            }
        }

        /// <summary>we always generate a name for an output when it is used as an
        ///     'AnswerFor' value.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        private static TextNeuron ReadOutput(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName);
            var iNew = ReadOutputExpression(reader);
            var iText = NeuronFactory.GetText(iNew);
            Brain.Current.Add(iText);
            var iData = BrainData.Current.NeuronInfo[iText];
            iData.Title = iName;
            iData.DescriptionText = iDesc;

            reader.ReadEndElement();
            return iText;
        }

        /// <summary>The read thes.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadThes(System.Xml.XmlReader reader)
        {
            var iStreamer = new ThesaurusXmlStreamer(fReadAssets, fParseErrors, fReadTopics, fStillToResolve);
            iStreamer.ReadXml(reader);
        }

        /// <summary>The read text assets.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTextAssets(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "TextAssets" && reader.EOF == false)
            {
                ReadTextAsset(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read text asset.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTextAsset(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iText = TextNeuron.GetFor(reader.Name);
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iAsset = ReadAssetEditor(reader);
            iText.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Asset, iAsset.Assets.Cluster);
            reader.ReadEndElement();
        }

        /// <summary>The read bot asset.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadBotAsset(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iAsset = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.BotID] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iAsset != null);
            ReadAssetEditor(reader, iAsset);
            reader.ReadEndElement();
        }

        /// <summary>The read user assets.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadUserAssets(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iAsset = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.CreatorID] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iAsset != null);
            ReadAssetEditor(reader, iAsset);
            reader.ReadEndElement();
        }

        /// <summary>Reads the editors.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadEditors(System.Xml.XmlReader reader, System.Collections.Generic.IList<EditorBase> list)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Editors")
            {
                EditorBase iNew;
                if (reader.Name == "Folder")
                {
                    iNew = ReadFolder(reader);
                }
                else if (reader.Name == "BrainFile" || reader.Name == "Topic")
                {
                    // there once was a time when topics were called brainfiles.
                    iNew = ReadPatternEditor(reader);
                }
                else if (reader.Name == "Asset")
                {
                    iNew = ReadAssetEditor(reader);
                }
                else if (reader.Name == "Query")
                {
                    iNew = ReadQuery(reader);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        string.Format("Unknown editor type: {0}, can't import from xml.", reader.Name));
                }

                if (iNew != null)
                {
                    list.Add(iNew);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>reads a query editor from the file.</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="EditorBase"/>.</returns>
        private EditorBase ReadQuery(System.Xml.XmlReader reader)
        {
            var iStreamer = new QueryXmlStreamer();
            return iStreamer.ReadQuery(reader);
        }

        /// <summary>The read pattern editor.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="EditorBase"/>.</returns>
        private EditorBase ReadPatternEditor(System.Xml.XmlReader reader)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.TextPatternTopic;
            var iEditor = new TextPatternEditor(iCluster);
            try
            {
                iEditor.IsOpen = true;
                try
                {
                    iEditor.Items.IsActive = false; // don't need ui updating during loads
                    var iStreamer = new TopicXmlStreamer(fParseErrors, fStillToResolve);
                    iStreamer.ReadTopic(reader, iEditor);
                    if (string.IsNullOrEmpty(iStreamer.ID) == false)
                    {
                        // so we can find it again when reading the thes.
                        fReadTopics.Add(iStreamer.ID, iEditor);
                    }
                }
                finally
                {
                    iEditor.IsOpen = false;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Project.Import", e.Message);
            }

            return iEditor;
        }

        /// <summary>Reads the folder.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="EditorBase"/>.</returns>
        private EditorBase ReadFolder(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            var iRes = new EditorFolder();
            string iDesc;
            string iName;
            ReadInfo(reader, out iDesc, out iName);
            iRes.Name = iName;
            iRes.DescriptionText = iDesc;
            ReadEditors(reader, iRes.Items);
            reader.ReadEndElement();
            return iRes;
        }

        /// <summary>Reads a asset editor. If the data needs to be read into a specific
        ///     cluster, this can be specified (for user + bot)</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The <see cref="AssetEditor"/>.</returns>
        private AssetEditor ReadAssetEditor(System.Xml.XmlReader reader, NeuronCluster cluster)
        {
            System.Diagnostics.Debug.Assert(cluster != null);
            var iAsset = new AssetEditor(cluster);
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

            return iAsset;
        }

        /// <summary>The read asset editor.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="ObjectEditor"/>.</returns>
        private ObjectEditor ReadAssetEditor(System.Xml.XmlReader reader)
        {
            var iStreamer = new AssetXmlStreamer(fReadAssets, fParseErrors, fStillToResolve);
            return iStreamer.ReadAsset(reader);
        }

        #endregion

        #region writing

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            try
            {
                WriteGlobals(writer);

                WriteOnlineData(writer);

                WriteUserAssets(writer);
                WriteBotAssets(writer);
                WriteTextAssets(writer);
                WriteThesaurus(writer);
                WriteEditors(writer, BrainData.Current.Editors);
            }
            finally
            {
                AlreadyRendered = null;
                AlreadyRead = null;
            }
        }

        /// <summary>writes the online part of the project to the xml file.</summary>
        /// <param name="writer"></param>
        private void WriteOnlineData(System.Xml.XmlWriter writer)
        {
            var iOnline = BrainData.Current.DesignerData.OnlineInfo;
            if (iOnline != null)
            {
                iOnline.WriteToXml(writer);
            }
        }

        /// <summary>The write globals.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteGlobals(System.Xml.XmlWriter writer)
        {
            WriteReflectionData(writer);
            WriteDoOnStartup(writer);
            WriteOpeningStatements(writer);
            WriteFallbackValues(writer);
            WriteContext(writer);
            WriteDoStatements(writer);
            WriteRepeatResponses(writer);

            var iProps = new ChatbotProperties(); // creating it this way automatically creates a ref to the project.
            writer.WriteStartElement("ModuleProperties");
            CheckWriteLegacy(writer, iProps);
            WriteModuleProps(writer, iProps);
            writer.WriteEndElement();
            iProps = null;
        }

        /// <summary>The write module props.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="props">The props.</param>
        private void WriteModuleProps(System.Xml.XmlWriter writer, ChatbotProperties props)
        {
            foreach (var i in props.ModuleProps)
            {
                i.WriteXmlValue(writer);
            }
        }

        /// <summary>checks if there are still legacy chatbot properties defined in the
        ///     project and if so, writes them out.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="iProps"></param>
        private void CheckWriteLegacy(System.Xml.XmlWriter writer, ChatbotProperties iProps)
        {
            if (BrainData.Current.DesignerData.Chatbotdata.UseOutputVarSwitchID != Neuron.EmptyId)
            {
                XmlStore.WriteElement(writer, "UseOutputVar", iProps.UseOutputVar);
            }

            if (BrainData.Current.DesignerData.Chatbotdata.SynonymResolveSwitchID != Neuron.EmptyId)
            {
                XmlStore.WriteElement(writer, "AutoResolveSyns", iProps.AutoResolveSyns);
            }

            if (BrainData.Current.DesignerData.Chatbotdata.UseSTTWeightID != Neuron.EmptyId)
            {
                XmlStore.WriteElement(writer, "UseSTTWeight", iProps.UseSTTWeight);
            }

            if (BrainData.Current.DesignerData.Chatbotdata.SingleTopPatternResultID != Neuron.EmptyId)
            {
                XmlStore.WriteElement(writer, "SingleTopPatternResult", iProps.SingleTopPatternResult);
            }
        }

        /// <summary>The write reflection data.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteReflectionData(System.Xml.XmlWriter writer)
        {
            var iChannel =
                (from i in BrainData.Current.CommChannels where i is ReflectionChannel select (ReflectionChannel)i)
                    .FirstOrDefault();
            if (iChannel != null)
            {
                var iSin = iChannel.Sin as ReflectionSin;
                var iValue = GetLoadedEntryPointsForExport(iSin);
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(iValue.GetType());
                valueSerializer.Serialize(writer, iValue);
            }
        }

        /// <summary>The get loaded entry points for export.</summary>
        /// <param name="sin">The sin.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private static System.Collections.Generic.List<ExportableReflectionSinEntryPoint> GetLoadedEntryPointsForExport(
            ReflectionSin sin)
        {
            var iList = new System.Collections.Generic.List<ExportableReflectionSinEntryPoint>();
            foreach (var i in sin.EntryPoints)
            {
                var iParamTypeNames =
                    (from p in i.Value.GetParameters() select p.ParameterType.AssemblyQualifiedName).ToList();
                var iNew = new ExportableReflectionSinEntryPoint
                               {
                                   AssemblyName =
                                       i.Value.DeclaringType.Assembly.Location, 
                                   MethodName = i.Value.Name, 
                                   ParameterTypes = iParamTypeNames, 
                                   TypeName = i.Value.ReflectedType.FullName, 
                                   MappedName =
                                       BrainData.Current.NeuronInfo[i.Key]
                                       .DisplayTitle
                               };
                iList.Add(iNew);
            }

            return iList;
        }

        /// <summary>The write do on startup.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteDoOnStartup(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("DoOnStartup");
            var iStarts = Brain.Current[(ulong)PredefinedNeurons.DoOnStartup] as NeuronCluster;
            var iTowrite = iStarts.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iTowrite != null)
            {
                WriteDo(writer, iTowrite);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write do.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="toWrite">The to write.</param>
        private void WriteDo(System.Xml.XmlWriter writer, TextNeuron toWrite)
        {
            WriteInfo(writer, BrainData.Current.NeuronInfo[toWrite]);
            if (string.IsNullOrEmpty(toWrite.Text) == false)
            {
                WriteExpression(writer, toWrite.Text);
            }
        }

        /// <summary>The write repeat responses.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteRepeatResponses(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Repetitions");
            var iReps = Brain.Current[(ulong)PredefinedNeurons.RepeatOutputPatterns] as NeuronCluster;
            System.Collections.Generic.List<NeuronCluster> iToRelease;
            using (var iChildren = iReps.Children) iToRelease = iChildren.ConvertTo<NeuronCluster>();
            foreach (var i in iToRelease)
            {
                WriteRepetition(writer, i);
            }

            writer.WriteEndElement();
            Factories.Default.CLists.Recycle(iToRelease);
        }

        /// <summary>The write repetition.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="i">The i.</param>
        private void WriteRepetition(System.Xml.XmlWriter writer, NeuronCluster i)
        {
            writer.WriteStartElement("Repetition");
            var iCond = i.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
            if (iCond != null)
            {
                WriteConditionPattern(writer, iCond);
            }

            WriteClusterContentAsOutputPatterns(writer, i);
            var iTowrite = i.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iTowrite != null)
            {
                writer.WriteStartElement("DoPatterns");
                WriteDo(writer, iTowrite);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>The write condition pattern.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cond">The cond.</param>
        private void WriteConditionPattern(System.Xml.XmlWriter writer, TextNeuron cond)
        {
            writer.WriteStartElement("Condition");

            WriteInfo(writer, BrainData.Current.NeuronInfo[cond]);
            if (string.IsNullOrEmpty(cond.Text) == false)
            {
                WriteExpression(writer, cond.Text);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write do statements.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteDoStatements(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("DoAfterStatement");
            var iDoAfter = Brain.Current[(ulong)PredefinedNeurons.DoAfterStatement] as NeuronCluster;
            var iTowrite = iDoAfter.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iTowrite != null)
            {
                WriteDo(writer, iTowrite);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write fallback values.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteFallbackValues(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Fallbacks");
            var iFallbacks = Brain.Current[(ulong)PredefinedNeurons.ResponsesForEmptyParse] as NeuronCluster;
            WriteClusterContentAsOutputPatterns(writer, iFallbacks);
            writer.WriteEndElement();
        }

        /// <summary>The write context.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteContext(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Context");

            var iBot = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.BotID];
            if (iBot != null)
            {
                var iContext = iBot.FindFirstOut((ulong)PredefinedNeurons.Context) as NeuronCluster;
                if (iContext != null)
                {
                    WriteClusterContentAsOutputPatterns(writer, iContext);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes the cluster content as text neurons -&gt; expressions.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="list">The i fallbacks.</param>
        private void WriteClusterContentAsOutputPatterns(System.Xml.XmlWriter writer, NeuronCluster list)
        {
            using (var iChildren = list.Children)
                foreach (var i in iChildren.ConvertTo<TextNeuron>())
                {
                    WriteOutputPattern(writer, i);
                }
        }

        /// <summary>we always generate a name for an <paramref name="output"/> when it is
        ///     used as an 'AnswerFor' value.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="output">The output.</param>
        private void WriteOutputPattern(System.Xml.XmlWriter writer, TextNeuron output)
        {
            writer.WriteStartElement("Output");

            WriteInfo(writer, BrainData.Current.NeuronInfo[output]);
            if (string.IsNullOrEmpty(output.Text) == false)
            {
                WriteOutputExpression(writer, output.Text);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write opening statements.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteOpeningStatements(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("OpeningStatements");
            var iStarts = Brain.Current[(ulong)PredefinedNeurons.ConversationStarts] as NeuronCluster;
            WriteClusterContentAsOutputPatterns(writer, iStarts);
            writer.WriteEndElement();
        }

        /// <summary>The write bot assets.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteBotAssets(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Bot");
            var iAsset = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.BotID] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iAsset != null);
            var iEditor = new AssetEditor(iAsset);
            WriteAssetEditor(writer, iEditor);
            writer.WriteEndElement();
        }

        /// <summary>The write text assets.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteTextAssets(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("TextAssets");
            foreach (var iId in TextSin.Words.Values)
            {
                var iText = Brain.Current[iId] as TextNeuron;
                if (iText != null)
                {
                    var iAsset = iText.FindFirstOut((ulong)PredefinedNeurons.Asset) as NeuronCluster;
                    if (iAsset != null)
                    {
                        var iEditor = new AssetEditor(iAsset);
                        writer.WriteStartElement(iText.Text);
                        WriteAssetEditor(writer, iEditor);
                        writer.WriteEndElement();
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes the thesaurus.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteThesaurus(System.Xml.XmlWriter writer)
        {
            var iStreamer = new ThesaurusXmlStreamer(fWrittenAssets, fWrittenPatterns);
            writer.WriteStartElement("thesaurus");
            iStreamer.WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>The write user assets.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteUserAssets(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("User");
            var iAsset = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.CreatorID] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iAsset != null);
            var iEditor = new AssetEditor(iAsset);
            WriteAssetEditor(writer, iEditor);
            writer.WriteEndElement();
        }

        /// <summary>Writes the editors.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="children">The children.</param>
        private void WriteEditors(System.Xml.XmlWriter writer, EditorCollection children)
        {
            writer.WriteStartElement("Editors");
            foreach (var i in children)
            {
                if (i is EditorFolder)
                {
                    WriteFolder(writer, (EditorFolder)i);
                }
                else if (i is TextPatternEditor)
                {
                    WritePatternEditor(writer, (TextPatternEditor)i);
                }
                else if (i is ObjectEditor)
                {
                    if (!(i is AssetEditor) && fWrittenAssets.ContainsKey(((ObjectEditor)i).Item) == false)
                    {
                        throw new System.InvalidOperationException(
                            "Attached assets need to be rendered before the project tree.");
                    }

                    WriteAssetEditor(writer, (ObjectEditor)i);
                }
                else if (i is QueryEditor)
                {
                    WriteQuery(writer, (QueryEditor)i);
                }
                else
                {
                    LogService.Log.LogWarning(
                        "Project exporter", 
                        string.Format("Unknown editor type: {0}, can't export to xml.", i.GetType()));
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>writes a query to the xml file.</summary>
        /// <param name="writer"></param>
        /// <param name="editor">The editor.</param>
        private void WriteQuery(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true;
            try
            {
                var iStreamer = new QueryXmlStreamer();
                iStreamer.WriteQuery(writer, editor);
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }
        }

        /// <summary>Writes the asset editor.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        private void WriteAssetEditor(System.Xml.XmlWriter writer, ObjectEditor editor)
        {
            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true;
            try
            {
                var iStreamer = new AssetXmlStreamer(fWrittenAssets);
                iStreamer.WriteAssetEditor(writer, editor);
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }
        }

        /// <summary>Writes the pattern editor.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        private void WritePatternEditor(System.Xml.XmlWriter writer, TextPatternEditor editor)
        {
            string iId = null;
            if (editor is ObjectTextPatternEditor)
            {
                iId = System.Guid.NewGuid().ToString();
                fWrittenPatterns.Add(editor.ItemID, iId);
            }

            var iStreamer = new TopicXmlStreamer(iId);
            iStreamer.WriteTopic(writer, editor);
        }

        /// <summary>Writes the folder.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="folder">The folder.</param>
        private void WriteFolder(System.Xml.XmlWriter writer, EditorFolder folder)
        {
            writer.WriteStartElement("Folder");
            WriteInfo(writer, folder.DescriptionText, folder.Name);
            WriteEditors(writer, folder.Items);
            writer.WriteEndElement();
        }

        #endregion

        #endregion
    }
}