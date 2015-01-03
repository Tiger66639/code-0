// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetChildAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the neuron at the specified index in the list of children of the
//   specified cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the neuron at the specified index in the list of children of the
    ///     specified cluster.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The cluster to return the child from</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 2: An IntNeuron, which contains the index position
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetChildAtInstruction)]
    public class GetChildAtInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetChildAtInstruction;
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
                return 2;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var iNeuron = list[0] as NeuronCluster;
                var iInt = GetAsInt(list[1]);
                if (iNeuron != null)
                {
                    if (iInt != null && iInt.HasValue)
                    {
                        return GetChildAt(iNeuron, iInt.Value);
                    }

                    LogService.Log.LogError(
                        "GetChildAtInstruction.GetValues", 
                        "Second argument should be an IntNeuron!");
                }
                else
                {
                    LogService.Log.LogError(
                        "GetChildAtInstruction.GetValues", 
                        "First arguement should be a NeuronCluster.");
                }
            }
            else
            {
                LogService.Log.LogError("GetChildAtInstruction.GetValues", "Invalid Nr of arguments specified");
            }

            return null;
        }

        /// <summary>The get child at.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetChildAt(NeuronCluster cluster, int index)
        {
            ulong iRes = 0;
            var iChildren = cluster.TryGetBufferedChildren();
            if (iChildren != null)
            {
                try
                {
                    if (iChildren.Count > index && index >= 0)
                    {
                        return iChildren[index] as Neuron;
                    }
                }
                finally
                {
                    cluster.ReleaseBufferedChildren(iChildren);
                }
            }
            else
            {
                LockManager.Current.RequestLock(cluster, LockLevel.Children, false);
                try
                {
                    var iList = cluster.ChildrenDirect;
                    if (iList.Count > index && index >= 0)
                    {
                        iRes = iList[index];
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(cluster, LockLevel.Children, false);
                }
            }

            if (iRes > 0)
            {
                return Brain.Current[iRes];
            }

            LogService.Log.LogError(
                "GetChildAtInstruction.GetValues", 
                string.Format("Index out of bounds for NeuronCluster {0}, index: {1}!", cluster.ID, index));
            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                NeuronCluster iCluster = null;
                var iInt = 0;
                var iHasInt = false;
                var iArgs = processor.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    var iExp = args[0] as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(processor);
                        if (iArgs.Count > 0)
                        {
                            iCluster = iArgs[0] as NeuronCluster;
                        }
                    }
                    else
                    {
                        iCluster = args[0] as NeuronCluster;
                    }

                    if (iCluster != null)
                    {
                        if (CalculateInt(processor, args[1], out iInt) == false)
                        {
                            // when can't do direct convert, try to get the value of the xpression (already know it isn't an int or double neuron, cause that would be caught by CalculateInt)
                            iExp = args[1] as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }

                            if (iArgs.Count >= 1)
                            {
                                var iconv = GetAsInt(iArgs[1]);
                                if (iconv.HasValue)
                                {
                                    iHasInt = true;
                                    iInt = iconv.Value;
                                }
                            }
                        }
                        else
                        {
                            iHasInt = true;
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetChildAtInstruction.GetValues", 
                            "First arguement should be a NeuronCluster.");
                    }
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }

                if (iHasInt)
                {
                    var iRes = GetChildAt(iCluster, iInt);
                    if (iRes != null)
                    {
                        processor.Mem.ArgumentStack.Peek().Add(iRes); // needs to be added to the result list
                    }
                }
                else
                {
                    LogService.Log.LogError("GetChildAtInstruction.GetValues", "Second argument should be an Int!");
                }

                return true;
            }

            return false;
        }

        /// <summary>checks if the value can be returned as a bool.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetBool(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as a <see langword="bool"/> value (argumnets still
        ///     need to be calculated).</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetBool(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args">The args, not calculated.</param>
        /// <returns><c>true</c> if this instance [can get int] the specified args;
        ///     otherwise, <c>false</c> .</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as an <see langword="int"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}