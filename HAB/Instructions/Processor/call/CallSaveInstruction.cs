// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallSaveInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Sends a neuroncluster's content to the processor for execution. The
//   cluster should contain expressions. Arguments: variable references that
//   need to be passed along can be declared after the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sends a neuroncluster's content to the processor for execution. The
    ///     cluster should contain expressions. Arguments: variable references that
    ///     need to be passed along can be declared after the cluster.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CallSaveInstruction)]
    public class CallSaveInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CallSaveInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CallSaveInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Any length is possible but at least 1 is required, second arguments
        ///     should be variable refs .
        /// </summary>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        /// <summary>Performs the tasks on the specified processor, using a new stack frame
        ///     for the variables so that previous variable values are kept, except
        ///     the variables that are passed in with the arguments.</summary>
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
                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    PrepareProcDict(processor, args);
                    var iFrame = CallInstCallFrame.CreateCallInst(iCluster);
                    iFrame.CausedNewVarDict = true;
                    processor.PushFrame(iFrame);
                }
                else
                {
                    LogService.Log.LogError("CallSaveInstruction.Execute", "First argument should be a neuron cluster.");
                }
            }
            else
            {
                LogService.Log.LogError("CallSaveInstruction.Execute", "No arguments specified");
            }
        }

        /// <summary>The prepare proc dict.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        private void PrepareProcDict(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (processor.Mem.VariableValues.Count > 0)
            {
                var iPrev = processor.Mem.VariableValues.Peek(); // so we can copy over values.
                var iNew = processor.Mem.VariableValues.Push();

                    // we need a new dictionary for the variables, so that variable values are local to the function.
                for (var i = 1; i < args.Count; i++)
                {
                    var iVar = args[i] as Variable;
                    if (iVar != null)
                    {
                        VarValuesList iVals;
                        if (iPrev.TryGetValue(iVar, out iVals))
                        {
                            iNew.AddShared(iVar, iVals);
                        }
                        else if (Settings.LogCallSaveVarNotFound)
                        {
                            LogService.Log.LogWarning(
                                "CallSaveInstruction.Execute", 
                                "Variable has no values in the current context: " + iVar.ID);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("CallSaveInstruction.Execute", "Can only pass over variables.");
                    }
                }
            }
            else
            {
                LogService.Log.LogError("CallSaveInstruction.Execute", "Internal error: no variable stack.");
            }
        }
    }
}