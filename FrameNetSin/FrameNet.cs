// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameNet.cs" company="">
//   
// </copyright>
// <summary>
//   Data class for Framenet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     Data class for Framenet.
    /// </summary>
    /// <remarks>
    ///     For datamapping between framenet and wordnet, assign a dictionary to
    ///     <see cref="FrameNet.WordNetMap" /> .
    /// </remarks>
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(ElementName = "frames", Namespace = "", IsNullable = false)]
    public class FrameNet : Data.ObservableObject
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameNet"/> class.</summary>
        public FrameNet()
        {
            Frames = new Data.ObservedCollection<Frame>(this);
        }

        #endregion

        #region Inner types

        /// <summary>
        ///     contains the word and wordnet id of the word (in it's specific
        ///     meaning).
        /// </summary>
        /// <remarks>
        ///     This class is used to provide a list of possible synsetID's found in
        ///     wordnet that can be linked to this lexical unit (the same text and POS
        ///     value, but a different meaning).
        /// </remarks>
        public class Synset
        {
            /// <summary>
            ///     Gets or sets the description of the meaning
            /// </summary>
            /// <value>
            ///     The description.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            ///     Gets or sets the SynsetID, found in wordnet.
            /// </summary>
            /// <value>
            ///     The ID.
            /// </value>
            public int ID { get; set; }
        }

        #endregion

        #region Fields 

        /// <summary>The f xml created.</summary>
        private string fXMLCreated;

        /// <summary>The f selected frame.</summary>
        private System.Collections.Generic.List<Frame> fSelectedFrame = new System.Collections.Generic.List<Frame>();

        #endregion

        #region Prop

        #region Frames

        /// <summary>
        ///     Gets the frames in this data set.
        /// </summary>
        [System.Xml.Serialization.XmlElement("frame", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Data.ObservedCollection<Frame> Frames { get; private set; }

        #endregion

        #region XMLCreated

        /// <summary>
        ///     Gets/sets the date-time at which the xml file was created.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public string XMLCreated
        {
            get
            {
                return fXMLCreated;
            }

            set
            {
                OnPropertyChanging("XMLCreated", fXMLCreated, value);
                fXMLCreated = value;
                OnPropertyChanged("XMLCreated");
            }
        }

        #endregion

        #region WordNetMapLU

        /// <summary>
        ///     Gets/sets the dictionary to use for mapping Lemma ids to SynSetIds
        ///     found in the wordnet db. (Lexical units and lexemes to synsets).
        /// </summary>
        /// <remarks>
        ///     This data is not read in from the FrameNet database, should be
        ///     provided seperatly.
        /// </remarks>
        /// <value>
        ///     A dictionary with key = lemma id and value = wordnet id (synset id).
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IDictionary<int, int> WordNetMapLU { get; set; }

        #endregion

        #region WordNetMapFE

        /// <summary>
        ///     Gets/sets the dictionary to use for mapping frame elements to
        ///     SynSetIds found in the wordnet db. (Lexical units and lexemes to
        ///     synsets).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IDictionary<int, int> WordNetMapFE { get; set; }

        #endregion

        #region WordNetMapLUChanged

        /// <summary>
        ///     Gets wether the wordnet map for lexical units/lexemes has changed
        ///     since it was assigned to the object.
        /// </summary>
        /// <remarks>
        ///     This can be used to find out if it needs to be saved because the user
        ///     added data.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool WordNetMapLUChanged { get; internal set; }

        #endregion

        #region WordNetMapFEChanged

        /// <summary>
        ///     Gets wether the wordnet map for frame elements has changed since it
        ///     was assigned to the object.
        /// </summary>
        /// <remarks>
        ///     This can be used to find out if it needs to be saved because the user
        ///     added data.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool WordNetMapFEChanged { get; internal set; }

        #endregion

        #endregion

        #region Functions

        /// <summary>Gets the word net ID for the specified lemma id.</summary>
        /// <param name="LemmaID">The lemma ID.</param>
        /// <returns>The <see cref="int"/>.</returns>
        internal int GetWordNetIDFor(int LemmaID)
        {
            if (WordNetMapLU != null)
            {
                int iFound;
                if (WordNetMapLU.TryGetValue(LemmaID, out iFound))
                {
                    return iFound;
                }
            }

            return 0;
        }

        /// <summary>Gets the word net <paramref name="ID"/> for the specified frame
        ///     element id.</summary>
        /// <param name="ID">The ID.</param>
        /// <returns>The <see cref="int"/>.</returns>
        internal int GetWordNetIDForElement(int ID)
        {
            if (WordNetMapFE != null)
            {
                int iFound;
                if (WordNetMapFE.TryGetValue(ID, out iFound))
                {
                    return iFound;
                }
            }

            return 0;
        }

        #endregion
    }
}