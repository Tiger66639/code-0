// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DistributionTree.cs" company="">
//   
// </copyright>
// <summary>
//   A (shallow) tree that builds a distribution of the specified values.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ML
{
    /// <summary>
    ///     A (shallow) tree that builds a distribution of the specified values.
    /// </summary>
    public class DistributionTree
    {
        #region Root

        /// <summary>
        ///     Gets/sets the root object for this distribution tree.
        /// </summary>
        public JaStDev.HAB.Neuron Root { get; set; }

        #endregion

        /// <summary>Converts the <paramref name="value"/> to a neuron and adds it to the
        ///     tree so that the distribution can be calculated.</summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Train(object value)
        {
            Train(JaStDev.HAB.BrainHelper.GetNeuronFor(value));
        }

        /// <summary>adds the <paramref name="value"/> to the tree so that the distribution
        ///     can be calculated.</summary>
        /// <param name="value"></param>
        public void Train(JaStDev.HAB.Neuron value)
        {
            if (value != null)
            {
                var iCount = Root.FindFirstOut(value.ID) as JaStDev.HAB.IntNeuron;
                if (iCount == null)
                {
                    iCount = JaStDev.HAB.NeuronFactory.GetInt(1);
                    JaStDev.HAB.Brain.Current.Add(iCount);
                    JaStDev.HAB.Link.Create(Root, iCount, value);
                }
                else
                {
                    iCount.IncValue();
                }
            }
            else
            {
                throw new System.ArgumentNullException("value");
            }
        }

        /// <summary>Gets the most frequently encountered value in this distribution.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public JaStDev.HAB.Neuron GetMostUsed()
        {
            var iMax = 0;
            JaStDev.HAB.Neuron iRes = null;
            var iLinks = JaStDev.HAB.Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iOut = Root.LinksOut) iLinks.AddRange(iOut);
                foreach (var i in iLinks)
                {
                    var iTo = i.To as JaStDev.HAB.IntNeuron;
                    if (iTo != null)
                    {
                        if (iTo.Value > iMax)
                        {
                            iMax = iTo.Value;
                            iRes = i.Meaning;
                        }
                    }
                }
            }
            finally
            {
                JaStDev.HAB.Factories.Default.LinkLists.Recycle(iLinks);
            }

            return iRes;
        }

        /// <summary>Deletes the distribution tree.</summary>
        /// <param name="retry">an optional hashet to put the retries in. When null, no retries will
        ///     be done.</param>
        public void Delete(System.Collections.Generic.HashSet<JaStDev.HAB.Neuron> retry = null)
        {
            var iToDel = JaStDev.HAB.Factories.Default.NLists.GetBuffer();
            try
            {
                using (var iOut = Root.LinksOut)
                {
                    foreach (var i in iOut)
                    {
                        iToDel.Add(i.To);
                        iToDel.Add(i.Meaning);
                    }
                }

                JaStDev.HAB.Brain.Current.Delete(Root);
                foreach (var i in iToDel)
                {
                    Delete(i, retry);
                }
            }
            finally
            {
                JaStDev.HAB.Factories.Default.NLists.Recycle(iToDel);
            }
        }

        /// <summary>The delete.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="retry">The retry.</param>
        private void Delete(JaStDev.HAB.Neuron neuron, System.Collections.Generic.HashSet<JaStDev.HAB.Neuron> retry)
        {
            if (neuron != null)
            {
                if (neuron.CanBeDeleted && JaStDev.HAB.BrainHelper.HasReferences(neuron) == false)
                {
                    JaStDev.HAB.Brain.Current.Delete(neuron);
                }
                else if (retry != null && retry.Contains(neuron) == false)
                {
                    retry.Add(neuron);
                }
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DistributionTree"/> class. Initializes a new instance of the <see cref="DistributionTree"/>
        ///     class.</summary>
        /// <param name="root">The root.</param>
        public DistributionTree(ulong root)
        {
            if (root == JaStDev.HAB.Neuron.EmptyId)
            {
                throw new System.ArgumentNullException("root");
            }

            Root = JaStDev.HAB.Brain.Current[root];
        }

        /// <summary>Initializes a new instance of the <see cref="DistributionTree"/> class. Initializes a new instance of the <see cref="DistributionTree"/>
        ///     class.</summary>
        /// <param name="root">The root.</param>
        public DistributionTree(JaStDev.HAB.Neuron root)
        {
            if (root == null)
            {
                throw new System.ArgumentNullException("root");
            }

            Root = root;
        }

        /// <summary>Initializes a new instance of the <see cref="DistributionTree"/> class. 
        ///     Initializes a new instance of the <see cref="DistributionTree"/>
        ///     class.</summary>
        internal DistributionTree()
        {
        }

        /// <summary>
        ///     Creates a new distribution tree. Use the
        ///     <see cref="ML.DistributionTree.Root" /> object to store the root that
        ///     was created for this tree so that it can be loaded again later on.
        /// </summary>
        /// <returns>
        ///     an object that can be used to work with the distribution tree.
        /// </returns>
        public static DistributionTree Create()
        {
            var iRoot = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRoot);
            var iRes = new DistributionTree(iRoot);
            return iRes;
        }

        #endregion
    }
}