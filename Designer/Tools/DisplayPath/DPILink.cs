// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPILink.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for items that go from the current path item to the neuron
//   found on the other side of a link, for a speicific meaning
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Base class for items that go from the current path item to the neuron
    ///     found on the other side of a link, for a speicific meaning
    /// </summary>
    public abstract class DPILink : DisplayPathItem
    {
        #region Meaning

        /// <summary>
        ///     Gets/sets the id of the neuron to use as meaning of a link, for
        ///     retrieving the result of this display path item.
        /// </summary>
        public ulong Meaning { get; set; }

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DPILink" /> class.
        /// </summary>
        public DPILink()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPILink"/> class.</summary>
        /// <param name="meaning">The meaning.</param>
        public DPILink(ulong meaning)
        {
            Meaning = meaning;
        }

        #endregion
    }
}