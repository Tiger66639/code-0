// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionSinFunctionAssembly.cs" company="">
//   
// </copyright>
// <summary>
//   contains the info for 1 assembly that is loaded because 1 or more of it's
//   functions is referenced in the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     contains the info for 1 assembly that is loaded because 1 or more of it's
    ///     functions is referenced in the network.
    /// </summary>
    public class ReflectionSinFunctionAssembly
    {
        /// <summary>
        ///     Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the nr of functions that are loaded from this assembly.
        /// </summary>
        /// <value>
        ///     The <see langword="ref" /> count.
        /// </value>
        public int RefCount { get; set; }
    }
}