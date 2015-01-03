// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of items in a list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of items in a list.
    /// </summary>
    /// <remarks>
    ///     <para>arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 The list of arguments is counted. The neuron that is returned is only
    ///                 registered as a temp neuron, so it is not permanent, but if it is somehow
    ///                 stored permanetly, it's id is updated. This allows for temporary values
    ///                 during code execution.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CountInstruction)]
    public class CountInstruction : SingleResultInstruction, ICalculateInt, ICalculateDouble
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CountInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CountInstruction;
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

        #region ICalculateDouble Members

        /// <summary>Calculate the <see langword="double"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return list.Count;
        }

        #endregion

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return list.Count;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The count of the list.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            IntNeuron iNeuron;
            if (list != null)
            {
                iNeuron = NeuronFactory.GetInt(list.Count);
            }
            else
            {
                iNeuron = NeuronFactory.GetInt(-1);
            }

            Brain.Current.MakeTemp(iNeuron);

                // we always return temp values: this is important, otherwise we can't use temporary values in the code.
            return iNeuron;
        }
    }
}