﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClustersSelectSource.cs" company="">
//   
// </copyright>
// <summary>
//   walks through the list of clusters that own a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     walks through the list of clusters that own a neuron.
    /// </summary>
    public class ClustersSelectSource : IForEachSource, 
                                        System.Collections.Generic.IEnumerator
                                            <System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>The f count.</summary>
        private int fCount = -1;

        /// <summary>The f current.</summary>
        private Neuron fCurrent;

        /// <summary>The f source.</summary>
        private RefCountedList<ulong> fSource;

        /// <summary>Initializes a new instance of the <see cref="ClustersSelectSource"/> class. Initializes a new instance of the <see cref="ClustersSelectSource"/>
        ///     class.</summary>
        /// <param name="source">The source.</param>
        public ClustersSelectSource(Neuron source)
        {
            fSource = new RefCountedList<ulong>();
            fSource.RefCount = 1;
            if (source.ClusteredByIdentifier != null)
            {
                using (var iList = source.ClusteredBy)
                {
                    fSource.Source = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);
                    fSource.Source.AddRange(iList);
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ClustersSelectSource"/> class. Prevents a default instance of the <see cref="ClustersSelectSource"/>
        ///     class from being created. for duplicate.</summary>
        /// <param name="count">The count.</param>
        /// <param name="source">The source.</param>
        private ClustersSelectSource(int count, RefCountedList<ulong> source)
        {
            fCount = count;
            fSource = source;
            lock (fSource) fSource.RefCount++;
        }

        #region IEnumerator<IEnumerable<Neuron>> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Current
        {
            get
            {
                yield return fCurrent;
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
            lock (fSource)
            {
                fSource.RefCount--;
                if (fSource.RefCount == 0 && Processor.CurrentProcessor != null)
                {
                    Factories.Default.IDLists.Recycle(fSource.Source);
                    fSource = null;
                }
            }
        }

        #endregion

        #region IForEachSource Members

        /// <summary>Gets the enumerator that can be used to get the data source items.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            return this;
        }

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a
        ///     duplicate, its' ok to return a new <see langword="enum"/> that goes
        ///     back to the start (a warning should be rendered that splits are not
        ///     supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            return new ClustersSelectSource(fCount, fSource);
        }

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The <see langword="enum"/> to move passed the end.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            fCount = fSource.Source.Count;
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
            fCount++;
            if (fCount < fSource.Source.Count)
            {
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(fSource.Source[fCount], out iFound))
                {
                    fCurrent = iFound;
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
            fCount = -1;
        }

        #endregion
    }
}