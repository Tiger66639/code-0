// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLNodesList.cs" company="">
//   
// </copyright>
// <summary>
//   contains code statements. For functions or conditional parts
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     contains code statements. For functions or conditional parts
    /// </summary>
    internal class NNLNodesList : NNLNode
    {
        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<NNLStatementNode> fItems =
            new System.Collections.ObjectModel.ObservableCollection<NNLStatementNode>();

        /// <summary>Initializes a new instance of the <see cref="NNLNodesList"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLNodesList(NodeType type)
            : base(type)
        {
            IsParam = false;
            fItems.CollectionChanged += fItems_CollectionChanged;
        }

        /// <summary>Handles the CollectionChanged event of the <see cref="fItems"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void fItems_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NNLStatementNode i in e.NewItems)
                {
                    i.Parent = this;
                }
            }
        }

        /// <summary>Renders the items to a list of neurons.</summary>
        /// <param name="renderTo">The render to.</param>
        public void RenderItems(NNLModuleCompiler renderTo)
        {
            if (RenderedItems == null)
            {
                System.Collections.Generic.List<Neuron> iRenderExtraIn; // put extra items in this list.
                RenderedItems = new System.Collections.Generic.List<Neuron>();
                if (IsParam == false)
                {
                    renderTo.RenderingTo.Push(RenderedItems);

                        // so that child items can render multiple objects. When rendering a param values list, multiple items need to be rendered in the parent list.
                    iRenderExtraIn = RenderedItems;
                }
                else
                {
                    iRenderExtraIn = renderTo.RenderingTo.Peek();
                }

                try
                {
                    foreach (var i in Items)
                    {
                        i.RenderItem(renderTo, iRenderExtraIn);
                        if (IsParam || !(i is NNLLocalDeclNode))
                        {
                            // param lists can have values, code lists can't have values included. NNLLocalDecls always store the var in the Item prop, but can render an instruction if needed.
                            if (i.Item != null)
                            {
                                // if null, should already have rendered an error.
                                RenderedItems.Add(i.Item);
                            }
                        }
                    }
                }
                finally
                {
                    if (IsParam == false)
                    {
                        renderTo.RenderingTo.Pop();
                    }
                }
            }
        }

        /// <summary>Renders the items in a reversed order to a list of neurons. This is
        ///     used for arguments and 'union'.</summary>
        /// <param name="renderTo">The render to.</param>
        public void RenderReverseItems(NNLModuleCompiler renderTo)
        {
            if (RenderedItems == null)
            {
                System.Collections.Generic.List<Neuron> iRenderExtraIn; // put extra items in this list.
                RenderedItems = new System.Collections.Generic.List<Neuron>();
                if (IsParam == false)
                {
                    renderTo.RenderingTo.Push(RenderedItems);

                        // so that child items can render multiple objects. When rendering a param values list, multiple items need to be rendered in the parent list.
                    iRenderExtraIn = RenderedItems;
                }
                else
                {
                    iRenderExtraIn = renderTo.RenderingTo.Peek();
                }

                try
                {
                    foreach (var i in Enumerable.Reverse(Items))
                    {
                        i.RenderItem(renderTo, iRenderExtraIn);
                        if (IsParam || !(i is NNLLocalDeclNode))
                        {
                            // param lists can have values, code lists can't have values included. NNLLocalDecls always store the var in the Item prop, but can render an instruction if needed.
                            if (i.Item != null)
                            {
                                // if null, should already have rendered an error.
                                RenderedItems.Add(i.Item);
                            }
                        }
                    }

                    RenderedItems.Reverse();

                        // reverse the result list to make certain that the values are in the order that they were originally specified
                }
                finally
                {
                    if (IsParam == false)
                    {
                        renderTo.RenderingTo.Pop();
                    }
                }
            }
        }

        #region Items

        /// <summary>
        ///     Gets the children.
        /// </summary>
        public System.Collections.Generic.IList<NNLStatementNode> Items
        {
            get
            {
                return fItems;
            }
        }

        #region RenderedItems

        /// <summary>
        ///     Gets the list of rendered items. this is used as a temporary store,
        ///     for the children (like conditional parts), but they empty the list
        ///     before finishing the render.
        /// </summary>
        public System.Collections.Generic.List<Neuron> RenderedItems { get; set; }

        #endregion

        #region IsParam

        /// <summary>
        ///     Gets/sets the value that indicates if this is a list of parameter
        ///     values or a code list (rendered differently). (code list by default).
        /// </summary>
        public bool IsParam { get; set; }

        #endregion

        #endregion
    }
}