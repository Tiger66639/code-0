// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompactBinaryReader.cs" company="">
//   
// </copyright>
// <summary>
//   a reader that's able read ulongs as a compact <see langword="byte" />
//   stream, where only the required amound of bytes are read.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a reader that's able read ulongs as a compact <see langword="byte" />
    ///     stream, where only the required amound of bytes are read.
    /// </summary>
    /// <remarks>
    ///     This class is used intensly, so it is optimized heavely to make use of
    ///     the underlying stream as much as possible, which buffers the data. This
    ///     prevents unnecessary buffer copying.
    /// </remarks>
    public class CompactBinaryReader : System.IO.BinaryReader
    {
        /// <summary>The f version.</summary>
        private int fVersion = 1;

        /// <summary>The f data.</summary>
        private readonly byte[] fData = new byte[8];

        /// <summary>The f input.</summary>
        private readonly Storage.BlockStream fInput; // so we have fast access to the stream in the correct cast.

        /// <summary>Initializes a new instance of the <see cref="CompactBinaryReader"/> class. Initializes a new instance of the <see cref="CompactBinaryReader"/>
        ///     class.</summary>
        /// <param name="input">The input.</param>
        public CompactBinaryReader(System.IO.Stream input)
            : base(input)
        {
            fInput = (Storage.BlockStream)input;
            System.Diagnostics.Debug.Assert(fInput != null);
        }

        #region Version

        /// <summary>
        ///     Gets/sets the version to use for reading hte data. When set to 0, a
        ///     full <see langword="ulong" /> is read, otherwise a compact version is
        ///     read and returned.
        /// </summary>
        public int Version
        {
            get
            {
                return fVersion;
            }

            set
            {
                fVersion = value;
            }
        }

        #endregion

        /// <summary>
        ///     Reads an 8-byte unsigned integer from the current stream and advances
        ///     the position of the stream by eight bytes.
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     The stream is closed.
        /// </exception>
        /// <returns>
        ///     An 8-byte unsigned integer read from this stream.
        /// </returns>
        public override ulong ReadUInt64()
        {
            if (Version == 0)
            {
                return base.ReadUInt64();
            }

            System.Array.Clear(fData, 0, fData.Length);
            var iLength = fInput.ReadByte();
            if (iLength != -1)
            {
                fInput.Read(fData, 0, iLength);
                return System.BitConverter.ToUInt64(fData, 0);
            }

            throw new System.IO.EndOfStreamException();
        }
    }
}