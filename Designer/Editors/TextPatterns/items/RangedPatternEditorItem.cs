// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RangedPatternEditorItem.cs" company="">
//   
// </copyright>
// <summary>
//   Used to define a selection range within a string. This class is used to
//   instruct the UI to select a part of text + to store previously selected
//   data so that the UI part can unload for virtualization.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to define a selection range within a string. This class is used to
    ///     instruct the UI to select a part of text + to store previously selected
    ///     data so that the UI part can unload for virtualization.
    /// </summary>
    public class SelectionRange
    {
        /// <summary>Gets or sets the start.</summary>
        public int Start { get; set; }

        /// <summary>Gets or sets the length.</summary>
        public int Length { get; set; }
    }

    /// <summary>
    ///     a base class for all objects that need to store a text range for some
    ///     reason.
    /// </summary>
    public abstract class RangedPatternEditorItem : PatternEditorItem
    {
        /// <summary>The f selectionrange.</summary>
        private SelectionRange fSelectionrange;

        /// <summary>Initializes a new instance of the <see cref="RangedPatternEditorItem"/> class. Initializes a new instance of the<see cref="RangedPatternEditorItem"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        public RangedPatternEditorItem(Neuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RangedPatternEditorItem"/> class.</summary>
        public RangedPatternEditorItem()
        {
        }

        /// <summary>Adds a selection node to the list for selecting the text selection
        ///     range.</summary>
        /// <param name="root">The root.</param>
        protected void AddSelectionRange(Search.DPIRoot root)
        {
            if (Selectionrange != null)
            {
                var iRange = new Search.DPITextRange { Start = Selectionrange.Start, Length = Selectionrange.Length };
                root.Items.Add(iRange);
            }
        }

        #region Selectionrange

        /// <summary>
        ///     Gets/sets the selection that should be applied to the UI
        /// </summary>
        public SelectionRange Selectionrange
        {
            get
            {
                return fSelectionrange;
            }

            set
            {
                if (value != fSelectionrange)
                {
                    fSelectionrange = value;
                    OnPropertyChanged("Selectionrange");
                }
            }
        }

        /// <summary>Stores the selectionrange without raising an event. This is used by
        ///     the UI to update the <paramref name="value"/> without having it
        ///     propagade back to the UI.</summary>
        /// <param name="value">The value.</param>
        internal void SetSelectionrange(SelectionRange value)
        {
            fSelectionrange = value;
        }

        /// <summary>
        ///     Asks all UI's to Refresh the selection range. This is actually used to
        ///     bring the item into focus.
        /// </summary>
        internal void RefreshSelectionRange()
        {
            if (fSelectionrange != null)
            {
                // when refreshing this, always make certain that there is a new object, otherwise, the binding wont fire properly.
                fSelectionrange = new SelectionRange { Length = fSelectionrange.Length, Start = fSelectionrange.Start };
            }
            else
            {
                fSelectionrange = new SelectionRange();
            }

            OnPropertyChanged("Selectionrange");
        }

        #endregion
    }
}