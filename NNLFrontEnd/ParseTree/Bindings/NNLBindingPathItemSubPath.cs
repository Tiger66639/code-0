// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathItemSubPath.cs" company="">
//   
// </copyright>
// <summary>
//   The nnl binding path item sub path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The nnl binding path item sub path.</summary>
    internal class NNLBindingPathItemSubPath : NNLBindingPathDotItem
    {
        /// <summary>The f node.</summary>
        private NNLStatementNode fNode;

        #region Node

        /// <summary>
        ///     Gets/sets the sub node.
        /// </summary>
        public NNLStatementNode Node
        {
            get
            {
                return fNode;
            }

            set
            {
                fNode = value;
                fNode.Parent = this;
            }
        }

        #endregion

        /// <summary>renders the item as a parameter value for a setter function. This
        ///     fails by default.</summary>
        /// <param name="renderTo"></param>
        internal override void RenderParam(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                Node.Render(renderTo);
                Item = Node.Item;
            }
        }

        /// <summary>Tries the render static.</summary>
        /// <param name="renderTo">The render to.</param>
        protected override void TryRenderStatic(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                Node.Render(renderTo);
                Item = Node.Item;
            }
        }
    }
}