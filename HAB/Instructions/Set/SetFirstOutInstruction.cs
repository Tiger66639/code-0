// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetFirstOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Sets the first link out to a new value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sets the first link out to a new value.
    /// </summary>
    /// <remarks>
    ///     arg: 1: From part 2: To part (this becomes the new value) 3: Meaning part
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetFirstOutInstruction)]
    public class SetFirstOutInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetFirstOutInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetFirstOutInstruction;
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
                return 3;
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
            if (args.Count >= 3)
            {
                var iFrom = args[0];
                if (iFrom != null)
                {
                    var iTo = args[1];
                    if (iTo != null)
                    {
                        var iMeaning = args[2];
                        if (iMeaning != null)
                        {
                            if (iMeaning.ID == TempId)
                            {
                                Brain.Current.Add(iMeaning);
                            }

                            Link.SetFirstOutTo(iFrom, iTo, iMeaning);
                            iFrom.SetFirstOutgoingLinkTo(iMeaning.ID, iTo);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "SetFirstOutInstruction.Execute", 
                                "Invalid third argument, Neuron expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SetFirstOutInstruction.Execute", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "SetFirstOutInstruction.Execute", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("SetFirstOutInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}