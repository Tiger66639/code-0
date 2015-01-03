// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCommonParentsFiltered.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the clusters that own the specified neurons, filtered using
//   the specified filter code. Each clustered that is returned, must own all
//   the arguments as children (order is free, but can further be specified in
//   the filter part).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the clusters that own the specified neurons, filtered using
    ///     the specified filter code. Each clustered that is returned, must own all
    ///     the arguments as children (order is free, but can further be specified in
    ///     the filter part).
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: a reference to a variable that is used in the next
    ///     argument. It temporarely stores every (non filtered) cluster. this is
    ///     usually passed along using a <see cref="ByRefExpression" /> . 2: a
    ///     reference to a result statement that determins if a cluster should be
    ///     included in the result or not. This is usually a
    ///     <see cref="BoolExpression" /> , using the previously specified var to
    ///     compare things against. this is usually passed along using a
    ///     <see cref="ByRefExpression" /> . 3: The first neuron for which to return
    ///     the owning clusters, followed by the others.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetCommonParentsFilteredInstruction)]
    public class GetCommonParentsFiltered : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonParentsFilteredInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetCommonParentsFilteredInstruction;
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
            if (list != null && list.Count >= 2)
            {
                var iVar = list[0] as Variable;
                var iExp = list[1] as ResultExpression;
                if (iVar != null)
                {
                    if (iExp != null)
                    {
                        System.Collections.Generic.List<ulong> iCommmons = null;
                        list.RemoveAt(0);

                            // we remove the first item from the list, cause this is the var for the filter, don't want this in the corresponding calculation
                        list.RemoveAt(0); // this instruction has 2 items in the front that need to be rmoved.
                        var iLock = BuildParentsLockFrom(list, 0, false);
                        LockManager.Current.RequestLocks(iLock);
                        try
                        {
                            iCommmons = GetCommonParentsUnsafeUlong(list);
                        }
                        finally
                        {
                            LockManager.Current.ReleaseLocks(iLock);
                        }

                        if (iCommmons != null)
                        {
                            ProcessResults(iCommmons, iExp, iVar, processor);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetCommonParentsFiltered.GetValues", 
                            "Invalid second argument, ResultExpression expected.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetCommonParentsFiltered.GetValues", 
                        "Invalid first argument, Variable expected.");
                }
            }
            else
            {
                LogService.Log.LogError("GetCommonParentsFiltered.GetValues", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The process results.</summary>
        /// <param name="list">The list.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="var">The var.</param>
        /// <param name="processor">The processor.</param>
        private static void ProcessResults(System.Collections.Generic.List<ulong> list, 
            ResultExpression exp, 
            Variable var, 
            Processor processor)
        {
            try
            {
                var iRes = processor.Mem.ArgumentStack.Peek();
                IGetBool iBoolExp = exp;
                if (iBoolExp != null)
                {
                    foreach (var i in list)
                    {
                        if (i != EmptyId)
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(i, out iFound))
                            {
                                // could already have been deleted cause we are outside of the lock.
                                var iToAdd = iFound as NeuronCluster;
                                if (iToAdd != null)
                                {
                                    var.StoreValue(iToAdd, processor);

                                        // we assign the current cluster to the var so the expression can evaluate it correctly.
                                    if (iBoolExp.GetBool(processor))
                                    {
                                        iRes.Add(iToAdd);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var i in list)
                    {
                        if (i != EmptyId)
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(i, out iFound))
                            {
                                // could already have been deleted cause we are outside of the lock.
                                var iToAdd = iFound as NeuronCluster;
                                if (iToAdd != null)
                                {
                                    var.StoreValue(iToAdd, processor);

                                        // we assign the current cluster to the var so the expression can evaluate it correctly.
                                    if (DoExpFilter(exp, processor))
                                    {
                                        iRes.Add(iToAdd);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Factories.Default.IDLists.Recycle(list);
            }
        }

        /// <summary>The do exp filter.</summary>
        /// <param name="exp">The exp.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool DoExpFilter(ResultExpression exp, Processor processor)
        {
            try
            {
                foreach (var iResItem in SolveResultExpNoStackChange(exp, processor))
                {
                    if (iResItem.ID != (ulong)PredefinedNeurons.True)
                    {
                        return false;
                    }
                }
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();

                    // we used the stack to get the result, so don't forget to free it again so that it can be reused.
            }

            return true;
        }
    }
}