// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToSInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts the contents of the var into a string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Instructions.Convert
{
    /// <summary>
    ///     Converts the contents of the var into a string.
    /// </summary>
    /// <remarks>
    ///     <para>arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a to a var.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a to indicate if the textneuron needs to be indexed or not. When not
    ///                 indexed, the neuron that is returned is only registered as a temp neuron,
    ///                 so it is not permanent, but if it is somehow stored permanetly, it's id
    ///                 is updated. This allows for temporary values during code execution.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ToSInstruction)]
    public class ToSInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ToSInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ToSInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The count of the list.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            TextNeuron iRes = null;
            if (list != null && list.Count >= 2)
            {
                var iStr = new System.Text.StringBuilder();
                var iIndex = CheckFirstArg(list);
                for (var i = 1; i < list.Count; i++)
                {
                    iStr.Append(BrainHelper.GetTextFrom(list[i]));
                }

                if (iIndex)
                {
                    iRes = TextNeuron.GetFor(iStr.ToString());
                }
                else
                {
                    iRes = NeuronFactory.GetText(iStr.ToString());
                    Brain.Current.MakeTemp(iRes);
                }
            }
            else
            {
                LogService.Log.LogError("ToS", "Invalid nr of arguments: bool and ref to var expected");
            }

            return iRes;
        }

        /// <summary>The check first arg.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckFirstArg(System.Collections.Generic.IList<Neuron> list)
        {
            if (list[0] == Brain.Current.TrueNeuron)
            {
                return true;
            }

            return false;
        }
    }
}