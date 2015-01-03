// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicFilterPattern.cs" company="">
//   
// </copyright>
// <summary>
//   a patter used by a topic to match against #user.topic
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a patter used by a topic to match against #user.topic
    /// </summary>
    public class TopicFilterPattern : ParsableTextPatternBase
    {
        /// <summary>Initializes a new instance of the <see cref="TopicFilterPattern"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public TopicFilterPattern(TextNeuron toWrap)
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
                return EditMode.AddTopicFilter;
            }
        }

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
            var iEditor = Owner as TextPatternEditor;
            System.Diagnostics.Debug.Assert(iEditor != null);
            list = new Search.DPITextPatternEditorRoot();
            list.Item = iEditor.Item;
            var iLink = new Search.DPILinkOut((ulong)PredefinedNeurons.TopicFilter);
            list.Items.Add(iLink);
            var iChild = new Search.DPIChild(iEditor.TopicFilters.IndexOf(this));
            list.Items.Add(iChild);
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            var iEditor = Owner as TextPatternEditor;
            var iNew = EditorsHelper.AddNewTopicFilter(
                iEditor.TopicFilters, 
                string.Empty, 
                iEditor.TopicFilters.IndexOf(this) + offset);
            return iNew;
        }

        #region parse

        /// <summary>
        ///     Instructs to <see cref="Parse" /> this instance.
        /// </summary>
        protected internal override void Parse()
        {
            var iEditor = Owner as TextPatternEditor;
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
                    var iRoot = (TextPatternEditor)Root;
                    var iTitle = iRoot != null ? iRoot.Name : string.Empty;
                    Neuron iLocalTo = Item.FindFirstClusteredBy((ulong)PredefinedNeurons.TopicFilter);
                    if (iLocalTo != null)
                    {
                        var iParser = new Parsers.InputParser((TextNeuron)Item, iTitle, iLocalTo);

                            // do an auto parse the data
                        iParser.Parse();
                        IsDirty = false;
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
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
            var iEditor = Owner as TextPatternEditor;
            if ((IsDirty || HasError || IsParsed == false) && (iEditor == null || iEditor.IsParsed))
            {
                // when the root editor is set to no-parse'  don't try to parse the pattern.
                ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
                if (string.IsNullOrEmpty(Expression.Trim()) == false)
                {
                    // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                    try
                    {
                        Neuron iLocalTo = Item.FindFirstClusteredBy((ulong)PredefinedNeurons.TopicFilter);
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
                        Parsers.InputParser.RemoveInputPattern((TextNeuron)Item);

                            // when something went wrong, we remove the compilation, cause it's invalid anyway.
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