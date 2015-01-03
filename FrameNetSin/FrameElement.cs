// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElement.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data for a single frame element in framenet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     Contains all the data for a single frame element in framenet.
    /// </summary>
    public class FrameElement : FrameBase
    {
        #region Abbreviation

        /// <summary>
        ///     Gets/sets the abbreviation used for the frame element.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("abbrev")]
        public string Abbreviation
        {
            get
            {
                return fAbbreviation;
            }

            set
            {
                OnPropertyChanging("Abbreviation", fAbbreviation, value);
                fAbbreviation = value;
                OnPropertyChanged("Abbreviation");
            }
        }

        #endregion

        #region CoreType

        /// <summary>
        ///     Gets/sets the abbreviation used for the frame element.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("coreType")]
        public string CoreType
        {
            get
            {
                return fCoreType;
            }

            set
            {
                OnPropertyChanging("CoreType", fCoreType, value);
                fCoreType = value;
                OnPropertyChanged("CoreType");
            }
        }

        #endregion

        #region Foreground

        /// <summary>
        ///     Gets/sets the abbreviation used for the frame element.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("fgColor")]
        public string Foreground
        {
            get
            {
                return fForeground;
            }

            set
            {
                OnPropertyChanging("Foreground", fForeground, value);
                fForeground = value;
                OnPropertyChanged("Foreground");
            }
        }

        #endregion

        #region Background

        /// <summary>
        ///     Gets/sets the abbreviation used for the frame element.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("bgColor")]
        public string Background
        {
            get
            {
                return fBackground;
            }

            set
            {
                OnPropertyChanging("Background", fBackground, value);
                fBackground = value;
                OnPropertyChanged("Background");
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
                    fWordNetID = Root.GetWordNetIDForElement(ID);
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
                    if (iRoot != null && iRoot.WordNetMapFE != null)
                    {
                        iRoot.WordNetMapFE[ID] = value;
                        iRoot.WordNetMapFEChanged = true;
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

        /// <summary>The f abbreviation.</summary>
        private string fAbbreviation;

        /// <summary>The f core type.</summary>
        private string fCoreType;

        /// <summary>The f foreground.</summary>
        private string fForeground;

        /// <summary>The f background.</summary>
        private string fBackground;

        /// <summary>The f word net id.</summary>
        private int fWordNetID;

        #endregion
    }
}