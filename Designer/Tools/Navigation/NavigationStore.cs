// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationStore.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a data store for  navigation (forward and backward) through the data of a document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Tools.Navigation
{
    /// <summary>Provides a data store for  navigation (forward and backward) through the data of a document.</summary>
    /// <typeparam name="T">The data format of the storage point</typeparam>
    public class NavigationStore<T> : Data.ObservableObject
    {
        #region Fields

        /// <summary>The f list.</summary>
        private readonly System.Collections.Generic.List<T> fList = new System.Collections.Generic.List<T>();

        /// <summary>The f go back point.</summary>
        private int fGoBackPoint;

                    // in case we did a step back, we store the last back point here. This allows us to go forward again, or do a cut off, when a new item is added.

        /// <summary>
        ///     The maximum number of flows that need to be stored in the selection history (to go back to previously selected
        ///     flows).
        /// </summary>
        private const int MAXSELECTIONHISTORY = 255;

        #endregion

        #region Prop

        #region CanNavigateForward

        /// <summary>
        ///     Gets the value that indicates if it is possible to navigate forward.
        /// </summary>
        public bool CanNavigateForward
        {
            get
            {
                return fList.Count > 0 && fGoBackPoint < fList.Count;
            }
        }

        #endregion

        #region CanNavigateBackward

        /// <summary>
        ///     Gets the value that indicates if it is possible to navigate backward.
        /// </summary>
        public bool CanNavigateBackward
        {
            get
            {
                return fList.Count > 0 && fGoBackPoint > 1;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Removes any reference to the specified storage point.</summary>
        /// <param name="toRemove">To remove.</param>
        public void Remove(T toRemove)
        {
            fList.RemoveAll(p => p.Equals(toRemove));
        }

        /// <summary>
        ///     Clears all the storage points
        /// </summary>
        public void Clear()
        {
            fGoBackPoint = 0;
            fList.Clear();
        }

        /// <summary>Adds the specified storage point and makes certain that the current navigation point is back at the end.</summary>
        /// <param name="toAdd">To add.</param>
        internal void Add(T toAdd)
        {
            while (fList.Count > fGoBackPoint)
            {
                // if we did a go-back, we first need to remove all the items that we skipped from the top, so that the goback for the next time is correct.
                fList.RemoveAt(fList.Count - 1);
            }

            fList.Add(toAdd);
            while (fList.Count > MAXSELECTIONHISTORY)
            {
                // make certain that the selection history list doesn't overflow.
                fList.RemoveAt(0);
            }

            fGoBackPoint = fList.Count;
        }

        /// <summary>Navigates forward.</summary>
        /// <returns>The <see cref="T"/>.</returns>
        public T NavigateForward()
        {
            fGoBackPoint++;
            if (fGoBackPoint < fList.Count - 1)
            {
                return fList[fGoBackPoint];
            }

            fGoBackPoint = fList.Count; // need to indicate that we are back at the front of the list.
            return fList[fGoBackPoint - 1];
        }

        /// <summary>Navigates backward.</summary>
        /// <returns>The <see cref="T"/>.</returns>
        public T NavigateBackward()
        {
            if (fGoBackPoint != fList.Count)
            {
                fGoBackPoint--;
            }
            else
            {
                fGoBackPoint -= 2;
            }

            return fList[fGoBackPoint];
        }

        #endregion
    }
}