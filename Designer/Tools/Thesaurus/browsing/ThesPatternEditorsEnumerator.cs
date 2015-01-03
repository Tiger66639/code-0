// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesPatternEditorsEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   provides an enumerator that can be used by a
//   <see cref="DropDownNSSelector" /> in a <see cref="TextPatternEditor" />
//   </para>
//   <para>
//   to select data from text pattern editors attached to the thesaurus nodes.
//   </para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     <para>
    ///         provides an enumerator that can be used by a
    ///         <see cref="DropDownNSSelector" /> in a <see cref="TextPatternEditor" />
    ///     </para>
    ///     <para>
    ///         to select data from text pattern editors attached to the thesaurus nodes.
    ///     </para>
    /// </summary>
    public class ThesPatternEditorsEnumerator : ThesaurusEnumerator
    {
        /// <summary>Initializes a new instance of the <see cref="ThesPatternEditorsEnumerator"/> class. Initializes a new instance of the<see cref="ThesPatternEditorsEnumerator"/> class.</summary>
        /// <param name="thesaurus">The thesaurus.</param>
        public ThesPatternEditorsEnumerator(Thesaurus thesaurus)
            : base(thesaurus)
        {
        }

        #region IEnumerator Members

        /// <summary>
        ///     Gets the current element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The enumerator is positioned before the first element of the
        ///     collection or after the last element.
        /// </exception>
        /// <returns>
        ///     The current element in the collection.
        /// </returns>
        public override object Current
        {
            get
            {
                var iRes = new ThesLinkedItemPEEnumerator(fThes, fThes.PosFilters[Counter].Item);
                return iRes;
            }
        }

        #endregion
    }
}