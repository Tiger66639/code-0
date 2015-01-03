// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Lexeme.cs" company="">
//   
// </copyright>
// <summary>
//   The lexeme.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>The lexeme.</summary>
    [System.Xml.Serialization.XmlRoot(ElementName = "lexeme", Namespace = "", IsNullable = false)]
    public class Lexeme : FrameCore
    {
        #region ID

        /// <summary>
        ///     Gets/sets the id of the frame.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public int ID
        {
            get
            {
                return fID;
            }

            set
            {
                OnPropertyChanging("ID", fID, value);
                fID = value;
                OnPropertyChanged("ID");
            }
        }

        #endregion

        #region POS

        /// <summary>
        ///     Gets/sets the part of speech of this lexical unit.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("pos")]
        public string POS
        {
            get
            {
                return fPOS;
            }

            set
            {
                OnPropertyChanging("POS", fPOS, value);
                fPOS = value;
                OnPropertyChanged("POS");
            }
        }

        #endregion

        #region BreakBefore

        /// <summary>
        ///     Gets/sets the wether the <see langword="break" /> is before or after
        ///     the item
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("breakBefore")]
        public bool BreakBefore
        {
            get
            {
                return fBreakBefore;
            }

            set
            {
                OnPropertyChanging("BreakBefore", fBreakBefore, value);
                fBreakBefore = value;
                OnPropertyChanged("BreakBefore");
            }
        }

        #endregion

        #region IsHeadWord

        /// <summary>
        ///     Gets/sets the wether this is a headword or not
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("headword")]
        public bool IsHeadWord
        {
            get
            {
                return fIsHeadWord;
            }

            set
            {
                OnPropertyChanging("IsHeadWord", fIsHeadWord, value);
                fIsHeadWord = value;
                OnPropertyChanged("IsHeadWord");
            }
        }

        #endregion

        #region Value

        /// <summary>
        ///     Gets/sets the string value of the lexeme.
        /// </summary>
        [System.Xml.Serialization.XmlText]
        public string Value
        {
            get
            {
                return fValue;
            }

            set
            {
                OnPropertyChanging("Value", fValue, value);
                fValue = value;
                OnPropertyChanged("Value");
            }
        }

        #endregion

        #region WordNetID

        /// <summary>
        ///     Gets/sets the id of the 'synset' that corresponds to the lemma. (both
        ///     refer to a single meaning of a word (but where the same meaning can be
        ///     expressed by different words). ex: bunnet - trunk
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("synsetId")]
        public int WordNetID
        {
            get
            {
                if (fWordNetID == 0)
                {
                    var iRoot = Root;
                    fWordNetID = Root.GetWordNetIDFor(ID);
                }

                return fWordNetID;
            }

            set
            {
                OnPropertyChanging("WordNetID", fWordNetID, value);
                fWordNetID = value;
                OnPropertyChanged("WordNetID");
                var iRoot = Root;
                if (iRoot != null && iRoot.WordNetMapLU != null)
                {
                    iRoot.WordNetMapLU[ID] = value;
                    iRoot.WordNetMapLUChanged = true;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the list of wordNet <see cref="ID" /> values that can be related
        ///     to this lexical unit (because they have the same textual
        ///     representation).
        /// </summary>
        /// <value>
        ///     The word net <see cref="ID" /> values.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<FrameNet.Synset> WordNetIDValues
        {
            get
            {
                var iRes = new System.Collections.Generic.List<FrameNet.Synset>();
                foreach (WordInfoRow i in WordNetSin.Default.GetWordInfoFor(Value, POS))
                {
                    var iNew = new FrameNet.Synset { ID = i.synsetid, Description = i.definition };
                    iRes.Add(iNew);
                }

                return iRes;
            }
        }

        #region Fields

        /// <summary>The f id.</summary>
        private int fID;

        /// <summary>The f pos.</summary>
        private string fPOS;

        /// <summary>The f is head word.</summary>
        private bool fIsHeadWord;

        /// <summary>The f break before.</summary>
        private bool fBreakBefore;

        /// <summary>The f value.</summary>
        private string fValue;

        /// <summary>The f word net id.</summary>
        private int fWordNetID;

        #endregion
    }
}