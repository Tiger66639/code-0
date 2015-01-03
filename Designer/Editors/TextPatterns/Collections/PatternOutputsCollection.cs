// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternOutputsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Wraps all the possible output values for a pattern def.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Wraps all the possible output values for a pattern def.
    /// </summary>
    public class PatternOutputsCollection : PatternEditorCollection<OutputPattern>, Search.IDisplayPathBuilder
    {
        /// <summary>
        ///     Gets or sets the tag to use when creating a display path. This is so
        ///     that the chatbotProperties window can put focus again on one of it's
        ///     global lists (fallbacks, opening statements,...)
        /// </summary>
        /// <value>
        ///     The display path tag.
        /// </value>
        public string DisplayPathTag { get; set; }

        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object. When this
        ///     object is requested to build a displaypath, it is for the<see cref="ChatbotProperties"/> object, for either the 'new' fallback
        ///     or starting statements.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIChatbotPropsRoot iRoot;
            if (DisplayPathTag == "FocusNewFallback")
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsFallbackSelected);
            }
            else if (DisplayPathTag == "FocusNewStart")
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsOpeningStatSelected);
            }
            else if (DisplayPathTag == "FocusNewRepeat")
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsRepetetitionSelected);
            }
            else
            {
                iRoot = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsContextSelected);
            }

            var iChild = new Search.DPITextTag(DisplayPathTag);
            iRoot.Items.Add(iChild);
            return new Search.DisplayPath(iRoot);
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="PatternOutputsCollection"/> class. Initializes a new instance of the<see cref="PatternOutputsCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public PatternOutputsCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternOutputsCollection"/> class. Initializes a new instance of the<see cref="PatternOutputsCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning between new cluster and owner.</param>
        public PatternOutputsCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternOutputsCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <param name="cluserMeaning">The cluser meaning.</param>
        public PatternOutputsCollection(INeuronWrapper owner, ulong linkMeaning, ulong cluserMeaning)
            : base(owner, linkMeaning, cluserMeaning)
        {
        }

        #endregion

        #region functions

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="OutputPattern"/>.</returns>
        public override OutputPattern GetWrapperFor(Neuron toWrap)
        {
            return new OutputPattern((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.TextPatternOutputs;
        }

        /// <summary>rebuilds all the patterns and adds any <paramref name="errors"/> to
        ///     the list.</summary>
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

        #endregion
    }
}