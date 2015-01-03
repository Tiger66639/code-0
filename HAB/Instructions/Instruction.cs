// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Instruction.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for types that represent a <see cref="Neuron" /> which the
//   <see cref="Processor" /> interprets as basic instruction blocks that can
//   be executed and which don't return a result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A base class for types that represent a <see cref="Neuron" /> which the
    ///     <see cref="Processor" /> interprets as basic instruction blocks that can
    ///     be executed and which don't return a result.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Instructions can work directly on the data like searching, creating
    ///         neurons or link. There aren't any methods provided by the
    ///         <see cref="Processor" /> to do this. This is no problem even not for a
    ///         sub processor, fo which changes might need to be discarded. All the brain
    ///         data can buffer the changes in the sub processors so they are only
    ///         applied when the sub processor becomes main again.
    ///     </para>
    ///     <para>
    ///         Retrieving neurons from the <see cref="Brain" /> is also save, even in
    ///         sub processors. This is because the brain checks the current processor.
    ///     </para>
    /// </remarks>
    public abstract class Instruction : Neuron
    {
        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without any
        ///     specific number of values.
        /// </remarks>
        public abstract int ArgCount { get; }

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Instruction;
            }
        }

        #endregion

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public abstract void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args);

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current
        ///     <see cref="System.Object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the current
        ///     <see cref="System.Object" /> .
        /// </returns>
        public override string ToString()
        {
            var iRes = GetType().Name;
            return iRes.Replace("Instruction", string.Empty);
        }

        /// <summary>Builds the <paramref name="list"/> of lock requests for a link. This
        ///     does not include the meaning: don't think it is required, as long as
        ///     the link is between the same 2 neurons? <paramref name="to"/> check
        ///     further.</summary>
        /// <param name="list">The list.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="writeable">The writeable.</param>
        protected static void BuildLinkLocks(System.Collections.Generic.List<LockRequestInfo> list, 
            Neuron from, 
            Neuron to, 
            bool writeable)
        {
            var iReq = LockRequestInfo.Create();
            iReq.Neuron = from;
            iReq.Level = LockLevel.LinksOut;
            iReq.Writeable = writeable;
            list.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = to;
            iReq.Level = LockLevel.LinksIn;
            list.Add(iReq);
        }

        /// <summary>Builds a lock that can be used for info modifications. args[0] = from,
        ///     arg[1] = tro, from args[2] everything is locked as value.</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        protected LockRequestList BuildInfoLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            LockRequestInfo iFound;
            try
            {
                var iReq = LockRequestInfo.Create();
                iReq.Neuron = args[0];
                iReq.Level = LockLevel.LinksOut;
                iReq.Writeable = writeable;

                    // this also has to be writeable, to make certain that no other thread can write to the infolist at the same time. Only need to lock 'from', cause 1 is enough (it's always the same direction, always the same neuron)
                iRes.Add(iReq);

                iReq = LockRequestInfo.Create();
                iReq.Neuron = args[1];
                iReq.Level = LockLevel.LinksIn;
                iReq.Writeable = false;
                iRes.Add(iReq);

                iReq = LockRequestInfo.Create();
                iReq.Neuron = args[2];
                iReq.Level = LockLevel.Value;
                iReq.Writeable = false;
                iRes.Add(iReq);

                for (var i = 3; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false
                        || (iFound.Level != LockLevel.Value && iFound.Level != LockLevel.All))
                    {
                        // don't need  no duplicate key registrations
                        iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.Value;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock for the cluster and all the children.</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        protected static LockRequestList BuildClusterLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            try
            {
                LockRequestInfo iFound;

                var iReq = LockRequestInfo.Create();
                iReq.Neuron = args[0];
                iReq.Level = LockLevel.Children;
                iReq.Writeable = writeable;
                iRes.Add(iReq);

                for (var i = 1; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false
                        || (iFound.Level != LockLevel.Parents && iFound.Level != LockLevel.All))
                    {
                        // need to prevent duplicates, cause this can screw up the lock system.
                        iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.Parents;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock on the parents list for all the specified items.</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public static LockRequestList BuildParentsLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            try
            {
                LockRequestInfo iFound;

                for (var i = 0; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false)
                    {
                        // need to prevent duplicates, cause this can screw up the lock system.
                        var iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.Parents;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock on the parents list for all the specified items.</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public static LockRequestList BuildLinksOutLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            try
            {
                LockRequestInfo iFound;

                for (var i = 0; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false)
                    {
                        // need to prevent duplicates, cause this can screw up the lock system.
                        var iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.LinksOut;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock on the parents list for all the specified items.</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public static LockRequestList BuildLinksInLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            try
            {
                LockRequestInfo iFound;

                for (var i = 0; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false)
                    {
                        // need to prevent duplicates, cause this can screw up the lock system.
                        var iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.LinksIn;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock on the parents list for all the specified items.</summary>
        /// <param name="args">The args.</param>
        /// <param name="from">the index to start from in the list..</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        protected static LockRequestList BuildParentsLockFrom(System.Collections.Generic.IList<Neuron> args, 
            int from, 
            bool writeable)
        {
            var iRes = LockRequestList.Create();
            var iDict = LockRequestsFactory.CreateInfoDict();
            try
            {
                LockRequestInfo iFound;

                for (var i = from; i < args.Count; i++)
                {
                    if (iDict.TryGetValue(args[i], out iFound) == false)
                    {
                        // need to prevent duplicates, cause this can screw up the lock system.
                        var iReq = LockRequestInfo.Create();
                        iReq.Neuron = args[i];
                        iReq.Level = LockLevel.Parents;
                        iReq.Writeable = writeable;
                        iRes.Add(iReq);
                        iDict.Add(args[i], iReq);
                    }
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        /// <summary>Builds a lock for a link (the 3 first neurons of the arg are locked).</summary>
        /// <param name="args">The args.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        protected LockRequestList BuildLinkLock(System.Collections.Generic.IList<Neuron> args, bool writeable)
        {
            var iRes = LockRequestList.Create();

            var iReq = LockRequestInfo.Create();
            iReq.Neuron = args[0];
            iReq.Level = LockLevel.LinksOut;
            iReq.Writeable = writeable;
            iRes.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = args[1];
            iReq.Level = LockLevel.LinksIn;
            iReq.Writeable = writeable;
            iRes.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = args[2];
            iReq.Level = LockLevel.Value;
            iReq.Writeable = writeable;
            iRes.Add(iReq);

            return iRes;
        }

        /// <summary>a convenience function that instructions (with 1 argument) can use
        ///     when the shortcut failed, and they need to revert to the regular
        ///     'execute' call.</summary>
        /// <param name="handler"></param>
        /// <param name="exp"></param>
        protected void ExecuteForExp(Processor handler, ResultExpression exp)
        {
            var iArgs = handler.Mem.ArgumentStack.Push();

                // has to be on the stack cause ResultExpressions will calculate their value in this.
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                exp.GetValue(handler);
                Execute(handler, iArgs);
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }
        }

        /// <summary>Gets the value of an int-neuron or double-neuron as an int.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <returns>The <see cref="int?"/>.</returns>
        public static int? GetAsInt(Neuron toConvert)
        {
            var iInt = toConvert as IntNeuron;
            if (iInt != null)
            {
                return iInt.Value;
            }

            var iD = toConvert as DoubleNeuron;
            if (iD != null)
            {
                return (int)iD.Value;
            }

            return null;
        }

        /// <summary>Gets the <paramref name="value"/> of an int-neuron or double-neuron as
        ///     an int.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool TryGetAsInt(Neuron toConvert, out int value)
        {
            var iInt = toConvert as IntNeuron;
            if (iInt != null)
            {
                value = iInt.Value;
                return true;
            }

            var iD = toConvert as DoubleNeuron;
            if (iD != null)
            {
                value = (int)iD.Value;
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>tries to convert the specified neuron to a dobule value.</summary>
        /// <param name="toConvert"></param>
        /// <returns>The <see cref="double?"/>.</returns>
        public static double? GetAsDouble(Neuron toConvert)
        {
            var iD = toConvert as DoubleNeuron;
            if (iD != null)
            {
                return iD.Value;
            }

            var iInt = toConvert as IntNeuron;
            if (iInt != null)
            {
                return iInt.Value;
            }

            return null;
        }

        /// <summary>tries to convert the specified neuron to a dobule value.</summary>
        /// <param name="toConvert"></param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool TryGetAsDouble(Neuron toConvert, out double value)
        {
            var iD = toConvert as DoubleNeuron;
            if (iD != null)
            {
                value = iD.Value;
                return true;
            }

            var iInt = toConvert as IntNeuron;
            if (iInt != null)
            {
                value = iInt.Value;
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>calculates a <see langword="double"/> from the input<paramref name="neuron"/> (if expression-&gt; calculate, otherwise, if
        ///     static, convert to double)</summary>
        /// <param name="processor"></param>
        /// <param name="neuron"></param>
        /// <param name="val"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool CalculateDouble(Processor processor, Neuron neuron, out double val)
        {
            var iExp = neuron as ResultExpression;
            if (iExp != null)
            {
                if (iExp.CanGetDouble())
                {
                    val = iExp.GetDouble(processor);
                }
                else if (iExp.CanGetInt())
                {
                    val = iExp.GetInt(processor);
                }
                else if (iExp.ID == (ulong)PredefinedNeurons.ReturnValue)
                {
                    // it's a return value, check if there is an int on the return stack of the proc. need to do this here cause can't pass along the 'processor' which is required.
                    if (processor.Mem.LastReturnValues.Count > 0)
                    {
                        var iList = processor.Mem.LastReturnValues.Peek();
                        if (iList.Count == 1 && iList[0] is DoubleNeuron)
                        {
                            val = ((DoubleNeuron)iList[0]).Value;
                            Factories.Default.NLists.Recycle(processor.Mem.LastReturnValues.Pop());

                                // the value is used, so remove it.
                            return true;
                        }
                    }

                    val = 0;
                    return false;
                }
                else
                {
                    val = 0;
                    return false;
                }
            }
            else if (neuron is DoubleNeuron)
            {
                val = ((DoubleNeuron)neuron).Value;
            }
            else if (neuron is IntNeuron)
            {
                val = ((IntNeuron)neuron).Value;
            }
            else
            {
                val = 0;
                return false;
            }

            return true;
        }

        /// <summary>The calculate int.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool CalculateInt(Processor processor, Neuron neuron, out int val)
        {
            var iExp = neuron as ResultExpression;
            if (iExp != null)
            {
                if (iExp.CanGetInt())
                {
                    val = iExp.GetInt(processor);
                }
                else if (iExp.CanGetDouble())
                {
                    val = (int)iExp.GetDouble(processor);
                }
                else if (iExp.ID == (ulong)PredefinedNeurons.ReturnValue)
                {
                    // it's a return value, check if there is an int on the return stack of the proc. need to do this here cause can't pass along the 'processor' which is required.
                    if (processor.Mem.LastReturnValues.Count > 0)
                    {
                        var iList = processor.Mem.LastReturnValues.Peek();
                        if (iList.Count == 1 && iList[0] is IntNeuron)
                        {
                            val = ((IntNeuron)iList[0]).Value;
                            Factories.Default.NLists.Recycle(processor.Mem.LastReturnValues.Pop());

                                // the value is used, so remove it.
                            return true;
                        }
                    }

                    val = 0;
                    return false;
                }
                else
                {
                    val = 0;
                    return false;
                }
            }
            else if (neuron is IntNeuron)
            {
                val = ((IntNeuron)neuron).Value;
            }
            else if (neuron is DoubleNeuron)
            {
                val = (int)((DoubleNeuron)neuron).Value;
            }
            else
            {
                val = 0;
                return false;
            }

            return true;
        }

        /// <summary>The calculate bool.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool CalculateBool(Processor processor, Neuron neuron, out bool val)
        {
            var iExp = neuron as ResultExpression;
            if (iExp != null)
            {
                if (iExp.CanGetBool())
                {
                    val = iExp.GetBool(processor);
                }
                else if (iExp.ID == (ulong)PredefinedNeurons.ReturnValue)
                {
                    // it's a return value, check if there is an int on the return stack of the proc. need to do this here cause can't pass along the 'processor' which is required.
                    if (processor.Mem.LastReturnValues.Count > 0)
                    {
                        var iList = processor.Mem.LastReturnValues.Peek();
                        if (iList.Count == 1
                            && (iList[0] == Brain.Current.TrueNeuron || iList[0] == Brain.Current.FalseNeuron))
                        {
                            val = iList[0] == Brain.Current.TrueNeuron;
                            Factories.Default.NLists.Recycle(processor.Mem.LastReturnValues.Pop());

                                // the value is used, so remove it.
                            return true;
                        }
                    }

                    val = false;
                    return false;
                }
                else
                {
                    val = false;
                    return false;
                }
            }
            else if (neuron == Brain.Current.TrueNeuron)
            {
                val = true;
            }
            else if (neuron == Brain.Current.FalseNeuron)
            {
                val = false;
            }
            else
            {
                val = false;
                return false;
            }

            return true;
        }

        /// <summary>Loads/calculates the arguments into the <paramref name="result"/> lest<paramref name="until"/> the specified amount has been reached. upon
        ///     return, <paramref name="i"/> is positioned 1 after the last arg, ready
        ///     to be used again.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <param name="result">The <paramref name="i"/> args.</param>
        /// <param name="until">The until.</param>
        /// <param name="i">The i.</param>
        protected static void LoadArgsUntil(
            Processor processor, System.Collections.Generic.IList<Neuron> args, System.Collections.Generic.List<Neuron> result, 
            int until, 
            ref int i)
        {
            while (i < args.Count)
            {
                var iExp = args[i] as ResultExpression;
                if (iExp != null)
                {
                    iExp.GetValue(processor);
                }
                else
                {
                    result.Add(args[i]);
                }

                i++;
                if (result.Count >= until)
                {
                    break;
                }
            }
        }

        /// <summary>Loads/calculates the arguments into the <paramref name="result"/> list
        ///     from the specified location untill the end. upon return,<paramref name="i"/> is positioned 1 after the last arg, ready to be
        ///     used again.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <param name="result">The <paramref name="i"/> args.</param>
        /// <param name="i">The i.</param>
        protected static void LoadArgsFrom(
            Processor processor, System.Collections.Generic.IList<Neuron> args, System.Collections.Generic.List<Neuron> result, 
            ref int i)
        {
            while (i < args.Count)
            {
                var iExp = args[i] as ResultExpression;
                if (iExp != null)
                {
                    iExp.GetValue(processor);
                }
                else
                {
                    result.Add(args[i]);
                }

                i++;
            }
        }

        /// <summary>a small helper function to get the <paramref name="result"/> of an
        ///     argument-expression</summary>
        /// <param name="handler"></param>
        /// <param name="toProcess"></param>
        /// <param name="result"></param>
        protected static void GetExpResult(
            Processor handler, 
            Neuron toProcess, System.Collections.Generic.List<Neuron> result)
        {
            var iExp = toProcess as ResultExpression;
            if (iExp != null)
            {
                iExp.GetValue(handler); // the expression uses the processor.ArgumentlistStack to put it's result on.
            }
            else
            {
                result.Add(toProcess);
            }
        }
    }
}