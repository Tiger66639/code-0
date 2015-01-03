// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnBase.cs" company="">
//   
// </copyright>
// <summary>
//   the base class for <see cref="ANN" /> algorithm
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ML
{
    /// <summary>
    ///     the base class for <see cref="ANN" /> algorithm
    /// </summary>
    public class AnnBase
    {
        /// <summary>Initializes a new instance of the <see cref="AnnBase"/> class.</summary>
        /// <param name="root">The root.</param>
        public AnnBase(ulong root)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AnnBase"/> class.</summary>
        /// <param name="root">The root.</param>
        public AnnBase(JaStDev.HAB.Neuron root)
        {
        }

        #region prop

        /// <summary>
        ///     Gets or sets the nr inputs available for the <see cref="ANN" />
        /// </summary>
        /// <value>
        ///     The nr inputs.
        /// </value>
        public int NrInputs { get; set; }

        /// <summary>
        ///     Gets or sets the nr outputs.
        /// </summary>
        /// <value>
        ///     The nr outputs.
        /// </value>
        public int NrOutputs { get; set; }

        /// <summary>
        ///     Gets or sets the nr layers.
        /// </summary>
        /// <value>
        ///     The nr layers.
        /// </value>
        public int NrLayers { get; set; }

        #endregion
    }
}