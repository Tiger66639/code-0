// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewObjectEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event hanlder for <see cref="JaStDev.HAB.WordNetSin.ObjectCreated" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Event hanlder for <see cref="JaStDev.HAB.WordNetSin.ObjectCreated" />
    /// </summary>
    public delegate void NewObjectEventHandler(object sender, NewObjectEventArgs e);

    /// <summary>
    ///     Event arguments for the
    ///     <see cref="JaStDev.HAB.WordNetSin.ObjectCreated" /> event.
    /// </summary>
    public class NewObjectEventArgs : WordNetEventArgs
    {
        /// <summary>
        ///     Gets or sets the string value for which a new object is created.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the SynSetID that is associated with the word.
        /// </summary>
        /// <value>
        ///     The syn set id.
        /// </value>
        public int SynSetId { get; set; }

        /// <summary>
        ///     Gets or sets the description for the object.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }
    }
}