// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ByRefExpression.cs" company="">
//   
// </copyright>
// <summary>
//   An expression that takes 1
//   <see cref="JaStDev.HAB.ByRefExpression.Argument" /> and simply returns
//   it. This is usefull to get the reference to code instead of solving it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An expression that takes 1
    ///     <see cref="JaStDev.HAB.ByRefExpression.Argument" /> and simply returns
    ///     it. This is usefull to get the reference to code instead of solving it.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.ByRefExpression, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Argument, typeof(Neuron))]
    public class ByRefExpression : SimpleResultExpression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile ByRefData fWorkData;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="ByRefExpression"/> class.</summary>
        internal ByRefExpression()
        {
        }

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

        /// <summary>Calcules the result of this expression and returns this as a list.</summary>
        /// <param name="processor"></param>
        internal override void GetValue(Processor processor)
        {
            var iArg = WorkData.Argument;
            if (iArg != null)
            {
                processor.Mem.ArgumentStack.Peek().Add(iArg);
            }
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
            var iArg = Argument;
            if (iArg != null)
            {
                return "^(" + iArg + ")";
            }

            return "^()";
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
                LoadWorkData(); // access the data so that it is loaded.
                var iExp = fWorkData.Argument as Expression;
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
        private class ByRefData
        {
            /// <summary>The argument.</summary>
            public Neuron Argument;
        }

        #endregion

        #region Prop

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ByRefExpression" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ByRefExpression;
            }
        }

        #endregion

        #region Argument

        /// <summary>
        ///     Gets/sets the argument to the expression.
        /// </summary>
        /// <remarks>
        ///     This neuron is simly returned when the expression is executed, even if
        ///     the argument is another expression, it is not solved.
        /// </remarks>
        public Neuron Argument
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Argument);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Argument, value);
            }
        }

        #endregion

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        private ByRefData WorkData
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
                    var iWorkData = new ByRefData();
                    iWorkData.Argument = Argument;
                    fWorkData = iWorkData;

                        // only assign at the very end. This is not 100% thread save: if 2 threads get this prop at the same time, both will build an object, only the last will be used, the otherone gets lost. The alternative is to lock something each time we get to the workdata, which doesn't make it faster. Better to suffer 1 small object loss, but we must set the field at the end, otherwise, the other thread might use an object that is not completely constructed yet.
                }
            }
        }

        #endregion

        #endregion
    }
}