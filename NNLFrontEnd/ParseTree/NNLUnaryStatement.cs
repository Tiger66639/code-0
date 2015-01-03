// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLUnaryStatement.cs" company="">
//   
// </copyright>
// <summary>
//   unary statements (like ref,...)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     unary statements (like ref,...)
    /// </summary>
    internal class NNLUnaryStatement : NNLStatementNode
    {
        /// <summary>The f child.</summary>
        private NNLStatementNode fChild;

        /// <summary>Initializes a new instance of the <see cref="NNLUnaryStatement"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLUnaryStatement(NodeType type)
            : base(type)
        {
        }

        #region Child

        /// <summary>
        ///     Gets/sets the child of this node.
        /// </summary>
        public NNLStatementNode Child
        {
            get
            {
                return fChild;
            }

            set
            {
                fChild = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        #region Operator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> used for the operation.
        /// </summary>
        public Token Operator { get; set; }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                switch (Operator)
                {
                    case Token.Variable:
                        BuildReadVarPath(renderTo);
                        break;
                    case Token.ThesVariable:
                        BuildReadThesPath(renderTo);
                        break;
                    case Token.At:
                        BuildReadCodeVarPAth(renderTo);
                        break;
                    case Token.Minus:
                        BuildUnaryStat(GetInvertOp(renderTo), renderTo);
                        break;
                    case Token.Not:
                        BuildUnaryStat((ulong)PredefinedNeurons.NotInstruction, renderTo);
                        break;
                    case Token.AssetVariable:
                        BuildReadAssetPath(renderTo);
                        break;
                    case Token.TopicRef:
                        BuildReadTopicPath(renderTo);
                        break;
                    case Token.ByRef:
                        BuildByref(renderTo);
                        break;
                    default:
                        LogPosError("Internal error: Unknown unary operator", renderTo);
                        break;
                }
            }
        }

        /// <summary>The build read code var p ath.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildReadCodeVarPAth(NNLModuleCompiler renderTo)
        {
            var iBind = FirstNodeParent.GetBindingFor(Token.At, renderTo);
            if (iBind == null)
            {
                // a path to a code var can always be handled, doesn't have to be externally defined, but it is allowed. When not done, create a temp object.
                iBind = new NNLBinding();
            }
            else
            {
                iBind.ResolveAllReferences(renderTo);

                    // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
            }

            ((NNLBindingPathNode)Child).RenderGet(renderTo, iBind);
            Item = Child.Item;
        }

        /// <summary>The build read topic path.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildReadTopicPath(NNLModuleCompiler renderTo)
        {
            var iBind = FirstNodeParent.GetBindingFor(Token.TopicRef, renderTo);
            if (iBind != null)
            {
                iBind.ResolveAllReferences(renderTo);

                    // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                ((NNLBindingPathNode)Child).RenderGet(renderTo, iBind);
            }
            else
            {
                LogPosError("No binding specified for topic paths in the current scope.", renderTo);
            }

            Item = Child.Item;
        }

        /// <summary>The build read asset path.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildReadAssetPath(NNLModuleCompiler renderTo)
        {
            var iBind = FirstNodeParent.GetBindingFor(Token.AssetVariable, renderTo);
            if (iBind != null)
            {
                iBind.ResolveAllReferences(renderTo);

                    // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                ((NNLBindingPathNode)Child).RenderGet(renderTo, iBind);
            }
            else
            {
                LogPosError("No binding specified for asset paths in the current scope.", renderTo);
            }

            Item = Child.Item;
        }

        /// <summary>The build read thes path.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildReadThesPath(NNLModuleCompiler renderTo)
        {
            var iBind = FirstNodeParent.GetBindingFor(Token.ThesVariable, renderTo);
            if (iBind != null)
            {
                iBind.ResolveAllReferences(renderTo);

                    // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                ((NNLBindingPathNode)Child).RenderGet(renderTo, iBind);
            }
            else
            {
                LogPosError("No binding specified for thesaurus paths in the current scope.", renderTo);
            }

            Item = Child.Item;
        }

        /// <summary>The build read var path.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildReadVarPath(NNLModuleCompiler renderTo)
        {
            var iBind = FirstNodeParent.GetBindingFor(Token.Variable, renderTo);
            if (iBind != null)
            {
                iBind.ResolveAllReferences(renderTo);

                    // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                ((NNLBindingPathNode)Child).RenderGet(renderTo, iBind);
            }
            else
            {
                LogPosError("No binding specified for variable paths in the current scope.", renderTo);
            }

            Item = Child.Item;
        }

        /// <summary>Builds the byref.</summary>
        /// <param name="renderTo">The render to.</param>
        private void BuildByref(NNLModuleCompiler renderTo)
        {
            Child.Render(renderTo);
            if (Child.Item != null)
            {
                foreach (var i in Child.Item.FindAllIn((ulong)PredefinedNeurons.Argument))
                {
                    if (i is ByRefExpression)
                    {
                        Item = i;
                        break;
                    }
                }

                if (Item == null)
                {
                    var iRes = NeuronFactory.Get<ByRefExpression>();
                    Brain.Current.Add(iRes);
                    iRes.Argument = Child.Item;
                    Item = iRes;
                }

                renderTo.Add(this);
            }
        }

        /// <summary>Gets the <see langword="operator"/> for the invert sign.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong GetInvertOp(NNLModuleCompiler renderTo)
        {
            var iType = Child.GetTypeDecl(renderTo);
            switch (iType)
            {
                case DeclType.Var:
                    return (ulong)PredefinedNeurons.InvertSignInstruction;
                case DeclType.Int:
                    return (ulong)PredefinedNeurons.InvertIntInstruction;
                case DeclType.IntAr:
                    return (ulong)PredefinedNeurons.InvertIntListInstruction;
                case DeclType.Double:
                    return (ulong)PredefinedNeurons.InvertDoubleInstruction;
                case DeclType.DoubleAr:
                    return (ulong)PredefinedNeurons.InvertDoubleListInstruction;
                case DeclType.StringAr:
                    LogPosError("Can't invert strings", renderTo);
                    return (ulong)PredefinedNeurons.Empty;
                case DeclType.String:
                    LogPosError("Can't invert strings", renderTo);
                    return (ulong)PredefinedNeurons.Empty;
                default:
                    LogPosError("internal error: Unknown type decleration", renderTo);
                    return (ulong)PredefinedNeurons.Empty;
            }
        }

        /// <summary>Builds the unary stat.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        private void BuildUnaryStat(ulong op, NNLModuleCompiler renderTo)
        {
            Child.Render(renderTo);

            var iArgs = new System.Collections.Generic.List<Neuron> { Child.Item };
            var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
            Item = GetResultStatement(op, iArgCluster, renderTo);
            renderTo.Add(this);
        }

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            NNLBinding iBind;
            switch (Operator)
            {
                case Token.Variable:
                    iBind = FirstNodeParent.GetBindingFor(Token.Variable, renderTo);
                    if (iBind != null)
                    {
                        iBind.ResolveAllReferences(renderTo);

                            // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                        return ((NNLBindingPathNode)Child).GetTypeDecl(renderTo, iBind);
                    }

                    break;
                case Token.ThesVariable:
                    iBind = FirstNodeParent.GetBindingFor(Token.ThesVariable, renderTo);
                    if (iBind != null)
                    {
                        iBind.ResolveAllReferences(renderTo);

                            // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                        return ((NNLBindingPathNode)Child).GetTypeDecl(renderTo, iBind);
                    }

                    break;
                case Token.Minus:
                    return Child.GetTypeDecl(renderTo);
                case Token.Not:
                    return DeclType.Bool;
                case Token.AssetVariable:
                    iBind = FirstNodeParent.GetBindingFor(Token.AssetVariable, renderTo);
                    if (iBind != null)
                    {
                        iBind.ResolveAllReferences(renderTo);

                            // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                        return ((NNLBindingPathNode)Child).GetTypeDecl(renderTo, iBind);
                    }

                    break;
                case Token.TopicRef:
                    iBind = FirstNodeParent.GetBindingFor(Token.TopicRef, renderTo);
                    if (iBind != null)
                    {
                        iBind.ResolveAllReferences(renderTo);

                            // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                        return ((NNLBindingPathNode)Child).GetTypeDecl(renderTo, iBind);
                    }

                    break;
                case Token.At:
                    iBind = FirstNodeParent.GetBindingFor(Token.At, renderTo);
                    if (iBind == null)
                    {
                        // a path to a code var can always be handled, doesn't have to be externally defined, but it is allowed. When not done, create a temp object.
                        iBind = new NNLBinding();
                    }
                    else
                    {
                        iBind.ResolveAllReferences(renderTo);

                            // makes certain that all the binding Items have resolved their references. This is important in case the binding is declared locallly in the same project and has not yet been rendered fully.
                    }

                    return ((NNLBindingPathNode)Child).GetTypeDecl(renderTo, iBind);
                case Token.ByRef:
                    return DeclType.Var;
                default:
                    return DeclType.Var;
            }

            return DeclType.none; // when we get here something went wrong, so no type.
        }
    }
}