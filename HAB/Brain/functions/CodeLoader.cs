// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeLoader.cs" company="">
//   
// </copyright>
// <summary>
//   provides functionality to load code objects into memory. This is used as
//   a speed optimisation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides functionality to load code objects into memory. This is used as
    ///     a speed optimisation.
    /// </summary>
    internal class CodeLoader
    {
        /// <summary>Prevents a default instance of the <see cref="CodeLoader"/> class from being created.</summary>
        private CodeLoader()
        {
        }

        /// <summary>Starts loading the code items attached to the specified object. this
        ///     is done async.</summary>
        /// <param name="start"></param>
        public static void Start(ulong start)
        {
            System.Action<ulong> iStart = InternalStart;
            iStart.BeginInvoke(start, null, null);
        }

        /// <summary>makes certain that the code for the specified expression (if any) is
        ///     loaded.</summary>
        /// <param name="toLoad"></param>
        public static void LoadFor(Expression toLoad)
        {
            if (toLoad != null && toLoad.IsCodeLoaded() == false)
            {
                var alreadyProcessed = new System.Collections.Generic.HashSet<Neuron>();
                toLoad.LoadCode(alreadyProcessed);
            }
        }

        /// <summary>performs the operation.</summary>
        /// <param name="start"></param>
        private static void InternalStart(ulong start)
        {
            var iStart = Brain.Current[start];
            var iCluster = iStart.RulesCluster;
            if (iCluster != null)
            {
                ProcessExpressions(iCluster);
            }

            iCluster = iStart.ActionsCluster;
            if (iCluster != null)
            {
                ProcessExpressions(iCluster);
            }

            iCluster = iStart as NeuronCluster;
            if (iCluster != null)
            {
                ProcessExpressions(iCluster);
            }
        }

        /// <summary>The process expressions.</summary>
        /// <param name="cluster">The cluster.</param>
        private static void ProcessExpressions(NeuronCluster cluster)
        {
            var list = cluster.GetBufferedChildren<Expression>();
            try
            {
                var alreadyProcessed = new System.Collections.Generic.HashSet<Neuron>();
                try
                {
                    foreach (var i in list)
                    {
                        i.LoadCode(alreadyProcessed);
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Code loader", e.ToString());
                }
            }
            finally
            {
                cluster.ReleaseBufferedChildren((System.Collections.IList)list);
            }
        }
    }
}