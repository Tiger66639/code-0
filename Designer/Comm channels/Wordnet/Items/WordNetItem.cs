// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItem.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a single item that was found in wordnet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Represents a single item that was found in wordnet.
    /// </summary>
    public class WordNetItem : WordNetItemBase
    {
        #region Functions

        /// <summary>Loads all the children that represent the related words for the
        ///     specified relationship.</summary>
        /// <param name="text">The text.</param>
        /// <param name="synsetId">The synset id.</param>
        /// <param name="relationship">The relationship.</param>
        public void LoadRelatedWordsFor(string text, int synsetId, int relationship)
        {
            var iWordsAdapter = new RelatedWordsTableAdapter();
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // we don't want to undo the fact that items are being added or removed.
            try
            {
                RelatedWordsDataTable iWordsData = iWordsAdapter.GetData(relationship, synsetId);
                foreach (RelatedWordsRow iRow in iWordsData)
                {
                    var iNew = new WordNetItem();
                    iNew.Synonyms = WordNetSin.GetSynonymsList(iRow.synsetid);
                    string iDesc;
                    iNew.POS = WordNetSin.Default.GetSynsetInfoFor(iRow.synsetid, out iDesc);
                    iNew.SynsetID = iRow.synsetid;
                    iNew.ShortText = WordNetSin.GetSynsetText(iRow.synsetid);
                    iNew.Description = iDesc;
                    iNew.HasItems = (int)iWordsAdapter.GetNrOfRelatedWordsFor(iRow.linkid, iRow.synsetid) > 0
                                        ? true
                                        : false;
                    Children.Add(iNew);
                    HasItems = true;
                }

                iWordsData = iWordsAdapter.GetLexDataBy(synsetId, relationship);
                foreach (RelatedWordsRow iRow in iWordsData)
                {
                    var iNew = new WordNetItem();
                    iNew.ShortText = WordNetSin.GetSynsetText(iRow.synsetid);
                    iNew.Synonyms = WordNetSin.GetSynonymsList(iRow.synsetid);
                    string iDesc;
                    iNew.POS = WordNetSin.Default.GetSynsetInfoFor(iRow.synsetid, out iDesc);
                    iNew.SynsetID = iRow.synsetid;
                    iNew.Description = iDesc;
                    iNew.HasItems = (int)iWordsAdapter.GetNrOfLexRelatedWordsFor(iRow.linkid, iRow.synsetid) > 0
                                        ? true
                                        : false;
                    Children.Add(iNew);
                    HasItems = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        #endregion

        /// <summary>Called when a new object is created, check if this wordnet item
        ///     represents the object and if so, updates 'IsLoaded'. Also asks each
        ///     child to do the same.</summary>
        /// <param name="synsetId">The synset Id.</param>
        internal void UpdateForNewObject(int synsetId)
        {
            if (SynsetID == synsetId)
            {
                fIsLoaded = null;
                OnPropertyChanged("IsLoaded");
            }

            foreach (var i in Children)
            {
                i.UpdateForNewObject(synsetId);
            }
        }

        /// <summary>Called when a neuron is deled.</summary>
        /// <param name="id">The id of the neuron that was deleted.</param>
        internal void UpdateForDelete(ulong id)
        {
            if (ID == id)
            {
                fIsLoaded = false;
                ID = Neuron.EmptyId; // we need to reset the id as well.
                OnPropertyChanged("IsLoaded");
            }

            foreach (var i in Children)
            {
                i.UpdateForDelete(id);
            }
        }

        #region fields

        /// <summary>The f is loaded.</summary>
        private bool? fIsLoaded;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f synonyms.</summary>
        private System.Collections.Generic.List<string> fSynonyms;

        #endregion

        #region Prop

        #region SynsetID

        /// <summary>
        ///     Gets the SynSetId as defined in wordnet, for this item, so we can
        ///     easely find it again in the brain.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int SynsetID { get; internal set; }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets wether the children are loaded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (value != fIsExpanded && HasItems)
                {
                    // can only load stuff if there is something to load.
                    fIsExpanded = value;
                    if (value)
                    {
                        LoadRelatedWordsFor(Text, SynsetID, Root.SelectedRelationship);
                    }
                    else
                    {
                        Children.Clear();
                    }

                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets/sets wether this wordnet item is already loaded in the
        ///     network.Can only be assigned 1 time.
        /// </summary>
        /// <remarks>
        ///     When null, only part of the info is loaded. Note the field is also
        ///     nullable, but this stores wether the data has been calculated or not.
        /// </remarks>
        public bool? IsLoaded
        {
            get
            {
                if (fIsLoaded.HasValue == false)
                {
                    // the first time we try to access the 'isloaded' value, we check the actuall val.
                    System.Action iUpdateIsLoaded = UpdateIsLoaded;

                        // getting the info from the network (FindObject), can take a long time, which blocks the UI, so we do this is a seperate thread and once calculated, ask the ui to update again.
                    iUpdateIsLoaded.BeginInvoke(null, null);
                }

                if (fIsLoaded.HasValue)
                {
                    // since the value is loaded async, it might not yet be set.
                    if (fIsLoaded == false && ID != Neuron.EmptyId)
                    {
                        return null;
                    }

                    return fIsLoaded.Value;
                }

                return false;
            }

            set
            {
                if (value != fIsLoaded)
                {
                    fIsLoaded = value;
                    if (value == true)
                    {
                        OnPropertyChanging("IsLoaded", false, true);
                        System.Action iLoad = DoCompactLoad;
                        iLoad.BeginInvoke(null, null);
                    }
                    else
                    {
                        WindowMain.UndoStore.BeginUndoGroup();
                        try
                        {
                            OnPropertyChanging("IsLoaded", true, false);
                            EditorsHelper.DeleteObject((NeuronCluster)Brain.Current[ID]);

                                // don't need to unregister the event monitor cause this statement does this indirectly.
                            ID = Neuron.EmptyId;
                        }
                        finally
                        {
                            WindowMain.UndoStore.EndUndoGroup();
                        }
                    }

                    OnPropertyChanged("IsLoaded");

                        // need to call for both, otherwise durin load, we will display as if it is unloaded, better to let it in null stae.
                }
            }
        }

        /// <summary>The do compact load.</summary>
        private void DoCompactLoad()
        {
            var iNew = WordNetSin.Default.LoadCompact(ShortText, SynsetID);

                // we need to use the shorttext cause the Text contains all the synonyms while we only need 1
            if (iNew != null)
            {
                ID = iNew.ID;
                var iRoot = Root;
                if (iRoot != null)
                {
                    iRoot.RegisterMonitorFor(ID);
                }
            }

            OnPropertyChanged("IsLoaded");
        }

        /// <summary>The update is loaded.</summary>
        private void UpdateIsLoaded()
        {
            Neuron iFound = WordNetSin.Default.FindObject(SynsetID);
            if (iFound != null)
            {
                ID = iFound.ID; // we store the id and not the neuron, cause the object can change, but not the id.
                var iPos = iFound.FindFirstOut((ulong)PredefinedNeurons.POS);
                if (iPos == null)
                {
                    var iPosGroup = iFound.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);
                    if (iPosGroup != null)
                    {
                        iPos = iPosGroup.FindFirstOut((ulong)PredefinedNeurons.POS);
                    }
                }

                fIsLoaded = iPos != null;
            }
            else
            {
                fIsLoaded = false;
                ID = Neuron.EmptyId;
            }

            OnPropertyChanged("IsLoaded");
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets wether the item is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                fIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region HasItems

        /// <summary>
        ///     Gets the wether the word has any children for the current
        ///     relationship.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool HasItems { get; internal set; }

        #endregion

        #region Text

        /// <summary>
        ///     Gets the text for the wordnet item.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public string Text { get; private set; }

        #endregion

        /// <summary>
        ///     Gets all the synonyms for this item (including the word itself, which
        ///     is first). This allows us to import the data easily.
        /// </summary>
        public System.Collections.Generic.List<string> Synonyms
        {
            get
            {
                return fSynonyms;
            }

            set
            {
                fSynonyms = value;
                if (value != null)
                {
                    var iStr = new System.Text.StringBuilder(); // build the text as a single string for displaying.
                    if (value.Count > 0)
                    {
                        iStr.Append(value[0]);
                        for (var i = 1; i < value.Count; i++)
                        {
                            iStr.Append(", ");
                            iStr.Append(value[i]);
                        }
                    }

                    Text = iStr.ToString();
                }
                else
                {
                    Text = string.Empty;
                }
            }
        }

        #region POS

        /// <summary>
        ///     Gets the part of speech for the item.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public string POS { get; internal set; }

        #endregion

        #region Description

        /// <summary>
        ///     Gets the description for the item.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public string Description { get; internal set; }

        #endregion

        #region DisplayTitle

        /// <summary>
        ///     Gets the text to display in the view.
        /// </summary>
        /// <value>
        ///     The display title.
        /// </value>
        public string DisplayTitle
        {
            get
            {
                return Text + "(" + POS + "): " + Description;
            }
        }

        /// <summary>
        ///     Gets or sets the description of the first synonym. While
        ///     <see cref="JaStDev.HAB.Designer.WordNetItem.Text" /> stores all the
        ///     synonym texts, this only the first, which is required for
        ///     <see cref="WordNetSin.LoadCompact" />
        /// </summary>
        /// <value>
        ///     The short text.
        /// </value>
        public string ShortText { get; set; }

        #endregion

        #endregion
    }
}