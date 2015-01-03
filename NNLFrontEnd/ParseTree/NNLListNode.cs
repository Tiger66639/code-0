// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLListNode.cs" company="">
//   
// </copyright>
// <summary>
//   a custom union node. Renders exactly the same, but it allows us to make a
//   difference between the L() statement and the paremeters for a
//   functioncall (so taht L() is seen as 1 parameter value).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a custom union node. Renders exactly the same, but it allows us to make a
    ///     difference between the L() statement and the paremeters for a
    ///     functioncall (so taht L() is seen as 1 parameter value).
    /// </summary>
    internal class NNLListNode : NNLNodesList
    {
        /// <summary>Initializes a new instance of the <see cref="NNLListNode"/> class.</summary>
        public NNLListNode()
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
                RenderReverseItems(renderTo);
                if (RenderedItems.Count > 1)
                {
                    // if there is only 1, don't need to render 'union'.
                    var iArgs = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
                    Item = GetResultStatement((ulong)PredefinedNeurons.UnionInstruction, iArgs, renderTo);
                }
                else
                {
                    Item = RenderedItems[0];
                }
            }
        }
    }
}