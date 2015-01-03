// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PopValueInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   This instruction removes the current list of <see cref="Neuron" /> from
//   the <see cref="Processor" /> 's parameters stack (for passing along
//   values to a function call).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This instruction removes the current list of <see cref="Neuron" /> from
    ///     the <see cref="Processor" /> 's parameters stack (for passing along
    ///     values to a function call).
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.PopValueInstruction)]
    public class PopValueInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PopValueInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PopValueInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        #region IExecStatement Members

        /// <summary>Performs the specified processor.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            var iFrame = processor.CallFrameStack.Peek();
            System.Diagnostics.Debug.Assert(processor != null);
            foreach (var i in args)
            {
                var iLoc = i as Local;
                if (iLoc != null)
                {
                    var iPrev = iLoc.ExtractValueNoInit(processor);
                    if (iPrev != null)
                    {
                        iFrame.LocalsBuffer.Add(
                            new System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>(
                                iLoc, 
                                iPrev.Data));
                        processor.StoreVariableValue(iLoc, iPrev, processor.Mem.ParametersStack.Pop());

                            // this is faster, don't need to do 2 lookups in the dict, already have the object that will store the data.
                    }
                    else
                    {
                        iFrame.LocalsBuffer.Add(
                            new System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>(
                                iLoc, 
                                null));
                        processor.StoreVariableValue(iLoc, processor.Mem.ParametersStack.Pop());
                    }
                }
                else
                {
                    LogService.Log.LogError("PopValue", string.Format("invalid parameter: {0}, only locals allowed", i));
                }
            }

            // processor.Mem.ArgumentStack.Push();
            return true;
        }

        #endregion

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.InvalidOperationException();
        }
    }
}