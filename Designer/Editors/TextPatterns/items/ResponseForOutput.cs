// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseForOutput.cs" company="">
//   
// </copyright>
// <summary>
//   A textNeuron wrapper that references an already existing output value,
//   defined in another pattern. This indicates that the 'from' part of the
//   reference can be the response to the 'to' part of the reference.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A textNeuron wrapper that references an already existing output value,
    ///     defined in another pattern. This indicates that the 'from' part of the
    ///     reference can be the response to the 'to' part of the reference.
    /// </summary>
    public class ResponseForOutput : ParsableTextPatternBase
    {
        /// <summary>Initializes a new instance of the <see cref="ResponseForOutput"/> class. Initializes a new instance of the <see cref="ResponseForOutput"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ResponseForOutput(TextNeuron toWrap)
            : base(toWrap)
        {
            IsPatternStyle = false;
        }

        #region ResponseFor

        /// <summary>
        ///     Gets/sets the neuron that we reference as being a response for.
        /// </summary>
        public Neuron ResponseFor
        {
            get
            {
                return Item;
            }

            set
            {
                if (value != Item)
                {
                    var iOwner = Owner as IResponseForOutputOwner;
                    var iValue = value as TextNeuron;
                    if (iValue != null)
                    {
                        var iNew = new ResponseForOutput(iValue);
                        var iIndex = iOwner.ResponseFor.IndexOf(this);
                        iOwner.ResponseFor[iIndex] = iNew; // we replace the entire object, cause that's all we wrap.

                        // OnPropertyChanged("ResponseFor");
                    }
                    else
                    {
                        iOwner.ResponseFor.Remove(this);
                    }
                }
            }
        }

        #endregion

        #region IsPatternStyle

        /// <summary>
        ///     Gets/sets wether this is a pattern style or reference style 'response
        ///     for' definition.
        /// </summary>
        public bool IsPatternStyle { get; private set; }

        #endregion

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
                if (IsPatternStyle)
                {
                    if (Item != null)
                    {
                        var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.ParsedPatternStart);
                        return iFound != null;
                    }

                    return false;
                }

                return true;
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
                return EditMode.AddResponseFor;
            }
        }

        /// <summary>
        ///     Gets the output pattern to which this item belongs.
        /// </summary>
        public override OutputPattern Output
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        ///     Gets the ruleOutput to which this object belongs (null by default).
        /// </summary>
        public override PatternRuleOutput RuleOutput
        {
            get
            {
                return null;
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            if (value != TextNeuron.GetFor(" ")
                && value.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternOutputs) == null)
            {
                // check if it is a pattern or reference style of response-for definition. When it's the " " textneuron, it's was added as a placeholder to select another pattern.
                IsPatternStyle = true;
            }
            else
            {
                IsPatternStyle = false;
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

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            var iOwner = Owner as IResponseForOutputOwner;
            if (IsPatternStyle == false)
            {
                iOwner.ResponseFor.Remove(this);
            }
            else
            {
                base.Delete();
            }

            if (iOwner != null && iOwner.ResponseFor != null)
            {
                iOwner.ResponseFor.CheckEmptyList();

                    // when a 'response-for' gets removed, we need to check if the list isn't empty, if so, remove the list as well.
            }
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iItem = Owner as PatternEditorItem;
            if (iItem != null)
            {
                iItem.FillDisplayPathForThis(ref list);

                    // this will create the list as well, when it finds the first item.
                var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.ResponseForOutputs);

                    // the link to the output statement.
                list.Items.Add(iOut);

                var iOwner = Owner as IResponseForOutputOwner;
                var iChild = new Search.DPIChild(iOwner.ResponseFor.IndexOf(this));
                list.Items.Add(iChild);
            }
        }

        /// <summary>Gets the display path that points to the current object. This is for
        ///     the <see cref="ResponseFor"/> value.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIRoot iRoot = null;
            FillDisplayPathForThis(ref iRoot);
            if (iRoot != null)
            {
                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            var iRule = Owner as IResponseForOutputOwner;
            var iNew = EditorsHelper.AddNewPatternStyleResponseFor(
                iRule.ResponseFor, 
                string.Empty, 
                iRule.ResponseFor.IndexOf(this) + offset);
            return iNew;
        }

        #region parse

        /// <summary>
        ///     Instructs to <see cref="Parse" /> this instance.
        /// </summary>
        protected internal override void Parse()
        {
            if (IsPatternStyle)
            {
                var iOwner = Rule;
                System.Diagnostics.Debug.Assert(iOwner != null);
                var iEditor = iOwner.Owner as TextPatternEditor;
                if ((IsDirty || HasError || IsParsed == false) && (iEditor == null || iEditor.IsParsed))
                {
                    // when the root editor is set to no-parse'  don't try to parse the pattern.
                    ForceParse();
                }
            }
        }

        /// <summary>The force parse.</summary>
        protected internal override void ForceParse()
        {
            if (IsPatternStyle)
            {
                ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
                if (Expression != null && string.IsNullOrEmpty(Expression.Trim()) == false)
                {
                    // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                    try
                    {
                        var iOwner = Rule;
                        System.Diagnostics.Debug.Assert(iOwner != null);
                        Neuron iLocalTo = iOwner.ResponsesFor.Cluster;

                            // the cluster with all the different response-for groups is used as a local pattern start.
                        var iRoot = (Data.NamedObject)Root;
                        var iTitle = iRoot != null ? iRoot.Name : string.Empty;

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
        }

        /// <summary>
        ///     Instructs to <see cref="Parse" /> this instance.
        /// </summary>
        protected internal override void ParseWithUndo()
        {
            if (IsPatternStyle)
            {
                var iGroup = (ResponsesForGroup)Owner;
                var iRule = iGroup.Rule;
                System.Diagnostics.Debug.Assert(iRule != null);
                var iEditor = iRule.Owner as TextPatternEditor;
                if ((IsDirty || HasError || IsParsed == false) && (iEditor == null || iEditor.IsParsed))
                {
                    // when the root editor is set to no-parse'  don't try to parse the pattern.
                    ResetParseError();

                        // assign the empty string and not null, to indicate that it has already been parsed.
                    if (string.IsNullOrEmpty(Expression.Trim()) == false)
                    {
                        // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                        try
                        {
                            Neuron iLocalTo = iRule.ResponsesFor.Cluster;

                                // the cluster with all the different response-for groups is used as a local pattern start.
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
        }

        #endregion
    }
}