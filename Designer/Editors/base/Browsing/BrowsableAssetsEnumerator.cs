﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowsableAssetsEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   The browsable assets enumerator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The browsable assets enumerator.</summary>
    public class BrowsableAssetsEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        #region fields

        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BrowsableAssetsEnumerator"/> class. Initializes a new instance of the<see cref="BrowsableOutputPatternEditorsEnumerator"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public BrowsableAssetsEnumerator(EditorCollection owner)
        {
            Owner = owner;
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
                    if (i is ObjectEditor)
                    {
                        return true;
                    }

                    if (i is EditorFolder)
                    {
                        return ((EditorFolder)i).ContainsAssets;
                    }
                }

                return false;
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
            return new BrowsableAssetsEnumerator(Owner);
        }

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
                if (iItem is ObjectEditor)
                {
                    return ((ObjectEditor)iItem).BrowsableItems;
                }

                if (iItem is IEditorsOwner)
                {
                    return ((IEditorsOwner)iItem).Editors.BrowsableAssets;
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
                if ((Owner[fCounter] is EditorFolder && ((EditorFolder)Owner[fCounter]).ContainsAssets)
                    || Owner[fCounter] is ObjectEditor)
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