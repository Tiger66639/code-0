// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DesignerTable.cs" company="">
//   
// </copyright>
// <summary>
//   Manages an index and data file group containd <see cref="NeuronData" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages an index and data file group containd <see cref="NeuronData" /> objects.
    /// </summary>
    internal class DesignerTable : System.IDisposable
    {
        #region const

        /// <summary>
        ///     The size of a single block in the blockstream. This should be able to contain at least an empty neuron, possibly
        ///     with
        ///     a few links.
        /// </summary>
        private const int BLOCKSIZE = 40;

        #endregion

        /// <summary>Cleans the table for the specified index position. This is done by
        ///     removing all the data blocks that are used by other data items. All
        ///     other data blocks are freed (the same as a delete operation). This means
        ///     that the object needs to be saved again after the clean.
        ///     Also makes certain that any of the blocks in the sequence aren't in the
        ///     'free' list multiple times.</summary>
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

        /// <summary>Removes all the indexes from the list that are already used by another sequence in hte
        ///     file. This is used to clean an object that somehow got broken and which might be
        ///     referencing blocks of other objects.</summary>
        /// <remarks>walks through the entire index file and fetches the blocks for each object that is stored
        ///     in the file.</remarks>
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

        /// <summary>
        ///     Flushes this instance.
        /// </summary>
        internal void Flush()
        {
            fIndexFile.Flush();
            fNeuronsFile.Flush();
        }

        #region fields

        /// <summary>The f neurons file.</summary>
        private Storage.BlockStream fNeuronsFile;

        /// <summary>The f data reader.</summary>
        private readonly System.IO.BinaryReader fDataReader;

        /// <summary>The f data writer.</summary>
        private readonly System.IO.BinaryWriter fDataWriter;

        /// <summary>The f index file.</summary>
        private System.IO.FileStream fIndexFile;

        /// <summary>The f index writer.</summary>
        private readonly System.IO.BinaryWriter fIndexWriter;

        /// <summary>The f index reader.</summary>
        private readonly System.IO.BinaryReader fIndexReader;

        #endregion

        #region ctor/~

        /// <summary>Initializes a new instance of the <see cref="DesignerTable"/> class. Initializes a new instance of the <see cref="NDBTable"/> class.</summary>
        /// <param name="tableName">Name of the table. This should include the path and the start of the filenames in the group, but
        ///     no extention.</param>
        /// <param name="readOnly">The read Only.</param>
        public DesignerTable(string tableName, bool readOnly)
        {
            fNeuronsFile = new Storage.BlockStream(tableName + ".dat", tableName + ".fb", BLOCKSIZE, readOnly);
            fDataReader = new System.IO.BinaryReader(fNeuronsFile);
            if (readOnly == false)
            {
                fDataWriter = new System.IO.BinaryWriter(fNeuronsFile);
                fIndexFile = new System.IO.FileStream(
                    tableName + ".idx", 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.ReadWrite, 
                    System.IO.FileShare.None);
                fIndexWriter = new System.IO.BinaryWriter(fIndexFile);
            }
            else
            {
                fIndexFile = new System.IO.FileStream(
                    tableName + ".idx", 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.Read, 
                    System.IO.FileShare.Read);
            }

            fIndexReader = new System.IO.BinaryReader(fIndexFile);
        }

        /// <summary>Finalizes an instance of the <see cref="DesignerTable"/> class. </summary>
        ~DesignerTable()
        {
            Free();
        }

        #endregion

        #region Functions

        /// <summary>Gets the NeuronData at the specified index.</summary>
        /// <remarks>The index pos determins the pos in the file where to read (with a record being 8 bytes).</remarks>
        /// <param name="index">The index.</param>
        /// <param name="toLoad">The to Load.</param>
        /// <returns>The neuron, or null if not found.</returns>
        internal NeuronData GetData(long index, Neuron toLoad = null)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                if (iFilePos > -1)
                {
                    fNeuronsFile.Position = iFilePos;
                    var iData = new NeuronData(toLoad);
                    iData.Read(fDataReader);
                    return iData;
                }

                if (Settings.LogNeuronNotFoundInLongTermMem)
                {
                    LogService.Log.LogWarning(
                        "DesignerTable.GetData", 
                        "NeuronData was deleted, can't retrieve neuron from storage.");
                }
            }
            else if (Settings.LogNeuronNotFoundInLongTermMem)
            {
                LogService.Log.LogWarning(
                    "DesignerTable.GetData", 
                    "Index out of range, can't retrieve NeuronData from storage.");
            }

            return null; // when we got here, there was a problem.
        }

        /// <summary>The get data title.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="string"/>.</returns>
        internal string GetDataTitle(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                if (iFilePos > -1)
                {
                    fNeuronsFile.Position = iFilePos;
                    return NeuronData.ReadDataTitle(fDataReader);
                }

                if (Settings.LogNeuronNotFoundInLongTermMem)
                {
                    LogService.Log.LogWarning(
                        "DesignerTable.GetData", 
                        "NeuronData was deleted, can't retrieve neuron from storage.");
                }
            }
            else if (Settings.LogNeuronNotFoundInLongTermMem)
            {
                LogService.Log.LogWarning(
                    "DesignerTable.GetData", 
                    "Index out of range, can't retrieve NeuronData from storage.");
            }

            return null; // when we got here, there was a problem.
        }

        /// <summary>Saves the NeuronData to the specified index position. If there already is a NeuronData saved, it is first
        ///     removed (the blocks are freed).</summary>
        /// <param name="index">The index.</param>
        /// <param name="toSave">To save.</param>
        internal void SaveData(long index, NeuronData toSave)
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
                fNeuronsFile.Buffer.WritePos = iFilePos * BLOCKSIZE;
            }

            toSave.Write(fDataWriter);
            fNeuronsFile.CloseCurrentBlock();
        }

        /// <summary>Gets the NeuronData at the specified index.</summary>
        /// <remarks>The index pos determins the pos in the file where to read (with a record being 8 bytes).</remarks>
        /// <param name="index">The index.</param>
        /// <returns>The neuron, or null if not found.</returns>
        internal bool ContainsData(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                fIndexFile.Position = iIndex;
                var iFilePos = fIndexReader.ReadInt64();
                return iFilePos > -1;
            }

            return false;
        }

        /// <summary>Removes the neuron at the specified index.</summary>
        /// <param name="index">The index into the index file which determins the first block of the neuron to remove.</param>
        internal void RemoveData(long index)
        {
            var iIndex = index * sizeof(long);
            if (fIndexFile.Length >= iIndex + sizeof(long))
            {
                // no exception:this is fasted to check if the index can be removed.
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
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
                fNeuronsFile.Close(); // close first, otherwise it can remain in memory a little longer.
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