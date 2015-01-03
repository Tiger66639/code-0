// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowMain.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for Window1.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class WindowMain : System.Windows.Window, System.Windows.IWeakEventListener
    {
        #region Current

        /// <summary>
        ///     Gets the current main window.
        /// </summary>
        public static WindowMain Current
        {
            get
            {
                return (WindowMain)System.Windows.Application.Current.MainWindow;
            }
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(BeforeSaveEventManager))
            {
                FlushDescriptionData(
                    System.Windows.Input.Keyboard.FocusedElement as System.Windows.Controls.RichTextBox);

                    // Whenever the data gets saved, we need to make certain that everything gets flushed ok, like the description box.
                return true;
            }

            return false;
        }

        #endregion

        /// <summary>The dump_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Dump_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // ProcessorFactory.DebugPrintNlistUsage();
            // Brain.Current.DumpLocks();
            ToolsList.Default.DescriptionTool.IsVisible = true;
            ToolsList.Default.DescriptionTool.IsActive = true;

            // ProjectManager.DumpStacks();
        }

        /// <summary>Syncs the explorer to specified neuron.</summary>
        /// <param name="iNeuron">The i neuron.</param>
        public void SyncExplorerToNeuron(Neuron iNeuron)
        {
            if (iNeuron != null)
            {
                ActivateTool(ToolsList.Default.ExplorerTool);

                    // use this instead of activate, to make certain it is made visible

                // ContentExplorer.Activate();
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<ulong>(SyncMainExplorerTo), 
                    iNeuron.ID);
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }
        }

        /// <summary>Syncs the main explorer to the specified id. Allows us to do this
        ///     async.</summary>
        /// <param name="id">The id.</param>
        private void SyncMainExplorerTo(ulong id)
        {
            var iExplorer = NeuronExplorerView.Default.DataContext as NeuronExplorer;

            iExplorer.Selection.SelectedID = id;
            NeuronExplorerView.Default.LstItems.Focus();
        }

        /// <summary>Shows the code for the neuron.</summary>
        /// <param name="neuron">The euron.</param>
        /// <returns>The code editor that represents the code.</returns>
        public CodeEditor ViewCodeForNeuron(Neuron neuron)
        {
            CodeEditor iEditor = null;
            if (neuron != null)
            {
                iEditor = (from i in BrainData.Current.CodeEditors where i.Item == neuron select i).FirstOrDefault();

                    // we first check if the code editor is already open.
                if (iEditor == null)
                {
                    iEditor = new CodeEditor(neuron);
                    iEditor.Name = BrainData.Current.NeuronInfo[neuron].DisplayTitle;

                        // used to be done automatically, not anymore for sub editors, so do it manually for items that we need to create.
                    BrainData.Current.CodeEditors.Add(iEditor);
                    BrainData.Current.OpenDocuments.Add(iEditor);
                }
                else
                {
                    if (BrainData.Current.OpenDocuments.Contains(iEditor) == false)
                    {
                        BrainData.Current.OpenDocuments.Add(iEditor);
                    }
                }

                BrainData.Current.ActiveDocument = iEditor;
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }

            return iEditor;
        }

        /// <summary>Shows the code for the neuron.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The code editor that represents the code.</returns>
        public TextPatternEditor ViewPatternEditor(NeuronCluster cluster)
        {
            TextPatternEditor iEditor = null;
            if (cluster != null)
            {
                iEditor =
                    (from i in BrainData.Current.Editors.AllTextPatternEditorsClosed() where i.Item == cluster select i)
                        .FirstOrDefault(); // we first check if the code editor is already open.
                if (iEditor == null)
                {
                    iEditor = new TextPatternEditor(cluster);
                    if (string.IsNullOrEmpty(iEditor.NeuronInfo.Title))
                    {
                        // make certain we have a unique name.
                        iEditor.Name = Parsers.TopicsDictionary.GetUnique("New topic ");
                    }
                    else
                    {
                        iEditor.Name = Parsers.TopicsDictionary.GetUnique(iEditor.NeuronInfo.DisplayTitle);
                    }

                    AddItemToOpenDocuments(iEditor);
                }
                else
                {
                    if (BrainData.Current.OpenDocuments.Contains(iEditor) == false)
                    {
                        BrainData.Current.OpenDocuments.Add(iEditor);
                    }

                    BrainData.Current.ActiveDocument = iEditor;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected patterns defenition!");
            }

            return iEditor;
        }

        /// <summary>Shows the asset editor for the <paramref name="neuron"/> (asets
        ///     attached to the neuron).</summary>
        /// <param name="neuron">The euron.</param>
        /// <returns>The object-asset editor .</returns>
        public ObjectEditor ViewAssetsForNeuron(Neuron neuron)
        {
            ObjectEditor iEditor = null;
            if (neuron != null)
            {
                foreach (var i in BrainData.Current.OpenDocuments)
                {
                    // check if the editor is already open.
                    var iEdit = i as ObjectEditor;
                    if (iEdit != null && iEdit.Item == neuron)
                    {
                        iEditor = iEdit;
                        break;
                    }
                }

                if (iEditor == null)
                {
                    var iCluster = neuron as NeuronCluster;
                    if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Asset)
                    {
                        iEditor = new AssetEditor(iCluster);
                    }
                    else
                    {
                        iEditor = new ObjectEditor(neuron);
                    }

                    iEditor.Name = BrainData.Current.NeuronInfo[neuron].DisplayTitle;

                        // used to be done automatically, not anymore for sub editors, so do it manually for items that we need to create.
                    AddItemToOpenDocuments(iEditor);
                }
                else if (BrainData.Current.OpenDocuments.Contains(iEditor) == false)
                {
                    BrainData.Current.OpenDocuments.Add(iEditor);
                    BrainData.Current.ActiveDocument = iEditor;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }

            return iEditor;
        }

        /// <summary>Shows the object-frames editor for the <paramref name="neuron"/>
        ///     (frames attached to the neuron).</summary>
        /// <param name="neuron">The euron.</param>
        /// <returns>The object-asset editor .</returns>
        public ObjectFramesEditor ViewFramesForNeuron(Neuron neuron)
        {
            ObjectFramesEditor iEditor = null;
            if (neuron != null)
            {
                foreach (var i in BrainData.Current.OpenDocuments)
                {
                    // check if the editor is already open.
                    var iEdit = i as ObjectFramesEditor;
                    if (iEdit != null && iEdit.Item == neuron)
                    {
                        iEditor = iEdit;
                        break;
                    }
                }

                if (iEditor == null)
                {
                    iEditor = new ObjectFramesEditor(neuron);
                    iEditor.Name = BrainData.Current.NeuronInfo[neuron].DisplayTitle;

                        // used to be done automatically, not anymore for sub editors, so do it manually for items that we need to create.
                    AddItemToOpenDocuments(iEditor);
                }
                else if (BrainData.Current.OpenDocuments.Contains(iEditor) == false)
                {
                    // don't know if this is really necessary?
                    BrainData.Current.OpenDocuments.Add(iEditor);
                    BrainData.Current.ActiveDocument = iEditor;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }

            return iEditor;
        }

        /// <summary>Shows the object-frames editor for the <paramref name="neuron"/>
        ///     (frames attached to the neuron).</summary>
        /// <param name="neuron">The euron.</param>
        /// <returns>The object-asset editor .</returns>
        public ObjectTextPatternEditor ViewPatternsForNeuron(Neuron neuron)
        {
            ObjectTextPatternEditor iEditor = null;
            if (neuron != null)
            {
                foreach (var i in BrainData.Current.OpenDocuments)
                {
                    // check if the editor is already open.
                    var iEdit = i as ObjectTextPatternEditor;
                    if (iEdit != null && iEdit.Item == neuron)
                    {
                        iEditor = iEdit;
                        break;
                    }
                }

                if (iEditor == null)
                {
                    iEditor =
                        (from i in BrainData.Current.Editors.AllTextPatternEditors()
                         where i.Item == neuron
                         select i as ObjectTextPatternEditor).FirstOrDefault();
                    if (iEditor == null)
                    {
                        iEditor = new ObjectTextPatternEditor(neuron);
                        iEditor.Name = BrainData.Current.NeuronInfo[neuron].DisplayTitle;
                        AddItemToOpenDocuments(iEditor);

                        // iEditor.ShowInProjectRoot();                                                  //topics attached to other neurons are automatically added to the projec, so we can find them easely again.
                    }
                }

                if (BrainData.Current.OpenDocuments.Contains(iEditor) == false)
                {
                    // in case
                    BrainData.Current.OpenDocuments.Add(iEditor);
                    BrainData.Current.ActiveDocument = iEditor;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }

            return iEditor;
        }

        /// <summary>Shows the chatbot's properties page.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ShowProperties_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ShowChatbotProps();
        }

        /// <summary>The show chatbot props.</summary>
        public void ShowChatbotProps()
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            try
            {
                if (BrainData.Current.ChatbotProps == null)
                {
                    BrainData.Current.ChatbotProps = new ChatbotProperties();
                    AddItemToOpenDocuments(BrainData.Current.ChatbotProps);
                }
                else
                {
                    BrainData.Current.ActiveDocument = BrainData.Current.ChatbotProps;
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<System.Windows.Input.Cursor>(ResetCursor), 
                    null); // we do this async cause of the loading, which can be done async.
            }
        }

        /// <summary>Resets the cursor. Used to reset it async.</summary>
        /// <param name="prev">The prev.</param>
        private void ResetCursor(System.Windows.Input.Cursor prev)
        {
            System.Windows.Input.Mouse.OverrideCursor = prev;
        }

        /// <summary>Used to <see langword="switch"/> button on the toolbar for the
        ///     dropdown menu.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void WrapPanel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.Button;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iSenderImage = iSender.Content as System.Windows.Controls.Image;
            System.Diagnostics.Debug.Assert(iSenderImage != null);
            BtnMainNewSelector.Command = iSender.Command;
            BtnMainNewSelector.ToolTip = iSender.ToolTip;
            ImageMainNewSelector.Source = iSenderImage.Source;
            ToggleShowNewSelectors.IsChecked = false; // close the popup
        }

        /// <summary>callback for the commchannels collectionSourceView.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Data.FilterEventArgs"/> instance containing the event data.</param>
        private void CommChannelsFilter(object sender, System.Windows.Data.FilterEventArgs e)
        {
#if PRO
            e.Accepted = e.Item is ChatBotChannel || e.Item is ReflectionChannel;
#else
         e.Accepted = true;
#endif
        }

        /// <summary>The mnu item manage android_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuItemManageAndroid_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgManageAndroid();
            iDlg.ShowDialog();
        }

        /// <summary>The install android cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InstallAndroidCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            AndroidManager.InstallApp();
        }

        /// <summary>The upload android cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UploadAndroidCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            AndroidManager.UploadCurrentProject();
        }

        /// <summary>a thest to see if the inport through wordnet sin works.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Export_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iList = new System.Collections.Generic.List<Neuron>();
            WordNetSin.Default.BeginProcess("aa", iList);

            string[] iPath = { "name", "given name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Jan", iList);

            iPath = new[] { "name", "family name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Bogaerts", iList);

            iPath = new[] { "name", "display name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Jakke", iList);

            WordNetSin.Default.BeginProcess("bb", iList);

            iPath = new[] { "name", "given name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Tom", iList);

            iPath = new[] { "name", "family name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Jones", iList);

            iPath = new[] { "name", "display name" };
            WordNetSin.Default.AddToProcess((ulong)PredefinedNeurons.Noun, iPath, "Tommy", iList);

            WordNetSin.Default.Process(iList, "test");
        }

        /// <summary>The window_loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Window_loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MainDock.AnchorablesSource = ToolsList.Default;

                // doing through code to make certain that props are only set 1 time (faster loading).

            // MainDock.DocumentsSource = BrainData.Current.OpenDocuments;
            RestoreLayout();
        }

        #region fields

        // IDescriptionable fCurrentDesc;                                                 //a ref to the object currently displayed in the global richtextbox description editor (so we can save the data again to it).
        /// <summary>The f current neuron info.</summary>
        private INeuronInfo fCurrentNeuronInfo;

                            // goes together with fCurrentDoc.  If this field is set, it indicates the neuroninfo from which we are depicting a description (doesn't have to be filled in).

        /// <summary>The f undo store.</summary>
        private static UndoSystem.UndoStore fUndoStore; // stores a reference to the undo store that should be used.

        /// <summary>The f help.</summary>
        private readonly ControlFramework.Controls.Help fHelp;

        #endregion

        #region ctor -dtor

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowMain" /> class.
        /// </summary>
        public WindowMain()
        {
            BeforeSaveEventManager.AddListener(BrainData.Current, this);
            fHelp = new ControlFramework.Controls.Help();
            fHelp.DefaultHelpFile =
                System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]), 
                    "NND.chm");
