// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowsableTopicsEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   provides browsing for a dropdown neuron selector, for selecting topic
//   content.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides browsing for a dropdown neuron selector, for selecting topic
    ///     content.
    /// </summary>
    public class BrowsableTopicsEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BrowsableTopicsEnumerator"/> class. Initializes a new instance of the<see cref="BrowsableOutputPatternEditorsEnumerator"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="topicsOnly">if set to <c>true</c> only the topics will be returned, otherwise the
        ///     entire content of each topic as well.</param>
        public BrowsableTopicsEnumerator(EditorCollection owner, bool topicsOnly = false)
        {
            Owner = owner;
            fTopicsOnly = topicsOnly;
        }

        #endregion

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public EditorCollection Owner { get; private set; }

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
                foreach (var i in Owner)
                {
                    if (i is TextPatternEditor)
                    {
                        return true;
                    }

                    if (i is EditorFolder)
                    {
                        return ((EditorFolder)i).ContainsTopics;
                    }
                }

                return false;
            }
        }

        #endregion

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

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public bool IsExpanded { get; set; }

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
            return new BrowsableTopicsEnumerator(Owner, fTopicsOnly);
        }

        #endregion

        #region fields

        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        /// <summary>The f topics only.</summary>
        private readonly bool fTopicsOnly;

        #endregion

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
        public object Current
        {
            get
            {
                var iItem = Owner[fCounter];
                if (iItem is TextPatternEditor)
                {
                    if (fTopicsOnly == false)
                    {
                        return ((TextPatternEditor)iItem).BrowsableItems;
                    }

                    return iItem;
                }

                if (iItem is IEditorsOwner)
                {
                    return new BrowsableTopicsEnumerator(((IEditorsOwner)iItem).Editors, fTopicsOnly);
                }

                return null; // shouldn't happen
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
            while (fCounter < Owner.Count - 1)
            {
                fCounter++;
                if ((Owner[fCounter] is EditorFolder && ((EditorFolder)Owner[fCounter]).ContainsTopics)
                    || Owner[fCounter] is TextPatternEditor)
                {
                    return true;
                }
            }

            return false;
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
            fCounter = -1;
        }

        #endregion
    }
}