// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DTree.cs" company="">
//   
// </copyright>
// <summary>
//   Provides decision tree functionality on the NN DB.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ML
{
    using System.Linq;

    /// <summary>
    ///     Provides decision tree functionality on the NN DB.
    /// </summary>
    public class DTree
    {
        /// <summary>The f end point.</summary>
        private static JaStDev.HAB.Neuron fEndPoint;

        /// <summary>The f accepts null.</summary>
        private JaStDev.HAB.NeuronCluster fAcceptsNull;

        /// <summary>The f distributions.</summary>
        private System.Collections.Generic.List<DistributionTree> fDistributions;

                                                                  // a helper class for training, if we allow for null values.
        #region Root

        /// <summary>
        ///     Gets the root of the decision tree. Store the id so it can be loaded
        ///     again later on.
        /// </summary>
        public JaStDev.HAB.Neuron Root { get; private set; }

        #endregion

        #region AcceptsNull

        /// <summary>
        ///     Gets the value that indicates if this tree can process
        ///     <see langword="null" /> values or not. When true, the tree will be a
        ///     little slower.
        /// </summary>
        public bool AcceptsNull
        {
            get
            {
                return fAcceptsNull != null;
            }
        }

        #endregion

        #region EndPoint

        /// <summary>
        ///     Gets the neuron used as the meaning for a link from the last leaf to
        ///     the result, to indicate that the last value has been found.
        /// </summary>
        /// <value>
        ///     The end point.
        /// </value>
        public static JaStDev.HAB.Neuron EndPoint
        {
            get
            {
                if (fEndPoint == null)
                {
                    fEndPoint = JaStDev.HAB.TextNeuron.GetFor("endpoint");
                }

                return fEndPoint;
            }
        }

        #endregion

        #region Distributions

        /// <summary>
        ///     Gets the list of distributions currently attached to the the tree.
        /// </summary>
        public System.Collections.Generic.List<DistributionTree> Distributions
        {
            get
            {
                if (fDistributions == null)
                {
                    fDistributions = new System.Collections.Generic.List<DistributionTree>();
                    using (JaStDev.HAB.IDListAccessor iChildren = fAcceptsNull.Children)
                        fDistributions.AddRange(
                            from i in iChildren.ConvertTo<JaStDev.HAB.Neuron>() select new DistributionTree(i));
                }

                return fDistributions;
            }
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DTree"/> class. Load a decision tree into a new DTree object.</summary>
        /// <param name="root">The root of the decision tree</param>
        public DTree(ulong root)
        {
            if (root == JaStDev.HAB.Neuron.EmptyId)
            {
                throw new System.ArgumentNullException("root");
            }

            Root = JaStDev.HAB.Brain.Current[root];
            SetAcceptsNull();
        }

        /// <summary>Initializes a new instance of the <see cref="DTree"/> class.</summary>
        /// <param name="root">The root of the decision tree.</param>
        public DTree(JaStDev.HAB.Neuron root)
        {
            if (root == null)
            {
                throw new System.ArgumentNullException("root");
            }

            Root = root;
            SetAcceptsNull();
        }

        /// <summary>
        ///     calculates if the tree accepts nulls or not.
        /// </summary>
        private void SetAcceptsNull()
        {
            JaStDev.HAB.Neuron iNullable = JaStDev.HAB.TextNeuron.GetFor("nullablefor");
            fAcceptsNull = Root.FindFirstOut(iNullable.ID) as JaStDev.HAB.NeuronCluster;
        }

        /// <summary>Creates a new decision tree. Use the <see cref="ML.DTree.Root"/>
        ///     object to store the root that was created for this tree so that it can
        ///     be loaded again later on.</summary>
        /// <param name="nullable">if set to <c>true</c> the tree will except and process<see langword="null"/> values (slower), otherwise<see langword="null"/> values will not be excepted.</param>
        /// <returns>an object that can be used to work with the decision tree.</returns>
        public static DTree Create(bool nullable)
        {
            var iRoot = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRoot);
            var iRes = new DTree(iRoot);
            if (nullable)
            {
                JaStDev.HAB.Neuron iNullable = JaStDev.HAB.TextNeuron.GetFor("nullablefor");
                System.Diagnostics.Debug.Assert(iNullable != null);
                iRes.fAcceptsNull = JaStDev.HAB.NeuronFactory.GetCluster();

                    // this becomes the root for the distribution tree, so we can process 'null' values correctly.
                JaStDev.HAB.Brain.Current.Add(iRes.fAcceptsNull);
                JaStDev.HAB.Link.Create(iRes.fAcceptsNull, iRoot, iNullable); // we attach 
            }

            return iRes;
        }

        #endregion

        #region train

        /// <summary>Trains the decision tree with the specified variables.</summary>
        /// <param name="variables">The list of variables. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <param name="result">The result value to assign to the variable sequence.</param>
        public void Train(System.Collections.IEnumerable variables, bool result)
        {
            if (result)
            {
                Train(variables, JaStDev.HAB.Brain.Current.TrueNeuron);
            }
            else
            {
                Train(variables, JaStDev.HAB.Brain.Current.FalseNeuron);
            }
        }

        /// <summary>Trains the decision tree with the specified variables.</summary>
        /// <param name="variables">The list of variables. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <param name="result">The result value to assign to the variable sequence.</param>
        public void Train(System.Collections.IEnumerable variables, string result)
        {
            Train(variables, JaStDev.HAB.BrainHelper.GetNeuronForText(result));
        }

        /// <summary>Trains the decision tree with the specified variables.</summary>
        /// <param name="variables">The list of variables. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <param name="result">The result value to assign to the variable sequence.</param>
        public void Train(System.Collections.IEnumerable variables, int result)
        {
            JaStDev.HAB.Neuron iRes = JaStDev.HAB.NeuronFactory.GetInt(result);
            JaStDev.HAB.Brain.Current.Add(iRes);
            Train(variables, iRes);
        }

        /// <summary>Trains the decision tree with the specified variables.</summary>
        /// <param name="variables">The list of variables. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <param name="result">The result value to assign to the variable sequence.</param>
        public void Train(System.Collections.IEnumerable variables, double result)
        {
            JaStDev.HAB.Neuron iRes = JaStDev.HAB.NeuronFactory.GetDouble(result);
            JaStDev.HAB.Brain.Current.Add(iRes);
            Train(variables, iRes);
        }

        /// <summary>Trains the decision tree with the specified variables.</summary>
        /// <param name="variables">The list of variables. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <param name="result">The result value to assign to the variable sequence.</param>
        public void Train(System.Collections.IEnumerable variables, JaStDev.HAB.Neuron result)
        {
            var iCurPos = Root;
            if (iCurPos != null)
            {
                var iVarPos = 0;
                foreach (var i in variables)
                {
                    // handle the list of variable until the leaf was found.
                    if (i == null && AcceptsNull == false)
                    {
                        throw new System.InvalidOperationException("The tree does not except null values");
                    }

                    var iVar = JaStDev.HAB.BrainHelper.GetNeuronFor(i);
                    var iNext = iCurPos.FindFirstOut(iVar.ID);
                    if (iNext == null)
                    {
                        iNext = JaStDev.HAB.NeuronFactory.GetNeuron();
                        JaStDev.HAB.Brain.Current.Add(iNext);
                        JaStDev.HAB.Link.Create(iCurPos, iNext, iVar);
                    }

                    if (i != null && AcceptsNull)
                    {
                        StoreDistribution(iVarPos, iVar);
                    }

                    iCurPos = iNext;
                    iVarPos++;
                }

                var iEnd = iCurPos.FindFirstOut(EndPoint.ID);

                    // every leaf points to an endpoint that contains the counts for each individual result that was found for the leaf.
                if (iEnd == null)
                {
                    iEnd = JaStDev.HAB.NeuronFactory.GetNeuron();
                    JaStDev.HAB.Brain.Current.Add(iEnd);
                    JaStDev.HAB.Link.Create(iCurPos, iEnd, EndPoint);
                }

                var iCount = iEnd.FindFirstOut(result.ID) as JaStDev.HAB.IntNeuron;

                    // we keep track of how many times each result was found for the same sequence.
                if (iCount == null)
                {
                    iCount = JaStDev.HAB.NeuronFactory.GetInt(1);
                    JaStDev.HAB.Brain.Current.Add(iCount);
                    JaStDev.HAB.Link.Create(iEnd, iCount, result);
                }
                else
                {
                    iCount.IncValue();
                }
            }
            else
            {
                throw new System.InvalidOperationException("Root not defined.");
            }
        }

        /// <summary>Stores the distribution <paramref name="value"/> for the variable at
        ///     the specified position. This allows us later on, when we encounter a
        ///     'null' value, to convert it to the most frequently used value, which
        ///     gives the best results.</summary>
        /// <param name="varPos">The var position within the list of variables.</param>
        /// <param name="value">The value that was encountered and which we will use to build the
        ///     distribution further.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void StoreDistribution(int varPos, JaStDev.HAB.Neuron value)
        {
            var iDistributionRoot = GetDistributionRoot(varPos);
            iDistributionRoot.Train(value);
        }

        /// <summary>converts the var position to the root of the distribution tree for the
        ///     variable.</summary>
        /// <param name="varPos">The var pos.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>The <see cref="DistributionTree"/>.</returns>
        private DistributionTree GetDistributionRoot(int varPos)
        {
            if (Distributions.Count <= varPos)
            {
                using (JaStDev.HAB.IDListAccessor iChildren = fAcceptsNull.ChildrenW)
                {
                    while (Distributions.Count <= varPos)
                    {
                        var iInt = JaStDev.HAB.NeuronFactory.GetInt(Distributions.Count);
                        JaStDev.HAB.Brain.Current.Add(iInt);
                        iChildren.Add(iInt);
                        Distributions.Add(new DistributionTree(iInt));
                    }
                }
            }

            return Distributions[varPos];
        }

        #endregion

        #region Test

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <returns>A list of values that were attached to the leaf defined by the<paramref name="variables"/> and which were encountered the most for
        ///     that perticular set of <paramref name="variables"/></returns>
        public System.Collections.IEnumerable Test(System.Collections.IList variables)
        {
            foreach (var i in TestNeurons(variables))
            {
                if (i is JaStDev.HAB.IntNeuron)
                {
                    yield return ((JaStDev.HAB.IntNeuron)i).Value;
                }
                else if (i is JaStDev.HAB.DoubleNeuron)
                {
                    yield return ((JaStDev.HAB.DoubleNeuron)i).Value;
                }
                else if (i == JaStDev.HAB.Brain.Current.TrueNeuron)
                {
                    yield return true;
                }
                else if (i == JaStDev.HAB.Brain.Current.FalseNeuron)
                {
                    yield return false;
                }
                else
                {
                    yield return JaStDev.HAB.BrainHelper.GetTextFrom(i);
                }
            }
        }

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <exception cref="System.InvalidCastException">Can't convert neuron to <see langword="bool"/></exception>
        /// <returns>A list of <see langword="bool"/> values that were attached to the leaf
        ///     defined by the <paramref name="variables"/> and which were encountered
        ///     the most for that perticular set of <paramref name="variables"/></returns>
        public System.Collections.Generic.IEnumerable<bool> TestBool(System.Collections.IList variables)
        {
            foreach (var i in TestNeurons(variables))
            {
                if (i == JaStDev.HAB.Brain.Current.TrueNeuron)
                {
                    yield return true;
                }
                else if (i == JaStDev.HAB.Brain.Current.FalseNeuron)
                {
                    yield return false;
                }
                else
                {
                    throw new System.InvalidCastException("Can't convert neuron to bool");
                }
            }
        }

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <exception cref="System.InvalidCastException">can't convert neuron to <see langword="int"/></exception>
        /// <returns>A list of ints that were attached to the leaf defined by the<paramref name="variables"/> and which were encountered the most for
        ///     that perticular set of <paramref name="variables"/></returns>
        public System.Collections.Generic.IEnumerable<int> TestInt(System.Collections.IList variables)
        {
            foreach (var i in TestNeurons(variables))
            {
                if (i is JaStDev.HAB.IntNeuron)
                {
                    yield return ((JaStDev.HAB.IntNeuron)i).Value;
                }
                else if (i is JaStDev.HAB.DoubleNeuron)
                {
                    yield return (int)((JaStDev.HAB.DoubleNeuron)i).Value;
                }
                else if (i == JaStDev.HAB.Brain.Current.TrueNeuron)
                {
                    yield return 1;
                }
                else if (i == JaStDev.HAB.Brain.Current.FalseNeuron)
                {
                    yield return 0;
                }
                else
                {
                    throw new System.InvalidCastException("can't convert neuron to int");
                }
            }
        }

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <exception cref="System.InvalidCastException">can't convert neuron to <see langword="double"/></exception>
        /// <returns>A list of doubles that were attached to the leaf defined by the<paramref name="variables"/> and which were encountered the most for
        ///     that perticular set of <paramref name="variables"/></returns>
        public System.Collections.Generic.IEnumerable<double> TestDouble(System.Collections.IList variables)
        {
            foreach (var i in TestNeurons(variables))
            {
                if (i is JaStDev.HAB.IntNeuron)
                {
                    yield return ((JaStDev.HAB.IntNeuron)i).Value;
                }
                else if (i is JaStDev.HAB.DoubleNeuron)
                {
                    yield return ((JaStDev.HAB.DoubleNeuron)i).Value;
                }
                else if (i == JaStDev.HAB.Brain.Current.TrueNeuron)
                {
                    yield return 1;
                }
                else if (i == JaStDev.HAB.Brain.Current.FalseNeuron)
                {
                    yield return 0;
                }
                else
                {
                    throw new System.InvalidCastException("can't convert neuron to double");
                }
            }
        }

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <returns>A list of strings that were attached to the leaf defined by the<paramref name="variables"/> and which were encountered the most for
        ///     that perticular set of <paramref name="variables"/></returns>
        public System.Collections.Generic.IEnumerable<string> TestString(System.Collections.IList variables)
        {
            foreach (var i in TestNeurons(variables))
            {
                yield return JaStDev.HAB.BrainHelper.GetTextFrom(i);
            }
        }

        /// <summary>
        ///     a helper class for storing info about the branches that the
        ///     <see cref="DTree.TestNeurons" /> still has to take/check.
        /// </summary>
        private class BranchPosition
        {
            /// <summary>The neuron.</summary>
            public JaStDev.HAB.Neuron Neuron;

            /// <summary>The var pos.</summary>
            public int VarPos;
        }

        /// <summary>Pushes the specified <paramref name="variables"/> through the decision
        ///     tree and returns all the highest results (0, 1 or more).</summary>
        /// <param name="variables">The variables to test. This should only contain neuron, bool, int,<see langword="double"/> or string values, and can be mixed</param>
        /// <returns>A list of neurons that were attached to the leaf defined by the<paramref name="variables"/> and which were encountered the most for
        ///     that perticular set of <paramref name="variables"/></returns>
        public System.Collections.Generic.IEnumerable<JaStDev.HAB.Neuron> TestNeurons(
            System.Collections.IList variables)
        {
            var iResults = new System.Collections.Generic.Dictionary<JaStDev.HAB.Neuron, int>();

                // keeps track of the current results: for each neuron a list of % that have been found already. At the end, an average can be taken of each list to produce a final 'max' result
            var iBranchesToVisit = new System.Collections.Generic.List<BranchPosition>();

                // keeps track of all the branches that still needs to be visited.
            BranchPosition iCurPos;
            if (Root != null)
            {
                iBranchesToVisit.Add(new BranchPosition { Neuron = Root, VarPos = 0 });
                var iCurBranch = 0;
                while (iCurBranch < iBranchesToVisit.Count)
                {
                    // we possibly need to traverse multiple paths, in case of 'empty' values, both in input as in tree.
                    iCurPos = iBranchesToVisit[iCurBranch++];
                    for (var iVarPos = iCurPos.VarPos; iVarPos < variables.Count; iVarPos++)
                    {
                        // handle the list of variable until the leaf was found.
                        JaStDev.HAB.Neuron iVar;
                        if (variables[iVarPos] == null)
                        {
                            // the input is null, so we check every possible path at this point.
                            iVar = GetMaxDistributionFor(iCurPos.VarPos);
                        }
                        else
                        {
                            iVar = JaStDev.HAB.BrainHelper.GetNeuronFor(variables[iVarPos]);
                        }

                        var iNext = iCurPos.Neuron.FindFirstOut((ulong)JaStDev.HAB.PredefinedNeurons.Empty);
                        if (iNext != null && iVar == GetMaxDistributionFor(iCurPos.VarPos))
                        {
                            iBranchesToVisit.Add(new BranchPosition { Neuron = iNext, VarPos = iVarPos });
                        }

                        iNext = iCurPos.Neuron.FindFirstOut(iVar.ID);
                        if (iNext == null)
                        {
                            iCurPos = null;
                            break;
                        }

                        iCurPos.Neuron = iNext;
                    }

                    if (iCurPos != null)
                    {
                        // if there is a curPos, check if it is an endpoint. if the current variable set didn't produce a branch, curpos is null.
                        var iEnd = iCurPos.Neuron.FindFirstOut(EndPoint.ID);

                            // every leaf points to an endpoint that contains the counts for each individual result that was found for the leaf.
                        if (iEnd != null)
                        {
                            StoreResults(iEnd, iResults);
                        }
                    }
                }

                var iMax = iResults.Values.Max();

                    // get the maximum value from the dicctionary so we can look up all the values that have a value of Max and return their indexes.
                foreach (var i in iResults)
                {
                    if (i.Value == iMax)
                    {
                        yield return i.Key;
                    }
                }
            }
            else
            {
                throw new System.InvalidOperationException("Root not defined.");
            }
        }

        /// <summary>Stores all the results and their weights in a dictionary, so that we
        ///     can combine the results of several paths (when there were<see langword="null"/> values in the training data)</summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="dict">The dict.</param>
        /// <exception cref="System.InvalidOperationException">Can't find count at end of leaf: can't calculate the result</exception>
        private void StoreResults(
            JaStDev.HAB.Neuron endpoint, System.Collections.Generic.Dictionary<JaStDev.HAB.Neuron, int> dict)
        {
            using (var iOut = endpoint.LinksOut)
            {
                foreach (var i in iOut)
                {
                    var iTo = i.To as JaStDev.HAB.IntNeuron;
                    if (iTo != null)
                    {
                        var iMeaning = i.Meaning;
                        int iFound;
                        if (dict.TryGetValue(iMeaning, out iFound) == false)
                        {
                            dict[iMeaning] = iTo.Value;
                        }
                        else
                        {
                            dict[iMeaning] = iTo.Value + +iFound;
                        }
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            "Can't find count at end of leaf: can't calculate the result");
                    }
                }
            }
        }

        /// <summary>Gets the max value from the distribution tree for the specified
        ///     variable.</summary>
        /// <param name="varPos">The index position of the variable in the list of input variables.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron GetMaxDistributionFor(int varPos)
        {
            if (Distributions.Count > varPos)
            {
                var iTree = Distributions[varPos];
                return iTree.GetMostUsed();
            }

            return JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.Empty];

                // if we can't find a max value for the specified index pos, we still need to use the 'empty' value.
        }

        #endregion

        #region maintenance

        /// <summary>
        ///     Deletes the tree from the db.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     If there is no root
        /// </exception>
        public void Delete()
        {
            if (Root != null)
            {
                var iRetry = new System.Collections.Generic.HashSet<JaStDev.HAB.Neuron>();
                DeleteTree(iRetry, Root);
                DeleteDistribution(iRetry);
                JaStDev.HAB.BrainHelper.DeleteRetries(iRetry);
                Root = null; // make certain that the object knows it has been deleted.
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Deletes the distributions attached to this tree.</summary>
        /// <param name="retry">The retry.</param>
        private void DeleteDistribution(System.Collections.Generic.HashSet<JaStDev.HAB.Neuron> retry)
        {
            foreach (var i in Distributions)
            {
                i.Delete(retry);
            }

            Distributions.Clear();
        }

        /// <summary>recursive delete function. Makes certain that the leafs are deleted
        ///     first.</summary>
        /// <param name="retry">The retry.</param>
        /// <param name="toDelete">To delete.</param>
        private static void DeleteTree(System.Collections.Generic.HashSet<JaStDev.HAB.Neuron> retry, 
            JaStDev.HAB.Neuron toDelete)
        {
            var iChildren = JaStDev.HAB.Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (JaStDev.HAB.ListAccessor<JaStDev.HAB.Link> iOut = toDelete.LinksOut)
                    foreach (var i in iOut)
                    {
                        iChildren.Add(i); // the next branch/leaf.
                    }

                foreach (var iN in iChildren)
                {
                    // walk through the list after the lock so that we don't get lock within lock cause that can give deadlocks.
                    var iTo = iN.To;
                    iN.Destroy();
                    var iMeaning = iN.Meaning;
                    if (iMeaning.CanBeDeleted && JaStDev.HAB.BrainHelper.HasReferences(iMeaning) == false)
                    {
                        JaStDev.HAB.Brain.Current.Delete(iMeaning);
                    }
                    else if (retry.Contains(iMeaning) == false)
                    {
                        // the meaning also needs to be deleted if possible.
                        retry.Add(iMeaning);
                    }

                    DeleteTree(retry, iTo); // first delete the leafs than the internal branches.
                }

                var iEndPoint = toDelete.FindFirstOut(EndPoint.ID);
                if (iEndPoint != null)
                {
                    DeleteEndPoint(retry, iEndPoint);
                }

                if (toDelete.CanBeDeleted && JaStDev.HAB.BrainHelper.HasOutReferences(toDelete) == false)
                {
                    // we use 'HasOutReferences' cause at this stage, we only check the neurons created specifically for the tree, no data values.
                    JaStDev.HAB.Brain.Current.Delete(toDelete);
                }
                else if (retry.Contains(toDelete) == false)
                {
                    retry.Add(toDelete);
                }
            }
            finally
            {
                JaStDev.HAB.Factories.Default.LinkLists.Recycle(iChildren);
            }
        }

        /// <summary>Deletes the end point, the results and counts linking to the endpoint.</summary>
        /// <param name="retry">The retry.</param>
        /// <param name="endPoint">The end point.</param>
        private static void DeleteEndPoint(System.Collections.Generic.HashSet<JaStDev.HAB.Neuron> retry, 
            JaStDev.HAB.Neuron endPoint)
        {
            var iToDel = JaStDev.HAB.Factories.Default.NLists.GetBuffer();
            try
            {
                using (JaStDev.HAB.ListAccessor<JaStDev.HAB.Link> iLinks = endPoint.LinksOut)
                {
                    foreach (var i in iLinks)
                    {
                        iToDel.Add(i.To);
                        iToDel.Add(i.Meaning);
                    }
                }

                JaStDev.HAB.Brain.Current.Delete(endPoint);
                foreach (var i in iToDel)
                {
                    if (i.CanBeDeleted)
                    {
                        JaStDev.HAB.Brain.Current.Delete(i);
                    }
                    else
                    {
                        retry.Add(i);
                    }
                }
            }
            finally
            {
                JaStDev.HAB.Factories.Default.NLists.Recycle(iToDel);
            }
        }

        #endregion
    }
}