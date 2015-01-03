// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PosGroupEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   The pos group event handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The pos group event handler.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    public delegate void PosGroupEventHandler(object sender, PosGroupEventArgs e);

    /// <summary>
    ///     Event arguments for the
    ///     <see cref="JaStDev.HAB.WordNetSin.POSGroupCreated" />
    /// </summary>
    public class PosGroupEventArgs : WordNetEventArgs
    {
        /// <summary>
        ///     Gets or sets the string value for which a new object is created.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="POS" /> that has been assigned to this
        ///     group.
        /// </summary>
        /// <value>
        ///     The POS.
        /// </value>
        public string POS { get; set; }
    }
}