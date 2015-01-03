// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemProfiledProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the neurons that were marked as leaks, for a single neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Profiler
{
    /// <summary>
    ///     Contains all the neurons that were marked as leaks, for a single neuron.
    /// </summary>
    public class MemProfiledProcessor : Data.ObservableObject
    {
        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<MemProfilerItem> fItems =
            new System.Collections.Generic.List<MemProfilerItem>();

        /// <summary>Initializes a new instance of the <see cref="MemProfiledProcessor"/> class. Initializes a new instance of the <see cref="MemProfiledProcessor"/>
        ///     class.</summary>
        /// <param name="stoppedAt">The stopped at.</param>
        public MemProfiledProcessor(Search.DisplayPath stoppedAt)
        {
            StoppedAd = stoppedAt;
        }

        #region StoppedAd

        /// <summary>
        ///     Gets the location at which the processor was stopped.
        /// </summary>
        public Search.DisplayPath StoppedAd { get; private set; }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the processor. Comes from the SplitPathAsString.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of neurons that were found to be leaks for the
        ///     processor.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public System.Collections.Generic.List<MemProfilerItem> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the item si expanded or not (for UI)
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                fIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the item is selected or not (to bind to from UI)
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                fIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region SplitPath

        /// <summary>
        ///     Gets/sets the split that the path of the processor took.
        /// </summary>
        public System.Collections.Generic.List<ulong> SplitPath { get; set; }

        #endregion
    }
}