// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolveInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   This instruction tries to solve a neuron on a new processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This instruction tries to solve a neuron on a new processor.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The new processor will be running for the same sin as the calling
    ///         processor. (min) 1 arg:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 The first neuron that needs to be solved (top of stack).
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SolveInstruction)]
    public class SolveInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.OutputInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SolveInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Any length is possible but at least 2 are required.
        /// </summary>
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
                var iNew = ProcessorFactory.GetProcessor();

                    // the current processor can be a debugger, in which case, we want the same type.                                                          //switch to the output sin, otherwise the message goes to the input sin.
                iNew.CurrentSin = processor.CurrentSin; // we keep the same sin.
                for (var i = args.Count - 1; i >= 0; i--)
                {
                    iNew.Push(args[i]);
                }

                iNew.Solve();
            }
            else
            {
                LogService.Log.LogError("SolveInstruction.Execute", "No arguments specified");
            }
        }
    }
}