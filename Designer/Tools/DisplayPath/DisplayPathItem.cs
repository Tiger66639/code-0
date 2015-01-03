// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayPathItem.cs" company="">
//   
// </copyright>
// <summary>
//   The root object for all items that are involved in creating a path for
//   displaying a selected item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     The root object for all items that are involved in creating a path for
    ///     displaying a selected item.
    /// </summary>
    public abstract class DisplayPathItem
    {
        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public abstract object GetFrom(ICodeItemsOwner owner);

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public abstract object GetFrom(PatternEditorItem owner);

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public abstract object GetFrom(System.Collections.IList list);
    }
}