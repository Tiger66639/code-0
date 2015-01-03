// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetChildrenRangeInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns a range of the children of a <see cref="NeuronCluster" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns a range of the children of a <see cref="NeuronCluster" /> .
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 the cluster from which to return all the children. -the lower boundery of
    ///                 the range -the Nr of items to copy.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetChildrenRangeInstruction)]
    public class GetChildrenRangeInstruction : MultiResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenRangeInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetChildrenRangeInstruction;
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
                return 3;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 3)
            {
                var iCluster = list[0] as NeuronCluster;
                var iLower = list[1] as IntNeuron;
                var iUpper = list[2] as IntNeuron;
                if (iCluster != null)
                {
                    if (iLower != null)
                    {
                        if (iUpper != null)
                        {
                            var iMemFac = Factories.Default;
                            System.Collections.Generic.List<ulong> iTemp = null;
                            var iRes = processor.Mem.ArgumentStack.Peek();
                            var iList = iCluster.Children;
                            iList.Lock();
                            try
                            {
                                if (CheckRange(iCluster.ChildrenDirect, iLower.Value, iUpper.Value))
                                {
                                    iTemp = iMemFac.IDLists.GetBuffer(iList.CountUnsafe);
                                    for (var i = 0; i < iUpper.Value; i++)
                                    {
                                        iTemp.Add(iCluster.ChildrenDirect[iLower.Value + i]);
                                    }
                                }
                            }
                            finally
                            {
                                iList.Dispose();
                            }

                            if (iTemp != null)
                            {
                                foreach (var i in iTemp)
                                {
                                    iRes.Add(Brain.Current[i]);
                                }

                                iMemFac.IDLists.Recycle(iTemp);
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetChildrenRange.GetValues", 
                                "Third argument should be an IntNeuron!");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("GetChildrenRange.GetValues", "Second argument should be an IntNeuron!");
                    }
                }
                else
                {
                    LogService.Log.LogError("GetChildrenRange.GetValues", "First argument should be a NeuronCluster!");
                }
            }
            else
            {
                LogService.Log.LogError("GetChildrenRange.GetValues", "No arguments specified");
            }
        }

        /// <summary>The check range.</summary>
        /// <param name="iList">The i list.</param>
        /// <param name="iLower">The i lower.</param>
        /// <param name="iUpper">The i upper.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CheckRange(System.Collections.Generic.IList<ulong> iList, int iLower, int iUpper)
        {
            if (iLower < 0)
            {
                LogService.Log.LogError("GetChildrenRange.GetValues", "Second argument: lower range can't be below 0!");
                return false;
            }

            if (iLower > iList.Count)
            {
                LogService.Log.LogError(
                    "GetChildrenRange.GetValues", 
                    "Second argument: lower range can't bigger then child count!");
                return false;
            }

            if (iUpper + iLower > iList.Count)
            {
                LogService.Log.LogError(
                    "GetChildrenRange.GetValues", 
                    "Third argument: upper range can't be more then child count from lower range!");
                return false;
            }

            return true;
        }

        #region IExecResultStatement Members

        #region internal types

        /// <summary>The get value data.</summary>
        private class GetValueData
        {
            /// <summary>The handler.</summary>
            public Processor Handler;

            /// <summary>The i args.</summary>
            public System.Collections.Generic.List<Neuron> iArgs;

            /// <summary>The input args.</summary>
            public System.Collections.Generic.IList<Neuron> InputArgs;

            /// <summary>The i stack pushed.</summary>
            public bool iStackPushed;

            /// <summary>The lower.</summary>
            public int Lower;

            /// <summary>The upper.</summary>
            public int Upper;
        }

        #endregion

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            var iData = new GetValueData();
            System.Collections.Generic.List<Neuron> iRes;
            iData.InputArgs = args;
            iData.Handler = handler;
            NeuronCluster iCluster;
            var iMemFac = Factories.Default;
            System.Collections.Generic.List<ulong> iTemp = null;
            if (args.Count > 2)
            {
                try
                {
                    iRes = handler.Mem.ArgumentStack.Peek();
                    if (GetInt(iData, 1, "first", out iData.Lower))
                    {
                        return true;
                    }

                    if (GetInt(iData, 2, "second", out iData.Upper))
                    {
                        return true;
                    }

                    if (args.Count == 3 && args[2] is NeuronCluster)
                    {
                        iCluster = args[0] as NeuronCluster;
                    }
                    else
                    {
                        if (iData.iStackPushed == false)
                        {
                            iData.iArgs = handler.Mem.ArgumentStack.Push();

                                // has to be on the stack cause ResultExpressions will calculate their value in this.
                        }
                        else
                        {
                            iData.iArgs.Clear();

                                // we clear the result list cause we might be adding some extra items to it, don't want to have the index value interfere with it.
                        }

                        GetExpResult(handler, args[0], iData.iArgs);
                        iCluster = iData.iArgs[0] as NeuronCluster;
                    }

                    if (iCluster != null)
                    {
                        LockManager.Current.RequestLock(iCluster, LockLevel.Children, false);
                        try
                        {
                            var iList = iCluster.ChildrenDirect;
                            if (CheckRange(iList, iData.Lower, iData.Upper))
                            {
                                iTemp = iMemFac.IDLists.GetBuffer(iList.Count);
                                for (var i = 0; i < iData.Upper; i++)
                                {
                                    iTemp.Add(iList[iData.Lower + i]);
                                }
                            }
                        }
                        finally
                        {
                            LockManager.Current.ReleaseLock(iCluster, LockLevel.Children, false);
                        }

                        foreach (var i in iTemp)
                        {
                            iRes.Add(Brain.Current[i]);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetChildrenRange.GetValues", 
                            "First argument should be a NeuronCluster!");
                    }
                }
                finally
                {
                    if (iData.iStackPushed)
                    {
                        handler.Mem.ArgumentStack.Pop();
                    }

                    if (iTemp != null)
                    {
                        iMemFac.IDLists.Recycle(iTemp);
                    }
                }

                return true;
            }

            return false; // let the system try to first calculate the arguments, see if we get an int then.
        }

        /// <summary>Gets the specified argument as an <see langword="int"/> (possibly
        ///     performing a calucation).</summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="errorText"></param>
        /// <param name="res">The res.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool GetInt(GetValueData data, int index, string errorText, out int res)
        {
            if (CalculateInt(data.Handler, data.InputArgs[index], out res) == false)
            {
                if (data.iStackPushed == false)
                {
                    data.iStackPushed = true;
                    data.iArgs = data.Handler.Mem.ArgumentStack.Push();

                        // has to be on the stack cause ResultExpressions will calculate their value in this.
                }
                else
                {
                    data.iArgs.Clear();
                }

                GetExpResult(data.Handler, data.InputArgs[index], data.iArgs);
                var iIntRes = GetAsInt(data.iArgs[0]);
                if (iIntRes.HasValue == false)
                {
                    res = 0;
                    LogService.Log.LogError(
                        "GetAtInstruction.InternalGetValue", 
                        errorText + " argument should be an Int.");
                    return true;
                }

                res = iIntRes.Value;
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