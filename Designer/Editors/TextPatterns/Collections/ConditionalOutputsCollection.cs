// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionalOutputsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The conditional outputs collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The conditional outputs collection.</summary>
    public class ConditionalOutputsCollection : PatternEditorCollection<PatternRuleOutput>, Search.IDisplayPathBuilder
    {
        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object. When this
        ///     object is requested to build a displaypath, it is for the<see cref="ChatbotProperties"/> object, for the repetition reponses.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public Search.DisplayPath GetDisplayPathFromThis()
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iFocused != null && iFocused.Tag != null)
            {
                var iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsRepetetitionSelected);
                var iChild = new Search.DPITextTag((string)iFocused.Tag);
                iRoot.Items.Add(iChild);
                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        #endregion

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="PatternRuleOutput"/>.</returns>
        public override PatternRuleOutput GetWrapperFor(Neuron toWrap)
        {
            return new PatternRuleOutput((NeuronCluster)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.Condition;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ConditionalOutputsCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public ConditionalOutputsCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConditionalOutputsCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public ConditionalOutputsCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        #endregion
    }
}