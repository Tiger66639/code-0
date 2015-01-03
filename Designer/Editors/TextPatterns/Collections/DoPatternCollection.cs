// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoPatternCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The do pattern collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The do pattern collection.</summary>
    public class DoPatternCollection : PatternEditorCollection<DoPattern>, Search.IDisplayPathBuilder
    {
        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object. When this
        ///     object is requested to build a displaypath, it is for the<see cref="ChatbotProperties"/> object, for either the 'new' fallback
        ///     or starting statements.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public Search.DisplayPath GetDisplayPathFromThis()
        {
            var iProps = Owner as ChatbotProperties;
            Search.DPIChatbotPropsRoot iRoot;
            Search.DPITextTag iChild;
            if (iProps.DoAfterStatement == this)
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsDoAfterSelected);
                iChild = new Search.DPITextTag("FocusNewDo");
            }
            else
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsDoOnStartupSelected);
                iChild = new Search.DPITextTag("FocusNewDoOnStartup");
            }

            iRoot.Items.Add(iChild);
            return new Search.DisplayPath(iRoot);
        }

        #endregion

        /// <summary>rebulds all the patterns.</summary>
        /// <param name="errors"></param>
        internal void Rebuild(System.Collections.Generic.List<string> errors)
        {
            foreach (var i in this)
            {
                i.ForceParse();
                if (i.HasError)
                {
                    errors.Add(i.ParseError);
                }
            }
        }

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="DoPattern"/>.</returns>
        public override DoPattern GetWrapperFor(Neuron toWrap)
        {
            return new DoPattern((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.DoPatterns;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DoPatternCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public DoPatternCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DoPatternCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public DoPatternCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DoPatternCollection"/> class. Initializes a new instance of the <see cref="DoPatternCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <param name="clusterMeaning">The cluster meaning.</param>
        public DoPatternCollection(INeuronWrapper owner, ulong linkMeaning, ulong clusterMeaning)
            : base(owner, linkMeaning, clusterMeaning)
        {
        }

        #endregion
    }
}