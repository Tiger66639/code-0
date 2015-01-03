// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DTreeTest.cs" company="">
//   
// </copyright>
// <summary>
//   The d tree test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MLTest
{
    /// <summary>The d tree test.</summary>
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class DTreeTest
    {
        /// <summary>The f root.</summary>
        private ulong fRoot;

        /// <summary>The f tree.</summary>
        private ML.DTree fTree;

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
            fTree = new ML.DTree(null);
        }

        /// <summary>The test invalid open 2.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        [Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedException(typeof(System.ArgumentNullException))]
        public void TestInvalidOpen2()
        {
            fTree = new ML.DTree(JaStDev.HAB.Neuron.EmptyId);
        }

        /// <summary>
        ///     Tests the creation of a new tree.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestCreate()
        {
            fTree = ML.DTree.Create(true);
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

            fTree = new ML.DTree(fRoot);
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

            fTree = new ML.DTree(JaStDev.HAB.Brain.Current[fRoot]);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fTree.Root);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(fRoot, fTree.Root.ID);
        }

        /// <summary>The train tree bool.</summary>
        private void TrainTreeBool()
        {
            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, true);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, true);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, false);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, false);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, true);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, true);
        }

        /// <summary>The train tree int.</summary>
        private void TrainTreeInt()
        {
            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 1);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 1);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 2);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 2);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 1);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 1);
        }

        /// <summary>The train tree double.</summary>
        private void TrainTreeDouble()
        {
            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 1.1);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 1.1);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 2.1);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 2.1);

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, 1.1);
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, 1.1);
        }

        /// <summary>The train tree text.</summary>
        private void TrainTreeText()
        {
            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, "a");
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, "a");

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, "b");
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, "b");

            fTree.Train(new System.Collections.Generic.List<object> { 1, 1.1, "test" }, "a");
            fTree.Train(new System.Collections.Generic.List<object> { true, fTree.Root, "test" }, "a");
        }

        /// <summary>
        ///     Tests training and testing a tree.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTreeBool()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            TrainTreeBool();
            var iRes =
                new System.Collections.Generic.List<bool>(
                    fTree.TestBool(new System.Collections.Generic.List<object> { 1, 1.1, "test" }));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, iRes.Count);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(true, iRes[0]);
        }

        /// <summary>
        ///     Tests training and testing a tree.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTreeInt()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            TrainTreeInt();
            var iRes =
                new System.Collections.Generic.List<int>(
                    fTree.TestInt(new System.Collections.Generic.List<object> { 1, 1.1, "test" }));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, iRes.Count);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, iRes[0]);
        }

        /// <summary>
        ///     Tests training and testing a tree.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTreeDouble()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            TrainTreeDouble();
            var iRes =
                new System.Collections.Generic.List<double>(
                    fTree.TestDouble(new System.Collections.Generic.List<object> { 1, 1.1, "test" }));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, iRes.Count);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1.1, iRes[0]);
        }

        /// <summary>
        ///     Tests training and testing a tree.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTreeString()
        {
            if (fTree == null)
            {
                TestCreate();
            }

            TrainTreeText();
            var iRes =
                new System.Collections.Generic.List<string>(
                    fTree.TestString(new System.Collections.Generic.List<object> { 1, 1.1, "test" }));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, iRes.Count);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("a", iRes[0]);
        }

        /// <summary>The test delete bool.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDeleteBool()
        {
            // Debugger.Break();
            var iTotal = JaStDev.HAB.Brain.Current.NeuronCount;

            fTree = ML.DTree.Create(true); // create a new tree so we can test 
            TrainTreeBool();
            fTree.Delete();
            fTree = null;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(iTotal, JaStDev.HAB.Brain.Current.NeuronCount);
        }

        /// <summary>The test delete int.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDeleteInt()
        {
            // Debugger.Break();
            var iTotal = JaStDev.HAB.Brain.Current.NeuronCount;

            fTree = ML.DTree.Create(true); // create a new tree so we can test 
            TrainTreeInt();
            fTree.Delete();
            fTree = null;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(iTotal, JaStDev.HAB.Brain.Current.NeuronCount);
        }

        /// <summary>The test delete double.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDeleteDouble()
        {
            // Debugger.Break();
            var iTotal = JaStDev.HAB.Brain.Current.NeuronCount;

            fTree = ML.DTree.Create(true); // create a new tree so we can test 
            TrainTreeDouble();
            fTree.Delete();
            fTree = null;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(iTotal, JaStDev.HAB.Brain.Current.NeuronCount);
        }

        /// <summary>The test delete string.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDeleteString()
        {
            // Debugger.Break();
            var iTotal = JaStDev.HAB.Brain.Current.NeuronCount;

            fTree = ML.DTree.Create(true); // create a new tree so we can test 
            TrainTreeText();
            fTree.Delete();
            fTree = null;

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(iTotal, JaStDev.HAB.Brain.Current.NeuronCount);
        }
    }
}