// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionsBlock.cs" company="">
//   
// </copyright>
// <summary>
//   A container for expressions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A container for expressions.
    /// </summary>
    /// <remarks>
    ///     Use this type of expression to group a sequence of expressions together
    ///     so that they can be reused together as a single unit, in other code
    ///     blocks.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ExpressionsBlock, typeof(Neuron))]
    public class ExpressionsBlock : Expression
    {
        #region Fields

        /// <summary>The f work data.</summary>
        private volatile BlockData fWorkData;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="ExpressionsBlock"/> class.</summary>
        internal ExpressionsBlock()
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
            return "EvaluateBlock(" + base.ToString() + ")";
        }

        /// <summary>Performs the sub statements.</summary>
        /// <param name="handler"></param>
        protected internal override void Execute(Processor handler)
        {
            // handler.Call(StatementsCluster);
            var iStatements = WorkData.StatementsCluster;
            if (iStatements != null)
            {
                var iFrame = ExpressionBlockFrame.Create(iStatements, this);
                handler.PushFrame(iFrame);
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
                var iItems = WorkData.StatementsCluster.GetBufferedChildren<Expression>();
                try
                {
                    foreach (var i in iItems)
                    {
                        i.LoadCode(alreadyProcessed);
                    }
                }
                finally
                {
                    WorkData.StatementsCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                }

                if (WorkData.Actions != null)
                {
                    iItems = WorkData.Actions.GetBufferedChildren<Expression>();
                    try
                    {
                        foreach (var i in iItems)
                        {
                            i.LoadCode(alreadyProcessed);
                        }
                    }
                    finally
                    {
                        WorkData.Actions.ReleaseBufferedChildren((System.Collections.IList)iItems);
                    }
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
        internal class BlockData
        {
            /// <summary>The actions.</summary>
            public NeuronCluster Actions; // we also buffer the actions to keep the code loded in mem.

            /// <summary>The statements cluster.</summary>
            public NeuronCluster StatementsCluster;
        }

        #endregion

        #region Prop

        #region WorkData

        /// <summary>
        ///     Gets the prefetched link data, for speed improvement of the exeuction
        ///     algorithm
        /// </summary>
        internal BlockData WorkData
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
        protected void LoadWorkData()
        {
            lock (this)
            {
                if (fWorkData == null)
                {
                    // could be that another thread beat us to the punch because of the lock.
                    var iMemFac = Factories.Default;
                    var iTemp = iMemFac.LinkLists.GetBuffer();
                    var iWorkData = new BlockData();
                    if (LinksOutIdentifier != null)
                    {
                        using (var iLinks = LinksOut) iTemp.AddRange(iLinks);
                    }

                    for (var i = 0; i < iTemp.Count; i++)
                    {
                        if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Statements)
                        {
                            iWorkData.StatementsCluster = (NeuronCluster)iTemp[i].To;
                        }
                        else if (iTemp[i].MeaningID == (ulong)PredefinedNeurons.Actions)
                        {
                            iWorkData.Actions = (NeuronCluster)iTemp[i].To;
                        }
                    }

                    iMemFac.LinkLists.Recycle(iTemp, false);
                    fWorkData = iWorkData;

                        // only assign at the very end. This is not 100% thread save: if 2 threads get this prop at the same time, both will build an object, only the last will be used, the otherone gets lost. The alternative is to lock something each time we get to the workdata, which doesn't make it faster. Better to suffer 1 small object loss, but we must set the field at the end, otherwise, the other thread might use an object that is not completely constructed yet.
                }
            }
        }

        #endregion

        #region StatementsCluster

        /// <summary>
        ///     Gets/sets the <see cref="NeuronCluster" /> used to store all the sub
        ///     statements of this conditional expression.
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

        #region Statements

        /// <summary>
        ///     Gets a readonly list with <see cref="Expression" /> s that are the
        ///     convertion of <see cref="ExpressionsBlock.StatementCluster" /> .
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Expression> Statements
        {
            get
            {
                var iCluster = StatementsCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Expression> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Expression>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(iExps);
                    }

                    LogService.Log.LogError(
                        "ExpressionsBlock.Statements", 
                        string.Format("Failed to convert Statements list of '{0}' to an executable list.", this));
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
                return (ulong)PredefinedNeurons.ExpressionsBlock;
            }
        }

        #endregion

        #endregion
    }
}