// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentsStack.cs" company="">
//   
// </copyright>
// <summary>
//   a stack implementation that uses as little as possible memory by reusing
//   the data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a stack implementation that uses as little as possible memory by reusing
    ///     the data.
    /// </summary>
    internal class ArgumentsStack
    {
        /// <summary>
        ///     Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return fData.Count;
            }
        }

        /// <summary>
        ///     reverses the list at the current pos with the list at the previous
        ///     pos. this is used to solve <see cref="ResultStatement" /> s whose
        ///     solve is called after the result list is pushed on the stack, cause
        ///     the instructions expect the last value on the stack to be the result
        ///     list. But the resultStatemnt also uses the arg-stack to calculate it's
        ///     argument values, a list that can't be re-used until the instruction is
        ///     done. To solve this, the result list and argument list are switched on
        ///     the stack during the call. A possibly better solution would be to also
        ///     use the stack for passing along the arguments for the instruction.
        /// </summary>
        internal void Reverse()
        {
            if (fCurrentPos > 0)
            {
                var iPrev = fData[fCurrentPos - 1];
                fData[fCurrentPos - 1] = fData[fCurrentPos];
                fData[fCurrentPos] = iPrev;
            }
            else
            {
                throw new System.InvalidOperationException(
                    "not enough values on the argument stack to perform a switch.");
            }
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fCurrentPos = -1;
        }

        #region MyRegion

        /// <summary>The f data.</summary>
        private readonly System.Collections.Generic.List<System.Collections.Generic.List<Neuron>> fData =
            new System.Collections.Generic.List<System.Collections.Generic.List<Neuron>>();

        /// <summary>The f current pos.</summary>
        private volatile int fCurrentPos = -1;

        #endregion

        #region functions

        /// <summary>Gets the last list on the stack.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<Neuron> Peek()
        {
            return fData[fCurrentPos];
        }

        /// <summary>removes and recycles the last list on the stack.</summary>
        internal void Pop()
        {
            if (fCurrentPos >= 0)
            {
                // make certain that we don't corrupt the thing.
                var iData = fData[fCurrentPos];
                fData[fCurrentPos] = null;
                fCurrentPos--;
                Factories.Default.NLists.Recycle(iData);
            }
            else
            {
                LogService.Log.LogWarning("Argument stack", "possible stac corruption");
            }
        }

        /// <summary>pops multiple items from the list at once and recycles all the itesm.
        ///     doesn't return the value.</summary>
        /// <param name="count"></param>
        internal void Pop(int count)
        {
            var iMemFac = Factories.Default;
            while (count > 0 && fCurrentPos >= 0)
            {
                // for some reason, during big, big runs, the count can be bigger then currentPos (shouldn't be, but it has something to do with context switches.
                var iData = fData[fCurrentPos];
                fData[fCurrentPos] = null;
                fCurrentPos--;
                iMemFac.NLists.Recycle(iData);
                count--;
            }

            if (count > 0)
            {
                LogService.Log.LogWarning("Argument stack", "possible stac corruption");
            }
        }

        /// <summary>
        ///     removes the last item from the stack, but doesn't recycle the list.
        /// </summary>
        internal void Release()
        {
            if (fCurrentPos >= 0)
            {
                fCurrentPos--;
            }
            else
            {
                LogService.Log.LogWarning("Argument stack", "possible stac corruption");
            }
        }

        /// <summary>Adds a new item on the stack and returns this value.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<Neuron> Push()
        {
            var iRes = Factories.Default.NLists.GetBuffer();
            if (fData.Count - 1 > fCurrentPos)
            {
                fData[++fCurrentPos] = iRes;
            }
            else
            {
                fData.Add(iRes);
                fCurrentPos++;
            }

            return iRes;
        }

        /// <summary>pushes the specified <paramref name="list"/> on the argument stack.</summary>
        /// <param name="list"></param>
        internal void Push(System.Collections.Generic.List<Neuron> list)
        {
            if (fData.Count - 1 > fCurrentPos)
            {
                fData[++fCurrentPos] = list;
            }
            else
            {
                fData.Add(list);
                fCurrentPos++;
            }
        }

        #endregion
    }
}