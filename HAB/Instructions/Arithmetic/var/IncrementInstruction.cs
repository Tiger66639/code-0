// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncrementInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   increments the value of every neuron (only <see langword="int" /> or
//   <see langword="double" /> allowed) in the argument list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Instructions.Arithmetic
{
    /// <summary>
    ///     increments the value of every neuron (only <see langword="int" /> or
    ///     <see langword="double" /> allowed) in the argument list.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>the first argument, either or</description>
    ///         </item>
    ///         <item>
    ///             <description>any nr of or neurons may follow.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IncrementInstruction)]
    public class IncrementInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IncrementInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IncrementInstruction;
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
                return -1;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iRes = processor.Mem.ArgumentStack.Peek();
                if (iRes.Capacity < list.Count)
                {
                    iRes.Capacity = list.Count;
                }

                foreach (var i in list)
                {
                    if (TryInt(i as IntNeuron) || TryDouble(i as DoubleNeuron))
                    {
                        iRes.Add(i);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Increment.GetValues", 
                            string.Format("{0} is not an int or double, can't decrement the value.", i.ID));
                    }
                }
            }
            else
            {
                LogService.Log.LogError("Increment.GetValues", "No arguments specified");
            }
        }

        /// <summary>The try double.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryDouble(DoubleNeuron i)
        {
            if (i != null)
            {
                i.IncValue();
                return true;
            }

            return false;
        }

        /// <summary>The try int.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryInt(IntNeuron i)
        {
            if (i != null)
            {
                i.IncValue();
                return true;
            }

            return false;
        }
    }
}