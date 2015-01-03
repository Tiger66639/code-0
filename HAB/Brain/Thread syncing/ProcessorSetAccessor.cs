// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorSetAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   An accessor for has sets containing processors of a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An accessor for has sets containing processors of a neuron.
    /// </summary>
    public class ProcessorSetAccessor : SetAccessor<Processor>
    {
        /// <summary>Initializes a new instance of the <see cref="ProcessorSetAccessor"/> class.</summary>
        internal ProcessorSetAccessor()
        {
            Level = LockLevel.Processors;
        }

        /// <summary>Initializes a new instance of the <see cref="ProcessorSetAccessor"/> class. Initializes a new instance of the <see cref="ProcessorSetAccessor"/>
        ///     class.</summary>
        /// <param name="set">The set.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">if <paramref name="set"/> to <c>true</c> [writeable].</param>
        public ProcessorSetAccessor(System.Collections.Generic.HashSet<Processor> set, Neuron neuron, bool writeable)
            : base(set, neuron, LockLevel.Processors, writeable)
        {
        }

        /// <summary>Adds the specified <paramref name="processor"/> to the data set.</summary>
        /// <param name="processor">The processor.</param>
        public void Add(Processor processor)
        {
            Lock();
            try
            {
                var iSet = Set;
                if (iSet.Contains(processor) == false)
                {
                    iSet.Add(processor);
                    processor.AddFrozenNeuron(Neuron);

                        // when a processor is added to a neuron, the processor also needs to know that the neuron is frozen.
                }
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>The add unsafe.</summary>
        /// <param name="processor">The processor.</param>
        internal void AddUnsafe(Processor processor)
        {
            var iSet = Set;
            if (iSet.Contains(processor) == false)
            {
                iSet.Add(processor);
                processor.AddFrozenNeuron(Neuron);

                    // when a processor is added to a neuron, the processor also needs to know that the neuron is frozen.
            }
        }

        /// <summary>Removes the specified processor.</summary>
        /// <param name="processor">The processor.</param>
        internal void Remove(Processor processor)
        {
            Lock();
            try
            {
                Set.Remove(processor);
                processor.RemoveFrozenNeuron(Neuron);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>The remove unsafe.</summary>
        /// <param name="processor">The processor.</param>
        internal void RemoveUnsafe(Processor processor)
        {
            Set.Remove(processor);
            processor.RemoveFrozenNeuron(Neuron);
        }

        /// <summary>Removes the <paramref name="processor"/> without changing the
        ///     processor. This is used by the <paramref name="processor"/> to clear
        ///     out the list.</summary>
        /// <param name="processor">The processor.</param>
        internal void RemoveDirect(Processor processor)
        {
            Set.Remove(processor);
        }

        /// <summary>Adds the <paramref name="processor"/> without changing the processor.
        ///     This is used by the <paramref name="processor"/> to move the data to
        ///     the last <paramref name="processor"/> in a split.</summary>
        /// <param name="processor">The processor.</param>
        internal void AddDirect(Processor processor)
        {
            if (Set.Contains(processor) == false)
            {
                Set.Add(processor);
            }
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        internal void Clear()
        {
            Lock();
            try
            {
                foreach (var i in Set)
                {
                    i.RemoveFrozenNeuron(Neuron);
                }

                Set.Clear();
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            UnlockNoIsChanged();
            Factories.Default.FrozenForAccFactory.Release(this);
        }
    }
}