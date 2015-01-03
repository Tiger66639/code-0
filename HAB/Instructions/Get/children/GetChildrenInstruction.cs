// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetChildrenInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the children of a <see cref="NeuronCluster" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the children of a <see cref="NeuronCluster" /> .
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 the cluster from which to return all the children.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetChildrenInstruction)]
    public class GetChildrenInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetChildrenInstruction;
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
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iRes = processor.Mem.ArgumentStack.Peek();
                    var iItems = iCluster.GetBufferedChildren<Neuron>();
                    try
                    {
                        iRes.AddRange(iItems);
                    }
                    finally
                    {
                        iCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                    }

                    // ulong[] iItems;
                    // using (ChildrenAccessor iList = iCluster.Children)                   //this lock needs to be a small as possible, can't have a brain.current[] inside the lock.
                    // iItems = iList.ToArray();
                    // foreach (ulong i in iItems)
                    // {
                    // Neuron iFound;
                    // if (Brain.Current.TryFindNeuron(i, out iFound) == true)        //perhaps its already deleted.
                    // iRes.Add(iFound);
                    // }
                }
                else
                {
                    LogService.Log.LogError("GetChildren.GetValues", "First argument should be a NeuronCluster!");
                }
            }
            else
            {
                LogService.Log.LogError("GetChildren.GetValues", "No arguments specified");
            }
        }
    }
}