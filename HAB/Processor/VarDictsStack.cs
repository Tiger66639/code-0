// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VarDictsStack.cs" company="">
//   
// </copyright>
// <summary>
//   a stack containing variable dictionaries, which tries to preserve as much
//   mem as possible by reusing the data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a stack containing variable dictionaries, which tries to preserve as much
    ///     mem as possible by reusing the data.
    /// </summary>
    public class VarDictsStack : System.Collections.Generic.IEnumerable<VarDict>
    {
        /// <summary>
        ///     Gets the nr of items currently stored.
        /// </summary>
        public int Count
        {
            get
            {
                return fCurrentPos + 1;
            }
        }

        #region IEnumerable<VarDict> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<VarDict> GetEnumerator()
        {
            for (var i = 0; i <= fCurrentPos; i++)
            {
                yield return fData[i];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be
        ///     used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (var i = 0; i <= fCurrentPos; i++)
            {
                yield return fData[i];
            }
        }

        #endregion

        /// <summary>The clear.</summary>
        /// <param name="proc">The proc.</param>
        internal void Clear(Processor proc)
        {
            for (var i = 0; i <= fCurrentPos; i++)
            {
                fData[i].Clear();
            }

            fCurrentPos = -1;
        }

        #region fields

        /// <summary>The f not used.</summary>
        private readonly System.Collections.Generic.Stack<VarValuesList> fNotUsed =
            new System.Collections.Generic.Stack<VarValuesList>();

                                                                         // stores al the lists that have already been created but which have been recycled.

        /// <summary>The f data.</summary>
        private readonly System.Collections.Generic.List<VarDict> fData = new System.Collections.Generic.List<VarDict>();

        /// <summary>The f current pos.</summary>
        private int fCurrentPos = -1;

        #endregion

        #region functions

        /// <summary>Gets the last list on the stack.</summary>
        /// <returns>The <see cref="VarDict"/>.</returns>
        internal VarDict Peek()
        {
            if (fData.Count > 0)
            {
                return fData[fCurrentPos];
            }

            return null; // if there is no more data, simply return null.
        }

        /// <summary>returns and removes the last list on the stack.</summary>
        /// <returns>The <see cref="VarDict"/>.</returns>
        internal VarDict Pop()
        {
            fData[fCurrentPos].Clear();

                // need to clear the list when popping, cause we don't do it when the list is requested.
            return fData[fCurrentPos--];
        }

        /// <summary>pops multiple items from the list at once. doesn't return the value.</summary>
        /// <param name="count"></param>
        internal void Pop(int count)
        {
            var iStart = fCurrentPos;
            while (iStart - fCurrentPos < count)
            {
                fData[fCurrentPos--].Clear();

                    // need to clear all the lists cause it isn't done when the list is requested.
            }

            fCurrentPos -= count;
        }

        /// <summary>Adds a new item on the stack and returns this value.</summary>
        /// <returns>The <see cref="VarDict"/>.</returns>
        internal VarDict Push()
        {
            VarDict iRes = null;
            if (fData.Count - 1 > fCurrentPos)
            {
                iRes = fData[++fCurrentPos];
            }
            else
            {
                iRes = new VarDict(fNotUsed);
                fData.Add(iRes);
                fCurrentPos++;
            }

            return iRes;
        }

        #endregion
    }
}