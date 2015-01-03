// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerPipe.cs" company="">
//   
// </copyright>
// <summary>
//   a pipe that provides a way to walk through all (or a subsection of) the
//   neurons in the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     a pipe that provides a way to walk through all (or a subsection of) the
    ///     neurons in the database.
    /// </summary>
    public class ExplorerPipe : IQueryPipe
    {
        ///// <summary>
        ///// when different from 0, used to filter on the type of neuron: textneuron, double, int, cluster.
        ///// </summary>
        // public ulong TypeFilter { get; set; }

        /// <summary>
        ///     when different from 0, used to determin the lower boundery to start
        ///     feeding neurons to the query.
        /// </summary>
        public ulong LowerRange { get; set; }

        /// <summary>
        ///     when different from 0, used to determin the upper boundery, or when to
        ///     stop sending neurons to the query.
        /// </summary>
        public ulong UpperRange { get; set; }

        #region IEnumerable<IEnumerable<Neuron>> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            return new ExplorerPipeEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>The get enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ExplorerPipeEnumerator(this);
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
            return new ExplorerPipeEnumerator((ExplorerPipeEnumerator)Enum);
        }

        #endregion

        #region IQueryPipe Members

        /// <summary>The goto end.</summary>
        /// <param name="Enum">The enum.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            ((ExplorerPipeEnumerator)Enum).CurrentIndex = ulong.MaxValue;
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
            // nothing to do.
        }

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        public void WriteV1(System.IO.BinaryWriter writer)
        {
            writer.Write((System.UInt16)1); // version type.
            writer.Write(LowerRange);
            writer.Write(UpperRange);
        }

        /// <summary>read the settings from a stream.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadV1(System.IO.BinaryReader reader)
        {
            if (reader.ReadUInt16() == 1)
            {
                LowerRange = reader.ReadUInt64();
                UpperRange = reader.ReadUInt64();
            }
            else
            {
                throw new System.InvalidOperationException("Invalid version found for the explorer-pipe's settings");
            }
        }

        #endregion
    }

    /// <summary>
    ///     an enumarotor for the <see cref="ExplorerPipe" /> . Does the actual
    ///     traversing of the data.
    /// </summary>
    public class ExplorerPipeEnumerator :
        System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>The f cur value.</summary>
        private Neuron fCurValue;

        /// <summary>The f owner.</summary>
        private ExplorerPipe fOwner;

        /// <summary>Initializes a new instance of the <see cref="ExplorerPipeEnumerator"/> class. Initializes a new instance of the<see cref="ExplorerPipeEnumerator"/> class.</summary>
        /// <param name="owner">The owner.</param>
        internal ExplorerPipeEnumerator(ExplorerPipe owner)
        {
            fOwner = owner;
            CurrentIndex = owner.LowerRange - 1;

                // always -1, cause the first call is moveNext, to go to the first element, which is at lowerrange
        }

        /// <summary>Initializes a new instance of the <see cref="ExplorerPipeEnumerator"/> class. Initializes a new instance of the<see cref="ExplorerPipeEnumerator"/> class.</summary>
        /// <param name="source">The source.</param>
        internal ExplorerPipeEnumerator(ExplorerPipeEnumerator source)
        {
            if (source != null)
            {
                CurrentIndex = source.CurrentIndex;
                fCurValue = source.fCurValue;
                fOwner = source.fOwner;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Gets or sets the current index.</summary>
        public ulong CurrentIndex { get; set; }

        #region IEnumerator<IEnumerable<Neuron>> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Current
        {
            get
            {
                yield return fCurValue;
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
            fOwner = null;
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
            var iLoop = true;
            while (iLoop)
            {
                CurrentIndex++;
                if ((fOwner.UpperRange > fOwner.LowerRange && CurrentIndex > fOwner.UpperRange)
                    || CurrentIndex > Brain.Current.NextID)
                {
                    // need to stop when out of range or when passed endpoint.
                    return false;
                }

                if (Brain.Current.TryFindNeuron(CurrentIndex, out fCurValue))
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
            CurrentIndex = fOwner.LowerRange;
            fCurValue = null;
        }

        #endregion
    }
}