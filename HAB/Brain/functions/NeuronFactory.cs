// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Use this class to create various types of neurons. This allows the system to recycle (garbage) disposed objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Use this class to create various types of neurons. This allows the system to recycle (garbage) disposed objects.
    /// </summary>
    public class NeuronFactory
    {
        /// <summary>
        ///     max nr of items that a processor buffers for a single type.
        /// </summary>
        private const int MAXPTHREADBUFFER = 100;

        /// <summary>
        ///     the maximum nr of items that get buffered per kind, anything more gets removed.
        /// </summary>
        private const int MAXTOTALBUFFERPERTYPE = 10000;

        /// <summary>
        ///     contains all the available neurons that can be reused. They have already been stored in lists that can be
        ///     dispatched to the different processors (or the global one).
        ///     Each list in the queue is used by a processor as a thread local reserve.
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Queue<System.Collections.Generic.Queue<Neuron>>> fAvailable =
                    new System.Collections.Generic.Dictionary
                        <System.Type, System.Collections.Generic.Queue<System.Collections.Generic.Queue<Neuron>>>();

        /// <summary>The f default.</summary>
        [System.ThreadStatic]
        private static NeuronFactory fDefault;

        /// <summary>
        ///     per type, a queue of neurons that can be reused if there is no processor available, so a general pool.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Queue<Neuron>> fBuffer = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Queue<Neuron>>();

        /// <summary>
        ///     The entry point for the factory.
        /// </summary>
        public static NeuronFactory Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new NeuronFactory();
                }

                return fDefault;
            }
        }

        /// <summary>gets a regular neuron.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetNeuron()
        {
            var iRes = Default.GetNeuron(typeof(Neuron));
            if (iRes != null)
            {
                return iRes;
            }

            return new Neuron();
        }

        /// <summary>Gets an int neuron.</summary>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="IntNeuron"/>.</returns>
        public static IntNeuron GetInt(int val = 0)
        {
            var iRes = (IntNeuron)Default.GetNeuron(typeof(IntNeuron));
            if (iRes != null)
            {
                iRes.InitValue(val);
                return iRes;
            }

            return new IntNeuron(val);
        }

        /// <summary>gets a cluster.</summary>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster GetCluster()
        {
            var iRes = Default.GetNeuron(typeof(NeuronCluster));
            if (iRes != null)
            {
                return (NeuronCluster)iRes;
            }

            return new NeuronCluster();
        }

        /// <summary>Gets a double.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="DoubleNeuron"/>.</returns>
        public static DoubleNeuron GetDouble(double value = 0)
        {
            var iRes = (DoubleNeuron)Default.GetNeuron(typeof(DoubleNeuron));
            if (iRes != null)
            {
                iRes.SetInitValue(value);
                return iRes;
            }

            return new DoubleNeuron(value);
        }

        /// <summary>Gets a double.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        public static TextNeuron GetText(string value)
        {
            var iRes = (TextNeuron)Default.GetNeuron(typeof(TextNeuron));
            if (iRes != null)
            {
                iRes.SetInitValue(value);
                return iRes;
            }

            return new TextNeuron(value);
        }

        /// <summary>Gets a neuron of specified type.</summary>
        /// <param name="typeOfNeuron"></param>
        /// <returns>The <see cref="T"/>.</returns>
        public static T Get<T>() where T : Neuron
        {
            Neuron iRes = Default.GetNeuron(typeof(T)) as T;
            if (iRes != null)
            {
                return (T)iRes;
            }

            // return new T();
            return (T)System.Activator.CreateInstance(typeof(T), true);
        }

        /// <summary>The get neuron.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetNeuron(System.Type type)
        {
            System.Collections.Generic.Queue<Neuron> iItems = null;
            if (fBuffer.TryGetValue(type, out iItems) == false || iItems.Count == 0)
            {
                // try to get a new list from the neuron factory.
                lock (fAvailable)
                {
                    System.Collections.Generic.Queue<System.Collections.Generic.Queue<Neuron>> iReserve;
                    if (fAvailable.TryGetValue(type, out iReserve) && iReserve.Count > 0)
                    {
                        iItems = iReserve.Dequeue();
                        fBuffer[type] = iItems;
                    }
                }
            }

            if (iItems != null && iItems.Count > 0)
            {
                return iItems.Dequeue();
            }

            return null;
        }

        /// <summary>Gets a neuron of specified type.</summary>
        /// <param name="neuronType">The neuron Type.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron Get(System.Type neuronType)
        {
            var iRes = Default.GetNeuron(neuronType);
            if (iRes != null)
            {
                return iRes;
            }

            return (Neuron)System.Activator.CreateInstance(neuronType, true);
        }

        /// <summary>collects the neuron for recycling.</summary>
        /// <param name="toAdd">To add.</param>
        /// <returns>True if the item was recycled, false if it was disgarded (cause there were already to many items buffered).</returns>
        public static bool Recycle(Neuron toAdd)
        {
            // don't try to put in the buffer list. this functio is always called from the GC thread, so it can't create new objects.
            lock (fAvailable)
            {
                // need to lock, cause in server mode, each core on the processor can have it's own gc thread.
                System.Collections.Generic.Queue<System.Collections.Generic.Queue<Neuron>> iGlobal;
                System.Collections.Generic.Queue<Neuron> iBag = null;
                if (fAvailable.TryGetValue(toAdd.GetType(), out iGlobal) == false)
                {
                    iGlobal =
                        new System.Collections.Generic.Queue<System.Collections.Generic.Queue<Neuron>>(MAXPTHREADBUFFER);
                    fAvailable.Add(toAdd.GetType(), iGlobal);
                }

                if (iGlobal.Count > 0)
                {
                    iBag = Enumerable.Last(iGlobal); // we add at the end.
                }

                if (iBag == null || iBag.Count >= MAXPTHREADBUFFER)
                {
                    iBag = new System.Collections.Generic.Queue<Neuron>(MAXPTHREADBUFFER);
                    iGlobal.Enqueue(iBag);
                }

                if (((iGlobal.Count - 1) * MAXPTHREADBUFFER) + iBag.Count < MAXTOTALBUFFERPERTYPE)
                {
                    iBag.Enqueue(toAdd);
                    return true;
                }

                return false;
            }
        }

        /// <summary>The clear.</summary>
        internal static void Clear()
        {
            if (fAvailable != null)
            {
                fAvailable.Clear();
            }

            if (fDefault != null && fDefault.fBuffer != null)
            {
                fDefault.fBuffer.Clear();
            }
        }
    }
}