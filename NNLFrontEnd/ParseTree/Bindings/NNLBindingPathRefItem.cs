// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathRefItem.cs" company="">
//   
// </copyright>
// <summary>
//   a binding item that references a code item by means of a
//   <see langword="namespace" /> path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a binding item that references a code item by means of a
    ///     <see langword="namespace" /> path.
    /// </summary>
    internal class NNLBindingPathRefItem : NNLBindingPathItem
    {
        /// <summary>The f node.</summary>
        private NNLStatementNode fNode;

        /// <summary>
        ///     the node to use as value. This is usually a path to a variable or
        ///     another binding path wrapped round a unary statement.
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
                if (fNode != null && fNode.Parent == null)
                {
                    fNode.Parent = this;
                }
            }
        }

        /// <summary>should be implemented by descendents and return the bindingItem that
        ///     should be used that matches the <see langword="operator"/> for this
        ///     path Item and which is a child of the specified binding item.</summary>
        /// <param name="from">The parent binding item that defines which are the possible next
        ///     binding items. (this is a state machine that determins how a path can
        ///     be constructed)</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        internal override NNLBindItemBase GetNextCallBackItemFrom(NNLBindItemBase from)
        {
            NNLBindItemBase iRes;
            from.BindingItems.TryGetValue(Token.Dot, out iRes);
            return iRes;
        }

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The dest to render to.</param>
        /// <param name="bindItem">The binding item that defines all the functions that can be called at
        ///     the current stage.</param>
        /// <param name="prev">The prev item, if any (so the return type can be checked)</param>
        /// <param name="prevType">The prev Type.</param>
        internal override void RenderGet(
            NNLModuleCompiler renderTo, 
            NNLBindItemBase bindItem, 
            NNLBindingPathItem prev, 
            DeclType prevType)
        {
            if (Item == null && Node != null)
            {
                Node.Render(renderTo);
                Item = Node.Item;
                if (Item == null)
                {
                    Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                        // if we didn't find a value, assign the empty neuron so other parts of the compiler don't render to many unwanted errors.
                }

                ExtraItems = Node.ExtraItems;
                if (Item.ID != (ulong)PredefinedNeurons.ReturnValue)
                {
                    // when the node references a function definition and not a sub-path (or @var), we don't need to include it in BindingPathCodeRefs. The
                    renderTo.BindingPathCodeRefs.Add(Item);

                        // whenever a get is rendered for a sub path, we need to make certain that the result is correctly passed along in case that the binding overrides the 'get' method with another function .
                }
            }
        }

        /// <summary>Gets the type decl.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="bindItem">The bind Item.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo, NNLBindItemBase bindItem, DeclType prev)
        {
            if (Node != null)
            {
                return Node.GetTypeDecl(renderTo);
            }

            return DeclType.none;
        }
    }
}