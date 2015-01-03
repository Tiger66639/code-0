// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockExpression.cs" company="">
//   
// </copyright>
// <summary>
//   A container for expressions that puts a lock on all the provided
//   arguments during the evaluation of the expressions. This allows for
//   thread save code. The lock is exclusive: only 1 processor can do reads or
//   writes on the locked objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A container for expressions that puts a lock on all the provided
    ///     arguments during the evaluation of the expressions. This allows for
    ///     thread save code. The lock is exclusive: only 1 processor can do reads or
    ///     writes on the locked objects.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A neuronlock locks a single neuron and requires 1 argument per lock. A
    ///         links locks requires 2 argument values per lock: the from and to part.
    ///         Meaning is not required. A lock is writeable, meaning only 1 thread can
    ///         have it at the same time, which makes it an exclusive lock.
    ///     </para>
    ///     <para>
    ///         Warning: although a split causes the block to be passed to the sub
    ///         processors, which will block untill allowed, they are not advised,
    ///         together with duplicate instructions, since they can cause unexpected
    ///         deadlocks. It is best to keep the number of 'update' instructions as
    ///         small as possible inside a lock, to minimize lock situations.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.LockExpression, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.NeuronsToLock, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.LinksToLock, typeof(Neuron))]
    public class LockExpression : ExpressionsBlock
    {
        /// <summary>Initializes a new instance of the <see cref="LockExpression"/> class.</summary>
        internal LockExpression()
        {
        }

        #region NeuronsToLockCluster

        /// <summary>
        ///     Gets the <see cref="NeuronCluster" /> used to store all the sub
        ///     statements of this conditional expression.
        /// </summary>
        public NeuronCluster NeuronsToLockCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.NeuronsToLock) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.NeuronsToLock, value);
            }
        }

        #endregion

        #region NeuronsToLock

        /// <summary>
        ///     Gets a readonly list with <see cref="Expression" /> s that are the
        ///     convertion of <see cref="ExpressionsBlock.StatementCluster" /> .
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Neuron> NeuronsToLock
        {
            get
            {
                var iCluster = NeuronsToLockCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Neuron> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Neuron>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Neuron>(iExps);
                    }

                    LogService.Log.LogError(
                        "LockExpression.NeuronsToLock", 
                        string.Format("Failed to convert arguments list of '{0}' to an executable list.", this));
                }

                return null;
            }
        }

        #endregion

        #region LinksToLockCluster

        /// <summary>
        ///     Gets the <see cref="NeuronCluster" /> used to store all the sub
        ///     statements of this conditional expression.
        /// </summary>
        public NeuronCluster LinksToLockCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.LinksToLock) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LinksToLock, value);
            }
        }

        #endregion

        #region LinksToLock

        /// <summary>
        ///     Gets a readonly list with <see cref="Expression" /> s that are the
        ///     convertion of <see cref="ExpressionsBlock.StatementCluster" /> .
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Neuron> LinksToLock
        {
            get
            {
                var iCluster = LinksToLockCluster;
                if (iCluster != null)
                {
                    System.Collections.Generic.List<Neuron> iExps;
                    using (var iChildren = iCluster.Children) iExps = iChildren.ConvertTo<Neuron>();
                    if (iExps != null)
                    {
                        return new System.Collections.ObjectModel.ReadOnlyCollection<Neuron>(iExps);
                    }

                    LogService.Log.LogError(
                        "LockExpression.LinksToLock", 
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
                return (ulong)PredefinedNeurons.LockExpression;
            }
        }

        #endregion

        /// <summary>
        ///     Returns a <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </returns>
        public override string ToString()
        {
            return "Lock(" + base.ToString() + ")";
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
                var iLocks = GetLocks(handler);
                if (iLocks != null)
                {
                    iFrame.Locks = iLocks;
                    LockManager.Current.RequestLocks(iFrame.Locks);
                }

                handler.PushFrame(iFrame);
            }
        }

        /// <summary>Gets the locks.</summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList GetLocks(Processor handler)
        {
            var iRes = LockRequestList.Create();

            GetNeuronLocks(handler, iRes);
            GetLinkLocks(handler, iRes);
            return iRes;
        }

        /// <summary>The get link locks.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="iRes">The i res.</param>
        private void GetLinkLocks(Processor handler, System.Collections.Generic.List<LockRequestInfo> iRes)
        {
            var iLinksToLock = LinksToLockCluster;
            var iNeurons = handler.Mem.ArgumentStack.Push();
            try
            {
                if (iLinksToLock != null)
                {
                    var iList = iLinksToLock.Children;
                    iList.Lock();
                    try
                    {
                        if (iNeurons.Capacity < iList.CountUnsafe)
                        {
                            iNeurons.Capacity = iList.CountUnsafe;

                                // make certain that there are at least enough items available as there are arguments.
                        }

                        foreach (var i in iList.List)
                        {
                            var iToSolve = Brain.Current[i]; // still a potential deadlock.
                            var iExp = iToSolve as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(handler);

                                    // this will put the results on the argument lsit of the processor (=iNeurons).
                            }
                            else
                            {
                                iNeurons.Add(iToSolve);
                            }
                        }
                    }
                    finally
                    {
                        iList.Dispose();
                    }
                }
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }

            var u = 0;
            while (u < iNeurons.Count)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Neuron = iNeurons[u];
                if (u % 2 == 0)
                {
                    iReq.Level = LockLevel.LinksIn;
                }
                else
                {
                    iReq.Level = LockLevel.LinksOut;
                }

                iReq.Writeable = true; // get a writeable lock, to make it accessible for only 1 thread at a time.
                iRes.Add(iReq);
                u++;
            }
        }

        /// <summary>The get neuron locks.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="iRes">The i res.</param>
        private void GetNeuronLocks(Processor handler, System.Collections.Generic.List<LockRequestInfo> iRes)
        {
            var iNeuronsToLock = NeuronsToLockCluster;
            System.Collections.Generic.List<Neuron> iNeurons = null;
            if (iNeuronsToLock != null)
            {
                iNeurons = handler.Mem.ArgumentStack.Push();
                try
                {
                    var iList = iNeuronsToLock.GetBufferedChildren<Neuron>();
                    try
                    {
                        if (iNeurons.Capacity < iList.Count)
                        {
                            iNeurons.Capacity = iList.Count; // reserve some space to speed things up.
                        }

                        foreach (var iToSolve in iList)
                        {
                            var iExp = iToSolve as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(handler);
                            }
                            else
                            {
                                iNeurons.Add(iToSolve);
                            }
                        }
                    }
                    finally
                    {
                        iNeuronsToLock.ReleaseBufferedChildren((System.Collections.IList)iList);
                    }
                }
                finally
                {
                    handler.Mem.ArgumentStack.Pop();
                }
            }

            foreach (var i in iNeurons)
            {
                var iReq = LockRequestInfo.Create();
                iReq.Neuron = i;
                iReq.Level = LockLevel.All;
                iReq.Writeable = true; // get a writeable lock, to make it accessible for only 1 thread at a time.
                iRes.Add(iReq);
            }
        }
    }
}