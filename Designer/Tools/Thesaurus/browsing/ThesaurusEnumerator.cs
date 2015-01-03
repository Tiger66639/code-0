// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for thesaurus browsers. Provides an enumerator for all the pos
//   values. it's default browsing behaviour is all the thesaurus data, but
//   nothing extra.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for thesaurus browsers. Provides an enumerator for all the pos
    ///     values. it's default browsing behaviour is all the thesaurus data, but
    ///     nothing extra.
    /// </summary>
    public class ThesaurusEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>The f thes.</summary>
        protected Thesaurus fThes;

        /// <summary>Initializes a new instance of the <see cref="ThesaurusEnumerator"/> class.</summary>
        /// <param name="thesaurus">The thesaurus.</param>
        public ThesaurusEnumerator(Thesaurus thesaurus)
        {
            Counter = 0;
            fThes = thesaurus;
        }

        /// <summary>
        ///     Gets or sets the current pos counter.
        /// </summary>
        /// <value>
        ///     The counter.
        /// </value>
        public int Counter { get; set; }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public Thesaurus Owner
        {
            get
            {
                return fThes;
            }
        }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public bool HasChildren
        {
            get
            {
                return fThes.PosFilters.Count > 0;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public bool IsExpanded { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can be selected. This is a group, so
        ///     it can't be selected, the children can.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow selection]; otherwise, <c>false</c> .
        /// </value>
        public bool AllowSelection
        {
            get
            {
                return false;
            }
        }

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return new ThesaurusEnumerator(fThes);
        }

        #endregion

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
        public virtual object Current
        {
            get
            {
                var iRes = new ThesPosItemEnumerator(fThes, fThes.PosFilters[Counter].Item);
                return iRes;
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            Counter++;
            return Counter < fThes.PosFilters.Count;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first
        ///     element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            Counter = 0;
        }
    }
}