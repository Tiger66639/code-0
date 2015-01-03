// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionPattern.cs" company="">
//   
// </copyright>
// <summary>
//   A patten that is used as a condition on a
//   <see cref="PatternRuleOutput" /> set, so we can make output conditional.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A patten that is used as a condition on a
    ///     <see cref="PatternRuleOutput" /> set, so we can make output conditional.
    /// </summary>
    public class ConditionPattern : ParsableTextPatternBase
    {
        /// <summary>Initializes a new instance of the <see cref="ConditionPattern"/> class. Initializes a new instance of the <see cref="ConditionPattern"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ConditionPattern(TextNeuron toWrap)
            : base(toWrap)
        {
        }

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
                    var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.BoolExpression);

                        // check if there is a conditional part, cause that's how the condition gets parsed.
                    return iFound != null;
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets the edit mode that should be activated when this pattern performs
        ///     an insert. This is required for the editor, so it can put the focus on
        ///     the correct item.
        /// </summary>
        public override EditMode RequiredEditMode
        {
            get
            {
                return EditMode.AddConditional;
            }
        }

        /// <summary>
        ///     Instructs to Parse this instance.
        /// </summary>
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
                    var iParser = new Parsers.ConditionParser((TextNeuron)Item, iTitle);
                    iParser.Parse();
                    IsDirty = false;
                    OnPropertyChanged("ParseError");

                        // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                }
                catch (System.Exception e)
                {
                    Parsers.ConditionParser.RemoveCondPattern((TextNeuron)Item);

                        // in case of error, don't store the parsed path.
                    ParseError = e.Message;
                }
            }
            else
            {
                Parsers.ConditionParser.RemoveCondPattern((TextNeuron)Item);

                    // in case of error, don't store the parsed path.
            }
        }

        /// <summary>
        ///     Instructs to parse this instance, while generating undo data for the
        ///     aprse.
        /// </summary>
        protected internal override void ParseWithUndo()
        {
            if (IsDirty || HasError || IsParsed == false)
            {
                ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
                if (string.IsNullOrEmpty(Expression.Trim()) == false)
                {
                    // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                    try
                    {
                        var iRoot = (Data.NamedObject)Root;
                        var iTitle = iRoot != null ? iRoot.Name : string.Empty;
                        var iParser = new Parsers.ConditionParser((TextNeuron)Item, iTitle);
                        iParser.Parse();
                        IsDirty = false;
                        GenerateBuildUndoData();
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
                    catch (System.Exception e)
                    {
                        Parsers.ConditionParser.RemoveCondPattern((TextNeuron)Item);

                            // in case of error, don't store the parsed path.
                        ParseError = e.Message;
                        GenerateCleanUndoData();
                    }
                }
                else
                {
                    Parsers.ConditionParser.RemoveCondPattern((TextNeuron)Item);

                        // in case of error, don't store the parsed path.
                }
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
            var iUndo = new BuildConditionPatternUndoItem();
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
            var iUndo = new ClearConditionPatterUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>
        ///     Clears the parse while generating undo data. This is used when the
        ///     item gets deleted with the delete command.
        /// </summary>
        protected internal override void ClearParseWithUndo()
        {
            Parsers.ConditionParser.RemoveCondPattern((TextNeuron)Item);
            var iUndo = new ClearConditionPatterUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            PatternRuleOutput iNew = null;
            var iCond = Owner as PatternRuleOutput;
            var iRule = iCond.Owner as PatternRule;
            if (iRule != null)
            {
                var iIndex = iRule.Conditionals.IndexOf(iCond);
                iNew = EditorsHelper.AddNewConditionalToPattern(iRule.Conditionals, iIndex + offset);
            }
            else
            {
                var iEditor = iCond.Owner as TextPatternEditor;
                if (iEditor != null)
                {
                    var iIndex = iEditor.Questions.IndexOf(iCond) + offset;
                    iNew = EditorsHelper.AddNewConditionalToPattern(iEditor.Questions, iIndex);
                }
            }

            if (iNew != null)
            {
                return EditorsHelper.AddNewCondition(iNew, string.Empty);

                    // we create an empty condition, so that there always is one, otherwise, the editor wont have one and cant create one
            }

            return null;
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
            var iItem = Owner as PatternEditorItem;
            if (iItem != null)
            {
                iItem.FillDisplayPathForThis(ref list);
                var iOutputs = new Search.DPILinkOut((ulong)PredefinedNeurons.Condition);
                list.Items.Add(iOutputs);
            }
        }
    }
}