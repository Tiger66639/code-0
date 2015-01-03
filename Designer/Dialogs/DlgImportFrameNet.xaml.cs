// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgImportFrameNet.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgImportFrameNet.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for DlgImportFrameNet.xaml
    /// </summary>
    public partial class DlgImportFrameNet : System.Windows.Window
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DlgImportFrameNet"/> class. 
        ///     Initializes a new instance of the <see cref="DlgImportFrameNet"/>
        ///     class. Loads the dat if</summary>
        public DlgImportFrameNet()
        {
            InitializeComponent();
        }

        #endregion

        /// <summary>The search next_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SearchNext_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iToSearch = TxtSearch.Text;
            for (var i = iSearchIndex + 1; i < fFrameNet.Frames.Count; i++)
            {
                var iFrame = fFrameNet.Frames[i];
                if (iFrame.Description.Contains(iToSearch) || iFrame.Name.Contains(iToSearch))
                {
                    LstFrames.SelectedIndex = i;
                    LstFrames.ScrollIntoView(LstFrames.SelectedItem);
                    iSearchIndex = i;
                    return;
                }
            }

            System.Windows.MessageBox.Show("End reached, going to start.");
            iSearchIndex = -1;
        }

        /// <summary>Called when ok is clicked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSelected = (from i in fFrameNet.Frames where i.IsSelected select i).ToList();

                // we convert to list so that we only have to walk through the intire list 1 time.
            if (CheckWordNetLinks(iSelected))
            {
                // all items check out ok.
                var iImportInto = ImportInto;
                foreach (var iFrame in iSelected)
                {
                    NeuronCluster iEvokers;
                    var iNew = iFrame.Import(out iEvokers);
                    if (iImportInto != null)
                    {
                        // if we
                        iImportInto.Frames.Add(new Frame(iNew));
                    }

                    var iInfo = BrainData.Current.NeuronInfo[iNew.ID];
                    iInfo.StoreDescription(iFrame.Description); // set the description
                    iInfo.DisplayTitle = iFrame.Name;
                    var i = 0;
                    foreach (var iEl in iFrame.Elements)
                    {
                        // set the info for all the frame elements (objects in neural network)
                        using (var iChildren = iNew.Children) iInfo = BrainData.Current.NeuronInfo[iChildren[i++]]; // childAccessor[] is threadsave.
                        iInfo.StoreDescription(iEl.Description); // set the description
                        iInfo.DisplayTitle = iEl.Name;
                    }

                    i = 0;
                    using (var iEvokersList = iEvokers.Children)
                    {
                        foreach (var iLex in iFrame.LexUnits)
                        {
                            // set the info for all the lexical units (objects in neural network)
                            if (iLex.Lexemes.Count == 1)
                            {
                                // if there is more than 1 lexeme, we always use the lexemes as evokers (the word parts) cause they are easier to search by the neural net.  This means that we must keep the iEvokers list in sync.
                                iInfo = BrainData.Current.NeuronInfo[iEvokersList[i++]];
                                iInfo.StoreDescription(iLex.Description);
                                iInfo.DisplayTitle = iLex.Name;
                            }
                            else
                            {
                                foreach (var iLexeme in iLex.Lexemes)
                                {
                                    iInfo = BrainData.Current.NeuronInfo[iEvokersList[i++]];
                                    var iDesc = string.Format(
                                        "Part of Lexical unit {0} ({1}), desc: {2}", 
                                        iLex.ID, 
                                        iLex.Name, 
                                        iLex.Description);
                                    iInfo.StoreDescription(iDesc);
                                    iInfo.DisplayTitle = iLexeme.Value;
                                }
                            }
                        }
                    }
                }

                System.Windows.MessageBox.Show(
                    "Import finished successfully!", 
                    "Import from FrameNet", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
                Close();
            }
        }

        /// <summary>Checks if all the word net links have been filled in. If not, an mbox
        ///     is displayed so the user can update, if needed.</summary>
        /// <param name="list">The list.</param>
        /// <returns>True: all items check out ok or user chose to ignore, false: some</returns>
        private bool CheckWordNetLinks(System.Collections.Generic.List<Framenet.Frame> list)
        {
            foreach (var iFrame in list)
            {
                foreach (var iLu in iFrame.LexUnits)
                {
                    foreach (var iLex in iLu.Lexemes)
                    {
                        if (iLex.WordNetID == 0)
                        {
                            var iRes =
                                System.Windows.MessageBox.Show(
                                    string.Format(
                                        "Lexeme {2} ({3}), in Lexical unit {1} ({0} has no mapping to wordnet, Continue?", 
                                        iLu.ID, 
                                        iLu.Name, 
                                        iLex.Value, 
                                        iLex.ID), 
                                    "Missing mapping", 
                                    System.Windows.MessageBoxButton.YesNo, 
                                    System.Windows.MessageBoxImage.Warning, 
                                    System.Windows.MessageBoxResult.No);
                            if (iRes == System.Windows.MessageBoxResult.No)
                            {
                                return false;
                            }
                        }
                    }

                    if (iLu.WordNetID == 0)
                    {
                        var iRes =
                            System.Windows.MessageBox.Show(
                                string.Format(
                                    "Lexical unit {0} ({1} has no mapping to wordnet, Continue?", 
                                    iLu.ID, 
                                    iLu.Name), 
                                "Missing mapping", 
                                System.Windows.MessageBoxButton.YesNo, 
                                System.Windows.MessageBoxImage.Warning, 
                                System.Windows.MessageBoxResult.No);
                        if (iRes == System.Windows.MessageBoxResult.No)
                        {
                            return false;
                        }
                    }
                }

                foreach (var iEl in iFrame.Elements)
                {
                    if (iEl.WordNetID == 0)
                    {
                        var iRes =
                            System.Windows.MessageBox.Show(
                                string.Format(
                                    "Frame element {0} ({1} has no mapping to wordnet, Continue?", 
                                    iEl.ID, 
                                    iEl.Name), 
                                "Missing mapping", 
                                System.Windows.MessageBoxButton.YesNo, 
                                System.Windows.MessageBoxImage.Warning, 
                                System.Windows.MessageBoxResult.No);
                        if (iRes == System.Windows.MessageBoxResult.No)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>Handles the Click event of the BtnCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>The txt search_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchNext_Click(sender, null);
            }
            else
            {
                iSearchIndex = -1; // when we search for a new word,start from the beginning.
            }
        }

        /// <summary>Handles the Indeterminate event of the ChkSelectAl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkSelectAl_Indeterminate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (fSelected != null)
            {
                // this event is triggered whenever an 'IsSelected' checkbox is clicked because the ChkSelectAll is reset.  Only need to reset when the sender is Chk
                foreach (var i in fFrameNet.Frames)
                {
                    i.IsSelected = false;
                }

                foreach (var i in fSelected)
                {
                    i.IsSelected = true;
                }

                fSelected = null;
            }
        }

        /// <summary>Handles the Unchecked event of the ChkSelectAl control.</summary>
        /// <remarks>Follows indeterminate: so fill the selection list.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkSelectAl_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = (from i in fFrameNet.Frames where i.IsSelected select i).ToList();
            foreach (var i in fFrameNet.Frames)
            {
                i.IsSelected = false;
            }
        }

        /// <summary>Handles the Checked event of the ChkSelectAl control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkSelectAl_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var i in fFrameNet.Frames)
            {
                i.IsSelected = true;
            }
        }

        /// <summary>Handles the Click event of the ChkIsSelected control.</summary>
        /// <remarks>Must make certain that the ChkSelectAll is kept in sync: whenever an
        ///     item is changed in the selection: we set the item to indetermined.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkIsSelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = null; // whenever we click on an item, the previous selection is undone
            ChkSelectAll.IsChecked = null;
        }

        /// <summary>Handles the Closing event of the Window control.</summary>
        /// <remarks>Need to save the mappings to wordnet!</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (fFrameNet != null)
            {
                var iRes = System.Windows.MessageBoxResult.None;

                    // we init to invalid state, this way, we only have to ask 1 time for 2 lists -> we can check if it has already been asked.
                if (fFrameNet.WordNetMapLUChanged)
                {
                    iRes =
                        System.Windows.MessageBox.Show(
                            "The mappings to wordnet have changed, would you like to save them?", 
                            "Mapping from FrameNet to WordNet", 
                            System.Windows.MessageBoxButton.YesNoCancel, 
                            System.Windows.MessageBoxImage.Question, 
                            System.Windows.MessageBoxResult.Yes);
                    if (iRes == System.Windows.MessageBoxResult.Yes)
                    {
                        var iPath = System.IO.Path.Combine(
                            Properties.Settings.Default.FrameNetPath, 
                            "BasicXml\\frXML\\LUMapToWordnet.xml");
                        var iSer =
                            new System.Xml.Serialization.XmlSerializer(typeof(Data.SerializableDictionary<int, int>));
                        using (var iWriter = new System.Xml.XmlTextWriter(iPath, System.Text.Encoding.Default)) iSer.Serialize(iWriter, fFrameNet.WordNetMapLU);
                    }
                    else if (iRes == System.Windows.MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (fFrameNet.WordNetMapFEChanged)
                {
                    if (iRes == System.Windows.MessageBoxResult.None)
                    {
                        iRes =
                            System.Windows.MessageBox.Show(
                                "The mappings to wordnet have changed, would you like to save them?", 
                                "Mapping from FrameNet to WordNet", 
                                System.Windows.MessageBoxButton.YesNoCancel, 
                                System.Windows.MessageBoxImage.Question, 
                                System.Windows.MessageBoxResult.Yes);
                    }

                    if (iRes == System.Windows.MessageBoxResult.Yes)
                    {
                        var iPath = System.IO.Path.Combine(
                            Properties.Settings.Default.FrameNetPath, 
                            "BasicXml\\frXML\\FEMapToWordnet.xml");
                        var iSer =
                            new System.Xml.Serialization.XmlSerializer(typeof(Data.SerializableDictionary<int, int>));
                        using (var iWriter = new System.Xml.XmlTextWriter(iPath, System.Text.Encoding.Default)) iSer.Serialize(iWriter, fFrameNet.WordNetMapFE);
                    }
                    else if (iRes == System.Windows.MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        /// <summary>The new frame editor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void NewFrameEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (BrainData.Current.CurrentEditorsList != null)
            {
                var iNew = new FrameEditor();
                iNew.Name = "New frame editor";
                BrainData.Current.CurrentEditorsList.Add(iNew);
                if (fFrameEditors != null)
                {
                    // need to make it available to the dialog as well.
                    fFrameEditors.Add(iNew);
                }

                ImportInto = iNew;
            }
            else
            {
                throw new System.InvalidOperationException(Properties.Resources.NoProjectTreeItemSelected);
            }
        }

        #region fields

        /// <summary>The f frame net.</summary>
        private Framenet.FrameNet fFrameNet;

        /// <summary>The i search index.</summary>
        private int iSearchIndex = -1;

        /// <summary>The f selected.</summary>
        private System.Collections.Generic.List<Framenet.Frame> fSelected;

                                                                // required to reset previous  selection when select all checkbox is indeterminate. 

        /// <summary>The f frame editors.</summary>
        private System.Collections.ObjectModel.ObservableCollection<FrameEditor> fFrameEditors;

        #endregion

        #region prop

        #region ShowImportInto

        /// <summary>
        ///     <see cref="ShowImportInto" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ShowImportIntoProperty =
            System.Windows.DependencyProperty.Register(
                "ShowImportInto", 
                typeof(bool), 
                typeof(DlgImportFrameNet), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets the <see cref="ShowImportInto" /> property. This
        ///     dependency property indicates wether the 'Import into' drop down box
        ///     is displayed or not.
        /// </summary>
        public bool ShowImportInto
        {
            get
            {
                return (bool)GetValue(ShowImportIntoProperty);
            }

            set
            {
                SetValue(ShowImportIntoProperty, value);
            }
        }

        #endregion

        #region ImportInto

        /// <summary>
        ///     <see cref="ImportInto" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ImportIntoProperty =
            System.Windows.DependencyProperty.Register(
                "ImportInto", 
                typeof(FrameEditor), 
                typeof(DlgImportFrameNet), 
                new System.Windows.FrameworkPropertyMetadata((FrameEditor)null));

        /// <summary>
        ///     Gets or sets the <see cref="ImportInto" /> property. This dependency
        ///     property indicates into which <see cref="FrameEditor" /> the frames
        ///     should be imported. Not required for import to work.
        /// </summary>
        public FrameEditor ImportInto
        {
            get
            {
                return (FrameEditor)GetValue(ImportIntoProperty);
            }

            set
            {
                SetValue(ImportIntoProperty, value);
            }
        }

        #endregion

        #region FrameEditors

        /// <summary>
        ///     Gets the list of available frames into which the item can be imported.
        /// </summary>
        public System.Collections.Generic.IList<FrameEditor> FrameEditors
        {
            get
            {
                if (fFrameEditors == null)
                {
                    fFrameEditors = new System.Collections.ObjectModel.ObservableCollection<FrameEditor>();
                    GetFrames(BrainData.Current.Editors, fFrameEditors);
                }

                return fFrameEditors;
            }
        }

        /// <summary>The get frames.</summary>
        /// <param name="list">The list.</param>
        /// <param name="res">The res.</param>
        private void GetFrames(System.Collections.Generic.IList<EditorBase> list, System.Collections.Generic.IList<FrameEditor> res)
        {
            foreach (var i in list)
            {
                if (i is FrameEditor)
                {
                    res.Add((FrameEditor)i);
                }
                else if (i is EditorFolder)
                {
                    GetFrames(((EditorFolder)i).Items, res);
                }
            }
        }

        #endregion

        #endregion

        #region load

        /// <summary>The load frame net.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool LoadFrameNet()
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.FrameNetPath) == false)
                {
                    LoadFrames();
                    LoadLUMaps();
                    LoadFEMaps();
                    DataContext = fFrameNet;
                    return true;
                }

                LogService.Log.LogError(
                    "FrameNetSin.FrameNet", 
                    "No location specified where to find the framenet data.");
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    "Failed to load FrameNet database!", 
                    "Import from FrameNet", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                LogService.Log.LogError("DlgImportFrameNet.LoadFrameNet", e.ToString());
            }

            return false;
        }

        /// <summary>
        ///     Loads the file: 'BasicXml\\frXML\\frames.xml'
        /// </summary>
        private void LoadFEMaps()
        {
            if (fFrameNet != null)
            {
                var iPath = System.IO.Path.Combine(Properties.Settings.Default.FrameNetPath, "FEMapToWordnet.xml");
                if (System.IO.File.Exists(iPath))
                {
                    var valueSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Data.SerializableDictionary<int, int>));
                    using (var iReader = System.Xml.XmlReader.Create(iPath))
                        fFrameNet.WordNetMapFE =
                            (System.Collections.Generic.IDictionary<int, int>)valueSerializer.Deserialize(iReader);
                }
                else
                {
                    fFrameNet.WordNetMapFE = new Data.SerializableDictionary<int, int>();
                }
            }
        }

        /// <summary>
        ///     Loads the file: 'BasicXml\\frXML\\LUMapToWordnet.xml'
        /// </summary>
        private void LoadLUMaps()
        {
            if (fFrameNet != null)
            {
                var iPath = System.IO.Path.Combine(Properties.Settings.Default.FrameNetPath, "LUMapToWordnet.xml");
                if (System.IO.File.Exists(iPath))
                {
                    var valueSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Data.SerializableDictionary<int, int>));
                    using (var iReader = System.Xml.XmlReader.Create(iPath))
                        fFrameNet.WordNetMapLU =
                            (System.Collections.Generic.IDictionary<int, int>)valueSerializer.Deserialize(iReader);
                }
                else
                {
                    fFrameNet.WordNetMapLU = new Data.SerializableDictionary<int, int>();
                }
            }
        }

        /// <summary>
        ///     Loads the file: 'BasicXml\\frXML\\frames.xml'
        /// </summary>
        private void LoadFrames()
        {
            var iPath = System.IO.Path.Combine(Properties.Settings.Default.FrameNetPath, "frames.xml");
            if (System.IO.File.Exists(iPath))
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Framenet.FrameNet));
                var iSettings = new System.Xml.XmlReaderSettings { DtdProcessing = System.Xml.DtdProcessing.Parse };

                    // framenet contains a dtd.  We need to change default reader settings of .net xmlreader to allow for this.
                using (var iReader = System.Xml.XmlReader.Create(iPath, iSettings)) fFrameNet = (Framenet.FrameNet)valueSerializer.Deserialize(iReader);
            }
            else
            {
                throw new System.InvalidOperationException(string.Format("Can't find {0}.", iPath));
            }
        }

        #endregion
    }
}