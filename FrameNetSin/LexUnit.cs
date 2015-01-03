// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LexUnit.cs" company="">
//   
// </copyright>
// <summary>
//   The lex unit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>The lex unit.</summary>
    public class LexUnit : FrameBase
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="LexUnit" /> class.
        /// </summary>
        public LexUnit()
        {
            Lexemes = new Data.ObservedCollection<Lexeme>(this);
        }

        #endregion

        #region Annotation

        /// <summary>
        ///     Gets/sets the annotation for the lexical unit.
        /// </summary>
        [System.Xml.Serialization.XmlElement("annotation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Annotation Annotation
        {
            get
            {
                return fAnnotation;
            }

            set
            {
                OnPropertyChanging("Annotation", fAnnotation, value);
                fAnnotation = value;
                OnPropertyChanged("Annotation");
            }
        }

        #endregion

        #region Lexemes

        /// <summary>
        ///     Gets the list of lexemes for this lexical unit.
        /// </summary>
        [System.Xml.Serialization.XmlArray("lexemes")]
        [System.Xml.Serialization.XmlArrayItem("lexeme")]
        public Data.ObservedCollection<Lexeme> Lexemes { get; private set; }

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

        #region Status

        /// <summary>
        ///     Gets/sets the part of speech of this lexical unit.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("status")]
        public string Status
        {
            get
            {
                return fStatus;
            }

            set
            {
                OnPropertyChanging("Status", fStatus, value);
                fStatus = value;
                OnPropertyChanged("Status");
            }
        }

        #endregion

        #region LemmaID

        /// <summary>
        ///     Gets/sets the id of the lemma associated with this lexical unit.
        /// </summary>
        /// <remarks>
        ///     best seen as the id of an 'object' cluster.
        /// </remarks>
        [System.Xml.Serialization.XmlAttribute("lemmaId")]
        public int LemmaID
        {
            get
            {
                return fLemmaID;
            }

            set
            {
                OnPropertyChanging("LemmaID", fLemmaID, value);
                fLemmaID = value;
                OnPropertyChanged("LemmaID");
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
                    fWordNetID = Root.GetWordNetIDFor(LemmaID);
                }

                return fWordNetID;
            }

            set
            {
                if (value != fWordNetID)
                {
                    OnPropertyChanging("WordNetID", fWordNetID, value);
                    fWordNetID = value;
                    OnPropertyChanged("WordNetID");
                    var iRoot = Root;
                    if (iRoot != null && iRoot.WordNetMapLU != null)
                    {
                        iRoot.WordNetMapLU[LemmaID] = value;
                        iRoot.WordNetMapLUChanged = true;
                    }
                }
            }
        }

        #endregion

        #region WordNetIDValues

        /// <summary>
        ///     Gets the list of wordNet ID values that can be related to this lexical
        ///     unit (because they have the same textual representation).
        /// </summary>
        /// <value>
        ///     The word net ID values.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IList<FrameNet.Synset> WordNetIDValues
        {
            get
            {
                var iRes = new System.Collections.Generic.List<FrameNet.Synset>();
                var iSplit = Name.Split('.');
                if (iSplit.Length < 2)
                {
                    foreach (WordInfoRow i in WordNetSin.Default.GetWordInfoFor(Name))
                    {
                        var iNew = new FrameNet.Synset { ID = i.synsetid, Description = i.definition };
                        iRes.Add(iNew);
                    }
                }
                else
                {
                    foreach (WordInfoRow i in WordNetSin.Default.GetWordInfoFor(iSplit[0], iSplit[1]))
                    {
                        var iNew = new FrameNet.Synset { ID = i.synsetid, Description = i.definition };
                        iRes.Add(iNew);
                    }
                }

                return iRes;
            }
        }

        #endregion

        #region Fields

        /// <summary>The f annotation.</summary>
        private Annotation fAnnotation;

        /// <summary>The f pos.</summary>
        private string fPOS;

        /// <summary>The f status.</summary>
        private string fStatus;

        /// <summary>The f lemma id.</summary>
        private int fLemmaID;

        /// <summary>The f word net id.</summary>
        private int fWordNetID;

        #endregion
    }
}