// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionalStatement.cs" company="">
//   
// </copyright>
// <summary>
//   A conditional statement (to compare with if-then-else + do-while +
//   while-do + repeat + for + foreach.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A conditional statement (to compare with if-then-else + do-while +
    ///     while-do + repeat + for + foreach.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the same conditional technique as gene.</para>
    ///     <para>
    ///         Doesn't do a for loop, this can easely be done with a while loop.
    ///     </para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>For each loop:</description>
    ///             </item>
    ///         </list>
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                     condition expected which should return the list of items to walk through,
    ///                     it's statements are executed. The item that is currently processed,
    ///                     should be defined with the <see cref="ConditionalGroup.LoopItem" />
    ///                 </description>
    ///             </item>
    ///         </list>
    ///         <para>
    ///             expression. -for each where a query is the source: the first conditional
    ///             part's condition contains the list of variables to process, the loop item
    ///             contains the source query to use. (so this is the inverse situation of a
    ///             normal <see langword="foreach" /> loop).
    ///         </para>
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ConditionalStatement, typeof(Neuron))]
    public class ConditionalStatement : Expression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile ConditionalData fWorkData;

        #endregion

        #region Inner types

        /// <summary>
        ///     Speed improvement: we cash some linkdata for faster execution.
        /// </summary>
        internal class ConditionalData
        {
            /// <summary>The case item.</summary>
            public ResultExpression CaseItem;

            /// <summary>The conditions cluster.</summary>
            public NeuronCluster ConditionsCluster;

            /// <summary>The loop item.</summary>
            public Neuron LoopItem;

            /// <summary>The loop style.</summary>
            public Neuron LoopStyle;

            /// <summary>The statements cluster.</summary>
            public NeuronCluster StatementsCluster;
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ConditionalStatement"/> class. 
        ///     Default constructor</summary>
        internal ConditionalStatement()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConditionalStatement"/> class. Creates a new ConditionalExpression, with the proper links already
        ///     made.</summary>
        /// <param name="conditions">The list of conditional expressions to evaluate.</param>
        /// <param name="loopstyle">The type of looping that should be used with this group.</param>
        internal ConditionalStatement(NeuronCluster conditions, Neuron loopstyle)
        {
            var iNew = new Link(conditions, this, (ulong)PredefinedNeurons.Condition);
            iNew = new Link(loopstyle, this, (ulong)PredefinedNeurons.LoopStyle);
        }

        #endregion

        #region Prop

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        internal ConditionalData WorkData
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
                    var iWorkData = new ConditionalData();
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks);
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Condition)
                        {
                            iWorkData.ConditionsCluster = (NeuronCluster)iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Statements)
                        {
                            iWorkData.StatementsCluster = (NeuronCluster)iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.LoopStyle)
                        {
                            iWorkData.LoopStyle = iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.LoopItem)
                        {
                            iWorkData.LoopItem = iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.CaseItem)
                        {
                            iWorkData.CaseItem = (ResultExpression)iTemp[i].To;
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp);
                    fWorkData = iWorkData;
                }
            }
        }

        #endregion

        #region Conditions

        /// <summary>
        ///     Gets the list of conditions that are contained in this group, as
        ///     conditional expressions.
        /// </summary>
        /// <remarks>
        ///     To edit this list, use the <see cref="Neuron.LinksTo" /> list.
        /// </remarks>
        public System.Collections.ObjectModel.ReadOnlyCollection<ConditionalExpression> Conditions
        {
            get
            {
                var iCluster = WorkData.ConditionsCluster;
                if (iCluster != null)
                {
                    var iExps = iCluster.GetBufferedChildren<ConditionalExpression>();
                    if (iExps != null)
                    {
                        try
                        {
                            return new System.Collections.ObjectModel.ReadOnlyCollection<ConditionalExpression>(iExps);
                        }
                        finally
                        {
                            iCluster.ReleaseBufferedChildren((System.Collections.IList)iExps);
                        }
                    }

                    LogService.Log.LogError(
                        "ConditionalGroup.Conditions", 
                        string.Format("Failed to convert Conditions list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region ConditionsCluster

        /// <summary>
        ///     Gets the <see cref="NeuronCluster" /> used to store all the sub
        ///     statements of this conditional expression.
        /// </summary>
        public NeuronCluster ConditionsCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Condition, value);
            }
        }

        #endregion

        #region Statements

        /// <summary>
        ///     Gets the list of conditions that are contained in this group, as
        ///     conditional expressions.
        /// </summary>
        /// <remarks>
        ///     To edit this list, use the <see cref="Neuron.LinksTo" /> list.
        /// </remarks>
        public System.Collections.ObjectModel.ReadOnlyCollection<Expression> Statements
        {
            get
            {
                var iCluster = WorkData.StatementsCluster;
                if (iCluster != null)
                {
                    var iExps = iCluster.GetBufferedChildren<Expression>();
                    if (iExps != null)
                    {
                        try
                        {
                            return new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(iExps);
                        }
                        finally
                        {
                            iCluster.ReleaseBufferedChildren((System.Collections.IList)iExps);
                        }
                    }

                    LogService.Log.LogError(
                        "ConditionalStatement.Statements", 
                        string.Format("Failed to convert statements list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region StatementsCluster

        /// <summary>
        ///     Gets the <see cref="NeuronCluster" /> used to store all the sub
        ///     statements of this conditional satement. (to be called before the
        ///     parts get evaluated, for function calls).
        /// </summary>
        public NeuronCluster StatementsCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Statements) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Statements, value);
            }
        }

        #endregion

        #region LoopStyle

        /// <summary>
        ///     Gets/sets the type of looping applied to this conditional groups.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Possible values are: Not, Case, Looped, CaseLooped, ForEach, For,
        ///         Until
        ///     </para>
        ///     <para>
        ///         This can be a result expression that needs to be solved. Only a single
        ///         value result is allowed.
        ///     </para>
        /// </remarks>
        public Neuron LoopStyle
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.LoopStyle);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LoopStyle, value);
            }
        }

        #endregion

        #region LoopItem

        /// <summary>
        ///     Gets/sets the <see cref="Variable" /> used to store the current value
        ///     of a for or <see langword="foreach" /> loop. Only has to be set if this
        ///     is a <see langword="foreach" /> loop.
        /// </summary>
        public Variable LoopItem
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.LoopItem) as Variable;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LoopItem, value);
            }
        }

        #endregion

        #region QuerySource

        /// <summary>
        ///     Gets/sets the <see cref="querySource" /> used to store the current
        ///     value of a for or <see langword="foreach" /> loop. Only has to be set
        ///     if this is a 'select-foreach' loop.
        /// </summary>
        public Neuron QuerySource
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.LoopItem);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LoopItem, value);
            }
        }

        #endregion

        #region CaseItem

        /// <summary>
        ///     Gets/sets the <see cref="ResultExpression" /> used to define the item
        ///     or list of neurons to compare against in a case statemnt. This value
        ///     is only required if it is a case or looped case.
        /// </summary>
        public ResultExpression CaseItem
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.CaseItem) as ResultExpression;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.CaseItem, value);
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ConditionalStatement" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ConditionalStatement;
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

        /// <summary>Only 1 loop style is allowed.</summary>
        /// <param name="handler"></param>
        protected internal override void Execute(Processor handler)
        {
            CallFrame iFrame;
            var iLoopStyle = SolveSingleResultExp(WorkData.LoopStyle, handler);
            if (Settings.CheckConditional)
            {
                CheckConditions(iLoopStyle.ID);
            }

            if (iLoopStyle != null)
            {
                switch (iLoopStyle.ID)
                {
                    case (ulong)PredefinedNeurons.Normal:
                        iFrame = IfFrame.Create(this);
                        handler.PushFrame(iFrame);
                        break;
                    case (ulong)PredefinedNeurons.Case:
                        iFrame = CaseFrame.CreateCase(this);
                        handler.PushFrame(iFrame);
                        break;
                    case (ulong)PredefinedNeurons.Looped:
                        iFrame = LoopedCallFrame.Create(this);
                        handler.PushFrame(iFrame);
                        break;
                    case (ulong)PredefinedNeurons.CaseLooped:
                        iFrame = CaseLoopedCallFrame.CreateCaseLoop(this);
                        handler.PushFrame(iFrame);
                        break;
                    case (ulong)PredefinedNeurons.Until:
                        PerformUntill(handler);
                        break;
                    case (ulong)PredefinedNeurons.ForEach:
                        PerformForEach(handler);
                        break;
                    case (ulong)PredefinedNeurons.QueryLoop:
                        PerformQuery(handler);
                        break;
                    case (ulong)PredefinedNeurons.QueryLoopIn:
                        PerformQueryIn(handler);
                        break;
                    case (ulong)PredefinedNeurons.QueryLoopOut:
                        PerformQueryOut(handler);
                        break;
                    case (ulong)PredefinedNeurons.QueryLoopClusters:
                        PerformQueryClusters(handler);
                        break;
                    case (ulong)PredefinedNeurons.QueryLoopChildren:
                        PerformQueryChildren(handler);
                        break;
                    default:
                        LogService.Log.LogError(
                            "Processor.EvaluateConditional", 
                            string.Format("Unknown loop style: '{0}'.", iLoopStyle));
                        break;
                }
            }
            else
            {
                LogService.Log.LogError(
                    "ConditionalGroup.Execute", 
                    string.Format("'{0}' is not/does not produce a valid loop style.", WorkData.LoopStyle));
            }
        }

        /// <summary>The perform query children.</summary>
        /// <param name="handler">The handler.</param>
        private void PerformQueryChildren(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                IForEachSource iQuery = null;
                if (WorkData.LoopItem != null)
                {
                    if (WorkData.LoopItem.ID == (ulong)PredefinedNeurons.CurrentSin)
                    {
                        // this is usually the case, can be retrieved really fast.
                        iQuery = new LinkInSelectSource(handler.CurrentSin);
                    }
                    else
                    {
                        iQuery = new LinkInSelectSource(SolveSingleResultExp(WorkData.LoopItem, handler));
                    }
                }

                if (iQuery != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the variables
                        var iFrame = ForQueryCallFrame.Create(iItems, iQuery, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Select child statement", 
                            "Can't perform select: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Select child statement", "Can't perform select: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>The perform query clusters.</summary>
        /// <param name="handler">The handler.</param>
        private void PerformQueryClusters(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                IForEachSource iQuery = null;
                if (WorkData.LoopItem != null)
                {
                    if (WorkData.LoopItem.ID == (ulong)PredefinedNeurons.CurrentSin)
                    {
                        // this is usually the case, can be retrieved really fast.
                        iQuery = new ClustersSelectSource(handler.CurrentSin);
                    }
                    else
                    {
                        iQuery = new ClustersSelectSource(SolveSingleResultExp(WorkData.LoopItem, handler));
                    }
                }

                if (iQuery != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the variables
                        var iFrame = ForQueryCallFrame.Create(iItems, iQuery, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Select cluster statement", 
                            "Can't perform select: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Select cluster statement", "Can't perform select: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>The perform query out.</summary>
        /// <param name="handler">The handler.</param>
        private void PerformQueryOut(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                IForEachSource iQuery = null;
                if (WorkData.LoopItem != null)
                {
                    if (WorkData.LoopItem.ID == (ulong)PredefinedNeurons.CurrentSin)
                    {
                        // this is usually the case, can be retrieved really fast.
                        iQuery = new LinkOutSelectSource(handler.CurrentSin);
                    }
                    else
                    {
                        iQuery = new LinkOutSelectSource(SolveSingleResultExp(WorkData.LoopItem, handler));
                    }
                }

                if (iQuery != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the variables
                        var iFrame = ForQueryCallFrame.Create(iItems, iQuery, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError("Select out statement", "Can't perform select: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Select out statement", "Can't perform select: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>The perform query in.</summary>
        /// <param name="handler">The handler.</param>
        private void PerformQueryIn(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                IForEachSource iQuery = null;
                if (WorkData.LoopItem != null)
                {
                    if (WorkData.LoopItem.ID == (ulong)PredefinedNeurons.CurrentSin)
                    {
                        // this is usually the case, can be retrieved really fast.
                        iQuery = new LinkInSelectSource(handler.CurrentSin);
                    }
                    else
                    {
                        iQuery = new LinkInSelectSource(SolveSingleResultExp(WorkData.LoopItem, handler));
                    }
                }

                if (iQuery != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the variables
                        var iFrame = ForQueryCallFrame.Create(iItems, iQuery, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError("Select in statement", "Can't perform select: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Select in statement", "Can't perform select: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>The perform query.</summary>
        /// <param name="handler">The handler.</param>
        private void PerformQuery(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                IForEachSource iQuery = null;
                if (WorkData.LoopItem != null)
                {
                    if (WorkData.LoopItem.ID == (ulong)PredefinedNeurons.CurrentSin)
                    {
                        // this is usually the case, can be retrieved really fast.
                        iQuery = handler.CurrentSin as IForEachSource;
                    }
                    else
                    {
                        iQuery = SolveSingleResultExp(WorkData.LoopItem, handler) as IForEachSource;
                    }
                }

                if (iQuery != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the variables
                        var iFrame = ForQueryCallFrame.Create(iItems, iQuery, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError("Select statement", "Can't perform select: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Select statement", "Can't perform select: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>Checks the conditions if there are any empty allowed + if so, that
        ///     they are at the back.</summary>
        /// <param name="loopStyleId">The loop Style Id.</param>
        private void CheckConditions(ulong loopStyleId)
        {
            var iList = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                int nrAlowedEmpty;
                if (loopStyleId == (ulong)PredefinedNeurons.ForEach || loopStyleId == (ulong)PredefinedNeurons.Until
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoop
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopChildren
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopClusters
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopIn
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopOut)
                {
                    nrAlowedEmpty = 0;
                }
                else
                {
                    nrAlowedEmpty = 1;
                }

                for (var i = iList.Count - 1; i >= 0; i--)
                {
                    if (iList[i].ExpData.Condition == null && nrAlowedEmpty <= 0)
                    {
                        LogService.Log.LogError(
                            "conditional.CheckConditions", 
                            string.Format("Invalid condition at index: {0}!", iList.Count - i - 1));
                    }

                    nrAlowedEmpty--;
                }

                if (loopStyleId == (ulong)PredefinedNeurons.ForEach || loopStyleId == (ulong)PredefinedNeurons.QueryLoop
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopChildren
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopClusters
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopIn
                    || loopStyleId == (ulong)PredefinedNeurons.QueryLoopOut)
                {
                    if (iList.Count != 1)
                    {
                        LogService.Log.LogWarning(
                            "Processor.PerformForEach", 
                            "To many condition expressions defined in group, only first is evaluated in a for - each loop!");
                    }
                }
                else if (loopStyleId == (ulong)PredefinedNeurons.Until)
                {
                    if (iList.Count != 1)
                    {
                        LogService.Log.LogWarning(
                            "Processor.PerformUntill", 
                            "To many condition expressions defined in group, only first is evaluated and performed in an untill loop!");
                    }
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iList);
            }
        }

        /// <summary>uses the result of the condition of each<see cref="ConditionalExpression"/> as the 'for' part</summary>
        /// <param name="handler">The handler.</param>
        /// <remarks>Only 1 condition statement allowed.</remarks>
        private void PerformForEach(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                var iLoopItem = WorkData.LoopItem as Variable;
                if (iLoopItem != null)
                {
                    if (iCond != null && iCond.Count != 0)
                    {
                        var iItems = SolveResultExp(iCond[0].ExpData.Condition, handler);

                            // this will make a local copy of the result data.
                        var iFrame = ForEachCallFrame.Create(iLoopItem, iItems, iCond[0].WorkData.StatementsCluster);
                        handler.PushFrame(iFrame);
                    }
                    else
                    {
                        LogService.Log.LogError("PerformForEach", "Can't perform for - each: no conditions defined!");
                    }
                }
                else
                {
                    LogService.Log.LogError("PerformForEach", "Can't perform for - each: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>Do the statements, check the condition after the perform (done through
        ///     the callframe).</summary>
        /// <param name="handler">The handler.</param>
        /// <remarks>only 1 conditional expressio allowed.</remarks>
        private void PerformUntill(Processor handler)
        {
            var iCond = WorkData.ConditionsCluster.GetBufferedChildren<ConditionalExpression>();
            try
            {
                if (iCond != null && iCond.Count != 0)
                {
                    var iFrame = UntilCallFrame.Create(iCond[0]);
                    handler.PushFrame(iFrame);
                }
                else
                {
                    LogService.Log.LogError("PerformForEach", "Can't perform for - each: no conditions defined!");
                }
            }
            finally
            {
                WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iCond);
            }
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Conditional: " + ID;
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
                if (fWorkData.CaseItem != null)
                {
                    fWorkData.CaseItem.LoadCode(alreadyProcessed);
                }

                var iItems = WorkData.ConditionsCluster.GetBufferedChildren<Expression>();
                try
                {
                    foreach (var i in iItems)
                    {
                        i.LoadCode(alreadyProcessed);
                    }
                }
                finally
                {
                    WorkData.ConditionsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
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