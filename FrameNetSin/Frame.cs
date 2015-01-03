// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Frame.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data for a single frame in framenet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     Contains all the data for a single frame in framenet.
    /// </summary>
    public class Frame : FrameBase
    {
        #region Fields

        // string fDescription;
        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Frame" /> class.
        /// </summary>
        public Frame()
        {
            Elements = new Data.ObservedCollection<FrameElement>(this);
            LexUnits = new Data.ObservedCollection<LexUnit>(this);
        }

        #endregion

        // #region Description (Definition)

        ///// <summary>
        ///// Gets/sets the description of the frame.
        ///// </summary>
        // [XmlElement(Form = XmlSchemaForm.Unqualified, ElementName = "definition")]
        // public string Description
        // {
        // get
        // {
        // return fDescription;
        // }
        // set
        // {
        // OnPropertyChanging("Description", fDescription, value);
        // fDescription = value;
        // OnPropertyChanged("Description");
        // }
        // }

        // #endregion
        #region Elements

        /// <summary>
        ///     Gets the list of Frame elements
        /// </summary>
        [System.Xml.Serialization.XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, ElementName = "fes")]
        [System.Xml.Serialization.XmlArrayItem("fe", typeof(FrameElement), 
            Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public Data.ObservedCollection<FrameElement> Elements { get; private set; }

        #endregion

        #region LexUnits

        /// <summary>
        ///     Gets the list of lexical units in this frame.
        /// </summary>
        [System.Xml.Serialization.XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, ElementName = "lexunits")
        ]
        [System.Xml.Serialization.XmlArrayItem("lexunit", typeof(LexUnit), 
            Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public Data.ObservedCollection<LexUnit> LexUnits { get; private set; }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the current item is selected or not.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (fIsSelected != value)
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                    var iOwner = Owner as FrameNet;
                    if (iOwner != null)
                    {
                    }
                }
            }
        }

        #endregion

        /// <summary>Imports this instance into the brain.</summary>
        /// <param name="lexUnits">The lex Units.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster Import(out NeuronCluster lexUnits)
        {
            var iRes = BrainHelper.CreateFrame(out lexUnits);
            NeuronCluster iObj;
            Neuron iEl;
            foreach (var i in Elements)
            {
                var iSplit = i.Name.Split('.'); // only need the first part (don't need .pos)
                if (i.WordNetID != 0)
                {
                    iObj = WordNetSin.Default.GetObject(iSplit[0], i.WordNetID);
                }
                else
                {
                    var iPos = iSplit.Length > 1 ? iSplit[1] : null;
                    iObj = GetObject(iSplit[0], i.ID, (ulong)PredefinedNeurons.FrameElementId, iPos);
                }

                iEl = BrainHelper.CreateFrameElement(iObj, null);
                AssignCoreType(iEl, i.CoreType);
                using (var iList = iRes.ChildrenW) iList.Add(iEl);
            }

            foreach (var i in LexUnits)
            {
                var iSplit = i.Name.Split('.'); // only need the first part (don't need .pos)
                if (i.Lexemes.Count == 1)
                {
                    if (i.WordNetID != 0)
                    {
                        iObj = WordNetSin.Default.GetObject(iSplit[0], i.WordNetID);
                    }
                    else if (i.Lexemes[0].WordNetID != 0)
                    {
                        iObj = WordNetSin.Default.GetObject(iSplit[0], i.Lexemes[0].WordNetID);
                    }
                    else
                    {
                        iObj = GetObject(iSplit[0], i.LemmaID, (ulong)PredefinedNeurons.LemmaId, i.POS);
                    }

                    using (var iList = lexUnits.ChildrenW) iList.Add(iObj);
                }
                else
                {
                    foreach (var iLexeme in i.Lexemes)
                    {
                        if (iLexeme.WordNetID != 0)
                        {
                            iObj = WordNetSin.Default.GetObject(iSplit[0], iLexeme.WordNetID);
                        }
                        else
                        {
                            iObj = GetObject(iSplit[0], iLexeme.ID, (ulong)PredefinedNeurons.LemmaId, iLexeme.POS);
                        }

                        using (var iList = lexUnits.ChildrenW) iList.Add(iObj);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Creates a link with meaning 'FrameImportance' from the<paramref name="item"/> to the neuron representing the specified
        ///     value.</summary>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        private void AssignCoreType(Neuron item, string value)
        {
            value = value.ToLower();
            ulong iTo = 0;
            if (value == "core")
            {
                iTo = (ulong)PredefinedNeurons.Frame_Core;
            }
            else if (value == "peripheral")
            {
                iTo = (ulong)PredefinedNeurons.Frame_peripheral;
            }
            else if (value == "extra-thematic")
            {
                iTo = (ulong)PredefinedNeurons.Frame_extra_thematic;
            }
            else
            {
                throw new System.ArgumentException("Invalid core type specified.", "value");
            }

            Link.Create(item, Brain.Current[iTo], (ulong)PredefinedNeurons.FrameImportance);
        }

        /// <summary>Gets the object cluster for the specified <paramref name="text"/> and
        ///     with the specified frame element id.</summary>
        /// <param name="text">The text.</param>
        /// <param name="feId">The id value to use as attached value for the cluster.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="pos">The part of speech that should be assigned to the object, if it is a
        ///     newly created one.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster GetObject(string text, int feId, ulong meaningID, string pos)
        {
            var iArgs = new GetObjectArgs();
            iArgs.Text = text;
            iArgs.AttachedInt = feId;
            iArgs.MeaningID = (ulong)PredefinedNeurons.FrameElementId;
            var iRes = BrainHelper.GetObject(iArgs);
            if (iArgs.IsNew && pos != null)
            {
                // still need to assign  the pos if there was any.
                pos = pos.ToLower();
                ulong iTo = 0;
                if (pos == "v" || pos == "verb")
                {
                    iTo = (ulong)PredefinedNeurons.Verb;
                }
                else if (pos == "n" || pos == "noun")
                {
                    iTo = (ulong)PredefinedNeurons.Noun;
                }
                else if (pos == "adv" || pos == "adverb")
                {
                    iTo = (ulong)PredefinedNeurons.Adverb;
                }
                else if (pos == "a" || pos == "adjective")
                {
                    iTo = (ulong)PredefinedNeurons.Adjective;
                }
                else if (pos == "prep" || pos == "preposition")
                {
                    iTo = (ulong)PredefinedNeurons.Preposition;
                }
                else if (pos == "con" || pos == "conjunction")
                {
                    iTo = (ulong)PredefinedNeurons.Conjunction;
                }
                else if (pos == "inter" || pos == "Intersection")
                {
                    iTo = (ulong)PredefinedNeurons.Interjection;
                }
                else
                {
                    throw new System.ArgumentException(string.Format("Unkown part of speech found (pos): {0}.", pos));
                }

                var iNew = new Link(Brain.Current[iTo], iRes, (ulong)PredefinedNeurons.POS);
            }

            return iRes;
        }
    }
}