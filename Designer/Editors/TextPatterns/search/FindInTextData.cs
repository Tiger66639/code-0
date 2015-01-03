// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindInTextData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data used to search for text text patterns Warning: when
//   the search is not finished, and the object needs to be unloaded, 'Cancel'
//   the search first, otherwise you get a memory leak (the Process object
//   gets added to SearchResults.Searches).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using JaStDev.Data;
using JaStDev.HAB.Designer.Search;

namespace JaStDev.HAB.Designer.Dialogs
{
    /// <summary>
    ///     Contains all the data used to search for text text patterns Warning: when
    ///     the search is not finished, and the object needs to be unloaded, 'Cancel'
    ///     the search first, otherwise you get a memory leak (the Process object
    ///     gets added to SearchResults.Searches).
    /// </summary>
    public class FindInTextData : ObservableObject, ISearchDataProvider
    {
        #region ctor/~

        /// <summary>Initializes a new instance of the <see cref="FindInTextData"/> class.</summary>
        /// <param name="scope">The scope.</param>
        public FindInTextData(TextPatternEditor scope)
        {
            fRequestScope = scope;
        }

        #endregion

        #region itnernal types

        /// <summary>The search level.</summary>
        private enum SearchLevel
        {
            /// <summary>The topic filters.</summary>
            TopicFilters, 

            /// <summary>The input.</summary>
            input, 

            /// <summary>The to cal.</summary>
            toCal, 

            /// <summary>The to eval.</summary>
            toEval, 

            /// <summary>The condition.</summary>
            condition, 

            /// <summary>The output.</summary>
            output, 

            /// <summary>The invalid.</summary>
            invalid, 

            /// <summary>The do pattern.</summary>
            doPattern, 

            /// <summary>The question.</summary>
            question
        }

        /// <summary>
        ///     keeps track of the current position: which editor, which rule, which
        ///     list (input, output or invalid) and which index in the list.
        /// </summary>
        private class EditorCursor
        {
            /// <summary>The f editor.</summary>
            private TextPatternEditor fEditor;

            /// <summary>
            ///     determins if the current rule was loaded or not, so we know if it
            ///     needs to be unloaded again when done or not.
            /// </summary>
            public bool RuleIsLoaded;

            /// <summary>Gets or sets the editor.</summary>
            public TextPatternEditor Editor
            {
                get
                {
                    return fEditor;
                }

                set
                {
                    fEditor = value;
                    ResultSet = null;
                }
            }

            /// <summary>
            ///     Gets or sets the result curren result set to use. When the editor
            ///     gets changed, this is reset.
            /// </summary>
            /// <value>
            ///     The result set.
            /// </value>
            public DisplayPathSet ResultSet { get; set; }

            /// <summary>
            ///     indicates the current level we are processing: input, output,
            ///     invalid, condition, do. This is for when we need to continue search
            ///     from previous one.
            /// </summary>
            public SearchLevel CurrentLevel { get; set; }

            /// <summary>
            ///     Gets or sets the index of the rule currently being processed.
            /// </summary>
            /// <value>
            ///     The index of the rule.
            /// </value>
            public int RuleIndex { get; set; }

            /// <summary>
            ///     Gets or sets the index of the input pattern being processed.
            /// </summary>
            /// <value>
            ///     The index of the input.
            /// </value>
            public int InputIndex { get; set; }

            /// <summary>
            ///     gets/sets the index of the <see cref="ResponsesForGroup" /> that is
            ///     currently being processed.
            /// </summary>
            public int ResponsesForIndex { get; set; }

            /// <summary>
            ///     Gets or sets the index of the conditional being processed. When
            ///     bigger then the list of conditionals, we use the root out/do list.
            /// </summary>
            /// <value>
            ///     The index of the conditional.
            /// </value>
            public int ConditionalIndex { get; set; }

            /// <summary>
            ///     Gets or sets the index of the output currently being processed.
            /// </summary>
            /// <value>
            ///     The index of the output.
            /// </value>
            public int OutputIndex { get; set; }

            /// <summary>
            ///     Gets or sets the index of the invalid currently being processed.
            /// </summary>
            /// <value>
            ///     The index of the invalid.
            /// </value>
            public int InvalidIndex { get; set; }

            /// <summary>
            ///     Gets or sets the position within the text string. This is set when
            ///     there was a match found and we need to continue searching in the
            ///     same string.
            /// </summary>
            /// <value>
            ///     The text pos.
            /// </value>
            public int TextPos { get; set; }

            /// <summary>
            ///     Gets or sets the index of the current question to process.
            /// </summary>
            /// <value>
            ///     The index of the question.
            /// </value>
            public int QuestionIndex { get; set; }

            /// <summary>Gets or sets the topic filter index.</summary>
            public int TopicFilterIndex { get; set; }

            /// <summary>
            ///     Gets or sets the selection range that we can use for the backing
            ///     TextPatternBase, so we can show the selection on the UI.
            /// </summary>
            /// <value>
            ///     The selection.
            /// </value>
            public SelectionRange Selection { get; set; }

            /// <summary>
            ///     Resets the cursor back to 0
            /// </summary>
            internal void Reset()
            {
                if (Editor.Items != null && Editor.Items.Count > RuleIndex)
                {
                    Editor.Items[RuleIndex].IsLoaded = RuleIsLoaded;
                }

                Editor = null;
                CurrentLevel = SearchLevel.TopicFilters;
                TopicFilterIndex = 0;
                RuleIndex = 0;
                InputIndex = 0;
                OutputIndex = 0;
                InvalidIndex = 0;
                TextPos = 0;
                Selection = null;
                ConditionalIndex = 0;
                QuestionIndex = 0;
                ResponsesForIndex = 0;
            }

