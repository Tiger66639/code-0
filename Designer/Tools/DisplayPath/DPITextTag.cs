// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPITextTag.cs" company="">
//   
// </copyright>
// <summary>
//   a display path item that allows a root to add an arbitrary text item it
//   can select based on some 'text-tag' information that it can link to one
//   of it's global views.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     a display path item that allows a root to add an arbitrary text item it
    ///     can select based on some 'text-tag' information that it can link to one
    ///     of it's global views.
    /// </summary>
    public class DPITextTag : DisplayPathItem
    {
        /// <summary>Initializes a new instance of the <see cref="DPITextTag"/> class.</summary>
        /// <param name="tag">The tag.</param>
        public DPITextTag(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        ///     Gets the tag.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner. not valid</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item. not valid</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            owner.PutFocusOn(Tag);
            return null;
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors. not valid</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }
    }
}