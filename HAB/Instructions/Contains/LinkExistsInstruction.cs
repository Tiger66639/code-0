// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkExistsInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the link exists. otherwise,
//   <see langword="false" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if the link exists. otherwise,
    ///     <see langword="false" /> is returned.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The from part of the link 2: The To part of the link 3: The
    ///     meaning of the link.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.LinkExistsInstruction)]
    public class LinkExistsInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LinkExistsInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.LinkExistsInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this
        ///     instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without
        ///     any specific number of values.
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

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 3)
            {
                var iFrom = list[0];
                var iTo = list[1];
                var iMeaning = list[2];
                if (iFrom != null)
                {
                    if (iTo != null)
                    {
                        if (iMeaning != null)
                        {
                            return Link.Exists(iFrom, iTo, iMeaning.ID);
                        }

                        LogService.Log.LogError(
                            "LinkExistsInstruction.InternalGetValue", 
                            "Invalid third argument, Neuron (meaning) expected, found null.");
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "LinkExistsInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron (to part) expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "LinkExistsInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron (from part) expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("LinkExistsInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return false;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CalculateBool(processor, list))
            {
                return Brain.Current.TrueNeuron;
            }

            return Brain.Current.FalseNeuron;
        }
    }
}