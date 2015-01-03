// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnFast.cs" company="">
//   
// </copyright>
// <summary>
//   a fast implementation of the ANN, uses more memory, keeps all data in
//   memory all the time.s
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ML
{
    /// <summary>
    ///     a fast implementation of the ANN, uses more memory, keeps all data in
    ///     memory all the time.s
    /// </summary>
    public class AnnFast : AnnBase
    {
        /// <summary>Initializes a new instance of the <see cref="AnnFast"/> class.</summary>
        /// <param name="root">The root.</param>
        public AnnFast(ulong root)
            : base(root)
        {
        }
    }
}