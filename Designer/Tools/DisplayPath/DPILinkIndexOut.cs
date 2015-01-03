// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPILinkIndexOut.cs" company="">
//   
// </copyright>
// <summary>
//   The dpi link index out.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>The dpi link index out.</summary>
    public class DPILinkIndexOut : DisplayPathItem
    {
        #region Index

        /// <summary>
        ///     Gets/sets the index of the link to use.
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

        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get from.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DPILinkIndexOut"/> class. 
        ///     Initializes a new instance of the <see cref="DPILinkIndexIn"/> class.</summary>
        public DPILinkIndexOut()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPILinkIndexOut"/> class. Initializes a new instance of the <see cref="DPILinkIndexIn"/> class.</summary>
        /// <param name="index">The index.</param>
        public DPILinkIndexOut(int index)
        {
            Index = index;
        }

        #endregion
    }
}