// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConduitPipe.cs" company="">
//   
// </copyright>
// <summary>
//   provides data from a cvs file to a query.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     provides data from a cvs file to a query.
    /// </summary>
    public class CustomConduitPipe : IQueryPipe, 
                                     System.Collections.Generic.IEnumerator
                                         <System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>The f conduit.</summary>
        private Designer.CustomConduitSupport.ICustomConduit fConduit;

        /// <summary>The f current values.</summary>
        private readonly System.Collections.Generic.List<Neuron> fCurrentValues =
            new System.Collections.Generic.List<Neuron>();

        #region Library

        /// <summary>
        ///     Gets/sets the name of the library that contains the code.
        /// </summary>
        public string Library { get; set; }

        #endregion

        #region EntryPoint

        /// <summary>
        ///     Gets/sets the name of the class that provides the entry point.
        /// </summary>
        public string EntryPoint { get; set; }

        #endregion

        /// <summary>
        ///     Gets or sets the conduit that this pipe uses for retrieving the data.
        /// </summary>
        /// <value>
        ///     The conduit.
        /// </value>
        public Designer.CustomConduitSupport.ICustomConduit Conduit
        {
            get
            {
                if (fConduit == null)
                {
                    var iFile = Library;
                    if (System.IO.Path.IsPathRooted(iFile) == false)
                    {
                        iFile = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, iFile);
                    }

                    try
                    {
                        var ass = System.Reflection.Assembly.LoadFrom(iFile); // make certain that the library is loaded.
                        if (ass != null)
                        {
                            var iType = System.Type.GetType(EntryPoint); // get the type
                            fConduit =
                                (Designer.CustomConduitSupport.ICustomConduit)System.Activator.CreateInstance(iType);

                                // and create the conduit.
                        }
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "Custom conduit", 
                            string.Format("failed to load library: {0}. error: {1}", iFile, e));
                    }
                }

                return fConduit;
            }

            set
            {
                if (value != fConduit)
                {
                    if (fConduit != null)
                    {
                        // if there is a previous conduit, make certain it is closed.
                        fConduit.Close();
                    }

                    fConduit = value;
                }
            }
        }

        #region FileName

        /// <summary>
        ///     Gets/sets the name of the file that contains the data.
        /// </summary>
        public string FileName { get; set; }

        #endregion

        #region Destination

        /// <summary>
        ///     Gets/sets the possible destination to write the data too. Not yet
        ///     fully supported.
        /// </summary>
        public string Destination { get; set; }

        #endregion

        #region IEnumerator<IEnumerable<Neuron>> Members

        /// <summary>
        ///     Gets the current value.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Current
        {
            get
            {
                return fCurrentValues;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            fConduit = null;
        }

        #endregion

        #region IEnumerable<IEnumerable<JaStDev.HAB.Neuron>> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be
        ///     used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IForEachSource Members

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a
        ///     duplicate, its' ok to return a new <see langword="enum"/> that goes
        ///     back to the start (a warning should be rendered that splits are not
        ///     supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            LogService.Log.LogError(
                "Query", 
                string.Format("Can't duplicate a reference to the datasource of the query: {0}", Library));
            return null;
        }

        #endregion

        #region IQueryPipe Members

        /// <summary>
        ///     makes certain that everything is loaded into memory.
        /// </summary>
        public void TouchMem()
        {
            // don't do anything.
        }

        /// <summary>
        ///     called when extra data needs to be saved to disk in seperate files.
        /// </summary>
        public void Flush()
        {
            // don't do anything.
        }

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        public void WriteV1(System.IO.BinaryWriter writer)
        {
            writer.Write((System.UInt16)1); // the version nr.
            if (Library != null)
            {
                writer.Write(Library);
            }
            else
            {
                writer.Write(string.Empty);
            }

            if (EntryPoint != null)
            {
                writer.Write(EntryPoint);
            }
            else
            {
                writer.Write(string.Empty);
            }

            if (FileName != null)
            {
                writer.Write(FileName);
            }
            else
            {
                writer.Write(string.Empty);
            }

            if (Destination != null)
            {
                writer.Write(Destination);
            }
            else
            {
                writer.Write(string.Empty);
            }

            fConduit.WriteV1(writer);
        }

        /// <summary>Reads the v1.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadV1(System.IO.BinaryReader reader)
        {
            if (reader.ReadUInt16() == 1)
            {
                // check version nr.
                Library = reader.ReadString();
                EntryPoint = reader.ReadString();
                FileName = reader.ReadString();
                Destination = reader.ReadString();
                Conduit.ReadV1(reader);
            }
            else
            {
                throw new System.InvalidOperationException("Unsupported version nr for Custom conduit's data.");
            }
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            var iRes = false;
            try
            {
                if (Conduit.IsOpen == false)
                {
                    Conduit.Open(FileName);
                }
                else
                {
                    fCurrentValues.Clear(); // previous values need to be removed, 
                }

                iRes = Conduit.ReadLine(fCurrentValues);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Custom conduit pipe", e.ToString());
                iRes = false;
            }

            if (iRes == false && Conduit.IsOpen)
            {
                Conduit.Close();
            }

            return iRes;
        }

        /// <summary>The reset.</summary>
        public void Reset()
        {
            if (fConduit.IsOpen)
            {
                fConduit.Close();
            }

            fConduit.Open(FileName);
        }

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The <see langword="enum"/> to move passed the end.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            Conduit.Cancel();
        }

        #endregion
    }
}