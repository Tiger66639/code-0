// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugLink.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for a <see cref="Link" /> object. This class provides an easy
//   access to the other side of the link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for a <see cref="Link" /> object. This class provides an easy
    ///     access to the other side of the link.
    /// </summary>
    /// <remarks>
    ///     While a <see cref="Link" /> object stores info about the 2 sides of a
    ///     link, and actual physical object is used by both sides, this class does
    ///     not. It only represents 1 side, through the
    ///     <see cref="JaStDev.HAB.Designer.DebugRef.PointsTo" /> property. The other
    ///     side is the <see cref="DebugNeuron" /> that owns this object. This allows
    ///     for a tree like walk of the links,which is more suitable for UI.
    /// </remarks>
    public class DebugLink : DebugRef
    {
        #region Fields

        /// <summary>The f item.</summary>
        private Link fItem;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DebugLink"/> class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="owner">The owner.</param>
        public DebugLink(Link item, DebugNeuron owner)
        {
            Item = item;
            Owner = owner;
            if (item.FromID == owner.Item.ID)
            {
                // we compare on ID cause this is faster to retrieve (a Neuron needs to be requested from teh brain, while id is local field.
                PointsTo = new DebugNeuron(item.To);
            }
            else
            {
                PointsTo = new DebugNeuron(item.From);
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the link that we provide a wrapper for.
        /// </summary>
        public Link Item
        {
            get
            {
                return fItem;
            }

            internal set
            {
                fItem = value;
                OnPropertyChanged("Item");
            }
        }

        #endregion
    }
}