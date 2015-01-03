// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRenderPipe.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that needs to be implemented by objects
//   that are able to render data that was send to a query as output.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     An <see langword="interface" /> that needs to be implemented by objects
    ///     that are able to render data that was send to a query as output.
    /// </summary>
    public interface IRenderPipe
    {
        /// <summary>
        ///     called when extra data needs to be saved to disk in seperate files.
        /// </summary>
        void Flush();

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        void WriteV1(System.IO.BinaryWriter writer);

        /// <summary>read the settings from a stream.</summary>
        /// <param name="reader">The reader.</param>
        void ReadV1(System.IO.BinaryReader reader);

        /// <summary>
        ///     called when the rendering is about to begin.
        /// </summary>
        void Open();

        /// <summary>
        ///     called when rendering is done and resources can be released.
        /// </summary>
        void Close();

        /// <summary>called when output was found</summary>
        /// <param name="values"></param>
        void Output(System.Collections.Generic.IList<Neuron> values);
    }
}