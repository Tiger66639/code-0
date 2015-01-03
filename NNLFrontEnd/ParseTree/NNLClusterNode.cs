// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLClusterNode.cs" company="">
//   
// </copyright>
// <summary>
//   a node for clusters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a node for clusters.
    /// </summary>
    internal class NNLClusterNode : NNLExpBlockNode
    {
        /// <summary>The f meaning.</summary>
        private NNLStatementNode fMeaning;

        #region Meaning

        /// <summary>
        ///     Gets/sets the meaning that should be assigned to the cluster.
        /// </summary>
        public NNLStatementNode Meaning
        {
            get
            {
                return fMeaning;
            }

            set
            {
                fMeaning = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iCluster = FindObject(renderTo) as NeuronCluster;
                var iMeaningId = Neuron.EmptyId;
                if (Meaning != null)
                {
                    Meaning.Render(renderTo);
                    var iMeaning = Meaning.Item;
                    if (iMeaning == null)
                    {
                        LogPosError("Unknown reference: " + Meaning, renderTo);
                    }
                    else
                    {
                        iMeaningId = iMeaning.ID;
                    }
                }

                if (Content == null && iCluster == null)
                {
                    iCluster = NeuronFactory.GetCluster();
                    Brain.Current.Add(iCluster);
                    Item = iCluster; // need to do this so we can add it to the rendering engine.
                    renderTo.Add(this);

                        // only do this when newly created here. When we found the item in the network, don't register again, cause it's not part of the code (the code contained an id, as a refernce to already existing item).
                }
                else if (iCluster == null)
                {
                    Content.Render(renderTo, iMeaningId);

                        // need to call a render, so that the children (params) are also rendered)
                    iCluster = (NeuronCluster)Content.Item;
                    Item = iCluster;
                    renderTo.Add(this);

                        // only do this when newly created here. When we found the item in the network, don't register again, cause it's not part of the code (the code contained an id, as a refernce to already existing item).
                }
                else if (Content != null)
                {
                    Content.RenderItemsInto(iCluster, renderTo);
                    Item = iCluster;
                }

                if (iMeaningId != Neuron.EmptyId)
                {
                    iCluster.Meaning = iMeaningId;
                }

                RenderFunctions(renderTo);
                RenderLinks(renderTo);
                ProcessAttributes(renderTo);
            }
        }
    }
}