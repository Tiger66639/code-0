// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetChannel.cs" company="">
//   
// </copyright>
// <summary>
//   Stores all the data generated from wordnet + registers it with the designer so that they are correctly labeled.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Stores all the data generated from wordnet + registers it with the designer so that they are correctly labeled.
    /// </summary>
    public class WordNetChannel : CommChannel, System.Windows.IWeakEventListener
    {
        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="WordNetChannel"/> class. 
        ///     Initializes a new instance of the <see cref="WordNetData"/> class.</summary>
        public WordNetChannel()
        {
            LastImportedWord = 0;
            fChildren = new Data.ObservedCollection<WordNetItemGroup>(this);
            fEventMonitor = new WordNetChannelEventMonitor(this);

            WordNetSin.Default.IncludeDescription = true;
            WordNetSin.Default.IncludeCompoundWords = IncludeCompoundWords;
            WordNetSin.Default.RegExFile = Properties.Settings.Default.RegExFile;
        }

        #endregion

        #region Fields

        /// <summary>The f event items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<WordNetEventItem> fEventItems =
            new System.Collections.ObjectModel.ObservableCollection<WordNetEventItem>();

        /// <summary>The f children.</summary>
        private readonly Data.ObservedCollection<WordNetItemGroup> fChildren;

        /// <summary>The f importer.</summary>
        private WordNetImporter fImporter;

        /// <summary>The f selected relationship.</summary>
        private int fSelectedRelationship;

        /// <summary>The f current text.</summary>
        private string fCurrentText;

        /// <summary>The f include compound words.</summary>
        private bool fIncludeCompoundWords;

        /// <summary>
        ///     We keep a ref to the event monitor, in this object, to keep it alive, because if there is no child item, the
        ///     event monitor is no where referenced in the event manager.
        /// </summary>
        private readonly WordNetChannelEventMonitor fEventMonitor;

        /// <summary>
        ///     gets the name of the wordnet db file that can be supplied with the app.
        /// </summary>
        private const string WORDNETDBFILE = "wordnet.mdf";

        #endregion

        #region prop

        #region EventItems

        /// <summary>
        ///     Gets the list of data that was stored for each event that occured in the WordNetSin.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<WordNetEventItem> EventItems
        {
            get
            {
                return fEventItems;
            }
        }

        #endregion

        #region Children

        /// <summary>Gets the children.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<WordNetItemGroup> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion

        #region SelectedRelationship

        /// <summary>
        ///     Gets/sets the ID of the currently selected relationhip, which determins the children that are displayed.
        /// </summary>
        /// <remarks>
        ///     This is not the index, but the actual value.
        /// </remarks>
        public int SelectedRelationship
        {
            get
            {
                return fSelectedRelationship;
            }

            set
            {
                if (value != fSelectedRelationship)
                {
                    fSelectedRelationship = value;
                    OnPropertyChanged("SelectedRelationship");
                    System.Action<int> iSetRecursiveAsync = SetRecursiveAsync;
                    iSetRecursiveAsync.BeginInvoke(value, null, null);

                        // we call async cause this call can take very long if the wordnet db isn't loaded yet.
                }
            }
        }

        /// <summary>Sets the IsrecursiveRelationship. Used for async calls.</summary>
        /// <param name="value">The value.</param>
        private void SetRecursiveAsync(int value)
        {
            var iRelDB = new wordnetDataSetTableAdapters.RelationshipsTableAdapter();
            IsRecursiveRelationship = iRelDB.IsRecursive(value) == "Y" ? true : false;
            if (LastSearchText != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(ReloadData));

                    // needs to be loaded from ui thread, otherwise it never happens.
            }
        }

        #endregion

        #region IsRecursiveRelationship

        /// <summary>
        ///     Gets wether the currently selected relationship is recusrive
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsRecursiveRelationship { get; internal set; }

        #endregion

        #region Relationships

        /// <summary>
        ///     Gets the list of available relationships.  This is the entire list of relationships as defined by wordnet,
        ///     augmented
        ///     with the list of synonyms (same words in the synset).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<WordNetRelationship> Relationships
        {
            get
            {
                var iRelAdapater = new wordnetDataSetTableAdapters.RelationshipsTableAdapter();
                RelationshipsDataTable iRelData = iRelAdapater.GetData();
                System.Collections.Generic.List<WordNetRelationship> iRes =
                    (from wordnetDataSet.RelationshipsRow i in iRelData
                     select
                         new WordNetRelationship
                             {
                                 ID = i.linkid, 
                                 Name = i.name, 
                                 Recurses = i.recurses == "Y" ? true : false
                             }).ToList();
                return iRes;
            }
        }

        #endregion

        #region IncludeCompoundWords

        /// <summary>
        ///     Gets/sets wether to include compound words in the import and search, or not.
        /// </summary>
        public bool IncludeCompoundWords
        {
            get
            {
                return fIncludeCompoundWords;
            }

            set
            {
                if (fIncludeCompoundWords != value)
                {
                    fIncludeCompoundWords = value;
                    OnPropertyChanged("IncludeCompoundWords");
                    WordNetSin.Default.IncludeCompoundWords = value;
                    if (string.IsNullOrEmpty(LastSearchText) == false)
                    {
                        LoadDataFor(LastSearchText);

                            // redo the search so that the items are removed/included in the display as well.
                    }
                }
            }
        }

        #endregion

        #region SearchText

        /// <summary>
        ///     Gets the text that is being searched for currently.
        /// </summary>
        /// <value>The search text.</value>
        public string LastSearchText { get; private set; }

        #endregion

        #region CurrentText

        /// <summary>
        ///     Gets/sets the text that has currently been filled into the searchtextbox. This is provided so that the
        ///     text remains visible after changing the tab.
        /// </summary>
        public string CurrentText
        {
            get
            {
                return fCurrentText;
            }

            set
            {
                fCurrentText = value;
                OnPropertyChanged("CurrentText");
            }
        }

        #endregion

        #region Importer

        /// <summary>
        ///     Gets/sets the wordnet importer that is currently importing the entire db. When null, there is no import operation
        ///     going on.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public WordNetImporter Importer
        {
            get
            {
                return fImporter;
            }

            set
            {
                fImporter = value;
                OnPropertyChanged("Importer");
                OnPropertyChanged("HasImporter");
            }
        }

        #endregion

        #region HasImporter

        /// <summary>
        ///     Gets the wether there is currently a global importer (for complete db) alive.
        /// </summary>
        public bool HasImporter
        {
            get
            {
                return fImporter != null;
            }
        }

        #endregion

        #region LastImportedWord

        /// <summary>
        ///     Gets/sets the index of the word that was last partially imported by a WordnetImporter. This allows us to resume
        ///     from a previously halted import. Note that the word at the specified index position, will be the first to
        ///     import in the new import.
        /// </summary>
        /// <remarks>
        ///     Init value is 0, to indicate that the first word was not yet completely imported.
        /// </remarks>
        public int LastImportedWord { get; set; }

        #endregion

        #region IsDBPresent

        /// <summary>
        ///     Gets the value that indicates if the wordnet database is present or not.
        ///     This is used to switch certain commands on/off depending on wether you have wordnet available or not.
        /// </summary>
        public static bool IsDBPresent
        {
            get
            {
                var iPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                return System.IO.File.Exists(System.IO.Path.Combine(iPath, WORDNETDBFILE));
            }
        }

        #endregion

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>true if the listener handled the event. It is considered an error by the<see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the
        ///     listener does not handle. Regardless, the method should return false if it receives an event that it does not
        ///     recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(ObjectCreatedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, NewObjectEventArgs>(OnObjectCreated), 
                    sender, 
                    e);
                return true;
            }

            if (managerType == typeof(POSGroupCreatedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, PosGroupEventArgs>(OnPosGroupCreated), 
                    sender, 
                    e);
                return true;
            }

            if (managerType == typeof(RelationshipCreatedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, NewRelationshipEventArgs>(OnRelationshipCreated), 
                    sender, 
                    e);
                return true;
            }

            if (managerType == typeof(RelationshipTypeCreatedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, NewMetaTypeEventArgs>(OnRelationshipTypeCreated), 
                    sender, 
                    e);
                return true;
            }

            if (managerType == typeof(WordNetStartedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(OnSearchStarted));
                return true;
            }

            if (managerType == typeof(WordNetFinishedEventManager))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(OnSearchFinished));
                return true;
            }

            return false;
        }

        /// <summary>The register monitor for.</summary>
        /// <param name="id">The id.</param>
        internal void RegisterMonitorFor(ulong id)
        {
            fEventMonitor.AddId(id);
        }

        /// <summary>Called when a neuron is changed: added/removed/replaced.  Need to ask each child to check the change and update
        ///     what needs
        ///     updating.</summary>
        /// <param name="id">The id.</param>
        internal void OnNeuronRemoved(ulong id)
        {
            foreach (var i in Children)
            {
                i.UpdateForDelete(id);
            }
        }

        /// <summary>
        ///     Called when a search has started.
        /// </summary>
        private void OnSearchStarted()
        {
            BrainData.Current.LearnCount++;
        }

        /// <summary>
        ///     Called when a search is finished.
        /// </summary>
        private void OnSearchFinished()
        {
            BrainData.Current.LearnCount--;
        }

        /// <summary>The log item.</summary>
        /// <param name="iEvent">The i event.</param>
        private void LogItem(WordNetEventItem iEvent)
        {
            EventItems.Add(iEvent);
            while (EventItems.Count > 2000)
            {
                EventItems.RemoveAt(0);
            }
        }

        /// <summary>Called when a new relationship type was created.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.NewRelationshipTypeEventArgs"/> instance containing the event data.</param>
        private void OnRelationshipTypeCreated(object sender, NewMetaTypeEventArgs e)
        {
            var iId = e.Neuron.ID;
            if (BrainData.Current.DefaultMeaningIds.Contains(iId) == false)
            {
                BrainData.Current.DefaultMeaningIds.Add(iId);
            }

            BrainData.Current.NeuronInfo[iId].DisplayTitle = e.Name;
            var iEvent = new WordNetEventItem();
            iEvent.Text = "Relationship type created: " + e.Name;
            LogItem(iEvent);
        }

        /// <summary>Called when a new relationship created.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.NewRelationshipEventArgs"/> instance containing the event data.</param>
        private void OnRelationshipCreated(object sender, NewRelationshipEventArgs e)
        {
            var iEvent = new WordNetEventItem();
            var iStr = new System.Text.StringBuilder("Relationship created from: '");
            iStr.Append(BrainData.Current.NeuronInfo[e.Neuron.ID]);
            iStr.Append(" ' to: '");
            iStr.Append(BrainData.Current.NeuronInfo[e.Related.ID]);
            iStr.Append("' meaning: '");
            iStr.Append(BrainData.Current.NeuronInfo[e.Related.Meaning]);
            iStr.Append("'");
            iEvent.Text = iStr.ToString();
            LogItem(iEvent);
        }

        /// <summary>Called when [object created]. Also alks each child if it represents the newly created object.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.NewObjectEventArgs"/> instance containing the event data.</param>
        private void OnObjectCreated(object sender, NewObjectEventArgs e)
        {
            var iInfo = BrainData.Current.NeuronInfo[e.Neuron.ID];
            if (iInfo != null)
            {
                // this is possible when the wordnetsin was importing when the old project got cleared.
                iInfo.DisplayTitle = e.Value;
                iInfo.StoreDescription(e.Description); // set the description
                if (Settings.StorageMode == NeuronStorageMode.AlwaysStream)
                {
                    // if we stream to disk (doing a full import), save the data to disk.
                    BrainData.Current.NeuronInfo.Store.SaveData(iInfo);
                }

                var iEvent = new WordNetEventItem();
                iEvent.Text = "object created: " + e.Value;
                LogItem(iEvent);
                foreach (var i in Children)
                {
                    // update the channel's ui, to indicate that a word was loaded or not.
                    i.UpdateForNewObject(e.SynSetId);
                }
            }
        }

        /// <summary>The on pos group created.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnPosGroupCreated(object sender, PosGroupEventArgs e)
        {
            var iInfo = BrainData.Current.NeuronInfo[e.Neuron.ID];
            if (iInfo != null)
            {
                // this is possible when the wordnetsin was importing when the old project got cleared.
                iInfo.DisplayTitle = e.Value + "(" + e.POS + ")";
                if (Settings.StorageMode == NeuronStorageMode.AlwaysStream)
                {
                    // if we stream to disk (doing a full import), save the data to disk.
                    BrainData.Current.NeuronInfo.Store.SaveData(iInfo);
                }

                var iEvent = new WordNetEventItem();
                iEvent.Text = "PosGroup created: " + e.Value + "(" + e.POS + ")";
                LogItem(iEvent);
                foreach (var i in Children)
                {
                    if (i.GroupFor == e.Value && i.PosString == e.POS)
                    {
                        i.SetLoaded(e.Neuron);
                    }
                }
            }
        }

        #endregion

        #region Functions

        /// <summary>The set sin.</summary>
        /// <param name="sin">The sin.</param>
        protected internal override void SetSin(Sin sin)
        {
            if (Sin == null)
            {
                // only try to register event handlers 1 time, after the project has loaded so that we can register correctly.
                ObjectCreatedEventManager.AddListener(WordNetSin.Default, this);
                RelationshipCreatedEventManager.AddListener(WordNetSin.Default, this);
                RelationshipTypeCreatedEventManager.AddListener(WordNetSin.Default, this);
                WordNetStartedEventManager.AddListener(WordNetSin.Default, this);
                WordNetFinishedEventManager.AddListener(WordNetSin.Default, this);
                POSGroupCreatedEventManager.AddListener(WordNetSin.Default, this);
            }

            base.SetSin(sin);
        }

        /// <summary>Loads all the children of the specified word for the current relationship.</summary>
        /// <param name="text">The text.</param>
        public void LoadDataFor(string text)
        {
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // we don't want to undo the fact that items are being added or removed.
            try
            {
                Children.Clear();
                fEventMonitor.Clear();
                LastSearchText = text;

                    // store the last searched item, so we can do the search again if the selected relationship is changed.
                LoadWordInfo();
                LoadRelatedFromRegEx();
                LoadMorphsOf();
                LoadCompoundWordInfo();
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>The reload data.</summary>
        private void ReloadData()
        {
            foreach (var i in Children)
            {
                i.Children.Clear();
                i.LoadChildren();
            }
        }

        /// <summary>The load morphs of.</summary>
        private void LoadMorphsOf()
        {
            MorphsTableAdapter iMorphs = new MorphsTableAdapter();
            MorphsDataTable iData = iMorphs.GetData(LastSearchText);
            foreach (MorphsRow i in iData)
            {
                var iNew = new WordNetItemMorphs();
                iNew.GroupFor = i.MorphOf;
                iNew.POS = WordNetSin.GetPos(i.pos);
                iNew.PosString = i.pos;
                iNew.RelationshipID = (uint)PredefinedNeurons.MorphOf;
                Children.Add(iNew);
                iNew.LoadChildren();
            }
        }

        /// <summary>The load related from reg ex.</summary>
        private void LoadRelatedFromRegEx()
        {
            if (WordNetSin.Default.Regexes != null)
            {
                foreach (var i in WordNetSin.Default.Regexes)
                {
                    var iMatch = System.Text.RegularExpressions.Regex.Match(LastSearchText, i.RegEx);
                    if (iMatch.Success)
                    {
                        var iGroupFor = iMatch.Result("${result}");
                        if (Enumerable.Count(WordNetSin.Default.GetWordInfoFor(iGroupFor, i.POS)) > 0)
                        {
                            // we still need to check if we found an actual existing word, if we didn't there is no group.
                            var iGroup = new WordNetItemConjugations();
                            iGroup.GroupFor = iGroupFor;
                            iGroup.RelationshipID = i.ID;
                            iGroup.POS = WordNetSin.GetPos(i.POS);
                            iGroup.PosString = i.POS;
                            iGroup.RegexDef = i;
                            Children.Add(iGroup);
                            iGroup.LoadChildren();
                        }
                    }
                }
            }
        }

        /// <summary>The load compound word info.</summary>
        private void LoadCompoundWordInfo()
        {
            if (IncludeCompoundWords)
            {
                CompundWordsTableAdapter iCompoundAdapter = new CompundWordsTableAdapter();
                wordnetDataSet.CompundWordsDataTable iData = iCompoundAdapter.GetData(LastSearchText);
                var iGroups = new System.Collections.Generic.Dictionary<string, WordNetItemGroup>();
                foreach (CompundWordsRow i in iData)
                {
                    var iGroup = GetGroup(iGroups, i.pos, i.Word);
                    var iNew = new WordNetItem();
                    iNew.Synonyms = WordNetSin.GetSynonymsList(i.synsetid);
                    iNew.ShortText = i.Word;
                    iNew.POS = i.pos;
                    iNew.SynsetID = i.synsetid;
                    iNew.Description = i.definition;
                    iGroup.Children.Add(iNew);
                    iNew.LoadRelatedWordsFor(LastSearchText, i.synsetid, SelectedRelationship);
                }
            }
        }

        /// <summary>Gets the WordnetItem group (which represents a posgroup, for the specified pos out of the list of
        ///     groups. If it doesn't yet exist, one is created, added to the list is children and the text is assigned
        ///     to it.</summary>
        /// <param name="groups">The groups.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="text">The text.</param>
        /// <returns>The <see cref="WordNetItemGroup"/>.</returns>
        private WordNetItemGroup GetGroup(System.Collections.Generic.Dictionary<string, WordNetItemGroup> groups, 
            string pos, 
            string text)
        {
            WordNetItemGroup iRes;
            if (groups.TryGetValue(pos, out iRes) == false)
            {
                iRes = new WordNetItemGroup();
                iRes.GroupFor = text;
                iRes.POS = WordNetSin.GetPos(pos);
                iRes.PosString = pos;
                Children.Add(iRes);
                groups.Add(pos, iRes);
            }

            return iRes;
        }

        /// <summary>The load word info.</summary>
        private void LoadWordInfo()
        {
            var iGroups = new System.Collections.Generic.Dictionary<string, WordNetItemGroup>();
            foreach (WordInfoRow iRow in WordNetSin.Default.GetWordInfoFor(LastSearchText))
            {
                var iNew = new WordNetItem();
                var iGroup = GetGroup(iGroups, iRow.pos, iRow.Word);
                iNew.Synonyms = WordNetSin.GetSynonymsList(iRow.synsetid);
                iNew.ShortText = iRow.Word;
                iNew.POS = iRow.pos;
                iNew.SynsetID = iRow.synsetid;
                iNew.Description = iRow.definition;
                iGroup.Children.Add(iNew);
                iNew.LoadRelatedWordsFor(LastSearchText, iRow.synsetid, SelectedRelationship);
            }
        }

        /// <summary>The import word.</summary>
        /// <param name="text">The text.</param>
        internal void ImportWord(string text)
        {
            // we call the load in anohter thread so that the UI stays responsive.
            System.Action<string> iFunc = WordNetSin.Default.Load;
            iFunc.BeginInvoke(text, null, null);
        }

        #endregion

        #region xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <remarks>When streaming to a module (for export), we do a mapping, to the index of the
        ///     neuron in the module that is currently being exported, and off course visa versa, when reading from a module.</remarks>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "SelectedRelationship", SelectedRelationship);
            XmlStore.WriteElement(writer, "IncludeCompoundWords", IncludeCompoundWords);
            XmlStore.WriteElement(writer, "CurrentText", CurrentText);
            XmlStore.WriteElement(writer, "LastImportedWord", LastImportedWord);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        /// <remarks>Descendents need to perform mapping between module index and neurons when importing from modules.</remarks>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            SelectedRelationship = XmlStore.ReadElement<int>(reader, "SelectedRelationship");
            IncludeCompoundWords = XmlStore.ReadElement<bool>(reader, "IncludeCompoundWords");
            string iFound = null;
            if (XmlStore.TryReadElement(reader, "CurrentText", ref iFound))
            {
                CurrentText = iFound;
            }

            var iInt = -1;
            if (XmlStore.TryReadElement(reader, "LastImportedWord", ref iInt))
            {
                LastImportedWord = iInt;
            }
        }

        #endregion
    }
}