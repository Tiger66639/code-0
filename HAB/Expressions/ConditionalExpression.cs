// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionalExpression.cs" company="">
//   
// </copyright>
// <summary>
//   This expression is always a part of a <see cref="ConditionalStatement" />
//   . It represents a single branch of a conditional statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This expression is always a part of a <see cref="ConditionalStatement" />
    ///     . It represents a single branch of a conditional statement.
    /// </summary>
    /// <remarks>
    ///     Execution of the statements of a conditional expression doesn't perform
    ///     the evaluation to check if the statements need to be executed, they are
    ///     always executed by calling this function. To evaluate, call
    ///     <see cref="ConditionalExpression.EvaluateCondition" /> . This is seperate
    ///     cause we have now way of knowing how and when it should be evaluated.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ConditionalPart, typeof(Neuron))]
    public class ConditionalExpression : ExpressionsBlock
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile ExpressionData fWorkData;

        #endregion

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var iStr = new System.Text.StringBuilder("(");
            var iCond = Condition;
            if (iCond != null)
            {
                iStr.Append(iCond);
            }

            iStr.AppendLine(")");

            return iStr.ToString();
        }

        /// <summary>Asks the expression to load all the data that it requires for being
        ///     executed. This is used as an optimisation so that the code-data can be
        ///     pre-fetched. This is to solve a problem with slower platforms where
        ///     the first input can be handled slowly.</summary>
        /// <param name="alreadyProcessed">The already Processed.</param>
        protected internal override void LoadCode(System.Collections.Generic.HashSet<Neuron> alreadyProcessed)
        {
            if (Brain.Current.IsInitialized && alreadyProcessed.Contains(this) == false)
            {
                base.LoadCode(alreadyProcessed);
                alreadyProcessed.Add(this);
                if (fWorkData == null)
                {
                    LoadExpData();
                }

                var iExp = fWorkData.Condition as Expression;
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

        #region Inner types

        /// <summary>
        ///     Speed improvement: we cash some linkdata for faster execution.
        /// </summary>
        internal class ExpressionData
        {
            /// <summary>The condition.</summary>
            public Neuron Condition;
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ConditionalExpression"/> class.</summary>
        internal ConditionalExpression()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConditionalExpression"/> class. Creates a new ConditionalExpression, with the proper links already
        ///     made and the item registered with the brain.</summary>
        /// <param name="condition">The condition that determins if the <paramref name="statements"/> are
        ///     executed or not.</param>
        /// <param name="statements">The list of statements that are executed if the<paramref name="condition"/> is evaluated to true.</param>
        internal ConditionalExpression(Variable condition, NeuronCluster statements)
        {
            Brain.Current.Add(this);
            var iNew = new Link(condition, this, (ulong)PredefinedNeurons.Condition);
            iNew = new Link(statements, this, (ulong)PredefinedNeurons.Statements);
        }

        #endregion

        #region Prop

        /// <summary>
        ///     The condition to evaluate. If this returns true, the
        ///     <see cref="Expression.Statements" /> are executed.
        /// </summary>
        /// <remarks>
        ///     This is a regular neuron and not a specific
        ///     <see cref="ConditonalExpression" /> cause the condition can be
        ///     anything. If the loop is a <see cref="PredefinedNeurons.WhileDo" /> or
        ///     <see cref="PredefinedNeurons.DoWhile" /> , it should be a
        ///     conditionalExpression, howeever, if it is cased, this can be a neuron,
        ///     or another <see cref="ResultExpression" /> .
        /// </remarks>
        public Neuron Condition
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Condition);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Condition, value);
            }
        }

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ConditionalPart" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ConditionalPart;
            }
        }

        #endregion

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        internal ExpressionData ExpData
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadExpData();
                }

                return fWorkData;
            }
        }

        /// <summary>The load exp data.</summary>
        private void LoadExpData()
        {
            lock (this)
            {
                if (fWorkData == null)
                {
                    // could be that another thread beat us to the punch because of the lock.
                    var iWorkData = new ExpressionData();
                    iWorkData.Condition = Condition;
                    fWorkData = iWorkData;

                        // only assign at the very end. This is not 100% thread save: if 2 threads get this prop at the same time, both will build an object, only the last will be used, the otherone gets lost. The alternative is to lock something each time we get to the workdata, which doesn't make it faster. Better to suffer 1 small object loss, but we must set the field at the end, otherwise, the other thread might use an object that is not completely constructed yet.          
                }
            }
        }

        #endregion

        #endregion
    }
}