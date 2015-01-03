// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CcToSInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a cluster containing string neurons (where each string neuron
//   represents a character), into a single string neuron. It is possible to
//   convert multiple items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a cluster containing string neurons (where each string neuron
    ///     represents a character), into a single string neuron. It is possible to
    ///     convert multiple items at once.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 a list of clusters containing only string neurons neurons, usually with 1
    ///                 char.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CcToSInstruction)]
    public class CcToSInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CcToSInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CcToSInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iList = new System.Collections.Generic.List<Neuron>();
                foreach (var iNeuron in list)
                {
                    var iCluster = iNeuron as NeuronCluster;
                    if (iCluster != null)
                    {
                        var iMemFac = Factories.Default;
                        System.Collections.Generic.List<ulong> iIds;
                        using (var iChildren = iCluster.Children)
                        {
                            iIds = iMemFac.IDLists.GetBuffer(iChildren.CountUnsafe);
                            iIds.AddRange(iChildren);
                        }

                        var iBuilder = new System.Text.StringBuilder();
                        foreach (var i in iIds)
                        {
                            var iInt = Brain.Current[i] as TextNeuron;
                            if (iInt != null)
                            {
                                iBuilder.Append(iInt.Text);
                            }
                            else
                            {
                                var iReal = Brain.Current[i];
                                LogService.Log.LogError(
                                    "CcToSInstruction.GetValues", 
                                    string.Format(
                                        "The cluster should only contain int neurons, found a {0} at pos '{1}': {2}", 
                                        iReal.GetType(), 
                                        iIds.IndexOf(i), 
                                        iReal));
                                return;
                            }
                        }

                        iMemFac.IDLists.Recycle(iIds);
                        var iStr = iBuilder.ToString();
                        var iRes = TextSin.Words.GetNeuronFor(iStr);
                        iList.Add(iRes);
                    }
                    else
                    {
                        LogService.Log.LogError("CcToSInstruction.GetValues", "Argument must be clusters!");
                    }
                }

                processor.Mem.ArgumentStack.Peek().AddRange(iList);
            }
            else
            {
                LogService.Log.LogError("CcToSInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }
    }
}