// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathItem.cs" company="">
//   
// </copyright>
// <summary>
//   base class for all binding-path items
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     base class for all binding-path items
    /// </summary>
    internal abstract class NNLBindingPathItem : NNLStatementNode
    {
        /// <summary>Initializes a new instance of the <see cref="NNLBindingPathItem"/> class.</summary>
        public NNLBindingPathItem()
            : base(NodeType.Path)
        {
        }

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            throw new System.NotSupportedException("Requires binding to calculate type decl.");
        }

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The dest to render to.</param>
        /// <param name="bindItem">The binding item that defines all the functions that can be called at
        ///     the current stage.</param>
        /// <param name="prev">The prev item, if any (so the return type can be checked)</param>
        /// <param name="prevType">The prev Type.</param>
        internal abstract void RenderGet(
            NNLModuleCompiler renderTo, 
            NNLBindItemBase bindItem, 
            NNLBindingPathItem prev, 
            DeclType prevType);

        /// <summary>renders the item as a parameter value for a setter function. This
        ///     fails by default.</summary>
        /// <param name="renderTo"></param>
        internal virtual void RenderParam(NNLModuleCompiler renderTo)
        {
            LogPosError(
                "Binding path item can't be converted to a parameter value. Operation is not supported", 
                renderTo);
        }

        /// <summary>The get type decl.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="bindItem">The bind item.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal abstract DeclType GetTypeDecl(NNLModuleCompiler renderTo, NNLBindItemBase bindItem, DeclType prev);

        /// <summary>should be implemented by descendents and return the bindingItem that
        ///     should be used that matches the <see langword="operator"/> for this
        ///     path Item and which is a child of the specified binding item.</summary>
        /// <param name="from">The parent binding item that defines which are the possible next
        ///     binding items. (this is a state machine that determins how a path can
        ///     be constructed)</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        internal abstract NNLBindItemBase GetNextCallBackItemFrom(NNLBindItemBase from);
    }
}