// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItemGroup.cs" company="">
//   
// </copyright>
// <summary>
//   This class contains a set of wordnet items that share a common
//   relationship with the item that was searched, usually regarding verbs.
//   This relationship was found using the regular expressions. This is
//   primarely used to display the original verb of a verb conjugation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This class contains a set of wordnet items that share a common
    ///     relationship with the item that was searched, usually regarding verbs.
    ///     This relationship was found using the regular expressions. This is
    ///     primarely used to display the original verb of a verb conjugation.
    /// </summary>
    public class WordNetItemGroup : WordNetItemBase
    {
        #region fields

        /// <summary>The f is loaded.</summary>
        private bool? fIsLoaded;

        #endregion

        #region GroupFor

        /// <summary>
        ///     Gets/sets the lemma of the verb, for which this is a group.
        /// </summary>
        public string GroupFor { get; set; }

        #endregion

        #region DisplayTitle

        /// <summary>
        ///     Gets/sets the display title for this group. This is either: Verbs,
        ///     Adjectives, Nouns, Adverbs
        /// </summary>
        public string DisplayTitle
        {
            get
            {
                if (POS == (ulong)PredefinedNeurons.Verb)
                {
                    return "Verbs";
                }

                if (POS == (ulong)PredefinedNeurons.Adjective)
                {
                    return "Adjectives";
                }

                if (POS == (ulong)PredefinedNeurons.Adverb)
                {
                    return "Adverbs";
                }

                if (POS == (ulong)PredefinedNeurons.Noun)
                {
                    return "Nouns";
                }

                return "Unknown group";
            }
        }

        #endregion

        #region RelationshipID

        /// <summary>
        ///     Gets/sets the id of the relationship between the verb of this group
        ///     and the original word that was searched.
        /// </summary>
        public ulong RelationshipID { get; set; }

        #endregion

        #region POS

        /// <summary>
        ///     Gets/sets the part of speech of the original word that the morphed
        ///     word relates too.
        /// </summary>
        public ulong POS { get; set; }

        #endregion

        #region PosString

        /// <summary>
        ///     Gets/sets the part of speech as a single letter string (used for
        ///     loading from wordnet).
        /// </summary>
        public string PosString { get; set; }

        #endregion

        /// <summary>The update for delete.</summary>
        /// <param name="id">The id.</param>
        internal void UpdateForDelete(ulong id)
        {
            if (ID == id)
            {
                fIsLoaded = false;
                OnPropertyChanged("IsLoaded");
            }
            else
            {
                foreach (var i in Children)
                {
                    i.UpdateForDelete(id);
                }
            }
        }

        /// <summary>The update for new object.</summary>
        /// <param name="synsetid">The synsetid.</param>
        internal void UpdateForNewObject(int synsetid)
        {
            foreach (var i in Children)
            {
                i.UpdateForNewObject(synsetid);
            }
        }

        /// <summary>
        ///     Loads the children.
        /// </summary>
        protected internal virtual void LoadChildren()
        {
            var iRoot = Root;
            var iWordsAdapter = new RelatedWordsTableAdapter();
            foreach (WordInfoRow iRow in WordNetSin.Default.GetWordInfoFor(GroupFor, PosString))
            {
                var iNew = new WordNetItem();
                iNew.Synonyms = WordNetSin.GetSynonymsList(iRow.synsetid);
                iNew.POS = iRow.pos;
                iNew.SynsetID = iRow.synsetid;
                iNew.Description = iRow.definition;
                Children.Add(iNew);
                iNew.LoadRelatedWordsFor(GroupFor, iRow.synsetid, iRoot.SelectedRelationship);
            }
        }

        /// <summary>Sets the <see cref="IsLoaded"/> to <see langword="true"/> without
        ///     actually trying to load the items (so when it is loaded from the
        ///     network).</summary>
        /// <param name="neuron">The neuron.</param>
        internal void SetLoaded(Neuron neuron)
        {
            ID = neuron.ID;
            fIsLoaded = true;
            OnPropertyChanged("IsLoaded");
        }

        #region IsLoaded

        /// <summary>
        ///     Gets/sets the wether the link has been created for this group.Can only
        ///     be assigned 1 time.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                if (fIsLoaded.HasValue == false)
                {
                    // the first time we try to access the 'isloaded' value, we check the actuall val.
                    fIsLoaded = InitIsLoaded();
                }

                return fIsLoaded.Value;
            }

            set
            {
                if (fIsLoaded != value)
                {
                    WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                        // this is not undoable, so make certain nothing is recorded.
                    try
                    {
                        if (value)
                        {
                            LoadIntoNetwork();
                        }
                        else
                        {
                            UnloadFromNetwork();
                        }

                        fIsLoaded = value;
                    }
                    finally
                    {
                        WindowMain.UndoStore.UndoStateStack.Pop();
                    }

                    OnPropertyChanged("IsLoaded");
                }
            }
        }

        /// <summary>Called when the <see cref="IsLoaded"/> value needs to be initialized.</summary>
        /// <param name="text">The textneuron that represents GroupFor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected virtual bool InitIsLoaded()
        {
            TextNeuron iText;
            if (TextSin.Words.TryGetNeuron(GroupFor, out iText))
            {
                var iFound = BrainHelper.FindPOSGroup(iText, POS);
                return iFound != null;
            }

            return false;
        }

        /// <summary>The unload from network.</summary>
        protected virtual void UnloadFromNetwork()
        {
            if (ID != Neuron.EmptyId)
            {
                Brain.Current.Delete(ID);
                ID = Neuron.EmptyId;
            }
        }

        /// <summary>The load into network.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void LoadIntoNetwork()
        {
            var iText = TextNeuron.GetFor(GroupFor);
            var iFound = BrainHelper.GetPOSGroup(iText, POS);
            if (iFound == null || iFound.ID == Neuron.EmptyId)
            {
                throw new System.InvalidOperationException(
                    string.Format("Couldn't find or create the part of speech group for {0}.", GroupFor));
            }

            ID = iFound.ID;
            var iRoot = Root;
            if (iRoot != null)
            {
                iRoot.RegisterMonitorFor(ID);
            }

            using (var iChildren = iFound.ChildrenW) // we don't lock all the children, but do each statement seperatly.
                foreach (var i in Children)
                {
                    if (i.ID != Neuron.EmptyId && iChildren.Contains(i.ID) == false)
                    {
                        iChildren.Add(i.ID);
                    }
                }
        }

        #endregion
    }
}