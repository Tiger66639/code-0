// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VarValuesList.cs" company="">
//   
// </copyright>
// <summary>
//   a list that keeps track which <see cref="VarDict" /> created it, so that
//   it can be shared accorss multiple VarDicts but only gets recycled by 1
//   (the first). The list itself is stored as a value, so that it can be
//   replaced with a new value without having to do copy the contents of the
//   lists all the time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a list that keeps track which <see cref="VarDict" /> created it, so that
    ///     it can be shared accorss multiple VarDicts but only gets recycled by 1
    ///     (the first). The list itself is stored as a value, so that it can be
    ///     replaced with a new value without having to do copy the contents of the
    ///     lists all the time.
    /// </summary>
    public class VarValuesList
    {
        /// <summary>The data.</summary>
        public System.Collections.Generic.List<Neuron> Data;

        /// <summary>Gets or sets the owner.</summary>
        internal VarDict Owner { get; set; }
    }
}