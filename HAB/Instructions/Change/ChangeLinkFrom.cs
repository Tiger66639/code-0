// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeLinkFrom.cs" company="">
//   
// </copyright>
// <summary>
//   Changes the from part of a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes the from part of a link.
    /// </summary>
    /// <remarks>
    ///     1: From part 2: to part 3: meaning part 4: new From
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChangeLinkFrom)]
    public class ChangeLinkFrom : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeLinkFrom" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChangeLinkFrom;
            }
        }

        #endregion

        /// <summary>Gets the arg count.</summary>
        public override int ArgCount
        {
            get
            {
                return 4;
            }
        }

        /// <summary>The execute.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(args != null);

            if (args.Count >= 4)
            {
                if (args[0] == null)
                {
                    LogService.Log.LogError("ChangeLinkFrom.Execute", "From part is null (first arg).");
                    return;
                }

                if (args[1] == null)
                {
                    LogService.Log.LogError("ChangeLinkFrom.Execute", "To part is null (second arg).");
                    return;
                }

                if (args[2] == null)
                {
                    LogService.Log.LogError("ChangeLinkFrom.Execute", "meaning is null (third arg).");
                    return;
                }

                if (args[3] == null)
                {
                    LogService.Log.LogError("ChangeLinkFrom.Execute", "New from is null (fourth arg).");
                    return;
                }

                var iNew = Link.Find(args[0], args[1], args[2]);
                if (iNew != null)
                {
                    iNew.From = args[3];
                }
                else
                {
                    LogService.Log.LogError(
                        "ChangeLinkFrom.Execute", 
                        string.Format("Link not found: to={0}, from={1}, meaning={2}.", args[0], args[1], args[2]));
                }
            }
            else
            {
                LogService.Log.LogError("ChangeLinkFrom.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}