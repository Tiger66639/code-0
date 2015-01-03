// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallFrame.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data required for a single list of code for execution.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Contains all the data required for a single list of code for execution.
    /// </summary>
    /// <remarks>
    ///     This class is used in a stack to allow for conditional statements. When we do a processor split, we need
    ///     to be able to continue at the correct call location (inside the correct conditional, in such a way that we
    ///     can go back again to code at a higher level).
    ///     <para>
    ///         Doesn't store variable dictionaries, cause they have a different scope. We do store when a dictionary was added
    ///         to a frame, this is to allow the removal of the dictionary using a stack algorithm.
    ///     </para>
    /// </remarks>
    public class CallFrame
    {
        /// <summary>The f locals buffer.</summary>
        private System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>> fLocalsBuffer;

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal virtual void Release()
        {
            if (Code != null && ExecSource is NeuronCluster)
            {
                // if it's not a cluster, it's from a neuron who's links are being executed. in this case, the release is done by the procesor.
                ((NeuronCluster)ExecSource).ReleaseBufferedChildren((System.Collections.IList)Code);
            }

            Factories.Default.CallFrames.ReleaseCallFrame(this);
        }

        /// <summary>The clear locals.</summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void ClearLocals()
        {
            throw new System.NotImplementedException();
        }

        #region ctor

        // public CallFrame(NeuronCluster execSource)
        // {
        // ExecSource = execSource;
        // CodeListType = ExecListType.Children;
        // Code = execSource.GetBufferedChildren<Expression>();
        // if (Code == null)
        // throw new InvalidOperationException(String.Format("Failed to transform children of cluster {0} to a list of expressions, evaluate branch aborted.", execSource.ToString()));
        // }

        /// <summary>The create.</summary>
        /// <param name="execSource">The exec source.</param>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public static CallFrame Create(NeuronCluster execSource)
        {
            var iRes = Factories.Default.CallFrames.GetCallFrame();
            iRes.InitCallFrame(execSource);
            return iRes;
        }

        /// <summary>The init call frame.</summary>
        /// <param name="execSource">The exec source.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void InitCallFrame(NeuronCluster execSource)
        {
            ExecSource = execSource;
            CodeListType = ExecListType.Children;
            Code = execSource.GetBufferedChildren<Expression>();
            if (Code == null)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to transform children of cluster {0} to a list of expressions, evaluate branch aborted.", 
                        execSource));
            }
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public virtual CallFrame CreateInstance()
        {
            return Factories.Default.CallFrames.GetCallFrame();
        }

        #endregion

        #region prop

        /// <summary>
        ///     Gets or sets the index of the next expression to execute, found in the <see cref="CallFrame.Code" /> list.
        /// </summary>
        /// <value>The next exp.</value>
        public int NextExp { get; set; }

        /// <summary>
        ///     Gets or sets the code to execute in this frame.
        /// </summary>
        /// <value>The code.</value>
        public System.Collections.Generic.IList<Expression> Code { get; set; }

        /// <summary>
        ///     Gets or sets the type of the code list (rules, children, actions, conditional).
        /// </summary>
        /// <value>The type of the code list.</value>
        public ExecListType CodeListType { get; set; }

        /// <summary>
        ///     Gets or sets the neuron that defined the code (through rules, actions, children, conditionalparts,...)
        /// </summary>
        /// <value>The exec source.</value>
        public Neuron ExecSource { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this frame caused a new Variables dictionary. False by default.
        /// </summary>
        /// <value><c>true</c> if [caused new var dict]; otherwise, <c>false</c>.</value>
        public bool CausedNewVarDict { get; set; }

        /// <summary>
        ///     Gets or sets the list of locks that were created for this frame. This is used by the
        ///     <see cref="Processor.PopFrame" />
        ///     to release the lock again.
        /// </summary>
        /// <value>The locks.</value>
        internal LockRequestList Locks { get; set; }

        /// <summary>
        ///     gets the list of locals that need to be restored after this frame is done.
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>> LocalsBuffer
        {
            get
            {
                if (fLocalsBuffer == null)
                {
                    fLocalsBuffer =
                        new System.Collections.Generic.List
                            <System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>>();
                }

                return fLocalsBuffer;
            }
        }

        /// <summary>
        ///     gets wether this frame has any locals that need to be restored after the frame is done.
        /// </summary>
        public bool HasLocals
        {
            get
            {
                return fLocalsBuffer != null && fLocalsBuffer.Count > 0;
            }
        }

        #endregion

        #region functions

        /// <summary>
        ///     resets the props to the default value, so the object can be reused.
        /// </summary>
        public virtual void Reset()
        {
            NextExp = 0;
            Code = null;
            ExecSource = null;
            CausedNewVarDict = default(bool);
            Locks = null;
            if (HasLocals)
            {
                // just to be save.
                LocalsBuffer.Clear();
            }
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <remarks>public version, makes certain that variables values are correctly popped.</remarks>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame was removed, otherwise false.</returns>
        public bool UpdateCallFrameStack(Processor proc)
        {
            proc.CheckStopRequested();

            // we do a catch round the update so that anything failing in this process doesn't fuck up the stack.  When
            // something wrong happens, we simply pollitly let the caller know he needs to move on to the next frame.
            try
            {
                var iRes = InternalUpdateCallFrameStack(proc);
                if (iRes)
                {
                    proc.PopFrame();
                }

                return iRes;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("CallFrame.UpdateCallFrameStack", e.ToString());
                return true;
            }
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <remarks>Internal version, allows for complete override on handling the CallFrameStack, withouth having to force
        ///     descendents to rewrite the popping of variable values.</remarks>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        protected virtual bool InternalUpdateCallFrameStack(Processor proc)
        {
            return NextExp >= Code.Count;
        }

        /// <summary>Duplicates this instance.</summary>
        /// <param name="index">The index.</param>
        /// <param name="splitter">The splitter.</param>
        /// <returns>A new callFrame of the same type, containing the same data.</returns>
        public CallFrame Duplicate(int index, ProcessorSplitter splitter)
        {
            var iRes = CreateInstance();

                // we can use the factory here. It's local to the processor that started the thread, so it will provide the recycled items, but the new processor gets it and will eventually start recycling it.
            CopyTo(iRes);
            if (HasLocals)
            {
                CopyLocals(iRes, index, splitter);
            }

            return iRes;
        }

        /// <summary>The copy locals.</summary>
        /// <param name="copyTo">The copy to.</param>
        /// <param name="index">The index.</param>
        /// <param name="splitter">The splitter.</param>
        private void CopyLocals(CallFrame copyTo, int index, ProcessorSplitter splitter)
        {
            var iMemFac = Factories.Default;
            foreach (var i in LocalsBuffer)
            {
                System.Collections.Generic.List<Neuron> iList = null;
                if (i.Value != null)
                {
                    switch (i.Key.SplitReactionID)
                    {
                        case 0:
                            iList = iMemFac.NLists.GetBuffer();

                                // this is for a new var value, so always use the factory, it will be recycled anyways.
                            foreach (var iNeuron in i.Value)
                            {
                                if (iNeuron.IsDeleted == false)
                                {
                                    System.Collections.Generic.List<Neuron> iFound;
                                    if (splitter.Clones.TryGetValue(iNeuron, out iFound))
                                    {
                                        iList.Add(iFound[index]);
                                    }
                                    else
                                    {
                                        iList.Add(iNeuron);
                                    }
                                }
                            }

                            break;
                        case (ulong)PredefinedNeurons.shared:

                            // shared locals: make certain that no other processor changes the content of the variable: different processors can remove the frame at different times, this should not hav an effect on the variables.
                            iList = i.Value;
                            break;
                        case (ulong)PredefinedNeurons.Copy:
                            iList = iMemFac.NLists.GetBuffer();

                                // this is for a new var value, so always use the factory, it will be recycled anyways.
                            iList.AddRange(i.Value);
                            break;
                        case (ulong)PredefinedNeurons.Duplicate:
                            iList = iMemFac.NLists.GetBuffer();

                                // this is for a new var value, so always use the factory, it will be recycled anyways.
                            foreach (var iNeuron in i.Value)
                            {
                                if (iNeuron.IsDeleted == false)
                                {
                                    System.Collections.Generic.List<Neuron> iFound;
                                    if (splitter.Clones.TryGetValue(iNeuron, out iFound))
                                    {
                                        iList.Add(iFound[index]);
                                    }
                                    else
                                    {
                                        LogService.Log.LogError(
                                            "processor", 
                                            "internal error: can't found split value for variable.");
                                    }
                                }
                            }

                            break;
                    }
                }

                if (iList != null)
                {
                    var iNew =
                        new System.Collections.Generic.KeyValuePair<Local, System.Collections.Generic.List<Neuron>>(
                            i.Key, 
                            iList);
                    copyTo.LocalsBuffer.Add(iNew);
                }
            }
        }

        /// <summary>Copies the content of the callframe to the target. Warning: locks aren't copied over, they are
        ///     thread local.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected virtual void CopyTo(CallFrame copyTo)
        {
            copyTo.CausedNewVarDict = CausedNewVarDict;
            copyTo.Code = Code;
            copyTo.CodeListType = CodeListType;
            copyTo.ExecSource = ExecSource;
            copyTo.NextExp = NextExp;
            if (Locks != null)
            {
                copyTo.Locks = LockRequestList.Duplicate(Locks);
            }
        }

        #endregion
    }

    /// <summary>The conditional frame.</summary>
    public abstract class ConditionalFrame : CallFrame
    {
        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public abstract Neuron ConditionType { get; }

        /// <summary>
        ///     the list of extra statement to be executed before the conditionals are evaluated.
        /// </summary>
        public NeuronCluster ExtraStatements { get; set; }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            ((ConditionalFrame)copyTo).ExtraStatements = ExtraStatements;
        }
    }

    /// <summary>
    ///     Base class for all frames that make use of all the conditions (if, case, loop, caseloop).
    /// </summary>
    public abstract class ConditionsFrame : ConditionalFrame
    {
        /// <summary>
        ///     Gets or sets the conditions to evaluate as case items.
        /// </summary>
        /// <value>The conditions.</value>
        public System.Collections.Generic.IList<ConditionalExpression> Conditions { get; set; }

        /// <summary>
        ///     Gets or sets the conditions cluster, used for debugging.
        /// </summary>
        /// <value>The conditions cluster.</value>
        public NeuronCluster ConditionsCluster { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ConditionsFrame"/> class.</summary>
        /// <param name="statement">The statement.</param>
        protected void Init(ConditionalStatement statement)
        {
            Conditions = statement.WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            ConditionsCluster = statement.WorkData.ConditionsCluster;
            CodeListType = ExecListType.Conditional;
            ExecSource = ConditionsCluster;
            ExtraStatements = statement.WorkData.StatementsCluster;
            System.Diagnostics.Debug.Assert(Conditions != null);
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var icopyTo = (ConditionsFrame)copyTo;
            icopyTo.Conditions = Conditions;
            icopyTo.ConditionsCluster = ConditionsCluster;
        }
    }

    /// <summary>The if frame.</summary>
    public class IfFrame : ConditionsFrame
    {
        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.Normal];
            }
        }

        /// <summary>Creates the specified statement.</summary>
        /// <param name="statement">The statement.</param>
        /// <returns>The <see cref="IfFrame"/>.</returns>
        public static IfFrame Create(ConditionalStatement statement)
        {
            var iRes = Factories.Default.IfFrames.GetCallFrame();
            iRes.Init(statement);
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.IfFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (Conditions != null)
            {
                ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)Conditions);
            }

            Factories.Default.IfFrames.ReleaseCallFrame(this);
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        /// <remarks>Internal version, allows for complete override on handling the CallFrameStack, withouth having to force
        ///     descendents to rewrite the popping of variable values.</remarks>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            if (NextExp != -1)
            {
                if (ExtraStatements != null && NextExp == 0)
                {
                    // first time that 'if' is activated, but there is a code list that needs to be performed first, so that that before checking the conditions.
                    var iFrame = CallInstCallFrame.CreateCallInst(ExtraStatements);

                        // need to be a function frame, so we can use a return statement to get out.
                    proc.PushFrame(iFrame);
                    NextExp = -2; // switch so that next time, we do the case itself.
                    return false;
                }

                NextExp = -1; // this makes certain that the frame is only run 1 time.
                for (var iIndex = 0; iIndex < Conditions.Count; iIndex++)
                {
                    var i = Conditions[iIndex];
                    if (proc.EvaluateCondition(null, i, iIndex))
                    {
                        var iCluster = i.WorkData.StatementsCluster;
                        if (iCluster != null)
                        {
                            var iFrame = Create(iCluster);
                            proc.PushFrame(iFrame);
                            return false;
                        }

                        return true;

                            // if the condition evaluated to true, but there was no code cluster, we simply need to exit the 'if' statemetn.
                    }
                }
            }

            return true;

                // when we get here, we went through the entire loop without calling return, so no evalulation passed.
        }
    }

    /// <summary>The case frame.</summary>
    public class CaseFrame : IfFrame
    {
        /// <summary>
        ///     Gets or sets the case item to evaluate during the loop.
        /// </summary>
        /// <value>The case item.</value>
        public ResultExpression CaseItem { get; set; }

        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.Case];
            }
        }

        /// <summary>Creates the object.</summary>
        /// <param name="statement">The statement.</param>
        /// <returns>The <see cref="CaseFrame"/>.</returns>
        public static CaseFrame CreateCase(ConditionalStatement statement)
        {
            var iRes = Factories.Default.CaseFrames.GetCallFrame();
            iRes.Init(statement);
            iRes.CaseItem = statement.WorkData.CaseItem;
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.CaseFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (Conditions != null)
            {
                ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)Conditions);
            }

            Factories.Default.CaseFrames.ReleaseCallFrame(this);
        }

        /// <summary>
        ///     resets the props to the default value, so the object can be reused.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            CaseItem = null;
        }

        /// <summary>The internal update call frame stack.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            if (NextExp != -1)
            {
                if (NextExp == 0 && ExtraStatements != null)
                {
                    // first time that 'if' is activated, but there is a code list that needs to be performed first, so that that before checking the conditions.
                    var iFrame = CallInstCallFrame.CreateCallInst(ExtraStatements);

                        // need to be a function frame, so we can use a return statement to get out.
                    proc.PushFrame(iFrame);
                    NextExp = -2; // switch so that next time, we do the case itself.
                    return false;
                }

                NextExp = -1; // this makes certain that the frame is only run 1 time.
                if (CaseItem != null)
                {
                    System.Collections.Generic.IEnumerable<Neuron> iCaseValues =
                        Neuron.SolveResultExpNoStackChange(CaseItem, proc);
                    if (iCaseValues != null)
                    {
                        try
                        {
                            for (var iIndex = 0; iIndex < Conditions.Count; iIndex++)
                            {
                                var i = Conditions[iIndex];
                                if (proc.EvaluateCondition(iCaseValues, i, iIndex))
                                {
                                    var iFrame = Create(i.WorkData.StatementsCluster);
                                    proc.PushFrame(iFrame);
                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            proc.Mem.ArgumentStack.Pop();
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "CaseFrame.InternalUpdateCallFrameStack", 
                            "Can't perform case: CaseItem is null after execution!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Processor.PerformCase", "Can't perform case: no case item defined!");
                }
            }

            return true;

                // when we get here, we went through the entire loop without calling return, so no evalulation passed.
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var iCopyTo = (CaseFrame)copyTo;
            iCopyTo.CaseItem = CaseItem;
        }
    }

    /// <summary>
    ///     CallFrame for looped frames.  Stores some more data in order to handle conditionals correctly.
    /// </summary>
    public class LoopedCallFrame : ConditionsFrame
    {
        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.Looped];
            }
        }

        /// <summary>Creates the specified statement.</summary>
        /// <param name="statement">The statement.</param>
        /// <returns>The <see cref="LoopedCallFrame"/>.</returns>
        public static LoopedCallFrame Create(ConditionalStatement statement)
        {
            var iRes = Factories.Default.LoopFrames.GetCallFrame();
            iRes.Init(statement);
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.LoopFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (Conditions != null)
            {
                ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)Conditions);
            }

            Factories.Default.LoopFrames.ReleaseCallFrame(this);
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            if (ExtraStatements != null && NextExp == 0)
            {
                var iFrame = CallInstCallFrame.CreateCallInst(ExtraStatements);

                    // need to be a function frame, so we can use a return statement to get out.
                proc.PushFrame(iFrame);
                NextExp = -1; // switch so that next time, we do the case itself.
                return false;
            }

            NextExp = 0; // after 1 of the pahts has been done, make certain we do the bit in front of the loop again.
            for (var iIndex = 0; iIndex < Conditions.Count; iIndex++)
            {
                var i = Conditions[iIndex];
                if (proc.EvaluateCondition(null, i, iIndex))
                {
                    var iFrame = Create(i.WorkData.StatementsCluster);
                    proc.PushFrame(iFrame);
                    return false;
                }
            }

            return true;

                // when we get here, we went through the entire loop without calling return, so no evalulation passed.
        }
    }

    /// <summary>
    ///     CallFrame for looped frames.  Stores some more data in order to handle conditionals correctly.
    /// </summary>
    public class CaseLoopedCallFrame : LoopedCallFrame
    {
        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.CaseLooped];
            }
        }

        /// <summary>
        ///     Gets or sets the case item to evaluate during the loop.
        /// </summary>
        /// <value>The case item.</value>
        public ResultExpression CaseItem { get; set; }

        /// <summary>Creates the specified statement.</summary>
        /// <param name="statement">The statement.</param>
        /// <returns>The <see cref="CaseLoopedCallFrame"/>.</returns>
        public static CaseLoopedCallFrame CreateCaseLoop(ConditionalStatement statement)
        {
            var iRes = Factories.Default.CaseLoopFrames.GetCallFrame();
            iRes.Init(statement);
            iRes.CaseItem = statement.WorkData.CaseItem;
            System.Diagnostics.Debug.Assert(iRes.CaseItem != null);
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.CaseLoopFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (Conditions != null)
            {
                ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)Conditions);
            }

            Factories.Default.CaseLoopFrames.ReleaseCallFrame(this);
        }

        /// <summary>The internal update call frame stack.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            if (ExtraStatements != null && NextExp == 0)
            {
                var iFrame = CallInstCallFrame.CreateCallInst(ExtraStatements);

                    // need to be a function frame, so we can use a return statement to get out.
                proc.PushFrame(iFrame);
                NextExp = -1; // switch so that next time, we do the case itself.
                return false;
            }

            NextExp = 0; // after each loop, do the bit in front of the loop again.
            System.Collections.Generic.IEnumerable<Neuron> iCaseValues = Neuron.SolveResultExpNoStackChange(
                CaseItem, 
                proc);
            if (iCaseValues != null)
            {
                try
                {
                    for (var iIndex = 0; iIndex < Conditions.Count; iIndex++)
                    {
                        var i = Conditions[iIndex];
                        if (proc.EvaluateCondition(iCaseValues, i, iIndex))
                        {
                            var iNew = Create(i.WorkData.StatementsCluster);

                                // need to call the code of the conditional part.
                            proc.PushFrame(iNew);
                            return false;
                        }
                    }
                }
                finally
                {
                    proc.Mem.ArgumentStack.Pop();
                }

                proc.PopFrame();

                    // when we get here, we went through the entire loop without calling return, so no evalulation passed.
                return true;
            }

            LogService.Log.LogError(
                "CaseLoopedCallFrame.PerformCase", 
                "Can't perform case: CaseItem is null after execution!");
            proc.PopFrame();
            return true;
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var icopyTo = (CaseLoopedCallFrame)copyTo;
            icopyTo.CaseItem = CaseItem;
        }
    }

    /// <summary>
    ///     CallFrame for ForEach frames.  Stores some more data in order to handle conditionals correctly.
    /// </summary>
    public class ForEachCallFrame : ConditionalFrame
    {
        /// <summary>Creates the specified loop item.</summary>
        /// <param name="loopItem">The loop item.</param>
        /// <param name="items">The items.</param>
        /// <param name="execSource">The exec source.</param>
        /// <returns>The <see cref="ForEachCallFrame"/>.</returns>
        public static ForEachCallFrame Create(
            Variable loopItem, System.Collections.Generic.List<Neuron> items, 
            NeuronCluster execSource)
        {
            var iRes = Factories.Default.ForEachFrames.GetCallFrame();
            iRes.Code = execSource.GetBufferedChildren<Expression>();
            if (iRes.Code == null)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to transform children of cluster {0} to a list of expressions, evaluate branch aborted.", 
                        execSource));
            }

            iRes.LoopItem = loopItem;
            iRes.Items = items;
            iRes.ExecSource = execSource;
            iRes.CodeListType = ExecListType.Children;
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.ForEachFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (Items != null)
            {
                Factories.Default.NLists.Recycle(Items);

                    // the list of items can be reused cause it was fully assigned to this object and this is released
                Items = null;
            }

            if (Code != null)
            {
                ((NeuronCluster)ExecSource).ReleaseBufferedChildren((System.Collections.IList)Code);
            }

            Factories.Default.ForEachFrames.ReleaseCallFrame(this);
        }

        #region prop

        /// <summary>
        ///     Gets or sets the loop item that stores the neuron in the collection.
        /// </summary>
        /// <value>The loop item.</value>
        public Variable LoopItem { get; set; }

        /// <summary>
        ///     Gets or sets the items to loop through.
        /// </summary>
        /// <value>The items.</value>
        public System.Collections.Generic.List<Neuron> Items { get; set; }

        /// <summary>
        ///     Gets or sets the index of the next item that should be returned.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a break was performed.
        ///     This is used when the callframe gets updated, so we don't advance to the next item in the list, but simply to the
        ///     next code item.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [did break]; otherwise, <c>false</c>.
        /// </value>
        public bool DidBreak { get; set; }

        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.ForEach];
            }
        }

        #endregion

        #region functions

        /// <summary>
        ///     resets the props to the default value, so the object can be reused.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            LoopItem = null;
            Items = null;
            Index = 0;
            DidBreak = default(bool);
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            // a foreach doesn't have an extra bit of code in front of it to be performed, this is rendered in front of it.
            if (DidBreak == false)
            {
                if (Index < Items.Count)
                {
                    LoopItem.StoreValue(Items[Index++], proc);
                    NextExp = 0; // need to reset the exp pos, otherwise we can't loop.
                    return false;
                }

                return true;
            }

            DidBreak = false;
            return NextExp >= Code.Count;
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var icopyTo = (ForEachCallFrame)copyTo;
            icopyTo.Index = Index;
            icopyTo.Items = Factories.Default.NLists.GetBuffer();

                // new List<Neuron>(Items);  //there was still a problem with this? causing the list to be spread accross multiple threads?
            icopyTo.Items.AddRange(Items); // need to make a copy, otherwise the other list might get reset
            icopyTo.LoopItem = LoopItem;
        }

        #endregion
    }

    /// <summary>The for query call frame.</summary>
    public class ForQueryCallFrame : ConditionalFrame
    {
        /// <summary>Creates the specified loop item.</summary>
        /// <param name="loopItems">The loop item.</param>
        /// <param name="source">The source.</param>
        /// <param name="execSource">The exec source.</param>
        /// <returns>The <see cref="ForQueryCallFrame"/>.</returns>
        public static ForQueryCallFrame Create(System.Collections.Generic.List<Neuron> loopItems, 
            IForEachSource source, 
            NeuronCluster execSource)
        {
            var iRes = Factories.Default.ForQueryFrames.GetCallFrame();
            iRes.Code = execSource.GetBufferedChildren<Expression>();
            if (iRes.Code == null)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to transform children of cluster {0} to a list of expressions, evaluate branch aborted.", 
                        execSource));
            }

            iRes.LoopItems = loopItems;
            iRes.ItemsSource = source;
            iRes.Enum = source.GetEnumerator();
            if (iRes.Enum == null)
            {
                LogService.Log.LogError(
                    "Query.Select", 
                    "The query has no datasource, can't perform a select statement.");
            }

            iRes.ExecSource = execSource;
            iRes.CodeListType = ExecListType.Children;
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.ForQueryFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            if (LoopItems != null)
            {
                Factories.Default.NLists.Recycle(LoopItems);

                    // the list of items can be reused cause it was fully assigned to this object and this is released
                LoopItems = null;
            }

            if (Code != null)
            {
                ((NeuronCluster)ExecSource).ReleaseBufferedChildren((System.Collections.IList)Code);
            }

            Factories.Default.ForQueryFrames.ReleaseCallFrame(this);
        }

        /// <summary>
        ///     moves the cursor to the end, so that the loop stops. This is used to perform a 'break' statement.
        /// </summary>
        internal void GotoEnd()
        {
            ItemsSource.GotoEnd(Enum);
        }

        #region prop

        /// <summary>
        ///     Gets or sets the loop item that stores the neuron in the collection.
        /// </summary>
        /// <value>The loop item.</value>
        public System.Collections.Generic.List<Neuron> LoopItems { get; set; }

        /// <summary>
        ///     Gets or sets the items source that provides the data to loop through.
        ///     This is stored so we can attempt to perform a duplicate during a split.
        /// </summary>
        /// <value>
        ///     The items source.
        /// </value>
        public IForEachSource ItemsSource { get; set; }

        /// <summary>
        ///     Gets or sets the items to loop through.
        /// </summary>
        /// <value>The items.</value>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a break was performed.
        ///     This is used when the callframe gets updated, so we don't advance to the next item in the list, but simply to the
        ///     next code item.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [did break]; otherwise, <c>false</c>.
        /// </value>
        public bool RecordFound { get; set; }

        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.QueryLoop];
            }
        }

        #endregion

        #region functions

        /// <summary>
        ///     resets the props to the default value, so the object can be reused.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            LoopItems = null;
            ItemsSource = null;
            Enum = null;
            RecordFound = default(bool);
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            var iMemFac = Factories.Default;
            GotoNextRec:

            // a foreach doesn't have an extra bit of code in front of it to be performed, this is rendered in front of it.
            if (RecordFound == false)
            {
                // at first run, this is true, so the first part of the if gets done.
                RecordFound = true;
                if (Enum != null && Enum.MoveNext())
                {
                    // make certain that there was a datasource selected and that it has items.
                    var iEnum = Enum.Current.GetEnumerator();
                    var iCount = 0;
                    Variable iStoreAfterLoop = null;
                    System.Collections.Generic.List<Neuron> iVals = null;
                    while (iEnum.MoveNext())
                    {
                        // we walk through each value of the line, the last variable gets all the remaining of the line.
                        var iVar = (Variable)LoopItems[iCount];
                        if (iCount < LoopItems.Count - 1)
                        {
                            iVar.StoreValue(iEnum.Current, proc);
                            iCount++;
                        }
                        else
                        {
                            if (iVals == null)
                            {
                                iStoreAfterLoop = iVar;
                                iVals = iMemFac.NLists.GetBuffer();
                            }

                            iVals.Add(iEnum.Current);
                        }
                    }

                    if (iVals != null)
                    {
                        iStoreAfterLoop.StoreValue(iVals, proc);
                    }

                    NextExp = 0; // need to reset the exp pos, otherwise we can't loop.
                    return false;
                }

                return true;
            }

            if (NextExp >= Code.Count)
            {
                RecordFound = false;

                    // wanted to jump a little below, at 'if(enum != ...), but that doesnt' appear to work for a goto.
                goto GotoNextRec;
            }

            return false;
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var icopyTo = (ForQueryCallFrame)copyTo;
            icopyTo.LoopItems = Factories.Default.NLists.GetBuffer();

                // new List<Neuron>(Items);  //there was still a problem with this? causing the list to be spread accross multiple threads?
            icopyTo.LoopItems.AddRange(LoopItems); // need to make a copy, otherwise the other list might get reset
            icopyTo.ItemsSource = ItemsSource;
            icopyTo.Enum = ItemsSource.Duplicate(Enum);
        }

        #endregion
    }

    /// <summary>
    ///     CallFrame for looped frames.  Stores some more data in order to handle conditionals correctly.
    /// </summary>
    public class UntilCallFrame : ConditionalFrame
    {
        /// <summary>
        ///     Gets the type of the condition.
        /// </summary>
        /// <value>The type of the condition.</value>
        public override Neuron ConditionType
        {
            get
            {
                return Brain.Current[(ulong)PredefinedNeurons.Until];
            }
        }

        /// <summary>
        ///     Gets or sets the condition to check
        /// </summary>
        /// <value>The condition.</value>
        public ConditionalExpression Condition { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this is the first run of the frame or not.  This is important
        ///     because an 'untill' condition needs to skip the first test.
        /// </summary>
        /// <value><c>true</c> if [first run]; otherwise, <c>false</c>.</value>
        public bool FirstRun { get; set; }

        /// <summary>
        ///     so that the untill frame knows if the extra bit of code (for function calls), has already been executed.
        /// </summary>
        public bool DidExtaBit { get; set; }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            Factories.Default.UntilFrames.ReleaseCallFrame(this);
        }

        /// <summary>
        ///     resets the props to the default value, so the object can be reused.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            FirstRun = default(bool);
            Condition = null;
            DidExtaBit = default(bool);
        }

        /// <summary>Updates the call frame stack for the specified processor with regards to the rules of the current frame.
        ///     This is used to handle different types of stack walking (like if, case, loop, foreach,...).</summary>
        /// <param name="proc">The proc.</param>
        /// <returns><c>true</c> when a frame needs to be removed, otherwise false.</returns>
        protected override bool InternalUpdateCallFrameStack(Processor proc)
        {
            if (FirstRun == false)
            {
                if (ExtraStatements != null && DidExtaBit == false)
                {
                    DidExtaBit = true;
                    var iFrame = CallInstCallFrame.CreateCallInst(ExtraStatements);

                        // need to be a function frame, so we can use a return statement to get out.
                    proc.PushFrame(iFrame);
                    NextExp = -1; // switch so that next time, we do the case itself.
                    return false;
                }

                if (proc.EvaluateCondition(null, Condition, 0) == false)
                {
                    return true;
                }

                DidExtaBit = false; // next run, make certain that the extra bit gets done.
                NextExp = 0; // need to reset the exp pos, otherwise we can't loop.
                return false;
            }

            FirstRun = false;
            return false;
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var icopyTo = (UntilCallFrame)copyTo;
            icopyTo.Condition = Condition;
            icopyTo.FirstRun = FirstRun;
            icopyTo.DidExtaBit = DidExtaBit;
        }

        #region ctor

        /// <summary>Creates the specified condition.</summary>
        /// <param name="condition">The condition.</param>
        /// <returns>The <see cref="UntilCallFrame"/>.</returns>
        public static UntilCallFrame Create(ConditionalExpression condition)
        {
            var iRes = Factories.Default.UntilFrames.GetCallFrame();
            iRes.Condition = condition;
            iRes.ExecSource = condition.WorkData.StatementsCluster;
            iRes.Code = condition.WorkData.StatementsCluster.GetBufferedChildren<Expression>();
            if (iRes.Code == null)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to transform children of cluster {0} to a list of expressions, evaluate branch aborted.", 
                        iRes.ExecSource));
            }

            iRes.CodeListType = ExecListType.Children;
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.UntilFrames.GetCallFrame();
        }

        #endregion
    }

    /// <summary>
    ///     a callframe for expression blocks.
    /// </summary>
    public class ExpressionBlockFrame : CallFrame
    {
        /// <summary>
        ///     Gets the <see cref="ExpressionsBlock" /> that created this frame.
        /// </summary>
        /// <remarks>
        ///     This property isn't used directly by the processor, it is provided for debuggers, so they can show a trace
        ///     from who caused the frame.
        /// </remarks>
        /// <value>The block.</value>
        public ExpressionsBlock Block { get; private set; }

        /// <summary>Creates the specified exec source.</summary>
        /// <param name="execSource">The exec source.</param>
        /// <param name="block">The block.</param>
        /// <returns>The <see cref="ExpressionBlockFrame"/>.</returns>
        public static ExpressionBlockFrame Create(NeuronCluster execSource, ExpressionsBlock block)
        {
            var iRes = Factories.Default.ExpressionFrames.GetCallFrame();
            iRes.InitCallFrame(execSource);
            iRes.Block = block;
            return iRes;
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.ExpressionFrames.GetCallFrame();
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            Factories.Default.ExpressionFrames.ReleaseCallFrame(this);
        }

        /// <summary>The copy to.</summary>
        /// <param name="copyTo">The copy to.</param>
        protected override void CopyTo(CallFrame copyTo)
        {
            base.CopyTo(copyTo);
            var iFrame = copyTo as ExpressionBlockFrame;
            if (iFrame != null)
            {
                iFrame.Block = Block;
            }
        }

        /// <summary>The reset.</summary>
        public override void Reset()
        {
            base.Reset();
            Block = null;
        }
    }

    /// <summary>
    ///     We create this class so we can identify callframes that were originated from a call instruction so
    ///     we can do the return correctly.
    ///     (also so we can build display paths easely in a designer).
    /// </summary>
    public class CallInstCallFrame : CallFrame
    {
        /// <summary>Creates the specified exec source.</summary>
        /// <param name="execSource">The exec source.</param>
        /// <returns>The <see cref="CallInstCallFrame"/>.</returns>
        public static CallInstCallFrame CreateCallInst(NeuronCluster execSource)
        {
            var iRes = Factories.Default.CallInstFrames.GetCallFrame();
            iRes.InitCallFrame(execSource);
            return iRes;
        }

        /// <summary>
        ///     releases this object so that it can be reused (to get pressure from the GC).
        /// </summary>
        internal override void Release()
        {
            Factories.Default.CallInstFrames.ReleaseCallFrame(this);
        }

        /// <summary>this gets a new object for the duplication process. It's a way to make the frame factory virtual.</summary>
        /// <returns>The <see cref="CallFrame"/>.</returns>
        public override CallFrame CreateInstance()
        {
            return Factories.Default.CallInstFrames.GetCallFrame();
        }
    }
}