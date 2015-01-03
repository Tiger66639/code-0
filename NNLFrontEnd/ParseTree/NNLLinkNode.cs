// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLLinkNode.cs" company="">
//   
// </copyright>
// <summary>
//   reperesents a link that needs to be made. The meanin for the link is in
//   the name of the object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     reperesents a link that needs to be made. The meanin for the link is in
    ///     the name of the object.
    /// </summary>
    internal class NNLLinkNode : NNLStatementNode
    {
        /// <summary>The f to.</summary>
        private NNLStatementNode fTo;

        /// <summary>Initializes a new instance of the <see cref="NNLLinkNode"/> class.</summary>
        public NNLLinkNode()
            : base(NodeType.Link)
        {
        }

        #region To

        /// <summary>
        ///     Gets/sets the location where the link points too.
        /// </summary>
        public NNLStatementNode To
        {
            get
            {
                return fTo;
            }

            set
            {
                fTo = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            To.Render(renderTo);
        }
    }
}