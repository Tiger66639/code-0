// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Sorts the values of the first argument, which must be a variable. The
//   second argument can possibly (not required) be a cluster that serves as a
//   callback. This cluster should return an <see langword="int" /> to determin
//   bigger, smaller or equal value. The 2 values to be compared, are passed
//   in as function arguments. Note that it is not allowed to perform a split
//   in this callback. arg 1: a variable (not required to be a byref, can
//   simply be a variable) 2: optionally a callback cluster to determin order.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sorts the values of the first argument, which must be a variable. The
    ///     second argument can possibly (not required) be a cluster that serves as a
    ///     callback. This cluster should return an <see langword="int" /> to determin
    ///     bigger, smaller or equal value. The 2 values to be compared, are passed
    ///     in as function arguments. Note that it is not allowed to perform a split
    ///     in this callback. arg 1: a variable (not required to be a byref, can
    ///     simply be a variable) 2: optionally a callback cluster to determin order.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.SortInstruction)]
    public class SortInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SortInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SortInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without any
        ///     specific number of values.
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        #region IExecStatement Members

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count >= 1)
            {
                Variable iSource;
                if (args[0] is Variable)
                {
                    iSource = (Variable)args[0];
                }
                else
                {
                    iSource = SolveSingleResultExp(args[0], handler) as Variable;
                }

                if (iSource != null)
                {
                    var iValues = iSource.ExtractValue(handler);
                    if (args.Count == 1)
                    {
                        // only the var was specified, so do a default sort.
                        iValues.Sort();
                    }
                    else
                    {
                        var iComparer = new CallbackCompareFunction();
                        iComparer.Callback = SolveSingleResultExp(args[1], handler) as NeuronCluster;

                            // get the callback function.
                        iValues.Sort(iComparer);
                        iComparer.ReleaseProc();
                    }
                }
                else
                {
                    LogService.Log.LogError("Sort.Execute", "First argument should be a variable.");
                }
            }
            else
            {
                LogService.Log.LogError("Sort.Execute", "Invalid nr of arguments specified");
            }

            return true;
        }

        #endregion

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    ///     this is a helper class for performing a complex sort that relies on a
    ///     callback function.
    /// </summary>
    internal class CallbackCompareFunction : System.Collections.Generic.IComparer<Neuron>
    {
        /// <summary>
        ///     the processor to use for performing the callback.
        /// </summary>
        private readonly Processor fProcessor;

        /// <summary>Initializes a new instance of the <see cref="CallbackCompareFunction"/> class. 
        ///     Initializes a new instance of the<see cref="CallbackCompareFunction"/> class.</summary>
        public CallbackCompareFunction()
        {
            fProcessor = ProcessorFactory.GetProcessor(); // create a new callback function to be used for the sort.
            fProcessor.SplitAllowed = false;

                // split is not allowed cause we do a simple 'call' instruction, we don't go through teh threadmanaged and set everything up for splits, hence, not allowed. 'Blocked calls' from this processor however, are allowed, these in turn can do splits.
        }

        /// <summary>Gets or sets the callback.</summary>
        public NeuronCluster Callback { get; set; }

        #region IComparer<Neuron> Members

        /// <summary>Compares the specified neurons through the callback.</summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int Compare(Neuron x, Neuron y)
        {
            var iList = Factories.Default.NLists.GetBuffer();

                // get some lists from the buffer so we can put the 2 args on the stack of the processor.
            iList.Add(y);
            fProcessor.Mem.ParametersStack.Push(iList);
            iList = Factories.Default.NLists.GetBuffer();
            iList.Add(x);
            fProcessor.Mem.ParametersStack.Push(iList);
            fProcessor.Call(Callback);
            if (fProcessor.Mem.LastReturnValues.Count > 0)
            {
                var iRes = fProcessor.Mem.LastReturnValues.Pop();
                if (iRes.Count > 0)
                {
                    if (iRes[0] is IntNeuron)
                    {
                        var iVal = ((IntNeuron)iRes[0]).Value;
                        if (iRes[0].ID == Neuron.TempId)
                        {
                            Brain.Current.Delete(iRes[0]);
                        }

                        return iVal;
                    }

                    LogService.Log.LogError("sort", string.Format("Callback {0} has to return an int value.", Callback));
                    return 0;
                }

                LogService.Log.LogError("sort", string.Format("Callback {0} did not return a value.", Callback));
                return 0;
            }

            LogService.Log.LogError("sort", string.Format("Callback {0} did not return a value.", Callback));
            return 0;
        }

        #endregion

        /// <summary>
        ///     releases the memory of the proc so it can be reused.
        /// </summary>
        internal void ReleaseProc()
        {
            ProcessorFactory.Recycle(fProcessor);
        }
    }
}