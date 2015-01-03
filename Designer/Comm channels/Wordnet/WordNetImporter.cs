// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetImporter.cs" company="">
//   
// </copyright>
// <summary>
//   provides functionality to import the entire wordnet database at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides functionality to import the entire wordnet database at once.
    /// </summary>
    /// <remarks>
    ///     Warning: during this type of import, the network is streamed to the disk
    ///     because the full wordnet is big.
    /// </remarks>
    public class WordNetImporter : Data.ObservableObject
    {
        /// <summary>
        ///     Stops the import of the data.
        /// </summary>
        public void Stop()
        {
            StopRequested = true;
            System.Action iAsyncStop = WordNetSin.Default.StopImports;
            iAsyncStop.BeginInvoke(null, null);
        }

        #region Fields

        /// <summary>The f current word.</summary>
        private string fCurrentWord;

        /// <summary>The f current position.</summary>
        private int fCurrentPosition;

        /// <summary>The f total nr words.</summary>
        private int fTotalNrWords;

        /// <summary>The f call at end.</summary>
        private System.Action<WordNetImporter> fCallAtEnd;

        /// <summary>Initializes a new instance of the <see cref="WordNetImporter"/> class.</summary>
        public WordNetImporter()
        {
            StopRequested = false;
        }

        #endregion

        #region Prop

        #region CurrentWord

        /// <summary>
        ///     Gets/sets the word currently being imported.
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

        #region CurrentPosition

        /// <summary>
        ///     Gets/sets the current index position of the word currently being
        ///     imported.s
        /// </summary>
        public int CurrentPosition
        {
            get
            {
                return fCurrentPosition;
            }

            set
            {
                fCurrentPosition = value;
                OnPropertyChanged("CurrentPosition");
            }
        }

        #endregion

        #region TotalNrWords

        /// <summary>
        ///     Gets or sets the total nr words that will be imported.
        /// </summary>
        /// <value>
        ///     The total nr words.
        /// </value>
        public int TotalNrWords
        {
            get
            {
                return fTotalNrWords;
            }

            set
            {
                fTotalNrWords = value;
                OnPropertyChanged("TotalNrWords");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a stop was requested or not.
        ///     this allows us to store the last imported index, so we can resume when
        ///     a stop was done.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [stop requested]; otherwise, <c>false</c> .
        /// </value>
        public bool StopRequested { get; private set; }

        #endregion

        #region Channel

        /// <summary>
        ///     Gets/sets a <see langword="ref" /> to the channel
        /// </summary>
        public WordNetChannel Channel { get; set; }

        #endregion

        #endregion

        #region Functions

        /// <summary>Imports all the data from the wordnet db to the network.</summary>
        /// <param name="callAtEnd">The callback to execute at the end of the import. Used to update the
        ///     ui back to a normal state when the import is done.</param>
        /// <param name="lastImported">The last Imported.</param>
        public void Import(System.Action<WordNetImporter> callAtEnd, int lastImported)
        {
            if (string.IsNullOrEmpty(ProjectManager.Default.Location))
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        "Warning: the project hasn't been saved yet, so the import process can't stream the data to disk. This operation will consume a lot of memory. Do you want to continue?", 
                        "Import WordNet", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Warning);
                if (iRes == System.Windows.MessageBoxResult.No)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(callAtEnd, this);

                        // still need to let the ui know it's done already.
                    return;
                }
            }

            fCallAtEnd = callAtEnd;
            System.Action<int> iAsync = InternalImport;
            iAsync.BeginInvoke(lastImported, null, null);
        }

        /// <summary>The internal import.</summary>
        /// <param name="lastImported">The last imported.</param>
        private void InternalImport(int lastImported)
        {
            var iPrevMode = Settings.StorageMode;
            if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
            {
                // only try to stream when we have a location to stream too.
                Settings.StorageMode = NeuronStorageMode.AlwaysStream;
            }

            var iPrevIncludeCompound = WordNetSin.Default.IncludeCompoundWords;
            try
            {
                WordNetSin.Default.IncludeCompoundWords = false; // we always set to false, this is faster.
                AllWordsTableAdapter iAdapter = new AllWordsTableAdapter();
                AllWordsDataTable iData = iAdapter.GetData();
                TotalNrWords = iData.Count;
                CurrentPosition = lastImported;
                for (var i = lastImported; i < iData.Count; i++)
                {
                    // init of lastImported = , so first val = 0, when lastImported is set, we reimport the word, cause it might have been cut of during the middle of the import (usually the case), so not all links/objects were properly created for that word.
                    if (StopRequested)
                    {
                        break;
                    }

                    CurrentPosition = i;
                    CurrentWord = iData[i].lemma;
                    WordNetSin.Default.Load(CurrentWord);
                }
            }
            finally
            {
                WordNetSin.Default.IncludeCompoundWords = iPrevIncludeCompound;
                Settings.StorageMode = iPrevMode;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(fCallAtEnd, this);
            }
        }

        #endregion
    }
}