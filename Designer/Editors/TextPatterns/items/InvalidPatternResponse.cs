// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidPatternResponse.cs" company="">
//   
// </copyright>
// <summary>
//   Wraps a textneuron used as a response in case the chatter didn't give a
//   valid answer to the question that this textneuron is attached to.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Wraps a textneuron used as a response in case the chatter didn't give a
    ///     valid answer to the question that this textneuron is attached to.
    /// </summary>
    public class InvalidPatternResponse : ParsableTextPatternBase
    {
        /// <summary>The f requires response.</summary>
        private bool fRequiresResponse;

        /// <summary>Initializes a new instance of the <see cref="InvalidPatternResponse"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public InvalidPatternResponse(TextNeuron toWrap)
            : base(toWrap)
        {
            fRequiresResponse = toWrap.FindFirstOut((ulong)PredefinedNeurons.RequiresResponse) != null;
        }

        #region RequiredEditMode

        /// <summary>
        ///     Gets the edit mode that should be activated when this pattern performs
        ///     an insert. This is required for the editor, so it can put the focus on
        ///     the correct item.
        /// </summary>
        public override EditMode RequiredEditMode
        {
            get
            {
                return EditMode.AddInvalid;
            }
        }

        #endregion

        #region Output

        /// <summary>
        ///     Gets the output pattern to which this item belongs.
        /// </summary>
        public override OutputPattern Output
        {
            get
            {
                return Owner as OutputPattern;
            }
        }

        #endregion

        #region RuleOutput

        /// <summary>
        ///     Gets the ruleOutput to which this object belongs (null by default).
        /// </summary>
        public override PatternRuleOutput RuleOutput
        {
            get
            {
                if (Output != null)
                {
                    return Output.RuleOutput;
                }

                return null;
            }
        }

        #endregion

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
                    var iParser = new Parsers.OutputParser((TextNeuron)Item, iTitle);
                    iParser.Parse();
                    IsDirty = false;
                    OnPropertyChanged("ParseError");

                        // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                }
                catch (System.Exception e)
                {
                    Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                        // in case of error, don't store the parsed path.
                    ParseError = e.Message;
                }
            }
            else
            {
                Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

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
                        var iParser = new Parsers.OutputParser((TextNeuron)Item, iTitle);
                        iParser.Parse();
                        IsDirty = false;
                        GenerateBuildUndoData();
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
                    catch (System.Exception e)
                    {
                        Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                            // in case of error, don't store the parsed path.
                        ParseError = e.Message;
                        GenerateCleanUndoData();
                    }
                }
                else
                {
                    Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

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
            var iUndo = new BuildOutputPatternUndoItem();
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
            var iUndo = new ClearOutputPatterUndoItem();
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
                Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);
                var iUndo = new ClearOutputPatterUndoItem();
                iUndo.Pattern = Item as TextNeuron;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            }
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            var iOut = Owner as OutputPattern;
            return EditorsHelper.AddNewInvalidPatternResponse(
                iOut.InvalidResponses, 
                string.Empty, 
                iOut.InvalidResponses.IndexOf(this) + offset);
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
            var iOut = Owner as OutputPattern;
            if (iOut != null)
            {
                iOut.FillDisplayPathForThis(ref list);
                var iLink = new Search.DPILinkOut((ulong)PredefinedNeurons.InvalidResponsesForPattern);

                    // link to list of invalid responses
                list.Items.Add(iLink);
                var iChild = new Search.DPIChild(iOut.InvalidResponses.IndexOf(this));

                    // child within the list of invalids.
                list.Items.Add(iChild);
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.RequiresResponse)
                {
                    SetRequiresResponse(false);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.RequiresResponse)
                {
                    SetRequiresResponse(true);
                }
            }
        }

        #region RequiresResponse

        /// <summary>
        ///     Gets/sets the value that indicates if the bot still requires a correct
        ///     answer or not. when true, the bot will remain waiting for a valid
        ///     answer.
        /// </summary>
        public bool RequiresResponse
        {
            get
            {
                return fRequiresResponse;
            }

            set
            {
                if (value != fRequiresResponse)
                {
                    InternalChange = true;
                    try
                    {
                        if (value)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.RequiresResponse, Item);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.RequiresResponse, 
                                (Neuron)null);
                        }
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    SetRequiresResponse(value);
                }
            }
        }

        /// <summary>Sets the requires response.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void SetRequiresResponse(bool value)
        {
            fRequiresResponse = value;
            OnPropertyChanged("RequiresResponse");
        }

        #endregion
    }
}