// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCaseItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines a single test to be run in a test case. This can contain sub test case items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     Defines a single test to be run in a test case. This can contain sub test case items.
    /// </summary>
    [System.Xml.Serialization.XmlRoot("item")]
    public class TestCaseItem : Data.OwnedObject, 
                                WPF.Controls.ITreeViewPanelItem, 
                                Data.INotifyCascadedPropertyChanged, 
                                Data.ICascadedNotifyCollectionChanged, 
                                Data.IOnCascadedChanged, 
                                System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f needs focus.</summary>
        private bool fNeedsFocus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestCaseItem" /> class.
        /// </summary>
        public TestCaseItem()
        {
            Items = new Data.CascadedObservedCollection<TestCaseItem>(this);
            Items.CollectionChanged += fItems_CollectionChanged;
            Results = new System.Collections.Generic.List<TestCaseResultItem>();
        }

        #region Items

        /// <summary>
        ///     Gets the sub items that need to be run, just after this test. (this is to check for respones).
        /// </summary>
        public Data.CascadedObservedCollection<TestCaseItem> Items { get; private set; }

        #endregion

        #region InputString

        /// <summary>
        ///     Gets/sets the input string that needs to be send to the textsin for this test.
        /// </summary>
        public string InputString
        {
            get
            {
                return fInputString;
            }

            set
            {
                if (value != fInputString)
                {
                    OnPropertyChanging("InputString", fInputString, value);
                    fInputString = value;
                    OnPropertyChanged("InputString");
                    UpdateRootChanged();
                }
            }
        }

        #endregion

        #region ValidationRegex

        /// <summary>
        ///     Gets/sets the Regex definition that should be used to validate the output that was rendered as a result of the
        ///     input.
        /// </summary>
        public string ValidationRegex
        {
            get
            {
                return fValidationRegex;
            }

            set
            {
                if (value != fValidationRegex)
                {
                    OnPropertyChanging("ValidationRegex", fValidationRegex, value);
                    fValidationRegex = value;
                    OnPropertyChanged("ValidationRegex");
                    UpdateRootChanged();
                }
            }
        }

        #endregion

        #region IsEnabled

        /// <summary>
        ///     Gets/sets the wether this test should be run or not.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return fIsEnabled;
            }

            set
            {
                OnPropertyChanging("IsEnabled", fIsEnabled, value);
                fIsEnabled = value;
                OnPropertyChanged("IsEnabled");
                UpdateRootChanged();
            }
        }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets/sets wether this test is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return fIsRunning;
            }

            set
            {
                if (fIsRunning != value)
                {
                    fIsRunning = value;
                    OnPropertyChanged("IsRunning");
                    if (value == false)
                    {
                        OnPropertyChanged("LastResult");

                            // when we change from running to non running, we usually have some result to show.
                    }
                }
            }
        }

        #endregion

        #region Results

        /// <summary>
        ///     Gets the list of results that have already been gathered for this test item.
        /// </summary>
        /// <remarks>
        ///     The order of the results is the same for each result item in an entire test case, so that you can
        ///     easely switch.
        /// </remarks>
        public System.Collections.Generic.List<TestCaseResultItem> Results { get; private set; }

        #endregion

        /// <summary>
        ///     Gets the last result, for easy display.
        /// </summary>
        /// <value>The last result.</value>
        public TestCaseResultItem LastResult
        {
            get
            {
                if (Results != null && Results.Count > 0)
                {
                    return Results[Results.Count - 1];
                }

                return null;
            }
        }

        #region Root

        /// <summary>
        ///     Gets the root testCase that owns this item.
        /// </summary>
        /// <value>The root.</value>
        public TestCase Root
        {
            get
            {
                var iOwner = Owner;
                while (iOwner != null)
                {
                    if (iOwner is TestCase)
                    {
                        return (TestCase)iOwner;
                    }

                    var iOwned = iOwner as Data.IOwnedObject;
                    iOwner = iOwned.Owner;
                }

                return null;
            }
        }

        #endregion

        /// <summary>Gets or sets a value indicating whether needs focus.</summary>
        public bool NeedsFocus
        {
            get
            {
                var iPrev = fNeedsFocus;
                fNeedsFocus = false; // when the focus got set, reset back to false so we can do it again later.
                return iPrev;
            }

            set
            {
                if (fNeedsFocus != value)
                {
                    fNeedsFocus = value;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "fNeedsFocus");
                }
            }
        }

        /// <summary>The add result.</summary>
        /// <param name="toAdd">The to add.</param>
        internal void AddResult(TestCaseResultItem toAdd)
        {
            Results.Add(toAdd);
            OnPropertyChanged("LastResult");
        }

        /// <summary>
        ///     for prop changes: Update the root for Ischanged.
        /// </summary>
        private void UpdateRootChanged()
        {
            var iRoot = Root;
            if (iRoot != null)
            {
                iRoot.IsChanged = true;
            }
        }

        /// <summary>Handles the CollectionChanged event of the fItems control.
        ///     need to raise the "HasChildren' when appropriate.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing
        ///     the event data.</param>
        private void fItems_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && Items.Count == 0)
                || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
                || (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                    && Items.Count - e.NewItems.Count == 0))
            {
                OnPropertyChanged("HasChildren");
            }
        }

        #region Fields

        /// <summary>The f input string.</summary>
        private string fInputString;

        /// <summary>The f validation regex.</summary>
        private string fValidationRegex;

        /// <summary>The f is enabled.</summary>
        private bool fIsEnabled = true;

        /// <summary>The f is running.</summary>
        private bool fIsRunning;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f needs bring into view.</summary>
        private bool fNeedsBringIntoView;

        #endregion

        #region Events  (NotifyCascadedPropertyChanged Members + ICascadedNotifyCollectionChanged Members)

        /// <summary>
        ///     Occurs when a property was changed in one of the thesaurus items. This is used for the tree display (only root
        ///     objects get events).
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when a collection was changed in one of the child items or the root list. This is used for the tree display
        ///     (only root objects get events).
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region Ui props

        // these props should be removed and put at the UI level. But for now, it's just faster doing it like this.

        /// <summary>The f test width.</summary>
        private double fTestWidth;

        #region TestWidth

        /// <summary>
        ///     Gets/sets the width of the test column
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

        /// <summary>The f verify width.</summary>
        private double fVerifyWidth;

        #region VerifyWidth

        /// <summary>
        ///     Gets/sets the width of the verify col
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

        /// <summary>The f result width.</summary>
        private double fResultWidth;

        #region ResultWidth

        /// <summary>
        ///     Gets/sets the width of the output col
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

        /// <summary>The f is enabled width.</summary>
        private double fIsEnabledWidth;

        #region IsEnabledWidth

        /// <summary>
        ///     Gets/sets the width of the duration col
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

        /// <summary>The f run width.</summary>
        private double fRunWidth;

        #region RunWidth

        /// <summary>
        ///     Gets/sets the width of  the treads col
        /// </summary>
        public double RunWidth
        {
            get
            {
                return fRunWidth;
            }

            set
            {
                fRunWidth = value;
                OnPropertyChanged("RunWidth");
            }
        }

        #endregion

        #endregion

        #region ITreeViewPanelItem Members

        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (fIsExpanded != value)
                {
                    SetIsExpanded(value);
                }
            }
        }

        /// <summary>Sets the isExpanded. Used so we can call async.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetIsExpanded(bool value)
        {
            fIsExpanded = value;
            OnPropertyChanged("IsExpanded");
            var iArgs = new Data.CascadedPropertyChangedEventArgs(
                this, 
                new System.ComponentModel.PropertyChangedEventArgs("IsExpanded"));
            Data.EventEngine.OnPropertyChanged(this, iArgs);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is selected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    SetSelected(value);
                    var iRoot = Root;
                    if (iRoot != null)
                    {
                        if (iRoot.SelectedItem != null && iRoot.SelectedItem != this)
                        {
                            iRoot.SelectedItem.IsSelected = false;
                        }

                        iRoot.SelectedItem = value ? this : null;
                    }
                }
            }
        }

        /// <summary>Sets the selected value without updating the  owner (called by the owner).</summary>
        /// <param name="value">The value.</param>
        internal void SetSelected(bool value)
        {
            if (value != fIsSelected)
            {
                // check for this, when 'IsSelected is set directly, this gets called 2 times, don't need this, faster to use 2 'if's
                fIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not. When the
        ///     list of children changes (becomes empty or gets the first item), this should
        ///     be raised when appropriate through a propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren
        {
            get
            {
                return Items != null && Items.Count > 0;
            }
        }

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>The tree items.</value>
        public System.Collections.IList TreeItems
        {
            get
            {
                return Items;
            }
        }

        /// <summary>
        ///     Gets the parent tree item.
        /// </summary>
        /// <value>The parent tree item.</value>
        public WPF.Controls.ITreeViewPanelItem ParentTreeItem
        {
            get
            {
                return Owner as WPF.Controls.ITreeViewPanelItem;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this object needs to be brought into view.
        /// </summary>
        /// <value><c>true</c> if [needs bring into view]; otherwise, <c>false</c>.</value>
        /// <remarks>
        ///     This needs to be a simple field set/get with propertyChanged call. The treeViewPanel uses this
        ///     to respond to it. It will also toglle it back off again when the operation is done.
        /// </remarks>
        public bool NeedsBringIntoView
        {
            get
            {
                return fNeedsBringIntoView;
            }

            set
            {
                fNeedsBringIntoView = value;
                var iArgs = new Data.CascadedPropertyChangedEventArgs(
                    this, 
                    new System.ComponentModel.PropertyChangedEventArgs("NeedsBringIntoView"));
                Data.EventEngine.OnPropertyChanged(this, iArgs);
                OnPropertyChanged("NeedsBringIntoView");
            }
        }

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }

            var iSource = args.OriginalSource as System.Collections.IList;
            if (iSource != null)
            {
                if (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                        && iSource.Count == 1)
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                        && iSource.Count == 0))
                {
                    OnPropertyChanged("HasChildren");
                }
            }
            else
            {
                OnPropertyChanged("HasChildren");

                    // if we can't check agains the nr of existing children, always let it update.
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event.</summary>
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
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
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

            InputString = XmlStore.ReadElement<string>(reader, "InputString");
            ValidationRegex = XmlStore.ReadElement<string>(reader, "ValidationRegex");
            IsEnabled = XmlStore.ReadElement<bool>(reader, "IsEnabled");

            var iLinkSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCaseItem));
            if (reader.Name == "SubItems")
            {
                reader.Read();
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement && reader.Name == "item")
                {
                    // the end element of  'item' is of the previous one.
                    var iNode = (TestCaseItem)iLinkSer.Deserialize(reader);
                    if (iNode == null)
                    {
                        // if for some reason, we failed to read the item, log an error, and advance to the next item so that we don't get in a loop.
                        LogService.Log.LogError(
                            "TestCaseItem.ReadXml", 
                            string.Format("Failed to read xml element {0} in stream.", reader.Name));
                        reader.Skip();
                    }
                    else
                    {
                        Items.Add(iNode);
                    }

                    // reader.ReadEndElement();
                }

                if (reader.NodeType == System.Xml.XmlNodeType.EndElement && reader.Name == "SubItems")
                {
                    reader.ReadEndElement(); // need to  read an extra end if there were subItems.
                }

                reader.ReadEndElement();
            }
            else
            {
                LogService.Log.LogError(
                    "TestCaseItem.ReadXml", 
                    string.Format("Failed to read xml element {0} in stream.", reader.Name));
                reader.Skip();
            }

            // reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "InputString", InputString);
            XmlStore.WriteElement(writer, "ValidationRegex", ValidationRegex);
            XmlStore.WriteElement(writer, "IsEnabled", IsEnabled);

            writer.WriteStartElement("SubItems");

            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(TestCaseItem));
            foreach (var i in Items)
            {
                iSer.Serialize(writer, i);
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}