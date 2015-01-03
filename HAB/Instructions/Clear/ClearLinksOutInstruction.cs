// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearLinksOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes all the outgoing links from a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes all the outgoing links from a neuron.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>The neuron for which to remove all the links.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ClearLinksOutInstruction)]
    public class ClearLinksOutInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearLinksOutInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ClearLinksOutInstruction;
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
                return 1;
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
                var iNeuron = args[0];
                if (iNeuron != null)
                {
                    if (iNeuron.LinksOutIdentifier != null)
                    {
                        var iMemFac = Factories.Default;
                        var iLinks = iMemFac.LinkLists.GetBuffer();
                        using (var iList = iNeuron.LinksOut) iLinks.AddRange(iLinks);

                        for (var i = 0; i < iLinks.Count; i++)
                        {
                            var iLink = iLinks[i];
                            if (iLink != null)
                            {
                                iLink.Destroy();
                            }
                        }

                        iMemFac.LinkLists.Recycle(iLinks);
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ClearLinksOutInstruction.Execute", 
                        string.Format("Can't clear Links out, Invalid Neuron specified: {0}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("ClearLinksOutInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}