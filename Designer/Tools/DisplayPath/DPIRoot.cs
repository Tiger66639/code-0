// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPIRoot.cs" company="">
//   
// </copyright>
// <summary>
//   Contains the neuron to start with for building the dipslay path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Contains the neuron to start with for building the dipslay path.
    /// </summary>
    public abstract class DPIRoot : DisplayPathItem
    {
        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<DisplayPathItem> fItems =
            new System.Collections.Generic.List<DisplayPathItem>();

        #region Item

        /// <summary>
        ///     Gets/sets the neuron to start from
        /// </summary>
        public Neuron Item { get; set; }

        #endregion

        /// <summary>
        ///     Gets all the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public System.Collections.Generic.List<DisplayPathItem> Items
        {
            get
            {
                return fItems;
            }
        }

        /// <summary>The select path result.</summary>
        internal abstract void SelectPathResult();

        /// <summary>Duplicates this instance.</summary>
        /// <returns>The <see cref="DPIRoot"/>.</returns>
        internal abstract DPIRoot Duplicate();
    }
}