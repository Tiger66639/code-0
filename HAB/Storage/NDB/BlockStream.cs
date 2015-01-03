// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockStream.cs" company="">
//   
// </copyright>
// <summary>
//   A stream class that saves data in blocks where 1 record can span
//   multiple, non consecutive blocks.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Storage
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A stream class that saves data in blocks where 1 record can span
    ///     multiple, non consecutive blocks.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The blockStream always wraps another stream that needs to be passed along
    ///         with the constructor. The
    ///         <see cref="JaStDev.HAB.Storage.BlockStream.Position" /> is used to align
    ///         with a block. The length returns the number of blocks.
    ///     </para>
    ///     <para>
    ///         A block sequence are a group of blocks that form 1 record. The end of
    ///         each block has a reference to the next block index. The last block points
    ///         to -1 to indicate the end of the sequence.
    ///     </para>
    /// </remarks>
    public class BlockStream : System.IO.Stream
    {
        /// <summary>
        ///     provides access to the buffer, so that the write position can be set.
        /// </summary>
        /// <value>
        ///     The buffer.
        /// </value>
        public NDBBuffer Buffer { get; private set; }

        /// <summary>
        ///     When overridden in a derived class, clears all buffers for this
        ///     stream and causes any buffered data to be written to the underlying
        ///     device.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Flush()
        {
            if (Settings.BufferFreeBlocksFiles)
            {
                // if the index files is buffered, make certain that the file is saved.
                System.IO.File.WriteAllBytes(FBFileName, ((System.IO.MemoryStream)fFreeBlocks).ToArray());

                    // need to use ToArray, can't use GetBuffer, cause the latter might contain empty values at the end (doesn't get truncated properly). ToArray does this correctly.
            }
            else
            {
                fFreeBlocks.Flush();
            }

            Buffer.Flush();
            fDataFile.Flush();

                // also flush the data file, to make certain all data is also written to disk, not just to the underlying stream.
        }

        /// <summary>
        ///     Flushes the data files. When the freeBlocks are loaded in mem, they
        ///     are only flushed to mem, not to disk. This is for real-time saving.
        /// </summary>
        public void FlushData()
        {
            fFreeBlocks.Flush();
            Buffer.Flush();
            fDataFile.Flush();

                // also flush the data file, to make certain all data is also written to disk, not just to the underlying stream.
        }

        /// <summary>
        ///     Closes the current stream and releases any resources (such as sockets
        ///     and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {
            base.Close();
            Buffer.Close();
            fDataFile.Close();
            fFreeBlocks.Close();
        }

        /// <summary>When overridden in a derived class, reads a sequence of bytes from the
        ///     current stream and advances the position within the stream by the
        ///     number of bytes read.</summary>
        /// <param name="buffer"><para>An array of bytes. When this method returns, the buffer contains the
        ///         specified <see langword="byte"/> array with the values between<paramref name="offset"/> and ( <paramref name="offset"/> +<paramref name="count"/></para>
        /// <list type="bullet"><item><description>
        ///                 1) replaced by the bytes read from the current source.
        ///             </description></item>
        /// </list>
        /// </param>
        /// <param name="offset">The zero-based <see langword="byte"/> offset in<paramref name="buffer"/> at which to begin storing the data read
        ///     from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <exception cref="System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is
        ///     larger than the <paramref name="buffer"/> length.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <returns>The total number of bytes read into the buffer. This can be less than
        ///     the number of bytes requested if that many bytes are not currently
        ///     available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var iSubPos = Buffer.PosInBlock; // the amount of bytes used in the current block.

            var iPos = Position; // the current block.
            var iRead = 0;
            var iDataSize = (int)(fBlockSize - sizeof(System.Int64) - iSubPos);

            while (count > 0)
            {
                if (count > iDataSize)
                {
                    // we need to do -sizeof(long), cause the last 8 bytes are the block number of the next block to go to.
                    iRead += Buffer.Read(buffer, offset, iDataSize);
                    offset += iDataSize;
                    count -= iDataSize;
                    iPos = fBufferReader.ReadInt64();
                    if (iPos > -1)
                    {
                        Position = iPos;
                        iDataSize = fBlockSize - sizeof(System.Int64);

                            // after we moved to a new block, recalculate the datasize of the new block
                    }
                    else
                    {
                        iDataSize = 0;
                        break;

                            // we have reached the end of the blockstream, so could be that we had to read just to the edge, could be invalid, don't know, let the caller deside.
                    }
                }
                else
                {
                    iRead += Buffer.Read(buffer, offset, count);
                    count = 0;

                        // we always set to 0 cause we were smaller than the blocksize, so the max bytes were read in 1 go, and we need to exit the loop.
                }
            }

            return iRead;
        }

        /// <summary>
        ///     Reads a <see langword="byte" /> from the stream and advances the
        ///     position within the stream by one byte, or returns -1 if at the end of
        ///     the stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     The stream does not support reading.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     Methods were called after the stream was closed.
        /// </exception>
        /// <returns>
        ///     The unsigned <see langword="byte" /> cast to an Int32, or -1 if at the
        ///     end of the stream.
        /// </returns>
        public override int ReadByte()
        {
            var iDataSize = (int)(fBlockSize - sizeof(System.Int64) - Buffer.PosInBlock);

                // the amount left in the current block.
            if (iDataSize == 0)
            {
                // reload a buffer if there is no more data to return.
                var iPos = fBufferReader.ReadInt64();
                if (iPos > -1)
                {
                    Position = iPos;
                }
                else
                {
                    return -1; // we have reached the end of the blockstream, so we can't read anymore bytes.
                }
            }

            return Buffer.ReadByte();
        }

        /// <summary>When overridden in a derived class, sets the position within the
        ///     current stream.</summary>
        /// <param name="offset">A <see langword="byte"/> offset relative to the<paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="System.IO.SeekOrigin"/> indicating the reference
        ///     point used to obtain the new position.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking, such as if the stream is
        ///     constructed from a pipe or console output.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            switch (origin)
            {
                case System.IO.SeekOrigin.Begin:
                    Position = offset;
                    break;
                case System.IO.SeekOrigin.Current:
                    Position += offset;
                    break;
                case System.IO.SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    break;
            }

            return Position;
        }

        /// <summary>When overridden in a derived class, sets the length of the current
        ///     stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support both writing and seeking, such as if the
        ///     stream is constructed from a pipe or console output.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void SetLength(long value)
        {
            Buffer.SetLength(value * fBlockSize); // this invalidates the buffer if required.
            if (fRecordPos > value)
            {
                fRecordPos = value;
            }
        }

        /// <summary>When overridden in a derived class, writes a sequence of bytes to the
        ///     current stream and advances the current position within this stream by
        ///     the number of bytes written.</summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes
        ///     from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based <see langword="byte"/> offset in<paramref name="buffer"/> at which to begin copying bytes to the
        ///     current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is
        ///     greater than the <paramref name="buffer"/> length.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var iWritePos = Buffer.WritePos;

            var iBlock = iWritePos / fBlockSize; // the current pos in wrapped
            var iSubPos = (int)(iWritePos % fBlockSize); // the current pos in the current block.
            var iDataSize = fBlockSize - sizeof(System.Int64) - iSubPos; // the nr of bytes that fit in 1 block

            // #if DEBUG
            // if(iPos != Position)
            // LogService.Log.LogError ("BlockStream","Buffer overrun");
            // #endif
            while (count > 0)
            {
                if (count > iDataSize)
                {
                    // we need to do -sizeof(long), cause the last 8 bytes are the block number of the next block to go to.
                    if (iDataSize > 0)
                    {
                        // only try to write if there was still room left. could be that the block was filled exactly till the end, in which case we start a new one. Thi sis important for android platform, otherwise we get exception when writing data size of 0.
                        Buffer.Write(buffer, offset, iDataSize);
                        offset += iDataSize;
                        count -= iDataSize;
                    }
                    else if (iDataSize < 0)
                    {
                        throw new System.Exception("Internal error: buffer overrun: " + iDataSize);
                    }

                    // iPos = -1;
                    // fWrapWriter.Write(iPos);                                                //not required, GetFreeBlock returns the next item anyway.  we need to write to get at the blockboundery before we do a call to GetFreeBlock, otherwise, we might get an invalid value when the length is returned (and we are not at blockboundery), which screws up the file completly.
                    // long iPrevPos = iBlock;
                    iBlock = GetFreeBlock();

                    // #if DEBUG
                    // if (iPos == iPrevPos)
                    // {
                    // LogService.Log.LogError("BlockStream", "Buffer overrun");
                    // iPos = GetFreeBlock();
                    // }
                    // #endif
                    // fToWrap.Position -= sizeof(long);                                       //go back, to write the pointer to the next block in the correct place: at the end of the block not at the start of the next one (where we are now).
                    fWrapWriter.Write(iBlock);
                    Buffer.WritePos = iBlock * fBlockSize;

                        // also need to go to the position so we can start writing again.
                    iDataSize = fBlockSize - sizeof(System.Int64);

                        // after the initial subPos was consumed, we can reset iDataSize
                }
                else
                {
                    Buffer.Write(buffer, offset, count);
                    count = 0;

                        // we always set to 0 cause we were smaller than the blocksize, so the max bytes were read in 1 go, and we need to exit the loop.		
                }
            }
        }

        /// <summary>
        ///     moves the underlying file pointer to the end of the block and writes a
        ///     -1, to indicate that the current block was the last in a sequence. If
        ///     we are at the end of the file, but not at a block end, we write a
        ///     series of 0's to fill the empty space, before a -1 is written.
        /// </summary>
        public void CloseCurrentBlock()
        {
            var iWritePos = Buffer.WritePos;
            var iPosInBlock = iWritePos % fBlockSize;
            var iEmtpyBytes = fBlockSize - sizeof(System.Int64) - iPosInBlock;

                // the nr of bytes that need to be added to the block 
            iPosInBlock = iWritePos + iEmtpyBytes; // find the end of the current block
            var iLength = Buffer.Length;
            if (iPosInBlock > iLength)
            {
                // check if the file is big enough to hold
                fWrapWriter.Write(fEmptyArray, 0, (System.Int32)iEmtpyBytes); // fill with 0 bytes
            }

            Buffer.WritePos = iPosInBlock;

                // move to the last pos in the block so we can write a -1 to indicate the end of the blocksequence (otherwise we wouldn't know where a sequence of blocks would end..
            fWrapWriter.Write((System.Int64)(-1));

            // we don't need to update the read position, this has already been taken care of, if needed due to the writepos changes. and writes.
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="System.IO.Stream"/> and
        ///     optionally releases the managed resources.</summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        ///     resources; <see langword="false"/> to release only unmanaged
        ///     resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Buffer.Dispose();
            fFreeBlocks.Dispose();
        }

        /// <summary>Gets a new free block. If there is non available, the length is
        ///     returned. This is a potential risc: when the length is returned, and
        ///     the file is not at a blockboundery, than you have a problem since it
        ///     will return an invalid value. So you need to make certain that you are
        ///     at a block boundery when requesting a new block.</summary>
        /// <returns>The <see cref="long"/>.</returns>
        private long GetFreeBlock()
        {
            while (true)
            {
                // we do a loop for as long as we find -1 and there are items in the free list.
                if (fFreeBlocks.Length > 0)
                {
                    var iNewLength = fFreeBlocks.Length - sizeof(System.Int64);
                    fFreeBlocks.Position = iNewLength;
                    var iRes = fFreeBLocksReader.ReadInt64();
                    fFreeBlocks.SetLength(iNewLength);
                    if (iRes > -1)
                    {
                        // -1 happens when there was an index in the free list that was actually used in an item that got cleaned and which reported the error in the index list.
                        return iRes;
                    }
                }
                else
                {
                    return Length;
                }
            }
        }

        /// <summary>Deletes the block sequence that starts at the specified index, and
        ///     moves the <paramref name="index"/> numbers to the freeblocks stream.</summary>
        /// <remarks>Removes the block sequence that starts on the specified pos. Deleted
        ///     blocks are moved to the FreeBlocks stream in such an order that they
        ///     would be reused in the same sequence as original.</remarks>
        /// <param name="index">The index.</param>
        public void Delete(long index)
        {
            fFreeBlocks.Position = fFreeBlocks.Length;
            InternalRemoveBlockSequence(index);
        }

        /// <summary>removes a block sequence without first positioning the freeblocks to
        ///     the end, use RemoveBlockSequence instead.</summary>
        /// <remarks>This was originally a recursive function, rewrote to none recursive,
        ///     to make certain that there couldn't be any stack overflows because of
        ///     an item with to many blocks.</remarks>
        /// <param name="pos">The pos.</param>
        private void InternalRemoveBlockSequence(long pos)
        {
            var iFrees = GetBlocksFrom(pos);
            foreach (var i in Enumerable.Reverse<long>(iFrees))
            {
                // we need to reverse the list so we can reuse it in the same order again (this is better for version control systems and the likes).
                fFreeBlocksWriter.Write(i);
            }
        }

        /// <summary>Gets all the blocks in a sequence, starting from the specified
        ///     position (without changing anything, except the<see langword="internal"/> position).</summary>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<long> GetBlocksFrom(long pos)
        {
            long iPosInBlock = fBlockSize - sizeof(System.Int64);
            var iBlocks = new System.Collections.Generic.List<long>();
            var iNextPos = pos;
            while (iNextPos > -1)
            {
                Position = iNextPos; // always adjust the position as well, so that things remain in sync.
                iBlocks.Add(iNextPos);
                Buffer.SetPosInBlock(iPosInBlock);
                iNextPos = fBufferReader.ReadInt64();
            }

            return iBlocks;
        }

        /// <summary>Frees the block sequence so that the specified index can be reused.
        ///     Starts the FreeForRewrite: the first block is only used to remove the
        ///     next blocks, it is not removed itself.</summary>
        /// <param name="pos">The pos.</param>
        public void FreeForRewrite(long pos)
        {
            fFreeBlocks.Position = fFreeBlocks.Length;
            long iPosInBlock = fBlockSize - sizeof(System.Int64);
            var iNextPos = (pos * fBlockSize) + iPosInBlock;
            if (Buffer.Length >= iNextPos + sizeof(System.Int64))
            {
                // if the previous write didn't completely succeed, or the file wasn't flushed properly, things can go wrong and the end of file wasn't written properly, so check for this, If we reach the end of the file, don't try to clean anymore.
                Position = pos;
                Buffer.SetPosInBlock(iPosInBlock);
                iNextPos = fBufferReader.ReadInt64();
                if (iNextPos > -1)
                {
                    InternalRemoveBlockSequence(iNextPos);
                }
            }

#if DEBUG
            else
            {
                throw new System.InvalidOperationException();
            }

#endif
        }

        /// <summary>Sets the position to a new empty block. This is always done to
        ///     performa write, so we assign the position to the buffer 'for writing'.</summary>
        /// <returns>The <see cref="long"/>.</returns>
        public long SetPosToEmptyBlock()
        {
            var iFound = GetFreeBlock();
            Buffer.WritePos = iFound * fBlockSize;
            return iFound;
        }

        /// <summary>Makes certain that non of the index numbers in the specified list are
        ///     currently recorded as 'free'.</summary>
        /// <param name="blocks">The blocks.</param>
        public void CleanFreeList(System.Collections.Generic.List<long> blocks)
        {
            fFreeBlocks.Position = 0;
            while (fFreeBlocks.Position < fFreeBlocks.Length)
            {
                var iIndex = fFreeBLocksReader.ReadInt64();
                if (blocks.Contains(iIndex))
                {
                    fFreeBlocks.Position -= sizeof(System.Int64);
                    fFreeBlocksWriter.Write((System.Int64)(-1));
                }
            }
        }

        /// <summary>
        ///     checks if all the items in the freeBlocks list are unique.
        /// </summary>
        public void VerifyFreeList()
        {
            var iItems = new System.Collections.Generic.HashSet<long>(); // keeps trakc of the items aleady loaded.
            fFreeBlocks.Position = 0;
            while (fFreeBlocks.Position < fFreeBlocks.Length)
            {
                var iIndex = fFreeBLocksReader.ReadInt64();
                if (iItems.Contains(iIndex))
                {
                    throw new System.InvalidOperationException("duplicates found");
                }

                iItems.Add(iIndex);
            }
        }

        /// <summary>Adds the specified list of <paramref name="blocks"/> to the queue of
        ///     free indexes.</summary>
        /// <remarks>This is usually called after a sequence of <paramref name="blocks"/>
        ///     got cleaned.</remarks>
        /// <param name="blocks">The blocks.</param>
        public void RegisterAsFree(System.Collections.Generic.List<long> blocks)
        {
            blocks.Sort();

                // sort the list so that the blocks follow each other. This gives most change that a freshly written neuron's blocks are close to each other, in order.
            fFreeBlocks.Position = fFreeBlocks.Length;
            for (var i = blocks.Count - 1; i <= 0; i--)
            {
                // write the free blocks in reverse order, so that when reading them out, the smallest is used first and the neuron can quickly be read again thanks to buffering.
                fFreeBlocksWriter.Write(blocks[i]);
            }
        }

        #region Fields

        /// <summary>The f data file.</summary>
        private readonly System.IO.FileStream fDataFile;

        /// <summary>The f block size.</summary>
        private readonly int fBlockSize;

        // BinaryReader fWrapReader;                                         //used to read the index pos to the next block directly from file (when this is the only value that needs to be read, otherwise the buffer is used).
        /// <summary>The f buffer reader.</summary>
        private readonly System.IO.BinaryReader fBufferReader; // used to read the index pos from the buffer

        /// <summary>The f wrap writer.</summary>
        private readonly System.IO.BinaryWriter fWrapWriter;

        /// <summary>The f record pos.</summary>
        private long fRecordPos;

        /// <summary>The f free blocks.</summary>
        private System.IO.Stream fFreeBlocks;

        /// <summary>The f free b locks reader.</summary>
        private System.IO.BinaryReader fFreeBLocksReader; // stores all the block nr that are free (have been deleted).

        /// <summary>The f free blocks writer.</summary>
        private System.IO.BinaryWriter fFreeBlocksWriter;

        /// <summary>The fb file name.</summary>
        private string FBFileName;

                       // when the fb stream is in mem, need to save it to file, need to know the filename, so store it here.

        /// <summary>The f empty array.</summary>
        private readonly byte[] fEmptyArray; // this is inititialized to 0,used to write empty space to file

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BlockStream"/> class.</summary>
        /// <param name="fileName">Name of the file to open and which stores records in a blocked
        ///     fashion.</param>
        /// <param name="fbFileName">Name of the file that contains all the free block indexes. This file
        ///     will also be opened.</param>
        /// <param name="blockSize">Size of a block.</param>
        /// <param name="readOnly">The read Only.</param>
        public BlockStream(string fileName, string fbFileName, int blockSize, bool readOnly)
        {
            System.Diagnostics.Debug.Assert(blockSize > 0);
            fEmptyArray = new byte[blockSize * 2];

                // this is used to write empty spaces to the file. We create it here, so it only gets created 1 time.
            System.Array.Clear(fEmptyArray, 0, blockSize * 2); // make certain it is cleaned.
            System.Diagnostics.Debug.Assert(fBlockSize <= int.MaxValue);

                // this is required cause in the Read routine, we cast fblockSize to an int.
            if (readOnly == false)
            {
                fDataFile = new System.IO.FileStream(
                    fileName, 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.ReadWrite, 
                    System.IO.FileShare.None);
                Buffer = new NDBBuffer(fDataFile, blockSize); // new BufferedStream(fDataFile, Settings.DBBufferSize);
                fWrapWriter = new System.IO.BinaryWriter(Buffer);
            }
            else
            {
                fDataFile = new System.IO.FileStream(
                    fileName, 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.Read, 
                    System.IO.FileShare.Read);
                Buffer = new NDBBuffer(fDataFile, blockSize); // new BufferedStream(fDataFile, Settings.DBBufferSize);
            }

            fBufferReader = new System.IO.BinaryReader(Buffer);
            fBlockSize = blockSize;
            LoadFreeBlocks(readOnly, fbFileName);
        }

        /// <summary>The load free blocks.</summary>
        /// <param name="readOnly">The read only.</param>
        /// <param name="fbFileName">The fb file name.</param>
        private void LoadFreeBlocks(bool readOnly, string fbFileName)
        {
            if (readOnly == false)
            {
                if (Settings.BufferFreeBlocksFiles == false)
                {
                    fFreeBlocks = new System.IO.FileStream(
                        fbFileName, 
                        System.IO.FileMode.OpenOrCreate, 
                        System.IO.FileAccess.ReadWrite, 
                        System.IO.FileShare.None);
                }
                else
                {
                    if (System.IO.File.Exists(fbFileName))
                    {
                        var iData = System.IO.File.ReadAllBytes(fbFileName);
                        fFreeBlocks = new System.IO.MemoryStream(iData.Length);

                            // memoryStream created with byteArray as arg are non writeable in android.
                        fFreeBlocks.Write(iData, 0, iData.Length);
                    }
                    else
                    {
                        fFreeBlocks = new System.IO.MemoryStream();
                    }

                    FBFileName = fbFileName;
                }

                fFreeBlocksWriter = new System.IO.BinaryWriter(fFreeBlocks);
            }
            else
            {
                fFreeBlocks = new System.IO.FileStream(
                    fbFileName, 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.ReadWrite, 
                    System.IO.FileShare.Read);
            }

            fFreeBLocksReader = new System.IO.BinaryReader(fFreeBlocks);
            fFreeBlocks.Position = fFreeBlocks.Length;
        }

        #endregion

        #region Prop

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports reading.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if the stream supports reading; otherwise,
        ///     false.
        /// </returns>
        public override bool CanRead
        {
            get
            {
                return fDataFile.CanRead;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports seeking.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if the stream supports seeking; otherwise,
        ///     false.
        /// </returns>
        public override bool CanSeek
        {
            get
            {
                return fDataFile.CanSeek;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether
        ///     the current stream supports writing.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if the stream supports writing; otherwise,
        ///     false.
        /// </returns>
        public override bool CanWrite
        {
            get
            {
                return fDataFile.CanWrite;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets the length in bytes of the
        ///     stream.
        /// </summary>
        /// <value>
        /// </value>
        /// <exception cref="System.NotSupportedException">
        ///     A class derived from Stream does not support seeking.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     Methods were called after the stream was closed.
        /// </exception>
        /// <returns>
        ///     A long value representing the length of the stream in bytes.
        /// </returns>
        public override long Length
        {
            get
            {
                var iToWrapLength = fDataFile.Length;
                var iVal = iToWrapLength / fBlockSize;
                if (iVal * fBlockSize == iToWrapLength)
                {
                    return iVal;
                }

                return iVal + 1;
            }
        }

        /// <summary>
        ///     When overridden in a derived class, gets or sets the position within
        ///     the current stream.
        /// </summary>
        /// <value>
        /// </value>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">
        ///     The stream does not support seeking.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     Methods were called after the stream was closed.
        /// </exception>
        /// <returns>
        ///     The current position within the stream.
        /// </returns>
        public override long Position
        {
            get
            {
                return fRecordPos;
            }

            set
            {
                // Debug.WriteLine ("Set record pos: " + value.ToString ());	
                if (value != fRecordPos)
                {
                    fRecordPos = value;
                    Buffer.Position = value * fBlockSize;
                }
                else
                {
                    Buffer.SetPosInBlock(0);

                        // when the pos was changed, make certain that the readbuffer is also at the start again, otherwise we get out of sync (it could have been read before, but when the pos is chagned, we always want toread from the start of the buffer.
                }
            }
        }

        #endregion
    }
}