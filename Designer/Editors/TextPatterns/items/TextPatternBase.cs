// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternBase.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for all items that represent a single pattern line.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A base class for all items that represent a single pattern line.
    /// </summary>
    public abstract class TextPatternBase : RangedPatternEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="TextPatternBase"/> class. Initializes a new instance of the <see cref="TextPatternBase"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public TextPatternBase(TextNeuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the rule to which this object belongs.
        /// </summary>
        public override PatternRule Rule
        {
            get
            {
                var iOwner = Owner as PatternEditorItem;
                if (iOwner != null)
                {
                    return iOwner.Rule;
                }

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
                if (Owner is PatternRule)
                {
                    var iOwner = Owner as PatternRule;
                    if (iOwner.ToCalculate != null && Enumerable.Contains(iOwner.ToCalculate, this))
                    {
                        return null;
                    }

                    return ((PatternRule)Owner).OutputSet;
                }

                return Owner as PatternRuleOutput;
            }
        }

        #region Expression

        /// <summary>
        ///     Gets/sets the expression definition of the pattern.
        /// </summary>
        public virtual string Expression
        {
            get
            {
                return NeuronInfo.DisplayTitle;
            }

            set
            {
                if (value != NeuronInfo.DisplayTitle)
                {
                    SetExpression(value);
                }
            }
        }

        /// <summary>stores the expression <paramref name="value"/> for the pattern.</summary>
        /// <param name="value">The value.</param>
        protected virtual void SetExpression(string value)
        {
            NeuronInfo.DisplayTitle = value;
        }

        #endregion

        #region functions

        /// <summary>
        ///     Called by the editor when the user has requested to remove the
        ///     selected items from the list. The PatternDef should remove from the
        ///     TextPatternEditor, other items can aks their owner to remove the
        ///     child.
        /// </summary>
        internal override void RemoveFromOwner()
        {
            ((EditorItem)Owner).RemoveChild(this);
        }

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            if (Neuron.IsEmpty(Item.ID) == false)
            {
                WindowMain.DeleteItemFromBrain(Item);
                Item = null; // needs to be reset, otherwise we keep an illegal reference
            }
        }

        /// <summary>
        ///     Tries to move focus to this instance by raising the propertychanged
        ///     for 'Selectionrange'.
        /// </summary>
        public void Focus()
        {
            OnPropertyChanged("Selectionrange");
        }

        #endregion
    }
}