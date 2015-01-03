// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetPipe.cs" company="">
//   
// </copyright>
// <summary>
//   provides a way to send words found in the wordnet dict, to a query.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     provides a way to send words found in the wordnet dict, to a query.
    /// </summary>
    public class WordNetPipe : IQueryPipe, 
                               System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>The f cur pos.</summary>
        private int fCurPos = -1;

        /// <summary>The f data.</summary>
        private AllWordsDataTable fData;

        #region IEnumerator<IEnumerable<Neuron>> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Current
        {
            get
            {
                if (fCurPos > -1 && fCurPos < fData.Count)
                {
                    TextNeuron iRes;
                    if (TextSin.Words.TryGetNeuron(fData[fCurPos].lemma, out iRes) == false)
                    {
                        // if we can't find the textneuron, create a temp, not a full neuron, to save mem.
                        iRes = NeuronFactory.GetText(fData[fCurPos].lemma);
                        Brain.Current.MakeTemp(iRes);
                    }

                    yield return iRes;
                    PosValuesTableAdapter iWordInfo = new PosValuesTableAdapter();
                    PosValuesDataTable iData = iWordInfo.GetData(fData[fCurPos].lemma);
                    foreach (var i in iData)
                    {
                        Neuron iFound;
                        if (Brain.Current.TryFindNeuron(WordNetSin.GetPos(i.pos), out iFound))
                        {
                            yield return iFound;
                        }
                    }
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Reset();
        }

        #endregion

        #region IEnumerable<IEnumerable<Neuron>> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            return this;
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
            return this;
        }

        #endregion

        #region IForEachSource Members

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a
        ///     duplicate, its' ok to return a new <see langword="enum"/> that goes
        ///     back to the start (a warning should be rendered that splits are not
        ///     supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            var iRes = new WordNetPipe();
            iRes.fCurPos = fCurPos;
            iRes.fData = fData;
            return iRes;
        }

        #endregion

        #region IQueryPipe Members

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The <see langword="enum"/> to move passed the end.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            var iEnum = (WordNetPipe)Enum;
            iEnum.fCurPos = int.MaxValue;
        }

        #endregion

        #region IQueryPipe Members

        /// <summary>
        ///     makes certain that everything is loaded into memory.
        /// </summary>
        public void TouchMem()
        {
            // nothing to do.
        }

        /// <summary>
        ///     called when extra data needs to be saved to disk in seperate files.
        /// </summary>
        public void Flush()
        {
            // nothing to flush.
        }

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        public void WriteV1(System.IO.BinaryWriter writer)
        {
            // nothing to write
        }

        /// <summary>read the settings from a stream.</summary>
        /// <param name="iTempReader"></param>
        public void ReadV1(System.IO.BinaryReader iTempReader)
        {
            // nothing to read.
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
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
            if (fData == null)
            {
                AllWordsTableAdapter iAdapter = new AllWordsTableAdapter();
                fData = iAdapter.GetData();
            }

            fCurPos++;
            return fCurPos < fData.Count;
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
            fCurPos = -1;
            fData = null;
        }

        #endregion
    }
}