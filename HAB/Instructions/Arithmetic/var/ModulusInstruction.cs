// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModulusInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the modulus of all the arguments, which must either be an
//   <see langword="int" /> or a double. If the first is int, an
//   <see langword="int" /> is returned, otherwise, if it is a double, the
//   result is also a double.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the modulus of all the arguments, which must either be an
    ///     <see langword="int" /> or a double. If the first is int, an
    ///     <see langword="int" /> is returned, otherwise, if it is a double, the
    ///     result is also a double.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>the first argument, either or</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 the second arguement, either or double, the same as argument 1.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ModulusInstruction)]
    public class ModulusInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModulusInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ModulusInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iRes = TryInt(list);
                if (iRes == null)
                {
                    iRes = TryDouble(list);
                }

                if (iRes != null)
                {
                    return iRes;
                }

                LogService.Log.LogError(
                    "ModulusInstruction.GetValues", 
                    "All arguments must be a value type, either IntNeuron or DoubleNeuron!");
            }
            else
            {
                LogService.Log.LogError("ModulusInstruction.GetValues", "Invalid nr of arguments specified");
            }

            return null;
        }

        /// <summary>The try double.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryDouble(System.Collections.Generic.IList<Neuron> list)
        {
            var iFirst = list[0] as DoubleNeuron;
            if (iFirst != null)
            {
                var iVal = iFirst.Value;

                for (var i = 1; i < list.Count; i++)
                {
                    var iSecond = list[i] as DoubleNeuron;
                    if (iSecond != null)
                    {
                        iVal %= iSecond.Value;
                    }
                    else
                    {
                        var iIntSec = list[i] as IntNeuron;
                        if (iIntSec != null)
                        {
                            iVal %= iIntSec.Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                var iRes = NeuronFactory.GetDouble(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            return null;
        }

        /// <summary>The try int.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryInt(System.Collections.Generic.IList<Neuron> list)
        {
            var iFirst = list[0] as IntNeuron;
            if (iFirst != null)
            {
                var iVal = iFirst.Value;

                for (var i = 1; i < list.Count; i++)
                {
                    var iSecond = list[i] as IntNeuron;
                    if (iSecond != null)
                    {
                        iVal %= iSecond.Value;
                    }
                    else
                    {
                        var iIntSec = list[i] as DoubleNeuron;
                        if (iIntSec != null)
                        {
                            iVal %= (int)iIntSec.Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                var iRes = NeuronFactory.GetInt(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            return null;
        }
    }
}