            /// <summary>tries to find the first item in the currently assigned editor. If
            ///     there is non, <see langword="false"/> is returned.</summary>
            /// <returns>The <see cref="bool"/>.</returns>
            internal bool GotoFirst()
            {
                while (Editor.TopicFilters.Count > TopicFilterIndex
                       && string.IsNullOrEmpty(Editor.TopicFilters[TopicFilterIndex].Expression) == true)
                {
                    TopicFilterIndex++;
                }

                if (Editor.TopicFilters.Count > TopicFilterIndex)
                {
                    return true; // we already are at the correct location, so simply return that we have a start.
                }
                else
                {
                    CurrentLevel = SearchLevel.input; // need to update the level.
                }

                while (Editor.Items.Count > RuleIndex)
                {
                    RuleIsLoaded = Editor.Items[RuleIndex].IsLoaded;
                    Editor.Items[RuleIndex].IsLoaded = true; // make certain that the data is loaded.
                    if (Editor.Items[RuleIndex].IsEmpty == true)
                    {
                        Editor.Items[RuleIndex].IsLoaded = RuleIsLoaded;
                        RuleIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Editor.Items.Count > RuleIndex)
                {
                    GotoFirstInRule();
                    return true;
                }

                if (Editor.Questions.Count > 0)
                {
                    RuleIndex = Editor.Items.Count + 1; // need to make certain that we always go to questions page.
                    CurrentLevel = SearchLevel.question;

                        // if we get here, there was nothing in the first page (statements)
                    while (Editor.Questions.Count > QuestionIndex && Editor.Questions[QuestionIndex].IsEmpty == true)
                    {
                        QuestionIndex++;
                    }

                    if (Editor.Questions.Count < QuestionIndex)
                    {
                        GotoFirstInConditional(Editor.Questions[QuestionIndex]);
                        return true;
                    }
                }

                return false;
            }

            /// <summary>The goto first in rule.</summary>
            private void GotoFirstInRule()
            {
                if (Editor.Items[RuleIndex].TextPatterns.Count == 0)
                {
                    if (Editor.Items[RuleIndex].ToEval != null)
                    {
                        CurrentLevel = SearchLevel.toEval;
                    }
                    else if (Editor.Items[RuleIndex].ToCal != null)
                    {
                        CurrentLevel = SearchLevel.toCal;
                    }
                    else
                    {
                        PatternRule iRule = Editor.Items[RuleIndex];
                        while (iRule.ResponsesFor.Count > ResponsesForIndex
                               && iRule.ResponsesFor[ResponsesForIndex].IsEmpty == true)
                        {
                            ResponsesForIndex++;
                        }

                        if (Editor.Items[RuleIndex].ResponsesFor.Count < ResponsesForIndex)
                        {
                            ResponsesForGroup iGrp = Editor.Items[RuleIndex].ResponsesFor[ResponsesForIndex];
                            while (iGrp.Conditionals.Count > ConditionalIndex
                                   && iGrp.Conditionals[ConditionalIndex].IsEmpty == true)
                            {
                                ConditionalIndex++;
                            }

                            if (iGrp.Conditionals[ConditionalIndex].Outputs.Count > 0)
                            {
                                CurrentLevel = SearchLevel.output;
                            }
                            else
                            {
                                CurrentLevel = SearchLevel.doPattern;
                            }
                        }
                        else
                        {
                            while (iRule.Conditionals.Count > ConditionalIndex
                                   && iRule.Conditionals[ConditionalIndex].IsEmpty == true)
                            {
                                ConditionalIndex++;
                            }

                            if (iRule.Conditionals.Count < ConditionalIndex)
                            {
                                GotoFirstInConditional(iRule.Conditionals[ConditionalIndex]);
                            }
                            else if (iRule.Outputs.Count > 0)
                            {
                                CurrentLevel = SearchLevel.output;
                            }
                            else
                            {
                                CurrentLevel = SearchLevel.doPattern;
                            }
                        }
                    }
                }
            }

            /// <summary>The goto first in conditional.</summary>
            /// <param name="iCond">The i cond.</param>
            private void GotoFirstInConditional(PatternRuleOutput iCond)
            {
                if (iCond.Condition != null)
                {
                    CurrentLevel = SearchLevel.condition;
                }
                else if (iCond.Outputs.Count > 0)
                {
                    CurrentLevel = SearchLevel.output;
                }
                else
                {
                    CurrentLevel = SearchLevel.doPattern;
                }
            }
        }

        #endregion

        #region fields

        /// <summary>The f text to search.</summary>
        private string fTextToSearch;

        /// <summary>The f request scope.</summary>
        private TextPatternEditor fRequestScope;

        /// <summary>The f process.</summary>
        private SearcherProcess fProcess;

        /// <summary>The f current.</summary>
        private int fCurrent; // for when we are searching the entire db or the index of the open document being checked.

        /// <summary>The f cursor.</summary>
        private EditorCursor fCursor = new EditorCursor(); // for keeping track within the current editor.

        /// <summary>The f result folder.</summary>
        private DisplayPathSetFolder fResultFolder;

                                     // stores the root result folder where we add DisplayPathSets to, 1 for each editor that is included in the result.

        /// <summary>The f do find all.</summary>
        private bool fDoFindAll = false;

                     // if the user requested a find all, we set this flag, so we can generate the item that was found differently.

        /// <summary>The f regex.</summary>
        private Regex fRegex;

                      // when set, we want to use a regular expression to evaluate. We cache the regex, this is a lot faster

        /// <summary>The f advance on continue.</summary>
        private bool fAdvanceOnContinue = false;

                     // sometimes we need to advance the cursor when we do a continue, sometimes we don't, depending on the last found item. This switch allows us to pass it along across the 'continue' border.

        /// <summary>The f replace text.</summary>
        private string fReplaceText = string.Empty;

        /// <summary>The f do replace.</summary>
        private bool fDoReplace = false;

        /// <summary>The f all editors.</summary>
        private List<TextPatternEditor> fAllEditors;

                                        // we keep a local list of the editors when searching the entire list, cause this is a generated list adn we need to be able to continue the search from within the list, which would not be possible with a generated list.
        #endregion

        #region prop

        #region TextToSearch

        /// <summary>
        ///     Gets/sets the text that needs to be searched.
        /// </summary>
        public string TextToSearch
        {
            get
            {
                return fTextToSearch;
            }

            set
            {
                fTextToSearch = value;
                if (fRegex != null)
                {
                    // reload the regex if the text changes, cause this gets cached and would otherwise work on invalid data
                    fRegex = new Regex(value);
                }

                OnPropertyChanged("TextToSearch");
            }
        }

        #endregion

        #region ReplaceText

        /// <summary>
        ///     Gets/sets the text that needs to replace the found parts.
        /// </summary>
        public string ReplaceText
        {
            get
            {
                return fReplaceText;
            }

            set
            {
                fReplaceText = value;
                OnPropertyChanged("ReplaceText");
            }
        }

        #endregion

        #region MatchCase

        /// <summary>
        ///     Gets/sets if the case should be matched or not.
        /// </summary>
        public bool MatchCase
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchMatchCase;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchMatchCase = value;
                OnPropertyChanged("MatchCase");
            }
        }

        #endregion

        #region AsRegEx

