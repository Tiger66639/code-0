// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternEditorCollection.cs" company="">
//   
// </copyright>
// <summary>
//   a base class for collections used in a <see cref="TextPatternEditor" /> .
//   The collection makes certain that UI data is loaded/unloaded at the
//   correct moments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>a base class for collections used in a <see cref="TextPatternEditor"/> .
    ///     The collection makes certain that UI data is loaded/unloaded at the
    ///     correct moments.</summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PatternEditorCollection<T> : ClusterCollection<T>
        where T : PatternEditorItem
    {
        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (PatternEditorItem i in this)
            {
                i.UnloadUIData();
            }

            base.ClearItems();
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            foreach (PatternEditorItem i in this)
            {
                i.UnloadUIData();
            }

            base.ClearItemsDirect();
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            this[index].UnloadUIData();
            base.RemoveItem(index);
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            this[index].UnloadUIData();
            base.RemoveItemDirect(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, T item)
        {
            this[index].UnloadUIData();
            base.SetItem(index, item);
        }

        /// <summary>Sets the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItemDirect(int index, T item)
        {
            this[index].UnloadUIData();
            base.SetItemDirect(index, item);
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="PatternEditorCollection{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.Designer.PatternEditorCollection`1"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public PatternEditorCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternEditorCollection{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.Designer.PatternEditorCollection`1"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public PatternEditorCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternEditorCollection{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.Designer.PatternEditorCollection`1"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <param name="cluserMeaning">The cluser meaning.</param>
        public PatternEditorCollection(INeuronWrapper owner, ulong linkMeaning, ulong cluserMeaning)
            : base(owner, linkMeaning, cluserMeaning)
        {
        }

        #endregion
    }
}