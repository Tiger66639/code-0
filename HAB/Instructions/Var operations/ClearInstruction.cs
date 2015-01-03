// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Clears the contents of a variables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Clears the contents of a variables.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 The variable for which to remove the content. This should be passed along
    ///                 through another variable or a byref expression.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ClearInstruction)]
    public class ClearInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ClearInstruction;
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
                var iMemFac = Factories.Default;
                for (var i = 0; i < args.Count; i++)
                {
                    var iVar = args[i] as Variable;
                    if (iVar != null)
                    {
                        if (iVar is Global)
                        {
                            var iDict = processor.GlobalValues;
                            if (iVar.SplitReactionID != (ulong)PredefinedNeurons.shared)
                            {
                                // recycle the data when possible.
                                System.Collections.Generic.List<Neuron> iOld;
                                if (iDict.TryGetValue((Global)iVar, out iOld))
                                {
                                    iMemFac.NLists.Recycle(iOld);
                                }
                            }

                            iDict.Remove((Global)iVar);
                        }
                        else if (!(iVar is SystemVariable))
                        {
                            if (processor.Mem.VariableValues.Count > 0)
                            {
                                var iDict = processor.Mem.VariableValues.Peek();
                                iDict.Remove(iVar);
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "ClearInstruction.Execute", 
                                string.Format("Can't clear var, Invalid var specified: {0}.", args[i]));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ClearInstruction.Execute", 
                            string.Format("Can't clear var, Invalid var specified: {0}.", args[i]));
                    }
                }
            }
            else
            {
                LogService.Log.LogError("ClearInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}