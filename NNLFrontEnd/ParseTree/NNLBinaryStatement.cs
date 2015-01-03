// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBinaryStatement.cs" company="">
//   
// </copyright>
// <summary>
//   The nnl binary statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The nnl binary statement.</summary>
    internal class NNLBinaryStatement : NNLStatementNode
    {
        /// <summary>The f left part.</summary>
        private NNLStatementNode fLeftPart;

        /// <summary>The f right part.</summary>
        private NNLStatementNode fRightPart;

        /// <summary>The f type decl.</summary>
        private DeclType fTypeDecl; // we calculate this during rendering, for ease

        /// <summary>Initializes a new instance of the <see cref="NNLBinaryStatement"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLBinaryStatement(NodeType type)
            : base(type)
        {
        }

        #region LeftPart

        /// <summary>
        ///     Gets/sets the left part of the operation.
        /// </summary>
        public NNLStatementNode LeftPart
        {
            get
            {
                return fLeftPart;
            }

            set
            {
                fLeftPart = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        #region RightPart

        /// <summary>
        ///     Gets/sets the right part of the statement.
        /// </summary>
        public NNLStatementNode RightPart
        {
            get
            {
                return fRightPart;
            }

            set
            {
                fRightPart = value;
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
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                RenderForOperator(renderTo);
            }
        }

        /// <summary>big case statement that does the rendering according to the operator.</summary>
        /// <param name="renderTo"></param>
        private void RenderForOperator(NNLModuleCompiler renderTo)
        {
            switch (Operator)
            {
                case Token.Assign:
                    if (TryBuildBindAssign(renderTo) == false && TryBuildAssignInt(renderTo) == false
                        && TryBuildAssignDouble(renderTo) == false)
                    {
                        BuildAssign(renderTo);
                    }

                    break;
                case Token.DoubleAnd:
                    BuildAndOrBoolean((ulong)PredefinedNeurons.And, renderTo);
                    break;
                case Token.Or:
                    BuildAndOrBoolean((ulong)PredefinedNeurons.Or, renderTo);
                    break;
                case Token.NotAssign:
                    BuildBoolean((ulong)PredefinedNeurons.Different, renderTo);
                    break;
                case Token.NotContains:
                    BuildBoolean((ulong)PredefinedNeurons.NotContains, renderTo);
                    break;
                case Token.Contains:
                    BuildBoolean((ulong)PredefinedNeurons.Contains, renderTo);
                    break;
                case Token.AddAssign:
                    if (TryBuildOpAssignInt(renderTo, (ulong)PredefinedNeurons.AddAssignIntInstruction) == false
                        && TryBuildOpAssignDouble(renderTo, (ulong)PredefinedNeurons.AddAssignDoubleInstruction)
                        == false && TryBuildBindAssign(renderTo) == false)
                    {
                        BuildAssignCalc(renderTo, (ulong)PredefinedNeurons.AdditionInstruction);
                    }

                    break;
                case Token.AddNotAssign:
                    BuildBindingOp((ulong)PredefinedNeurons.AssignAddNot, renderTo);
                    break;
                case Token.MinusAssign:
                    if (TryBuildOpAssignInt(renderTo, (ulong)PredefinedNeurons.MinusAssignIntInstruction) == false
                        && TryBuildOpAssignDouble(renderTo, (ulong)PredefinedNeurons.MinusAssignDoubleInstruction)
                        == false && TryBuildBindAssign(renderTo) == false)
                    {
                        BuildAssignCalc(renderTo, (ulong)PredefinedNeurons.MinusInstruction);
                    }

                    break;
                case Token.AndAssign:
                    BuildBindingOp((ulong)PredefinedNeurons.And, renderTo);
                    break;
                case Token.OrAssign:
                    BuildBindingOp((ulong)PredefinedNeurons.Or, renderTo);
                    break;
                case Token.ListAssign:
                    BuildBindingOp((ulong)PredefinedNeurons.List, renderTo);
                    break;
                case Token.MultiplyAssign:
                    if (TryBuildOpAssignInt(renderTo, (ulong)PredefinedNeurons.MultiplyStoreIntInstruction) == false
                        && TryBuildOpAssignDouble(renderTo, (ulong)PredefinedNeurons.MultiplyStoreDoubleIntruction)
                        == false && TryBuildBindAssign(renderTo) == false)
                    {
                        BuildAssignCalc(renderTo, (ulong)PredefinedNeurons.MultiplyInstruction);
                    }

                    break;
                case Token.DivAssign:
                    if (TryBuildOpAssignInt(renderTo, (ulong)PredefinedNeurons.DivStoreIntInstruction) == false
                        && TryBuildOpAssignDouble(renderTo, (ulong)PredefinedNeurons.DivStoreDoubleInstruction) == false
                        && TryBuildBindAssign(renderTo) == false)
                    {
                        BuildAssignCalc(renderTo, (ulong)PredefinedNeurons.DivideInstruction);
                    }

                    break;
                case Token.ModulusAssign:
                    if (TryBuildOpAssignInt(renderTo, (ulong)PredefinedNeurons.ModStoreIntInstruction) == false
                        && TryBuildOpAssignDouble(renderTo, (ulong)PredefinedNeurons.ModStoreDoubleInstruction) == false
                        && TryBuildBindAssign(renderTo) == false)
                    {
                        BuildAssignCalc(renderTo, (ulong)PredefinedNeurons.ModulusInstruction);
                    }

                    break;
                case Token.IsEqual:
                    BuildBoolean((ulong)PredefinedNeurons.Equal, renderTo);
                    break;
                case Token.IsBiggerThen:
                    BuildBoolean((ulong)PredefinedNeurons.Bigger, renderTo);
                    break;
                case Token.IsBiggerThenOrEqual:
                    BuildBoolean((ulong)PredefinedNeurons.BiggerOrEqual, renderTo);
                    break;
                case Token.IsSmallerThen:
                    BuildBoolean((ulong)PredefinedNeurons.Smaller, renderTo);
                    break;
                case Token.IsSmallerThenOrEqual:
                    BuildBoolean((ulong)PredefinedNeurons.SmallerOrEqual, renderTo);
                    break;
                case Token.OptionStart:
                    CheckBuildCalc((ulong)PredefinedNeurons.GetAtInstruction, renderTo);
                    break;
                case Token.Minus:
                    if (TryBuildOpInt((ulong)PredefinedNeurons.MinusIntInstruction, renderTo) == false
                        && TryBuildOpDouble((ulong)PredefinedNeurons.MinusDoubleInstruction, renderTo) == false)
                    {
                        CheckBuildCalc((ulong)PredefinedNeurons.MinusInstruction, renderTo);
                    }

                    break;
                case Token.Add:
                    if (TryBuildOpInt((ulong)PredefinedNeurons.AddIntInstruction, renderTo) == false
                        && TryBuildOpDouble((ulong)PredefinedNeurons.AddDoubleInstruction, renderTo) == false)
                    {
                        CheckBuildCalc((ulong)PredefinedNeurons.AdditionInstruction, renderTo);
                    }

                    break;
                case Token.Multiply:
                    if (TryBuildOpInt((ulong)PredefinedNeurons.MultiplyIntInstruction, renderTo) == false
                        && TryBuildOpDouble((ulong)PredefinedNeurons.MultiplyDoubleInstruction, renderTo) == false)
                    {
                        CheckBuildCalc((ulong)PredefinedNeurons.MultiplyInstruction, renderTo);
                    }

                    break;
                case Token.Divide:
                    if (TryBuildOpInt((ulong)PredefinedNeurons.DivIntInstruction, renderTo) == false
                        && TryBuildOpDouble((ulong)PredefinedNeurons.DivDoubleInstruction, renderTo) == false)
                    {
                        CheckBuildCalc((ulong)PredefinedNeurons.DivideInstruction, renderTo);
                    }

                    break;
                case Token.Modulus:
                    if (TryBuildOpInt((ulong)PredefinedNeurons.ModIntInstruction, renderTo) == false
                        && TryBuildOpDouble((ulong)PredefinedNeurons.ModDoubleInstruction, renderTo) == false)
                    {
                        CheckBuildCalc((ulong)PredefinedNeurons.ModulusInstruction, renderTo);
                    }

                    break;
                default:
                    LogPosError("Internal error: unknown binary operator: " + Operator, renderTo);
                    break;
            }
        }

        /// <summary>The build assign calc.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="op">The op.</param>
        private void BuildAssignCalc(NNLModuleCompiler renderTo, ulong op)
        {
            if (fRightPart != null)
            {
                fRightPart.Render(renderTo);

                    // important: first render right, then left. If both do a 'return', we need to make certain that args are in the correct order.
            }

            if (fLeftPart != null)
            {
                fLeftPart.Render(renderTo);
            }

            if (fLeftPart.Item != null && fRightPart.Item != null)
            {
                var iArgs = new System.Collections.Generic.List<Neuron> { fLeftPart.Item, fRightPart.Item };
                var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, Name);
                fTypeDecl = GetTypDeclFromInstruction(op);
                Neuron iRight;
                if (fTypeDecl == DeclType.none)
                {
                    iRight = GetStatement(op, iArgCluster, renderTo);
                }
                else
                {
                    iRight = GetResultStatement(op, iArgCluster, renderTo);
                }

                BuildAssign(fLeftPart.Item, iRight, renderTo);
            }
            else
            {
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // assign a neuron to the item so we don't render the same error multiple times.
            }
        }

        /// <summary>The try build var list calc.</summary>
        /// <param name="inst">The inst.</param>
        /// <param name="renderTo">The render to.</param>
        private void TryBuildVarListCalc(ulong inst, NNLModuleCompiler renderTo)
        {
            if (fLeftPart != null)
            {
                fLeftPart.Render(renderTo);
            }
            else
            {
                LogPosError("left side of assignment missing", renderTo);
            }

            if (fRightPart != null)
            {
                fRightPart.Render(renderTo);
            }
            else
            {
                LogPosError("right side of assignment missing", renderTo);
            }

            if (LeftPart.Item is Variable)
            {
                var iArgs = new System.Collections.Generic.List<Neuron>
                                {
                                    GetByRef(LeftPart.Item, renderTo), 
                                    fRightPart.Item
                                };
                var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, Name);
                Item = GetResultStatement(inst, iArgCluster, renderTo);
                fTypeDecl = GetTypDeclFromInstruction(inst);
            }
            else
            {
                LogPosError("variable expected in left side.", renderTo);
            }
        }

        /// <summary>The try build op assign double.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="op">The op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildOpAssignDouble(NNLModuleCompiler renderTo, ulong op)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Double)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Double)
                {
                    BuildCalc(op, renderTo);
                    fTypeDecl = DeclType.Double;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try get type decl op double.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryGetTypeDeclOpDouble(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Double)
            {
                var iRight = RightPart.GetTypeDecl(renderTo);
                if (iRight == DeclType.Double || iRight == DeclType.Int)
                {
                    fTypeDecl = DeclType.Double;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try build op assign int.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="op">The op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildOpAssignInt(NNLModuleCompiler renderTo, ulong op)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Int)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Int)
                {
                    BuildCalc(op, renderTo);
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try get type decl op int.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryGetTypeDeclOpInt(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Int)
            {
                var iRight = RightPart.GetTypeDecl(renderTo);
                if (iRight == DeclType.Int || iRight == DeclType.Double)
                {
                    fTypeDecl = DeclType.Int;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try build op double.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildOpDouble(ulong op, NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Double)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Double)
                {
                    BuildCalc(op, renderTo);
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try build op int.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildOpInt(ulong op, NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Int)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Int)
                {
                    BuildCalc(op, renderTo);
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try build assign int.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildAssignInt(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Int)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Int)
                {
                    if (RightPart is NNLBinaryStatement)
                    {
                        var iRight = (NNLBinaryStatement)RightPart;
                        if (iRight.Operator == Token.Add)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.AddStoreIntInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Minus)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.MinusStoreIntInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Multiply)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.MultiplyStoreIntInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Divide)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.DivStoreIntInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Modulus)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.ModStoreIntInstruction, renderTo);
                        }
                        else
                        {
                            BuildCalc((ulong)PredefinedNeurons.StoreIntInstruction, renderTo);
                        }
                    }
                    else
                    {
                        BuildCalc((ulong)PredefinedNeurons.StoreIntInstruction, renderTo);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>The try get type deck assign int.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryGetTypeDeckAssignInt(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Int)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Int)
                {
                    fTypeDecl = DeclType.Int;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The try build assign double.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildAssignDouble(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Double)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Double)
                {
                    if (RightPart is NNLBinaryStatement && ((NNLBinaryStatement)RightPart).Operator == Token.Add)
                    {
                        var iRight = (NNLBinaryStatement)RightPart;
                        if (iRight.Operator == Token.Add)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.AddStoreDoubleInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Minus)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.MinusStoreDoubleInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Multiply)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.MultiplyStoreDoubleIntruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Divide)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.DivStoreDoubleInstruction, renderTo);
                        }
                        else if (iRight.Operator == Token.Modulus)
                        {
                            Build3ArgCalc((ulong)PredefinedNeurons.ModStoreDoubleInstruction, renderTo);
                        }
                        else
                        {
                            BuildCalc((ulong)PredefinedNeurons.StoreDoubleInstruction, renderTo);
                        }
                    }
                    else
                    {
                        BuildCalc((ulong)PredefinedNeurons.StoreDoubleInstruction, renderTo);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>The try get type decl assign double.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryGetTypeDeclAssignDouble(NNLModuleCompiler renderTo)
        {
            if (LeftPart.GetTypeDecl(renderTo) == DeclType.Double)
            {
                if (RightPart.GetTypeDecl(renderTo) == DeclType.Double)
                {
                    fTypeDecl = DeclType.Double;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The build binding op.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        private void BuildBindingOp(ulong op, NNLModuleCompiler renderTo)
        {
            // if (fRightPart != null) fRightPart.Render(renderTo); -> this gets done in the RenderSet
            var iLeft = LeftPart as NNLUnaryStatement;
            if (iLeft != null && iLeft.Child is NNLBindingPathNode)
            {
                var iPath = (NNLBindingPathNode)iLeft.Child;
                var iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
                if (iBind == null)
                {
                    LogPosError(string.Format("No binding found for {0}", iLeft.Operator), renderTo);
                }
                else
                {
                    iPath.RenderSet(renderTo, iBind, RightPart, Operator);
                }

                Item = iPath.Item;
                fTypeDecl = iPath.GetTypeDecl(renderTo, iBind);
            }
            else
            {
                LogPosError("binding path expected on left side", renderTo);
            }
        }

        /// <summary>The get type decl binding op.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        private void GetTypeDeclBindingOp(ulong op, NNLModuleCompiler renderTo)
        {
            var iLeft = LeftPart as NNLUnaryStatement;
            if (iLeft != null && iLeft.Child is NNLBindingPathNode)
            {
                var iPath = (NNLBindingPathNode)iLeft.Child;
                var iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
                fTypeDecl = iPath.GetTypeDecl(renderTo, iBind);
            }
        }

        /// <summary>checks if a build calc needs to be done or a bind.</summary>
        /// <param name="op"></param>
        /// <param name="renderTo"></param>
        private void CheckBuildCalc(ulong op, NNLModuleCompiler renderTo)
        {
            var iLeft = LeftPart as NNLUnaryStatement;
            if (iLeft != null && iLeft.Child is NNLBindingPathNode)
            {
                var iPath = (NNLBindingPathNode)iLeft.Child;
                var iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
                if (iBind == null)
                {
                    BuildCalc(op, renderTo);
                }
                else
                {
                    iPath.RenderSet(renderTo, iBind, RightPart, Operator);
                }
            }
            else
            {
                BuildCalc(op, renderTo);
            }
        }

        /// <summary>Builds the item in case it is a calculation.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render To.</param>
        private void BuildCalc(ulong op, NNLModuleCompiler renderTo)
        {
            if (fRightPart != null)
            {
                fRightPart.Render(renderTo);

                    // important: first render right, then left. If both do a 'return', we need to make certain that args are in the correct order.
            }

            if (fLeftPart != null)
            {
                fLeftPart.Render(renderTo);
            }

            if (fLeftPart.Item != null && fRightPart.Item != null)
            {
                var iArgs = new System.Collections.Generic.List<Neuron> { fLeftPart.Item, fRightPart.Item };
                var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, Name);
                fTypeDecl = GetTypDeclFromInstruction(op);
                if (fTypeDecl == DeclType.none)
                {
                    Item = GetStatement(op, iArgCluster, renderTo);
                }
                else
                {
                    Item = GetResultStatement(op, iArgCluster, renderTo);
                }
            }
            else
            {
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // assign a neuron to the item so we don't render the same error multiple times.
            }
        }

        /// <summary>builds a calc instruction, but the rightpart is not fully rendered,
        ///     instead it is treated as a binary- operation who's 2 arguments are
        ///     used instead of the binary operation. This is used to build things
        ///     like AddStoreInt, which takes 3 args: the <see langword="int"/> to
        ///     store, and the 2 to add.</summary>
        /// <param name="op"></param>
        /// <param name="renderTo"></param>
        private void Build3ArgCalc(ulong op, NNLModuleCompiler renderTo)
        {
            var iRight = (NNLBinaryStatement)RightPart;
            iRight.RightPart.Render(renderTo);

                // important: first render right, then left. If both do a 'return', we need to make certain that args are in the correct order.
            iRight.LeftPart.Render(renderTo);
            if (fLeftPart != null)
            {
                fLeftPart.Render(renderTo);
            }

            var iArgs = new System.Collections.Generic.List<Neuron>
                            {
                                fLeftPart.Item, 
                                iRight.LeftPart.Item, 
                                iRight.RightPart.Item
                            };
            var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, Name);
            var iInst = Brain.Current[op] as Instruction;
            if (iInst is ResultInstruction)
            {
                // addstorexx is always without result, but we check for this to make certain
                Item = GetResultStatement(op, iArgCluster, renderTo);
            }
            else
            {
                Item = GetStatement(op, iArgCluster, renderTo);
            }

            renderTo.Add(iArgCluster);
            fTypeDecl = GetTypDeclFromInstruction(op);
        }

        // private void CheckBuildBool(ulong op, NNLModuleCompiler renderTo)
        // {
        // NNLUnaryStatement iLeft = LeftPart as NNLUnaryStatement;
        // if (iLeft != null && iLeft.Child is NNLBindingPathNode)
        // {
        // NNLBindingPathNode iPath = (NNLBindingPathNode)iLeft.Child;
        // NNLBinding iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
        // if (iBind == null)
        // BuildBoolean(op, renderTo);
        // else
        // iPath.RenderSet(renderTo, iBind, RightPart, Operator);
        // }
        // else
        // BuildBoolean(op, renderTo);
        // }

        /// <summary>Builds the item as a boolean expression.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render To.</param>
        private void BuildBoolean(ulong op, NNLModuleCompiler renderTo)
        {
            if (Operator == Token.DoubleAnd || Operator == Token.Or)
            {
                if (BuildBoolPartsAndOr(renderTo))
                {
                    Item = CreateBoolExp(op, renderTo);
                }
            }
            else if (BuildBoolParts(renderTo))
            {
                Item = CreateBoolExp(op, renderTo);
            }

            fTypeDecl = DeclType.Bool;
        }

        /// <summary>builds the left and right side of the bool.</summary>
        /// <param name="renderTo"></param>
        /// <returns>returns <see langword="true"/> if the <see langword="bool"/>
        ///     expression still has to be created. otherwise <see langword="false"/>
        ///     (error or already created.</returns>
        private bool BuildBoolParts(NNLModuleCompiler renderTo)
        {
            var iRes = true;

            // first need to render the right part for regular bool expressions (== != contains !contains > < >= <=) cause the return value is stack based, so last in is first out, which has to be the left side.
            if (fRightPart != null)
            {
                var iFoundBinding = false;
                var iLeft = LeftPart as NNLUnaryStatement;
                if (iLeft != null && iLeft.Child is NNLBindingPathNode)
                {
                    var iPath = (NNLBindingPathNode)iLeft.Child;
                    var iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
                    if (iBind != null)
                    {
                        Item = iPath.RenderGetAndOperatorOverload(renderTo, iBind, RightPart, Operator, DeclType.Bool);
                        iLeft.Item = iPath.Item;

                            // we used a special render so the unary statement doesn't yet know about the item, do this here so the rest of the algorithm has the correct value.
                        iFoundBinding = true;
                    }

                    if (Item != null)
                    {
                        return false;
                    }
                }

                if (iFoundBinding == false)
                {
                    fRightPart.Render(renderTo);
                    if (fRightPart.Item == null)
                    {
                        Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                            // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                        LogPosError("Invalid right side of bool expression", renderTo);
                        iRes = false;
                    }
                    else
                    {
                        if (fLeftPart == null)
                        {
                            Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                                // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                            LogPosError("No left part defined in boolean", renderTo);
                            iRes = false;
                        }
                        else
                        {
                            fLeftPart.Render(renderTo);
                            if (fLeftPart.Item == null)
                            {
                                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                                    // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                                iRes = false;
                                LogPosError("Invalid left side of bool expression", renderTo);
                            }
                        }
                    }
                }
            }
            else
            {
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                LogPosError("Invalid right side of bool expression", renderTo);
                iRes = false;
            }

            return iRes;
        }

        /// <summary>builds the left and right side of the bool.</summary>
        /// <param name="renderTo"></param>
        /// <returns>returns <see langword="true"/> if the <see langword="bool"/>
        ///     expression still has to be created. otherwise <see langword="false"/>
        ///     (error or already created.</returns>
        private bool BuildBoolPartsAndOr(NNLModuleCompiler renderTo)
        {
            var iRes = true;
            if (fLeftPart == null)
            {
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                LogPosError("No left part defined in boolean", renderTo);
                iRes = false;
            }
            else
            {
                fLeftPart.Render(renderTo);
                if (fLeftPart.Item == null)
                {
                    iRes = false;
                    LogPosError("Invalid left side of bool expression", renderTo);
                }
                else if (fRightPart != null)
                {
                    var iRightContent = new System.Collections.Generic.List<Neuron>();
                    renderTo.RenderingTo.Push(iRightContent);
                    try
                    {
                        fRightPart.Render(renderTo);
                    }
                    finally
                    {
                        renderTo.RenderingTo.Pop();
                    }

                    if (fRightPart.Item == null)
                    {
                        Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                            // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                        LogPosError("Invalid right side of bool expression", renderTo);
                        iRes = false;
                    }
                    else if ((fLeftPart.ExtraItems != null && fLeftPart.ExtraItems.Count > 0) || iRightContent.Count > 0)
                    {
                        // if the left part rendered extra code (it contained a path), we need to do an if now so that we make certain that
                        iRes = false;
                        if (Operator == Token.DoubleAnd)
                        {
                            BuildIfAnd(renderTo, iRightContent);
                        }
                        else if (Operator == Token.Or)
                        {
                            BuildIfOr(renderTo, iRightContent);
                        }
                        else
                        {
                            iRes = true;
                        }
                    }
                }
            }

            return iRes;
        }

        /// <summary>Builds an if statement for the 'and' <see langword="operator"/> (cause
        ///     there were function calls that caused items to be rendered before the<see langword="bool"/> exp). This extra if statement is required to
        ///     make certain taht we don't do calculate the right part if not
        ///     required.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="iRightContent">The i Right Content.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void BuildIfAnd(NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> iRightContent)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek();
            iRightContent.Add(BuildAssignment(renderTo, GetBoolResVar(renderTo), fRightPart.Item));
            var iRightExtraCode = GetParentsFor(iRightContent, (ulong)PredefinedNeurons.Code, renderTo, null);

            var iCondExp = GetConditionalExp(renderTo, iRightExtraCode, fLeftPart.Item);
            var iElse = GetConditionalExp(renderTo, GetBoolResCluster(renderTo, Brain.Current.FalseNeuron), null);
            var iParts = GetParentsFor(
                new System.Collections.Generic.List<Neuron> { iCondExp, iElse }, 
                (ulong)PredefinedNeurons.Code, 
                renderTo, 
                null); // we need to create another cluster that contains the parts for the if statement.
            iRenderingTo.Add(GetIfStatement(renderTo, iParts));
            Item = GetBoolResVar(renderTo);
        }

        /// <summary>finds or builds the bool-result-variable.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="Variable"/>.</returns>
        private Variable GetBoolResVar(NNLModuleCompiler renderTo)
        {
            var iId = Brain.Current.Modules.Varmanager.BoolResVar;

                // first get the boolResVar variable that is used to return the value of the calculation to the boolres.
            if (iId > Neuron.EmptyId)
            {
                var iRes = Brain.Current[Brain.Current.Modules.Varmanager.BoolResVar] as Variable;
                renderTo.AddExternal(iRes); // we need to know 
                return iRes;
            }

            var iVar = NeuronFactory.Get<Variable>();
            Brain.Current.Add(iVar);
            Link.Create(iVar, iVar, (ulong)PredefinedNeurons.True); // let it know that it handles bool values.
            renderTo.Add(iVar);
            Brain.Current.Modules.Varmanager.BoolResVar = iVar.ID;
            NNLModuleCompiler.NetworkDict.SetName(iVar, "BoolResult");
            return iVar;
        }

        /// <summary>looks for or creates a cluster that contains a single assignment:
        ///     BoolRes = false; where boolres is a regular variable that is re-used
        ///     for each boolean expression that needs to be split up into an 'if'
        ///     statement.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="rightPart">The right Part.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster GetBoolResCluster(NNLModuleCompiler renderTo, Neuron rightPart)
        {
            var iId = Brain.Current.Modules.Varmanager.BoolResVar;

                // first get the boolResVar variable that is used to return the value of the calculation to the boolres.
            var iVar = GetBoolResVar(renderTo);
            Assignment iAss = null;
            foreach (var i in iVar.FindAllIn((ulong)PredefinedNeurons.LeftPart))
            {
                iAss = i as Assignment;
                if (iAss != null && iAss.RightPart == rightPart)
                {
                    break;
                }

                iAss = null; // always reset, otherwise it keeps a ref to the last itme, which we don't want.
            }

            if (iAss == null)
            {
                iAss = BuildAssignment(renderTo, iVar, rightPart);
            }
            else
            {
                renderTo.AddExternal(iAss); // we need to know 
            }

            return GetParentsFor(
                new System.Collections.Generic.List<Neuron> { iAss }, 
                (ulong)PredefinedNeurons.Code, 
                renderTo, 
                null); // get the
        }

        /// <summary>builds an assignment with the specified parts.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="left">the left side of the assignemnt</param>
        /// <param name="right">the right side of the assignemnt</param>
        /// <returns>The <see cref="Assignment"/>.</returns>
        private Assignment BuildAssignment(NNLModuleCompiler renderTo, Variable left, Neuron right)
        {
            var iRes = NeuronFactory.Get<Assignment>();
            Brain.Current.Add(iRes);
            iRes.LeftPart = left;
            iRes.RightPart = right;
            renderTo.Add(iRes);
            return iRes;
        }

        /// <summary>searches for an already existing conditional expression or creates one
        ///     for the specified code and conditional.</summary>
        /// <param name="renderTo"></param>
        /// <param name="codeList"></param>
        /// <param name="condition"></param>
        /// <returns>The <see cref="ConditionalExpression"/>.</returns>
        private ConditionalExpression GetConditionalExp(
            NNLModuleCompiler renderTo, 
            NeuronCluster codeList, 
            Neuron condition)
        {
            ConditionalExpression iRes = null;
            foreach (var i in codeList.FindAllIn((ulong)PredefinedNeurons.Statements))
            {
                var iTemp = i as ConditionalExpression;
                if (iTemp != null && iTemp.Condition == condition)
                {
                    iRes = iTemp;
                    break;
                }
            }

            if (iRes == null)
            {
                // if we didn't find an existing one, need to create one.
                iRes = NeuronFactory.Get<ConditionalExpression>();
                Brain.Current.Add(iRes);
                iRes.StatementsCluster = codeList;
                if (condition != null)
                {
                    iRes.Condition = condition;
                }

                renderTo.Add(iRes);
            }

            return iRes;
        }

        /// <summary>The get if statement.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iParts">The i parts.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetIfStatement(NNLModuleCompiler renderTo, NeuronCluster iParts)
        {
            ConditionalStatement iRes = null;
            foreach (var i in iParts.FindAllIn((ulong)PredefinedNeurons.Condition))
            {
                var iTemp = i as ConditionalStatement;
                if (iTemp != null && iTemp.LoopStyle.ID == (ulong)PredefinedNeurons.Normal)
                {
                    var iOtherExtraItems = iTemp.FindFirstOut((ulong)PredefinedNeurons.Statements);
                    var iOtherCaseItem = iTemp.FindFirstOut((ulong)PredefinedNeurons.CaseItem);
                    if (iOtherExtraItems == null && iOtherCaseItem == null)
                    {
                        iRes = iTemp;
                        break;
                    }
                }
            }

            if (iRes == null)
            {
                iRes = NeuronFactory.Get<ConditionalStatement>();
                Brain.Current.Add(iRes);
                Link.Create(iRes, (ulong)PredefinedNeurons.Normal, (ulong)PredefinedNeurons.LoopStyle);
                Link.Create(iRes, iParts, (ulong)PredefinedNeurons.Condition);
                renderTo.Add(iRes);
            }
            else
            {
                renderTo.AddExternal(iRes); // we need to know 
            }

            return iRes;
        }

        /// <summary>The build if or.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iRightContent">The i right content.</param>
        private void BuildIfOr(NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> iRightContent)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek();
            iRightContent.Add(BuildAssignment(renderTo, GetBoolResVar(renderTo), fRightPart.Item));
            var iRightExtraCode = GetParentsFor(iRightContent, (ulong)PredefinedNeurons.Code, renderTo, null);

            var iCondExp = GetConditionalExp(
                renderTo, 
                GetBoolResCluster(renderTo, Brain.Current.TrueNeuron), 
                fLeftPart.Item);
            var iElse = GetConditionalExp(renderTo, iRightExtraCode, null);
            var iParts = GetParentsFor(
                new System.Collections.Generic.List<Neuron> { iCondExp, iElse }, 
                (ulong)PredefinedNeurons.Code, 
                renderTo, 
                null); // we need to create another cluster that contains the parts for the if statement.
            iRenderingTo.Add(GetIfStatement(renderTo, iParts));
            Item = GetBoolResVar(renderTo);
        }

        /// <summary>builds a boolean expression for the And/Or operators. If, the right or
        ///     left operand contains a function call, the <see langword="bool"/>
        ///     expression is converted into another function call that performs the
        ///     calculation and returns <see langword="true"/> or false. This is done
        ///     cause function calls in a boolean expression need to be called before
        ///     evaluating the boolean. Depending on wether the<see langword="operator"/> is AND or OR, any functions in the right
        ///     part either need to be called or not.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo"></param>
        private void BuildAndOrBoolean(ulong op, NNLModuleCompiler renderTo)
        {
            if (ContainsFunctionsCall(renderTo))
            {
                System.Collections.Generic.List<Neuron> iRenderingTo;

                    // need to make certain that there is only 1 function call rendered for all the booleans with the same root-boolean.
                var iIsRoot = GetRenderToList(renderTo, out iRenderingTo);
                try
                {
                    if (fLeftPart == null)
                    {
                        Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                            // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                        LogPosError("No left part defined in boolean", renderTo);
                    }
                    else
                    {
                        fLeftPart.Render(renderTo);
                        if (fLeftPart.Item == null)
                        {
                            LogPosError("Invalid left side of bool expression", renderTo);
                        }
                        else if (fRightPart != null)
                        {
                            RenderJumpIf(op, fLeftPart.Item, renderTo, iRenderingTo);
                            fRightPart.Render(renderTo);
                            if (fRightPart.Item == null)
                            {
                                LogPosError("Invalid right side of bool expression", renderTo);
                            }
                            else if (iIsRoot)
                            {
                                RenderLastReturns(op, renderTo, iRenderingTo);
                            }
                            else
                            {
                                Item = fRightPart.Item; // we always use the return value as bool condition.
                            }
                        }
                    }

                    fTypeDecl = DeclType.Bool;
                }
                finally
                {
                    renderTo.RenderingTo.Pop(); // alsways need to remove this list, cause if we didn't create
                    if (iIsRoot)
                    {
                        ExtraItems = iRenderingTo;

                            // need to assign this manually, cause we used a customized list for this.
                        if (renderTo.IsRenderingCondPart == false)
                        {
                            // need to copy over the extra items to the main list.
                            iRenderingTo = renderTo.RenderingTo.Peek();
                            iRenderingTo.AddRange(ExtraItems);
                        }
                    }
                }
            }
            else
            {
                BuildBoolean(op, renderTo);
            }
        }

        /// <summary>Checks if left or right is a function call or expression block call.
        ///     These need to be rendered differently.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ContainsFunctionsCall(NNLModuleCompiler renderTo)
        {
            var iPath = LeftPart as NNLPathNode;
            if (iPath != null && iPath.IsFunctionCall(renderTo))
            {
                return true;
            }

            var iBool = LeftPart as NNLBinaryStatement;
            if (iBool != null)
            {
                if (iBool.ContainsFunctionsCall(renderTo))
                {
                    return true;
                }
            }

            iPath = RightPart as NNLPathNode;
            if (iPath != null)
            {
                return iPath.IsFunctionCall(renderTo);
            }

            return false;
        }

        /// <summary>for or statements that aren't in the root (top) boolean, the location
        ///     to jump-to, still needs to be determined, which can be done here.</summary>
        /// <param name="renderTo"></param>
        /// <param name="addTo">The add To.</param>
        /// <param name="offset">The offset.</param>
        private void CloseJumpPos(
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> addTo, 
            int offset = 0)
        {
            IntNeuron iToClose;
            var iPos = addTo.Count + offset - 1;
            while (renderTo.JumpPoints.Count > 0)
            {
                iToClose = renderTo.JumpPoints.Pop();
                iToClose.Value = iPos;
            }
        }

        /// <summary>renders the last returns. If the <paramref name="op"/> was 'or', it
        ///     needs to render 2 returns: the first for the right part, the second
        ///     for the first part (which is reachable through a jump). the jump point
        ///     should be adjusted accordingly. Warning: the actual return value
        ///     depends on how the <see langword="bool"/> is rendered: if it is part
        ///     of a conditional with multiple parts, an <see langword="int"/> needs
        ///     to be returned, otherwise a bool.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo"></param>
        /// <param name="addTo">The add To.</param>
        private void RenderLastReturns(
            ulong op, 
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> addTo)
        {
            NNLModuleCompiler.ConditionalRenderData iCondData = null;
            if (renderTo.IsRenderingCondPart)
            {
                iCondData = renderTo.CondPartIndex.Peek();
            }

            if (iCondData != null && iCondData.TotalNrOfCondParts > 1)
            {
                // need to render returns with index pos if we are rendering for a conditional part. If it is for an assignment or somehting else, use the bool value.
                var iReturnValue = iCondData.GetReturnValue(renderTo);
                var iArgs = new System.Collections.Generic.List<Neuron> { fRightPart.Item, iReturnValue };
                var iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                addTo.Add(GetStatement((ulong)PredefinedNeurons.ReturnValueIfInstruction, iArgCluster, renderTo));
                if (op == (ulong)PredefinedNeurons.Or || op == (ulong)PredefinedNeurons.And)
                {
                    iArgs = new System.Collections.Generic.List<Neuron> { iReturnValue };
                    iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                    addTo.Add(GetStatement((ulong)PredefinedNeurons.ReturnValueInstruction, iArgCluster, renderTo));
                }

                Item = iReturnValue; // the value that will be used for the case.
            }
            else
            {
                NeuronCluster iArgCluster;
                System.Collections.Generic.List<Neuron> iArgs;
                iArgs = new System.Collections.Generic.List<Neuron> { fRightPart.Item };
                iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                addTo.Add(GetStatement((ulong)PredefinedNeurons.ReturnValueInstruction, iArgCluster, renderTo));

                if (op == (ulong)PredefinedNeurons.Or)
                {
                    iArgs = new System.Collections.Generic.List<Neuron> { Brain.Current.TrueNeuron };
                }
                else if (op == (ulong)PredefinedNeurons.And)
                {
                    iArgs = new System.Collections.Generic.List<Neuron> { Brain.Current.FalseNeuron };

                        // if it's an '&&' operator, we jumped if the leftpart failed.
                }
                else
                {
                    throw new System.InvalidOperationException();
                }

                iArgCluster = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                addTo.Add(GetStatement((ulong)PredefinedNeurons.ReturnValueInstruction, iArgCluster, renderTo));
                Item = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];

                    // we always use the return value as a result for the bool. a conditional can overwrite this with an int, if it was converted to a case.
            }

            CloseJumpPos(renderTo, addTo);
        }

        /// <summary>renders a jumpif instruction so that the current frame can be exited.</summary>
        /// <param name="op">The op.</param>
        /// <param name="cond">the condition</param>
        /// <param name="renderTo">The render To.</param>
        /// <param name="addTo">The add To.</param>
        private void RenderJumpIf(
            ulong op, 
            Neuron cond, 
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> addTo)
        {
            NeuronCluster iArgCl;
            var iArgs = new System.Collections.Generic.List<Neuron>();
            iArgs.Add(cond);
            if (op == (ulong)PredefinedNeurons.And)
            {
                // for &&, we need to get the inverse of the condition cause we don't need to check the next part if the left part failed.
                iArgCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                cond = GetResultStatement((ulong)PredefinedNeurons.NotInstruction, iArgCl, renderTo);
                iArgs.Clear();
                iArgs.Add(cond);
            }

            var iJumpTo = NeuronFactory.GetInt();
            Brain.Current.Add(iJumpTo);
            iArgs.Add(iJumpTo);
            renderTo.JumpPoints.Push(iJumpTo);

            iArgCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
            var iToAdd = GetStatement((ulong)PredefinedNeurons.JmpIfInstruction, iArgCl, renderTo);
            addTo.Add(iToAdd);
        }

        /// <summary>The get render to list.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="result">The result.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetRenderToList(NNLModuleCompiler renderTo, out System.Collections.Generic.List<Neuron> result)
        {
            result = null;
            foreach (var i in renderTo.RenderingTo)
            {
                // find the first list on the stack that is specific for an ANDOR boolean, if there is one, re-use that to render all the extra code in.
                result = i as BuildAndOrList;
                if (result != null)
                {
                    break;
                }
            }

            if (result == null)
            {
                // we only need 1 list, not lots of them.
                result = new BuildAndOrList();
                renderTo.RenderingTo.Push(result);
                return true;
            }

            renderTo.RenderingTo.Push(result);

                // make certain that the same list is used again for our children, cause all the code for the boolean needs to go in a single callback.
            return false;
        }

        /// <summary>The create bool exp.</summary>
        /// <param name="op">The op.</param>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron CreateBoolExp(ulong op, NNLModuleCompiler renderTo)
        {
            using (var iLinks = fLeftPart.Item.LinksIn)
            {
                foreach (var i in iLinks)
                {
                    if (i.MeaningID == (ulong)PredefinedNeurons.LeftPart)
                    {
                        var iBool = i.From as BoolExpression;
                        if (iBool != null && iBool.RightPart == fRightPart.Item)
                        {
                            var iOp = iBool.Operator;
                            if (iOp != null && iOp.ID == op)
                            {
                                renderTo.Add(iBool);
                                return iBool;
                            }
                        }
                    }
                }
            }

            var iBoolExp = NeuronFactory.Get<BoolExpression>(); // not found, create a new one.
            Brain.Current.Add(iBoolExp);
            Link.Create(iBoolExp, op, (ulong)PredefinedNeurons.Operator);
            iBoolExp.LeftPart = fLeftPart.Item;
            iBoolExp.RightPart = fRightPart.Item;
            renderTo.Add(iBoolExp);
            return iBoolExp;
        }

        /// <summary>The try build bind assign.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryBuildBindAssign(NNLModuleCompiler renderTo)
        {
            var iLeft = LeftPart as NNLUnaryStatement;
            if (iLeft != null && iLeft.Child is NNLBindingPathNode)
            {
                var iPath = (NNLBindingPathNode)iLeft.Child;
                var iBind = FirstNodeParent.GetBindingFor(iLeft.Operator, renderTo);
                if (iBind != null)
                {
                    iPath.RenderSet(renderTo, iBind, RightPart, Operator);
                    Item = iPath.Item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Builds the item as an assignement.</summary>
        /// <param name="renderTo">The render To.</param>
        private void BuildAssign(NNLModuleCompiler renderTo)
        {
            if (fLeftPart == null)
            {
                LogPosError("No left part defined in assignment", renderTo);
            }

            // if (fRightPart is NNLBinaryStatement && ((NNLBinaryStatement)fRightPart).Operator == Token.Add)
            // BuildCalc((ulong)PredefinedNeurons.StoreIntInstruction, renderTo);
            else if (LeftPart is NNLUnaryStatement && ((NNLUnaryStatement)LeftPart).Child is NNLBindingPathNode)
            {
                TryBuildBindAssign(renderTo);
            }
            else
            {
                fLeftPart.Render(renderTo);
                fRightPart.Render(renderTo);
                if (fRightPart.Item is Assignment)
                {
                    LogPosError(
                        "Invalid right side of assignement (multiple assignements in the same statement not supported)", 
                        renderTo);
                }

                if (fLeftPart.Item == null || fLeftPart.Item is Statement)
                {
                    // statements are also illegal on the left side: there is nothing to assign to.
                    LogPosError("Invalid left side of assignement", renderTo);
                }
                else
                {
                    if (fRightPart.Item == null)
                    {
                        LogPosError("Invalid right side of assignement", renderTo);
                    }
                    else if (fRightPart.GetTypeDecl(renderTo) == DeclType.none)
                    {
                        LogPosError("Right side doesn't return a value", renderTo);
                    }
                    else
                    {
                        BuildAssign(fLeftPart.Item, fRightPart.Item, renderTo);
                    }
                }
            }
        }

        /// <summary>The build assign.</summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="renderTo">The render to.</param>
        private void BuildAssign(Neuron left, Neuron right, NNLModuleCompiler renderTo)
        {
            Assignment iRes = null;
            using (var iLinks = left.LinksIn)
            {
                foreach (var i in iLinks)
                {
                    if (i.MeaningID == (ulong)PredefinedNeurons.LeftPart)
                    {
                        var iAssign = i.From as Assignment;
                        if (iAssign != null && iAssign.RightPart == right)
                        {
                            iRes = iAssign;
                            break;
                        }
                    }
                }
            }

            if (iRes == null)
            {
                iRes = NeuronFactory.Get<Assignment>();
                Brain.Current.Add(iRes);
                renderTo.Add(iRes);
                iRes.LeftPart = left;
                iRes.RightPart = right;
            }

            Item = iRes;
            renderTo.Add(this);
            fTypeDecl = DeclType.none;
        }

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            switch (Operator)
            {
                case Token.Assign:
                    if (TryGetTypeDeckAssignInt(renderTo) == false && TryGetTypeDeclAssignDouble(renderTo) == false)
                    {
                        fTypeDecl = DeclType.none;
                    }

                    break;
                case Token.DoubleAnd:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.Or:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.NotAssign:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.NotContains:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.Contains:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.AddAssign:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.AddInstruction);

                            // when you add some items to a var, you add them to the end of the list.
                    }

                    break;
                case Token.AddNotAssign:
                    GetTypeDeclBindingOp((ulong)PredefinedNeurons.AssignAddNot, renderTo);
                    break;
                case Token.MinusAssign:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.SubstractInstruction);

                            // when you add some items to a var, you add them to the end of the list.
                    }

                    break;
                case Token.AndAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.OrAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.ListAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.MultiplyAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.DivAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.ModulusAssign:
                    fTypeDecl = DeclType.none;
                    break;
                case Token.IsEqual:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.IsBiggerThen:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.IsBiggerThenOrEqual:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.IsSmallerThen:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.IsSmallerThenOrEqual:
                    fTypeDecl = DeclType.Bool;
                    break;
                case Token.OptionStart:
                    BuildCalc((ulong)PredefinedNeurons.GetAtInstruction, renderTo);
                    break;
                case Token.Minus:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.MinusInstruction);
                    }

                    break;
                case Token.Add:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.AdditionInstruction);
                    }

                    break;
                case Token.Multiply:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.MultiplyInstruction);
                    }

                    break;
                case Token.Divide:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.DivideInstruction);
                    }

                    break;
                case Token.Modulus:
                    if (TryGetTypeDeclOpInt(renderTo) == false && TryGetTypeDeclOpDouble(renderTo) == false)
                    {
                        fTypeDecl = GetTypDeclFromInstruction((ulong)PredefinedNeurons.ModulusInstruction);
                    }

                    break;
                default:
                    LogPosError("Internal error: unknown binary operator: " + Operator, renderTo);
                    break;
            }

            return fTypeDecl;
        }

        /// <summary>
        ///     a specialized list of neurons that allows us to check if the current
        ///     list that we are rendering to, is for building a
        ///     <see langword="bool" /> expression or not.
        /// </summary>
        private class BuildAndOrList : System.Collections.Generic.List<Neuron>
        {
        };
    }
}