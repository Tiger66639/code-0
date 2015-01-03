// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   objects that manage memory for a processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     objects that manage memory for a processor.
    /// </summary>
    public class MemoryFactory
    {
        #region ArgumentStack

        /// <summary>
        ///     Gets the stack used to store the list of arguments that need to be filled by the instructions.
        /// </summary>
        /// <remarks>This field is so intensely used that direct access is provided (a little faster)</remarks>
        internal ArgumentsStack ArgumentStack = new ArgumentsStack();

        #endregion

        /// <summary>The global values.</summary>
        internal System.Collections.Generic.Dictionary<Global, System.Collections.Generic.List<Neuron>> GlobalValues =
            new System.Collections.Generic.Dictionary<Global, System.Collections.Generic.List<Neuron>>();

        /// <summary>
        ///     some clusters can return a value when called, this is where the processor temporarily stors the last return result
        ///     so that the statement that called the function can pick it up again and return this value.
        ///     this field should usually be null, except between a call to the 'return' instruction and using (or clearing again)
        ///     this value.
        ///     this field isn't required to be duplicated.
        /// </summary>
        public System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>> LastReturnValues =
            new System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>>();

        /// <summary>
        ///     Manages locks and lists of locks, so that they can be reused. This is done to get presure of the gc.
        /// </summary>
        internal LockRequestsFactory LocksFactory = new LockRequestsFactory();

        /// <summary>The parameters stack.</summary>
        public System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>> ParametersStack =
            new System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>>();

        #region VariableValues

        /// <summary>
        ///     Gets the stack of dictionaries containing all the current values for the variables.
        /// </summary>
        /// <remarks>
        ///     Providing direct access to the var, is used a lot, direct access is a little faster.
        /// </remarks>
        public VarDictsStack VariableValues = new VarDictsStack(); // volatile

        #endregion

        /// <summary>clears the mem. Should be done outside fo the 'ffreeMem' lock, cause that is faster.</summary>
        /// <param name="proc"></param>
        internal void Clear(Processor proc)
        {
            ArgumentStack.Clear();
            VariableValues.Clear(proc);
            var iMemFac = Factories.Default;
            foreach (var i in GlobalValues)
            {
                if (i.Key.SplitReactionID != (ulong)PredefinedNeurons.shared)
                {
                    // don't recycle the content of shared vars, that could cause problems, simply let the gc handle this.
                    iMemFac.NLists.Recycle(i.Value, false);
                }
            }

            GlobalValues.Clear();
            foreach (var i in ParametersStack)
            {
                // make certain that any left-overs are recycled
                iMemFac.NLists.Recycle(i, false);
            }

            ParametersStack.Clear();
            foreach (var i in LastReturnValues)
            {
                iMemFac.NLists.Recycle(i, false);
            }

            LastReturnValues.Clear();
        }
    }

    /// <summary>
    ///     This class is responsible for creating 'root' processors for the core (so not in response of a split).
    ///     It is used by various <see cref="Sin" /> types like the <see cref="ImageSin" /> or the <see cref="NeuralTimer" />.
    ///     Use the <see cref="ProcessorFactory.Factory" /> to get to the object.  You can assign a custom factory if you
    ///     have overwritten the <see cref="Processor" /> class.
    /// </summary>
    public class ProcessorFactory : IProcessorFactory
    {
        #region Prop

        #region Factory

        /// <summary>
        ///     Gets/sets the factory that should be used for creating processors. Use this to determin
        ///     which type of debug processors or other type of processors will be used when a time tick is passed.
        /// </summary>
        /// <remarks>
        ///     The default value, is this object itself.  This uses normall processors.
        /// </remarks>
        public static IProcessorFactory Factory
        {
            get
            {
                if (fFactory == null)
                {
                    fFactory = new ProcessorFactory();
                }

                return fFactory;
            }

            set
            {
                fFactory = value;
            }
        }

        #endregion

        #endregion

        #region test

        /// <summary>The debug print nlist usage.</summary>
        public static void DebugPrintNlistUsage()
        {
            foreach (var i in fFreeMem)
            {
                System.Diagnostics.Debug.Print(Factories.Default.NLists.BufferCount.ToString());
            }
        }

        #endregion

        #region fields

        /// <summary>
        ///     Creates <see cref="Processor" /> objects. Required to provide debugging fascilities.
        /// </summary>
        private static IProcessorFactory fFactory;

        /// <summary>
        ///     stores all the processors that can be reused (no longer active and collected for recycling).
        /// </summary>
        private static readonly System.Collections.Generic.Stack<MemoryFactory> fFreeMem =
            new System.Collections.Generic.Stack<MemoryFactory>();

        #endregion

        #region Functions

        /// <summary>Gets the processor. This is part of the IProcessorFactory interface.
        ///     Don't use this to start a new processor directly, instead use the static function,
        ///     this will try to recycle processors, which is better for mem.
        ///     This is the backup function (from IPocessorFactory, in case no other was specified).</summary>
        /// <returns>The <see cref="Processor"/>.</returns>
        public Processor CreateProcessor()
        {
            return new Processor();
        }

        /// <summary>
        ///     Use this function to get a new root-processor.
        /// </summary>
        /// <returns>an activated processor</returns>
        public static Processor GetProcessor()
        {
            Processor iRes;
            MemoryFactory iMem = null;
            iRes = Factory.CreateProcessor();
            lock (fFreeMem)
            {
                if (fFreeMem.Count > 0)
                {
                    iMem = fFreeMem.Pop();
                }
            }

            if (iMem == null)
            {
                iMem = new MemoryFactory();
            }
            else
            {
                iMem.Clear(iRes); // it's a lot faster if this is done outside of the lock.
            }

            iRes.Mem = iMem;
            Factory.ActivateProc(iRes);
            return iRes;
        }

        /// <summary>
        ///     Used by the brain to get a processor during a split instruction.
        /// </summary>
        /// <returns>A non activated processor</returns>
        public static Processor GetSubProcessor()
        {
            Processor iRes = null;
            MemoryFactory iMem = null;
            iRes = Factory.CreateProcessor();
            lock (fFreeMem)
            {
                if (fFreeMem.Count > 0)
                {
                    iMem = fFreeMem.Pop();
                    iMem.Clear(iRes);
                }
            }

            if (iMem == null)
            {
                iMem = new MemoryFactory();
            }

            iRes.Mem = iMem;
            return iRes;
        }

        /// <summary>Recycles the specified processor. When the processor was part of a split,
        ///     make certain that only the head is recycled. The rest will be recycled automaticallly.</summary>
        /// <param name="proc">The proc.</param>
        internal static void Recycle(Processor proc)
        {
            proc.Mem.Clear(proc);

                // do a clear in the thread that stops, this way, when it gets re-used,  a core can't accidentally have old buffer values, but all have been cleared.
            lock (fFreeMem) fFreeMem.Push(proc.Mem);
            proc.Mem = null; // remove refs as soon as possible.
        }

        /// <summary>Called when a processor is about to be started. This is always called after a<see cref="IProcessorFactory.CreateProcessor"/>
        ///     was called, but can also be called at other times, when a processor gets reused.
        ///     nothing required.</summary>
        /// <param name="proc"></param>
        public void ActivateProc(Processor proc)
        {
        }

        #endregion
    }
}