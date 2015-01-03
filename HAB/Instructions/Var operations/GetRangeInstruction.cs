// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetRangeInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets a range of items starting at the specified index. args: -the lower
//   boundery -the nr of items. -the first item of the list (can be supplied
//   using a var)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets a range of items starting at the specified index. args: -the lower
    ///     boundery -the nr of items. -the first item of the list (can be supplied
    ///     using a var)
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.GetRangeInstruction)]
    public class GetRangeInstruction : MultiResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRangeInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetRangeInstruction;
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
                var iLower = GetAsInt(list[0]);
                var iUpper = GetAsInt(list[1]);
                if (iLower.HasValue)
                {
                    if (iUpper.HasValue)
                    {
                        var iRes = processor.Mem.ArgumentStack.Peek();
                        if (CheckRange(list, iLower.Value, iUpper.Value, 2))
                        {
                            for (var i = 0; i < iUpper.Value; i++)
                            {
                                iRes.Add(list[iLower.Value + i]);
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetRangeInstruction.GetValues", 
                            "Second argument should be an IntNeuron!");
                    }
                }
                else
                {
                    LogService.Log.LogError("GetRangeInstruction.GetValues", "First argument should be an IntNeuron!");
                }
            }
            else
            {
                LogService.Log.LogError("GetRangeInstruction.GetValues", "No arguments specified");
            }
        }

        /// <summary>checks the range of the arguments.</summary>
        /// <param name="iList"></param>
        /// <param name="iLower"></param>
        /// <param name="iUpper"></param>
        /// <param name="offset">if the range values were part of the list, this value is used to
        ///     offset the nr of list items so that the range values are no longer
        ///     part of it.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CheckRange(System.Collections.Generic.IList<Neuron> iList, 
            int iLower, 
            int iUpper, 
            int offset)
        {
            if (iLower < 0)
            {
                LogService.Log.LogError(
                    "GetRangeInstruction.GetValues", 
                    "first argument: lower range can't be below 0!");
                return false;
            }

            if (iLower > iList.Count - offset)
            {
                LogService.Log.LogError(
                    "GetRangeInstruction.GetValues", 
                    "first argument: lower range can't bigger then child count!");
                return false;
            }

            if (iUpper + iLower > iList.Count - offset)
            {
                LogService.Log.LogError(
                    "GetRangeInstruction.GetValues", 
                    "second argument: upper range can't be more then child count from lower range!");
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

            /// <summary>The i lower.</summary>
            public int iLower;

            /// <summary>The input args.</summary>
            public System.Collections.Generic.IList<Neuron> InputArgs;

            /// <summary>The i stack pushed.</summary>
            public bool iStackPushed;

            /// <summary>The i upper.</summary>
            public int iUpper;
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
            if (args.Count > 2)
            {
                try
                {
                    iRes = handler.Mem.ArgumentStack.Peek();
                    if (GetInt(iData, 0, "first", out iData.iLower))
                    {
                        return true;
                    }

                    if (GetInt(iData, 1, "second", out iData.iUpper))
                    {
                        return true;
                    }

                    if (args.Count == 3 && args[2] is Variable)
                    {
                        // small improvement: if the arg is a var-> faster calculation.
                        iData.iArgs = ((Variable)args[2]).ExtractValue(handler);
                        System.Diagnostics.Debug.Assert(iData.iArgs != null);
                    }
                    else
                    {
                        if (iData.iStackPushed == false)
                        {
                            iData.iStackPushed = true;
                            iData.iArgs = handler.Mem.ArgumentStack.Push();

                                // has to be on the stack cause ResultExpressions will calculate their value in this.
                        }
                        else
                        {
                            iData.iArgs.Clear();

                                // we clear the result list cause we might be adding some extra items to it, don't want to have the index value interfere with it.
                        }

                        for (var i = 2; i < args.Count; i++)
                        {
                            GetExpResult(handler, args[i], iData.iArgs);
                        }
                    }

                    if (CheckRange(iData.iArgs, iData.iLower, iData.iUpper, 0))
                    {
                        for (var i = 0; i < iData.iUpper; i++)
                        {
                            iRes.Add(iData.iArgs[iData.iLower + i]);
                        }
                    }
                }
                finally
                {
                    if (iData.iStackPushed)
                    {
                        handler.Mem.ArgumentStack.Pop();
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