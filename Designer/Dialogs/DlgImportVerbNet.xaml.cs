// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgImportVerbNet.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgImportVerbNet.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for DlgImportVerbNet.xaml
    /// </summary>
    public partial class DlgImportVerbNet : System.Windows.Window, VerbNetProvider.IVNLabeler
    {
        /// <summary>Initializes a new instance of the <see cref="DlgImportVerbNet"/> class.</summary>
        public DlgImportVerbNet()
        {
            InitializeComponent();
        }

        /// <summary>The btn import_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iImporter = new VerbNetProvider.VerbNet();
            iImporter.Labeler = this;

            // iImporter.Roles
            var iImportInto = ImportInto;
            if (iImportInto != null)
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                try
                {
                    var iRes =
                        iImporter.ImportSelected((System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext);
                    foreach (var i in iRes)
                    {
                        iImportInto.Frames.Add(new Frame(i));
                    }

                    if (BrainData.Current.OpenDocuments.Contains(iImportInto) == false)
                    {
                        BrainData.Current.OpenDocuments.Add(iImportInto);
                    }

                    Close();
                }
                finally
                {
                    System.Windows.Input.Mouse.OverrideCursor = null;
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Please select a frame editor to import the frames to.", 
                    "Import from verbnet", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        /// <summary>The btn close_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        #region fields

        /// <summary>The f cur search index.</summary>
        private int fCurSearchIndex = -1;

        /// <summary>The f frame editors.</summary>
        private System.Collections.ObjectModel.ObservableCollection<FrameEditor> fFrameEditors;

        /// <summary>The f selected.</summary>
        private System.Collections.Generic.List<VerbNetProvider.VNCLASS> fSelected;

                                                                         // required to reset previous  selection when select all checkbox is indeterminate. 
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
                typeof(DlgImportVerbNet), 
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
                typeof(DlgImportVerbNet), 
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

        #endregion

        #endregion

        #region Select

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

        /// <summary>The chk select al_ unchecked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iItems = (System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext;
            fSelected = (from i in iItems where i.NeedsImport select i).ToList();
            foreach (var i in iItems)
            {
                i.NeedsImport = false;
            }
        }

        /// <summary>The chk select al_ indeterminate.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Indeterminate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (fSelected != null)
            {
                // this event is triggered whenever an 'IsSelected' checkbox is clicked because the ChkSelectAll is reset.  Only need to reset when the sender is Chk
                var iItems = (System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext;
                foreach (var i in iItems)
                {
                    i.NeedsImport = false;
                }

                foreach (var i in fSelected)
                {
                    i.NeedsImport = true;
                }

                fSelected = null;
            }
        }

        /// <summary>The chk select al_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iItems = (System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext;
            foreach (var i in iItems)
            {
                i.NeedsImport = true;
            }
        }

        #endregion

        #region Frame editors

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

        #region Bring into view

        /// <summary>The trv verbs_ selected.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TrvVerbs_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.Controls.TreeViewItem)e.OriginalSource;
            iSender.BringIntoView();
        }

        /// <summary>The lst members_ selected.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void LstMembers_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.Controls.ListBoxItem)e.OriginalSource;
            iSender.BringIntoView();
        }

        #endregion

        #region search

        /// <summary>The search next_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SearchNext_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iItems = (System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext;
                var iToSearch = TxtSearch.Text;
                for (var i = fCurSearchIndex + 1; i < iItems.Count; i++)
                {
                    if (SearchInClass(iToSearch, i))
                    {
                        return;
                    }
                }

                for (var i = 0; i < fCurSearchIndex; i++)
                {
                    if (SearchInClass(iToSearch, i))
                    {
                        return;
                    }
                }

                System.Windows.MessageBox.Show("All items have been searched.");
                fCurSearchIndex = -1;
            }
            finally
            {
                e.Handled = true;
            }
        }

        /// <summary>Searches the in class.</summary>
        /// <param name="iToSearch">The i to search.</param>
        /// <param name="iIndex">The i Index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool SearchInClass(string iToSearch, int iIndex)
        {
            var iItems = (System.Collections.Generic.IList<VerbNetProvider.VNCLASS>)DataContext;
            var iClass = iItems[iIndex];
            if (iClass.ID.Contains(iToSearch))
            {
                SelectClass(iClass, iIndex);
                return true;
            }

            var iFound = (from ii in iClass.MEMBERS where ii.name.Contains(iToSearch) select ii).FirstOrDefault();
            if (iFound != null)
            {
                SelectClass(iClass, iIndex);
                System.Action<VerbNetProvider.MEMBERSMEMBER> iAsync = s => s.IsSelected = true;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    iAsync, 
                    System.Windows.Threading.DispatcherPriority.Background, 
                    iFound);

                    // we call async on UI thread so that the treeview item can be selected, so that the listbox item, containing all the Members, is also loaded, which allows us to bring things into view.
                return true;
            }

            return false;
        }

        /// <summary>Selects the specified class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        private void SelectClass(VerbNetProvider.VNCLASS item, int index)
        {
            item.IsSelected = true;
            fCurSearchIndex = index;
        }

        /// <summary>The txt search_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchNext_Click(sender, e);
            }
            else
            {
                fCurSearchIndex = -1; // when we search for a new word,start from the beginning.
            }
        }

        #endregion

        #region IVNLabeler Members

        /// <summary>Assigns the title to the specified neuron.</summary>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        public void SetTitle(Neuron item, string value)
        {
            BrainData.Current.NeuronInfo[item.ID].DisplayTitle = value;
        }

        /// <summary>The set info.</summary>
        /// <param name="item">The item.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        public void SetInfo(Neuron item, string title, string description)
        {
            var iInfo = BrainData.Current.NeuronInfo[item.ID];
            iInfo.DisplayTitle = title;
            iInfo.StoreDescription(description);
        }

        /// <summary>The store role root.</summary>
        /// <param name="value">The value.</param>
        public void StoreRoleRoot(ulong value)
        {
            LargeIDCollection iCol;
            if (BrainData.Current.Thesaurus.Data.TryGetValue((ulong)PredefinedNeurons.VerbNetRole, out iCol) == false)
            {
                iCol =
                    BrainData.Current.Thesaurus.CreateRelationship(Brain.Current[(ulong)PredefinedNeurons.VerbNetRole]);
            }

            iCol.Add(value);
        }

        /// <summary>The set ref to title.</summary>
        /// <param name="item">The item.</param>
        /// <param name="basedOn">The based on.</param>
        public void SetRefToTitle(Neuron item, Neuron basedOn)
        {
            BrainData.Current.NeuronInfo[item.ID].DisplayTitle = "Ref to: "
                                                                 + BrainData.Current.NeuronInfo[basedOn.ID].DisplayTitle;
        }

        #endregion
    }
}