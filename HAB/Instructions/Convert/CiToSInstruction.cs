// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CiToSInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a cluster containing <see langword="int" /> neurons into a
//   string, where each <see langword="int" /> neuron represents the ascii
//   value of a character. It is possible to convert multiple items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a cluster containing <see langword="int" /> neurons into a
    ///     string, where each <see langword="int" /> neuron represents the ascii
    ///     value of a character. It is possible to convert multiple items at once.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a list of clusters containing only neurons.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CiToSInstruction)]
    public class CiToSInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CiToSInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CiToSInstruction;
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
                return 1;
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
                        var iBuilder = new System.Text.StringBuilder();
                        using (var iChildren = iCluster.Children)
                        {
                            iIds = iMemFac.IDLists.GetBuffer(iChildren.CountUnsafe);
                            iIds.AddRange(iChildren);
                        }

                        foreach (var i in iIds)
                        {
                            var iInt = Brain.Current[i] as IntNeuron;
                            if (iInt != null)
                            {
                                iBuilder.Append((char)iInt.Value);
                            }
                            else
                            {
                                var iReal = Brain.Current[i];
                                LogService.Log.LogError(
                                    "CiToSInstruction.GetValues", 
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
                        var iRes = TextSin.Words.GetNeuronFor(iStr, processor);
                        iList.Add(iRes);
                    }
                    else
                    {
                        LogService.Log.LogError("CiToSInstruction.GetValues", "Arguments must be clusters!");
                    }
                }

                processor.Mem.ArgumentStack.Peek().AddRange(iList);
            }
            else
            {
                LogService.Log.LogError("CiToSInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }
    }
}