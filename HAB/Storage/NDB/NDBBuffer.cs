// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBBuffer.cs" company="">
//   
// </copyright>
// <summary>
//   a stream that buffers another stream in such a way that it works fast
//   with the ndb (whhich is not the case with the regular bufferedSTream)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Storage
{
    /// <summary>
    ///     a stream that buffers another stream in such a way that it works fast
    ///     with the ndb (whhich is not the case with the regular bufferedSTream)
    /// </summary>
    public class NDBBuffer : System.IO.Stream
    {
        /// <summary>
        ///     the buffer for the endof the file usually isn't the full length of the
        ///     buffer.
        /// </summary>
        public int BufferSize;

        /// <summary>
        ///     the index of the current block within the current buffer.
        /// </summary>
        public int CurrentBlock;

        /// <summary>The f start of buffer.</summary>
        private long fStartOfBuffer;

        /// <summary>
        ///     gets/sets the position within the current block.
        /// </summary>
        public long PosInBlock;

        /// <summary>Initializes a new instance of the <see cref="NDBBuffer"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="blockSize">The block Size.</param>
        public NDBBuffer(System.IO.Stream toWrap, int blockSize)
        {
            fToWrap = toWrap;
            fBlockSize = blockSize;
            fBuffer = new System.IO.MemoryStream(Settings.DBBufferSize);
            fBuffer.SetLength(Settings.DBBufferSize);
        }

        /// <summary>
        ///     the start in the stream of the current buffer.
        /// </summary>
        public long StartOfBuffer
        {
            get
            {
                return fStartOfBuffer;
            }

            set
            {
                if (value >= 0)
                {
                    // Debug.Assert(value < fToWrap.Length && value >= 0);
                    fStartOfBuffer = value;
                    PosInBlock = 0;
                    CurrentBlock = 0;
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }
        }

        /// <summary>
        ///     the current absolute position in the wrapped stream.
        /// </summary>
        public long AbsolutePos
        {
            get
            {
                return StartOfBuffer + Buffer.Position;
            }

            set
            {
                var iInBuffer = value - StartOfBuffer; // the pos within the buffer.
                Buffer.Position = iInBuffer;
                CurrentBlock = (int)(iInBuffer / fBlockSize);
                PosInBlock = iInBuffer % fBlockSize;
            }
        }

        /// <summary>
        ///     gets or sets the absolute position within the wrapped stream. This is
        ///     primarely for read purposes. For write, use the SetAbsPos
        /// </summary>
        /// <returns>
        ///     The current position within the stream.
        /// </returns>
        public override long Position
        {
            get
            {
                return AbsolutePos;
            }

            set
            {
                if (StartOfBuffer > value || StartOfBuffer + BufferSize < value + fBlockSize)
                {
                    // the new pos is outside of the buffer, so reload. We need to make certain that we can read an entire block
                    StartOfBuffer = value;
                    fBufferLoaded = false;
                }
                else
                {
                    AbsolutePos = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the write position.
        /// </summary>
        /// <value>
        ///     The write pos.
        /// </value>
        public long WritePos
        {
            get
            {
                if (fWritePos != -1)
                {
                    return fWritePos;
                }

                return fToWrap.Position;
            }

            set
            {
                if (fWritePos != value)
                {
                    if (fBufferLoaded && AbsolutePos > value
                        && (AbsolutePos + BufferSize < value || AbsolutePos + BufferSize < value + fBlockSize))
                    {
                        // we need to reload the buffer if we write in the middle of it. whenever we start a write operation, we will always write an entire block, so we can check for this now without worrying about the writes
                        fBufferLoaded = false;

                            // this has to be done before setting hte pos and writePos cause 'AbsolutePos' can recalculate the current buffer
                    }

                    fToWrap.Position = value;
                    fWritePos = value;
                }
            }
        }

        /// <summary>
        ///     Gets the buffer, makes certain that it is loaded.
        /// </summary>
        /// <value>
        ///     The buffer.
        /// </value>
        internal System.IO.MemoryStream Buffer
        {
            get
            {
                // Debug.Assert(StartOfBuffer >= 0);
                if (fBufferLoaded == false)
                {
                    // Debug.Assert(StartOfBuffer >= 0);
                    if (StartOfBuffer >= 0)
                    {
                        fToWrap.Position = StartOfBuffer;

                            // need to make certain that the stream is at the correct pos, could have done a read in between.
                        var iBuf = fBuffer.GetBuffer();
                        var iLength = fToWrap.Length;
                        if (iLength - StartOfBuffer > iBuf.Length)
                        {
                            fToWrap.Read(iBuf, 0, iBuf.Length);
                            BufferSize = iBuf.Length;
                        }
                        else
                        {
                            BufferSize = (int)(iLength - StartOfBuffer);
                            fToWrap.Read(iBuf, 0, BufferSize);
                        }

                        fWritePos = -1; // the position within the file has changed, so the write pos is no longer valid.
                        fBuffer.Position = 0;

                            // need to make certain that the buffer starts to read from the front again.
                        fBufferLoaded = true;
                    }
                    else
                    {
                        throw new System.InvalidOperationException();
                    }
                }

                return fBuffer;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports reading.
        /// </summary>
        /// <exception cref="System.NotImplementedException" />
        /// <returns>
        ///     <see langword="true" /> if the stream supports reading; otherwise,
        ///     false.
        /// </returns>
        public override bool CanRead
        {
            get
            {
                return fToWrap.CanRead;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports seeking.
        /// </summary>
        /// <exception cref="System.NotImplementedException" />
        /// <returns>
        ///     <see langword="true" /> if the stream supports seeking; otherwise,
        ///     false.
        /// </returns>
        public override bool CanSeek
        {
            get
            {
                return fToWrap.CanSeek;
                
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports writing.
        /// </summary>
        /// <exception cref="System.NotImplementedException" />
        /// <returns>
        ///     <see langword="true" /> if the stream supports writing; otherwise,
        ///     false.
        /// </returns>
        public override bool CanWrite
        {
            get
            {
                return fToWrap.CanWrite;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets the length in bytes of the
        ///     stream.
        /// </summary>
        /// <returns>
        ///     A long value representing the length of the stream in bytes.
        /// </returns>
        public override long Length
        {
            get
            {
                return fToWrap.Length;
            }
        }

        /// <summary>Sets the pos in block and advances the buffer</summary>
        /// <param name="posInBlock">The pos In Block.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void SetPosInBlock(long posInBlock)
        {
            Buffer.Position += posInBlock - PosInBlock;

                // the previous pos needs to be substracted, cause we aren't added to the value, we are replacing.
            PosInBlock = posInBlock;
        }

        /// <summary>When overridden in a derived class, sets the length of the current
        ///     stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            if (StartOfBuffer + BufferSize > value)
            {
                // if a resize makes the buffer outside of the file, make certain that it is reloaded
                fBufferLoaded = false;
            }

            if (fWritePos > value)
            {
                fWritePos = value;
            }

            fToWrap.SetLength(value);
        }

        /// <summary>When overridden in a derived class, sets the position within the
        ///     current stream.</summary>
        /// <param name="offset">A <see langword="byte"/> offset relative to the<paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="System.IO.SeekOrigin"/> indicating the reference
        ///     point used to obtain the new position.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            fBufferLoaded = false; // after a seek, always reload cause the actual pos has changed.
            var iRes = fToWrap.Seek(offset, origin);
            fWritePos = iRes; // position has changed so the write pos is also no longer valid.
            StartOfBuffer = iRes;
            return iRes;
        }

        /// <summary>Reads data into the specified <paramref name="buffer"/> and moves the
        ///     different cursor positions ahead.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var iRes = Buffer.Read(buffer, offset, count);
            PosInBlock += iRes;
            if (PosInBlock >= fBlockSize)
            {
                CurrentBlock++;
                PosInBlock = PosInBlock - fBlockSize;
            }

            return iRes;
        }

        /// <summary>
        ///     When overridden in a derived class, clears all buffers for this stream
        ///     and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            fToWrap.Flush();
        }

        /// <summary>When overridden in a derived class, writes a sequence of bytes to the
        ///     current stream and advances the current position within this stream by
        ///     the number of bytes written.</summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes
        ///     from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based <see langword="byte"/> offset in<paramref name="buffer"/> at which to begin copying bytes to the
        ///     current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            fToWrap.Write(buffer, offset, count);

                // the write is seperated from the read buffer. Before the write started, the pos was changed, specific for the writeprocess, which checked if the buffer remains valid or not.
            fWritePos += count;
        }

        /// <summary>
        ///     Reads a <see langword="byte" /> from the stream and advances the
        ///     position within the stream by one byte, or returns -1 if at the end of
        ///     the stream.
        /// </summary>
        /// <returns>
        ///     The unsigned <see langword="byte" /> cast to an Int32, or -1 if at the
        ///     end of the stream.
        /// </returns>
        public override int ReadByte()
        {
            var iRes = Buffer.ReadByte();
            if (iRes != -1)
            {
                // if ires == -1, we couldn't read anymore, so don't advance the cursor pos.
                PosInBlock++;
                if (PosInBlock >= fBlockSize)
                {
                    CurrentBlock++;
                    PosInBlock = PosInBlock - fBlockSize;
                }
            }

            return iRes;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="System.IO.Stream"/> and
        ///     optionally releases the managed resources.</summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        ///     resources; <see langword="false"/> to release only unmanaged
        ///     resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            fToWrap.Dispose();
        }

        #region fields

        /// <summary>The f to wrap.</summary>
        private readonly System.IO.Stream fToWrap; // the stream that we profide a buffer for.

        /// <summary>The f buffer.</summary>
        private readonly System.IO.MemoryStream fBuffer;

        /// <summary>The f buffer loaded.</summary>
        private bool fBufferLoaded;

        /// <summary>The f block size.</summary>
        private readonly int fBlockSize;

        /// <summary>The f write pos.</summary>
        private long fWritePos = -1;

        #endregion
    }
}