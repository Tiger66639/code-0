// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBTest.cs" company="">
//   
// </copyright>
// <summary>
//   The db test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BrainTest
{
    using System.Linq;

    /// <summary>The db test.</summary>
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class DBTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides information about and
        ///     functionality for the current test run.
        /// </summary>
        public Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext { get; set; }

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

        /// <summary>The test link self ref load.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestLinkSelfRefLoad()
        {
            ulong iId;
            var iNew = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron and create a self link.
            JaStDev.HAB.Brain.Current.Add(iNew);
            iId = iNew.ID;
            JaStDev.HAB.Link.Create(iNew, iNew, (ulong)JaStDev.HAB.PredefinedNeurons.Adverb);
            var iDBLoc = System.IO.Path.Combine(TestContext.DeploymentDirectory, "LinkSelfRefTest");
            if (System.IO.Directory.Exists(iDBLoc) == false)
            {
                System.IO.Directory.CreateDirectory(iDBLoc);
            }

            var iFile = System.IO.Path.Combine(iDBLoc, "LinkSelfRefTest.xml");
            JaStDev.HAB.Brain.Current.Save(iFile);

                // save the project and reload it again (make certain that the neuron was unloaded and loaded again.
            JaStDev.HAB.Brain.Current.Clear();
            JaStDev.HAB.Brain.Load(iFile);
            iNew = JaStDev.HAB.Brain.Current[iId];

            try
            {
                JaStDev.HAB.Link iOut = null; // get the 2 link objects.
                using (var iLinks = iNew.LinksOut)
                    if (iLinks.Count == 1)
                    {
                        iOut = iLinks[0];
                    }

                JaStDev.HAB.Link iIn = null; // get the 2 link objects.
                using (var iLinks = iNew.LinksIn)
                    if (iLinks.Count == 1)
                    {
                        iIn = iLinks[0];
                    }

                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(iOut, iIn);
            }
            finally
            {
                JaStDev.HAB.Brain.Current.Clear();

                    // make certain that the db is unloaed again, otherwise the files will remain open.
                System.IO.Directory.Delete(iDBLoc, true); // do some cleanup
            }
        }

        /// <summary>The test inverse link out resolve.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestInverseLinkOutResolve()
        {
            JaStDev.HAB.Neuron iSecond;
            ulong iId;
            var iLoadAlso = new System.Collections.Generic.List<ulong>();

                // list of ids that we want to load before loading iId again.
            var iNew = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
            JaStDev.HAB.Brain.Current.Add(iNew);
            iId = iNew.ID;
            for (var i = 0; i < 1000; i++)
            {
                // create enough new neurons and links to the first one, so we can have an inverse link resolve later on.
                iSecond = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
                JaStDev.HAB.Brain.Current.Add(iSecond);
                if (iLoadAlso.Count > 5)
                {
                    // we want to load the first 5 neurons before loading the other side, so that the inverse resolve can find some items in the cache.
                    iLoadAlso.Add(iNew.ID);
                }

                JaStDev.HAB.Link.Create(iNew, iSecond, (ulong)JaStDev.HAB.PredefinedNeurons.Adverb);
            }

            JaStDev.HAB.Link.Create(iNew, iNew, (ulong)JaStDev.HAB.PredefinedNeurons.Adjective);

                // also create a self links
            iSecond = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
            JaStDev.HAB.Brain.Current.Add(iSecond);
            JaStDev.HAB.Link.Create(iNew, iSecond, (ulong)JaStDev.HAB.PredefinedNeurons.Adjective);

                // make certain that there is another type of link as well.
            var iDBLoc = System.IO.Path.Combine(TestContext.DeploymentDirectory, "InverseLinkOutResolve");
            if (System.IO.Directory.Exists(iDBLoc) == false)
            {
                System.IO.Directory.CreateDirectory(iDBLoc);
            }

            var iFile = System.IO.Path.Combine(iDBLoc, "InverseLinkOutResolve.xml");
            JaStDev.HAB.Brain.Current.Save(iFile);

                // save the project and reload it again (make certain that the neuron was unloaded and loaded again.
            JaStDev.HAB.Brain.Current.Clear();
            JaStDev.HAB.Brain.Load(iFile);

            try
            {
                var iTemp = new System.Collections.Generic.List<JaStDev.HAB.Neuron>();
                foreach (var i in iLoadAlso)
                {
                    // load the items in advance of iId so that we have something int he cache.
                    iTemp.Add(JaStDev.HAB.Brain.Current[i]);
                }

                iNew = JaStDev.HAB.Brain.Current[iId]; // the loading of the item itself.

                using (var iLinks = iNew.LinksOut) Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1002, iLinks.Count);
                foreach (var i in iTemp)
                {
                    // verify the result.
                    JaStDev.HAB.Link iOut = null; // get the 2 link objects.
                    using (var iLinks = iNew.LinksOut) iOut = (from u in iLinks where u.ToID == iId select u).FirstOrDefault();

                    JaStDev.HAB.Link iIn = null; // get the 2 link objects.
                    using (var iLinks = iNew.LinksIn) iIn = (from u in iLinks where u.FromID == iId select u).FirstOrDefault();

                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(iOut, iId);
                }
            }
            finally
            {
                JaStDev.HAB.Brain.Current.Clear();

                    // make certain that the db is unloaed again, otherwise the files will remain open.
                System.IO.Directory.Delete(iDBLoc, true); // do some cleanup
            }
        }

        /// <summary>The test inverse link in resolve.</summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestInverseLinkInResolve()
        {
            JaStDev.HAB.Neuron iSecond;
            ulong iId;
            var iLoadAlso = new System.Collections.Generic.List<ulong>();

                // list of ids that we want to load before loading iId again.
            var iNew = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
            JaStDev.HAB.Brain.Current.Add(iNew);
            iId = iNew.ID;
            for (var i = 0; i < 1000; i++)
            {
                // create enough new neurons and links to the first one, so we can have an inverse link resolve later on.
                iSecond = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
                JaStDev.HAB.Brain.Current.Add(iSecond);
                if (iLoadAlso.Count > 5)
                {
                    // we want to load the first 5 neurons before loading the other side, so that the inverse resolve can find some items in the cache.
                    iLoadAlso.Add(iNew.ID);
                }

                JaStDev.HAB.Link.Create(iSecond, iNew, (ulong)JaStDev.HAB.PredefinedNeurons.Adverb);
            }

            JaStDev.HAB.Link.Create(iNew, iNew, (ulong)JaStDev.HAB.PredefinedNeurons.Adjective);

                // also create a self links
            iSecond = JaStDev.HAB.NeuronFactory.GetNeuron(); // create new neuron 
            JaStDev.HAB.Brain.Current.Add(iSecond);
            JaStDev.HAB.Link.Create(iSecond, iNew, (ulong)JaStDev.HAB.PredefinedNeurons.Adjective);

                // make certain that there is another type of link as well.
            var iDBLoc = System.IO.Path.Combine(TestContext.DeploymentDirectory, "InverseLinkInResolve");
            if (System.IO.Directory.Exists(iDBLoc) == false)
            {
                System.IO.Directory.CreateDirectory(iDBLoc);
            }

            var iFile = System.IO.Path.Combine(iDBLoc, "InverseLinkInResolve.xml");
            JaStDev.HAB.Brain.Current.Save(iFile);

                // save the project and reload it again (make certain that the neuron was unloaded and loaded again.
            JaStDev.HAB.Brain.Current.Clear();
            JaStDev.HAB.Brain.Load(iFile);

            try
            {
                var iTemp = new System.Collections.Generic.List<JaStDev.HAB.Neuron>();
                foreach (var i in iLoadAlso)
                {
                    // load the items in advance of iId so that we have something int he cache.
                    iTemp.Add(JaStDev.HAB.Brain.Current[i]);
                }

                iNew = JaStDev.HAB.Brain.Current[iId]; // the loading of the item itself.

                using (var iLinks = iNew.LinksIn) Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1002, iLinks.Count);
                foreach (var i in iTemp)
                {
                    // verify the result.
                    JaStDev.HAB.Link iIn = null; // get the 2 link objects.
                    using (var iLinks = iNew.LinksIn) iIn = (from u in iLinks where u.FromID == iId select u).FirstOrDefault();

                    JaStDev.HAB.Link iOut = null; // get the 2 link objects.
                    using (var iLinks = iNew.LinksOut) iOut = (from u in iLinks where u.ToID == iId select u).FirstOrDefault();

                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(iOut, iId);
                }
            }
            finally
            {
                JaStDev.HAB.Brain.Current.Clear();

                    // make certain that the db is unloaed again, otherwise the files will remain open.
                System.IO.Directory.Delete(iDBLoc, true); // do some cleanup
            }
        }
    }
}