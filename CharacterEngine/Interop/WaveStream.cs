// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaveStream.cs" company="">
//   
// </copyright>
// <summary>
//   The wave stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WaveLib
{
    /// <summary>The wave stream.</summary>
    public class WaveStream : System.IO.Stream, System.IDisposable
    {
        /// <summary>The m_ data pos.</summary>
        private long m_DataPos;

        /// <summary>The m_ length.</summary>
        private long m_Length;

        /// <summary>The m_ stream.</summary>
        private readonly System.IO.Stream m_Stream;

        /// <summary>Initializes a new instance of the <see cref="WaveStream"/> class.</summary>
        /// <param name="fileName">The file name.</param>
        public WaveStream(string fileName)
            : this(new System.IO.FileStream(fileName, System.IO.FileMode.Open))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WaveStream"/> class.</summary>
        /// <param name="S">The s.</param>
        public WaveStream(System.IO.Stream S)
        {
            m_Stream = S;
            ReadHeader();
        }

        /// <summary>Gets the format.</summary>
        public WaveFormat Format { get; private set; }

        /// <summary>Gets a value indicating whether can read.</summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets a value indicating whether can seek.</summary>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets a value indicating whether can write.</summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets the length.</summary>
        public override long Length
        {
            get
            {
                return m_Length;
            }
        }

        /// <summary>Gets or sets the position.</summary>
        public override long Position
        {
            get
            {
                return m_Stream.Position - m_DataPos;
            }

            set
            {
                Seek(value, System.IO.SeekOrigin.Begin);
            }
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            if (m_Stream != null)
            {
                m_Stream.Close();
            }

            System.GC.SuppressFinalize(this);
        }

        /// <summary>The read chunk.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadChunk(System.IO.BinaryReader reader)
        {
            var ch = new byte[4];
            reader.Read(ch, 0, ch.Length);
            return System.Text.Encoding.ASCII.GetString(ch);
        }

        /// <summary>The read header.</summary>
        /// <exception cref="Exception"></exception>
        private void ReadHeader()
        {
            var Reader = new System.IO.BinaryReader(m_Stream);
            if (ReadChunk(Reader) != "RIFF")
            {
                throw new System.Exception("Invalid file format");
            }

            Reader.ReadInt32(); // File length minus first 8 bytes of RIFF description, we don't use it

            if (ReadChunk(Reader) != "WAVE")
            {
                throw new System.Exception("Invalid file format");
            }

            if (ReadChunk(Reader) != "fmt ")
            {
                throw new System.Exception("Invalid file format");
            }

            var len = Reader.ReadInt32();
            if (len < 16)
            {
                // bad format chunk length
                throw new System.Exception("Invalid file format");
            }

            Format = new WaveFormat(22050, 16, 2); // initialize to any format
            Format.wFormatTag = Reader.ReadInt16();
            Format.nChannels = Reader.ReadInt16();
            Format.nSamplesPerSec = Reader.ReadInt32();
            Format.nAvgBytesPerSec = Reader.ReadInt32();
            Format.nBlockAlign = Reader.ReadInt16();
            Format.wBitsPerSample = Reader.ReadInt16();

            // advance in the stream to skip the wave format block 
            len -= 16; // minimum format size
            while (len > 0)
            {
                Reader.ReadByte();
                len--;
            }

            // assume the data chunk is aligned
            while (m_Stream.Position < m_Stream.Length && ReadChunk(Reader) != "data")
            {
                ;
            }

            if (m_Stream.Position >= m_Stream.Length)
            {
                throw new System.Exception("Invalid file format");
            }

            m_Length = Reader.ReadInt32();
            m_DataPos = m_Stream.Position;

            Position = 0;
        }

        /// <summary>Finalizes an instance of the <see cref="WaveStream"/> class. </summary>
        ~WaveStream()
        {
            Dispose();
        }

        /// <summary>The close.</summary>
        public override void Close()
        {
            Dispose();
        }

        /// <summary>The flush.</summary>
        public override void Flush()
        {
        }

        /// <summary>The set length.</summary>
        /// <param name="len">The len.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void SetLength(long len)
        {
            throw new System.InvalidOperationException();
        }

        /// <summary>The seek.</summary>
        /// <param name="pos">The pos.</param>
        /// <param name="o">The o.</param>
        /// <returns>The <see cref="long"/>.</returns>
        public override long Seek(long pos, System.IO.SeekOrigin o)
        {
            switch (o)
            {
                case System.IO.SeekOrigin.Begin:
                    m_Stream.Position = pos + m_DataPos;
                    break;
                case System.IO.SeekOrigin.Current:
                    m_Stream.Seek(pos, System.IO.SeekOrigin.Current);
                    break;
                case System.IO.SeekOrigin.End:
                    m_Stream.Position = m_DataPos + m_Length - pos;
                    break;
            }

            return Position;
        }

        /// <summary>The read.</summary>
        /// <param name="buf">The buf.</param>
        /// <param name="ofs">The ofs.</param>
        /// <param name="count">The count.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int Read(byte[] buf, int ofs, int count)
        {
            var toread = (int)System.Math.Min(count, m_Length - Position);
            return m_Stream.Read(buf, ofs, toread);
        }

        /// <summary>The write.</summary>
        /// <param name="buf">The buf.</param>
        /// <param name="ofs">The ofs.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void Write(byte[] buf, int ofs, int count)
        {
            throw new System.InvalidOperationException();
        }
    }
}