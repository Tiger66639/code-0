// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPITextRange.cs" company="">
//   
// </copyright>
// <summary>
//   A display path item that provides support for selecting a range of text
//   in something. This is currently used for textpatterns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     A display path item that provides support for selecting a range of text
    ///     in something. This is currently used for textpatterns.
    /// </summary>
    public class DPITextRange : DisplayPathItem
    {
        #region Start

        /// <summary>
        ///     Gets/sets the start of the range.
        /// </summary>
        public int Start { get; set; }

        #endregion

        #region Length

        /// <summary>
        ///     Gets/sets the length of the range.
        /// </summary>
        public int Length { get; set; }

        #endregion

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner. Not supported for the moment.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            var iPattern = owner as RangedPatternEditorItem;
            if (iPattern != null)
            {
                iPattern.IsSelected = true;

                    // we already select it, otherwise the SelectionRange setter doens't work on the first time (the item needs to be selected to receive focus, focus is required to show the selectionrange).
                iPattern.Selectionrange = new SelectionRange { Start = Start, Length = Length };
                return iPattern;
            }

            return null;
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors. Not supported.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DPITextRange" /> class.
        /// </summary>
        public DPITextRange()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPITextRange"/> class.</summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        public DPITextRange(int start, int length)
        {
        }

        #endregion
    }
}