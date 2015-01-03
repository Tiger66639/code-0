// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes all the info neurons that are attached to a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes all the info neurons that are attached to a link.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>From part</description>
    ///         </item>
    ///         <item>
    ///             <description>To part</description>
    ///         </item>
    ///         <item>
    ///             <description>Meaning part.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ClearInfoInstruction)]
    public class ClearInfoInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ClearInfoInstruction;
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
                if (CheckArgs(args) == false)
                {
                    return;
                }

                Link iLink;

                // LockRequestList iLock = BuildClearInfoLock(args, out iLink);
                iLink = Link.Find(args[0], args[1], args[2]);
                if (iLink != null)
                {
                    using (var iInfo = iLink.InfoW) iInfo.Clear();
                }
                else
                {
                    LogService.Log.LogError(
                        "ClearInfoInstruction.Execute", 
                        string.Format("Could not find link from={0}, to={1}, meaing={2}", args[0], args[1], args[2]));
                }
            }
            else
            {
                LogService.Log.LogError("ClearInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> args)
        {
            if (args[0] == null)
            {
                LogService.Log.LogError("ClearInfoInstruction.Execute", "From part is null (first arg).");
                return false;
            }

            if (args[1] == null)
            {
                LogService.Log.LogError("ClearInfoInstruction.Execute", "To part is null (second arg).");
                return false;
            }

            if (args[2] == null)
            {
                LogService.Log.LogError("ClearInfoInstruction.Execute", "meaning is null (third arg).");
                return false;
            }

            return true;
        }
    }
}