// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for PageRogetThesaurus.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for PageRogetThesaurus.xaml
    /// </summary>
    public partial class ThesaurusView : System.Windows.Controls.UserControl
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThesaurusView" /> class.
        /// </summary>
        public ThesaurusView()
        {
            InitializeComponent();
        }

        #endregion

        #region Selection

        /// <summary>Handles the PreviewMouseDown event of the SubRelExpander control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void SubRelExpander_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                iThes.SelectedSubRelationship = iSender.DataContext as ThesaurusSubItemCollection;
            }
        }

        #endregion

        /// <summary>Handles the Selected event of the SubItems control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SubItems_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSource = e.OriginalSource as System.Windows.FrameworkElement;
            if (iSource != null)
            {
                var iSub = iSource.DataContext as ThesaurusSubItem;
                if (iSub != null)
                {
                    iSub.IsSelected = true;
                }
            }
        }

        /// <summary>Handles the Click event of the MnuItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iBuilder = new ThesaurusBuilder();
            iBuilder.Start();
            e.Handled = true;
        }

        /// <summary>Handles the PreviewMouseDown event of the TreeItems control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void TreeItems_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource == ThesPanel)
            {
                // if we don't filter on this, each time the contextmenu opens, we loose the selected item, which we don't want.
                var iThes = (Thesaurus)DataContext;
                if (iThes.SelectedItem != null)
                {
                    iThes.SelectedItem.IsSelected = false;
                }

                // iThes.SelectedItem = null;
                // if (fSelectedTrvItem != null)
                // fSelectedTrvItem.IsSelected = false;            //when the user clicks on the empty space of the treeview, we must unselect the item so that the commands work correctly.
                Focus();

                    // put focus, so we get the correct context (otherwise the commands in the contextmenu might not work).
                e.Handled = true; // otherwise the focus dont' work.
            }
        }

        /// <summary>Handles the Click event of the NewConjType control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectConjTypes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iThes = (Thesaurus)DataContext;
            System.Diagnostics.Debug.Assert(iThes != null);
            var iDlg = new Dialogs.DlgSelectMeanings();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            iDlg.SelectedItems = (from i in iThes.ConjugationMeanings select i.Item).ToList();
            if (iDlg.ShowDialog() == true)
            {
                foreach (var i in iDlg.UnselectedItems)
                {
                    var iFound = (from u in iThes.ConjugationMeanings where u.Item == i select u).FirstOrDefault();
                    System.Diagnostics.Debug.Assert(iFound != null);
                    iThes.ConjugationMeanings.Remove(iFound);
                }

                foreach (var i in iDlg.NewSelectedItems)
                {
                    var iRel = new ThesaurusRelItem(i);
                    iThes.ConjugationMeanings.Add(iRel);
                }
            }
        }

        /// <summary>Handles the PreviewMouseRightButtonDown event of the ThsTreeViewItem
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void ThsTreeViewItem_PreviewMouseRightButtonDown(
            object sender, 
            System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.TreeViewItem;
            if (item != null)
            {
                item.Focus();
            }
        }

        /// <summary>Handles the TiltWheel event of the ThesPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The<see cref="JaStDev.HAB.Designer.WPF.Controls.MouseTiltEventArgs"/>
        ///     instance containing the event data.</param>
        private void ThesPanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            var iThes = (Thesaurus)DataContext;
            iThes.ThesHorScrollPos += e.Tilt;
        }

        /// <summary>Handles the SelectionChanged event of the LstObjectRelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void LstObjectRelated_SelectionChanged(
            object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListView;
            if (iSender != null && iSender.IsLoaded)
            {
                var iThes = iSender.DataContext as Thesaurus;
                iThes.ObjectRelated.SelectedIndex = iSender.SelectedIndex;
            }
        }

        /// <summary>Handles the SelectionChanged event of the LstPosRelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void LstPosRelated_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListView;
            if (iSender != null && iSender.IsLoaded)
            {
                var iThes = iSender.DataContext as Thesaurus;
                iThes.POSRelated.SelectedIndex = iSender.SelectedIndex;
            }
        }

        #region NeedsBringIntoView

        /// <summary>
        ///     NeedsBringIntoView Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty NeedsBringIntoViewProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "NeedsBringIntoView", 
                typeof(bool), 
                typeof(ThesaurusView), 
                new System.Windows.FrameworkPropertyMetadata(false, OnNeedsBringIntoViewChanged));

        /// <summary>Gets the NeedsBringIntoView property. This attached property indicates
        ///     wether this object needs to be brough into the viewable area.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool GetNeedsBringIntoView(System.Windows.DependencyObject d)
        {
            return (bool)d.GetValue(NeedsBringIntoViewProperty);
        }

        /// <summary>Sets the NeedsBringIntoView property. This attached property indicates
        ///     wether this object needs to be brough into the viewable area.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetNeedsBringIntoView(System.Windows.DependencyObject d, bool value)
        {
            d.SetValue(NeedsBringIntoViewProperty, value);
        }

        /// <summary>Handles changes to the NeedsBringIntoView property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnNeedsBringIntoViewChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var iSender = d as System.Windows.Controls.TreeViewItem;
                if (iSender != null)
                {
                    iSender.Loaded += iSender_Loaded;
                }
            }
        }

        /// <summary>The i sender_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void iSender_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.TreeViewItem;
            iSender.Loaded -= iSender_Loaded;
            iSender.BringIntoView();
            SetNeedsBringIntoView(iSender, false); // reset
        }

        #endregion

        #region Add

        /// <summary>Handles the Executed event of the NewSynonymCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewSynonymCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            if (iThes != null && iThes.SelectedItem != null)
            {
                var iIn = new DlgStringQuestion();
                iIn.Owner = System.Windows.Application.Current.MainWindow;
                iIn.Question = "Value:";
                iIn.Title = "New synonym";
                if ((bool)iIn.ShowDialog())
                {
                    var iStrVal = iIn.Answer.ToLower();
                    EditorsHelper.AddSynonym(iThes.SelectedItem.Item as NeuronCluster, iStrVal, iThes.SelectedItem.POS);
                }
            }
        }

        /// <summary>Handles the CanExecute event of the NewSynonymCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewSynonymCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the NewItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewItemCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iObj = EditorsHelper.CreateNewObject(false, iThes.SelectedPOSFilter);
                if (iObj != null)
                {
                    if (iThes.SelectedItem != null && iThes.SelectedItem.Owner != null)
                    {
                        // when no item selected, add as root. When selected item has no owner, it's been removed.
                        Thesaurus.CreateRelationship(iThes.SelectedItem.Item, iObj, iThes.SelectedRelationship);
                        iThes.SelectedItem.IsExpanded = true; // always make certain that the new item is expanded.
                    }
                    else
                    {
                        iThes.AddRootItem(iThes.SelectedRelationship, iObj);
                        var iUndo = new RootItemUndoData(BrainAction.Created, iThes.SelectedRelationship, iObj);
                        WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the NewItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewItemCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedRelationship != null;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the NewSubItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewSubItemCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iObj = EditorsHelper.CreateNewObject(false, iThes.SelectedPOSFilter);
                if (iObj != null)
                {
                    iThes.SelectedSubRelationship.Add(new ThesaurusSubItem(iObj));

                        // SelectedSubrelationship is guaranteed assigned, because of the CanExecute check.
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the NewSubItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NewSubItemCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedSubRelationship != null;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the AddThesaurusRelationshipCmd control.</summary>
        /// <remarks>Adds a new recursive relationship to use.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddThesaurusRelationshipCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iDlg = new DlgSelectMeaning();
                var iRes = iDlg.ShowDialog();
                if (iRes.HasValue && iRes.Value)
                {
                    var iNew = iThes.CreateRelationship(iDlg.SelectedValue);
                    var iTrue = Brain.Current.TrueNeuron;
                    if (Link.Exists(iDlg.SelectedValue, iTrue, (ulong)PredefinedNeurons.IsRecursive) == false)
                    {
                        // this type of relatinship is recursive, cause it is added to th relationships list.
                        var iLink = Link.Create(iDlg.SelectedValue, iTrue, (ulong)PredefinedNeurons.IsRecursive);
                        var iLinkUndo = new LinkUndoItem(iLink, BrainAction.Created);
                        WindowMain.UndoStore.AddCustomUndoItem(iLinkUndo);
                    }

                    iThes.SelectedRelationshipIndex = iThes.Relationships.Count - 1;

                        // also select the newly added relationship.
                    var iThesUndo = new ThesRelItemUndoData(BrainAction.Created, iDlg.SelectedValue.ID, iNew);
                    WindowMain.UndoStore.AddCustomUndoItem(iThesUndo);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the AddNoRecursiveThesRelCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddNoRecursiveThesRelCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iDlg = new DlgSelectMeaning();
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                var iPrevSubItemsLoaded = iThes.IsSubItemsLoaded;
                iThes.IsSubItemsLoaded = true;

                    // we need to check if the current item doesn't have a relationship aleady for the specified relationship, so check for this, and if not, add it.
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iExisting =
                        (from i in iThes.NoRecursiveRelationships where i.Item == iDlg.SelectedValue select i)
                            .FirstOrDefault();
                    if (iExisting == null)
                    {
                        var iRelItem = new ThesaurusRelItem(iDlg.SelectedValue);
                        var iTrue = Brain.Current.TrueNeuron;
                        var iLink = Link.Find(
                            iDlg.SelectedValue, 
                            iTrue, 
                            Brain.Current[(ulong)PredefinedNeurons.IsRecursive]);
                        if (iLink != null)
                        {
                            var iUndo = new LinkUndoItem(iLink, BrainAction.Removed);
                            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                            iLink.Destroy();
                        }

                        iThes.NoRecursiveRelationships.Add(iRelItem);
                    }

                    var iSubCol =
                        (from i in iThes.SubItems where i.Relationship == iDlg.SelectedValue select i).FirstOrDefault();

                        // SubItems is always filled in since the command can only be executed when there is a selected ThesaurusItem, and we set the isloaded to true.
                    if (iSubCol == null)
                    {
                        var iCluster = NeuronFactory.GetCluster();
                        iCluster.Meaning = iDlg.SelectedValue.ID;
                        WindowMain.AddItemToBrain(iCluster);
                        var iLink = new Link(iCluster, iThes.SelectedItem.Item, iDlg.SelectedValue.ID);
                        var iLinkUndo = new LinkUndoItem(iLink, BrainAction.Created);
                        WindowMain.UndoStore.AddCustomUndoItem(iLinkUndo);
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                    iThes.IsSubItemsLoaded = iPrevSubItemsLoaded;
                }
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the AddNoRecursiveThesRelCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddNoRecursiveThesRelCmd_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedItem != null;
            e.Handled = true;
        }

        /// <summary>The add conj_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddConj_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iItem = new ThesaurusLinkedItem(iThes.SelectedItem.Item);
                iThes.Conjugations.Items.Add(iItem);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the NewSubItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddConj_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = LstConjugations.ItemsSource != null && iThes.SelectedItem != null;

                // there needs to be an itemsSource, we don't check directly on thesaurus, cause we don't want to regenerate a new object if not needed.
            e.Handled = true;
        }

        #endregion

        #region Add pos related

        /// <summary>Handles the CanExecute event of the AddPosRelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddPosRelated_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = LstPosRelated.ItemsSource != null && iThes.SelectedItem != null;

                // there needs to be an itemsSource, we don't check directly on thesaurus, cause we don't want to regenerate a new object if not needed.
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the AddAddPosRelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddAddPosRelated_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iItem = new ThesaurusLinkedItem(iThes.SelectedItem.Item);
                iThes.POSRelated.Items.Add(iItem);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the AddAddPosRelated control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddObjectRelated_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iItem = new ThesaurusLinkedItem(iThes.SelectedItem.Item);
                iThes.ObjectRelated.Items.Add(iItem);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        #endregion

        #region delete

        /// <summary>Handles the CanExecute event of the DeleteConj control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteConj_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstConjugations.ItemsSource != null && LstConjugations.SelectedItem != null;

                // there needs to be an itemsSource, we don't check directly on thesaurus, cause we don't want to regenerate a new object if not needed.
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the DeleteCon control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteCon_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iToDel = LstConjugations.SelectedItem as ThesaurusLinkedItem;
                System.Diagnostics.Debug.Assert(iToDel != null);
                if (iToDel.Delete() == false)
                {
                    var iList = LstConjugations.ItemsSource as System.Collections.Generic.IList<ThesaurusLinkedItem>;
                    iList.Remove(iToDel);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the DeletePos control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeletePos_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstPosRelated.ItemsSource != null && LstPosRelated.SelectedItem != null;

                // there needs to be an itemsSource, we don't check directly on thesaurus, cause we don't want to regenerate a new object if not needed.
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the DeletePos control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeletePos_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iToDel = LstPosRelated.SelectedItem as ThesaurusLinkedItem;
                System.Diagnostics.Debug.Assert(iToDel != null);
                if (iToDel.Delete() == false)
                {
                    var iList = LstPosRelated.ItemsSource as System.Collections.Generic.IList<ThesaurusLinkedItem>;
                    iList.Remove(iToDel);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the DeletePos control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteObject_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstObjectRelated.ItemsSource != null && LstObjectRelated.SelectedItem != null;

                // there needs to be an itemsSource, we don't check directly on thesaurus, cause we don't want to regenerate a new object if not needed.
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the DeletePos control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteObject_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iToDel = LstObjectRelated.SelectedItem as ThesaurusLinkedItem;
                System.Diagnostics.Debug.Assert(iToDel != null);
                if (iToDel.Delete() == false)
                {
                    var iList = LstObjectRelated.ItemsSource as System.Collections.Generic.IList<ThesaurusLinkedItem>;
                    iList.Remove(iToDel);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        #endregion

        #region Delete special

        /// <summary>The delete special_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSpecial_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            var iSender = e.OriginalSource as System.Windows.FrameworkElement;
            var iItem = iSender.DataContext as ThesaurusItem;
            e.CanExecute = iItem != null || iThes.SelectedItem != null;
            e.Handled = true;
        }

        /// <summary>The delete special_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void DeleteSpecial_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new DlgSelectDeletionMethod();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                var iThes = DataContext as Thesaurus;
                var iSender = e.OriginalSource as System.Windows.FrameworkElement;
                var iItem = iSender.DataContext as ThesaurusItem; // can be null

                Neuron iToDelete;
                if (((string)e.Parameter) == "sub")
                {
                    iToDelete = iThes.SubItems.SelectedItem.SelectedItem.Item;
                }
                else
                {
                    iToDelete = iItem != null ? iItem.Item : iThes.SelectedItem.Item;
                }

                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we group all the data together so a single undo command cand restore the previous state.
                try
                {
                    NeuronDeleter iDeleter;
                    switch (iDlg.NeuronDeletionMethod)
                    {
                        case DeletionMethod.Remove:
                            if (((string)e.Parameter) == "sub")
                            {
                                RemoveSubItemCmd_Executed(sender, e);
                            }
                            else
                            {
                                RemoveItem(iItem);
                            }

                            break;
                        case DeletionMethod.DeleteIfNoRef:
                            if (((string)e.Parameter) == "sub")
                            {
                                RemoveSubItemCmd_Executed(sender, e);
                            }
                            else
                            {
                                RemoveItem(iItem);
                            }

                            iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                            iDeleter.Start(iToDelete);
                            break;
                        case DeletionMethod.Delete:
                            if (iToDelete.CanBeDeleted && BrainData.Current.NeuronInfo[iToDelete.ID].IsLocked == false)
                            {
                                iDeleter = new NeuronDeleter(DeletionMethod.Delete, iDlg.BranchHandling);
                                iDeleter.Start(iToDelete);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show(
                                    string.Format(
                                        "Neuron {0} can't be deleted because it is locked by the project or used as a meaning or info.", 
                                        iItem), 
                                    "Delete", 
                                    System.Windows.MessageBoxButton.OK, 
                                    System.Windows.MessageBoxImage.Warning);
                            }

                            break;
                        default:
                            throw new System.InvalidOperationException();
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion

        #region Delete

        /// <summary>Deletes a thesaurus item + any text patterns attached to the thesaurus
        ///     item. First checks if there are any children and asks if the children
        ///     also need to be deleted. This is only called on the thesaurus, not on
        ///     child items.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iThes = DataContext as Thesaurus;
                if (iThes != null && iThes.SelectedItem != null)
                {
                    var iToWrapper = iThes.SelectedItem.Owner as INeuronWrapper;

                        // check if it is a root or not. This is fast: if the owner is the thesaurus, it isn't a neuronwrapper.
                    if (iToWrapper == null)
                    {
                        // if it's a root item, we need to generate some extra undo data, otherwise we can't properly undo the deletion of a root item.
                        var iUndo = new RootItemUndoData(
                            BrainAction.Removed, 
                            iThes.SelectedRelationship, 
                            iThes.SelectedItem.Item);
                        WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    }

                    EditorsHelper.DeleteThesaurusItem((NeuronCluster)iThes.SelectedItem.Item);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Deletes a sub thesaurus item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteSubItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);
            if (iThes.IsSubItemsLoaded)
            {
                var iSubItems = iThes.SubItems.SelectedItem;
                if (iSubItems != null && iSubItems.SelectedItem != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        DeleteSubThesItem(iSubItems.SelectedItem);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>The delete sub thes item.</summary>
        /// <param name="item">The item.</param>
        private void DeleteSubThesItem(ThesaurusSubItem item)
        {
            if (item.Item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic) != null)
            {
                // if the sub item has a textpattern, also delete this.
                var iEditor = new ObjectTextPatternEditor(item.Item); // don't need to open it, the delete does this.
                EditorsHelper.DeleteTextPatternEditor(iEditor);
            }

            EditorsHelper.DeleteThesaurusItem((NeuronCluster)item.Item);
        }

        /// <summary>deletes a pattern editor that is attached to the selected thesaurus
        ///     item</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuDeletePatterns_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iThes = DataContext as Thesaurus;
                if (iThes != null && iThes.SelectedItem != null)
                {
                    if (iThes.SelectedItem.HasTextPattern)
                    {
                        var iEditor = new ObjectTextPatternEditor(iThes.SelectedItem.Item);
                        BrainData.Current.OpenDocuments.Remove(iEditor);

                            // need to make certain that this editor is closed, normally, the caller of DeleteEditor is responsible for this, but he doesn't know about the sub editor, so need to do this manually.
                        EditorsHelper.DeleteTextPatternEditor(iEditor);
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region Remove

        /// <summary>Handles the CanExecute event of the RemoveCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveItemCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);
            e.CanExecute = iThes.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the RemoveCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveItemCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iThes = DataContext as Thesaurus;
                System.Diagnostics.Debug.Assert(iThes != null && iThes.SelectedItem != null);
                RemoveItem(iThes.SelectedItem);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Removes the item from the thesaurus (curently selected relationship).</summary>
        /// <param name="toRemove">To remove.</param>
        private void RemoveItem(ThesaurusItem toRemove)
        {
            var iThes = DataContext as Thesaurus;
            var iToWrapper = toRemove.Owner as INeuronWrapper;

                // check if it is a root or not. This is fast: if the owner is the thesaurus, it isn't a neuronwrapper.
            if (iToWrapper == null)
            {
                iThes.RemoveRootItem(iThes.SelectedRelationship, toRemove.Item);
                var iUndo = new RootItemUndoData(BrainAction.Removed, iThes.SelectedRelationship, toRemove.Item);
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            }
            else
            {
                iThes.RemoveRelationship(iToWrapper.Item, toRemove.Item, iThes.SelectedRelationship);
            }
        }

        /// <summary>Handles the Executed event of the RemoveSubItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveSubItemCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);
            if (iThes.IsSubItemsLoaded)
            {
                var iSubItems = iThes.SubItems.SelectedItem;
                if (iSubItems != null && iSubItems.SelectedItem != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iSubItems.Remove(iSubItems.SelectedItem);

                            // this is a clustercollection, so the cluster gets updated automatically.
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Handles the CanExecute event of the RemoveSubItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveSubItemCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);
            if (iThes.IsSubItemsLoaded)
            {
                var iSubItems = iThes.SubItems.SelectedItem;
                e.CanExecute = iSubItems != null && iSubItems.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion

        #region Copy

        /// <summary>Handles the CanExecute event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Copy_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThesaurus = DataContext as Thesaurus;
            e.CanExecute = iThesaurus != null && iThesaurus.SelectedItem != null;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Copy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThesaurus = DataContext as Thesaurus;
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.NeuronIDFormat, iThesaurus.SelectedItem.Item.ID, false);
            System.Windows.Clipboard.SetDataObject(iData, false);
        }

        #endregion

        #region Cut/copy/paste linked Items

        /// <summary>Handles the CanExecute event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CopyLinkedItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iOrSource = sender as System.Windows.Controls.ListView;
            e.CanExecute = iOrSource.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CopyLinkedItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CopySelectedLinked(sender as System.Windows.Controls.ListView);
        }

        /// <summary>The copy selected linked.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="ThesaurusLinkedItem"/>.</returns>
        private ThesaurusLinkedItem CopySelectedLinked(System.Windows.Controls.ListView list)
        {
            System.Diagnostics.Debug.Assert(list != null);
            var iItem = list.SelectedItem as ThesaurusLinkedItem;
            var iList = new System.Collections.Generic.List<ulong>();
            if (iItem.Relationship != null)
            {
                iList.Add(iItem.Relationship.ID);
            }
            else
            {
                iList.Add(Neuron.EmptyId);
            }

            if (iItem.Related != null)
            {
                iList.Add(iItem.Related.ID);
            }
            else
            {
                iList.Add(Neuron.EmptyId);
            }

            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.MultiNeuronIDFormat, iList, false);
            System.Windows.Clipboard.SetDataObject(iData, false);
            return iItem;
        }

        /// <summary>Handles the CanExecute event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CutLinkedItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iOrSource = sender as System.Windows.Controls.ListView;
            e.CanExecute = iOrSource.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CutLinkedItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = CopySelectedLinked(sender as System.Windows.Controls.ListView);
            iItem.Delete();
        }

        /// <summary>Handles the CanExecute event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteLinkedItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            if (iThes.SelectedItem != null
                && System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
            {
                var iList =
                    (System.Collections.Generic.List<ulong>)
                    System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat);
                e.CanExecute = iList.Count == 2;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteLinkedItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            var iList =
                (System.Collections.Generic.List<ulong>)
                System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat);
            var iSender = sender as System.Windows.Controls.ListView;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iDest = iSender.ItemsSource as System.Collections.Generic.IList<ThesaurusLinkedItem>;
            System.Diagnostics.Debug.Assert(iDest != null);
            var iItem = new ThesaurusLinkedItem(iThes.SelectedItem.Item);
            iDest.Add(iItem);
            iItem.Relationship = Brain.Current[iList[0]];
            iItem.Related = Brain.Current[iList[1]];
        }

        #endregion

        #region Search commands

        /// <summary>Handles the KeyDown event of the TxtSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                FindNextCmd_Executed(sender, null);
                e.Handled = true;
            }
            else if (e.Key != System.Windows.Input.Key.F3)
            {
                // when the textbox is focused and we press f3 to go to the next item, we don't want to reset the search.
                var iThes = (Thesaurus)DataContext;
                System.Diagnostics.Debug.Assert(iThes != null);
                iThes.Searcher = null; // for each char that is changed, a new search needs to be performed.
            }
        }

        /// <summary>Handles the Executed event of the Find control. Simply focuses on the
        ///     search box.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Find_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            TxtSearch.Focus();
            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.IsNullOrEmpty(TxtSearch.Text) == false;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = (Thesaurus)DataContext;

            if (iThes.Searcher == null || iThes.Searcher.Process == null)
            {
                iThes.Searcher = new ThesSearcher(iThes);
                iThes.Searcher.Start(TxtSearch.Text.ToLower());
            }
            else if (iThes.Searcher.Process.IsRunning == false)
            {
                iThes.Searcher.Continue();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "The previous search is still running!", 
                    "Search", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            if (e != null)
            {
                // when called from the TExtBox, this is null cause we don't give the param along.
                e.Handled = true;
            }
        }

        #endregion

        #region import/export

        /// <summary>Handles the Executed event of the ImportCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iImporter = new ThesaurusFileImporter(); // no undo data for this operation.
            iImporter.Import(e.Parameter as ThesaurusItem, iThes.SelectedRelationship);
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the ExportCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iExporter = new ThesaurusFileExporter(); // no undo data for this operation.
            iExporter.Export(e.Parameter as ThesaurusItem, iThes.SelectedRelationship);
            e.Handled = true;
        }

        /// <summary>The menu item_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iExporter = new ThesaurusFileExporter(); // no undo data for this operation.
            iExporter.Export(null, iThes.SelectedRelationship, true);
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the ImportWordListCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportWordListCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iDlg = new Microsoft.Win32.OpenFileDialog();
                iDlg.Filter = Properties.Resources.CSVFileFilter;
                iDlg.FilterIndex = 1;
                iDlg.Multiselect = true;

                var iRes = iDlg.ShowDialog();
                if (iRes.HasValue && iRes == true)
                {
                    foreach (var iFile in iDlg.FileNames)
                    {
                        try
                        {
                            ThesauruscsvStreamer.Import(iFile, iThes.SelectedItem, iThes.SelectedRelationship);
                        }
                        catch (System.Exception ex)
                        {
                            LogService.Log.LogError(
                                "Import csv file", 
                                string.Format(
                                    "An error occured while trying to imort the file '{0}', with the error: {1}", 
                                    iFile, 
                                    ex.Message));
                        }
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the ImportWordListCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportWordListCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedItem != null;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the ImportClipboardWordListCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ImportClipboardWordListCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iText = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
                try
                {
                    ThesauruscsvStreamer.ImportString(iText, iThes.SelectedItem, iThes.SelectedRelationship);
                }
                catch (System.Exception ex)
                {
                    LogService.Log.LogError(
                        "Import from clipboard", 
                        string.Format(
                            "An error occured while trying to imort the data '{0}', with the error: {1}", 
                            iText, 
                            ex.Message));
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>The import clipboard word list cmd_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ImportClipboardWordListCmd_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedItem != null
                           && System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.Text);
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the ExportWordListCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportWordListCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            System.Diagnostics.Debug.Assert(iThes != null);

            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.CSVFileFilter;
            iDlg.FileName = iThes.SelectedItem.NeuronInfo.DisplayTitle;
            iDlg.FilterIndex = 1;
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                ThesauruscsvStreamer.Export(iDlg.FileName, iThes.SelectedItem, iThes.SelectedRelationship);
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the ExportWordListCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ExportWordListCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iThes = DataContext as Thesaurus;
            e.CanExecute = iThes != null && iThes.SelectedItem != null;
            e.Handled = true;
        }

        #endregion
    }
}