// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternEditorItem.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for every object used by the <see cref="TextPatternEditor" />
//   . Allows easy management of the selected items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for every object used by the <see cref="TextPatternEditor" />
    ///     . Allows easy management of the selected items.
    /// </summary>
    public abstract class PatternEditorItem : EditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="PatternEditorItem"/> class. 
        ///     Initializes a new instance of the <see cref="PatternEditorItem"/>
        ///     class.</summary>
        public PatternEditorItem()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternEditorItem"/> class. Initializes a new instance of the <see cref="PatternEditorItem"/>
        ///     class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        public PatternEditorItem(Neuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the rule to which this object belongs.
        /// </summary>
        public abstract PatternRule Rule { get; }

        /// <summary>
        ///     Gets the ruleOutput to which this object belongs (null by default).
        /// </summary>
        public virtual PatternRuleOutput RuleOutput
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        ///     Gets the output pattern to which this item belongs. By default, this
        ///     is null.
        /// </summary>
        public virtual OutputPattern Output
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        ///     Called by the editor when the user has requested to remove the
        ///     selected items from the list. The PatternDef should remove from the
        ///     TextPatternEditor, other items can aks their owner to remove the
        ///     child.
        /// </summary>
        internal abstract void RemoveFromOwner();

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal abstract void Delete();

        /// <summary>The fill display path for this.</summary>
        /// <param name="list">The list.</param>
        internal abstract void FillDisplayPathForThis(ref Search.DPIRoot list);

        /// <summary>Raises the propertyChanged event for the specified property. This is
        ///     used by the <see cref="Search.DPITextTag"/> . It's used to set focus to
        ///     either the input or output textbox for creating new rules.</summary>
        /// <param name="value">The value.</param>
        internal void PutFocusOn(string value)
        {
            TextPatternEditorResources.NeedsFocus = true;
            TextPatternEditorResources.FocusOn.PropName = value;
            TextPatternEditorResources.FocusOn.Item = this;
            OnPropertyChanged(value);
        }
    }
}