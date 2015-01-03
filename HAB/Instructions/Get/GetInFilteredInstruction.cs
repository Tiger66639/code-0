// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInFilteredInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the neurons that start from the specified neuron, with the
//   specified meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the neurons that start from the specified neuron, with the
    ///     specified meaning.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 4</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>to part of link.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a reference to the var that will hold the meaning of the link
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a reference to the var that will hold the from part of the link.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a reference to a result statement that evaluates to inlcude the 'to' in
    ///                 the result or not.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetInFilteredInstruction)]
    public class GetInFilteredInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInFilteredInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetInFilteredInstruction;
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
                return 4;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 4)
            {
                var iFrom = list[0];
                if (iFrom != null)
                {
                    var iVar1 = list[1] as Variable;
                    var iVar2 = list[2] as Variable;
                    var iExp = list[3] as ResultExpression;
                    if (iVar1 != null)
                    {
                        if (iVar2 != null)
                        {
                            if (iExp != null)
                            {
                                if (iFrom.LinksInIdentifier != null)
                                {
                                    var iMemFac = Factories.Default;
                                    var iRes = processor.Mem.ArgumentStack.Peek();
                                    var iTemp = iMemFac.LinkLists.GetBuffer();
                                    using (var iLinks = iFrom.LinksIn) iTemp.AddRange(iLinks); // we make a snapshot of the links, to avoid deadlocks.
                                    foreach (var i in iTemp)
                                    {
                                        Neuron iMeaning = null;
                                        Brain.Current.TryFindNeuron(i.MeaningID, out iMeaning);
                                        Neuron iLFrom = null;
                                        Brain.Current.TryFindNeuron(i.FromID, out iLFrom);
                                        if (iMeaning != null && iLFrom != null)
                                        {
                                            // the links are no longer locked, so it could be that the link already got destroyed, so check for this.
                                            iVar1.StoreValue(iMeaning, processor);
                                            iVar2.StoreValue(iLFrom, processor);
                                            var iResOk = false;

                                                // init to false, so we can handle the case in which there is no result.
                                            var iData = SolveResultExpNoStackChange(iExp, processor);
                                            if (iData != null)
                                            {
                                                try
                                                {
                                                    foreach (var iResItem in iData)
                                                    {
                                                        if (iResItem.ID != (ulong)PredefinedNeurons.True)
                                                        {
                                                            iResOk = false;

                                                                // if there is is anything but a true, we're in trouble.
                                                            break;
                                                        }

                                                        iResOk = true; // if there is a true, we have an ok.
                                                    }
                                                }
                                                finally
                                                {
                                                    processor.Mem.ArgumentStack.Pop();

                                                        // we used the stack to get the result, so don't forget to free it again so that it can be reused.
                                                }
                                            }

                                            if (iResOk)
                                            {
                                                iRes.Add(iLFrom);
                                            }
                                        }
                                    }

                                    iMemFac.LinkLists.Recycle(iTemp);
                                }
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "GetInFilteredInstruction.InternalGetValue", 
                                    "Invalid fourth argument, ResultExpression expected.");
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetInFilteredInstruction.InternalGetValue", 
                                "Invalid third argument, Variable expected.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetInFilteredInstruction.InternalGetValue", 
                            "Invalid second argument, Variable expected.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetInFilteredInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetInFilteredInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }
        }
    }
}