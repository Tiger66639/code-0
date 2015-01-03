// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   A helper class for generating the content of the thesaurus, based on the entry points of the TextSin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A helper class for generating the content of the thesaurus, based on the entry points of the TextSin.
    /// </summary>
    internal class ThesaurusBuilder : Data.ObservableObject
    {
        /// <summary>
        ///     Raised when the TextSin has got some text it wants to output.
        /// </summary>
        public event System.EventHandler Finished;

        /// <summary>
        ///     Called when the operation is finished. Raises the event.
        /// </summary>
        protected void OnFinished()
        {
            if (Finished != null)
            {
                Finished(this, System.EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Shows the dialog box to the user so the process can be started.
        /// </summary>
        public void Start()
        {
            StartFromTextSin = false;
            var iDlg = new DlgBuildThesaurus();
            iDlg.Owner = WindowMain.Current;
            iDlg.DataContext = this;
            iDlg.ShowDialog();
        }

        /// <summary>
        ///     Performs the actual build, called from the dialog.
        /// </summary>
        internal void Build()
        {
            System.Action iBuild = InternalBuild;
            iBuild.BeginInvoke(null, null);
        }

        /// <summary>The internal build.</summary>
        private void InternalBuild()
        {
            IsRunning = true;
            try
            {
                CurrentWordIndex = 0;
                PrepareResultList();
                if (StartFromTextSin)
                {
                    BuildFromText();
                }
                else
                {
                    BuildFromObject();
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>The build from object.</summary>
        private void BuildFromObject()
        {
            Neuron iFound;
            TotalWords = Brain.Current.NextID;
            var iRels = BrainData.Current.Thesaurus.Relationships;
            for (var i = Neuron.StartId; i < Brain.Current.NextID; i++)
            {
                if (StopRequested)
                {
                    break;
                }

                CurrentWordIndex = i;
                if (Brain.Current.TryFindNeuron(i, out iFound))
                {
                    // the neuron exists, so process
                    var iCluster = iFound as NeuronCluster;
                    if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                    {
                        for (var iRelIndex = 0; iRelIndex < iRels.Count; iRelIndex++)
                        {
                            var iRel = iRels[iRelIndex];
                            if (iRel.Item != null)
                            {
                                CurRelId = iRel.Item.ID;
                                ProcessObject(iCluster);
                            }

                            if (StopRequested)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>The build from text.</summary>
        private void BuildFromText()
        {
            TotalWords = (ulong)TextSin.Words.Count;
            var iRels = BrainData.Current.Thesaurus.Relationships;
            foreach (var i in TextSin.Words.Values)
            {
                if (StopRequested)
                {
                    break;
                }

                for (var iRelIndex = 0; iRelIndex < iRels.Count; iRelIndex++)
                {
                    var iRel = iRels[iRelIndex];
                    if (iRel.Item != null)
                    {
                        CurRelId = iRel.Item.ID;
                        var iText = Brain.Current[i] as TextNeuron;
                        if (iText != null)
                        {
                            CurrentWord = iText.Text;
                            CurrentWordIndex++;
                            ProcessTextNeuron(iText);
                        }
                    }

                    if (StopRequested)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     transfers the result lists to the thesaurus dictionary.
        /// </summary>
        internal void StoreResult()
        {
            BrainData.Current.Thesaurus.Data = fResults;
        }

        /// <summary>
        ///     Prepares the result list containing all the lists for the relationships.
        /// </summary>
        private void PrepareResultList()
        {
            fResults.Clear();
            foreach (var i in BrainData.Current.Thesaurus.Relationships)
            {
                fResults.Add(i.Item.ID, new LargeIDCollection());
            }
        }

        /// <summary>The process text neuron.</summary>
        /// <param name="toProcess">The to process.</param>
        private void ProcessTextNeuron(TextNeuron toProcess)
        {
            if (toProcess.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = toProcess.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                try
                {
                    foreach (var iCluster in iClusters)
                    {
                        System.Diagnostics.Debug.Assert(iClusters != null);
                        if (iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                        {
                            ProcessObject(iCluster);
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusters);
                }
            }
        }

        /// <summary>The process object.</summary>
        /// <param name="toProcess">The to process.</param>
        private void ProcessObject(NeuronCluster toProcess)
        {
            if (fProcessed.Contains(toProcess.ID) == false)
            {
                var iIsChild = false;
                if (toProcess.ClusteredByIdentifier != null)
                {
                    System.Collections.Generic.List<NeuronCluster> iClusters;
                    using (var iList = toProcess.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                    try
                    {
                        foreach (var iCluster in iClusters)
                        {
                            System.Diagnostics.Debug.Assert(iClusters != null);
                            if (iCluster.Meaning == CurRelId)
                            {
                                iIsChild = true;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.CLists.Recycle(iClusters);
                    }
                }

                if (iIsChild == false)
                {
                    // when it is not used as a child, it is a possible root item, still depends if we we only want roots that already have children or not.
                    if (IncludeAllRoots || toProcess.FindFirstOut(CurRelId) != null)
                    {
                        // we have our root: a root only needs to have a cluster, it doens't actually need a content in those children.
                        fResults[CurRelId].Add(toProcess.ID);
                    }
                }
            }
        }

        #region Fields

        /// <summary>The f processed.</summary>
        private readonly System.Collections.Generic.HashSet<ulong> fProcessed =
            new System.Collections.Generic.HashSet<ulong>();

                                                                   // contains all the roots that have been found so far, for a single relationship.

        /// <summary>The f results.</summary>
        private readonly Data.SerializableDictionary<ulong, LargeIDCollection> fResults =
            new Data.SerializableDictionary<ulong, LargeIDCollection>(); // a result list for each relationship.

        /// <summary>The f current word.</summary>
        private string fCurrentWord;

        /// <summary>The f current word index.</summary>
        private ulong fCurrentWordIndex;

        /// <summary>The f is running.</summary>
        private bool fIsRunning;

        /// <summary>The f total words.</summary>
        private ulong fTotalWords;

        #endregion

        #region Prop

        #region IncludeAllRoots

        /// <summary>
        ///     When true, all the objects that aren't used as a child for the relationship, are used as roots. When false,
        ///     only objects that also have child items for the relationship are included (that is to say, have a cluster with
        ///     possible child items).
        /// </summary>
        /// <value><c>true</c> if [include all roots]; otherwise, <c>false</c>.</value>
        public bool IncludeAllRoots { get; set; }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets or sets a value indicating whether the build function is running or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning
        {
            get
            {
                return fIsRunning;
            }

            internal set
            {
                if (value != fIsRunning)
                {
                    fIsRunning = value;
                    if (fIsRunning == false)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(OnFinished));
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     When true, we start the search from the textsin's entry points: the textneurons. When false,
        ///     we use all objects. The difference is that in the first inctance, we will only find objects
        ///     that have textneurons, while in the second, we can import 'dead' objects, which only represent
        ///     a folder.
        /// </summary>
        /// <value><c>true</c> if [start from text sin]; otherwise, <c>false</c>.</value>
        public bool StartFromTextSin { get; set; }

        #region CurRelId

        /// <summary>
        ///     Gets or sets the id of the relatinshiop we are processing.
        /// </summary>
        /// <value>The cur rel id.</value>
        public ulong CurRelId { get; set; }

        #endregion

        #region CurrentWord

        /// <summary>
        ///     Gets/sets the the word that we are currently processing.
        /// </summary>
        public string CurrentWord
        {
            get
            {
                return fCurrentWord;
            }

            set
            {
                fCurrentWord = value;
                OnPropertyChanged("CurrentWord");
            }
        }

        #endregion

        #region StopRequested

        /// <summary>
        ///     Gets or sets a value indicating whether we should stop the process.
        /// </summary>
        /// <value><c>true</c> if [stop requested]; otherwise, <c>false</c>.</value>
        public bool StopRequested { get; set; }

        #endregion

        #region TotalWords

        /// <summary>Gets or sets the total words.</summary>
        public ulong TotalWords
        {
            get
            {
                return fTotalWords;
            }

            set
            {
                fTotalWords = value;
                OnPropertyChanged("TotalWords");
            }
        }

        #endregion

        #region CurrentWordIndex

        /// <summary>
        ///     Gets/sets the index of the current word that we are processing.
        /// </summary>
        public ulong CurrentWordIndex
        {
            get
            {
                return fCurrentWordIndex;
            }

            set
            {
                fCurrentWordIndex = value;
                OnPropertyChanged("CurrentWordIndex");
            }
        }

        #endregion

        #endregion
    }
}