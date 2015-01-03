// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrepareLocalInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   prepares locals for usage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     prepares locals for usage.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 The first to prepare. This should be passed along through another
    ///                 variable or a byref expression.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.PrepareLocalInstruction)]
    public class PrepareLocalInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PrepareLocalInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PrepareLocalInstruction;
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
            if (args != null && args.Count >= 1)
            {
                var iFrame = processor.CallFrameStack.Peek();
                var iDict = processor.Mem.VariableValues.Peek();
                for (var i = 0; i < args.Count; i++)
                {
                    var iVar = args[i] as Local;
                    if (iVar != null)
                    {
                        var iPrev = iVar.ExtractValueNoInit(processor);
                        if (iPrev != null)
                        {
                            iFrame.LocalsBuffer.Add(
                                new System.Collections.Generic.KeyValuePair
                                    <Local, System.Collections.Generic.List<Neuron>>(iVar, iPrev.Data));

                                // store the previous value so we can reset.
                            iPrev.Data = null;

                                // set to null, otherwise the list gets recycled, but it's still being used in the buffer of the frame.
                            iDict.Remove(iVar); // reset to null so that we can have a new possible value.s
                        }
                        else
                        {
                            iFrame.LocalsBuffer.Add(
                                new System.Collections.Generic.KeyValuePair
                                    <Local, System.Collections.Generic.List<Neuron>>(iVar, null));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "PrepareLocalInstruction.Execute", 
                            string.Format("Can't prepare local, Invalid var specified: {0}.", args[i]));
                    }
                }
            }
            else
            {
                LogService.Log.LogError("PrepareLocalInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}