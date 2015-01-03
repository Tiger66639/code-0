// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesPOSItemPEEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   provides an enumerator for the <see cref="DropDownNSSelector" /> to have
//   a <see cref="BrowserDataSource" /> object for all the text patterns in a
//   thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides an enumerator for the <see cref="DropDownNSSelector" /> to have
    ///     a <see cref="BrowserDataSource" /> object for all the text patterns in a
    ///     thesaurus.
    /// </summary>
    public class ThesLinkedItemPEEnumerator : ThesPosItemEnumerator
    {
        /// <summary>Initializes a new instance of the <see cref="ThesLinkedItemPEEnumerator"/> class. Initializes a new instance of the<see cref="ThesLinkedItemPEEnumerator"/> class.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="pos">The pos.</param>
        public ThesLinkedItemPEEnumerator(Thesaurus thes, Neuron pos)
            : base(thes, pos)
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
                return new ThesItemPEEnumerator(Thesaurus, POS, Items[Counter]);
            }
        }

        #endregion
    }
}