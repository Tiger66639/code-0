// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecordedResult.cs" company="">
//   
// </copyright>
// <summary>
//   A single record event is stored in a RecordResultGroup. This contains all
//   the altermatives and single words that were recorded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>
    ///     A single record event is stored in a RecordResultGroup. This contains all
    ///     the altermatives and single words that were recorded.
    /// </summary>
    public class RecordResultGroup : Data.ObservableObject
    {
        /// <summary>The f selected index.</summary>
        private int fSelectedIndex;

        /// <summary>The f text.</summary>
        private string fText;

        /// <summary>The f inputs.</summary>
        private readonly System.Collections.Generic.List<string> fInputs = new System.Collections.Generic.List<string>();

                                                                 // only the strings, for quick processing.

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<RecordedResult> fItems =
            new System.Collections.Generic.List<RecordedResult>(); // full data.

        #region Text

        /// <summary>
        ///     Gets/sets the selected text for this item
        /// </summary>
        public string Text
        {
            get
            {
                return fText;
            }

            set
            {
                fText = value;
                OnPropertyChanged("Text");
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items: all the data that was recorded.
        /// </summary>
        public System.Collections.Generic.List<RecordedResult> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region SelectedIndex

        /// <summary>
        ///     Gets/sets the index of the selected item. When changed, the text is
        ///     also updated.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return fSelectedIndex;
            }

            set
            {
                fSelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        #endregion

        #region Text

        /// <summary>
        ///     Gets the list of strings, for quick accesss.
        /// </summary>
        public System.Collections.Generic.List<string> Inputs
        {
            get
            {
                return fInputs;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Contains all the values for a single result.
    /// </summary>
    public class RecordedResult : Data.ObservableObject
    {
        /// <summary>The f words.</summary>
        private readonly System.Collections.Generic.List<RecordedResultItem> fWords =
            new System.Collections.Generic.List<RecordedResultItem>();

        #region Text

        /// <summary>
        ///     Gets/sets the full text of the result
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Words

        /// <summary>
        ///     Gets the individual words that were found in the text (every word has
        ///     a corresponding weight).
        /// </summary>
        public System.Collections.Generic.List<RecordedResultItem> Words
        {
            get
            {
                return fWords;
            }
        }

        #endregion

        #region inner types

        /// <summary>The recorded result item.</summary>
        public class RecordedResultItem
        {
            /// <summary>
            ///     Gets or sets the weight of the current word
            /// </summary>
            /// <value>
            ///     The weight.
            /// </value>
            public double Weight { get; set; }

            /// <summary>
            ///     Gets or sets the word that this item represents.
            /// </summary>
            /// <value>
            ///     The word.
            /// </value>
            public string Word { get; set; }
        }

        #endregion
    }
}