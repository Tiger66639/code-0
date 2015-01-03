// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertLinkInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Creates a new link between from and to and performs an insert when the
//   link is added to from and/or to.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Creates a new link between from and to and performs an insert when the
    ///     link is added to from and/or to.
    /// </summary>
    /// <remarks>
    ///     Args: 1: from part 2: from index (can be invalid, indicates an add) 3: to
    ///     part 4: to index (can be invalid, indicates an add) 5: meaning part
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.InsertLinkInstruction)]
    public class InsertLinkInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InsertLinkInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InsertLinkInstruction;
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
                return 5;
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

            if (args.Count >= 5)
            {
                int iFrom;
                int iTo;
                if (CheckArgs(args, out iFrom, out iTo))
                {
                    var iArgs = new Link.CreateArgs
                                    {
                                        To = args[2], 
                                        From = args[0], 
                                        Meaning = args[4], 
                                        FromIndex = iFrom, 
                                        ToIndex = iTo
                                    };
                    var iNew = new Link(iArgs);
                }
            }
            else
            {
                LogService.Log.LogError("InsertLinkInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="args">The args.</param>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> args, out int from, out int to)
        {
            from = -1;
            to = -1;
            if (args[0] == null)
            {
                LogService.Log.LogError("InsertLinkInstruction.Execute", "From part is null (first arg).");
                return false;
            }

            var iTemp = args[1] as IntNeuron;
            if (iTemp != null)
            {
                from = iTemp.Value;
            }

            if (args[2] == null)
            {
                LogService.Log.LogError("InsertLinkInstruction.Execute", "To part is null (third arg).");
                return false;
            }

            iTemp = args[3] as IntNeuron;
            if (iTemp != null)
            {
                to = iTemp.Value;
            }

            if (args[4] == null)
            {
                LogService.Log.LogError("InsertLinkInstruction.Execute", "meaning is null (fith arg).");
                return false;
            }

            return true;
        }
    }
}