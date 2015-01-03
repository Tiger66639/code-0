// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcAcessorsFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The proc acessors factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The proc acessors factory.</summary>
    public class ProcAcessorsFactory
    {
        /// <summary>The maxnrrecycledlists.</summary>
        private const int MAXNRRECYCLEDLISTS = 10;

        /// <summary>
        ///     contains all the available Objects that can be reused. They have
        ///     already been stored in lists that can be dispatched to the different
        ///     threads.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<ProcessorSetAccessor>> fAvailable = new System.Collections.Generic.Queue<System.Collections.Generic.Queue<ProcessorSetAccessor>>();

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<ProcessorSetAccessor> fBuffer;

        /// <summary>Gets the call frame.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        /// <returns>The <see cref="ProcessorSetAccessor"/>.</returns>
        public ProcessorSetAccessor Get(System.Collections.Generic.HashSet<Processor> list, 
            Neuron neuron, 
            bool writeable)
        {
            if (fBuffer == null || fBuffer.Count == 0)
            {
                lock (fAvailable)
                {
                    if (fAvailable.Count > 0)
                    {
                        fBuffer = fAvailable.Dequeue();
                    }
                }
            }

            ProcessorSetAccessor iRes;
            if (fBuffer != null && fBuffer.Count > 0)
            {
                iRes = fBuffer.Dequeue();
            }
            else
            {
                iRes = new ProcessorSetAccessor();
            }

            iRes.Set = list;
            iRes.Neuron = neuron;
            iRes.IsWriteable = writeable;
            return iRes;
        }

        /// <summary>Releases the specified to callframe.</summary>
        /// <param name="toRelease">To release.</param>
        public void Release(ProcessorSetAccessor toRelease)
        {
            if (fBuffer == null)
            {
                fBuffer = new System.Collections.Generic.Queue<ProcessorSetAccessor>(MAXNRRECYCLEDLISTS);
            }
            else if (fBuffer.Count >= MAXNRRECYCLEDLISTS)
            {
                lock (fAvailable) fAvailable.Enqueue(fBuffer);
                fBuffer = new System.Collections.Generic.Queue<ProcessorSetAccessor>(MAXNRRECYCLEDLISTS);
            }

            fBuffer.Enqueue(toRelease);
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fBuffer = null;
            fAvailable.Clear();
        }
    }
}