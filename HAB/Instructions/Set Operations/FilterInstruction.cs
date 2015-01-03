// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Runs every item in the list (at the end, after the filter), through a
//   filter, to determin if it can be included in the result set.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Runs every item in the list (at the end, after the filter), through a
    ///     filter, to determin if it can be included in the result set.
    /// </summary>
    /// <remarks>
    ///     Arg: -1. 1: a <see langword="ref" /> to the var that can be used in the
    ///     filter condition. It will contain the item that needs to be
    ///     <see langword="checked" /> by the filter. 2: a <see langword="ref" /> to a
    ///     result statement (usually a <see langword="bool" /> condition), which is
    ///     used to determin if an item (var 1) can be included in the result set or
    ///     not. 3: the first item that needs to be filtered. result: a list of
    ///     neurons that passed the filter.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.FilterInstruction)]
    public class FilterInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.FilterInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.FilterInstruction;
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
            var iRes = processor.Mem.ArgumentStack.Peek();
            if (list != null && list.Count >= 2)
            {
                var iVar = list[0] as Variable;
                var iExp = list[1] as ResultExpression;
                if (iVar != null)
                {
                    if (iExp != null)
                    {
                        IGetBool iBoolExp = iExp;
                        if (iBoolExp != null && iBoolExp.CanGetBool())
                        {
                            // normally, there's a shortcut we can use.
                            for (var i = 2; i < list.Count; i++)
                            {
                                var iNeuron = list[i];
                                iVar.StoreValue(iNeuron, processor);

                                    // we assign the current cluster to the var so the expression can evaluate it correctly.
                                if (iBoolExp.GetBool(processor))
                                {
                                    iRes.Add(iNeuron);
                                }
                            }
                        }
                        else
                        {
                            for (var i = 2; i < list.Count; i++)
                            {
                                var iNeuron = list[i];
                                iVar.StoreValue(iNeuron, processor);

                                    // we assign the current cluster to the var so the expression can evaluate it correctly.
                                var iResOk = false;

                                    // init to false, so we can handle the case in which there is no result.
                                try
                                {
                                    foreach (var iResItem in SolveResultExpNoStackChange(iExp, processor))
                                    {
                                        if (iResItem.ID != (ulong)PredefinedNeurons.True)
                                        {
                                            iResOk = false; // if there is is anything but a true, we're in trouble.
                                            break;
                                        }

                                        iResOk = true; // if there is a true, we have an ok.
                                    }
                                }
                                finally
                                {
                                    processor.Mem.ArgumentStack.Pop();
                                }

                                if (iResOk)
                                {
                                    iRes.Add(iNeuron);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "FilterInstruction.InternalGetValue", 
                            "Invalid second argument, ResultExpression expected.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "FilterInstruction.InternalGetValue", 
                        "Invalid first argument, Variable expected.");
                }
            }
            else
            {
                LogService.Log.LogError("FilterInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }
        }
    }
}