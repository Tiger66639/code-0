// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLUnionNode.cs" company="">
//   
// </copyright>
// <summary>
//   a node used for the union statement (list expression)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a node used for the union statement (list expression)
    /// </summary>
    internal class NNLUnionNode : NNLNodesList
    {
        /// <summary>Initializes a new instance of the <see cref="NNLUnionNode"/> class.</summary>
        public NNLUnionNode()
            : base(NodeType.Union)
        {
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                IsParam = true;

                    // always set to IsParam, so that the RenderItems doesn't add the target to the stack, for others to add extra code to, we are rendering a union, any extra code should be rendered just in front of the union, so no new lists on the renderTargetStack.
                RenderReverseItems(renderTo); // render in revers order to correct for 'REutrnVAlue' which is a stack.
                var iArgs = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
                Item = GetResultStatement((ulong)PredefinedNeurons.UnionInstruction, iArgs, renderTo);
            }
        }
    }
}