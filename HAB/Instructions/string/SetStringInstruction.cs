// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetStringInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Combines all the neurons into a string (except the first one) and assigns
//   this string to the first neuron (which must be a string). If the
//   texneuron was previously stored in the dictionary, then this is updated,
//   when possible, otherwise the <see langword="ref" /> is removed from the
//   dictionay. So the value is always stored in the first neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Combines all the neurons into a string (except the first one) and assigns
    ///     this string to the first neuron (which must be a string). If the
    ///     texneuron was previously stored in the dictionary, then this is updated,
    ///     when possible, otherwise the <see langword="ref" /> is removed from the
    ///     dictionay. So the value is always stored in the first neuron.
    /// </summary>
    /// <remarks>
    ///     arg: 1: a textneuron that will receive the result 2: The first value that
    ///     needs to be assigned to the first arg.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetStringInstruction)]
    public class SetStringInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetStringInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetStringInstruction;
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
        /// <param name="list">The list.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            // TextNeuron iRes = null;
            if (list != null && list.Count >= 2)
            {
                var iStr = new System.Text.StringBuilder();
                var iFirst = list[0] as TextNeuron;
                if (iFirst != null)
                {
                    for (var i = 1; i < list.Count; i++)
                    {
                        iStr.Append(BrainHelper.GetTextFrom(list[i]));
                    }

                    bool iInDict;
                    if (TextSin.Words.ContainsKey(iFirst.Text))
                    {
                        TextSin.Words.Remove(iFirst);
                        iInDict = true;
                    }
                    else
                    {
                        iInDict = false;
                    }

                    iFirst.Text = iStr.ToString();
                    if (iInDict)
                    {
                        TextSin.Words.AddWhenPossible(iFirst);
                    }
                }
                else
                {
                    LogService.Log.LogError("SetString", "First value should be a textNeuron.");
                }
            }
            else
            {
                LogService.Log.LogError("SetString", "Invalid nr of arguments: bool and ref to var expected");
            }
        }
    }
}