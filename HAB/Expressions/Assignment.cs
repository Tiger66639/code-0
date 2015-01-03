// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Assignment.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   An expression that assigns the right part (after evaluation) to the left
//   part (without evaluation). This only works for <see cref="TextNeuron" />
//   , <see cref="IntNeuron" /> , <see cref="DoubleNeuron" />
//   </para>
//   <para>or <see cref="Variable" /> s.</para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     <para>
    ///         An expression that assigns the right part (after evaluation) to the left
    ///         part (without evaluation). This only works for <see cref="TextNeuron" />
    ///         , <see cref="IntNeuron" /> , <see cref="DoubleNeuron" />
    ///     </para>
    ///     <para>or <see cref="Variable" /> s.</para>
    /// </summary>
    /// <remarks>
    ///     When from is a result expression (but not a var), this is first resolved
    ///     untill it's first child is no longer a result expression. When from :
    ///     textNeuron to <see cref="TextNeuron" /> -> copy text
    ///     <see cref="IntNeuron" /> to <see cref="IntNeuron" /> -> copy value
    ///     <see cref="DoubleNeuron" /> To <see cref="DoubleNeuron" /> -> copy value
    ///     <see cref="Variable" /> to <see cref="Variable" /> -> copy value of second
    ///     var Note: when <see cref="Expression.ByReference" /> is true, a
    ///     <see langword="ref" /> to the right variable is stored in left.
    ///     <see cref="Variable" /> to <see cref="Expression" /> -> Execute expression
    ///     + store result of expression / Note: when
    ///     <see cref="Expression.ByReference" /> is true, a <see langword="ref" /> to
    ///     the expression is stored in the var. variable to any other -> store
    ///     reference of object in variable
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Assignment, typeof(Neuron))]
    public class Assignment : Expression
    {
        #region fields

        /// <summary>The f work data.</summary>
        private volatile AssignData fWorkData;

        #endregion

        #region Inner types

        /// <summary>
        ///     Speed improvement: we cash some linkdata for faster execution.
        /// </summary>
        private class AssignData
        {
            /// <summary>The left part.</summary>
            public Neuron LeftPart;

            /// <summary>The right part.</summary>
            public Neuron RightPart;
        }

        #endregion

        #region Prop

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        private AssignData WorkData
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
                    var iWorkData = new AssignData();
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks); // make local copy so thatt here is not deadlock.
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.LeftPart)
                        {
                            iWorkData.LeftPart = iTemp[i].To;

                                // doing cache access inside reader lock, should be save, cause it's only a reader lock?
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.RightPart)
                        {
                            iWorkData.RightPart = iTemp[i].To;

                                // doing cache access inside reader lock, should be save, cause it's only a reader lock?
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp, false);

                    fWorkData = iWorkData;
                }
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
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Assignment" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Assignment;
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
            var iLeft = LeftPart;
            var iRight = RightPart;
            return (iLeft != null ? iLeft.ToString() : "?") + " = " + (iRight != null ? iRight.ToString() : "?");
        }

        /// <summary>Inheriters should implement this function for performing the
        ///     expression.</summary>
        /// <remarks>Implementers don't have to increment the procoessor positions or
        ///     anything (except for conditional statements offcourse), this is done
        ///     by <see cref="Expression.Execute"/></remarks>
        /// <param name="handler">The processor to execute the statements on.</param>
        protected internal override void Execute(Processor handler)
        {
            var iLeft = WorkData.LeftPart;
            while (iLeft is ResultExpression && !(iLeft is Variable))
            {
                iLeft = Enumerable.FirstOrDefault(SolveResultExpNoStackChange(iLeft, handler));
                handler.Mem.ArgumentStack.Pop();

                    // if the result is an expression again, solve it untill we have a var or static. The result is part of the stack, so release it so that the stack wont grow out of control.
            }

            if (iLeft is Variable)
            {
                AssignToVar((Variable)iLeft, handler);
            }
            else if (iLeft is TextNeuron)
            {
                DoText((TextNeuron)iLeft, handler);
            }
            else if (iLeft is DoubleNeuron)
            {
                DoDouble((DoubleNeuron)iLeft, handler);
            }
            else if (iLeft is IntNeuron)
            {
                DoInt((IntNeuron)iLeft, handler);
            }
            else
            {
                LogService.Log.LogError(
                    "Assignment.Execute", 
                    string.Format("Invalid (unassignable) left part: {0}.", iLeft));
            }
        }

        /// <summary>The do text.</summary>
        /// <param name="left">The left.</param>
        /// <param name="handler">The handler.</param>
        private void DoText(TextNeuron left, Processor handler)
        {
            lock (left)
            {
                // make the value operation thread save.
                var iRight = SolveSingleResultExp(WorkData.RightPart, handler) as TextNeuron;
                if (iRight != null)
                {
                    left.Text = iRight.Text;
                }
                else
                {
                    LogService.Log.LogError("Assignment.Execute", string.Format("TextNeuron expected in right part."));
                    left.Text = string.Empty;
                }
            }
        }

        /// <summary>The do int.</summary>
        /// <param name="left">The left.</param>
        /// <param name="handler">The handler.</param>
        private void DoInt(IntNeuron left, Processor handler)
        {
            LockManager.Current.RequestLock(left, LockLevel.Value, true);

                // the whole calculation needs to be thread save.
            try
            {
                int iRightVal;
                if (WorkData.RightPart.CanGetInt())
                {
                    iRightVal = WorkData.RightPart.GetInt(handler);
                }
                else
                {
                    var iRight = SolveSingleResultExp(WorkData.RightPart, handler) as IntNeuron;
                    if (iRight != null)
                    {
                        iRightVal = iRight.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Assignment.Execute", 
                            string.Format("IntNeuron expected in right part."));
                        iRightVal = 0;
                    }
                }

                left.SetValueDirect(iRightVal);
            }
            finally
            {
                LockManager.Current.ReleaseLock(left, LockLevel.Value, true);
            }

            left.SetIsChangedNoUnfreeze(true); // don't unfreeze, cause then we create mem leaks all over the place
            if (Brain.Current.HasNeuronChangedEvents)
            {
                Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", left));
            }
        }

        /// <summary>The do double.</summary>
        /// <param name="left">The left.</param>
        /// <param name="handler">The handler.</param>
        private void DoDouble(DoubleNeuron left, Processor handler)
        {
            LockManager.Current.RequestLock(left, LockLevel.Value, true);

                // the whole calculation needs to be thread save.
            try
            {
                double iRightVal;
                if (WorkData.RightPart.CanGetDouble())
                {
                    iRightVal = WorkData.RightPart.GetDouble(handler);
                }
                else
                {
                    var iRight = SolveSingleResultExp(WorkData.RightPart, handler) as DoubleNeuron;
                    if (iRight != null)
                    {
                        iRightVal = iRight.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Assignment.Execute", 
                            string.Format("DoubleNeuron expected in right part."));
                        iRightVal = 0;
                    }
                }

                left.SetValueDirect(iRightVal);
            }
            finally
            {
                LockManager.Current.ReleaseLock(left, LockLevel.Value, true);
            }

            left.SetIsChangedNoUnfreeze(true); // don't unfreeze, cause then we create mem leaks all over the place
            if (Brain.Current.HasNeuronChangedEvents)
            {
                Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", left));
            }
        }

        /// <summary>The assign to var.</summary>
        /// <param name="var">The var.</param>
        /// <param name="handler">The handler.</param>
        private void AssignToVar(Variable var, Processor handler)
        {
            var iRight = WorkData.RightPart;
            if (iRight is Variable)
            {
                // if the right is a var, we store the value of the var into left.
                // can be done faster: don't use a temp list, but directly copy to the list of the var
                var iRes = handler.Mem.ArgumentStack.Push();
                try
                {
                    ((Variable)iRight).GetValue(handler);
                    var.StoreValue(iRes, handler);
                }
                finally
                {
                    handler.Mem.ArgumentStack.Release();

                        // need to do a release cause the storeValue will consume the list.
                }
            }
            else
            {
                // if right is not a var or it is byref, we ask it to be solved (solving will return the var itself it is byref).
                if (iRight != null)
                {
                    var.StoreValue(SolveResultExp(iRight, handler), handler);
                }
                else
                {
                    LogService.Log.LogError("Assignment.Execute", string.Format("Neuron expected in right part."));
                }
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
                var iExp = fWorkData.RightPart as Expression;
                if (iExp != null)
                {
                    iExp.LoadCode(alreadyProcessed);
                }

                iExp = fWorkData.LeftPart as Expression;
                if (iExp != null)
                {
                    iExp.LoadCode(alreadyProcessed);
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