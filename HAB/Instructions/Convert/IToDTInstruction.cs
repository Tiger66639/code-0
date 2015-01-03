// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IToDTInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts the list of input integers into a cluster that contains date
//   integers and has the dateTime meaning (so a dateTime cluster)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts the list of input integers into a cluster that contains date
    ///     integers and has the dateTime meaning (so a dateTime cluster)
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
    [NeuronID((ulong)PredefinedNeurons.IToDTInstruction)]
    public class IToDTInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IToDTInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IToDTInstruction;
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
        /// <param name="processor">The processor.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The count of the list.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Time;
            if (ProcessYear(iRes, list))
            {
                if (ProcessMonth(iRes, list))
                {
                    if (ProcessDay(iRes, list))
                    {
                        if (ProcessHour(iRes, list))
                        {
                            if (ProcessMinute(iRes, list))
                            {
                                ProcessSecond(iRes, list);
                            }
                        }
                    }
                }
            }

            iRes.SetIsFrozen(true, processor);
            return iRes;
        }

        /// <summary>The process second.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessSecond(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 6)
            {
                var iIndex = list[5] as IntNeuron;
                if (iIndex != null)
                {
                    if (iIndex.Value >= 0 && iIndex.Value <= Time.Current.Seconds.Count)
                    {
                        iRes.ChildrenW.Add(Time.Current.Seconds[iIndex.Value - 1]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The process minute.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessMinute(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 5)
            {
                var iIndex = list[4] as IntNeuron;
                if (iIndex != null)
                {
                    if (iIndex.Value >= 0 && iIndex.Value <= Time.Current.Minutes.Count)
                    {
                        iRes.ChildrenW.Add(Time.Current.Minutes[iIndex.Value - 1]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The process hour.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessHour(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 4)
            {
                var iIndex = list[3] as IntNeuron;
                if (iIndex != null)
                {
                    if (iIndex.Value >= 0 && iIndex.Value <= Time.Current.Hours.Count)
                    {
                        using (var iList = iRes.ChildrenW) iList.Add(Time.Current.Hours[iIndex.Value - 1]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The process day.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessDay(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 3)
            {
                var iIndex = list[2] as IntNeuron;
                if (iIndex != null)
                {
                    if (iIndex.Value >= 0 && iIndex.Value <= Time.Current.Days.Count)
                    {
                        using (var iList = iRes.ChildrenW) iList.Add(Time.Current.Days[iIndex.Value - 1]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The process month.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessMonth(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 2)
            {
                var iIndex = list[1] as IntNeuron;
                if (iIndex != null)
                {
                    if (iIndex.Value >= 0 && iIndex.Value <= Time.Current.Months.Count)
                    {
                        using (var iList = iRes.ChildrenW) iList.Add(Time.Current.Months[iIndex.Value - 1]);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The process year.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessYear(NeuronCluster iRes, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iIndex = list[0] as IntNeuron;
                if (iIndex != null)
                {
                    var iFound = Time.Current.GetYear(iIndex);
                    using (var iList = iRes.ChildrenW) iList.Add(iFound);
                    return true;
                }
            }

            return false;
        }
    }
}