        /// <summary>
        ///     Gets/sets the value that indicates if the string compare needs to be
        ///     done using a regular expression or a simple compare.
        /// </summary>
        public bool AsRegEx
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchAsRegEx;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchAsRegEx = value;
                OnPropertyChanged("AsRegEx");
            }
        }

        #endregion

        #region SearchScope

        /// <summary>
        ///     Gets/sets the scope to search in: the current editor, all open
        ///     editors, the entire project
        /// </summary>
        /// <remarks>
        ///     0=Current editor 1=All open editors 2=entire project.
        /// </remarks>
        public int SearchScope
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchScope;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchScope = value;
                OnPropertyChanged("SearchScope");
            }
        }

        #endregion

        #region RequestScope

        /// <summary>
        ///     Gets the object that requested the search action. This is used to
        ///     check if it's valid for doing a findNext from a specific editor, given
        ///     the current search scope.
        /// </summary>
        public TextPatternEditor RequestScope
        {
            get
            {
                return fRequestScope;
            }
        }

        #endregion

        #region IncludeNoReply

        /// <summary>
        ///     Gets/sets the value that indicates wether to include 'When not
        ///     replied' (invalid responses) in the search.
        /// </summary>
        public bool IncludeNoReply
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeNoReply;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeNoReply = value;
                OnPropertyChanged("IncludeNoReply");
            }
        }

        #endregion

        #region IncludeOutput

        /// <summary>
        ///     Gets/sets the value that indicates wether to include the 'output'
        ///     statements.
        /// </summary>
        public bool IncludeOutput
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeOutput;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeOutput = value;
                OnPropertyChanged("IncludeOutput");
            }
        }

        #endregion

        #region Includeinput

        /// <summary>
        ///     Gets/sets the value that indicates wehter to include the 'input'
        ///     statements.
        /// </summary>
        public bool Includeinput
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeinput;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeinput = value;
                OnPropertyChanged("Includeinput");
            }
        }

        #endregion

        #region IncludeDo

        /// <summary>
        ///     Gets/sets the value that indicates of the 'do patterns' should be
        ///     included in the search.
        /// </summary>
        public bool IncludeDo
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeDo;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeDo = value;
                OnPropertyChanged("IncludeDo");
            }
        }

        #endregion

        #region IncludeCondition

        /// <summary>
        ///     Gets the value that indicates of the 'condition patterns' should be
        ///     included in the search.
        /// </summary>
        public bool IncludeCondition
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeCondition;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeCondition = value;
                OnPropertyChanged("IncludeCondition");
            }
        }

        #endregion

        #region IncludeQuestions

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public bool IncludeQuestions
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeQuestions;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeQuestions = value;
                OnPropertyChanged("IncludeQuestions");
            }
        }

        #endregion

        #region IncludeTopicFilters

        /// <summary>
        ///     Gets/sets the value that indicates if topic filters need to be
        ///     included in the search.
        /// </summary>
        public bool IncludeTopicFilters
        {
            get
            {
                return Properties.Settings.Default.TextPatternSearchIncludeTopicFilters;
            }

            set
            {
                Properties.Settings.Default.TextPatternSearchIncludeTopicFilters = value;
                OnPropertyChanged("IncludeTopicFilters");
            }
        }

        #endregion

        #region Process

        /// <summary>
        ///     Gets/sets the process that performs the actual search, async.
        /// </summary>
        public SearcherProcess Process
        {
            get
            {
                return fProcess;
            }

            set
            {
                fProcess = value;
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>
        ///     Cancels the search operation. This is important: it needs to be called
        ///     if the object needs to be unloaded before the search is complete.
        /// </summary>
        public void Cancel()
        {
            if (Process != null)
            {
                Process.Cancel();
            }
        }

        /// <summary>
        ///     Finds the next occurrance.
        /// </summary>
        public void FindNext()
        {
            if (string.IsNullOrEmpty(TextToSearch) == false)
            {
                fDoReplace = false;
                if (Process != null)
                {
                    CalculateTotalCount(); // recalculate the length, cause something in the data might have changed.
                    Process.ContinueUlong();
                }
                else
                {
                    InternalStart();
                }
            }
        }

        /// <summary>
        ///     If the current selection meets the search criteria, this is replaced,
        ///     otherwise we first do a 'FindNext' before doi
        /// </summary>
        public void Replace()
        {
            TextPatternEditor iFocusedEdit = null;
            if (BrainData.Current.ActiveDocument != null)
            {
                iFocusedEdit = BrainData.Current.ActiveDocument as TextPatternEditor;
            }

            if (iFocusedEdit != null)
            {
                TextPatternBase iSelected = iFocusedEdit.SelectedItem as TextPatternBase;
                if (iSelected != null)
                {
                    string iSub = iSelected.Expression.Substring(
                        iSelected.Selectionrange.Start, 
                        iSelected.Selectionrange.Length);
                    if (CompareText(iSub, false) == false)
                    {
                        fDoReplace = false;

                            // make certain we don't replace yet, first present the result to the user, so he can choose.
                        FindNext();
                    }
                    else
                    {
                        iSelected.Expression = iSelected.Expression.Substring(0, iSelected.Selectionrange.Start)
                                               + ReplaceText
                                               + iSelected.Expression.Substring(
                                                   iSelected.Selectionrange.Start + iSelected.Selectionrange.Length);
                        fCursor.TextPos = fCursor.TextPos - fTextToSearch.Length + ReplaceText.Length;
                    }
                }
            }

            fDoReplace = false;

                // make certain we don't replace yet, first present the result to the user, so he can choose.
            FindNext();
        }

        /// <summary>
        ///     Finds all occurances of the specified item.
        /// </summary>
        public void FindAll()
        {
            if (string.IsNullOrEmpty(TextToSearch) == false)
            {
                fDoFindAll = true;
                fDoReplace = false;
                InternalStart();
            }
        }

        /// <summary>The replace all.</summary>
        internal void ReplaceAll()
        {
            if (string.IsNullOrEmpty(TextToSearch) == false)
            {
                fDoFindAll = true;
                fDoReplace = true;
                WindowMain.UndoStore.BeginUndoGroup();

                    // this operation does a lot of changes at once, but in a different thread, with some complicated query structures, so best way to make certain we have 1 undo group, is by starting it now and stopping it, when the search is done.
                InternalStart();
            }
        }

        /// <summary>
        ///     Starts the earch process.
        /// </summary>
        private void InternalStart()
        {
            Process = ProcessTracker.Default.InitSearch(Searchid, this);
            CalculateTotalCount();
            fCurrent = 0;
            if (MatchCase == false)
            {
                // if we do case insensitive, adjust tolower now, so we only have to do it 1 time.
                fTextToSearch = fTextToSearch.ToLower();
            }

            if (AsRegEx == true)
            {
                // reload the regex pattern cause the pattern itself has changed.
                fRegex = new Regex(fTextToSearch);
            }

            if (fDoFindAll == false)
            {
                Process.StartUlong();
            }
            else
            {
                Process.FindAllUlong();
            }
        }

        /// <summary>
        ///     Calculates the total count of the items that need to be searched.
        /// </summary>
        private void CalculateTotalCount()
        {
            if (SearchScope == 2)
            {
                // that't the entire project
                fAllEditors = BrainData.Current.Editors.AllTextPatternEditorsClosed().ToList();

                    // don't need to open them when building the count.
                Process.TotalCount = fAllEditors.Count;
            }
            else if (SearchScope == 1)
            {
                // open documents.
                Process.TotalCount = BrainData.Current.OpenDocuments.Count;
            }
            else if (SearchScope == 0)
            {
                Process.TotalCount = RequestScope.Items.Count;
            }
        }

        /// <summary>Callback for searching the text found at the specified id.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool Searchid(ulong id)
        {
            bool iRes = false;
            TextNeuron iText = Brain.Current[id] as TextNeuron;
            if (iText != null && string.IsNullOrEmpty(iText.Text) == false)
            {
                string iVal = iText.Text;
                if (fCursor.TextPos > 0)
                {
                    iVal = iVal.Substring(fCursor.TextPos);

                        // we take the substring starting at the end of the last match, so we can check for multiple occurencies within the same string.
                }

                iRes = CompareText(iVal);
            }

            if (iRes == false)
            {
                fCursor.TextPos = 0;
                AdvanceCursor();
            }
            else
            {
                fCursor.TextPos = fCursor.Selection.Start + fCursor.Selection.Length;
                if (fCursor.TextPos + TextToSearch.Length > iText.Text.Length)
                {
                    // if we are at the end of the text, advance to the next pattern.
                    fAdvanceOnContinue = true;
                }
            }

            return iRes;
        }

        /// <summary>The compare text.</summary>
        /// <param name="iVal">The i val.</param>
        /// <param name="setRange">The set range.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareText(string iVal, bool setRange = true)
        {
            bool iRes = false;
            if (MatchCase == false)
            {
                iVal = iVal.ToLower();
            }

            if (AsRegEx == false)
            {
                iRes = iVal.Contains(TextToSearch);
                if (iRes == true && setRange == true)
                {
                    fCursor.Selection = new SelectionRange();
                    fCursor.Selection.Start = fCursor.TextPos + iVal.IndexOf(TextToSearch);
                    fCursor.Selection.Length = TextToSearch.Length;
                }
            }
            else
            {
                Match iMatch = fRegex.Match(iVal);
                iRes = iMatch.Success;
                if (iRes == true && setRange == true)
                {
                    fCursor.Selection = new SelectionRange()
                                            {
                                                Start = fCursor.TextPos + iMatch.Index, 
                                                Length = iMatch.Length
                                            };
                }
            }

            return iRes;
        }

        /// <summary>Gets the data, <see langword="internal"/> version.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private IEnumerable<ulong> InternalGetData()
        {
            switch (SearchScope)
            {
                case 0:
                    return GetDataFromCurrentScope();
                case 1:
                    return GetDataFromOpenDocs();
                case 2:
                    return GetDataFromAll();
                default:
                    throw new InvalidOperationException("Unknown searchs cope");
            }
        }

        /// <summary>Gets the <see cref="TextPatternBase"/> at the current position in the
        ///     cursor.</summary>
        /// <param name="fCursor">The f cursor.</param>
        /// <returns>The <see cref="TextPatternBase"/>.</returns>
        private TextPatternBase GetCurrent()
        {
            if (fCursor.TopicFilterIndex < fCursor.Editor.TopicFilters.Count)
            {
                return fCursor.Editor.TopicFilters[fCursor.TopicFilterIndex];
            }
            else if (fCursor.RuleIndex < fCursor.Editor.Items.Count)
            {
                PatternRule iRule = fCursor.Editor.Items[fCursor.RuleIndex];
                if (fCursor.CurrentLevel == SearchLevel.input)
                {
                    // when we continue from previous search, CurrentLevel will be bigger, if appropriate.
                    return iRule.TextPatterns[fCursor.InputIndex];
                }
                else if (fCursor.CurrentLevel == SearchLevel.toEval)
                {
                    return iRule.ToEval;
                }
                else if (fCursor.CurrentLevel == SearchLevel.toCal)
                {
                    return iRule.ToCal;
                }
                else
                {
                    IConditionalOutputsCollectionOwner iOwner = null;
                    if (fCursor.ResponsesForIndex < iRule.ResponsesFor.Count)
                    {
                        iOwner = iRule.ResponsesFor[fCursor.ResponsesForIndex];
                    }
                    else if (fCursor.ConditionalIndex < iRule.Conditionals.Count)
                    {
                        iOwner = iRule;
                    }

                    if (fCursor.CurrentLevel == SearchLevel.output)
                    {
                        if (iOwner == null)
                        {
                            return iRule.Outputs[fCursor.OutputIndex];
                        }
                        else
                        {
                            return iOwner.Conditionals[fCursor.ConditionalIndex].Outputs[fCursor.OutputIndex];
                        }
                    }
                    else if (fCursor.CurrentLevel == SearchLevel.condition)
                    {
                        if (iOwner == null)
                        {
                            return iRule.Conditionals[fCursor.ConditionalIndex].Condition;
                        }
                        else
                        {
                            return iOwner.Conditionals[fCursor.ConditionalIndex].Condition;
                        }
                    }
                    else if (fCursor.CurrentLevel == SearchLevel.doPattern)
                    {
                        if (iOwner == null)
                        {
                            return iRule.Do;
                        }
                        else
                        {
                            return iOwner.Conditionals[fCursor.ConditionalIndex].Do;
                        }
                    }
                    else
                    {
                        if (iOwner == null)
                        {
                            OutputPattern iOut = iRule.Outputs[fCursor.OutputIndex];
                            if (fCursor.InvalidIndex < iOut.InvalidResponses.Count)
                            {
                                return iOut.InvalidResponses[fCursor.InvalidIndex];
                            }
                        }
                        else
                        {
                            OutputPattern iOut =
                                iOwner.Conditionals[fCursor.ConditionalIndex].Outputs[fCursor.OutputIndex];
                            if (fCursor.InvalidIndex < iOut.InvalidResponses.Count)
                            {
                                return iOut.InvalidResponses[fCursor.InvalidIndex];
                            }
                        }
                    }
                }
            }
            else if (fCursor.Editor.Questions != null && fCursor.QuestionIndex < fCursor.Editor.Questions.Count)
            {
                PatternRuleOutput iOut = fCursor.Editor.Questions[fCursor.QuestionIndex];
                if (fCursor.CurrentLevel == SearchLevel.question)
                {
                    return iOut.Condition;
                }
                else if (fCursor.CurrentLevel == SearchLevel.output)
                {
                    return iOut.Outputs[fCursor.OutputIndex];
                }
                else if (fCursor.CurrentLevel == SearchLevel.doPattern)
                {
                    return iOut.Do;
                }
                else if (fCursor.CurrentLevel == SearchLevel.invalid)
                {
                    return iOut.Outputs[fCursor.OutputIndex].InvalidResponses[fCursor.InvalidIndex];
                }
            }

            return null;
        }

        /// <summary>The advance cursor.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void AdvanceCursor()
        {
            if (fCursor.TopicFilterIndex < fCursor.Editor.TopicFilters.Count - 1)
            {
                // we start with topicFilters. If there are still items left, go to the next in the list, otherwise we let the next rule to decide where to go to.
                fCursor.TopicFilterIndex++;
            }
            else if (fCursor.RuleIndex < fCursor.Editor.Items.Count)
            {
                PatternRule iRule = fCursor.Editor.Items[fCursor.RuleIndex];
                if (fCursor.CurrentLevel == SearchLevel.input)
                {
                    // when we continue from previous search, CurrentLevel will be bigger, if appropriate.
                    AdvanceCursorFromInput(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.toEval)
                {
                    AdvanceCursorFromToEval(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.toCal)
                {
                    AdvanceCursorFromToCal(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.condition)
                {
                    AdvanceFromCondition(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.output)
                {
                    AdvanceFromOutput(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.doPattern)
                {
                    AdvanceFromDo(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.invalid)
                {
                    AdvanceFromInvalid(iRule);
                }
                else if (fCursor.CurrentLevel == SearchLevel.TopicFilters)
                {
                    fCursor.TopicFilterIndex++; // need to indicate that the last topicfilter has been consumed.
                    fCursor.RuleIndex = -1;

                        // if we are in a topicfilter, we are at the start of topic, so we need go to the first rule. Easiest to do that is to go to the next rule, but to do that, we first need to set the curent pos to -1, so we will try rule 0 next
                    GotoNextRule();
                }
                else
                {
                    throw new InvalidOperationException("Internal error: Unknonw current level in search!");
                }
            }
            else if (fCursor.Editor.Questions != null && fCursor.QuestionIndex < fCursor.Editor.Questions.Count)
            {
                PatternRuleOutput iQuestion = fCursor.Editor.Questions[fCursor.QuestionIndex];
                if (fCursor.CurrentLevel == SearchLevel.output)
                {
                    AdvanceFromQuestionOutput(iQuestion);
                }
                else if (fCursor.CurrentLevel == SearchLevel.question)
                {
                    AdvanceFromQuestion(iQuestion);
                }
                else if (fCursor.CurrentLevel == SearchLevel.invalid)
                {
                    AdvanceFromQuestionInvalid(iQuestion);
                }
                else if (fCursor.CurrentLevel == SearchLevel.doPattern)
                {
                    AdvanceFromQuestionDo(iQuestion);
                }
                else
                {
                    throw new InvalidOperationException("Internal error: Unknonw current level in search!");
                }
            }
        }

        /// <summary>The advance from question do.</summary>
        /// <param name="pos">The pos.</param>
        private void AdvanceFromQuestionDo(PatternRuleOutput pos)
        {
            fCursor.QuestionIndex++;
            fCursor.CurrentLevel = SearchLevel.question;
        }

        /// <summary>The advance from question invalid.</summary>
        /// <param name="pos">The pos.</param>
        private void AdvanceFromQuestionInvalid(PatternRuleOutput pos)
        {
            fCursor.InvalidIndex++;
            OutputPattern iOut = pos.Outputs[fCursor.OutputIndex];
            if (iOut.InvalidResponses.Count <= fCursor.InvalidIndex)
            {
                fCursor.InvalidIndex = 0;
                fCursor.OutputIndex++;
                if (fCursor.OutputIndex < pos.Outputs.Count)
                {
                    fCursor.CurrentLevel = SearchLevel.output;
                }
                else
                {
                    fCursor.OutputIndex = 0;
                    if (pos.Do != null)
                    {
                        fCursor.CurrentLevel = SearchLevel.doPattern;
                    }
                    else
                    {
                        fCursor.QuestionIndex++;
                        fCursor.CurrentLevel = SearchLevel.question;
                    }
                }
            }
        }

        /// <summary>Advances from question (conditional), to the first output.</summary>
        /// <param name="pos">The pos.</param>
        private void AdvanceFromQuestion(PatternRuleOutput pos)
        {
            if (pos.Outputs.Count > 0)
            {
                fCursor.CurrentLevel = SearchLevel.output;
                fCursor.OutputIndex = 0;
            }
            else
            {
                fCursor.QuestionIndex++;
            }
        }

        /// <summary>The advance from question output.</summary>
        /// <param name="pos">The pos.</param>
        private void AdvanceFromQuestionOutput(PatternRuleOutput pos)
        {
            OutputPattern iOut = pos.Outputs[fCursor.OutputIndex];
            if (iOut.InvalidResponses.Count > 0)
            {
                fCursor.InvalidIndex = 0;
                fCursor.CurrentLevel = SearchLevel.invalid;
            }
            else
            {
                fCursor.InvalidIndex = 0;
                fCursor.OutputIndex++;
                if (pos.Outputs.Count <= fCursor.OutputIndex)
                {
                    fCursor.OutputIndex = 0;
                    if (pos.Do != null)
                    {
                        fCursor.CurrentLevel = SearchLevel.doPattern;
                    }
                    else
                    {
                        fCursor.QuestionIndex++;
                        fCursor.CurrentLevel = SearchLevel.question;
                    }
                }
            }
        }

        /// <summary>The advance from invalid.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceFromInvalid(PatternRule rule)
        {
            IConditionalOutputsCollectionOwner iOwner = null;
            if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
            {
                iOwner = rule.ResponsesFor[fCursor.ResponsesForIndex];
            }
            else if (fCursor.ConditionalIndex < rule.Conditionals.Count)
            {
                iOwner = rule;
            }

            if (iOwner != null)
            {
                PatternRuleOutput iOutSet = iOwner.Conditionals[fCursor.ConditionalIndex];
                if (fCursor.OutputIndex < iOutSet.Outputs.Count)
                {
                    OutputPattern iOut = iOutSet.Outputs[fCursor.OutputIndex];
                    if (fCursor.InvalidIndex < iOut.InvalidResponses.Count - 1)
                    {
                        // do -1, cause we are going to advance, so we need to know if there still is one after the current.
                        fCursor.InvalidIndex++;
                    }
                    else if (fCursor.OutputIndex < iOutSet.Outputs.Count - 1)
                    {
                        // do -1, cause we are going to advance, so we need to know if there still is one after the current.
                        fCursor.CurrentLevel = SearchLevel.output;
                        fCursor.OutputIndex++;
                        fCursor.InvalidIndex = 0;
                    }
                    else if (iOutSet.HasDo == true)
                    {
                        fCursor.CurrentLevel = SearchLevel.doPattern;
                    }
                    else
                    {
                        GotoNextConditionalOrRule(rule);
                    }
                }
                else if (iOutSet.HasDo == true)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextConditionalOrRule(rule);
                }
            }
            else if (fCursor.OutputIndex < rule.Outputs.Count)
            {
                OutputPattern iOut = rule.Outputs[fCursor.OutputIndex];
                if (fCursor.InvalidIndex < iOut.InvalidResponses.Count - 1)
                {
                    fCursor.InvalidIndex++;
                }
                else if (fCursor.OutputIndex < rule.Outputs.Count - 1)
                {
                    fCursor.OutputIndex++;
                    fCursor.CurrentLevel = SearchLevel.output;
                }
                else if (rule.Do != null)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextRule();
                }
            }
            else if (rule.Do != null)
            {
                fCursor.CurrentLevel = SearchLevel.doPattern;
            }
            else
            {
                GotoNextRule();
            }
        }

        /// <summary>The advance from do.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceFromDo(PatternRule rule)
        {
            IConditionalOutputsCollectionOwner iOwner = null;
            if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
            {
                iOwner = rule.ResponsesFor[fCursor.ResponsesForIndex];
            }
            else if (fCursor.ConditionalIndex < rule.Conditionals.Count)
            {
                iOwner = rule;
            }

            if (iOwner != null)
            {
                PatternRuleOutput iOutSet = iOwner.Conditionals[fCursor.ConditionalIndex];
                GotoNextConditionalOrRule(rule);
            }
            else
            {
                GotoNextRule();
            }
        }

        /// <summary>The advance from output.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceFromOutput(PatternRule rule)
        {
            if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
            {
                ResponsesForGroup iGrp = rule.ResponsesFor[fCursor.ResponsesForIndex];
                if (fCursor.ConditionalIndex < iGrp.Conditionals.Count)
                {
                    PatternRuleOutput iOutSet = iGrp.Conditionals[fCursor.ConditionalIndex];
                    if (fCursor.OutputIndex < iOutSet.Outputs.Count)
                    {
                        AdvanceFromOutputItem(rule, iOutSet.Outputs[fCursor.OutputIndex], iOutSet);
                    }
                    else if (iOutSet.HasDo == true)
                    {
                        fCursor.CurrentLevel = SearchLevel.doPattern;
                    }
                    else
                    {
                        GotoNextConditionalOrRule(rule);
                    }
                }
            }
            else if (fCursor.ConditionalIndex < rule.Conditionals.Count)
            {
                PatternRuleOutput iOutSet = rule.Conditionals[fCursor.ConditionalIndex];
                if (fCursor.OutputIndex < iOutSet.Outputs.Count)
                {
                    AdvanceFromOutputItem(rule, iOutSet.Outputs[fCursor.OutputIndex], iOutSet);
                }
                else if (iOutSet.HasDo == true)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextConditionalOrRule(rule);
                }
            }
            else if (fCursor.OutputIndex < rule.Outputs.Count)
            {
                OutputPattern iOut = rule.Outputs[fCursor.OutputIndex];
                if (iOut.InvalidResponses.Count > 0)
                {
                    fCursor.InvalidIndex = 0; // this doesn't get reset properly otherwise.
                    fCursor.CurrentLevel = SearchLevel.invalid;
                }
                else if (fCursor.OutputIndex < rule.Outputs.Count - 1)
                {
                    fCursor.OutputIndex++;
                }
                else if (rule.Do != null)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextRule();
                }
            }
            else if (rule.Do != null)
            {
                fCursor.CurrentLevel = SearchLevel.doPattern;
            }
            else
            {
                GotoNextRule();
            }
        }

        /// <summary>The advance from output item.</summary>
        /// <param name="rule">The rule.</param>
        /// <param name="iOut">The i out.</param>
        /// <param name="iOutSet">The i out set.</param>
        private void AdvanceFromOutputItem(PatternRule rule, OutputPattern iOut, PatternRuleOutput iOutSet)
        {
            if (iOut.InvalidResponses.Count > 0)
            {
                fCursor.InvalidIndex = 0;

                    // jsut to make certain, it doesn't happen for outputs directly underneath the rule
                fCursor.CurrentLevel = SearchLevel.invalid;
            }
            else if (fCursor.OutputIndex < iOutSet.Outputs.Count - 1)
            {
                fCursor.OutputIndex++;
            }
            else if (iOutSet.HasDo == true)
            {
                fCursor.CurrentLevel = SearchLevel.doPattern;
            }
            else
            {
                GotoNextConditionalOrRule(rule);
            }
        }

        /// <summary>The advance from condition.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceFromCondition(PatternRule rule)
        {
            ConditionalOutputsCollection iCol;
            if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
            {
                ResponsesForGroup iGrp = rule.ResponsesFor[fCursor.ResponsesForIndex];
                iCol = iGrp.Conditionals;
            }
            else
            {
                iCol = rule.Conditionals;
            }

            if (iCol[fCursor.ConditionalIndex].Outputs.Count > 0)
            {
                fCursor.CurrentLevel = SearchLevel.output;
            }
            else if (iCol[fCursor.ConditionalIndex].Do != null)
            {
                fCursor.CurrentLevel = SearchLevel.doPattern;
            }
            else
            {
                GotoNextConditionalOrRule(rule);
            }
        }

        /// <summary>The goto next conditional or rule.</summary>
        /// <param name="rule">The rule.</param>
        private void GotoNextConditionalOrRule(PatternRule rule)
        {
            bool iFound = false;
            fCursor.ConditionalIndex++;
            fCursor.OutputIndex = 0;
            fCursor.InvalidIndex = 0;

            IConditionalOutputsCollectionOwner iOwner;
            if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
            {
                iOwner = rule.ResponsesFor[fCursor.ResponsesForIndex];
            }
            else
            {
                iOwner = rule;
            }

            if (iOwner.Conditionals.Count > fCursor.ConditionalIndex)
            {
                iFound = FindNextConditionLevel(iOwner);
            }

            if (iFound == false)
            {
                // if we haven't foundi t yet, check the root output/do
                fCursor.ResponsesForIndex++;
                while (fCursor.ResponsesForIndex < rule.ResponsesFor.Count
                       && rule.ResponsesFor[fCursor.ResponsesForIndex].IsEmpty == true)
                {
                    fCursor.ResponsesForIndex++;
                }

                if (fCursor.ResponsesForIndex < rule.ResponsesFor.Count)
                {
                    fCursor.ConditionalIndex = 0;
                    iFound = FindNextConditionLevel(rule.ResponsesFor[fCursor.ResponsesForIndex]);
                }

                if (iFound == false)
                {
                    if (rule.Conditionals.Count > fCursor.ConditionalIndex)
                    {
                        iFound = FindNextConditionLevel(rule);
                    }

                    if (iFound == false)
                    {
                        if (rule.Outputs.Count > 0)
                        {
                            fCursor.CurrentLevel = SearchLevel.output;
                        }
                        else if (rule.Do != null)
                        {
                            fCursor.CurrentLevel = SearchLevel.doPattern;
                        }
                        else
                        {
                            GotoNextRule();
                        }
                    }
                }
            }
        }

        /// <summary>The advance cursor from to eval.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceCursorFromToEval(PatternRule rule)
        {
            if (rule.ToCal != null)
            {
                fCursor.CurrentLevel = SearchLevel.toCal;
            }
            else
            {
                while (rule.ResponsesFor != null && rule.ResponsesFor.Count > fCursor.ResponsesForIndex)
                {
                    if (FindNextConditionLevel(rule.ResponsesFor[fCursor.ResponsesForIndex]) == true)
                    {
                        // if we haven't foundi t yet, check the root output/do
                        return;
                    }

                    fCursor.ResponsesForIndex++;
                }

                if (rule.Conditionals.Count > fCursor.ConditionalIndex)
                {
                    if (FindNextConditionLevel(rule) == true)
                    {
                        // if we haven't foundi t yet, check the root output/do
                        return;
                    }

                    fCursor.ConditionalIndex++;
                }

                if (rule.Outputs.Count > 0)
                {
                    fCursor.CurrentLevel = SearchLevel.output;
                }
                else if (rule.Do != null)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextRule();
                }
            }
        }

        /// <summary>The advance cursor from to cal.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceCursorFromToCal(PatternRule rule)
        {
            while (rule.ResponsesFor != null && rule.ResponsesFor.Count > fCursor.ResponsesForIndex)
            {
                if (FindNextConditionLevel(rule.ResponsesFor[fCursor.ResponsesForIndex]) == true)
                {
                    // if we haven't foundi t yet, check the root output/do
                    return;
                }

                fCursor.ResponsesForIndex++;
            }

            if (rule.Conditionals.Count > fCursor.ConditionalIndex)
            {
                if (FindNextConditionLevel(rule) == true)
                {
                    // if we haven't foundi t yet, check the root output/do
                    return;
                }

                fCursor.ConditionalIndex++;
            }

            if (rule.Outputs.Count > 0)
            {
                fCursor.CurrentLevel = SearchLevel.output;
            }
            else if (rule.Do != null)
            {
                fCursor.CurrentLevel = SearchLevel.doPattern;
            }
            else
            {
                GotoNextRule();
            }
        }

        /// <summary>Advances the cursor when it's at an input patttern.</summary>
        /// <param name="rule">The rule.</param>
        private void AdvanceCursorFromInput(PatternRule rule)
        {
            if (fCursor.InputIndex < rule.TextPatterns.Count - 1)
            {
                // do -1, cause we are going to advance, so we need to know if there still is one after the current.
                fCursor.InputIndex++;
            }
            else if (rule.ToEval != null)
            {
                fCursor.CurrentLevel = SearchLevel.toEval;
            }
            else if (rule.ToCal != null)
            {
                fCursor.CurrentLevel = SearchLevel.toCal;
            }
            else
            {
                while (rule.ResponsesFor != null && rule.ResponsesFor.Count > fCursor.ResponsesForIndex)
                {
                    if (FindNextConditionLevel(rule.ResponsesFor[fCursor.ResponsesForIndex]) == true)
                    {
                        // if we haven't foundi t yet, check the root output/do
                        return;
                    }

                    fCursor.ResponsesForIndex++;
                }

                if (rule.Conditionals.Count > fCursor.ConditionalIndex)
                {
                    if (FindNextConditionLevel(rule) == true)
                    {
                        // if we haven't foundi t yet, check the root output/do
                        return;
                    }
                }

                if (rule.Outputs.Count > 0)
                {
                    fCursor.CurrentLevel = SearchLevel.output;
                }
                else if (rule.Do != null)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    GotoNextRule();
                }
            }
        }

        /// <summary>Finds the next condition level (condition, output or do pattern). If
        ///     not found/valid, we return <see langword="false"/></summary>
        /// <param name="rule">The rule.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool FindNextConditionLevel(IConditionalOutputsCollectionOwner rule)
        {
            bool iFound;
            do
            {
                iFound = true;
                if (rule.Conditionals[fCursor.ConditionalIndex].Condition != null)
                {
                    fCursor.CurrentLevel = SearchLevel.condition;
                }
                else if (rule.Conditionals[fCursor.ConditionalIndex].Outputs.Count > 0)
                {
                    fCursor.CurrentLevel = SearchLevel.output;
                }
                else if (rule.Conditionals[fCursor.ConditionalIndex].Do != null)
                {
                    fCursor.CurrentLevel = SearchLevel.doPattern;
                }
                else
                {
                    iFound = false;
                    fCursor.ConditionalIndex++;
                }
            }
            while (iFound == false && fCursor.ConditionalIndex < rule.Conditionals.Count);
            return iFound;
        }

        /// <summary>The goto next rule.</summary>
        private void GotoNextRule()
        {
            if (fCursor.Editor.Items.Count > fCursor.RuleIndex && fCursor.RuleIndex >= 0)
            {
                fCursor.Editor.Items[fCursor.RuleIndex].IsLoaded = fCursor.RuleIsLoaded;
            }

            fCursor.RuleIndex++;
            if (fCursor.Editor.Items.Count > fCursor.RuleIndex)
            {
                fCursor.RuleIsLoaded = fCursor.Editor.Items[fCursor.RuleIndex].IsLoaded;
                fCursor.Editor.Items[fCursor.RuleIndex].IsLoaded = true;

                    // make certain that the rule is loaded. Do this one by one, so we don't overtax mem usage.
            }
            else
            {
                fCursor.RuleIsLoaded = false;
            }

            fCursor.InputIndex = 0;
            fCursor.ResponsesForIndex = 0;
            fCursor.OutputIndex = 0;
            fCursor.ConditionalIndex = 0;
            fCursor.InvalidIndex = 0;
            if (fCursor.Editor.Items.Count > fCursor.RuleIndex)
            {
                fCursor.CurrentLevel = SearchLevel.input;

                    // when all the outputs of a rule have been procesed, go to the next rule.
                PatternRule iRule = fCursor.Editor.Items[fCursor.RuleIndex];
                if (iRule.TextPatterns.Count == 0)
                {
                    AdvanceCursor(); // we need to advance some more if there aren't any text patterns.
                }
            }
            else
            {
                fCursor.CurrentLevel = SearchLevel.question;
            }
        }

        /// <summary>Gets the current item and advances the cursor until an item has been
        ///     found that matches the filter, or null.</summary>
        /// <returns>The <see cref="TextPatternBase"/>.</returns>
        private TextPatternBase GetCurrentFiltered()
        {
            TextPatternBase iRes = GetCurrent();

                // we get the current, don't advance already, cause it could be a match and we want to be able to return the match + the exact pos of the match + find new matches within the string.
            while (iRes != null)
            {
                if (IncludeTopicFilters == true && iRes is TopicFilterPattern)
                {
                    // if topic filters are inlcuded in the search and we currently have a topicfilter pattern, it can be returned.
                    return iRes;
                }

                if (fCursor.RuleIndex < fCursor.Editor.Items.Count)
                {
                    // questions need to be filtered differently
                    if ((Includeinput == false && iRes is InputPattern)
                        || (IncludeOutput == false && iRes is OutputPattern)
                        || (IncludeNoReply == false && iRes is InvalidPatternResponse)
                        || (IncludeCondition == false && iRes is ConditionPattern)
                        || (IncludeDo == false && iRes is DoPattern))
                    {
                        AdvanceCursor();
                        iRes = GetCurrent();
                        continue;
                    }
                    else
                    {
                        return iRes;
                    }
                }
                else if (IncludeQuestions == true)
                {
                    if ((IncludeOutput == false && iRes is OutputPattern)
                        || (IncludeCondition == false && iRes is ConditionPattern))
                    {
                        AdvanceCursor();
                        iRes = GetCurrent();
                        continue;
                    }
                    else
                    {
                        return iRes;
                    }
                }
                else
                {
                    AdvanceCursor();
                    iRes = GetCurrent();
                    continue;
                }
            }

            return null;
        }

        /// <summary>returns the id's of all the inputpatterns, outputpatterns and
        ///     invalid-response-patterns. found in the editor assigned as current
        ///     scope.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private IEnumerable<ulong> GetDataFromCurrentScope()
        {
            fCursor.Editor = RequestScope;
            if (fCursor.GotoFirst() == true)
            {
                TextPatternBase iFound = GetCurrentFiltered();
                while (iFound != null)
                {
                    Process.CurrentPos = fCursor.RuleIndex + 1;

                        // we add 1 cause the index is 0 based. we set here, cause the CurrentPos is determined depending on the type of loop being used.
                    yield return iFound.Item.ID;
                    iFound = GetCurrentFiltered();
                }
            }

            fCursor.Reset();
        }

        /// <summary>returns the id's of all the inputpatterns, outputpatterns and
        ///     invalid-response-patterns. found in the editors that are currently
        ///     open.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private IEnumerable<ulong> GetDataFromOpenDocs()
        {
            while (fCurrent < BrainData.Current.OpenDocuments.Count)
            {
                TextPatternEditor iEditor = BrainData.Current.OpenDocuments[fCurrent] as TextPatternEditor;
                fCurrent++;
                Process.CurrentPos++;
                if (iEditor != null)
                {
                    fCursor.Editor = iEditor;
                    if (fCursor.GotoFirst() == true)
                    {
                        TextPatternBase iFound = GetCurrentFiltered();
                        while (iFound != null)
                        {
                            yield return iFound.Item.ID;
                            iFound = GetCurrentFiltered();
                        }
                    }

                    fCursor.Reset();
                }
            }
        }

        /// <summary>returns the id's of all the inputpatterns, outputpatterns and
        ///     invalid-response-patterns. found in the editors that are currently
        ///     defined in the project.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private IEnumerable<ulong> GetDataFromAll()
        {
            while (fCurrent < fAllEditors.Count)
            {
                TextPatternEditor iEditor = fAllEditors[fCurrent];
                bool iIsOpen = iEditor.IsOpen;
                iEditor.IsOpen = true;
                fCursor.Editor = iEditor;
                try
                {
                    if (fCursor.GotoFirst() == true)
                    {
                        TextPatternBase iRes = GetCurrentFiltered();
                        while (iRes != null)
                        {
                            yield return iRes.Item.ID;
                            iRes = GetCurrentFiltered();
                        }
                    }
                }
                finally
                {
                    fCursor.Reset(); // do before closing the editor, cause the rest will also unload the last rule.
                    if (BrainData.Current.OpenDocuments.Contains(fCursor.Editor) == false)
                    {
                        // only try to close the editor if it didn't get loaded because there was some data to show, otherwise leave it.
                        iEditor.IsOpen = iIsOpen;
                    }
                }

                Process.CurrentPos++;

                    // we advance the pos manually cause we aren't returning a value, but we did include it with the total count.
                fCurrent++;
            }
        }

        #endregion

        #region ISearchDataProvider Members

        /// <summary>Gets the data that needs to be searched in the form of neuron id's.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public IEnumerable<ulong> GetData()
        {
            return InternalGetData();
        }

        /// <summary>Continues to get the data from when we stopped at the previous run.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public IEnumerable<ulong> ContinueData()
        {
            return InternalGetData();
        }

        /// <summary>
        ///     This function is called when a valid search result is found and the
        ///     object at the current pointer position should be selected.
        /// </summary>
        public void SelectCurrent()
        {
            TextPatternBase iPattern = GetCurrent();
            if (fDoFindAll == true)
            {
                if (fResultFolder == null)
                {
                    // build result view objects if they don't yet exist.
                    fResultFolder = new DisplayPathSetFolder();
                    fResultFolder.Title = TextToSearch;
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<BaseDisplayPathSet>(SearchResults.Default.Items.Add), 
                        fResultFolder); // do async cause this is synced with the UI
                }

                if (fCursor.ResultSet == null)
                {
                    fCursor.ResultSet = new DisplayPathSet();
                    fCursor.ResultSet.Title = fCursor.Editor.Name;
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<BaseDisplayPathSet>(fResultFolder.Items.Add), 
                        fCursor.ResultSet); // do async cause this is displayed in the ui.
                }

                iPattern.Selectionrange = fCursor.Selection;

                if (fDoReplace == true)
                {
                    iPattern.Expression = iPattern.Expression.Substring(0, iPattern.Selectionrange.Start) + ReplaceText
                                          + iPattern.Expression.Substring(
                                              iPattern.Selectionrange.Start + iPattern.Selectionrange.Length);
                    iPattern.Selectionrange = new SelectionRange()
                                                  {
                                                      Start = fCursor.Selection.Start, 
                                                      Length = ReplaceText.Length
                                                  };
                }

                DisplayPath iPath = iPattern.GetDisplayPathFromThis();

                    // needs to be done from this thread, otherwise, the cursor might have moved on.
                iPath.Title = iPattern.Expression;
                App.Current.Dispatcher.BeginInvoke(new Action<DisplayPath>(fCursor.ResultSet.Items.Add), iPath);
            }

            Action<TextPatternBase, TextPatternEditor> iAsync =
                new Action<TextPatternBase, TextPatternEditor>(InternalSetlectCurrent);
            App.Current.Dispatcher.BeginInvoke(iAsync, iPattern, fCursor.Editor);
            if (fAdvanceOnContinue == true)
            {
                // we continue from a prev pos, so advance 1 before we continue.
                fCursor.TextPos = 0;
                AdvanceCursor();
                fAdvanceOnContinue = false;
            }
        }

        /// <summary>Activates the doc. We need a small seperate function for this cause we
        ///     ca't access WindowMain.Current from another thread then the UI.</summary>
        /// <param name="toActivate">To activate.</param>
        private void ActivateDoc(object toActivate)
        {
            if (BrainData.Current.OpenDocuments.Contains(fCursor.Editor) == false)
            {
                BrainData.Current.OpenDocuments.Add(fCursor.Editor); // make certain that the editor is loaded
            }

            WindowMain.Current.ActivateDoc(toActivate);
        }

        /// <summary>Selects the current item, brings it into view and gives it keyboard
        ///     focus.</summary>
        /// <param name="pattern">The pattern, so we can call async but advance in the main thread.</param>
        /// <param name="editor">The editor.</param>
        private void InternalSetlectCurrent(TextPatternBase pattern, TextPatternEditor editor)
        {
            if (fDoFindAll == false)
            {
                ActivateDoc(editor); // the window can only be accessed from the ui thread.
                OutputPattern iOut = pattern.Owner as OutputPattern;

                    // invalid patterns are hidden inside the output pattern, make certain that it is expanded.
                if (iOut != null)
                {
                    iOut.IsExpanded = true;
                }
                else if (pattern is DoPattern)
                {
                    // it could be a do pattern, in which case we need to make certain that owner is expanded.
                    if (pattern.Owner is PatternRule)
                    {
                        ((PatternRule)pattern.Owner).IsToCalculateVisible = true;
                    }
                    else if (pattern.Owner is PatternRuleOutput)
                    {
                        ((PatternRuleOutput)pattern.Owner).IsDoPatternVisible = true;
                    }
                }
                else if (pattern is TopicFilterPattern)
                {
                    ((TextPatternEditor)pattern.Owner).IsTopicsFiltersSelected = true;
                }

                pattern.IsSelected = true;
                pattern.Selectionrange = fCursor.Selection;
            }
        }

        /// <summary>
        ///     Called when the search process has reached the end.
        /// </summary>
        public void Finished()
        {
            Process.TotalCount = 0;

                // reset, otherwise the count remains up, giving errors in the process-tracker for next items.
            Process = null;
            App.Current.Dispatcher.BeginInvoke(new Action(FininishedInternal));
        }

        /// <summary>
        ///     called when the search is Fininished. This should be performed on the
        ///     ui thread.
        /// </summary>
        private void FininishedInternal()
        {
            if (fDoFindAll == true)
            {
                if (fDoReplace == true)
                {
                    WindowMain.UndoStore.EndUndoGroup();

                        // a replace all started an undo group on it's side of the algorithm, now, over here at the end, we need to close it again.
                }

                fDoFindAll = false;
                WindowMain.Current.ActivateTool(ToolsList.Default.SearchResultsTool);

                    // make certain it is visible and active
                SearchResults.Default.SelectedIndex = SearchResults.Default.Items.Count - 1;
            }
            else
            {
                MessageBox.Show("End of search reached.", "Search", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        /// <summary>Continues to get the data from when we stopped at the previous run
        ///     (while searching id's and strings). Not used.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public IEnumerable<object> ContinueObjectData()
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets the data that needs to be searched in the form of neuron id's and
        ///     strings. Not used.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public IEnumerable<object> GetObjectData()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}