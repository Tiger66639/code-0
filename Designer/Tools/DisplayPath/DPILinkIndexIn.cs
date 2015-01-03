// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPILinkIndexIn.cs" company="">
//   
// </copyright>
// <summary>
//   Allows to go from the current path item to the neuron found on an
//   incomming link at a specific index position.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Allows to go from the current path item to the neuron found on an
    ///     incomming link at a specific index position.
    /// </summary>
    public class DPILinkIndexIn : DisplayPathItem
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="DPILinkIndexIn" /> class.
        /// </summary>
        public DPILinkIndexIn()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPILinkIndexIn"/> class.</summary>
        /// <param name="index">The index.</param>
        public DPILinkIndexIn(int index)
        {
            Index = index;
        }

        #endregion
    }
}