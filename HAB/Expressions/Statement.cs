// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Statement.cs" company="">
//   
// </copyright>
// <summary>
//   Contains 1 instruction (expression) that the <see cref="Processor" /> can
//   execute. It contains a reference to the instruction object and the
//   arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Contains 1 instruction (expression) that the <see cref="Processor" /> can
    ///     execute. It contains a reference to the instruction object and the
    ///     arguments.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Statement, typeof(Neuron))]
    public class Statement : Expression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile StatementData fWorkData;

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
        ///     execute. This can not be a <see cref="ResultExpression" /> , in which
        ///     case it is first resolved, only real instruction references are
        ///     allowed.
        /// </summary>
        /// <remarks>
        ///     This is usually a reference to the 1st linked object in the
        ///     <see cref="Neuron.ToLinks" /> .
        /// </remarks>
        public Instruction Instruction
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Instruction) as Instruction;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Instruction, value);
            }
        }

        #endregion

        #region ArgumentsCluster

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

        #region Arguments

        /// <summary>
        ///     Gets a readonly list with <see cref="Expression" /> s that are the
        ///     convertion of <see cref="JaStDev.HAB.Statement.ArgumentsCluster" /> .
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Neuron> Arguments
        {
            get
            {
                var iCluster = ArgumentsCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Neuron> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Neuron>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Neuron>(iExps);
                    }

                    LogService.Log.LogError(
                        "Statement.Arguments", 
                        string.Format("Failed to convert arguments list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExpressionsBlock" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Statement;
            }
        }

        #endregion

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

        /// <summary>The to string.</summary>
        /// <returns>A <see cref="System.String"/> that represents the current<see cref="System.Object"/> .</returns>
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
                iRes.Append("(!EmptyRef!)");
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

        /// <summary>The execute.</summary>
        /// <param name="handler">The handler.</param>
        protected internal override void Execute(Processor handler)
        {
            var iInst = WorkData.Instruction;
            if (iInst != null)
            {
                var iOrArgs = WorkData.ArgumentsCluster;
                bool iFoundStat;
                var iStat = iInst as IExecStatement;
                if (iStat != null)
                {
                    var iMemFac = Factories.Default;
                    System.Collections.Generic.IList<Neuron> iArgsList;
                    var iCleanUp = false;
                    if (iOrArgs != null)
                    {
                        iArgsList = iOrArgs.GetBufferedChildren<Neuron>();
                    }
                    else
                    {
                        iCleanUp = true;
                        iArgsList = iMemFac.NLists.GetBuffer();

                            // get a temp empty list to pass along to the instruction.
                    }

                    try
                    {
                        iFoundStat = iStat.Perform(handler, iArgsList);
                    }
                    finally
                    {
                        if (iCleanUp)
                        {
                            iMemFac.NLists.Recycle((System.Collections.Generic.List<Neuron>)iArgsList);
                        }
                        else
                        {
                            iOrArgs.ReleaseBufferedChildren((System.Collections.IList)iArgsList);
                        }
                    }
                }
                else
                {
                    iFoundStat = false;
                }

                if (iFoundStat == false)
                {
                    var iArgs = handler.Mem.ArgumentStack.Push();

                        // has to be on the stack, so that it can store the result of the expressions.
                    if (iInst.ArgCount > -1)
                    {
                        // reserve enough space, when -1, we don't know the required space, so take default of 10.
                        if (iArgs.Capacity < iInst.ArgCount)
                        {
                            iArgs.Capacity = iInst.ArgCount;
                        }
                        else if (iArgs.Capacity < 10)
                        {
                            iArgs.Capacity = 10;
                        }
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
                                        iExp.GetValue(handler);

                                            // the expression uses the processor.ArgumentlistStack to put it's result on.
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

                        iInst.Execute(handler, iArgs);

                            // do before removing the arguments, othetwise the list can be reused by the instruction.
                    }
                    finally
                    {
                        handler.Mem.ArgumentStack.Pop();
                    }
                }
            }
            else
            {
                LogService.Log.LogError(
                    "Statement.Execute", 
                    string.Format(
                        "Failed to execute The InstructionId '{0}' doesn't map to an instruction object.", 
                        WorkData.Instruction));
            }
        }

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

        #region Inner types

        /// <summary>
        ///     Speed improvement: we cash some linkdata for faster execution.
        /// </summary>
        private class StatementData
        {
            /// <summary>The arguments cluster.</summary>
            public NeuronCluster ArgumentsCluster;

            /// <summary>The instruction.</summary>
            public Instruction Instruction;
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="Statement"/> class. 
        ///     default constructor</summary>
        internal Statement()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Statement"/> class. Creates the proper initial links and registers it to the brain.</summary>
        /// <param name="inst">The instruction to execute.</param>
        /// <param name="args">The list of arguments for the instruction.</param>
        public Statement(Instruction inst, NeuronCluster args)
        {
            Brain.Current.Add(this);
            var iNew = new Link(inst, this, (ulong)PredefinedNeurons.Instruction);
            iNew = new Link(args, this, (ulong)PredefinedNeurons.Arguments);
        }

        #endregion

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        private StatementData WorkData
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
                    var iWorkData = new StatementData();
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks);
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Arguments)
                        {
                            iWorkData.ArgumentsCluster = (NeuronCluster)iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Instruction)
                        {
                            iWorkData.Instruction = (Instruction)iTemp[i].To;
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp);
                    fWorkData = iWorkData;
                }
            }
        }

        #endregion
    }
}