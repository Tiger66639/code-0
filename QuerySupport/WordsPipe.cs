// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordsPipe.cs" company="">
//   
// </copyright>
// <summary>
//   provides a way to loop through all the textneurons that were registered
//   with the <see cref="JaStDev.HAB.TextSin.Words" /> dict.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     provides a way to loop through all the textneurons that were registered
    ///     with the <see cref="JaStDev.HAB.TextSin.Words" /> dict.
    /// </summary>
    public class WordsPipe : IQueryPipe
    {
        #region IEnumerable<Neuron> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            return new WordsPipeEnum();
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
            return new WordsPipeEnum();
        }

        #endregion

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a
        ///     duplicate, its' ok to return a new <see langword="enum"/> that goes
        ///     back to the start (a warning should be rendered that splits are not
        ///     supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            return new WordsPipeEnum((WordsPipeEnum)Enum);
        }

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The <see langword="enum"/> to move passed the end.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            var iEnum = (WordsPipeEnum)Enum;
            iEnum.CurrentIndex = iEnum.Source.Count + 1;
        }

        #region IQueryPipe Members

        /// <summary>
        ///     makes certain that everything is loaded into memory.
        /// </summary>
        public void TouchMem()
        {
            // nothing to load
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
            // nothing to write.
        }

        /// <summary>read the settings from a stream.</summary>
        /// <param name="iTempReader"></param>
        public void ReadV1(System.IO.BinaryReader iTempReader)
        {
            // nothing to read.
        }

        #endregion
    }

    /// <summary>
    ///     an enumerator for the wordsdicct, that makes certain it's converted into
    ///     a neuron.
    /// </summary>
    public class WordsPipeEnum : System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>The f cur neuron.</summary>
        private Neuron fCurNeuron;

        /// <summary>The f current.</summary>
        private int fCurrent = -1;

        /// <summary>Initializes a new instance of the <see cref="WordsPipeEnum"/> class.</summary>
        /// <param name="source">The source.</param>
        internal WordsPipeEnum(WordsPipeEnum source)
        {
            fCurrent = source.fCurrent;
            fCurNeuron = source.fCurNeuron;
            Source = source.Source;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WordsPipeEnum" /> class.
        /// </summary>
        internal WordsPipeEnum()
        {
            Source = Enumerable.ToList(TextSin.Words);

                // make a local copy, so we can change the content of the dict (create new textneurons) while walking through the items.
        }

        /// <summary>Gets the source.</summary>
        public System.Collections.Generic.List<ulong> Source { get; private set; }

        /// <summary>Gets or sets the current index.</summary>
        public int CurrentIndex
        {
            get
            {
                return fCurrent;
            }

            set
            {
                fCurrent = value;
            }
        }

        #region IEnumerator<Neuron> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Current
        {
            get
            {
                yield return fCurNeuron;
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
            if (Processor.CurrentProcessor != null)
            {
                Source = null;
            }

            fCurrent = -1;
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
            if (Source != null)
            {
                fCurrent++;
                if (Source.Count <= fCurrent)
                {
                    return false;
                }

                while (Brain.Current.TryFindNeuron(Source[fCurrent], out fCurNeuron) == false)
                {
                    // make certain that the item can be found. If this is not the case, move on to the next id.
                    fCurrent++;
                    if (Source.Count <= fCurrent)
                    {
                        return false;
                    }
                }

                return true;
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
            fCurNeuron = null;
            fCurrent = -1;
        }

        #endregion
    }
}