#if PRO
            DesignerVisibility = System.Windows.Visibility.Collapsed;
            if (ProjectManager.Default.IsViewerVisibility == System.Windows.Visibility.Collapsed)
            {
                // if we ar in viewer mode, hide it all.
                BasicVibility = System.Windows.Visibility.Collapsed;
                ProVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                OnlyProVisibility = System.Windows.Visibility.Visible;

                    // pro visibilitity is not for the viewer, this is the lowest.
            }

            Title = ProjectManager.SHORTAPPNAMEBASIC;
#else
         BasicVibility = Visibility.Collapsed;
         if (ProjectManager.Default.IsViewerVisibility == Visibility.Collapsed)         {

// if we ar in viewer mode, hide it all.
            DesignerVisibility = Visibility.Collapsed;
            ProVisibility = Visibility.Collapsed;
         }
         Title = ProjectManager.SHORTAPPNAME;
#endif
            InitializeComponent();

            // CreateInitDocs();
            GlobalCommands.Register(this); // add all the debug commands available in the app.

#if DEBUG
            BtnDump.Visibility = System.Windows.Visibility.Visible;
            BtnClean.Visibility = System.Windows.Visibility.Visible;
            MnuItemManageModules.Visibility = System.Windows.Visibility.Visible;
#endif
        }

        /// <summary>Finalizes an instance of the <see cref="WindowMain"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="WindowMain"/> is reclaimed by garbage
        ///     collection.</summary>
        ~WindowMain()
        {
            BeforeSaveEventManager.RemoveListener(BrainData.Current, this);
        }

        #endregion

        #region prop

        #region UndoSTores

        /// <summary>
        ///     Gets the globaal undo store.
        /// </summary>
        public static UndoSystem.UndoStore UndoStore
        {
            get
            {
                if (fUndoStore == null)
                {
                    fUndoStore = new UndoSystem.UndoStore();

                    // fUndoStore.UndoStoreChanged += fUndoStore_UndoStoreChanged;
                    fUndoStore.MaxSecBetweenUndo = 0;
                    fUndoStore.MaxUndoLevel = Properties.Settings.Default.MaxNrOfUndo;
                    fUndoStore.GetFocused += fUndoStore_GetFocused;
                    fUndoStore.SetFocused += fUndoStore_SetFocused;
                    if (BrainData.Current != null)
                    {
                        fUndoStore.Register(BrainData.Current);
                    }
                }

                return fUndoStore;
            }
        }

        #endregion

        #region DesignerVisibility

        /// <summary>
        ///     <see cref="DesignerVisibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty DesignerVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "DesignerVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(WindowMain), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Visible));

        /// <summary>
        ///     Gets or sets the <see cref="DesignerVisibility" /> property. This
        ///     dependency property indicates the visibility state of controls that
        ///     should only be visible in the full designer version.
        /// </summary>
        public System.Windows.Visibility DesignerVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(DesignerVisibilityProperty);
            }

            set
            {
                SetValue(DesignerVisibilityProperty, value);
            }
        }

        #endregion

        #region ProVisibility

        /// <summary>
        ///     <see cref="ProVisibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ProVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "ProVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(WindowMain), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Visible));

        /// <summary>
        ///     Gets or sets the <see cref="ProVisibility" /> property. This dependency
        ///     property indicates the visibiity state of controls that should only be
        ///     visible in the full desginer and pro chatbot version.
        /// </summary>
        public System.Windows.Visibility ProVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(ProVisibilityProperty);
            }

            set
            {
                SetValue(ProVisibilityProperty, value);
            }
        }

        #endregion

        #region OnlyProVisibility

        /// <summary>
        ///     <see cref="OnlyProVisibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty OnlyProVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "OnlyProVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(WindowMain), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        /// <summary>
        ///     Gets or sets the <see cref="OnlyProVisibility" /> property. This
        ///     dependency property indicates the visibility that should be applied to
        ///     objects that only should be visible in the pro version.
        /// </summary>
        public System.Windows.Visibility OnlyProVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(OnlyProVisibilityProperty);
            }

            set
            {
                SetValue(OnlyProVisibilityProperty, value);
            }
        }

        #endregion

        #region BasicVibility

        /// <summary>
        ///     <see cref="BasicVibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty BasicVibilityProperty =
            System.Windows.DependencyProperty.Register(
                "BasicVibility", 
                typeof(System.Windows.Visibility), 
                typeof(WindowMain), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Visible));

        /// <summary>
        ///     Gets or sets the <see cref="BasicVibility" /> property. This dependency
        ///     property indicates the visibility status for items that shoul only be
        ///     active <see langword="int" /> he 'Basic version..
        /// </summary>
        public System.Windows.Visibility BasicVibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(BasicVibilityProperty);
            }

            set
            {
                SetValue(BasicVibilityProperty, value);
            }
        }

        #endregion

        #region CurrentDescription

        /// <summary>
        ///     <see cref="CurrentDescription" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CurrentDescriptionProperty =
            System.Windows.DependencyProperty.Register(
                "CurrentDescription", 
                typeof(IDescriptionable), 
                typeof(WindowMain), 
                new System.Windows.FrameworkPropertyMetadata((IDescriptionable)null));

        /// <summary>
        ///     Gets or sets the <see cref="CurrentDescription" /> property. This
        ///     dependency property indicates the object that provides the data for
        ///     the description box. a <see langword="ref" /> to the object currently
        ///     displayed in the global richtextbox description editor (so we can save
        ///     the data again to it).
        /// </summary>
        public IDescriptionable CurrentDescription
        {
            get
            {
                return (IDescriptionable)GetValue(CurrentDescriptionProperty);
            }

            set
            {
                SetValue(CurrentDescriptionProperty, value);
            }
        }

        #endregion

        #endregion

        #region Helpers

        /// <summary>Flushes the description data back to where it came from.</summary>
        /// <param name="focused">The Richtextbox that was focused, if any.</param>
        internal void FlushDescriptionData(System.Windows.Controls.RichTextBox focused)
        {
            if (focused != null && DescriptionView.Current != null && DescriptionView.Current.IsChanged)
            {
                var iCurDesc = CurrentDescription;
                if (iCurDesc != null)
                {
                    iCurDesc.Description = DescriptionView.Current.Document;
                }
            }
        }

        /// <summary>Adds the item to brain taking care of the undo data.</summary>
        /// <param name="value">The neuron to add to the brain.</param>
        public static void AddItemToBrain(Neuron value)
        {
            Brain.Current.Add(value);
            var iUndoData = new NeuronUndoItem { Neuron = value, ID = value.ID, Action = BrainAction.Created };
            UndoStore.AddCustomUndoItem(iUndoData);
        }

        /// <summary>Deletes the item from brain taking care of the undo data.</summary>
        /// <param name="toDelete">The neuron to delete from the brain.</param>
        public static void DeleteItemFromBrain(Neuron toDelete)
        {
            if (toDelete.CanBeDeleted)
            {
                UndoStore.BeginUndoGroup(false, false);

                    // we make certain that we don't reverse the order, for a delete instruction. If we would do this, we would mess up all the updates.
                try
                {
                    var iUndoData = new NeuronUndoItem
                                        {
                                            Neuron = toDelete, 
                                            ID = toDelete.ID, 
                                            Action = BrainAction.Removed
                                        };
                    UndoStore.AddCustomUndoItem(iUndoData);

                        // first item in the list must be the delete, so that it gets created first as well.
                    var iTime = toDelete as NeuronCluster;
                    if (iTime != null && iTime.Meaning == (ulong)PredefinedNeurons.Time)
                    {
                        Time.Current.DeleteTimeCluster(iTime);
                    }
                    else
                    {
                        if (Brain.Current.Delete(toDelete) == false)
                        {
                            var iMsg = string.Format("Neuron can't be deleted: {0}.", toDelete);
                            System.Windows.MessageBox.Show(
                                iMsg, 
                                "Delete neuron", 
                                System.Windows.MessageBoxButton.OK, 
                                System.Windows.MessageBoxImage.Error);
                            LogService.Log.LogError("WindowMain.DeleteItemFromBrain", iMsg);
                        }
                    }
                }
                finally
                {
                    UndoStore.EndUndoGroup();
                }
            }
            else
            {
                var iMsg = string.Format("Neuron can't be deleted: {0}.", toDelete);
                System.Windows.MessageBox.Show(
                    iMsg, 
                    "Delete neuron", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Activates/focuses the doc so that the user can edit the doc.</summary>
        /// <param name="obj">The obj.</param>
        internal void ActivateDoc(object obj)
        {
            BrainData.Current.ActiveDocument = obj;
        }

        /// <summary>Activates/focuses the doc at the specified <paramref name="index"/> so
        ///     that the user can edit the doc.</summary>
        /// <param name="index">The index.</param>
        internal void ActivateDocAtIndex(int index)
        {
            BrainData.Current.ActiveDocument = BrainData.Current.OpenDocuments[index];
        }

        /// <summary>Activates the toolwindow.</summary>
        /// <param name="content">The content.</param>
        internal void ActivateTool(ToolViewItem content)
        {
            try
            {
                content.IsVisible = true;
                content.IsActive = true;
            }
            catch
            {
            }
        }

        #endregion

        #region commands

        #region Undo/redo

        /// <summary>Handles the OnExecuted event of the Undo command binding.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void Undo_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (UndoStore.UndoData.Count > 0)
            {
                // as a sanity check, we make certain that there is data to be undone.
                UndoStore.Undo(); // a simple call to undo is all that's needed.
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Undo command binding.</summary>
        /// <remarks>This is provided so you can easily change or call this function from
        ///     your application.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void Undo_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoStore.UndoData.Count > 0;

                // All undo data is tored in the UndoData list, if this is empty, there is nothing to undo.
            e.Handled = true;
        }

        /// <summary>Handles the OnExecuted event of the Redo command binding.</summary>
        /// <remarks>This is provided so you can easily change or call this function from
        ///     your application.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void Redo_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (UndoStore.RedoData.Count > 0)
            {
                // All redo data is stored in the RedoData list so if this is empty, we can't perform a redo.
                UndoStore.Redo(); // a simple call to redo is all that's needed.
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Redo command binding.</summary>
        /// <remarks>This is provided so you can easily change or call this function from
        ///     your application.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void Redo_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoStore.RedoData.Count > 0;

                // All undo data is tored in the UndoData list, if this is empty, there is nothing to undo.
            e.Handled = true;
        }

        #endregion

        #region Change to

        /// <summary>Handles the CanExecute event of the ChangeTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ChangeTo_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is System.Type)
            {
                INeuronWrapper iNeuron = null;
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    iNeuron = iFocused.DataContext as INeuronWrapper;
                }

                if (iNeuron == null)
                {
                    var iSource = e.OriginalSource as System.Windows.FrameworkElement;
                    if (iSource != null)
                    {
                        iNeuron = iSource.DataContext as INeuronWrapper;
                    }
                }

                if (iNeuron != null)
                {
                    e.CanExecute = true;
                    return;
                }
            }

            e.CanExecute = false;
        }

        /// <summary>Handles the Executed event of the ChangeTo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ChangeTo_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iType = (System.Type)e.Parameter;
            Neuron iNeuron = null;
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iFocused != null && iFocused.DataContext is INeuronWrapper)
            {
                iNeuron = ((INeuronWrapper)iFocused.DataContext).Item;
            }
            else
            {
                var iSource = e.OriginalSource as System.Windows.FrameworkElement;
                if (iSource != null && iSource.DataContext is INeuronWrapper)
                {
                    iNeuron = ((INeuronWrapper)iSource.DataContext).Item;
                }
            }

            if (iNeuron != null)
            {
                EditorsHelper.TryChangeTypeTo(iNeuron, iType);
            }
            else
            {
                System.Windows.MessageBox.Show("Can't find selected neuron!");
            }
        }

        #endregion

        #region ShareLink

        /// <summary>The share link_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ShareLink_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            Neuron iToEdit = null;
            var iSource = e.OriginalSource as System.Windows.FrameworkElement;
            if (iSource != null && e.Parameter is NeuronData.NeuronDataOverlay)
            {
                if (iSource.DataContext is INeuronWrapper)
                {
                    iToEdit = ((INeuronWrapper)iSource.DataContext).Item;
                }

                if (iToEdit == null)
                {
                    var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                    {
                        iToEdit = ((INeuronWrapper)iFocused.DataContext).Item;
                    }
                }
            }

            e.CanExecute = iToEdit != null;
        }

        /// <summary>The share link_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ShareLink_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iOverlayData = e.Parameter as NeuronData.NeuronDataOverlay;
                var iSource = e.OriginalSource as System.Windows.FrameworkElement;
                if (iOverlayData != null && iSource != null && iSource.DataContext is INeuronWrapper)
                {
                    var iToEdit = ((INeuronWrapper)iSource.DataContext).Item;
                    if (iToEdit == null)
                    {
                        var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                        if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                        {
                            iToEdit = ((INeuronWrapper)iFocused.DataContext).Item;
                        }
                    }

                    if (iToEdit != null)
                    {
                        var iTo = iOverlayData.Data.Neuron.FindFirstOut(iOverlayData.Overlay.ItemID);
                        if (iTo != null)
                        {
                            if (Link.Exists(iToEdit, iTo, iOverlayData.Overlay.ItemID) == false)
                            {
                                var iNew = new Link(iTo, iToEdit, iOverlayData.Overlay.ItemID);
                                var iUndoData = new LinkUndoItem(iNew, BrainAction.Created);
                                UndoStore.AddCustomUndoItem(iUndoData);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Link already exists!");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(
                                "Can't find 'To' part on the original link!", 
                                "Share link error", 
                                System.Windows.MessageBoxButton.OK, 
                                System.Windows.MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, 
                    "Share link", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region DeleteNeuron

        /// <summary>Handles the CanExecute event of the DeleteNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteNeuron_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iToDelete = e.Parameter as Neuron;
            if (iToDelete == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iToDelete = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            e.CanExecute = iToDelete != null && iToDelete.CanBeDeleted;
        }

        /// <summary>Handles the Executed event of the DeleteNeuron command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iToDelete = e.Parameter as Neuron;
                if (iToDelete == null)
                {
                    var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                    {
                        iToDelete = ((INeuronWrapper)iFocused.DataContext).Item;
                    }
                }

                if (iToDelete != null)
                {
                    var iRes = System.Windows.MessageBox.Show(
                        string.Format("Delete neuron: '{0}'?", iToDelete), 
                        "Delete neuron", 
                        System.Windows.MessageBoxButton.OKCancel, 
                        System.Windows.MessageBoxImage.Question, 
                        System.Windows.MessageBoxResult.No);
                    if (iRes == System.Windows.MessageBoxResult.OK)
                    {
                        UndoStore.BeginUndoGroup(false, true);

                            // a delete needs to be reversed, since it will first delete all the links (deletions which will also be recorded)
                        try
                        {
                            DeleteItemFromBrain(iToDelete);
                        }
                        finally
                        {
                            UndoStore.EndUndoGroup();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, 
                    "Delete neuron", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region RunNeuron

        /// <summary>The run neuron_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RunNeuron_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iToRun = e.Parameter as Neuron;
            if (iToRun == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iToRun = ((INeuronWrapper)iFocused.DataContext).Item;
                }
            }

            e.CanExecute = iToRun != null;
        }

        /// <summary>The run neuron_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RunNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iToRun = e.Parameter as Neuron;
                if (iToRun == null)
                {
                    var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                    {
                        iToRun = ((INeuronWrapper)iFocused.DataContext).Item;
                    }
                }

                if (iToRun != null)
                {
                    if (iToRun is Queries.Query)
                    {
                        TryStartQuery((Queries.Query)iToRun);
                    }
                    else
                    {
                        RunNeuron(iToRun);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, 
                    "Run neuron", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>The run neuron.</summary>
        /// <param name="iToRun">The i to run.</param>
        private void RunNeuron(Neuron iToRun)
        {
            var iClusterToRun = iToRun as NeuronCluster;
            Processor iProc;
            if (iClusterToRun != null)
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        string.Format("Run the cluster as function (no will solve it)?"), 
                        "Delete neuron", 
                        System.Windows.MessageBoxButton.YesNoCancel, 
                        System.Windows.MessageBoxImage.Question, 
                        System.Windows.MessageBoxResult.Cancel);
                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    iProc = ProcessorFactory.GetProcessor();
                    iProc.CallSingle(iClusterToRun);

                        // this is async, otherwise the ui can get stuck because of the debugger.
                }
                else if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            else
            {
                iProc = ProcessorFactory.GetProcessor(); // by default we solve it.
                iProc.Push(iToRun);
                iProc.Solve();
            }
        }

        /// <summary>tries to start a query, if possible.</summary>
        /// <param name="query"></param>
        public static void TryStartQuery(Queries.Query query)
        {
            if (query.ActionsForInput != null)
            {
                query.Run();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Query hasn't been compiled yet", 
                    "Run query", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Sandbox

        /// <summary>Handles the CanExecute event of the Sandbox command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Sandbox_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ProjectManager.Default.SandboxRunning == false && ProjectManager.Default.IsSandBox == false
                           && string.IsNullOrEmpty(ProjectManager.Default.Location) == false
                           && string.IsNullOrEmpty(ProjectManager.Default.SandboxLocation) == false;
        }

        /// <summary>Handles the Executed event of the Sandbox command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Sandbox_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ProjectManager.Default.StartSandBox();
        }

        #endregion

        #region Continue debug

        /// <summary>The continue debug_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ContinueDebug_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                if (iProc != null)
                {
                    e.CanExecute = iProc.IsPaused;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

        /// <summary>The continue debug_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ContinueDebug_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                iProc.DebugContinue();
            }
        }

        #endregion

        #region Pause debug

        /// <summary>The pause debug_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PauseDebug_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                if (iProc != null)
                {
                    e.CanExecute = !iProc.IsPaused || iProc.DebugMode == DebugMode.SlowMotion;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

        /// <summary>The pause debug_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PauseDebug_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                iProc.DebugPause();
            }
        }

        #endregion

        #region Stop processors

        /// <summary>Handles the CanExecute event of the StopProcessors control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void StopProcessors_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ProcessorManager.Current.HasProcessors;
        }

        /// <summary>Handles the Executed event of the StopProcessors control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void StopProcessors_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ProcessorManager.Current.StopAndUnDeadLock();
        }

        /// <summary>Handles the Executed event of the KillProcessor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void KillProcessor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = e.Parameter as ProcItem;
            if (iProc == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    iProc = iFocused.DataContext as ProcItem;
                }
            }

            if (iProc != null && iProc.Processor != null)
            {
                iProc.Processor.Kill();
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Handles the Executed event of the KillAllButProcessorCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void KillAllButProcessorCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = e.Parameter as ProcItem;
            if (iProc == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    iProc = iFocused.DataContext as ProcItem;
                }
            }

            if (iProc != null && iProc.Processor != null)
            {
                iProc.Processor.PreventKill = true;
                try
                {
                    ProcessorManager.Current.StopProcessors();
                }
                finally
                {
                    iProc.Processor.PreventKill = false;

                        // always need to reset, otherwise we screw up for the next time.
                }
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        #endregion

        #region Step next debug

        /// <summary>The step next debug_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StepNextDebug_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                if (iProc != null)
                {
                    e.CanExecute = iProc.IsPaused;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

        /// <summary>The step next debug_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StepNextDebug_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = ProcessorManager.Current.SelectedProcessor;
            if (iItem != null)
            {
                var iProc = iItem.Processor;
                iProc.DebugStepNext();
            }
        }

        #endregion

        #region Attach To Cur Processor

        /// <summary>The attach to cur processor_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AttachToCurProcessor_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (ProcessorManager.Current.SelectedProcessor != null)
            {
                if (e.Parameter != null)
                {
                    e.CanExecute = e.Parameter is Neuron;
                }
                else
                {
                    var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (iFocused != null)
                    {
                        var iWrapper = iFocused.DataContext as INeuronWrapper;
                        e.CanExecute = iWrapper != null && iWrapper.Item != null;
                        return;
                    }
                }
            }

            e.CanExecute = false;
        }

        /// <summary>The attach to cur processor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AttachToCurProcessor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (ProcessorManager.Current.SelectedProcessor != null)
            {
                var iProc = ProcessorManager.Current.SelectedProcessor.Processor;
                Neuron iToAttach = null;
                if (e.Parameter != null)
                {
                    iToAttach = e.Parameter as Neuron;
                }
                else
                {
                    var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                    if (iFocused != null)
                    {
                        var iWrapper = iFocused.DataContext as INeuronWrapper;
                        iToAttach = iWrapper.Item;
                    }
                }

                if (iToAttach != null)
                {
                    ProcessorManager.Current.AtttachedDict.Attach(iToAttach, iProc);
                }
            }
        }

        #endregion

        #region Inspect expression

        /// <summary>Handles the CanExecute event of the InspectExpression control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InspectExpression_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                e.CanExecute = e.Parameter is ResultExpression;
            }
            else
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    var iWrapper = iFocused.DataContext as INeuronWrapper;
                    e.CanExecute = iWrapper != null && iWrapper.Item is ResultExpression;
                    return;
                }

                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the InspectExpression control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InspectExpression_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ResultExpression iExp = null;
            if (e.Parameter != null)
            {
                iExp = (ResultExpression)e.Parameter;
            }
            else
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                var iWrapper = iFocused.DataContext as INeuronWrapper;
                iExp = iWrapper.Item as ResultExpression;
            }

            if (iExp != null && ProcessorManager.Current.SelectedProcessor != null)
            {
                System.Collections.Generic.List<DebugNeuron> iItems;
                if (iExp is Variable)
                {
                    // when it is a variable, we need to make certain that we don't initialize it just for inspecting it's value, since this can change the result of the processor.
                    iItems =
                        (from i in
                             ((Variable)iExp).GetValueWithoutInit(ProcessorManager.Current.SelectedProcessor.Processor)
                         where i != null
                         select new DebugNeuron(i)).ToList();
                }
                else
                {
                    var iSource = Neuron.SolveResultExp(iExp, ProcessorManager.Current.SelectedProcessor.Processor);
                    if (iSource != null)
                    {
                        iItems = (from i in iSource where i != null select new DebugNeuron(i)).ToList();
                    }
                    else
                    {
                        iItems = new System.Collections.Generic.List<DebugNeuron>(); // no values, so empty list.
                    }
                }

                var iRes = new DlgInspectExpression(iExp, iItems);
                iRes.Owner = System.Windows.Application.Current.MainWindow;
                iRes.Show();
            }
        }

        #endregion

        #region Close editor

        /// <summary>The close_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Close_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BrainData.Current.ActiveDocument != null;
        }

        /// <summary>The close_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Close_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            BrainData.Current.OpenDocuments.Remove(BrainData.Current.ActiveDocument);
        }

        #endregion

        // #region BrowseNeuron
        // private void BrowseNeuronCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        // {
        // if (e.Parameter != null)
        // e.CanExecute = e.Parameter is Neuron;
        // else
        // {
        // FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
        // if (iFocused != null)
        // {
        // INeuronWrapper iWrapper = iFocused.DataContext as INeuronWrapper;
        // e.CanExecute = iWrapper != null && iWrapper.Item is Neuron;
        // }
        // else
        // e.CanExecute = false;
        // }
        // }

        // private void BrowseNeuronCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        // {
        // Neuron iNeuron = null;
        // if (e.Parameter != null)
        // iNeuron = (Neuron)e.Parameter;
        // else
        // {
        // FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
        // INeuronWrapper iWrapper = iFocused.DataContext as INeuronWrapper;
        // iNeuron = iWrapper.Item;
        // }
        // if (iNeuron != null)
        // {
        // List<DebugNeuron> iItems = new List<DebugNeuron>();
        // iItems.Add(new DebugNeuron(iNeuron));
        // DlgInspectExpression iRes = new DlgInspectExpression(null, iItems);
        // iRes.Title = "Browse neuron";
        // iRes.Owner = App.Current.MainWindow;
        // iRes.Show();
        // }
        // } 
        // #endregion
        #region Clear BreakPoints

        /// <summary>The clear break points_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ClearBreakPoints_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BrainData.Current != null && BrainData.Current.BreakPoints != null
                           && BrainData.Current.BreakPoints.Count > 0;
        }

        /// <summary>The clear break points_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ClearBreakPoints_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            BrainData.Current.BreakPoints.Clear();
        }

        #endregion

        #region Store splitPath

        /// <summary>The store split path_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StoreSplitPath_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iProc = e.Parameter as ProcItem;
            if (iProc == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    iProc = iFocused.DataContext as ProcItem;
                }
            }

            e.CanExecute = iProc != null;
        }

        /// <summary>The store split path_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void StoreSplitPath_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProc = e.Parameter as ProcItem;
            if (iProc == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    iProc = iFocused.DataContext as ProcItem;
                }
            }

            if (iProc != null)
            {
                StoreSplitPath(iProc.Processor.SplitPath, iProc.Name);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Stores the split path into the processorManager so that it can be
        ///     traversed. This is wrapped in an undo transaction.</summary>
        /// <param name="list">The list.</param>
        /// <param name="name">The name.</param>
        public void StoreSplitPath(System.Collections.Generic.List<ulong> list, string name)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                var iNew = new SplitPath();
                foreach (var i in list)
                {
                    var iItem = new SplitPathItem();
                    iItem.ItemID = i;
                    iNew.Items.Add(iItem);
                }

                iNew.Name = "Path for " + name;
                var iRan = new System.Random(System.DateTime.Now.Minute);

                    // we use the current minute as a random seed for generating colors.
                var iColor = new System.Windows.Media.Color();
                iColor.R = (byte)iRan.Next(byte.MaxValue);
                iColor.G = (byte)iRan.Next(byte.MaxValue);
                iColor.B = (byte)iRan.Next(byte.MaxValue);
                iNew.Color = iColor;
                ProcessorManager.Current.SplitPaths.Add(iNew);
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region ImportFrom

        /// <summary>Handles the Executed event of the ImportFrameNetData control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportFrameNetData_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new DlgImportFrameNet();
            if (iDlg.LoadFrameNet())
            {
                iDlg.Owner = this;
                if (e.Parameter != null)
                {
                    iDlg.ImportInto = (FrameEditor)e.Parameter;
                    iDlg.ShowImportInto = false;
                }

                iDlg.Show();
            }
        }

        /// <summary>Handles the Executed event of the ImportFrameNetData control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportVerbNetData_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new DlgImportVerbNet();
            System.Action<DlgImportVerbNet> iLoadData = LoadVerbNet;

                // we load the data async, so that the user doesn't see to much of a delay from loading all the files.
            iLoadData.BeginInvoke(iDlg, null, null);

            iDlg.Owner = this;
            if (e.Parameter != null)
            {
                iDlg.ImportInto = (FrameEditor)e.Parameter;
                iDlg.ShowImportInto = false;
            }

            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            iDlg.Show();
        }

        /// <summary>The load verb net.</summary>
        /// <param name="dialog">The dialog.</param>
        private void LoadVerbNet(DlgImportVerbNet dialog)
        {
            try
            {
                var iVal = VerbNetProvider.VerbNet.Load(Properties.Settings.Default.VerbNetPath);
                if (iVal != null)
                {
                    System.Action<DlgImportVerbNet, System.Collections.Generic.IList<VerbNetProvider.VNCLASS>> SetDataContext;
                    SetDataContext = (dlg, list) =>
                        {
                            System.Windows.Input.Mouse.OverrideCursor = null;
                            if (list == null || list.Count == 0)
                            {
                                System.Windows.MessageBox.Show(
                                    string.Format(
                                        "No verbnet class found at '{0}', please check the Directory settings in the Options dialog!", 
                                        Properties.Settings.Default.VerbNetPath), 
                                    "Import from VerbNet", 
                                    System.Windows.MessageBoxButton.OK, 
                                    System.Windows.MessageBoxImage.Exclamation);
                            }

                            if (dlg != null && dlg.IsActive)
                            {
                                // in case that the window was closed during async loading.
                                dlg.DataContext = list;
                            }
                        };
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        SetDataContext, 
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        dialog, 
                        iVal); // we must set the data context async, on the UI thread.
                }
                else
                {
                    System.Windows.Input.Mouse.OverrideCursor = null;
                    System.Windows.MessageBox.Show(
                        "Failed to load VertNet data!", 
                        "Import from VerbNet", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
            catch (System.Exception e)
            {
                System.Action<string> iError; // we need to set the OverrideCursor from the main gui thread.
                iError = str =>
                    {
                        System.Windows.Input.Mouse.OverrideCursor = null; // just to be certain
                        System.Windows.MessageBox.Show(e.ToString());
                    };
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    iError, 
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    e.ToString());
            }
        }

        #endregion

        #region Import/export modules

        /// <summary>The import module cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ImportModuleCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iLoader = new ModuleImporter();
            iLoader.Import();
        }

        /// <summary>Handles the CanExecute event of the ExportBrainFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportModuleCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is CodeEditor) || (e.Parameter is EditorFolder)
                           || (BrainData.Current.ActiveDocument != null
                               && BrainData.Current.ActiveDocument is CodeEditor);
        }

        /// <summary>The export module cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ExportModuleCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new ModuleExporter();

            EditorBase iEditor;
            if (e.Parameter is EditorBase)
            {
                iEditor = (EditorBase)e.Parameter;
            }
            else if (e.Parameter is string && string.Compare((string)e.Parameter, "all") == 0)
            {
                iEditor = null;
            }
            else
            {
                iEditor = (EditorBase)BrainData.Current.ActiveDocument;
            }

            iExporter.Export(iEditor);
        }

        /// <summary>The mnu mange modules_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuMangeModules_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iWin = new ModuleManager();
            iWin.Owner = this;
            iWin.ShowDialog();
        }

        #endregion

        #region Import/export TextPatterns

        /// <summary>Handles the Executed event of the ImportBrainFileCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportTopicCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iImporter = new TopicImporter();
            iImporter.Import();
        }

        /// <summary>Handles the CanExecute event of the ExportBrainFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportTopic_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is TextPatternEditor)
                           || (BrainData.Current.ActiveDocument != null
                               && BrainData.Current.ActiveDocument is TextPatternEditor);
        }

        /// <summary>Handles the Executed event of the ExportBrainFileCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportTopicCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new TopicExporter();
            var iEditor = e.Parameter as TextPatternEditor;
            if (iEditor == null)
            {
                iEditor = BrainData.Current.ActiveDocument as TextPatternEditor;
            }

            System.Diagnostics.Debug.Assert(iEditor != null);
            iExporter.Export(iEditor);
        }

        /// <summary>Handles the Executed event of the ExportAllBrainFilesCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportAllTopicsCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new TopicsExporter();
            iExporter.Export();
        }

        /// <summary>Handles the Executed event of the ImportGlobalPatternsCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportGlobalPatternsCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iImporter = new globalsImporter();
            iImporter.Import();
        }

        /// <summary>Handles the Executed event of the ExportglobalPatternsCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportglobalPatternsCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new GlobalsExporter();
            iExporter.Export();
        }

        /// <summary>Handles the Executed event of the ImportAssetCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportAssetCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iImporter = new AssetImporter();
            iImporter.Import();
        }

        /// <summary>Handles the Executed event of the ImportGenericDataCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportGenericDataCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            GenericsImporter.Import();
        }

        /// <summary>Handles the Executed event of the ImportCustomCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportCustomCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CustomConduit.StartImport();
        }

        /// <summary>Handles the CanExecute event of the ExportAssetCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportAssetCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is ObjectEditor)
                           || (BrainData.Current.ActiveDocument != null
                               && BrainData.Current.ActiveDocument is ObjectEditor);
        }

        /// <summary>Handles the Executed event of the ExportAssetCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportAssetCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new AssetExporter();
            var iEditor = e.Parameter as ObjectEditor;
            if (iEditor == null)
            {
                iEditor = BrainData.Current.ActiveDocument as ObjectEditor;
            }

            System.Diagnostics.Debug.Assert(iEditor != null);
            iExporter.Export(iEditor);
        }

        /// <summary>Handles the Executed event of the ExportQueryCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportQueryCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new QueryExporter();
            var iEditor = e.Parameter as QueryEditor;
            if (iEditor == null)
            {
                iEditor = BrainData.Current.ActiveDocument as QueryEditor;
            }

            System.Diagnostics.Debug.Assert(iEditor != null);
            iExporter.Export(iEditor);
        }

        /// <summary>Handles the CanExecute event of the ExportQueryCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportQueryCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is QueryEditor)
                           || (BrainData.Current.ActiveDocument != null
                               && BrainData.Current.ActiveDocument is QueryEditor);
        }

        /// <summary>Handles the Executed event of the ImportQueryCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportQueryCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iImport = new QueryImporter();
            iImport.Import();
        }

        /// <summary>Handles the Executed event of the ImportChatLogHistCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportChatLogHistCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iImporter = new ChatLogImporter();
            iImporter.Import();
        }

        /// <summary>Handles the Executed event of the ExportChatLogHistCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportChatLogHistCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExporter = new ChatLogExporter();
            iExporter.Export();
        }

        #endregion

        #region online

        /// <summary>checks if there is already an online version installed.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadOnline_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BrainData.Current != null && BrainData.Current.DesignerData != null
                           && BrainData.Current.DesignerData.OnlineInfo != null
                           && BrainData.Current.DesignerData.OnlineInfo.IsInstalled;
            e.Handled = true;
        }

        /// <summary>allows for installation and configuration of the online version from
        ///     the chatbot.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InstallOnlineCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iConfig = new DlgOnlineConfig();
            iConfig.Owner = this;
            iConfig.ShowDialog();
        }

        /// <summary>downloads the project so it can be opened locally.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadOnline_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iLoc = PathUtil.VerifyPathEnd(BrainData.Current.DesignerData.OnlineInfo.FTPLocation, '/') + "Data/";
            if (iLoc.StartsWith("ftp://") == false)
            {
                iLoc = "ftp://" + iLoc;
            }

            var iOnline = new System.Uri(iLoc, System.UriKind.Absolute);
            var iDownload = new FtpDownloader();
            iDownload.DownloadTo(iOnline, BrainData.Current.DesignerData.OnlineInfo.FTPLocation);
        }

        /// <summary>uploads the entire db to the online site, replacing the prevous
        ///     version.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadOnline_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iUpload = new OnlineInstaller();
            iUpload.ReplaceDb();
        }

        /// <summary>The sync online_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SyncOnline_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSync = new OnlineDataSynchronizer();
            iSync.Sync();
        }

        #endregion

        #region help

        /// <summary>Handles the CanExecute event of the <see cref="ControlFramework.Controls.Help"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Help_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = System.Windows.Input.Keyboard.FocusedElement is System.Windows.UIElement;
        }

        /// <summary>Handles the Executed event of the <see cref="ControlFramework.Controls.Help"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Help_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            fHelp.ShowHelpFor(System.Windows.Input.Keyboard.FocusedElement as System.Windows.UIElement);
        }

        /// <summary>Handles the Executed event of the OpenInternetPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void OpenInternetPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = new System.Diagnostics.Process();
            iExplorer.StartInfo.FileName = (string)e.Parameter;
            iExplorer.Start();
        }

        #endregion

        #endregion

        #region Event handlers

        #region Menu

        /// <summary>Handles the Click event of the MnuExit menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>Handles the Click event of the MnuRecentProject menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuRecentProject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e.OriginalSource != MnuRecentProjects)
            {
                // need to make certain that we don't do something when the user clicks on 'Recently opened'.
                var iSender = (System.Windows.Controls.MenuItem)e.OriginalSource;
                var iPath = iSender.Header as string;
                if (System.IO.File.Exists(iPath))
                {
                    var iLoader = new ProjectLoader();
                    iLoader.Open(iPath);
                }
                else
                {
                    System.Windows.MessageBox.Show(string.Format("Couldn't find the file: {0}!", iPath));
                }
            }
        }

        /// <summary>Handles the Click event of the MnuOptions control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuOptions_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new WindowOptions();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Executed event of the RebuildProjectCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RebuildProjectCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (ProjectManager.Default.ProjectChanged)
            {
                var iRes = System.Windows.MessageBox.Show(
                    "The project has changed, would you like to save first?", 
                    "Rebuild project", 
                    System.Windows.MessageBoxButton.YesNoCancel, 
                    System.Windows.MessageBoxImage.Question, 
                    System.Windows.MessageBoxResult.Yes);
                if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }

                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    var iSave = new ProjectSaver();
                    iSave.EndedOk += iSave_EndedOk;
                    iSave.Save();
                    return; // we exit so we don't try to convert it while saving.
                }
            }

            var iConverter = new ProjectConverter();
            iConverter.Convert();
        }

        /// <summary>Handles the Click event of the MnuItemRebuildPatterns control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void RebuildAllTopicsCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new DlgRebuildPatterns();
            iDlg.Owner = Current;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the EndedOk event of the iSave control. called when the
        ///     project has saved and we can convert it.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void iSave_EndedOk(object sender, System.EventArgs e)
        {
            var iConverter = new ProjectConverter();
            iConverter.Convert();
        }

        /// <summary>The mnu_ warnings exceptions_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Mnu_WarningsExceptions_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgWarningsAndExceptions();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuRemoveBrokenRef menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuRemoveBrokenRef_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgFixBrokenRefs();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>The mnu clean orphans_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuCleanOrphans_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgCleanOrphans();
            iDlg.Owner = this;
            iDlg.Title = "Clean orphans";
            iDlg.CleanType = CleanType.Orphans;
            iDlg.ShowDialog();
        }

        /// <summary>The mnu clean orphan flow items_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuCleanOrphanFlowItems_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgCleanOrphans();
            iDlg.Owner = this;
            iDlg.Title = "Clean orphans flow items";
            iDlg.CleanType = CleanType.FlowItems;
            iDlg.ShowDialog();
        }

        /// <summary>The mnu del empty clusters_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuDelEmptyClusters_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgCleanOrphans();
            iDlg.Owner = this;
            iDlg.Title = "Delete empty clusters";
            iDlg.CleanType = CleanType.EmptyClusters;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuEditPOS menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuEditPOS_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgPOSEditor();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuChangeRef control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuChangeRef_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgMergeNeurons();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuOverlays control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuOverlays_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgOverlays();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        #region help

        /// <summary>Handles the Click event of the MnuHelpAbout control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuHelpAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgAbout();
            iDlg.Owner = this;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuSearchHelp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuSearchHelp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fHelp.ShowHelpSearch();
        }

        /// <summary>Handles the Click event of the MnuContentsHelp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuContentsHelp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fHelp.ShowHelpContents();
        }

        /// <summary>Handles the Click event of the MnuIndexHelp control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuIndexHelp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fHelp.ShowHelpIndex(string.Empty);
        }

        #endregion

        /// <summary>Handles the Click event of the MnuEditTestCase control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuEditTestCase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.FrameworkElement;
            var iTestCase = iSender.DataContext as Test.TestCase;

            if (iTestCase != null)
            {
                BrainData.Current.OpenDocuments.Add(iTestCase);
                ActivateDoc(iTestCase);
            }
        }

        /// <summary>Handles the Click event of the MnuRunTestCase control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuRunTestCase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.FrameworkElement;
            var iTestCase = iSender.DataContext as Test.TestCase;

            if (iTestCase != null)
            {
                BrainData.Current.OpenDocuments.Add(iTestCase);
                ActivateDoc(iTestCase);
                iTestCase.Run();
            }
        }

        /// <summary>Handles the Click event of the MnuManageTestCase control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuManageTestCase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new DlgManageTestCases();
            iDlg.Owner = Current;
            iDlg.ShowDialog();
        }

        /// <summary>Handles the Click event of the MnuViewWordNet control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuViewWordNet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            try
            {
                var iWordNet =
                    (from i in BrainData.Current.OpenDocuments where i is WordNetChannel select (WordNetChannel)i)
                        .FirstOrDefault();
                if (iWordNet == null)
                {
                    if (BrainData.Current.DesignerData.WordnetChannel == null)
                    {
                        iWordNet = new WordNetChannel();
                        iWordNet.NeuronID = (ulong)PredefinedNeurons.WordNetSin;
                        iWordNet.NeuronInfo.DisplayTitle = "WordNet";
                        BrainData.Current.DesignerData.WordnetChannel = iWordNet;
                    }
                    else
                    {
                        iWordNet = BrainData.Current.DesignerData.WordnetChannel;
                    }

                    AddItemToOpenDocuments(iWordNet);
                }
                else
                {
                    ActivateDoc(iWordNet);
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<System.Windows.Input.Cursor>(ResetCursor), 
                    null); // we do this async cause of the loading, which can be done async.
            }
        }

        #endregion

        #region General

        /// <summary>Checks if the application can be closed, and ask the appropriate
        ///     questions in cases it doesn't know.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ProjectOperation.IsInOperation)
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        "The application is performing a task, are you certain you want to exit?", 
                        "Quit application!", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Question);
                if (iRes == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            else if (ProjectManager.Default.ProjectChanged && ProjectManager.Default.IsSandBox == false)
            {
                // sandboxes don't need saving, they are always temp.
                System.Windows.MessageBoxResult iRes;
                if (ProjectManager.Default.AutoSave == false)
                {
                    // autosave projects always need ot be saved, for data consistency.
                    iRes = System.Windows.MessageBox.Show(
                        "The project has been changed, save first?", 
                        "Quit application!", 
                        System.Windows.MessageBoxButton.YesNoCancel, 
                        System.Windows.MessageBoxImage.Question);
                }
                else
                {
                    iRes = System.Windows.MessageBoxResult.Yes;
                }

                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    var iSaver = new ProjectSaver();
                    iSaver.EndedOk += iSaver_EndedOk;
                    iSaver.Save(); // a save will also take care of any running processors.
                    e.Cancel = true;

                        // we cancel cause the save is async, and we want to keep the ui responsive.  if the save succeeds, it will retry to close the window through the event handler.
                    return;
                }

                e.Cancel = iRes == System.Windows.MessageBoxResult.Cancel;
            }

            if (e.Cancel == false)
            {
                if (Brain.Current != null)
                {
                    // when stopping, we stil need to make certain that the 'OnShutDown' code is called. This done when we clear the network 
                    Brain.Current.Clear();

                        // even though this can raise network events, we can savely call it, cause the flag to raise the network events or not, has already been properly set (Settings.RaiseNetworkEvents).
                }

                if (BrainData.Current != null)
                {
                    BrainData.Current.Clear();

                        // also need to make certain that chatbot channel unloads all COM objects. (was originally only done in sandbox mode).
                }

                SaveLayout(GetLayoutFileName());
                CharacterEngine.TickGenerator.Default.Stop();

                    // when the app stops, we need to make certain that the multi media timer isn't running anymore cause that can be a problem we can easely avoid.
            }
        }

        /// <summary>saves the layout to file.</summary>
        /// <param name="path"></param>
        private void SaveLayout(string path)
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(MainDock);
            using (var stream = new System.IO.StreamWriter(path)) serializer.Serialize(stream);
        }

        /// <summary>Handles the EndedOk event of the iSaver control.</summary>
        /// <remarks>We canceled the close because a save needed to be performed, which is
        ///     done async to keep the ui responsive. offcourse we can't keep the
        ///     window_closing function blocked untill the save is done because that
        ///     would defeat the entire async functionality, so it cancels the
        ///     operation. When the save has succeeded ok (so the user didn't cancel
        ///     or it failed somehow), we can close the window. Since the project wont
        ///     be changed, it wont try to save 2 times.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void iSaver_EndedOk(object sender, System.EventArgs e)
        {
            if (ProjectManager.Default.IsSandBox && Brain.Current != null)
            {
                // if this is a sandbox, and we are stopping, we stil need to make certain that the 'OnShutDown' code is called. This done when we clear the network.
                Brain.Current.Clear();
            }

            SaveLayout(GetLayoutFileName());
            Close();
        }

        #endregion

        #region File

        #region Save

        /// <summary>Handles the Executed event of the Save command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Save_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPrev = System.Windows.Input.Keyboard.FocusedElement;
            Focus();

                // we do this to make certain that all the bindings have commited their data before saving. This only works if the UpdateSourceTrigger is explicit and the update is done in the previewLostKeyboardFocus + unload
            if (iPrev != null)
            {
                iPrev.Focus(); // most of the time, this isn't required, cause it remains there, the event gets canceled
            }

            var iSaver = new ProjectSaver();
            iSaver.Save();
        }

        /// <summary>The save as_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SaveAs_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPrev = System.Windows.Input.Keyboard.FocusedElement;
            Focus();

                // we do this to make certain that all the bindings have commited their data before saving. This only works if the UpdateSourceTrigger is explicit and the update is done in the previewLostKeyboardFocus + unload
            if (iPrev != null)
            {
                iPrev.Focus(); // most of the time, this isn't required, cause it remains there, the event gets canceled
            }

            var iSaver = new ProjectSaver();
            iSaver.SaveAs();
        }

        #endregion

        #region New

        /// <summary>Handles the Executed event of the New command.</summary>
        /// <remarks>Simply starts a new process that is the same as this one.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void New_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ProjectManager.Default.CreateNew();
            DescriptionView.SetDocument(null);

                // when a new item is created, need to make certain that the desc editor is also empty.
        }

        #endregion

        #region Open

        /// <summary>Handles the Executed event of the Open command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Open_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSaver = new ProjectLoader();
            iSaver.Open();
        }

        /// <summary>Handles the CanExecute event of the Open control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Open_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ProjectManager.Default.SandboxRunning == false;
        }

        #endregion

        #endregion

        #region Insert

        /// <summary>Handles the Click event of the MnuNewCommChannel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewCommChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iNew = new TextSin();
            iNew.Text = "New text channel";
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                AddItemToBrain(iNew);
                var iView = new TextChannel();
                iView.SetSin(iNew);
                iView.IsVisible = true;

                // iView.Name = iNew.Name;
                BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                ActivateDoc(iView);
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Click event of the MnuNewImageChannel menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewImageChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iNew = new ImageSin();
            iNew.Text = "New Image channel";
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                AddItemToBrain(iNew);
                var iView = new ImageChannel();
                iView.NeuronID = iNew.ID;
                iView.IsVisible = true;

                // iView.Name = iNew.Name;
                BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                ActivateDoc(iView);
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Click event of the MnuNewImageChannel menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewGridChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iNew = new GridSin();
            iNew.Text = "New Grid channel";
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                AddItemToBrain(iNew);
                var iView = new GridChannel();
                iView.NeuronID = iNew.ID;
                iView.IsVisible = true;
                BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                ActivateDoc(iView);
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Click event of the MnuNewAudioChannel menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewAudioChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iNew = new AudioSin();
            iNew.Text = "New text channel";
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                AddItemToBrain(iNew);
                var iView = new AudioChannel();
                iView.NeuronID = iNew.ID;
                iView.IsVisible = true;

                // iView.Name = iNew.Name;
                BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                ActivateDoc(iView);
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Click event of the MnuNewReflectionChannel menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewReflectionChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iNew = new ReflectionSin();
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                AddItemToBrain(iNew);
                iNew.Text = "Reflection channel - " + iNew.ID;
                var iView = new ReflectionChannel();
                iView.NeuronID = iNew.ID;
                iView.IsVisible = true;
                BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                ActivateDoc(iView);
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Click event of the MnuNewReflectionChannel menu item.</summary>
        /// <remarks>If there currently is a textchannel active, we ask the user if he
        ///     wants to use the active textsin as a back end or to create a new one.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewChatBotChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                TextSin iBackEnd = null;
                if (BrainData.Current.ActiveDocument != null && BrainData.Current.ActiveDocument is TextChannel)
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            "Would you like to use the current Text sensory interface as a back-end for the chatbot channel? If you select 'no', a new one will be created.", 
                            "New chatbot", 
                            System.Windows.MessageBoxButton.YesNoCancel, 
                            System.Windows.MessageBoxImage.Question);
                    if (iRes == System.Windows.MessageBoxResult.Yes)
                    {
                        iBackEnd = ((TextChannel)BrainData.Current.ActiveDocument).Sin as TextSin;
                    }
                    else if (iRes == System.Windows.MessageBoxResult.No)
                    {
                        iBackEnd = new TextSin();
                        iBackEnd.Text = "ChatbotBackend";
                        AddItemToBrain(iBackEnd);
                    }
                }
                else
                {
                    iBackEnd = new TextSin();
                    iBackEnd.Text = "ChatbotBackend";
                    AddItemToBrain(iBackEnd);
                }

                if (iBackEnd != null)
                {
                    // could be null if user canceled operation.
                    var iNew = new IntSin();
                    AddItemToBrain(iNew);
                    Link.Create(iBackEnd, iNew, (ulong)PredefinedNeurons.IntSin);

                        // so we can easily find the intSin related to the TextSin, when we need to request a selection.
                    iNew.Text = "NewChatbot";
                    var iView = new ChatBotChannel();
                    iView.NeuronID = iNew.ID;
                    iView.TextSin = iBackEnd;
                    iView.IsVisible = true;

                    // iView.Name = iNew.Name;
                    BrainData.Current.CommChannels.Add(iView); // this stores the item in the brain.
                    ActivateDoc(iView);
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action(UndoStore.EndUndoGroup));

                    // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
            }
        }

        /// <summary>Handles the Executed event of the NewMindMap control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewMindMap_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = new MindMap();
                    iNew.Name = "New mind map";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    AddItemToOpenDocuments(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Adds the <paramref name="item"/> to open documents and makes certain
        ///     it is activated.</summary>
        /// <param name="item">The item.</param>
        public void AddItemToOpenDocuments(object item)
        {
            BrainData.Current.OpenDocuments.Add(item); // this actually shows the item
            ActivateDoc(item);
        }

        /// <summary>Handles the Executed event of the NewFrameEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewFrameEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = new FrameEditor();
                    iNew.Name = "New frame editor";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new visual editor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void NewVisualEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = new VisualEditor();
                    iNew.Name = "New visual editor";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new pattern editor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void NewPatternEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iCluster = NeuronFactory.GetCluster();
                    AddItemToBrain(iCluster);
                    iCluster.Meaning = (ulong)PredefinedNeurons.TextPatternTopic;
                    var iNew = new TextPatternEditor(iCluster);
                    iNew.Name = Parsers.TopicsDictionary.GetUnique("New topic ");
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new query_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void NewQuery_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iQuery = new Queries.Query();
                    AddItemToBrain(iQuery);
                    var iEditor = new QueryEditor(iQuery);
                    iEditor.Name = "Query";
                    BrainData.Current.CurrentEditorsList.Add(iEditor);
                    BrainData.Current.OpenDocuments.Add(iEditor); // this actually shows the item
                    ActivateDoc(iEditor);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new asset editor_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void NewAssetEditor_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iCluster = NeuronFactory.GetCluster();
                    Brain.Current.MakeTemp(iCluster);

                        // don't register, but make temp, so that if there is no editing, the cluster gets removed.
                    var iNew = new AssetEditor(iCluster);
                    iNew.Name = "New asset editor";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The new test case_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NewTestCase_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                var iName = new DlgStringQuestion();
                iName.Question = "Name of test case:";
                iName.Answer = "new test case";
                var iRes = iName.ShowDialog();
                if (iRes.HasValue && iRes.Value)
                {
                    var iNew = new Test.TestCase();
                    iNew.Name = iName.Answer;
                    iNew.IsChanged = true; // need to make certain it gets saved.
                    var iTextChannels = iNew.TextChannels;
                    if (iTextChannels.Count > 0)
                    {
                        iNew.RunOn = iTextChannels[0];
                    }

                    BrainData.Current.TestCases.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the Executed event of the NewFlow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewFlow_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = new FlowEditor();
                    iNew.Name = "New Flow Editor";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                    BrainData.Current.OpenDocuments.Add(iNew); // this actually shows the item
                    ActivateDoc(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the Executed event of the NewFolder control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewFolder_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = new EditorFolder();
                    iNew.Name = "New folder";
                    BrainData.Current.CurrentEditorsList.Add(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the Executed event of the NewCodeCluster control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewCodeCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iCluster = NeuronFactory.GetCluster();
                    iCluster.Meaning = (ulong)PredefinedNeurons.Code;
                    AddItemToBrain(iCluster);
                    var iCode = new CodeEditor(iCluster);
                    iCode.Name = "New code cluster";
                    BrainData.Current.CurrentEditorsList.Add(iCode);
                    BrainData.Current.CodeEditors.Add(iCode);
                    BrainData.Current.OpenDocuments.Add(iCode); // this actually shows the item
                    ActivateDoc(iCode);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the Executed event of the NewCodeCluster control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            UndoStore.BeginUndoGroup();
            try
            {
                if (BrainData.Current.CurrentEditorsList != null)
                {
                    var iNew = NeuronFactory.GetNeuron();
                    AddItemToBrain(iNew);
                    var iCode = new CodeEditor(iNew);
                    iCode.Name = "New neuron";
                    BrainData.Current.CurrentEditorsList.Add(iCode);
                    BrainData.Current.CodeEditors.Add(iCode);
                    BrainData.Current.OpenDocuments.Add(iCode); // this actually shows the item
                    ActivateDoc(iCode);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "There is no project folder selected to put new items in.");
                }
            }
            finally
            {
                UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region Docking control

        /// <summary>The restore layout.</summary>
        private void RestoreLayout()
        {
            var iPath = GetLayoutFileName();
            if (System.IO.File.Exists(iPath) == false)
            {
                iPath = GetLayoutFileName(NeuronDataDictionary.DefaultDataPath);
            }

            if (System.IO.File.Exists(iPath))
            {
                // object[] iOpenDocs = BrainData.Current.OpenDocuments.ToArray();
                // BrainData.Current.OpenDocuments.Clear();                                      //we can't reload the layout while there are documents open. When starting the app with project path specfied, the open documents are already loaded (channels), so we need to take account with this.
                RestoreLayout(iPath);

                // foreach (object i in iOpenDocs)
                // BrainData.Current.OpenDocuments.Add(i);
            }

#if BASIC || PRO
            if (ProjectManager.Default.IsViewerVisibility == System.Windows.Visibility.Collapsed)
            {
                ToolsList.Default.SetViewer();
            }
            else
            {
                ToolsList.Default.SetPro();
            }

#else
         if (ProjectManager.Default.IsViewerVisibility == Visibility.Collapsed)        // if we are in Viewer mode, hide it all
            ToolsList.Default.SetViewer();
#endif
        }

        /// <summary>restores the layout of the documents and tool windows from the last
        ///     session.</summary>
        /// <param name="path"></param>
        private void RestoreLayout(string path)
        {
            try
            {
                var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(MainDock);
                using (var stream = new System.IO.StreamReader(path)) serializer.Deserialize(stream);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Application", "Failed to restore previous layout: " + e.Message);
            }
        }

        /// <summary>The get layout file name.</summary>
        /// <param name="root">The root.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetLayoutFileName(string root = null)
        {
            string iPath;
            if (root == null)
            {
                root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                iPath = System.IO.Path.Combine(root, "NND");
            }
            else
            {
                iPath = root; // don't need to add 'nnd' at the end when we want a layout from the default data.
            }

            if (System.IO.Directory.Exists(iPath) == false)
            {
                System.IO.Directory.CreateDirectory(iPath);
            }

            if (ProjectManager.Default.IsSandBox == false)
            {
                if (ProjectManager.Default.IsViewerVisibility == System.Windows.Visibility.Collapsed)
                {
                    // if we are in Viewer mode, hide it all
                    iPath = System.IO.Path.Combine(iPath, App.VIEWERLAYOUTFILE);
                }
                else
                {
#if BASIC
               iPath = Path.Combine(iPath, App.BASICDESIGNLAYOUTFILE);
#elif PRO
                    iPath = System.IO.Path.Combine(iPath, App.PRODESIGNLAYOUTFILE);
#else
               iPath = Path.Combine(iPath, App.DESIGNLAYOUTFILE);
#endif
                }
            }
            else
            {
                iPath = System.IO.Path.Combine(iPath, App.SANDBOXLAYOUTFILE);
            }

            return iPath;
        }

        /// <summary>Trigged whenever an item inside the docking control gets keyboard
        ///     focus. This is done to check if the new selected item has as data
        ///     context, something who's description can be edited.</summary>
        /// <remarks>This function makes a copy of the DocumentFlow, this way, it can be
        ///     displayed in multiple editors (like a note and the global view).</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void DockingControl_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDescEditor = e.OriginalSource as System.Windows.Controls.RichTextBox;
            if (iDescEditor == null || iDescEditor.Name != "DescriptionEditor")
            {
                // don't do anything if we are moving to the global editor.
                var iTool = ToolsList.Default.DescriptionTool;
                System.Diagnostics.Debug.Assert(iTool != null);
                var iSender = e.OriginalSource as System.Windows.FrameworkElement;
                object iData = null;

                // if (iSender is LayoutAnchorablePane)                                                                    //need to do convertion, in case the editor space (background) is selected, which usually gives the toolframe, so get it's contents.
                // iData = ((LayoutAnchorablePane)iSender).Content;
                // else 
                if (iSender != null)
                {
                    iData = iSender.DataContext;
                }

                if (iData != null)
                {
                    IDescriptionable iFound = null;

                    fCurrentNeuronInfo = iData as INeuronInfo;
                    if (fCurrentNeuronInfo != null && fCurrentNeuronInfo.NeuronInfo != null)
                    {
                        iFound = fCurrentNeuronInfo.NeuronInfo;
                    }
                    else
                    {
                        iFound = iData as IDescriptionable;
                    }

                    if (iFound != null)
                    {
                        // if we haven't found anything, we simply leave the previous itemm visible.
                        DescriptionView.SetDocument(iFound.Description);
                        CurrentDescription = iFound;
                        iTool.Title = "Description: " + iFound.DescriptionTitle; // done by binding.
                    }
                }
                else
                {
                    DescriptionView.SetDocument(null);
                    iTool.Title = "Description";
                }
            }
        }

        /// <summary>Need to save the data back to the flowdocument when old focus is the
        ///     global editor. If old focused is the editor of the current document,
        ///     save to global.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DockingControlContent_PrvLostKeyboardFocus(
            object sender, 
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            // FlushDescriptionData(e.OldFocus as RichTextBox);
        }

        #endregion

        #region Undo

        /// <summary>Handles the SetFocused event of the <see cref="fUndoStore"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UndoSystem.UndoStoreEventArgs"/> instance containing the event
        ///     data.</param>
        private static void fUndoStore_SetFocused(object sender, UndoSystem.UndoStoreEventArgs e)
        {
            var iPath = e.Item.FocusedElement as Search.DisplayPath;
            if (iPath != null)
            {
                iPath.SelectPathResult();
            }
        }

        /// <summary>Handles the GetFocused event of the <see cref="fUndoStore"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UndoSystem.UndoStoreEventArgs"/> instance containing the event
        ///     data.</param>
        private static void fUndoStore_GetFocused(object sender, UndoSystem.UndoStoreEventArgs e)
        {
            if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
            {
                // we can only do this if the call comes from the UI thread. This gets called during the TopicImport for instance.
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null)
                {
                    var iPathBuilder = iFocused.DataContext as Search.IDisplayPathBuilder;
                    if (iPathBuilder != null)
                    {
                        e.Item.FocusedElement = iPathBuilder.GetDisplayPathFromThis();
                    }
                }
            }
        }

        #endregion

        /// <summary>The button_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.GC.Collect();
        }

        /// <summary>Handles the Click event of the BtnCancelSearch control: Cancels a
        ///     search operation.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCancelSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                var iSub = iSender.DataContext as Search.SearcherProcess;
                if (iSub == null)
                {
                    Search.ProcessTracker.Default.CancelAll();
                }
                else
                {
                    iSub.Cancel();
                }
            }
        }

        /// <summary>The btn close tl frame_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnCloseTlFrame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.FrameworkElement)sender;
            BrainData.Current.OpenDocuments.Remove(iSender.DataContext);
        }

        /// <summary>Handles the MouseDoubleClick event of the TotalProcCound control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TotalProcCound_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ProcessorManager.Current.TotalProcessorCount = 0;
            }
        }

        /// <summary>The document container_ closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DocumentContainer_Closed(object sender, AvalonDock.DocumentClosedEventArgs e)
        {
            BrainData.Current.OpenDocuments.Remove(e.Document.Content);

                // remove from the OpenDocuments list, which will also remove it from the AvalonsDoc
        }

        #endregion
    }
}