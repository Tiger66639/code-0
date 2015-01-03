// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITesteable.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by
//   communication channels that can be tested with testcases.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by
    ///     communication channels that can be tested with testcases.
    /// </summary>
    public interface ITesteable
    {
        /// <summary>
        ///     Gets or sets the textsin that should be used to communicate with
        ///     during testing.
        /// </summary>
        /// <value>
        ///     The textsin.
        /// </value>
        TextSin Textsin { get; }

        /// <summary>
        ///     Gets/sets the ID of the
        ///     <see cref="JaStDev.HAB.Designer.CommChannel.Sin" />
        /// </summary>
        /// <remarks>
        ///     This prop is primarely for streaming
        /// </remarks>
        ulong NeuronID { get; set; }

        /// <summary>Sends the text to sin. Use this when called from a thread other then
        ///     UI thread.</summary>
        /// <param name="value">The value.</param>
        void SendTextToSinAsync(string value);
    }
}