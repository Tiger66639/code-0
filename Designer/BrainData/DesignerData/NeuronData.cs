// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronData.cs" company="">
//   
// </copyright>
// <summary>
//   A class that stores extra data for neurons, like a title and description.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A class that stores extra data for neurons, like a title and description.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Don't create these objects yourself, instead use the
    ///         <see cref="NeuronDataDictionary" />
    ///     </para>
    ///     <para>
    ///         Because <see cref="NeuronData" /> should only be saved when there actually
    ///         is data available, the property setters will make certain that the object
    ///         is registered with the <see cref="BrainData" /> as soon as there is data.
    ///     </para>
    /// </remarks>
    public class NeuronData : Data.OwnedObject, System.Xml.Serialization.IXmlSerializable, IDescriptionable
    {
        // , INDBStreamable
        /// <summary>
        ///     Called when a project is loaded. It allows to register some items
        ///     properly, like loading the overlays.
        /// </summary>
        /// <remarks>
        ///     <see cref="Overlays" /> are loaded by comparing data against the
        ///     Overlay info in the brainData object, which is only available after
        ///     being fully loaded.
        /// </remarks>
        internal void RegisterNeuron()
        {
            if (fNeuron != null)
            {
                LoadOverlays();
                EventManager.Current.RegisterNeuronData(this);
                if (Brain.Current.IsValidID(fNeuron.ID))
                {
                    var iProc = ProcessorManager.Current.SelectedProcessor;
                    IsNextStatement = iProc != null && iProc.Processor is DebugProcessor
                                      && iProc.Processor.NextStatement == fNeuron;
                }
            }
        }

        /// <summary>Copies the relevant display data from the specified source.</summary>
        /// <param name="source">The source.</param>
        public void CopyFrom(NeuronData source)
        {
            DisplayTitle = source.DisplayTitle;
            DescriptionText = source.DescriptionText;
            Category = source.Category;
            if (source.CustomData != null)
            {
                CustomData = new System.IO.MemoryStream();
                source.CustomData.CopyTo(CustomData);
            }
        }

        #region Inner types

        /// <summary>The neuron data overlay.</summary>
        public class NeuronDataOverlay
        {
            /// <summary>
            ///     Gets or sets the data object that uses the overlay.
            /// </summary>
            /// <value>
            ///     The data.
            /// </value>
            public NeuronData Data { get; set; }

            /// <summary>
            ///     Gets or sets the overlay.
            /// </summary>
            /// <value>
            ///     The overlay.
            /// </value>
            public OverlayText Overlay { get; set; }
        }

        #endregion

        #region fields

        /// <summary>The f neuron.</summary>
        private Neuron fNeuron;

        /// <summary>The f title.</summary>
        private string fTitle;

        /// <summary>The f category.</summary>
        private string fCategory;

        /// <summary>The f description.</summary>
        private string fDescription;

        // string fGUID;
        /// <summary>The f overlays.</summary>
        private System.Collections.ObjectModel.ObservableCollection<NeuronDataOverlay> fOverlays;

        /// <summary>The f custom data.</summary>
        private System.IO.MemoryStream fCustomData;

        /// <summary>The f is lock count.</summary>
        private int fIsLockCount;

        /// <summary>The f is chagned.</summary>
        private bool fIsChagned;

        /// <summary>The f is next statement.</summary>
        private bool fIsNextStatement;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronData"/> class. Normal constructor, with id.</summary>
        /// <param name="id">The id of the neuron to wrap.</param>
        internal NeuronData(ulong id)
        {
            IsReadError = false;
            if (id != Neuron.EmptyId && id != Neuron.TempId)
            {
                Neuron = Brain.Current[id];
            }
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronData"/> class.</summary>
        /// <param name="item">The item.</param>
        internal NeuronData(Neuron item)
        {
            IsReadError = false;
            if (item != null)
            {
                Neuron = item;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NeuronData" /> class.
        /// </summary>
        internal NeuronData()
        {
            IsReadError = false;
        }

        /// <summary>Finalizes an instance of the <see cref="NeuronData"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="NeuronData"/> is reclaimed by garbage
        ///     collection.</summary>
        ~NeuronData()
        {
            EventManager.Current.UnRegisterNeuronData(this);
        }

        #endregion

        #region Prop

        #region Title

        /// <summary>
        ///     Gets/sets the title of the neuron. To display, use
        ///     <see cref="DisplaytTitle" /> instead.
        /// </summary>
        /// <remarks>
        ///     only returns the physically stored name, no composite name. Primarely
        ///     used for storage.
        /// </remarks>
        public string Title
        {
            get
            {
                return fTitle;
            }

            set
            {
                if (fTitle != value)
                {
                    OnPropertyChanging("Title", fTitle, value);
                    fTitle = value;
                    OnPropertyChanged("Title");
                    OnPropertyChanged("DisplayTitle");
                    OnPropertyChanged("DescriptionTitle");
                }
            }
        }

        #endregion

        #region DisplayTitle

        /// <summary>
        ///     Gets/sets the title that should be displayed.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Primarely used for display. we check the title of the neuron, if not
        ///         found, a default title is generated: for textneurons, the text,
        ///         otherwise the type name + id.
        ///     </para>
        ///     <para>when set, we always set to the brainData.</para>
        /// </remarks>
        /// <value>
        ///     The display title.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public string DisplayTitle
        {
            get
            {
                var iRes = Title;
                if (string.IsNullOrEmpty(iRes))
                {
                    iRes = RenderOrBuildTitle();
                }

                return iRes;
            }

            set
            {
                var iDisplayTitle = DisplayTitle;
                var iIsEqual = (string.IsNullOrEmpty(iDisplayTitle) && string.IsNullOrEmpty(value))
                               || iDisplayTitle == value;
                if (!iIsEqual)
                {
                    // only try to change if different, saves some time, null and "" aren't equal by c# but they should be, so check for this.
                    if (fNeuron != null)
                    {
                        if (fNeuron.ID != Neuron.EmptyId)
                        {
                            // if the neuron has been deleted, don't try to change anything, cause that wont work.
                            if (fNeuron is TextNeuron)
                            {
                                SetValueToTextNeuron(value);
                            }
                            else if (fNeuron is DoubleNeuron)
                            {
                                SetValueToDouble(value);
                            }
                            else if (fNeuron is IntNeuron)
                            {
                                SetValueToInt(value);
                            }
                            else
                            {
                                Title = value;

                                    // we always do this, even if the value can be stored in the neuron itself. This is to call the observers + register undo data. 
                                IsChanged = true;
                                if (value == RenderTitleForCompare())
                                {
                                    // check if the new value is the same as the render title, if so, we might want to release the object (if there is no other data to save), otherwise, we want to save the object.
                                    fTitle = null;

                                        // the render title is the same as the assigned one, so don't need to save the title anymore
                                }

                                TryUpdateNameOfMember(fTitle);
                            }
                        }
                    }
                    else
                    {
                        Title = value;

                            // if there is no neuron, it's still a temp and we created a wrapper for it but didn't instantiate yet, so store the title value?
                        IsChanged = true;
                    }
                }
            }
        }

        /// <summary>this function can be used by parsers to assign a display title to
        ///     neurons. It wont generate any undo data and will also not check for
        ///     value-change and wether it is a valueneuron. The string will simply be
        ///     stored. Any 'nameOfMember' values are also not updated.</summary>
        /// <param name="value"></param>
        public void SetTitleNoUndo(string value)
        {
            if (fTitle != value)
            {
                fTitle = value;
                OnPropertyChanged("Title");
                OnPropertyChanged("DisplayTitle");
                OnPropertyChanged("DescriptionTitle");
            }

            IsChanged = true;
        }

        /// <summary>Checks if the neuron (which has to exists), has a link out with
        ///     meaning <see cref="PredifenedNeurons.NameOfMember"/> . If so, and on
        ///     the other side, there is a textneuron, try to update it's text value.
        ///     This is used for Rules and dummy thesaurus objects.</summary>
        /// <param name="value">The value.</param>
        private void TryUpdateNameOfMember(string value)
        {
            System.Diagnostics.Debug.Assert(fNeuron != null);
            var iFound = fNeuron.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);
            if (iFound != null)
            {
                // rules always need to have a name member, so that we can find them again in code.
                fNeuron.SetFirstOutgoingLinkTo(
                    (ulong)PredefinedNeurons.NameOfMember, 
                    BrainHelper.GetNeuronForText(value));
                if (iFound.CanBeDeleted && BrainHelper.HasReferences(iFound) == false)
                {
                    // we always replace the text value and don't reassign cause the neuron that reperesents the name is shared, don't want to screw up the other values.
                    BrainHelper.DeleteText(iFound);
                }
            }
            else
            {
                var iCluster = fNeuron as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.Meaning == (ulong)PredefinedNeurons.PatternRule
                        || iCluster.Meaning == (ulong)PredefinedNeurons.TextPatternTopic)
                    {
                        fNeuron.SetFirstOutgoingLinkTo(
                            (ulong)PredefinedNeurons.NameOfMember, 
                            BrainHelper.GetNeuronForText(value));
                    }
                }
            }
        }

        /// <summary>Sets the <paramref name="value"/> to int. Note: when the<paramref name="value"/> is assigned to the neuron, it triggers an
        ///     event in the brain, this raises the 'DisplayTitle changed event.</summary>
        /// <param name="value">The value.</param>
        private void SetValueToInt(string value)
        {
            var iNeuron = fNeuron as IntNeuron;
            int iVal;
            if (int.TryParse(value, out iVal))
            {
                OnPropertyChanging("DisplayTitle", iNeuron.Value.ToString(), value);

                    // needs to be done before assigning the value, otherwise we get invalid focused ui item
                iNeuron.Value = iVal;
                IsChanged = true;
            }
            else
            {
                throw new System.InvalidCastException("Int value expected");
            }
        }

        /// <summary>Sets the <paramref name="value"/> to double. Note: when the<paramref name="value"/> is assigned to the neuron, it triggers an
        ///     event in the brain, this raises the 'DisplayTitle changed event.</summary>
        /// <param name="value">The value.</param>
        private void SetValueToDouble(string value)
        {
            var iNeuron = fNeuron as DoubleNeuron;
            double iVal;
            if (double.TryParse(value, out iVal))
            {
                OnPropertyChanging("DisplayTitle", iNeuron.Value.ToString(), value);

                    // needs to be done before assigning the value, otherwise we get invalid focused ui item
                iNeuron.Value = iVal;
                IsChanged = true;
            }
            else
            {
                throw new System.InvalidCastException("Double value expected");
            }
        }

        /// <summary>Sets the <paramref name="value"/> to text neuron. Note: when the<paramref name="value"/> is assigned to the neuron, it triggers an
        ///     event in the brain, this raises the 'DisplayTitle changed event.</summary>
        /// <param name="value">The value.</param>
        private void SetValueToTextNeuron(string value)
        {
            if (UpdateTextNeuron(value) == false)
            {
                // if the update of a textneuron was canceled, we don't store the data, but we need to let the actual ui object that triggered the change know that it needs to reset it's data back to the orginal val (hence the OnPropertyChagned).
                OnPropertyChanged("DisplayTitle");
            }
            else
            {
                IsChanged = true;
            }
        }

        /// <summary>Stores the displayTitle in the textneuron.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool UpdateTextNeuron(string value)
        {
            var iText = (TextNeuron)fNeuron;
            if (InDictionary)
            {
                // update the textsin's index after an update of a textneuron, but only if it was already in there.
                ulong iFound;
                if (TextSin.Words.TryGetID(value, out iFound))
                {
                    // check if the new value has already been used.
                    var iOverwrite =
                        System.Windows.MessageBox.Show(
                            string.Format(
                                "The new text value is already used by another textneuron ({0}), overwrite the old entrypoint with the new one?", 
                                iFound), 
                            "Duplicate text neurons", 
                            System.Windows.MessageBoxButton.YesNoCancel, 
                            System.Windows.MessageBoxImage.Question);
                    if (iOverwrite == System.Windows.MessageBoxResult.Cancel)
                    {
                        return false;
                    }

                    if (iOverwrite == System.Windows.MessageBoxResult.Yes)
                    {
                        TextSin.Words.Remove(iText);
                        TextSin.Words[value] = iText.ID;
                    }
                }
                else
                {
                    TextSin.Words.Remove(iText);
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        // only try to add again if it is not null, null strings aren't added to the dict.
                        TextSin.Words[value] = iText.ID;
                    }
                }
            }

            OnPropertyChanging("DisplayTitle", iText.Text, value);

                // needs to be done before actually assigning the value, otherwise we get an invalid focused item in the undo data.
            iText.Text = value;
            return true;
        }

        /// <summary>renders the title, or for compounds, posgroups and objects: if there
        ///     is no title, one is built and stored.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string RenderOrBuildTitle()
        {
            var iCluster = fNeuron as NeuronCluster;
            if (iCluster != null)
            {
                if (iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                {
                    Title = BrainHelper.BuildLabelForObject(iCluster);
                    IsChanged = true;
                    return fTitle;
                }

                if (iCluster.Meaning == (ulong)PredefinedNeurons.POSGroup)
                {
                    Title = BrainHelper.GetTextFromPosGroup(iCluster);
                    IsChanged = true;
                    return fTitle;
                }

                if (iCluster.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                {
                    Title = BrainHelper.GetTextFromCompound(iCluster);
                    IsChanged = true;
                    return fTitle;
                }

                return RenderTitleForCompare();
            }

            return RenderTitleForCompare();
        }

        /// <summary>Renders the title.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string RenderTitleForCompare()
        {
            if (fNeuron is TextNeuron)
            {
                return ((TextNeuron)fNeuron).Text;
            }

            if (fNeuron is DoubleNeuron)
            {
                return ((DoubleNeuron)fNeuron).Value.ToString();
            }

            if (fNeuron is IntNeuron)
            {
                return ((IntNeuron)fNeuron).Value.ToString();
            }

            if (fNeuron is Expression)
            {
                return fNeuron + (" - " + fNeuron.ID);
            }

            if (fNeuron != null)
            {
                return fNeuron.GetType().Name + (" - " + fNeuron.ID);
            }

            return null;
        }

        #endregion

        #region ID

        /// <summary>
        ///     Gets the id of the object we are storing extra info for.
        /// </summary>
        public ulong ID
        {
            get
            {
                if (fNeuron != null)
                {
                    return fNeuron.ID;
                }

                return Neuron.EmptyId;
            }

            internal set
            {
                if (ID != value)
                {
                    // only try to load a neuron if it isnt' loaded already, this is a small speed up.
                    if (value == Neuron.EmptyId)
                    {
                        fNeuron = null;
                    }
                    else if (Brain.Current.TryFindNeuron(value, out fNeuron) == false)
                    {
                        // we use the tryFind, so that there is no exception, cause neuronData is stored in a sErializable dict, which would fail in it's entirity, which we don't want.
                        fNeuron = null;
                        LogService.Log.LogError(
                            "NeuronData.ID", 
                            string.Format("No neuron found with id: {0}", value));
                        ProjectManager.Default.DataError = true;
                    }
                }
            }
        }

        #endregion

        #region Neuron

        /// <summary>
        ///     Gets the neuron taht this object wraps.
        /// </summary>
        /// <value>
        ///     The neuron.
        /// </value>
        public Neuron Neuron
        {
            get
            {
                return fNeuron;
            }

            private set
            {
                if (value != fNeuron)
                {
                    if (fNeuron != null)
                    {
                        EventManager.Current.UnRegisterNeuronData(this);
                    }

                    fNeuron = value;
                    RegisterNeuron();
                }
            }
        }

        #endregion

        #region DescriptionTitle

        /// <summary>
        ///     Gets a title that the description editor can use to display in the
        ///     header.
        /// </summary>
        /// <value>
        /// </value>
        public string DescriptionTitle
        {
            get
            {
                return DisplayTitle;
            }
        }

        #endregion

        #region Description

        /// <summary>
        ///     Gets/sets a possible description for the neuron.
        /// </summary>
        /// <remarks>
        ///     This value is stored as a string internally to save room, otherwise
        ///     the app mem explodes.
        /// </remarks>
        public System.Windows.Documents.FlowDocument Description
        {
            get
            {
                var iDescription = DescriptionText; // this will return either the local copy or the one on disk.
                if (iDescription != null)
                {
                    var stringReader = new System.IO.StringReader(iDescription);
                    var xmlReader = System.Xml.XmlReader.Create(stringReader);
                    try
                    {
                        return System.Windows.Markup.XamlReader.Load(xmlReader) as System.Windows.Documents.FlowDocument;
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "NeuronData.Description", 
                            "Xaml error: " + e + "  Cleaning database for " + ID);
                        BrainData.Current.NeuronInfo.CleanDBFor(this);
                        var iRes = Helper.CreateDefaultFlowDoc(iDescription);
                        fDescription = System.Windows.Markup.XamlWriter.Save(iRes);

                            // store the new description so we don't always get an error.
                        return iRes;
                    }
                }

                return Helper.CreateDefaultFlowDoc();
            }

            set
            {
                var iVal = System.Windows.Markup.XamlWriter.Save(value);
                var iDescription = DescriptionText;
                if (iDescription != iVal)
                {
                    OnPropertyChanging("DescriptionText", iDescription, value);

                        // when set, we only update through the xml text format.
                    fDescription = iVal;
                    OnPropertyChanged("Description");
                    OnPropertyChanged("DescriptionText");
                    IsChanged = true;
                }
            }
        }

        #endregion

        #region DescriptionText

        /// <summary>
        ///     Gets/sets the description in xml format.
        /// </summary>
        public string DescriptionText
        {
            get
            {
                return fDescription;
            }

            set
            {
                OnPropertyChanging("DescriptionText", fDescription, value);
                fDescription = value;
                OnPropertyChanged("Description");
                OnPropertyChanged("DescriptionText");
                IsChanged = true;
            }
        }

        #endregion

        #region Category

        /// <summary>
        ///     Gets/sets the category of the neuron.
        /// </summary>
        /// <remarks>
        ///     This is primarely used by
        ///     <see cref="JaStDev.HAB.Designer.NeuronData.Neuron" /> s used on the
        ///     toolbox.
        /// </remarks>
        public string Category
        {
            get
            {
                return fCategory;
            }

            set
            {
                if (value != fCategory)
                {
                    OnPropertyChanging("Category", fCategory, value);
                    fCategory = value;
                    OnPropertyChanged("Category");
                    IsChanged = true;
                }
            }
        }

        #endregion

        #region CustomData

        /// <summary>
        ///     <para>
        ///         Gets/sets a possible stream that can be used to store extra, custom
        ///         data. This can be used by editors for instance, to store extra data in
        ///         the db.
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 The queries use this to store the source code and other settings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 also used to store extra 'attribute' data defined in nnl code, like
        ///                 'designerProperties' which are used by the bot's properties page to
        ///                 list all of the registered props.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        public System.IO.MemoryStream CustomData
        {
            get
            {
                return fCustomData;
            }

            set
            {
                fCustomData = value;
                OnPropertyChanged("CustomData");
                IsChanged = true;
            }
        }

        #endregion

        #region InDictionary

        /// <summary>
        ///     Gets/sets the if this item is stored in it's corresponding dictionary.
        /// </summary>
        /// <remarks>
        ///     Currently only valid for <see cref="TextNeuron" /> s. Also see
        ///     <see cref="JaStDev.HAB.Designer.NeuronData.IsDictionaryItem" /> .
        /// </remarks>
        public bool InDictionary
        {
            get
            {
                var iText = fNeuron as TextNeuron;
                if (iText != null)
                {
                    ulong iFound;
                    if (string.IsNullOrEmpty(iText.Text) == false && TextSin.Words.TryGetID(iText.Text, out iFound))
                    {
                        return iFound == iText.ID;
                    }
                }

                return false;
            }

            set
            {
                if (value != InDictionary)
                {
                    if (value)
                    {
                        ulong iFound;
                        var iText = fNeuron as TextNeuron;
                        if (iText != null)
                        {
                            if (TextSin.Words.TryGetID(iText.Text, out iFound))
                            {
                                // check if the text has already been used in the dict.
                                var iOverwrite =
                                    System.Windows.MessageBox.Show(
                                        string.Format(
                                            "The text value is already used by another textneuron ({0}), overwrite the old entrypoint with the new one?", 
                                            iFound), 
                                        "Duplicate text neurons", 
                                        System.Windows.MessageBoxButton.YesNo, 
                                        System.Windows.MessageBoxImage.Question);
                                if (iOverwrite == System.Windows.MessageBoxResult.Yes)
                                {
                                    TextSin.Words[iText.Text] = ID;
                                }
                            }
                            else
                            {
                                TextSin.Words[iText.Text] = ID;
                            }
                        }
                    }
                    else
                    {
                        var iText = fNeuron as TextNeuron;
                        if (iText != null)
                        {
                            TextSin.Words.Remove(iText);
                        }
                    }
                }
            }
        }

        #endregion

        #region IsDictionaryItem

        /// <summary>
        ///     Gets if this item can be put in a dictionary or not.
        /// </summary>
        /// <remarks>
        ///     Currently, only text neurons are supported.
        /// </remarks>
        public bool IsDictionaryItem
        {
            get
            {
                return fNeuron is TextNeuron;
            }
        }

        #endregion

        #region IsNextStatement

        /// <summary>
        ///     Gets/sets the wether this neuron is the next statement for the
        ///     currently selected processor (in
        ///     <see cref="JaStDev.HAB.Designer.ProcessorManager.SelectedProcessor" />
        ///     ).
        /// </summary>
        public bool IsNextStatement
        {
            get
            {
                return fIsNextStatement;
            }

            set
            {
                fIsNextStatement = value;
                OnPropertyChanged("IsNextStatement");
            }
        }

        #endregion

        #region IsLocked

        /// <summary>
        ///     Gets/sets wether the neuron is locked by the project or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is used while deleting items. When true, the delete is prevented.
        ///         The value signifies that the project uses the neuron somewhere as a
        ///         root item (like a flow, frame or similar). These are only explicitly
        ///         removed, not 'when no longer used'.
        ///     </para>
        ///     <para>
        ///         This property is recursive, so you always have to assign an equal
        ///         amount of <c>false</c> s as <c>true</c> s.
        ///     </para>
        /// </remarks>
        public bool IsLocked
        {
            get
            {
                return fIsLockCount != 0;
            }

            set
            {
                if (value)
                {
                    fIsLockCount++;
                }
                else if (fIsLockCount > 0)
                {
                    fIsLockCount--;
                }

                if (fIsLockCount > 0)
                {
                    BrainData.Current.NeuronInfo.Commit(this);

                        // we only do a commit, don't do a Save, cause it might be that this item is a temp, which is still valid, we don't want to save this data.
                }
                else if (NeedsPersisting == false && fNeuron != null && fNeuron.ID > (ulong)PredefinedNeurons.EndOfStatic)
                {
                    BrainData.Current.NeuronInfo.SetWeak(this);
                }

                OnPropertyChanged("IsLocked");
            }
        }

        #endregion

        #region IsChagned

        /// <summary>
        ///     Gets/sets the wether the object has been changed since the last time
        ///     it was saved.
        /// </summary>
        /// <remarks>
        ///     When set to true, we make certain that the object remains in memory
        ///     untill it needs to be streamed.
        /// </remarks>
        public bool IsChanged
        {
            get
            {
                return fIsChagned;
            }

            set
            {
                if (value != fIsChagned)
                {
                    fIsChagned = value;
                    if (value)
                    {
                        BrainData.Current.NeuronInfo.Commit(this);
                    }
                    else if (IsLocked == false && Neuron.ID > (ulong)PredefinedNeurons.EndOfStatic)
                    {
                        // locked items can't be unloaded cause than we might loose the lock count. Static items can also not be made weak since they always need to remain in the dict (used to often).
                        BrainData.Current.NeuronInfo.SetWeak(this);
                    }
                }
            }
        }

        /// <summary>Sets the <see cref="JaStDev.HAB.Designer.NeuronData.IsChanged"/>
        ///     value, without changing the NeuronDataDictionary.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetChanged(bool value)
        {
            fIsChagned = value;
        }

        #endregion

        #region Overlays

        /// <summary>
        ///     Gets the list of overlays that are active for this neuron, wrapped
        ///     into a container that is more suited for menu commands because each
        ///     container also has a link to this <see cref="NeuronData" /> object for
        ///     finding all the data required to build new, similar links.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyObservableCollection<NeuronDataOverlay> Overlays
        {
            get
            {
                if (fOverlays == null)
                {
                    LoadOverlays();
                }

                return new System.Collections.ObjectModel.ReadOnlyObservableCollection<NeuronDataOverlay>(fOverlays);
            }
        }

        #endregion

        #region HasOverlays

        /// <summary>
        ///     Gets if there are currenlty any overlays loaded for the item.
        /// </summary>
        public bool HasOverlays
        {
            get
            {
                return fOverlays != null && fOverlays.Count > 0;
            }
        }

        #endregion

        #region NeedsPersisting

        /// <summary>
        ///     Gets wether this object has data that requires it to be saved with the
        ///     project.
        /// </summary>
        /// <remarks>
        ///     When there is a title (different than the generated version), a
        ///     category or a description, the object needs streaming.
        /// </remarks>
        public bool NeedsPersisting
        {
            get
            {
                return
                    !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Category)
                      && string.IsNullOrEmpty(DescriptionText) && CustomData == null);
            }
        }

        #endregion

        #region IsReadError

        /// <summary>
        ///     Gets/sets a flag that indicates if something went wrong during the
        ///     read process. This is a way for the db to try and read a broken
        ///     NeuronData object + allowing for a way to report that something went
        ///     wrong so that the db can try and fix itself.
        /// </summary>
        public bool IsReadError { get; set; }

        #endregion

        #endregion

        #region functions

        /// <summary>Called when [property changing].</summary>
        /// <param name="aProp">A prop.</param>
        /// <param name="aOld">A old.</param>
        /// <param name="aNew">A new.</param>
        protected override void OnPropertyChanging(string aProp, object aOld, object aNew)
        {
            var iUndoData = new NeuronDataUndoItem(Neuron, aProp, aOld);
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
        }

        /// <summary>An easy way to Store the intitial description for a neuron with a
        ///     simple string. It will automatically format it to xaml.</summary>
        /// <param name="value">The value.</param>
        public void StoreDescription(string value)
        {
            fDescription =
                string.Format(
                    "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph TextAlignment=\"Left\" FontFamily=\"Segoe UI\" FontSize=\"12\" NumberSubstitution.CultureSource=\"User\">{0}</Paragraph></FlowDocument>", 
                    XmlTextEncoder.Encode(value));
        }

        /// <summary>Called when a property of the underlying<see cref="JaStDev.HAB.Designer.NeuronData.Neuron"/> has changed.
        ///     This can have an effect on the display title, so it needs to be
        ///     updated.</summary>
        /// <param name="prop">The prop.</param>
        internal void NeuronPropChanged(string prop)
        {
            if (prop != "Meaning")
            {
                // when the meaning of the neuron changes, we don't need to update the displayTitle.
                OnPropertyChanged("DisplayTitle");
            }
        }

        /// <summary>Called when the <paramref name="neuron"/> itself is changed.</summary>
        /// <param name="neuron">The new neuron.</param>
        internal void NeuronChanged(Neuron neuron)
        {
            Neuron = neuron;
            OnPropertyChanged("DisplayTitle");
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </returns>
        public override string ToString()
        {
            return DisplayTitle;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            ID = XmlStore.ReadElement<ulong>(reader, "ID");
            XmlStore.TryReadElement(reader, "Title", ref fTitle);

            // XmlStore.TryReadElement<string>(reader, "Title", ref fTitle);
            XmlStore.TryReadElement(reader, "Category", ref fCategory);
            string iTemp = null;
            XmlStore.TryReadElement(reader, "NameSpace", ref iTemp); // namespaces no longer exist, so read in dummy.

            if (reader.Name == "FlowDocument")
            {
                // this is to catch some old code that still stored the description inline.
                if (reader.IsEmptyElement == false)
                {
                    fDescription = reader.ReadOuterXml();
                }
                else
                {
                    reader.ReadStartElement("FlowDocument");
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <remarks>Doesn't set the 'IsChanged' property to true, since this is not the
        ///     main storage type, so when saving to xml, it generally isn't saved for
        ///     the project, so this still needs to be done seperatly in case that it
        ///     is saved for the project in xml.</remarks>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "ID", ID);
            XmlStore.WriteElement(writer, "Title", Title);
            XmlStore.WriteElement(writer, "Category", Category);

            if (fDescription != null)
            {
                writer.WriteRaw(fDescription);
            }
        }

        #endregion

        #region overlays

        /// <summary>
        ///     Reloads the overlays if they had been loaded before. This is usually
        ///     done when the list of overlays has been updated.
        /// </summary>
        internal void ReloadOverlays()
        {
            if (fOverlays != null)
            {
                fOverlays.Clear();
                LoadOverlays();
            }
        }

        /// <summary>
        ///     Loads the overlay text objects that are valid for this object.
        /// </summary>
        private void LoadOverlays()
        {
            if (fOverlays == null)
            {
                fOverlays = new System.Collections.ObjectModel.ObservableCollection<NeuronDataOverlay>();
            }

            foreach (var i in BrainData.Current.Overlays)
            {
                if (fNeuron.FindFirstOut(i.ItemID) != null)
                {
                    fOverlays.Add(new NeuronDataOverlay { Data = this, Overlay = i });
                }
            }

            OnPropertyChanged("Overlays");
            OnPropertyChanged("HasOverlays");
        }

        /// <summary>Removes the overlay that is defined for the specified neuron, if there
        ///     is one.</summary>
        /// <remarks>Doesnt' check if the Overlay list is already loaded, check<see cref="NeuronData.OverlaysLoaded"/> befor calling this function.</remarks>
        /// <param name="id">The id.</param>
        internal void RemoveOverlay(ulong id)
        {
            if (fOverlays != null)
            {
                for (var i = 0; i < fOverlays.Count; i++)
                {
                    // we use a for loop, this should be faster since we only need to go throught the list once.
                    var iOverlay = fOverlays[i];
                    if (iOverlay.Overlay.ItemID == id)
                    {
                        fOverlays.RemoveAt(i);
                        if (fOverlays.Count == 0)
                        {
                            fOverlays = null;

                            // BrainData.Current.NeuronInfo.RemoveItemHasOverlays(this);
                            OnPropertyChanged("HasOverlays");
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>Checks if there is an overlay defined for the specified id and if so,
        ///     adds it to the overlays for this neuron.</summary>
        /// <param name="toAdd">The to Add.</param>
        internal void AddOverlay(OverlayText toAdd)
        {
            if (toAdd != null)
            {
                if (fOverlays == null)
                {
                    fOverlays = new System.Collections.ObjectModel.ObservableCollection<NeuronDataOverlay>();
                }

                fOverlays.Add(new NeuronDataOverlay { Overlay = toAdd, Data = this });
            }

            OnPropertyChanged("Overlays");
            OnPropertyChanged("HasOverlays");
        }

        #endregion

        #region INDBStreamable Members

        /// <summary>reads only the title part from the specified reader.</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="string"/>.</returns>
        internal static string ReadDataTitle(System.IO.BinaryReader reader)
        {
            reader.ReadUInt64(); // the id
            var iLength = reader.ReadInt32(); // the length of the category
            reader.ReadChars(iLength); // the category
            iLength = reader.ReadInt32(); // the length of the title
            if (iLength > 0)
            {
                var iValue = reader.ReadChars(iLength); // the title
                return new string(iValue);
            }

            return null;
        }

        /// <summary>Reads the object from the specified reader.</summary>
        /// <param name="reader">The reader.</param>
        public void Read(System.IO.BinaryReader reader)
        {
            ID = reader.ReadUInt64();
            try
            {
                ReadCategory(reader);
                ReadTitle(reader);
                ReadCustomData(reader);
                ReadDescription(reader);
            }
            catch
            {
                IsReadError = true;
            }
        }

        /// <summary>The read description.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadDescription(System.IO.BinaryReader reader)
        {
            char[] iRead;
            try
            {
                var iNrChar = reader.ReadInt32();
                if (iNrChar > 0)
                {
                    iRead = reader.ReadChars(iNrChar);
                    fDescription = new string(iRead);
                }
                else
                {
                    fDescription = null;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "NeuronData.Read", 
                    string.Format("Failed to read the description field for neuron {0}, error: {1}.", ID, e));
                throw; // throw again, so we prevent from reading any further.
            }
        }

        /// <summary>Reads the name space. Has become obsolete, so only reads ina dummy.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadCustomData(System.IO.BinaryReader reader)
        {
            try
            {
                var iNrChar = reader.ReadInt32();
                if (iNrChar > 0)
                {
                    fCustomData = new System.IO.MemoryStream();
                    fCustomData.SetLength(iNrChar);
                    reader.Read(fCustomData.GetBuffer(), 0, iNrChar);

                        // we need to load the mem stream like this, if we use the constructor to pass in a char[], we can't do GetBuffer() anymore.
                }
                else
                {
                    fCustomData = null;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "NeuronData.Read", 
                    string.Format("Failed to read the  GUID field for neuron {0}, error: {1}.", ID, e));
                throw; // throw again, so we prevent from reading any further.
            }
        }

        /// <summary>The read title.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTitle(System.IO.BinaryReader reader)
        {
            char[] iRead;
            try
            {
                var iNrChar = reader.ReadInt32();
                if (iNrChar > 0)
                {
                    iRead = reader.ReadChars(iNrChar);
                    fTitle = new string(iRead);
                }
                else
                {
                    fTitle = null;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "NeuronData.Read", 
                    string.Format("Failed to title field for neuron {0}, error: {1}.", ID, e));
                throw; // throw again, so we prevent from reading any further.
            }
        }

        /// <summary>The read category.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadCategory(System.IO.BinaryReader reader)
        {
            char[] iRead;
            try
            {
                var iNrChar = reader.ReadInt32();
                if (iNrChar > 0)
                {
                    iRead = reader.ReadChars(iNrChar);
                    fCategory = new string(iRead);
                }
                else
                {
                    fCategory = null;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "NeuronData.Read", 
                    string.Format("Failed to read the category field for neuron {0}, error: {1}.", ID, e));
                throw; // throw again, so we prevent from reading any further.
            }
        }

        /// <summary>Writes the object to the specified writer.</summary>
        /// <remarks>Warning: doesn't set<see cref="JaStDev.HAB.Designer.NeuronData.IsChanged"/> to<see langword="false"/> when saved cause this can cause deadlocks
        ///     between a lock on the <see cref="DesignerTable"/> through
        ///     DesignerStore, and NeuronDataDict.Lock. ex: do an import-all wordnet
        ///     data + a project save.</remarks>
        /// <param name="writer">The writer.</param>
        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(ID);
            if (string.IsNullOrEmpty(fCategory) == false)
            {
                writer.Write(fCategory.Length);
                writer.Write(fCategory.ToCharArray());
            }
            else
            {
                writer.Write(0);
            }

            if (string.IsNullOrEmpty(fTitle) == false)
            {
                writer.Write(fTitle.Length);
                writer.Write(fTitle.ToCharArray());
            }
            else
            {
                writer.Write(0);
            }

            if (fCustomData != null)
            {
                writer.Write((System.Int32)fCustomData.Length);
                fCustomData.Position = 0;
                writer.Write(fCustomData.GetBuffer()); // save the data to file.
            }
            else
            {
                writer.Write(0);

                    // this used to be for namespaces,we skip this now by setting it's length to 0. can be used in the future for some other string.
            }

            if (string.IsNullOrEmpty(fDescription) == false)
            {
                writer.Write(fDescription.Length);
                writer.Write(fDescription.ToCharArray());
            }
            else
            {
                writer.Write(0);
            }
        }

        #endregion
    }
}