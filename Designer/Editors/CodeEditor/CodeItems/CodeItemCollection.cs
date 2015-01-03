// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Contains a list of <see cref="CodeItem" /> objects that are the children
//   of a neuron cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains a list of <see cref="CodeItem" /> objects that are the children
    ///     of a neuron cluster.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a separete class since it has to make certain that all changes to
    ///         this list are also reflected in the underlying list of the
    ///         <see cref="NeuronCluster" /> .
    ///     </para>
    ///     <para>
    ///         doesn't raise events when the list is changed cause this confuses the
    ///         undo system. Instead, drag handlers and functions generate their own undo
    ///         data.
    ///     </para>
    /// </remarks>
    public class CodeItemCollection : ClusterCollection<CodeItem>
    {
        #region prop

        /// <summary>
        ///     Gets/sets wether the collection is monitoring the network for changes.
        ///     This is also valid for all child items.
        /// </summary>
        /// <remarks>
        ///     When true, the collection uses more system resources. False is used
        ///     for displaying the code being executed, this can not change.
        /// </remarks>
        /// <value>
        /// </value>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }

            set
            {
                if (value != base.IsActive)
                {
                    base.IsActive = value;
                    foreach (var i in this)
                    {
                        i.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public CodeItemCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        /// <param name="isActive">if set to <c>true</c> the collection should be active, so it should
        ///     monitor any changes. Otherwise, the list wont monitor any changes and
        ///     all child items will also be loaded de-activated.</param>
        public CodeItemCollection(INeuronWrapper owner, NeuronCluster childList, bool isActive)
            : base(owner, childList, isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public CodeItemCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
        {
        }

        #endregion

        #region Functions

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="CodeItem"/>.</returns>
        public override CodeItem GetWrapperFor(Neuron toWrap)
        {
            var iRes = EditorsHelper.CreateCodeItemFor(toWrap, IsActive);
            return iRes;
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            if (linkMeaning == (ulong)PredefinedNeurons.Arguments)
            {
                // a code list created for arguments, has a different meaning cause it can't really be executed (it can have statics) but is part of the code.
                return (ulong)PredefinedNeurons.ArgumentsList;
            }

            return (ulong)PredefinedNeurons.Code;
        }

        #endregion
    }
}