// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultStatement.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="ResultExpression" /> that retrieves the result from an
//   instruction (like IndexOf).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see cref="ResultExpression" /> that retrieves the result from an
    ///     instruction (like IndexOf).
    /// </summary>
    /// <remarks>
    ///     We use a general purpose expression that maps to instructions and don't
    ///     create custom expressions directly (for instance, we could have made an
    ///     IndexOfExpression to get the index of an item), except than you can't
    ///     easely extend it without changing the core. Instructions could be loaded
    ///     from another lib, but this can also be done with expressions?
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ResultStatement, typeof(Neuron))]
    public class ResultStatement : SimpleResultExpression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile ResultStatementData fWorkData;

        #endregion

        #region Inner types

        /// <summary>
        ///     Speed improvement: we cash some linkdata for faster execution.
        /// </summary>
        internal class ResultStatementData
        {
            /// <summary>The arguments cluster.</summary>
            public NeuronCluster ArgumentsCluster;

            /// <summary>The instruction.</summary>
            public ResultInstruction Instruction;
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ResultStatement"/> class. 
        ///     default constructor</summary>
        internal ResultStatement()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ResultStatement"/> class. Creates the proper initial links + registers the object with the
        ///     brain.</summary>
        /// <param name="inst">The instruction to execute.</param>
        /// <param name="args">The list of arguments for the instruction.</param>
        public ResultStatement(Instruction inst, NeuronCluster args)
        {
            Brain.Current.Add(this); // we need to be registered to the brain, otherwise the link wont' create.
            Link.Create(this, inst, (ulong)PredefinedNeurons.Instruction);
            Link.Create(this, args, (ulong)PredefinedNeurons.Arguments);
        }

        #endregion

        #region Prop

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        internal ResultStatementData WorkData
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadWorkData();
                }

                return fWorkData;
            }
        }

        /// <summary>The load work data.</summary>
        private void LoadWorkData()
        {
            lock (this)
            {
                if (fWorkData == null)
                {
                    // could be that another thread beat us to the punch because of the lock.
                    var iMemFac = Factories.Default;
                    var iWorkData = new ResultStatementData();
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks);
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Arguments)
                        {
                            iWorkData.ArgumentsCluster = iTemp[i].To as NeuronCluster;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Instruction)
                        {
                            iWorkData.Instruction = iTemp[i].To as ResultInstruction;
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp);
                    fWorkData = iWorkData;
                }
            }
        }

        #endregion

        #region IsTerminator

        /// <summary>
        ///     Gets a value indicating whether this expression causes the processor
        ///     to stop (an exit). This can be used by debuggers, to quickly check
        ///     this value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is terminator; otherwise, <c>false</c> .
        /// </value>
        public override bool IsTerminator
        {
            get
            {
                return WorkData.Instruction != null
                       && WorkData.Instruction.ID == (ulong)PredefinedNeurons.ExitSolveInstruction;
            }
        }

        #endregion

        #region Instruction

        /// <summary>
        ///     Gets/sets the <see cref="Neuron" /> of the instruction set item to
        ///     execute. This can not be a <see cref="ResultExpression" /> , only real
        ///     instruction references are allowed.
        /// </summary>
        /// <remarks>
        ///     This is usually a reference to the 1st linked object in the
        ///     <see cref="Neuron.ToLinks" /> .
        /// </remarks>
        public ResultInstruction Instruction
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Instruction) as ResultInstruction;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Instruction, value);
            }
        }

        #endregion

        #region Arguments

        /// <summary>
        ///     Gets the list containing all the argument values for the instruction.
        ///     These are <see cref="JaStDev.HAB.Neuron.ID" /> values that should
        ///     point to <see cref="ResultExpression" /> objects.
        /// </summary>
        /// <remarks>
        ///     This is a reference to the 2th linked object in the
        ///     <see cref="Neuron.ToLinks" /> , which should be a cluster.
        /// </remarks>
        public NeuronCluster ArgumentsCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Arguments) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Arguments, value);
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ResultStatement" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ResultStatement;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

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
            var iRes = new System.Text.StringBuilder();
            Neuron iTemp = Instruction;
            if (iTemp != null)
            {
                iRes.Append(iTemp);
            }
            else
            {
                iRes.Append("?");

                    // we format it with an error sign indicating we didn't find it, error log already done.
            }

            iRes.Append("(");
            var iArgs = ArgumentsCluster;
            if (iArgs != null)
            {
                var iArgsList = iArgs.Children;
                iArgsList.Lock();
                try
                {
                    for (var i = 0; i < iArgsList.CountUnsafe; i++)
                    {
                        if (i > 0)
                        {
                            iRes.Append(',');
                        }

                        iTemp = Brain.Current[iArgsList.GetUnsafe(i)]; // still potential deadlock.
                        if (iTemp != null)
                        {
                            iRes.Append(iTemp);
                        }
                        else
                        {
                            iRes.Append(string.Format("(!{0}!)", i));
                        }
                    }
                }
                finally
                {
                    iArgsList.Dispose();
                }
            }

            iRes.Append(")");
            return iRes.ToString();
        }

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="processor"></param>
        internal override void GetValue(Processor processor)
        {
            var iOrArgs = WorkData.ArgumentsCluster;
            var iFastest = WorkData.Instruction as IExecResultStatement;
            if (iFastest != null)
            {
                var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                try
                {
                    if (iFastest.GetValue(processor, iItems))
                    {
                        return; // found a value, exit.
                    }
                }
                finally
                {
                    iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            // when we get here, couldn't use 'fastest' so do the slower method.
            var iInst = WorkData.Instruction;
            if (iInst != null)
            {
                var iArgs = processor.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    if (iOrArgs != null)
                    {
                        var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                        try
                        {
                            foreach (var iToSolve in iItems)
                            {
                                var iExp = iToSolve as ResultExpression;
                                if (iExp != null)
                                {
                                    iExp.GetValue(processor);
                                }
                                else
                                {
                                    iArgs.Add(iToSolve);
                                }
                            }
                        }
                        finally
                        {
                            iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                        }
                    }

                    processor.Mem.ArgumentStack.Reverse();
                    try
                    {
                        iInst.GetValues(processor, iArgs);

                            // do this before removing iArgs from the stack, otherwise we get stack polution.
                    }
                    finally
                    {
                        processor.Mem.ArgumentStack.Reverse();
                    }
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }
            }
            else
            {
                LogService.Log.LogError(
                    "ResultStatement.Execute", 
                    string.Format(
                        "Failed to execute The InstructionId '{0}' doesn't map to an instruction object.", 
                        WorkData.Instruction));
            }
        }

        #region int, double and bool speed optimisations.

        /// <summary>returns if this object can return an bool.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetBool()
        {
            var iInst = WorkData.Instruction as IExecResultStatement;
            if (iInst != null)
            {
                var iItems = WorkData.ArgumentsCluster.GetBufferedChildren<Neuron>();
                try
                {
                    return iInst.CanGetBool(iItems);
                }
                finally
                {
                    WorkData.ArgumentsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            return WorkData.Instruction is ICalculateBool;
        }

        /// <summary>gets the <see langword="bool"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool GetBool(Processor processor)
        {
            var iOrArgs = WorkData.ArgumentsCluster;
            var iFastest = WorkData.Instruction as IExecResultStatement;
            if (iFastest != null)
            {
                var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                try
                {
                    return iFastest.GetBool(processor, iItems);
                }
                finally
                {
                    iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            var iInst = (ICalculateBool)WorkData.Instruction;
            System.Diagnostics.Debug.Assert(iInst != null);
            var iArgs = processor.Mem.ArgumentStack.Push();
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                if (iOrArgs != null)
                {
                    var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                    try
                    {
                        foreach (var iToSolve in iItems)
                        {
                            var iExp = iToSolve as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }
                            else
                            {
                                iArgs.Add(iToSolve);
                            }
                        }
                    }
                    finally
                    {
                        iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                    }
                }

                return iInst.CalculateBool(processor, iArgs);

                    // do this before removing iArgs from the stack, otherwise we get stack polution.
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();
            }
        }

        /// <summary>returns if this object can return an int.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetInt()
        {
            var iInst = WorkData.Instruction as IExecResultStatement;
            if (iInst != null)
            {
                var iItems = WorkData.ArgumentsCluster.GetBufferedChildren<Neuron>();
                try
                {
                    return iInst.CanGetInt(iItems);
                }
                finally
                {
                    WorkData.ArgumentsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            return WorkData.Instruction is ICalculateInt || WorkData.Instruction is ICalculateDouble;
        }

        /// <summary>Gets the <see langword="int"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int GetInt(Processor processor)
        {
            var iOrArgs = WorkData.ArgumentsCluster;
            var iFastest = WorkData.Instruction as IExecResultStatement;
            if (iFastest != null)
            {
                var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                try
                {
                    return iFastest.GetInt(processor, iItems);
                }
                finally
                {
                    iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            var iArgs = processor.Mem.ArgumentStack.Push();
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                if (iOrArgs != null)
                {
                    var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                    try
                    {
                        foreach (var iToSolve in iItems)
                        {
                            var iExp = iToSolve as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }
                            else
                            {
                                iArgs.Add(iToSolve);
                            }
                        }
                    }
                    finally
                    {
                        iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                    }
                }

                var iInst = WorkData.Instruction as ICalculateInt;
                if (iInst != null)
                {
                    return iInst.CalculateInt(processor, iArgs);

                        // do this before removing iArgs from the stack, otherwise we get stack polution.
                }
                else
                {
                    var iDoubleInst = (ICalculateDouble)WorkData.Instruction;
                    return (int)iDoubleInst.CalculateDouble(processor, iArgs);

                        // do this before removing iArgs from the stack, otherwise we get stack polution.
                }
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();
            }
        }

        /// <summary>returns if this object can return an double.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetDouble()
        {
            var iInst = WorkData.Instruction as IExecResultStatement;
            if (iInst != null)
            {
                var iItems = WorkData.ArgumentsCluster.GetBufferedChildren<Neuron>();
                try
                {
                    return iInst.CanGetDouble(WorkData.ArgumentsCluster.GetBufferedChildren<Neuron>());
                }
                finally
                {
                    WorkData.ArgumentsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            return WorkData.Instruction is ICalculateDouble || WorkData.Instruction is ICalculateInt;

                // an int can be converted to a double
        }

        /// <summary>Gets the <see langword="double"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public override double GetDouble(Processor processor)
        {
            var iOrArgs = WorkData.ArgumentsCluster;
            var iFastest = WorkData.Instruction as IExecResultStatement;
            if (iFastest != null)
            {
                var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                try
                {
                    return iFastest.GetDouble(processor, iItems);
                }
                finally
                {
                    iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }

            var iArgs = processor.Mem.ArgumentStack.Push();
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                if (iOrArgs != null)
                {
                    var iItems = iOrArgs.GetBufferedChildren<Neuron>();
                    try
                    {
                        foreach (var iToSolve in iItems)
                        {
                            var iExp = iToSolve as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }
                            else
                            {
                                iArgs.Add(iToSolve);
                            }
                        }
                    }
                    finally
                    {
                        iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iItems);
                    }
                }

                var iInst = WorkData.Instruction as ICalculateDouble;
                if (iInst != null)
                {
                    return iInst.CalculateDouble(processor, iArgs);

                        // do this before removing iArgs from the stack, otherwise we get stack polution.
                }
                else
                {
                    var iIntInst = (ICalculateInt)WorkData.Instruction;
                    return iIntInst.CalculateInt(processor, iArgs);

                        // do this before removing iArgs from the stack, otherwise we get stack polution.
                }
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();
            }
        }

        #endregion

        /// <summary>Asks the expression to load all the data that it requires for being
        ///     executed. This is used as an optimisation so that the code-data can be
        ///     pre-fetched. This is to solve a problem with slower platforms where
        ///     the first input can be handled slowly.</summary>
        /// <param name="alreadyProcessed">The already Processed.</param>
        protected internal override void LoadCode(System.Collections.Generic.HashSet<Neuron> alreadyProcessed)
        {
            if (Brain.Current.IsInitialized && fWorkData == null && alreadyProcessed.Contains(this) == false)
            {
                alreadyProcessed.Add(this);
                LoadWorkData();
                var iItems = WorkData.ArgumentsCluster.GetBufferedChildren<Neuron>();
                try
                {
                    foreach (var i in iItems)
                    {
                        var iExp = i as Expression;
                        if (iExp != null)
                        {
                            iExp.LoadCode(alreadyProcessed);
                        }
                    }
                }
                finally
                {
                    WorkData.ArgumentsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }
            }
        }

        /// <summary>small optimizer, checks if the code is loaded alrady or not. This is
        ///     used to see if a start point needs to be loaded or not, whithout
        ///     having to set up mem all the time.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected internal override bool IsCodeLoaded()
        {
            return fWorkData != null;
        }

        #endregion
    }
}