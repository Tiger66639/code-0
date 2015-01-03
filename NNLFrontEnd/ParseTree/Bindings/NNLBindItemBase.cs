// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindItemBase.cs" company="">
//   
// </copyright>
// <summary>
//   base class for a section in a binding. This is either for regular
//   sections or for function sections.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     base class for a section in a binding. This is either for regular
    ///     sections or for function sections.
    /// </summary>
    internal class NNLBindItemBase : NNLStatementNode
    {
        /// <summary>The f binding items.</summary>
        private System.Collections.Generic.Dictionary<Token, NNLBindItemBase> fBindingItems;

        /// <summary>The f sub items to resolve.</summary>
        private System.Collections.Generic.List<string> fSubItemsToResolve;

        /// <summary>Initializes a new instance of the <see cref="NNLBindItemBase"/> class. 
        ///     Initializes a new instance of the <see cref="NNLBindItemBase"/>
        ///     class.</summary>
        public NNLBindItemBase()
            : base(NodeType.Bind)
        {
        }

        #region Operator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> to overload. When this is
        ///     'Token.Word', it's a static.
        /// </summary>
        public Token Operator { get; set; }

        #endregion

        /// <summary>
        ///     gets the parent binding for this item.
        /// </summary>
        public NNLBinding RootBinding
        {
            get
            {
                var iParent = Parent;
                while (iParent != null && !(iParent is NNLBinding))
                {
                    iParent = iParent.Parent;
                }

                return iParent as NNLBinding;
            }
        }

        #region BindingItems

        /// <summary>
        ///     Gets the list of binding items defined in this binding
        /// </summary>
        public System.Collections.Generic.Dictionary<Token, NNLBindItemBase> BindingItems
        {
            get
            {
                if (fBindingItems == null)
                {
                    fBindingItems = new System.Collections.Generic.Dictionary<Token, NNLBindItemBase>();
                }

                return fBindingItems;
            }
        }

        #endregion

        #region SubItemsToResolve

        /// <summary>
        ///     Gets the list of binding items that are referenced by this binding and
        ///     which should be included as children.
        /// </summary>
        public System.Collections.Generic.List<string> SubItemsToResolve
        {
            get
            {
                if (fSubItemsToResolve == null)
                {
                    fSubItemsToResolve = new System.Collections.Generic.List<string>();
                }

                return fSubItemsToResolve;
            }
        }

        #endregion

        /// <summary>The render.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            ResolveReferences(renderTo);

            // don't need to render the children, this is done by the binding itself, to prevent us from getting stuck in an ethernal loop.
        }

        /// <summary>makes certain taht all the references to other binding items are
        ///     resolved.</summary>
        /// <param name="renderTo">the object to render to. Can be null, in case that the data was read
        ///     from file. In this case, when there are problems, an exception is
        ///     thrown instead of logged to the renderer.</param>
        internal virtual void ResolveReferences(NNLModuleCompiler renderTo)
        {
            if (fSubItemsToResolve != null)
            {
                var iRoot = RootBinding;
                foreach (var i in fSubItemsToResolve)
                {
                    var iItem = RootBinding.FindBindingItem(i);
                    if (iItem != null)
                    {
                        if (BindingItems.ContainsKey(iItem.Operator) == false)
                        {
                            BindingItems.Add(iItem.Operator, iItem);
                        }
                        else if (renderTo != null)
                        {
                            LogPosError(
                                string.Format(
                                    "There is already a sub section for the {0} operator.", 
                                    Tokenizer.GetSymbolForToken(iItem.Operator)), 
                                renderTo);
                        }
                        else
                        {
                            throw new System.InvalidOperationException(
                                string.Format(
                                    "There is already a sub section for the {0} operator.", 
                                    Tokenizer.GetSymbolForToken(iItem.Operator)));
                        }
                    }
                    else if (renderTo != null)
                    {
                        LogPosError("Reference to an unknown binding item: " + i, renderTo);
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Reference to an unknown binding item: " + i);
                    }
                }

                fSubItemsToResolve = null; // no need to resolve them multiple times.
            }
        }

        /// <summary>write the data to a <paramref name="stream"/> so that the binding can
        ///     be loaded without a full recompile.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write((System.Int32)Operator);

            if (fBindingItems != null)
            {
                stream.Write(fBindingItems.Count);
                foreach (var i in fBindingItems)
                {
                    stream.Write(i.Value.Name);

                        // we only write the name, to prevent us from getting stuck in an ethernal loop. The items theselves ar saved by the binding.
                }
            }
            else
            {
                stream.Write(0);
            }
        }

        /// <summary>Reads the content from the specified binary reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            Operator = (Token)reader.ReadInt32();

            var iCount = reader.ReadInt32(); // read the list of bindings
            if (iCount > 0)
            {
                fSubItemsToResolve = new System.Collections.Generic.List<string>();
                while (iCount > 0)
                {
                    fSubItemsToResolve.Add(reader.ReadString());
                    iCount--;
                }
            }
        }
    }
}