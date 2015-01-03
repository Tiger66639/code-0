// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompactBinaryWriter.cs" company="">
//   
// </copyright>
// <summary>
//   writes ulongs in a compact manner to save space.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     writes ulongs in a compact manner to save space.
    /// </summary>
    public class CompactBinaryWriter : System.IO.BinaryWriter
    {
        /// <summary>The f ulong.</summary>
        private readonly System.IO.MemoryStream fUlong; // so we can write the the byte array.

        /// <summary>The f ulong writer.</summary>
        private readonly System.IO.BinaryWriter fUlongWriter;

                                                // without having to use BitConverter.GetBytes, cause then we use lots more memory

        /// <summary>Initializes a new instance of the <see cref="CompactBinaryWriter"/> class. Initializes a new instance of the <see cref="CompactBinaryWriter"/>
        ///     class.</summary>
        /// <param name="output">The output.</param>
        public CompactBinaryWriter(System.IO.Stream output)
            : base(output)
        {
            fUlong = new System.IO.MemoryStream();
            fUlongWriter = new System.IO.BinaryWriter(fUlong);
        }

        /// <summary>Writes an eight-byte unsigned integer to the current stream and
        ///     advances the stream position by eight bytes.</summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        public override void Write(ulong value)
        {
            fUlong.Position = 0;
            fUlongWriter.Write(value); // use a memstream instead of BitConverter.GetBytes. This uses less memory.
            fUlongWriter.Flush(); // make certain that it is actually written.
            byte iNrBytes = 8;
            if (value <= byte.MaxValue)
            {
                iNrBytes = 1;
            }
            else if (value < ushort.MaxValue)
            {
                iNrBytes = 2;
            }
            else if (value <= 0xFFF)
            {
                iNrBytes = 3;
            }
            else if (value <= uint.MaxValue)
            {
                iNrBytes = 4;
            }
            else if (value <= 0xFFFFF)
            {
                iNrBytes = 5;
            }
            else if (value <= 0xFFFFFF)
            {
                iNrBytes = 6;
            }
            else if (value <= 0xFFFFFFF)
            {
                iNrBytes = 7;
            }

            Write(iNrBytes);
            Write(fUlong.GetBuffer(), 0, iNrBytes);
        }

        /// <summary>writes a regular <see langword="ulong"/> without compacting.</summary>
        /// <param name="value"></param>
        internal void WriteUlong(ulong value)
        {
            base.Write(value);
        }
    }
}