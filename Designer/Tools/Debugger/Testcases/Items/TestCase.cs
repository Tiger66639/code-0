// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCase.cs" company="">
//   
// </copyright>
// <summary>
//   The root class for an entire test case. Contains all the test items that need to be run.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    using System.Linq;

    /// <summary>
    ///     The root class for an entire test case. Contains all the test items that need to be run.
    /// </summary>
    [System.Xml.Serialization.XmlRoot("TestCase", IsNullable = false)]
    public class TestCase : Data.OwnedObject, 
                            WPF.Controls.ITreeViewRoot, 
                            IDocOpener, 
                            Data.IOnCascadedChanged, 
                            Data.INotifyCascadedPropertyChanged, 
                            Data.ICascadedNotifyCollectionChanged, 
                            System.Xml.Serialization.IXmlSerializable, 
                            IDocumentInfo
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestCase" /> class.
        /// </summary>
        public TestCase()
        {
            Items = new Data.CascadedObservedCollection<TestCaseItem>(this);
            SelectedItems = new TestCaseSelectedItems(this);
        }

        #endregion

        /// <summary>The run.</summary>
        internal void Run()
        {
            fRunner = new TestCaseRunner();
            fRunner.CaseToRun = this;
            fRunner.RunOn = RunOn;
            fRunner.Finished += fRunner_Finished;
            fRunner.Run();
            OnPropertyChanged("IsRunning");
        }

        /// <summary>Handles the Finished event of the fRunner control. Called when the test is done.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fRunner_Finished(object sender, System.EventArgs e)
        {
            fRunner = null;
            OnPropertyChanged("IsRunning");
        }

        /// <summary>
        ///     Stops the test from running, if it was running.
        /// </summary>
        internal void Stop()
        {
            if (fRunner != null)
            {
                fRunner.Stop();
            }
        }

        /// <summary>Saves the testcase to specified path.
        ///     A new xml file will be made.</summary>
        /// <param name="path">The path.</param>
        internal void Save(string path)
        {
            var iFileName = System.IO.Path.Combine(path, Name + '.' + BrainData.TESTCASEEXT);
            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCase));
            using (var iWriter = new System.Xml.XmlTextWriter(iFileName, null)) iSer.Serialize(iWriter, this);
            IsChanged = false;
        }

        /// <summary>The deep copy.</summary>
        /// <returns>The <see cref="TestCase"/>.</returns>
        internal TestCase DeepCopy()
        {
            System.IO.MemoryStream iFile;
            using (iFile = new System.IO.MemoryStream())
            {
                var iSerW = new System.Xml.Serialization.XmlSerializer(typeof(TestCase));
                iSerW.Serialize(iFile, this);
                iFile.Flush();
                iFile.Position = 0;
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iRSettings = new System.Xml.XmlReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iRSettings))
                {
                    var iNew = new TestCase();
                    if (iReader.IsStartElement())
                    {
                        iNew.ReadXml(iReader);
                    }

                    return iNew;
                }
            }
        }

        #region fields

        /// <summary>The f name.</summary>
        private string fName;

        /// <summary>The f is open.</summary>
        private bool fIsOpen;

        /// <summary>The f is changed.</summary>
        private bool fIsChanged;

        /// <summary>The f runner.</summary>
        private TestCaseRunner fRunner; // to execute/run the test

        /// <summary>The f run on.</summary>
        private ITesteable fRunOn;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        /// <summary>The f result width.</summary>
        private double fResultWidth = 140;

        /// <summary>The f test width.</summary>
        private double fTestWidth = 140;

        /// <summary>The f verify width.</summary>
        private double fVerifyWidth = 140;

        /// <summary>The f is enabled width.</summary>
        private double fIsEnabledWidth = 50;

        #endregion

        #region events

        #region ICascadedNotifyCollectionChanged Members

        /// <summary>
        ///     Occurs when [cascaded collection changed].
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region INotifyCascadedPropertyChanged Members

        /// <summary>
        ///     Occurs when [cascaded property changed].
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        #endregion

        #endregion

        #region Prop

        #region Name

        /// <summary>
        ///     Gets/sets the name of the test case
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(fName))
                {
                    fName = "new test case";
                }

                return fName;
            }

            set
            {
                if (value != fName)
                {
                    if (fName != null)
                    {
                        // we only store a change if it was not the original name assign.
                        IsChanged = true;
                    }

                    fName = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the items that define the test.
        /// </summary>
        public Data.CascadedObservedCollection<TestCaseItem> Items { get; private set; }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        public TestCaseSelectedItems SelectedItems { get; private set; }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets/sets the item that is currently selected. This should be a shild of the testcase (no check are done for this
        /// </summary>
        public TestCaseItem SelectedItem
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    return SelectedItems[0];
                }

                return null;
            }

            set
            {
                if (System.Windows.Input.Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Control)
                {
                    SelectedItems.ClearInternal();
                }

                SetSelectedItem(value);
            }
        }

        /// <summary>The set selected item.</summary>
        /// <param name="value">The value.</param>
        internal void SetSelectedItem(TestCaseItem value)
        {
            if (value != null)
            {
                SelectedItems.Add(value);
            }
            else if (SelectedItems.Count > 0)
            {
                // this is to prevent endless recursion.
                SelectedItems.Clear();
            }

            OnPropertyChanged("SelectedItem");
        }

        #endregion

        #region ITreeViewRoot Members

        /// <summary>
        ///     Gets a list to all the children of the tree root.
        /// </summary>
        /// <value>The tree items.</value>
        public System.Collections.IList TreeItems
        {
            get
            {
                return Items;
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets/sets the wether the data is loaded from file or not. When a testcase needs to be shown, it gets loaded. Loaded
        ///     testcases get saved when the project gets saved.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return fIsOpen;
            }

            set
            {
                if (value != fIsOpen)
                {
                    if (value)
                    {
                        LoadData();
                    }
                    else
                    {
                        ClearData();
                    }

                    OnPropertyChanged("IsOpen");
                }
            }
        }

        /// <summary>
        ///     Clears the data so that it doesn't occupy mem when not used.
        /// </summary>
        private void ClearData()
        {
            SelectedItems.Clear();
            if (IsChanged == false)
            {
                // don't drop the data when something changed. We also leave the testcase open when it changed, so we know it needs to be saved.
                fIsOpen = false;

                    // needs to be done before clearing the data, otherwise the IsChanged will get set and we will try to save an empty testcase.
                Items.Clear();
            }
        }

        /// <summary>
        ///     Loads the data from the current designer path.
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
            {
                var iFileName = System.IO.Path.Combine(
                    BrainData.Current.NeuronInfo.StoragePath, 
                    Name + '.' + BrainData.TESTCASEEXT);
                LoadDataFrom(iFileName);
            }
        }

        /// <summary>Loads the data from the specified file.</summary>
        /// <param name="fileName">Name of the file.</param>
        internal void LoadDataFrom(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var iSettings = new System.Xml.XmlReaderSettings();
                iSettings.IgnoreComments = true;
                iSettings.IgnoreWhitespace = true;
                iSettings.IgnoreProcessingInstructions = true;
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCase));
                using (var iReader = System.Xml.XmlReader.Create(fileName, iSettings))
                {
                    iReader.Read(); // read the first part
                    ReadXml(iReader);
                }
            }

            fIsOpen = true;
        }

        #endregion

        #region IsChagned

        /// <summary>
        ///     Gets/sets the wether the test has been modified since last time.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return fIsChanged;
            }

            set
            {
                fIsChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets/sets the wether the test is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return fRunner != null;
            }
        }

        #endregion

        #region RunOn

        /// <summary>
        ///     Gets/sets the comm Channel to run this test on.  We need the channel, so we chan use the
        ///     same send technique + let the ui of the channel update stuff.
        /// </summary>
        public ITesteable RunOn
        {
            get
            {
                return fRunOn;
            }

            set
            {
                if (value != fRunOn)
                {
                    OnPropertyChanging("RunOn", fRunOn, value);
                    fRunOn = value;
                    OnPropertyChanged("RunOn");
                    IsChanged = true;
                }
            }
        }

        #endregion

        #region TextChannels

        /// <summary>
        ///     Gets all the text sins on which the test could be run.
        /// </summary>
        /// <value>The text sins.</value>
        public System.Collections.Generic.IList<ITesteable> TextChannels
        {
            get
            {
                System.Collections.Generic.List<ITesteable> iList = null;
                if (WindowMain.Current.DesignerVisibility == System.Windows.Visibility.Visible)
                {
                    iList =
                        (from i in BrainData.Current.CommChannels where i is TextChannel select (ITesteable)i).ToList();
                }
                else
                {
                    iList = new System.Collections.Generic.List<ITesteable>();
                }

                iList.AddRange(from i in BrainData.Current.CommChannels where i is ChatBotChannel select (ITesteable)i);
                return iList;
            }
        }

        #endregion

        #region UI related

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll pos.
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                fHorScrollPos = value;
                OnPropertyChanged("HorScrollPos");
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scroll position.
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                fVerScrollPos = value;
                OnPropertyChanged("VerScrollPos");
            }
        }

        #endregion

        #region ResultWidth

        /// <summary>
        ///     Gets/sets the width of the 'enabled' column
        /// </summary>
        public double ResultWidth
        {
            get
            {
                return fResultWidth;
            }

            set
            {
                fResultWidth = value;
                OnPropertyChanged("ResultWidth");
            }
        }

        #endregion

        #region TestWidth

        /// <summary>
        ///     Gets/sets the width of the 'Test' column
        /// </summary>
        public double TestWidth
        {
            get
            {
                return fTestWidth;
            }

            set
            {
                fTestWidth = value;
                OnPropertyChanged("TestWidth");
            }
        }

        #endregion

        #region VerifyWidth

        /// <summary>
        ///     Gets/sets the width of the verify column
        /// </summary>
        public double VerifyWidth
        {
            get
            {
                return fVerifyWidth;
            }

            set
            {
                fVerifyWidth = value;
                OnPropertyChanged("VerifyWidth");
            }
        }

        #endregion

        #region IsEnabledWidth

        /// <summary>
        ///     Gets/sets the width of  the duration  column
        /// </summary>
        public double IsEnabledWidth
        {
            get
            {
                return fIsEnabledWidth;
            }

            set
            {
                fIsEnabledWidth = value;
                OnPropertyChanged("IsEnabledWidth");
            }
        }

        #endregion

        #endregion

        #region IDocumentInfo Members

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>The document title.</value>
        public string DocumentTitle
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>The document info.</value>
        public string DocumentInfo
        {
            get
            {
                return "Testcase: " + Name;
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>The type of the document.</value>
        public string DocumentType
        {
            get
            {
                return "Testcase";
            }
        }

        /// <summary>Gets the document icon.</summary>
        public object DocumentIcon
        {
            get
            {
                return "/Images/Tools/TestCase.png";
            }
        }

        #endregion

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (IsOpen)
            {
                // when loading from xml, Isopen is still false. Don't set IsChanged in this case cause we are simply loading from xml.
                IsChanged = true; // this is a catch all for collectionchanges.
            }

            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable
        ///     interface, you should return null (Nothing in Visual Basic) from this method, and instead,
        ///     if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        ///     method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            reader.Read();

            var iId = XmlStore.ReadElement<ulong>(reader, "RunOn");
            if (iId != Neuron.EmptyId)
            {
                fRunOn =
                    (from i in BrainData.Current.CommChannels where i.NeuronID == iId select (ITesteable)i)
                        .FirstOrDefault();
            }

            fIsEnabledWidth = XmlStore.ReadElement<double>(reader, "IsEnabledWidth");
            fResultWidth = XmlStore.ReadElement<double>(reader, "ResultWidth");
            fTestWidth = XmlStore.ReadElement<double>(reader, "TestWidth");
            fVerifyWidth = XmlStore.ReadElement<double>(reader, "VerifyWidth");

            var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCaseItem));
            while (reader.Name == "item")
            {
                var iNode = (TestCaseItem)iLinkSer.Deserialize(reader);
                if (iNode == null)
                {
                    // if for some reason, we failed to read the item, log an error, and advance to the next item so that we don't get in a loop.
                    LogService.Log.LogError(
                        "TestCase.ReadXml", 
                        string.Format("Failed to read xml element {0} in stream.", reader.Name));
                    reader.Skip();
                }
                else
                {
                    Items.Add(iNode);
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (RunOn != null)
            {
                XmlStore.WriteElement(writer, "RunOn", RunOn.NeuronID);
            }
            else
            {
                XmlStore.WriteElement(writer, "RunOn", Neuron.EmptyId);
            }

            XmlStore.WriteElement(writer, "IsEnabledWidth", IsEnabledWidth);
            XmlStore.WriteElement(writer, "ResultWidth", ResultWidth);
            XmlStore.WriteElement(writer, "TestWidth", TestWidth);
            XmlStore.WriteElement(writer, "VerifyWidth", VerifyWidth);

            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCaseItem));
            foreach (var i in Items)
            {
                iSer.Serialize(writer, i);
            }
        }

        #endregion
    }
}