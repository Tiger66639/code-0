// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoPattern.cs" company="">
//   
// </copyright>
// <summary>
//   a pattern that defines the memory operations/actions/... that need to be
//   performed when a pattern rule gets activated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a pattern that defines the memory operations/actions/... that need to be
    ///     performed when a pattern rule gets activated.
    /// </summary>
    public class DoPattern : ParsableTextPatternBase
    {
        /// <summary>Initializes a new instance of the <see cref="DoPattern"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public DoPattern(TextNeuron toWrap)
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
                    var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.ParsedDoPattern);
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
                return EditMode.AddDo;
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
                    var iParser = new Parsers.DoParser((TextNeuron)Item, iTitle);
                    iParser.Parse();
                    IsDirty = false;
                    OnPropertyChanged("ParseError");

                        // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                }
                catch (System.Exception e)
                {
                    Parsers.DoParser.RemoveDoPattern((TextNeuron)Item); // in case of error, don't store the parsed path.
                    ParseError = e.Message;
                }
            }
            else
            {
                Parsers.DoParser.RemoveDoPattern((TextNeuron)Item); // in case of error, don't store the parsed path.
            }
        }

        /// <summary>
        ///     Parses the with undo.
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
                        string iTitle;
                        var iRoot = Root as Data.NamedObject;
                        iTitle = iRoot != null
                                     ? iRoot.Name
                                     : (Root is IDocumentInfo ? ((IDocumentInfo)Root).DocumentTitle : string.Empty);
                        var iParser = new Parsers.DoParser((TextNeuron)Item, iTitle);
                        iParser.Parse();
                        IsDirty = false;
                        GenerateBuildUndoData();
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
                    catch (System.Exception e)
                    {
                        Parsers.DoParser.RemoveDoPattern((TextNeuron)Item);

                            // in case of error, don't store the parsed path.
                        ParseError = e.Message;
                        GenerateCleanUndoData();
                    }
                }
                else
                {
                    Parsers.DoParser.RemoveDoPattern((TextNeuron)Item); // in case of error, don't store the parsed path.
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
            var iUndo = new BuildDoPatternUndoItem();
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
            var iUndo = new ClearDoPatterUndoItem();
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
                Parsers.DoParser.RemoveDoPattern((TextNeuron)Item);
                var iUndo = new ClearDoPatterUndoItem();
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
            return null; // there can always only be 1 do pattern 
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
                var iRule = iItem as PatternRule;
                Search.DPILinkOut iDoes;
                if (iRule != null)
                {
                    if (iRule.ToCal == this)
                    {
                        iDoes = new Search.DPILinkOut((ulong)PredefinedNeurons.Calculate);
                    }
                    else if (iRule.ToEval == this)
                    {
                        iDoes = new Search.DPILinkOut((ulong)PredefinedNeurons.Evaluate);
                    }
                    else
                    {
                        iDoes = new Search.DPILinkOut((ulong)PredefinedNeurons.DoPatterns);
                    }
                }
                else
                {
                    iDoes = new Search.DPILinkOut((ulong)PredefinedNeurons.DoPatterns);
                }

                list.Items.Add(iDoes);
            }
            else
            {
                var iProps = Root as ChatbotProperties;
                if (iProps.DoAfter == this)
                {
                    list = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsDoAfterSelected);
                }
                else if (iProps.DoStartup == this)
                {
                    list = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsDoOnStartupSelected);
                }
                else
                {
                    throw new System.InvalidOperationException("Unknown focused list type!");
                }
            }
        }
    }
}