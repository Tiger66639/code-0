// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputPattern.cs" company="">
//   
// </copyright>
// <summary>
//   represents a single textpattern item, which is a string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     represents a single textpattern item, which is a string.
    /// </summary>
    public class InputPattern : ParsableTextPatternBase
    {
        /// <summary>The f allow duplicate.</summary>
        private bool? fAllowDuplicate;

        /// <summary>Initializes a new instance of the <see cref="InputPattern"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public InputPattern(TextNeuron toWrap)
            : base(toWrap)
        {
        }

        #region IsParsed

        /// <summary>
        ///     Gets a value indicating whether this instance is parsed or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is parsed; otherwise, <c>false</c> .
        /// </value>
        public override bool IsParsed
        {
            get
            {
                if (Item != null)
                {
                    var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.ParsedPatternStart);
                    return iFound != null;
                }

                return false;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the edit mode that should be activated when this pattern performs
        ///     an insert. This is required for the editor, so it can put the focus on
        ///     the correct item.
        /// </summary>
        public override EditMode RequiredEditMode
        {
            get
            {
                return EditMode.AddPattern;
            }
        }

        #region AllowDuplicate

        /// <summary>
        ///     Gets or sets wether the pattern is allowed to be a duplicate or not.
        /// </summary>
        /// <value>
        ///     True: duplicates are allowed false: duplicates are not allowed null:
        ///     it's the fallback for a set of duplicates (the fallback is used when
        ///     there is no proper focus found).
        /// </value>
        public bool? AllowDuplicate
        {
            get
            {
                return fAllowDuplicate;
            }

            set
            {
                if (value != fAllowDuplicate)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        fAllowDuplicate = value;
                        if (value.HasValue)
                        {
                            if (value.Value == false)
                            {
                                EditorsHelper.SetFirstOutgoingLinkTo(
                                    Item, 
                                    (ulong)PredefinedNeurons.InputPatternPartialMode, 
                                    (Neuron)null);
                            }
                            else
                            {
                                EditorsHelper.SetFirstOutgoingLinkTo(
                                    Item, 
                                    (ulong)PredefinedNeurons.InputPatternPartialMode, 
                                    (ulong)PredefinedNeurons.PartialInputPattern);
                            }
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.InputPatternPartialMode, 
                                (ulong)PredefinedNeurons.PartialInputPatternFallback);
                        }

                        Parse(); // reparse, in case a new error popped up because of this change (duplicates)
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }

                    OnPropertyChanged("AllowDuplicate");
                }
            }
        }

        #endregion

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any data change at the correct
        ///     moment.
        /// </summary>
        protected internal override void GenerateBuildUndoData()
        {
            var iUndo = new BuildTextPatternUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any compile events at the correct
        ///     moment.
        /// </summary>
        protected internal override void GenerateCleanUndoData()
        {
            var iUndo = new ClearTextPatternBuildUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>
        ///     Clears the parse while generating undo data. This is used when the
        ///     item gets deleted with the delete command.
        /// </summary>
        protected internal override void ClearParseWithUndo()
        {
            if (Item != null)
            {
                // can be when loosing focus, no data has been entered.
                Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);
                var iUndo = new ClearTextPatternBuildUndoItem();
                iUndo.Pattern = Item as TextNeuron;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            }
        }

        /// <summary>Gets the display path that points to the current object.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIRoot iRoot = null;
            FillDisplayPathForThis(ref iRoot);
            if (iRoot != null)
            {
                AddSelectionRange(iRoot);
                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iItem = Owner as PatternRule;
            if (iItem != null)
            {
                iItem.FillDisplayPathForThis(ref list);
                var iChild = new Search.DPIChild(iItem.TextPatterns.IndexOf(this));
                list.Items.Add(iChild);
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iFound = value.FindFirstOut((ulong)PredefinedNeurons.InputPatternPartialMode);
            if (iFound == null)
            {
                fAllowDuplicate = false;
            }
            else if (iFound.ID == (ulong)PredefinedNeurons.PartialInputPattern)
            {
                fAllowDuplicate = true;
            }
            else if (iFound.ID == (ulong)PredefinedNeurons.PartialInputPatternFallback)
            {
                fAllowDuplicate = null;
            }
            else
            {
                fAllowDuplicate = false;
            }
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            var iRule = Owner as PatternRule;
            var iNew = EditorsHelper.AddNewTextPattern(
                iRule.TextPatterns, 
                string.Empty, 
                iRule.TextPatterns.IndexOf(this) + offset);
            return iNew;
        }

        #region parse

        /// <summary>
        ///     Instructs to <see cref="Parse" /> this instance.
        /// </summary>
        protected internal override void Parse()
        {
            var iOwner = (PatternRule)Owner;
            System.Diagnostics.Debug.Assert(iOwner != null);
            var iEditor = iOwner.Owner as TextPatternEditor;
            if ((IsDirty || HasError || IsParsed == false) && (iEditor == null || iEditor.IsParsed))
            {
                // when the root editor is set to no-parse'  don't try to parse the pattern.
                ForceParse();
            }
        }

        /// <summary>The force parse.</summary>
        protected internal override void ForceParse()
        {
            ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
            if (string.IsNullOrEmpty(Expression.Trim()) == false)
            {
                // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                try
                {
                    var iRoot = (Data.NamedObject)Root;
                    var iTitle = iRoot != null ? iRoot.Name : string.Empty;
                    Neuron iLocalTo = null;
                    if (iRoot is TextPatternEditor)
                    {
                        var iEditor = (TextPatternEditor)iRoot;
                        if (iEditor.IsLocal)
                        {
                            iLocalTo = iEditor.Item;
                        }
                    }

                    var iParser = new Parsers.InputParser((TextNeuron)Item, iTitle, iLocalTo);

                        // do an auto parse the data
                    iParser.Parse();
                    IsDirty = false;
                    OnPropertyChanged("ParseError");

                        // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                }
                catch (System.Exception e)
                {
                    Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);

                        // when something went wrong, we remove the compilation, cause it's invalid anyway.
                    ParseError = e.Message;
                }
            }
            else
            {
                Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);

                    // string is empty, simply clear out the parse tree + store some undo data so we can rebuild it again.
            }
        }

        /// <summary>
        ///     Instructs to <see cref="Parse" /> this instance.
        /// </summary>
        protected internal override void ParseWithUndo()
        {
            var iOwner = (PatternRule)Owner;
            System.Diagnostics.Debug.Assert(iOwner != null);
            var iEditor = iOwner.Owner as TextPatternEditor;
            if ((IsDirty || HasError || IsParsed == false) && (iEditor == null || iEditor.IsParsed))
            {
                // when the root editor is set to no-parse'  don't try to parse the pattern.
                ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
                if (string.IsNullOrEmpty(Expression.Trim()) == false)
                {
                    try
                    {
                        var iLocalTo = iEditor.IsLocal ? iEditor.Item : null;

                        var iParser = new Parsers.InputParser((TextNeuron)Item, iEditor.Name, iLocalTo);

                            // do an auto parse the data
                        iParser.Parse();
                        IsDirty = false;
                        GenerateBuildUndoData();
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
                    catch (System.Exception e)
                    {
                        try
                        {
                            // try to clean the pattern, if that fails, don't try it again.
                            Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);

                                // when something went wrong, we remove the compilation, cause it's invalid anyway.
                        }
                        catch
                        {
                            // simply log.
                        }

                        ParseError = e.Message;
                        GenerateCleanUndoData();
                    }
                }
                else
                {
                    Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);

                        // string is empty, simply clear out the parse tree + store some undo data so we can rebuild it again. 
                }
            }
        }

        #endregion
    }
}