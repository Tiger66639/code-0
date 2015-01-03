// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   An instruction that adds an error to the log. The text to log is declared
//   as a list of neurons which are transformed into text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An instruction that adds an error to the log. The text to log is declared
    ///     as a list of neurons which are transformed into text.
    /// </summary>
    /// <remarks>
    ///     Arguments: unlimited list of neurons which are transformed into text
    ///     (with ToString), concatenated and added to the log as error.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ErrorInstruction)]
    public class ErrorInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ErrorInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ErrorInstruction;
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
            var iStr = new System.Text.StringBuilder();
            foreach (var i in args)
            {
                iStr.Append(i);
            }

            LogService.Log.LogError("ErrorInstruction.Execute", iStr.ToString());
        }
    }
}