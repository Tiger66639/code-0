// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetSin.cs" company="">
//   
// </copyright>
// <summary>
//   A  which connects to the WordNet database for retrieving textual information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A <see cref="Sin" /> which connects to the WordNet database for retrieving textual information.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This sin is used internally by the brain to retrieve more info about text that it doesn't yet understand.
    ///         The brain performs this request by sending textneurons to this sin.  Each textneuron will be searched in
    ///         the database, and when found, links will be added to it. This means that, if the word is already loaded, it
    ///         will be loaded again (and only partly overwritten through the dictionary of the textsin).
    ///     </para>
    ///     <para>
    ///         This sin uses a Sql server database file as it's backing store.
    ///     </para>
    ///     <para>
    ///         Algorithm:
    ///         -a word is first searched in the 'WordInfo' datatable (query).  This contains all the senses (meanings) of
    ///         the word. For most senses, there is also an example sentence included.
    ///         -for each sense of the word, we create an object cluster which contains all the synonyms of that sense.
    ///         This can be single textneurons or compound words, which are clusters of textneurons (representing a composed
    ///         word like 'far away')
    ///         -Create a part of speech group for each unique pos of all the senses, and add all the objects to the cluster
    ///         that have the same pos.
    ///         -if the sense contained an example sentence, store this as well (linked to the object) + solved because it
    ///         normally describes some characteristic of the object. -> not yet done
    ///         -next: do a lookup for each sense in the 'RelatedWords' datatable (query).  This contains all the relationships
    ///         of the sense with other senses.  We do a lookup in this 'RelatedWords' table for the sense + each record from
    ///         'Relationships'. this is done for both semantic and syntactic relationships.
    ///         -for each relationship, create a new cluster, with as meaning, the relationship.  Lookup the correct object for
    ///         each word (lookup synsetid in dict) and add the object to the cluster.
    ///         -if the relationship is declared as recursive, perform the operation recursive untill a point is reached that
    ///         is
    ///         already in the database, or a root node.
    ///         -check all the regular expressions that are defined, to find syntactical transformations, like plural nouns
    ///         (noun, nouns),
    ///         or verb conjugations (find, finding).
    ///         -also mak
    ///     </para>
    ///     <para>
    ///         During generation, all IntValue neurons are reused. This means that all objects using the same synsetid loaded
    ///         during the
    ///         same run of the application, will use the same neuron as synset id.  If they are loaded on different runs, a
    ///         new neuron
    ///         is created.
    ///     </para>
    ///     <para>
    ///         The original wordnet database also contained VerbNet data. This is not imported from here, the actual verbnet
    ///         data is used.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.WordNetSin)]
    [NeuronID((ulong)PredefinedNeurons.SynSetID, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.CompoundWord, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.WordNetRelationships, typeof(NeuronCluster))]
    [NeuronID((ulong)PredefinedNeurons.MorphOf, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.IsRecursive, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.LoadAssetItem, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.LoadThesValue, typeof(Neuron))]
    public class WordNetSin : Sin
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WordNetSin" /> class.
        /// </summary>
        public WordNetSin()
        {
            Brain.Current.Cleared += Current_Cleared;
        }

        /// <summary>Handles the Cleared event of the Current control.</summary>
        /// <remarks>when the brain gets cleared, we need to reset fDefault, otherwise, we keep a reference to the neuron of the
        ///     previous
        ///     network.  We also unreg the event handler so that the wordnetsin can be unloaded completely.
        ///     LinkDefs are also reset.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Current_Cleared(object sender, System.EventArgs e)
        {
            Brain.Current.Cleared -= Current_Cleared;
            fLinkDefs = null;
            fReverseLinkDefs = null;
            fDefault = null;
        }

        #region inner types

        /// <summary>
        ///     We declare a custom type for this dict, so we can easely remove items from the list by using the ulong, through
        ///     a double internal dictionary
        /// </summary>
        public class LinkDefsDict : Data.SerializableDictionary<string, ulong>
        {
        }

        /// <summary>The regex def.</summary>
        public class RegexDef
        {
            /// <summary>
            ///     Gets or sets the ID to be used when the link is created between the 2 word forms
            /// </summary>
            /// <value>The ID.</value>
            public ulong ID { get; set; }

            /// <summary>
            ///     Gets or sets the part of speech that the regex value represents. For instance,
            ///     if the regex value = 'words', and the match is 'word', the pos is 'n' for nouns, since
            ///     the regex value should ref the nouns posgropu of 'word'.
            /// </summary>
            /// <value>The POS.</value>
            public string POS { get; set; }

            /// <summary>
            ///     Gets or sets the regular expression that defines how a verb is conjugated
            /// </summary>
            /// <remarks>
            ///     To return values from the regex, use 'result', 'before1', 'before2','after1', 'after2'...
            ///     where result is always the root word that needs to be searched  on wordnet, while before 1,2,.. upto 9 are leading
            ///     and after1, after2 are trailing parts of the conjugated for (like 'ing' or 's' at the end).
            /// </remarks>
            /// <value>The reg ex.</value>
            public string RegEx { get; set; }
        }

        /// <summary>
        ///     An exception used to get the import process stopeed immediatly without having to concern about
        ///     data consistency (used to stop imports when a network gets cleared).
        /// </summary>
        [System.Serializable]
        public class StopImportException : System.Exception
        {
            /// <summary>Initializes a new instance of the <see cref="StopImportException" /> class.</summary>
            public StopImportException()
            {
            }

            /// <summary>Initializes a new instance of the <see cref="StopImportException"/> class.</summary>
            /// <param name="message">The message.</param>
            public StopImportException(string message)
                : base(message)
            {
            }

            /// <summary>Initializes a new instance of the <see cref="StopImportException"/> class.</summary>
            /// <param name="message">The message.</param>
            /// <param name="inner">The inner.</param>
            public StopImportException(string message, System.Exception inner)
                : base(message, inner)
            {
            }

            /// <summary>Initializes a new instance of the <see cref="StopImportException"/> class.</summary>
            /// <param name="info">The info.</param>
            /// <param name="context">The context.</param>
            protected StopImportException(
                System.Runtime.Serialization.SerializationInfo info, 
                System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
        }

        /// <summary>
        ///     A custom lust with regexes containing verb construction definitions.  This is a seperate
        ///     class for easy streaming with xml.
        /// </summary>
        public class VerbDefRegexList : System.Collections.Generic.List<RegexDef>
        {
        }

        /// <summary>
        ///     Stores the fields required for searching or creating a relationship cluster. Allows us to group them
        ///     together without having to pass a lot of args.
        /// </summary>
        private class RelationshipArgs
        {
            /// <summary>The data.</summary>
            public RelatedWordsDataTable Data;

            /// <summary>The relation.</summary>
            public RelationshipsRow Relation;

            /// <summary>The relationship link.</summary>
            public ulong RelationshipLink;

            /// <summary>The result.</summary>
            public NeuronCluster Result;

            /// <summary>The synset id.</summary>
            public int SynsetId;
        }

        #endregion inner types

        #region Fields

        /// <summary>The f link defs lock.</summary>
        private static readonly object fLinkDefsLock = new object();

        /// <summary>
        ///     All the linkids that are used in the lexlinkref table. This allows us to be faster (only have to lookup in 1
        ///     table).
        /// </summary>
        private static readonly int[] LEXLINKS = { 30, 50, 71, 80, 81, 91, 92, 93, 94, 95, 96 };

        /// <summary>The f default.</summary>
        private static WordNetSin fDefault;

        /// <summary>The f link defs.</summary>
        private static LinkDefsDict fLinkDefs;

        /// <summary>The f reverse link defs.</summary>
        private static System.Collections.Generic.Dictionary<ulong, string> fReverseLinkDefs;

        /// <summary>The f stop requested.</summary>
        private static bool fStopRequested;

        /// <summary>The f current searches.</summary>
        private readonly System.Collections.Generic.Dictionary<string, System.Threading.ManualResetEvent> fCurrentSearches = new System.Collections.Generic.Dictionary<string, System.Threading.ManualResetEvent>();

        // allows us to quickly do a lookup when a neuron is deleted, to see if it is one of our linkDefs.
        /// <summary>The f int values.</summary>
        private readonly System.Collections.Generic.Dictionary<int, ulong> fIntValues =
            new System.Collections.Generic.Dictionary<int, ulong>();

        /// <summary>The f int values lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fIntValuesLock =
            new System.Threading.ReaderWriterLockSlim();

        /// <summary>The f reg ex file.</summary>
        private string fRegExFile;

        /// <summary>The f rel items.</summary>
        private RelationshipsDataTable fRelItems;

        // dictionary used to store already generated IntNeurons in so that we can reuse id value neurons during generation
        // provides multi thread safe access to the IntValues dict, requiered cause an import can be performed from many different threads at the same time.
        // stores all the text values that are currently being searched, so that threads trying to search for the same string can wait untill the event is signaled (instead of searching themselves).
        /// <summary>The f wordnet meaning.</summary>
        private Neuron fWordnetMeaning;

        #endregion Fields

        #region Events

        /// <summary>
        ///     Occurs when a search has finihsed.
        /// </summary>
        public event System.EventHandler Finihsed;

        /// <summary>
        ///     Occurs when the <see cref="WordNetSin" /> has created a new root 'object' cluster for a relationship type.
        /// </summary>
        public event NewRootHandler NewRoot;

        /// <summary>
        ///     Occurs when a new object was created by WordNet.
        /// </summary>
        /// <remarks>
        ///     Warning: this event can be called from multiple threads, keep this in mind when processing for UI.
        /// </remarks>
        public event NewObjectEventHandler ObjectCreated;

        /// <summary>
        ///     Occurs when a new POSGroup was created by wordnet.
        /// </summary>
        /// <remarks>
        ///     Warning: this event can be called from multiple threads, keep this in mind when processing for UI.
        /// </remarks>
        public event PosGroupEventHandler POSGroupCreated;

        /// <summary>
        ///     Occurs when a new cluster with related words is created for an object.
        /// </summary>
        /// <remarks>
        ///     Warning: this event can be called from multiple threads, keep this in mind when processing for UI.
        /// </remarks>
        public event NewRelationshipEventHandler RelationshipCreated;

        /// <summary>
        ///     Occurs when a new neuron was created for a relationship type that did not yet exist.
        /// </summary>
        /// <remarks>
        ///     Warning: this event can be called from multiple threads, keep this in mind when processing for UI.
        /// </remarks>
        public event NewMetaTypeEventHandler RelationshipTypeCreated;

        /// <summary>
        ///     Occurs when a new search is started.
        /// </summary>
        public event System.EventHandler Started;

        #endregion Events

        #region Prop

        #region Default

        /// <summary>
        ///     Gets the entry point.
        /// </summary>
        /// <remarks>
        ///     A brain usually only has 1 WordNetSin (although theoretically more are allowed). This prop simply provides
        ///     quick acces to it.
        /// </remarks>
        public static WordNetSin Default
        {
            get
            {
                if (fDefault == null)
                {
                    // try to load the WordNetSin.
                    fDefault = Brain.Current[(ulong)PredefinedNeurons.WordNetSin] as WordNetSin;
                    Brain.Current.AddSin(fDefault);

                    // this is to fix a bug in the orinal code that did not add the static sins to the Brain's list. Some projects were created this way (like AICI 1).
                }

                return fDefault;
            }
        }

        #endregion Default

        /// <summary>
        ///     Gets a lis of regular expressions that the sin will try to match against
        ///     searched words.
        /// </summary>
        /// <value>The verb def regexes.</value>
        public VerbDefRegexList Regexes { get; private set; }

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="PredefinedNeurons.TextNeuron" />.
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.WordNetSin;
            }
        }

        #region LinkDefs

        /// <summary>
        ///     Gets the dictionary containing all the known words with their corresponding Neuron id.
        /// </summary>
        /// <remarks>
        ///     this is loaded from the network.
        /// </remarks>
        public static LinkDefsDict LinkDefs
        {
            get
            {
                if (fLinkDefs == null && Brain.Current != null)
                {
                    LoadLinkDefs();
                }
                else if (Brain.Current == null)
                {
                    LogService.Log.LogError(
                        "WordNetSin.LinkDefs", 
                        "Can't load LinkDef dictionray, Brain not yet loaded!");
                }

                return fLinkDefs;
            }
        }

        /// <summary>The load link defs.</summary>
        private static void LoadLinkDefs()
        {
            lock (fLinkDefsLock)
            {
                if (fLinkDefs == null)
                {
                    fLinkDefs = Brain.Current.Storage.GetProperty<LinkDefsDict>(typeof(WordNetSin), "LinkDefs");
                    if (fLinkDefs == null)
                    {
                        fLinkDefs = new LinkDefsDict();
                        fReverseLinkDefs = new System.Collections.Generic.Dictionary<ulong, string>();
                    }
                    else
                    {
                        fReverseLinkDefs = new System.Collections.Generic.Dictionary<ulong, string>();

                        // we need to build the reverse list so we can quickly remove neurons when needed.
                        foreach (var i in fLinkDefs)
                        {
                            fReverseLinkDefs.Add(i.Value, i.Key);
                        }
                    }
                }
            }
        }

        #endregion LinkDefs

        #region IncludeDescription

        /// <summary>
        ///     Gets or sets a value indicating whether to include description info for the lemma when a new object is created.
        /// </summary>
        /// <value><c>true</c> if [include description]; otherwise, <c>false</c>.</value>
        public bool IncludeDescription { get; set; }

        #endregion IncludeDescription

        #region IncludeCompoundWords

        /// <summary>
        ///     Gets or sets a value indicating whether to include compound words in the import process. A compound word is any
        ///     word (or group of words) that includes the searchterm, so a %word% search is done.
        /// </summary>
        /// <remarks>
        ///     This value is used for all imports wether they come from requests from the brain or direct function calls.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if [include compound words]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeCompoundWords { get; set; }

        #endregion IncludeCompoundWords

        #region RegExFile

        /// <summary>
        ///     Gets/sets the path to the file that contains all the regular expressions + The corresponding id's.
        /// </summary>
        /// <remarks>
        ///     When a regular expression gives a match, which returns a verb, a link is created between the posgroup, as
        ///     returned by the regular expression and the word that is being important, using the id that is specified
        ///     in the file.
        ///     <para>
        ///         When assigned, the file is loaded into mem so it can be accessed quickly.
        ///     </para>
        /// </remarks>
        public string RegExFile
        {
            get
            {
                return fRegExFile;
            }

            set
            {
                if (value != fRegExFile)
                {
                    fRegExFile = value;
                    LoadRegexes();
                }
            }
        }

        /// <summary>The load regexes.</summary>
        private void LoadRegexes()
        {
            if (System.IO.File.Exists(RegExFile))
            {
                using (
                    var iFile = new System.IO.FileStream(RegExFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    // we make a local memory copy of the xml file cause this loads faster.
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(VerbDefRegexList));
                    Regexes = (VerbDefRegexList)iSer.Deserialize(iFile);
                }
            }
            else
            {
                LogService.Log.LogWarning(
                    "WordnetSin.LoadRegexes", 
                    string.Format("Couldn't find Verb construction def file at: {0}, created new.", RegExFile));
                Regexes = new VerbDefRegexList();
            }
        }

        #endregion RegExFile

        #region RelItems

        /// <summary>
        ///     Gets the list of relationship items from the db. This is to prevent us from having to rerun this query each time.
        /// </summary>
        public RelationshipsDataTable RelItems
        {
            get
            {
                if (fRelItems == null)
                {
                    var iRelAdapater = new wordnetDataSetTableAdapters.RelationshipsTableAdapter();
                    fRelItems = iRelAdapater.GetData();
                }

                return fRelItems;
            }
        }

        #endregion RelItems

        #endregion Prop

        #region Event callers

        /// <summary>
        ///     Called when a search is finished. Raises the <see cref="WordNetSin.Finished" /> event.
        /// </summary>
        protected virtual void OnFinished()
        {
            if (Finihsed != null)
            {
                Finihsed(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Called when a new possible root as found.</summary>
        /// <param name="rel">The rel.</param>
        /// <param name="root">The root.</param>
        /// <param name="otherPosibbleRoot">The other Posibble Root.</param>
        protected virtual void OnFoundPossibleRoot(Neuron rel, NeuronCluster root, Neuron otherPosibbleRoot)
        {
            if (NewRoot != null)
            {
                NewRoot(
                    this, 
                    new NewRootEventArgs { Relationship = rel, Item = root, PossibleOtherRoot = otherPosibbleRoot });
            }
        }

        /// <summary>Called when a new root is found for a relationship.</summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="item">The item.</param>
        protected virtual void OnFoundRoot(Neuron relationship, NeuronCluster item)
        {
            if (NewRoot != null)
            {
                NewRoot(this, new NewRootEventArgs { Relationship = relationship, Item = item });
            }
        }

        /// <summary>Called when the <see cref="WordNetSin.ObjectCreated"/> is raised.</summary>
        /// <param name="value">The value for which a new object was created.</param>
        /// <param name="cluster">The cluster that was created.</param>
        /// <param name="id">The id for which the new object was created.</param>
        /// <param name="desc">The desc.</param>
        protected virtual void OnObjectCreated(string value, NeuronCluster cluster, int id, string desc)
        {
            if (ObjectCreated != null)
            {
                string iDesc = null;
                if (IncludeDescription)
                {
                    iDesc = desc;
                }

                ObjectCreated(
                    this, 
                    new NewObjectEventArgs { Neuron = cluster, Value = value, SynSetId = id, Description = iDesc });
            }
        }

        /// <summary>The on pos group created.</summary>
        /// <param name="value">The value.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="pos">The pos.</param>
        protected virtual void OnPOSGroupCreated(string value, NeuronCluster cluster, string pos)
        {
            if (POSGroupCreated != null)
            {
                POSGroupCreated(this, new PosGroupEventArgs { Neuron = cluster, POS = pos, Value = value });
            }
        }

        /// <summary>Called when the <see cref="WordNetSin.RelationshipCreated"/> is raised.</summary>
        /// <param name="cluster">The cluster that was created.</param>
        /// <param name="related">The cluster that is related to the created object.</param>
        /// <param name="id">The synsetid for which the new object was created.</param>
        protected virtual void OnRelationshipCreated(NeuronCluster cluster, NeuronCluster related, int id)
        {
            if (RelationshipCreated != null)
            {
                RelationshipCreated(
                    this, 
                    new NewRelationshipEventArgs { Neuron = cluster, Related = related, SynsetId = id });
            }
        }

        /// <summary>Called when <see cref="WordNetSin.RelationshipTypeCreated"/> is raised.</summary>
        /// <param name="relationship">The relationship for which a neuron was created.</param>
        /// <param name="neuron">The neuron that was created.</param>
        /// <param name="isRecursive">The is Recursive.</param>
        protected virtual void OnRelationshipTypeCreated(string relationship, Neuron neuron, bool isRecursive)
        {
            if (RelationshipTypeCreated != null)
            {
                RelationshipTypeCreated(
                    this, 
                    new NewMetaTypeEventArgs { Neuron = neuron, Name = relationship, IsRecursive = isRecursive });
            }
        }

        /// <summary>
        ///     Called when a search is started. Raises the <see cref="WordNetSin.Started" /> event.
        /// </summary>
        protected virtual void OnStarted()
        {
            if (Started != null)
            {
                Started(this, System.EventArgs.Empty);
            }
        }

        #endregion Event callers

        #region Functions

        #region send data to network

        /// <summary>creates a parsed thespath and links this to the string value with 'LoadAssetItem'.
        ///     This is used to instruct the android/chatbot network to load this data both as thes relationships</summary>
        /// <param name="pos">The pos.</param>
        /// <param name="thesPath"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        public void AddToProcess(
            ulong pos, 
            string[] thesPath, 
            string value, System.Collections.Generic.List<Neuron> result)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.ParsedThesVar;
            using (var iChildren = iCluster.ChildrenW) iChildren.Add(pos);
            foreach (var i in thesPath)
            {
                var iChild = BrainHelper.GetNeuronForText(i);
                using (var iChildren = iCluster.ChildrenW) iChildren.Add(iChild);
            }

            var iVal = BrainHelper.GetNeuronForText(value);
            var iLink = Link.Create(iCluster, iVal, (ulong)PredefinedNeurons.LoadAssetItem);
            result.Add(iCluster);
        }

        /// <summary>Used to create the start of an instruction list that needs to be sent to the processor.
        ///     This uses the SynsetId as a link and not the LoadAssetItem, cause we don't want to add the 'lookup-kye'
        ///     to the thesaurus.</summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        public void BeginProcess(string id, System.Collections.Generic.List<Neuron> result)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            var iId = GetFor(id);
            Link.Create(iCluster, iId, (ulong)PredefinedNeurons.SynSetID);
            result.Add(iCluster);
        }

        /// <summary>sends the list of neurons to the network for processing.</summary>
        /// <remarks>what the network does with this input is up to the implementation inside the network, but
        ///     in general, the values are added as thesuarus items to the thespath. If an id is specified,
        ///     an asset is created with the id as value (and attribute something like 'id') + the thespath as attrib with the
        ///     'value' as values part of the asset item.</remarks>
        /// <param name="toSend"></param>
        /// <param name="title">a possible log text.</param>
        public void Process(System.Collections.Generic.List<Neuron> toSend, string title)
        {
            var iProc = ProcessorFactory.GetProcessor();
            toSend.Reverse();

            // we reverse the list cause we want the synset to be executed first (first in the list), but we are workin with a stack, so the last becomes first,...
            Process(toSend, iProc, title);
        }

        #endregion send data to network

        #region Overrides

        // protected override void RemoveEntryPoint(Neuron toRemove)
        // {
        // IntNeuron iInt = toRemove as IntNeuron;
        // if (iInt != null)
        // {
        // fIntValues.Remove(iInt.Value);
        // }
        // else
        // {
        // LinkDefsDict iLinkDefs = LinkDefs;                                         //make certain they are loaded, before we check fReverseLinkDefs, otherwise the latter might still be null.
        // string iKey;
        // if (fReverseLinkDefs.TryGetValue(toRemove.ID, out iKey) == true)
        // {
        // fReverseLinkDefs.Remove(toRemove.ID);
        // LinkDefs.Remove(iKey);
        // }
        // }
        // }

        /// <summary>
        ///     Creates an exact duplicate of this Neuron so the <see cref="Processor" /> can perform a split.
        /// </summary>
        /// <returns>
        ///     An exact duplicate of the argument, but with a new id.
        /// </returns>
        /// <remarks>
        ///     A new id is created for the neuron cause all neurons should have unique numbers.
        /// </remarks>
        public override Neuron Duplicate()
        {
            throw new BrainException("WordNet sins can't be duplicated");
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            if (fLinkDefs != null && Brain.Current != null)
            {
                Brain.Current.Storage.SaveProperty(typeof(WordNetSin), "LinkDefs", fLinkDefs);
            }
        }

        /// <summary>Rerturns the type of the specified property. This is used so we can read all the property types from the storage at
        ///     once
        ///     for converting storage type.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        public override System.Type GetTypeForProperty(string name)
        {
            if (name == "LinkDefs")
            {
                return typeof(LinkDefsDict);
            }

            return base.GetTypeForProperty(name);
        }

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend"></param>
        /// <remarks><para>Only textneurons are allowed.</para>
        /// <para>This method is called by the <see cref="Brain"/> itself during/after processing of input.</para>
        /// </remarks>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                var iText = BrainHelper.GetTextFrom(i);
                if (iText != null)
                {
                    SearchOnWordNet(i, iText);
                }
                else
                {
                    LogService.Log.LogError(
                        "WordNetSin.Output", 
                        "Only textneurons are accepted as valid data to output.");
                }
            }
        }

        /// <summary>
        ///     Called when all the data of the sensory interface needs to be loaded into memory.
        /// </summary>
        public override void TouchMem()
        {
            base.TouchMem();
            if (fLinkDefs == null && Brain.Current != null)
            {
                LoadLinkDefs();
            }
        }

        #endregion Overrides

        /// <summary>Gets the wordnet meaning.</summary>
        public Neuron WordnetMeaning
        {
            get
            {
                if (fWordnetMeaning == null)
                {
                    var iWord = BrainHelper.GetNeuronForText("wordnet meaning");
                    fWordnetMeaning = GetObject(iWord, 1);

                    // we use a fixed '1' as synset, this does not exist and it guarantees us that we always use the same object as link meaning.
                }

                return fWordnetMeaning;
            }
        }

        /// <summary>Gets the id of the neuron that represents  the part of speech as specified by a single letter:
        ///     v = verb, n = noun, a = adjective, r = adverb. All else is emptyID.</summary>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        public static ulong GetPos(string pos)
        {
            ulong iPos;
            if (pos == "v")
            {
                // verb
                iPos = (ulong)PredefinedNeurons.Verb;
            }
            else if (pos == "n")
            {
                // noun
                iPos = (ulong)PredefinedNeurons.Noun;
            }
            else if (pos == "a")
            {
                // adjective
                iPos = (ulong)PredefinedNeurons.Adjective;
            }
            else if (pos == "r")
            {
                // adverb
                iPos = (ulong)PredefinedNeurons.Adverb;
            }
            else
            {
                iPos = EmptyId;
            }

            return iPos;
        }

        /// <summary>Gets all the synonym words for the specified synset id.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<string> GetSynonymsList(int synsetId)
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iSynonymsAdapter = new wordnetDataSetTableAdapters.SynonymsTableAdapter();
            var iSynonymsData = iSynonymsAdapter.GetData(synsetId);
            for (var i = 0; i < iSynonymsData.Count; i++)
            {
                iRes.Add(iSynonymsData[i].word);
            }

            return iRes;
        }

        /// <summary>Gets all the synonym words for the specified synset id.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetSynonymsText(int synsetId)
        {
            var iSynonymsAdapter = new wordnetDataSetTableAdapters.SynonymsTableAdapter();
            var iSynonymsData = iSynonymsAdapter.GetData(synsetId);
            var iRes = new System.Text.StringBuilder();
            if (iSynonymsData.Count > 0)
            {
                iRes.Append(iSynonymsData[0].word);
                for (var i = 1; i < iSynonymsData.Count; i++)
                {
                    iRes.Append(", ");
                    iRes.Append(iSynonymsData[i].word);
                }
            }

            return iRes.ToString();
        }

        /// <summary>Gets the first text value for the specified synsetId.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetSynsetText(int synsetId)
        {
            var iSynonymsAdapter = new wordnetDataSetTableAdapters.SynonymsTableAdapter();
            var iSynonymsData = iSynonymsAdapter.GetData(synsetId);
            if (iSynonymsData.Count > 0)
            {
                return iSynonymsData[0].word;
            }

            return null;
        }

        /// <summary>Gets the ID of the neuron that represents the link def in the brain. This can be used as the
        ///     meaning of a cluster for instance.</summary>
        /// <param name="key">The textvalue of the relationship.</param>
        /// <param name="recursive">if set to <c>true</c>, and the relationship needs to be created,it will be a recursive
        ///     relationship.</param>
        /// <returns>The key of the neuron that represents the relationship.</returns>
        /// <remarks>If there is no neuron yet defined for this type of relationship, one is created and wrapped in an object that also
        ///     contains
        ///     a textNeuron for the key.</remarks>
        public ulong GetLinkDef(string key, bool recursive)
        {
            ulong iFound;
            if (LinkDefs.TryGetValue(key, out iFound) == false)
            {
                Neuron iNew;
                var iObj = BrainHelper.CreateObject(key, out iNew); // we create an intire object
                var iRelCluster = Brain.Current[(ulong)PredefinedNeurons.WordNetRelationships] as NeuronCluster;

                // also need to store the linkDef in the Relationships cluster so that the system knows this neuron is a relationship.
                System.Diagnostics.Debug.Assert(iRelCluster != null);
                using (var iChildren = iRelCluster.ChildrenW) iChildren.Add(iObj);

                // we add the object, not the text directly, this is more inline with how the rest of the system works (2 dec 2008).
                iFound = iNew.ID;
                LinkDefs[key] = iFound;
                fReverseLinkDefs[iFound] = key; // build the reverse data, so we can quickly delete.
                if (recursive)
                {
                    Link.Create(iNew, Brain.Current.TrueNeuron, (ulong)PredefinedNeurons.IsRecursive);
                }

                OnRelationshipTypeCreated(key, iNew, recursive);
            }

            return iFound;
        }

        /// <summary>Imports all the related words as found through the regex def and creates a morphOf cluster to indicate the
        ///     relationship + creates
        ///     a link between the texneuron and the morphOf cluster using the specified id (in the regexdef).</summary>
        /// <param name="value">The text.</param>
        /// <param name="regex">The regex.</param>
        /// <param name="text">The text.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster ImportFromRegexDef(Neuron value, RegexDef regex, string text)
        {
            var iMatch = System.Text.RegularExpressions.Regex.Match(text, regex.RegEx);
            if (iMatch.Success)
            {
                var iPos = GetPos(regex.POS);
                if (Brain.Current.IsExistingID(regex.ID))
                {
                    var iFirstMatch = iMatch.Result("${result}");
                    if (iFirstMatch != null)
                    {
                        var iVerb = BrainHelper.GetNeuronForText(iFirstMatch);
                        var iList = UnsafeSearchOnWordNet(iVerb, iFirstMatch);
                        foreach (var iObj in iList)
                        {
                            var iTo = BrainHelper.FindPOSGroup(iObj, iPos);
                            if (iTo != null)
                            {
                                var iMorphOf = NeuronFactory.GetCluster();
                                iMorphOf.Meaning = (ulong)PredefinedNeurons.MorphOf;
                                Brain.Current.Add(iMorphOf);
                                iMorphOf.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, Brain.Current[iPos]);

                                // give the morphOf, a posgroup, this allows easier searching in the database.                                                              //there is only 1 posgroup that we need to link to, so the first object we find that belongs to the posgroup, will cause the link to be created, that's all we need.
                                using (var iChildren = iMorphOf.ChildrenW)
                                {
                                    // do a lock for each operation cause we are writing to cache in between statements, don't want to create deadlocks.
                                    AddPartOfRegexParse("before", iChildren, iMatch);
                                    iChildren.Add(iTo);
                                    AddPartOfRegexParse("after", iChildren, iMatch);
                                }

                                Link.Create(value, iMorphOf, (ulong)PredefinedNeurons.MorphOf);
                                OnPOSGroupCreated(iFirstMatch, iMorphOf, regex.POS);

                                // this isn't really a posgroup, but it looks alot like it, so we use the same event so the editor knows about it.
                                return iMorphOf;
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Wordnetsin.ImportVerbConstructs", 
                            string.Format(
                                "can't create MorphOf cluster for {0}, because of an invalid regular expression (no result defined): {1}", 
                                text, 
                                regex.RegEx));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "Wordnetsin.ImportVerbConstructs", 
                        string.Format(
                            "can't create link between {0} and it's morphOf cluster, because of an invalid link id: {1}.", 
                            text, 
                            regex.ID));
                }
            }

            return null;
        }

        /// <summary>Imports all the info for a single morph of item.</summary>
        /// <param name="original">The original.</param>
        /// <param name="morphed">The morphed.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="text">The text.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster ImportMorphOf(Neuron original, string morphed, string pos, string text)
        {
            var iPos = GetPos(pos);
            var iText = BrainHelper.GetNeuronForText(morphed);
            var iList = UnsafeSearchOnWordNet(iText, morphed); // we get all the objects for the specified morph word
            foreach (var iObj in iList)
            {
                var iTo = BrainHelper.FindPOSGroup(iObj, iPos);
                if (iTo != null)
                {
                    var iMorphOf = NeuronFactory.GetCluster();
                    iMorphOf.Meaning = (ulong)PredefinedNeurons.MorphOf;
                    Brain.Current.Add(iMorphOf);
                    iMorphOf.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, Brain.Current[iPos]);

                    // give the morphOf, a posgroup, this allows easier searching in the database.
                    using (var iChildren = iMorphOf.ChildrenW)
                    {
                        iChildren.Add(iTo);

                        // this needs improving.
                        if (pos == "v")
                        {
                            if (text.EndsWith("ing"))
                            {
                                iChildren.Add(GetFor("ing"));

                                // to handle words like swimming, which is a morph of swim.
                            }
                            else if (text.EndsWith("s"))
                            {
                                // to handle words like I teach, he teaches
                                iChildren.Add(GetFor("s"));
                            }
                        }
                    }

                    Link.Create(original, iMorphOf, (ulong)PredefinedNeurons.MorphOf);
                    OnPOSGroupCreated(morphed, iMorphOf, pos);
                    return iMorphOf;
                }
            }

            return null;
        }

        /// <summary>Tries to find the specified text in the WordNet database and load all the related info into the<see cref="Brain"/>.</summary>
        /// <remarks>This function catches all exceptions and displays them in the log, so it is a safe entry point into the
        ///     database.</remarks>
        /// <param name="toLoad">The string to load. This will be translated into a singled TextNeuron which is searched for.</param>
        public void Load(string toLoad)
        {
            try
            {
                var iText = BrainHelper.GetNeuronForText(toLoad);
                if (iText != null)
                {
                    SearchOnWordNet(iText, toLoad);
                }
                else
                {
                    LogService.Log.LogError(
                        "WordNetSin.Output", 
                        string.Format("Failed to learn about '{0}' from WordNet for an unkown reason.", toLoad));
                }
            }
            catch (StopImportException)
            {
                // don't do anything, we use the stopexception to force an emmidiate stop of a long lasting import.
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "WordNetSing.Load", 
                    string.Format(
                        "Failed to load data for the word '{0}' into the network with the exception: {1}.", 
                        toLoad, 
                        e));
            }
        }

        /// <summary>Tries to find the record with specified text and synsetID, and imports all the relationships defined in wordnet
        ///     with objects that have already been created (so only loads 1 object with possible relationships, not everything
        ///     related to the word).</summary>
        /// <param name="Text">The text to search.</param>
        /// <param name="SynsetID">The synset ID of the word to import.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron LoadCompact(string Text, int SynsetID)
        {
            try
            {
                var iAdapter = new wordnetDataSetTableAdapters.WordInfoTableAdapter();
                var iTable = iAdapter.GetFor(Text, SynsetID);
                if (iTable.Count > 0)
                {
                    // only process if we found items.
                    var iRow = iTable[0];
                    NeuronCluster iObj;
                    OnStarted();

                    // need to let any observers know that a new search has been started, even though it is but a compact one.
                    try
                    {
                        iObj = ImportSynset(iRow.synsetid, iRow.Word, iRow.pos, iRow.Sense, iRow.wsd);
                    }
                    finally
                    {
                        OnFinished();
                    }

                    return iObj;
                }

                LogService.Log.LogError(
                    "WordNetSing.LoadCompact", 
                    string.Format("Failed to find '{0}' (SynsetID: {1}) in wordnet.", Text, SynsetID));
            }
            catch (StopImportException)
            {
                // don't do anything, we use the stopexception to force an emmidiate stop of a long lasting import.
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "WordNetSing.LoadCompact", 
                    string.Format(
                        "Failed to load data for the word '{0}' (SynsetID: {2}) into the network with the exception: {1}.", 
                        Text, 
                        e, 
                        SynsetID));
            }

            return null;
        }

        /// <summary>
        ///     Stops all the currently running imports as fast as possible without completing them. This is to
        /// </summary>
        public void StopImports()
        {
            if (fCurrentSearches.Count > 0)
            {
                // we only try to stop something if there is anything running.
                fStopRequested = true;
                try
                {
                    var fWaitHandles = Enumerable.ToArray(fCurrentSearches.Values);
                    System.Diagnostics.Debug.Assert(fWaitHandles != null);
                    System.Threading.WaitHandle.WaitAll(fWaitHandles); // wait till all of them are done.
                }
                finally
                {
                    fStopRequested = false;

                    // we need to reset this, otherwise we can't restart any imports cause they will stop emmidiatly.
                }
            }
        }

        /// <summary>Adds a part of the regex result (front or back), depending on the  part par.</summary>
        /// <param name="part">The part.</param>
        /// <param name="children">The children.</param>
        /// <param name="match">The match.</param>
        private void AddPartOfRegexParse(
            string part, 
            ChildrenAccessor children, 
            System.Text.RegularExpressions.Match match)
        {
            for (var i = 1; i < 10; i++)
            {
                var iFound = match.Result("${" + part + i + "}");
                if (iFound != null)
                {
                    children.Add(GetFor(iFound));
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>Searhces for and builds all the compound words that contain the specified word.</summary>
        /// <remarks>Does not check the value of <see cref="WordNetSin.IncludeCompoundWords"/>, will always render the compount
        ///     words.</remarks>
        /// <param name="obj">The obj for which to search compound words.</param>
        /// <param name="word">The word to search for.</param>
        private void BuildCompoundWords(NeuronCluster obj, WordInfoRow word)
        {
            var iCompoundAdapter = new wordnetDataSetTableAdapters.CompundWordsTableAdapter();
            var iData = iCompoundAdapter.GetData(word.Word.ToLower());
            foreach (var i in iData)
            {
                if (fStopRequested)
                {
                    throw new StopImportException(); // if a stop was requested, do this immediatly so the process stops
                }

                if (i.synsetid != word.synsetid)
                {
                    // we don't want to store a compound link to itself.
                    ImportSynset(i.synsetid, i.Word, i.pos, i.Sense, i.wsd);
                }
            }
        }

        /// <summary>Creates all the relationships of the word in the brain. This includes semantic and lexical types
        ///     which are 2 different tables in wordnet + the reverse linking.</summary>
        /// <remarks>A relationship is always stored in the following manner:
        ///     - a new neuron cluster is made that contains all the items that are part of the relationship (except
        ///     the 'word' itself).
        ///     - this cluster gets as meaning, the relationship type.
        ///     - a link is made between the word and the new cluster, whith as meaning, the relationship type.
        ///     -&gt; this structure allows us to do a quick search in 2 directions: from a given word, find all the objects
        ///     that are related in the specified way (find a link with the specified meaning to a cluster).  You can
        ///     als search the other way: find the word to which this is 'the relationship type): find a cluster to which
        ///     the neuron belongs which has the specified relationship type, find the link 'out' on that cluster with the
        ///     relationship type as meaning and you have found the related word.</remarks>
        /// <param name="obj">The obj that represents the word.</param>
        /// <param name="synsetid">The synsed id to use.</param>
        /// <param name="word">The word.</param>
        /// <param name="pos">The pos.</param>
        private void BuildRelationships(NeuronCluster obj, int synsetid, string word, string pos)
        {
            var iNoRels = true;

            // if the object has no relationship values what-so-ever, check if there are also no inversed relationships. If this is the case, add it as a root for the 'is a' relationship. if we don't do this, the pos value/wordnet meaning,.. gets lost during export/import of xml data. Easy fix: make certain it is a root.
            foreach (var i in RelItems)
            {
                if (fStopRequested)
                {
                    throw new StopImportException(); // if a stop was requested, do this immediatly so the process stops
                }

                if (i.name.EndsWith("hypernym") == false && i.name.EndsWith("meronym") == false)
                {
                    // don't import duplicate relationships, we can do thi through the backref..
                    var iLinkDef = GetLinkDef(i.name, i.recurses == "Y");
                    var iFound = obj.FindFirstOut(iLinkDef) as NeuronCluster;
                    if (iFound == null)
                    {
                        // the relationship could already exist because of an already existing synonym or because the word is getting reloaded. In this case, we want to check if the existing cluster contains all the related words, and add what is missing (don't remove.
                        var iRes = GetRelationship(i, synsetid);
                        if (iRes != null)
                        {
                            obj.SetFirstOutgoingLinkTo(iLinkDef, iRes.Result);

                            // we use this style of creating the link, this way, if there was a previous link with a wrong value, this is correctly updated.
                            OnRelationshipCreated(obj, iRes.Result, synsetid);
                            if (i.recurses == "Y")
                            {
                                // no root if no recursion offcourse.
                                CheckRoot(obj, iLinkDef, i.linkid, synsetid);

                                // we only need to check if it is a root, if it has a relationship. If it isn't used in wordnet as an other end for this relationship, it is a root.
                                iNoRels = false;
                            }
                            else
                            {
                                CheckRoot(obj, iRes);
                            }
                        }
                    }

                    // else
                    // UpdateRelationship(i, iFound, synsetid);
                }
            }

            if (iNoRels)
            {
                CheckRootForNoRel(obj, synsetid);
            }
        }

        /// <summary>Some none recusrive relationships should be stored as recursive (for adjectives mostly).
        ///     roots are:
        ///     -an object that have multiple children -&gt; always
        ///     - an object that references a single other object and the other object also only references the object or nothing
        ///     and the other
        ///     object is not yet a root itself.
        ///     -not the relationship 'derivation': it produces a tree that is to deep (1500+ levels) and which don't mean that
        ///     much. Also to
        ///     difficult to search properly.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="iRes">The i res.</param>
        private void CheckRoot(NeuronCluster obj, RelationshipArgs iRes)
        {
            var iWordsAdapter = new wordnetDataSetTableAdapters.RelatedWordsTableAdapter();

            // wordnetDataSet.RelatedWordsDataTable iWordsData = iWordsAdapter.ReverseGetDataBy(iRes.Relation.linkid, iRes.SynsetId);
            if (iRes.Data.Count > 0 && iRes.Relation.name != "derivation" && iRes.Relation.name != "antonym")
            {
                var iCanBeRoot = true;
                if (iRes.Data.Count == 1)
                {
                    var iRow = iRes.Data[0];
                    var iChildsRefs = iWordsAdapter.GetData(iRes.Relation.linkid, iRow.synsetid);
                    if (
                        !(iChildsRefs.Count == 0 || (iChildsRefs.Count == 1 && iChildsRefs[0].synsetid == iRes.SynsetId)))
                    {
                        iCanBeRoot = false;
                    }
                }

                if (iCanBeRoot)
                {
                    if (iRes.Data.Count > 1)
                    {
                        OnFoundRoot(Brain.Current[iRes.RelationshipLink], obj);
                    }
                    else
                    {
                        ulong iChild;
                        using (var iList = iRes.Result.Children) iChild = iList[0];
                        OnFoundPossibleRoot(Brain.Current[iRes.RelationshipLink], obj, Brain.Current[iChild]);
                    }
                }
            }
        }

        /// <summary>Checks if the synset is a root for the specified relations and raises the event if so.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="linkId">The link id.</param>
        /// <param name="synsed">The synsed.</param>
        private void CheckRoot(NeuronCluster obj, ulong rel, int linkId, int synsed)
        {
            var iCheckRootAd = new wordnetDataSetTableAdapters.CheckRootTableAdapter();
            int iRevCount;
            int iCount;
            var iTemp = iCheckRootAd.ReverseSemCount(linkId, synsed);
            iRevCount = (iTemp != null) ? (int)iTemp : 0;

            iTemp = iCheckRootAd.ReverseSyntaxCount(linkId, synsed);
            iRevCount += (iTemp != null) ? (int)iTemp : 0;

            iTemp = iCheckRootAd.SemCount(linkId, synsed);
            iCount = (iTemp != null) ? (int)iTemp : 0;

            iTemp = iCheckRootAd.SyntaxCount(linkId, synsed);
            iCount += (iTemp != null) ? (int)iTemp : 0;

            if (iCount > 0 && iRevCount == 0)
            {
                OnFoundRoot(Brain.Current[rel], obj);
            }
        }

        /// <summary>Checks if there is another object that has the specified object as a child for one of the recursive relationships,
        ///     if this is not the case, the object is promoted to a root in the 'is a' relationship. This is done because
        ///     otherwise,
        ///     the pos value, wordnetmeaning, ... get lost during export/import of xml data.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="synsetid">The synsetid.</param>
        private void CheckRootForNoRel(NeuronCluster obj, int synsetid)
        {
            var iWordsAdapter = new wordnetDataSetTableAdapters.CheckRootTableAdapter();

            var iTemp = iWordsAdapter.GetNrRelationshipsFor(synsetid);
            var iCount = (iTemp != null) ? (int)iTemp : 0;
            if (iCount == 0)
            {
                var iLinkDef = GetLinkDef("hyponym", true);
                OnFoundRoot(Brain.Current[iLinkDef], obj);
            }
        }

        /// <summary>Creates a NeuronCluster and assigns it the specified synsetID. The clusterr doesn't have a meaning.</summary>
        /// <remarks>The result cluster should be used as an object cluster.</remarks>
        /// <param name="synsetid">The synsetid to assign to the cluster.</param>
        /// <returns>A cluster with a link to an intneuron, both registered.</returns>
        private NeuronCluster CreateSynsetCluster(int synsetid)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            IntNeuron iIntNeuron = null;
            fIntValuesLock.EnterUpgradeableReadLock();
            try
            {
                ulong iId;
                if (fIntValues.TryGetValue(synsetid, out iId) == false)
                {
                    iIntNeuron = NeuronFactory.GetInt(synsetid);
                    Brain.Current.Add(iIntNeuron);
                    fIntValuesLock.EnterWriteLock();
                    try
                    {
                        fIntValues.Add(synsetid, iIntNeuron.ID);
                    }
                    finally
                    {
                        fIntValuesLock.ExitWriteLock();
                    }
                }
                else
                {
                    iIntNeuron = Brain.Current[iId] as IntNeuron;
                }
            }
            finally
            {
                fIntValuesLock.ExitUpgradeableReadLock();
            }

            var iLink = Link.Create(iRes, iIntNeuron, (ulong)PredefinedNeurons.SynSetID);
            return iRes;
        }

        /// <summary>Create or gets all the textneurons and Compound word clusters that are synonyms of the
        ///     specified word. Compound word clusters are created for all synonyms that have multiple words.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <param name="caption">The caption.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        private System.Collections.Generic.IList<Neuron> GetSynonyms(int synsetId, out string caption)
        {
            var iSynonymsAdapter = new wordnetDataSetTableAdapters.SynonymsTableAdapter();
            var iSynonymsData = iSynonymsAdapter.GetData(synsetId);
            var iRes = new System.Collections.Generic.List<Neuron>();
            var iLbl = new System.Text.StringBuilder();
            foreach (var i in iSynonymsData)
            {
                if (i != iSynonymsData[0])
                {
                    // when not first item, add a ','
                    iLbl.Append(", ");
                }

                iLbl.Append(i.word);

                var iSyn = BrainHelper.GetNeuronForText(i.word);
                System.Diagnostics.Debug.Assert(iSyn != null);
                if (iRes.Contains(iSyn) == false)
                {
                    // skip doubles.
                    iRes.Add(iSyn);
                }
            }

            caption = iLbl.ToString();
            return iRes;
        }

        /// <summary>Imports all the words for which the currently searched word is a morph, and builds the correct
        ///     relationships. A morph is a word that means the same as another word, but in another form, for instance
        ///     conjugated verbs or certain adverbs, like buy/bought, swim/swam,...
        ///     These are stored in another table.</summary>
        /// <remarks>The reference between a morph and it's origal representation, are done through a link between the
        ///     PosGroups of the morph and the orinal, for the pos that is returned in the result query.
        ///     For instance, the verbgroup 'bought' for verbs, links to 'Buy' for verbs, using he 'Morph of' neuron
        ///     as meaning for the link.</remarks>
        /// <param name="value">The text.</param>
        /// <param name="text">The text.</param>
        private void ImportMorphs(Neuron value, string text)
        {
            // import morphs moet  anders gedaan worden: obj moet wijzen naar alle posgroupen van morphOf, niet 1.
            var iMorphs = new wordnetDataSetTableAdapters.MorphsTableAdapter();
            var iData = iMorphs.GetData(text);
            foreach (var i in iData)
            {
                if (fStopRequested)
                {
                    throw new StopImportException(); // if a stop was requested, do this immediatly so the process stops
                }

                ImportMorphOf(value, i.MorphOf, i.pos, text);
            }
        }

        /// <summary>Imports all the words for a single synset and creates a single object for that synset. All relations
        ///     are also iported.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <param name="word">The word.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="sense">The sense.</param>
        /// <param name="def">The WSD.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ImportSynset(int synsetId, string word, string pos, int sense, string def)
        {
            string icaption;
            var iSynonyms = GetSynonyms(synsetId, out icaption);

            // this will also return the textneuron that we are searching for, since it is in the synonyms list.
            var iObj = FindObject(iSynonyms, synsetId);
            var iText = BrainHelper.GetNeuronForText(word);

            // we need this to find the posgroup: the word always stays the same, so the textneuron is used in a couple of posgroups.
            if (iObj == null)
            {
                iObj = CreateObject(iSynonyms, synsetId);
            }

            var iPosGroup = BrainHelper.FindPOSGroup(iText, GetPos(pos));
            if (iPosGroup == null)
            {
                var iList = new System.Collections.Generic.List<Neuron>();
                iList.Add(iText);
                iList.Add(iObj);
                iPosGroup = BrainHelper.CreatePOSGroup(iList, GetPos(pos));
                OnPOSGroupCreated(word, iPosGroup, pos);
            }
            else
            {
                var iChildren = iPosGroup.ChildrenW;
                iChildren.Lock(iObj);
                try
                {
                    if (iChildren.ContainsUnsafe(iObj) == false)
                    {
                        iChildren.AddUnsafe(iObj);
                    }
                }
                finally
                {
                    iChildren.Unlock(iObj);
                    iChildren.Dispose();
                }
            }

            StoreSense(iObj, iText, sense);
            StoreDefinition(iObj, def);
            BuildRelationships(iObj, synsetId, word, pos);
            OnObjectCreated(icaption, iObj, synsetId, def);
            return iObj;
        }

        /// <summary>Checks if there are any Verb constructions regular expressions defined, and if so, if any returns a match
        ///     for the specified text neuron. If this is the case, we build some extra links and possibly neurons, to indicate
        ///     the verb relationship.</summary>
        /// <remarks>This is required for the following reason: suppose you have the conjugated verb 'working'. This can be found
        ///     in the wordnet database, but it doesn't provide any info for it as a verb. So we need to build it our own.
        ///     If there is a regular expression that says to extract the verb 'work' out of 'working', than we build the
        ///     extra link between 'work' (which possibly also needs to be created) and the newly created posgroup
        ///     for 'working' as a verb.</remarks>
        /// <param name="value">The text.</param>
        /// <param name="text">The text.</param>
        private void ImportVerbConstructs(Neuron value, string text)
        {
            if (Regexes != null)
            {
                foreach (var i in Regexes)
                {
                    if (fStopRequested)
                    {
                        throw new StopImportException();

                        // if a stop was requested, do this immediatly so the process stops
                    }

                    ImportFromRegexDef(value, i, text);
                }
            }
        }

        /// <summary>Searches the specified text on wordNet and creates all the brain neurons for the result, or indicates
        ///     that nothing was found.</summary>
        /// <remarks>Because this function usually gets called when the network is running, it is possible that the same word is
        ///     requested from
        ///     different threads at the same time.  Because an import can take a bit of time, we need to prevent that the same
        ///     word isn't
        ///     requested a second time while already being processed.  This is done through a dictionary.</remarks>
        /// <param name="value">The text to search. This should always be a single word without spaces (no compound
        ///     words allowed), these are automically loaded for single words.</param>
        /// <param name="text">The text.</param>
        private void SearchOnWordNet(Neuron value, string text)
        {
            if (fStopRequested)
            {
                throw new StopImportException();

                // if a stop was requested, do this immediatly so the process stops, since this function can be called recursively (by subfunctions), we try to stop at the start
            }

            System.Threading.ManualResetEvent iEvent = null;
            lock (fCurrentSearches)
            {
                // make certain only 1 thread can access the dict at the same time.
                if (fCurrentSearches.TryGetValue(text, out iEvent) == false)
                {
                    // if not in dict, add it now
                    fCurrentSearches.Add(text, new System.Threading.ManualResetEvent(false));
                }
            }

            if (iEvent == null)
            {
                // Log.LogInfo("WordNetSin.SearchOnWordNet", string.Format("Learning about '{0}' from WordNet.", text));
                OnStarted();
                try
                {
                    UnsafeSearchOnWordNet(value, text);
                }
                finally
                {
                    OnFinished();
                    lock (fCurrentSearches)
                    {
                        // make certain only 1 thread can access the dict at the same time.
                        iEvent = fCurrentSearches[text];
                        fCurrentSearches.Remove(text);
                    }

                    if (iEvent != null)
                    {
                        iEvent.Set(); // let the other threads continue.
                    }
                }
            }
            else
            {
                LogService.Log.LogInfo(
                    "WordNetSin.SearchOnWordNet", 
                    string.Format("Waiting untill '{0}' has been learned from WordNet.", value));
                iEvent.WaitOne();
            }
        }

        /// <summary>Stores the example sentence together with the object raises the event that the object was created.
        ///     We do this here cause this is most econmic for the system: less sql queries.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="def">The WSD.</param>
        private void StoreDefinition(NeuronCluster obj, string def)
        {
            if (def != null && obj.FindFirstOut(WordnetMeaning.ID) == null)
            {
                // make certain that we don't create the link 2 times (for multiple senses = synonyms).
                var iVal = NeuronFactory.GetText(def); // we don't add to the dict to save space.
                Brain.Current.Add(iVal);
                Link.Create(obj, iVal, WordnetMeaning);
            }
        }

        /// <summary>Makes certain that text neuron stores a ref to the object (in it's 'clusteredBy' list) at
        ///     the index position determined by the sense. We do this to make certain that the search
        ///     algorithms can pick up the sense order as defined in wordnet, by simply checking the sequence of parents.</summary>
        /// <param name="obj"></param>
        /// <param name="text"></param>
        /// <param name="sense">The sense.</param>
        private void StoreSense(NeuronCluster obj, Neuron text, int sense)
        {
            IDListAccessor iClusteredBy = text.ClusteredByW;
            iClusteredBy.Lock(); // a move doesn't require the other neurons to be locked, only the list itself.
            try
            {
                var iIndex = iClusteredBy.IndexOfUnsafe(obj.ID);
                if (sense != iIndex)
                {
                    iClusteredBy.MoveUnsave(iIndex, sense);
                }
            }
            finally
            {
                iClusteredBy.Unlock();
                iClusteredBy.Dispose();
            }
        }

        /// <summary>Gets all the textneurons and Compound word clusters that are synonyms of the
        ///     specified word. If a textneuron or compound word is missing, it is not created
        ///     but null is returned.</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        private System.Collections.Generic.IList<Neuron> TryFindSynonyms(int synsetId)
        {
            var iSynonymsAdapter = new wordnetDataSetTableAdapters.SynonymsTableAdapter();
            var iSynonymsData = iSynonymsAdapter.GetData(synsetId);
            var iRes = new System.Collections.Generic.List<Neuron>();
            foreach (var i in iSynonymsData)
            {
                var iSyn = BrainHelper.TryFindCompoundWord(i.word);
                if (iSyn != null)
                {
                    iRes.Add(iSyn);
                }
                else
                {
                    return null;
                }
            }

            return iRes;
        }

        /// <summary>see <see cref="WordNetSin.SearchOnWordNet"/></summary>
        /// <param name="value">The text.</param>
        /// <param name="text">The text.</param>
        /// <returns>all the objects that were created.</returns>
        private System.Collections.Generic.IList<NeuronCluster> UnsafeSearchOnWordNet(Neuron value, string text)
        {
            if (fStopRequested)
            {
                throw new StopImportException(); // if a stop was requested, do this immediatly so the process stops
            }

            var iRes = new System.Collections.Generic.List<NeuronCluster>();
            var iIncludeCompound = IncludeCompoundWords;

            // we store this value before we begin cause the input can take some time and we want the value as it was when we started.
            var iRows = GetWordInfoFor(text);
            foreach (var i in iRows)
            {
                var iObj = ImportSynset(i.synsetid, i.Word, i.pos, i.Sense, i.definition);
                if (iIncludeCompound)
                {
                    BuildCompoundWords(iObj, i);
                }

                iRes.Add(iObj);
            }

            ImportVerbConstructs(value, text);
            ImportMorphs(value, text);
            return iRes;
        }

        #region Object

        /// <summary>Tries to find the neuron cluster that represents the object for the specified text and synsetid.
        ///     If it doesn't find something, it returns null. No neurons are created during the search process, including
        ///     no textneurons for any of the synonyms (this used to be a problem).</summary>
        /// <param name="synsedId">The synsed id.</param>
        /// <returns>A neuroncluster that represents the object, or null if nothing is found.</returns>
        public NeuronCluster FindObject(int synsedId)
        {
            var iSyns = TryFindSynonyms(synsedId);
            if (iSyns != null && iSyns.Count > 0)
            {
                return FindObject(iSyns[0], synsedId);
            }

            return null;
        }

        /// <summary>Gets the cluster that represents the object with the specified synset id.  If it can't find it, one is
        ///     created.</summary>
        /// <param name="content">The content.</param>
        /// <param name="synsetId">The synset Id.</param>
        /// <returns>A neuroncluster that represents the object</returns>
        public NeuronCluster GetObject(System.Collections.Generic.IList<Neuron> content, int synsetId)
        {
            NeuronCluster iSyn = null;
            if (content != null)
            {
                // this can be null if its not a TextNeuron.  Can happen if there is some sort of data inconsistency.
                iSyn = FindObject(content, synsetId);
                if (iSyn == null)
                {
                    iSyn = CreateObject(content, synsetId);
                }
            }

            return iSyn;
        }

        /// <summary>Gets the cluster that represents the object with the specified synset id.  If it can't find it, one is
        ///     created.</summary>
        /// <param name="text">The text.</param>
        /// <param name="synsetId">The synset id.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetObject(string text, int synsetId)
        {
            var iText = BrainHelper.GetNeuronForText(text);
            return GetObject(iText, synsetId);
        }

        /// <summary>Gets the cluster that represents the object with the specified synset id.  If it can't find it, one is
        ///     created.</summary>
        /// <param name="text">The text to search for as a neuron.</param>
        /// <param name="synsetId">The synset Id.</param>
        /// <returns>A neuroncluster that represents the object</returns>
        public NeuronCluster GetObject(Neuron text, int synsetId)
        {
            NeuronCluster iSyn = null;
            if (text != null)
            {
                // this can be null if its not a TextNeuron.  Can happen if there is some sort of data inconsistency.
                iSyn = FindObject(text, synsetId);
                if (iSyn == null)
                {
                    iSyn = CreateObject(text, synsetId);
                }
            }

            return iSyn;
        }

        /// <summary>Creates a new cluster with meaning 'same' and adds a new text neuron to it.</summary>
        /// <remarks>Doesn't directly add to textsin dict, but is done indirectly.</remarks>
        /// <param name="text">The text that should be assigned to the text neuron that is created.</param>
        /// <param name="synsetid">The synsetid of the sense. This is also stored in the object so that we can find it again.  It also
        ///     tags it as comming from wordnet.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster CreateObject(string text, int synsetid)
        {
            var iText = BrainHelper.GetNeuronForText(text);
            return CreateObject(iText, synsetid);
        }

        /// <summary>Creates a new cluster with meaning 'object' and adds the neuron or compound word cluster to it.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="synsetid">The synsetid of the sense. This is also stored in the object so that we can find it again.  It also
        ///     tags it as comming from wordnet.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        /// <remarks>Also makes certain that the textneuron is stored in the dictionary of the textsin so that it can be used.</remarks>
        private NeuronCluster CreateObject(Neuron neuron, int synsetid)
        {
            var iRes = CreateSynsetCluster(synsetid);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iChildren = iRes.ChildrenW) iChildren.Add(neuron);
            return iRes;
        }

        /// <summary>Creates a new cluster with meaning 'object' and adds all the neuron or compound word cluster to it.</summary>
        /// <param name="neurons">The neurons.</param>
        /// <param name="synsetid">The synsetid.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster CreateObject(System.Collections.Generic.IList<Neuron> neurons, int synsetid)
        {
            var iRes = CreateSynsetCluster(synsetid);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iChildren = iRes.ChildrenW) iChildren.AddRange(neurons);
            return iRes;
        }

        /// <summary>Tries to find the neuron cluster that represents the object for the specified text neuron and synsetid.
        ///     If it doesn't find something, it returns null.</summary>
        /// <param name="text">The text.</param>
        /// <param name="synsedId">The synsed id.</param>
        /// <returns>A neuroncluster that represents the object, or null if nothing is found.</returns>
        private NeuronCluster FindObject(Neuron text, int synsedId)
        {
            if (text != null && text.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusteredBy;
                using (var iList = text.ClusteredBy) iClusteredBy = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iRes = (from u in iClusteredBy
                                let iSynset = u.FindFirstOut((ulong)PredefinedNeurons.SynSetID) as IntNeuron
                                where
                                    u != null && u.Meaning == (ulong)PredefinedNeurons.Object && iSynset != null
                                    && iSynset.Value == synsedId
                                select u).FirstOrDefault();
                    return iRes;
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusteredBy);
                }
            }

            return null;
        }

        /// <summary>Tries to find the neuron cluster that represents the object for the specified neurons (textneurons and/or compound
        ///     words) and synsetid.
        ///     If it doesn't find something, it returns null.</summary>
        /// <param name="content">The content.</param>
        /// <param name="synsedId">The synsed id.</param>
        /// <returns>A neuroncluster that represents the object, or null if nothing is found.</returns>
        private NeuronCluster FindObject(System.Collections.Generic.IList<Neuron> content, int synsedId)
        {
            if (content != null && content[0].ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iTemp;
                using (var iList = content[0].ClusteredBy) iTemp = iList.ConvertTo<NeuronCluster>();

                // copy to temp list so that we don't block the other threads to long (otherwise we get deadlocks).
                foreach (var i in iTemp)
                {
                    if (i != null && i.Meaning == (ulong)PredefinedNeurons.Object)
                    {
                        var iSynset = i.FindFirstOut((ulong)PredefinedNeurons.SynSetID) as IntNeuron;
                        if (iSynset != null && iSynset.Value == synsedId)
                        {
                            return i;
                        }
                    }
                }
            }

            return null;
        }

        #endregion Object

        #region Relationships

        // private bool HasRelationship(wordnetDataSet.RelationshipsRow rel, int synsetid)
        // {
        // RelatedWordsTableAdapter iWordsAdapter = new RelatedWordsTableAdapter();
        // wordnetDataSet.RelatedWordsDataTable iWordsData = iWordsAdapter.GetData(rel.linkid, synsetid);
        // if (iWordsData.Count > 0)
        // return true;
        // else
        // {
        // iWordsData = iWordsAdapter.GetLexDataBy(synsetid, rel.linkid);
        // return iWordsData.Count > 0;
        // }
        // }
        /// <summary>Creates a relationship between the 2 neurons with the specified relationship using the wordnet structure.</summary>
        /// <remarks><para>If the 'from' neuron already has a link to a cluster for the specified relationship, the item is simply added,
        ///         otherwise, a new cluster is created for the relationship and the item is added.
        ///         If the item is already in the list, it is not added again.</para>
        /// <para>Doesn't trigger any wordnet events (regular brain events are triggered like normal).</para>
        /// <para>To remove a thesaurus relationship, use <see cref="WordNetSin.RemoveRelationship"/></para>
        /// </remarks>
        /// <param name="start">From part of pseudo link.</param>
        /// <param name="to">To part of pseudo link.</param>
        /// <param name="relationship">The meaning part of pseudo link.</param>
        public void CreateRelationship(Neuron start, Neuron to, Neuron relationship)
        {
            NeuronCluster iFound;
            if (start.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                using (var iList = start.LinksOut) iLinks.AddRange(iList); // need to prevent deadlocks with cache.
                iFound = (from i in iLinks
                          let iCluster = i.To as NeuronCluster
                          where i.MeaningID == relationship.ID && iCluster.Meaning == relationship.ID
                          select iCluster).FirstOrDefault();
                Factories.Default.LinkLists.Recycle(iLinks);
            }
            else
            {
                iFound = null;
            }

            if (iFound == null)
            {
                iFound = NeuronFactory.GetCluster();
                iFound.Meaning = relationship.ID;
                Brain.Current.Add(iFound);
                var iLink = new Link(iFound, start, relationship);
            }

            var iChildren = iFound.ChildrenW;
            iChildren.Lock(to);
            try
            {
                if (iChildren.ContainsUnsafe(to) == false)
                {
                    iChildren.AddUnsafe(to);
                }
            }
            finally
            {
                iChildren.Unlock(to);
                iChildren.Dispose();
            }
        }

        /// <summary>Removes a relationship between the 2 neurons, using the specified relationship neuron, in a manner consistent with
        ///     the thesaurus. (so this is is for removing thesaurus relationships).</summary>
        /// <param name="start">The start.</param>
        /// <param name="to">To.</param>
        /// <param name="relationship">The relationship.</param>
        public void RemoveRelationship(Neuron start, Neuron to, Neuron relationship)
        {
            NeuronCluster iFound;
            if (start.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                using (var iList = start.LinksOut) iLinks.AddRange(iList); // need to prevent deadlocks with cache.
                iFound = (from i in iLinks
                          let iCluster = i.To as NeuronCluster
                          where i.MeaningID == relationship.ID && iCluster.Meaning == relationship.ID
                          select iCluster).FirstOrDefault();
                Factories.Default.LinkLists.Recycle(iLinks);
            }
            else
            {
                iFound = null;
            }

            if (iFound != null)
            {
                bool iDelete;
                var iList = iFound.ChildrenW;
                iList.Lock(to);
                try
                {
                    iList.RemoveUnsafe(to);
                    iDelete = iList.CountUnsafe == 0;
                }
                finally
                {
                    iList.Unlock(to);
                    iList.Dispose();
                }

                if (iDelete)
                {
                    Brain.Current.Delete(iFound);
                }
            }
        }

        /// <summary>Creates a cluster and fills it with all the objects that are related to it as specified in the rel arg.</summary>
        /// <param name="args">The arguments to build the resationship.</param>
        private void BuildRelationship(RelationshipArgs args)
        {
            NeuronCluster iCluster;
            if (args.Result == null)
            {
                // it could be that we simply need to complete an already existing list.
                iCluster = NeuronFactory.GetCluster();
                Brain.Current.Add(iCluster);
                args.Result = iCluster;
            }
            else
            {
                iCluster = args.Result;
            }

            iCluster.Meaning = args.RelationshipLink; // always make certain of the correct meaning.
            foreach (var iRow in args.Data)
            {
                if (fStopRequested)
                {
                    throw new StopImportException(); // if a stop was requested, do this immediatly so the process stops
                }

                string iLbl;

                // don't do anything with this in here, related words aren't labeled yet, only when they get imported.
                var iSynonyms = GetSynonyms(iRow.synsetid, out iLbl);
                var iWord = GetObject(iSynonyms, iRow.synsetid);
                System.Diagnostics.Debug.Assert(iWord != null);
                var iList = iCluster.ChildrenW;
                iList.Lock(iWord);
                try
                {
                    if (iList.ContainsUnsafe(iWord) == false)
                    {
                        // we check that the cluster doesn't already have the item.  This somehow happens, probably because some items are double checked.
                        iList.AddUnsafe(iWord);
                    }
                }
                finally
                {
                    iList.Unlock(iWord);
                    iList.Dispose();
                }
            }
        }

        /// <summary>Checks if the specified object contains a link to another object with the specified meaning + that
        ///     points to an object that links to the specified synsetid.</summary>
        /// <param name="item">The item to start the search from.</param>
        /// <param name="meaning">The meaning to search for on the item.</param>
        /// <param name="synsetid">The synsetid that should be found on the 'To' neuron, of the 'meaning' link.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool Exists(Neuron item, ulong meaning, int synsetid)
        {
            using (var iLinks = item.LinksOut)
            {
                var iList = from i in iLinks where i.MeaningID == meaning select i.ToID;
                foreach (var i in iList)
                {
                    var iItem = Brain.Current[i];
                    var iId = iItem.FindFirstOut((ulong)PredefinedNeurons.SynSetID) as IntNeuron;
                    if (iId != null && iId.Value == synsetid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Checks if the relationship cluster already exists, if so, this is returned, otherwise a new one is created.</summary>
        /// <param name="rel">The relationship for which to get a cluster</param>
        /// <param name="synsetid">The synsed id to use.</param>
        /// <returns>The <see cref="RelationshipArgs"/>.</returns>
        private RelationshipArgs GetRelationship(RelationshipsRow rel, int synsetid)
        {
            var iArgs = new RelationshipArgs();
            var iWordsAdapter = new wordnetDataSetTableAdapters.RelatedWordsTableAdapter();
            if (LEXLINKS.Contains(rel.linkid) == false)
            {
                // we know which linkids are in the lexlinkref table and which aren't, so we don't need to lookup in both lists.
                var iWordsData = iWordsAdapter.GetData(rel.linkid, synsetid);
                if (iWordsData.Count > 0)
                {
                    iArgs.Relation = rel;
                    iArgs.SynsetId = synsetid;
                    iArgs.Data = iWordsData;
                    iArgs.RelationshipLink = GetLinkDef(rel.name, rel.recurses == "Y");
                    BuildRelationship(iArgs);
                    return iArgs;
                }
            }
            else
            {
                var iWordsData = iWordsAdapter.GetLexDataBy(synsetid, rel.linkid);
                if (iWordsData.Count > 0)
                {
                    iArgs.Relation = rel;
                    iArgs.SynsetId = synsetid;
                    iArgs.Data = iWordsData;
                    iArgs.RelationshipLink = GetLinkDef(rel.name, rel.recurses == "Y");
                    BuildRelationship(iArgs);
                    return iArgs;
                }
            }

            return null;
        }

        /// <summary>Updates the content of an already existing relationship cluster: it adds all the items that are still
        ///     missing.</summary>
        /// <param name="rel">The rel.</param>
        /// <param name="result">The result.</param>
        /// <param name="synsetid">The synsetid.</param>
        private void UpdateRelationship(RelationshipsRow rel, NeuronCluster result, int synsetid)
        {
            var iArgs = new RelationshipArgs { Result = result };
            var iWordsAdapter = new wordnetDataSetTableAdapters.RelatedWordsTableAdapter();
            if (LEXLINKS.Contains(synsetid) == false)
            {
                // we know which linkids are in the lexlinkref table and which aren't, so we don't need to lookup in both lists.
                var iWordsData = iWordsAdapter.GetData(rel.linkid, synsetid);
                if (iWordsData.Count > 0)
                {
                    iArgs.Relation = rel;
                    iArgs.SynsetId = synsetid;
                    iArgs.Data = iWordsData;
                    iArgs.RelationshipLink = GetLinkDef(rel.name, rel.recurses == "Y");
                    BuildRelationship(iArgs);

                    // simply build the relationship, it will only add what's missing and won't create the result cluster if it already exists.
                }
            }
            else
            {
                var iWordsData = iWordsAdapter.GetLexDataBy(synsetid, rel.linkid);
                if (iWordsData.Count > 0)
                {
                    iArgs.Relation = rel;
                    iArgs.SynsetId = synsetid;
                    iArgs.Data = iWordsData;
                    iArgs.RelationshipLink = GetLinkDef(rel.name, rel.recurses == "Y");
                    BuildRelationship(iArgs);
                }
            }
        }

        #endregion Relationships

        #region data retrieving

        /// <summary>Gets some extra info for a synsetID, like the description and the part of speech (which
        ///     is returned).</summary>
        /// <param name="synsetId">The synset id.</param>
        /// <param name="definition">The description.</param>
        /// <returns>The part of speech value.</returns>
        public string GetSynsetInfoFor(int synsetId, out string definition)
        {
            var iAd = new wordnetDataSetTableAdapters.synsetInfoTableAdapter();
            var iData = iAd.GetData(synsetId);
            if (iData.Count > 0)
            {
                definition = iData[0].definition;
                return iData[0].pos;
            }

            throw new System.InvalidOperationException(string.Format("unknown synset id: {0}", synsetId));
        }

        /// <summary>Gets all the different meanings for the specified word.</summary>
        /// <param name="text">The text.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<WordInfoRow> GetWordInfoFor(string text)
        {
            var iWordInfoAdapter = new wordnetDataSetTableAdapters.WordInfoTableAdapter();
            return from wordnetDataSet.WordInfoRow i in iWordInfoAdapter.GetData(text) select i;
        }

        /// <summary>The get word info for.</summary>
        /// <param name="text">The text.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<WordInfoRow> GetWordInfoFor(
            string text, 
            string pos)
        {
            var iWordInfoAdapter = new wordnetDataSetTableAdapters.WordInfoTableAdapter();
            return from wordnetDataSet.WordInfoRow i in iWordInfoAdapter.GetDataForPOS(text, pos) select i;
        }

        #endregion data retrieving

        #endregion Functions
    }
}