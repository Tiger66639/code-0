// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoolExpression.cs" company="">
//   
// </copyright>
// <summary>
//   <see langword="delegate" /> used for output events without arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     <see langword="delegate" /> used for output events without arguments.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate bool CalculateBoolEventHandler(Processor processor);

    /// <summary>
    ///     A result expression that can be evaluated to a boolean.
    /// </summary>
    /// <remarks>
    ///     Only 1 <see langword="operator" /> allowed.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.BoolExpression, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.And, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Or, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Contains, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.NotContains, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Different, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Equal, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.BiggerOrEqual, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Smaller, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.SmallerOrEqual, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Bigger, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.False, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.True, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Clusters, typeof(Neuron))]
    public class BoolExpression : ResultExpression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile BoolExpData fWorkData;

        #endregion

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

        /// <summary>We push the result of the expression on the stack, this is normally a<see cref="PredefinedNeuron.True"/> or<see cref="PredefinedNeuron.False"/></summary>
        /// <param name="handler"></param>
        protected internal override void Execute(Processor handler)
        {
            if (WorkData.CalculateBool(handler))
            {
                handler.Push(Brain.Current.TrueNeuron);
            }
            else
            {
                handler.Push(Brain.Current.FalseNeuron);
            }
        }

        /// <summary>Returns the value of the compare as a list containing a<see langword="true"/> or <see langword="false"/> node. provided to
        ///     solve general purpose result expressions.</summary>
        /// <param name="processor">The processor.</param>
        internal override void GetValue(Processor processor)
        {
            if (WorkData.CalculateBool(processor))
            {
                processor.Mem.ArgumentStack.Peek().Add(Brain.Current.TrueNeuron);
            }
            else
            {
                processor.Mem.ArgumentStack.Peek().Add(Brain.Current.FalseNeuron);
            }
        }

        /// <summary>The get bool regular.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetBoolRegular(Processor processor)
        {
            var iArgStackCount = 0;
            System.Collections.Generic.IList<Neuron> iLeft = SolveResultExpNoStackChange(WorkData.LeftPart, processor);
            System.Collections.Generic.IList<Neuron> iOpStart = SolveResultExpNoStackChange(
                WorkData.Operator, 
                processor);
            if (iLeft != null && iOpStart != null)
            {
                iArgStackCount = 2;
            }
            else if (iLeft != null || iOpStart != null)
            {
                iArgStackCount = 1;
            }

            try
            {
                System.Collections.Generic.IList<Neuron> iOp;
                iOp = (iOpStart != null) ? iOpStart : null;
                if (iLeft != null)
                {
                    // there were no left parts to evaluate, this is a state of error, so log and return that the condition failed.
                    if (iOp != null && iOp.Count > 0)
                    {
                        var iO = iOp[0];
                        if (iO.ID == (ulong)PredefinedNeurons.Or)
                        {
                            return CompareOr(iLeft, processor, ref iArgStackCount);
                        }
                        else if (iO.ID == (ulong)PredefinedNeurons.And)
                        {
                            return CompareAnd(iLeft, processor, iO, ref iArgStackCount);
                        }
                        else
                        {
                            System.Collections.Generic.IList<Neuron> iRight =
                                SolveResultExpNoStackChange(WorkData.RightPart, processor);
                            if (iRight != null)
                            {
                                // SolveResultExp can return null, in which case no list was created on the stack.
                                iArgStackCount++;
                            }

                            if (iO.ID == (ulong)PredefinedNeurons.Contains)
                            {
                                // we treat the contains seperatly, cause this needs a different check type.
                                return (iRight != null) ? EvaluateContains(iLeft, iRight) : false;
                            }
                            else if (iO.ID == (ulong)PredefinedNeurons.NotContains)
                            {
                                // not contains, is same as contains, but reverse the value. This is a lot faster than using 2 bool expressions in the neural code.
                                return (iRight != null) ? EvaluateNotContains(iLeft, iRight) : true;
                            }
                            else if (Enumerable.Count(iLeft) == 0)
                            {
                                return EvaluateEmptyLeft(iO, iRight);
                            }
                            else if (iO.ID == (ulong)PredefinedNeurons.Equal)
                            {
                                return CompareEqual(iLeft, iRight, iO);
                            }
                            else if (iO.ID == (ulong)PredefinedNeurons.Different)
                            {
                                return CompareDifferent(iLeft, iRight, iO);
                            }
                            else
                            {
                                return CompareNormal(iLeft, iRight, iO);
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "BoolExpression.Evaluate", 
                            string.Format("No valid operator part found during evaluation of: {0}", this));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "BoolExpression.Evaluate", 
                        string.Format("No valid left part to evaluate for: {0}", this));
                }

                return false;
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop(iArgStackCount); // calculated 3 lists, so remove them.
            }
        }

        /// <summary>optimisation: when the left and right can return a bool, we take a
        ///     shortcut and don't use the stack.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetBoolFromBoolInterface(Processor processor)
        {
            IGetBool iLeft = WorkData.LeftPart;
            IGetBool iRight = WorkData.RightPart;
            var iOp = SolveSingleResultExp(WorkData.Operator, processor);
            if (iLeft != null && iRight != null)
            {
                switch (iOp.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return iLeft.GetBool(processor) == iRight.GetBool(processor);
                    case (ulong)PredefinedNeurons.And:
                        return iLeft.GetBool(processor) && iRight.GetBool(processor);
                    case (ulong)PredefinedNeurons.Or:
                        return iLeft.GetBool(processor) || iRight.GetBool(processor);
                    case (ulong)PredefinedNeurons.Different:
                        return iLeft.GetBool(processor) != iRight.GetBool(processor);
                    default:
                        throw new System.InvalidOperationException();
                }
            }

            throw new System.InvalidOperationException();
        }

        /// <summary>Optimisation: when both left and right can return an int, we don't use
        ///     the stack.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetBoolFromIntInterface(Processor processor)
        {
            IGetInt iLeft = WorkData.LeftPart;
            IGetInt iRight = WorkData.RightPart;
            var iOp = SolveSingleResultExp(WorkData.Operator, processor);
            if (iLeft != null && iRight != null)
            {
                switch (iOp.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return iLeft.GetInt(processor) == iRight.GetInt(processor);
                    case (ulong)PredefinedNeurons.Bigger:
                        return iLeft.GetInt(processor) > iRight.GetInt(processor);
                    case (ulong)PredefinedNeurons.BiggerOrEqual:
                        return iLeft.GetInt(processor) >= iRight.GetInt(processor);
                    case (ulong)PredefinedNeurons.Smaller:
                        return iLeft.GetInt(processor) < iRight.GetInt(processor);
                    case (ulong)PredefinedNeurons.SmallerOrEqual:
                        return iLeft.GetInt(processor) <= iRight.GetInt(processor);
                    case (ulong)PredefinedNeurons.Different:
                        return iLeft.GetInt(processor) != iRight.GetInt(processor);
                    default:
                        throw new System.InvalidOperationException();
                }
            }

            throw new System.InvalidOperationException();
        }

        /// <summary>Optimisation: when both left and right can return an int, we don't use
        ///     the stack.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetBoolFromDoubleInterface(Processor processor)
        {
            IGetDouble iLeft = WorkData.LeftPart;
            IGetDouble iRight = WorkData.RightPart;
            var iOp = SolveSingleResultExp(WorkData.Operator, processor);
            if (iLeft != null && iRight != null)
            {
                switch (iOp.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return iLeft.GetDouble(processor) == iRight.GetDouble(processor);
                    case (ulong)PredefinedNeurons.Bigger:
                        return iLeft.GetDouble(processor) > iRight.GetDouble(processor);
                    case (ulong)PredefinedNeurons.BiggerOrEqual:
                        return iLeft.GetDouble(processor) >= iRight.GetDouble(processor);
                    case (ulong)PredefinedNeurons.Smaller:
                        return iLeft.GetDouble(processor) < iRight.GetDouble(processor);
                    case (ulong)PredefinedNeurons.SmallerOrEqual:
                        return iLeft.GetDouble(processor) <= iRight.GetDouble(processor);
                    case (ulong)PredefinedNeurons.Different:
                        return iLeft.GetDouble(processor) != iRight.GetDouble(processor);
                    default:
                        throw new System.InvalidOperationException();
                }
            }

            throw new System.InvalidOperationException();
        }

        /// <summary>Compares the equal.</summary>
        /// <param name="iLeft">The i left.</param>
        /// <param name="iRight">The i Right.</param>
        /// <param name="iO">The i O.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CompareEqual(System.Collections.Generic.IList<Neuron> iLeft, System.Collections.Generic.IList<Neuron> iRight, 
            Neuron iO)
        {
            if (iRight.Count != iLeft.Count)
            {
                return false;
            }

            for (var i = 0; i < iLeft.Count; i++)
            {
                if (iLeft[i].CompareWith(iRight[i], iO) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares the different.</summary>
        /// <param name="iLeft">The i left.</param>
        /// <param name="iRight">The i Right.</param>
        /// <param name="iO">The i O.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CompareDifferent(System.Collections.Generic.IList<Neuron> iLeft, System.Collections.Generic.IList<Neuron> iRight, 
            Neuron iO)
        {
            if (iRight != null && iLeft != null)
            {
                if (iRight.Count != iLeft.Count)
                {
                    return true;
                }

                for (var i = 0; i < iLeft.Count; i++)
                {
                    if (iLeft[i].CompareWith(iRight[i], iO) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            if ((iRight == null && iLeft != null) || (iLeft == null && iRight != null))
            {
                return true;
            }

            return false;
        }

        /// <summary>The compare normal.</summary>
        /// <param name="iLeft">The i left.</param>
        /// <param name="iRight">The i right.</param>
        /// <param name="iO">The i o.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CompareNormal(System.Collections.Generic.IList<Neuron> iLeft, System.Collections.Generic.IList<Neuron> iRight, 
            Neuron iO)
        {
            var iDidCompare = false;
            foreach (var iL in iLeft)
            {
                // check every possible combination, if non is false, it's true.
                if (iRight != null)
                {
                    foreach (var iR in iRight)
                    {
                        iDidCompare = true;
                        if (iL.CompareWith(iR, iO) == false)
                        {
                            return false;
                        }
                    }
                }
            }

            return iDidCompare;
        }

        /// <summary>The compare or.</summary>
        /// <param name="iLeft">The i left.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="argStackCount">The arg stack count.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareOr(System.Collections.Generic.IList<Neuron> iLeft, 
            Processor processor, 
            ref int argStackCount)
        {
            System.Collections.Generic.IList<Neuron> iRight = null;
            foreach (var iL in iLeft)
            {
                // check every possible combination, if non is false, it's true.
                if (iL.ID != (ulong)PredefinedNeurons.True)
                {
                    if (iRight == null)
                    {
                        iRight = SolveResultExpNoStackChange(WorkData.RightPart, processor);
                        if (iRight != null)
                        {
                            argStackCount++;
                        }
                    }

                    if (iRight != null)
                    {
                        foreach (var iR in iRight)
                        {
                            if (iR.ID == (ulong)PredefinedNeurons.True)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The compare and.</summary>
        /// <param name="iLeft">The i left.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="iO">The i o.</param>
        /// <param name="argStackCount">The arg stack count.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareAnd(System.Collections.Generic.IList<Neuron> iLeft, 
            Processor processor, 
            Neuron iO, 
            ref int argStackCount)
        {
            System.Collections.Generic.IList<Neuron> iRight = null;
            foreach (var iL in iLeft)
            {
                // check every possible combination, if non is false, it's true.
                if (iL.ID != (ulong)PredefinedNeurons.True)
                {
                    return false;
                }

                if (iRight == null)
                {
                    iRight = SolveResultExpNoStackChange(WorkData.RightPart, processor);
                    if (iRight != null)
                    {
                        argStackCount++;
                    }
                }

                if (iRight != null)
                {
                    foreach (var iR in iRight)
                    {
                        if (iL.CompareWith(iR, iO) == false)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The evaluate empty left.</summary>
        /// <param name="op">The op.</param>
        /// <param name="iRight">The i right.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool EvaluateEmptyLeft(Neuron op, System.Collections.Generic.IEnumerable<Neuron> iRight)
        {
            if (op.ID == (ulong)PredefinedNeurons.Different)
            {
                // if we have no result, but are trying to do a '!=' we need to return true, if the right part is not empty offcourse.
                return Enumerable.Count(iRight) > 0;
            }

            return false;
        }

        /// <summary>Performs the evaluation for a contains operator, which checks if all
        ///     the items in the <paramref name="right"/> side are also in the<paramref name="left"/> list (but not the other way).</summary>
        /// <param name="left">The list to check.</param>
        /// <param name="right">The items that need to be found in the list</param>
        /// <returns>True if all the items were contained in the list, otherwise false.</returns>
        private bool EvaluateContains(System.Collections.Generic.IEnumerable<Neuron> left, System.Collections.Generic.IEnumerable<Neuron> right)
        {
            var iLeft = left as NeuronCluster;

            var iRes = false; // we init to false, so that if there aren't any items on the right, we return false.
            foreach (var iRight in right)
            {
                if (Enumerable.Contains(left, iRight) == false)
                {
                    return false;
                }

                iRes = true; // so we can return false in case there were no items in the right list.
            }

            return iRes;
        }

        /// <summary>Performs the evaluation for a Not contains operator, which checks if
        ///     all the items in the <paramref name="right"/> side are also in the<paramref name="left"/> list (but not the other way).</summary>
        /// <param name="left">The list to check.</param>
        /// <param name="right">The items that need to be found in the list</param>
        /// <returns>True if all the items were contained in the list, otherwise false.</returns>
        private bool EvaluateNotContains(System.Collections.Generic.IEnumerable<Neuron> left, System.Collections.Generic.IEnumerable<Neuron> right)
        {
            var iLeft = left as NeuronCluster;

            foreach (var iRight in right)
            {
                if (Enumerable.Contains(left, iRight))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var iStr = new System.Text.StringBuilder();
            var iToWrite = LeftPart;
            if (iToWrite != null)
            {
                iStr.Append(iToWrite);
            }
            else
            {
                iStr.Append("??");
            }

            iStr.Append(" ");
            iToWrite = Operator;
            if (iToWrite == null)
            {
                iStr.Append("?");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.Equal)
            {
                iStr.Append("==");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.Smaller)
            {
                iStr.Append("<");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.Bigger)
            {
                iStr.Append(">");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.SmallerOrEqual)
            {
                iStr.Append("<=");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.BiggerOrEqual)
            {
                iStr.Append(">=");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.Different)
            {
                iStr.Append("!=");
            }
            else if (iToWrite.ID == (ulong)PredefinedNeurons.Contains)
            {
                iStr.Append("contains");
            }
            else if (iToWrite != null)
            {
                iStr.Append(iToWrite);
            }
            else
            {
                iStr.Append("?");
            }

            iStr.Append(" ");
            iToWrite = RightPart;
            if (iToWrite != null)
            {
                iStr.Append(iToWrite);
            }
            else
            {
                iStr.Append("??");
            }

            return iStr.ToString();
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
                if (fWorkData.LeftPart is Expression)
                {
                    ((Expression)fWorkData.LeftPart).LoadCode(alreadyProcessed);
                }

                if (fWorkData.RightPart is Expression)
                {
                    ((Expression)fWorkData.RightPart).LoadCode(alreadyProcessed);
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
        private class BoolExpData
        {
            /// <summary>The left part.</summary>
            public Neuron LeftPart;

            /// <summary>The operator.</summary>
            public Neuron Operator;

            /// <summary>The right part.</summary>
            public Neuron RightPart;

            /// <summary>The calculate bool event.</summary>
            public event CalculateBoolEventHandler CalculateBoolEvent;

            /// <summary>Calculates any possible optimisations that can be applied.</summary>
            /// <param name="owner">The owner.</param>
            internal void CalculateOptimisations(BoolExpression owner)
            {
                if ((LeftPart is IGetBool && ((IGetBool)LeftPart).CanGetBool())
                    && (RightPart is IGetBool && ((IGetBool)RightPart).CanGetBool()))
                {
                    CalculateBoolEvent = owner.GetBoolFromBoolInterface;
                }
                else if ((LeftPart is IGetInt && ((IGetInt)LeftPart).CanGetInt())
                         && (RightPart is IGetInt && ((IGetInt)RightPart).CanGetInt()))
                {
                    CalculateBoolEvent = owner.GetBoolFromIntInterface;
                }
                else if ((LeftPart is IGetDouble && ((IGetDouble)LeftPart).CanGetDouble())
                         && (RightPart is IGetDouble && ((IGetDouble)RightPart).CanGetDouble()))
                {
                    CalculateBoolEvent = owner.GetBoolFromDoubleInterface;
                }
                else
                {
                    CalculateBoolEvent = owner.GetBoolRegular;
                }
            }

            /// <summary>Calculates the bool.</summary>
            /// <param name="handler">The handler.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            internal bool CalculateBool(Processor handler)
            {
                return CalculateBoolEvent(handler);
            }
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BoolExpression"/> class. 
        ///     Default constructor.</summary>
        internal BoolExpression()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BoolExpression"/> class. Creates a new <see langword="bool"/> expression, with the proper links
        ///     already made.</summary>
        /// <param name="left">The left part of the expression.</param>
        /// <param name="op">The <see langword="operator"/> to execute.</param>
        /// <param name="right">The right part of the expression.</param>
        internal BoolExpression(Neuron left, Neuron op, Neuron right)
        {
            var iNew = new Link(left, this, (ulong)PredefinedNeurons.LeftPart);
            iNew = new Link(op, this, (ulong)PredefinedNeurons.Operator);
            iNew = new Link(right, this, (ulong)PredefinedNeurons.RightPart);
        }

        #endregion

        #region Prop

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        private BoolExpData WorkData
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
                    var iWorkData = new BoolExpData();
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks);
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.LeftPart)
                        {
                            iWorkData.LeftPart = iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.RightPart)
                        {
                            iWorkData.RightPart = iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Operator)
                        {
                            iWorkData.Operator = iTemp[i].To;
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp);
                    iWorkData.CalculateOptimisations(this);
                    fWorkData = iWorkData;
                }
            }
        }

        #endregion

        #region Operator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> that should be used.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is 'Equal' by defualt. Other values could be 'Bigger', 'Smaller',
        ///         'Different, 'BiggerOrEqual', 'SmallerOrEqual' or 'Contains'.
        ///     </para>
        ///     <para>
        ///         If this is an expression, it is first executed before the compare is
        ///         done (so multiple operators are allowed?).
        ///     </para>
        /// </remarks>
        public Neuron Operator
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Operator);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, value);
            }
        }

        #endregion

        #region LeftPart

        /// <summary>
        ///     Gets/sets the left part of the expression.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The left part is compared to the
        ///         <see cref="JaStDev.HAB.BoolExpression.RightPart" /> using the
        ///         <see cref="BoolExpresssion.Operator" /> (which must be of a known
        ///         type) to produce a result.
        ///     </para>
        ///     <para>
        ///         If this is an expression, it is first executed before the compare is
        ///         done.
        ///     </para>
        /// </remarks>
        public Neuron LeftPart
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.LeftPart);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LeftPart, value);
            }
        }

        #endregion

        #region RightPart

        /// <summary>
        ///     Gets/sets the right part of the expression.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The right part is compared to the
        ///         <see cref="JaStDev.HAB.BoolExpression.LeftPart" /> using the
        ///         <see cref="BoolExpresssion.Operator" /> (which must be of a known
        ///         type) to produce a result.
        ///     </para>
        ///     <para>
        ///         If this is an expression, it is first executed before the compare is
        ///         done.
        ///     </para>
        /// </remarks>
        public Neuron RightPart
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.RightPart);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.RightPart, value);
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.BoolExpression;
            }
        }

        #endregion

        #endregion

        #region IGetBool Members

        /// <summary>returns if this object can return an int. A <see langword="bool"/>
        ///     expression can always return the result as a <see langword="bool"/>
        ///     value.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetBool()
        {
            return true;
        }

        /// <summary>Evaluates the boolean expression and returns the result. True if the
        ///     evaluation was true, otherwise false.</summary>
        /// <param name="processor">The processor.</param>
        /// <returns>True if the <see langword="bool"/> expression evaluated to true.</returns>
        public override bool GetBool(Processor processor)
        {
            return WorkData.CalculateBool(processor);
        }

        #endregion
    }
}