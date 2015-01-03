// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveLinksOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Destroys a link between 2 neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Destroys a link between 2 neurons.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: from part 2: to part 3: meaning
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveLinksOutInstruction)]
    public class RemoveLinksOutInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinksOutInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveLinksOutInstruction;
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
                return 2;
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

            if (args.Count >= 2)
            {
                if (args[0] != null)
                {
                    if (args[0].ID == EmptyId)
                    {
                        // do this after the lock, so no one can change this.
                        LogService.Log.LogError("RemoveLinksOutInstruction.Execute", "From part is null (first arg).");
                        return;
                    }

                    if (args[1] == null || args[1].ID == EmptyId)
                    {
                        LogService.Log.LogError("RemoveLinksOutInstruction.Execute", "meaning is null (second arg).");
                        return;
                    }

                    if (args[0].LinksOutIdentifier != null)
                    {
                        var iMemFac = Factories.Default;
                        var iTemp = iMemFac.LinkLists.GetBuffer();
                        using (var iList = args[0].LinksOut) iTemp.AddRange(iList);
                        foreach (var i in iTemp)
                        {
                            if (i.MeaningID == args[1].ID)
                            {
                                // we check id's that way we don't need to get all the neurons in the links.
                                i.Destroy();
                            }
                        }

                        iMemFac.LinkLists.Recycle(iTemp);
                    }
                }
                else
                {
                    LogService.Log.LogError("RemoveLinksOutInstruction.Execute", "From part is null (first arg).");
                }
            }
            else
            {
                LogService.Log.LogError("RemoveLinksOutInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}