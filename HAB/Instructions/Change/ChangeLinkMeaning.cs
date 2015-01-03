// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeLinkMeaning.cs" company="">
//   
// </copyright>
// <summary>
//   Changes the meaning of a link
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes the meaning of a link
    /// </summary>
    /// <remarks>
    ///     1: From part 2: to part 3: meaning part 4: new meaning
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChangeLinkMeaning)]
    public class ChangeLinkMeaning : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeLinkMeaning" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChangeLinkMeaning;
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
                return 4;
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
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(args != null);

            if (args.Count >= 4)
            {
                if (args[0] == null)
                {
                    LogService.Log.LogError("ChangeLinkMeaning.Execute", "From part is null (first arg).");
                    return;
                }

                if (args[1] == null)
                {
                    LogService.Log.LogError("ChangeLinkMeaning.Execute", "To part is null (second arg).");
                    return;
                }

                if (args[2] == null)
                {
                    LogService.Log.LogError("ChangeLinkMeaning.Execute", "meaning is null (third arg).");
                    return;
                }

                if (args[3] == null)
                {
                    LogService.Log.LogError("ChangeLinkMeaning.Execute", "New meaing is null (fourth arg).");
                    return;
                }

                var iNew = Link.Find(args[0], args[1], args[2]);
                if (iNew != null)
                {
                    iNew.Meaning = args[3];
                }
                else
                {
                    LogService.Log.LogError(
                        "ChangeLinkMeaning.Execute", 
                        string.Format("Link not found: to={0}, from={1}, meaning={2}.", args[0], args[1], args[2]));
                }
            }
            else
            {
                LogService.Log.LogError("ChangeLinkMeaning.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}