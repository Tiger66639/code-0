// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBinding.cs" company="">
//   
// </copyright>
// <summary>
//   contains binding info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     contains binding info.
    /// </summary>
    internal class NNLBinding : NNLNode
    {
        /// <summary>The f all bind items.</summary>
        private System.Collections.Generic.Dictionary<string, NNLBindItemBase> fAllBindItems;

        /// <summary>The f operator overloads.</summary>
        private System.Collections.Generic.Dictionary<string, NNLFunctionNode> fOperatorOverloads;

        /// <summary>The f root.</summary>
        private NNLBindItem fRoot;

        /// <summary>The f binding items.</summary>
        private readonly System.Collections.Generic.Dictionary<Token, NNLBindItemBase> fBindingItems =
            new System.Collections.Generic.Dictionary<Token, NNLBindItemBase>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NNLBinding" /> class.
        /// </summary>
        public NNLBinding()
            : base(NodeType.Bind)
        {
            Register = false;
            UseStatics = false;
        }

        /// <summary>
        ///     gets/sets if this bindign needs to be registered with the compiler or
        ///     not. When true, the binding will be saved in the module so that it can
        ///     be used by other part of the system without having to continuasly
        ///     recompile the entire code base.
        /// </summary>
        public bool Register { get; set; }

        /// <summary>
        ///     when true, 'dot items' will first see if they can be converted to a
        ///     static. Only if they can't will they be converted to text. This is
        ///     used for thes paths: nouns and relationshiptypes.
        /// </summary>
        public bool UseStatics { get; set; }

        #region Operator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> to overload
        /// </summary>
        public Token Operator { get; set; }

        #endregion

        #region OperatorOverloads

        /// <summary>
        ///     Gets the list of <see langword="operator" /> overloads that are
        ///     globally defined for this binding.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLFunctionNode> OperatorOverloads
        {
            get
            {
                if (fOperatorOverloads == null)
                {
                    fOperatorOverloads = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                }

                return fOperatorOverloads;
            }
        }

        #endregion

        #region Root

        /// <summary>
        ///     Gets/sets the root binding item. this is used for the first item in a
        ///     path.
        /// </summary>
        public NNLBindItem Root
        {
            get
            {
                return fRoot;
            }

            set
            {
                fRoot = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        #region BindingItems

        /// <summary>
        ///     Gets the list of binding items defined in this binding as direct
        ///     children.
        /// </summary>
        public System.Collections.Generic.Dictionary<Token, NNLBindItemBase> BindingItems
        {
            get
            {
                return fBindingItems;
            }
        }

        #endregion

        #region AllBindItems

        /// <summary>
        ///     Gets the dict with all the binding items in this binding, by name, so
        ///     we can easily check if all names are unique + easily resolve
        ///     references.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLBindItemBase> AllBindItems
        {
            get
            {
                if (fAllBindItems == null)
                {
                    fAllBindItems = new System.Collections.Generic.Dictionary<string, NNLBindItemBase>();
                }

                return fAllBindItems;
            }
        }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            RenderFunctions(renderTo);
            base.Render(renderTo);
            if (Root != null)
            {
                Root.Render(renderTo);
            }

            foreach (var i in AllBindItems)
            {
                // use allBindings here, so that the items don't need to render their own child nodes. This prevents us from getting into an ethernal loop (bindItems can reference other bindItems, including themselves).
                i.Value.Render(renderTo);
            }

            if (fOperatorOverloads != null)
            {
                foreach (var i in fOperatorOverloads)
                {
                    i.Value.Render(renderTo);
                }
            }

            if (Register)
            {
                renderTo.AddRegisteredBinding(this);
            }
        }

        /// <summary>tries to find a binding item with the specified name. Goes through all
        ///     the sub items as well, so it searches the entire binding.</summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        internal NNLBindItemBase FindBindingItem(string name)
        {
            if (fAllBindItems != null)
            {
                NNLBindItemBase iRes;
                if (AllBindItems.TryGetValue(name, out iRes))
                {
                    return iRes;
                }

                return null;
            }

            return null;
        }

        /// <summary>writes the content of the binding to a <paramref name="stream"/> so
        ///     that it can be reloaded for other compilations without having to
        ///     recompile the entire project.</summary>
        /// <param name="stream">The stream.</param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write((System.Int32)Operator);
            stream.Write(UseStatics);
            if (fRoot != null)
            {
                stream.Write(true); // there is a root.
                fRoot.Write(stream);
            }
            else
            {
                stream.Write(false); // there is no root.
            }

            stream.Write(AllBindItems.Count);
            foreach (var i in AllBindItems)
            {
                // we render all the bindings here in 1 list, so that items references themselves don't cause ethernal loops.
                stream.Write(i.Value.GetType().ToString()); // so we know which type of bind item.
                i.Value.Write(stream);
            }

            stream.Write(fBindingItems.Count);
            foreach (var i in BindingItems)
            {
                // also need to know which items were in this list.
                stream.Write(i.Value.Name);
            }

            if (HasFunctions)
            {
                stream.Write(Functions.Count);
                foreach (var i in Functions)
                {
                    i.Write(stream);
                }
            }
            else
            {
                stream.Write(0);
            }

            WriteList(stream, fOperatorOverloads);
        }

        /// <summary>Reads the data from specified stream reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            Operator = (Token)reader.ReadInt32();
            UseStatics = reader.ReadBoolean();
            if (reader.ReadBoolean())
            {
                // there is a root defined.
                Root = new NNLBindItem();

                    // only assign as root, no need to add to the list of Allbindings, this is only used during parsing.
                Root.Read(reader);
            }

            var iCount = reader.ReadInt32(); // read the list of bindings
            while (iCount > 0)
            {
                var iType = reader.ReadString();
                var iNew = (NNLBindItemBase)System.Activator.CreateInstance(System.Type.GetType(iType));
                iNew.Read(reader);
                AllBindItems.Add(iNew.Name, iNew);

                    // only add here, no need to add to the list of Allbindings, that is only used during parsing.
                iNew.Parent = this;
                iCount--;
            }

            iCount = reader.ReadInt32();
            while (iCount > 0)
            {
                var iName = reader.ReadString();
                NNLBindItemBase iFound;
                if (AllBindItems.TryGetValue(iName, out iFound))
                {
                    BindingItems.Add(iFound.Operator, iFound);
                }
                else
                {
                    throw new System.InvalidOperationException("Internal error: can't find binding section: " + iName);
                }

                iCount--;
            }

            iCount = reader.ReadInt32();
            while (iCount > 0)
            {
                var iNew = new NNLFunctionNode();
                iNew.Read(reader);
                Functions.Add(iNew);
                iNew.Parent = this;
                iCount--;
            }

            fOperatorOverloads = ReadList(reader);

            if (Root != null)
            {
                Root.ResolveReferences(null);
            }

            foreach (var i in AllBindItems)
            {
                i.Value.ResolveReferences(null);
            }
        }

        /// <summary>Resolves all references of the binding items.</summary>
        /// <param name="renderTo">The render to.</param>
        internal void ResolveAllReferences(NNLModuleCompiler renderTo)
        {
            if (fAllBindItems != null)
            {
                foreach (var i in fAllBindItems)
                {
                    i.Value.ResolveReferences(renderTo);
                }
            }

            if (fRoot != null)
            {
                fRoot.ResolveReferences(renderTo);
            }
        }
    }
}