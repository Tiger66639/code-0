// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParsableTextPatternBase.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for pattern items that can be parsed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class for pattern items that can be parsed.
    /// </summary>
    public abstract class ParsableTextPatternBase : TextPatternBase
    {
        /// <summary>The f is dirty.</summary>
        private bool fIsDirty;

        /// <summary>The f parse error.</summary>
        private string fParseError = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="ParsableTextPatternBase"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ParsableTextPatternBase(TextNeuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the edit mode that should be activated when this pattern performs
        ///     an insert. This is required for the editor, so it can put the focus on
        ///     the correct item.
        /// </summary>
        public abstract EditMode RequiredEditMode { get; }

        /// <summary>stores the expression <paramref name="value"/> for the pattern.</summary>
        /// <param name="value">The value.</param>
        protected override void SetExpression(string value)
        {
            // if (string.IsNullOrEmpty(value) == false)
            // {
            WindowMain.UndoStore.BeginUndoGroup();

                // the parse also generates undo data, this needs to be bunlded with the edit itself.
            try
            {
                base.SetExpression(value);
                ParseWithUndo();
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            // }
            // else
            // Delete(); //this is a bit of a problem for conditions: when deleted, the editor forgets to recreate when you type in the edit field.
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            NeuronInfo.PropertyChanged += NeuronInfo_PropertyChanged;
        }

        /// <summary>
        ///     Called when all the data kept in memory for the UI section can be
        ///     unloaded.
        /// </summary>
        internal override void UnloadUIData()
        {
            var iInfo = GetCurrentNeuronInfo();

                // don't need to create it: if not existing, the callback can't be registered.
            if (iInfo != null)
            {
                // this could be null if the item was already unloaded.
                iInfo.PropertyChanged -= NeuronInfo_PropertyChanged;
            }

            base.UnloadUIData();
        }

        /// <summary>Handles the PropertyChanged event of the <see cref="NeuronInfo"/>
        ///     control. When the display title changes, we need to let the expression
        ///     update + need to indicate that this pattern is dirty and needs a
        ///     recompile.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void NeuronInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayTitle")
            {
                IsDirty = true;
                OnPropertyChanged("Expression");
            }
        }

        /// <summary>
        ///     checks if this item needs to be reparsed, but not generating undo data
        ///     for the parse.
        /// </summary>
        protected internal virtual void Parse()
        {
            if (IsDirty || HasError || IsParsed == false)
            {
                ForceParse();
            }
        }

        /// <summary>
        ///     Forces to <see cref="Parse" /> this instance, but not generating undo
        ///     data for the parse.
        /// </summary>
        protected internal abstract void ForceParse();

        /// <summary>
        ///     Instructs to parse this instance, while generating undo data for the
        ///     aprse.
        /// </summary>
        protected internal abstract void ParseWithUndo();

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any data change at the correct
        ///     moment.
        /// </summary>
        protected internal abstract void GenerateBuildUndoData();

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any compile events at the correct
        ///     moment.
        /// </summary>
        protected internal abstract void GenerateCleanUndoData();

        /// <summary>
        ///     Clears the parse while generating undo data. This is used when the
        ///     item gets deleted with the delete command.
        /// </summary>
        protected internal abstract void ClearParseWithUndo();

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset from the current index at which the item should be
        ///     inserted. When 0, it's the saem pos, when 1, it's just after the
        ///     current item</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public abstract ParsableTextPatternBase Insert(int offset = 0);

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                ClearParseWithUndo();
                base.Delete();
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #region prop

        /// <summary>
        ///     Gets the output pattern to which this item belongs. By default, this
        ///     is null.
        /// </summary>
        public override OutputPattern Output
        {
            get
            {
                return Owner as OutputPattern;
            }
        }

        /// <summary>
        ///     Gets/sets the expression definition of the pattern.
        /// </summary>
        public override string Expression
        {
            get
            {
                return base.Expression;
            }

            set
            {
                bool iIsEqual;
                if (string.IsNullOrEmpty(value))
                {
                    // if one is null or "", we have to do an IsnNullOrEmpty compare, otherwise we sometimes get false results cause one is null and the other is ""
                    iIsEqual = string.IsNullOrEmpty(Expression);
                }
                else
                {
                    iIsEqual = value == Expression;
                }

                if (!iIsEqual)
                {
                    // only assign when possible
                    base.Expression = value;
                }
            }
        }

        #region IsParsed

        /// <summary>
        ///     Gets a value indicating whether this instance is parsed or not.
        /// </summary>
        /// <remarks>
        ///     The default is implemented for output patterns, cause these are the
        ///     most common (more different types).
        /// </remarks>
        /// <value>
        ///     <c>true</c> if this instance is parsed; otherwise, <c>false</c> .
        /// </value>
        public virtual bool IsParsed
        {
            get
            {
                if (Item != null)
                {
                    var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.ParsedPatternOutput);
                    return iFound != null;
                }

                return false;
            }
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance needs to be reparsed or
        ///     not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is dirty/needs a reparse; otherwise,
        ///     <c>false</c> .
        /// </value>
        public bool IsDirty
        {
            get
            {
                return fIsDirty;
            }

            protected set
            {
                if (value != fIsDirty)
                {
                    fIsDirty = value;
                }
            }
        }

        #region ParseError

        /// <summary>
        ///     Gets or sets the last parse error. Will reparse if there is no parse
        ///     error and no parse result (done for the ui, so it updates
        ///     automatically).
        /// </summary>
        /// <value>
        ///     The parse error.
        /// </value>
        public string ParseError
        {
            get
            {
                if (string.IsNullOrEmpty(fParseError) && IsParsed == false && string.IsNullOrEmpty(Expression) == false)
                {
                    // if there is no parseError but it's not parsed yet and there is something to parse, do this now
                    Parse();
                }

                return fParseError;
            }

            set
            {
                if (value != fParseError)
                {
                    if (value == null)
                    {
                        // we need to make certain that it never stores as null, but as string.empyt, otherwise some of the WPF template-triggers wont work properly.
                        fParseError = string.Empty;
                    }
                    else
                    {
                        fParseError = value;
                    }

                    OnPropertyChanged("ParseError");
                }
            }
        }

        /// <summary>
        ///     Resets the parse error without rasing a prop chagned. This is called
        ///     just before a recompile. There is no prop changed to make certain that
        ///     there isn't a reparse when the prop value (ParseError) is retrieved
        ///     before the actual parse is done, cause this can confuse the system and
        ///     it is <see langword="double" /> work.
        /// </summary>
        public void ResetParseError()
        {
            fParseError = string.Empty;
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance has a parse error or
        ///     not. (checking the actual parse error can reparse it).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has error; otherwise, <c>false</c> .
        /// </value>
        public bool HasError
        {
            get
            {
                return string.IsNullOrEmpty(fParseError) == false;
            }
        }

        #endregion
    }
}