// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPIParent.cs" company="">
//   
// </copyright>
// <summary>
//   Allows to go to a parent at a specific index of the current path item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Allows to go to a parent at a specific index of the current path item.
    /// </summary>
    public class DPIParent : DisplayPathItem
    {
        #region Index

        /// <summary>
        ///     Gets/sets the index position of the parent
        /// </summary>
        public int Index { get; set; }

        #endregion

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            return list[Index];
        }
    }
}