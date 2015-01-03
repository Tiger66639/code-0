// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StDevInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the stdev of all the arguments, which must either be an int,a
//   <see langword="double" /> or time (spans). If the first is int, an
//   <see langword="int" /> is returned, otherwise, if it is a double, the
//   result is also a double, same for time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Returns the stdev of all the arguments, which must either be an int,a
    ///     <see langword="double" /> or time (spans). If the first is int, an
    ///     <see langword="int" /> is returned, otherwise, if it is a double, the
    ///     result is also a double, same for time.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>the first argument, either int, or time</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 the second arguement, either time, or double, the same as argument 1.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.StDevInstruction)]
    public class StDevInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StDevInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.StDevInstruction;
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

                if (iRes == null)
                {
                    iRes = TryTime(list, processor);
                }

                if (iRes != null)
                {
                    return iRes;
                }

                LogService.Log.LogError(
                    "StDevInstruction.GetValues", 
                    "All arguments must be a time cluster or a value type (either IntNeuron or DoubleNeuron)!");
            }
            else
            {
                LogService.Log.LogError("StDevInstruction.GetValues", "Invalid nr of arguments specified");
            }

            return null;
        }

        /// <summary>The try time.</summary>
        /// <param name="list">The list.</param>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryTime(System.Collections.Generic.IList<Neuron> list, Processor proc)
        {
            var iVals = new System.Collections.Generic.List<double>();
            for (var i = 0; i < list.Count; i++)
            {
                var iToCheck = list[i] as NeuronCluster;
                if (iToCheck != null)
                {
                    var iTime = Time.GetTimeSpan(iToCheck);
                    if (iTime.HasValue)
                    {
                        iVals.Add(iTime.Value.Ticks);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var iVal = StandardDeviation(iVals);
            return Time.Current.GetTimeSpanCluster(new System.TimeSpan((long)iVal), proc);
        }

        /// <summary>The try double.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryDouble(System.Collections.Generic.IList<Neuron> list)
        {
            var iVals = new System.Collections.Generic.List<double>();
            for (var i = 0; i < list.Count; i++)
            {
                var iToCheck = list[i] as DoubleNeuron;
                if (iToCheck != null)
                {
                    iVals.Add(iToCheck.Value);
                }
                else
                {
                    return null;
                }
            }

            var iVal = StandardDeviation(iVals);
            var iRes = NeuronFactory.GetDouble(iVal);
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }

        /// <summary>The try int.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryInt(System.Collections.Generic.IList<Neuron> list)
        {
            var iVals = new System.Collections.Generic.List<double>();
            for (var i = 0; i < list.Count; i++)
            {
                var iToCheck = list[i] as IntNeuron;
                if (iToCheck != null)
                {
                    iVals.Add(iToCheck.Value);
                }
                else
                {
                    return null;
                }
            }

            var iVal = StandardDeviation(iVals);

            var iRes = NeuronFactory.GetInt((int)iVal);
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }

        /// <summary>The standard deviation.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double StandardDeviation(System.Collections.Generic.List<double> data)
        {
            double iRes = 0;
            double iAvg = 0;
            double IVariance = 0;
            var iMax = 0;

            iMax = data.Count;
            if (iMax > 0)
            {
                iAvg = Enumerable.Average(data);
                for (var i = 0; i < iMax; i++)
                {
                    IVariance += System.Math.Pow(data[i] - iAvg, 2);
                }

                if (IVariance == 0)
                {
                    iRes = 0;
                }
                else
                {
                    iRes = IVariance / iMax;
                }

                iRes = System.Math.Sqrt(iRes);
            }

            return iRes;
        }
    }
}