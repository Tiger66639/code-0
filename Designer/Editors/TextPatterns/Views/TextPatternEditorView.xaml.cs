// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for TextPatternEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using JaStDev.HAB.Designer.Dialogs;
using JaStDev.HAB.Parsers;

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for TextPatternEditorView.xaml
    /// </summary>
    public partial class TextPatternEditorView : CtrlEditorBase, ITextPatternEditorView
    {
        // CompositeCollection fItemsSource;
        /// <summary>The f current edit mode.</summary>
        private EditMode fCurrentEditMode = EditMode.Normal;

        /// <summary>The f scroll main.</summary>
        private ScrollViewer fScrollMain;

                             // it's in a template, so we don't have a direct name, but we occationally need it, so.

        /// <summary>Initializes a new instance of the <see cref="TextPatternEditorView"/> class.</summary>
        public TextPatternEditorView()
        {
            InitializeComponent();
        }

        #region CurrentEditMode

        /// <summary>
        ///     Gets/sets the editing mode currently active. So we know when to move
        ///     keyboard focus or not.
        /// </summary>
        public EditMode CurrentEditMode
        {
            get
            {
                return fCurrentEditMode;
            }

            set
            {
                fCurrentEditMode = value;
            }
        }

        #endregion

        /// <summary>Raises the <see cref="System.Windows.FrameworkElement.SizeChanged"/>
        ///     event, using the specified information as part of the eventual event
        ///     data. To properly adjust the splitter position</summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TextPatternEditor iEditor = (TextPatternEditor)DataContext;
            if (iEditor != null && iEditor.AddValSplitPos.IsStar == true && sizeInfo.WidthChanged == true)
            {
                // try to assing an initial 'static/hard width' to the splitters, otherwise they keep working with star pos, which gives strange results.
                iEditor.AddValSplitPos = new GridLength(LstMain.ActualWidth / 2);
            }
        }

        /// <summary>Handles the MouseDown event of the ScrollMain control. When the user
        ///     clicks on the entire list, clear the selection.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void ScrollMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == this)
            {
                TextPatternEditor iEditor = DataContext as TextPatternEditor;
                if (iEditor != null)
                {
                    iEditor.SelectedItems.Clear();
                }
            }
        }

        /// <summary>Handles the MouseLeftButtonDown event of the This control. To clear
        ///     the selection when the user clicks on the blank screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void This_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).TemplatedParent == LstMain)
            {
                TextPatternEditor iEditor = DataContext as TextPatternEditor;
                if (iEditor != null)
                {
                    iEditor.SelectedItems.Clear();
                }
            }
        }

        /// <summary>Handles the Executed event of the Find control. Open the FindText
        ///     dialog, if not yet opened, and activate it.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = (TextPatternEditor)DataContext;
            if (iEditor != null)
            {
                string iToSearch = null;
                FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
                if (iFocused != null && iFocused.DataContext is TextPatternBase)
                {
                    TextPatternBase iData = iFocused.DataContext as TextPatternBase;
                    if (iData != null && iData.Selectionrange != null)
                    {
                        iToSearch = iData.Expression.Substring(iData.Selectionrange.Start, iData.Selectionrange.Length);
                    }
                }

                DlgFindText iDlg = DlgFindText.Default; // this will also show the dialog box.
                iEditor.LastSearchQuery = new FindInTextData(iEditor); // reset all the data for the search box.
                iEditor.LastSearchQuery.TextToSearch = iToSearch;
                iDlg.DataContext = iEditor.LastSearchQuery;
            }
        }

        /// <summary>Handles the Executed event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = (TextPatternEditor)DataContext;
            if (iEditor != null && iEditor.LastSearchQuery != null)
            {
                iEditor.LastSearchQuery.FindNext();
            }
        }

        /// <summary>Handles the CanExecute event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TextPatternEditor iEditor = (TextPatternEditor)DataContext;
            if (iEditor != null && iEditor.LastSearchQuery != null)
            {
                if (iEditor.LastSearchQuery.SearchScope == 0)
                {
                    e.CanExecute = iEditor.LastSearchQuery.RequestScope == iEditor;
                }
                else
                {
                    e.CanExecute = true;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the MouseLeftButtonDown event of the ContentControl control.
        ///     Called when the user clicks on the background of a rule. Will select
        ///     the entire rule.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContentControl iCtrl = sender as ContentControl;
            if (iCtrl != null)
            {
                PatternRule iRule = iCtrl.DataContext as PatternRule;
                if (iRule != null)
                {
                    iRule.IsSelected = true;
                    iCtrl.Focus();
                    e.Handled = true;
                }
            }
        }

        /// <summary>If the AddValSplitPos isn't yet initialised to a fixed size, do this
        ///     now.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                if (iEditor.AddValSplitPos.IsStar == true)
                {
                    iEditor.AddValSplitPos = new GridLength((ActualWidth - 6) / 2);
                }

                if (fScrollMain != null)
                {
                    fScrollMain.ScrollToHorizontalOffset(iEditor.HorScrollPos);
                    fScrollMain.ScrollToVerticalOffset(iEditor.VerScrollPos);
                }
            }
        }

        /// <summary>Whent he datacontext changes, also need to make certain that there is
        ///     a proper initial value.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextPatternEditor iEditor = e.NewValue as TextPatternEditor;
            if (iEditor != null && IsLoaded == true)
            {
                if (iEditor.AddValSplitPos.IsStar == true)
                {
                    iEditor.AddValSplitPos = new GridLength((ActualWidth - 6) / 2);
                }

                if (fScrollMain != null)
                {
                    fScrollMain.ScrollToHorizontalOffset(iEditor.HorScrollPos);
                    fScrollMain.ScrollToVerticalOffset(iEditor.VerScrollPos);
                }
            }
        }

        /// <summary>The scroll main_ scroll changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ScrollMain_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null && this.IsLoaded == true && fScrollMain != null)
            {
                iEditor.VerScrollPos = fScrollMain.VerticalOffset;
                iEditor.HorScrollPos = fScrollMain.HorizontalOffset;
            }
        }

        /// <summary>Handles the Executed event of the SelectAll control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.SelectedItems.Clear();
                if (iEditor.IsQuestionsSelected == true)
                {
                    foreach (PatternRuleOutput i in iEditor.Questions)
                    {
                        iEditor.SelectedItems.Add(i);
                    }
                }
                else
                {
                    foreach (PatternRule i in iEditor.Items)
                    {
                        iEditor.SelectedItems.Add(i);
                    }
                }
            }
        }

        /// <summary>The toggle all do_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleAllDo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.SelectedItems.Clear();
                if (iEditor.IsQuestionsSelected == true && iEditor.Questions.Count > 0)
                {
                    bool iVal = !iEditor.Questions[0].IsDoPatternVisible;
                    foreach (PatternRuleOutput i in iEditor.Questions)
                    {
                        i.IsDoPatternVisible = iVal;
                    }
                }
                else if (iEditor.Items.Count > 0)
                {
                    bool iVal = !iEditor.Items[0].IsToCalculateVisible;
                    foreach (PatternRule i in iEditor.Items)
                    {
                        i.IsToCalculateVisible = iVal;
                        if (i.Conditionals != null)
                        {
                            foreach (PatternRuleOutput u in i.Conditionals)
                            {
                                u.IsDoPatternVisible = iVal;
                                foreach (OutputPattern o in u.Outputs)
                                {
                                    o.IsDoExpanded = iVal;
                                }
                            }

                            foreach (OutputPattern o in i.Outputs)
                            {
                                o.IsDoExpanded = iVal;
                            }
                        }

                        i.OutputSet.IsDoPatternVisible = iVal;
                        foreach (OutputPattern o in i.Outputs)
                        {
                            o.IsDoExpanded = iVal;
                        }
                    }
                }
            }
        }

        /// <summary>The rad items select_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RadItemsSelect_Checked(object sender, RoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsItemsSelected = true;
            }
        }

        /// <summary>The rad questions select_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RadQuestionsSelect_Checked(object sender, RoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsQuestionsSelected = true;
            }
        }

        /// <summary>The rad filters select_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RadFiltersSelect_Checked(object sender, RoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsTopicsFiltersSelected = true;
            }
        }

        /// <summary>
        ///     Updates the spelling on this editor.
        /// </summary>
        public void RequestUpdateSpelling()
        {
            Properties.Settings.Default.EditorsUseSpellcheck = !Properties.Settings.Default.EditorsUseSpellcheck;

                // toggle the editorsettings 2 times, so that everything gets updated and set back to original setting.
            Properties.Settings.Default.EditorsUseSpellcheck = !Properties.Settings.Default.EditorsUseSpellcheck;
        }

        /// <summary>The sv main_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SVMain_Loaded(object sender, RoutedEventArgs e)
        {
            fScrollMain = sender as ScrollViewer;
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null && IsLoaded == true)
            {
                fScrollMain.ScrollToHorizontalOffset(iEditor.HorScrollPos);
                fScrollMain.ScrollToVerticalOffset(iEditor.VerScrollPos);
            }
        }

        #region Toggle

        /// <summary>Handles the Executed event of the ToggleQuestions control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ToggleQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsQuestionsSelected = true;
            }
        }

        /// <summary>The toggle topic filters_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleTopicFilters_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsTopicsFiltersSelected = true;
            }
        }

        /// <summary>Handles the Executed event of the ToggleStatements control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ToggleStatements_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iEditor != null)
            {
                iEditor.IsItemsSelected = true;
            }
        }

        #endregion

        #region rename

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            e.CanExecute = iEditor != null && iEditor.SelectedItems.Count == 1 && iEditor.SelectedItem is PatternRule;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            PatternRule iRule = iEditor.SelectedItem as PatternRule;
            if (iRule != null)
            {
                DlgStringQuestion iName = new DlgStringQuestion();
                iName.Question = "Change rule name";
                iName.Answer = iRule.NeuronInfo.DisplayTitle;
                iName.Owner = WindowMain.Current;
                bool? iRes = iName.ShowDialog();
                if (iRes.HasValue == true && iRes.Value == true)
                {
                    if (TopicsDictionary.CheckUniqueRuleName(
                        iName.Answer, 
                        ((INeuronWrapper)iRule.Owner).Item, 
                        iRule.Item) == false)
                    {
                        if (
                            MessageBox.Show(
                                string.Format(
                                    "'{0}' is already used as the name of a rule in the current context. Names should be unique. Do you wan to continue and assign this duplicate name?", 
                                    iName.Answer), 
                                "Duplicate names", 
                                MessageBoxButton.YesNo, 
                                MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    iRule.NeuronInfo.DisplayTitle = iName.Answer;
                }
            }
        }

        #endregion

        #region InsertItem

        /// <summary>Handles the CanExecute event of the InsertItemCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertItemCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
            e.CanExecute = iFocused != null
                           && (iFocused.DataContext is ParsableTextPatternBase || iFocused.DataContext is PatternRule);
        }

        /// <summary>Handles the Executed event of the InsertItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iFocused != null)
            {
                iEditor.SelectedItems.Clear();

                    // no items selected, so that we don't have multiple selections after the insert.
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    if (iFocused.DataContext is PatternRule)
                    {
                        PatternRule iRule = iFocused.DataContext as PatternRule;
                        CurrentEditMode = EditMode.AddPattern;
                        int iIndex = iEditor.Items.IndexOf(iRule);
                        PatternRule iRes = EditorsHelper.MakePatternRule(iEditor, iIndex);
                        Debug.Assert(iRes != null);
                        iRes.NeuronInfo.DisplayTitle = TopicsDictionary.GetUniqueRuleName(
                            "new rule ", 
                            iEditor.Items.Cluster);
                        InputPattern iNew = EditorsHelper.AddNewTextPattern(iRes.TextPatterns, string.Empty);
                        iNew.IsSelected = true;
                    }
                    else
                    {
                        ParsableTextPatternBase iItem = iFocused.DataContext as ParsableTextPatternBase;
                        CurrentEditMode = iItem.RequiredEditMode;
                        ParsableTextPatternBase iNew = iItem.Insert();
                        if (iNew != null)
                        {
                            iNew.IsSelected = true;
                            iItem.IsSelected = false;

                                // we deactivate the previous selection cause  the ctrl key is pressed during  the insert operation, causing  an 'add' in the selected prop, which we don't want.
                        }
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>Handles the Executed event of the InsertItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertItemAfterCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
            TextPatternEditor iEditor = DataContext as TextPatternEditor;
            if (iFocused != null)
            {
                iEditor.SelectedItems.Clear();

                    // no items selected, so that we don't have multiple selections after the insert.
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    if (iFocused.DataContext is PatternRule)
                    {
                        PatternRule iRule = iFocused.DataContext as PatternRule;
                        CurrentEditMode = EditMode.AddPattern;
                        int iIndex = iEditor.Items.IndexOf(iRule);
                        PatternRule iRes = EditorsHelper.MakePatternRule(iEditor, iIndex + 1);
                        Debug.Assert(iRes != null);
                        iRes.NeuronInfo.DisplayTitle = TopicsDictionary.GetUniqueRuleName(
                            "new rule ", 
                            iEditor.Items.Cluster);
                        InputPattern iNew = EditorsHelper.AddNewTextPattern(iRes.TextPatterns, string.Empty);
                        iNew.IsSelected = true;
                    }
                    else
                    {
                        ParsableTextPatternBase iItem = iFocused.DataContext as ParsableTextPatternBase;
                        CurrentEditMode = iItem.RequiredEditMode;
                        ParsableTextPatternBase iNew = iItem.Insert(1);
                        if (iNew != null)
                        {
                            iNew.IsSelected = true;
                            iItem.IsSelected = false;

                                // we deactivate the previous selection cause  the ctrl key is pressed during  the insert operation, causing  an 'add' in the selected prop, which we don't want.
                        }
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion
    }
}