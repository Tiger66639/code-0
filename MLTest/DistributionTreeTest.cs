// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DistributionTreeTest.cs" company="">
//   
// </copyright>
// <summary>
//   Tests the distributionTree
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MLTest
{
    /// <summary>
    ///     Tests the distributionTree
    /// </summary>
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class DistributionTreeTest
    {
        /// <summary>The f root.</summary>
        private ulong fRoot;

        /// <summary>The f tree.</summary>
        private ML.DistributionTree fTree;

        /// <summary>The init db.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void InitDB()
        {
            if (JaStDev.HAB.Brain.Current != null)
            {
                JaStDev.HAB.Brain.Current.Clear();

                    // make certain we have an empty brain so we can create a new tree and then delete it.
            }

            JaStDev.HAB.Brain.New(); // need to make certain that we have a new brain for this test.
        }

        /// <summary>The test invalid open.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        [Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(typeof(System.ArgumentNullException))]
        public void TestInvalidOpen()
        {
            fTree = new ML.DistributionTree(null);
        }

        /// <summary>The test invalid open 2.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        [Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(typeof(System.ArgumentNullException))]
        public void TestInvalidOpen2()
        {
            fTree = new ML.DistributionTree(JaStDev.HAB.Neuron.EmptyId);
        }

        /// <summary>
        ///     Tests the create.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestCreate()
        {
            fTree = ML.DistributionTree.Create();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree.Root);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(JaStDev.HAB.Neuron.EmptyId, fTree.Root.ID);
            fRoot = fTree.Root.ID;
        }

        /// <summary>The test open.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestOpen()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            fTree = new ML.DistributionTree(fRoot);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree.Root);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(fRoot, fTree.Root.ID);
        }

        /// <summary>The test open 2.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestOpen2()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            fTree = new ML.DistributionTree(JaStDev.HAB.Brain.Current[fRoot]);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree.Root);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(fRoot, fTree.Root.ID);
        }

        /// <summary>The test tree.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTree()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            TrainTree();

            var iRes = fTree.GetMostUsed();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(iRes);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(JaStDev.HAB.TextNeuron), iRes.GetType());
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("a", ((JaStDev.HAB.TextNeuron)iRes).Text);
        }

        /// <summary>The train tree.</summary>
        private void TrainTree()
        {
            fTree.Train("a");
            fTree.Train("a");
            fTree.Train("a");
            fTree.Train(1);
            fTree.Train(1);
            fTree.Train(2);
            fTree.Train(true);
        }

        /// <summary>The test delete.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDelete()
        {
            // Debugger.Break();
            var iTotal = JaStDev.HAB.Brain.Current.NeuronCount;

            fTree = ML.DistributionTree.Create(); // create a new tree so we can test 
            TrainTree();
            fTree.Delete();
            fTree = null;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(iTotal, JaStDev.HAB.Brain.Current.NeuronCount);
        }
    }
}