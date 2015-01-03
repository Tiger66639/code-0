// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DesignerStore.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the <see cref="NeuronData" /> objects on disk in a similar
//   manner as the core does: in block files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages all the <see cref="NeuronData" /> objects on disk in a similar
    ///     manner as the core does: in block files.
    /// </summary>
    public class DesignerStore : System.IDisposable
    {
        #region Fields

        /// <summary>The f tables.</summary>
        private readonly System.Collections.Generic.List<DesignerTable> fTables =
            new System.Collections.Generic.List<DesignerTable>();

                                                                        // contains all the file groups that store the neurons. We need multiple tables since 1 file can only be lon.maxValue in size, while we can have ulong.maxvalue nr of neurons (the index alone requires multiple file). 
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

        #endregion

        /// <summary>The clean for.</summary>
        /// <param name="data">The data.</param>
        internal void CleanFor(NeuronData data)
        {
            System.Diagnostics.Debug.Assert(IsLoaded);
            var iId = data.ID;
            var iTableIdx = (int)(iId % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = iId / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.CleanData((long)iNeuronIndex);
            data.IsChanged = true;
        }

        /// <summary>Cleans the db for the specified id. All data will be lost.</summary>
        /// <param name="id">The id.</param>
        internal void CleanFor(ulong id)
        {
            System.Diagnostics.Debug.Assert(IsLoaded);
            var iTableIdx = (int)(id % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = id / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.CleanData((long)iNeuronIndex);
        }

        #region Ctor/~

        /// <summary>Initializes a new instance of the <see cref="DesignerStore"/> class.</summary>
        /// <param name="nrTables">The nr tables.</param>
        public DesignerStore(int nrTables)
        {
            NrOfTables = nrTables;
        }

        /// <summary>Finalizes an instance of the <see cref="DesignerStore"/> class. </summary>
        ~DesignerStore()
        {
            Free();
        }

        /// <summary>
        ///     Closes the file handles.
        /// </summary>
        private void Free()
        {
            foreach (var i in fTables)
            {
                i.Dispose();
            }

            fTables.Clear();
        }

        #endregion

        #region Prop

        #region NrOfTables

        /// <summary>
        ///     Gets/sets the number of data files that are used. This property can
        ///     only be set 1 time.
        /// </summary>
        /// <remarks>
        ///     We only allow to set 1 time since, if there are neurons stored, and we
        ///     add a file, we can't find the correct neurons anymore, cause the
        ///     algorithm to select the correct table to request a neuron from uses
        ///     the nr of tables to calculate this.
        /// </remarks>
        public int NrOfTables { get; private set; }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets the wether the data files have been loaded or not.
        /// </summary>
        /// <remarks>
        ///     We are loaded, when there are items in the list because if there are
        ///     non, we can't save.
        /// </remarks>
        public bool IsLoaded
        {
            get
            {
                return fTables.Count > 0;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Loads all the files so they can be accessed.</summary>
        /// <param name="path">The path.</param>
        /// <param name="readOnly">The read Only.</param>
        public void LoadFiles(string path, bool readOnly = false)
        {
            Free();
            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    string iTableName;
                    for (var i = 1; i <= NrOfTables; i++)
                    {
                        iTableName = System.IO.Path.Combine(path, "NeuronData" + i);
                        var iTable = new DesignerTable(iTableName, readOnly);
                        fTables.Add(iTable);
                    }
                }
            }
            catch (System.Exception e)
            {
                Free();
                throw new System.InvalidOperationException("Failed to open designer database files", e);
            }
        }

        /// <summary>Tries to load the neuronData with the specified ID</summary>
        /// <remarks>If the <paramref name="id"/> can't be found <see langword="null"/>
        ///     should be returned.</remarks>
        /// <param name="id">The id of the neuronData to load.</param>
        /// <returns>The object that was loaded or <see langword="null"/> if not found.</returns>
        public NeuronData GetData(ulong id)
        {
            if (IsLoaded)
            {
                // if new project, can't yet be loaded, so can't get data.
                try
                {
                    var iTableIdx = (int)(id % (ulong)NrOfTables);
                    System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
                    var iTable = fTables[iTableIdx];
                    System.Diagnostics.Debug.Assert(iTable != null);
                    var iNeuronIndex = id / (ulong)NrOfTables;
                    System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                    NeuronData iRes;
                    lock (iTable)

                        // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                        iRes = iTable.GetData((long)iNeuronIndex);
                    if (iRes != null && iRes.IsReadError)
                    {
                        if (iRes.ID == Neuron.EmptyId)
                        {
                            // for some reason, the id got lost, but we could still have a name and or description, so try to save it.
                            iRes.ID = id;
                        }

                        CleanFor(iRes);
                        iRes.IsReadError = false;
                    }

                    return iRes;
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "DesignerStore.GetData", 
                        string.Format("Failed to retrieve designer data for {0}, error: {1}.", id, e));
                    CleanFor(id);

                        // remove all the data when there was something so bad that we couldn't read the entire thing.
                }
            }

            return null;
        }

        /// <summary>Tries to load the neuronData for the specified neuron</summary>
        /// <remarks>If the id can't be found <see langword="null"/> should be returned.</remarks>
        /// <param name="toLoad">The to Load.</param>
        /// <returns>The object that was loaded or <see langword="null"/> if not found.</returns>
        public NeuronData GetData(Neuron toLoad)
        {
            if (IsLoaded)
            {
                // if new project, can't yet be loaded, so can't get data.
                try
                {
                    var iTableIdx = (int)(toLoad.ID % (ulong)NrOfTables);
                    System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
                    var iTable = fTables[iTableIdx];
                    System.Diagnostics.Debug.Assert(iTable != null);
                    var iNeuronIndex = toLoad.ID / (ulong)NrOfTables;
                    System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                    NeuronData iRes;
                    lock (iTable)

                        // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                        iRes = iTable.GetData((long)iNeuronIndex, toLoad);
                    if (iRes != null && iRes.IsReadError)
                    {
                        if (iRes.ID == Neuron.EmptyId)
                        {
                            // for some reason, the id got lost, but we could still have a name and or description, so try to save it.
                            iRes = new NeuronData(toLoad);
                            iRes.IsReadError = true;
                        }

                        CleanFor(iRes);
                        iRes.IsReadError = false;
                    }

                    return iRes;
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "DesignerStore.GetData", 
                        string.Format("Failed to retrieve designer data for {0}, error: {1}.", toLoad.ID, e));
                    CleanFor(toLoad.ID);

                        // remove all the data when there was something so bad that we couldn't read the entire thing.
                }
            }

            return null;
        }

        /// <summary>a shortcut for only reading the title part of the designerdata. The
        ///     rest will be skipped as much as possible, no neuronData object gets
        ///     created.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        internal string GetDataTitle(ulong id)
        {
            if (IsLoaded)
            {
                // if new project, can't yet be loaded, so can't get data.
                try
                {
                    var iTableIdx = (int)(id % (ulong)NrOfTables);
                    System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
                    var iTable = fTables[iTableIdx];
                    System.Diagnostics.Debug.Assert(iTable != null);
                    var iNeuronIndex = id / (ulong)NrOfTables;
                    System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                    lock (iTable)

                        // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                        return iTable.GetDataTitle((long)iNeuronIndex);
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "DesignerStore.GetData", 
                        string.Format("Failed to retrieve designer data for {0}, error: {1}.", id, e));
                }
            }

            return null;
        }

        /// <summary>Determines whether the db contains the specified <paramref name="id"/>
        ///     without loading the data.</summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if the specified <paramref name="id"/> contains ID;
        ///     otherwise, <c>false</c> .</returns>
        internal bool ContainsID(ulong id)
        {
            if (IsLoaded)
            {
                // if new project, can't yet be loaded, so can't get data.
                try
                {
                    var iTableIdx = (int)(id % (ulong)NrOfTables);
                    System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
                    var iTable = fTables[iTableIdx];
                    System.Diagnostics.Debug.Assert(iTable != null);
                    var iNeuronIndex = id / (ulong)NrOfTables;
                    System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                    lock (iTable)

                        // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                        return iTable.ContainsData((long)iNeuronIndex);
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "DesignerStore.GetData", 
                        string.Format("Failed to check designer data for {0}, error: {1}.", id, e));
                }
            }

            return false;
        }

        /// <summary>Tries to save the <see cref="NeuronData"/> to the storage.</summary>
        /// <remarks>When implementing this function, it is not required to lock the entire
        ///     NeuronData, nor is it required to change<see cref="JaStDev.HAB.Designer.NeuronData.IsChanged"/> . This is all
        ///     done by the caller of this function.</remarks>
        /// <param name="toSave">The object to save.</param>
        public void SaveData(NeuronData toSave)
        {
            System.Diagnostics.Debug.Assert(IsLoaded);
            var iId = toSave.ID;
            var iTableIdx = (int)(iId % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = iId / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.SaveData((long)iNeuronIndex, toSave);
            toSave.IsChanged = false;

                // we do this here and not in NeuronData.SaveData, since this would cause a deadlock between the lock on the table and the flock in NeuronDataDictionay, if another thread wants to retrive some neurondata at the same time (ex: import all wordnet data + do a save).
        }

        /// <summary>
        ///     Flushes the file contents
        /// </summary>
        internal void Flush()
        {
            foreach (var i in fTables)
            {
                lock (i) i.Flush();
            }
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory,
        ///     if it is stored..</summary>
        /// <param name="toRemove">The object to remove</param>
        public void RemoveData(NeuronData toRemove)
        {
            RemoveData(toRemove.ID);
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory,
        ///     if it is stored.</summary>
        /// <param name="toRemove">The object to remove</param>
        public void RemoveData(ulong toRemove)
        {
            System.Diagnostics.Debug.Assert(IsLoaded);
            var iTableIdx = (int)(toRemove % (ulong)NrOfTables);
            if (fTables.Count > iTableIdx)
            {
                var iTable = fTables[iTableIdx];
                if (iTable != null)
                {
                    var iNeuronIndex = toRemove / (ulong)NrOfTables;
                    if (iNeuronIndex <= long.MaxValue)
                    {
                        // don't check if the table actually has the item, always ask for remove in a lock. This way, we can check inside the lock, if the item is actually stored or not. Is fastest.
                        lock (iTable)

                            // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                            iTable.RemoveData((long)iNeuronIndex);
                    }
                }
            }
        }

        #endregion
    }
}