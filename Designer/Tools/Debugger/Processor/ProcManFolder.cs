// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcManFolder.cs" company="">
//   
// </copyright>
// <summary>
//   A folder that containa processor manager items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A folder that containa processor manager items.
    /// </summary>
    public class ProcManFolder : ProcManItem, IProcessorsOwner
    {
        #region fields

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded = true;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ProcManFolder"/> class.</summary>
        /// <param name="splitStart">The split start.</param>
        public ProcManFolder(ProcItem splitStart)
        {
            Processors = new Data.ObservedCollection<ProcManItem>(this);
            SplitStart = splitStart;
        }

        #endregion

        /// <summary>
        ///     Call this when you are done changing hte processors list, so that the
        ///     ui can be updated.
        /// </summary>
        public void ProcessorsChanged()
        {
            OnPropertyChanged("UIProcessors");
        }

        /// <summary>The get values for.</summary>
        /// <param name="toDisplay">The to display.</param>
        public override void GetValuesFor(Variable toDisplay)
        {
            foreach (var i in Processors)
            {
                i.GetValuesFor(toDisplay);
            }
        }

        /// <summary>The clear values.</summary>
        public override void ClearValues()
        {
            foreach (var i in Processors)
            {
                i.ClearValues();
            }
        }

        /// <summary>Called when a split <paramref name="path"/> was unselected. Allows us
        ///     to update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public override void OnSplitPathUnSelected(SplitPath path)
        {
            foreach (var i in Processors)
            {
                i.OnSplitPathUnSelected(path);
            }
        }

        /// <summary>Called when a split <paramref name="path"/> was selected. Allows us to
        ///     update all the processors for a selectionchange in the splitpaths.</summary>
        /// <param name="path">The path.</param>
        public override void OnSplitPathSelected(SplitPath path)
        {
            foreach (var i in Processors)
            {
                i.OnSplitPathSelected(path);
            }
        }

        #region prop

        #region IsFolder

        /// <summary>
        ///     Gets wether this is a folder or not (ProcManFolder overrides it and
        ///     returns true). This is to let wpf bind to the value.
        /// </summary>
        public override bool IsFolder
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Processors

        /// <summary>
        ///     Gets/sets the list of processors currently running. These are 'sub
        ///     items'.
        /// </summary>
        /// <remarks>
        ///     Call 'ListChanged' when done changing, so that the ui can be updated.
        /// </remarks>
        public Data.ObservedCollection<ProcManItem> Processors { get; private set; }

        #endregion

        #region UIProcessors

        /// <summary>
        ///     Gets/sets the list of processors currently running, as a non
        ///     observable list. This is used for the ui, so we can modify the
        ///     processors list from other threads.
        /// </summary>
        /// <remarks>
        ///     We allow for a setter, so that we can change the list from another
        ///     thread, to indicate list change.
        /// </remarks>
        public System.Collections.Generic.List<ProcManItem> UIProcessors
        {
            get
            {
                lock (Processors) return Enumerable.ToList(Processors);
            }
        }

        #endregion

        #region SplitStart

        /// <summary>
        ///     Gets the processor wrapper from which the split started. This is
        ///     required because it keeps track of all sub processors. We need the
        ///     <see langword="ref" /> to exist for as long as there are sub processors
        ///     (the creator processor can dy out, but we still need it's lists' cause
        ///     they are warned when changed.
        /// </summary>
        public ProcItem SplitStart { get; internal set; }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the folder is expanded or not.
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
            }
        }

        #endregion

        #endregion
    }
}