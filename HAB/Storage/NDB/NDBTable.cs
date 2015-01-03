// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBTable.cs" company="">
//   
// </copyright>
// <summary>
//   A logical unit of consecutive neurons that are stored together in a
//   single file group.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Storage.NDB
{
    /// <summary>
    ///     A logical unit of consecutive neurons that are stored together in a
    ///     single file group.
    /// </summary>
    internal class NDBTable : System.IDisposable
    {
        #region const

        /// <summary>
        ///     The size of a single block in the blockstream. This should be able to
        ///     contain at least an empty neuron, possibly with a few links.
        /// </summary>
        public static int BlockSize = 101;

        #endregion

        #region fields

        /// <summary>The f neurons file.</summary>
        private BlockStream fNeuronsFile;

        /// <summary>The f index file.</summary>
        private System.IO.Stream fIndexFile;

        /// <summary>The f index writer.</summary>
        private System.IO.BinaryWriter fIndexWriter;

        /// <summary>The f index reader.</summary>
        private System.IO.BinaryReader fIndexReader;

        /// <summary>The f data reader.</summary>
        private readonly CompactBinaryReader fDataReader;

        /// <summary>The f data writer.</summary>
        private readonly CompactBinaryWriter fDataWriter;

        /// <summary>The f table name.</summary>
        private readonly string fTableName;

        /// <summary>
        ///     Used to serialize the neurons, is able to store/retrieve the type of
        ///     the neurons again. This is a shared resource with all the tables,
        ///     since it knows the id's of the neurontypes, used to recreate the
        ///     objects during deserialization.
        /// </summary>
        private readonly NDBSerializer fSerializer;

        #endregion

        #region ctor/~

        /// <summary>Initializes a new instance of the <see cref="NDBTable"/> class.</summary>
        /// <param name="tableName">Name of the table. This should include the path and the start of the
        ///     filenames in the group, but no extention.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="readOnly">The read Only.</param>
        public NDBTable(string tableName, NDBSerializer serializer, bool readOnly)
        {
            System.Diagnostics.Debug.Assert(serializer != null);
            fSerializer = serializer;
            fTableName = tableName;
            fNeuronsFile = new BlockStream(tableName + ".dat", tableName + ".fb", BlockSize, readOnly);
            fDataReader = new CompactBinaryReader(fNeuronsFile);
            if (readOnly == false)
            {
                fDataWriter = new CompactBinaryWriter(fNeuronsFile);
            }

            LoadIndexFile(readOnly);
        }

        /// <summary>The load index file.</summary>
        /// <param name="readOnly">The read only.</param>
        private void LoadIndexFile(bool readOnly)
        {
            if (Settings.BufferIndexFiles)
            {
                var iFileName = fTableName + ".idx";
                if (System.IO.File.Exists(iFileName))
                {
                    var iData = System.IO.File.ReadAllBytes(iFileName);
                    fIndexFile = new System.IO.MemoryStream(iData.Length);

                        // memoryStream created with byteArray as arg are non writeable in android.
                    fIndexFile.Write(iData, 0, iData.Length);
                }
                else
                {
                    fIndexFile = new System.IO.MemoryStream();
                }
            }
            else if (readOnly == false)
            {
                fIndexFile = new System.IO.FileStream(
                    fTableName + ".idx", 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.ReadWrite, 
                    System.IO.FileShare.None);
            }
            else
            {
                fIndexFile = new System.IO.FileStream(
                    fTableName + ".idx", 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.Read, 
                    System.IO.FileShare.Read);
            }

            fIndexReader = new System.IO.BinaryReader(fIndexFile);
            if (readOnly == false)
            {
                fIndexWriter = new System.IO.BinaryWriter(fIndexFile);
            }
        }

        /// <summary>Finalizes an instance of the <see cref="NDBTable"/> class. </summary>
        ~NDBTable()
        {
            Free();
        }

        #endregion

        #region Functions

        /// <summary>Gets the neuron at the specified index.</summary>
        /// <remarks>The <paramref name="index"/> pos determins the pos in the file where
        ///     to read (with a record being 8 bytes).</remarks>
        /// <param name="index">The index.</param>
        /// <returns>The neuron, or <see langword="null"/> if not found.</returns>
        internal LinkResolverData GetNeuron(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                if (iFilePos > -1)
                {
                    fNeuronsFile.Position = iFilePos;
                    return fSerializer.Deserialize(fDataReader);
                }

                if (Settings.LogNeuronNotFoundInLongTermMem)
                {
                    LogService.Log.LogWarning(
                        "NDBTable.GetNeuron", 
                        "Neuron was deleted, can't retrieve neuron from storage.");
                }
            }
            else if (Settings.LogNeuronNotFoundInLongTermMem)
            {
                LogService.Log.LogWarning(
                    "NDBTable.GetNeuron", 
                    "Index out of range, can't retrieve neuron from storage.");
            }

            return null; // when we got here, there was a problem.
        }

        /// <summary>Saves the neuron to the specified <paramref name="index"/> position.
        ///     If there already is a neuron saved, it is first removed (the blocks
        ///     are freed).</summary>
        /// <param name="index">The index.</param>
        /// <param name="toSave">To save.</param>
        internal void SaveNeuron(long index, Neuron toSave)
        {
            var iIndex = index * sizeof(long);
            long iFilePos = -1;
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                iFilePos = fIndexReader.ReadInt64();
                if (iFilePos >= 0)
                {
                    // if the neuron had already been saved, first free it's previous blocks so that we can reuse them (otherwise, we would get large 'phantom' data blobs inside the stream that aren't referenced by any neuron.
                    fNeuronsFile.FreeForRewrite(iFilePos);

                        // we use FreeForRewrite and not Delete cause this also removes the first block, which we want to reuse.
                }
            }

            if (iFilePos == -1)
            {
                iFilePos = fNeuronsFile.SetPosToEmptyBlock();

                    // get a new empty block to store in the index file and position the NeuronsFile at this new empty block.
                if (fIndexFile.Length < iIndex)
                {
                    fIndexFile.Position = fIndexFile.Length;

                        // move to end and fill with -1, to indicate that all the previous index positions haven't been assigned a block yet.
                    while (fIndexFile.Length < iIndex)
                    {
                        fIndexWriter.Write((long)-1);
                    }
                }

                fIndexFile.Position = iIndex;
                fIndexWriter.Write(iFilePos);
            }
            else
            {
                fNeuronsFile.Buffer.WritePos = iFilePos * BlockSize;
            }

            fSerializer.Serialize(fDataWriter, toSave);
            fNeuronsFile.CloseCurrentBlock();
        }

        /// <summary>Removes the neuron at the specified index.</summary>
        /// <param name="index">The index into the index file which determins the first block of the
        ///     neuron to remove.</param>
        internal void RemoveNeuron(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length > iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                if (iFilePos >= 0)
                {
                    // don't try to delete, if there is nothing to delete.
                    fNeuronsFile.Delete(iFilePos);
                    fIndexFile.Position = iIndex;
                    fIndexWriter.Write((long)-1);

                        // we need to store that the neuron is removed. Can't use 0 cause that's the first block.
                }
            }

            // we don't throw an exception if we try to remove a neuron that isn't stored yet in the db. If the id had been recycled.
            // the neuron  would have been removed from the removal list.
        }

        /// <summary>Cleans the table for the specified <paramref name="index"/> position.
        ///     This is done by removing all the data blocks that are used by other
        ///     data items. All other data blocks are freed (the same as a delete
        ///     operation). This means that the object needs to be saved again after
        ///     the clean. Also makes certain that any of the blocks in the sequence
        ///     aren't in the 'free' list multiple times.</summary>
        /// <param name="index">The index.</param>
        internal void CleanData(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length > iIndex + sizeof(long))
            {
                // no exception:this is fasted to check if the index can be removed.
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                if (iFilePos >= 0)
                {
                    // don't try to delete, if there is nothing to delete.
                    var iBlocks = fNeuronsFile.GetBlocksFrom(iFilePos);
                    fNeuronsFile.CleanFreeList(iBlocks);
                    fIndexFile.Position = iIndex;
                    fIndexWriter.Write((long)-1);

                        // we need to store that the neuron is removed. Can't use 0 cause that's the first block.
                    VerifyFileAgainst(iBlocks);
                    fNeuronsFile.RegisterAsFree(iBlocks);
                }
            }
        }

        /// <summary>Removes all the indexes from the list that are already used by another
        ///     sequence in hte file. This is used to clean an object that somehow got
        ///     broken and which might be referencing <paramref name="blocks"/> of
        ///     other objects.</summary>
        /// <remarks>walks through the entire index file and fetches the<paramref name="blocks"/> for each object that is stored in the file.</remarks>
        /// <param name="blocks">The blocks.</param>
        private void VerifyFileAgainst(System.Collections.Generic.List<long> blocks)
        {
            fIndexFile.Position = 0;
            while (fIndexFile.Position < fIndexFile.Length)
            {
                var iStart = fIndexReader.ReadInt64();
                if (iStart > -1)
                {
                    var iCompareTo = fNeuronsFile.GetBlocksFrom(iStart);
                    foreach (var i in iCompareTo)
                    {
                        blocks.Remove(i); // simply try to reomve it is enough.
                    }
                }
            }
        }

        /// <summary>Marks the position at the specified <paramref name="index"/> as free.
        ///     This is used when 2 id's point to the same physical db location
        ///     (neuron).</summary>
        /// <param name="index">The index.</param>
        internal void MarkAsFree(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length > iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                fIndexWriter.Write((long)-1);

                    // we need to store that the neuron is removed. Can't use 0 cause that's the first block.
            }
        }

        /// <summary>Determines whether the id exists.</summary>
        /// <remarks>When there is a valid file pointer in the <paramref name="index"/>
        ///     file, the neuron exists, otherwise it doesn't.</remarks>
        /// <param name="index">The index.</param>
        /// <returns><c>true</c> if [is existing ID] [the specified index]; otherwise,<c>false</c> .</returns>
        internal bool IsExistingID(long index)
        {
            var iIndex = index * sizeof(ulong);
            if (fIndexFile.Length > 0 && fIndexFile.Length >= iIndex + sizeof(long))
            {
                // always need to add sizeof(long) cause we need to read 1 more item.
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                return iFilePos >= 0; // pos 0 is the first, this is also allowed.
            }

            return false;
        }

        /// <summary>
        ///     Flushes the files
        /// </summary>
        internal void Flush()
        {
            if (Settings.BufferIndexFiles)
            {
                // if the index files is buffered, make certain that the file is saved.
                using (var iStream = System.IO.File.OpenWrite(fTableName + ".idx"))
                    iStream.Write(((System.IO.MemoryStream)fIndexFile).GetBuffer(), 0, (int)fIndexFile.Length);

                        // write this way. GetBuffer() can have a bunch of 0 at the end (cause it's longer then 'length'), don't save them, they can cause problems.
            }
            else
            {
                fIndexWriter.Flush();
            }

            fNeuronsFile.Flush();
        }

        /// <summary>
        ///     only flushes the absolute minimum to garantee proper operation in
        ///     real-time usage situations.
        /// </summary>
        internal void FlushData()
        {
            // if (Settings.BufferIndexFiles == false)                                                      
            fIndexWriter.Flush();
            fNeuronsFile.FlushData();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Free();
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Closes the file handles.
        /// </summary>
        private void Free()
        {
            if (fNeuronsFile != null)
            {
                fNeuronsFile.Close();
                fNeuronsFile.Dispose();
                fNeuronsFile = null;
            }

            if (fIndexFile != null)
            {
                fIndexFile.Close();
                fIndexFile.Dispose();
                fIndexFile = null;
            }
        }

        #endregion
    }
}