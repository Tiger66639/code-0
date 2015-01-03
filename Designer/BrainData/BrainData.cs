// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainData.cs" company="">
//   
// </copyright>
// <summary>
//   The entry point for the designer regarding data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     The entry point for the designer regarding data.
    /// </summary>
    public class BrainData : Data.ObservableObject, IEditorsOwner
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrainData" /> class.
        /// </summary>
        public BrainData()
        {
            fCodeEditors.CollectionChanged += fCodeEditors_CollectionChanged;
            CollectionChanging += BrainData_CollectionChanging;
            Parsers.TopicsDictionary.NameGetter = GetNameFor;

                // mapping for the topicsDict, so it can find the names for the rules.
        }

        #endregion

        /// <summary>
        ///     Clears all the data from this instance.
        /// </summary>
        public void Clear()
        {
            if (DesignerData != null)
            {
                // if this is already null, the clear has already happened, or something went wrong during the load a project, don't try to clear anything else, cause there is nothing.
                var iComms = CommChannels.ToArray();

                    // get a local copy of all the commchannels before the designerData is reset, cause we need to let the commchannels know that the project is unloaded, which is done by an absense of designerdata.
                var iWordnet = DesignerData.WordnetChannel;
                NeuronInfo.Dispose();

                    // to close all the files, needs to be done before DesignerData is reset, otherwise we can't reach it.
                OpenDocuments.Clear();

                    // needs to be done before DesignerData reset cause it's stored in the designerData.
                DesignerData = null;
                fTestCases = null;

                    // important: this must be done after setting the designerData to null, cause this reset otherwise causes the list of testcases to be reloaded after it was set to null but before it can actually load the data (so it would always have an empty list, which is no good).
                foreach (var i in iComms)
                {
                    i.UpdateOpenDocuments();

                        // designerdata has been reset, so that these  commchannels can unload anything that is required .
                }

                if (iWordnet != null)
                {
                    iWordnet.UpdateOpenDocuments();
                }

                fCodeEditors.Clear();
                fClusterLists = null; // we have to reset this, otherwise the neurons wont get reloaded.
                fPosTypes = null; // same as for fClusterLists.
                InstructionToolBoxItems = null;

                    // also need to reset the instructions toolbox so that all items get reloaded when a new project is loaded.

                // ChatbotProps = null;                                                                         //don't need to reset, this is done when all the open documents are unloaded.
                Parsers.TopicsDictionary.Clear();
                AttachedTopics = null;
                if (System.IO.Directory.Exists(Properties.Settings.Default.CustomSpellingDictsPath))
                {
                    System.IO.File.Create(
                        System.IO.Path.Combine(
                            Properties.Settings.Default.CustomSpellingDictsPath, 
                            Properties.Resources.IgnoreAllDict));

                        // make certain that the IgnoreAllDict list is empty each time we restart the application
                }
            }
        }

        /// <summary>
        ///     Creates a new <see cref="BrainData.Current" />
        /// </summary>
        /// <remarks>
        ///     Items created:
        ///     -all default toolbox items
        ///     -the wordnetSin channel.
        /// </remarks>
        public static void New()
        {
            var iData = new DesignerDataFile(); // load the default (empty) data.
            iData.Name = "New project";
            iData.OpenDocuments = new OpenDocsCollection();
            iData.CommChannels = new CommChannelCollection();
            iData.BreakPoints = new BreakPointCollection();
            iData.ToolBoxItems = new ToolBoxItemCollection();
            iData.PlaySpeed = new System.TimeSpan(0, 0, 0, 2, 0);
            iData.DefaultMeaningIds = new SmallIDCollection();
            iData.AssetPronounIds = new AssetPronounsMap();
            iData.ModulePropIds = new SmallIDCollection();
            iData.Chatbotdata = new ChatbotData();
            iData.Chatbotdata.ParserMap = new ParserMap();
            iData.Chatbotdata.DoFunctionMap = new FunctionMap();
            iData.NeuronInfo = new NeuronDataDictionary();
            iData.Instructions = new NeuronCollection<Instruction>();
            iData.Operators = new NeuronCollection<Neuron>();
            iData.Watches = new WatchCollection();
            iData.Editors = new EditorCollection();
            iData.SplitPaths = new Data.ObservedCollection<SplitPath>();
            iData.Thesaurus = new Thesaurus();
            iData.Overlays = new System.Collections.Generic.List<OverlayText>();
            iData.TemplateVersion = ProjectManager.PROJECT_TEMPLATE_VER;

                // so we don't get a bogus upgrade question for new empty project?
            Current.DesignerData = iData;

            Current.NeuronInfo.LoadDefaults();

                // we load the defaults before creating the new default toolboxes, so that we have a start, but update this correctly.
            Current.LoadDefaultToolBoxItems();

            Current.AttachedTopics = new AttachedTopicsCollection();

            /*
          * This is moved to a seperate command. We don't want to load the wordnet channel from the beginning. It is curently only used for as a 'helper' resource.
         WordNetChannel iWordNet = new WordNetChannel();
         iWordNet.NeuronID = (ulong)PredefinedNeurons.WordNetSin;
         iWordNet.NeuronInfo.DisplayTitle = "WordNet";
         Current.CommChannels.Add(iWordNet);
          */
            Current.InstructionToolBoxItems = null;

                // we need to reset this value so that we can recalculate the instructions, if we don't do this, the list remains empty, since it gets loaded when the datafile is assigned to the brainData, but there are no instructions yet loaded at that time (the designer data is required for this)
            Current.LoadInstructionsView();
        }

        /// <summary>
        ///     Loads all the default toolbox items, which are the <see cref="TypeToolBoxItem" />s.
        /// </summary>
        private void LoadDefaultToolBoxItems()
        {
            LoadToolBoxItemFor(typeof(Neuron), "General", "Neuron");
            LoadToolBoxItemFor(typeof(NeuronCluster), "General", "Cluster");
            LoadToolBoxItemFor(typeof(TextNeuron), "General", "Text neuron");
            LoadToolBoxItemFor(typeof(TextSin), "General", "Text Sin");
            LoadToolBoxItemFor(typeof(IntNeuron), "General", "Int neuron");
            LoadToolBoxItemFor(typeof(DoubleNeuron), "General", "Double neuron");

            // LoadToolBoxItemFor(typeof(KnoledgeNeuron), "General", "Knoledge neuron");
            LoadToolBoxItemFor(typeof(Assignment), "Code", "Assignment");
            LoadToolBoxItemFor(typeof(Statement), "Code", "Statement");
            LoadToolBoxItemFor(typeof(ResultStatement), "Code", "Result statement");
            LoadToolBoxItemFor(typeof(ExpressionsBlock), "Code", "Code block");
            LoadToolBoxItemFor(typeof(LockExpression), "Code", "Lock");
            LoadToolBoxItemFor(typeof(ConditionalStatement), "Code", "Condional statement");
            LoadToolBoxItemFor(typeof(ConditionalExpression), "Code", "condional part");
            LoadToolBoxItemFor(typeof(BoolExpression), "Code", "bool expression");
            LoadToolBoxItemFor(typeof(Variable), "Code", "Variable");
            LoadToolBoxItemFor(typeof(Global), "Code", "Global");
            LoadToolBoxItemFor(typeof(Local), "Code", "Local");
            LoadToolBoxItemFor(typeof(ByRefExpression), "Code", "ByRef");

            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.Empty, "Global values", "Empty");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.BeginTextBlock, "Global values", "BeginTextBlock");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.EndTextBlock, "Global values", "EndTextBlock");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.CurrentSin, "Global values", "CurrentSin");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.CurrentFrom, "Global values", "CurrentFrom");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.CurrentTo, "Global values", "CurrentTo");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.CurrentMeaning, "Global values", "CurrentMeaning");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.CurrentInfo, "Global values", "CurrentInfo");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.Time, "Global values", "Time");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.ReturnValue, "Global values", "Return value");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.True, "Global values", "True");
            LoadNeuronToolBoxItemFor((ulong)PredefinedNeurons.False, "Global values", "False");

            LoadDefaultInstructions();

            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Equal, "Operators", "==");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Smaller, "Operators", "<");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Bigger, "Operators", ">");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.SmallerOrEqual, "Operators", "<=");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.BiggerOrEqual, "Operators", ">=");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Different, "Operators", "!=");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Contains, "Operators", "Contains");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.NotContains, "Operators", "!Contains");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.And, "Operators", "&&");
            LoadOperatorToolBoxItemFor((ulong)PredefinedNeurons.Or, "Operators", "||");
        }

        /// <summary>The load default instructions.</summary>
        private void LoadDefaultInstructions()
        {
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddChildInstruction, "Add", "Add Child");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddLinkInstruction, "Add", "Add Link");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddInfoInstruction, "Add", "Add Info");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AdditionInstruction, "Arithmetic", "+");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MinusInstruction, "Arithmetic", "-");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MultiplyInstruction, "Arithmetic", "*");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DivideInstruction, "Arithmetic", "/");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ModulusInstruction, "Arithmetic", "%");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.CompleteSequenceInstruction, 
                "Arithmetic", 
                "Complete sequence");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AvgInstruction, "Arithmetic", "Avg");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MaxInstruction, "Arithmetic", "Max");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MinInstruction, "Arithmetic", "Min");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StDevInstruction, "Arithmetic", "StDev");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DecrementInstruction, "Arithmetic", "Decrement");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IncrementInstruction, "Arithmetic", "Increment");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddStoreIntInstruction, "Arithmetic", "AddStoreInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.AddStoreDoubleInstruction, 
                "Arithmetic", 
                "AddStoreDouble");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MultiplyStoreIntInstruction, 
                "Arithmetic", 
                "MultiplyStoreInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MultiplyStoreDoubleIntruction, 
                "Arithmetic", 
                "MultiplyStoreDouble");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MinusStoreIntInstruction, 
                "Arithmetic", 
                "MinusStoreInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MinusStoreDoubleInstruction, 
                "Arithmetic", 
                "MinusStoreDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DivStoreIntInstruction, "Arithmetic", "DivStoreInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.DivStoreDoubleInstruction, 
                "Arithmetic", 
                "DivStoreDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ModStoreIntInstruction, "Arithmetic", "ModStoreInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ModStoreDoubleInstruction, 
                "Arithmetic", 
                "ModStoreDouble");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StoreIntInstruction, "Arithmetic", "StoreInt");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StoreDoubleInstruction, "Arithmetic", "StoreDouble");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.AddAssignIntInstruction, 
                "Arithmetic", 
                "AddAssignInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.AddAssignDoubleInstruction, 
                "Arithmetic", 
                "AddAssignDouble");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MinusAssignIntInstruction, 
                "Arithmetic", 
                "MinusAssignInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MinusAssignDoubleInstruction, 
                "Arithmetic", 
                "MinusAssignDouble");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ModIntInstruction, "Arithmetic", "ModInt");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ModDoubleInstruction, "Arithmetic", "ModDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DivIntInstruction, "Arithmetic", "DivInt");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DivDoubleInstruction, "Arithmetic", "DivDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MultiplyIntInstruction, "Arithmetic", "MultiplyInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MultiplyDoubleInstruction, 
                "Arithmetic", 
                "MultiplyDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MinusIntInstruction, "Arithmetic", "MinusInt");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MinusDoubleInstruction, "Arithmetic", "MinusDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddIntInstruction, "Arithmetic", "AddInt");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddDoubleInstruction, "Arithmetic", "AddDouble");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeLinkFrom, "Change", "Change link From");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeLinkTo, "Change", "Change link To");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeLinkMeaning, "Change", "Change link meaning");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeInfoInstruction, "Change", "Change info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeChildInstruction, "Change", "Change child");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChangeParentInstruction, "Change", "Change parent");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClearChildrenInstruction, "Clear", "Clear children");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClearInfoInstruction, "Clear", "Clear info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClearLinksInInstruction, "Clear", "Clear links in");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClearLinksOutInstruction, "Clear", "Clear links out");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SToCcInstruction, "Convert", "SToCc");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SToCiInstruction, "Convert", "SToCi");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SToIInstruction, "Convert", "SToI");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SToDInstruction, "Convert", "SToD");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CcToSInstruction, "Convert", "CcToS");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CiToSInstruction, "Convert", "CiToS");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IToSInstruction, "Convert", "IToS");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DToSInstruction, "Convert", "DToS");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IToDInstruction, "Convert", "IToD");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DToIInstruction, "Convert", "DToI");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IToDTInstruction, "Convert", "IToDateTime");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ToSInstruction, "Convert", "ToS");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StoLiInstruction, "Convert", "StoLi");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetChildAtInstruction, "Get at", "Get child at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetClusterAtInstruction, "Get at", "Get cluster at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetInfoAtInstruction, "Get at", "Get info at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetInAtInstruction, "Get at", "Get in at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetOutAtInstruction, "Get at", "Get out at");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetFirstChildInstruction, 
                "Get first", 
                "Get first child");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetFirstClusterInstruction, 
                "Get first", 
                "Get first cluster");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetFirstInfoInstruction, 
                "Get first", 
                "Get first info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetFirstInInstruction, "Get first", "Get first in");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetFirstOutInstruction, "Get first", "Get first out");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetLastChildInstruction, 
                "Get last", 
                "Get last child");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetLastClusterInstruction, 
                "Get last", 
                "Get last cluster");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetLastInfoInstruction, "Get last", "Get last info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetLastInInstruction, "Get last", "Get last in");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetLastOutInstruction, "Get last", "Get last out");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetNextChildInstruction, 
                "Get next", 
                "Get next child");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetNextClusterInstruction, 
                "Get next", 
                "Get next cluster");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetNextInfoInstruction, "Get next", "Get next info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetNextInInstruction, "Get next", "Get next in");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetNextOutInstruction, "Get next", "Get next out");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetPrevChildInstruction, 
                "Get prev", 
                "Get prev child");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetPrevClusterInstruction, 
                "Get prev", 
                "Get prev cluster");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetPrevInfoInstruction, "Get prev", "Get prev info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetPrevInInstruction, "Get prev", "Get prev in");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetPrevOutInstruction, "Get prev", "Get prev out");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ChildCountInstruction, "Count", "Child count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClusterCountInstruction, "Count", "Cluster count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.LinkInCountInstruction, "Count", "Link in count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.LinkOutCountInstruction, "Count", "Link out count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InfoCountInstruction, "Count", "Info count");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.LinkExistsInstruction, "Contains", "Link exists");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IsClusteredByInstruction, 
                "Contains", 
                "Is clustered by all");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IsClusteredByAnyInstruction, 
                "Contains", 
                "Is clustered by");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ContainsAllChildrenInstruction, 
                "Contains", 
                "Contains all children");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ContainsChildrenInstruction, 
                "Contains", 
                "ContainsChildren");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ContainsLinksInInstruction, 
                "Contains", 
                "Contains links in");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ContainsLinksOutInstruction, 
                "Contains", 
                "Contains links out");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SplitStringInstruction, "strings", "Split string");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.EndsWithInstruction, "strings", "EndsWith");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.LengthInstruction, "strings", "Length");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SubStringInstruction, "strings", "SubString");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetCharAtInstruction, "strings", "GetCharAr");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SetStringInstruction, "strings", "SetString");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetChildrenInstruction, "Get", "Get children");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetChildrenRangeInstruction, 
                "Get", 
                "Get children range");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetChildrenOfTypeInstruction, 
                "Get", 
                "Get children of Type X");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetChildrenFilteredInstruction, 
                "Get", 
                "Get children filtered");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetClustersInstruction, "Get", "Get clusters");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetClustersWithMeaningInstruction, 
                "Get", 
                "Get clusters with meaning X");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetClustersFilteredInstruction, 
                "Get", 
                "Get clusters filtered");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetClusterMeaningInstruction, 
                "Get", 
                "Get cluster meaning");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetLinkMeaningInstruction, "Get", "Get link meaning");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetInfoInstruction, "Get", "Get info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetOutgoingInstruction, "Get", "Get outgoing");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetIncommingInstruction, "Get", "Get incomming");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetInfoFilteredInstruction, 
                "Get", 
                "Get info filtered");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetoutFilteredInstruction, "Get", "GetOutFiltered");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetInFilteredInstruction, "Get", "GetInFiltered");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetAllIncommingInstruction, 
                "Get", 
                "Get all incomming");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetAllOutgoingInstruction, "Get", "Get all outgoing");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetCommonParentsFilteredInstruction, 
                "Get common", 
                "Get common parents filtered");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetCommonParentsInstruction, 
                "Get common", 
                "Get common parents");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetCommonParentsWithMeaningInstruction, 
                "Get common", 
                "Get common parents with meaning");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetCommonInInstruction, 
                "Get common", 
                "Get Common In");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetCommonOutInstruction, 
                "Get common", 
                "Get Common Out");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetRandomInfoInstruction, 
                "Get Random", 
                "Get random info");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetRandomClusterInstruction, 
                "Get Random", 
                "Get random clustered-by");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetRandomOutInstruction, 
                "Get Random", 
                "Get random outgoing");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetRandomInInstruction, 
                "Get Random", 
                "Get random incomming");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetRandomChildInstruction, 
                "Get Random", 
                "Get random child");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IndexOfChildInstruction, "Index of", "IndexOf child");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IndexOfClusterInstruction, 
                "Index of", 
                "IndexOf cluster");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IndexOfInfoInstruction, "Index of", "IndexOf info");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IndexOfLinkInstruction, "Index of", "IndexOf link");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InsertChildInstruction, "Insert", "Insert Child");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InsertLinkInstruction, "Insert", "Insert Link");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InsertInfoInstruction, "Insert", "Insert info");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.CopyChildrenInstruction, 
                "Copy/move", 
                "Copy children");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CopyInfoInstruction, "Copy/move", "Copy info");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.MoveChildrenInstruction, 
                "Copy/move", 
                "Move children");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MoveInfoInstruction, "Copy/move", "Move info");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.NewInstruction, "Neurons", "New");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DeleteInstruction, "Neurons", "Delete");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.FreezeInstruction, "Neurons", "Freeze");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.UnFreezeInstruction, "Neurons", "UnFreeze");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DuplicateInstruction, "Neurons", "Duplicate");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.TypeOfInstruction, "Neurons", "TypeOf");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.HasReferencesInstruction, "Neurons", "HasReferences");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.MakeClusterInstruction, "Neurons", "Make cluster");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DeleteFrozenInstruction, "Neurons", "DeleteFrozen");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SplitInstruction, "Processor split", "Split");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.SplitAccumInstruction, 
                "Processor split", 
                "SplitAccum");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.SplitWeightedInstruction, 
                "Processor split", 
                "SplitWeighted");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.SplitFixedInstruction, 
                "Processor split", 
                "SplitFixed");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetSplitResultsInstruction, 
                "Processor split", 
                "Get Split results");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ClearSplitResultsInstruction, 
                "Processor split", 
                "Clear split results");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveSplitResultInstruction, 
                "Processor split", 
                "Remove split result");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.AddSplitResultInstruction, 
                "Processor split", 
                "Add split result");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetSplitCountInstruction, 
                "Processor split", 
                "Get split count");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PeekInstruction, "Processor stack", "Peek");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PopInstruction, "Processor stack", "Pop");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PushInstruction, "Processor stack", "Push");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.StackCountInstruction, 
                "Processor stack", 
                "Stack count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PushValueInstruction, "Processor stack", "PushValue");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PopValueInstruction, "Processor stack", "PopValue");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ParamStackCountInstruction, 
                "Processor stack", 
                "ParamStackcount");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetWeightInstruction, 
                "Processor weight", 
                "GetWeight");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetWeightOfInstruction, 
                "Processor weight", 
                "GetWeightOf");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetMaxWeightInstruction, 
                "Processor weight", 
                "GetMaxWeight");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IncreaseWeightInstruction, 
                "Processor weight", 
                "IncreaseWeight");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IncreaseWeightOfInstruction, 
                "Processor weight", 
                "IncreaseWeightOf");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.DecreaseWeightInstruction, 
                "Processor weight", 
                "DecreaseWeight");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.DecreaseWeightOfInstruction, 
                "Processor weight", 
                "DecreaseWeightOf");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ResetWeightInstruction, 
                "Processor weight", 
                "ResetWeight");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ApplyWeightInstruction, 
                "Processor weight", 
                "ApplyWeight");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ContinueInstruction, "Processor", "Continue");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.BreakInstruction, "Processor", "Break");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AwakeInstruction, "Processor", "Awake");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SuspendInstruction, "Processor", "Suspend");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ExecuteInstruction, "Processor", "Execute");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.OutputInstruction, "Processor", "Output");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SolveInstruction, "Processor", "Solve");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CallInstruction, "Processor", "Call");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CallSaveInstruction, "Processor", "CallSave");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.PerformResultInstruction, 
                "Processor", 
                "PerformResult");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.PerformInstruction, "Processor", "Perform");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.BlockedSolveInstruction, "Processor", "BlockedSolve");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.BlockedCallInstruction, "Processor", "BlockedCall");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.PassFrozenToCallerInstruction, 
                "Processor", 
                "PassFrozenToCaller");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ReturnValueInstruction, "Processor", "ReturnValue");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ReturnValueIfInstruction, 
                "Processor", 
                "ReturnValueIf");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ReturnInstruction, "Processor", "return");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ExitLinkInstruction, "Processor", "ExitLink");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ExitNeuronInstruction, "Processor", "Exit Neuron");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ExitSolveInstruction, "Processor", "ExitSolve");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.JmpIfInstruction, "Processor", "JmpIf");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.WarningInstruction, "Processor", "Warning");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ErrorInstruction, "Processor", "Error");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveChildInstruction, "Remove", "Remove child");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveLinkInstruction, "Remove", "Remove link");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveInfoInstruction, "Remove", "Remove info");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveLinksInInstruction, 
                "Remove", 
                "Remove links in");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveLinksOutInstruction, 
                "Remove", 
                "Remove links out");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveChildAtInstruction, 
                "Remove", 
                "Remove child at");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveLinkInAtInstruction, 
                "Remove", 
                "Remove link in at");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.RemoveLinkOutAtInstruction, 
                "Remove", 
                "Remove link out at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveInfoAtInstruction, "Remove", "Remove info at");

            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.SetClusterMeaningInstruction, 
                "Set", 
                "SetClusterMeaning");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SetFirstOutInstruction, "Set", "SetFirstOut");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SetChildAtInstruction, "Set", "SetChildAt");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.UnionInstruction, "Set operations", "Union");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IntersectInstruction, "Set operations", "Intersect");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IntersectVarInstruction, 
                "Set operations", 
                "IntersectVar");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.ComplementInstruction, 
                "Set operations", 
                "Complement");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InterleafInstruction, "Set operations", "Interleaf");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ReverseInstruction, "Set operations", "Reverse");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.DistinctInstruction, "Set operations", "Distinct");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.FilterInstruction, "Set operations", "Filter");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.CountInstruction, "var operations", "Count");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetFirstInstruction, "var operations", "First");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetLastInstruction, "var operations", "Get Last");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetAtInstruction, "var operations", "Get at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SetAtInstruction, "var operations", "Set at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveAtInstruction, "var operations", "Remove at");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetRangeInstruction, "var operations", "Get range");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.GetRandomInstruction, "var operations", "Get random");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ClearInstruction, "var operations", "Clear");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.PrepareLocalInstruction, 
                "var operations", 
                "PrepareLocal");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.IsInitializedInstruction, 
                "var operations", 
                "IsInitialized");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SubstractInstruction, "var operations", "Substract");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.AddInstruction, "var operations", "Add");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.RemoveInstruction, "var operations", "Remove");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.ReplaceInstruction, "var operations", "Replace");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InsertInstruction, "var operations", "Insert");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.SortInstruction, "var operations", "Sort");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.NotInstruction, "Unary", "Not");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InvertSignInstruction, "Unary", "InvertSign");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InvertDoubleInstruction, "Unary", "InvertSignDouble");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.InvertIntInstruction, "Unary", "InvertSignInt");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.InvertIntListInstruction, 
                "Unary", 
                "InvertSignIntList");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.InvertDoubleListInstruction, 
                "Unary", 
                "InvertSignDoubleList");

            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.IsTimerActiveInstruction, "Timer", "IsTimerActive");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.GetTimerIntervalInstruction, 
                "Timer", 
                "GetTimerInterval");
            LoadInstructionToolBoxItemFor(
                (ulong)PredefinedNeurons.SetTimerIntervalInstruction, 
                "Timer", 
                "SetTimerInterval");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StopTimerInstruction, "Timer", "StopTimer");
            LoadInstructionToolBoxItemFor((ulong)PredefinedNeurons.StartTimerInstruction, "Timer", "StartTimer");
        }

        /// <summary>The load neuron tool box item for.</summary>
        /// <param name="item">The item.</param>
        /// <param name="category">The category.</param>
        /// <param name="title">The title.</param>
        private void LoadNeuronToolBoxItemFor(ulong item, string category, string title)
        {
            var iNew = new NeuronToolBoxItem();
            iNew.Item = Brain.Current[item];
            iNew.NeuronInfo.Category = category;
            iNew.NeuronInfo.DisplayTitle = title;
            ToolBoxItems.Add(iNew);
        }

        /// <summary>The load operator tool box item for.</summary>
        /// <param name="item">The item.</param>
        /// <param name="category">The category.</param>
        /// <param name="title">The title.</param>
        private void LoadOperatorToolBoxItemFor(ulong item, string category, string title)
        {
            var iNew = new NeuronToolBoxItem();
            iNew.Item = Brain.Current[item];
            iNew.NeuronInfo.Category = category;
            iNew.NeuronInfo.DisplayTitle = title;
            ToolBoxItems.Add(iNew);
            Operators.Add(iNew.Item);
        }

        /// <summary>Loads the instruction tool box item for.</summary>
        /// <remarks>Don't actually create the toolbox item, this is created dynamically later on.</remarks>
        /// <param name="item">The item.</param>
        /// <param name="category">The category.</param>
        /// <param name="title">The title.</param>
        private void LoadInstructionToolBoxItemFor(ulong item, string category, string title)
        {
            try
            {
                var iData = NeuronInfo[item];
                iData.Category = category;
                iData.DisplayTitle = title;
                Instructions.Add(iData.Neuron as Instruction);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("BrainData.LoadInstructionToolBoxItemFor", e.ToString());
            }
        }

        /// <summary>Creates a typed toolbox item, sets everything up and adds it to the list of toolbox items.</summary>
        /// <param name="type">The type for which we create a toolbox item.</param>
        /// <param name="category">The category name to assign to the toolbox item.</param>
        /// <param name="title">The title.</param>
        private void LoadToolBoxItemFor(System.Type type, string category, string title)
        {
            var iNew = new TypeToolBoxItem();
            iNew.ItemType = type;
            iNew.DisplayTitle = title;
            iNew.Category = category;
            ToolBoxItems.Add(iNew);
        }

        /// <summary>
        ///     Reloads all the toolbox items so that the toolbox is back to the default state.
        /// </summary>
        public void ReloadToolboxItems()
        {
            if (DesignerData != null)
            {
                // we can only reset if there is any data.
                Instructions.Clear();
                Operators.Clear(); // operator list is also rebuild.
                ToolBoxItems.Clear();
                LoadDefaultToolBoxItems();
                SortInstructions(Instructions);
                InstructionToolBoxItems.Clear(); // don't need to completele delete, can simply rebuild the list.
                LoadInstructionsView();
            }
        }

        /// <summary>Shortcut to register ALL NeuronData objects directly to the root.</summary>
        /// <param name="item">The item.</param>
        internal void RegisterNeuronData(NeuronData item)
        {
            RegisterChild(item);
        }

        /// <summary>Loads the attached topics. has to be done after the neuronInfo is loaded, cause it needs access to this data.
        ///     The path has to be the project path. It will add the 'DataPath' itself.</summary>
        /// <param name="fPath">The f path.</param>
        internal void LoadAttachedTopics(string fPath)
        {
            var iAttachedTopics = new AttachedTopicsCollection();
            iAttachedTopics.ReadData(
                System.IO.Path.Combine(
                    fPath, 
                    NeuronDataDictionary.DATAPATH, 
                    Properties.Resources.AttachedTopicsFileName));

                // also need to read out the list of topics that are attached to other objects and which need to be registered to the list of topics.
            Current.AttachedTopics = iAttachedTopics;
        }

        #region fields

        /// <summary>The f current.</summary>
        private static BrainData fCurrent;

        /// <summary>The f designer data.</summary>
        private DesignerDataFile fDesignerData;

        /// <summary>The f last undo item.</summary>
        private object fLastUndoItem; // this is used to check if the data has changed since the last save.

        /// <summary>The f code editors.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<CodeEditor> fCodeEditors =
            new System.Collections.ObjectModel.ObservableCollection<CodeEditor>();

                                                                                         // don't use observedCollection, cause than an add/remove of a codeEditor would also be added to the undo/redo list, which we don't want.

        /// <summary>The f current editors list.</summary>
        private System.Collections.Generic.IList<EditorBase> fCurrentEditorsList;

        /// <summary>The f link lists.</summary>
        private System.Collections.Generic.List<Neuron> fLinkLists;

        /// <summary>The f cluster lists.</summary>
        private System.Collections.Generic.List<Neuron> fClusterLists;

        /// <summary>The f pos types.</summary>
        private System.Collections.Generic.List<Neuron> fPosTypes;

        /// <summary>The f learn count.</summary>
        private int fLearnCount;

        /// <summary>The f instruction tool box items.</summary>
        private System.Collections.ObjectModel.ObservableCollection<ToolBoxItem> fInstructionToolBoxItems;

        /// <summary>The f test cases.</summary>
        private Data.ObservedCollection<Test.TestCase> fTestCases;

        /// <summary>The f chatbot props.</summary>
        private ChatbotProperties fChatbotProps;

        /// <summary>The testcaseext.</summary>
        public const string TESTCASEEXT = "testcase";

        /// <summary>The f active document.</summary>
        private object fActiveDocument;

        #endregion

        #region Events

        /// <summary>
        ///     Raised just before the Brain data specific to the app is saved.
        /// </summary>
        public event System.EventHandler BeforeSave;

        /// <summary>
        ///     Raised just after the Brain data specific to the app is saved.
        /// </summary>
        public event System.EventHandler AfterSave;

        /// <summary>
        ///     Raised just after the Brain data specific to the application is loaded.
        /// </summary>
        public event System.EventHandler AfterLoad;

        #endregion

        #region Prop

        #region Name

        /// <summary>
        ///     Gets/sets the name of the project. When changed, we also update the namespaces, so that they use the
        ///     correct project name.
        /// </summary>
        public string Name
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Name;
                }

                return null;
            }

            set
            {
                if (DesignerData != null)
                {
                    OnPropertyChanging("Name", DesignerData.Name, value);
                    DesignerData.Name = value;
                    SetChatbotChannelCaption(value);
                    OnPropertyChanged("Name");
                }
                else
                {
                    throw new System.InvalidOperationException("No data file loaded.");
                }
            }
        }

        /// <summary>Changes the caption of the chatbot channel (only 1 currently available in the system, so we take the
        ///     first that is found.</summary>
        /// <param name="value">The value.</param>
        private void SetChatbotChannelCaption(string value)
        {
            ChatBotChannel iFound = null;
            System.Collections.Generic.IList<CommChannel> iChannels = Current.CommChannels;
            if (iChannels != null)
            {
                iFound = (from i in iChannels where i is ChatBotChannel select (ChatBotChannel)i).FirstOrDefault();
            }

            if (iFound != null)
            {
                iFound.NeuronInfo.DisplayTitle = value;
            }
        }

        #endregion

        #region DesignerData

        /// <summary>
        ///     Gets/sets the data specific to the designer.
        /// </summary>
        /// <remarks>
        ///     This includes a lot of the properties exposed by this class. This property makes certain that when a new
        ///     value is loaded, old data is unloaded and new data is properly registered.
        /// </remarks>
        public DesignerDataFile DesignerData
        {
            get
            {
                return fDesignerData;
            }

            set
            {
                if (value != fDesignerData)
                {
                    if (fDesignerData != null)
                    {
                        Unregister(fDesignerData);
                    }

                    fDesignerData = value;
                    if (fDesignerData != null)
                    {
                        Register(fDesignerData);
                    }

                    var iType = typeof(DesignerDataFile);

                        // for each property in the designerdata, raise the event that it is changed.
                    foreach (var iProp in iType.GetProperties())
                    {
                        OnPropertyChanged(iProp.Name);
                    }
                }
            }
        }

        #endregion

        #region CommChannels

        /// <summary>
        ///     Gets the list of communcation channels that are available for the currently loaded <see cref="Brain" />.
        /// </summary>
        public CommChannelCollection CommChannels
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.CommChannels;
                }

                return null;
            }
        }

        #endregion

        #region Debugmode

        /// <summary>
        ///     Gets/sets which level of debugging should be used by the <see cref="DebugProcessor" />s used
        ///     by this application.
        /// </summary>
        public DebugMode Debugmode
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Debugmode;
                }

                return DebugMode.Off;
            }

            set
            {
                if (DesignerData != null)
                {
                    if (DesignerData.Debugmode != value)
                    {
                        // if we don't do this, we can't select a proc in the UI cause by setting the debugMode, the selected proc gets reset (tree gets refreshed).
                        DesignerData.Debugmode = value;
                        OnPropertyChanged("Debugmode");
                        if (ProcessorManager.Current != null)
                        {
                            ProcessorManager.Current.DebugModeChanged();
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("No data file loaded.");
                }
            }
        }

        #endregion

        #region BreakOnException

        /// <summary>
        ///     Gets/sets wether the processor should break when an exception occurs (error is logged).
        /// </summary>
        public bool BreakOnException
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.BreakOnException;
                }

                return false;
            }

            set
            {
                if (DesignerData != null)
                {
                    DesignerData.BreakOnException = value;
                    OnPropertyChanged("BreakOnException");
                }
                else
                {
                    throw new System.InvalidOperationException("No data file loaded.");
                }
            }
        }

        #endregion

        #region BreakPoints

        /// <summary>
        ///     Gets the list of all the expressions that are defined as break points.
        ///     Remember to lock this field each time you use it cause this is accessed acrorss multiple threads (by the processors
        ///     and the designer).
        /// </summary>
        public BreakPointCollection BreakPoints
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.BreakPoints;
                }

                return null;
            }
        }

        #endregion

        #region ToolBoxItems

        /// <summary>
        ///     Gets the list of toolbox items Loaded in the designer.
        /// </summary>
        /// <remarks>
        ///     this only stores the <see cref="NeuronToolBoxItem" />s, not all of them,
        ///     caus <see cref="TypeToolBoxitem" />s are hardcoded.
        /// </remarks>
        public ToolBoxItemCollection ToolBoxItems
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.ToolBoxItems;
                }

                return null;
            }
        }

        #endregion

        #region InstructionToolBoxItems

        /// <summary>
        ///     Gets the list of instructions as toolbox items.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<ToolBoxItem> InstructionToolBoxItems
        {
            get
            {
                if (fInstructionToolBoxItems == null)
                {
                    fInstructionToolBoxItems = new System.Collections.ObjectModel.ObservableCollection<ToolBoxItem>();
                    LoadInstructionsView();
                }

                return fInstructionToolBoxItems;
            }

            internal set
            {
                fInstructionToolBoxItems = value;
                OnPropertyChanged("InstructionToolBoxItems");
                OnPropertyChanged("InstructionsView");
            }
        }

        #endregion

        #region InstructionsView

        /// <summary>
        ///     Gets a view for all the instructions (used by the toolbox).
        /// </summary>
        public System.Windows.Data.ListCollectionView InstructionsView
        {
            get
            {
                var iRes = new System.Windows.Data.ListCollectionView(InstructionToolBoxItems);
                iRes.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription(
                        "Category", 
                        System.ComponentModel.ListSortDirection.Ascending));
                iRes.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription(
                        "Title", 
                        System.ComponentModel.ListSortDirection.Ascending));
                var iDesc = new System.Windows.Data.PropertyGroupDescription();
                iDesc.PropertyName = "Category";
                iRes.GroupDescriptions.Add(iDesc);
                return iRes;
            }
        }

        #endregion

        #region CodeEditors

        /// <summary>
        ///     Gets the list of known code editor objects
        /// </summary>
        /// <remarks>
        ///     this seperate list is required and needs to be stored in xml cause the 'OpenDocuments' list uses this
        ///     to find the actual object as it only stores references.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<CodeEditor> CodeEditors
        {
            get
            {
                return fCodeEditors;
            }
        }

        #endregion

        #region DefaultMeaningIds

        /// <summary>
        ///     Gets the list of neuron id's that are frequently used as meaning neurons.
        /// </summary>
        /// <remarks>
        ///     This list allows for fast editing of meaning values.
        ///     This is the format in which it is saved, it is provided here, in case.
        /// </remarks>
        public SmallIDCollection DefaultMeaningIds
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.DefaultMeaningIds;
                }

                return null;
            }
        }

        #endregion

        #region Default meanings

        /// <summary>
        ///     Gets a sorted list (on DisplayTitle) with all the neurons that are prefefined as meanings of links.
        /// </summary>
        /// <remarks>
        ///     Don't keep this list alife, it gets recreated each time you call the getter,
        ///     this is because the brain might unload memory objects, so they shouldn't be
        ///     kept to long in mem.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<Neuron> DefaultMeanings
        {
            get
            {
                if (DefaultMeaningIds != null)
                {
                    // can somehow happen sometimes when switching between projects.
                    var iRes =
                        (from i in DefaultMeaningIds orderby Current.NeuronInfo[i].DisplayTitle select Brain.Current[i])
                            .ToList();
                    return iRes;
                }
                else
                {
                    var iRes = new System.Collections.Generic.List<Neuron>();
                    return iRes;
                }
            }
        }

        #endregion

        #region Default meaning data

        /// <summary>
        ///     Gets a sorted list (on DisplayTitle) with all the neuronDatas that are prefefined as meanings of links.
        /// </summary>
        /// <remarks>
        ///     Don't keep this list alife, it gets recreated each time you call the getter,
        ///     this is because the brain might unload memory objects, so they shouldn't be
        ///     kept to long in mem.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<NeuronData> DefaultMeaningsData
        {
            get
            {
                if (DefaultMeaningIds != null)
                {
                    // can somehow happen sometimes when switching between projects.
                    var iRes =
                        (from i in DefaultMeaningIds let d = Current.NeuronInfo[i] orderby d.DisplayTitle select d)
                            .ToList();
                    return iRes;
                }
                else
                {
                    var iRes = new System.Collections.Generic.List<NeuronData>();
                    return iRes;
                }
            }
        }

        #endregion

        #region AssetPronounIds

        /// <summary>
        ///     Gets the list of neuron id's that are used by the asset editors to express things like: why, when, how, where,..
        /// </summary>
        /// <remarks>
        /// </remarks>
        public AssetPronounsMap AssetPronounIds
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.AssetPronounIds;
                }

                return null;
            }
        }

        #endregion

        #region PersonMapIds

        /// <summary>
        ///     Gets the list of neuron id's that are used by the thesaurus to create a link between objects that represent things
        ///     like: I, me, my, myself, mine, you, your, yours,..
        /// </summary>
        /// <remarks>
        /// </remarks>
        public PersonsMap PersonMapIds
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.PersonMapIds;
                }

                return null;
            }
        }

        #endregion

        #region LogicalOpsInfo - neuronData

        /// <summary>
        ///     Gets a list of NeuronData objects for the logical operators.
        /// </summary>
        /// <value>The logical ops data.</value>
        public System.Collections.Generic.IList<NeuronData> LogicalOpsData
        {
            get
            {
                var iRes = new System.Collections.Generic.List<NeuronData>();
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.Or]);
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.And]);
                return iRes;
            }
        }

        #endregion

        #region List items

        /// <summary>
        ///     Gets a list of NeuronData objects that can be used for cluster meanings to represent lists.
        /// </summary>
        /// <value>The logical ops data.</value>
        public System.Collections.Generic.IList<NeuronData> ListOpsData
        {
            get
            {
                var iRes = new System.Collections.Generic.List<NeuronData>();
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.Or]);
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.And]);
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.Argument]);
                iRes.Add(NeuronInfo[(ulong)PredefinedNeurons.List]);
                return iRes;
            }
        }

        #endregion

        #region Current

        /// <summary>
        ///     Gets the extra data required by the designer for the <see cref="Brain" />.
        /// </summary>
        public static BrainData Current
        {
            get
            {
                if (fCurrent == null)
                {
                    fCurrent = new BrainData();
                }

                return fCurrent;
            }

            private set
            {
                if (fCurrent != value)
                {
                    fCurrent = null;

                        // important: when we get the undostore, it tries to create it if it doesn't exist + register this class, so we need  to be null when this is retrieved for the first time.
                    var iStore = WindowMain.UndoStore;
                    if (iStore != null)
                    {
                        WindowMain.UndoStore.Register(value); // important: need to do this before fCurrent is assigned
                    }

                    fCurrent = value;
                }
            }
        }

        #endregion

        #region OpenDocuments

        /// <summary>
        ///     Gets the list of currently open documents.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public OpenDocsCollection OpenDocuments
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.OpenDocuments;
                }

                return null;
            }
        }

        #endregion

        #region ActiveDocument

        /// <summary>
        ///     Gets/sets the document that is currently active.
        /// </summary>
        public object ActiveDocument
        {
            get
            {
                return fActiveDocument;
            }

            set
            {
                fActiveDocument = value;
                OnPropertyChanged("ActiveDocument");
            }
        }

        #endregion

        #region NeuronInfo

        /// <summary>
        ///     Gets the dictionary with all the objects containing extra info for the neurons.
        /// </summary>
        /// <remarks>
        ///     When a neuron is created, there is no neurondata automatically created to go with it.
        ///     Whenever an object
        /// </remarks>
        public NeuronDataDictionary NeuronInfo
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.NeuronInfo;
                }

                return null;
            }
        }

        #endregion

        #region Editors

        /// <summary>
        ///     Gets the Tree of editors defined in the current project.
        /// </summary>
        /// <remarks>
        ///     This list contains all the root iems.  Some can be folders, so a tree like structure is possible.
        ///     This is important for some events.
        /// </remarks>
        public EditorCollection Editors
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Editors;
                }

                return null;
            }
        }

        #endregion

        #region CurrentEditorsList

        /// <summary>
        ///     Gets/sets the currently active editors list.  This can be the root list (see <see cref="BrainData.Editors" />)
        ///     or a child folder node.
        /// </summary>
        public System.Collections.Generic.IList<EditorBase> CurrentEditorsList
        {
            get
            {
                return fCurrentEditorsList;
            }

            set
            {
                if (fCurrentEditorsList != value)
                {
                    fCurrentEditorsList = value;
                    OnPropertyChanged("CurrentEditorsList");
                }
            }
        }

        #endregion

        #region Instructions

        /// <summary>
        ///     Gets the list of all the available instructions in the brain.
        /// </summary>
        /// <remarks>
        ///     This property is used by the <see cref="CodeEditors" />
        /// </remarks>
        public NeuronCollection<Instruction> Instructions
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Instructions;
                }

                return null;
            }
        }

        #endregion

        #region Operators

        /// <summary>
        ///     Gets the list of all the available operators (as in Add, substract, equals,...) in the brain.
        /// </summary>
        /// <remarks>
        ///     This property is used by the <see cref="CodeEditors" /> in some templates.
        /// </remarks>
        public NeuronCollection<Neuron> Operators
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Operators;
                }

                return null;
            }
        }

        #endregion

        #region FrameElementImportances

        /// <summary>
        ///     Gets the list of neurons that can be used as frame element importances.
        /// </summary>
        /// <remarks>
        ///     This is a helper prop so that frame element editors provide the same list of values.
        /// </remarks>
        /// <value>The frame element importances.</value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<Neuron> FrameElementImportances
        {
            get
            {
                var iRes = new System.Collections.Generic.List<Neuron>();
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Frame_Core]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Frame_peripheral]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Frame_extra_thematic]);
                return iRes;
            }
        }

        #endregion

        #region FrameElementImportances

        /// <summary>
        ///     Gets the list of neurons that can be used as logical operator for restrictions on frame element.
        /// </summary>
        /// <remarks>
        ///     This is a helper prop so that frame element editors provide the same list of values.
        /// </remarks>
        /// <value>The frame element importances.</value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<Neuron> FrameElementLogicOperators
        {
            get
            {
                var iRes = new System.Collections.Generic.List<Neuron>();
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.And]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Or]);
                return iRes;
            }
        }

        #endregion

        #region FrameElementImportances

        /// <summary>
        ///     Gets the list of neurons that can be used as restriction modifiers for restrictions on frame element.
        /// </summary>
        /// <remarks>
        ///     This is a helper prop so that frame element editors provide the same list of values.
        /// </remarks>
        /// <value>The frame element importances.</value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<Neuron> FrameElementInclusionModifiers
        {
            get
            {
                var iRes = new System.Collections.Generic.List<Neuron>();
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.RestrictionModifierInclude]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.RestrictionModifierExclude]);
                return iRes;
            }
        }

        #endregion

        #region PosValues

        /// <summary>
        ///     Gets the list of possible pos values.
        /// </summary>
        /// <remarks>
        ///     This is a helper prop so that editors can provide a drop down list.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<Neuron> PosValues
        {
            get
            {
                var iRes = new System.Collections.Generic.List<Neuron>();
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Verb]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Noun]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Adverb]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Adjective]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Preposition]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Conjunction]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Interjection]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Article]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Determiner]);
                iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Complementizer]);
                return iRes;
            }
        }

        #endregion

        #region Thesaurus

        /// <summary>
        ///     Gets the thesaurus root neurons for the network.
        /// </summary>
        public Thesaurus Thesaurus
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Thesaurus;
                }

                return null;
            }
        }

        #endregion

        #region TestCases

        /// <summary>
        ///     Gets the list of testcases that are available for this project.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Test.TestCase> TestCases
        {
            get
            {
                if (fTestCases == null)
                {
                    LoadTestCases();
                }

                return fTestCases;
            }
        }

        #endregion

        #region LinkLists

        /// <summary>
        ///     Gets a list containing all the neurons used to identify lists of links that can be searched.  This is currently
        ///     'In' and 'Out'.
        /// </summary>
        /// <remarks>
        ///     This is static info (core feature of the brain), so don't serialize.
        ///     This prop is provided for xaml, is used on CodeEditor.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<Neuron> LinkLists
        {
            get
            {
                if (fLinkLists == null)
                {
                    fLinkLists = new System.Collections.Generic.List<Neuron>();
                    fLinkLists.Add(Brain.Current[(ulong)PredefinedNeurons.In]);
                    fLinkLists.Add(Brain.Current[(ulong)PredefinedNeurons.Out]);
                }

                return fLinkLists;
            }
        }

        #endregion

        #region ClusterLists

        /// <summary>
        ///     Gets the list of neurons that represent lists related to clusters that can be searched. This is currently
        ///     'Children' and
        ///     'Clusters'.
        /// </summary>
        /// <remarks>
        ///     This is static info (core feature of the brain), so don't serialize.
        ///     This prop is provided for xaml, is used on CodeEditor.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<Neuron> ClusterLists
        {
            get
            {
                if (fClusterLists == null)
                {
                    fClusterLists = new System.Collections.Generic.List<Neuron>();
                    fClusterLists.Add(Brain.Current[(ulong)PredefinedNeurons.Children]);
                    fClusterLists.Add(Brain.Current[(ulong)PredefinedNeurons.Clusters]);
                }

                return fClusterLists;
            }
        }

        #endregion

        #region GrammarTypes

        /// <summary>
        ///     Gets the list of all the neurons that represent the different grammar types (part of speech).
        /// </summary>
        public System.Collections.Generic.IList<Neuron> GrammarTypes
        {
            get
            {
                if (fPosTypes == null)
                {
                    fPosTypes = new System.Collections.Generic.List<Neuron>();
                    fPosTypes.Add(null);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Adjective]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Adverb]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Article]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Complementizer]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Conjunction]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Determiner]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Interjection]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Noun]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Preposition]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Pronoun]);
                    fPosTypes.Add(Brain.Current[(ulong)PredefinedNeurons.Verb]);
                }

                return fPosTypes;
            }
        }

        #endregion

        #region IsChanged

        /// <summary>
        ///     Gets the if the project data has been changed since the last save.
        /// </summary>
        /// <remarks>
        ///     We use a little trick to see if the data is changed: we keep track of the last undo item
        /// </remarks>
        public bool IsChanged
        {
            get
            {
                if (WindowMain.UndoStore.UndoData.Count > 0)
                {
                    return fLastUndoItem != WindowMain.UndoStore.UndoData[WindowMain.UndoStore.UndoData.Count - 1];
                }

                return fLastUndoItem != null;
            }
        }

        #endregion

        #region LearnCount

        /// <summary>
        ///     Gets or sets the nr of units that are currently learning.
        /// </summary>
        /// <remarks>
        ///     Used to adjust the <see cref="BrainData.IsLearning" /> value.
        /// </remarks>
        /// <value>The learn count.</value>
        internal int LearnCount
        {
            get
            {
                return fLearnCount;
            }

            set
            {
                fLearnCount = value;
                OnPropertyChanged("IsLearning");
            }
        }

        #endregion

        #region IsLearning

        /// <summary>
        ///     Gets wether the network is currently learning (aqcuiring) data from an external resource like wordnet, framenet,
        ///     the internet,...
        /// </summary>
        /// <remarks>
        ///     To change this value, use <see cref="BrainData.LearnCount" />.
        /// </remarks>
        public bool IsLearning
        {
            get
            {
                return LearnCount > 0;
            }
        }

        #endregion

        #region Overlays

        /// <summary>
        ///     Gets/sets the list of overlays to use in this project.
        /// </summary>
        /// <remarks>
        ///     This is a simple list and not obsevable because we need to recalculate all the items each time anything in the
        ///     list is changed + all the changes which were done in 1 edit (so a store after the close button was pressed) should
        ///     be undoable in 1 go.
        /// </remarks>
        public System.Collections.Generic.List<OverlayText> Overlays
        {
            get
            {
                if (DesignerData != null)
                {
                    return DesignerData.Overlays;
                }

                return null;
            }

            set
            {
                if (DesignerData != null)
                {
                    OnPropertyChanging("Overlays", DesignerData.Overlays, value);
                    DesignerData.Overlays = value;
                    NeuronInfo.ReloadOverlays();
                    OnPropertyChanged("Overlays");
                }
            }
        }

        #endregion

        #region ChatbotProps

        /// <summary>
        ///     Gets/sets the object that manages the properties of the chatbot.
        /// </summary>
        public ChatbotProperties ChatbotProps
        {
            get
            {
                return fChatbotProps;
            }

            set
            {
                if (value != fChatbotProps)
                {
                    if (fChatbotProps != null)
                    {
                        // for undo and everything.
                        UnRegisterChild(fChatbotProps);
                    }

                    fChatbotProps = value;
                    if (fChatbotProps != null)
                    {
                        // for undo and everything.
                        RegisterChild(fChatbotProps);
                    }

                    OnPropertyChanged("ChatbotProps");
                }
            }
        }

        #endregion

        #region AttachedTopics

        /// <summary>
        ///     Gets the list of topics that are attached to other objects (like thesaurus nodes or assets).
        /// </summary>
        public AttachedTopicsCollection AttachedTopics { get; internal set; }

        #endregion

        #endregion

        #region Functions

        /// <summary>This function is used by the <see cref="TopcisDictionary"/> to retrieve the name of rules. This way, it can
        ///     reside in the backside but have a link to designer info.</summary>
        /// <param name="toMap">To map.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetNameFor(ulong toMap)
        {
            if (NeuronInfo != null)
            {
                return NeuronInfo.GetTitleFor(toMap);
            }

            return null;
        }

        /// <summary>
        ///     Makes certain that the entire db is loaded into memory.
        /// </summary>
        internal void TouchMem()
        {
            NeuronInfo.TouchMem();
        }

        /// <summary>Unregisters the specified data.</summary>
        /// <remarks>Also resets the root <see cref="BrainData.CurrentEditorsList"/>.</remarks>
        /// <param name="data">The data.</param>
        private void Unregister(DesignerDataFile data)
        {
            data.CommChannels.Owner = null;
            if (data.WordnetChannel != null)
            {
                data.WordnetChannel.Owner = null;
            }

            data.ToolBoxItems.Owner = null;
            data.DefaultMeaningIds.Owner = null;
            data.AssetPronounIds.Owner = null;
            if (data.ModulePropIds != null)
            {
                data.ModulePropIds.Owner = null;
            }

            data.PersonMapIds.Owner = null;
            data.Watches.Owner = null;
            data.Editors.Owner = null;
            data.SplitPaths.Owner = null;
            data.Chatbotdata.ParserMap.Owner = null;
            data.Chatbotdata.DoFunctionMap.Owner = null;
            CurrentEditorsList = null;
            OnPropertyChanged("TestCases");

                // let all UI elements attached to the Testcases, update there lists, otherewise the list doesn't get reloaded.
            OnPropertyChanged("AvailableChannels");
            OnPropertyChanged("CommChannels");
        }

        /// <summary>Registers the specified data.</summary>
        /// <remarks>Also sets the root <see cref="BrainData.CurrentEditorsList"/>.</remarks>
        /// <param name="data">The data.</param>
        private void Register(DesignerDataFile data)
        {
            data.NeuronInfo.RegisterNeurons();
            UpdateForOldData(data);
            data.CommChannels.Owner = this;
            foreach (var i in data.CommChannels)
            {
                i.AfterLoaded();
            }

            if (data.WordnetChannel != null)
            {
                data.WordnetChannel.Owner = this;
                data.WordnetChannel.AfterLoaded();
            }

            data.ToolBoxItems.Owner = this;
            data.DefaultMeaningIds.Owner = this;
            data.Watches.Owner = this;
            data.SplitPaths.Owner = this;
            foreach (var i in data.Watches)
            {
                i.RegisterNeuron();
            }

            data.Editors.Owner = this;
            data.Chatbotdata.ParserMap.Owner = this;
            data.Chatbotdata.DoFunctionMap.Owner = this;
            data.AssetPronounIds.Owner = this;
            data.ModulePropIds.Owner = this;
            data.PersonMapIds.Owner = this;
            CurrentEditorsList = data.Editors;
            SortInstructions(data.Instructions);
            LoadInstructionsView();
            foreach (var i in data.Editors)
            {
                i.Register();
            }

            foreach (var i in data.Editors.AllCodeEditors())
            {
                // neuronEditors need to be registered as well after loading
                CodeEditors.Add(i);
            }

            data.OpenDocuments.OpenLoaded();
            OnPropertyChanged("TestCases");

                // let all UI elements attached to the Testcases, update there lists, otherewise the list doesn't get reloaded.
            OnPropertyChanged("AvailableChannels");
            OnPropertyChanged("CommChannels");
        }

        /// <summary>The update for old data.</summary>
        /// <param name="data">The data.</param>
        private void UpdateForOldData(DesignerDataFile data)
        {
            if (data.OpenDocuments == null)
            {
                data.OpenDocuments = new OpenDocsCollection();
            }

            if (data.PersonMapIds == null)
            {
                data.PersonMapIds = new PersonsMap();
            }

            if (data.AssetPronounIds == null)
            {
                // for upgrading from older versions (pre chatbotdesigner versions).
                data.AssetPronounIds = new AssetPronounsMap();
            }

            if (data.ModulePropIds == null)
            {
                data.ModulePropIds = new SmallIDCollection();
            }

            if (data.Thesaurus == null)
            {
                // to handle files that were created before the thesaurus was stored there, create a new one.
                data.Thesaurus = new Thesaurus();
            }

            if (data.Chatbotdata == null)
            {
                // ChatBotdata field was added after the project construction. Some projects don't have this assigned, so check and if not yet created, init the object.
                data.Chatbotdata = new ChatbotData();
            }

            if (data.Chatbotdata.ParserMap == null)
            {
                // was added later on, so check for non existence.
                data.Chatbotdata.ParserMap = new ParserMap();
            }

            if (data.Chatbotdata.DoFunctionMap == null)
            {
                // was added later on, so check for non existence.
                data.Chatbotdata.DoFunctionMap = new FunctionMap();
            }
        }

        /// <summary>
        ///     Loads the 'names' of all the test cases that can be found in the current 'designer' dir.
        /// </summary>
        private void LoadTestCases()
        {
            fTestCases = new Data.ObservedCollection<Test.TestCase>(this);
            if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
            {
                foreach (var iFile in System.IO.Directory.GetFiles(NeuronInfo.StoragePath, "*." + TESTCASEEXT))
                {
                    var iNew = new Test.TestCase();
                    iNew.Name = System.IO.Path.GetFileNameWithoutExtension(iFile);
                    fTestCases.Add(iNew);
                }
            }
        }

        /// <summary>The sort instructions.</summary>
        /// <param name="list">The list.</param>
        private void SortInstructions(NeuronCollection<Instruction> list)
        {
            // we sort the instruction list, each time we load. this is saved to ensure the list is sorted.
            var iTemp = list.OrderBy(p => p != null ? Current.NeuronInfo[p.ID].DisplayTitle : string.Empty).ToList();

                // we need to convert cause we are going to modify the list.
            list.Clear();
            foreach (var i in iTemp)
            {
                list.Add(i);
            }
        }

        /// <summary>
        ///     Loads all the instructions into a sorted view, ready for display.
        /// </summary>
        private void LoadInstructionsView()
        {
            if (Instructions != null)
            {
                fInstructionToolBoxItems = new System.Collections.ObjectModel.ObservableCollection<ToolBoxItem>();
                foreach (Neuron i in Instructions)
                {
                    var iNew = new InstructionToolBoxItem();
                    iNew.Item = i;
                    fInstructionToolBoxItems.Add(iNew);
                }

                OnPropertyChanged("InstructionsView");
            }
        }

        /// <summary>
        ///     Raises the event.
        /// </summary>
        protected internal void OnLoaded()
        {
            if (AfterLoad != null)
            {
                AfterLoad(this, new System.EventArgs());
            }

            Brain.Current.CallNetActivityEvent((ulong)PredefinedNeurons.OnStarted);
        }

        /// <summary>
        ///     Stores the current undo location so that we can keep track of when the project was changed.
        /// </summary>
        internal void StoreUndoLocation()
        {
            if (WindowMain.UndoStore.UndoData.Count > 0)
            {
                fLastUndoItem = WindowMain.UndoStore.UndoData[WindowMain.UndoStore.UndoData.Count - 1];
            }
            else
            {
                fLastUndoItem = null;
            }
        }

        /// <summary>
        ///     Performs custom actions after saving. Note: this is also called when the save didn't succeed completely successful.
        /// </summary>
        internal void OnAfterSave()
        {
            // foreach (CodeEditor i in CodeEditors)                                                  //remove all the temp toolboxitems for the variables that can be used in code editors.
            // i.UnParkRegisteredVariables();
            if (AfterSave != null)
            {
                AfterSave(this, new System.EventArgs());
            }
        }

        /// <summary>
        ///     Allows to perform + performs various actions before the data is saved.  Calls the apropriate event handlers.
        /// </summary>
        protected internal virtual void OnBeforeSave()
        {
            // foreach (CodeEditor i in CodeEditors)                                                  //remove all the temp toolboxitems for the variables that can be used in code editors.
            // i.ParkRegisteredVariables();
            if (BeforeSave != null)
            {
                BeforeSave(this, new System.EventArgs());
            }
        }

        #endregion

        #region eventhandlers

        #region List syncing

        /// <summary>Called when the CodeEditors collection changes.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>- Whenever an item is removed from this list, we need to make certain
        ///     it is also removed from the open documents.</remarks>
        private void fCodeEditors_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CleanOpenDocuments<CodeEditor>(e.Action, e.OldItems);
        }

        /// <summary>Handles the CollectionChanging event of the BrainData control.</summary>
        /// <remarks>We handle changes in the lists of the <see cref="DesignerDataFile"/> here so that we can
        ///     handle tree structures correctly (if we attach to the list itself, it doesn't get called recurivelly).
        ///     This recursiveness is important for the Editors list, cause it is a tree.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.CollectionChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void BrainData_CollectionChanging(object sender, UndoSystem.Interfaces.CollectionChangingEventArgs e)
        {
            if (e.OrignalSource == CommChannels)
            {
                CleanOpenDocuments(e);
            }
            else if (e.OrignalSource == DefaultMeaningIds)
            {
                OnPropertyChanged("DefaultMeanings");
            }
            else if (e.OrignalSource == Editors)
            {
                Editors_CollectionChanging(sender, e);
            }
        }

        /// <summary>Handles the CollectionChanging event of the Editors control.</summary>
        /// <remarks>When the <see cref="BrainData.Editors"/> collection is changed because there were editors removed or
        ///     the list got reset,  we need to make certain that the openDocuments list is also updated + let the editor know it
        ///     got
        ///     removed from the project.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.CollectionChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void Editors_CollectionChanging(object sender, UndoSystem.Interfaces.CollectionChangingEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ((EditorBase)e.Item).EditorRemoved(OpenDocuments);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    ((EditorBase)e.Item).EditorRemoved(OpenDocuments);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (EditorBase i in e.Items)
                    {
                        i.EditorRemoved(OpenDocuments);
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>Cleans the list of open documents with regards to the specified action and involved items.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.CollectionChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void CleanOpenDocuments(UndoSystem.Interfaces.CollectionChangingEventArgs e)
        {
            if (OpenDocuments != null)
            {
                // this can be null if a clean is performed when there was no previous newPRoject called.
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    OpenDocuments.Remove(e.Item);
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    foreach (var i in e.Items)
                    {
                        OpenDocuments.Remove(i);
                    }
                }
            }
        }

        /// <summary>Cleans the list of open documents with regards to the specified action and involved items.</summary>
        /// <typeparam name="T">The type of neuron that should be cleaned from the open documents.</typeparam>
        /// <param name="action">The action to perform: simple remove or complete reset.</param>
        /// <param name="toRemove">The items to remove.</param>
        private void CleanOpenDocuments<T>(
            System.Collections.Specialized.NotifyCollectionChangedAction action, 
            System.Collections.IList toRemove)
        {
            if (OpenDocuments != null)
            {
                // this can be null if a clean is performed when there was no previous newPRoject called.
                if (action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var i in toRemove)
                    {
                        OpenDocuments.Remove(i);
                    }
                }
                else if (action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    var iToRemove = (from i in OpenDocuments where i is T select i).ToList();
                    foreach (var i in iToRemove)
                    {
                        OpenDocuments.Remove(i);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}