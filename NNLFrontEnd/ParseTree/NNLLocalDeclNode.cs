// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLLocalDeclNode.cs" company="">
//   
// </copyright>
// <summary>
//   the type of the parameter or local var.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     the type of the parameter or local var.
    /// </summary>
    public enum DeclType
    {
        /// <summary>The var.</summary>
        Var, // any type of neuron

        /// <summary>The int.</summary>
        Int, // single int, for fast calculations         

        /// <summary>The int ar.</summary>
        IntAr, // int array, 

        /// <summary>The double.</summary>
        Double, // single double for fast calculations

        /// <summary>The double ar.</summary>
        DoubleAr, // double array, 

        /// <summary>The string ar.</summary>
        StringAr, // array of strings

        /// <summary>The string.</summary>
        String, // single string

        /// <summary>The bool.</summary>
        Bool, 

        /// <summary>The none.</summary>
        none // no type allowed
    }

    /// <summary>
    ///     specifies the scope of the variable: local to a function, local to a
    ///     link-solve, global (for a single processor).
    /// </summary>
    public enum VarScope
    {
        /// <summary>The local.</summary>
        Local, 

        /// <summary>The var.</summary>
        Var, 

        /// <summary>The global.</summary>
        Global
    }

    /// <summary>
    ///     declares a variable.
    /// </summary>
    internal class NNLLocalDeclNode : NNLStatementNode
    {
        /// <summary>The f init value.</summary>
        private NNLStatementNode fInitValue;

        /// <summary>The f scope.</summary>
        private VarScope fScope = VarScope.Local;

        /// <summary>The f sub decls.</summary>
        private System.Collections.Generic.List<NNLLocalDeclNode> fSubDecls;

        /// <summary>Initializes a new instance of the <see cref="NNLLocalDeclNode"/> class.</summary>
        public NNLLocalDeclNode()
            : base(NodeType.Var)
        {
            IsParam = false;
        }

        #region TypeDecl

        /// <summary>
        ///     Gets/sets the type of the local.
        /// </summary>
        public DeclType TypeDecl { get; set; }

        #endregion

        #region Scope

        /// <summary>
        ///     Gets/sets the scope of the variable.
        /// </summary>
        public VarScope Scope
        {
            get
            {
                return fScope;
            }

            set
            {
                fScope = value;
            }
        }

        #endregion

        #region InitValue

        /// <summary>
        ///     The init value to assign to the local
        /// </summary>
        public NNLStatementNode InitValue
        {
            get
            {
                return fInitValue;
            }

            set
            {
                fInitValue = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        #region SplitReaction

        /// <summary>
        ///     the id of the neuron (if any) that determins how to behave during a
        ///     split.
        /// </summary>
        public ulong SplitReaction { get; set; }

        #endregion

        #region SubDecls

        /// <summary>
        ///     Gets the list of declarations that were also declared on the same line
        ///     and should be rendered with 1 prepare statement (if it are locals).
        /// </summary>
        public System.Collections.Generic.List<NNLLocalDeclNode> SubDecls
        {
            get
            {
                if (fSubDecls == null)
                {
                    fSubDecls = new System.Collections.Generic.List<NNLLocalDeclNode>();
                }

                return fSubDecls;
            }
        }

        #endregion

        #region IsParam

        /// <summary>
        ///     Gets/sets the value that indicates if this is a parameter or regular
        ///     variable (rendered differently)
        /// </summary>
        public bool IsParam { get; set; }

        #endregion

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            return TypeDecl;
        }

        /// <summary>renders the decl with the option of surpressing the possible 'prepare'
        ///     statement for locals. This is done when rendering sub-declerations, so
        ///     that the root decleration can make certain that the list is rendered
        ///     with 1 prepare statement.</summary>
        /// <param name="renderTo"></param>
        /// <param name="renderPrepare"></param>
        internal void RenderDecl(NNLModuleCompiler renderTo, bool renderPrepare)
        {
            if (Item == null)
            {
                if (ID == 0)
                {
                    Item = FindExisting(renderTo);
                    if (Item == null)
                    {
                        Variable iNew;
                        switch (Scope)
                        {
                            case VarScope.Local:
                                iNew = NeuronFactory.Get<Local>();
                                break;
                            case VarScope.Var:
                                iNew = NeuronFactory.Get<Variable>();
                                break;
                            case VarScope.Global:
                                iNew = NeuronFactory.Get<Global>();
                                break;
                            default:
                                throw new System.InvalidOperationException();
                        }

                        Brain.Current.Add(iNew);
                        Item = iNew;
                        renderTo.RegisterVar(this);
                        InitVar(renderTo);
                    }
                    else
                    {
                        renderTo.Add(this);
                    }
                }
                else
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(ID, out iFound))
                    {
                        if (!(iFound is Variable))
                        {
                            LogPosError(
                                "variable decleration references an external neruron that is not a variable.", 
                                renderTo);
                        }

                        Item = iFound;
                        InitVar(renderTo);
                        renderTo.AddExternal(iFound);
                    }
                    else
                    {
                        LogPosError("Invalid ID: " + ID, renderTo);
                    }
                }

                if (renderPrepare && Scope == VarScope.Local && IsParam == false)
                {
                    // if this is a variable decleration inside of a function, make certain that we generate code for preparing the local.
                    RenderPrepareLocal(renderTo);
                }
            }
            else if (ID == 0)
            {
                renderTo.RegisterVar(this);

                    // the var has already been rendered, but perhaps not yet registered in the current module.
            }
            else
            {
                renderTo.AddExternal(Item); // always make certain that the items are registered as being used.
            }
        }

        /// <summary>The init var.</summary>
        /// <param name="renderTo">The render to.</param>
        private void InitVar(NNLModuleCompiler renderTo)
        {
            RegisterType();
            if (InitValue != null)
            {
                AssignInitValue(renderTo, (Variable)Item);
            }

            if (SplitReaction != 0)
            {
                Link.Create(Item, SplitReaction, (ulong)PredefinedNeurons.SplitReaction);
            }

            if (string.IsNullOrEmpty(Name) == false)
            {
                NNLModuleCompiler.NetworkDict.SetName(Item, Name);
            }
        }

        /// <summary>
        ///     makes certain that the engine can determin the type of the var, when
        ///     applicable.
        /// </summary>
        private void RegisterType()
        {
            switch (TypeDecl)
            {
                case DeclType.Int:
                    if (Link.Exists(Item, Item, (ulong)PredefinedNeurons.IntNeuron) == false)
                    {
                        Link.Create(Item, Item, (ulong)PredefinedNeurons.IntNeuron);
                    }

                    break;
                case DeclType.Double:
                    if (Link.Exists(Item, Item, (ulong)PredefinedNeurons.DoubleNeuron) == false)
                    {
                        Link.Create(Item, Item, (ulong)PredefinedNeurons.DoubleNeuron);
                    }

                    break;
                case DeclType.Bool:
                    if (Link.Exists(Item, Item, (ulong)PredefinedNeurons.True) == false)
                    {
                        Link.Create(Item, Item, (ulong)PredefinedNeurons.True);
                    }

                    break;
            }
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            RenderDecl(renderTo, true);
        }

        /// <summary>renders a prepare-local statement. Does some optimisations by trying
        ///     to group these calls together. If the previously generated statement
        ///     was also a prepareLocal, combine the 2.</summary>
        /// <param name="renderTo"></param>
        private void RenderPrepareLocal(NNLModuleCompiler renderTo)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek();
            var iPrev = TryGetPrevPrepareLocal(renderTo, iRenderingTo);
            if (iPrev != null)
            {
                CombinePrePareLocal(renderTo, iPrev, iRenderingTo);
            }
            else
            {
                var iArgs = new System.Collections.Generic.List<Neuron>();
                var iArgsCl = PrepareArgs(renderTo, iArgs);
                iRenderingTo.Add(GetStatement((ulong)PredefinedNeurons.PrepareLocalInstruction, iArgsCl, renderTo));
            }
        }

        /// <summary>The prepare args.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iArgs">The i args.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster PrepareArgs(NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> iArgs)
        {
            var iByRef = GetByRef(Item, renderTo);
            iArgs.Add(iByRef);
            if (fSubDecls != null)
            {
                foreach (var i in fSubDecls)
                {
                    i.RenderDecl(renderTo, false);
                    iByRef = GetByRef(i.Item, renderTo);
                    iArgs.Add(iByRef);
                }
            }

            return GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
        }

        /// <summary>The combine pre pare local.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="prev">The prev.</param>
        /// <param name="renderingTo">The rendering to.</param>
        private void CombinePrePareLocal(
            NNLModuleCompiler renderTo, 
            Statement prev, System.Collections.Generic.List<Neuron> renderingTo)
        {
            var iCluster = prev.ArgumentsCluster;
            System.Collections.Generic.List<Neuron> iArgs;
            using (var iChildren = iCluster.Children) iArgs = iChildren.ConvertTo<Neuron>();

            if (BrainHelper.HasIncommingReferences(prev) == false)
            {
                // if the previous statement is not yet used 
                renderTo.Remove(prev); // need to remove it frst, otherwise can't delete it.
                Brain.Current.Delete(prev);
                if (BrainHelper.HasIncommingReferences(iCluster) == false)
                {
                    renderTo.Remove(iCluster); // need to remove it frst, otherwise can't delete it.
                    Brain.Current.Delete(iCluster);
                }
            }

            var iArgsCl = PrepareArgs(renderTo, iArgs);
            renderingTo.Add(GetStatement((ulong)PredefinedNeurons.PrepareLocalInstruction, iArgsCl, renderTo));
            Factories.Default.NLists.Recycle(iArgs);
        }

        /// <summary>The try get prev prepare local.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iRenderingTo">The i rendering to.</param>
        /// <returns>The <see cref="Statement"/>.</returns>
        private Statement TryGetPrevPrepareLocal(
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> iRenderingTo)
        {
            if (iRenderingTo != null && iRenderingTo.Count > 0)
            {
                var iPrev = iRenderingTo[iRenderingTo.Count - 1] as Statement;
                if (iPrev != null && iPrev.Instruction != null
                    && iPrev.Instruction.ID == (ulong)PredefinedNeurons.PrepareLocalInstruction)
                {
                    return iPrev;
                }
            }

            return null;
        }

        /// <summary>The assign init value.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iNew">The i new.</param>
        private void AssignInitValue(NNLModuleCompiler renderTo, Variable iNew)
        {
            InitValue.Render(renderTo);
            if (InitValue.Item is IntNeuron)
            {
                // need to create
                var iArgs = new System.Collections.Generic.List<Neuron>();
                iArgs.Add(Brain.Current[(ulong)PredefinedNeurons.IntNeuron]);
                iArgs.Add(InitValue.Item);
                var iArgsCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                iNew.Value = GetResultStatement((ulong)PredefinedNeurons.NewInstruction, iArgsCl, renderTo);
            }
            else if (InitValue.Item is DoubleNeuron)
            {
                var iArgs = new System.Collections.Generic.List<Neuron>();
                iArgs.Add(Brain.Current[(ulong)PredefinedNeurons.DoubleNeuron]);
                iArgs.Add(InitValue.Item);
                var iArgsCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                iNew.Value = GetResultStatement((ulong)PredefinedNeurons.NewInstruction, iArgsCl, renderTo);
            }
            else
            {
                iNew.Value = InitValue.Item;
            }
        }

        /// <summary>The find existing.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="Variable"/>.</returns>
        private Variable FindExisting(NNLModuleCompiler renderTo)
        {
            if (Scope == VarScope.Local)
            {
                var iFound = Brain.Current.Modules.Varmanager.FindLocal(Name);
                if (iFound != null)
                {
                    Neuron iInitVal;
                    if (InitValue != null)
                    {
                        InitValue.Render(renderTo);
                        iInitVal = InitValue.Item;
                    }
                    else
                    {
                        iInitVal = null;
                    }

                    System.Type iVarType = null;
                    switch (TypeDecl)
                    {
                        case DeclType.Int:
                            iVarType = typeof(int);
                            break;
                        case DeclType.Double:
                            iVarType = typeof(double);
                            break;
                        case DeclType.Bool:
                            iVarType = typeof(bool);
                            break;
                    }

                    return Brain.Current.Modules.Varmanager.FindLocal(Name, iInitVal, iVarType, SplitReaction);
                }

                return null;
            }

            return null;
        }

        /// <summary>writes the content of the delceration to a <paramref name="stream"/>
        ///     so it can be reloaded without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write((System.Int16)TypeDecl);
            stream.Write((System.Int16)Scope);
        }

        /// <summary>provides a way to load a local decl from a pre-compiled stream.</summary>
        /// <param name="reader"></param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            TypeDecl = (DeclType)reader.ReadInt16();
            Scope = (VarScope)reader.ReadInt16();
        }
    }
}