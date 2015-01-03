// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VarManager.cs" company="">
//   
// </copyright>
// <summary>
//   provides services to manage a set of locals, ints and doubles that can be
//   shared accross multiple modules. This is shared accross compilers, so it
//   is thread save. This data is atached to the project.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides services to manage a set of locals, ints and doubles that can be
    ///     shared accross multiple modules. This is shared accross compilers, so it
    ///     is thread save. This data is atached to the project.
    /// </summary>
    public class VarManager
    {
        /// <summary>The f bool res var.</summary>
        private ulong fBoolResVar;

        /// <summary>The f doubles.</summary>
        private System.Collections.Generic.Dictionary<double, ulong> fDoubles;

                                                                     // stores all the statics that were created for this parse so that they can be reused.

        /// <summary>The f doubles lock.</summary>
        private object fDoublesLock = new object();

        /// <summary>The f ints.</summary>
        private System.Collections.Generic.Dictionary<int, ulong> fInts;

                                                                  // stores all the statics that were created for this parse so that they can be reused.

        /// <summary>The f ints lock.</summary>
        private object fIntsLock = new object();

        /// <summary>The f is changed.</summary>
        private bool fIsChanged; // so we know when something needs to be saved or not.

        /// <summary>The f locals.</summary>
        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ulong>> fLocals;

                                                                                                      // stores all the local vars that were created for this module, so they can be reused.

        /// <summary>The f locals lock.</summary>
        private object fLocalsLock = new object();

        #region Locals

        /// <summary>
        ///     Gets the locals
        /// </summary>
        internal System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ulong>> Locals
        {
            get
            {
                if (fLocals == null)
                {
                    LoadData();
                }

                return fLocals;
            }
        }

        #endregion

        #region Ints

        /// <summary>
        ///     Gets the ints
        /// </summary>
        internal System.Collections.Generic.Dictionary<int, ulong> Ints
        {
            get
            {
                if (fInts == null)
                {
                    LoadData();
                }

                return fInts;
            }
        }

        #endregion

        #region Doubles

        /// <summary>
        ///     Gets the boubles
        /// </summary>
        internal System.Collections.Generic.Dictionary<double, ulong> Doubles
        {
            get
            {
                if (fDoubles == null)
                {
                    LoadData();
                }

                return fDoubles;
            }
        }

        #endregion

        #region BoolResVar

        /// <summary>
        ///     Gets/sets the id of the variable that is used by the
        ///     <see cref="BoolExpression" /> as a result store for any calculations it
        ///     had to do.
        /// </summary>
        public ulong BoolResVar
        {
            get
            {
                return fBoolResVar;
            }

            set
            {
                fBoolResVar = value;
                fIsChanged = true;
            }
        }

        #endregion

        /// <summary>
        ///     Loads the data.
        /// </summary>
        private void LoadData()
        {
            fIsChanged = false;
            fInts = new System.Collections.Generic.Dictionary<int, ulong>();
            fDoubles = new System.Collections.Generic.Dictionary<double, ulong>();
            fLocals = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ulong>>();

            string iFile;
            if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
            {
                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, "common.obj");
            }
            else
            {
                iFile = null;
            }

            if (System.IO.File.Exists(iFile))
            {
                using (var iStream = new System.IO.FileStream(iFile, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                    )
                {
                    var iBuffer = new System.IO.BufferedStream(iStream, Settings.DBBufferSize);
                    var iReader = new System.IO.BinaryReader(iBuffer);
                    ReadInts(iReader);
                    ReadDoubles(iReader);
                    ReadLocals(iReader);
                    if (iBuffer.Position < iBuffer.Length)
                    {
                        // older versions didn't yet support the boolResVar 
                        fBoolResVar = iReader.ReadUInt64();
                    }
                }
            }
        }

        /// <summary>The read locals.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadLocals(System.IO.BinaryReader reader)
        {
            var iLocals = Locals;
            var iCount = reader.ReadInt32();
            while (iCount > 0)
            {
                var iName = reader.ReadString();
                var iNrItems = reader.ReadInt32();
                var iItems = new System.Collections.Generic.List<ulong>();
                while (iNrItems > 0)
                {
                    iItems.Add(reader.ReadUInt64());
                    iNrItems--;
                }

                iLocals.Add(iName, iItems);
                iCount--;
            }
        }

        /// <summary>The read doubles.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadDoubles(System.IO.BinaryReader reader)
        {
            var iDoubles = Doubles;
            var iCount = reader.ReadInt32();
            while (iCount > 0)
            {
                var iVal = reader.ReadDouble();
                var iId = reader.ReadUInt64();
                iDoubles.Add(iVal, iId);
                iCount--;
            }
        }

        /// <summary>The read ints.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadInts(System.IO.BinaryReader reader)
        {
            var iInts = Ints;
            var iCount = reader.ReadInt32();
            while (iCount > 0)
            {
                var iVal = reader.ReadInt64();
                var iId = reader.ReadUInt64();
                iInts.Add((int)iVal, iId);
                iCount--;
            }
        }

        /// <summary>The flush.</summary>
        internal void Flush()
        {
            if (fIsChanged)
            {
                fIsChanged = false;
                string iFile;
                if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                {
                    iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, "common.obj");
                }
                else
                {
                    iFile = null;
                }

                if (string.IsNullOrEmpty(iFile) == false)
                {
                    if ((fInts == null || fInts.Count == 0) && (fDoubles == null || fInts.Count == 0)
                        && (fLocals == null || fLocals.Count == 0))
                    {
                        // if no data, don' try to save anything.
                        if (System.IO.File.Exists(iFile))
                        {
                            // delete the file if it was still there
                            System.IO.File.Delete(iFile);
                        }
                    }
                    else
                    {
                        using (
                            var iStr = new System.IO.FileStream(
                                iFile, 
                                System.IO.FileMode.Create, 
                                System.IO.FileAccess.ReadWrite))
                        {
                            var iWriter = new System.IO.BinaryWriter(iStr);
                            WriteInts(iWriter);
                            WriteDoubles(iWriter);
                            WriteLocals(iWriter);
                            iWriter.Write(fBoolResVar);
                        }
                    }
                }
            }
        }

        /// <summary>The write ints.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteInts(System.IO.BinaryWriter writer)
        {
            if (fInts != null)
            {
                writer.Write(fInts.Count);
                foreach (var i in fInts)
                {
                    writer.Write((System.Int64)i.Key);
                    writer.Write(i.Value);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        /// <summary>The write doubles.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteDoubles(System.IO.BinaryWriter writer)
        {
            if (fDoubles != null)
            {
                writer.Write(fDoubles.Count);
                foreach (var i in fDoubles)
                {
                    writer.Write(i.Key);
                    writer.Write(i.Value);
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        /// <summary>The write locals.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteLocals(System.IO.BinaryWriter writer)
        {
            if (fLocals != null)
            {
                writer.Write(fLocals.Count);
                foreach (var i in fLocals)
                {
                    writer.Write(i.Key);
                    writer.Write(i.Value.Count);
                    foreach (var u in i.Value)
                    {
                        writer.Write(u);
                    }
                }
            }
            else
            {
                writer.Write(0);
            }
        }

        /// <summary>
        ///     Touches the mem. Make certain that all the data is loaded in mem.
        /// </summary>
        internal void TouchMem()
        {
            if (fDoubles == null)
            {
                LoadData();
            }

            fIsChanged = true;
        }

        /// <summary>retrieves a neuron that represents the specified double.</summary>
        /// <param name="p"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron GetDouble(double p)
        {
            var iDoubles = Doubles;
            lock (iDoubles)
            {
                DoubleNeuron iRes;
                ulong iFound;
                if (iDoubles.TryGetValue(p, out iFound) == false)
                {
                    iRes = NeuronFactory.GetDouble(p);
                    Brain.Current.Add(iRes);
                    iDoubles.Add(p, iRes.ID);
                    fIsChanged = true;
                }
                else
                {
                    iRes = Brain.Current[iFound] as DoubleNeuron;
                }

                if (iRes == null)
                {
                    throw new System.InvalidOperationException("failed to find a double at " + iFound);
                }

                return iRes;
            }
        }

        /// <summary>gets a neuron that represents the specified <see langword="int"/>
        ///     value.</summary>
        /// <param name="p"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public Neuron GetInt(int p)
        {
            var iInts = Ints;
            lock (iInts)
            {
                ulong iFound;
                IntNeuron iRes;
                if (iInts.TryGetValue(p, out iFound) == false)
                {
                    iRes = NeuronFactory.GetInt(p);
                    Brain.Current.Add(iRes);
                    iInts.Add(p, iRes.ID);
                    fIsChanged = true;
                }
                else
                {
                    iRes = Brain.Current[iFound] as IntNeuron;
                }

                if (iRes == null)
                {
                    throw new System.InvalidOperationException("failed to find an int at " + iFound);
                }

                return iRes;
            }
        }

        /// <summary>tries to find a local with the specified name.</summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<ulong> FindLocal(string name)
        {
            var iLocals = Locals;
            lock (iLocals)
            {
                System.Collections.Generic.List<ulong> iFound;
                iLocals.TryGetValue(name, out iFound);
                return iFound;
            }
        }

        /// <summary>Finds the already existing local (if any) with the specified split
        ///     reaction, initial value and type.</summary>
        /// <param name="name">The name.</param>
        /// <param name="initVal">The init val.</param>
        /// <param name="varType">Type of the var.</param>
        /// <param name="splitReaction">The split Reaction.</param>
        /// <returns>The <see cref="Variable"/>.</returns>
        public Variable FindLocal(string name, Neuron initVal, System.Type varType, ulong splitReaction)
        {
            foreach (var i in FindLocal(name))
            {
                var iVar = Brain.Current[i] as Variable;
                if (iVar != null && iVar.Value == initVal && iVar.SplitReactionID == splitReaction
                    && iVar.TypeOfValue == varType)
                {
                    // also need to make certain taht the types match.
                    return iVar;
                }
            }

            return null;
        }

        /// <summary>adds the specified local to the list if it hasn't been added yet.</summary>
        /// <param name="name"></param>
        /// <param name="toAdd"></param>
        public void AddLocal(string name, Neuron toAdd)
        {
            var iLocals = Locals;
            lock (iLocals)
            {
                System.Collections.Generic.List<ulong> iFound;
                if (iLocals.TryGetValue(name, out iFound) == false)
                {
                    iFound = new System.Collections.Generic.List<ulong>();
                    iLocals.Add(name, iFound);
                }

                if (iFound.Contains(toAdd.ID) == false)
                {
                    iFound.Add(toAdd.ID);
                }
            }

            fIsChanged = true;
        }

        /// <summary>checks if the <paramref name="neuron"/> is stored in one of the
        ///     dictionaries. if so, it will be removed.</summary>
        /// <param name="neuron"></param>
        /// <param name="name">The name.</param>
        public void TryRemove(Neuron neuron, string name)
        {
            ulong iFound;
            var iInt = neuron as IntNeuron;
            if (iInt != null)
            {
                if (Ints.TryGetValue(iInt.Value, out iFound) && iFound == neuron.ID)
                {
                    Ints.Remove(iInt.Value);
                    fIsChanged = true;
                }
            }
            else
            {
                var iDouble = neuron as DoubleNeuron;
                if (iDouble != null)
                {
                    if (Doubles.TryGetValue(iDouble.Value, out iFound) && iFound == neuron.ID)
                    {
                        Doubles.Remove(iDouble.Value);
                        fIsChanged = true;
                    }
                }
                else
                {
                    var iLocal = neuron as Local;
                    if (iLocal != null)
                    {
                        System.Collections.Generic.List<ulong> iList;
                        if (Locals.TryGetValue(name, out iList))
                        {
                            iList.Remove(neuron.ID);
                            if (iList.Count == 0)
                            {
                                Locals.Remove(name);
                            }

                            fIsChanged = true;
                        }
                    }
                }
            }
        }
    }
}